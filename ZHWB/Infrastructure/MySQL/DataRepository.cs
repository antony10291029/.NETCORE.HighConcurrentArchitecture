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
        private readonly ConcurrentQueue<MySqlConnection> FreeConnectionQueue;//空闲连接对象队列
        private readonly ConcurrentDictionary<MySqlConnection, bool> BusyConnectionDic;//使用中（忙）连接对象集合
        private readonly ConcurrentDictionary<MySqlConnection, int> ConnectionPoolUsingDicNew;//连接池使用率最大值DefaultMaxConnectionUsingCount
        private readonly Semaphore ConnectionPoolSemaphore;
        private readonly object freeConnLock = new object(), addConnLock = new object();
        public int maxConnectionCount = 10000;//默认最大可用连接数10000个
        private IRabitMQHandler _mQHandler;
        public DataRepository(IConfiguration configuration, IRabitMQHandler mQHandler)
        {
            _mQHandler = mQHandler;
            maxConnectionCount = int.Parse(configuration["MySQL:maxConnectionCount"]);
            _connstr = configuration["MySQL:connectionString"];
            FreeConnectionQueue = new ConcurrentQueue<MySqlConnection>();
            BusyConnectionDic = new ConcurrentDictionary<MySqlConnection, bool>();
            ConnectionPoolUsingDicNew = new ConcurrentDictionary<MySqlConnection, int>();
            ConnectionPoolSemaphore = new Semaphore(maxConnectionCount, maxConnectionCount, "ConnectionPoolSemaphore");
        }
        private MySqlConnection CreateMySQLConnection()
        {
            var connection = new MySqlConnection(_connstr);
            connection.Open();
            return connection;//创建一个全新的连接（TCP）
        }

        private MySqlConnection CreateMySQLConnectionWithPool()
        {
        SelectMQConnectionLine:
            ConnectionPoolSemaphore.WaitOne();
            MySqlConnection connection = null;
            if (FreeConnectionQueue.Count + BusyConnectionDic.Count < maxConnectionCount)
            {
                //锁保证不创建多余的连接数
                lock (addConnLock)
                {
                    if (FreeConnectionQueue.Count + BusyConnectionDic.Count < maxConnectionCount)
                    {
                        connection = CreateMySQLConnection();
                        BusyConnectionDic[connection] = true;
                        ConnectionPoolUsingDicNew[connection] = 1;
                        return connection;
                    }
                }
            }
            if (!FreeConnectionQueue.TryDequeue(out connection))
            {
                goto SelectMQConnectionLine;
            }
            else if (ConnectionPoolUsingDicNew[connection] + 1 > maxConnectionCount 
            || (connection.State != ConnectionState.Open)
            ||!connection.Ping()
            )
            {
                connection.Close();
                connection.Dispose();
                connection = CreateMySQLConnection();
                ConnectionPoolUsingDicNew[connection] = 0;
            }
            BusyConnectionDic[connection] = true;
            ConnectionPoolUsingDicNew[connection] = ConnectionPoolUsingDicNew[connection] + 1;
            return connection;
        }
        /// <summary>
        /// 将连接重置为空闲状态后续备用
        /// </summary>
        private void ResetMQConnectionToFree(MySqlConnection connection)
        {
            //锁保证不释放多余的连接
            lock (freeConnLock)
            {
                bool result = false;
                BusyConnectionDic.TryRemove(connection, out result);
                if (FreeConnectionQueue.Count + BusyConnectionDic.Count > maxConnectionCount)
                {
                    connection.Close();
                    connection.Dispose();
                }
                else
                {
                    FreeConnectionQueue.Enqueue(connection);
                }
                ConnectionPoolSemaphore.Release();
            }
        }

        /// <summary>
        /// 普通查询
        /// </summary>
        public List<T> Query<T>(string sql, object param = null) where T : Model
        {
            var conn = CreateMySQLConnectionWithPool();
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
                ResetMQConnectionToFree(conn);
            }
        }
        public T QuerySingle<T>(string sql, object param = null) where T : Model
        {
            var conn = CreateMySQLConnectionWithPool();
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
                ResetMQConnectionToFree(conn);
            }
        }
        public T QueryFirst<T>(string sql, object param = null) where T : Model
        {
            var conn = CreateMySQLConnectionWithPool();
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
                ResetMQConnectionToFree(conn);
            }
        }
        /// <summary>
        /// 事务查询
        /// </summary>
        public List<T> QueryTransaction<T>(string sql, object param = null, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted) where T : Model
        {
            var conn = CreateMySQLConnectionWithPool();
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
                ResetMQConnectionToFree(conn);
            }
        }
        public void SetData<T>(T data) where T : Model
        {
            string Tbl_Name = typeof(T).Name;
            string routkey="UPDATE";
            if (string.IsNullOrEmpty(data.Id))
            {
                data.Id = Guid.NewGuid().ToString();
                routkey="ADD";
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
