using System;
using ZHWB_PersistenceService.Models;
using Microsoft.Extensions.Configuration;

namespace ZHWB_PersistenceService
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
                IConfiguration configuration = builder.Build();
                Configuration.MySqlConnStr = configuration["MySQL:connectionString"];
                Configuration.MQHost = configuration["rabbitMQ:host"];
                Configuration.MQPort = int.Parse(configuration["rabbitMQ:port"]);
                Configuration.MQusername = configuration["rabbitMQ:username"];
                Configuration.MQpassword = configuration["rabbitMQ:password"];
                //服务注册这里的实体类字段要和服务器端Domian实体一致和数据库表一致
                //一个表对应一个MQ连接，丢失后自动重连
                IService user = new SyncService<User>();
                user.Start();
                IService role = new SyncService<Role>();
                role.Start();
                IService userrole = new SyncService<UserRole>();
                userrole.Start();
                Console.WriteLine("Press Ctrl+C to Exit...");
                while (true)
                {

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
