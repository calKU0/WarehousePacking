namespace WarehousePacking.Shared.DTOs
{
    public class JlDto
    {
        public int JlId { get; set; }
        public string JlCode { get; set; } = string.Empty;
        public string JlEanCode { get; set; } = string.Empty;
        public int Status { get; set; }
        public string StatusSymbol { get; set; } = string.Empty;
        public string LocationCode { get; set; } = string.Empty;
        public string DestZone { get; set; } = string.Empty;
        public decimal Weight { get; set; }
        public string Type => Weight >= 120m ? "PALETA" : "PACZKA";
        public string ReadyToPack { get; set; } = string.Empty;
        public List<JlClientDto> Clients { get; set; } = new();
    }
}