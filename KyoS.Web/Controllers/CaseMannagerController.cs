using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using KyoS.Web.Migrations;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin, Manager, TCMSupervisor")]
    public class CaseMannagerController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IRenderHelper _renderHelper;

        public CaseMannagerController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
            _renderHelper = renderHelper;
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> Index(int idError = 0)
        {
            UserEntity user_logged = await _context.Users

                                                  .Include(u => u.Clinic)
                                                  .ThenInclude(c => c.Setting)

                                                  .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("Manager"))
            {
                
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                if (idError == 1) //Imposible to delete
                {
                    ViewBag.Delete = "N";
                }

                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                if (clinic != null)
                    return View(await _context.CaseManagers
                                              .Include(f => f.Clinic)
                                              .Include(f => f.TCMSupervisor)
                                              .Include(f => f.TCMClients)
                                              .ThenInclude(f => f.Client)
                                              .Where(f => f.Clinic.Id == clinic.Id).OrderBy(f => f.Name).ToListAsync());
               
            }
            else
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    TCMSupervisorEntity supervisorEntity = _context.TCMSupervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

                    if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }

                    if (idError == 1) //Imposible to delete
                    {
                        ViewBag.Delete = "N";
                    }

                    return View(await _context.CaseManagers
                                              .Include(f => f.Clinic)
                                              .Include(f => f.TCMSupervisor)
                                              .Include(f => f.TCMClients)
                                              .ThenInclude(f => f.Client)
                                              .Where(f => f.Clinic.Id == user_logged.Clinic.Id
                                                       && f.TCMSupervisor.Id == supervisorEntity.Id)
                                              .OrderBy(f => f.Name).ToListAsync());
                   
                }
            }
            return null;
        }

        [Authorize(Roles = "Manager")]
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

            CaseMannagerViewModel model;

            if (User.IsInRole("Manager"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
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
                    model = new CaseMannagerViewModel
                    {
                        Clinics = list,
                        IdClinic = clinic.Id,
                        StatusList = _combosHelper.GetComboClientStatus(),
                        UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.CaseManager, user_logged.Clinic.Id),
                        Money = 0,
                        IdTCMsupervisor = 0,
                        TCMsupervisors = _combosHelper.GetComboTCMSupervisorByClinic(user_logged.Clinic.Id),
                        IdGender = 0,
                        GenderList = _combosHelper.GetComboGender(),
                        IdAccountType = 0,
                        AccountTypeList = _combosHelper.GetComboAccountType()

                    };
                    return View(model);
                }
            }

            model = new CaseMannagerViewModel
            {
                Clinics = _combosHelper.GetComboClinics(),
                IdStatus = 1,
                StatusList = _combosHelper.GetComboClientStatus(),
                UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.CaseManager, 0),
                Money = 0,
                IdTCMsupervisor = 0,
                TCMsupervisors = _combosHelper.GetComboTCMSupervisorByClinic(0)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Create(CaseMannagerViewModel caseMannagerViewModel)
        {
            if (ModelState.IsValid)
            {
                CaseMannagerEntity casemannager = await _context.CaseManagers.FirstOrDefaultAsync(f => f.Name == caseMannagerViewModel.Name);
                if (casemannager == null)
                {
                    if (caseMannagerViewModel.IdUser == "0")
                    {
                        ModelState.AddModelError(string.Empty, "You must select a linked user");
                        return View(caseMannagerViewModel);
                    }

                    string path = string.Empty;
                    if (caseMannagerViewModel.SignatureFile != null)
                    {
                        path = await _imageHelper.UploadImageAsync(caseMannagerViewModel.SignatureFile, "Signatures");
                    }
                    CaseMannagerEntity caseMannagerEntity = await _converterHelper.ToCaseMannagerEntity(caseMannagerViewModel, path, true);

                    _context.Add(caseMannagerEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the Case Mannager: {caseMannagerEntity.Name}");
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
            return View(caseMannagerViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            CaseMannagerEntity caseMannagerEntity = await _context.CaseManagers.FirstOrDefaultAsync(t => t.Id == id);
            if (caseMannagerEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.CaseManagers.Remove(caseMannagerEntity);
                await _context.SaveChangesAsync();
            }
            catch (System.Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            CaseMannagerEntity caseMannagerEntity = await _context.CaseManagers
                                                                  .Include(f => f.Clinic)
                                                                  .Include(f => f.TCMSupervisor)
                                                                  .FirstOrDefaultAsync(f => f.Id == id);
            if (caseMannagerEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            CaseMannagerViewModel caseMannagerViewModel;
            if (User.IsInRole("Manager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                caseMannagerViewModel = _converterHelper.ToCaseMannagerViewModel(caseMannagerEntity, user_logged.Clinic.Id);
                if (user_logged.Clinic != null)
                {
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = user_logged.Clinic.Name,
                        Value = $"{user_logged.Clinic.Id}"
                    });
                    caseMannagerViewModel.Clinics = list;
                }

            }
            else
                caseMannagerViewModel = _converterHelper.ToCaseMannagerViewModel(caseMannagerEntity, 0);

            return View(caseMannagerViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id, CaseMannagerViewModel caseMannagerViewModel)
        {
            if (id != caseMannagerViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                string path = caseMannagerViewModel.SignaturePath;
                if (caseMannagerViewModel.SignatureFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(caseMannagerViewModel.SignatureFile, "Signatures");
                }
                CaseMannagerEntity caseMannagerEntity = await _converterHelper.ToCaseMannagerEntity(caseMannagerViewModel, path, false);
                _context.Update(caseMannagerEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the Case Mannager: {caseMannagerEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(caseMannagerViewModel);
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> CasesForCaseManager(StatusType status = StatusType.Open ,int idCaseManager = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<TCMClientEntity> tcmClientList = await _context.TCMClient
                                                                .Include(n => n.Casemanager)
                                                                .Include(n => n.Client)
                                                                .Include(n => n.TCMNote)
                                                                .ThenInclude(n => n.TCMNoteActivity)
                                                                .ThenInclude(n => n.TCMDomain)
                                                                .Where(n => n.Casemanager.Id == idCaseManager
                                                                    && n.Status == status)
                                                                .ToListAsync();

            return View(tcmClientList);

        }

        [Authorize(Roles = "Manager")]
        public IActionResult CreateModal(int id = 0)
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

            CaseMannagerViewModel model = new CaseMannagerViewModel();

            if (User.IsInRole("Manager"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
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
                    model = new CaseMannagerViewModel
                    {
                        Clinics = list,
                        IdClinic = clinic.Id,
                        StatusList = _combosHelper.GetComboClientStatus(),
                        UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.CaseManager, user_logged.Clinic.Id),
                        Money = 0,
                        IdTCMsupervisor = 0,
                        TCMsupervisors = _combosHelper.GetComboTCMSupervisorByClinic(user_logged.Clinic.Id),
                        RaterEducation = string.Empty,
                        RaterFMHCertification = string.Empty,
                        IdGender = 0,
                        GenderList = _combosHelper.GetComboGender(),
                        IdAccountType = 0,
                        AccountTypeList = _combosHelper.GetComboAccountType(),
                        IdPaymentMethod = 0,
                        PaymentMethodList = _combosHelper.GetComboPaymentMethod(),
                        Name = "-"
                    };
                    return View(model);
                }
            }

           
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(CaseMannagerViewModel caseMannagerViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                CaseMannagerEntity casemannager = await _context.CaseManagers.FirstOrDefaultAsync(f => f.Name == caseMannagerViewModel.Name);
                if (casemannager == null)
                {
                    if (caseMannagerViewModel.IdUser == "0")
                    {
                        caseMannagerViewModel.StatusList = _combosHelper.GetComboClientStatus();
                        caseMannagerViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.CaseManager, user_logged.Clinic.Id);
                        caseMannagerViewModel.TCMsupervisors = _combosHelper.GetComboTCMSupervisorByClinic(user_logged.Clinic.Id);
                        caseMannagerViewModel.GenderList = _combosHelper.GetComboGender();
                        caseMannagerViewModel.PaymentMethodList = _combosHelper.GetComboPaymentMethod();
                        caseMannagerViewModel.AccountTypeList = _combosHelper.GetComboAccountType();
                        caseMannagerViewModel.Clinics = _combosHelper.GetComboClinics();
                        ModelState.AddModelError(string.Empty, "You must select a linked user");

                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", caseMannagerViewModel) });
                    }

                    string path = string.Empty;
                    if (caseMannagerViewModel.SignatureFile != null)
                    {
                        path = await _imageHelper.UploadImageAsync(caseMannagerViewModel.SignatureFile, "Signatures");
                    }
                    CaseMannagerEntity caseMannagerEntity = await _converterHelper.ToCaseMannagerEntity(caseMannagerViewModel, path, true);

                    _context.Add(caseMannagerEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        
                        List<CaseMannagerEntity> caseManager_List = await _context.CaseManagers

                                                                                  .Include(n => n.Clinic)
                                                                                  .Include(n => n.TCMSupervisor)
                                                                                  .Include(n => n.TCMClients)
                                                                                  .ThenInclude(n => n.Client)

                                                                                  .Where(cm => cm.Clinic.Id == user_logged.Clinic.Id)
                                                                                  .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewCaseManagers", caseManager_List) });                        
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the Case Mannager: {caseMannagerEntity.Name}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else
                {
                    caseMannagerViewModel.StatusList = _combosHelper.GetComboClientStatus();
                    caseMannagerViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.CaseManager, user_logged.Clinic.Id);
                    caseMannagerViewModel.TCMsupervisors = _combosHelper.GetComboTCMSupervisorByClinic(user_logged.Clinic.Id);
                    caseMannagerViewModel.GenderList = _combosHelper.GetComboGender();
                    caseMannagerViewModel.PaymentMethodList = _combosHelper.GetComboPaymentMethod();
                    caseMannagerViewModel.AccountTypeList = _combosHelper.GetComboAccountType();
                    caseMannagerViewModel.Clinics = _combosHelper.GetComboClinics();
                    ModelState.AddModelError(string.Empty, "You must select a linked user");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", caseMannagerViewModel) });
                }
            }

            caseMannagerViewModel.StatusList = _combosHelper.GetComboClientStatus();
            caseMannagerViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.CaseManager, user_logged.Clinic.Id);
            caseMannagerViewModel.TCMsupervisors = _combosHelper.GetComboTCMSupervisorByClinic(user_logged.Clinic.Id);
            caseMannagerViewModel.GenderList = _combosHelper.GetComboGender();
            caseMannagerViewModel.PaymentMethodList = _combosHelper.GetComboPaymentMethod();
            caseMannagerViewModel.AccountTypeList = _combosHelper.GetComboAccountType();
            caseMannagerViewModel.Clinics = _combosHelper.GetComboClinics();
            if (caseMannagerViewModel.IdUser == "0")
            {
                ModelState.AddModelError(string.Empty, "You must select a linked user");
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", caseMannagerViewModel) });
        }

        public async Task<IActionResult> EditModal(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            CaseMannagerEntity caseMannagerEntity = await _context.CaseManagers
                                                                  .Include(f => f.Clinic)
                                                                  .Include(f => f.TCMSupervisor)
                                                                  .FirstOrDefaultAsync(f => f.Id == id);
            if (caseMannagerEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            CaseMannagerViewModel caseMannagerViewModel;
            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                caseMannagerViewModel = _converterHelper.ToCaseMannagerViewModel(caseMannagerEntity, user_logged.Clinic.Id);
                if (user_logged.Clinic != null)
                {
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = user_logged.Clinic.Name,
                        Value = $"{user_logged.Clinic.Id}"
                    });
                    caseMannagerViewModel.Clinics = list;
                }

            }
            else
                caseMannagerViewModel = _converterHelper.ToCaseMannagerViewModel(caseMannagerEntity, 0);

            return View(caseMannagerViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModal(int id, CaseMannagerViewModel caseMannagerViewModel)
        {
            UserEntity user_logged = _context.Users
                                            .Include(u => u.Clinic)
                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (id != caseMannagerViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                if (caseMannagerViewModel.IdUser == "0")
                {
                    caseMannagerViewModel.StatusList = _combosHelper.GetComboClientStatus();
                    caseMannagerViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.CaseManager, user_logged.Clinic.Id);
                    caseMannagerViewModel.TCMsupervisors = _combosHelper.GetComboTCMSupervisorByClinic(user_logged.Clinic.Id);
                    ModelState.AddModelError(string.Empty, "You must select a linked user");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditModal", caseMannagerViewModel) });
                }

                string path = caseMannagerViewModel.SignaturePath;
                if (caseMannagerViewModel.SignatureFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(caseMannagerViewModel.SignatureFile, "Signatures");
                }
                CaseMannagerEntity caseMannagerEntity = await _converterHelper.ToCaseMannagerEntity(caseMannagerViewModel, path, false);
                _context.Update(caseMannagerEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    List<CaseMannagerEntity> caseManager_List = await _context.CaseManagers

                                                                              .Include(n => n.Clinic)
                                                                              .Include(n => n.TCMSupervisor)
                                                                              .Include(n => n.TCMClients)
                                                                              .ThenInclude(n => n.Client)

                                                                              .Where(cm => cm.Clinic.Id == user_logged.Clinic.Id)
                                                                              .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewCaseManagers", caseManager_List) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the Case Manager: {caseMannagerEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            caseMannagerViewModel.StatusList = _combosHelper.GetComboClientStatus();
            caseMannagerViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.CaseManager, user_logged.Clinic.Id);
            caseMannagerViewModel.TCMsupervisors = _combosHelper.GetComboTCMSupervisorByClinic(user_logged.Clinic.Id);
            if (caseMannagerViewModel.IdUser == "0")
            {
                ModelState.AddModelError(string.Empty, "You must select a linked user");                
            }            

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditModal", caseMannagerViewModel) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int id, TCMSupervisorViewModel tcmSupervisorViewModel)
        {
            if (id != tcmSupervisorViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                if (tcmSupervisorViewModel.IdUser == "0")
                {
                    if (User.IsInRole("Admin"))
                    {
                        tcmSupervisorViewModel.Clinics = _combosHelper.GetComboClinics();
                        tcmSupervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMSupervisor, 0);
                        tcmSupervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();
                    }
                    else
                    {
                        List<SelectListItem> list = new List<SelectListItem>();
                        list.Insert(0, new SelectListItem
                        {
                            Text = user_logged.Clinic.Name,
                            Value = $"{user_logged.Clinic.Id}"
                        });
                        tcmSupervisorViewModel.Clinics = list;
                        tcmSupervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMSupervisor, user_logged.Clinic.Id);
                        tcmSupervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();
                    }

                    ModelState.AddModelError(string.Empty, "You must select a linked user");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", tcmSupervisorViewModel) });
                }

                string path = tcmSupervisorViewModel.SignaturePath;
                if (tcmSupervisorViewModel.SignatureFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(tcmSupervisorViewModel.SignatureFile, "Signatures");
                }
                TCMSupervisorEntity tcmSupervisorEntity = await _converterHelper.ToTCMsupervisorEntity(tcmSupervisorViewModel, path, false, user_logged.UserName);
                _context.Update(tcmSupervisorEntity);

                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMSupervisorEntity> tcmSupervisor = await _context.TCMSupervisors

                                                                            .Include(s => s.Clinic)

                                                                            .OrderBy(f => f.Name)
                                                                            .ToListAsync();
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMSupervisors", tcmSupervisor) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the TCM supervisor: {tcmSupervisorEntity.Name}");
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
                tcmSupervisorViewModel.Clinics = _combosHelper.GetComboClinics();
                tcmSupervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();
                tcmSupervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMSupervisor, 0);
            }
            else
            {
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = user_logged.Clinic.Name,
                    Value = $"{user_logged.Clinic.Id}"
                });

                tcmSupervisorViewModel.Clinics = list;
                tcmSupervisorViewModel.StatusList = _combosHelper.GetComboClientStatus();
                tcmSupervisorViewModel.UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.TCMSupervisor, user_logged.Clinic.Id);
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", tcmSupervisorViewModel) });
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<IActionResult> Signatures()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }


            return View(await _context.CaseManagers

                                      .Include(c => c.Clinic)
                                      .Where(c => c.Clinic.Id == user_logged.Clinic.Id)
                                      .OrderBy(c => c.Name).ToListAsync());
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<IActionResult> EditSignature(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            CaseMannagerEntity casemanagerEntity = await _context.CaseManagers
                                                                 .FirstOrDefaultAsync(c => c.Id == id);

            if (casemanagerEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            CaseMannagerViewModel casemanagerViewModel = _converterHelper.ToCaseMannagerViewModel(casemanagerEntity, user_logged.Clinic.Id);

            return View(casemanagerViewModel);
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<JsonResult> SaveCaseManagerSignature(string id, string dataUrl)
        {
            string signPath = await _imageHelper.UploadSignatureAsync(dataUrl, "CaseMannager");

            CaseMannagerEntity casemanager = await _context.CaseManagers
                                                           .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(id));
            if (casemanager != null)
            {
                casemanager.SignaturePath = signPath;
                _context.Update(casemanager);
                await _context.SaveChangesAsync();
            }

            return Json(new { redirectToUrl = Url.Action("Signatures", "CaseMannager") });
        }


    }
}
