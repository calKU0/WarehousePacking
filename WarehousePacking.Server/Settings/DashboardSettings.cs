using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Server.Settings
{
    public class DashboardSettings
    {
        public List<PanelConfiguration> Panels { get; set; } = new();
    }

    public class PanelConfiguration
    {
        public DashboardPanel Panel { get; set; }
        public bool IsEnabled { get; set; } = true;
        public int RefreshIntervalSeconds { get; set; } = 15;
        public int SlideIntervalSeconds { get; set; } = 30;
    }
}
