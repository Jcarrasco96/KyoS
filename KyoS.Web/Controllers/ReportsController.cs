using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspNetCore.Reporting;
using KyoS.Common.Enums;
using KyoS.Common.Helpers;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    public class ReportsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IDateHelper _dateHelper;
        private readonly IWebHostEnvironment _webhostEnvironment;
        private readonly IImageHelper _imageHelper;
        private readonly IReportHelper _reportHelper;

        public ReportsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IDateHelper dateHelper, IWebHostEnvironment webHostEnvironment, IImageHelper imageHelper, IReportHelper reportHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _dateHelper = dateHelper;
            _webhostEnvironment = webHostEnvironment;
            _imageHelper = imageHelper;
            _reportHelper = reportHelper;            
        }
                
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> DailyAssistance()
        {
            UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                         .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
                return View(null);

            return View(await _context.Weeks.Include(w => w.Days)
                                                .ThenInclude(d => d.Workdays_Clients)
                                                .ThenInclude(wc => wc.Client)
                                                .ThenInclude(c => c.Group)

                                                .Include(w => w.Days)
                                                .ThenInclude(d => d.Workdays_Clients)
                                                .ThenInclude(g => g.Facilitator)

                                                .Include(w => w.Days)
                                                .ThenInclude(d => d.Workdays_Clients)
                                                .ThenInclude(wc => wc.Note)

                                                .Where(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                          && w.Days.Where(d => d.Service == ServiceType.PSR).Count() > 0))
                                                .ToListAsync());            
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> PrintDailyAssistance(int id)
        {
            List<Workday_Client> workdayClientList = await _context.Workdays_Clients
                                                               .Include(wc => wc.Facilitator)
                                                               .ThenInclude(f => f.Clinic)

                                                               .Include(wc => wc.Workday)

                                                               .Include(wc => wc.Client)

                                                               .Where(wc => (wc.Workday.Id == id && wc.Facilitator.LinkedUser == User.Identity.Name))
                                                               .ToListAsync();
            if (workdayClientList.Count() == 0)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.DailyAssistanceReport(workdayClientList);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }
    }
}