﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    public class TCMServicesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IRenderHelper _renderHelper;

        public TCMServicesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
            _renderHelper = renderHelper;
        }
        
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Index(int idError = 0)
        {
            if (User.IsInRole("Admin"))
            {
                List<TCMServiceEntity> tcmservices = await _context.TCMServices
                                                                   .Include(m => m.Stages)
                                                                   .OrderBy(f => f.Code)
                                                                   .ToListAsync();

                return View(tcmservices);
            }
            else
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .ThenInclude(c => c.Setting)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                if (idError == 1) //Imposible to delete
                {
                    ViewBag.Delete = "N";
                }
                List<TCMServiceEntity> tcmservices = await _context.TCMServices
                                       .Include(m => m.Stages)
                                       .OrderBy(f => f.Code)
                                       .ToListAsync();

                return View(tcmservices);
            }
        }

        [Authorize(Roles = "Admin, Manager")]
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

            TCMServiceViewModel model;

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
                    model = new TCMServiceViewModel
                    {
                        Clinics = list,
                        IdClinic = clinic.Id,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now
                    };
                    return View(model);
                }
            }

            model = new TCMServiceViewModel
            {
                Clinics = _combosHelper.GetComboClinics(),
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Create(TCMServiceViewModel tcmServiceViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMServiceEntity tcmServiceEntity = await _context.TCMServices.FirstOrDefaultAsync(s => s.Code == tcmServiceViewModel.Code);
                if (tcmServiceEntity == null)
                {
                    tcmServiceEntity = await _converterHelper.ToTCMServiceEntity(tcmServiceViewModel, true, user_logged.UserName);
                    _context.Add(tcmServiceEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        List<TCMServiceEntity> tcmService = await _context.TCMServices
                                                                           .Include(m => m.Stages)
                                                                           .OrderBy(f => f.Code)
                                                                           .ToListAsync();
                        
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMServices", tcmService) });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the TCM service.");

                    if (User.IsInRole("Admin"))
                    {
                        tcmServiceViewModel.Clinics = _combosHelper.GetComboClinics();
                    }
                    else
                    {
                        List<SelectListItem> list = new List<SelectListItem>();
                        list.Insert(0, new SelectListItem
                        {
                            Text = user_logged.Clinic.Name,
                            Value = $"{user_logged.Clinic.Id}"
                        });

                        tcmServiceViewModel.Clinics = list;
                    }
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmServiceViewModel) });
                }
            }

            //recovery data
            if (User.IsInRole("Admin"))
            {
                tcmServiceViewModel.Clinics = _combosHelper.GetComboClinics();
            }
            else
            {
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = user_logged.Clinic.Name,
                    Value = $"{user_logged.Clinic.Id}"
                });

                tcmServiceViewModel.Clinics = list;
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmServiceViewModel) });
        }

        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMServiceEntity tcmServiceEntity = await _context.TCMServices.FirstOrDefaultAsync(s => s.Id == id);
            if (tcmServiceEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMServices.Remove(tcmServiceEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMServiceEntity tcmServiceEntity = await _context.TCMServices.Include(s => s.Clinic).FirstOrDefaultAsync(s => s.Id == id);
            if (tcmServiceEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMServiceViewModel tcmServiceViewModel;

            if (!User.IsInRole("Admin"))
            {
                tcmServiceViewModel = _converterHelper.ToTCMServiceViewModel(tcmServiceEntity, user_logged.Clinic.Id);

                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = user_logged.Clinic.Name,
                    Value = $"{user_logged.Clinic.Id}"
                });

                tcmServiceViewModel.Clinics = list;
                tcmServiceViewModel.IdClinic = user_logged.Clinic.Id;
            }
            else
            {
                tcmServiceViewModel = _converterHelper.ToTCMServiceViewModel(tcmServiceEntity, 0);
            }

            return View(tcmServiceViewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, TCMServiceViewModel tcmServiceViewModel)
        {
            if (id != tcmServiceViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMServiceEntity tcmServiceEntity = await _converterHelper.ToTCMServiceEntity(tcmServiceViewModel, false, user_logged.UserName);
                _context.Update(tcmServiceEntity);

                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMServiceEntity> tcmService = await _context.TCMServices
                                                                      .Include(m => m.Stages)
                                                                      .OrderBy(f => f.Code)
                                                                      .ToListAsync();
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMServices", tcmService) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the TCM service: {tcmServiceEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            //recovery data
            if (User.IsInRole("Admin"))
            {
                tcmServiceViewModel.Clinics = _combosHelper.GetComboClinics();
            }
            else
            {
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = user_logged.Clinic.Name,
                    Value = $"{user_logged.Clinic.Id}"
                });

                tcmServiceViewModel.Clinics = list;
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", tcmServiceViewModel) });
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateStage(int id = 0)
        {
            TCMStageViewModel model;

            TCMServiceEntity tcmservice = await _context.TCMServices
                                                .Include(g => g.Stages)
                                                .FirstOrDefaultAsync(m => m.Id == id);

            if (User.IsInRole("Manager"))
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
                    model = new TCMStageViewModel
                    {
                        tCMservice = tcmservice,
                        Clinics = list,
                        IdClinic = clinic.Id,
                        Id_TCMService = id,
                        ID_Etapa = tcmservice.Stages.Count() + 1,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now
                    };

                    return View(model);
                }
            }

            model = new TCMStageViewModel
            {
                Clinics = _combosHelper.GetComboClinics(),
                Id_TCMService = id,
                tCMservice = tcmservice,
                ID_Etapa = tcmservice.Stages.Count() + 1,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateStage(TCMStageViewModel tcmStageViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMServiceEntity tcmservice = await _context.TCMServices
                                               .Include(g => g.Stages)
                                               .FirstOrDefaultAsync(m => m.Id == tcmStageViewModel.Id_TCMService);
            if (User.IsInRole("Manager"))
            {
                if (ModelState.IsValid)
                {
                    TCMStageEntity tcmStageEntity = await _converterHelper.ToTCMStageEntity(tcmStageViewModel, true, user_logged.UserName);
                    tcmStageEntity.ID_Etapa = tcmservice.Stages.Count() + 1;
                    _context.Add(tcmStageEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMServiceEntity> tcmServices = await _context.TCMServices
                                                                           .Include(m => m.Stages)
                                                                           .OrderBy(f => f.Code)
                                                                           .ToListAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMServices", tcmServices) });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            //recovery data
            if (User.IsInRole("Admin"))
            {
                tcmStageViewModel.Clinics = _combosHelper.GetComboClinics();
            }
            else
            {
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = user_logged.Clinic.Name,
                    Value = $"{user_logged.Clinic.Id}"
                });

                tcmStageViewModel.Clinics = list;
            }

            
            tcmStageViewModel.tCMservice = tcmservice;
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateStage", tcmStageViewModel) });
        }

        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> EditStage(int id = 0)
        {
            
            TCMStageViewModel model;
            TCMStageEntity tcmStage = await _context.TCMStages
                                                .Include(g => g.tCMservice)
                                                .Include(g => g.Clinic)
                                                .FirstOrDefaultAsync(m => m.Id == id);
            if (tcmStage != null)
            {
                model = _converterHelper.ToTCMStageViewModel(tcmStage);
                return View(model);
            }
            return RedirectToAction("Index", new { idError = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> EditStage(TCMStageViewModel tcmStageViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            TCMStageEntity tcmStageEntity = await _converterHelper.ToTCMStageEntity(tcmStageViewModel, false, user_logged.UserName);

            if (User.IsInRole("Manager"))
            {
                if (ModelState.IsValid)
                {
                   
                    _context.Update(tcmStageEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        List<TCMServiceEntity> tcmService = await _context.TCMServices
                                                                           .Include(m => m.Stages)
                                                                           .OrderBy(f => f.Code)
                                                                           .ToListAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMServices", tcmService) });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            //recovery data
            if (User.IsInRole("Admin"))
            {
                tcmStageViewModel.Clinics = _combosHelper.GetComboClinics();
            }
            else
            {
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = user_logged.Clinic.Name,
                    Value = $"{user_logged.Clinic.Id}"
                });

                tcmStageViewModel.Clinics = list;
            }
            TCMServiceEntity tcmservice = await _context.TCMServices
                                               .Include(g => g.Stages)
                                               .FirstOrDefaultAsync(m => m.Id == tcmStageViewModel.Id_TCMService);
            tcmStageViewModel.tCMservice = tcmservice;
            
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditStage", tcmStageViewModel) });
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> ServiceActivity (int idError = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }
            List<TCMServiceEntity> tcmservices = await _context.TCMServices
                                                               .Include(m => m.TCMServiceActivity)
                                                               .OrderBy(f => f.Code)
                                                               .ToListAsync();

            return View(tcmservices);
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> CreateActivity(int id = 0)
        {
            TCMServiceActivityViewModel model;

            TCMServiceEntity tcmservice = await _context.TCMServices
                                                        .Include(g => g.TCMServiceActivity)
                                                        .FirstOrDefaultAsync(m => m.Id == id);

            if (User.IsInRole("Manager") || User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor"))
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
                    model = new TCMServiceActivityViewModel
                    {
                        TcmService = tcmservice,
                        Clinics = list,
                        IdClinic = clinic.Id,
                        IdService = id,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        Status = true
                    };

                    return View(model);
                }
            }

            model = new TCMServiceActivityViewModel
            {
                Clinics = _combosHelper.GetComboClinics(),
                IdService = id,
                TcmService = tcmservice
                
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> CreateActivity(TCMServiceActivityViewModel tcmServiceActivityViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMServiceEntity tcmservice = await _context.TCMServices
                                                        .Include(g => g.TCMServiceActivity)
                                                        .FirstOrDefaultAsync(m => m.Id == tcmServiceActivityViewModel.IdService);

            if (User.IsInRole("Manager") || User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor"))
            {
                if (ModelState.IsValid)
                {
                    TCMServiceActivityEntity tcmServiceActivityEntity = await _converterHelper.ToTCMServiceActivityEntity(tcmServiceActivityViewModel, true, user_logged.UserName);
                    
                    _context.Add(tcmServiceActivityEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMServiceEntity> tcmServices = await _context.TCMServices
                                                                           .Include(m => m.TCMSubServices)
                                                                           .ThenInclude(m => m.TCMSubServiceSteps)
                                                                           .OrderBy(f => f.Code)
                                                                           .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMSubServices", tcmServices) });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            //recovery data
            List<SelectListItem> list = new List<SelectListItem>();
            list.Insert(0, new SelectListItem
            {
                Text = user_logged.Clinic.Name,
                Value = $"{user_logged.Clinic.Id}"
            });

            tcmServiceActivityViewModel.Clinics = list;
            
            tcmServiceActivityViewModel.TcmService = tcmservice;
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateActivity", tcmServiceActivityViewModel) });
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> EditServiceActivity(int id = 0, int origin = 0)
        {

            TCMServiceActivityViewModel model;
            TCMServiceActivityEntity tcmServiceActivity = await _context.TCMServiceActivity
                                                                        .Include(g => g.TcmService)
                                                                        .ThenInclude(g => g.Clinic)
                                                                        .FirstOrDefaultAsync(m => m.Id == id);
            if (tcmServiceActivity != null)
            {
                model = _converterHelper.ToTCMServiceActivityViewModel(tcmServiceActivity);
                ViewData["origin"] = origin;
                return View(model);
            }

            return RedirectToAction("ServiceActivity", new { idError = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> EditServiceActivity(TCMServiceActivityViewModel tcmServiceActivityViewModel, int origin = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMServiceActivityEntity tcmServiceActivityEntity = await _converterHelper.ToTCMServiceActivityEntity(tcmServiceActivityViewModel, false, user_logged.UserName);

            if (User.IsInRole("Manager") || User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor"))
            {
                if (ModelState.IsValid)
                {

                    _context.Update(tcmServiceActivityEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        if (origin == 0)
                        {
                            List<TCMServiceEntity> tcmServices = await _context.TCMServices
                                                                               .Include(m => m.TCMSubServices)
                                                                               .ThenInclude(m => m.TCMSubServiceSteps)
                                                                               .OrderBy(f => f.Code)
                                                                               .ToListAsync();

                            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMSubServices", tcmServices) });
                        }
                        else
                        {
                            if (origin == 1)
                            {
                                List<TCMServiceActivityEntity> listActivity = await _context.TCMServiceActivity
                                                                                            .Include(n => n.TcmService)
                                                                                            .Where(n => n.Approved < 2)
                                                                                            .ToListAsync();

                                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMPendingActivity", listActivity) });

                            }
                            else
                            {
                                return RedirectToAction(nameof(Index));
                            }
                        }
                        
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            //recovery data
            List<SelectListItem> list = new List<SelectListItem>();
            list.Insert(0, new SelectListItem
            {
                Text = user_logged.Clinic.Name,
                Value = $"{user_logged.Clinic.Id}"
            });

            tcmServiceActivityViewModel.Clinics = list;
            
            TCMServiceEntity tcmservice = await _context.TCMServices
                                                        .Include(g => g.TCMServiceActivity)
                                                        .FirstOrDefaultAsync(m => m.Id == tcmServiceActivityViewModel.IdService);
            tcmServiceActivityViewModel.TcmService = tcmservice;

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditServiceActivity", tcmServiceActivityViewModel) });
        }

        [Authorize(Roles = "TCMSupervisor, Manager")]
        public async Task<IActionResult> ApprovedActivity(int id, int origin = 0)
        {
            TCMServiceActivityEntity activity = await _context.TCMServiceActivity
                                                              .Include(n => n.TcmService)        
                                                              .FirstOrDefaultAsync(n => n.Id == id);
            activity.Approved = 2;
            _context.Update(activity);

            await _context.SaveChangesAsync();
            if (origin == 1)
            {
                return RedirectToAction(nameof(ServiceActivity));
            }

            return RedirectToAction(nameof(PendingActivity));
        }
        
        [Authorize(Roles = "TCMSupervisor, Manager, CaseManager")]
        public async Task<IActionResult> PendingActivity()
        {
            UserEntity user_logged = _context.Users
                                            .Include(u => u.Clinic)
                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                List<TCMServiceActivityEntity> listActivity = await _context.TCMServiceActivity
                                                                            .Include(n => n.TcmService)
                                                                            .Where(n => n.Approved < 2)
                                                                            .ToListAsync();

                return View(listActivity);
            }
            else
            {
                List<TCMServiceActivityEntity> listActivity = await _context.TCMServiceActivity
                                                                            .Include(n => n.TcmService)
                                                                            .Where(n => n.Approved < 2
                                                                                && n.CreatedBy == user_logged.UserName)
                                                                            .ToListAsync();

                return View(listActivity);
            }
            
        }

        [Authorize(Roles = "TCMSupervisor, Manager")]
        public async Task<IActionResult> DeleteActivity(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMServiceActivityEntity tcmServiceActivityEntity = await _context.TCMServiceActivity.FirstOrDefaultAsync(s => s.Id == id);
            if (tcmServiceActivityEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMServiceActivity.Remove(tcmServiceActivityEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("PendingActivity", new { idError = 1 });
            }

            return RedirectToAction("ServiceActivity", new { idError = 0 });
        }

        [Authorize(Roles = "TCMSupervisor, Manager")]
        public async Task<IActionResult> TCMSubServices(int idError = 0)
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }
            List<TCMServiceEntity> tcmservices = await _context.TCMServices
                                                               .Include(m => m.TCMSubServices)
                                                               .ThenInclude(m => m.TCMSubServiceSteps)
                                                               .OrderBy(f => f.Code)
                                                               .ToListAsync();

            return View(tcmservices);
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> CreateSubService(int id = 0)
        {
            TCMSubServiceViewModel model;

            TCMServiceEntity tcmservice = await _context.TCMServices
                                                        .Include(g => g.TCMSubServices)
                                                        .FirstOrDefaultAsync(m => m.Id == id);

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic != null)
            {
                model = new TCMSubServiceViewModel
                {
                    TcmService = tcmservice,
                    Id_TCMService = id,
                    CreatedBy = user_logged.UserName,
                    CreatedOn = DateTime.Now,
                    
                };

                return View(model);
            }

            model = new TCMSubServiceViewModel
            {
               
                Id_TCMService = id,
                TcmService = tcmservice,
                
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> CreateSubService(TCMSubServiceViewModel tcmSubServiceViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMServiceEntity tcmservice = await _context.TCMServices
                                                        .Include(g => g.TCMSubServices)
                                                        .FirstOrDefaultAsync(m => m.Id == tcmSubServiceViewModel.Id_TCMService);
            if (ModelState.IsValid)
            {
                TCMSubServiceEntity tcmSubServiceEntity = await _converterHelper.ToTCMSubServiceEntity(tcmSubServiceViewModel, true, user_logged.UserName);
                _context.Add(tcmSubServiceEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMServiceEntity> tcmServices = await _context.TCMServices
                                                                       .Include(m => m.TCMSubServices)
                                                                       .ThenInclude(m => m.TCMSubServiceSteps)
                                                                       .OrderBy(f => f.Code)
                                                                       .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMSubServices", tcmServices) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            //recovery data

            tcmSubServiceViewModel.TcmService = tcmservice;
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateSubService", tcmSubServiceViewModel) });
        }

        [Authorize(Roles = "TCMSupervisor, Manager")]
        public async Task<IActionResult> EditSubService(int id = 0)
        {

            TCMSubServiceViewModel model;
            TCMSubServiceEntity tcmSubService = await _context.TCMSubServices
                                                              .Include(g => g.TcmService)
                                                              .FirstOrDefaultAsync(m => m.Id == id);
            if (tcmSubService != null)
            {
                model = _converterHelper.ToTCMSubServiceViewModel(tcmSubService);
                return View(model);
            }
            return RedirectToAction("Index", new { idError = 1 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor, Manager")]
        public async Task<IActionResult> EditSubService(TCMSubServiceViewModel tcmSubServiceViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMSubServiceEntity tcmSubServiceEntity = await _converterHelper.ToTCMSubServiceEntity(tcmSubServiceViewModel, false, user_logged.UserName);

            if (ModelState.IsValid)
            {

                _context.Update(tcmSubServiceEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMServiceEntity> tcmServices = await _context.TCMServices
                                                                        .Include(m => m.TCMSubServices)
                                                                        .ThenInclude(m => m.TCMSubServiceSteps)
                                                                        .OrderBy(f => f.Code)
                                                                        .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMSubServices", tcmServices) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            //recovery data
           
            TCMServiceEntity tcmservice = await _context.TCMServices
                                                        .Include(g => g.TCMSubServices)
                                                        .FirstOrDefaultAsync(m => m.Id == tcmSubServiceViewModel.Id_TCMService);
            tcmSubServiceViewModel.TcmService = tcmservice;

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditSubService", tcmSubServiceViewModel) });
        }

        public JsonResult GetListSubServices(int idService)
        {
            List<TCMSubServiceEntity> subServices = _context.TCMSubServices
                                                            .Where(o => o.TcmService.Id == idService
                                                                     && o.Active == true)
                                                            .ToList();
            if (subServices.Count == 0)
            {
                subServices.Insert(0, new TCMSubServiceEntity
                {
                    Name = "[First select Service...]",
                    Id = 0
                });
            }
            return Json(new SelectList(subServices, "Id", "Name"));
        }

        public JsonResult GetSubService(int idDomain)
        {
            TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(o => o.Id == idDomain);
            string text = "Select Domain";
            if (domain != null)
            {
                text = domain.NameSubService;
            }
            return Json(text);
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public IActionResult CopyActivity(int idServiceTo = 0)
        {
            TCMCopyActivityViewModel model;

            model = new TCMCopyActivityViewModel
            {
                IdServiceFrom = 0,
                IdServiceTo = idServiceTo,
                Services = _combosHelper.GetComboTCMServices(),
                TCMService = _context.TCMServices.FirstOrDefault(n => n.Id == idServiceTo)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> CopyActivity(TCMCopyActivityViewModel tcmCopyActivityViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMServiceEntity tcmServiceFrom = await _context.TCMServices
                                                        .Include(g => g.TCMServiceActivity)
                                                        .FirstOrDefaultAsync(m => m.Id == tcmCopyActivityViewModel.IdServiceFrom);
            TCMServiceEntity tcmServiceTo = _context.TCMServices.Find(tcmCopyActivityViewModel.IdServiceTo);
            List<TCMServiceActivityEntity> list = new List<TCMServiceActivityEntity>();
            if (ModelState.IsValid)
            {
                foreach (var item in tcmServiceFrom.TCMServiceActivity)
                {
                    item.Id = 0;
                    item.TcmService = tcmServiceTo;
                    list.Add(item);
                }
                _context.AddRange(list);

                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMServiceEntity> tcmServices = await _context.TCMServices
                                                                       .Include(m => m.TCMServiceActivity)
                                                                       .OrderBy(f => f.Code)
                                                                       .ToListAsync();
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMServiceActivity", tcmServices) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            //recovery data

            tcmCopyActivityViewModel.Services = _combosHelper.GetComboTCMServices();
            tcmCopyActivityViewModel.TCMService = tcmServiceTo;

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CopyActivity", tcmCopyActivityViewModel) });
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public IActionResult DuplicateService(int idServiceFrom = 0)
        {
            TCMCopyActivityViewModel model;

            model = new TCMCopyActivityViewModel
            {
                IdServiceFrom = idServiceFrom,
                IdServiceTo = 0,
                Services = _combosHelper.GetComboTCMServices(),
                TCMService = _context.TCMServices.FirstOrDefault(n => n.Id == idServiceFrom)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> DuplicateService(TCMCopyActivityViewModel tcmCopyActivityViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMServiceEntity tcmServiceFrom = await _context.TCMServices
                                                            .Include(g => g.Stages)
                                                            .Include(g => g.TCMServiceActivity)
                                                            .Include(g => g.TCMSubServices)
                                                            .Include(g => g.Clinic)
                                                            .FirstOrDefaultAsync(m => m.Id == tcmCopyActivityViewModel.IdServiceFrom);
            TCMServiceEntity tcmNewService = new TCMServiceEntity()
            { 
                Name = tcmServiceFrom.Name,
                Clinic = tcmServiceFrom.Clinic,
                CreatedBy = user_logged.UserName,
                CreatedOn = tcmServiceFrom.CreatedOn,
                Description = tcmServiceFrom.Description                
            };

            tcmNewService.Stages = new List<TCMStageEntity>();
            tcmNewService.TCMSubServices = new List<TCMSubServiceEntity>();
            tcmNewService.TCMServiceActivity = new List<TCMServiceActivityEntity>();
            string[] aux = tcmServiceFrom.Code.Split('.');
            int mount = await _context.TCMServices.Where(n => n.Code.Contains(aux[0]) == true).CountAsync();
            tcmNewService.Code = aux[0] + '.' + mount;
            if (ModelState.IsValid)
            {
                if (tcmCopyActivityViewModel.copyAllObjective == true)
                {
                    List<TCMStageEntity> tempList = new List<TCMStageEntity>();
                    foreach (var item in tcmServiceFrom.Stages)
                    {
                        TCMStageEntity temp = new TCMStageEntity();
                        temp.Clinic = item.Clinic;
                        temp.CreatedBy = item.CreatedBy;
                        temp.CreatedOn = item.CreatedOn;
                        temp.Description = item.Description;
                        temp.ID_Etapa = item.ID_Etapa;
                        temp.Name = item.Name;
                        temp.tCMservice = tcmNewService;
                        tempList.Add(temp);
                                                
                    }
                    tcmNewService.Stages = tempList;
                }

                if (tcmCopyActivityViewModel.copyAllActivity == true)
                {
                   
                    foreach (var item in tcmServiceFrom.TCMServiceActivity)
                    {
                        TCMServiceActivityEntity temp = new TCMServiceActivityEntity();
                        temp.Approved = 1;
                        temp.CreatedBy = item.CreatedBy;
                        temp.CreatedOn = item.CreatedOn;
                        temp.Description = item.Description;
                        temp.Frecuency = item.Frecuency;
                        temp.Name = item.Name;
                        temp.Status = item.Status;
                        temp.Unit = item.Unit;
                        temp.TcmService = tcmNewService;
                        tcmNewService.TCMServiceActivity.Add(temp);
                        temp = new TCMServiceActivityEntity();
                    }
                }

                if (tcmCopyActivityViewModel.copyAllSubService == true)
                {
                    
                    foreach (var item in tcmServiceFrom.TCMSubServices)
                    {
                        TCMSubServiceEntity temp1 = new TCMSubServiceEntity();
                        temp1.Active = item.Active;
                        temp1.CreatedBy = item.CreatedBy;
                        temp1.CreatedOn = item.CreatedOn;
                        temp1.Description = item.Description;
                        temp1.Name = item.Name;
                        temp1.TcmService = tcmNewService;
                        tcmNewService.TCMSubServices.Add(temp1);
                        //temp1 = new TCMSubServiceEntity();
                    }
                }

                _context.Add(tcmNewService);

                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMServiceEntity> tcmService = await _context.TCMServices
                                                                      .Include(m => m.Stages)
                                                                      .OrderBy(f => f.Code)
                                                                      .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMServices", tcmService) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            //recovery data

            tcmCopyActivityViewModel.Services = _combosHelper.GetComboTCMServices();
            tcmCopyActivityViewModel.TCMService = tcmServiceFrom;

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "DuplicateService", tcmCopyActivityViewModel) });
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> CreateSubServiceStep(int id = 0)
        {
            TCMSubServiceStepViewModel model;

            TCMSubServiceEntity tcmSubservice = await _context.TCMSubServices
                                                              .Include(g => g.TcmService)
                                                              .Include(g => g.TCMSubServiceSteps)
                                                              .FirstOrDefaultAsync(m => m.Id == id);

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic != null)
            {
                model = new TCMSubServiceStepViewModel
                {
                    TcmSubService = tcmSubservice,
                    Id_TCMService = tcmSubservice.Id,
                    Id_TCMSubService = id,
                    CreatedBy = user_logged.UserName,
                    CreatedOn = DateTime.Now,
                    Orden = tcmSubservice.TCMSubServiceSteps.Count() + 1,
                    Active = true,
                    Units = 1

                };

                return View(model);
            }

            model = new TCMSubServiceStepViewModel
            {

                TcmSubService = tcmSubservice,
                Id_TCMService = tcmSubservice.Id,
                Id_TCMSubService = id,
                CreatedBy = user_logged.UserName,
                CreatedOn = DateTime.Now,
                Orden = tcmSubservice.TCMSubServiceSteps.Count() + 1,
                Active = true,
                Units = 1

            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> CreateSubServiceStep(TCMSubServiceStepViewModel model)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMSubServiceEntity tcmSubService = await _context.TCMSubServices
                                                              .Include(g => g.TcmService)
                                                              .FirstOrDefaultAsync(m => m.Id == model.Id_TCMSubService);
            if (ModelState.IsValid)
            {
                TCMSubServiceStepEntity tcmSubServiceStepEntity = await _converterHelper.ToTCMSubServiceStepEntity(model, true, user_logged.UserName);
                _context.Add(tcmSubServiceStepEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMServiceEntity> tcmServices = await _context.TCMServices
                                                                       .Include(m => m.TCMSubServices)
                                                                       .ThenInclude(m => m.TCMSubServiceSteps)
                                                                       .OrderBy(f => f.Code)
                                                                       .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMSubServices", tcmServices) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            //recovery data

            model.TcmSubService = tcmSubService;
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateSubServiceStep", model) });
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> EditSubServiceStep(int id = 0)
        {
            TCMSubServiceStepEntity tcmSubservicestep = await _context.TCMSubServiceSteps
                                                                      .Include(g => g.TcmSubService)
                                                                      .ThenInclude(g => g.TcmService)
                                                                      .FirstOrDefaultAsync(m => m.Id == id);

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (tcmSubservicestep != null)
            {
                TCMSubServiceStepViewModel model = _converterHelper.ToTCMSubServiceStepViewModel(tcmSubservicestep);

                return View(model);
            }
           
            return View(null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> EditSubServiceStep(TCMSubServiceStepViewModel model)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMSubServiceEntity tcmSubService = await _context.TCMSubServices
                                                              .Include(g => g.TcmService)
                                                              .FirstOrDefaultAsync(m => m.Id == model.Id_TCMSubService);
            if (ModelState.IsValid)
            {
                TCMSubServiceStepEntity tcmSubServiceStepEntity = await _converterHelper.ToTCMSubServiceStepEntity(model, false, user_logged.UserName);
                _context.Update(tcmSubServiceStepEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMServiceEntity> tcmServices = await _context.TCMServices
                                                                       .Include(m => m.TCMSubServices)
                                                                       .ThenInclude(m => m.TCMSubServiceSteps)
                                                                       .OrderBy(f => f.Code)
                                                                       .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMSubServices", tcmServices) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            //recovery data

            model.TcmSubService = tcmSubService;
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditSubServiceStep", model) });
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public IActionResult DeleteStep(int id = 0)
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
        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> DeleteStep(DeleteViewModel model)
        {
            TCMSubServiceStepEntity entity = await _context.TCMSubServiceSteps
                                                           .FirstOrDefaultAsync(n => n.Id == model.Id_Element);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.TCMSubServiceSteps.Remove(entity);
                    await _context.SaveChangesAsync();

                    List<TCMServiceEntity> tcmServices = await _context.TCMServices
                                                                       .Include(m => m.TCMSubServices)
                                                                       .ThenInclude(m => m.TCMSubServiceSteps)
                                                                       .OrderBy(f => f.Code)
                                                                       .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMSubServices", tcmServices) });
                }
                catch (Exception)
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMSubServices", _context.TCMSubServiceSteps.Include(n => n.TcmSubService).ThenInclude(m => m.TCMSubServiceSteps).ToList()) });
                }


            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMSubServices", _context.TCMSubServiceSteps.Include(n => n.TcmSubService).ThenInclude(m => m.TCMSubServiceSteps).ToList()) });
        }

    }
}
