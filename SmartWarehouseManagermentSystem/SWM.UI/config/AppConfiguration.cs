using System;
using System.IO;
using System.Web.Script.Serialization;

namespace SWM.UI.Config
{
    /// <summary>Đọc cấu hình từ config/appsettings.json (copy ra thư mục chạy khi build).</summary>
    public sealed class AppConfiguration
    {
        private static AppConfiguration _current;

        public static AppConfiguration Current
        {
            get
            {
                if (_current == null)
                    throw new InvalidOperationException("Gọi AppConfiguration.Load() trước khi sử dụng.");
                return _current;
            }
        }

        public DatabaseSettings Database { get; set; }
        public PlcSettings Plc { get; set; }
        public SerialSettings Serial { get; set; }
        public ApplicationSettings Application { get; set; }
        public WarehouseSettings Warehouse { get; set; }

        public static void Load()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "appsettings.json");
            if (!File.Exists(path))
                throw new FileNotFoundException("Không tìm thấy file cấu hình: config/appsettings.json", path);

            string json = File.ReadAllText(path);
            _current = new JavaScriptSerializer().Deserialize<AppConfiguration>(json);
        }
    }

    public class DatabaseSettings
    {
        public string ConnectionString { get; set; }
    }

    public class PlcSettings
    {
        public string IpAddress { get; set; }
        public int StationNumber { get; set; }
    }

    public class SerialSettings
    {
        public string PortName { get; set; }
        public int BaudRate { get; set; }
    }

    public class ApplicationSettings
    {
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; }
    }

    public class WarehouseSettings
    {
        public string AgvId { get; set; }
        public string StkId { get; set; }
        public string InputPortName { get; set; }
        public string OutputPortName { get; set; }
        public string InputPortId { get; set; }
        public string OutputPortId { get; set; }
    }
}
