using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Contracts.DTOs
{
    public class PackageData
    {
        public int Id { get; set; }
        public int InvoiceId { get; set; }
        public Courier Courier { get; set; }
        public string InternalBarcode { get; set; } = string.Empty;
        public string CourierName { get; set; } = string.Empty;
        public PackageType PackageType { get; set; }
        public DocumentStatus Status { get; set; }
        public string InvoiceName { get; set; } = string.Empty;
        public string PackageName { get; set; } = string.Empty;
        public string SenderBankAccount { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string References { get; set; } = string.Empty;
        public string Representative { get; set; } = string.Empty;
        public string RepresentativeEmail { get; set; } = string.Empty;
        public int PackageQuantity { get; set; } = 1;
        public decimal Insurance { get; set; }
        public decimal Weight { get; set; }
        public int Length { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int WysNumber { get; set; }
        public int WysType { get; set; }
        public string PackingUser { get; set; } = string.Empty;
        public DateTime DateShipped { get; set; }
        public bool HasInvoice { get; set; }
        public bool TaxFree { get; set; }
        public bool ManualSend { get; set; }
        public bool ManualEdit { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public Recipient Recipient { get; set; } = new Recipient();
        public ShipmentServices ShipmentServices { get; set; } = new ShipmentServices();
    }
}