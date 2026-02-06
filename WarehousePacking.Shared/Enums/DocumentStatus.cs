using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarehousePacking.Shared.Enums
{
    public enum DocumentStatus
    {
        InProgress = 1,
        Bufor = 2,
        Ready = 3,
        Cancel = 6,
        Delete = -1,
    }
}