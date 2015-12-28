using System;
using System.Linq;
using Microsoft.AspNet.Mvc;
using Microsoft.TeamFoundation.Build.WebApi;

namespace EagleMonitoring.Ui.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
