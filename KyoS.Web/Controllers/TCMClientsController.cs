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
using System.IO;
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
                                          .Include(g => g.TCMReferralForm)
                                          .Where(g => (g.Casemanager.Id == caseManager.Id))
                                          .OrderBy(g => g.Client.Name)
                                          .ToListAsync());
            }

            if (user_logged.UserType.ToString() == "Manager" )
            {
              
                List<TCMClientEntity> tcmClient = await _context.TCMClient
                                                                 .Include(g => g.Casemanager)
                                                                 .Include(g => g.Client)
                                                                 .Include(g => g.TCMReferralForm)
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
                                                                 .Include(g => g.TCMReferralForm)
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

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMClientEntity tcmclient = await _context.TCMClient
                                                      .FirstOrDefaultAsync(s => s.Id == id);
            if (tcmclient == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMClient.Remove(tcmclient);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("Index", "TCMClients");
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
                                                                    .Include(g => g.TcmIntakeAppendixE)
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
                                                                    .Include(g => g.TcmIntakeAppendixE)
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
                                                          .Include(g => g.TcmIntakeAppendixE)
                                                          .FirstOrDefaultAsync(g => (g.Id == id));

                if (tcmClient != null)
                {
                    if (tcmClient.TCMFarsFormList == null)
                        tcmClient.TCMFarsFormList = new List<TCMFarsFormEntity>();
                   
                }

                return View(tcmClient);
            }
          
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, CaseManager, TCMSupervisor")]
        public async Task<IActionResult> TCMCaseActive(int warning = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<TCMClientEntity> tcmClient = new List<TCMClientEntity>();

            if (user_logged.UserType.ToString() == "CaseManager")
            {
                CaseMannagerEntity caseManager = await _context.CaseManagers
                                                               .FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);

                tcmClient = await _context.TCMClient
                                          .Include(g => g.Casemanager)
                                          .Include(g => g.Client)
                                          .ThenInclude(g => g.Clients_HealthInsurances)
                                          .ThenInclude(g => g.HealthInsurance)
                                          .Where(g => (g.Casemanager.Id == caseManager.Id
                                                    && g.Status == StatusType.Open))
                                          .OrderBy(g => g.Client.Name)
                                          .ToListAsync();
            }

            if (user_logged.UserType.ToString() == "Manager")
            {

                tcmClient = await _context.TCMClient
                                          .Include(g => g.Casemanager)
                                          .Include(g => g.Client)
                                          .ThenInclude(g => g.Clients_HealthInsurances)
                                          .ThenInclude(g => g.HealthInsurance)
                                          .Where(s => s.Client.Clinic.Id == user_logged.Clinic.Id
                                                   && s.Status == StatusType.Open)
                                          .OrderBy(g => g.Casemanager.Name)
                                          .ToListAsync();
               
            }

            if (user_logged.UserType.ToString() == "TCMSupervisor")
            {

                tcmClient = await _context.TCMClient
                                          .Include(g => g.Casemanager)
                                          .Include(g => g.Client)
                                          .ThenInclude(g => g.Clients_HealthInsurances)
                                          .ThenInclude(g => g.HealthInsurance)
                                          .Where(s => s.Client.Clinic.Id == user_logged.Clinic.Id
                                                   && s.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                                   && s.Status == StatusType.Open)
                                          .OrderBy(g => g.Casemanager.Name)
                                          .ToListAsync();
              
            }

            ViewData["warning"] = warning;
            if (warning == 0)
            {
                return View(tcmClient);
            }
            else
            {
                if (warning == 1)
                {
                    return View(tcmClient.Where(n => n.DataClose.Date.AddDays(-15) > DateTime.Today.Date && n.DataClose.Date <= DateTime.Today.Date));
                }
                else 
                {
                    return View(tcmClient.Where(n => n.DataClose.Date < DateTime.Today.Date));
                }
            }            
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> TCMReferralAccept(int? idTCMClient)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (idTCMClient == null)
            {
                return RedirectToAction("Home/Error404");
            }

            if (User.IsInRole("TCMSupervisor"))
            {
                TCMReferralFormEntity tcmReferral = await _context.TCMReferralForms
                                                                  .Include(g => g.TcmClient)
                                                                  .FirstOrDefaultAsync(g => g.TcmClient.Id == idTCMClient
                                                                                         && g.TcmClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName);
                if (tcmReferral == null)
                {
                    return RedirectToAction("Home/Error404");
                }

                TCMReferralFormViewModel tcmReferralViewModel = _converterHelper.ToTCMReferralFormViewModel(tcmReferral);
                return View(tcmReferralViewModel);
            }
            if (User.IsInRole("CaseManager"))
            {
                TCMReferralFormEntity tcmReferral = await _context.TCMReferralForms
                                                                  .Include(g => g.TcmClient)
                                                                  .FirstOrDefaultAsync(g => g.TcmClient.Id == idTCMClient
                                                                                         && g.TcmClient.Casemanager.LinkedUser == user_logged.UserName);
                if (tcmReferral == null)
                {
                    return RedirectToAction("Home/Error404");
                }

                TCMReferralFormViewModel tcmReferralViewModel = _converterHelper.ToTCMReferralFormViewModel(tcmReferral);
                return View(tcmReferralViewModel);
            }
            if (User.IsInRole("Manager"))
            {
                TCMReferralFormEntity tcmReferral = await _context.TCMReferralForms
                                                                  .Include(g => g.TcmClient)
                                                                  .FirstOrDefaultAsync(g => g.TcmClient.Id == idTCMClient);
                if (tcmReferral == null)
                {
                    return RedirectToAction("Home/Error404");
                }

                TCMReferralFormViewModel tcmReferralViewModel = _converterHelper.ToTCMReferralFormViewModel(tcmReferral);
                return View(tcmReferralViewModel);
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [HttpPost]
        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TCMReferralAccept(TCMReferralFormViewModel model)
        {
            UserEntity user_logged = _context.Users
                                           .Include(u => u.Clinic)
                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMReferralFormEntity tcmReferral = await _converterHelper.ToTCMReferralFormEntity(model, false, user_logged.UserName);
                _context.TCMReferralForms.Update(tcmReferral);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "TCMClients");

                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public IActionResult CreateReferralForm(int idTCMClient = 0)
        {
            TCMClientEntity tcmclient = _context.TCMClient

                                                .Include(n => n.Client)
                                                .ThenInclude(d => d.Clients_Diagnostics)
                                                .ThenInclude(d => d.Diagnostic)
                                                .Include(n => n.Client)
                                                .ThenInclude(d => d.LegalGuardian)
                                                .Include(n => n.Casemanager)
                                                .ThenInclude(d => d.TCMSupervisor)
                                                .Include(n => n.Client)
                                                .ThenInclude(d => d.Client_Referred)
                                                .ThenInclude(d => d.Referred)
                                                .Include(n => n.Client)
                                                .ThenInclude(d => d.Clients_HealthInsurances)
                                                .ThenInclude(d => d.HealthInsurance)

                                                .FirstOrDefault(n => n.Id == idTCMClient);
            
            TCMReferralFormViewModel salida;
            
            Client_Diagnostic client_Diagnostic = new Client_Diagnostic();
            if (tcmclient.Client.Clients_Diagnostics != null)
            {
                client_Diagnostic = tcmclient.Client
                                             .Clients_Diagnostics
                                             .FirstOrDefault(n => n.Principal == true);
            }

            ReferredEntity Referred = new ReferredEntity();
            if (tcmclient.Client.Client_Referred.Count() > 0)
            {
                Referred = tcmclient.Client
                                    .Client_Referred
                                    .FirstOrDefault(n => n.Service == ServiceAgency.TCM)
                                    .Referred;
            }

            Client_HealthInsurance Client_HealtInsurance = new Client_HealthInsurance();
            if (tcmclient.Client.Clients_HealthInsurances.Count() > 0)
            {
                Client_HealtInsurance = tcmclient.Client
                                                 .Clients_HealthInsurances
                                                 .FirstOrDefault(n => n.Active == true 
                                                                   && n.Agency == ServiceAgency.TCM);
            }

            LegalGuardianEntity legalGuardian = new LegalGuardianEntity();
            if (tcmclient.Client.LegalGuardian != null)
            {
                legalGuardian = tcmclient.Client
                                         .LegalGuardian;
            }
            
            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    salida = new TCMReferralFormViewModel
                    {
                        //Client
                        Address = tcmclient.Client.FullAddress,
                        SecondaryPhone = tcmclient.Client.TelephoneSecondary,
                        SSN = tcmclient.Client.SSN,
                        DateOfBirth = tcmclient.Client.DateOfBirth,
                        MedicaidId = tcmclient.Client.MedicaidID,
                        Gender = (tcmclient.Client.Gender == GenderType.Female) ? "Female" : "Male",
                        HMO = string.Empty,
                        PrimaryPhone = tcmclient.Client.Telephone,
                        //audit
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        //tcmClient
                        CaseNumber = tcmclient.CaseNumber,
                        NameClient = tcmclient.Client.Name,
                        //Referred
                        AssignedTo = tcmclient.Casemanager.Name,
                        CaseAccepted = false,
                        Comments = string.Empty,
                        DateAssigned = DateTime.Today,
                        TCMSign = false,
                        TCMSupervisorSign = false,
                        NameSupervisor = tcmclient.Casemanager.TCMSupervisor.Name,
                        Program = "Mental Health Targeted Case Management",
                        //Health Insurance
                        AuthorizedDate = Client_HealtInsurance != null ? Client_HealtInsurance.ApprovedDate : new DateTime(),
                        ExperatedDate = Client_HealtInsurance != null ? Client_HealtInsurance.ApprovedDate.AddMonths(Client_HealtInsurance.DurationTime) : new DateTime(),
                        UnitsApproved = Client_HealtInsurance != null ? Client_HealtInsurance.Units : 0,
                        //Legal Guardian
                        LegalGuardianName = legalGuardian.Name,
                        LegalGuardianPhone = legalGuardian.Telephone,
                        //Diagnostic
                        Dx = client_Diagnostic.Diagnostic.Code,
                        Dx_Description = client_Diagnostic.Diagnostic.Description,
                        //Referred
                        ReferredBy_Name = Referred.Name,
                        ReferredBy_Phone = Referred.Telephone,
                        ReferredBy_Title = Referred.Title,
                        TcmClient = tcmclient,
                        IdTCMClient = tcmclient.Id
                    };

                    return View(salida);
                }
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [HttpPost]
        [Authorize(Roles = "Manager, TCMSupervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReferralForm(TCMReferralFormViewModel model)
        {
            UserEntity user_logged = _context.Users
                                                .Include(u => u.Clinic)
                                                .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                if (ModelState.IsValid)
                {
                    TCMReferralFormEntity tcmReferral = await _converterHelper.ToTCMReferralFormEntity(model, true, user_logged.UserName);
                    tcmReferral.TCMSupervisorSign = true;
                    _context.Add(tcmReferral);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Index", "TCMClients");
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }


                }
                else
                {
                    return View(model);
                }
            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
          
            return View(model);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintReferralForm(int id)
        {
            TCMClientEntity entity = await _context.TCMClient

                                                    .Include(n => n.Client)
                                                    .ThenInclude(d => d.Clients_Diagnostics)
                                                    .ThenInclude(d => d.Diagnostic)
                                                      
                                                    .Include(n => n.Client)
                                                    .ThenInclude(d => d.LegalGuardian)
                                                      
                                                    .Include(n => n.Casemanager)
                                                    .ThenInclude(d => d.TCMSupervisor)

                                                    .Include(n => n.Casemanager)
                                                    .ThenInclude(d => d.Clinic)

                                                    .Include(n => n.Client)
                                                    .ThenInclude(d => d.Client_Referred)
                                                    .ThenInclude(d => d.Referred)

                                                    .Include(n => n.Client)
                                                    .ThenInclude(d => d.Clients_HealthInsurances)
                                                    .ThenInclude(d => d.HealthInsurance)

                                                    .Include(t => t.TCMReferralForm)

                                                    .FirstOrDefaultAsync(n => n.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMReferralFormReport(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "Manager, CaseManager, TCMSupervisor")]
        public async Task<IActionResult> Transfers()
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
                ViewData["IdTCMLogin"] = caseManager.Id;
                return View(await _context.TCMTransfers
                                          .Include(g => g.TCMAssignedFrom)
                                          .Include(g => g.TCMAssignedTo)
                                          .Include(g => g.TCMClient)
                                          .ThenInclude(g => g.Client)
                                          .Include(g => g.TCMSupervisor)
                                          .Where(g => (g.TCMAssignedFrom.Id == caseManager.Id
                                                    || g.TCMAssignedTo.Id == caseManager.Id))
                                          .OrderBy(g => g.TCMClient.Client.Name)
                                          .ToListAsync());
            }

            if (user_logged.UserType.ToString() == "Manager")
            {
                ViewData["IdTCMLogin"] = 0;
                return View(await _context.TCMTransfers
                                          .Include(g => g.TCMAssignedFrom)
                                          .Include(g => g.TCMAssignedTo)
                                          .Include(g => g.TCMClient)
                                          .ThenInclude(g => g.Client)
                                          .Include(g => g.TCMSupervisor)
                                          .Where(g => (g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id))
                                          .OrderBy(g => g.TCMClient.Client.Name)
                                          .ToListAsync());
            }

            if (user_logged.UserType.ToString() == "TCMSupervisor")
            {
                TCMSupervisorEntity tcmSupervisor = await _context.TCMSupervisors
                                                                  .FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
                ViewData["IdTCMLogin"] = 0;
                return View(await _context.TCMTransfers
                                          .Include(g => g.TCMAssignedFrom)
                                          .Include(g => g.TCMAssignedTo)
                                          .Include(g => g.TCMClient)
                                          .ThenInclude(g => g.Client)
                                          .Include(g => g.TCMSupervisor)
                                          .Where(g => (g.TCMSupervisor.Id == tcmSupervisor.Id))
                                          .OrderBy(g => g.TCMClient.Client.Name)
                                          .ToListAsync());
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult CreateTransfer(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(m => m.Client)
                                                .Include(m => m.Client.LegalGuardian)
                                                .Include(n => n.Casemanager)
                                                .ThenInclude(n => n.TCMSupervisor)
                                                .Include(n => n.TcmServicePlan)
                                                .ThenInclude(n => n.TCMServicePlanReview)
                                                .Include(m => m.TCMNote)
                                                .FirstOrDefault(n => n.Id == id);
            TCMSupervisorEntity tcmsupervisor = _context.TCMSupervisors
                                                        .FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
           
            TCMTransferViewModel model;
            DateTime temp = new DateTime();
            
            if (user_logged.Clinic != null)
                {

                    model = new TCMTransferViewModel
                    {
                        IdTCMClient = id,
                        Address = tcmClient.Client.FullAddress,
                        ChangeInformation = false,
                        CityStateZip = tcmClient.Client.City + ',' +tcmClient.Client.State + ',' + tcmClient.Client.ZipCode,
                        DateAudit = DateTime.Today,
                        DateAuditSign = DateTime.Today,
                        DateLastService = (tcmClient.TCMNote.Count() > 0)? tcmClient.TCMNote.Max(n => n.DateOfService) : temp,
                        DateServicePlanORLastSPR = (tcmClient.TcmServicePlan == null)? temp : (tcmClient.TcmServicePlan.TCMServicePlanReview == null)? tcmClient.TcmServicePlan.DateServicePlan : tcmClient.TcmServicePlan.TCMServicePlanReview.DateServicePlanReview,
                        EndTransferDate = DateTime.Today,
                        HasClientChart = false,
                        Id = 0,
                        IdCaseManagerFrom = tcmClient.Casemanager.Id, 
                        IdCaseManagerTo = 0,
                        IdTCMSupervisor = tcmClient.Casemanager.TCMSupervisor.Id,
                        OpeningDate = tcmClient.DataOpen,
                        OpeningDateAssignedTo = DateTime.Today,
                        PrimaryPhone = string.Empty,
                        OtherPhone = string.Empty,
                        Return = false,
                        TCMAssignedFromAccept = false,
                        TCMAssignedToAccept =  false,
                        TransferFollow = string.Empty,
                        TCMsFrom = _combosHelper.GetComboCaseManagersByTCMSupervisor(user_logged.UserName,1),
                        TCMsTo = _combosHelper.GetComboCaseManagersByTCMSupervisor(user_logged.UserName,1),
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Today,
                        TCMClient = tcmClient,
                        TCMSupervisor = tcmsupervisor,
                        TCMAssignedFrom = tcmClient.Casemanager,
                        TCMSupervisorAccept = true

                    };
                    if (tcmClient.Client.LegalGuardian != null)
                    {
                        model.LegalGuardianName = tcmClient.Client.LegalGuardian.Name;
                        model.LegalGuardianPhone = tcmClient.Client.LegalGuardian.Telephone;
                    }
                    return View(model);
                }
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> CreateTransfer(TCMTransferViewModel transferViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMTransferEntity tcmTransfer = new TCMTransferEntity();

            if (ModelState.IsValid)
            {
                tcmTransfer = await _converterHelper.ToTCMTransferEntity(transferViewModel, true, user_logged.UserName);
                _context.Add(tcmTransfer);

                try
                {
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Transfers");
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            return View(transferViewModel);
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult SelectTCMClientForTransfer()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMTransferViewModel model = new TCMTransferViewModel()
            {
                IdTCMClient = 0,
                TCMClients = _combosHelper.GetComboTCMClientsByCaseManagerByTCMSupervisor(user_logged.UserName,1).OrderBy(n => n.Text),

            };
           
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> SelectTCMClientForTransfer(TCMTransferViewModel tcmTransferViewModel)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            return RedirectToAction("CreateTransfer", "TCMClients", new { id = tcmTransferViewModel.IdTCMClient });
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult EditTransfer(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMTransferEntity transfer = _context.TCMTransfers
                                                 .Include(m => m.TCMClient)
                                                 .ThenInclude(m => m.Client)
                                                 .ThenInclude(m => m.LegalGuardian)
                                                 .Include(n => n.TCMAssignedFrom)
                                                 .Include(n => n.TCMAssignedTo)
                                                 .Include(n => n.TCMSupervisor)
                                                 .Include(n => n.TCMClient)
                                                 .ThenInclude(n => n.TcmServicePlan)
                                                 .ThenInclude(n => n.TCMServicePlanReview)
                                                 .Include(m => m.TCMClient)
                                                 .ThenInclude(n => n.TCMNote)
                                                 .FirstOrDefault(n => n.Id == id);

            TCMTransferViewModel model = _converterHelper.ToTCMTransferViewModel(transfer);

            if (User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {

                   
                    if (model.TCMClient.Client.LegalGuardian != null)
                    {
                        model.LegalGuardianName = model.TCMClient.Client.LegalGuardian.Name;
                        model.LegalGuardianPhone = model.TCMClient.Client.LegalGuardian.Telephone;
                    }
                    return View(model);
                }
            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> EditTransfer(TCMTransferViewModel transferViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMTransferEntity tcmTransfer = new TCMTransferEntity();

            if (ModelState.IsValid)
            {
                tcmTransfer = await _converterHelper.ToTCMTransferEntity(transferViewModel, false, user_logged.UserName);
                _context.Update(tcmTransfer);
                try
                {
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Transfers");
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            return View(transferViewModel);
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult AcceptTransfer(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMTransferEntity transfer = _context.TCMTransfers
                                                 .Include(m => m.TCMClient)
                                                 .ThenInclude(m => m.Client)
                                                 .ThenInclude(m => m.LegalGuardian)
                                                 .Include(n => n.TCMAssignedFrom)
                                                 .Include(n => n.TCMAssignedTo)
                                                 .Include(n => n.TCMSupervisor)
                                                 .Include(n => n.TCMClient)
                                                 .ThenInclude(n => n.TcmServicePlan)
                                                 .ThenInclude(n => n.TCMServicePlanReview)
                                                 .Include(m => m.TCMClient)
                                                 .ThenInclude(n => n.TCMNote)
                                                 .FirstOrDefault(n => n.Id == id);

            TCMTransferViewModel model = _converterHelper.ToTCMTransferViewModel(transfer);

            if (User.IsInRole("CaseManager"))
            {
                if (model.TCMClient.Client.LegalGuardian != null)
                {
                    model.LegalGuardianName = model.TCMClient.Client.LegalGuardian.Name;
                    model.LegalGuardianPhone = model.TCMClient.Client.LegalGuardian.Telephone;
                }
                if (transfer.TCMAssignedFrom.Id == _context.CaseManagers.FirstOrDefault(n => n.LinkedUser == user_logged.UserName).Id)
                {
                    ViewData["From"] = 1;
                }
                else
                {
                    ViewData["From"] = 0;
                }
                return View(model);
            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> AcceptTransfer(TCMTransferViewModel transferViewModel, int from)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMTransferEntity tcmTransfer = _context.TCMTransfers.FirstOrDefault(n => n.Id == transferViewModel.Id);
            if (from == 1)
            {
                tcmTransfer.TCMAssignedFromAccept = transferViewModel.TCMAssignedFromAccept;
                tcmTransfer.LastModifiedBy = user_logged.UserName;
                tcmTransfer.LastModifiedOn = DateTime.Now;
            }
            else
            {
                tcmTransfer.TCMAssignedToAccept = transferViewModel.TCMAssignedToAccept;
                tcmTransfer.OpeningDateAssignedTo = transferViewModel.OpeningDateAssignedTo;
                tcmTransfer.LastModifiedBy = user_logged.UserName;
                tcmTransfer.LastModifiedOn = DateTime.Now;
            }

            if (tcmTransfer.TCMAssignedFromAccept == true && tcmTransfer.TCMAssignedToAccept == true && tcmTransfer.TCMSupervisorAccept == true)
            {
                TCMClientEntity tcmClient = _context.TCMClient.Include(n => n.Casemanager).FirstOrDefault(n => n.Id == transferViewModel.IdTCMClient);
                tcmClient.Casemanager = _context.CaseManagers.FirstOrDefault(n => n.Id == transferViewModel.IdCaseManagerTo);
                _context.Update(tcmClient);
            }

            if (ModelState.IsValid)
            {
                _context.Update(tcmTransfer);
                try
                {
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Transfers");
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            return View(transferViewModel);
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult TransferReadOnly(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMTransferEntity transfer = _context.TCMTransfers
                                                 .Include(m => m.TCMClient)
                                                 .ThenInclude(m => m.Client)
                                                 .ThenInclude(m => m.LegalGuardian)
                                                 .Include(n => n.TCMAssignedFrom)
                                                 .Include(n => n.TCMAssignedTo)
                                                 .Include(n => n.TCMSupervisor)
                                                 .Include(n => n.TCMClient)
                                                 .ThenInclude(n => n.TcmServicePlan)
                                                 .ThenInclude(n => n.TCMServicePlanReview)
                                                 .Include(m => m.TCMClient)
                                                 .ThenInclude(n => n.TCMNote)
                                                 .FirstOrDefault(n => n.Id == id);

            TCMTransferViewModel model = _converterHelper.ToTCMTransferViewModel(transfer);

            if (model.TCMClient.Client.LegalGuardian != null)
            {
                model.LegalGuardianName = model.TCMClient.Client.LegalGuardian.Name;
                model.LegalGuardianPhone = model.TCMClient.Client.LegalGuardian.Telephone;
            }

            return View(model);
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult ReturnTransfer(int idTransfer = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMTransferEntity transfer = _context.TCMTransfers
                                                 .Include(n => n.TCMClient)
                                                 .ThenInclude(n => n.Client)
                                                 .Include(n => n.TCMSupervisor)
                                                 .Include(n => n.TCMAssignedFrom)
                                                 .FirstOrDefault(n => n.Id == idTransfer);

            TCMTransferViewModel model = new TCMTransferViewModel()
            {
                IdTCMClient = transfer.TCMClient.Id,
                TCMClient = transfer.TCMClient,
                TCMAssignedFrom = transfer.TCMAssignedFrom,
                IdCaseManagerFrom = transfer.TCMAssignedFrom.Id,
                Id = transfer.Id
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> ReturnTransfer(TCMTransferViewModel tcmTransferViewModel)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            TCMClientEntity tcmClient = _context.TCMClient.Include(n => n.Casemanager).FirstOrDefault(n => n.Id == tcmTransferViewModel.IdTCMClient);
            tcmClient.Casemanager = _context.CaseManagers.FirstOrDefault(n => n.Id == tcmTransferViewModel.IdCaseManagerFrom);

            TCMTransferEntity transferEntity = _context.TCMTransfers.FirstOrDefault(n => n.Id == tcmTransferViewModel.Id);
            transferEntity.Return = true;
            _context.Update(transferEntity);
            _context.Update(tcmClient);
            try
            {
                await _context.SaveChangesAsync();

                return RedirectToAction("Transfers");
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.InnerException.Message);
            }

            return View(tcmTransferViewModel);
        }

        [Authorize(Roles = "TCMSupervisor, Manager")]
        public IActionResult ViewPendingTransfer(int idTransfer = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMTransferEntity transfer = _context.TCMTransfers
                                                 .Include(m => m.TCMClient)
                                                 .FirstOrDefault(n => n.Id == idTransfer);

            TCMTransferViewModel model = new TCMTransferViewModel();

            if (User.IsInRole("TCMSupervisor") || User.IsInRole("Manager"))
            {
                model.TCMSupervisorAccept = transfer.TCMSupervisorAccept;
                model.TCMAssignedFromAccept = transfer.TCMAssignedFromAccept;
                model.TCMAssignedToAccept = transfer.TCMAssignedToAccept;
            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            return View(model);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTransfer(int id)
        {
            TCMTransferEntity entity = await _context.TCMTransfers

                                                     .Include(t => t.TCMClient)
                                                     .ThenInclude(t => t.Client)

                                                     .Include(t => t.TCMAssignedFrom)

                                                     .Include(t => t.TCMAssignedTo)

                                                     .Include(t => t.TCMSupervisor)  
                                                     .ThenInclude(s => s.Clinic)

                                                     .FirstOrDefaultAsync(n => n.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMTransferReport(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }
    }
}
