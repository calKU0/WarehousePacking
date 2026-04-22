namespace WarehousePacking.Contracts.DTOs.Couriers.Fedex
{
    public class FedexShipmentRequest
    {
        public string? LabelResponseOptions { get; set; }
        public AccountNumber? AccountNumber { get; set; }
        public RequestedShipment? RequestedShipment { get; set; }
    }
}