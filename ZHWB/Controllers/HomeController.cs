using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZHWB.ViewModels;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
namespace ZHWB.Controllers
{
    public class HomeController : Controller
    {
        IConfiguration Configuration;
        public HomeController(IConfiguration configuration){
            Configuration=configuration;
        }
        
        public IActionResult Index()
        {
            ViewData["PubKey"]=Configuration["RSA:publicKey"];
            return View();
        }
        
        [Authorize("api")]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.This Page Need Authorize";
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        
    }
}
