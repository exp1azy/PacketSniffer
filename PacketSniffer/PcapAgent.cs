﻿using SharpPcap.LibPcap;
using SharpPcap.Statistics;
using StackExchange.Redis;
using System.Net.Sockets;
using System.Net;
using SharpPcap;
using Newtonsoft.Json;
using PacketSniffer.Resources;
using System.Collections.Concurrent;
using Serilog;
using PcapDevice = WebSpectre.Shared.Capture.PcapDevice;
using WebSpectre.Shared.Capture;
using System.Management;
using WebSpectre.Shared.Agents;

namespace PacketSniffer
{
    /// <summary>
    /// Класс-обработчик сетевого трафика.
    /// </summary>
    public class PcapAgent
    {
        private readonly IConfiguration _config;
        private readonly RedisService _redisService;
        private readonly int _maxQueueSize;

        private Task? _captureTask;
        private CancellationTokenSource? _captureCancellation;

        private ConcurrentQueue<StatisticsEventArgs> _statisticsQueue;
        private ConcurrentQueue<RawCapture> _rawPacketsQueue;

        private string _redisStreamKey = $"host_{Environment.MachineName}";
        private string _rawPacketValueKey = "raw_packets";
        private string _statisticsValueKey = "statistics";

        private bool _isSnifferCapturing = false;

        /// <summary>
        /// Конструктор <see cref="PcapAgent"/>.
        /// </summary>
        /// <param name="config">Файл конфигурации.</param>
        public PcapAgent(IConfiguration config, RedisService redisService)
        {
            _config = config;
            _redisService = redisService;

            _statisticsQueue = new ConcurrentQueue<StatisticsEventArgs>();
            _rawPacketsQueue = new ConcurrentQueue<RawCapture>();

            if (int.TryParse(_config["MaxQueueSize"], out var maxQueueSize))
            {
                _maxQueueSize = maxQueueSize;
            }
            else
            {
                _maxQueueSize = 20;
                Log.Logger.Warning(Error.FailedToReadQueuesSizeData);
            }
        }

        /// <summary>
        /// true, если захват трафика запущен, иначе false.
        /// </summary>
        public bool IsSnifferCapturing => _isSnifferCapturing;

        /// <summary>
        /// Метод, необходимый для получения IPv4-адресов устройств данной машины.
        /// </summary>
        /// <returns>IPv4-адреса.</returns>
        public async Task<HostInfo> GetHostInfo()
        {
            var os = Environment.OSVersion.VersionString;
            var motherboard = GetMotherboardInfo();
            var memory = GetMemoryInfo();
            var cpu = GetCPUInfo();
            var gpu = GetGPUInfo();
            var addresses = Dns.GetHostAddresses(Dns.GetHostName()).Select(ip => ip.ToString()).ToArray();

            return new HostInfo
            {
                OSVersion = os,
                Hardware = new Hardware 
                { 
                    MotherboardInfo = motherboard ,
                    MemoryInfo = memory,
                    CPUInfo = cpu,
                    GPUInfo = gpu
                },
                IPAddresses = addresses
            };           
        }
           
        /// <summary>
        /// Получить доступные устройства.
        /// </summary>
        /// <returns>Список устройств.</returns>
        public List<PcapDevice> GetDevices()
        {
            var devices = LibPcapLiveDeviceList.Instance;
            if (devices.Count < 1)           
                throw new ApplicationException();

            var formattedDevices = new List<PcapDevice>();
            
            foreach (var device in devices)
            {
                formattedDevices.Add(new PcapDevice
                {
                    Addresses = device.Interface.Addresses.Select(a => (WebSpectre.Shared.Capture.PcapAddress)a).ToList(),
                    Description = device.Interface.Description,
                    FriendlyName = device.Interface.FriendlyName,
                    GatewayAddresses = device.Interface.GatewayAddresses.Select(a => a.ToString()).ToList(),
                    MacAddress = device.Interface.MacAddress == null ? null : device.Interface.MacAddress.ToString(),
                });
            }

            return formattedDevices;
        }

        /// <summary>
        /// Запустить захват сетевого трафика по указанному устройству.
        /// </summary>
        /// <param name="adapter">Устройство, по которому необходимо запустить прослушивание сетевого трафика.</param>
        /// <exception cref="ApplicationException"></exception>
        public void Start(string adapter)
        {
            if (string.IsNullOrEmpty(_config["RedisConnection"]))
            {
                Log.Logger.Error(Error.FailedToReadRedisConnectionString);
                throw new ApplicationException(Error.FailedToReadRedisConnectionString);
            }

            var os = Environment.OSVersion;
            if (os.Platform != PlatformID.Win32NT)
            {
                Log.Logger.Error(Error.UnsupportedOS);
                throw new ApplicationException(Error.UnsupportedOS);
            }

            var devices = LibPcapLiveDeviceList.Instance;
            if (devices.Count < 1)
            {
                Log.Logger.Error(Error.NoDevicesWereFound);
                throw new ApplicationException(Error.NoDevicesWereFound);
            }

            int interfaceIndex = GetInterfaceIndex(devices, adapter);
            if (interfaceIndex < 0)
            {
                Log.Logger.Error(Error.NoSuchInterface, adapter);
                throw new ApplicationException($"{Error.NoSuchInterface} {adapter}");
            }
             
            if (_captureTask == null || _captureTask.IsCompleted)
            {
                _captureCancellation = new CancellationTokenSource();

                try
                {
                    _captureTask = ListenRequiredInterfaceAsync(devices, interfaceIndex, _captureCancellation.Token);

                    _isSnifferCapturing = true;
                }
                catch (ApplicationException)
                {

                    _isSnifferCapturing = false;
                    throw;
                }
            }
            else
            {
                _isSnifferCapturing = true;
            }

            Log.Logger.Information(Information.LocalSniffingStarted);
        }

