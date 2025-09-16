using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs.Requests
{
    public class CreatePackageRequest
    {
        public int RouteId { get; set; }
        public int ClientId { get; set; }
        public int ClientAddressId { get; set; }
    }
}