using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Server.Settings
{
    public class WorkstationSettings
    {
        public string PrinterLabel { get; set; } = "";
        public string PrinterInvoice { get; set; } = "";
        public PackingWarehouse PackingWarehouse { get; set; }
        public PackingLevel PackingLevel { get; set; }
        public StationType StationType { get; set; }
        public string StationNumber { get; set; } = "";
        public DashboardSettings DashboardSettings { get; set; } = new DashboardSettings();
    }
}