using System.ComponentModel;

namespace WarehousePacking.Contracts.Data.Enums
{
    public enum MonitorPanel
    {
        [Description("Picking")]
        Picking = 0,

        [Description("Sortowanie")]
        Sorting = 1,

        [Description("Pakowanie")]
        Packing = 2,

        [Description("Załadunek")]
        Loading = 3,

        [Description("Przyjęcie")]
        Receiving = 4,

        [Description("Zatowarowanie")]
        Stocking = 5,
    }
}