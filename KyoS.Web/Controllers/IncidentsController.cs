using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KyoS.Common.Enums;
using KyoS.Common.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;

namespace KyoS.Web.Controllers
{
    public class IncidentsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        
        public IncidentsController(DataContext context, IConverterHelper converterHelper)
        {
            _context = context;            
            _converterHelper = converterHelper;           
        }
        
        [Authorize(Roles = "Admin, Manager, Supervisor, Facilitator, CaseManager, Documents_Assistant")]
        public async Task<IActionResult> Index(int idError = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            if (User.IsInRole("Admin"))
            {
                return View(await _context.Incidents
                                          .Include(i => i.UserCreatedBy)
                                          .ThenInclude(u => u.Clinic)
                                          .Include(i => i.client)
                                          .Include(i => i.UserAsigned)
                                          .OrderByDescending(i => i.CreatedDate)
                                          .ToListAsync());
            }

            if (User.IsInRole("CaseManager"))
            {
                return View(await _context.Incidents
                                          .Include(i => i.UserCreatedBy)
                                          .ThenInclude(u => u.Clinic)
                                          .Include(i => i.client)
                                          .Include(i => i.UserAsigned)
                                          .OrderByDescending(i => i.CreatedDate)
                                          .ToListAsync());
            }
            
            if (User.IsInRole("Manager"))
            {
                return View(await _context.Incidents
                                          .Include(i => i.UserCreatedBy)
                                          .ThenInclude(u => u.Clinic)
                                          .Include(i => i.client)
                                          .Include(i => i.UserAsigned)
                                          .Where(i => i.UserCreatedBy.Clinic.Id == user_logged.Clinic.Id)
                                          .OrderByDescending(i => i.CreatedDate)
                                          .ToListAsync());
            }

            if (User.IsInRole("Supervisor") || User.IsInRole("Facilitator") || User.IsInRole("Documents_Assistant"))
            {
                return View(await _context.Incidents
                                          .Include(i => i.UserCreatedBy)
                                          .ThenInclude(u => u.Clinic)
                                          .Include(i => i.client)
                                          .Include(i => i.UserAsigned)
                                          .Where(i => i.UserCreatedBy == user_logged || i.UserAsigned == user_logged)
                                          .OrderByDescending(i => i.CreatedDate)
                                          .ToListAsync());
            }

            return View(null);
        }
        
        [Authorize(Roles = "Admin, Manager, Supervisor, Facilitator, Documents_Assistant")]
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

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);            

            IncidentViewModel model = new IncidentViewModel();
            List<SelectListItem> list = new List<SelectListItem>();

            if (User.IsInRole("Facilitator"))
            {
                FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                list = _context.Clients.Where(n => n.IdFacilitatorPSR == facilitator.Id || n.IndividualTherapyFacilitator.Id == facilitator.Id).OrderBy(n => n.Name).Select(c => new SelectListItem
                {
                    Text = $"{c.Name + " | " + c.AdmisionDate.ToShortDateString()}",
                    Value = $"{c.Id}"
                }).ToList();

            }
            else
            {
                list = _context.Clients.OrderBy(n => n.Name).Select(c => new SelectListItem
                {
                    Text = $"{c.Name + " | " + c.AdmisionDate.ToShortDateString()}",
                    Value = $"{c.Id}"
                }).ToList();
            }
            
            List<SelectListItem> list_user = _context.Users.OrderBy(n => n.FirstName).Select(c => new SelectListItem
            {
                Text = $"{c.FullName}",
                Value = $"{c.Id}"
            }).ToList();

            list_user.Insert(0, new SelectListItem
            {
                Text = "[Select a person to assign this incident...]",
                Value = "0"
            });

            model = new IncidentViewModel()
            {
                IdClient = 0,
                Clients = list,
                IdUserAssigned = "",
                Users = list_user
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager, Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> Create(IncidentViewModel incidentViewModel)
        {
            if (ModelState.IsValid)
            {
                IncidentEntity incident = await _context.Incidents.FirstOrDefaultAsync(c => c.Description == incidentViewModel.Description && c.client.Id == incidentViewModel.IdClient);
                if (incident == null)
                {
                    UserEntity user_logged = _context.Users
                                                     .Include(u => u.Clinic)
                                                     .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    IncidentEntity incidentEntity = await _converterHelper.ToIncidentEntity(incidentViewModel, true, user_logged.Id);

                    _context.Add(incidentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the incident: {incidentEntity.Description}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else
                {
                    return RedirectToAction("Create", new { id = 2 });
                }
            }
            return View(incidentViewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            IncidentEntity incidentEntity = await _context.Incidents.FirstOrDefaultAsync(i => i.Id == id);
            if (incidentEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Incidents.Remove(incidentEntity);
                await _context.SaveChangesAsync();
            }
            catch (System.Exception)
            {

                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin, Manager, Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            IncidentEntity incidentEntity = await _context.Incidents
                                                          .Include(n => n.client)
                                                          .Include(n => n.UserAsigned)
                                                          .Include(i => i.UserCreatedBy)                                                      
                                                          .FirstOrDefaultAsync(i => i.Id == id);

            if (incidentEntity.UserAsigned != null)
            {
                if (incidentEntity.UserAsigned.Id == user_logged.Id)
                {
                    ViewData["Assigned"] = 1;
                }
                else
                {
                    ViewData["Assigned"] = 0;
                }
            }
            else
            {
                ViewData["Assigned"] = 0;
            }
            if (incidentEntity.UserCreatedBy != null)
            {
                if (incidentEntity.UserCreatedBy.Id == user_logged.Id)
                {
                    ViewData["CreatedBy"] = 1;
                }
                else
                {
                    ViewData["CreatedBy"] = 0;
                }
            }
            else
            {
                ViewData["CreatedBy"] = 0;
            }

            if (incidentEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            IncidentViewModel incidentViewModel = _converterHelper.ToIncidentViewModel(incidentEntity);

            return View(incidentViewModel);
        }

        [Authorize(Roles = "Admin, Manager, Supervisor, Facilitator, Documents_Assistant")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IncidentViewModel incidentViewModel)
        {
            if (id != incidentViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                IncidentEntity incidentEntity = await _converterHelper.ToIncidentEntity(incidentViewModel, false, user_logged.Id);
                _context.Update(incidentEntity);

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));                    
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the incident: {incidentEntity.Description}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(incidentViewModel);
        }
    }
}