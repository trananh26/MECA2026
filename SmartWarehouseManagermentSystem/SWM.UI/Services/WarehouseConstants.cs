using SWM.UI.Config;

namespace SWM.UI.Services
{
    /// <summary>Truy cập cấu hình kho từ appsettings.json (section Warehouse).</summary>
    internal static class WarehouseConstants
    {
        private static WarehouseSettings W => AppConfiguration.Current.Warehouse;

        public static string AgvId => W.AgvId;
        public static string StkId => W.StkId;
        public static string InputPortName => W.InputPortName;
        public static string OutputPortName => W.OutputPortName;
        public static string InputPortId => W.InputPortId;
        public static string OutputPortId => W.OutputPortId;
    }
}
