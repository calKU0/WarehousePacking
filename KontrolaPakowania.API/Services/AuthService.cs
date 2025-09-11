using KontrolaPakowania.API.Data;
using KontrolaPakowania.API.Data.Enums;
using KontrolaPakowania.API.Services.Interfaces;
using KontrolaPakowania.Shared.DTOs;
using System.Data;

namespace KontrolaPakowania.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDbExecutor _db;

        public AuthService(IDbExecutor db)
        {
            _db = db;
        }

        public async Task<bool> Login(string username, string password)
        {
            const string procedure = "kp.ValidateCredentials";
            var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { username, password }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result == 1;
        }

        public async Task<bool> ValidatePasswordAsync(string password)
        {
            const string procedure = "kp.ValidatePassword";
            var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { password }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result == 1;
        }
    }
}