namespace WarehousePacking.Contracts.DTOs.Dashboards
{
    public class DashboardColorConfiguration
    {
        public int ZoneLastActivityThresholdMinutes { get; set; }
        public int OperatorLastActivityYellowThresholdMinutes { get; set; }
        public int OperatorLastActivityRedThresholdMinutes { get; set; }
        public int PackingDownBlueThreshold { get; set; }
        public int PackingDownRedThreshold { get; set; }
        public int PackingDownPackingTimeYellowThreshold { get; set; }
        public int PackingDownPackingTimeRedThreshold { get; set; }
        public int PackingUpBlueThreshold { get; set; }
        public int PackingUpRedThreshold { get; set; }
        public int PackingUpPackingTimeYellowThreshold { get; set; }
        public int PackingUpPackingTimeRedThreshold { get; set; }
        public int SortingBlueThreshold { get; set; }
        public int SortingRedThreshold { get; set; }
    }
}
