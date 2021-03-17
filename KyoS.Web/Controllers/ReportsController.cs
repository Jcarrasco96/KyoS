using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
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

        public ReportsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IDateHelper dateHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _dateHelper = dateHelper;
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
                                            .ThenInclude(d => d.Workdays_Activities_Facilitators)
                                            .ThenInclude(waf => waf.Activity)
                                            .ThenInclude(a => a.Theme)

                                            .Include(w => w.Days)
                                            .ThenInclude(d => d.Workdays_Activities_Facilitators)
                                            .ThenInclude(waf => waf.Facilitator)

                                            .Where(w => (w.Clinic.Id == user_logged.Clinic.Id))
                                            .ToListAsync());
            
        }
    }
}