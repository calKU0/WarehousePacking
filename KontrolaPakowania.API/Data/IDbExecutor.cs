using KontrolaPakowania.API.Data.Enums;
using System.Data;

namespace KontrolaPakowania.API.Data
{
    public interface IDbExecutor
    {
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null, CommandType? commandType = null, Connection connection = Connection.WMSConnection);

        Task<T> QuerySingleOrDefaultAsync<T>(string sql, object? param = null, CommandType? commandType = null, Connection connection = Connection.WMSConnection);

        Task<TFirst?> QuerySingleOrDefaultAsync<TFirst, TSecond>(string sql, Func<TFirst, TSecond, TFirst> map, string splitOn, object? param = null, CommandType? commandType = null, Connection connectionName = Connection.WMSConnection);
        Task<TFirst?> QuerySingleOrDefaultAsync<TFirst, TSecond, TThird>(string sql, Func<TFirst, TSecond, TThird, TFirst> map, string splitOn, object? param = null, CommandType? commandType = null, Connection connectionName = Connection.WMSConnection);
    }
}