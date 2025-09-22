using KontrolaPakowania.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs.Requests
{
    public class CreatePackageRequest
    {
        public string Username { get; set; } = string.Empty;
        public Courier Courier { get; set; }
        public int ClientId { get; set; }
        public int ClientAddressId { get; set; }
        public PackingWarehouse PackageWarehouse { get; set; }
        public PackingLevel PackingLevel { get; set; }

        public string StationNumber { get; set; } = string.Empty;
    }
}