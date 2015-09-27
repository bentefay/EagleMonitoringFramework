using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;

namespace Eagle.Server.UI.Web2.Controllers
{
    public class BuildsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
