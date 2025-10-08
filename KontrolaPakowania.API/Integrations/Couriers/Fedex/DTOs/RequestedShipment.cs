using System.Collections.Generic;

namespace KontrolaPakowania.API.Integrations.Couriers.Fedex.DTOs
{
    public class RequestedShipment
    {
        public string? ShipDatestamp { get; set; }
        public TotalDeclaredValue? TotalDeclaredValue { get; set; }
        public Shipper? Shipper { get; set; }
        public List<Recipient>? Recipients { get; set; }
        public ShippingChargesPayment? ShippingChargesPayment { get; set; }
        public string? RecipientLocationNumber { get; set; }
        public string? PickupType { get; set; }
        public string? ServiceType { get; set; }
        public string? PackagingType { get; set; }
        public double? TotalWeight { get; set; }
        public LabelSpecification? LabelSpecification { get; set; }
        public string? PreferredCurrency { get; set; }
        public int? TotalPackageCount { get; set; }
        public MasterTrackingId? MasterTrackingId { get; set; }
        public List<RequestedPackageLineItem>? RequestedPackageLineItems { get; set; }
        public CustomsClearanceDetail? CustomsClearanceDetail { get; set; }
    }
}