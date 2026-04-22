namespace WarehousePacking.Contracts.DTOs.Couriers.Fedex
{
    public class FedexShipmentResponse
    {
        public string? TransactionId { get; set; }
        public Output? Output { get; set; }
    }
}