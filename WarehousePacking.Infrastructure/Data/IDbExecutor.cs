using System.Data;
using WarehousePacking.Contracts.Data.Enums;

namespace WarehousePacking.Infrastructure.Data
{
    public interface IDbExecutor
    {
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CommandType? commandType = CommandType.StoredProcedure, Connection connection = Connection.ERPConnection);

        Task<T> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, CommandType? commandType = CommandType.StoredProcedure, Connection connection = Connection.ERPConnection);

        Task<TFirst?> QuerySingleOrDefaultAsync<TFirst, TSecond>(string sql, Func<TFirst, TSecond, TFirst> map, string splitOn, object? param = null, CommandType? commandType = CommandType.StoredProcedure, Connection connectionName = Connection.ERPConnection);

        Task<TFirst?> QuerySingleOrDefaultAsync<TFirst, TSecond, TThird>(string sql, Func<TFirst, TSecond, TThird, TFirst> map, string splitOn, object? param = null, CommandType? commandType = CommandType.StoredProcedure, Connection connectionName = Connection.ERPConnection);
    }
}