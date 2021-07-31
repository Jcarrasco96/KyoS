using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin, Mannager")]
    public class WorkDaysController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IDateHelper _dateHelper;
        public WorkDaysController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IDateHelper dateHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _dateHelper = dateHelper;
        }
        public async Task<IActionResult> Index(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            if (User.IsInRole("Admin"))
                return View(await _context.Weeks.Include(w => w.Clinic)
                                                .Include(w => w.Days).ToListAsync());
            else
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic == null)
                    return View(await _context.Weeks.Include(w => w.Clinic)
                                                    .Include(w => w.Days).ToListAsync());

                return View(await _context.Weeks.Include(w => w.Days)
                                                .Where(w => w.Clinic.Id == user_logged.Clinic.Id)
                                                .ToListAsync());               
            }
        }

        public IActionResult Create()
        {
            WeekViewModel model;

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = clinic.Name,
                        Value = $"{clinic.Id}"
                    });
                    model = new WeekViewModel
                    {
                        IdClinic = clinic.Id,
                        Clinics = list
                    };
                    return View(model);
                }
            }

            model = new WeekViewModel
            {
                Clinics = _combosHelper.GetComboClinics()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WeekViewModel entity)
        {
            if (ModelState.IsValid)
            {
                string[] workdays;
                List<DateTime> datelist = new List<DateTime>();
                Dictionary<int, DateTime> numofweeks = new Dictionary<int, DateTime>();
                WorkdayEntity workday_entity;
                WeekEntity week_entity;
                ClinicEntity clinic_entity;

                if (!string.IsNullOrEmpty(entity.Workdays))
                {
                    workdays = entity.Workdays.ToString().Split(',');
                    foreach (string value in workdays)
                    {
                        datelist.Add(Convert.ToDateTime(value));
                    }

                    int numofweek;
                    DateTime initdate;
                    DateTime finaldate;

                    foreach (DateTime date in datelist)
                    {
                        numofweek = _dateHelper.GetWeekOfYear(date);
                        if(!numofweeks.ContainsKey(numofweek))
                            numofweeks.Add(numofweek, date);
                    }

                    foreach (var item in numofweeks)
                    {
                        initdate = _dateHelper.FirstDateOfWeek(item.Key == 52 ? item.Value.AddYears(-1).Year : item.Value.Year, item.Key, CultureInfo.CurrentCulture);
                        finaldate = initdate.AddDays(6);
                        WeekEntity week = new WeekEntity
                        {
                            InitDate = initdate,
                            FinalDate = finaldate,
                            Clinic = _context.Clinics.FirstOrDefault(c => c.Id == entity.IdClinic),
                            WeekOfYear = item.Key
                        };

                        week_entity = await _context.Weeks
                                                    .FirstOrDefaultAsync(w => (w.Clinic == week.Clinic 
                                                                                        && w.InitDate == week.InitDate 
                                                                                        && w.FinalDate == week.FinalDate));
                        if(week_entity == null)
                            _context.Add(week);

                        foreach (DateTime item1 in datelist)
                        {
                            if (item1.Date >= week.InitDate && item1.Date <= week.FinalDate)
                            {
                                workday_entity = await _context.Workdays.FirstOrDefaultAsync(w => (w.Date == item1 
                                                                                                && w.Week.Clinic.Id == entity.IdClinic));
                                if (workday_entity == null)
                                {
                                    WorkdayEntity workday;
                                    if (week_entity == null)
                                    {
                                        workday = new WorkdayEntity
                                        {
                                            Date = item1,
                                            Week = week
                                        };
                                        clinic_entity = week.Clinic;
                                    }
                                    else
                                    {
                                        workday = new WorkdayEntity
                                        {
                                            Date = item1,
                                            Week = week_entity
                                        };
                                        clinic_entity = week_entity.Clinic;
                                    }
                                    _context.Add(workday);

                                    //obtengo los clientes que esten activos de la clinica para generarles la asistencia del dia acabado de crear
                                    var clients = await _context.Clients
                                                                .Include(c => c.Group)
                                                                .ThenInclude(g => g.Facilitator)
                                                                .Include(c => c.MTPs)
                                                                .Where(c => (c.Group.Facilitator.Clinic.Id == clinic_entity.Id 
                                                                             && c.Status == StatusType.Open 
                                                                             && c.Group.Facilitator.Status == StatusType.Open)).ToListAsync();

                                    DateTime developed_date;
                                    Workday_Client workday_client;
                                    foreach (ClientEntity client in clients)
                                    {
                                        developed_date = client.MTPs.FirstOrDefault(m => m.Active == true).MTPDevelopedDate;
                                        //si el workday que estoy creando es mayor o igual que la fecha de desarrollo del mtp del cliente entonces creo el workday_client
                                        if (workday.Date >= developed_date)
                                        {
                                            workday_client = new Workday_Client
                                            {
                                                Workday = workday,
                                                Client = client,
                                                Facilitator = client.Group.Facilitator,
                                                Session = client.Group.Meridian,
                                                Present = true
                                            };
                                            _context.Add(workday_client);
                                        }                                        
                                    }
                                }                           
                            }
                        }                        
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, "Already exists the elements");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }                
            }
            return RedirectToAction(nameof(Index));                      
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            WorkdayEntity workdayEntity = await _context.Workdays.FirstOrDefaultAsync(w => w.Id == id);
            if (workdayEntity == null)
            {
                return NotFound();
            }

            try
            {
                _context.Workdays.Remove(workdayEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}