using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs
{
    public class JlDto
    {
        public int Id { get; set; }
        public string Barcode { get; set; } = "";
        public string Name { get; set; } = "";
        public int Status { get; set; }
        public decimal Weight { get; set; }
        public string Courier { get; set; } = "";
        public int RouteId { get; set; }
        public int Priority { get; set; }
        public int Sorting { get; set; }
        public bool OutsideEU { get; set; } = false;
        public string ClientName { get; set; } = "";
        public int ClientAddressId { get; set; }
    }
}