namespace WarehousePacking.Contracts.DTOs.Couriers.Fedex
{
    public class Output
    {
        public List<TransactionShipment>? TransactionShipments { get; set; }
        public List<Alert>? Alerts { get; set; }
        public string? JobId { get; set; }
    }
}