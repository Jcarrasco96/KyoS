﻿using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Manager")]
    public class DiagnosticsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;

        public DiagnosticsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
        }
        
        public async Task<IActionResult> Index(int idError = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            if (User.IsInRole("Admin"))
            {
                return View(await _context.Diagnostics.OrderBy(d => d.Code).ToListAsync());
            }
            else
            {
                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);

                if (clinic != null)
                {
                    List<DiagnosticEntity> diagnostics = await _context.Diagnostics
                                                                       .Include(n => n.Client_Diagnostics)
                                                                       .ThenInclude(n => n.Client)
                                                                       .OrderBy(d => d.Code).ToListAsync();
                    List<DiagnosticEntity> diagnostics_by_clinic = new List<DiagnosticEntity>();
                    UserEntity user;
                    foreach (DiagnosticEntity item in diagnostics)
                    {
                        user = _context.Users.FirstOrDefault(u => u.Id == item.CreatedBy);
                        if (clinic.Users.Contains(user))
                        {
                            diagnostics_by_clinic.Add(item);
                        }
                    }
                    return View(diagnostics_by_clinic);
                }
                else
                {
                    return View(null);
                }
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

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            DiagnosticViewModel model = new DiagnosticViewModel();            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiagnosticViewModel diagnosticViewModel)
        {
            if (ModelState.IsValid)
            {
                DiagnosticEntity diagnostic = await _context.Diagnostics.FirstOrDefaultAsync(c => c.Code == diagnosticViewModel.Code);
                if (diagnostic == null)
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    DiagnosticEntity diagnosticEntity = _converterHelper.ToDiagnosticEntity(diagnosticViewModel, true, user_logged.Id);

                    _context.Add(diagnosticEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the diagnostic: {diagnosticEntity.Code}");
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
            return View(diagnosticViewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            DiagnosticEntity diagnosticEntity = await _context.Diagnostics.FirstOrDefaultAsync(c => c.Id == id);
            if (diagnosticEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DiagnosticViewModel diagnosticViewModel = _converterHelper.ToDiagnosticViewModel(diagnosticEntity);

            return View(diagnosticViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DiagnosticViewModel diagnosticViewModel)
        {
            if (id != diagnosticViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                DiagnosticEntity diagnosticEntity = _converterHelper.ToDiagnosticEntity(diagnosticViewModel, false, user_logged.Id);
                _context.Update(diagnosticEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the diagnostic: {diagnosticEntity.Code}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(diagnosticViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DiagnosticEntity diagnosticEntity = await _context.Diagnostics.FirstOrDefaultAsync(c => c.Id == id);
            if (diagnosticEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Diagnostics.Remove(diagnosticEntity);
                await _context.SaveChangesAsync();
            }
            catch (System.Exception)
            {

                return RedirectToAction("Index", new { idError = 1 });
            }
            
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CreateModal(int id = 0)
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
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            DiagnosticViewModel model = new DiagnosticViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(DiagnosticViewModel diagnosticViewModel)
        {
            if (ModelState.IsValid)
            {
                DiagnosticEntity diagnostic = await _context.Diagnostics.FirstOrDefaultAsync(c => c.Code == diagnosticViewModel.Code);
                if (diagnostic == null)
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    DiagnosticEntity diagnosticEntity = _converterHelper.ToDiagnosticEntity(diagnosticViewModel, true, user_logged.Id);

                    _context.Add(diagnosticEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        
                        List<DiagnosticEntity> diagnostics_List = await _context.Diagnostics                                                                                
                                                                                .OrderBy(d => d.Code)
                                                                                .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDiagnostics", diagnostics_List) });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the diagnostic: {diagnosticEntity.Code}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else
                {
                    //return RedirectToAction("Create", new { id = 2 });
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", diagnosticViewModel) });
                }
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", diagnosticViewModel) });
        }

        public async Task<IActionResult> EditModal(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            DiagnosticEntity diagnosticEntity = await _context.Diagnostics.FirstOrDefaultAsync(c => c.Id == id);
            if (diagnosticEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DiagnosticViewModel diagnosticViewModel = _converterHelper.ToDiagnosticViewModel(diagnosticEntity);

            return View(diagnosticViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModal(int id, DiagnosticViewModel diagnosticViewModel)
        {
            if (id != diagnosticViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                DiagnosticEntity diagnosticEntity = _converterHelper.ToDiagnosticEntity(diagnosticViewModel, false, user_logged.Id);
                _context.Update(diagnosticEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<DiagnosticEntity> diagnostics_List = await _context.Diagnostics

                                                                            .OrderBy(d => d.Code)
                                                                            .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDiagnostics", diagnostics_List) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the diagnostic: {diagnosticEntity.Code}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", diagnosticViewModel) });
        }

        public async Task<IActionResult> ViewDetails(int id = 0, StatusType status = StatusType.Open, bool principal = true)
        {
            List<ClientEntity> clients = new List<ClientEntity>();
            if (id > 0)
            {
                UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }
                if (principal == false)
                {
                    clients = await _context.Clients
                                            .Include(n => n.Clients_Diagnostics)
                                            .ThenInclude(n => n.Diagnostic)
                                            .Where(n => n.Clients_Diagnostics.Where(m => m.Principal == principal && m.Diagnostic.Id == id).Count() > 0
                                                     && n.Status == status)
                                            .OrderBy(d => d.Name)
                                            .ToListAsync();
                }
                else
                {
                    clients = await _context.Clients
                                            .Include(n => n.Clients_Diagnostics)
                                            .ThenInclude(n => n.Diagnostic)
                                            .Where(n => n.Clients_Diagnostics.Where(m => m.Principal == principal && m.Diagnostic.Id == id).Count() > 0)
                                            .OrderBy(d => d.Name)
                                            .ToListAsync();
                }

            }


            return View(clients);
        }

    }
}