using Dapper;
using KontrolaPakowania.API.Data.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace KontrolaPakowania.API.Data
{
    public class DapperDbExecutor : IDbExecutor
    {
        private readonly IConfiguration _config;

        public DapperDbExecutor(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CommandType? commandType = null, Connection connectionName = Connection.WMSConnection)
        {
            var connectionString = _config.GetConnectionString(connectionName.ToString())
                ?? throw new InvalidOperationException($"Connection string '{connectionName}' not found.");

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            return await connection.QueryAsync<T>(sql, param, commandType: commandType);
        }

        public async Task<T?> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, CommandType? commandType = null, Connection connectionName = Connection.WMSConnection)
        {
            var connectionString = _config.GetConnectionString(connectionName.ToString())
                ?? throw new InvalidOperationException($"Connection string '{connectionName}' not found.");

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            return await connection.QuerySingleOrDefaultAsync<T>(sql, param, commandType: commandType);
        }

    }
}