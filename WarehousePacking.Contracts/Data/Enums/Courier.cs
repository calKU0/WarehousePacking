using System.ComponentModel;

namespace WarehousePacking.Contracts.Data.Enums
{
    public enum Courier
    {
        [Description("Nieznany")]
        Unknown = 0,

        [Description("DPD")]
        DPD = 22,

        [Description("GLS")]
        GLS = 23,

        [Description("Fedex")]
        Fedex = 24,

        [Description("Hellmann")]
        Hellmann = 25,

        [Description("DPD-Romania")]
        DPD_Romania = 26,

        [Description("Schenker")]
        Schenker = 27,

        [Description("Odbiór własny")]
        Personal_Collection = 29,

        [Description("Raben")]
        Raben = 29,

        [Description("Trans. na zlecenie")]
        Transport_On_Request = 30,

        [Description("Transport odbiorcy")]
        Recipient_Transport = 31,

        [Description("Suus")]
        Suus = 32,

        [Description("Dachser")]
        Dachser = 33,

        [Description("Diera")]
        Diera = 34,

        [Description("Transport dostawcy")]
        Supplier_Transport = 35
    }
}