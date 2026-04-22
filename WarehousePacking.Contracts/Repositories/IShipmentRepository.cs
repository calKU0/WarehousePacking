using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;

namespace WarehousePacking.Contracts.Repositories
{
    public interface IShipmentRepository
    {
        Task<int> CreateErpShipmentDocument(ShipmentResponse shipment, string courier);
        Task<bool> DeleteErpShipmentDocument(int wysNumber, int wysType);
        Task<bool> AddErpAttributes(int documentId, ShipmentResponse shipmentInfo);
        Task<PackageData?> GetShipmentDataByBarcode(string barcode);
        Task<IEnumerable<Recipient>?> SearchAddress(string code);
        Task<IEnumerable<SearchInvoiceResult>?> SearchInvoice(string code);
        Task<RoutesStatus> GetRoutesStatus();
        Task<IEnumerable<RoutePackages>> GetRoutePackages(string courierName);
        Task<int> CloseRoute(string courierName);
    }
}
