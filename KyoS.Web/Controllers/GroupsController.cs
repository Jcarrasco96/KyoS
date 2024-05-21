﻿using KyoS.Common.Enums;
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
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Manager, Facilitator")]
    public class GroupsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IReportHelper _reportHelper;
        private readonly IRenderHelper _renderHelper;

        public GroupsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IReportHelper reportHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
            _renderHelper = renderHelper;
        }

        [Authorize(Roles = "Manager, Facilitator")]
        public async Task<IActionResult> Index(int all = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");                
            }

            int count = await _context.Clients
                                      .Where(c => (c.Status == StatusType.Open && c.Group == null && c.Clinic.Id == user_logged.Clinic.Id &&
                                                   c.MTPs.Count() > 0 && c.Service == ServiceType.PSR))
                                      .CountAsync();
            if (count > 0)
            {
                ViewBag.Message = "1";
                ViewData["count"] = count;
            }

            if (user_logged.UserType.ToString() == "Manager")
            {
                if (all == 0)
                {
                    List<GroupEntity> group = await _context.Groups

                                                           .Include(g => g.Facilitator)

                                                           .Include(g => g.Clients)
                                                           .ThenInclude(g => g.Clients_Diagnostics)
                                                           .ThenInclude(g => g.Diagnostic)
                                                           .Include(g => g.Clients)
                                                           .ThenInclude(g => g.Clients_HealthInsurances)
                                                           .ThenInclude(g => g.HealthInsurance)
                                                           .Include(g => g.Schedule)
                                                           .Where(g => (g.Facilitator.Clinic.Id == user_logged.Clinic.Id && g.Service == Common.Enums.ServiceType.PSR))
                                                           .OrderBy(g => g.Facilitator.Name)
                                                           .ToListAsync();
                    ViewData["all"] = all;
                    return View(group);
                }
                else
                {
                    List<GroupEntity> group = await _context.Groups

                                                           .Include(g => g.Facilitator)

                                                           .Include(g => g.Clients)
                                                           .ThenInclude(g => g.Clients_Diagnostics)
                                                           .ThenInclude(g => g.Diagnostic)
                                                           .Include(g => g.Clients)
                                                           .ThenInclude(g => g.Clients_HealthInsurances)
                                                           .ThenInclude(g => g.HealthInsurance)
                                                           .Include(g => g.Schedule)
                                                           .Where(g => (g.Facilitator.Clinic.Id == user_logged.Clinic.Id 
                                                                     && g.Service == Common.Enums.ServiceType.PSR
                                                                     && g.Clients.Count() > 0))
                                                           .OrderBy(g => g.Facilitator.Name)
                                                           .ToListAsync();
                    ViewData["all"] = all;
                    return View(group);
                }
               
            }
            if (user_logged.UserType.ToString() == "Facilitator")
            {
                if (all == 0)
                {
                    List<GroupEntity> group = await _context.Groups

                                                            .Include(g => g.Facilitator)

                                                            .Include(g => g.Clients)
                                                            .ThenInclude(g => g.Clients_Diagnostics)
                                                            .ThenInclude(g => g.Diagnostic)
                                                            .Include(g => g.Clients)
                                                            .ThenInclude(g => g.Clients_HealthInsurances)
                                                            .ThenInclude(g => g.HealthInsurance)
                                                            .Include(g => g.Schedule)
                                                            .Where(g => (g.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                 && g.Service == Common.Enums.ServiceType.PSR)
                                                                 && g.Facilitator.LinkedUser == user_logged.UserName)
                                                            .OrderBy(g => g.Facilitator.Name)
                                                            .ToListAsync();
                    ViewData["all"] = all;
                    return View(group);
                }
                else
                {
                    List<GroupEntity> group = await _context.Groups

                                                            .Include(g => g.Facilitator)

                                                            .Include(g => g.Clients)
                                                            .ThenInclude(g => g.Clients_Diagnostics)
                                                            .ThenInclude(g => g.Diagnostic)
                                                            .Include(g => g.Clients)
                                                            .ThenInclude(g => g.Clients_HealthInsurances)
                                                            .ThenInclude(g => g.HealthInsurance)
                                                            .Include(g => g.Schedule)
                                                            .Where(g => (g.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                 && g.Service == Common.Enums.ServiceType.PSR)
                                                                 && g.Facilitator.LinkedUser == user_logged.UserName
                                                                 && g.Clients.Count() == 0)
                                                            .OrderBy(g => g.Facilitator.Name)
                                                            .ToListAsync();
                    ViewData["all"] = all;
                    return View(group);
                }
            }

            return RedirectToAction("Home/Error404");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create(int id = 0, int error = 0, int idFacilitator = 0, int idClient = 0, int all = 0)
        {
            if (id == 1)
            {
                ViewBag.Creado = "Y";
            }
            else
            {
                if (id == 2)
                {
                    ViewBag.Creado = "E";
                }
                else
                {
                    ViewBag.Creado = "N";
                }
            }

            //the facilitator has not availability on that dates
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

            GroupViewModel model;
            MultiSelectList client_list;
            List<ClientEntity> clients = new List<ClientEntity>();
                        
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                    .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic != null)
            {
                model = new GroupViewModel
                {
                    Facilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id),
                    Schedules = _combosHelper.GetComboSchedulesByClinic(user_logged.Clinic.Id, ServiceType.PSR)
                };

                clients = await _context.Clients

                                        .Include(c => c.MTPs)

                                        .Where(c => (c.Clinic.Id == user_logged.Clinic.Id 
                                                    && c.Status == Common.Enums.StatusType.Open
                                                    && c.Service == Common.Enums.ServiceType.PSR
                                                    && c.Group == null))
                                        .OrderBy(c => c.Name).ToListAsync();

                clients = clients.Where(c => c.MTPs.Count > 0).ToList();
                client_list = new MultiSelectList(clients, "Id", "Name", clients);
                ViewData["clients"] = client_list;
                ViewData["all"] = all;
                return View(model);
            }

            return View(null);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupViewModel model, IFormCollection form, int all = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                switch (form["Meridian"])
                {
                    case "Am":
                        {
                            model.Am = true;
                            model.Pm = false;
                            model.SharedSession = false;
                            break;
                        }
                    case "SharedAm":
                        {
                            model.Am = true;
                            model.Pm = false;
                            model.SharedSession = true;
                            break;
                        }
                    case "Pm":
                        {
                            model.Am = false;
                            model.Pm = true;
                            model.SharedSession = false;
                            break;
                        }
                    case "SharedPm":
                        {
                            model.Am = false;
                            model.Pm = true;
                            model.SharedSession = true;
                            break;
                        }
                    default:
                        break;
                }

                model.Service = Common.Enums.ServiceType.PSR;
                GroupEntity group = await _converterHelper.ToGroupEntity(model, true);
                _context.Add(group);

                if (!string.IsNullOrEmpty(form["clients"]))
                {
                    string[] clients = form["clients"].ToString().Split(',');
                    ClientEntity client;
                    foreach (string value in clients)
                    {
                        client = await _context.Clients
                                               .Include(c => c.MTPs)
                                               .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(value));
                        DateTime developed_date;
                        List<WorkdayEntity> workdays;
                        WorkdayEntity workday_Temp = new WorkdayEntity(); ;
                        List<Workday_Client> workday_Client_group;
                        List<Workday_Client> workday_client = new List<Workday_Client>();                        
                        if (client != null)
                        {
                            client.Group = group;
                            client.IdFacilitatorPSR = group.Facilitator.Id;
                           
                            _context.Update(client);
                            //verifico que las notas de group se generen a partir de la fecha de los goals y objetivos de group Therapy, sino tiene objetivos se generan a aprtir de hoy
                            List<ObjetiveEntity> listObjetive = _context.Objetives.Where(n => n.Goal.MTP.Client.Id == client.Id
                                                                                           && n.Goal.Service == ServiceType.PSR)
                                                                                  .ToList();
                            DateTime initialgoal = new DateTime();
                            if (listObjetive.Count() > 0)
                            {
                                initialgoal = listObjetive.Min(n => n.DateOpened);
                            }
                            else
                            {
                                initialgoal = DateTime.Today;
                            }
                            //verifico que el cliente tenga la asistencia necesaria dada su fecha de desarrollo de notas
                            developed_date = client.MTPs.FirstOrDefault(m => m.Active == true).MTPDevelopedDate;
                            workdays = await _context.Workdays
                                                     .Include(w => w.Workdays_Clients)
                                                     .ThenInclude(wc => wc.Client)
                                                     .Where(w => (w.Date >= developed_date
                                                               && w.Date >= initialgoal
                                                               && w.Week.Clinic.Id == user_logged.Clinic.Id
                                                               && w.Service == Common.Enums.ServiceType.PSR))
                                                     .ToListAsync();
                            workday_Client_group = await _context.Workdays_Clients
                                                                .Include(w => w.Workday)

                                                                .Where(w => (w.Workday.Date >= developed_date
                                                                       && w.Workday.Week.Clinic.Id == user_logged.Clinic.Id
                                                                       && w.Workday.Service == Common.Enums.ServiceType.Group
                                                                       && w.Client.Id == client.Id))
                                                                .ToListAsync();
                            foreach (var item in workday_Client_group)
                            {
                                workday_Temp = workdays.FirstOrDefault(n => n.Date == item.Workday.Date);
                                if (workday_Temp != null)
                                {
                                    workdays.Remove(workday_Temp);
                                }
                                workday_Temp = new WorkdayEntity();
                            }
                            ScheduleEntity schedule = _context.Schedule.FirstOrDefault(n => n.Id == model.IdSchedule);
                            foreach (WorkdayEntity item in workdays)
                            {
                                //si el cliente no tiene asistencia en un dia laborable en Workdays_Clients entonces se crea
                                if (!item.Workdays_Clients.Any(wc => wc.Client.Id == client.Id))
                                {
                                    //Verify the client is not present in other services of notes at the same time
                                    if (this.VerifyNotesAtSameTime(client.Id, group.Meridian, item.Date, schedule.InitialTime, schedule.EndTime, ServiceType.PSR))
                                    {
                                        return RedirectToAction(nameof(Create), new { error = 2, idClient = client.Id });
                                    }

                                    //verifico que el facilitator tenga disponibilidad para dar la terapia en el dia correspondiente                            
                                    if (this.VerifyFreeTimeOfFacilitator(group.Facilitator.Id, ServiceType.PSR, group.Meridian, item.Date))
                                    {
                                        return RedirectToAction(nameof(Create), new { error = 1, idFacilitator = group.Facilitator.Id });
                                    }

                                    //verifico que el Cliente tenga ese tiempo disponible en el TCM
                                    if (user_logged.Clinic.Setting.TCMClinic == true)
                                    {
                                        if (this.VerifyTCMNotesAtSameTime(client.Id, item.Date, schedule.InitialTime, schedule.EndTime))
                                        {
                                            return RedirectToAction(nameof(Create), new { error = 2, idClient = client.Id });
                                        }
                                    }

                                    workday_client.Add(new Workday_Client
                                    {
                                        Workday = item,
                                        Client = client,
                                        Facilitator = client.Group.Facilitator,
                                        Session = client.Group.Meridian,
                                        Present = true,
                                        SharedSession = client.Group.SharedSession,
                                        CodeBill = user_logged.Clinic.CodePSRTherapy,
                                        Schedule = client.Group.Schedule
                                    });                                    
                                }                                
                            }
                            if (workday_client.Count() > 0)
                            {
                                _context.AddRange(workday_client);
                            }
                        }
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Create", new { id = 1, all = all });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the group");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            model.Facilitators = _combosHelper.GetComboFacilitators();
            model.Schedules = _combosHelper.GetComboSchedulesByClinic(user_logged.Clinic.Id, ServiceType.PSR);

            MultiSelectList client_list = new MultiSelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["clients"] = client_list;
            ViewData["all"] = all;
            return View(model);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int? id, int error = 0, int idFacilitator = 0, int idClient = 0, int all = 0, string test = "")
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GroupEntity groupEntity = await _context.Groups.Include(g => g.Facilitator)
                                                           .Include(g => g.Schedule)
                                                           .Include(g => g.Clients).FirstOrDefaultAsync(g => g.Id == id);
            if (groupEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            //the facilitator has not availability on that dates
            if (error == 1)
            {
                FacilitatorEntity facilitator = _context.Facilitators
                                                        .FirstOrDefault(f => f.Id == idFacilitator);
                ViewBag.Error = "0";
                ViewBag.errorText = $"Error. The facilitator {facilitator.Name} has another therapy already at that time" + test;
            }

            //One client has a created note from another service at that time.
            if (error == 2)
            {
                ClientEntity client = _context.Clients
                                              .FirstOrDefault(f => f.Id == idClient);
                ViewBag.Error = "1";
                ViewBag.errorText = $"Error. The client {client.Name} has a created note from another therapy at that time" + test;
            }

            MultiSelectList client_list;
            GroupViewModel groupViewModel = _converterHelper.ToGroupViewModel(groupEntity);
            ViewData["am"] = groupViewModel.Am && !groupViewModel.SharedSession ? "true" : "false";
            ViewData["pm"] = groupViewModel.Pm && !groupViewModel.SharedSession ? "true" : "false";
            ViewData["Sharedam"] = groupViewModel.Am && groupViewModel.SharedSession ? "true" : "false";
            ViewData["Sharedpm"] = groupViewModel.Pm && groupViewModel.SharedSession ? "true" : "false";
            List<ClientEntity> clients = new List<ClientEntity>();
                        
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                    .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic != null)
            {
                groupViewModel.Facilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id);
               
                clients = await _context.Clients

                                        .Include(c => c.MTPs)

                                        .Where(c => (c.Clinic.Id == user_logged.Clinic.Id 
                                                    && c.Status == Common.Enums.StatusType.Open
                                                    && c.Service == Common.Enums.ServiceType.PSR
                                                    && c.Group == null))
                                        .OrderBy(c => c.Name)
                                        .ToListAsync();

                clients = clients.Where(c => c.MTPs.Count > 0).ToList();
                foreach (ClientEntity item in groupViewModel.Clients) 
                {
                    clients.Add(item);
                }
                client_list = new MultiSelectList(clients, "Id", "Name", groupViewModel.Clients.Select(c => c.Id));
                ViewData["clients"] = client_list;
                ViewData["all"] = all;
                return View(groupViewModel);
            }          
                        
            return View(null);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GroupViewModel model, IFormCollection form, int all = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                switch (form["Meridian"])
                {
                    case "Am":
                        {
                            model.Am = true;
                            model.Pm = false;
                            model.SharedSession = false;
                            break;
                        }
                    case "SharedAm":
                        {
                            model.Am = true;
                            model.Pm = false;
                            model.SharedSession = true;
                            break;
                        }
                    case "Pm":
                        {
                            model.Am = false;
                            model.Pm = true;
                            model.SharedSession = false;
                            break;
                        }
                    case "SharedPm":
                        {
                            model.Am = false;
                            model.Pm = true;
                            model.SharedSession = true;
                            break;
                        }
                    default:
                        break;
                }                

                model.Service = Common.Enums.ServiceType.PSR;
                GroupEntity group = await _converterHelper.ToGroupEntity(model, false);
                _context.Update(group);

                GroupEntity original_group = await _context.Groups
                                                           .Include(g => g.Clients)
                                                           .Include(g => g.Schedule)
                                                           .FirstOrDefaultAsync(g => g.Id == model.Id);

                foreach (ClientEntity value in original_group.Clients)
                {
                    value.Group = null;
                    _context.Update(value);
                }

                if (!string.IsNullOrEmpty(form["clients"]))
                {
                    string[] clients = form["clients"].ToString().Split(',');
                    ClientEntity client;
                                      
                    foreach (string value in clients)
                    {
                        client = await _context.Clients
                                               .Include(c => c.MTPs)
                                               .ThenInclude(c => c.Goals)
                                               .ThenInclude(c => c.Objetives)
                                               .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(value));

                        DateTime developed_date;
                        List<WorkdayEntity> workdays;
                        WorkdayEntity workday_Temp = new WorkdayEntity();
                        List<Workday_Client> workday_Client_group;
                        List<Workday_Client> workday_client = new List<Workday_Client>();
                        
                        if (client != null)
                        {
                            client.Group = group;
                            client.IdFacilitatorPSR = group.Facilitator.Id;
                           
                            _context.Update(client);
                            //verifico que las notas de group se generen a partir de la fecha de los goals y objetivos de group Therapy, sino tiene objetivos se generan a aprtir de hoy
                            List<ObjetiveEntity> listObjetive = _context.Objetives.Where(n => n.Goal.MTP.Client.Id == client.Id
                                                                                           && n.Goal.Service == ServiceType.PSR)
                                                                                  .ToList();
                            DateTime initialgoal = new DateTime();
                            if (listObjetive.Count() > 0)
                            {
                                initialgoal = listObjetive.Min(n => n.DateOpened);
                            }
                            else
                            {
                                initialgoal = DateTime.Today;
                            }
                            //verifico que el cliente tenga la asistencia necesaria dada su fecha de desarrollo de notas
                            developed_date = client.MTPs.FirstOrDefault(m => m.Active == true).MTPDevelopedDate;
                            workdays = await _context.Workdays
                                                     .Include(w => w.Workdays_Clients)
                                                     .ThenInclude(wc => wc.Client)
                                                     .Where(w => (w.Date >= developed_date
                                                               && w.Date >= initialgoal
                                                               && w.Week.Clinic.Id == user_logged.Clinic.Id
                                                               && w.Service == Common.Enums.ServiceType.PSR))
                                                     .ToListAsync();
                            //aqui cargo todas las notas de grupo para ver las fechas y que en esas fechas no se generen notas de PSR
                            workday_Client_group = await _context.Workdays_Clients
                                                                 .Include(w => w.Workday)
                                                           
                                                                 .Where(w => (w.Workday.Date >= developed_date
                                                                        && w.Workday.Week.Clinic.Id == user_logged.Clinic.Id
                                                                        && w.Workday.Service == Common.Enums.ServiceType.Group
                                                                        && w.Client.Id == client.Id))
                                                                 .ToListAsync();
                            foreach (var item in workday_Client_group)
                            {
                                workday_Temp = workdays.FirstOrDefault(n => n.Date == item.Workday.Date);
                                if (workday_Temp != null)
                                {
                                    workdays.Remove(workday_Temp);
                                }
                                workday_Temp = new WorkdayEntity();
                            }

                            foreach (WorkdayEntity item in workdays)
                            {
                                //si el cliente no tiene asistencia en un dia laborable en Workdays_Clients entonces se crea
                                if (!item.Workdays_Clients.Any(wc => wc.Client.Id == client.Id) && client.MTPs.Where(n => n.Goals.Where(g => g.Service == ServiceType.PSR && g.Objetives.Where(o => o.DateOpened <= item.Date && o.DateResolved >= item.Date).Count() > 0).Count() > 0).Count() > 0)
                                {
                                    //Verify the client is not present in other services of notes at the same time
                                    if (this.VerifyNotesAtSameTime(client.Id, group.Meridian, item.Date, original_group.Schedule.InitialTime, original_group.Schedule.EndTime, ServiceType.PSR))
                                    {
                                        string test = ", review date (" + item.Date.ToShortDateString() + "). Individual Therapy";
                                        return RedirectToAction(nameof(Edit), new { id = model.Id, error = 2, idClient = client.Id, test = test });
                                    }

                                    //verifico que el facilitator tenga disponibilidad para dar la terapia en el dia correspondiente                            
                                    if (this.VerifyFreeTimeOfFacilitator(group.Facilitator.Id, ServiceType.PSR, group.Meridian, item.Date))
                                    {
                                        return RedirectToAction(nameof(Edit), new { id = model.Id, error = 1, idFacilitator = group.Facilitator.Id });
                                    }

                                    //verifico que el Cliente tenga ese tiempo disponible en el TCM
                                    if (user_logged.Clinic.Setting.TCMClinic == true)
                                    {
                                        if (this.VerifyTCMNotesAtSameTime(client.Id, item.Date, original_group.Schedule.InitialTime, original_group.Schedule.EndTime))
                                        {
                                            return RedirectToAction(nameof(Edit), new { id = model.Id, error = 2, idClient = client.Id });
                                        }
                                    }

                                    workday_client.Add(new Workday_Client
                                    {
                                        Workday = item,
                                        Client = client,
                                        Facilitator = client.Group.Facilitator,
                                        Session = client.Group.Meridian,
                                        Present = true,
                                        SharedSession = client.Group.SharedSession,
                                        CodeBill = user_logged.Clinic.CodeGroupTherapy,
                                        Schedule = client.Group.Schedule
                                    }); 
                                   
                                }                                
                            }
                            if (workday_client.Count() > 0)
                            {
                                _context.AddRange(workday_client);
                            }                                                       
                        }
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", new { all = all});
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the group");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            GroupEntity groupEntity = await _context.Groups
                                                    .Include(g => g.Facilitator)
                                                    .Include(g => g.Clients)
                                                    .FirstOrDefaultAsync(g => g.Id == model.Id);

            GroupViewModel groupViewModel = _converterHelper.ToGroupViewModel(groupEntity);
            ViewData["am"] = groupViewModel.Am ? "true" : "false";

            List<ClientEntity> clients_list = await _context.Clients

                                                            .Include(c => c.MTPs)

                                                            .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                                      && c.Status == Common.Enums.StatusType.Open
                                                                      && c.Service == Common.Enums.ServiceType.PSR
                                                                      && c.Group == null))
                                                            .OrderBy(c => c.Name)
                                                            .ToListAsync();

            clients_list = clients_list.Where(c => c.MTPs.Count > 0).ToList();
            foreach (ClientEntity item in groupViewModel.Clients)
            {
                clients_list.Add(item);
            }

            MultiSelectList client_list = new MultiSelectList(clients_list, "Id", "Name", groupViewModel.Clients.Select(c => c.Id));
            ViewData["clients"] = client_list;
            ViewData["all"] = all;
            return View(groupViewModel);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Print(int id)
        {            
            var result = await _reportHelper.GroupAsyncReport(id);
            return await Task.Run(() => File(result, System.Net.Mime.MediaTypeNames.Application.Pdf));            
        }

        [Authorize(Roles = "Manager, Facilitator")]
        public async Task<IActionResult> Group(int all = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
            {
                return RedirectToAction("Home/Error404");
            }

            int count = await _context.Clients
                                      .Where(c => (c.Status == StatusType.Open && c.Group == null && c.Clinic.Id == user_logged.Clinic.Id &&
                                                   c.MTPs.Count() > 0 && c.Service == ServiceType.Group))
                                      .CountAsync();
            if (count > 0)
            {
                ViewBag.Message = "1";
                ViewData["count"] = count;
            }

            if (user_logged.UserType.ToString() == "Manager")
            {
                if (all == 0)
                {
                    List<GroupEntity> group = await _context.Groups
                                                            .Include(g => g.Facilitator)
                                                            .Include(g => g.Clients)
                                                            .Include(g => g.Schedule)
                                                            .Include(g => g.Clients)
                                                            .ThenInclude(g => g.Clients_Diagnostics)
                                                            .ThenInclude(g => g.Diagnostic)
                                                            .Include(g => g.Clients)
                                                            .ThenInclude(g => g.Clients_HealthInsurances)
                                                            .ThenInclude(g => g.HealthInsurance)
                                                            .Where(g => (g.Facilitator.Clinic.Id == user_logged.Clinic.Id && g.Service == Common.Enums.ServiceType.Group))
                                                            .OrderBy(g => g.Facilitator.Name)
                                                            .ToListAsync();
                    ViewData["all"] = all;
                    return View(group);
                }
                else
                {
                    List<GroupEntity> group = await _context.Groups
                                                            .Include(g => g.Facilitator)
                                                            .Include(g => g.Clients)
                                                            .Include(g => g.Schedule)
                                                            .Include(g => g.Clients)
                                                            .ThenInclude(g => g.Clients_Diagnostics)
                                                            .ThenInclude(g => g.Diagnostic)
                                                            .Include(g => g.Clients)
                                                            .ThenInclude(g => g.Clients_HealthInsurances)
                                                            .ThenInclude(g => g.HealthInsurance)
                                                            .Where(g => (g.Facilitator.Clinic.Id == user_logged.Clinic.Id 
                                                                      && g.Service == Common.Enums.ServiceType.Group
                                                                      && g.Clients.Count() > 0))
                                                            .OrderBy(g => g.Facilitator.Name)
                                                            .ToListAsync();
                    ViewData["all"] = all;
                    return View(group);
                }
            }
            if (user_logged.UserType.ToString() == "Facilitator")
            {
                if (all == 0)
                {
                    List<GroupEntity> group = await _context.Groups
                                                            .Include(g => g.Facilitator)
                                                            .Include(g => g.Clients)
                                                            .Include(g => g.Schedule)
                                                            .Include(g => g.Clients)
                                                            .ThenInclude(g => g.Clients_Diagnostics)
                                                            .ThenInclude(g => g.Diagnostic)
                                                            .Include(g => g.Clients)
                                                            .ThenInclude(g => g.Clients_HealthInsurances)
                                                            .ThenInclude(g => g.HealthInsurance)
                                                            .Where(g => (g.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                && g.Service == Common.Enums.ServiceType.Group
                                                                && g.Facilitator.LinkedUser == user_logged.UserName))
                                                            .OrderBy(g => g.Facilitator.Name)
                                                            .ToListAsync();
                    ViewData["all"] = all;
                    return View(group);
                }
                else
                {
                    List<GroupEntity> group = await _context.Groups
                                                            .Include(g => g.Facilitator)
                                                            .Include(g => g.Clients)
                                                            .Include(g => g.Schedule)
                                                            .Include(g => g.Clients)
                                                            .ThenInclude(g => g.Clients_Diagnostics)
                                                            .ThenInclude(g => g.Diagnostic)
                                                            .Include(g => g.Clients)
                                                            .ThenInclude(g => g.Clients_HealthInsurances)
                                                            .ThenInclude(g => g.HealthInsurance)
                                                            .Where(g => (g.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                && g.Service == Common.Enums.ServiceType.Group
                                                                && g.Facilitator.LinkedUser == user_logged.UserName
                                                                && g.Clients.Count() > 0))
                                                            .OrderBy(g => g.Facilitator.Name)
                                                            .ToListAsync();
                    ViewData["all"] = all;
                    return View(group);
                }
            }
            return RedirectToAction("Home/Error404");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateGT(int id = 0, int error = 0, int idFacilitator = 0, int idClient = 0, int all = 0)
        {
            if (id == 1)
            {
                ViewBag.Creado = "Y";
            }
            else
            {
                if (id == 2)
                {
                    ViewBag.Creado = "E";
                }
                else
                {
                    ViewBag.Creado = "N";
                }
            }

            //the facilitator has not availability on that dates
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

            GroupViewModel model;
            MultiSelectList client_list;
            List<ClientEntity> clients = new List<ClientEntity>();

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null)
            {
                return RedirectToAction("Home/Error404");
            }

            model = new GroupViewModel
            {
                Facilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id),
                Schedules = _combosHelper.GetComboSchedulesByClinic(user_logged.Clinic.Id, ServiceType.Group)
            };

            clients = await _context.Clients

                                    .Include(c => c.MTPs)

                                    .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                        && c.Status == Common.Enums.StatusType.Open
                                        && c.Service == Common.Enums.ServiceType.Group
                                        && c.Group == null))
                                    .OrderBy(c => c.Name).ToListAsync();

            clients = clients.Where(c => c.MTPs.Count > 0).ToList();
            client_list = new MultiSelectList(clients, "Id", "Name");
            ViewData["clients"] = client_list;
            ViewData["all"] = all;
            return View(model);                     
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGT(GroupViewModel model, IFormCollection form, int all = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                switch (form["Meridian"])
                {
                    case "Am":
                        {
                            model.Am = true;
                            model.Pm = false;
                            break;
                        }
                    case "Pm":
                        {
                            model.Am = false;
                            model.Pm = true;
                            break;
                        }
                    default:
                        break;
                }                

                model.Service = Common.Enums.ServiceType.Group;
                GroupEntity group = await _converterHelper.ToGroupEntity(model, true);
                _context.Add(group);

                if (!string.IsNullOrEmpty(form["clients"]))
                {
                    string[] clients = form["clients"].ToString().Split(',');
                    ClientEntity client;
                    foreach (string value in clients)
                    {
                        client = await _context.Clients
                                               .Include(c => c.MTPs)
                                               .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(value));
                        DateTime developed_date;
                        List<WorkdayEntity> workdays;
                        WorkdayEntity workday_Temp = new WorkdayEntity(); ;
                        List<Workday_Client> workday_Client_PSR;
                        List<Workday_Client> workday_client = new List<Workday_Client>();
                        if (client != null)
                        {                            
                            client.Group = group;
                            client.IdFacilitatorGroup = group.Facilitator.Id;
                         
                            _context.Update(client);

                            //verifico que las notas de group se generen a partir de la fecha de los goals y objetivos de group Therapy, sino tiene objetivos se generan a aprtir de hoy
                            List<ObjetiveEntity> listObjetive = _context.Objetives.Where(n => n.Goal.MTP.Client.Id == client.Id
                                                                                           && n.Goal.Service == ServiceType.Group)
                                                                                  .ToList();
                            DateTime initialgoal = new DateTime();
                            if (listObjetive.Count() > 0)
                            {
                                initialgoal = listObjetive.Min(n => n.DateOpened);
                            }
                            else
                            {
                                initialgoal = DateTime.Today;
                            }

                            //verifico que el cliente tenga la asistencia necesaria dada su fecha de desarrollo de notas
                            developed_date = client.MTPs.FirstOrDefault(m => m.Active == true).MTPDevelopedDate;

                            workdays = await _context.Workdays

                                                     .Include(w => w.Workdays_Clients)
                                                     .ThenInclude(wc => wc.Client)

                                                     .Where(w => (w.Date >= developed_date
                                                               && w.Date >= initialgoal
                                                               && w.Week.Clinic.Id == user_logged.Clinic.Id
                                                               && w.Service == Common.Enums.ServiceType.Group))
                                                     .ToListAsync();
                            workday_Client_PSR = await _context.Workdays_Clients
                                                                .Include(w => w.Workday)

                                                                .Where(w => (w.Workday.Date >= developed_date
                                                                       && w.Workday.Week.Clinic.Id == user_logged.Clinic.Id
                                                                       && w.Workday.Service == Common.Enums.ServiceType.PSR
                                                                       && w.Client.Id == client.Id))
                                                                .ToListAsync();
                            foreach (var item in workday_Client_PSR)
                            {
                                workday_Temp = workdays.FirstOrDefault(n => n.Date == item.Workday.Date);
                                if (workday_Temp != null)
                                {
                                    workdays.Remove(workday_Temp);
                                }
                                workday_Temp = new WorkdayEntity();
                            }
                            
                            ScheduleEntity schedule = _context.Schedule.FirstOrDefault(n => n.Id == model.IdSchedule);
                            foreach (WorkdayEntity item in workdays)
                            {
                                //si el cliente no tiene asistencia en un dia laborable en Workdays_Clients entonces se crea
                                if (!item.Workdays_Clients.Any(wc => wc.Client.Id == client.Id))
                                {
                                    //Verify the client is not present in other services of notes at the same time
                                     if (this.VerifyNotesAtSameTime(client.Id, group.Meridian, item.Date, schedule.InitialTime, schedule.EndTime, ServiceType.Group))
                                     {
                                         return RedirectToAction(nameof(CreateGT), new { error = 2, idClient = client.Id });
                                     }

                                     //verifico que el facilitator tenga disponibilidad para dar la terapia en el dia correspondiente                            
                                     if (this.VerifyFreeTimeOfFacilitator(group.Facilitator.Id, ServiceType.Group, group.Meridian, item.Date))
                                     {
                                         return RedirectToAction(nameof(CreateGT), new { error = 1, idFacilitator = group.Facilitator.Id });
                                     }

                                    //verifico que el Cliente tenga ese tiempo disponible en el TCM
                                    if (user_logged.Clinic.Setting.TCMClinic == true)
                                    {
                                        if (this.VerifyTCMNotesAtSameTime(client.Id, item.Date, schedule.InitialTime, schedule.EndTime))
                                        {
                                            return RedirectToAction(nameof(CreateGT), new { error = 1, idFacilitator = group.Facilitator.Id });
                                        }
                                    }

                                    workday_client.Add(new Workday_Client
                                     {
                                         Workday = item,
                                         Client = client,
                                         Facilitator = client.Group.Facilitator,
                                         Session = client.Group.Meridian,
                                         Present = true,
                                         SharedSession = false,
                                         CodeBill = user_logged.Clinic.CodeGroupTherapy,
                                         Schedule = client.Group.Schedule
                                     });
                                   /* if ((!this.VerifyNotesAtSameTime(client.Id, group.Meridian, item.Date, ServiceType.Group))
                                        && (!this.VerifyFreeTimeOfFacilitator(group.Facilitator.Id, ServiceType.Group, group.Meridian, item.Date)))
                                    {
                                        workday_client.Add(new Workday_Client
                                        {
                                            Workday = item,
                                            Client = client,
                                            Facilitator = client.Group.Facilitator,
                                            Session = client.Group.Meridian,
                                            Present = true,
                                            SharedSession = false,
                                            CodeBill = user_logged.Clinic.CodeGroupTherapy,
                                            Schedule = client.Group.Schedule
                                        });
                                    }*/
                                }                                
                            }

                            if (workday_client.Count() > 0)
                            {
                                _context.AddRange(workday_client);
                            }
                        }
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("CreateGT", new { id = 1, all = all});
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the group");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            model.Facilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id);

            List<ClientEntity> clients_list = await _context.Clients

                                                            .Include(c => c.MTPs)

                                                            .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                                && c.Status == Common.Enums.StatusType.Open
                                                                && c.Service == Common.Enums.ServiceType.Group))
                                                            .OrderBy(c => c.Name).ToListAsync();

            clients_list = clients_list.Where(c => c.MTPs.Count > 0).ToList();
            MultiSelectList client_list = new MultiSelectList(clients_list, "Id", "Name");
            ViewData["clients"] = client_list;

            return View(model);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditGT(int? id, int error = 0, int idFacilitator = 0, int idClient = 0, int all = 0, string test = "")
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GroupEntity groupEntity = await _context.Groups
                                                    .Include(g => g.Facilitator)
                                                    .Include(g => g.Clients)
                                                    .Include(g => g.Schedule)
                                                    .FirstOrDefaultAsync(g => g.Id == id);
            if (groupEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            //the facilitator has not availability on that dates
            if (error == 1)
            {
                FacilitatorEntity facilitator = _context.Facilitators
                                                        .FirstOrDefault(f => f.Id == idFacilitator);
                ViewBag.Error = "0";
                ViewBag.errorText = $"Error. The facilitator {facilitator.Name} has another therapy already at that time" + test;
            }

            //One client has a created note from another service at that time.
            if (error == 2)
            {
                ClientEntity client = _context.Clients
                                              .FirstOrDefault(f => f.Id == idClient);
                ViewBag.Error = "1";
                ViewBag.errorText = $"Error. The client {client.Name} has a created note from another therapy at that time" + test;
            }

            MultiSelectList client_list;
            GroupViewModel groupViewModel = _converterHelper.ToGroupViewModel(groupEntity);
            ViewData["am"] = groupViewModel.Am ? "true" : "false";
            List<ClientEntity> clients = new List<ClientEntity>();
                        
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null)
            {
                return RedirectToAction("Home/Error404");
            }
            
            groupViewModel.Facilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id);
            groupViewModel.Schedules = _combosHelper.GetComboSchedulesByClinic(user_logged.Clinic.Id, ServiceType.Group);

            clients = await _context.Clients

                                    .Include(c => c.MTPs)

                                    .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                && c.Status == Common.Enums.StatusType.Open
                                                && c.Service == Common.Enums.ServiceType.Group
                                                && c.Group == null))
                                    .OrderBy(c => c.Name)
                                    .ToListAsync();

            clients = clients.Where(c => c.MTPs.Count > 0).ToList();
            foreach (ClientEntity item in groupViewModel.Clients)
            {
                clients.Add(item);
            }
            client_list = new MultiSelectList(clients, "Id", "Name", groupViewModel.Clients.Select(c => c.Id));
            ViewData["all"] = all;
            ViewData["clients"] = client_list;
            return View(groupViewModel);                                   
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGT(GroupViewModel model, IFormCollection form, int all = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                switch (form["Meridian"])
                {
                    case "Am":
                        {
                            model.Am = true;
                            model.Pm = false;
                            break;
                        }
                    case "Pm":
                        {
                            model.Am = false;
                            model.Pm = true;
                            break;
                        }
                    default:
                        break;
                }

                model.Service = Common.Enums.ServiceType.Group;
                GroupEntity group = await _converterHelper.ToGroupEntity(model, false);
                _context.Update(group);

                GroupEntity original_group = await _context.Groups
                                                           .Include(g => g.Clients)
                                                           .Include(g => g.Schedule)
                                                           .FirstOrDefaultAsync(g => g.Id == model.Id);

                foreach (ClientEntity value in original_group.Clients)
                {
                    value.Group = null;
                    _context.Update(value);
                }

                if (!string.IsNullOrEmpty(form["clients"]))
                {
                    string[] clients = form["clients"].ToString().Split(',');
                    ClientEntity client;
                                   
                    foreach (string value in clients)
                    {
                        client = await _context.Clients
                                               .Include(c => c.MTPs)
                                               .ThenInclude(c => c.Goals)
                                               .ThenInclude(c => c.Objetives)
                                               .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(value));

                        DateTime developed_date;
                        List<WorkdayEntity> workdays;
                        WorkdayEntity workday_Temp = new WorkdayEntity(); ;
                        List<Workday_Client> workday_Client_PSR;
                        List<Workday_Client> workday_client = new List<Workday_Client>();

                        if (client != null)
                        {
                            client.Group = group;
                            client.IdFacilitatorGroup = group.Facilitator.Id;

                            _context.Update(client);
                           //verifico que las notas de group se generen a partir de la fecha de los goals y objetivos de group Therapy, sino tiene objetivos se generan a aprtir de hoy
                            List<ObjetiveEntity> listObjetive = _context.Objetives.Where(n => n.Goal.MTP.Client.Id == client.Id
                                                                                           && n.Goal.MTP.Active == true
                                                                                           && n.Goal.Service == ServiceType.Group)
                                                                                  .ToList();
                            DateTime initialgoal = new DateTime();
                            if (listObjetive.Count() > 0)
                            {
                                initialgoal = listObjetive.Min(n => n.DateOpened);
                            }
                            else
                            {
                                initialgoal = DateTime.Today;
                            }
                            //verifico que el cliente tenga la asistencia necesaria dada su fecha de desarrollo de notas
                            developed_date = client.MTPs.FirstOrDefault(m => m.Active == true).MTPDevelopedDate;
                            workdays = await _context.Workdays
                                                     .Include(w => w.Workdays_Clients)
                                                     .ThenInclude(wc => wc.Client)
                                                     .Where(w => (w.Date >= developed_date
                                                               && w.Date >= initialgoal
                                                               && w.Week.Clinic.Id == user_logged.Clinic.Id
                                                               && w.Service == Common.Enums.ServiceType.Group))
                                                     .ToListAsync();
                            //aqui cargo todas las notas de PSR para ver las fechas y que en esas fechas no se generen notas de GROUP
                            workday_Client_PSR = await _context.Workdays_Clients
                                                               .Include(w => w.Workday)

                                                               .Where(w => (w.Workday.Date >= developed_date
                                                                      && w.Workday.Week.Clinic.Id == user_logged.Clinic.Id
                                                                      && w.Workday.Service == Common.Enums.ServiceType.PSR
                                                                      && w.Client.Id == client.Id))
                                                               .ToListAsync();
                            foreach (var item in workday_Client_PSR)
                            {
                                workday_Temp = workdays.FirstOrDefault(n => n.Date == item.Workday.Date);
                                if (workday_Temp != null)
                                {
                                    workdays.Remove(workday_Temp);
                                }
                                workday_Temp = new WorkdayEntity();
                            }
                            foreach (WorkdayEntity item in workdays)
                            {
                                //si el cliente no tiene asistencia en un dia laborable en Workdays_Clients entonces se crea
                                if (!item.Workdays_Clients.Any(wc => wc.Client.Id == client.Id) && client.MTPs.Where(n => n.Goals.Where(g => g.Service == ServiceType.Group && g.Objetives.Where(o => o.DateOpened <= item.Date && o.DateResolved >= item.Date).Count() > 0).Count() > 0).Count() > 0)
                                {
                                    //Verify the client is not present in other services of notes at the same time
                                     if (this.VerifyNotesAtSameTime(client.Id, group.Meridian, item.Date, original_group.Schedule.InitialTime, original_group.Schedule.EndTime, ServiceType.Group))
                                     {
                                         string test = ", review date (" + item.Date.ToShortDateString() + "). Individual Therapy";
                                         return RedirectToAction(nameof(EditGT), new { id = model.Id, error = 2, idClient = client.Id, all, test = test });
                                     }

                                     //verifico que el facilitator tenga disponibilidad para dar la terapia en el dia correspondiente                            
                                     if (this.VerifyFreeTimeOfFacilitator(group.Facilitator.Id, ServiceType.Group, group.Meridian, item.Date))
                                     {
                                         return RedirectToAction(nameof(EditGT), new { id = model.Id, error = 1, idFacilitator = group.Facilitator.Id, all });
                                     }

                                    //verifico que el Cliente tenga ese tiempo disponible en el TCM
                                    if (user_logged.Clinic.Setting.TCMClinic == true)
                                    {
                                        if (this.VerifyTCMNotesAtSameTime(client.Id, item.Date, original_group.Schedule.InitialTime, original_group.Schedule.EndTime))
                                        {
                                            return RedirectToAction(nameof(EditGT), new { id = model.Id, error = 2, idClient = client.Id, all });
                                        }
                                    }

                                    workday_client.Add(new Workday_Client
                                     {
                                         Workday = item,
                                         Client = client,
                                         Facilitator = client.Group.Facilitator,
                                         Session = client.Group.Meridian,
                                         Present = true,
                                         SharedSession = false,
                                         Schedule = client.Group.Schedule
                                     });     
                                }                                
                            }

                            if (workday_client.Count() > 0)
                            {
                                _context.AddRange(workday_client);
                            }
                        }
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Group", new { all = all});
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the group");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            GroupEntity groupEntity = await _context.Groups
                                                    .Include(g => g.Facilitator)
                                                    .Include(g => g.Clients)
                                                    .FirstOrDefaultAsync(g => g.Id == model.Id);

            GroupViewModel groupViewModel = _converterHelper.ToGroupViewModel(groupEntity);
            ViewData["am"] = groupViewModel.Am ? "true" : "false";

            List<ClientEntity> clients_list = await _context.Clients                                                            

                                                            .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                                      && c.Status == Common.Enums.StatusType.Open
                                                                      && c.Service == Common.Enums.ServiceType.Group))
                                                            .OrderBy(c => c.Name)
                                                            .ToListAsync();

            MultiSelectList client_list = new MultiSelectList(clients_list, "Id", "Name", groupViewModel.Clients.Select(c => c.Id));
            ViewData["clients"] = client_list;
            ViewData["all"] = all;
            return View(groupViewModel);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ListClientWithoutGroup(ServiceType serviceType = ServiceType.PSR)
        {
            List<ClientEntity> clients = new List<ClientEntity>();

            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                    .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic != null)
            {
               
                clients = await _context.Clients

                                        .Include(c => c.MTPs)
                                        .Include(c => c.Clients_HealthInsurances)
                                        .ThenInclude(c => c.HealthInsurance)
                                        .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                    && c.Status == Common.Enums.StatusType.Open
                                                    && c.Service == serviceType
                                                    && c.Group == null))
                                        .OrderBy(c => c.Name).ToListAsync();

                clients = clients.Where(c => c.MTPs.Count > 0).ToList();
                if (serviceType == ServiceType.Group)
                {
                    ViewData["service"] = "Group";
                }
                else
                {
                    ViewData["service"] = "PSR";
                }
                return View(clients);
            }

            return View(null);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ListClientWithoutIndividualTherapy(ServiceType serviceType = ServiceType.Individual)
        {
            List<ClientEntity> clients = new List<ClientEntity>();

            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                    .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic != null)
            {

                clients = await _context.Clients

                                        .Include(c => c.MTPs)
                                        .Include(c => c.Clients_HealthInsurances)
                                        .ThenInclude(c => c.HealthInsurance)
                                        .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                    && c.Status == Common.Enums.StatusType.Open
                                                    && c.IndividualTherapyFacilitator == null))
                                        .OrderBy(c => c.Name).ToListAsync();

                clients = clients.Where(c => c.MTPs.Count > 0).ToList();
               
                ViewData["service"] = "Individual";
                
                return View(clients);
            }

            return View(null);
        }

        [Authorize(Roles = "Manager, Facilitator")]
        public async Task<IActionResult> Individual()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
            {
                return RedirectToAction("Home/Error404");
            }

            int count = await _context.Clients
                                      .Where(c => (c.Status == StatusType.Open && c.IndividualTherapyFacilitator == null && c.Clinic.Id == user_logged.Clinic.Id &&
                                                   c.MTPs.Count() > 0))
                                      .CountAsync();
            if (count > 0)
            {
                ViewBag.Message = "1";
                ViewData["count"] = count;
            }

            if (user_logged.UserType.ToString() == "Manager")
            {
                List<GroupEntity> group = await _context.Groups
                                                        .Include(g => g.Facilitator)
                                                        .ThenInclude(n => n.ClientsFromIndividualTherapy)
                                                        .ThenInclude(g => g.DischargeList)
                                                        .Include(g => g.Clients)
                                                        
                                                        .Include(g => g.Schedule)
                                                        .ThenInclude(g => g.SubSchedules)

                                                        .Where(g => (g.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                            && g.Service == ServiceType.Individual))
                                                        .OrderBy(g => g.Facilitator.Name)
                                                        .ToListAsync();
                return View(group);
            }
          
            if (user_logged.UserType.ToString() == "Facilitator")
            {
                List<GroupEntity> group = await _context.Groups
                                                        .Include(g => g.Facilitator)
                                                        .ThenInclude(n => n.ClientsFromIndividualTherapy)
                                                        .ThenInclude(g => g.DischargeList)
                                                        .Include(g => g.Clients)
                                                        
                                                        .Include(g => g.Schedule)
                                                        .ThenInclude(g => g.SubSchedules)

                                                        .Where(g => (g.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                            && g.Service == ServiceType.Individual
                                                            && g.Facilitator.LinkedUser == user_logged.UserName))
                                                        .OrderBy(g => g.Facilitator.Name)
                                                        .ToListAsync();
                return View(group);
            }

            return RedirectToAction("Home/Error404");
        }

        [Authorize(Roles = "Manager")]
        public IActionResult CreateIndividualGroup(int id = 0, int error = 0, int idFacilitator = 0, int idClient = 0)
        {
            if (id == 1)
            {
                ViewBag.Creado = "Y";
            }
            else
            {
                if (id == 2)
                {
                    ViewBag.Creado = "E";
                }
                else
                {
                    ViewBag.Creado = "N";
                }
            }

            GroupViewModel model;
           
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null)
            {
                return RedirectToAction("Home/Error404");
            }

            model = new GroupViewModel
            {
                Facilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id),
                Schedules = _combosHelper.GetComboSchedulesByClinic(user_logged.Clinic.Id, ServiceType.Individual)
            };
           
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateIndividualGroup(GroupViewModel model, IFormCollection form)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                switch (form["Meridian"])
                {
                    case "Am":
                        {
                            model.Am = true;
                            model.Pm = false;
                            break;
                        }
                    case "Pm":
                        {
                            model.Am = false;
                            model.Pm = true;
                            break;
                        }
                    default:
                        break;
                }

                model.Service = Common.Enums.ServiceType.Individual;
                GroupEntity group = await _converterHelper.ToGroupEntity(model, true);
                _context.Add(group);

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("CreateIndividualGroup", new { id = 1 });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the group");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            model.Facilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id);

            return View(model);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditIndividualGroup(int? id, int error = 0, int idFacilitator = 0, int idClient = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GroupEntity groupEntity = await _context.Groups
                                                    .Include(g => g.Facilitator)
                                                    .Include(g => g.Clients)
                                                    .Include(g => g.Schedule)
                                                    .FirstOrDefaultAsync(g => g.Id == id);
            if (groupEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            //the facilitator has not availability on that dates
           
            GroupViewModel groupViewModel = _converterHelper.ToGroupViewModel(groupEntity);
            ViewData["am"] = groupViewModel.Am ? "true" : "false";
            
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null)
            {
                return RedirectToAction("Home/Error404");
            }

            groupViewModel.Facilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id);
            groupViewModel.Schedules = _combosHelper.GetComboSchedulesByClinic(user_logged.Clinic.Id, ServiceType.Individual);

            return View(groupViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditIndividualGroup(GroupViewModel model, IFormCollection form)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                switch (form["Meridian"])
                {
                    case "Am":
                        {
                            model.Am = true;
                            model.Pm = false;
                            break;
                        }
                    case "Pm":
                        {
                            model.Am = false;
                            model.Pm = true;
                            break;
                        }
                    default:
                        break;
                }

                model.Service = Common.Enums.ServiceType.Individual;
                GroupEntity group = await _converterHelper.ToGroupEntity(model, false);
                _context.Update(group);

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Individual));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the group");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            GroupEntity groupEntity = await _context.Groups
                                                    .Include(g => g.Facilitator)
                                                    .Include(g => g.Clients)
                                                    .FirstOrDefaultAsync(g => g.Id == model.Id);

            GroupViewModel groupViewModel = _converterHelper.ToGroupViewModel(groupEntity);
            ViewData["am"] = groupViewModel.Am ? "true" : "false";

            return View(groupViewModel);
        }

        [Authorize(Roles = "Manager")]
        public IActionResult DeleteIndividual(int id = 0)
        {
            if (id > 0)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                DeleteViewModel model = new DeleteViewModel
                {
                    Id_Element = id,
                    Desciption = "Do you want to delete this record?"

                };
                return View(model);
            }
            else
            {
                //Edit
                //return View(new Client_DiagnosticViewModel());
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteIndividual(DeleteViewModel groupViewModel)
        {

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                GroupEntity group = await _context.Groups
                                                  .FirstAsync(n => n.Id == groupViewModel.Id_Element);

                try
                {
                    _context.Groups.Remove(group);
                    await _context.SaveChangesAsync();
                    List<GroupEntity> ListGroup = await _context.Groups

                                                                .Include(g => g.Facilitator)
                                                                .ThenInclude(n => n.ClientsFromIndividualTherapy)
                                                                .Include(g => g.Clients)
                                                                .Include(g => g.Schedule)

                                                                .Where(g => (g.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                    && g.Service == ServiceType.Individual))
                                                                .OrderBy(g => g.Facilitator.Name)
                                                                .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewIndividualGroup", ListGroup) });
                }
                catch (Exception)
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewIndividualGroup", _context.Schedule.Include(n => n.SubSchedules).ToListAsync()) });
                }


            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewIndividualGroup", _context.Schedule.Include(n => n.SubSchedules).ToListAsync()) });
        }


        [Authorize(Roles = "Manager")]
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

            return true;*/

            return false;
        }

        [Authorize(Roles = "Manager")]
        private bool VerifyNotesAtSameTime(int idClient, string session, DateTime date, DateTime initialTime, DateTime endTime, ServiceType service)
        {
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
            }
            

            //PSR notes
            if (session == "AM" && service == ServiceType.PSR)
            {
                if (_context.Workdays_Clients
                            .Where(wc => (wc.Client.Id == idClient && wc.Session == "8.00 - 9.00 AM" && wc.Workday.Date == date))
                            .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "9.05 - 10.05 AM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "10.15 - 11.15 AM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "11.20 - 12.20 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Workday.Date == date && wc.Workday.Service == ServiceType.Group))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient 
                                    && wc.Workday.Date == date 
                                    && wc.Workday.Service == ServiceType.Individual
                                    && ((wc.IndividualNote.SubSchedule.InitialTime.TimeOfDay < initialTime.TimeOfDay && wc.IndividualNote.SubSchedule.EndTime.TimeOfDay > initialTime.TimeOfDay
                                        || wc.IndividualNote.SubSchedule.InitialTime.TimeOfDay < endTime.TimeOfDay && wc.IndividualNote.SubSchedule.EndTime.TimeOfDay > endTime.TimeOfDay))))
                                  .Count() > 0)
                    return true;
                return false;
            }
            if (session == "PM" && service == ServiceType.PSR)
            {
                if (_context.Workdays_Clients
                            .Where(wc => (wc.Client.Id == idClient && wc.Session == "12.45 - 1.45 PM" && wc.Workday.Date == date))
                            .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "1.50 - 2.50 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "3.00 - 4.00 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "4.05 - 5.05 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Workday.Date == date && wc.Workday.Service == ServiceType.Group))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient
                                    && wc.Workday.Date == date
                                    && wc.Workday.Service == ServiceType.Individual
                                    && ((wc.IndividualNote.SubSchedule.InitialTime.TimeOfDay < initialTime.TimeOfDay && wc.IndividualNote.SubSchedule.EndTime.TimeOfDay > initialTime.TimeOfDay
                                        || wc.IndividualNote.SubSchedule.InitialTime.TimeOfDay < endTime.TimeOfDay && wc.IndividualNote.SubSchedule.EndTime.TimeOfDay > endTime.TimeOfDay))))
                                  .Count() > 0)
                    return true;
                return false;
            }

            //Group notes
            if (session == "AM" && service == ServiceType.Group)
            {
                if (_context.Workdays_Clients
                            .Where(wc => (wc.Client.Id == idClient && wc.Session == "8.00 - 9.00 AM" && wc.Workday.Date == date))
                            .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "9.05 - 10.05 AM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "10.15 - 11.15 AM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "11.20 - 12.20 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Workday.Date == date && wc.Workday.Service == ServiceType.PSR))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient
                                    && wc.Workday.Date == date
                                    && wc.Workday.Service == ServiceType.Individual
                                    && ((wc.IndividualNote.SubSchedule.InitialTime.TimeOfDay < initialTime.TimeOfDay && wc.IndividualNote.SubSchedule.EndTime.TimeOfDay > initialTime.TimeOfDay
                                        || wc.IndividualNote.SubSchedule.InitialTime.TimeOfDay < endTime.TimeOfDay && wc.IndividualNote.SubSchedule.EndTime.TimeOfDay > endTime.TimeOfDay))))
                                  .Count() > 0)
                    return true;
                return false;
            }
            if (session == "PM" && service == ServiceType.Group)
            {
                if (_context.Workdays_Clients
                            .Where(wc => (wc.Client.Id == idClient && wc.Session == "12.45 - 1.45 PM" && wc.Workday.Date == date))
                            .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "1.50 - 2.50 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "3.00 - 4.00 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "4.05 - 5.05 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Workday.Date == date && wc.Workday.Service == ServiceType.PSR))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient
                                    && wc.Workday.Date == date
                                    && wc.Workday.Service == ServiceType.Individual
                                    && ((wc.IndividualNote.SubSchedule.InitialTime.TimeOfDay < initialTime.TimeOfDay && wc.IndividualNote.SubSchedule.EndTime.TimeOfDay > initialTime.TimeOfDay
                                        || wc.IndividualNote.SubSchedule.InitialTime.TimeOfDay < endTime.TimeOfDay && wc.IndividualNote.SubSchedule.EndTime.TimeOfDay > endTime.TimeOfDay))))
                                  .Count() > 0)
                    return true;
                return false;
            }

            return true;
        }

        [Authorize(Roles = "Manager")]
        private bool VerifyTCMNotesAtSameTime(int idClient, DateTime date, DateTime initialTime, DateTime endTime)
        {
            TCMClientEntity tcmclient = _context.TCMClient
                                                .Include(n => n.TCMNote)
                                                .ThenInclude(n => n.TCMNoteActivity)
                                                .Include(n => n.Client)
                                                .AsSplitQuery()
                                                .FirstOrDefault(c => c.Client.Id == idClient);
            if (tcmclient != null)
            {
                if (tcmclient.TCMNote.Count() > 0)
                {
                    if (tcmclient.TCMNote.Where(n => (n.DateOfService == date
                                       && n.TCMNoteActivity.Where(m => (m.StartTime.TimeOfDay <= initialTime.TimeOfDay && m.EndTime.TimeOfDay >= initialTime.TimeOfDay)
                                           || (m.StartTime.TimeOfDay <= endTime.TimeOfDay && m.EndTime.TimeOfDay >= endTime.TimeOfDay)
                                           || (m.StartTime.TimeOfDay > initialTime.TimeOfDay && m.EndTime.TimeOfDay > initialTime.TimeOfDay && m.StartTime.TimeOfDay < endTime.TimeOfDay && m.EndTime.TimeOfDay < endTime.TimeOfDay))
                                       .Count() > 0))
                                     .Count() > 0)
                        return true;
                    else return false;
                }
                else
                {
                    return false;
                }                 
            }          

            return false;
        }

        #endregion
    }
}