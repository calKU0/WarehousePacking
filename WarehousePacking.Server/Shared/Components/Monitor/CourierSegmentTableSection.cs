namespace WarehousePacking.Server.Shared.Components.Monitor
{
    public sealed class CourierSegmentTableSection
    {
        public string Title { get; set; } = string.Empty;
        public IReadOnlyList<CourierSegmentTableRow> Rows { get; set; } = Array.Empty<CourierSegmentTableRow>();
    }
}