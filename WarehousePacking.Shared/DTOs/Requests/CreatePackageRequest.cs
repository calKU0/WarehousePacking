using WarehousePacking.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.DTOs.Requests
{
    public class CreatePackageRequest
    {
        public string Username { get; set; } = string.Empty;
        public Courier Courier { get; set; }
        public int ClientId { get; set; }
        public string AddressName { get; set; } = string.Empty;
        public string AddressCity { get; set; } = string.Empty;
        public string AddressStreet { get; set; } = string.Empty;
        public string AddressPostalCode { get; set; } = string.Empty;
        public string AddressCountry { get; set; } = string.Empty;
        public PackingWarehouse PackageWarehouse { get; set; }
        public PackingLevel PackingLevel { get; set; }

        public string StationNumber { get; set; } = string.Empty;
    }
}