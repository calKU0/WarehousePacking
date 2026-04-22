namespace WarehousePacking.Contracts.DTOs
{
    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string StationNumber { get; set; } = string.Empty;
        public DateTime LoginDate { get; set; } = DateTime.Now;
    }
}