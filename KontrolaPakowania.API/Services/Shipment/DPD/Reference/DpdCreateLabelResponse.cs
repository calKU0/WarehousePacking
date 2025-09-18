namespace KontrolaPakowania.API.Services.Shipment.DPD.Reference
{
    public class DpdCreateLabelResponse
    {
        public string Status { get; set; } = string.Empty;
        public string DocumentData { get; set; } = string.Empty;
        public DpdLabelSession Session { get; set; } = new();
        public string DocumentId { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
    }

    public class DpdLabelSession
    {
        public long SessionId { get; set; }
        public List<DpdPackageResponse> Packages { get; set; } = new();
    }
}