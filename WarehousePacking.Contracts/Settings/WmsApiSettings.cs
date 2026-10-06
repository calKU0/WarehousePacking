namespace WarehousePacking.Contracts.Settings
{
    public class WmsApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;

        /// <summary>Company the WMS integrations are registered under.</summary>
        public int CompanyId { get; set; } = 62;

        /// <summary>WMS warehouse this packing station works in.</summary>
        public string WarehouseId { get; set; } = "6";
    }
}
