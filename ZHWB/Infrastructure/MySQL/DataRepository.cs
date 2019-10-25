using Dapper;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Data;
using System.Collections.Concurrent;
using System.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using ZHWB.Infrastructure.MessageQueue;

namespace ZHWB.Infrastructure.MySQL
{
    /// <summary>
    /// MySQL查询连接池。这里只提供查询不提供直接写入 写入统一使用队列
    /// </summary>
    public class DataRepository : IDataRepository
    {
        private string _connstr;
        private IRabitMQHandler _mQHandler;
        public DataRepository(IConfiguration configuration, IRabitMQHandler mQHandler)
        {
            _mQHandler = mQHandler;
            _connstr = configuration["MySQL:connectionString"];
        }
        private MySqlConnection CreateMySQLConnection()
        {
            var connection = new MySqlConnection(_connstr);
            connection.Open();
            return connection;//创建一个全新的连接（TCP）
        }
        /// <summary>
        /// 将连接重置为空闲状态后续备用
        /// </summary>
        private void ConnectionToFree(MySqlConnection connection)
        {
            connection.Close();
            connection.Dispose();
        }

        /// <summary>
        /// 普通查询
        /// </summary>
        public List<T> Query<T>(string sql, object param = null) where T : Model
        {
            var conn = CreateMySQLConnection();
            try
            {
                return conn.Query<T>(sql, param).ToList();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionToFree(conn);
            }
        }
        public T QuerySingle<T>(string sql, object param = null) where T : Model
        {
            var conn = CreateMySQLConnection();
            try
            {
                return conn.QuerySingle<T>(sql, param);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionToFree(conn);
            }
        }
        public T QueryFirst<T>(string sql, object param = null) where T : Model
        {
            var conn = CreateMySQLConnection();
            try
            {
                return conn.QueryFirst<T>(sql, param);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionToFree(conn);
            }
        }
        /// <summary>
        /// 事务查询
        /// </summary>
        public List<T> QueryTransaction<T>(string sql, object param = null, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) where T : Model
        {
            var conn = CreateMySQLConnection();
            var transaction = conn.BeginTransaction(isolationLevel);
            try
            {
                var res = conn.Query<T>(sql, param, transaction);
                transaction.Commit();
                return res.ToList();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw ex;
            }
            finally
            {
                ConnectionToFree(conn);
            }
        }
        public void SetData<T>(T data) where T : Model
        {
            string Tbl_Name = typeof(T).Name;
            string routkey = "UPDATE";
            if (string.IsNullOrEmpty(data.Id))
            {
                data.Id = Guid.NewGuid().ToString();
                routkey = "ADD";
            }
            var msg = JsonConvert.SerializeObject(data);
            _mQHandler.SendMsg(Tbl_Name + "_exchange",
            Tbl_Name + "_queue",
            routkey, msg);
        }
        public void RemoveData<T>(string id) where T : Model
        {
            string Tbl_Name = typeof(T).Name;
            _mQHandler.SendMsg(Tbl_Name + "_exchange",
            Tbl_Name + "_queue",
            "DEL", id);
        }
        public void AddList<T>(List<T> data) where T : Model
        {
            string Tbl_Name = typeof(T).Name;
            var msg = JsonConvert.SerializeObject(data);
            _mQHandler.SendMsg(Tbl_Name + "_exchange",
            Tbl_Name + "_queue",
            "ADDLIST", msg);
        }
        public void UpdateList<T>(List<T> data) where T : Model
        {
            string Tbl_Name = typeof(T).Name;
            var msg = JsonConvert.SerializeObject(data);
            _mQHandler.SendMsg(Tbl_Name + "_exchange",
            Tbl_Name + "_queue",
            "UPDATELIST", msg);
        }
        public void RemoveList<T>(List<string> keys) where T : Model
        {
            string Tbl_Name = typeof(T).Name;
            var msg = JsonConvert.SerializeObject(keys);
            _mQHandler.SendMsg(Tbl_Name + "_exchange",
            Tbl_Name + "_queue",
            "DELLIST", msg);
        }
    }
}
