namespace WarehousePacking.API.Integrations.Couriers.Fedex.DTOs
{
    public class FedexShipmentRequest
    {
        public string? LabelResponseOptions { get; set; }
        public AccountNumber? AccountNumber { get; set; }
        public RequestedShipment? RequestedShipment { get; set; }
    }
}