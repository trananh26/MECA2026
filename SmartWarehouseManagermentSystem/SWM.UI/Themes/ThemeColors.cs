using System.Windows.Media;

namespace SWM.UI.Themes
{
    /// <summary>Màu dùng trong code-behind (map, buffer, AGV) — khớp với AppColors.xaml.</summary>
    internal static class ThemeColors
    {
        private static SolidColorBrush Brush(string hex)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            brush.Freeze();
            return brush;
        }

        public static readonly Brush BufferEmpty = Brush("#D4EDDA");
        public static readonly Brush BufferEmptyText = Brush("#2E5E45");
        public static readonly Brush BufferFull = Brush("#2E7D5A");
        public static readonly Brush BufferFullText = Brush("#FFFFFF");

        public static readonly Brush MapLink = Brush("#8FAF9E");
        public static readonly Brush MapNode = Brush("#6B8F7E");

        public static readonly Brush AgvRun = Brush("#27AE60");
        public static readonly Brush AgvPark = Brush("#E67E22");
        public static readonly Brush AgvCharge = Brush("#D4AC0D");
        public static readonly Brush AgvIdle = Brush("#95A5A6");
        public static readonly Brush AgvAlarm = Brush("#E74C3C");
        public static readonly Brush AgvTrayFull = Brush("#1A252F");

        public static readonly Brush BatteryGood = Brush("#27AE60");
        public static readonly Brush BatteryMid = Brush("#E67E22");
        public static readonly Brush BatteryLow = Brush("#E74C3C");
        public static readonly Brush BatteryDefault = Brush("#E67E22");
    }
}
