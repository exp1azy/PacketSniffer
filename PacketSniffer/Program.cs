using PacketSniffer.Startup;
using Serilog;

namespace PacketSniffer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.RunAsProcess();
            builder.StartWithWindows();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.EventLog("PacketSniffer", manageEventSource: true)
                .CreateLogger();

            builder.Services.AddHttpsRedirection(options =>
            {
                options.HttpsPort = 59037;
            });

            var connection = builder.AddRedis();

            builder.Services.AddSingleton<PcapAgent>();
            builder.Services.AddTransient(sp => new RedisService(connection, $"host_{Environment.MachineName}"));

            builder.Services.AddAuthentication();

            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.MapControllers();

            app.Run();
        }
    }
}
