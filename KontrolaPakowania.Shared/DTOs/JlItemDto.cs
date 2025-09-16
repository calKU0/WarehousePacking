using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs
{
    public class JlItemDto
    {
        public int Id { get; set; }
        public int PositionNumber { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string SupplierCode { get; set; } = "";
        public int DocumentId { get; set; }
        public int DocumentType { get; set; }
        public decimal DocumentQuantity { get; set; }
        public decimal JlQuantity { get; set; }
        public string Unit { get; set; } = "";
        public decimal Weight { get; set; }
        public decimal Volume { get; set; }
        public string Country { get; set; } = "";
        public string JlCode { get; set; } = "";
        public string ProductType { get; set; } = "";
        public byte[]? Image { get; set; }
    }
}