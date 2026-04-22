namespace WarehousePacking.Contracts.DTOs.Couriers.DPD_Romania
{
    public class DpdRomaniaCreateShipmentResponse
    {
        public string? Id { get; set; }
        public DpdRomaniaError? Error { get; set; }
    }

    public class DpdRomaniaError
    {
        public string? Context { get; set; }
        public string? Message { get; set; }
        public string? Id { get; set; }
        public int Code { get; set; }
        public string? Component { get; set; }
    }
}