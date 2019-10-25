using System.Buffers;

using RabbitMQ.Client;
using RabbitMQ.Util;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Data;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Threading;
using System;
using RabbitMQ.Client.Content;
namespace ZHWB.Infrastructure.MessageQueue
{
    public class RabitMQHandler : IRabitMQHandler
    {
        private string _host;
        private int _port;
        private string _username;
        private string _password;
        private readonly ConcurrentQueue<IConnection> FreeConnectionQueue;//空闲连接对象队列
        private readonly ConcurrentDictionary<IConnection, bool> BusyConnectionDic;//使用中（忙）连接对象集合
        private readonly ConcurrentDictionary<IConnection, int> MQConnectionPoolUsingDicNew;//连接池使用率最大值DefaultMaxConnectionUsingCount
        private const int DefaultMaxConnectionUsingCount = 1000;//默认最大连接可访问次数
        private readonly object freeConnLock = new object();
        private readonly object  addConnLock = new object();
        public int maxConnectionCount = 1000;//默认最大保持可用连接数10000个
        
        public RabitMQHandler(IConfiguration configuration)
        {
            maxConnectionCount=int.Parse(configuration["rabbitMQ:maxConnectionCount"]);
            _host = configuration["rabbitMQ:host"];
            _port = int.Parse(configuration["rabbitMQ:port"]);
            _username = configuration["rabbitMQ:username"];
            _password = configuration["rabbitMQ:password"];

            FreeConnectionQueue = new ConcurrentQueue<IConnection>();
            BusyConnectionDic = new ConcurrentDictionary<IConnection, bool>();
            MQConnectionPoolUsingDicNew = new ConcurrentDictionary<IConnection, int>();
        }
        private IConnection CreateMQConnection()
        {
            var factory = new ConnectionFactory() { HostName = _host, Port = _port, UserName = _username, Password = _password };
            factory.AutomaticRecoveryEnabled = true;
            var connection = factory.CreateConnection();
            return connection;//创建一个全新的连接（TCP）
        }

        private IConnection CreateMQConnectionWithPool()
        {
        SelectMQConnectionLine:
            IConnection mqConnection = null;
            if (FreeConnectionQueue.Count + BusyConnectionDic.Count < maxConnectionCount)
            {
                //锁保证不创建多余的连接数
                lock (addConnLock)
                {
                    if (FreeConnectionQueue.Count + BusyConnectionDic.Count < maxConnectionCount)
                    {
                        mqConnection = CreateMQConnection();
                        BusyConnectionDic[mqConnection] = true;
                        MQConnectionPoolUsingDicNew[mqConnection] = 1;
                        return mqConnection;
                    }
                }
            }
            if (!FreeConnectionQueue.TryDequeue(out mqConnection))
            {
                goto SelectMQConnectionLine;
            }
            else if (MQConnectionPoolUsingDicNew[mqConnection] + 1 > DefaultMaxConnectionUsingCount || !mqConnection.IsOpen) 
            {
                mqConnection.Close();
                mqConnection.Dispose();
                mqConnection = CreateMQConnection();
                MQConnectionPoolUsingDicNew[mqConnection] = 0;
            }
            BusyConnectionDic[mqConnection] = true;
            MQConnectionPoolUsingDicNew[mqConnection] = MQConnectionPoolUsingDicNew[mqConnection] + 1;
            return mqConnection;
        }
        private void ResetMQConnectionToFree(IConnection connection)
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
            }
        }
        public int GetMessageCount(IConnection connection, string QueueName)
        {
            int msgCount = 0;
            try
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(QueueName, true, false, false, null); 
                    msgCount = (int)channel.MessageCount(QueueName);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ResetMQConnectionToFree(connection);
            }
            return msgCount;
        }
        public string SendMsg(string exchange,  string queueName,string routingKey, string msg, bool durable = false)
        {
            IConnection connection=CreateMQConnectionWithPool();
            try
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange,ExchangeType.Direct);
                    channel.QueueDeclare(queue: queueName, durable: durable, exclusive: false, autoDelete: false, arguments: null);
                    channel.QueueBind(queueName,exchange,routingKey,null);
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    var body = Encoding.UTF8.GetBytes(msg);
                    channel.BasicPublish(exchange: exchange, routingKey: routingKey, basicProperties: properties, body: body);
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            finally
            {
                ResetMQConnectionToFree(connection);
            }
        }
    }
}