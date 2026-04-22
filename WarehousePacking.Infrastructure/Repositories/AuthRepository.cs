using System.Data;
using WarehousePacking.Contracts.Data.Enums;
using WarehousePacking.Contracts.DTOs;
using WarehousePacking.Contracts.Repositories;
using WarehousePacking.Infrastructure.Data;

namespace WarehousePacking.Infrastructure.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly IDbExecutor _context;
        public AuthRepository(IDbExecutor context)
        {
            _context = context;
        }

        public async Task ChangePassword(string newPassword)
        {
            const string procedure = "kp.ChangeManagerPassword";
            await _context.QuerySingleOrDefaultAsync<int>(procedure, new { newPassword }, commandType: CommandType.StoredProcedure, connection: Connection.ERPConnection);
        }

        public async Task<IEnumerable<LoginDto>> GetLoggedUsersAsync()
        {
            const string procedure = "kp.GetLoggedUsers";
            return await _context.QueryAsync<LoginDto>(procedure, commandType: CommandType.StoredProcedure, connection: Connection.ERPConnection);
        }

        public async Task<string> Login(LoginDto login)
        {
            const string procedure = "kp.LoginUser";
            var result = await _context.QuerySingleOrDefaultAsync<string>(procedure, new { username = login.Username, password = login.Password, stationNumber = login.StationNumber }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result;
        }

        public async Task<bool> LogoutAsync(string username)
        {
            const string procedure = "kp.LogoutUser";
            var rows = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { username }, CommandType.StoredProcedure, Connection.ERPConnection);
            return rows > 0;
        }

        public async Task<bool> ValidatePasswordAsync(string password)
        {
            const string procedure = "kp.ValidatePassword";
            var result = await _context.QuerySingleOrDefaultAsync<int>(procedure, new { password }, CommandType.StoredProcedure, Connection.ERPConnection);
            return result == 1;
        }
    }
}