        /// <summary>
        /// Остановить захват сетевого трафика.
        /// </summary>
        public void Stop()
        {
            if (_captureTask != null)
            {
                _captureCancellation!.Cancel();

                _captureTask?.Wait();

                _captureCancellation.Dispose();
                _captureCancellation = null;

                _isSnifferCapturing = false;

                Log.Logger.Information(Information.LocalSniffingStopped);
            }
        }

        /// <summary>
        /// Слушает сетевой трафик по указанному индексу устройства.
        /// </summary>
        /// <param name="devices">Сетевые устройства.</param>
        /// <param name="interfaceToSniff">Индекс устройства, с которого осуществляется перехват сетевого трафика</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        private async Task ListenRequiredInterfaceAsync(LibPcapLiveDeviceList devices, int interfaceToSniff, CancellationToken cancellationToken)
        {
            var filter = _config["Filters"];
            if (string.IsNullOrEmpty(filter))
            {
                Log.Logger.Error(Error.FailedToReadProtocols);
                throw new ApplicationException(Error.FailedToReadProtocols);
            }

            using var statisticsDevice = new StatisticsDevice(devices[interfaceToSniff].Interface);
            using var device = devices[interfaceToSniff];

            statisticsDevice.OnPcapStatistics += Device_OnPcapStatistics!;
            device.OnPacketArrival += new PacketArrivalEventHandler(Device_OnPacketArrival);

            statisticsDevice.Open();
            device.Open();

            statisticsDevice.StartCapture();
            device.StartCapture();

            while (!cancellationToken.IsCancellationRequested)
                await Task.Delay(2000);

            statisticsDevice.StopCapture();
            device.StopCapture();
        }

        /// <summary>
        /// Метод-обработчик события OnPacketArrival.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Device_OnPacketArrival(object sender, PacketCapture e)
        {
            var rawPacket = e.GetPacket();

            if (_rawPacketsQueue.Count < _maxQueueSize)
                _rawPacketsQueue.Enqueue(rawPacket);                           
            else
                HandleRawPacketsQueueAsync().Wait();                                    
        }

        /// <summary>
        /// Метод-обработчик события OnPcapStatistics.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Device_OnPcapStatistics(object sender, StatisticsEventArgs e)
        {          
            if (_statisticsQueue.Count < _maxQueueSize)          
                _statisticsQueue.Enqueue(e);                               
            else          
                HandleStatisticsQueueAsync().Wait();      
        }

        /// <summary>
        /// Метод, необходимый для массовой загрузки сырых пакетов в поток Redis из очереди.
        /// </summary>
        /// <returns></returns>
        private async Task HandleRawPacketsQueueAsync()
        {
            var entries = new List<NameValueEntry>();

            while (_rawPacketsQueue.TryDequeue(out var rawPacket))              
                entries.Add(new NameValueEntry(_rawPacketValueKey, JsonConvert.SerializeObject(rawPacket))); 

            await _redisService.StreamAddAsync(_redisStreamKey, entries.ToArray());
        }

        /// <summary>
        /// Метод, необходимый для массовой загрузки статистики в поток Redis из очереди.
        /// </summary>
        /// <returns></returns>
        private async Task HandleStatisticsQueueAsync()
        {
            var entries = new List<NameValueEntry>();

            while (_statisticsQueue.TryDequeue(out var statistics))
                entries.Add(new NameValueEntry(_statisticsValueKey, JsonConvert.SerializeObject(statistics)));

            await _redisService.StreamAddAsync(_redisStreamKey, entries.ToArray());         
        }

        /// <summary>
        /// Получить индекс запрашиваемого устройства.
        /// </summary>
        /// <param name="devices">Устройства.</param>
        /// <param name="interfaceToSniff">Интерфейс, необходимый для захвата пакетов.</param>
        /// <returns>Индекс устройства.</returns>
        private int GetInterfaceIndex(LibPcapLiveDeviceList devices, string interfaceToSniff) =>
            devices.IndexOf(devices.FirstOrDefault(d => d.Description == interfaceToSniff));

        private MotherboardInfo GetMotherboardInfo()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            var mboardInfo = new MotherboardInfo();

            foreach (var obj in searcher.Get())
            {
                mboardInfo.Manufacturer += obj["Manufacturer"];
                mboardInfo.Model += obj["Product"];
            }

            return mboardInfo;
        }

        private MemoryInfo GetMemoryInfo()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
            long memorySize = 0;
            foreach (var obj in searcher.Get())
            {
                memorySize += Convert.ToInt64(obj["Capacity"]);
            }

            return new MemoryInfo
            {
                TotalMemory = (int)(memorySize / (1024 * 1024))
            };
        }

        public CPUInfo GetCPUInfo()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            var cpu = new CPUInfo();
            foreach (var obj in searcher.Get())
            {
                cpu.Processor += obj["Name"];
                cpu.NumberOfCores += obj["NumberOfCores"];
                cpu.MaxClockSpeed += obj["MaxClockSpeed"];
            }

            return cpu;
        }

        public GPUInfo GetGPUInfo()
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            var gpu = new GPUInfo();
            foreach (var obj in searcher.Get())
            {
                gpu.GraphicsCard = $"{obj["Name"]}";
            }

            return gpu;
        }
    }
}