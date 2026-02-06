namespace WarehousePacking.API.Integrations.Couriers.DPD_Romania.DTOs
{
    public class DpdRomaniaErrorResponse
    {
        public string? Context { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public int Code { get; set; }
        public string? Component { get; set; }
    }
}