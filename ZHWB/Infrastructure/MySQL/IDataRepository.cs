using System.Collections.Generic;
using System.Data;
namespace ZHWB.Infrastructure.MySQL
{
    public interface IDataRepository
    {
         List<T> Query<T>(string sql, object param = null) where T : Model;
         T QuerySingle<T>(string sql, object param = null) where T : Model;
         T QueryFirst<T>(string sql, object param = null) where T : Model;
         List<T> QueryTransaction<T>(string sql, object param = null, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) where T : Model;
         void SetData<T>(T data) where T : Model;
         void RemoveData<T>(string id) where T : Model;
         void AddList<T>(List<T> data) where T : Model;
         void UpdateList<T>(List<T> data) where T : Model;
         void RemoveList<T>(List<string> keys) where T:Model;
    }
}