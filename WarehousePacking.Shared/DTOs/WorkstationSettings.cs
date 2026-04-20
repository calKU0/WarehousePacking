using WarehousePacking.Shared.Enums;

namespace WarehousePacking.Shared.DTOs
{
    public class WorkstationSettings
    {
        public const int DefaultMonitorRefreshIntervalSeconds = 15;
        public const int DefaultMonitorSlideIntervalSeconds = 30;

        public string PrinterLabel { get; set; } = "";
        public string PrinterInvoice { get; set; } = "";
        public PackingWarehouse PackingWarehouse { get; set; }
        public PackingLevel PackingLevel { get; set; }
        public StationType StationType { get; set; }
        public string StationNumber { get; set; } = "";
        public int MonitorRefreshIntervalSeconds { get; set; } = DefaultMonitorRefreshIntervalSeconds;
        public int MonitorSlideIntervalSeconds { get; set; } = DefaultMonitorSlideIntervalSeconds;
    }
}