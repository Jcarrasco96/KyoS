using Microsoft.AspNetCore.Mvc;
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
                            List<TCMServiceEntity> tcmService = await _context.TCMServices
                                                                               .Include(m => m.TCMServiceActivity)
                                                                               .OrderBy(f => f.Code)
                                                                               .ToListAsync();
                            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMServiceActivity", tcmService) });
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

            return RedirectToAction(nameof(PendingActivity));
        }
    }
}
