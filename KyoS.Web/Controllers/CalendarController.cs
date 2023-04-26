using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    public class CalendarController : Controller
    {
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;

        public CalendarController(DataContext context, ICombosHelper combosHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
        }

        [Authorize(Roles = "Manager")]
        public IActionResult Index()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                                .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            CalendarCMH model = new CalendarCMH
            {
                IdClient = 0,
                Clients = _combosHelper.GetComboClientsByClinic(user_logged.Clinic.Id, false),
                IdFacilitator = 0,
                Facilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id, false)
            };

            return View(model);
        }

        [Authorize(Roles = "Manager")]
        public IActionResult Events(string start, string end, int idClient/*, int idFacilitator*/)
        {
            if (idClient != 0/* || idFacilitator != 0*/)
            {
                DateTime initDate = Convert.ToDateTime(start);
                DateTime finalDate = Convert.ToDateTime(end);

                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                List<Workday_Client> list = new List<Workday_Client>();

                IQueryable<Workday_Client> query = _context.Workdays_Clients

                                                           .Include(wc => wc.Schedule)

                                                           .Include(wc => wc.Workday)

                                                           .Where(wc => (wc.Workday.Date >= initDate && wc.Workday.Date <= finalDate && wc.Present == true));

                if (idClient != 0)
                    query = query.Where(wc => wc.Client.Id == idClient);
                /*if (idFacilitator != 0)
                    query = query.Where(wc => wc.Facilitator.Id == idFacilitator);*/

                list = query.ToList();
                
                var events = list.Select(wc => new
                                  {
                                    title = (wc.Workday.Service == ServiceType.PSR) ? "PSR Therapy" :
                                                (wc.Workday.Service == ServiceType.Group) ? "Group Therapy" :
                                                    (wc.Workday.Service == ServiceType.Individual) ? "Individual Therapy" : string.Empty,
                                    start = new DateTime(wc.Workday.Date.Year, wc.Workday.Date.Month, wc.Workday.Date.Day,
                                                         (wc.Schedule) != null ? wc.Schedule.InitialTime.Hour : 0, (wc.Schedule) != null ? wc.Schedule.InitialTime.Minute : 0, 0)
                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    end = new DateTime(wc.Workday.Date.Year, wc.Workday.Date.Month, wc.Workday.Date.Day,
                                                         (wc.Schedule) != null ? wc.Schedule.EndTime.Hour : 0, (wc.Schedule) != null ? wc.Schedule.EndTime.Minute : 0, 0)
                                                            .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    backgroundColor = (wc.Workday.Service == ServiceType.PSR) ? "#fcf8e3" :
                                                            (wc.Workday.Service == ServiceType.Group) ? "#d9edf7" :
                                                                (wc.Workday.Service == ServiceType.Individual) ? "Individual" : "#dff0d8",
                                    textColor = (wc.Workday.Service == ServiceType.PSR) ? "#9e7d67" :
                                                            (wc.Workday.Service == ServiceType.Group) ? "#487c93" :
                                                                (wc.Workday.Service == ServiceType.Individual) ? "#417c49" : "#417c49",          
                                    borderColor = (wc.Workday.Service == ServiceType.PSR) ? "#9e7d67" :
                                                            (wc.Workday.Service == ServiceType.Group) ? "#487c93" :
                                                                (wc.Workday.Service == ServiceType.Individual) ? "#417c49" : "#417c49"                    
                                  })
                                 .ToList();

                return new JsonResult(events);
            }
            else
            { 
                var events = new List<Workday_Client>();
                return new JsonResult(events);
            }
        }
    }
}
