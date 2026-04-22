using System.ComponentModel;

namespace WarehousePacking.Contracts.Data.Enums
{
    public enum WarehouseTaskStatus
    {
        [Description("Zamknięty")]
        Closed,
        [Description("Nowy")]
        New,
        [Description("W realizacji")]
        InProgress,
    }
}
