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

        public TCMClientsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IReportHelper reportHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
            _renderHelper = renderHelper;
        }

        [Authorize(Roles = "Manager, CaseManager, TCMSupervisor")]
        public async Task<IActionResult> Index()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.UserType.ToString() == "CaseManager")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                CaseMannagerEntity caseManager = await _context.CaseManagers
                                                               .FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);

                return View(await _context.TCMClient
                                          .Include(g => g.Casemanager)
                                          .Include(g => g.Client)
                                          .Where(g => (g.Casemanager.Id == caseManager.Id))
                                          .OrderBy(g => g.Client.Name)
                                          .ToListAsync());
            }

            if (user_logged.UserType.ToString() == "Manager" || user_logged.UserType.ToString() == "TCMSupervisor")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }
                List<TCMClientEntity> tcmClient = await _context.TCMClient
                                                                 .Include(g => g.Casemanager)
                                                                 .Include(g => g.Client)
                                                                 .Where(s => s.Client.Clinic.Id == user_logged.Clinic.Id)
                                                                 .OrderBy(g => g.Casemanager.Name)
                                                                 .ToListAsync();
                return View(tcmClient);
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
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
                        tcmClient = await _converterHelper.ToTCMClientEntity(model, true, user_logged.UserName);
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
        public async Task<IActionResult> Edit(int? id, int error = 0, int idFacilitator = 0, int idClient = 0)
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
                return View(tcmClientViewModel);
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [HttpPost]
        [Authorize(Roles = "Manager, TCMSupervisor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TCMClientViewModel model)
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

                        TCMClientEntity tcmClient = await _converterHelper.ToTCMClientEntity(model, false, user_logged.UserName);
                        _context.TCMClient.Update(tcmClient);
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
            if (user_logged.UserType.ToString() == "Manager" || user_logged.UserType.ToString() == "TCMSupervisor")
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
            if (user_logged.UserType.ToString() == "Manager" || user_logged.UserType.ToString() == "TCMSupervisor")
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
                                       .Where(g => (g.Casemanager.Id == caseManager.Id))
                                       .OrderBy(g => g.Client.Name)
                                       .ToListAsync();
                TCMServicePlanEntity servicePlan = null;
                List<TCMClientEntity> tcmClientsTemp = await _context.TCMClient
                                       .Include(g => g.Casemanager)
                                       .Include(g => g.Client)
                                       .Where(g => (g.Casemanager.Id == caseManager.Id))
                                       .OrderBy(g => g.Client.Name)
                                       .ToListAsync();
                for (int i = 0; i < tcmClientsT.Count(); i++)
                {
                    servicePlan = await _context.TCMServicePlans.FirstOrDefaultAsync(c => c.TcmClient.CaseNumber == tcmClientsT[i].CaseNumber);
                    if (servicePlan != null)
                    {
                        tcmClientsTemp.Remove(tcmClientsT[i]);
                    }
                }


                return View(tcmClientsTemp);
            }
            if (user_logged.UserType.ToString() == "Manager" || user_logged.UserType.ToString() == "TCMSupervisor")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }
                List<TCMClientEntity> tcmClients = await _context.TCMClient
                                                          .Include(g => g.Casemanager)
                                                          .Include(g => g.Client)
                                                          .Where(s => (s.Client.Clinic.Id == user_logged.Clinic.Id))
                                                          .OrderBy(g => g.Casemanager.Name)
                                                          .ToListAsync();

                TCMServicePlanEntity servicePlan = null;
                List<TCMClientEntity> tcmClientsTemp = await _context.TCMClient
                                                          .Include(g => g.Casemanager)
                                                          .Include(g => g.Client)
                                                          .Where(s => (s.Client.Clinic.Id == user_logged.Clinic.Id))
                                                          .OrderBy(g => g.Casemanager.Name)
                                                          .ToListAsync();
                for (int i = 0; i < tcmClients.Count(); i++)
                {
                    servicePlan = await _context.TCMServicePlans.FirstOrDefaultAsync(c => c.TcmClient.CaseNumber == tcmClients[i].CaseNumber);
                    if (servicePlan != null)
                    {
                        tcmClientsTemp.Remove(tcmClients[i]);
                    }
                }

                return View(tcmClientsTemp);

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

    }
}
