using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.DTOs
{
    public class Recipient
    {
        public int GidNumber { get; set; }
        public int GidType { get; set; }
        public string Acronym { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NIP { get; set; } = string.Empty;
        public int Type { get; set; }
    }
}