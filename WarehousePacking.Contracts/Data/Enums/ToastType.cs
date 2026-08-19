namespace WarehousePacking.Contracts.Data.Enums
{
    /// <summary>
    /// Severity of a notification. It picks the colour, the icon and how long
    /// the message stays up (see Toast.razor).
    ///
    ///   Success / Info  something went through, or a plain remark.
    ///   Warning         the action was refused for a reason the operator can
    ///                   act on: a wrong password, a rule that says no, nothing
    ///                   found. Expected, recoverable, nothing broke.
    ///   Error           something actually failed: an exception, or an
    ///                   operation the system could not carry out.
    /// </summary>
    public enum ToastType
    {
        Success,
        Error,
        Info,
        Warning
    }
}