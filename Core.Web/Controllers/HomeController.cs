using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Web.Models;
using Newtonsoft.Json;
using Core.Models;

namespace Core.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("jank")]
        public JsonResult JankTest([FromBody]PostTestModel data)
        {
            try
            {
                Console.WriteLine($"Received Data: {JsonConvert.SerializeObject(data)}");
                return new JsonResult(data);
            }
            catch (Exception)
            {
                return new JsonResult(new object() { });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
