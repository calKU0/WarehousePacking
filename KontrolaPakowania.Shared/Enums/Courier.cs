using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KontrolaPakowania.Shared.Enums
{
    public enum Courier
    {
        [Description("DPD")]
        DPD,
        [Description("GLS")]
        GLS,
        [Description("Fedex")]
        Fedex,
        [Description("DPD Romania")]
        DPD_Romania,
        [Description("Shenker")]
        Schenker,
        [Description("Hellmann")]
        Hellmann,
        [Description("Odbiór własny")]
        Personal_Collection,
        [Description("Nieznany")]
        Unknown,
    }
}