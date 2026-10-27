namespace WarehousePacking.Contracts.DTOs
{
    /// <summary>
    /// A barcode from the ERP that belongs to a bulk package of an item (e.g.
    /// "opak." = 10 szt.) rather than to a single piece.
    /// </summary>
    public class ItemPackageCode
    {
        public string Code { get; set; } = string.Empty;
        public int ItemErpId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string BaseUnit { get; set; } = string.Empty;
        public string PackageUnit { get; set; } = string.Empty;

        /// <summary>How many base units one package holds.</summary>
        public decimal QuantityPerPackage { get; set; }
    }
}
