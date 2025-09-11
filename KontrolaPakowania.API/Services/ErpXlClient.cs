using cdn_api;
using KontrolaPakowania.API.Services.Exceptions;
using KontrolaPakowania.API.Services.Interfaces;
using KontrolaPakowania.Shared.DTOs.Requests;
using KontrolaPakowania.Shared.Enums;
using KontrolaPakowania.Shared.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;

namespace KontrolaPakowania.API.Services
{
    public class ErpXlClient : IErpXlClient
    {
        [DllImport("ClaRUN.dll")]
        public static extern void AttachThreadToClarion(int _flag);

        private readonly XlApiSettings _config;
        private readonly IServiceProvider _serviceProvider;
        private int _sessionId;

        public ErpXlClient(IOptions<XlApiSettings> config, IServiceProvider serviceProvider)
        {
            _config = config.Value;
            _serviceProvider = serviceProvider;
        }

        public int Login()
        {
            XLLoginInfo_20241 xLLoginInfo = new()
            {
                Wersja = _config.ApiVersion,
                ProgramID = _config.ProgramName,
                Baza = _config.Database,
                OpeIdent = _config.Username,
                OpeHaslo = _config.Password,
                TrybWsadowy = 1
            };

            int result = cdn_api.cdn_api.XLLogin(xLLoginInfo, ref _sessionId);
            if (result != 0)
            {
                throw new XlApiException(result, "Failed to login to ERP XL API");
            }
            return result;
        }

        public int Logout()
        {
            AttachThreadToClarion(1);
            XLLogoutInfo_20241 xLLogoutInfo = new()
            {
                Wersja = _config.ApiVersion,
            };

            int result = cdn_api.cdn_api.XLLogout(_sessionId);
            if (result != 0)
            {
                throw new XlApiException(result, $"Błąd wylogowania XL.");
            }
            return result;
        }

        public int CreatePackage(OpenPackageRequest request)
        {
            AttachThreadToClarion(1);
            ManageTransaction(0); // Open transaction

            int resultId = 0;

            XLPaczkaInfo_20241 xLPaczka = new()
            {
                Wersja = _config.ApiVersion,
                Tryb = 2, // 1 - Interactive, 2 - Wsadowy,
                NaKoszt = 0, // 0 - Nasz, 1 - Klienta
                TrasaID = request.RouteId,
            };

            var result = cdn_api.cdn_api.XLNowaPaczka(_sessionId, ref resultId, xLPaczka);
            if (result != 0)
            {
                string errorMessage = CheckError((int)ErrorCode.NowaPaczka, result);
                ManageTransaction(2); // Close transaction
                throw new XlApiException(result, $"Nie udało się utworzyć paczki w XL. {errorMessage}");
            }

            ManageTransaction(1); // Commit transaction
            return resultId;
        }

        public bool AddPositionToPackage(AddPackedPositionRequest request)
        {
            bool isSuccess = true;
            AttachThreadToClarion(1);
            ManageTransaction(0); // Open transaction

            XLSpiInfo_20241 xLSpiInfo = new()
            {
                Wersja = _config.ApiVersion,
                TrNNumer = request.DocumentId,
                TrNTyp = request.DocumentType,
                TrNFirma = 449892,
                TrNLp = 0,
                TrNSubLp = 1,
                Ilosc = "1.00"
            };

            var result = cdn_api.cdn_api.XLDodajDokumentDoPaczki(request.DocumentRef, xLSpiInfo);
            if (result != 0)
            {
                string errorMessage = CheckError((int)ErrorCode.DodajDokumentDoPaczki, result);
                ManageTransaction(2); // Close transaction
                isSuccess = false;
                throw new XlApiException(result, $"Nie udało się dodać pozycji dokumentu do paczki w XL. ID dokumentu: {request.DocumentId}, LP: {request.PositionNumber}. {errorMessage}");
            }

            ManageTransaction(1); // Commit transaction
            return isSuccess;
        }

        public bool RemovePositionFromPackage(RemovePackedPositionRequest request)
        {
            bool isSuccess = true;
            AttachThreadToClarion(1);
            ManageTransaction(0); // Open transaction

            XLSpiInfo_20241 xLSpiInfo = new()
            {
                Wersja = _config.ApiVersion,
                TrNNumer = request.DocumentId,
                TrNTyp = request.DocumentType,
                TrNFirma = 449892,
                TrNLp = request.PositionNumber,
                Ilosc = request.Quantity,
            };

            var result = cdn_api.cdn_api.XLUsunDokumentZPaczki(request.DocumentRef, xLSpiInfo);
            if (result != 0)
            {
                string errorMessage = CheckError((int)ErrorCode.UsunDokumentZPaczki, result);
                ManageTransaction(2); // Close transaction
                isSuccess = false;
                throw new XlApiException(result, $"Nie udało się usunąć pozycji dokumentu z paczki w XL. ID dokumentu: {request.DocumentId}, LP: {request.PositionNumber}. {errorMessage}");
            }

            ManageTransaction(1); // Commit transaction
            return isSuccess;
        }

        public int ClosePackage(ClosePackageRequest request)
        {
            AttachThreadToClarion(1);
            ManageTransaction(0); // Open transaction

            XLZamknieciePaczkiInfo_20241 xLZamknieciePaczkiInfo = new()
            {
                Wersja = _config.ApiVersion,
                Tryb = Convert.ToInt32(request.Status)
            };

            var result = cdn_api.cdn_api.XLZamknijPaczke(request.DocumentRef, xLZamknieciePaczkiInfo);
            if (result != 0)
            {
                string errorMessage = CheckError((int)ErrorCode.ZamknijPaczke, result);
                ManageTransaction(2); // Close transaction
                throw new XlApiException(result, $"Nie udało się zamknąć paczki w XL. {errorMessage}");
            }

            ManageTransaction(1); // Commit transaction
            return xLZamknieciePaczkiInfo.ID;
        }

        private string CheckError(int function, int errorCode)
        {
            XLKomunikatInfo_20241 xLKomunikat = new()
            {
                Wersja = _config.ApiVersion,
                Funkcja = function,
                Blad = errorCode,
                Tryb = 0
            };
            int result = cdn_api.cdn_api.XLOpisBledu(xLKomunikat);

            if (result == 0)
                return xLKomunikat.OpisBledu;
            else
                return $"Error while checking error. Error code: {result}";
        }

        private int ManageTransaction(int type, string token = "")
        {
            XLTransakcjaInfo_20241 xLTransakcja = new()
            {
                Wersja = _config.ApiVersion,
                Tryb = type
            };
            int result = cdn_api.cdn_api.XLTransakcja(_sessionId, xLTransakcja);
            return result;
        }
    }
}