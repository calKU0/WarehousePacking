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
        public string LogoCourier { get; set; } = "";
        public CourierServices CourierServices { get; set; } = new();
        public int RouteId { get; set; }
        public int Priority { get; set; }
        public int Sorting { get; set; }
        public bool OutsideEU { get; set; } = false;
        public string ClientName { get; set; } = "";
        public int ClientAddressId { get; set; }
    }

    public class CourierServices
    {
        public bool Return { get; set; }
        public bool _12 { get; set; }
        public bool Saturday { get; set; }
        public bool Dropshipping { get; set; }
    }
}