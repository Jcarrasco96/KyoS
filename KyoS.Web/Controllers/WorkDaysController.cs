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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Manager")]
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

            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            return View(await _context.Weeks
                                      .Include(w => w.Days)
                                      .Where(w => (w.Clinic.Id == user_logged.Clinic.Id && w.Days.Where(d => d.Service == ServiceType.PSR).Count() > 0))
                                      .ToListAsync());
        }

        public async Task<IActionResult> IndividualWorkdays(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            return View(await _context.Weeks.Include(w => w.Days)
                                            .ThenInclude(n => n.Workdays_Clients)
                                            .ThenInclude(f => f.Facilitator)
                                            .Where(w => (w.Clinic.Id == user_logged.Clinic.Id 
                                                      && w.Days.Where(d => d.Service == ServiceType.Individual).Count() > 0))
                                            .ToListAsync());
        }

        public async Task<IActionResult> GroupWorkdays(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            return View(await _context.Weeks
                                      .Include(w => w.Days)
                                      .Where(w => (w.Clinic.Id == user_logged.Clinic.Id && w.Days.Where(d => d.Service == ServiceType.Group).Count() > 0))
                                      .ToListAsync());
        }

        public IActionResult Create(int error = 0, int idFacilitator = 0, int idClient = 0)
        {
            //One facilitator has not availability on that dates
            if (error == 1)
            {
                FacilitatorEntity facilitator = _context.Facilitators
                                                        .FirstOrDefault(f => f.Id == idFacilitator);
                ViewBag.Error = "0";
                ViewBag.errorText = $"Error. The facilitator {facilitator.Name} has another therapy already at that time";
            }

            //One client has a created note from another service at that time.
            if (error == 2)
            {
                ClientEntity client = _context.Clients
                                              .FirstOrDefault(f => f.Id == idClient);
                ViewBag.Error = "1";
                ViewBag.errorText = $"Error. The client {client.Name} has a created note from another therapy at that time";
            }

            //Imposible to create new workdays due to settings
            if (error == 3)
            {
                ViewBag.Error = "2";
                ViewBag.errorText = "Unable to create a new week. Please contact the administrator";
            }

            WeekViewModel model;

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
                    UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    //Unable to create new week due to the settings
                    SettingEntity setting = await _context.Settings
                                                          .FirstOrDefaultAsync(s => s.Clinic.Id == user_logged.Clinic.Id);
                    if(setting != null)
                    { 
                        if (setting.AvailableCreateNewWorkdays == false)
                        {
                            return RedirectToAction(nameof(Create), new { error = 3 });
                        }
                    }

                    workdays = entity.Workdays.ToString().Split(',');
                    foreach (string value in workdays)
                    {
                        datelist.Add(Convert.ToDateTime(value));
                    }

                    //obtengo los clientes de la terapia psr que esten activos para verificar que su facilitator pueda realizar la terapia, y para
                    //verificar que cada cliente no haya tenido terapia en ese mismo tiempo y posteriormente generarles la asistencia de los dias que se desean crear
                    List<ClientEntity> clients = await _context.Clients

                                                .Include(c => c.Group)
                                                .ThenInclude(g => g.Facilitator)

                                                .Include(c => c.Group)
                                                .ThenInclude(g => g.Schedule)

                                                .Include(c => c.MTPs)

                                                .Where(c => (c.Group.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                          && c.Status == StatusType.Open
                                                          && c.Group.Facilitator.Status == StatusType.Open
                                                          && c.Group.Service == ServiceType.PSR
                                                          && c.Service == ServiceType.PSR)).ToListAsync();

                    int numofweek;
                    DateTime initdate;
                    DateTime finaldate;
                    foreach (DateTime date in datelist)
                    {
                        numofweek = _dateHelper.GetWeekOfYear(date);
                        if (!numofweeks.ContainsKey(numofweek))
                        {
                            numofweeks.Add(numofweek, date);
                        }

                        //verifico la disponibilidad de los facilitadores involucrados en los dias que se desean crear, y verifico que el cliente
                        //no tenga otra nota de otra terapia ya creada en el mismo momento
                        foreach (var client in clients)
                        {
                            //Verify the client is not present in other services of notes at the same time
                            if (this.VerifyNotesAtSameTime(client.Id, client.Group.Meridian, date, ServiceType.PSR))
                            {
                                return RedirectToAction(nameof(Create), new { error = 2, idClient = client.Id });
                            }

                            //verifico que el facilitator tenga disponibilidad para dar la terapia en el dia correspondiente                            
                            if (this.VerifyFreeTimeOfFacilitator(client.Group.Facilitator.Id, ServiceType.PSR, client.Group.Meridian, date))
                            {
                                return RedirectToAction(nameof(Create), new { error = 1, idFacilitator = client.Group.Facilitator.Id });
                            }
                        }
                    }

                    foreach (KeyValuePair<int, DateTime> item in numofweeks)
                    {
                        initdate = _dateHelper.FirstDateOfWeek(item.Value.Year, item.Key, CultureInfo.CurrentCulture);
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
                        if (week_entity == null)
                        {
                            _context.Add(week);
                        }

                        foreach (DateTime item1 in datelist)
                        {
                            if (item1.Date >= week.InitDate && item1.Date <= week.FinalDate)
                            {
                                workday_entity = await _context.Workdays
                                                               .FirstOrDefaultAsync(w => (w.Date == item1
                                                                                                && w.Week.Clinic.Id == entity.IdClinic
                                                                                                && w.Service == ServiceType.PSR));
                                if (workday_entity == null)
                                {
                                    WorkdayEntity workday;
                                    if (week_entity == null)
                                    {
                                        workday = new WorkdayEntity
                                        {
                                            Date = item1,
                                            Week = week,
                                            Service = ServiceType.PSR
                                        };
                                        clinic_entity = week.Clinic;
                                    }
                                    else
                                    {
                                        workday = new WorkdayEntity
                                        {
                                            Date = item1,
                                            Week = week_entity,
                                            Service = ServiceType.PSR
                                        };
                                        clinic_entity = week_entity.Clinic;
                                    }
                                    _context.Add(workday);

                                    DateTime developed_date;
                                    List<Workday_Client> workday_client = new List<Workday_Client>();
                                    foreach (ClientEntity client in clients)
                                    {
                                        developed_date = client.MTPs.FirstOrDefault(m => m.Active == true).MTPDevelopedDate;
                                        //si el workday que estoy creando es mayor o igual que la fecha de desarrollo del mtp del cliente entonces creo el workday_client
                                        if (workday.Date >= developed_date)
                                        {
                                            workday_client.Add(new Workday_Client
                                            {
                                                Workday = workday,
                                                Client = client,
                                                Facilitator = client.Group.Facilitator,
                                                Session = client.Group.Meridian,
                                                Present = true,
                                                GroupSize = client.Group.Clients.Count(),
                                                SharedSession = client.Group.SharedSession,
                                                CodeBill = clinic_entity.CodePSRTherapy,
                                                Schedule = client.Group.Schedule
                                            });                                           
                                        }
                                    }
                                    _context.AddRange(workday_client);
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

        public IActionResult CreateIndividual(int error = 0, int idFacilitator = 0)
        {
            //the facilitator has not availability on that dates
            if (error == 1)
            {
                FacilitatorEntity facilitator = _context.Facilitators
                                                        .FirstOrDefault(f => f.Id == idFacilitator);
                ViewBag.Error = "0";
                ViewBag.errorText = $"Error. The facilitator {facilitator.Name} has another therapy already at that time";
            }

            //Imposible to create new workdays due to settings
            if (error == 2)
            {
                ViewBag.Error = "1";
                ViewBag.errorText = "Unable to create a new week. Please contact the administrator";
            }

            //Imposible to create new workdays due to the facilitator has any created workday_client on that dates
            if (error == 3)
            {
                FacilitatorEntity facilitator = _context.Facilitators
                                                        .FirstOrDefault(f => f.Id == idFacilitator);
                ViewBag.Error = "2";
                ViewBag.errorText = $"Error. The facilitator {facilitator.Name} has individual therapy already on those dates";
            }

            WeekViewModel model;

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            MultiSelectList facilitator_list;
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity>();

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

                facilitators = _context.Facilitators
                                       .Where(f => (f.Clinic.Id == user_logged.Clinic.Id && f.Status == StatusType.Open))
                                       .OrderBy(c => c.Name).ToList();
                facilitator_list = new MultiSelectList(facilitators, "Id", "Name");
                ViewData["facilitators"] = facilitator_list;
                return View(model);
            }
            else
            {
                return RedirectToAction("Home/Error404");
            }           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIndividual(WeekViewModel entity, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                string[] workdays;
                List<DateTime> datelist = new List<DateTime>();
                Dictionary<int, DateTime> numofweeks = new Dictionary<int, DateTime>();
                WorkdayEntity workday_entity;
                WeekEntity week_entity;
                ClinicEntity clinic_entity;

                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                //Unable to create new week due to the settings
                SettingEntity setting = await _context.Settings
                                                      .FirstOrDefaultAsync(s => s.Clinic.Id == user_logged.Clinic.Id);
                if (setting != null)
                {
                    if (setting.AvailableCreateNewWorkdays == false)
                    {
                        return RedirectToAction(nameof(CreateIndividual), new { error = 2 });
                    }
                }

                if (!string.IsNullOrEmpty(entity.Workdays) && !string.IsNullOrEmpty(form["facilitators"]))
                {
                    string[] facilitators = form["facilitators"].ToString().Split(',');
                    workdays = entity.Workdays.ToString().Split(',');
                    foreach (string value in workdays)
                    {
                        //Checking if each facilitator doesn't have any element at the same date, and service
                        foreach (string item in facilitators)
                        {
                            if (_context.Workdays_Clients.Where(wc => (wc.Workday.Date == Convert.ToDateTime(value)
                                                                    && wc.Workday.Service == ServiceType.Individual)
                                                                    && wc.Facilitator.Id == Convert.ToInt32(item))
                                                         .Count() > 0)
                            {
                                List<Workday_Client> listWorkdayClient = _context.Workdays_Clients
                                                                                 .Include(n => n.Schedule)
                                                                                 .ThenInclude(n => n.SubSchedules)
                                                                                 .Where(wc => (wc.Workday.Date == Convert.ToDateTime(value)
                                                                                                    && wc.Workday.Service == ServiceType.Individual)
                                                                                                    && wc.Facilitator.Id == Convert.ToInt32(item))
                                                                                                  .ToList();
                                GroupEntity groupActual = _context.Groups
                                                                  .Include(n => n.Schedule)
                                                                  .ThenInclude(n => n.SubSchedules)
                                                                  .FirstOrDefault(n => n.Service == ServiceType.Individual
                                                                   && n.Facilitator.Id == Convert.ToInt32(item));
                                if (groupActual != null)
                                {
                                    List<SubScheduleEntity> subSchedulesActuales = groupActual.Schedule.SubSchedules.ToList();

                                    if (listWorkdayClient.Count() > 0)
                                    {
                                        string time;
                                        List<SubScheduleEntity> subScheduleList = new List<SubScheduleEntity>();
                                        foreach (var product in listWorkdayClient)
                                        {
                                            subScheduleList = product.Schedule.SubSchedules.ToList();
                                            time = product.Session.Substring(0, 7);
                                            foreach (var element in subScheduleList)
                                            {
                                                if (element.InitialTime.ToShortTimeString().ToString().Contains(time) == true)
                                                {
                                                    if (subSchedulesActuales.Exists(n => (n.InitialTime.TimeOfDay >= element.InitialTime.TimeOfDay && n.InitialTime.TimeOfDay <= element.EndTime.TimeOfDay)
                                                     || (n.EndTime.TimeOfDay >= element.InitialTime.TimeOfDay && n.EndTime.TimeOfDay <= element.EndTime.TimeOfDay)
                                                     || (n.InitialTime.TimeOfDay <= element.InitialTime.TimeOfDay && n.EndTime.TimeOfDay >= element.InitialTime.TimeOfDay)
                                                     || (n.InitialTime.TimeOfDay <= element.EndTime.TimeOfDay && n.EndTime.TimeOfDay >= element.EndTime.TimeOfDay)) == true)
                                                    {
                                                        return RedirectToAction(nameof(CreateIndividual), new { error = 3, idFacilitator = Convert.ToInt32(item) });
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                
                            }
                        }                        

                        datelist.Add(Convert.ToDateTime(value));
                    }

                    int numofweek;
                    DateTime initdate;
                    DateTime finaldate;

                    foreach (DateTime date in datelist)
                    {
                        numofweek = _dateHelper.GetWeekOfYear(date);
                        if (!numofweeks.ContainsKey(numofweek))
                        {
                            numofweeks.Add(numofweek, date);
                        }
                    }

                    foreach (KeyValuePair<int, DateTime> item in numofweeks)
                    {
                        initdate = _dateHelper.FirstDateOfWeek(item.Value.Year, item.Key, CultureInfo.CurrentCulture);
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
                        if (week_entity == null)
                        {
                            _context.Add(week);
                        }

                        foreach (DateTime item1 in datelist)
                        {
                            if (item1.Date >= week.InitDate && item1.Date <= week.FinalDate)
                            {
                                workday_entity = await _context.Workdays
                                                               .FirstOrDefaultAsync(w => (w.Date == item1
                                                                                                && w.Week.Clinic.Id == entity.IdClinic
                                                                                                && w.Service == ServiceType.Individual));
                                if (workday_entity == null)
                                {
                                    WorkdayEntity workday;
                                    if (week_entity == null)
                                    {
                                        workday = new WorkdayEntity
                                        {
                                            Date = item1,
                                            Week = week,
                                            Service = ServiceType.Individual
                                        };
                                        clinic_entity = week.Clinic;
                                    }
                                    else
                                    {
                                        workday = new WorkdayEntity
                                        {
                                            Date = item1,
                                            Week = week_entity,
                                            Service = ServiceType.Individual
                                        };
                                        clinic_entity = week_entity.Clinic;
                                    }
                                    _context.Add(workday);

                                    FacilitatorEntity facilitator;                                    
                                    foreach (var value in facilitators)
                                    {
                                        facilitator = await _context.Facilitators
                                                                    .Where(f => f.Id == Convert.ToInt32(value))
                                                                    .FirstOrDefaultAsync();

                                        //verifico que el facilitator tenga disponibilidad para dar la terapia en el dia correspondiente                            
                                        if (this.VerifyFreeTimeOfFacilitator(facilitator.Id, ServiceType.Individual, string.Empty, item1))
                                        {
                                            return RedirectToAction(nameof(CreateIndividual), new { error = 1, idFacilitator = facilitator.Id });
                                        }

                                        GroupEntity group = await _context.Groups
                                                                          .Include(n => n.Schedule)
                                                                          .ThenInclude(n => n.SubSchedules)
                                                                          .FirstOrDefaultAsync(n => n.Service == ServiceType.Individual
                                                                             && n.Facilitator.Id == facilitator.Id);

                                        List<SubScheduleEntity> subSchedules = group.Schedule.SubSchedules.ToList();
                                        ScheduleEntity schedule = facilitator.Groups.FirstOrDefault(n => n.Service == ServiceType.Individual).Schedule;
                                        Workday_Client workday_client = new Workday_Client();

                                        foreach (var subSchedule in subSchedules)
                                        {
                                            workday_client = new Workday_Client
                                            {
                                                Workday = workday,
                                                Client = null,
                                                Facilitator = facilitator,
                                                Session = subSchedule.InitialTime.ToShortTimeString().ToString() + " - " + subSchedule.EndTime.ToShortTimeString().ToString(),
                                                Present = true,
                                                SharedSession = false,
                                                CodeBill = clinic_entity.CodeIndTherapy,
                                                Schedule = schedule
                                            };
                                            _context.Add(workday_client);
                                            workday_client = new Workday_Client();
                                        }
                                    }
                                }
                                else
                                {
                                    FacilitatorEntity facilitator;
                                    clinic_entity = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == entity.IdClinic);
                                    foreach (var value in facilitators)
                                    {
                                        facilitator = await _context.Facilitators
                                                                    .Where(f => f.Id == Convert.ToInt32(value))
                                                                    .FirstOrDefaultAsync();

                                        //verifico que el facilitator tenga disponibilidad para dar la terapia en el dia correspondiente                            
                                        if (this.VerifyFreeTimeOfFacilitator(facilitator.Id, ServiceType.Individual, string.Empty, item1))
                                        {
                                            return RedirectToAction(nameof(CreateIndividual), new { error = 1, idFacilitator = facilitator.Id });
                                        }

                                        GroupEntity group = await _context.Groups
                                                                         .Include(n => n.Schedule)
                                                                         .ThenInclude(n => n.SubSchedules)
                                                                         .FirstOrDefaultAsync(n => n.Service == ServiceType.Individual
                                                                            && n.Facilitator.Id == facilitator.Id);

                                        List<SubScheduleEntity> subSchedules = group.Schedule.SubSchedules.ToList();
                                        ScheduleEntity schedule = facilitator.Groups.FirstOrDefault(n => n.Service == ServiceType.Individual).Schedule;
                                        Workday_Client workday_client = new Workday_Client();

                                        foreach (var subSchedule in subSchedules)
                                        {
                                            workday_client = new Workday_Client
                                            {
                                                Workday = workday_entity,
                                                Client = null,
                                                Facilitator = facilitator,
                                                Session = subSchedule.InitialTime.ToShortTimeString().ToString() + " - " + subSchedule.EndTime.ToShortTimeString().ToString(),
                                                Present = true,
                                                SharedSession = false,
                                                CodeBill = clinic_entity.CodeIndTherapy,
                                                Schedule = schedule
                                            };
                                            _context.Add(workday_client);
                                            workday_client = new Workday_Client();
                                        }

                                    }
                                }
                            }
                        }
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(IndividualWorkdays));
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

        public IActionResult CreateGroup(int error = 0, int idFacilitator = 0, int idClient = 0)
        {
            //One facilitator has not availability on that dates
            if (error == 1)
            {
                FacilitatorEntity facilitator = _context.Facilitators
                                                        .FirstOrDefault(f => f.Id == idFacilitator);
                ViewBag.Error = "0";
                ViewBag.errorText = $"Error. The facilitator {facilitator.Name} has another therapy already at that time";
            }

            //One client has a created note from another service at that time.
            if (error == 2)
            {
                ClientEntity client = _context.Clients
                                              .FirstOrDefault(f => f.Id == idClient);
                ViewBag.Error = "1";
                ViewBag.errorText = $"Error. The client {client.Name} has a created note from another therapy at that time";
            }

            //Imposible to create new workdays due to settings
            if (error == 3)
            {
                ViewBag.Error = "2";
                ViewBag.errorText = "Unable to create a new week. Please contact the administrator";
            }

            WeekViewModel model;

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null)
            {
                return RedirectToAction("Home/Error404");
            }

            List<SelectListItem> list = new List<SelectListItem>();
            list.Insert(0, new SelectListItem
            {
                Text = user_logged.Clinic.Name,
                Value = $"{user_logged.Clinic.Id}"
            });
            model = new WeekViewModel
            {
                IdClinic = user_logged.Clinic.Id,
                Clinics = list
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(WeekViewModel entity)
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
                    UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    //Unable to create new week due to the settings
                    SettingEntity setting = await _context.Settings
                                                          .FirstOrDefaultAsync(s => s.Clinic.Id == user_logged.Clinic.Id);
                    if (setting != null)
                    {
                        if (setting.AvailableCreateNewWorkdays == false)
                        {
                            return RedirectToAction(nameof(CreateGroup), new { error = 3 });
                        }
                    }

                    workdays = entity.Workdays.ToString().Split(',');
                    foreach (string value in workdays)
                    {
                        datelist.Add(Convert.ToDateTime(value));
                    }

                    //obtengo los clientes de la terapia de grupo que esten activos para verificar que su facilitator pueda realizar la terapia, y para
                    //verificar que cada cliente no haya tenido terapia en ese mismo tiempo y posteriormente generarles la asistencia de los dias que se desean crear
                    List<ClientEntity> clients = await _context.Clients

                                                .Include(c => c.Group)
                                                .ThenInclude(c => c.Schedule)
                                                .Include(c => c.Group)
                                                .ThenInclude(g => g.Facilitator)

                                                .Include(c => c.MTPs)

                                                .Where(c => (c.Group.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                             && c.Status == StatusType.Open
                                                             && c.Group.Facilitator.Status == StatusType.Open
                                                             && c.Group.Service == ServiceType.Group
                                                             && c.Service == ServiceType.Group)).ToListAsync();

                    int numofweek;
                    DateTime initdate;
                    DateTime finaldate;
                    foreach (DateTime date in datelist)
                    {
                        numofweek = _dateHelper.GetWeekOfYear(date);
                        if (!numofweeks.ContainsKey(numofweek))
                        {
                            numofweeks.Add(numofweek, date);
                        }

                        //verifico la disponibilidad de los facilitadores involucrados en los dias que se desean crear, y verifico que el cliente
                        //no tenga otra nota de otra terapia ya creada en el mismo momento
                        foreach (var client in clients)
                        {
                            //Verify the client is not present in other services of notes at the same time
                            if (this.VerifyNotesAtSameTime(client.Id, client.Group.Meridian, date, ServiceType.Group,client.Group.Schedule.InitialTime,client.Group.Schedule.EndTime))
                            {
                                return RedirectToAction(nameof(CreateGroup), new { error = 2, idClient = client.Id });
                            }

                            //verifico que el facilitator tenga disponibilidad para dar la terapia en el dia correspondiente                            
                            if (this.VerifyFreeTimeOfFacilitator(client.Group.Facilitator.Id, ServiceType.Group, client.Group.Meridian, date))
                            {
                                return RedirectToAction(nameof(CreateGroup), new { error = 1, idFacilitator = client.Group.Facilitator.Id });
                            }
                        }
                    }

                    foreach (KeyValuePair<int, DateTime> item in numofweeks)
                    {
                        initdate = _dateHelper.FirstDateOfWeek(item.Value.Year, item.Key, CultureInfo.CurrentCulture);
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
                        if (week_entity == null)
                        {
                            _context.Add(week);
                        }

                        foreach (DateTime item1 in datelist)
                        {
                            if (item1.Date >= week.InitDate && item1.Date <= week.FinalDate)
                            {
                                workday_entity = await _context.Workdays
                                                               .FirstOrDefaultAsync(w => (w.Date == item1
                                                                                       && w.Week.Clinic.Id == entity.IdClinic
                                                                                       && w.Service == ServiceType.Group));
                                if (workday_entity == null)
                                {
                                    WorkdayEntity workday;
                                    if (week_entity == null)
                                    {
                                        workday = new WorkdayEntity
                                        {
                                            Date = item1,
                                            Week = week,
                                            Service = ServiceType.Group
                                        };
                                        clinic_entity = week.Clinic;
                                    }
                                    else
                                    {
                                        workday = new WorkdayEntity
                                        {
                                            Date = item1,
                                            Week = week_entity,
                                            Service = ServiceType.Group
                                        };
                                        clinic_entity = week_entity.Clinic;
                                    }
                                    _context.Add(workday);

                                    DateTime developed_date;
                                    List<Workday_Client> workday_client = new List<Workday_Client>();
                                    foreach (ClientEntity client in clients)
                                    {
                                        developed_date = client.MTPs.FirstOrDefault(m => m.Active == true).MTPDevelopedDate;
                                        //si el workday que estoy creando es mayor o igual que la fecha de desarrollo del mtp del cliente entonces creo el workday_client
                                        if (workday.Date >= developed_date)
                                        {
                                            workday_client.Add(new Workday_Client
                                            {
                                                Workday = workday,
                                                Client = client,
                                                Facilitator = client.Group.Facilitator,
                                                Session = client.Group.Meridian,
                                                Present = true,
                                                GroupSize = client.Group.Clients.Count(),
                                                SharedSession = false,
                                                CodeBill = clinic_entity.CodeGroupTherapy,
                                                Schedule = client.Group.Schedule
                                            });                                            
                                        }
                                    }
                                    _context.AddRange(workday_client);
                                }
                            }
                        }
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(GroupWorkdays));
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
                return RedirectToAction("Home/Error404");
            }

            WorkdayEntity workdayEntity = await _context.Workdays.FirstOrDefaultAsync(w => w.Id == id);
            if (workdayEntity == null)
            {
                return RedirectToAction("Home/Error404");
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

        public async Task<IActionResult> DeleteGroup(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            WorkdayEntity workdayEntity = await _context.Workdays.FirstOrDefaultAsync(w => w.Id == id);
            if (workdayEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Workdays.Remove(workdayEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("GroupWorkdays", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }        

        #region Utils Functions     
        private bool VerifyFreeTimeOfFacilitator(int idFacilitator, ServiceType service, string session, DateTime date)
        {
            //Group notes
            /*if (session == "AM" && service == ServiceType.Group)
            {
                if (_context.Workdays_Clients
                            .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "8.00 - 9.00 AM" && wc.Workday.Date == date))
                            .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "9.05 - 10.05 AM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "10.15 - 11.15 AM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "11.20 - 12.20 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "AM" && wc.Workday.Date == date && wc.Workday.Service == ServiceType.PSR))
                                  .Count() > 0
                                  )
                    return true;
                return false;
            }
            if (session == "PM" && service == ServiceType.Group)
            {
                if (_context.Workdays_Clients
                            .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "12.45 - 1.45 PM" && wc.Workday.Date == date))
                            .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "1.50 - 2.50 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "3.00 - 4.00 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "4.05 - 5.05 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "PM" && wc.Workday.Date == date && wc.Workday.Service == ServiceType.PSR))
                                  .Count() > 0
                                  )
                    return true;
                return false;
            }

            //PSR notes
            if (session == "AM" && service == ServiceType.PSR)
            {
                if (_context.Workdays_Clients
                            .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "8.00 - 9.00 AM" && wc.Workday.Date == date))
                            .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "9.05 - 10.05 AM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "10.15 - 11.15 AM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "11.20 - 12.20 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "AM" && wc.Workday.Date == date && wc.Workday.Service == ServiceType.Group))
                                  .Count() > 0
                                  )
                    return true;
                return false;
            }
            if (session == "PM" && service == ServiceType.PSR)
            {
                if (_context.Workdays_Clients
                            .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "12.45 - 1.45 PM" && wc.Workday.Date == date))
                            .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "1.50 - 2.50 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "3.00 - 4.00 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "4.05 - 5.05 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Session == "PM" && wc.Workday.Date == date && wc.Workday.Service == ServiceType.Group))
                                  .Count() > 0
                                  )
                    return true;
                return false;
            }

            //Individual notes
            if (service == ServiceType.Individual)
            {
                if (_context.Workdays_Clients
                            .Where(wc => (wc.Facilitator.Id == idFacilitator && wc.Workday.Date == date && wc.Workday.Service != ServiceType.Individual))
                            .Count() > 0)
                    return true;
                return false;
            }

            return true;*/

            return false;
        }
        
        private bool VerifyNotesAtSameTime(int idClient, string session, DateTime date, ServiceType service, DateTime initialTime = new DateTime(), DateTime endTime = new DateTime(), int idWordayClient = 0)
        {
            Workday_Client wordayClient = _context.Workdays_Clients
                                                  .Include(n => n.Schedule)
                                                  .ThenInclude(n => n.SubSchedules)
                                                  .Include(n => n.Workday)
                                                  .FirstOrDefault(n => n.Id == idWordayClient);
            //Individual notes
            if (session == "8.00 - 9.00 AM" || session == "9.05 - 10.05 AM" || session == "10.15 - 11.15 AM" || session == "11.20 - 12.20 PM")
            {
                if (_context.Workdays_Clients
                            .Where(wc => (wc.Client.Id == idClient && wc.Session == "AM" && wc.Workday.Date == date))
                            .Count() > 0)
                    return true;
                return false;

            }
            else
            {
                if (session == "12.45 - 1.45 PM" || session == "1.50 - 2.50 PM" || session == "3.00 - 4.00 PM" || session == "4.05 - 5.05 PM")
                {
                    if (_context.Workdays_Clients
                                .Where(wc => (wc.Client.Id == idClient && wc.Session == "PM" && wc.Workday.Date == date))
                                .Count() > 0)
                        return true;
                    return false;
                }
                else
                {
                    //PSR notes
                    if (session == "AM")
                    {
                        if (_context.Workdays_Clients
                                    .Where(wc => (wc.Client.Id == idClient 
                                        && wc.Session.Contains("AM") == true 
                                        && wc.Workday.Date == date
                                        && wc.Id != idWordayClient))
                                    .Count() > 0)
                            return true;
                        return false;
                    }
                    else
                    {
                        if (session == "PM")
                        {
                            if (_context.Workdays_Clients
                                        .Where(wc => (wc.Client.Id == idClient 
                                            && wc.Session.Contains("PM") == true 
                                            && wc.Workday.Date == date
                                            && wc.Id != idWordayClient))
                                        .Count() > 0)
                                return true;
                            return false;
                        }
                        else
                        {
                            List<Workday_Client> temp = _context.Workdays_Clients.Where(n => n.Workday.Date == date
                                                                   && ((n.Schedule.InitialTime >= initialTime
                                                                   && n.Schedule.InitialTime <= endTime)
                                                                       || (n.Schedule.EndTime >= initialTime
                                                                          && n.Schedule.EndTime <= endTime)
                                                                       || (n.Schedule.InitialTime <= initialTime
                                                                          && n.Schedule.EndTime >= initialTime)
                                                                       || (n.Schedule.InitialTime <= endTime
                                                                          && n.Schedule.EndTime >= endTime))
                                                                  && n.Id != idWordayClient
                                                                  && n.Client.Id == idClient)
                                                               .ToList();


                            if (temp.Count() > 0)
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }

                        }
                    }
                }
               

            }            
        }
        #endregion
    }
}