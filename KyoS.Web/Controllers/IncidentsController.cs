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
        
        [Authorize(Roles = "Admin, Mannager, Facilitator, CaseMannager")]
        public async Task<IActionResult> Index(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            if (User.IsInRole("Admin"))
            {
                return View(await _context.Incidents
                                          .Include(i => i.UserCreatedBy)
                                          .ThenInclude(u => u.Clinic)
                                          .OrderByDescending(i => i.CreatedDate)
                                          .ToListAsync());
            }

            if (User.IsInRole("CaseMannager"))
            {
                return View(await _context.Incidents
                                          .Include(i => i.UserCreatedBy)
                                          .ThenInclude(u => u.Clinic)
                                          .OrderByDescending(i => i.CreatedDate)
                                          .ToListAsync());
            }
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
            {
                return RedirectToAction("Home/Error404");
            }

            if (User.IsInRole("Mannager"))
            {
                return View(await _context.Incidents
                                          .Include(i => i.UserCreatedBy)
                                          .ThenInclude(u => u.Clinic)
                                          .Where(i => i.UserCreatedBy.Clinic.Id == user_logged.Clinic.Id)
                                          .OrderByDescending(i => i.CreatedDate)
                                          .ToListAsync());
            }

            if (User.IsInRole("Facilitator"))
            {
                return View(await _context.Incidents
                                          .Include(i => i.UserCreatedBy)
                                          .ThenInclude(u => u.Clinic)
                                          .Where(i => i.UserCreatedBy == user_logged)
                                          .OrderByDescending(i => i.CreatedDate)
                                          .ToListAsync());
            }

            return View(null);
        }
        
        [Authorize(Roles = "Admin, Mannager, Facilitator")]
        public IActionResult Create(int id = 0)
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

            IncidentViewModel model = new IncidentViewModel();
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IncidentViewModel incidentViewModel)
        {
            if (ModelState.IsValid)
            {
                IncidentEntity incident = await _context.Incidents.FirstOrDefaultAsync(c => c.Description == incidentViewModel.Description);
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

        [Authorize(Roles = "Admin, Mannager, Facilitator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            IncidentEntity incidentEntity = await _context.Incidents
                                                          .Include(i => i.UserCreatedBy)                                                      
                                                          .FirstOrDefaultAsync(i => i.Id == id);
            if (incidentEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            IncidentViewModel incidentViewModel = _converterHelper.ToIncidentViewModel(incidentEntity);

            return View(incidentViewModel);
        }

        [Authorize(Roles = "Admin, Mannager, Facilitator")]
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