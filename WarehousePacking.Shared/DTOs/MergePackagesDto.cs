namespace WarehousePacking.Shared.DTOs
{
    public class MergePackagesDto
    {
        public string InitialBarcode { get; set; } = string.Empty;
        public string MergingBarcode { get; set; } = string.Empty;
        public Dimensions Dimensions { get; set; } = new();
    }
}
