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

        public async Task<bool> Login(LoginDto login)
        {
            if (string.IsNullOrEmpty(login.Username) || string.IsNullOrEmpty(login.StationNumber) || string.IsNullOrEmpty(login.Password))
                throw new ArgumentException("Username, Password and StationNumber cannot be empty");

            const string procedure = "kp.LoginUser";
            var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { username = login.Username, password = login.Password, stationNumber = login.StationNumber }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result == 1;
        }

        public async Task<IEnumerable<LoginDto>> GetLoggedUsersAsync()
        {
            const string procedure = "kp.GetLoggedUsers";
            return await _db.QueryAsync<LoginDto>(procedure, commandType: CommandType.StoredProcedure, connection: Connection.ERPConnection);
        }

        public async Task<bool> LogoutAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
                throw new ArgumentException("Username cannot be empty");

            const string procedure = "kp.LogoutUser";
            var rows = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { username }, CommandType.StoredProcedure, Connection.ERPConnection);
            return rows > 0;
        }

        public async Task<bool> ValidatePasswordAsync(string password)
        {
            const string procedure = "kp.ValidatePassword";
            var result = await _db.QuerySingleOrDefaultAsync<int>(procedure, new { password }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result == 1;
        }
    }
}