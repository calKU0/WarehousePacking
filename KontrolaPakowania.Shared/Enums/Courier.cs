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

        [Description("DPD-Romania")]
        DPD_Romania,

        [Description("Schenker")]
        Schenker,

        [Description("Hellmann")]
        Hellmann,

        [Description("Odbiór własny")]
        Personal_Collection,

        [Description("Raben")]
        Raben,

        [Description("Trans. na zlecenie")]
        Transport_On_Request,

        [Description("Transport odbiorcy")]
        Recipient_Transport,

        [Description("Transport dostawcy")]
        Supplier_Transport,

        [Description("Diera")]
        Diera,

        [Description("Dachser")]
        Dachser,

        [Description("Suus")]
        Suus,

        [Description("Nieznany")]
        Unknown,
    }
}