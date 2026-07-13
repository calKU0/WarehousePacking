using System.ComponentModel;

namespace WarehousePacking.Contracts.Data.Enums
{
    public enum LogisticUnitStatus
    {
        [Description("Wolny")]
        Free = 0,

        [Description("W użyciu")]
        InUse = 1,

        [Description("Wyłączony")]
        Disabled = 2,

        [Description("W użyciu na przyjęciu")]
        InUseOnReceipt = 3,

        [Description("Zamknięta")]
        Closed = 4,

        [Description("W składowaniu")]
        InStorage = 5,

        [Description("W użyciu na wydaniu")]
        InUseOnDispatch = 6,

        [Description("Gotowa do sortowania")]
        ReadyForSorting = 9,

        [Description("W użyciu na sortowaniu")]
        InUseOnSorting = 11,

        [Description("Gotowa do pakowania")]
        ReadyForPacking = 12,

        [Description("W trakcie pakowania")]
        BeingPacked = 13,

        [Description("Gotowa do załadunku")]
        ReadyForLoading = 14,

        [Description("Załadowana")]
        Loaded = 15,

        [Description("Odblokowana")]
        Unlocked = 17,

        [Description("Zablokowana")]
        Locked = 18,

        [Description("Do rozłożenia")]
        ToBeUnpacked = 19
    }
}
