using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{

    public class TCMClientsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IReportHelper _reportHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IImageHelper _imageHelper;

        public TCMClientsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IReportHelper reportHelper, IRenderHelper renderHelper, IImageHelper imageHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
            _renderHelper = renderHelper;
            _imageHelper = imageHelper;
        }

        [Authorize(Roles = "Manager, CaseManager, TCMSupervisor")]
        public async Task<IActionResult> Index()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (user_logged.UserType.ToString() == "CaseManager")
            {
                CaseMannagerEntity caseManager = await _context.CaseManagers
                                                               .FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);

                return View(await _context.TCMClient
                                          .Include(g => g.Casemanager)
                                          .Include(g => g.Client)
                                          .Where(g => (g.Casemanager.Id == caseManager.Id))
                                          .OrderBy(g => g.Client.Name)
                                          .ToListAsync());
            }

            if (user_logged.UserType.ToString() == "Manager" )
            {
              
                List<TCMClientEntity> tcmClient = await _context.TCMClient
                                                                 .Include(g => g.Casemanager)
                                                                 .Include(g => g.Client)
                                                                 .Where(s => s.Client.Clinic.Id == user_logged.Clinic.Id)
                                                                 .OrderBy(g => g.Casemanager.Name)
                                                                 .ToListAsync();
                return View(tcmClient);
            }

            if (user_logged.UserType.ToString() == "TCMSupervisor")
            {

                List<TCMClientEntity> tcmClient = await _context.TCMClient
                                                                 .Include(g => g.Casemanager)
                                                                 .Include(g => g.Client)
                                                                 .Where(s => s.Client.Clinic.Id == user_logged.Clinic.Id
                                                                 && s.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName)
                                                                 .OrderBy(g => g.Casemanager.Name)
                                                                 .ToListAsync();
                return View(tcmClient);
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
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

            TCMClientViewModel model;

            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    model = new TCMClientViewModel
                    {
                        CaseMannagers = _combosHelper.GetComboCasemannagersByClinic(user_logged.Clinic.Id),
                        Clients = _combosHelper.GetComboClientsForTCMCaseNotOpen(user_logged.Clinic.Id),
                        IdStatus = 1,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        StatusList = _combosHelper.GetComboClientStatus(),
                        DataOpen = DateTime.Today.Date,
                        Period = 6,
                    };

                    return View(model);
                }
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [HttpPost]
        [Authorize(Roles = "Manager, TCMSupervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TCMClientViewModel model)
        {
            UserEntity user_logged = _context.Users
                                                .Include(u => u.Clinic)
                                                .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                if (ModelState.IsValid)
                {
                    model.Casemanager = _context.CaseManagers.FirstOrDefault(u => u.Id == model.IdCaseMannager);
                    model.Client = _context.Clients.FirstOrDefault(u => u.Id == model.IdClient);
                    TCMClientEntity tcmClient = await _context.TCMClient.FirstOrDefaultAsync(s => s.Client.Id == model.IdClient
                                                                                             && s.Status == StatusType.Open);
                    if (tcmClient == null)
                    {
                        model.DataClose = model.DataOpen.AddMonths(model.Period);
                        tcmClient = _converterHelper.ToTCMClientEntity(model, true, user_logged.UserName);
                        _context.Add(tcmClient);
                        try
                        {
                            await _context.SaveChangesAsync();
                            List<TCMClientEntity> tcmClients = await _context.TCMClient
                                                             .Include(g => g.Casemanager)
                                                             .Include(g => g.Client)
                                                             .Where(s => s.Client.Clinic.Id == user_logged.Clinic.Id)
                                                             .OrderBy(g => g.Casemanager.Name)
                                                             .ToListAsync();
                            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMClient", tcmClients) });
                        }
                        catch (System.Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }

                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the TCM Case.");
                        model = new TCMClientViewModel
                        {
                            CaseMannagers = _combosHelper.GetComboCasemannagersByClinic(user_logged.Clinic.Id),
                            Clients = _combosHelper.GetComboClientsForTCMCaseNotOpen(user_logged.Clinic.Id),
                            IdStatus = 1,
                            StatusList = _combosHelper.GetComboClientStatus(),
                            DataOpen = DateTime.Today.Date,
                            Period = 6,
                        };
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
                    }

                }
                else
                {
                    model = new TCMClientViewModel
                    {
                        CaseMannagers = _combosHelper.GetComboCasemannagersByClinic(user_logged.Clinic.Id),
                        Clients = _combosHelper.GetComboClientsForTCMCaseNotOpen(user_logged.Clinic.Id),
                        IdStatus = 1,
                        StatusList = _combosHelper.GetComboClientStatus(),
                        DataOpen = DateTime.Today.Date,
                        Period = 6,
                    };
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
                }
            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            model.CaseMannagers = _combosHelper.GetComboCaseManager();

            MultiSelectList client_list = new MultiSelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["clients"] = client_list;

            return View(model);
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
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

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> Edit(int? id, int error = 0, int idFacilitator = 0, int idClient = 0, int origin = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMClientEntity tcmClientEntity = await _context.TCMClient
                                                            .Include(g => g.Casemanager)
                                                            .Include(g => g.Client)
                                                            .FirstOrDefaultAsync(g => g.Id == id);
            if (tcmClientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }            

            TCMClientViewModel tcmClientViewModel = _converterHelper.ToTCMClientViewModel(tcmClientEntity);

            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                List<SelectListItem> list = _context.Clients.Where(c => (c.Id == tcmClientViewModel.IdClient))
                                                            .Select(c => new SelectListItem
                                                            {
                                                                Text = $"{c.Name}",
                                                                Value = $"{c.Id}"
                                                            }).ToList();
                tcmClientViewModel.Clients = list;

                List<SelectListItem> list_Status = new List<SelectListItem>
                                { new SelectListItem { Text = StatusType.Open.ToString(), Value = "1"},
                                  new SelectListItem { Text = StatusType.Close.ToString(), Value = "2"}};

                tcmClientViewModel.StatusList = list_Status;

                List<SelectListItem> listCaseManager = _context.CaseManagers.Select(f => new SelectListItem
                {
                    Text = $"{f.Name}",
                    Value = $"{f.Id}"
                }).ToList();

                tcmClientViewModel.CaseMannagers = listCaseManager;
                ViewData["origin"] = origin;
                return View(tcmClientViewModel);
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [HttpPost]
        [Authorize(Roles = "Manager, TCMSupervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TCMClientViewModel model, int origin = 0)
        {
            UserEntity user_logged = _context.Users
                                           .Include(u => u.Clinic)
                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

            model.Casemanager = _context.CaseManagers.FirstOrDefault(u => u.Id == model.IdCaseMannager);
            model.Client = _context.Clients.FirstOrDefault(u => u.Id == model.IdClient);

            if (ModelState.IsValid)
            {
                if (model.Casemanager != null)
                {
                    if (model.Client != null)
                    {
                        if (model.IdStatus == 2)
                            model.DataClose = DateTime.Today.Date;
                        else
                            model.DataClose = model.DataOpen.AddMonths(model.Period);

                        TCMClientEntity tcmClient = _converterHelper.ToTCMClientEntity(model, false, user_logged.UserName);
                        _context.TCMClient.Update(tcmClient);
                        try
                        {
                            await _context.SaveChangesAsync();
                            if (origin == 0)
                            {
                                List<TCMClientEntity> tcmClients = await _context.TCMClient
                                                                                 .Include(g => g.Casemanager)
                                                                                 .Include(g => g.Client)
                                                                                 .Where(s => s.Client.Clinic.Id == user_logged.Clinic.Id)
                                                                                 .OrderBy(g => g.Casemanager.Name)
                                                                                 .ToListAsync();
                                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMClient", tcmClients) });

                            }
                            if (origin == 1)
                            {
                                List<TCMClientEntity> tcmClients = await _context.TCMClient
                                                                                 .Include(g => g.Casemanager)
                                                                                 .Include(g => g.Client)
                                                                                 .Where(s => s.Client.Clinic.Id == user_logged.Clinic.Id)
                                                                                 .OrderBy(g => g.Casemanager.Name)
                                                                                 .ToListAsync();
                                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "GetCaseOpen", tcmClients) });

                            }
                            if (origin == 2)
                            {
                                List<TCMClientEntity> tcmClients = await _context.TCMClient
                                                                                 .Include(g => g.Casemanager)
                                                                                 .Include(g => g.Client)
                                                                                 .Where(s => s.Client.Clinic.Id == user_logged.Clinic.Id
                                                                                     && s.Status == StatusType.Close)
                                                                                 .OrderBy(g => g.Casemanager.Name)
                                                                                 .ToListAsync();
                                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMClient", tcmClients) });

                            }

                        }
                        catch (System.Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                    ModelState.AddModelError(string.Empty, "Already not exists the TCM Client.");
                }
                ModelState.AddModelError(string.Empty, "Already not exists the Casemanager.");
            }

            List<SelectListItem> listClient = _context.Clients.Where(c => (c.Id == model.IdClient))
                                            .Select(c => new SelectListItem
                                            {
                                                Text = $"{c.Name}",
                                                Value = $"{c.Id}"
                                            }).ToList();
            List<SelectListItem> list_Status = new List<SelectListItem>
                                { new SelectListItem { Text = StatusType.Open.ToString(), Value = "1"},
                                  new SelectListItem { Text = StatusType.Close.ToString(), Value = "2"}};

            model.CaseMannagers = _combosHelper.GetComboCasemannagersByClinic(user_logged.Clinic.Id);
            model.Clients = listClient;
            model.StatusList = list_Status;


            if (model.IdClient == 0)
            {
                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", model) });
            }
            if (model.IdCaseMannager == 0)
            {
                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", model) });
            }
            if (model.IdStatus == 0)
            {
                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", model) });
            }

            return View(model);
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> TCMClientList()
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.UserType.ToString() == "CaseManager")
            {

                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
                List<TCMClientEntity> Client = await _context.TCMClient
                                                    .Include(g => g.Client)
                                                    .Where(g => (g.Casemanager.Id == caseManager.Id))
                                                    .ToListAsync();

                return View(Client);
            }

            return View(null);

        }

        [Authorize(Roles = "Manager, CaseManager, TCMSupervisor")]
        public async Task<IActionResult> GetCaseOpen()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.UserType.ToString() == "CaseManager")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
                List<TCMClientEntity> tcmClientsT = await _context.TCMClient
                                                                  .Include(g => g.Casemanager)
                                                                  .Include(g => g.Client)
                                                                  .Where(g => (g.Casemanager.Id == caseManager.Id
                                                                    && g.Status == StatusType.Open))
                                                                  .OrderBy(g => g.Client.Name)
                                                                  .ToListAsync();
                tcmClientsT = tcmClientsT.Where(wc => wc.TcmServicePlan == null).ToList();

                return View(tcmClientsT);
            }
            if (user_logged.UserType.ToString() == "TCMSupervisor")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                List<TCMClientEntity> tcmClientsT = await _context.TCMClient
                                                                  .Include(g => g.Casemanager)
                                                                  .Include(g => g.Client)
                                                                  .Where(g => (g.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                                                    && g.Status == StatusType.Open))
                                                                  .OrderBy(g => g.Client.Name)
                                                                  .ToListAsync();
                tcmClientsT = tcmClientsT.Where(wc => wc.TcmServicePlan == null).ToList();

                return View(tcmClientsT);
            }
            if (user_logged.UserType.ToString() == "Manager")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }
                List<TCMClientEntity> tcmClient = await _context.TCMClient
                                                          .Include(g => g.Casemanager)
                                                          .Include(g => g.Client)
                                                          .Where(s => (s.Client.Clinic.Id == user_logged.Clinic.Id
                                                              && s.Status == StatusType.Open))
                                                          .OrderBy(g => g.Casemanager.Name)
                                                          .ToListAsync();

                tcmClient = tcmClient.Where(wc => wc.TcmServicePlan == null).ToList();

                return View(tcmClient);

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, CaseManager, TCMSupervisor")]
        public async Task<IActionResult> GetCaseClose()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.UserType.ToString() == "CaseManager")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
                return View(await _context.TCMClient
                                          .Include(g => g.Casemanager)
                                          .Include(g => g.Client)
                                          .Where(g => (g.Casemanager.Id == caseManager.Id
                                                    && g.Status == StatusType.Close))
                                          .OrderBy(g => g.Client.Name)
                                          .ToListAsync());
            }
            if (user_logged.UserType.ToString() == "Manager")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }
                List<TCMClientEntity> tcmClient = await _context.TCMClient
                                                                .Include(g => g.Casemanager)
                                                                .Include(g => g.Client)
                                                                .Where(s => (s.Client.Clinic.Id == user_logged.Clinic.Id
                                                                          && s.Status == StatusType.Close))
                                                                .OrderBy(g => g.Casemanager.Name)
                                                                .ToListAsync();
                return View(tcmClient);

            }
            if (user_logged.UserType.ToString() == "TCMSupervisor")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }
                List<TCMClientEntity> tcmClient = await _context.TCMClient
                                                                .Include(g => g.Casemanager)
                                                                .Include(g => g.Client)
                                                                .Where(s => (s.Client.Clinic.Id == user_logged.Clinic.Id
                                                                          && s.Status == StatusType.Close
                                                                          && s.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName))
                                                                .OrderBy(g => g.Casemanager.Name)
                                                                .ToListAsync();
                return View(tcmClient);

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, CaseManager, TCMSupervisor")]
        public async Task<IActionResult> GetCaseNotServicePlan()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.UserType.ToString() == "CaseManager")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
                List<TCMClientEntity> tcmClientsT = await _context.TCMClient
                                                                  .Include(g => g.Casemanager)
                                                                  .Include(g => g.Client)
                                                                  .Include(g => g.TcmServicePlan)
                                                                  .Include(g => g.TcmIntakeAppendixJ)
                                                                  .Include(g => g.TCMAssessment)
                                                                  .Include(g => g.TcmIntakeAcknowledgementHipa)
                                                                  .Include(g => g.TCMIntakeAdvancedDirective)
                                                                  .Include(g => g.TcmIntakeConsentForRelease)
                                                                  .Include(g => g.TcmIntakeConsentForTreatment)
                                                                  .Include(g => g.TcmIntakeConsumerRights)
                                                                  .Include(g => g.TCMIntakeForeignLanguage)
                                                                  .Include(g => g.TCMIntakeForm)
                                                                  .Include(g => g.TCMIntakeOrientationChecklist)
                                                                  .Include(g => g.TCMIntakeWelcome)
                                                                  .Where(g => (g.Casemanager.Id == caseManager.Id
                                                                        && g.Status == StatusType.Open
                                                                        && (g.TcmServicePlan == null 
                                                                            || g.TCMAssessment == null
                                                                            || g.TcmIntakeAppendixJ == null
                                                                            || g.TcmIntakeAcknowledgementHipa == null
                                                                            || g.TCMIntakeAdvancedDirective == null
                                                                            || g.TcmIntakeConsentForRelease == null
                                                                            || g.TcmIntakeConsentForTreatment == null
                                                                            || g.TcmIntakeConsumerRights == null
                                                                            || g.TCMIntakeForeignLanguage == null
                                                                            || g.TCMIntakeForm == null
                                                                            || g.TCMIntakeOrientationChecklist == null
                                                                            || g.TCMIntakeWelcome == null)))
                                                                  .OrderBy(g => g.Client.Name)
                                                                  .ToListAsync();
               

                return View(tcmClientsT);
            }

            if (user_logged.UserType.ToString() == "TCMSupervisor")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                List<TCMClientEntity> tcmClientsT = await _context.TCMClient
                                                                  .Include(g => g.Casemanager)
                                                                  .Include(g => g.Client)
                                                                  .Include(g => g.TcmServicePlan)
                                                                  .Include(g => g.TcmIntakeAppendixJ)
                                                                  .Include(g => g.TCMAssessment)
                                                                  .Include(g => g.TcmIntakeAcknowledgementHipa)
                                                                  .Include(g => g.TCMIntakeAdvancedDirective)
                                                                  .Include(g => g.TcmIntakeConsentForRelease)
                                                                  .Include(g => g.TcmIntakeConsentForTreatment)
                                                                  .Include(g => g.TcmIntakeConsumerRights)
                                                                  .Include(g => g.TCMIntakeForeignLanguage)
                                                                  .Include(g => g.TCMIntakeForm)
                                                                  .Include(g => g.TCMIntakeOrientationChecklist)
                                                                  .Include(g => g.TCMIntakeWelcome)
                                                                  .Where(g => (g.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                                                        && g.Status == StatusType.Open
                                                                        && (g.TcmServicePlan == null
                                                                            
                                                                            || g.TCMAssessment == null
                                                                            || g.TcmIntakeAppendixJ == null
                                                                            || g.TcmIntakeAcknowledgementHipa == null
                                                                            || g.TCMIntakeAdvancedDirective == null
                                                                            || g.TcmIntakeConsentForRelease == null
                                                                            || g.TcmIntakeConsentForTreatment == null
                                                                            || g.TcmIntakeConsumerRights == null
                                                                            
                                                                            || g.TCMIntakeForeignLanguage == null
                                                                            || g.TCMIntakeForm == null
                                                                            || g.TCMIntakeOrientationChecklist == null
                                                                            || g.TCMIntakeWelcome == null)))
                                                                  .OrderBy(g => g.Client.Name)
                                                                  .ToListAsync();


                return View(tcmClientsT);
            }

            if (user_logged.UserType.ToString() == "Manager")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }
                List<TCMClientEntity> tcmClients = await _context.TCMClient
                                                                 .Include(g => g.Casemanager)
                                                                 .Include(g => g.Client)
                                                                 .Include(g => g.TcmServicePlan)
                                                                 .Include(g => g.TcmIntakeAppendixJ)
                                                                 .Include(g => g.TCMAssessment)
                                                                 .Include(g => g.TcmIntakeAcknowledgementHipa)
                                                                 .Include(g => g.TCMIntakeAdvancedDirective)
                                                                 .Include(g => g.TcmIntakeConsentForRelease)
                                                                 .Include(g => g.TcmIntakeConsentForTreatment)
                                                                 .Include(g => g.TcmIntakeConsumerRights)
                                                                 .Include(g => g.TCMIntakeForeignLanguage)
                                                                 .Include(g => g.TCMIntakeForm)
                                                                 .Include(g => g.TCMIntakeOrientationChecklist)
                                                                 .Include(g => g.TCMIntakeWelcome)
                                                                 .Where(s => (s.Client.Clinic.Id == user_logged.Clinic.Id
                                                                    && s.Status == StatusType.Open
                                                                    && (s.TcmServicePlan == null
                                                                        || s.TCMAssessment == null
                                                                        || s.TcmIntakeAppendixJ == null
                                                                        || s.TcmIntakeAcknowledgementHipa == null
                                                                        || s.TCMIntakeAdvancedDirective == null
                                                                        || s.TcmIntakeConsentForRelease == null
                                                                        || s.TcmIntakeConsentForTreatment == null
                                                                        || s.TcmIntakeConsumerRights == null
                                                                        || s.TCMIntakeForeignLanguage == null
                                                                        || s.TCMIntakeForm == null
                                                                        || s.TCMIntakeOrientationChecklist == null
                                                                        || s.TCMIntakeWelcome == null)))
                                                                .OrderBy(g => g.Casemanager.Name)
                                                                .ToListAsync();

                return View(tcmClients);

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, CaseManager, TCMSupervisor")]
        public async Task<IActionResult> AllDocuments()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.UserType.ToString() == "CaseManager")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || user_logged.Clinic.Setting.MentalHealthClinic == false)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
                
                List<TCMClientEntity> tcmClientList = await _context.TCMClient
                                                                    .Include(g => g.Casemanager)
                                                                    .Include(g => g.Client)
                                                                    .Include(g => g.TcmServicePlan)
                                                                    .Include(g => g.TcmServicePlan.TCMServicePlanReview)
                                                                    .Include(g => g.TcmServicePlan.TCMAdendum)
                                                                    .Include(g => g.TCMFarsFormList)
                                                                    .Include(g => g.TcmServicePlan.TCMDischarge)
                                                                    .Include(g => g.TcmIntakeAppendixJ)
                                                                    .Include(g => g.TCMAssessment)
                                                                    .Include(g => g.TCMIntakeForm)
                                                                    .Include(g => g.TcmIntakeConsentForTreatment)
                                                                    .Include(g => g.TcmIntakeConsentForRelease)
                                                                    .Include(g => g.TcmIntakeConsumerRights)
                                                                    .Include(g => g.TcmIntakeAcknowledgementHipa)
                                                                    .Include(g => g.TCMIntakeOrientationChecklist)
                                                                    .Include(g => g.TCMIntakeAdvancedDirective)
                                                                    .Include(g => g.TCMIntakeForeignLanguage)
                                                                    .Include(g => g.TCMIntakeWelcome)
                                                                    .Where(g => (g.Casemanager.Id == caseManager.Id))
                                                                    .OrderBy(g => g.Client.Name)
                                                                    .ToListAsync();

                List<TCMClientEntity> salida = new List<TCMClientEntity>();
                foreach (var item in tcmClientList)
                {
                    if (item.TcmServicePlan == null)
                        item.TcmServicePlan = new TCMServicePlanEntity();
                    if (item.TcmServicePlan.TCMAdendum == null)
                        item.TcmServicePlan.TCMAdendum = new List<TCMAdendumEntity>();
                    if (item.TcmServicePlan.TCMServicePlanReview == null)
                        item.TcmServicePlan.TCMServicePlanReview = new TCMServicePlanReviewEntity();
                    if (item.TcmServicePlan.TCMDischarge == null)
                        item.TcmServicePlan.TCMDischarge = new TCMDischargeEntity();
                    if (item.TCMFarsFormList == null)
                        item.TCMFarsFormList = new List<TCMFarsFormEntity>();
                    if (item.TCMAssessment == null)
                        item.TCMAssessment = new TCMAssessmentEntity();
                    if (item.TcmIntakeAppendixJ == null)
                        item.TcmIntakeAppendixJ = new TCMIntakeAppendixJEntity();
                    
                    salida.Add(item);
                }

                return View(salida);
            }
            if (user_logged.UserType.ToString() == "Manager" || user_logged.UserType.ToString() == "TCMSupervisor")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || user_logged.Clinic.Setting.MentalHealthClinic == false)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }
                List<TCMClientEntity> tcmClientList = await _context.TCMClient
                                                                    .Include(g => g.Casemanager)
                                                                    .Include(g => g.Client)
                                                                    .Include(g => g.TcmServicePlan)
                                                                    .Include(g => g.TcmServicePlan.TCMServicePlanReview)
                                                                    .Include(g => g.TcmServicePlan.TCMAdendum)
                                                                    .Include(g => g.TCMFarsFormList)
                                                                    .Include(g => g.TcmServicePlan.TCMDischarge)
                                                                    .Include(g => g.TcmIntakeAppendixJ)
                                                                    .Include(g => g.TCMAssessment)
                                                                    .Include(g => g.TCMIntakeForm)
                                                                    .Include(g => g.TcmIntakeConsentForTreatment)
                                                                    .Include(g => g.TcmIntakeConsentForRelease)
                                                                    .Include(g => g.TcmIntakeConsumerRights)
                                                                    .Include(g => g.TcmIntakeAcknowledgementHipa)
                                                                    .Include(g => g.TCMIntakeOrientationChecklist)
                                                                    .Include(g => g.TCMIntakeAdvancedDirective)
                                                                    .Include(g => g.TCMIntakeForeignLanguage)
                                                                    .Include(g => g.TCMIntakeWelcome)
                                                                    .Where(g => (g.Client.Clinic.Id == user_logged.Clinic.Id))
                                                                    .OrderBy(g => g.Client.Name)
                                                                    .ToListAsync();
                List<TCMClientEntity> salida = new List<TCMClientEntity>();
                foreach (var item in tcmClientList)
                {
                    if (item.TcmServicePlan == null)
                        item.TcmServicePlan = new TCMServicePlanEntity();
                    if (item.TcmServicePlan.TCMAdendum == null)
                        item.TcmServicePlan.TCMAdendum = new List<TCMAdendumEntity>();
                    if (item.TcmServicePlan.TCMServicePlanReview == null)
                        item.TcmServicePlan.TCMServicePlanReview = new TCMServicePlanReviewEntity();
                    if (item.TcmServicePlan.TCMDischarge == null)
                        item.TcmServicePlan.TCMDischarge = new TCMDischargeEntity();
                    if (item.TCMFarsFormList == null)
                        item.TCMFarsFormList = new List<TCMFarsFormEntity>();
                    if (item.TCMAssessment == null)
                        item.TCMAssessment = new TCMAssessmentEntity();
                    if (item.TcmIntakeAppendixJ == null)
                        item.TcmIntakeAppendixJ = new TCMIntakeAppendixJEntity();

                    salida.Add(item);
                }

                return View(salida);

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "CaseManager,TCMSupervisor")]
        public async Task<IActionResult> Clients(int idError = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            if (User.IsInRole("CaseManager"))
            {
                return View(await _context.TCMClient
                                          .Include(c => c.Client)
                                          .ThenInclude(c => c.Clinic)
                                          .Where(c => c.Casemanager.LinkedUser == user_logged.UserName)
                                          .OrderBy(c => c.Client.Name).ToListAsync());
            }
            if (User.IsInRole("TCMSupervisor"))
            {
                return View(await _context.TCMClient
                                          .Include(c => c.Client)
                                          .ThenInclude(c => c.Clinic)
                                          .Where(c => c.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName)
                                          .OrderBy(c => c.Client.Name).ToListAsync());
            }
            return View();
        }

        [Authorize(Roles = "CaseManager,TCMSupervisor")]
        public async Task<IActionResult> EditSignature(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ClientEntity clientEntity = await _context.Clients
                                                      .FirstOrDefaultAsync(c => c.Id == id);

            if (clientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            ClientViewModel clientViewModel = await _converterHelper.ToClientViewModel(clientEntity, user_logged.Id);

            return View(clientViewModel);
        }

        [Authorize(Roles = "CaseManager,TCMSupervisor")]
        public async Task<JsonResult> SaveClientSignature(string id, string dataUrl)
        {
            string signPath = await _imageHelper.UploadSignatureAsync(dataUrl, "Clients");

            ClientEntity client = await _context.Clients
                                                .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(id));
            if (client != null)
            {
                client.SignPath = signPath;
                _context.Update(client);
                await _context.SaveChangesAsync();
            }

            return Json(new { redirectToUrl = Url.Action("Clients", "TCMClients") });
        }

        [Authorize(Roles = "Manager, CaseManager, TCMSupervisor")]
        public async Task<IActionResult> TCMCaseHistory(int id = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor") || User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || user_logged.Clinic.Setting.MentalHealthClinic == false)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);

                TCMClientEntity tcmClient = await _context.TCMClient
                                                          .Include(g => g.Casemanager)
                                                          .ThenInclude(g => g.TCMSupervisor)
                                                          .Include(g => g.Client)
                                                          .Include(g => g.TcmServicePlan)
                                                          .Include(g => g.TcmServicePlan.TCMServicePlanReview)
                                                          .Include(g => g.TcmServicePlan.TCMAdendum)
                                                          .Include(g => g.TCMFarsFormList)
                                                          .Include(g => g.TcmServicePlan.TCMDischarge)
                                                          .Include(g => g.TcmIntakeAppendixJ)
                                                          .Include(g => g.TCMAssessment)
                                                          .Include(g => g.TCMNote)
                                                          .ThenInclude(g => g.TCMNoteActivity)
                                                          .FirstOrDefaultAsync(g => (g.Id == id));

                if (tcmClient != null)
                {
                    if (tcmClient.TcmServicePlan == null)
                        tcmClient.TcmServicePlan = new TCMServicePlanEntity();
                    if (tcmClient.TcmServicePlan.TCMAdendum == null)
                        tcmClient.TcmServicePlan.TCMAdendum = new List<TCMAdendumEntity>();
                    if (tcmClient.TcmServicePlan.TCMServicePlanReview == null)
                        tcmClient.TcmServicePlan.TCMServicePlanReview = new TCMServicePlanReviewEntity();
                    if (tcmClient.TcmServicePlan.TCMDischarge == null)
                        tcmClient.TcmServicePlan.TCMDischarge = new TCMDischargeEntity();
                    if (tcmClient.TCMFarsFormList == null)
                        tcmClient.TCMFarsFormList = new List<TCMFarsFormEntity>();
                    if (tcmClient.TCMAssessment == null)
                        tcmClient.TCMAssessment = new TCMAssessmentEntity();
                    if (tcmClient.TcmIntakeAppendixJ == null)
                        tcmClient.TcmIntakeAppendixJ = new TCMIntakeAppendixJEntity();

                }

                return View(tcmClient);
            }
          
            return RedirectToAction("NotAuthorized", "Account");
        }
    }
}
