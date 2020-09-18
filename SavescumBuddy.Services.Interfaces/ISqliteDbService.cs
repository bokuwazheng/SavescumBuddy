using System.Collections.Generic;

namespace SavescumBuddy.Services.Interfaces
{
    public interface ISqliteDbService
    {
        void Execute(string sql, object args = null);
        T ExecuteScalar<T>(string sql, object args = null);
        List<IDbEntity> Query<IDbEntity>(string sql, object args = null);
        IDbEntity QueryFirstOrDefault<IDbEntity>(string sql, object args = null);
    }
}
