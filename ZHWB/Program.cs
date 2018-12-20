using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting.WindowsServices;
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
            var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
            var pathToContentRoot = Path.GetDirectoryName(pathToExe);
            return WebHost.CreateDefaultBuilder(args)
             .UseKestrel().UseUrls("http://*:5000","https://*:5001")
             .UseStartup<Startup>()
             .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventSourceLogger();
                });
             //.UseContentRoot(pathToContentRoot);
             /*.UseKestrel(option => {
                 option.Listen(IPAddress.Any, 5000);
                 option.Listen(IPAddress.Any, 5001, listenOptions =>
                    {
                        listenOptions.UseHttps("*.pfx", "testPassword");
                    });
             });*/
        }
        //https://docs.microsoft.com/zh-cn/aspnet/core/host-and-deploy/windows-service?view=aspnetcore-2.1
        //dotnet publish -f netcoreapp2.1 -c Release 控制台部署 dotnet ZHWB.dll加载执行控制台
    }
}
