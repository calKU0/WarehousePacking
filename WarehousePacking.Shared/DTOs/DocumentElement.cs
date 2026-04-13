namespace WarehousePacking.Shared.DTOs
{
    public class DocumentElement
    {
        public int Lp { get; set; }
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal Weight { get; set; }
        public decimal Volume { get; set; }
    }
}
