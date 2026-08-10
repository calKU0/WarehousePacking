namespace WarehousePacking.Contracts.DTOs.Dashboards
{
    public class DashboardColorConfiguration
    {
        public int ZoneLastActivityThresholdMinutes { get; set; }
        public int OperatorLastActivityYellowThresholdMinutes { get; set; }
        public int OperatorLastActivityRedThresholdMinutes { get; set; }
        public int PackingDownBlueThreshold { get; set; }
        public int PackingDownRedThreshold { get; set; }
        public int PackingDownPackingTimeYellowThreshold { get; set; }
        public int PackingDownPackingTimeRedThreshold { get; set; }
        public int PackingUpBlueThreshold { get; set; }
        public int PackingUpRedThreshold { get; set; }
        public int PackingUpPackingTimeYellowThreshold { get; set; }
        public int PackingUpPackingTimeRedThreshold { get; set; }
        public int SortingBlueThreshold { get; set; }
        public int SortingRedThreshold { get; set; }

        // --- Personal collection (Odbiór własny) -----------------------------
        // Unlike every threshold above, these rows are not coloured by how long
        // they have waited but by which status they are in, so the config is a
        // list of WarehouseTaskStatus ids rather than a number of minutes.
        // Comma-separated, e.g. "18" or "16,18". A status in none of the three
        // lists is left uncoloured.
        //
        // The defaults below apply until kp.GetDashboardColorConfiguration
        // returns columns of these names — Dapper leaves a property untouched
        // when the result set has no matching column.

        /// <summary>Statuses painted green. Defaults to 18 = ReadyForLoading.</summary>
        public string? PersonalCollectionReadyStatuses { get; set; } = "18";

        /// <summary>Statuses painted amber. Empty by default.</summary>
        public string? PersonalCollectionWarningStatuses { get; set; }

        /// <summary>Statuses painted red. Empty by default.</summary>
        public string? PersonalCollectionErrorStatuses { get; set; }
    }
}
