using System.ComponentModel;

namespace WarehousePacking.Contracts.Data.Enums
{
    public enum WarehouseTaskStatus
    {
        [Description("Nowy")]
        New = 0,

        [Description("Zamknięty")]
        Closed = 1,

        [Description("Anulowany")]
        Canceled = 2,

        [Description("W trakcie realizacji")]
        InExecution = 3,

        [Description("Zrealizowany")]
        CompletedSuccessfully = 5,

        [Description("Zrealizowany częściowo")]
        PartiallyCompletedSuccessfully = 9,

        [Description("Dokument tymczasowy")]
        TemporaryDocument = 10,

        [Description("Do realizacji")]
        ReadyForExecution = 12,

        [Description("W realizacji")]
        Processing = 13,

        [Description("Nowy - zarezerwowany")]
        NewReserved = 14,

        [Description("Do ponownego przetworzenia")]
        ToReprocess = 15,

        [Description("Skompletowany")]
        Picked = 16,

        [Description("Skompletowany częściowo")]
        PartiallyPicked = 17,

        [Description("Gotowy do załadunku")]
        ReadyForLoading = 18
    }
}
