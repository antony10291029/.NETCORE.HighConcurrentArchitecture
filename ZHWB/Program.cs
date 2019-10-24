using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
namespace ZHWB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
             .UseKestrel(options=>{
                 options.ListenAnyIP(5000);
                 options.ListenAnyIP(5001);
             })
             .UseStartup<Startup>()
             .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventSourceLogger();
                });
        }
        //https://docs.microsoft.com/zh-cn/aspnet/core/host-and-deploy/windows-service?view=aspnetcore-2.1
        //dotnet publish -c release -o publish 控制台部署 dotnet ZHWB.dll加载执行控制台
    }
}
