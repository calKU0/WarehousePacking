namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs
{
    public class FedexShipmentResponse
    {
        public string? TransactionId { get; set; }
        public Output? Output { get; set; }
    }
}