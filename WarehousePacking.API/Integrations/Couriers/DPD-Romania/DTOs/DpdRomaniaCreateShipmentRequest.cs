namespace WarehousePacking.API.Integrations.Couriers.DPD_Romania.DTOs
{
    public class DpdRomaniaCreateShipmentRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Language { get; set; } = "EN";
        public DPDService? Service { get; set; }
        public DPDContent? Content { get; set; }
        public DPDPayment? Payment { get; set; }
        public DPDRecipient? Recipient { get; set; }
        public string? ShipmentNote { get; set; }
        public string? Ref1 { get; set; }
        public string? Ref2 { get; set; }

        public class DPDService
        {
            public int ServiceId { get; set; }
            public bool AutoAdjustPickupDate { get; set; }
            public AdditionalServices? AdditionalServices { get; set; }
        }

        public class AdditionalServices
        {
            public COD? COD { get; set; }
        }

        public class COD
        {
            public decimal Amount { get; set; }
            public string CurrencyCode { get; set; } = "RON";
            public OBPDetails? OBPDetails { get; set; }
            public bool PayoutToThirdParty { get; set; }
            public string ProcessingType { get; set; } = "CASH";
            public bool IncludeShippingPrice { get; set; }
        }

        public class OBPDetails
        {
            public string Option { get; set; } = "OPEN";
            public int ReturnShipmentServiceId { get; set; }
            public string ReturnShipmentPayer { get; set; } = "SENDER";
        }

        public class DPDContent
        {
            public int ParcelsCount { get; set; }
            public decimal TotalWeight { get; set; }
            public string Contents { get; set; } = "";
            public string Package { get; set; } = "";
            public List<Parcel>? Parcels { get; set; }
        }

        public class Parcel
        {
            public int SeqNo { get; set; }
            public ParcelSize? Size { get; set; }
            public decimal Weight { get; set; }
        }

        public class ParcelSize
        {
            public decimal Depth { get; set; }
            public decimal Width { get; set; }
            public decimal Height { get; set; }
        }

        public class DPDPayment
        {
            public string CourierServicePayer { get; set; } = "SENDER";
        }

        public class DPDRecipient
        {
            public Phone? Phone1 { get; set; }
            public bool PrivatePerson { get; set; } = true;
            public string ClientName { get; set; } = "";
            public string ContactName { get; set; } = "";
            public string Email { get; set; } = "";
            public Address? Address { get; set; }
        }

        public class Phone
        {
            public string Number { get; set; } = "";
        }

        public class Address
        {
            public int CountryId { get; set; }
            public string PostCode { get; set; } = "";
            public string SiteName { get; set; } = "";
            public string StreetType { get; set; } = "str.";
            public string StreetName { get; set; } = "";
            public string StreetNo { get; set; } = "";
        }
    }
}