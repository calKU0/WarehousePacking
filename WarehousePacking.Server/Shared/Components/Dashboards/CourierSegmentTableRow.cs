namespace WarehousePacking.Server.Shared.Components.Dashboard
{
    public sealed class CourierSegmentTableRow
    {
        public string Key { get; set; } = string.Empty;
        public string Courier { get; set; } = string.Empty;
        public string CourierSrc { get; set; } = string.Empty;
        public int Jls { get; set; }
        public decimal Weight { get; set; }
        public decimal Quantity { get; set; }
        public int SharePercent { get; set; }
        public string ShareClass { get; set; } = string.Empty;
        public string AnimationClass { get; set; } = string.Empty;
    }
}