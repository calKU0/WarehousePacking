using System.ComponentModel;

namespace WarehousePacking.Contracts.Data.Enums
{
    public enum DashboardPanel
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

        [Description("Odbiór Własny")]
        PersonalCollection = 5,

        [Description("Zatowarowanie")]
        Stocking = 6,
    }
}