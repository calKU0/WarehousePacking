using KontrolaPakowania.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs
{
    public class PackageInfo
    {
        public int Id { get; set; }
        public Courier Courier { get; set; }
        public string RecipientName { get; set; } = string.Empty;
        public string RecipientCity { get; set; } = string.Empty;
        public string RecipientStreet { get; set; } = string.Empty;
        public string RecipientPostalCode { get; set; } = string.Empty;
        public string RecipientCountry { get; set; } = string.Empty;
        public string RecipientPhone { get; set; } = string.Empty;
        public string RecipientEmail { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string References { get; set; } = string.Empty;
        public int PackageQuantity { get; set; } = 1;
        public decimal Weight { get; set; }
        public ShipmentServices Services { get; set; } = new ShipmentServices();
    }

    public class ShipmentServices
    {
        public bool POD { get; set; }
        public bool EXW { get; set; }
        public bool ROD { get; set; }
        public bool S10 { get; set; }
        public bool S12 { get; set; }
        public bool Saturday { get; set; }
        public bool COD { get; set; }
        public decimal CODAmount { get; set; }
    }
}