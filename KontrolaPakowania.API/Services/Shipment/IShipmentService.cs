using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;

namespace KontrolaPakowania.API.Services.Shipment
{
    public interface IShipmentService
    {
        Task<PackageData?> GetShipmentDataByBarcode(string barcode);

        Task<IEnumerable<Recipient>?> SearchAddress(string code);

        Task<IEnumerable<SearchInvoiceResult>?> SearchInvoice(string code);

        Task<bool> DeleteErpShipmentDocument(int wysNumber, int wysType);

        Task<int> CreateErpShipmentDocument(ShipmentResponse shipment);

        Task<bool> AddErpAttributes(int documentId, PackageData packageInfo);

        Task<RoutesStatus> GetRoutesStatus();

        Task<IEnumerable<RoutePackages>> GetRoutePackages(Courier courier);

        Task<int> CloseRoute(Courier courier);
    }
}