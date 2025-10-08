using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services.Shipment.GLS;
using KontrolaPakowania.Shared.DTOs;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Helpers;
using System.Data;
using System.Reflection.Metadata;

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
            return await _db.QuerySingleOrDefaultAsync<int>(procedure, new { shipment.PackageId, shipment.TrackingNumber, shipment.TrackingLink, shipment.PackageInfo.ShipmentServices.CODAmount, shipment.PackageInfo.Insurance }, CommandType.StoredProcedure, Connection.ERPConnection);
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

            var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { POD, ROD, EXW, S10, S12, Saturday, COD, HasInvoice, documentId }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result == 8;
        }

        public async Task<PackageData?> GetShipmentDataByBarcode(string barcode)
        {
            const string procedure = "kp.GetPackageData";

            var result = await _db.QuerySingleOrDefaultAsync<PackageData, ShipmentServices>(
                procedure,
                (pkg, services) => { pkg.ShipmentServices = services; return pkg; },
                "POD",
                new { barcode },
                CommandType.StoredProcedure,
                Connection.ERPConnection
            );

            if (result is not null)
            {
                result.Courier = CourierHelper.GetCourierFromName(result.CourierName);
                result.ShipmentServices = ShipmentServices.FromString(result.CourierName);
            }

            return result;
        }
    }
}