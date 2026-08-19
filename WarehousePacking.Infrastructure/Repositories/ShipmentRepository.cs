using System.Data;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.DTOs.Requests;
using WarehousePacking.Contracts.Repositories;
using WarehousePacking.Infrastructure.Data;

namespace WarehousePacking.Infrastructure.Repositories
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly IDbExecutor _context;

        public ShipmentRepository(IDbExecutor context)
        {
            _context = context;
        }

        public async Task<int> CreateErpShipmentDocument(ShipmentResponse shipment, string courier)
        {
            const string procedure = "kp.CreateShipmentDocument";
            return await _context.QuerySingleOrDefaultAsync<int>(procedure, new { shipment.PackageId, shipment.TrackingNumber, shipment.TrackingLink, shipment.PackageInfo.ShipmentServices.CODAmount, shipment.PackageInfo.Insurance, courier }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<bool> DeleteErpShipmentDocument(int wysNumber, int wysType)
        {
            const string procedure = "kp.DeleteShipmentDocument";
            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { wysNumber, wysType }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result >= 1;
        }

        public async Task<bool> AddErpAttributes(int documentId, ShipmentResponse shipmentInfo)
        {
            var packageInfo = shipmentInfo.PackageInfo;
            const string procedure = "kp.AddShipmentAttributes";

            var rod = packageInfo.ShipmentServices.ROD ? "TAK" : "NIE";
            var pod = packageInfo.ShipmentServices.POD ? "TAK" : "NIE";
            var exw = packageInfo.ShipmentServices.EXW ? "TAK" : "NIE";
            var s10 = packageInfo.ShipmentServices.D10 ? "TAK" : "NIE";
            var s12 = packageInfo.ShipmentServices.D12 ? "TAK" : "NIE";
            var saturday = packageInfo.ShipmentServices.Saturday ? "TAK" : "NIE";
            var cod = packageInfo.ShipmentServices.COD ? "TAK" : "NIE";
            var hasInvoice = packageInfo.HasInvoice ? "TAK" : "NIE";
            var manualEdit = packageInfo.ManualEdit ? "TAK" : "NIE";
            var manualSend = packageInfo.ManualSend ? "TAK" : "NIE";

            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { POD = pod, ROD = rod, EXW = exw, S10 = s10, S12 = s12, Saturday = saturday, COD = cod, HasInvoice = hasInvoice, ManualEdit = manualEdit, ManualSend = manualSend, shipmentInfo.ExternalId, documentId }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result > 0;
        }

        public async Task<PackageData?> GetShipmentDataByBarcode(string barcode)
        {
            const string procedure = "kp.GetPackageData";

            return await _context.QuerySingleOrDefaultAsync<PackageData, Recipient, ShipmentServices>
            (
                procedure,
                (pkg, recipient, services) =>
                {
                    pkg.ShipmentServices = services;
                    pkg.Recipient = recipient;
                    return pkg;
                },
                splitOn: "GidNumber,POD",
                param: new { barcode },
                commandType: CommandType.StoredProcedure,
                connectionName: Connection.ERPConnection
            );
        }

        public async Task<IEnumerable<Recipient>?> SearchAddress(string code)
        {
            const string procedure = "kp.SearchAddress";
            return await _context.QueryAsync<Recipient>(procedure, new { code }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<IEnumerable<SearchInvoiceResult>?> SearchInvoice(string code)
        {
            const string procedure = "kp.SearchInvoice";
            return await _context.QueryAsync<SearchInvoiceResult>(procedure, new { code }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<RoutesStatus> GetRoutesStatus()
        {
            const string procedure = "kp.GetRoutesStatus";
            return await _context.QuerySingleOrDefaultAsync<RoutesStatus>(procedure, param: null, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<IEnumerable<RoutePackages>> GetRoutePackages(string courierName)
        {
            const string procedure = "kp.GetRoutePackages";
            return await _context.QueryAsync<RoutePackages>(procedure, new { courier = courierName }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<int> CloseRoute(string courierName)
        {
            const string procedure = "kp.CloseRoute";
            return await _context.QuerySingleOrDefaultAsync<int>(procedure, new { courier = courierName }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<bool> IsPackageReadyToShip(string barcode)
        {
            const string procedure = "kp.IsPackageReadyToShip";
            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { Barcode = barcode }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result == 1;
        }
    }
}
