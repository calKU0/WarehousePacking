namespace WarehousePacking.Shared.DTOs
{
    public class CourierConfiguration
    {
        public string Courier { get; set; }
        public bool AutomaticFvGeneration { get; set; }
        public TimeSpan? AutomaticFvStart { get; set; }
        public TimeSpan? AutomaticFvEnd { get; set; }
        public decimal WeightUpPL { get; set; }
        public decimal WeightBottomPL { get; set; }
        public decimal WeightUpExport { get; set; }
        public decimal WeightBottomExport { get; set; }
        public virtual decimal MaxPackageWeight { get; set; }
    }
}