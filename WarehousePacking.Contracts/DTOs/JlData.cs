using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs
{
    public class JlData
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int Status { get; set; }
        public decimal Weight { get; set; }
        public string CourierName { get; set; } = string.Empty;
        public Courier Courier { get; set; }
        public PackingLevel Level { get; set; }
        public PackingWarehouse Warehouse { get; set; }
        public ShipmentServices ShipmentServices { get; set; } = new();
        public string DestinationCountry { get; set; } = string.Empty;
        public bool OutsideEU { get; set; }
        public decimal Quantity { get; set; }
        public string Location { get; set; } = string.Empty;
        public string LocationId { get; set; } = string.Empty;
        public bool ReadyToPack { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime DocumentDate { get; set; }
        public int ClientId { get; set; }
        public int AddressId { get; set; }
        public int AddressType { get; set; }
        public string AddressName { get; set; } = string.Empty;
        public string AddressCountry { get; set; } = string.Empty;
        public string AddressCity { get; set; } = string.Empty;
        public string AddressStreet { get; set; } = string.Empty;
        public string AddressPostalCode { get; set; } = string.Empty;
        public string ClientSymbol { get; set; } = string.Empty;
        public bool PackageClosed { get; set; }
        public string InternalBarcode { get; set; } = string.Empty;
        public string PackingRequirements { get; set; } = string.Empty;
    }
}