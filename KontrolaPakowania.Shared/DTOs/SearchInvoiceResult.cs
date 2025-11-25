using KontrolaPakowania.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs
{
    public class SearchInvoiceResult
    {
        public int InvoiceId { get; set; }
        public string InvoiceName { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public string ClientAcronym { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
    }
}