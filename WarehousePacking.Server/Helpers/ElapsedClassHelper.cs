namespace WarehousePacking.Server.Helpers
{
    public static class ElapsedClassHelper
    {
        public static string GetBadgeClass(DateTime start, int redThreshold = 4, int? yellowThreshold = null)
        {
            var elapsed = DashboardClock.Now - FormatHelper.ToLocalTime(start);

            if (elapsed.TotalMinutes >= redThreshold)
            {
                return "text-bg-danger";
            }

            if (yellowThreshold.HasValue && elapsed.TotalMinutes >= yellowThreshold.Value)
            {
                return "text-bg-warning";
            }

            return "text-bg-success";
        }

        public static string GetRowClass(DateTime start, int redThreshold = 4, int? yellowThreshold = null)
        {
            var elapsed = DashboardClock.Now - FormatHelper.ToLocalTime(start);

            if (elapsed.TotalMinutes >= redThreshold)
            {
                return "table-danger";
            }

            if (yellowThreshold.HasValue && elapsed.TotalMinutes >= yellowThreshold.Value)
            {
                return "table-warning";
            }

            return string.Empty;
        }
    }
}
