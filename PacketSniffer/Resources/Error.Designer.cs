﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PacketSniffer.Resources {
    using System;
    
    
    /// <summary>
    ///   Класс ресурса со строгой типизацией для поиска локализованных строк и т.д.
    /// </summary>
    // Этот класс создан автоматически классом StronglyTypedResourceBuilder
    // с помощью такого средства, как ResGen или Visual Studio.
    // Чтобы добавить или удалить член, измените файл .ResX и снова запустите ResGen
    // с параметром /str или перестройте свой проект VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Error {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Error() {
        }
        
        /// <summary>
        ///   Возвращает кэшированный экземпляр ResourceManager, использованный этим классом.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PacketSniffer.Resources.Error", typeof(Error).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Перезаписывает свойство CurrentUICulture текущего потока для всех
        ///   обращений к ресурсу с помощью этого класса ресурса со строгой типизацией.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Не удалось прочитать задержку подключения к серверу Redis из файла appsettings.json. Установлено значение &quot;10&quot;.
        /// </summary>
        internal static string FailedToReadConnectionDelay {
            get {
                return ResourceManager.GetString("FailedToReadConnectionDelay", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Не удалось прочитать данные о сети из файла appsettings.json.
        /// </summary>
        internal static string FailedToReadNetworkPrefixData {
            get {
                return ResourceManager.GetString("FailedToReadNetworkPrefixData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Не удалось прочитать порт из файла appsettings.json.
        /// </summary>
        internal static string FailedToReadPort {
            get {
                return ResourceManager.GetString("FailedToReadPort", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Не удалось прочитать протоколы, необходимые для перехвата пакетов, из файла appsettings.
        /// </summary>
        internal static string FailedToReadProtocols {
            get {
                return ResourceManager.GetString("FailedToReadProtocols", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Не удалось прочитать размер очередей из файла appsettings.json. Установлено значение &quot;20&quot;.
        /// </summary>
        internal static string FailedToReadQueuesSizeData {
            get {
                return ResourceManager.GetString("FailedToReadQueuesSizeData", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Не удалось прочитать строку подключения к серверу Redis.
        /// </summary>
        internal static string FailedToReadRedisConnectionString {
            get {
                return ResourceManager.GetString("FailedToReadRedisConnectionString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Не удалось установить срок действия ключа Redis. Ключ не существует, либо указанное время не может быть установлено.
        /// </summary>
        internal static string FailedToSetKeyExpirationTime {
            get {
                return ResourceManager.GetString("FailedToSetKeyExpirationTime", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Не удалось установить соединение с Redis.
        /// </summary>
        internal static string NoConnection {
            get {
                return ResourceManager.GetString("NoConnection", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на На вашем компьютере не было найдено ни одного сетевого устройства.
        /// </summary>
        internal static string NoDevicesWereFound {
            get {
                return ResourceManager.GetString("NoDevicesWereFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на На вашем компьютере не найден запрашиваемый сетевой интерфейс по префиксу: {0}.
        /// </summary>
        internal static string NoSuchInterface {
            get {
                return ResourceManager.GetString("NoSuchInterface", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Произошла непредвиденная ошибка: {0}.
        /// </summary>
        internal static string Unexpected {
            get {
                return ResourceManager.GetString("Unexpected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на Ваша платформа не поддерживается, поскольку служба использует специальные функции Npcap, присутствующие только в Win32NT.
        /// </summary>
        internal static string UnsupportedOS {
            get {
                return ResourceManager.GetString("UnsupportedOS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на VPN-соединение на компьютере не установлено.
        /// </summary>
        internal static string VirtualAdapterIsDisabled {
            get {
                return ResourceManager.GetString("VirtualAdapterIsDisabled", resourceCulture);
            }
        }
    }
}
