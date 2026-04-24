using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs
{
    public class WorkstationSettings
    {
        public const int DefaultDashboardRefreshIntervalSeconds = 15;
        public const int DefaultDashboardSlideIntervalSeconds = 30;

        public string PrinterLabel { get; set; } = "";
        public string PrinterInvoice { get; set; } = "";
        public PackingWarehouse PackingWarehouse { get; set; }
        public PackingLevel PackingLevel { get; set; }
        public StationType StationType { get; set; }
        public string StationNumber { get; set; } = "";
        public int DashboardRefreshIntervalSeconds { get; set; } = DefaultDashboardRefreshIntervalSeconds;
        public int DashboardSlideIntervalSeconds { get; set; } = DefaultDashboardSlideIntervalSeconds;
    }
}