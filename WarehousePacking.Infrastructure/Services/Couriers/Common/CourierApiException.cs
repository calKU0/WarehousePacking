namespace WarehousePacking.Infrastructure.Services.Couriers.Common
{
    /// <summary>
    /// A courier rejected a request or answered with something unusable. The
    /// message is written for the operator: it ends up on the packing station
    /// screen and, for invalid package data, in the e-mail sent to the representative.
    /// </summary>
    public sealed class CourierApiException : Exception
    {
        private CourierApiException(string message, bool isInvalidPackageData, Exception? innerException)
            : base(message, innerException)
        {
            IsInvalidPackageData = isInvalidPackageData;
        }

        public CourierApiException(string message, Exception? innerException = null)
            : this(message, isInvalidPackageData: false, innerException)
        {
        }

        /// <summary>
        /// The courier refused the package's own data — address, postal code,
        /// phone, weight and the like. Only these are worth the representative's
        /// attention: they have to correct the document before it can be resent.
        /// </summary>
        public bool IsInvalidPackageData { get; }

        public static CourierApiException InvalidPackageData(string message, Exception? innerException = null) =>
            new(message, isInvalidPackageData: true, innerException);
    }
}
