namespace WarehousePacking.Contracts.DTOs
{
    public class SearchInvoiceResult
    {
        public int InvoiceId { get; set; }
        public string InvoiceName { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public string ClientAcronym { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
    }
}