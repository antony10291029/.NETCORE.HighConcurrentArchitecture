using RabbitMQ.Client;
using RabbitMQ.Util;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
namespace ZHWB_PersistenceService
{
    public static class Configuration
    {
        public static string MySqlConnStr { get; set; }
        public static string MQHost { get; set; }
        public static int MQPort { get; set; }
        public static string MQusername { get; set; }
        public static string MQpassword { get; set; }
        public static IConnection CreateMQConnection()
        {
            var factory = new ConnectionFactory() { HostName = MQHost, Port = MQPort, UserName = MQusername, Password = MQpassword };
            factory.AutomaticRecoveryEnabled = true;//自动恢复连接
            var connection = factory.CreateConnection();
            return connection;//创建一个连接
        }
        public static IDbConnection CreateMySqlConnection()
        {
            var connection = new MySqlConnection(MySqlConnStr);
            connection.Open();
            return connection;
        }
    }
}