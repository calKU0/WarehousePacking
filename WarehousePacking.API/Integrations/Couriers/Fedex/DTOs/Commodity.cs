using System.Collections.Generic;
using System;

namespace WarehousePacking.API.Integrations.Couriers.Fedex.DTOs
{
    public class Commodity
    {
        public UnitPrice? UnitPrice { get; set; }
        public int? NumberOfPieces { get; set; }
        public int? Quantity { get; set; }
        public string? QuantityUnits { get; set; }
        public CustomsValue? CustomsValue { get; set; }
        public string? CountryOfManufacture { get; set; }
        public string? CIMarksAndNumbers { get; set; }
        public string? HarmonizedCode { get; set; }
        public string? Description { get; set; }
        public string? Name { get; set; }
        public Weight? Weight { get; set; }
        public string? ExportLicenseNumber { get; set; }
        public DateTime? ExportLicenseExpirationDate { get; set; }
        public string? PartNumber { get; set; }
        public string? Purpose { get; set; }
    }
}