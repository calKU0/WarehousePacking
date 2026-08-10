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

        /// <summary>
        /// Row-state class for a row that has been waiting too long. Returns the
        /// app-wide `is-*` states (css/components.css), not Bootstrap's pale
        /// `table-*` tints, so a late row looks the same on the wallboard as it
        /// does on the packing list.
        /// </summary>
        public static string GetRowClass(DateTime start, int redThreshold = 4, int? yellowThreshold = null)
        {
            var elapsed = DashboardClock.Now - FormatHelper.ToLocalTime(start);

            if (elapsed.TotalMinutes >= redThreshold)
            {
                return "is-error";
            }

            if (yellowThreshold.HasValue && elapsed.TotalMinutes >= yellowThreshold.Value)
            {
                return "is-warning";
            }

            return string.Empty;
        }
    }
}
