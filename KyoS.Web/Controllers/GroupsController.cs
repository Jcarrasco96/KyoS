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
    [Authorize(Roles = "Mannager")]
    public class GroupsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IReportHelper _reportHelper;
        
        public GroupsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IReportHelper reportHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
                return View(await _context.Groups.Include(g => g.Facilitator).Include(g => g.Clients).OrderBy(g => g.Facilitator.Name).ToListAsync());
            else
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic == null)
                    return View(await _context.Groups.Include(g => g.Facilitator).Include(g => g.Clients).OrderBy(g => g.Facilitator.Name).ToListAsync());

                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                if (clinic != null)
                    return View(await _context.Groups.Include(g => g.Facilitator).Include(g => g.Clients)
                                                     .Where(g => g.Facilitator.Clinic.Id == clinic.Id).OrderBy(g => g.Facilitator.Name).ToListAsync());
                else
                    return View(await _context.Groups.Include(g => g.Facilitator).Include(g => g.Clients).OrderBy(g => g.Facilitator.Name).ToListAsync());
            }
        }

        public async Task<IActionResult> Create(int id = 0)
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
            MultiSelectList client_list;
            List<ClientEntity> clients = new List<ClientEntity>();

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    model = new GroupViewModel
                    {
                        Facilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id)
                    };
                    clients = await _context.Clients
                                            .Include(c => c.MTPs)
                                            .Where(c => (c.Clinic.Id == user_logged.Clinic.Id && c.Status == Common.Enums.StatusType.Open))
                                            .OrderBy(c => c.Name).ToListAsync();
                    clients = clients.Where(c => c.MTPs.Count > 0).ToList();
                    client_list = new MultiSelectList(clients, "Id", "Name");
                    ViewData["clients"] = client_list;
                    return View(model);
                }
            }

            model = new GroupViewModel
            {
                Facilitators = _combosHelper.GetComboFacilitators()
            };

            clients = await _context.Clients
                                    .Include(c => c.MTPs)
                                    .OrderBy(c => c.Name).ToListAsync();
            clients = clients.Where(c => c.MTPs.Count > 0).ToList();
            client_list = new MultiSelectList(clients, "Id", "Name");
            ViewData["clients"] = client_list;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupViewModel model, IFormCollection form)
        {
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
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);

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
                        DateTime admission_date;
                        List<WorkdayEntity> workdays;
                        Workday_Client workday_client;
                        if (client != null)
                        {
                            client.Group = group;
                            _context.Update(client);

                            //verifico que el cliente tenga la asistencia necesaria dada su fecha de admision
                            admission_date = client.MTPs.First().AdmisionDate;
                            workdays = await _context.Workdays
                                                     .Include(w => w.Workdays_Clients)
                                                     .ThenInclude(wc => wc.Client)
                                                     .Where(w => (w.Date >= admission_date
                                                                  && w.Week.Clinic.Id == user_logged.Clinic.Id))
                                                     .ToListAsync();
                            foreach (WorkdayEntity item in workdays)
                            {
                                //si el cliente no tiene asistencia en un dia laborable en Workdays_Clients entonces se crea
                                if (!item.Workdays_Clients.Any(wc => wc.Client.Id == client.Id))
                                {
                                    workday_client = new Workday_Client
                                    {
                                        Workday = item,
                                        Client = client,
                                        Facilitator = client.Group.Facilitator,
                                        Session = client.Group.Meridian,
                                        Present = true
                                    };
                                    _context.Add(workday_client);
                                }
                                /*else  //si tiene asistencia, solo hay que verificar que la session(am o pm) sea la misma
                                {
                                    workday_client = item.Workdays_Clients.FirstOrDefault(wc => wc.Client.Id == client.Id);
                                    if (workday_client.Session != client.Group.Meridian)
                                    {
                                        workday_client.Session = client.Group.Meridian;
                                    }
                                    workday_client.Facilitator = group.Facilitator;
                                    _context.Update(workday_client);
                                }*/
                            }
                        }
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Create", new { id = 1 });
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

            MultiSelectList client_list = new MultiSelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["clients"] = client_list;

            return View(model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GroupEntity groupEntity = await _context.Groups.Include(g => g.Facilitator)
                                                           .Include(g => g.Clients).FirstOrDefaultAsync(g => g.Id == id);
            if (groupEntity == null)
            {
                return NotFound();
            }

            MultiSelectList client_list;
            GroupViewModel groupViewModel = _converterHelper.ToGroupViewModel(groupEntity);
            ViewData["am"] = groupViewModel.Am ? "true" : "false";
            List<ClientEntity> clients = new List<ClientEntity>();

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    groupViewModel.Facilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id);

                    clients = await _context.Clients
                                            .Include(c => c.MTPs)
                                            .Where(c => (c.Clinic.Id == user_logged.Clinic.Id && c.Status == Common.Enums.StatusType.Open))
                                            .OrderBy(c => c.Name)
                                            .ToListAsync();
                    clients = clients.Where(c => c.MTPs.Count > 0).ToList();
                    client_list = new MultiSelectList(clients, "Id", "Name", groupViewModel.Clients.Select(c => c.Id));
                    ViewData["clients"] = client_list;
                    return View(groupViewModel);
                }
            }

            clients = await _context.Clients.Include(c => c.MTPs).ToListAsync();
            clients = clients.Where(c => c.MTPs.Count > 0).ToList();
            client_list = new MultiSelectList(clients, "Id", "Name", groupViewModel.Clients.Select(c => c.Id));
            ViewData["clients"] = client_list;
            return View(groupViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GroupViewModel model, IFormCollection form)
        {
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

                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);

                GroupEntity group = await _converterHelper.ToGroupEntity(model, false);
                _context.Update(group);

                GroupEntity original_group = await _context.Groups.Include(g => g.Clients)
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
                    DateTime admission_date;
                    List<WorkdayEntity> workdays;
                    Workday_Client workday_client;
                    foreach (string value in clients)
                    {
                        client = await _context.Clients
                                               .Include(c => c.MTPs)
                                               .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(value));
                        if (client != null)
                        {
                            client.Group = group;
                            _context.Update(client);

                            //verifico que el cliente tenga la asistencia necesaria dada su fecha de admision
                            admission_date = client.MTPs.First().AdmisionDate;
                            workdays = await _context.Workdays
                                                     .Include(w => w.Workdays_Clients)
                                                     .ThenInclude(wc => wc.Client)
                                                     .Where(w => (w.Date >= admission_date
                                                                  && w.Week.Clinic.Id == user_logged.Clinic.Id))
                                                     .ToListAsync();
                            foreach (WorkdayEntity item in workdays)
                            {
                                //si el cliente no tiene asistencia en un dia laborable en Workdays_Clients entonces se crea
                                if (!item.Workdays_Clients.Any(wc => wc.Client.Id == client.Id))
                                {
                                    workday_client = new Workday_Client
                                    {
                                        Workday = item,
                                        Client = client,
                                        Facilitator = client.Group.Facilitator,
                                        Session = client.Group.Meridian,
                                        Present = true
                                    };
                                    _context.Add(workday_client);
                                }
                                /*else  //si tiene asistencia, solo hay que verificar que la session(am o pm) sea la misma
                                {
                                    workday_client = item.Workdays_Clients.FirstOrDefault(wc => wc.Client.Id == client.Id);
                                    if (workday_client.Session != client.Group.Meridian)
                                    {
                                        workday_client.Session = client.Group.Meridian;                                        
                                    }
                                    workday_client.Facilitator = group.Facilitator;
                                    _context.Update(workday_client);
                                } */
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
                        ModelState.AddModelError(string.Empty, "Already exists the group");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            GroupEntity groupEntity = await _context.Groups.Include(g => g.Facilitator)
                                                           .Include(g => g.Clients).FirstOrDefaultAsync(g => g.Id == model.Id);
            GroupViewModel groupViewModel = _converterHelper.ToGroupViewModel(groupEntity);
            ViewData["am"] = groupViewModel.Am ? "true" : "false";

            //List<ClientEntity> list = await (from cl in _context.Clients
            //                                 join g in groupViewModel.Clients on cl.Id equals g.Id
            //                                 select cl).ToListAsync();

            MultiSelectList client_list = new MultiSelectList(await _context.Clients.ToListAsync(), "Id", "Name", groupViewModel.Clients.Select(c => c.Id));
            ViewData["clients"] = client_list;

            return View(groupViewModel);
        }

        public async Task<IActionResult> Print(int id)
        {            
            var result = await _reportHelper.GroupAsyncReport(id);
            return await Task.Run(() => File(result, System.Net.Mime.MediaTypeNames.Application.Pdf));            
        }        
    }
}