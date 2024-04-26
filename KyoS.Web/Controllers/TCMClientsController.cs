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
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Reporting;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using KyoS.Common.Helpers;
using KyoS.Web.Migrations;
using AspNetCore;
using DocumentFormat.OpenXml.Wordprocessing;

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
        private readonly IFileHelper _fileHelper;
        private readonly IMimeType _mimeType;
        private readonly IWebHostEnvironment _webhostEnvironment;

        public IConfiguration Configuration { get; }


        public TCMClientsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IReportHelper reportHelper, IRenderHelper renderHelper, IImageHelper imageHelper, IFileHelper fileHelper, IConfiguration configuration, IWebHostEnvironment webhostEnvironment, IMimeType mimeType)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
            _renderHelper = renderHelper;
            _imageHelper = imageHelper;
            _fileHelper = fileHelper;
            Configuration = configuration;
            _mimeType = mimeType;
            _webhostEnvironment = webhostEnvironment;
        }

        [Authorize(Roles = "Manager, CaseManager, TCMSupervisor, Biller")]
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

            if (user_logged.UserType.ToString() == "Manager" || user_logged.UserType.ToString() == "Biller")
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
                    TCMClientEntity tcmClient = await _context.TCMClient.FirstOrDefaultAsync(s => ((s.Client.Id == model.IdClient
                                                                                               && s.Status == StatusType.Open)
                                                                                               || (s.CaseNumber == model.CaseNumber)));
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
                        ModelState.AddModelError(string.Empty, "Already exists the TCM Case or Case Number.");
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
                                                                  .AsSplitQuery()
                                                                  .Where(g => (g.Casemanager.Id == caseManager.Id
                                                                        && g.Status == StatusType.Open
                                                                        && (g.TcmServicePlan == null 
                                                                            || g.TCMAssessment == null
                                                                            || (g.TcmIntakeAppendixJ == null
                                                                             && g.TcmIntakeAppendixI == null))))
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
                                                                  .AsSplitQuery()
                                                                  .Where(g => (g.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                                                        && g.Status == StatusType.Open
                                                                        && (g.TcmServicePlan == null
                                                                            
                                                                            || g.TCMAssessment == null
                                                                            || (g.TcmIntakeAppendixJ == null
                                                                             && g.TcmIntakeAppendixI == null))))
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
                                                                 .AsSplitQuery()
                                                                 .Where(s => (s.Client.Clinic.Id == user_logged.Clinic.Id
                                                                    && s.Status == StatusType.Open
                                                                    && (s.TcmServicePlan == null
                                                                        || s.TCMAssessment == null
                                                                        || (s.TcmIntakeAppendixJ == null
                                                                         && s.TcmIntakeAppendixI == null))))
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
                                                                    .Include(g => g.TcmIntakeAppendixI)
                                                                    .AsSplitQuery()
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
                   // if (item.TcmIntakeAppendixJ == null)
                    //    item.TcmIntakeAppendixJ = new TCMIntakeAppendixJEntity();
                    
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
                                                                    .Include(g => g.TcmIntakeAppendixI)
                                                                    .AsSplitQuery()
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
                   // if (item.TcmIntakeAppendixJ == null)
                   //     item.TcmIntakeAppendixJ = new TCMIntakeAppendixJEntity();

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
                                          .AsSplitQuery()
                                          .Where(c => c.Casemanager.LinkedUser == user_logged.UserName)
                                          .OrderBy(c => c.Client.Name).ToListAsync());
            }
            if (User.IsInRole("TCMSupervisor"))
            {
                return View(await _context.TCMClient
                                          .Include(c => c.Client)
                                          .ThenInclude(c => c.Clinic)
                                          .AsSplitQuery()
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

        [Authorize(Roles = "Manager, CaseManager, TCMSupervisor, Biller")]
        public async Task<IActionResult> TCMCaseHistory(int id = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor") || User.IsInRole("CaseManager") || User.IsInRole("Biller"))
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
                                                          .Include(g => g.TcmIntakeAppendixI)
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
            if (tcmclient.Client.Clients_Diagnostics.Count() > 0)
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
                        Dx = (client_Diagnostic.Diagnostic != null)? client_Diagnostic.Diagnostic.Code : string.Empty,
                        Dx_Description = (client_Diagnostic.Diagnostic != null) ? client_Diagnostic.Diagnostic.Description : string.Empty,
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
                tcmTransfer = _converterHelper.ToTCMTransferEntity(transferViewModel, true, user_logged.UserName);
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
                tcmTransfer = _converterHelper.ToTCMTransferEntity(transferViewModel, false, user_logged.UserName);
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

        [Authorize(Roles = "Manager, TCMSupervisor")]

        //esto fue para generar los medical history de TCM a partir de los exixtentes en MH
        public async Task<IActionResult> CopyTCMMedicalHistory()
        {
            List<TCMClientEntity> listTCMClient = await _context.TCMClient
                                                                .Include(n => n.Client)
                                                                .ThenInclude(n => n.IntakeMedicalHistory)
                                                                .Include(n => n.Casemanager)
                                                                .ToListAsync();
            
            if (listTCMClient.Count() == 0)
            {
                return RedirectToAction("Home/Error404");
            }

            List<TCMIntakeMedicalHistoryEntity> TCMmedicalHistoryList = new List<TCMIntakeMedicalHistoryEntity>();

            foreach (var item in listTCMClient)
            {
                if (item.Client.IntakeMedicalHistory != null)
                {
                    TCMIntakeMedicalHistoryEntity TCMmedicalHistory = new TCMIntakeMedicalHistoryEntity();

                    TCMmedicalHistory.AddressPhysician = item.Client.IntakeMedicalHistory.AddressPhysician;
                    TCMmedicalHistory.AdmissionedFor = item.Casemanager.Name;
                    TCMmedicalHistory.AgeFirstTalked = item.Client.IntakeMedicalHistory.AgeFirstTalked;
                    TCMmedicalHistory.AgeFirstWalked = item.Client.IntakeMedicalHistory.AgeFirstWalked;
                    TCMmedicalHistory.AgeOfFirstMenstruation = item.Client.IntakeMedicalHistory.AgeOfFirstMenstruation;
                    TCMmedicalHistory.AgeToiletTrained = item.Client.IntakeMedicalHistory.AgeToiletTrained;
                    TCMmedicalHistory.AgeWeaned = item.Client.IntakeMedicalHistory.AgeWeaned;
                    TCMmedicalHistory.Allergies = item.Client.IntakeMedicalHistory.Allergies;
                    TCMmedicalHistory.Allergies_Describe = item.Client.IntakeMedicalHistory.Allergies_Describe;
                    TCMmedicalHistory.AndOrSoiling = item.Client.IntakeMedicalHistory.AndOrSoiling;
                    TCMmedicalHistory.Anemia = item.Client.IntakeMedicalHistory.Anemia;
                    TCMmedicalHistory.AreYouCurrently = item.Client.IntakeMedicalHistory.AreYouCurrently;
                    TCMmedicalHistory.AreYouPhysician = item.Client.IntakeMedicalHistory.AreYouPhysician;
                    TCMmedicalHistory.Arthritis = item.Client.IntakeMedicalHistory.Arthritis;
                    TCMmedicalHistory.AssumingCertainPositions = item.Client.IntakeMedicalHistory.AssumingCertainPositions;
                    TCMmedicalHistory.BackPain = item.Client.IntakeMedicalHistory.BackPain;
                    TCMmedicalHistory.BeingConfused = item.Client.IntakeMedicalHistory.BeingConfused;
                    TCMmedicalHistory.BeingDisorientated = item.Client.IntakeMedicalHistory.BeingDisorientated;
                    TCMmedicalHistory.BirthWeight = item.Client.IntakeMedicalHistory.BirthWeight;
                    TCMmedicalHistory.BlackStools = item.Client.IntakeMedicalHistory.BlackStools;
                    TCMmedicalHistory.BloodInUrine = item.Client.IntakeMedicalHistory.BloodInUrine;
                    TCMmedicalHistory.BloodyStools = item.Client.IntakeMedicalHistory.BloodyStools;
                    TCMmedicalHistory.BottleFedUntilAge = item.Client.IntakeMedicalHistory.BottleFedUntilAge;
                    TCMmedicalHistory.BreastFed = item.Client.IntakeMedicalHistory.BreastFed;
                    TCMmedicalHistory.BurningUrine = item.Client.IntakeMedicalHistory.BurningUrine;
                    TCMmedicalHistory.Calculating = item.Client.IntakeMedicalHistory.Calculating;
                    TCMmedicalHistory.Cancer = item.Client.IntakeMedicalHistory.Cancer;
                    TCMmedicalHistory.ChestPain = item.Client.IntakeMedicalHistory.ChestPain;
                    TCMmedicalHistory.ChronicCough = item.Client.IntakeMedicalHistory.ChronicCough;
                    TCMmedicalHistory.ChronicIndigestion = item.Client.IntakeMedicalHistory.ChronicIndigestion;
                    TCMmedicalHistory.City = item.Client.IntakeMedicalHistory.City;
                    TCMmedicalHistory.Complications = item.Client.IntakeMedicalHistory.Complications;
                    TCMmedicalHistory.Complications_Explain = item.Client.IntakeMedicalHistory.Complications_Explain;
                    TCMmedicalHistory.Comprehending = item.Client.IntakeMedicalHistory.Comprehending;
                    TCMmedicalHistory.Concentrating = item.Client.IntakeMedicalHistory.Concentrating;
                    TCMmedicalHistory.Constipation = item.Client.IntakeMedicalHistory.Constipation;
                    TCMmedicalHistory.ConvulsionsOrFits = item.Client.IntakeMedicalHistory.ConvulsionsOrFits;
                    TCMmedicalHistory.CoughingOfBlood = item.Client.IntakeMedicalHistory.CoughingOfBlood;
                    TCMmedicalHistory.CreatedBy = item.Client.IntakeMedicalHistory.CreatedBy;
                    TCMmedicalHistory.CreatedOn = item.Client.IntakeMedicalHistory.CreatedOn;
                    TCMmedicalHistory.DateOfLastBreastExam = item.Client.IntakeMedicalHistory.DateOfLastBreastExam;
                    TCMmedicalHistory.DateOfLastPelvic = item.Client.IntakeMedicalHistory.DateOfLastPelvic;
                    TCMmedicalHistory.DateOfLastPeriod = item.Client.IntakeMedicalHistory.DateOfLastPeriod;
                    TCMmedicalHistory.DateSignatureEmployee = item.Client.IntakeMedicalHistory.DateSignatureEmployee;
                    TCMmedicalHistory.DateSignatureLegalGuardian = item.Client.IntakeMedicalHistory.DateSignatureLegalGuardian;
                    TCMmedicalHistory.DateSignaturePerson = item.Client.IntakeMedicalHistory.DateSignaturePerson;
                    TCMmedicalHistory.DescriptionOfChild = item.Client.IntakeMedicalHistory.DescriptionOfChild;
                    TCMmedicalHistory.Diabetes = item.Client.IntakeMedicalHistory.Diabetes;
                    TCMmedicalHistory.Diphtheria = item.Client.IntakeMedicalHistory.Diphtheria;
                    TCMmedicalHistory.Documents = item.Client.IntakeMedicalHistory.Documents;
                    TCMmedicalHistory.DoYouSmoke = item.Client.IntakeMedicalHistory.DoYouSmoke;
                    TCMmedicalHistory.DoYouSmoke_PackPerDay = item.Client.IntakeMedicalHistory.DoYouSmoke_PackPerDay;
                    TCMmedicalHistory.DoYouSmoke_Year = item.Client.IntakeMedicalHistory.DoYouSmoke_Year;
                    TCMmedicalHistory.EarInfections = item.Client.IntakeMedicalHistory.EarInfections;
                    TCMmedicalHistory.EndTime = item.Client.IntakeMedicalHistory.EndTime;
                    TCMmedicalHistory.Epilepsy = item.Client.IntakeMedicalHistory.Epilepsy;
                    TCMmedicalHistory.EyeTrouble = item.Client.IntakeMedicalHistory.EyeTrouble;
                    TCMmedicalHistory.Fainting = item.Client.IntakeMedicalHistory.Fainting;
                    TCMmedicalHistory.FamilyAsthma = item.Client.IntakeMedicalHistory.FamilyAsthma;
                    TCMmedicalHistory.FamilyAsthma_ = item.Client.IntakeMedicalHistory.FamilyAsthma_;
                    TCMmedicalHistory.FamilyCancer = item.Client.IntakeMedicalHistory.FamilyCancer;
                    TCMmedicalHistory.FamilyCancer_ = item.Client.IntakeMedicalHistory.FamilyCancer_;
                    TCMmedicalHistory.FamilyDiabetes = item.Client.IntakeMedicalHistory.FamilyDiabetes;
                    TCMmedicalHistory.FamilyDiabetes_ = item.Client.IntakeMedicalHistory.FamilyDiabetes_;
                    TCMmedicalHistory.FamilyEpilepsy = item.Client.IntakeMedicalHistory.FamilyEpilepsy;
                    TCMmedicalHistory.FamilyEpilepsy_ = item.Client.IntakeMedicalHistory.FamilyEpilepsy_;
                    TCMmedicalHistory.FamilyGlaucoma = item.Client.IntakeMedicalHistory.FamilyGlaucoma;
                    TCMmedicalHistory.FamilyGlaucoma_ = item.Client.IntakeMedicalHistory.FamilyGlaucoma_;
                    TCMmedicalHistory.FamilyHayFever = item.Client.IntakeMedicalHistory.FamilyHayFever;
                    TCMmedicalHistory.FamilyHayFever_ = item.Client.IntakeMedicalHistory.FamilyHayFever_;
                    TCMmedicalHistory.FamilyHeartDisease = item.Client.IntakeMedicalHistory.FamilyHeartDisease;
                    TCMmedicalHistory.FamilyHeartDisease_ = item.Client.IntakeMedicalHistory.FamilyHeartDisease_;
                    TCMmedicalHistory.FamilyHighBloodPressure = item.Client.IntakeMedicalHistory.FamilyHighBloodPressure;
                    TCMmedicalHistory.FamilyHighBloodPressure_ = item.Client.IntakeMedicalHistory.FamilyHighBloodPressure_;
                    TCMmedicalHistory.FamilyKidneyDisease = item.Client.IntakeMedicalHistory.FamilyKidneyDisease;
                    TCMmedicalHistory.FamilyKidneyDisease_ = item.Client.IntakeMedicalHistory.FamilyKidneyDisease_;
                    TCMmedicalHistory.FamilyNervousDisorders = item.Client.IntakeMedicalHistory.FamilyNervousDisorders;
                    TCMmedicalHistory.FamilyNervousDisorders_ = item.Client.IntakeMedicalHistory.FamilyNervousDisorders_;
                    TCMmedicalHistory.FamilyOther = item.Client.IntakeMedicalHistory.FamilyOther;
                    TCMmedicalHistory.FamilyOther_ = item.Client.IntakeMedicalHistory.FamilyOther_;
                    TCMmedicalHistory.FamilySyphilis = item.Client.IntakeMedicalHistory.FamilySyphilis;
                    TCMmedicalHistory.FamilySyphilis_ = item.Client.IntakeMedicalHistory.FamilySyphilis_;
                    TCMmedicalHistory.FamilyTuberculosis = item.Client.IntakeMedicalHistory.FamilyTuberculosis;
                    TCMmedicalHistory.FamilyTuberculosis_ = item.Client.IntakeMedicalHistory.FamilyTuberculosis_;
                    TCMmedicalHistory.FirstYearMedical = item.Client.IntakeMedicalHistory.FirstYearMedical;
                    TCMmedicalHistory.Fractures = item.Client.IntakeMedicalHistory.Fractures;
                    TCMmedicalHistory.FrequentColds = item.Client.IntakeMedicalHistory.FrequentColds;
                    TCMmedicalHistory.FrequentHeadaches = item.Client.IntakeMedicalHistory.FrequentHeadaches;
                    TCMmedicalHistory.FrequentNoseBleeds = item.Client.IntakeMedicalHistory.FrequentNoseBleeds;
                    TCMmedicalHistory.FrequentSoreThroat = item.Client.IntakeMedicalHistory.FrequentSoreThroat;
                    TCMmedicalHistory.FrequentVomiting = item.Client.IntakeMedicalHistory.FrequentVomiting;
                    TCMmedicalHistory.HaveYouEverBeenPregnant = item.Client.IntakeMedicalHistory.HaveYouEverBeenPregnant;
                    TCMmedicalHistory.HaveYouEverHadComplications = item.Client.IntakeMedicalHistory.HaveYouEverHadComplications;
                    TCMmedicalHistory.HaveYouEverHadComplications = item.Client.IntakeMedicalHistory.HaveYouEverHadComplications;
                    TCMmedicalHistory.HaveYouEverHadExcessive = item.Client.IntakeMedicalHistory.HaveYouEverHadExcessive;
                    TCMmedicalHistory.HaveYouEverHadPainful = item.Client.IntakeMedicalHistory.HaveYouEverHadPainful;
                    TCMmedicalHistory.HaveYouEverHadSpotting = item.Client.IntakeMedicalHistory.HaveYouEverHadSpotting;
                    TCMmedicalHistory.HayFever = item.Client.IntakeMedicalHistory.HayFever;
                    TCMmedicalHistory.HeadInjury = item.Client.IntakeMedicalHistory.HeadInjury;
                    TCMmedicalHistory.Hearing = item.Client.IntakeMedicalHistory.Hearing;
                    TCMmedicalHistory.HearingTrouble = item.Client.IntakeMedicalHistory.HearingTrouble;
                    TCMmedicalHistory.HeartPalpitation = item.Client.IntakeMedicalHistory.HeartPalpitation;
                    TCMmedicalHistory.Hemorrhoids = item.Client.IntakeMedicalHistory.Hemorrhoids;
                    TCMmedicalHistory.Hepatitis = item.Client.IntakeMedicalHistory.Hepatitis;
                    TCMmedicalHistory.Hernia = item.Client.IntakeMedicalHistory.Hernia;
                    TCMmedicalHistory.HighBloodPressure = item.Client.IntakeMedicalHistory.HighBloodPressure;
                    TCMmedicalHistory.Hoarseness = item.Client.IntakeMedicalHistory.Hoarseness;
                    TCMmedicalHistory.Immunizations = item.Client.IntakeMedicalHistory.Immunizations;
                    TCMmedicalHistory.InfectiousDisease = item.Client.IntakeMedicalHistory.InfectiousDisease;
                    TCMmedicalHistory.InformationProvided = item.Client.IntakeMedicalHistory.InformationProvided;
                    TCMmedicalHistory.Jaundice = item.Client.IntakeMedicalHistory.Jaundice;
                    TCMmedicalHistory.KidneyStones = item.Client.IntakeMedicalHistory.KidneyStones;
                    TCMmedicalHistory.KidneyTrouble = item.Client.IntakeMedicalHistory.KidneyTrouble;
                    TCMmedicalHistory.LastModifiedBy = item.Client.IntakeMedicalHistory.LastModifiedBy;
                    TCMmedicalHistory.LastModifiedBy = item.Client.IntakeMedicalHistory.LastModifiedBy;
                    TCMmedicalHistory.LastModifiedOn = item.Client.IntakeMedicalHistory.LastModifiedOn;
                    TCMmedicalHistory.Length = item.Client.IntakeMedicalHistory.Length;
                    TCMmedicalHistory.ListAllCurrentMedications = item.Client.IntakeMedicalHistory.ListAllCurrentMedications;
                    TCMmedicalHistory.LossOfMemory = item.Client.IntakeMedicalHistory.LossOfMemory;
                    TCMmedicalHistory.Mumps = item.Client.IntakeMedicalHistory.Mumps;
                    TCMmedicalHistory.Nervousness = item.Client.IntakeMedicalHistory.Nervousness;
                    TCMmedicalHistory.NightSweats = item.Client.IntakeMedicalHistory.NightSweats;
                    TCMmedicalHistory.Normal = item.Client.IntakeMedicalHistory.Normal;
                    TCMmedicalHistory.PainfulJoints = item.Client.IntakeMedicalHistory.PainfulJoints;
                    TCMmedicalHistory.PainfulMuscles = item.Client.IntakeMedicalHistory.PainfulMuscles;
                    TCMmedicalHistory.PainfulUrination = item.Client.IntakeMedicalHistory.PainfulUrination;
                    TCMmedicalHistory.PerformingCertainMotions = item.Client.IntakeMedicalHistory.PerformingCertainMotions;
                    TCMmedicalHistory.Planned = item.Client.IntakeMedicalHistory.Planned;
                    TCMmedicalHistory.Poliomyelitis = item.Client.IntakeMedicalHistory.Poliomyelitis;
                    TCMmedicalHistory.PrimaryCarePhysician = item.Client.IntakeMedicalHistory.PrimaryCarePhysician;
                    TCMmedicalHistory.ProblemWithBedWetting = item.Client.IntakeMedicalHistory.ProblemWithBedWetting;
                    TCMmedicalHistory.Reading = item.Client.IntakeMedicalHistory.Reading;
                    TCMmedicalHistory.RheumaticFever = item.Client.IntakeMedicalHistory.RheumaticFever;
                    TCMmedicalHistory.Rheumatism = item.Client.IntakeMedicalHistory.Rheumatism;
                    TCMmedicalHistory.ScarletFever = item.Client.IntakeMedicalHistory.ScarletFever;
                    TCMmedicalHistory.Seeing = item.Client.IntakeMedicalHistory.Seeing;
                    TCMmedicalHistory.SeriousInjury = item.Client.IntakeMedicalHistory.SeriousInjury;
                    TCMmedicalHistory.ShortnessOfBreath = item.Client.IntakeMedicalHistory.ShortnessOfBreath;
                    TCMmedicalHistory.SkinTrouble = item.Client.IntakeMedicalHistory.SkinTrouble;
                    TCMmedicalHistory.Speaking = item.Client.IntakeMedicalHistory.Speaking;
                    TCMmedicalHistory.StartTime = item.Client.IntakeMedicalHistory.StartTime;
                    TCMmedicalHistory.State = item.Client.IntakeMedicalHistory.State;
                    TCMmedicalHistory.StomachPain = item.Client.IntakeMedicalHistory.StomachPain;
                    TCMmedicalHistory.Surgery = item.Client.IntakeMedicalHistory.Surgery;
                    TCMmedicalHistory.SwellingOfFeet = item.Client.IntakeMedicalHistory.SwellingOfFeet;
                    TCMmedicalHistory.SwollenAnkles = item.Client.IntakeMedicalHistory.SwollenAnkles;
                    TCMmedicalHistory.TCMClient = item;
                    TCMmedicalHistory.TCMClient_FK = item.Id;
                    TCMmedicalHistory.Tuberculosis = item.Client.IntakeMedicalHistory.Tuberculosis;
                    TCMmedicalHistory.Unplanned = item.Client.IntakeMedicalHistory.Unplanned;
                    TCMmedicalHistory.UsualDurationOfPeriods = item.Client.IntakeMedicalHistory.UsualDurationOfPeriods;
                    TCMmedicalHistory.UsualIntervalBetweenPeriods = item.Client.IntakeMedicalHistory.UsualIntervalBetweenPeriods;
                    TCMmedicalHistory.VaricoseVeins = item.Client.IntakeMedicalHistory.VaricoseVeins;
                    TCMmedicalHistory.VenerealDisease = item.Client.IntakeMedicalHistory.VenerealDisease;
                    TCMmedicalHistory.VomitingOfBlood = item.Client.IntakeMedicalHistory.VomitingOfBlood;
                    TCMmedicalHistory.Walking = item.Client.IntakeMedicalHistory.Walking;
                    TCMmedicalHistory.WeightLoss = item.Client.IntakeMedicalHistory.WeightLoss;
                    TCMmedicalHistory.WhoopingCough = item.Client.IntakeMedicalHistory.WhoopingCough;
                    TCMmedicalHistory.WritingSentence = item.Client.IntakeMedicalHistory.WritingSentence;
                    TCMmedicalHistory.ZipCode = item.Client.IntakeMedicalHistory.ZipCode;

                    TCMmedicalHistoryList.Add(TCMmedicalHistory);
                }
            }

            try
            {
                _context.TCMIntakeMedicalHistory.UpdateRange(TCMmedicalHistoryList);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("Index", "TCMClients");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DownloadApprovedTCMNotesSimultaneous(int id)
        {
            TCMClientEntity TCMclient = await _context.TCMClient
                                                      .Include(n => n.Client)
                                                      .FirstOrDefaultAsync(c => c.Id == id);

            if (TCMclient == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Task<List<FileContentResult>> fileContent1Task = TCMNotesListPrint(id);
           

            await Task.WhenAll(fileContent1Task);
            var fileContent1 = await fileContent1Task;
           
            List<FileContentResult> fileContentList = new List<FileContentResult>();
            fileContentList.AddRange(fileContent1);
           
            return File(_fileHelper.Zip(fileContentList), "application/zip", $"{TCMclient.Client.Name}_Notes.zip");
        }

        public async Task<IActionResult> DownloadTCMIntakeSimultaneous(int id)
        {
            TCMClientEntity TCMclient = await _context.TCMClient
                                                      .Include(n => n.Client)
                                                      .Include(n => n.TCMIntakeForm)
                                                      .Include(n => n.TcmIntakeConsentForTreatment)
                                                      .Include(n => n.TcmIntakeConsentForRelease)
                                                      .Include(n => n.TcmIntakeConsumerRights)
                                                      .Include(n => n.TcmIntakeAcknowledgementHipa)
                                                      .Include(n => n.TCMIntakeOrientationChecklist)
                                                      .Include(n => n.TCMIntakeAdvancedDirective)
                                                      .Include(n => n.TCMIntakeForeignLanguage)
                                                      .Include(n => n.TCMIntakeWelcome)
                                                      .Include(n => n.TCMIntakeClientSignatureVerification)
                                                      .Include(n => n.TCMIntakeClientIdDocumentVerification)
                                                      .Include(n => n.TCMIntakeNutritionalScreen)
                                                      .Include(n => n.TCMIntakePersonalWellbeing)
                                                      .Include(n => n.TCMIntakeColumbiaSuicide)
                                                      .Include(n => n.TCMIntakePainScreen)
                                                      .Include(n => n.Casemanager)
                                                      .ThenInclude(n => n.Clinic)
                                                      .FirstOrDefaultAsync(c => c.Id == id);

            if (TCMclient == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Task<List<FileContentResult>> fileContent1Task = TCMIntakeFormPrint(id);
            Task<List<FileContentResult>> fileContent1Task1 = TCMConsentTreamentPrint(id);
            Task<List<FileContentResult>> fileContent1Task2 = TCMConsentReleasePrint(id);
            Task<List<FileContentResult>> fileContent1Task3 = TCMConsumerRightsPrint(id);
            Task<List<FileContentResult>> fileContent1Task4 = TCMAcknowledgementHippaPrint(id);
            Task<List<FileContentResult>> fileContent1Task5 = TCMOrientationCheckListPrint(id);
            Task<List<FileContentResult>> fileContent1Task6 = TCMAdvancedDirectivePrint(id);
            Task<List<FileContentResult>> fileContent1Task7 = TCMForeignLanguagePrint(id);
            Task<List<FileContentResult>> fileContent1Task8 = TCMWelcomePrint(id);
            Task<List<FileContentResult>> fileContent1Task9 = TCMClientSignaturePrint(id);
            Task<List<FileContentResult>> fileContent1Task10 = TCMCIdDocumentsPrint(id);
            Task<List<FileContentResult>> fileContent1Task11 = TCMCNutricionalPrint(id);
            Task<List<FileContentResult>> fileContent1Task12 = TCMPersonalPrint(id);
            Task<List<FileContentResult>> fileContent1Task13 = TCMColumbPrint(id);
            Task<List<FileContentResult>> fileContent1Task14 = TCMPainScreenPrint(id);


            await Task.WhenAll(fileContent1Task, fileContent1Task1, fileContent1Task2, fileContent1Task3, fileContent1Task4, fileContent1Task5, fileContent1Task6, fileContent1Task7, fileContent1Task8, fileContent1Task9, fileContent1Task10, fileContent1Task11, fileContent1Task12, fileContent1Task13, fileContent1Task14);
            var fileContent = await fileContent1Task;
            var fileContent1 = await fileContent1Task1;
            var fileContent2 = await fileContent1Task2;
            var fileContent3 = await fileContent1Task3;
            var fileContent4 = await fileContent1Task4;
            var fileContent5 = await fileContent1Task5;
            var fileContent6 = await fileContent1Task6;
            var fileContent7 = await fileContent1Task7;
            var fileContent8 = await fileContent1Task8;
            var fileContent9 = await fileContent1Task9;
            var fileContent10 = await fileContent1Task10;
            var fileContent11 = await fileContent1Task11;
            var fileContent12 = await fileContent1Task12;
            var fileContent13 = await fileContent1Task13;
            var fileContent14 = await fileContent1Task14;


            List<FileContentResult> fileContentList = new List<FileContentResult>();
            fileContentList.AddRange(fileContent);
            fileContentList.AddRange(fileContent1);
            fileContentList.AddRange(fileContent2);
            fileContentList.AddRange(fileContent3);
            fileContentList.AddRange(fileContent4);
            fileContentList.AddRange(fileContent5);
            fileContentList.AddRange(fileContent6);
            fileContentList.AddRange(fileContent7);
            fileContentList.AddRange(fileContent8);
            fileContentList.AddRange(fileContent9);
            fileContentList.AddRange(fileContent10);
            fileContentList.AddRange(fileContent11);
            fileContentList.AddRange(fileContent12);
            fileContentList.AddRange(fileContent13);
            fileContentList.AddRange(fileContent14);

            return File(_fileHelper.Zip(fileContentList), "application/zip", $"{TCMclient.Client.Name}_Documents.zip");
        }


        private async Task<List<FileContentResult>> TCMNotesListPrint(int idTCMClient)
        {
            List<TCMNoteEntity> tcmNoteList = new List<TCMNoteEntity>();
            
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                tcmNoteList = await db.TCMNote

                                      .Include(wc => wc.TCMClient)
                                       .ThenInclude(c => c.Casemanager)
                                        .ThenInclude(c => c.TCMSupervisor)

                                      .Include(wc => wc.TCMNoteActivity)

                                      .Include(wc => wc.TCMClient)
                                       .ThenInclude(c => c.Client)
                                        .ThenInclude(c => c.Clinic)

                                      .AsSplitQuery()

                                      .Where(wc => wc.TCMClient.Id == idTCMClient && wc.Status == NoteStatus.Approved)
                                      .ToListAsync();
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            foreach (var tcmNote in tcmNoteList)
            {
                if (tcmNote.TCMClient.Client.Clinic.Name == "SAPPHIRE MENTAL HEALTH CENTER LLC")
                {
                    if (tcmNote.TCMNoteActivity.Count() > 0)
                    {
                        stream = _reportHelper.SapphireMHCTCMNoteReportSchema1(tcmNote);
                        //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                        string name = tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Notes/{name}.pdf"));
                    }
                    
                }
                if (tcmNote.TCMClient.Client.Clinic.Name == "ORION MENTAL HEALTH CENTER LLC")
                {
                    if (tcmNote.TCMNoteActivity.Count() > 0)
                    {
                        stream = _reportHelper.OrionMHCTCMNoteReportSchema1(tcmNote);
                        //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                        string name = tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Notes/{name}.pdf"));
                    }

                }
                if (tcmNote.TCMClient.Client.Clinic.Name == "MY FLORIDA CASE MANAGEMENT SERVICES LLC")
                {
                    if (tcmNote.TCMNoteActivity.Count() > 0)
                    {
                        stream = _reportHelper.MyFloridaTCMNoteReportSchema1(tcmNote);
                        //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                        string name = tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Notes/{name}.pdf"));
                    }

                }
                if (tcmNote.TCMClient.Client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
                {
                    if (tcmNote.TCMNoteActivity.Count() > 0)
                    {
                        stream = _reportHelper.CommunityHTCTCMNoteReportSchema1(tcmNote);
                        //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                        string name = tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Notes/{name}.pdf"));
                    }

                }
            }

            return fileContentList;
        }
        private async Task<List<FileContentResult>> TCMIntakeFormPrint(int idTCMClient)
        {
            TCMIntakeFormEntity intakeForm = new TCMIntakeFormEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                intakeForm = await db.TCMIntakeForms
                                     .Include(n => n.TcmClient)
                                     .ThenInclude(n => n.Casemanager)
                                     .ThenInclude(n => n.Clinic)
                                     .Include(n => n.TcmClient)
                                     .ThenInclude(n => n.Client)
                                     .ThenInclude(n => n.Clients_Diagnostics)
                                     .ThenInclude(n => n.Diagnostic)
                                     .Include(n => n.TcmClient)
                                     .ThenInclude(n => n.Client)
                                     .ThenInclude(n => n.Client_Referred)
                                     .ThenInclude(n => n.Referred)
                                     .Include(n => n.TcmClient)
                                     .ThenInclude(n => n.Client)
                                     .ThenInclude(n => n.LegalGuardian)
                                     .Include(n => n.TcmClient)
                                     .ThenInclude(n => n.Client)
                                     .ThenInclude(n => n.Clients_HealthInsurances)
                                     .ThenInclude(n => n.HealthInsurance)

                                     .AsSplitQuery()

                                     .FirstOrDefaultAsync(wc => wc.TcmClient_FK == idTCMClient );
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (intakeForm != null)
            {
                stream = _reportHelper.TCMIntakeFormReport(intakeForm);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Intake Form";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 1/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMConsentTreamentPrint(int idTCMClient)
        {
            TCMIntakeConsentForTreatmentEntity consentForTreament = new TCMIntakeConsentForTreatmentEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                consentForTreament = await db.TCMIntakeConsentForTreatment
                                             .Include(n => n.TcmClient)
                                             .ThenInclude(n => n.Casemanager)
                                             .ThenInclude(n => n.Clinic)
                                             .Include(n => n.TcmClient)
                                             .ThenInclude(n => n.Client)
                                             .AsSplitQuery()

                                             .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (consentForTreament != null)
            {
                stream = _reportHelper.TCMIntakeConsentForTreatmentReport(consentForTreament);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Consent for Treament";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 1/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMConsentReleasePrint(int idTCMClient)
        {
            List<TCMIntakeConsentForReleaseEntity> consentForRelease = new List<TCMIntakeConsentForReleaseEntity>();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                consentForRelease = await db.TCMIntakeConsentForRelease
                                             .Include(n => n.TcmClient)
                                             .ThenInclude(n => n.Casemanager)
                                             .ThenInclude(n => n.Clinic)
                                             .Include(n => n.TcmClient)
                                             .ThenInclude(n => n.Client)
                                             .AsSplitQuery()

                                             .Where(wc => wc.TcmClient.Id == idTCMClient)
                                             .ToListAsync();
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (consentForRelease.Count() > 0)
            {
                foreach (var item in consentForRelease)
                {
                    stream = _reportHelper.TCMIntakeConsentForRelease(item);
                    //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                    string name = "Consent for Release";
                    fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 1/{name}.pdf"));
                }
                
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMConsumerRightsPrint(int idTCMClient)
        {
            TCMIntakeConsumerRightsEntity consumerRight = new TCMIntakeConsumerRightsEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                consumerRight = await db.TCMIntakeConsumerRights
                                        .Include(n => n.TcmClient)
                                        .ThenInclude(n => n.Casemanager)
                                        .ThenInclude(n => n.Clinic)
                                        .Include(n => n.TcmClient)
                                        .ThenInclude(n => n.Client)
                                        .AsSplitQuery()

                                        .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (consumerRight != null)
            {
                stream = _reportHelper.TCMIntakeConsumerRights(consumerRight);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Consumer Rights";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 1/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMAcknowledgementHippaPrint(int idTCMClient)
        {
            TCMIntakeAcknowledgementHippaEntity acknoledgement = new TCMIntakeAcknowledgementHippaEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                acknoledgement = await db.TCMIntakeAcknowledgement
                                         .Include(n => n.TcmClient)
                                         .ThenInclude(n => n.Casemanager)
                                         .ThenInclude(n => n.Clinic)
                                         .Include(n => n.TcmClient)
                                         .ThenInclude(n => n.Client)
                                         .AsSplitQuery()

                                         .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (acknoledgement != null)
            {
                stream = _reportHelper.TCMIntakeAcknowledgementHippa(acknoledgement);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Acknowledgement Hippa";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 1/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMOrientationCheckListPrint(int idTCMClient)
        {
            TCMIntakeOrientationChecklistEntity orientactionChechList = new TCMIntakeOrientationChecklistEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                orientactionChechList = await db.TCMIntakeOrientationCheckList
                                                .Include(n => n.TcmClient)
                                                .ThenInclude(n => n.Casemanager)
                                                .ThenInclude(n => n.Clinic)
                                                .Include(n => n.TcmClient)
                                                .ThenInclude(n => n.Client)
                                                .AsSplitQuery()

                                                .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (orientactionChechList != null)
            {
                stream = _reportHelper.TCMIntakeOrientationCheckList(orientactionChechList);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Orientation Check List";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 1/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMAdvancedDirectivePrint(int idTCMClient)
        {
            TCMIntakeAdvancedDirectiveEntity advancedDirective = new TCMIntakeAdvancedDirectiveEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                advancedDirective = await db.TCMIntakeAdvancedDirective
                                            .Include(n => n.TcmClient)
                                            .ThenInclude(n => n.Casemanager)
                                            .ThenInclude(n => n.Clinic)
                                            .Include(n => n.TcmClient)
                                            .ThenInclude(n => n.Client)
                                            .AsSplitQuery()

                                            .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (advancedDirective != null)
            {
                stream = _reportHelper.TCMIntakeAdvancedDirective(advancedDirective);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Advanced Directive";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 1/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMForeignLanguagePrint(int idTCMClient)
        {
            TCMIntakeForeignLanguageEntity foreignLanguage = new TCMIntakeForeignLanguageEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                foreignLanguage = await db.TCMIntakeForeignLanguage
                                          .Include(n => n.TcmClient)
                                          .ThenInclude(n => n.Casemanager)
                                          .ThenInclude(n => n.Clinic)
                                          .Include(n => n.TcmClient)
                                          .ThenInclude(n => n.Client)
                                          .AsSplitQuery()

                                          .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (foreignLanguage != null)
            {
                stream = _reportHelper.TCMIntakeForeignLanguage(foreignLanguage);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Foreign Language";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 1/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMWelcomePrint(int idTCMClient)
        {
            TCMIntakeWelcomeEntity welcome = new TCMIntakeWelcomeEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                welcome = await db.TCMIntakeWelcome
                                  .Include(n => n.TcmClient)
                                  .ThenInclude(n => n.Casemanager)
                                  .ThenInclude(n => n.Clinic)
                                  .Include(n => n.TcmClient)
                                  .ThenInclude(n => n.Client)
                                  .AsSplitQuery()

                                  .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (welcome != null)
            {
                stream = _reportHelper.TCMIntakeWelcome(welcome);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Correspondenc";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 1/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMClientSignaturePrint(int idTCMClient)
        {
            TCMIntakeClientSignatureVerificationEntity clientSignaure = new TCMIntakeClientSignatureVerificationEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                clientSignaure = await db.TCMIntakeClientSignatureVerification
                                         .Include(n => n.TcmClient)
                                         .ThenInclude(n => n.Casemanager)
                                         .ThenInclude(n => n.Clinic)
                                         .Include(n => n.TcmClient)
                                         .ThenInclude(n => n.Client)
                                         .AsSplitQuery()

                                         .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (clientSignaure != null)
            {
                stream = _reportHelper.TCMIntakeClientSignatureVerification(clientSignaure);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Client Signature Verification Form";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 1/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMCIdDocumentsPrint(int idTCMClient)
        {
            TCMIntakeClientIdDocumentVerificationEntity idDocuments = new TCMIntakeClientIdDocumentVerificationEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                idDocuments = await db.TCMIntakeClientDocumentVerification
                                      .Include(n => n.TcmClient)
                                      .ThenInclude(n => n.Casemanager)
                                      .ThenInclude(n => n.Clinic)
                                      .Include(n => n.TcmClient)
                                      .ThenInclude(n => n.Client)
                                      .AsSplitQuery()

                                      .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (idDocuments != null)
            {
                stream = _reportHelper.TCMIntakeClientDocumentVerification(idDocuments);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Id Documents Verification Form";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 1/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMCNutricionalPrint(int idTCMClient)
        {
            TCMIntakeNutritionalScreenEntity nutritional = new TCMIntakeNutritionalScreenEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                nutritional = await db.TCMIntakeNutritionalScreen
                                      .Include(n => n.TcmClient)
                                      .ThenInclude(n => n.Casemanager)
                                      .ThenInclude(n => n.Clinic)
                                      .Include(n => n.TcmClient)
                                      .ThenInclude(n => n.Client)
                                      .AsSplitQuery()

                                      .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (nutritional != null)
            {
                stream = _reportHelper.TCMIntakeNutritionalScreen(nutritional);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Nutritional Screen";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 1/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMPersonalPrint(int idTCMClient)
        {
            TCMIntakePersonalWellbeingEntity personal = new TCMIntakePersonalWellbeingEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                personal = await db.TCMIntakePersonalWellbeing
                                   .Include(n => n.TcmClient)
                                   .ThenInclude(n => n.Casemanager)
                                   .ThenInclude(n => n.Clinic)
                                   .Include(n => n.TcmClient)
                                   .ThenInclude(n => n.Client)
                                   .AsSplitQuery()

                                   .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (personal != null)
            {
                stream = _reportHelper.TCMIntakePersonalWellbeing(personal);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Personal Wellbeing";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 1/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMColumbPrint(int idTCMClient)
        {
            TCMIntakeColumbiaSuicideEntity columb = new TCMIntakeColumbiaSuicideEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                columb = await db.TCMIntakeColumbiaSuicide
                                 .Include(n => n.TcmClient)
                                 .ThenInclude(n => n.Casemanager)
                                 .ThenInclude(n => n.Clinic)
                                 .Include(n => n.TcmClient)
                                 .ThenInclude(n => n.Client)
                                 .AsSplitQuery()

                                 .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (columb != null)
            {
                stream = _reportHelper.TCMIntakeColumbiaSuicide(columb);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Columb-Suicide";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 1/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMPainScreenPrint(int idTCMClient)
        {
            TCMIntakePainScreenEntity pain = new TCMIntakePainScreenEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                pain = await db.TCMIntakePainScreen
                               .Include(n => n.TcmClient)
                               .ThenInclude(n => n.Casemanager)
                               .ThenInclude(n => n.Clinic)
                               .Include(n => n.TcmClient)
                               .ThenInclude(n => n.Client)
                               .AsSplitQuery()

                               .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (pain != null)
            {
                stream = _reportHelper.TCMIntakePainScreen(pain);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Pain Screen";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 1/{name}.pdf"));
            }

            return fileContentList;
        }

        public async Task<IActionResult> DownloadTCMIntakeSection4Simultaneous(int id)
        {
            TCMClientEntity TCMclient = await _context.TCMClient
                                                      .Include(n => n.Client)
                                                      .Include(n => n.Casemanager)
                                                      .ThenInclude(n => n.Clinic)
                                                      .Include(n => n.TCMAssessment)
                                                      .Include(n => n.TcmIntakeAppendixI)
                                                      .Include(n => n.TcmIntakeAppendixJ)
                                                      .Include(n => n.TcmServicePlan)
                                                      .ThenInclude(n => n.TCMAdendum)
                                                      .Include(n => n.TcmServicePlan)
                                                      .ThenInclude(n => n.TCMServicePlanReview)
                                                      .Include(n => n.TcmServicePlan)
                                                      .ThenInclude(n => n.TCMDischarge)
                                                      .FirstOrDefaultAsync(c => c.Id == id);

            if (TCMclient == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Task<List<FileContentResult>> fileContent1Task = TCMAssessmentPrint(id);
            Task<List<FileContentResult>> fileContent1Task1 = TCMAppendixIPrint(id);
            Task<List<FileContentResult>> fileContent1Task2 = TCMAppendixJPrint(id);
            Task<List<FileContentResult>> fileContent1Task3 = TCMServicePlanPrint(id);
            Task<List<FileContentResult>> fileContent1Task4 = TCMAddendumPrint(id);
            Task<List<FileContentResult>> fileContent1Task5 = TCMServicePlanReviewPrint(id);
            Task<List<FileContentResult>> fileContent1Task6 = TCMDischargePrint(id);



            await Task.WhenAll(fileContent1Task, fileContent1Task1, fileContent1Task2, fileContent1Task3, fileContent1Task4, fileContent1Task5, fileContent1Task6);
            var fileContent = await fileContent1Task;
            var fileContent1 = await fileContent1Task1;
            var fileContent2 = await fileContent1Task2;
            var fileContent3 = await fileContent1Task3;
            var fileContent4 = await fileContent1Task4;
            var fileContent5 = await fileContent1Task5;
            var fileContent6 = await fileContent1Task6;

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            fileContentList.AddRange(fileContent);
            fileContentList.AddRange(fileContent1);
            fileContentList.AddRange(fileContent2);
            fileContentList.AddRange(fileContent3);
            fileContentList.AddRange(fileContent4);
            fileContentList.AddRange(fileContent5);
            fileContentList.AddRange(fileContent6);
          

            return File(_fileHelper.Zip(fileContentList), "application/zip", $"{TCMclient.Client.Name}_Documents.zip");
        }

        private async Task<List<FileContentResult>> TCMAssessmentPrint(int idTCMClient)
        {
            TCMAssessmentEntity assessment = new TCMAssessmentEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                assessment = await db.TCMAssessment 
                                     .Include(n => n.TcmClient)
                                     .ThenInclude(n => n.Casemanager)
                                     .ThenInclude(n => n.Clinic)
                                     .Include(n => n.TcmClient)
                                     .ThenInclude(n => n.Client)
                                     .Include(n => n.TcmClient)
                                     .ThenInclude(n => n.Client)
                                     .ThenInclude(n => n.Client_Referred)
                                     .ThenInclude(n => n.Referred)
                                     .Include(n => n.TcmClient)
                                     .ThenInclude(n => n.Client)
                                     .ThenInclude(n => n.Clients_Diagnostics)
                                     .ThenInclude(n => n.Diagnostic)
                                     .Include(n => n.DrugList)
                                     .Include(n => n.HospitalList)
                                     .Include(n => n.HouseCompositionList)
                                     .Include(n => n.IndividualAgencyList)
                                     .Include(n => n.MedicalProblemList)
                                     .Include(n => n.MedicationList)
                                     .Include(n => n.PastCurrentServiceList)
                                     .Include(n => n.SurgeryList)
                                     .Include(n => n.TCMSupervisor)
                                     .Include(n => n.TcmClient)
                                     .ThenInclude(n => n.Client)
                                     .ThenInclude(n => n.Psychiatrist)
                                     .Include(n => n.TcmClient)
                                     .ThenInclude(n => n.Client)
                                     .ThenInclude(n => n.Doctor)
                                     
                                     .AsSplitQuery()

                                     .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient
                                                             && wc.Approved == 2);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (assessment != null)
            {
                stream = _reportHelper.TCMIntakeAssessment(assessment);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Assessment";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 4/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMAppendixIPrint(int idTCMClient)
        {
            TCMIntakeAppendixIEntity appendix = new TCMIntakeAppendixIEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                appendix = await db.TCMIntakeAppendixI
                                   .Include(n => n.TcmClient)
                                   .ThenInclude(n => n.Casemanager)
                                   .ThenInclude(n => n.Clinic)
                                   .Include(n => n.TcmClient)
                                   .ThenInclude(n => n.Client)
                                   .AsSplitQuery()

                                   .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient
                                                           && wc.Approved == 2);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (appendix != null)
            {
                //stream = _reportHelper.TCMIntakeAppendixJ(appendix);
                
                //string name = "Appendix I";
                //fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Intake/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMAppendixJPrint(int idTCMClient)
        {
            TCMIntakeAppendixJEntity appendix = new TCMIntakeAppendixJEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                appendix = await db.TCMIntakeAppendixJ
                                   .Include(n => n.TcmClient)
                                   .ThenInclude(n => n.Casemanager)
                                   .ThenInclude(n => n.Clinic)
                                   .Include(n => n.TcmClient)
                                   .ThenInclude(n => n.Client)
                                   .Include(n => n.TcmSupervisor)
                                   .AsSplitQuery()

                                   .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient 
                                                           && wc.Approved == 2);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (appendix != null)
            {
                stream = _reportHelper.TCMIntakeAppendixJ(appendix);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Appendix J";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 4/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMServicePlanPrint(int idTCMClient)
        {
            TCMServicePlanEntity serviceplan = new TCMServicePlanEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                serviceplan = await db.TCMServicePlans
                                      .Include(n => n.TcmClient)
                                      .ThenInclude(n => n.Casemanager)
                                      .ThenInclude(n => n.Clinic)
                                      .Include(n => n.TcmClient)
                                      .ThenInclude(n => n.Client)
                                      .ThenInclude(n => n.Clinic)
                                      .Include(n => n.TcmClient)
                                      .ThenInclude(n => n.Client)
                                      .ThenInclude(n => n.Clients_Diagnostics)
                                      .ThenInclude(n => n.Diagnostic)
                                      .Include(n => n.TCMDomain)
                                      .ThenInclude(n => n.TCMObjetive)
                                      .Include(n => n.TCMSupervisor)
                                      .AsSplitQuery()

                                      .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient
                                                              && wc.Approved == 2);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (serviceplan != null)
            {
                if (serviceplan.TcmClient.Client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
                {
                    stream = _reportHelper.FloridaSocialHSTCMServicePlan(serviceplan);
                    //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                    string name = "Service Plan";
                    fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 4/{name}.pdf"));
                }
                if (serviceplan.TcmClient.Client.Clinic.Name == "SAPPHIRE MENTAL HEALTH CENTER LLC")
                {
                    stream = _reportHelper.SapphireMHCTCMServicePlan(serviceplan);
                    //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                    string name = "Service Plan";
                    fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 4/{name}.pdf"));
                }
                if (serviceplan.TcmClient.Client.Clinic.Name == "ORION MENTAL HEALTH CENTER LLC")
                {
                    stream = _reportHelper.OrionMHCTCMServicePlan(serviceplan);
                    //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                    string name = "Service Plan";
                    fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 4/{name}.pdf"));
                }
                if (serviceplan.TcmClient.Client.Clinic.Name == "MY FLORIDA CASE MANAGEMENT SERVICES LLC")
                {
                    stream = _reportHelper.MyFloridaTCMServicePlan(serviceplan);
                    //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                    string name = "Service Plan";
                    fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 4/{name}.pdf"));
                }
                if (serviceplan.TcmClient.Client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
                {
                    stream = _reportHelper.CommunityHTCTCMServicePlan(serviceplan);
                    //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                    string name = "Service Plan";
                    fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 4/{name}.pdf"));
                }
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMAddendumPrint(int idTCMClient)
        {
            List<TCMAdendumEntity> addendumerviceplanList = new List<TCMAdendumEntity>();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                addendumerviceplanList = await db.TCMAdendums
                                                 .Include(n => n.TcmServicePlan)
                                                 .ThenInclude(n => n.TcmClient)
                                                 .ThenInclude(n => n.Casemanager)
                                                 .ThenInclude(n => n.Clinic)
                                                 .Include(n => n.TcmServicePlan)
                                                 .ThenInclude(n => n.TcmClient)
                                                 .ThenInclude(n => n.Client)
                                                 .Include(n => n.TcmDomain)
                                                 .ThenInclude(n => n.TCMObjetive)
                                                 .Include(n => n.TcmServicePlan)
                                                 .ThenInclude(n => n.TCMSupervisor)
                                                 .AsSplitQuery()
                                                 
                                                 .Where(wc => wc.TcmServicePlan.TcmClient_FK == idTCMClient
                                                           && wc.Approved == 2)
                                                 .ToListAsync();
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (addendumerviceplanList.Count() > 0)
            {
                foreach (var item in addendumerviceplanList)
                {
                    stream = _reportHelper.TCMAdendum(item);
                    //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                    string name = "Addendum";
                    fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 4/{name}.pdf"));
                }               
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMServicePlanReviewPrint(int idTCMClient)
        {
            TCMServicePlanReviewEntity serviceplanR = new TCMServicePlanReviewEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                serviceplanR = await db.TCMServicePlanReviews
                                       .Include(n => n.TcmServicePlan)
                                       .ThenInclude(n => n.TcmClient)
                                       .ThenInclude(n => n.Casemanager)
                                       
                                       .Include(n => n.TcmServicePlan)
                                       .ThenInclude(n => n.TcmClient)
                                       .ThenInclude(n => n.Client)
                                       .ThenInclude(n => n.Clients_Diagnostics)
                                       .ThenInclude(n => n.Diagnostic)

                                       .Include(n => n.TcmServicePlan)
                                       .ThenInclude(n => n.TcmClient)
                                       .ThenInclude(n => n.Client)
                                       .ThenInclude(c => c.LegalGuardian)

                                       .Include(n => n.TCMSupervisor)
                                       .ThenInclude(n => n.Clinic)

                                       .Include(n => n.TCMServicePlanRevDomain)
                                       .ThenInclude(n => n.TCMServicePlanRevDomainObjectiive)

                                       .AsSplitQuery()

                                       .FirstOrDefaultAsync(wc => wc.TcmServicePlan.TcmClient.Id == idTCMClient
                                                               && wc.Approved == 2);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (serviceplanR != null)
            {
                stream = _reportHelper.TCMServicePlanReview(serviceplanR);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Service Plan Review";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 4/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMDischargePrint(int idTCMClient)
        {
            TCMDischargeEntity discharge = new TCMDischargeEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                discharge = await db.TCMDischarge
                                    .Include(n => n.TcmServicePlan)
                                    .ThenInclude(n => n.TcmClient)
                                    .ThenInclude(n => n.Casemanager)
                                    .ThenInclude(n => n.Clinic)
                                    .Include(n => n.TcmServicePlan)
                                    .ThenInclude(n => n.TcmClient)
                                    .ThenInclude(n => n.Client)
                                    .Include(n => n.TcmDischargeFollowUp)
                                    .Include(n => n.TcmDischargeServiceStatus)
                                    .Include(n => n.TCMSupervisor)
                                    .Include(n => n.TcmServicePlan)
                                    .ThenInclude(n => n.TCMDomain)
                                    .AsSplitQuery()

                                    .FirstOrDefaultAsync(wc => wc.TcmServicePlan.TcmClient.Id == idTCMClient 
                                                            && wc.Approved == 2);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (discharge != null)
            {
                stream = _reportHelper.TCMDischarge(discharge);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "Discharge";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 4/{name}.pdf"));
            }

            return fileContentList;
        }

        public async Task<IActionResult> DownloadTCMIntakeSection5Simultaneous(int id)
        {
            TCMClientEntity TCMclient = await _context.TCMClient
                                                      .Include(n => n.Client)
                                                      .Include(n => n.Casemanager)
                                                      .ThenInclude(n => n.Clinic)
                                                      .Include(n => n.TCMFarsFormList)
                                                      .AsSplitQuery()
                                                      .FirstOrDefaultAsync(c => c.Id == id);

            if (TCMclient == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Task<List<FileContentResult>> fileContent1Task = TCMInterventionLogPrint(id);
            Task<List<FileContentResult>> fileContent1Task1 = TCMFarsPrint(id);
           



            await Task.WhenAll(fileContent1Task, fileContent1Task1);
            var fileContent = await fileContent1Task;
            var fileContent1 = await fileContent1Task1;
          

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            fileContentList.AddRange(fileContent);
            fileContentList.AddRange(fileContent1);
          
            return File(_fileHelper.Zip(fileContentList), "application/zip", $"{TCMclient.Client.Name}_Documents.zip");
        }

        private async Task<List<FileContentResult>> TCMInterventionLogPrint(int idTCMClient)
        {
            TCMIntakeInterventionLogEntity interventionLog = new TCMIntakeInterventionLogEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                interventionLog = await db.TCMIntakeInterventionLog
                                          .Include(n => n.TcmClient)

                                          .Include(n => n.TcmClient)
                                          .ThenInclude(n => n.Casemanager)
                                          .ThenInclude(n => n.Clinic)

                                          .AsSplitQuery()

                                          .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (interventionLog != null)
            {
                stream = _reportHelper.TCMIntakeInterventionLog(interventionLog);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "INTERVENTION LOG";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 5/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMFarsPrint(int idTCMClient)
        {
            List<TCMFarsFormEntity> farsList = new List<TCMFarsFormEntity>();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                farsList = await db.TCMFarsForm
                                   .Include(n => n.TCMClient)
                                   .ThenInclude(n => n.Casemanager)
                                   .ThenInclude(n => n.Clinic)
                                   .Include(n => n.TCMClient)
                                   .ThenInclude(n => n.Client)
                                   .ThenInclude(n => n.Clinic)
                                   .AsSplitQuery()

                                   .Where(wc => wc.TCMClient.Id == idTCMClient
                                             && wc.Status == FarsStatus.Approved)
                                   .ToListAsync();
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (farsList.Count() > 0)
            {
                foreach (var item in farsList)
                {
                    if (item.TCMClient.Client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
                    {
                        stream = _reportHelper.TCMFloridaSocialHSFarsReport(item);
                        //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                        string name = "FARS FORM";
                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 4/{name}.pdf"));
                    }
                    if (item.TCMClient.Client.Clinic.Name == "SAPPHIRE MENTAL HEALTH CENTER LLC")
                    {
                        stream = _reportHelper.TCMSapphireMHCFarsReport(item);
                        //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                        string name = "FARS FORM";
                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 4/{name}.pdf"));
                    }
                    if (item.TCMClient.Client.Clinic.Name == "ORION MENTAL HEALTH CENTER LLC")
                    {
                        stream = _reportHelper.TCMOrionMHCFarsReport(item);
                        //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                        string name = "FARS FORM";
                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 4/{name}.pdf"));
                    }
                    if (item.TCMClient.Client.Clinic.Name == "MY FLORIDA CASE MANAGEMENT SERVICES LLC")
                    {
                        stream = _reportHelper.TCMMyFloridaFarsReport(item);
                        //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                        string name = "FARS FORM";
                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 4/{name}.pdf"));
                    }
                    if (item.TCMClient.Client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
                    {
                        stream = _reportHelper.TCMCommunityHTCFarsReport(item);
                        //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                        string name = "FARS FORM";
                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 4/{name}.pdf"));
                    }
                }
            }

            return fileContentList;
        }

        public async Task<IActionResult> DownloadTCMIntakeSection2Simultaneous(int id)
        {
            TCMClientEntity TCMclient = await _context.TCMClient
                                                      .Include(n => n.Client)
                                                      .Include(n => n.Casemanager)
                                                      .ThenInclude(n => n.Clinic)
                                                      .ThenInclude(n => n.Setting)
                                                      .Include(n => n.Client)
                                                      .ThenInclude(n => n.Documents)
                                                      .Include(n => n.Client)
                                                      .ThenInclude(n => n.IntakeFeeAgreement)
                                                      .AsSplitQuery()

                                                      .FirstOrDefaultAsync(c => c.Id == id);

            if (TCMclient == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Task<List<FileContentResult>> fileContent1Task = TCMDocumentsPrint(id);
            //Task<List<FileContentResult>> fileContent1Task1 = TCMClientFeeAgreementPrint(id);

            await Task.WhenAll(fileContent1Task/*, fileContent1Task1*/);
            var fileContent = await fileContent1Task;
            //var fileContent1 = await fileContent1Task1;


            List<FileContentResult> fileContentList = new List<FileContentResult>();
            fileContentList.AddRange(fileContent);
            //fileContentList.AddRange(fileContent1);


            return File(_fileHelper.Zip(fileContentList), "application/zip", $"{TCMclient.Client.Name}_Documents.zip");
        }

        private async Task<List<FileContentResult>> TCMDocumentsPrint(int idTCMClient)
        {
            List<DocumentEntity> documents = new List<DocumentEntity>();
            TCMClientEntity tcmClient = new TCMClientEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                tcmClient = await db.TCMClient
                                    .Include(n => n.Client)
                                    .ThenInclude(n => n.Documents)
                                    .AsSplitQuery()

                                    .FirstOrDefaultAsync(wc => wc.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            //Stream stream = null;

            if (tcmClient != null)
            {
                if (tcmClient.Client.Documents.Count() > 0)
                {
                    foreach (var item in tcmClient.Client.Documents)
                    {
                        try
                        {
                            string path;
                            path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(item.FileUrl)}");
                            fileContentList.Add(File(_fileHelper.FileToByteArray(path), _mimeType.GetMimeType(item.FileName), $"Section 4/{item.FileName}.pdf"));
                        }
                        finally { }
                      
                    }                    
                }                
            }

            return fileContentList;
        }

        public async Task<IActionResult> DownloadTCMIntakeSection3Simultaneous(int id)
        {
            TCMClientEntity TCMclient = await _context.TCMClient
                                                      .Include(n => n.Client)
                                                      .Include(n => n.Casemanager)
                                                      .ThenInclude(n => n.Clinic)
                                                      .Include(n => n.TCMIntakeMiniMental)
                                                      .Include(n => n.TCMIntakeMedicalHistory)
                                                      .Include(n => n.TCMIntakeCoordinationCare)
                                                      .AsSplitQuery()
                                                      .FirstOrDefaultAsync(c => c.Id == id);

            if (TCMclient == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Task<List<FileContentResult>> fileContent1Task = TCMMiniMentalPrint(id);
            Task<List<FileContentResult>> fileContent1Task1 = TCMMedicalHistoryPrint(id);
            Task<List<FileContentResult>> fileContent1Task2 = TCMCoordinationCarePrint(id);
            
            await Task.WhenAll(fileContent1Task, fileContent1Task1, fileContent1Task2);
            var fileContent = await fileContent1Task;
            var fileContent1 = await fileContent1Task1;
            var fileContent2 = await fileContent1Task2;

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            fileContentList.AddRange(fileContent);
            fileContentList.AddRange(fileContent1);
            fileContentList.AddRange(fileContent2);


            return File(_fileHelper.Zip(fileContentList), "application/zip", $"{TCMclient.Client.Name}_Documents.zip");
        }

        private async Task<List<FileContentResult>> TCMMiniMentalPrint(int idTCMClient)
        {
            TCMIntakeMiniMentalEntity minimental = new TCMIntakeMiniMentalEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                minimental = await db.TCMIntakeMiniMental
                                     .Include(n => n.TcmClient)
                                     .ThenInclude(n => n.Casemanager)
                                     .ThenInclude(n => n.Clinic)
                                     .Include(n => n.TcmClient)
                                     .ThenInclude(n => n.Client)
                                                    
                                     .AsSplitQuery()

                                     .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (minimental != null)
            {
                stream = _reportHelper.TCMIntakeMiniMental(minimental);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "MINI MENTAL";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 3/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMMedicalHistoryPrint(int idTCMClient)
        {
            TCMIntakeMedicalHistoryEntity medical = new TCMIntakeMedicalHistoryEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                medical = await db.TCMIntakeMedicalHistory
                                  .Include(n => n.TCMClient)
                                  .ThenInclude(n => n.Casemanager)
                                  
                                  .Include(n => n.TCMClient)
                                  .ThenInclude(n => n.Client)
                                  .ThenInclude(n => n.Clinic)

                                  .Include(n => n.TCMClient)
                                  .ThenInclude(n => n.TCMAssessment)
                                  .ThenInclude(n => n.MedicationList)

                                  .AsSplitQuery()

                                  .FirstOrDefaultAsync(wc => wc.TCMClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (medical != null)
            {
                stream = _reportHelper.TCMIntakeMedicalHistoryReport(medical);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "MEDICAL HISTORY";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 3/{name}.pdf"));
            }

            return fileContentList;
        }

        private async Task<List<FileContentResult>> TCMCoordinationCarePrint(int idTCMClient)
        {
            TCMIntakeCoordinationCareEntity coordination = new TCMIntakeCoordinationCareEntity();

            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                coordination = await db.TCMIntakeCoordinationCare
                                       .Include(n => n.TcmClient)
                                       .ThenInclude(n => n.Casemanager)
                                       .ThenInclude(n => n.Clinic)

                                       .Include(n => n.TcmClient)
                                       .ThenInclude(n => n.Client)
                                       .ThenInclude(n => n.LegalGuardian)

                                       .Include(n => n.TcmClient)
                                       .ThenInclude(n => n.TCMIntakeForm)

                                       .AsSplitQuery()

                                       .FirstOrDefaultAsync(wc => wc.TcmClient.Id == idTCMClient);
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            if (coordination != null)
            {
                stream = _reportHelper.TCMIntakeCoordinationCare(coordination);
                //string name = tcmNote.TCMClient.Client.Name + " - " + tcmNote.DateOfService.ToShortDateString() + " - " + tcmNote.TCMNoteActivity.FirstOrDefault().ServiceName;
                string name = "COORDINATION OF CARE";
                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Section 3/{name}.pdf"));
            }

            return fileContentList;
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<IActionResult> AuthorizationClients(int idError = 0)
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

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk"))
            {
                List<TCMClientEntity> list = await _context.TCMClient
                                                           .Include(n => n.Client)
                                                           .ThenInclude(n => n.Clients_HealthInsurances)
                                                           .ThenInclude(n => n.HealthInsurance)
                                                           .Include(n => n.Casemanager)
                                                           .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id
                                                                    && n.Status == StatusType.Open)
                                                           .ToListAsync();
                List<AuthorizationViewModel> authorizations = new List<AuthorizationViewModel>();
                AuthorizationViewModel authorization = new AuthorizationViewModel();

                foreach (var item in list)
                {
                    if (item.Client.Clients_HealthInsurances.Where(n => n.Agency == ServiceAgency.TCM).Count() == 0)
                    {
                        authorization.IdClientHealthInsurance = 0;
                        authorization.IdClient = item.Client.Id;
                        authorization.IdTCMClient = item.Id;
                        authorization.TCMClientName = item.Client.Name;
                        authorization.CaseManagerName = item.Casemanager.Name;
                        authorization.HealthInsurance = "Empty";
                        authorization.Status = item.Status;
                        authorization.DateOpen = item.DataOpen;
                        authorization.Agency = "TCM";
                        authorization.Info = 0;

                        authorizations.Add(authorization);
                        authorization = new AuthorizationViewModel();
                    }
                    else
                    {
                        if (item.Client.Clients_HealthInsurances.Where(n => n.Agency == ServiceAgency.TCM && n.Active == true).Count() > 0)
                        {
                            foreach (var item1 in item.Client.Clients_HealthInsurances.Where(n => n.Agency == ServiceAgency.TCM && n.Active == true && n.HealthInsurance.NeedAuthorization == true))
                            {
                                if (item1.ExpiredDate.Date < DateTime.Today.Date)
                                {
                                    authorization.IdClientHealthInsurance = 0;
                                    authorization.IdClient = item.Client.Id;
                                    authorization.IdTCMClient = item.Id;
                                    authorization.TCMClientName = item.Client.Name;
                                    authorization.CaseManagerName = item.Casemanager.Name;
                                    authorization.HealthInsurance = item1.HealthInsurance.Name;
                                    authorization.Status = item.Status;
                                    authorization.DateOpen = item.DataOpen;
                                    authorization.Agency = "TCM";
                                    authorization.Info = 0;
                                    authorization.ExpiratedDate = item1.ExpiredDate;
                                    authorization.EffectiveDate = item1.ApprovedDate;

                                    authorizations.Add(authorization);
                                    authorization = new AuthorizationViewModel();
                                }
                                else
                                {
                                    if (item1.ExpiredDate.Date <= DateTime.Today.Date.AddDays(30))
                                    {
                                        authorization.IdClientHealthInsurance = 0;
                                        authorization.IdClient = item.Client.Id;
                                        authorization.IdTCMClient = item.Id;
                                        authorization.TCMClientName = item.Client.Name;
                                        authorization.CaseManagerName = item.Casemanager.Name;
                                        authorization.HealthInsurance = item1.HealthInsurance.Name;
                                        authorization.Status = item.Status;
                                        authorization.DateOpen = item.DataOpen;
                                        authorization.Agency = "TCM";
                                        authorization.Info = 1;
                                        authorization.ExpiratedDate = item1.ExpiredDate;
                                        authorization.EffectiveDate = item1.ApprovedDate;

                                        authorizations.Add(authorization);
                                        authorization = new AuthorizationViewModel();
                                    }
                                }
                            }
                        }
                        else
                        {
                            authorization.IdClientHealthInsurance = 0;
                            authorization.IdClient = item.Client.Id;
                            authorization.IdTCMClient = item.Id;
                            authorization.TCMClientName = item.Client.Name;
                            authorization.CaseManagerName = item.Casemanager.Name;
                            authorization.HealthInsurance = item.Casemanager.Name;
                            authorization.HealthInsurance = "Empty";
                            authorization.Status = item.Status;
                            authorization.DateOpen = item.DataOpen;
                            authorization.Agency = "TCM";
                            authorization.Info = 0;

                            authorizations.Add(authorization);
                            authorization = new AuthorizationViewModel();
                        }

                    }
                    
                }

                return View(authorizations);
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

    }
}
