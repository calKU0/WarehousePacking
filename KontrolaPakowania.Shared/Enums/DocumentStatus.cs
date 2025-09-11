using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.Enums
{
    public enum DocumentStatus
    {
        CloseOpenDocument = -3,
        Cancel = -2,
        Delete = -1,
        Confirm = 0,
        Bufor = 1,
        Print = 2,
        CloseWithoutPrint = -10,
        CloseAndPrint = 10
    }
}