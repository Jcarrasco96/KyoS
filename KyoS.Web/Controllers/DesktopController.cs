using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KyoS.Web.Controllers
{
    public class DesktopController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}