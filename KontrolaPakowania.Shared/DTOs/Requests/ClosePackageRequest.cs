using KontrolaPakowania.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.DTOs.Requests
{
    public class ClosePackageRequest
    {
        public int DocumentRef { get; set; }
        public int DocumentId { get; set; }
        public string InternalBarcode { get; set; } = string.Empty;
        public DocumentStatus Status { get; set; }
    }
}