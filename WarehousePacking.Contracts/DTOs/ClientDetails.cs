namespace WarehousePacking.Contracts.DTOs
{
    public class ClientDetails
    {
        public int AddressId { get; set; }
        public int AddressType { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}