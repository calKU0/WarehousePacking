namespace WarehousePacking.Server.Settings
{
    public class CrystalReportsOptions
    {
        public CrystalDbOptions Database { get; set; } = new();
        public Dictionary<string, string> Reports { get; set; } = new();
    }

    public class CrystalDbOptions
    {
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}