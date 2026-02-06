namespace WarehousePacking.API.Settings
{
    public class FedexSettings
    {
        public FedexRestSettings Rest { get; set; } = new();
        public FedexSoapSettings Soap { get; set; } = new();
    }

    public class FedexRestSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Account { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
    }

    public class FedexSoapSettings
    {
        public int CourierId { get; set; }
        public string AccessCode { get; set; } = string.Empty;
        public string DropshippingAccessCode { get; set; } = string.Empty;
        public string SenderId { get; set; } = string.Empty;
        public string DropshippingSenderId { get; set; } = string.Empty;
    }
}