using System.Threading.Tasks;
using System;
using Dapper;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;
using ZHWB_PersistenceService.Models;
using System.Threading;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
namespace ZHWB_PersistenceService
{
    public class SyncService<T> : IService where T : Model
    {
        string Tbl_Name;
        string exchange;
        string queuename;
        string db_tblname;
        string cols;
        string values;
        string setvalues;
        public SyncService()
        {
            Tbl_Name = typeof(T).Name;
            db_tblname = Tbl_Name.ToLower();//表名在LINUX可能严格区分大小写
            exchange = Tbl_Name + "_exchange";
            queuename = Tbl_Name + "_queue";
            var propinfos = typeof(T).GetProperties();
            var props = propinfos.Select(s => s.Name).ToArray();
            cols = string.Join(",", props);
            values = "@" + string.Join(",@", props);
            var sql = props.Where(s => s != "Id").Select(s => s + "=@" + s).ToList();
            setvalues = string.Join(",", sql);
        }

        IConnection mqConn;
        IModel channel;
        public void Start()
        {
            if (mqConn == null)
            {
                mqConn = Configuration.CreateMQConnection();
            }
            if (channel == null)
            {
                channel = mqConn.CreateModel();
            }
            Listen();
        }
        public delegate void ActCallback(string msg);
        /// <summary>
        /// 监听动作
        /// </summary>
        private void Listen()
        {
            channel.ExchangeDeclare(exchange, ExchangeType.Direct);
            channel.QueueDeclare(queue: queuename, durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.QueueBind(queuename, exchange, "");//第三个参数根本就无效后续自行判断
            //channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (ch, ea) =>
             {
                 channel.BasicAck(ea.DeliveryTag, false);//确认收到消息
                     var body = ea.Body;
                 var message = Encoding.UTF8.GetString(body);
                 switch (ea.RoutingKey)
                 {//判断key
                         case "ADD": ADD(message); break;
                     case "UPDATE": UPDATE(message); break;
                     case "ADDLIST": ADDLIST(message); break;
                     case "UPDATELIST": UPDATELIST(message); break;
                     case "DEL": DEL(message); break;
                     case "DELLIST": DELLIST(message); break;
                     default: break;
                 }
             };
            channel.BasicConsume(queuename, false, consumer);//订阅
            Console.WriteLine(Tbl_Name + "-Service is OK");
        }
        private void ADD(string message)
        {
            try
            {
                using (var conn = Configuration.CreateMySqlConnection())
                {
                    var obj = JsonConvert.DeserializeObject<T>(message);
                    var sql = "insert into " + db_tblname + "(" + cols + ") values(" + values + ")";
                    conn.Execute(sql, obj);
                    Console.WriteLine("ADD-" + db_tblname + "-" + message);
                }
            }
            catch (Exception e)
            {
                //记录异常
                Console.WriteLine(e.Message);
            }
        }
        private void UPDATE(string message)
        {
            try
            {
                using (var conn = Configuration.CreateMySqlConnection())
                {
                    var obj = JsonConvert.DeserializeObject<T>(message);
                    var sql = "update " + db_tblname + " set " + setvalues + " where id=@id";
                    conn.Execute(sql, obj);
                    Console.WriteLine("UPDATE-" + db_tblname + "-" + message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private void UPDATELIST(string message)
        {
            try
            {
                using (var conn = Configuration.CreateMySqlConnection())
                {
                    var objs = JsonConvert.DeserializeObject<List<T>>(message);
                    var trans = conn.BeginTransaction();
                    foreach (var obj in objs)
                    {
                        var sql = "update " + db_tblname + " set " + setvalues + " where id=@id";
                        conn.Execute(sql, obj);
                    }
                    trans.Commit();
                    Console.WriteLine("UPDATELIST-" + db_tblname + "-" + message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private void ADDLIST(string message)
        {
            try
            {
                using (var conn = Configuration.CreateMySqlConnection())
                {
                    var objs = JsonConvert.DeserializeObject<List<T>>(message);
                    var sql = "insert into " + db_tblname + "(" + cols + ") values(" + values + ")";
                    var tras = conn.BeginTransaction();
                    foreach (var obj in objs)
                    {
                        conn.Execute(sql, obj);
                    }
                    tras.Commit();
                    Console.WriteLine("ADDLIST-" + db_tblname + "-" + message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private void DELLIST(string message)
        {
            try
            {
                using (var conn = Configuration.CreateMySqlConnection())
                {
                    var objs = JsonConvert.DeserializeObject<List<string>>(message);
                    var sql = "delete from " + db_tblname + " where id=@Id";
                    var tras = conn.BeginTransaction();
                    foreach (var obj in objs)
                    {
                        conn.Execute(sql, new { Id = obj });
                    }
                    tras.Commit();
                    Console.WriteLine("DELLIST-" + db_tblname + "-" + message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private void DEL(string message)
        {
            try
            {
                using (var conn = Configuration.CreateMySqlConnection())
                {
                    var sql = "delete from " + db_tblname + " where id=@Id";
                    conn.Execute(sql, new { Id = message });
                    Console.WriteLine("DEL-" + db_tblname + "-" + message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
