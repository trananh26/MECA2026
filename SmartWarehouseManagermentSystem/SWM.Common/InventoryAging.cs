using System;

namespace SWM.Common
{
    /// <summary>Định dạng thời gian tồn kho từ UPDATETIME (lúc nhập hàng vào ô).</summary>
    public static class InventoryAging
    {
        public static string Format(DateTime storedSince)
        {
            TimeSpan aging = DateTime.Now - storedSince;
            if (aging < TimeSpan.Zero)
                aging = TimeSpan.Zero;

            // >= 1 ngày: "2d 05:30" | dưới 1 ngày: "05:30" (giờ:phút)
            if (aging.Days > 0)
                return string.Format("{0}d {1:D2}:{2:D2}", aging.Days, aging.Hours, aging.Minutes);

            return string.Format("{0:D2}:{1:D2}", (int)aging.TotalHours, aging.Minutes);
        }

        public static string FormatFromUpdateTime(string updateTime)
        {
            if (string.IsNullOrWhiteSpace(updateTime))
                return string.Empty;

            if (!DateTime.TryParse(updateTime, out DateTime storedSince))
                return string.Empty;

            return Format(storedSince);
        }
    }
}
