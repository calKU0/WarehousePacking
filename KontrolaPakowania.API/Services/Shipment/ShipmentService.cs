using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services.Shipment.GLS;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using KontrolaPakowania.Shared.Helpers;
using System.Data;
using System.Reflection.Metadata;
using static KontrolaPakowania.API.Integrations.Couriers.DPD_Romania.DTOs.DpdRomaniaCreateShipmentRequest;

namespace KontrolaPakowania.API.Services.Shipment
{
    public class ShipmentService : IShipmentService
    {
        private readonly IDbExecutor _db;

        public ShipmentService(IDbExecutor db)
        {
            _db = db;
        }

        public async Task<int> CreateErpShipmentDocument(ShipmentResponse shipment)
        {
            const string procedure = "kp.CreateShipmentDocument";
            string courier = shipment.PackageInfo.Courier.GetDescription();
            return await _db.QuerySingleOrDefaultAsync<int>(procedure, new { shipment.PackageId, shipment.TrackingNumber, shipment.TrackingLink, shipment.PackageInfo.ShipmentServices.CODAmount, shipment.PackageInfo.Insurance, courier }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<bool> DeleteErpShipmentDocument(int wysNumber, int wysType)
        {
            const string procedure = "kp.DeleteShipmentDocument";
            var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { wysNumber, wysType }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result >= 1;
        }

        public async Task<bool> AddErpAttributes(int documentId, PackageData packageInfo)
        {
            const string procedure = "kp.AddShipmentAttributes";

            string ROD = packageInfo.ShipmentServices.ROD ? "TAK" : "NIE";
            string POD = packageInfo.ShipmentServices.POD ? "TAK" : "NIE";
            string EXW = packageInfo.ShipmentServices.EXW ? "TAK" : "NIE";
            string S10 = packageInfo.ShipmentServices.D10 ? "TAK" : "NIE";
            string S12 = packageInfo.ShipmentServices.D12 ? "TAK" : "NIE";
            string Saturday = packageInfo.ShipmentServices.Saturday ? "TAK" : "NIE";
            string COD = packageInfo.ShipmentServices.COD ? "TAK" : "NIE";
            string HasInvoice = packageInfo.HasInvoice ? "TAK" : "NIE";
            string ManualEdit = packageInfo.ManualEdit ? "TAK" : "NIE";
            string ManualSend = packageInfo.ManualSend ? "TAK" : "NIE";

            var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { POD, ROD, EXW, S10, S12, Saturday, COD, HasInvoice, ManualEdit, ManualSend, documentId }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result == 8;
        }

        public async Task<PackageData?> GetShipmentDataByBarcode(string barcode)
        {
            const string procedure = "kp.GetPackageData";

            var result = await _db.QuerySingleOrDefaultAsync<PackageData, Recipient, ShipmentServices>
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

            if (result is not null)
            {
                result.Courier = CourierHelper.GetCourierFromName(result.CourierName);
                result.ShipmentServices = ShipmentServices.FromString(result.CourierName);
            }

            return result;
        }

        public async Task<IEnumerable<Recipient>?> SearchAddress(string code)
        {
            const string procedure = "kp.SearchAddress";
            return await _db.QueryAsync<Recipient>(procedure, new { code }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<IEnumerable<SearchInvoiceResult>?> SearchInvoice(string code)
        {
            const string procedure = "kp.SearchInvoice";
            return await _db.QueryAsync<SearchInvoiceResult>(procedure, new { code }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<RoutesStatus> GetRoutesStatus()
        {
            const string procedure = "kp.GetRoutesStatus";
            return await _db.QuerySingleOrDefaultAsync<RoutesStatus>(procedure, param: null, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<IEnumerable<RoutePackages>> GetRoutePackages(Courier courier)
        {
            const string procedure = "kp.GetRoutePackages";
            string courierName = courier.GetDescription();
            return await _db.QueryAsync<RoutePackages>(procedure, new { courier = courierName }, CommandType.StoredProcedure, Connection.ERPConnection);
        }

        public async Task<int> CloseRoute(Courier courier)
        {
            const string procedure = "kp.CloseRoute";
            string courierName = courier.GetDescription();
            return await _db.QuerySingleOrDefaultAsync<int>(procedure, new { courier = courierName }, CommandType.StoredProcedure, Connection.ERPConnection);
        }
    }
}