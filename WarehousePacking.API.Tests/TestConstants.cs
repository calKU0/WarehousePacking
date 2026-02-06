using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.API.Tests
{
    public static class TestConstants
    {
        // Route / Client data
        public const int RouteId = 22;

        public const int ClientAddressId = 515921;
        public const int ClientAddressType = 864;
        public const int ClientId = 7237;
        public const string Username = "KURKRZ";

        // Source Documents
        public const int SourceDocumentId = 1944244;

        public const int SourceDocumentType = 2033;

        // Packages
        public const int PackageId = 10630;

        public const string PackageBarcode = "3002209000023";
        public const string NonPLPackageBarcode = "999999999999999999";

        // Shipments
        public const int ShipmentId = 10792;
    }
}