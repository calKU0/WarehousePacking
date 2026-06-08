namespace WarehousePacking.Server.Helpers
{
    public static class FormatHelper
    {
        public static string FormatQuantity(decimal value, string? unit = null, string? format = null)
        {
            if (string.IsNullOrEmpty(format))
            {
                format = value % 1 == 0 ? "F0" : "F2";
            }

            return string.IsNullOrEmpty(unit)
                ? value.ToString(format)
                : $"{value.ToString(format)} {unit}";
        }
        public static string FormatElapsed(DateTime start)
        {
            var localStart = ToLocalTime(start);
            DateTime now = DateTime.Now;

            var elapsed = now - localStart;
            if (elapsed.TotalDays >= 1)
            {
                return $"{(int)elapsed.TotalDays}d {(int)elapsed.TotalHours % 24}h";
            }

            if (elapsed.TotalHours >= 1)
            {
                return $"{(int)elapsed.TotalHours}h {elapsed.Minutes}m";
            }

            return $"{Math.Max(0, elapsed.Minutes)}m {Math.Max(0, elapsed.Seconds)}s";
        }

        public static DateTime ToLocalTime(DateTime value)
        {
            return value.Kind == DateTimeKind.Local
                ? value
                : DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime();
        }
    }
}
