using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs
{
    public class ClientDetails
    {
        public int AddressId { get; set; }
        public int AddressType { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}