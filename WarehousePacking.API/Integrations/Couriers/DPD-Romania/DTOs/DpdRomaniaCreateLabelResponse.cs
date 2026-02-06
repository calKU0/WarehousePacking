using static System.Runtime.InteropServices.Marshalling.IIUnknownCacheStrategy;

namespace WarehousePacking.API.Integrations.Couriers.DPD_Romania.DTOs
{
    public class DpdRomaniaCreateLabelResponse
    {
        public byte[]? Data { get; set; }
        public LabelInfo[]? PrintLabelsInfo { get; set; }
        public DpdRomaniaErrorResponse? Error { get; set; }
    }

    public class LabelInfo
    {
        public string ParcelId { get; set; } = default!;
        public int? HubId { get; set; }
        public int? OfficeId { get; set; }
        public string? OfficeName { get; set; }
        public int? DeadlineDay { get; set; }
        public int? DeadlineMonth { get; set; }
        public int? TourId { get; set; }
        public string FullBarcode { get; set; } = default!;
        public int ExportPriority { get; set; }
    }
}