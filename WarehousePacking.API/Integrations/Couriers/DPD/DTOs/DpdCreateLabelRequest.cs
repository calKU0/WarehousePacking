namespace WarehousePacking.API.Integrations.Couriers.DPD.DTOs
{
    public class DpdCreateLabelRequest
    {
        public LabelSearch? LabelSearchParams { get; set; }
        public string? OutputDocFormat { get; set; }
        public string? Format { get; set; }
        public string? OutputType { get; set; }
        public string? Variant { get; set; }

        public class LabelSearch
        {
            public string? Policy { get; set; }
            public Session? Session { get; set; }
            public string? DocumentId { get; set; }
        }

        public class Package
        {
            public string? Reference { get; set; }
            public List<Parcel>? Parcels { get; set; }
        }

        public class Parcel
        {
            public string? Reference { get; set; }
            public string? Waybill { get; set; }
        }

        public class Session
        {
            public long? SessionId { get; set; }
            public List<Package>? Packages { get; set; }
            public string? Type { get; set; }
        }
    }
}