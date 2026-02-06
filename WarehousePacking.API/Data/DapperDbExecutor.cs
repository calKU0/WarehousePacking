using Dapper;
using WarehousePacking.API.Data.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace WarehousePacking.API.Data
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

        public async Task<TFirst?> QuerySingleOrDefaultAsync<TFirst, TSecond>(string sql, Func<TFirst, TSecond, TFirst> map, string splitOn, object? param = null, CommandType? commandType = null, Connection connectionName = Connection.WMSConnection)
        {
            var connectionString = _config.GetConnectionString(connectionName.ToString())
                ?? throw new InvalidOperationException($"Connection string '{connectionName}' not found.");

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var result = await connection.QueryAsync<TFirst, TSecond, TFirst>(sql, map, param, commandType: commandType, splitOn: splitOn);

            return result.FirstOrDefault();
        }

        public async Task<TFirst?> QuerySingleOrDefaultAsync<TFirst, TSecond, TThird>(string sql, Func<TFirst, TSecond, TThird, TFirst> map, string splitOn, object? param = null, CommandType? commandType = null,Connection connectionName = Connection.WMSConnection)
        {
            var connectionString = _config.GetConnectionString(connectionName.ToString())
                ?? throw new InvalidOperationException($"Connection string '{connectionName}' not found.");

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var result = await connection.QueryAsync<TFirst, TSecond, TThird, TFirst>(
                sql,
                map,
                param,
                splitOn: splitOn,
                commandType: commandType
            );

            return result.FirstOrDefault();
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