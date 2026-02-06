namespace WarehousePacking.API.Integrations.Couriers.DPD_Romania.DTOs
{
    public class DpdRomaniaCreateLabelRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Language { get; set; } = "EN";
        public long? ClientSystemId { get; set; }
        public string Format { get; set; } = "zpl";
        public string PaperSize { get; set; } = string.Empty;
        public List<ParcelToPrint> Parcels { get; set; } = new();
        public string? PrinterName { get; set; }
        public string? Dpi { get; set; }
        public string? AdditionalWaybillSenderCopy { get; set; }
    }

    public class ParcelToPrint
    {
        public Parcel Parcel { get; set; } = default!;
    }

    public class Parcel
    {
        public string Id { get; set; } = default!;
    }
}