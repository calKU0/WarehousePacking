namespace WarehousePacking.Server.Shared.Components.Modals
{
    /// <summary>
    /// The supervisor actions offered by <see cref="ManagerControlModal"/> and
    /// handled by the packing screens.
    ///
    /// The numbers are the ones the panel used to pass around as bare ints and
    /// are kept as-is, so nothing shifts meaning: ids 1, 3 and 10 belong to
    /// actions that were retired from the panel but are still reachable from
    /// code, and staying explicit about that is cheaper than renumbering.
    /// </summary>
    public enum ManagerAction
    {
        Labels = 1,
        PackAll = 2,
        Contents = 3,
        Courier = 4,
        LoggedOperators = 5,
        JlsInProgress = 6,
        ReleasePackage = 7,
        ChangeWarehouse = 8,
        MergePackages = 9,
        SendToBuffer = 10,
        CourierConfiguration = 11,
        ChangePassword = 12
    }
}
