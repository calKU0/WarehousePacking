using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;

namespace WarehousePacking.Contracts.Services
{
    public interface IShipmentService
    {
        Task<PackageData?> GetShipmentDataByBarcode(string barcode);

        Task<IEnumerable<Recipient>?> SearchAddress(string code);

        Task<IEnumerable<SearchInvoiceResult>?> SearchInvoice(string code);

        Task<bool> DeleteErpShipmentDocument(int wysNumber, int wysType);

        Task<int> CreateErpShipmentDocument(ShipmentResponse shipment);

        Task<bool> AddErpAttributes(int documentId, ShipmentResponse shipmentInfo);

        Task<RoutesStatus> GetRoutesStatus();

        Task<IEnumerable<RoutePackages>> GetRoutePackages(Courier courier);

        Task<int> CloseRoute(Courier courier);
        Task<bool> IsPackageReadyToShip(string barcode);
    }
}