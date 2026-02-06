namespace WarehousePacking.API.Integrations.Couriers.Fedex.DTOs
{
    public class Dimensions
    {
        public int? Length { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string? Units { get; set; }
    }
}