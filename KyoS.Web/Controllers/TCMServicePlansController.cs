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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace KyoS.Web.Controllers
{
    public class TCMServicePlansController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IReportHelper _reportHelper;
        public TCMServicePlansController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IReportHelper reportHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
            _reportHelper = reportHelper;
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> Index(int idError = 0, string caseNumber = "")
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            
            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
               return RedirectToAction("NotAuthorized", "Account");
            }
                
            ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
            CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
            
            List<TCMServicePlanEntity> servicePlan = null;
            
            if (user_logged.UserType.ToString() == "CaseManager")
            {

                if (caseNumber == "")
                {
                    servicePlan = await _context.TCMServicePlans
                                                         .Include(h => h.TCMDomain)
                                                         .ThenInclude(h => h.TCMObjetive)
                                                         .Include(g => g.TcmClient)
                                                         .ThenInclude(f => f.Client)
                                                         .Include(t => t.TcmClient.Casemanager)
                                                         .Where(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                                && g.TcmClient.Status == StatusType.Open))
                                                         .OrderBy(g => g.TcmClient.CaseNumber)
                                                         .ToListAsync();
                    ViewData["origin"] = 0;
                    return View(servicePlan);
                }
                else
                {
                    servicePlan = await _context.TCMServicePlans
                                                         .Include(h => h.TCMDomain)
                                                         .ThenInclude(h => h.TCMObjetive)
                                                         .Include(g => g.TcmClient)
                                                         .ThenInclude(f => f.Client)
                                                         .Include(t => t.TcmClient.Casemanager)
                                                         .Where(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                          && g.TcmClient.CaseNumber == caseNumber))
                                                         .OrderBy(g => g.TcmClient.CaseNumber)
                                                         .ToListAsync();

                }


            }
            if (user_logged.UserType.ToString() == "Manager")
            {
                servicePlan = await _context.TCMServicePlans
                                                        .Include(h => h.TCMDomain)
                                                        .ThenInclude(h => h.TCMObjetive)
                                                        .Include(g => g.TcmClient)
                                                        .ThenInclude(f => f.Client)
                                                        .Include(t => t.TcmClient.Casemanager)
                                                        .Where(g => (g.TcmClient.Client.Clinic.Id == clinic.Id))
                                                        .OrderBy(g => g.TcmClient.CaseNumber)
                                                        .ToListAsync();
                ViewData["origin"] = 0;
                return View(servicePlan);
            }
            if (user_logged.UserType.ToString() == "TCMSupervisor")
            {
                servicePlan = await _context.TCMServicePlans
                                                     .Include(h => h.TCMDomain)
                                                     .ThenInclude(h => h.TCMObjetive)
                                                     .Include(g => g.TcmClient)
                                                     .ThenInclude(f => f.Client)
                                                     .Include(t => t.TcmClient.Casemanager)
                                                     .Where(g => (g.TcmClient.Client.Clinic.Id == clinic.Id))
                                                     .OrderBy(g => g.TcmClient.CaseNumber)
                                                     .ToListAsync();
                ViewData["origin"] = 0;
                return View(servicePlan);
            }
            
            ViewData["origin"] = caseNumber;
            
            return View(servicePlan);

        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Create(int id, int origin = 0)
        {
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans.Include(u => u.TcmClient)
                                                         .FirstOrDefault(u => u.TcmClient.Id == id);
            TCMServicePlanViewModel model;

            if (tcmServicePlan == null)
            {
                if (User.IsInRole("CaseManager"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        List <TCMClientEntity> tcmClient = _context.TCMClient
                                                        .Include(u => u.Client)
                                                        .Where(u => u.Id == id)
                                                        .ToList();

                        List<SelectListItem> list_Client = tcmClient.Select(c => new SelectListItem
                        {
                            Text = $"{c.Client.Name}",
                            Value = $"{c.Id}"
                        })
                            .ToList();

                        List<ClinicEntity> clinic = _context.Clinics                                                         
                                                         .Where(u => u.Id == user_logged.Clinic.Id)
                                                         .ToList();

                        List<SelectListItem> list_Clinins = clinic.Select(c => new SelectListItem
                        {
                            Text = $"{c.Name}",
                            Value = $"{c.Id}"
                        })
                            .ToList();

                        model = new TCMServicePlanViewModel
                        {
                            ID_TcmClient = id,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            TcmClients = list_Client,
                            ID_Clinic = user_logged.Clinic.Id,
                            Clinics = list_Clinins,
                            ID_Status = 1,
                            status = _combosHelper.GetComboClientStatus(),
                            CaseNumber = _context.TCMClient.FirstOrDefault(u => u.Id == id).CaseNumber,
                            Date_ServicePlan = DateTime.Now.Date,
                            Date_Intake = DateTime.Now.Date,
                            Date_Assessment = DateTime.Now.Date,
                            Date_Certification = DateTime.Now.Date

                        };
                        ViewData["origin"] = origin;
                        return View(model);
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("Index", "TCMServicePlans", new { caseNumber = tcmServicePlan.TcmClient.CaseNumber });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Create(TCMServicePlanViewModel tcmServicePlanViewModel, int origin = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic) 
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMServicePlanEntity tcmServicePlanEntity = _context.TCMServicePlans
                                                                    .Include(n => n.TcmClient)
                                                                    .FirstOrDefault(n => n.TcmClient.Id == tcmServicePlanViewModel.ID_TcmClient);
                if (tcmServicePlanEntity == null)
                {
                    tcmServicePlanEntity = await _converterHelper.ToTCMServicePlanEntity(tcmServicePlanViewModel, true, user_logged.UserName);
                    _context.Add(tcmServicePlanEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (origin == 0)
                        {
                            return RedirectToAction("Index", "TCMServicePlans", new { caseNumber = tcmServicePlanEntity.TcmClient.CaseNumber });
                        }
                        else
                        {
                            return RedirectToAction("ServicePlanStarted", "TCMServicePlans", new { approved = (origin - 1) });
                        }
                        
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the TCM service.");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmServicePlanViewModel) });
                }
            }

            
           return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmServicePlanViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Edit(int Id = 0, int origin = 0)
        {
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans
                                                          .Include(f => f.TCMDomain)
                                                          .ThenInclude(f => f.TCMObjetive)
                                                          .Include(f => f.TcmClient)
                                                          .FirstOrDefault(u => u.Id == Id);
            TCMServicePlanViewModel model;

            if (tcmServicePlan != null && tcmServicePlan.Approved != 2)
            {
                if (User.IsInRole("CaseManager"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        List<TCMClientEntity> tcmClient = _context.TCMClient
                                                       .Include(u => u.Client)
                                                       .Where(u => u.Id == tcmServicePlan.TcmClient.Id)
                                                       .ToList();

                        List<SelectListItem> list_Client = tcmClient.Select(c => new SelectListItem
                        {
                            Text = $"{c.Client.Name}",
                            Value = $"{c.Id}"
                        })
                            .ToList();

                        List<ClinicEntity> clinic = _context.Clinics
                                                         .Where(u => u.Id == user_logged.Clinic.Id)
                                                         .ToList();

                        List<SelectListItem> list_Clinins = clinic.Select(c => new SelectListItem
                        {
                            Text = $"{c.Name}",
                            Value = $"{c.Id}"
                        })
                            .ToList();

                        model = new TCMServicePlanViewModel
                        {
                            ID_TcmClient = tcmServicePlan.TcmClient.Id,
                            TcmClients = list_Client,
                            ID_Clinic = user_logged.Clinic.Id,
                            Clinics = list_Clinins,
                            Status = tcmServicePlan.Status,
                            ID_Status = (tcmServicePlan.Status == StatusType.Open) ? 1 : 2,
                            status = _combosHelper.GetComboClientStatus(),
                            CaseNumber = _context.TCMClient.FirstOrDefault(u => u.Id == tcmServicePlan.TcmClient.Id).CaseNumber,
                            Date_ServicePlan = tcmServicePlan.DateServicePlan,
                            Date_Intake = tcmServicePlan.DateIntake,
                            Date_Assessment = tcmServicePlan.DateAssessment,
                            Date_Certification = tcmServicePlan.DateCertification,
                            strengths = tcmServicePlan.Strengths,
                            weakness = tcmServicePlan.Weakness,
                            dischargerCriteria = tcmServicePlan.DischargerCriteria,
                            Id = Id,
                            TCMDomain = tcmServicePlan.TCMDomain,
                            Approved = tcmServicePlan.Approved,
                            CreatedBy = tcmServicePlan.CreatedBy,
                            CreatedOn = tcmServicePlan.CreatedOn
                            
                        };

                        ViewData["origin"] = origin;
                        model.TCMDomain = model.TCMDomain.Where(n => (n.Origin == "Service Plan")).ToList();
                        return View(model);
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("Index", "TCMServicePlans", new { caseNumber = tcmServicePlan.TcmClient.CaseNumber });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Edit(int id, TCMServicePlanViewModel serviceplanViewModel, int origin = 0)
        {
            UserEntity user_logged = _context.Users
                                              .Include(u => u.Clinic)
                                              .FirstOrDefault(u => u.UserName == User.Identity.Name);

             if (ModelState.IsValid && serviceplanViewModel.Approved != 2)
             {
                 
                  TCMServicePlanEntity tcmServicePlanEntity = await _converterHelper.ToTCMServicePlanEntity(serviceplanViewModel, false, user_logged.UserName);

                List<TCMMessageEntity> messages = tcmServicePlanEntity.TCMMessages.Where(m => (m.Status == MessageStatus.NotRead && m.Notification == false)).ToList();
                //todos los mensajes no leidos que tiene el Workday_Client de la nota los pongo como leidos
                foreach (TCMMessageEntity value in messages)
                {
                    value.Status = MessageStatus.Read;
                    value.DateRead = DateTime.Now;
                    _context.Update(value);

                    //I generate a notification to supervisor
                    TCMMessageEntity notification = new TCMMessageEntity
                    {
                        TCMNote = null,
                        TCMFarsForm = null,
                        TCMServicePlan = tcmServicePlanEntity,
                        TCMServicePlanReview = null,
                        TCMAddendum = null,
                        TCMDischarge = null,
                        TCMAssessment = null,
                        Title = "Update on reviewed TCM Service plan",
                        Text = $"The TCM Service plan of {tcmServicePlanEntity.TcmClient.Client.Name} on {tcmServicePlanEntity.DateServicePlan.ToShortDateString()} was rectified",
                        From = value.To,
                        To = value.From,
                        DateCreated = DateTime.Now,
                        Status = MessageStatus.NotRead,
                        Notification = true
                    };
                    _context.Add(notification);
                }

                _context.Update(tcmServicePlanEntity);
                  try
                  {
                       await _context.SaveChangesAsync();

                    if (origin == 0)
                    {
                        return RedirectToAction("TCMIntakeSectionDashboard", "TCMIntakes", new { id = tcmServicePlanEntity.TcmClient.Id, section = 4 });
                        //return RedirectToAction("Index", "TCMServicePlans", new { caseNumber = tcmServicePlanEntity.TcmClient.CaseNumber });
                    }
                    else
                    {
                        if (origin == 3)
                        {
                            return RedirectToAction("TCMServicePlanWithReview", "TCMServicePlans");
                        }
                        else
                        {
                            if (origin == 4)
                            {
                                return RedirectToAction("MessagesOfServicePlan", "TCMMessages");
                            }
                            else
                            {
                                return RedirectToAction("ServicePlanStarted", "TCMServicePlans", new { approved = (origin - 1) });
                            }
                        }
                            
                    }
                    
                  }
                  catch (System.Exception ex)
                  {
                       ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                  }
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", serviceplanViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> FinishEditing(int id, int origin = 0)
        {
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans
                                                          .Include(u => u.TcmClient)
                                                          .ThenInclude(u => u.Client)
                                                          .FirstOrDefault(u => u.Id == id);
           
            if (tcmServicePlan != null)
            {
                if (User.IsInRole("CaseManager"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        tcmServicePlan.Approved = 1;
                        _context.Update(tcmServicePlan);
                        try
                        {
                            await _context.SaveChangesAsync();
                            if (origin == 0)
                            {
                                return RedirectToAction("TCMIntakeSectionDashboard", "TCMIntakes", new { id = tcmServicePlan.TcmClient.Id, section = 4 });
                            }
                            else
                            {
                                return RedirectToAction("ServicePlanStarted", "TCMServicePlans", new { approved = 1 });
                            }
                        }
                        catch (System.Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("Index", "TCMServicePlans");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> AproveServicePlan(int id, int origi = 0)
        {
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans.Include(u => u.TcmClient)
                                                         .ThenInclude(u => u.Client)
                                                         .FirstOrDefault(u => u.Id == id);

            if (tcmServicePlan != null)
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        tcmServicePlan.Approved = 2;
                        tcmServicePlan.TCMSupervisor = await _context.TCMSupervisors.FirstOrDefaultAsync(n => n.LinkedUser == user_logged.UserName);
                        _context.Update(tcmServicePlan);
                        try
                        {
                            await _context.SaveChangesAsync();
                            if (origi == 0)
                            {
                                return RedirectToAction("ServicePlanStarted", "TCMServicePlans", new { approved = 1 });
                            }
                            if (origi == 3)
                            {
                                return RedirectToAction("Notifications", "TCMMessages");
                            }
                        }
                        catch (System.Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("Index", "TCMServicePlans");
        }
        
        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> ApprovedServicePlan(int idError = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);


            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
            CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);

            if (user_logged.UserType.ToString() == "CaseManager")
            {

                List<TCMServicePlanEntity> servicePlan = await _context.TCMServicePlans
                                                    .Include(h => h.TCMDomain)
                                                    .ThenInclude(h => h.TCMObjetive)
                                                    .Include(g => g.TcmClient)
                                                    .ThenInclude(f => f.Client)
                                                    .Include(t => t.TcmClient.Casemanager)
                                                    .Where(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                       && g.Approved == 2))
                                                    .ToListAsync();

                return View(servicePlan);
            }
            if (user_logged.UserType.ToString() == "Manager")
            {
                List<TCMServicePlanEntity> servicePlan = await _context.TCMServicePlans
                                                     .Include(h => h.TCMDomain)
                                                     .ThenInclude(h => h.TCMObjetive)
                                                     .Include(g => g.TcmClient)
                                                     .ThenInclude(f => f.Client)
                                                     .Include(t => t.TcmClient.Casemanager)
                                                     .Where(g => (g.TcmClient.Client.Clinic.Id == clinic.Id
                                                         && g.Approved == 2))
                                                     .ToListAsync();
                return View(servicePlan);
            }
            if (user_logged.UserType.ToString() == "TCMSupervisor")
            {
                List<TCMServicePlanEntity> servicePlan = await _context.TCMServicePlans
                                                     .Include(h => h.TCMDomain)
                                                     .ThenInclude(h => h.TCMObjetive)
                                                     .Include(g => g.TcmClient)
                                                     .ThenInclude(f => f.Client)
                                                     .Include(t => t.TcmClient.Casemanager)
                                                     .Where(g => (g.TcmClient.Client.Clinic.Id == clinic.Id
                                                        && g.Approved == 2))
                                                     .ToListAsync();
                return View(servicePlan);
            }

            return View(null);

        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> EditDomain(int? id, int origi  = 0, int aview = 0, int idAddendum = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMDomainEntity tcmDomainEntity = await _context.TCMDomains
                                                            .Include(u => u.TcmServicePlan)
                                                            .ThenInclude(g => g.TcmClient)
                                                            .ThenInclude(g => g.Client)
                                                            .FirstOrDefaultAsync(s => s.Id == id);
            if (tcmDomainEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMDomainViewModel tcmDomainViewModel = null;

            if (User.IsInRole("CaseManager"))
            {
                tcmDomainViewModel = _converterHelper.ToTCMDomainViewModel(tcmDomainEntity);
               
            }
            ViewData["origi"] = origi;
            ViewData["aview"] = aview;
            ViewData["idAddendum"] = idAddendum;
            
            return View(tcmDomainViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> EditDomain(int id, TCMDomainViewModel tcmDomainViewModel, int origi=0, int aview = 0, int idAddendum = 0)
        {
            if (id != tcmDomainViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans
                                                           .Include(u => u.TcmClient)
                                                           .ThenInclude(g => g.Client)
                                                           .FirstOrDefault(h => h.Id == tcmDomainViewModel.Id_ServicePlan);

            tcmDomainViewModel.TcmServicePlan = tcmServicePlan;

            if (ModelState.IsValid)
            {
                TCMDomainEntity tcmDomainEntity = await _converterHelper.ToTCMDomainEntity(tcmDomainViewModel, false, tcmDomainViewModel.Origin, user_logged.UserName);
                _context.Update(tcmDomainEntity);

                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMDomainEntity> domainList = await _context.TCMDomains
                                                                 .Include(h => h.TCMObjetive)
                                                                 .Include(g => g.TcmServicePlan)
                                                                 .Where(g => (g.TcmServicePlan.Id == tcmDomainViewModel.Id_ServicePlan))
                                                                 .OrderBy(g => g.Code)
                                                                 .ToListAsync();
                    if (origi == 0)
                    {
                        if (aview == 0)
                        {
                            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomains", domainList) });
                        }
                        else
                        {
                            return RedirectToAction("ServicePlanStarted", "TCMServicePlans", new { approved = aview });
                            //return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomains", domainList) });
                        }
                    }
                    if (origi == 1)
                    {
                        TCMAdendumEntity tcmadendumEntity = await _context.TCMAdendums
                                                              .Include(u => u.TcmServicePlan)
                                                              .ThenInclude(u => u.TCMDomain)
                                                              .ThenInclude(g => g.TCMObjetive.Where(m => m.Origin == "Addendum"))
                                                              .Include(u => u.TcmServicePlan)
                                                              .ThenInclude(g => g.TcmClient)
                                                              .ThenInclude(g => g.Client)
                                                              .Include(u => u.TcmServicePlan)
                                                              .ThenInclude(g => g.TcmClient)
                                                              .ThenInclude(g => g.Casemanager)
                                                              .ThenInclude(g => g.Clinic)
                                                              .FirstOrDefaultAsync(s => s.Id == idAddendum);
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainsAddendum", _converterHelper.ToTCMAdendumViewModel(tcmadendumEntity)) });

                    }
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditDomain", tcmDomainViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateDomain(int id = 0, int origin = 0, int aview = 0)
        {
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans.Include(u => u.TcmClient)
                                                          .ThenInclude(u => u.Client)
                                                          .FirstOrDefault(u => u.Id == id);
            TCMDomainViewModel model = null; 

            if (tcmServicePlan != null)
            {
                if (User.IsInRole("CaseManager"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {

                        IEnumerable<SelectListItem> list_Services = _combosHelper.GetComboServicesNotUsed(tcmServicePlan.Id);

                        model = new TCMDomainViewModel
                        {
                            Date_Identified = DateTime.Today,
                            Services = list_Services,
                            TcmServicePlan = tcmServicePlan,
                            Id_ServicePlan = id,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now
                        };
                        ViewData["origin"] = origin;
                        ViewData["aview"] = aview;
                        return View(model);
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
                return RedirectToAction("NotAuthorized", "Account");
            }

            return RedirectToAction("Index", "TCMServicePlans", new { caseNumber = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateDomain(TCMDomainViewModel tcmDomainViewModel, int origin = 0, int aview = 0)
        {
            UserEntity user_logged = _context.Users
                                           .Include(u => u.Clinic)
                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans
                                                     .Include(u => u.TcmClient)
                                                     .ThenInclude(u => u.Client)
                                                     .FirstOrDefault(g => g.Id == tcmDomainViewModel.Id_ServicePlan);
            tcmDomainViewModel.TcmServicePlan = tcmServicePlan;

            if (tcmDomainViewModel.Id_Service == 0)
            {
                IEnumerable<SelectListItem> list_Services = _combosHelper.GetComboServicesNotUsed(tcmServicePlan.Id);
                TCMDomainViewModel model = tcmDomainViewModel;

                model.Services = list_Services;
                ModelState.AddModelError(string.Empty, "You must select a Service code");

                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateDomain", model) });

            }

            if (ModelState.IsValid)
            {

                TCMDomainEntity tcmDomainEntity = _context.TCMDomains
                                                          .Include(f => f.TcmServicePlan)
                                                          .FirstOrDefault(g => (g.TcmServicePlan.Id == tcmDomainViewModel.TcmServicePlan.Id
                                                             && g.Code == tcmDomainViewModel.Code));
                if (tcmDomainEntity == null)
                {
                    CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
                    tcmDomainEntity = await _converterHelper.ToTCMDomainEntity(tcmDomainViewModel, true,"Service Plan", user_logged.UserName);
                    _context.Add(tcmDomainEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMDomainEntity> domainList = await _context.TCMDomains
                                                                 .Include(h => h.TCMObjetive)
                                                                 .Include(g => g.TcmServicePlan)
                                                                 .Where(g => (g.TcmServicePlan.Id == tcmDomainViewModel.Id_ServicePlan))
                                                                 .OrderBy(g => g.Code)
                                                                 .ToListAsync();
                        if (origin == 0)
                        {
                            if (aview == 0)
                            {
                                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomains", domainList) });
                            }
                            else
                            {
                                return RedirectToAction("ServicePlanStarted", "TCMServicePlans", new { approved = aview });
                                //return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomains", domainList) });
                            }

                        }
                        if (origin == 1)
                        {

                            return RedirectToAction(nameof(Adendum));
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                   
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateDomain", tcmDomainViewModel) });
                }
            }

            
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateDomain", tcmDomainViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteDomain(int? id, int origin = 0, int aview = 0)
        {
            //TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans.Include(n => n.TcmClient).Include(n => n.TCMDomain.Where(m => m.Id == id)).FirstOrDefault();
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMDomainEntity tcmDomainEntity = await _context.TCMDomains
                                                            .Include(n => n.TCMObjetive)
                                                            .Include(n => n.TcmServicePlan)
                                                            .ThenInclude(n => n.TcmClient)
                                                            .FirstOrDefaultAsync(s => s.Id == id);
            if (tcmDomainEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMDomains.Remove(tcmDomainEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                ModelState.AddModelError(string.Empty, $"Impossible to delete this domain: {tcmDomainEntity.Code}");
                return RedirectToAction("Edit", new { id = tcmDomainEntity.TcmServicePlan.Id });
            }
            if (origin == 0)
            {
                if (aview == 0)
                {
                    return RedirectToAction("Edit", new { id = tcmDomainEntity.TcmServicePlan.Id });
                }
                else
                {
                    return RedirectToAction("ServicePlanStarted", "TCMServicePlans", new { approved = aview });
                    //return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomains", domainList) });
                }

            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateObjetive(int id = 0, int Origin = 0, int idAddendum = 0)
        {
           
            TCMObjetiveViewModel model = null;
            TCMDomainEntity tcmdomain = await _context.TCMDomains
                                                .Include(g => g.TCMObjetive)
                                                .Include(h => h.TcmServicePlan)
                                                .ThenInclude(h => h.TCMService)
                                                .ThenInclude(h => h.Stages)
                                                .FirstOrDefaultAsync(m => m.Id == id);

            if (User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    model = new TCMObjetiveViewModel
                    {
                        TcmDomain = tcmdomain,
                        Id = 0,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        Id_Stage = 0,
                        Stages = _combosHelper.GetComboStagesNotUsed(tcmdomain),
                        Id_Domain = tcmdomain.Id,
                        ID_Objetive = tcmdomain.TCMObjetive.Count() + 1,
                        Start_Date = DateTime.Today.Date,
                        Target_Date = DateTime.Today.Date,
                        End_Date = DateTime.Today.Date,
                        task = "es para que veas el problema del textarea",
                        Origi = Origin,
                        Origin = ""
                    };

                    ViewData["idAddendum"] = idAddendum;
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateObjetive(TCMObjetiveViewModel tcmObjetiveViewModel, int Origin, int idAddendum = 0)
        {
            UserEntity user_logged = await _context.Users

                                                 .Include(u => u.Clinic)
                                                 .ThenInclude(c => c.Setting)

                                                 .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (User.IsInRole("CaseManager"))
            {

                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                TCMDomainEntity tcmDomain = _context.TCMDomains
                                                        .Include(f => f.TCMObjetive)
                                                        .Include(f => f.TcmServicePlan)
                                                        .ThenInclude(f => f.TcmClient)
                                                        .FirstOrDefault(f => f.Id == tcmObjetiveViewModel.Id_Domain);
                TCMObjetiveEntity tcmObjetiveEntity = await _converterHelper.ToTCMObjetiveEntity(tcmObjetiveViewModel, true, Origin, user_logged.UserName);
                tcmObjetiveEntity.TcmDomain = tcmDomain;

                if (ModelState.IsValid)
                {

                    _context.Add(tcmObjetiveEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                       
                        if (Origin == 0)
                        {
                            List<TCMDomainEntity> domainList = await _context.TCMDomains
                                                                        .Include(h => h.TCMObjetive)
                                                                        .Where(g => (g.TcmServicePlan.Id == tcmDomain.TcmServicePlan.Id))
                                                                        .OrderBy(g => g.Code)
                                                                        .ToListAsync();
                            
                            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomains", domainList) });

                        }
                        if (Origin == 1)
                        {
                            TCMAdendumEntity tcAddendumEntity = await _context.TCMAdendums
                                                                               .Include(n => n.TcmDomain)
                                                                               .ThenInclude(g => g.TCMObjetive.Where(m => m.Origin == "Addendum"))
                                                                               .FirstOrDefaultAsync(s => s.Id == idAddendum);

                            ViewData["idAddendum"] = idAddendum;
                            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainsAddendum", tcAddendumEntity.TcmDomain) });
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    TCMDomainEntity tcmdomain = await _context.TCMDomains
                                                   .Include(g => g.TCMObjetive)
                                                   .Include(g => g.TcmServicePlan)
                                                   .FirstOrDefaultAsync(m => m.Id == tcmObjetiveViewModel.Id_Domain);
                    tcmObjetiveViewModel.TcmDomain = tcmdomain;
                    tcmObjetiveViewModel.Id_Stage = 0;
                    tcmObjetiveViewModel.Stages = _combosHelper.GetComboStagesNotUsed(tcmdomain);
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateDomain", tcmObjetiveViewModel) });
                }

                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateDomain", tcmObjetiveViewModel) });

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        public JsonResult GetDescriptionStage(int idStage)
        {
            TCMStageEntity stage = _context.TCMStages.FirstOrDefault(o => o.Id == idStage);
            string text = "Select Stage";
            if (stage != null)
            {
                text = stage.Description;
            }
            return Json(text);
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> EditObjetive(int id = 0, int origi = 0, int idAddendum = 0)
        {
            UserEntity user_logged = await _context.Users

                                                  .Include(u => u.Clinic)
                                                  .ThenInclude(c => c.Setting)

                                                  .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor"))
            {

                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                TCMObjetiveEntity objetiveEntity = await _context.TCMObjetives
                                                       .Include(g => g.TcmDomain)
                                                       .FirstOrDefaultAsync(m => m.Id == id);

                TCMStageEntity stage = await _context.TCMStages
                                                .Include(g => g.tCMservice)
                                                .FirstOrDefaultAsync(m => (m.Name == objetiveEntity.Name
                                                && m.tCMservice.Name == objetiveEntity.TcmDomain.Name));
                List<TCMStageEntity> listStage = _context.TCMStages
                                                        .Where(m => (m.Name == objetiveEntity.Name
                                                             && m.tCMservice.Name == objetiveEntity.TcmDomain.Name))
                                                        .ToList();

                List<SelectListItem> list = listStage.Select(c => new SelectListItem
                {
                    Text = $"{c.Name}",
                    Value = $"{c.Id}"
                })
                    .ToList();

                TCMObjetiveViewModel model = null;
                model = new TCMObjetiveViewModel
                {
                    TcmDomain = objetiveEntity.TcmDomain,
                    Id_Domain = objetiveEntity.TcmDomain.Id,
                    Id_Stage = stage.Id,
                    Stages = list,
                    name = objetiveEntity.Name,
                    descriptionStages = stage.Description,
                    ID_Objetive = objetiveEntity.IdObjetive,
                    Start_Date = objetiveEntity.StartDate,
                    Target_Date = objetiveEntity.TargetDate,
                    End_Date = objetiveEntity.EndDate,
                    task = objetiveEntity.Task,
                    Responsible = objetiveEntity.Responsible,
                    Origi = origi,
                    CreatedOn = objetiveEntity.CreatedOn,
                    CreatedBy = objetiveEntity.CreatedBy
                    
                };

                ViewData["idAddendum"] = idAddendum;
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
        public async Task<IActionResult> EditObjetive(TCMObjetiveViewModel tcmObjetiveViewModel, int origi = 0, int idAddendum = 0) 
        {
            UserEntity user_logged = await _context.Users
                                                 .Include(u => u.Clinic)
                                                 .ThenInclude(c => c.Setting)
                                                 .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (User.IsInRole("CaseManager"))
            {

                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                TCMDomainEntity tcmDomain = _context.TCMDomains
                                                    .Include(f => f.TcmServicePlan)
                                                    .ThenInclude(f => f.TcmClient)
                                                    .ThenInclude(f => f.Casemanager)
                                                    .FirstOrDefault(f => f.Id == tcmObjetiveViewModel.Id_Domain);
                TCMObjetiveEntity tcmObjetiveEntity = await _converterHelper.ToTCMObjetiveEntity(tcmObjetiveViewModel, false, origi, user_logged.UserName);
                tcmObjetiveEntity.TcmDomain = tcmDomain;
               
                if (ModelState.IsValid)
                {
                    _context.Update(tcmObjetiveEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        if (origi == 0)
                        {
                            List<TCMDomainEntity> domainList = await _context.TCMDomains
                                                                        .Include(h => h.TCMObjetive)
                                                                        .Where(g => g.TcmServicePlan.Id == tcmDomain.TcmServicePlan.Id)
                                                                        .OrderBy(g => g.Code)
                                                                        .ToListAsync();

                            //return RedirectToAction("EditDomain", "TCMServicePlans");
                            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomains", domainList) });

                        }
                        if (origi == 1)
                        {
                            TCMAdendumEntity tcAddendumEntity = await _context.TCMAdendums
                                                                              .Include(n => n.TcmDomain)
                                                                              .ThenInclude(g => g.TCMObjetive.Where(m => m.Origin == "Addendum"))
                                                                              .FirstOrDefaultAsync(s => s.Id == idAddendum);

                            ViewData["idAddendum"] = idAddendum;
                            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainsAddendum", tcAddendumEntity.TcmDomain) });
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    TCMDomainEntity tcmdomain = await _context.TCMDomains
                                                   .Include(g => g.TCMObjetive)
                                                   .Include(g => g.TcmServicePlan)
                                                   .FirstOrDefaultAsync(m => m.Id == tcmObjetiveViewModel.Id_Domain);

                    tcmObjetiveViewModel.TcmDomain = tcmdomain;
                    tcmObjetiveViewModel.Stages = _combosHelper.GetComboStagesNotUsed(tcmdomain);
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjetive", tcmObjetiveViewModel) });
                }

                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjetive", tcmObjetiveViewModel) });

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteObjetive(int? id, int origi = 0, int aview = 0, int idAddendum = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMObjetiveEntity tcmObjetiveEntity = await _context.TCMObjetives
                                                                .Include(n => n.TcmDomain)
                                                                .ThenInclude(n => n.TcmServicePlan)
                                                                .ThenInclude(n => n.TcmClient)
                                                                .FirstOrDefaultAsync(s => s.Id == id);
            if (tcmObjetiveEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMObjetives.Remove(tcmObjetiveEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1, caseNumber = tcmObjetiveEntity.TcmDomain.TcmServicePlan.TcmClient.CaseNumber });
            }

            if (origi == 0)
            {
                if (aview == 0)
                {
                    return RedirectToAction("Edit", new { id = tcmObjetiveEntity.TcmDomain.TcmServicePlan.Id });
                }
                else
                {
                    return RedirectToAction("ServicePlanStarted", "TCMServicePlans", new { approved = aview });
                    //return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomains", domainList) });
                }
            }
            if (origi == 1)
            {
                return RedirectToAction("EditAdendum", "TCMServicePlans", new { id = idAddendum });
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> Adendum(int idError = 0, string caseNumber = "", int aview = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);


            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
            CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
            TCMClientEntity tcmClient = await _context.TCMClient.FirstOrDefaultAsync(n => n.CaseNumber == caseNumber);
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans
                                                          .FirstOrDefault(u => u.TcmClient.CaseNumber == caseNumber);

            if (tcmServicePlan != null && tcmServicePlan.Approved == 2)
            {
                if (user_logged.UserType.ToString() == "CaseManager")
                {
                    List<TCMAdendumEntity> adendum = new List<TCMAdendumEntity>();
                    if (caseNumber == "")
                    {
                        adendum = await _context.TCMAdendums
                                                .Include(h => h.TcmDomain)
                                                .ThenInclude(h => h.TCMObjetive)
                                                .Include(h => h.TcmServicePlan)
                                                .ThenInclude(h => (h.TcmClient))
                                                .ThenInclude(h => (h.Client))
                                                .Where(h => (h.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id
                                                   && h.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == clinic.Id))
                                                .ToListAsync();
                    }
                    else
                    {
                        adendum = await _context.TCMAdendums
                                                .Include(h => h.TcmDomain)
                                                .ThenInclude(h => h.TCMObjetive)
                                                .Include(h => h.TcmServicePlan)
                                                .ThenInclude(h => (h.TcmClient))
                                                .ThenInclude(h => (h.Client))
                                                .Where(h => (h.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id
                                                   && h.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == clinic.Id
                                                   && h.TcmServicePlan.TcmClient.CaseNumber == caseNumber))
                                                .ToListAsync();

                        ViewData["tcmClientId"] = caseNumber;
                        if (tcmClient != null)
                            ViewData["Id"] = tcmClient.Id;
                    }

                    ViewData["aview"] = aview;
                    return View(adendum);
                }
                if (user_logged.UserType.ToString() == "Manager" || user_logged.UserType.ToString() == "TCMSupervisor")
                {
                    List<TCMAdendumEntity> adendum = await _context.TCMAdendums
                                                       .Include(h => h.TcmDomain)
                                                       .ThenInclude(h => h.TCMObjetive)
                                                       .Include(h => h.TcmServicePlan)
                                                       .ThenInclude(h => (h.TcmClient))
                                                       .Include(h => h.TcmServicePlan.TcmClient.Client)
                                                       .Include(h => h.TcmServicePlan.TcmClient.Casemanager)
                                                       .Where(h => h.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == clinic.Id)
                                                       .ToListAsync();
                    ViewData["tcmClientId"] = caseNumber;
                    if (tcmClient != null)
                        ViewData["Id"] = tcmClient.Id;
                    ViewData["aview"] = aview;
                    return View(adendum);
                }
               
            }
            else
            {
                if (aview == 0)
                {
                    return RedirectToAction("TCMIntakeSectionDashboard", "TCMIntakes", new { id = _context.TCMClient.FirstOrDefault(n => n.Id == tcmClient.Id).Id, section = 4 });
                }
                else
                {
                    List<TCMAdendumEntity> adendum = await _context.TCMAdendums
                                                        .Include(h => h.TcmDomain)
                                                        .ThenInclude(h => h.TCMObjetive)
                                                        .Include(h => h.TcmServicePlan)
                                                        .ThenInclude(h => (h.TcmClient))
                                                        .Include(h => h.TcmServicePlan.TcmClient.Client)
                                                        .Include(h => h.TcmServicePlan.TcmClient.Casemanager)
                                                        .Where(h => h.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == clinic.Id)
                                                        .ToListAsync();

                    ViewData["tcmClientId"] = caseNumber;
                    if (tcmClient != null)
                        ViewData["Id"] = 0;
                    ViewData["aview"] = aview;
                    return View(adendum);
                }
                
            }

            return View(null);

        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateAdendum(string idClient = "")
        {
            TCMAdendumViewModel model = null;
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                              .FirstOrDefault(u => u.UserName == User.Identity.Name);
            CaseMannagerEntity caseManager = _context.CaseManagers.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    model = new TCMAdendumViewModel
                    {
                        Date_Identified = DateTime.Today.Date,
                        ID_TcmDominio = 0,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        TcmDominio = _combosHelper.GetComboTCMServices(),
                        ID_TcmServicePlan = 0,
                        ListTcmServicePlan = _combosHelper.GetComboServicesPlan(user_logged.Clinic.Id, caseManager.Id, idClient),
                        

                    };
                    return View(model);
                }
                return RedirectToAction("NotAuthorized", "Account");

            }
            return RedirectToAction("Adendum", "TCMServicePlans");
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateAdendum(TCMAdendumViewModel tcmAdendumViewModel)
        {
            UserEntity user_logged = _context.Users
                                           .Include(u => u.Clinic)
                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);
            CaseMannagerEntity caseManager = _context.CaseManagers
                                                     .FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

            if (tcmAdendumViewModel.ID_TcmServicePlan == 0)
            {
                ModelState.AddModelError(string.Empty, "You must select a TCM case");

            }
            else if (tcmAdendumViewModel.ID_TcmDominio == 0)
            {
                ModelState.AddModelError(string.Empty, "You must select a TCM services");

            }
            else
            {
                if (user_logged.UserType.ToString() == "CaseManager")
                {
                    if (ModelState.IsValid)
                    {
                        TCMServiceEntity tcmService = await _context.TCMServices.FirstAsync(d => d.Id == tcmAdendumViewModel.ID_TcmDominio);

                        TCMServicePlanEntity tcmServicePlan = await _context.TCMServicePlans
                                                .FirstOrDefaultAsync(s => s.Id == tcmAdendumViewModel.ID_TcmServicePlan);

                        TCMDomainEntity tcmDomain = _context.TCMDomains.FirstOrDefault(n => n.Code == tcmService.Code);

                        if (tcmDomain == null)
                        {
                            tcmDomain = new TCMDomainEntity
                            {
                                DateIdentified = tcmAdendumViewModel.Date_Identified,
                                LongTerm = tcmAdendumViewModel.Long_term,
                                NeedsIdentified = tcmAdendumViewModel.Needs_Identified,
                                TCMObjetive = null,
                                TcmServicePlan = tcmServicePlan,
                                Name = tcmService.Name,
                                Code = tcmService.Code,
                                Origin = "Addendum",
                                CreatedBy = tcmAdendumViewModel.CreatedBy,
                                CreatedOn = tcmAdendumViewModel.CreatedOn

                            };

                        }
                        else
                        {
                            //tcmAdendumViewModel.Long_term = tcmDomain.LongTerm;
                            //tcmAdendumViewModel.Needs_Identified = tcmDomain.NeedsIdentified;
                        }
                       
                        
                         tcmAdendumViewModel.TcmServicePlan = tcmServicePlan;
                         tcmAdendumViewModel.TcmDomain = tcmDomain;

                         TCMAdendumEntity tcmAdendum = await _converterHelper.ToTCMAdendumEntity(tcmAdendumViewModel, true, user_logged.UserName);
                            _context.Add(tcmAdendum);
                        try
                        {
                            await _context.SaveChangesAsync();

                            List<TCMAdendumEntity> adendum = await _context.TCMAdendums
                                                       .Include(h => h.TcmDomain)
                                                       .ThenInclude(h => h.TCMObjetive)
                                                       .Include(h => h.TcmServicePlan)
                                                       .ThenInclude(h => h.TcmClient)
                                                       .Include(h => h.TcmServicePlan.TcmClient.Casemanager)
                                                       .Include(h => (h.TcmServicePlan.TcmClient.Client))
                                                       .ToListAsync();
                            List<TCMAdendumEntity> salida = new List<TCMAdendumEntity>();
                            foreach (var item in adendum)
                            {
                                if (item.TcmServicePlan.TcmClient.CaseNumber == tcmServicePlan.TcmClient.CaseNumber)
                                    salida.Add(item);
                            }

                            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_TCMAdendum", salida) });

                        }
                        catch (System.Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                    else
                    {
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateAdendum", tcmAdendumViewModel) });
                    }

                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            tcmAdendumViewModel.Date_Identified = DateTime.Today.Date;
            tcmAdendumViewModel.ID_TcmDominio = 0;
            tcmAdendumViewModel.TcmDominio = _combosHelper.GetComboTCMServices();
            tcmAdendumViewModel.ID_TcmServicePlan = 0;
            tcmAdendumViewModel.ListTcmServicePlan = _combosHelper.GetComboServicesPlan(user_logged.Clinic.Id, caseManager.Id,"");

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateAdendum", tcmAdendumViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> EditAdendum(int? id, int aview = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMAdendumEntity tcmadendumEntity = await _context.TCMAdendums
                                                              .Include(u => u.TcmServicePlan)
                                                              .ThenInclude(u => u.TCMDomain)
                                                              .ThenInclude(g => g.TCMObjetive.Where(m => m.Origin == "Addendum"))
                                                              .Include(u => u.TcmServicePlan)
                                                              .ThenInclude(g => g.TcmClient)
                                                              .ThenInclude(g => g.Client)
                                                              .FirstOrDefaultAsync(s => s.Id == id);
            if (tcmadendumEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMAdendumViewModel tcmAdendumViewModel = null;

            if (User.IsInRole("CaseManager"))
            {
                List<TCMServicePlanEntity> tcmSerivicePlan = _context.TCMServicePlans
                                                                     .Include(g => g.TcmClient)
                                                                     .ThenInclude(g => g.Client)
                                                                     .Where(c => c.Id == tcmadendumEntity.TcmServicePlan.Id)
                                                                     .ToList();

                List<SelectListItem> list_servicePlan = tcmSerivicePlan.Select(c => new SelectListItem
                {
                    Text = $"{c.TcmClient.CaseNumber}",
                    Value = $"{c.Id}"
                })
                .ToList();

                List<TCMDomainEntity> tcmDomain = _context.TCMDomains
                                                          .Include(g => g.TCMObjetive)
                                                          .Where(c => c.Id == tcmadendumEntity.TcmDomain.Id)
                                                          .ToList();
                List<SelectListItem> list_domain = tcmDomain.Select(c => new SelectListItem
                {
                    Text = $"{c.Code + "-" + c.Name}",
                    Value = $"{c.Id}"
                })
                .ToList();

                tcmAdendumViewModel = new TCMAdendumViewModel
                {
                    Id = tcmadendumEntity.Id,
                    TcmServicePlan = tcmadendumEntity.TcmServicePlan,
                    ID_TcmServicePlan = 0,
                    ListTcmServicePlan = list_servicePlan,
                    ID_TcmDominio = 0,
                    TcmDominio = list_domain,
                    TcmDomain = tcmadendumEntity.TcmDomain,
                    Date_Identified = tcmadendumEntity.TcmDomain.DateIdentified,
                    Long_term = tcmadendumEntity.LongTerm,
                    Needs_Identified = tcmadendumEntity.NeedsIdentified,
                    CreatedBy = tcmadendumEntity.CreatedBy,
                    CreatedOn = tcmadendumEntity.CreatedOn
                };
                ViewData["aview"] = aview;
                return View(tcmAdendumViewModel);
            }

            ViewData["aview"] = aview;
            return View(tcmAdendumViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> EditAdendum(int? id, TCMAdendumViewModel tcmAdendumViewModel, int aview = 0)
        {
            if (id != tcmAdendumViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans
                                                           .Include(u => u.TcmClient)
                                                           .ThenInclude(g => g.Client)
                                                           .FirstOrDefault(h => h.Id == tcmAdendumViewModel.ID_TcmServicePlan);
            TCMDomainEntity tcmDomain = _context.TCMDomains
                                                .Include(u => u.TCMObjetive)
                                                .FirstOrDefault(h => h.Id == tcmAdendumViewModel.ID_TcmDominio);

            tcmDomain.NeedsIdentified = tcmAdendumViewModel.Needs_Identified;
            tcmDomain.LongTerm = tcmAdendumViewModel.Long_term;
            tcmDomain.DateIdentified = tcmAdendumViewModel.Date_Identified;
            tcmAdendumViewModel.TcmDomain = tcmDomain;
            tcmAdendumViewModel.TcmServicePlan = tcmServicePlan;

            CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);

            if (user_logged.UserType.ToString() == "CaseManager")
            {
                if (ModelState.IsValid)
                {
                    TCMAdendumEntity tcmAdendumEntity = await _converterHelper.ToTCMAdendumEntity(tcmAdendumViewModel, false, user_logged.UserName);
                    _context.Update(tcmAdendumEntity);

                    try
                    {
                        await _context.SaveChangesAsync();
                        ViewData["aview"] = aview;
                        return RedirectToAction("Adendum", "TCMServicePlans", new { caseNumber = tcmServicePlan.TcmClient.CaseNumber});
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditAdendum", tcmAdendumViewModel) });
                }
            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            return RedirectToAction("Home/Error404");

        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> FinishEditingAdendum(int id, int origin = 0)
        {
            TCMAdendumEntity tcmAdendum = _context.TCMAdendums
                                                  .Include(u => u.TcmDomain)
                                                  .ThenInclude(h => h.TCMObjetive)
                                                  .Include(u => u.TcmServicePlan.TcmClient.Casemanager)
                                                  .Include(u => u.TcmServicePlan)
                                                  .ThenInclude(u => u.TcmClient)
                                                  .ThenInclude(u => u.Client)
                                                  .FirstOrDefault(u => u.Id == id);

            if (tcmAdendum != null)
            {
                if (User.IsInRole("CaseManager"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        tcmAdendum.Approved = 1;
                        _context.Update(tcmAdendum);
                        try
                        {
                            await _context.SaveChangesAsync();

                            if (origin == 0)
                            {
                                return RedirectToAction("Adendum", "TCMServicePlans", new { caseNumber = tcmAdendum.TcmServicePlan.TcmClient.CaseNumber, aview = origin });
                            }
                            else
                            {
                                return RedirectToAction("AdendumApproved", "TCMServicePlans", new { approved = 0 });
                            }
                            
                        }
                        catch (System.Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("Adendum", "TCMServicePlans");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> AproveAdendum(int id)
        {
            TCMAdendumEntity tcmAdendum = _context.TCMAdendums
                                                  .Include(u => u.TcmServicePlan.TcmClient)
                                                  .ThenInclude(u => u.Client)
                                                  .FirstOrDefault(u => u.Id == id);

            if (tcmAdendum != null)
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        tcmAdendum.Approved = 2;
                        _context.Update(tcmAdendum);
                        try
                        {
                            await _context.SaveChangesAsync();

                            return RedirectToAction("AdendumApproved", "TCMServicePlans", new { approved = 1});
                        }
                        catch (System.Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("Adendum", "TCMServicePlans");
        }

        [Authorize(Roles = "Manager, CaseManager")]
        public async Task<IActionResult> Domains(int idError = 0)
        {
            if (User.IsInRole("Manager"))
            {
                return View(await _context.TCMDomains
                                          .Include(f => f.TCMObjetive)
                                          .Include(g => g.TcmServicePlan)
                                          .ThenInclude(g => g.TcmClient)
                                          .ThenInclude(g => g.Client)
                                          .OrderBy(f => f.TcmServicePlan.TcmClient.Casemanager)
                                          .ToListAsync());
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


                return View(await _context.TCMDomains
                                          .Include(f => f.TCMObjetive)
                                          .Include(g => g.TcmServicePlan)
                                          .ThenInclude(g => g.TcmClient)
                                          .ThenInclude(g => g.Client)
                                          .Where(s => s.TcmServicePlan.TcmClient.Casemanager.LinkedUser == user_logged.UserName)
                                          .OrderBy(f => f.Name)
                                          .ToListAsync());
            }
        }

        [Authorize(Roles = "Manager, CaseManager")]
        public async Task<IActionResult> Objectives(int idError = 0)
        {
            if (User.IsInRole("Manager"))
            {
                return View(await _context.TCMObjetives
                                          .Include(g => g.TcmDomain)
                                          .OrderBy(f => f.IdObjetive)
                                          .ToListAsync());
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


                return View(await _context.TCMObjetives
                                        .Include(g => g.TcmDomain)
                                        .OrderBy(f => f.IdObjetive)
                                        .ToListAsync());
            }
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> ServicePlanStarted(string caseNumber = "", int approved = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);


            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
            CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);

            List<TCMServicePlanEntity> servicePlan = null;

            if (user_logged.UserType.ToString() == "CaseManager")
            {

                if (caseNumber == "")
                {
                    if (approved == 2)
                    {
                        servicePlan = await _context.TCMServicePlans
                                                         .Include(h => h.TCMDomain)
                                                         .ThenInclude(h => h.TCMObjetive)
                                                         .Include(g => g.TcmClient)
                                                         .ThenInclude(f => f.Client)
                                                         .Include(t => t.TcmClient.Casemanager)
                                                         .Where(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                                && g.Approved == approved))
                                                         .OrderBy(g => g.TcmClient.CaseNumber)
                                                         .ToListAsync();

                    }
                    else
                    {
                        servicePlan = await _context.TCMServicePlans
                                                         .Include(h => h.TCMDomain)
                                                         .ThenInclude(h => h.TCMObjetive)
                                                         .Include(g => g.TcmClient)
                                                         .ThenInclude(f => f.Client)
                                                         .Include(t => t.TcmClient.Casemanager)
                                                         .Where(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                                && g.Approved != 2))
                                                         .OrderBy(g => g.TcmClient.CaseNumber)
                                                         .ToListAsync();

                    }

                }
                else
                {
                    if (approved == 2)
                    {
                        servicePlan = await _context.TCMServicePlans
                                                         .Include(h => h.TCMDomain)
                                                         .ThenInclude(h => h.TCMObjetive)
                                                         .Include(g => g.TcmClient)
                                                         .ThenInclude(f => f.Client)
                                                         .Include(t => t.TcmClient.Casemanager)
                                                         .Where(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                          && g.TcmClient.CaseNumber == caseNumber
                                                          && g.Approved == approved))
                                                         .OrderBy(g => g.TcmClient.CaseNumber)
                                                         .ToListAsync();

                    }
                    else
                    {
                        servicePlan = await _context.TCMServicePlans
                                                         .Include(h => h.TCMDomain)
                                                         .ThenInclude(h => h.TCMObjetive)
                                                         .Include(g => g.TcmClient)
                                                         .ThenInclude(f => f.Client)
                                                         .Include(t => t.TcmClient.Casemanager)
                                                         .Where(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                          && g.TcmClient.CaseNumber == caseNumber
                                                          && g.Approved != 2))
                                                         .OrderBy(g => g.TcmClient.CaseNumber)
                                                         .ToListAsync();

                    }

                }


            }
            if (user_logged.UserType.ToString() == "Manager" || user_logged.UserType.ToString() == "TCMSupervisor")
            {
                 servicePlan = await _context.TCMServicePlans
                                             .Include(h => h.TCMDomain)
                                             .ThenInclude(h => h.TCMObjetive)
                                             .Include(g => g.TcmClient)
                                             .ThenInclude(f => f.Client)
                                             .Include(t => t.TcmClient.Casemanager)
                                             .Where(g => (g.TcmClient.Client.Clinic.Id == clinic.Id
                                                       && g.Approved == approved))
                                             .OrderBy(g => g.TcmClient.CaseNumber)
                                             .ToListAsync();
            }
           
            ViewData["origin"] = caseNumber;
            return View(servicePlan);
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> AdendumApproved(string tcmClientId = "", int approved = 0, int aview = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);


            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
            CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
            TCMClientEntity tcmClient = await _context.TCMClient.FirstOrDefaultAsync(n => n.CaseNumber == tcmClientId);


            if (user_logged.UserType.ToString() == "CaseManager")
            {
                List<TCMAdendumEntity> adendum = new List<TCMAdendumEntity>();
                if (tcmClientId == "")
                {
                    adendum = await _context.TCMAdendums
                                            .Include(h => h.TcmDomain)
                                            .ThenInclude(h => h.TCMObjetive)
                                            .Include(h => h.TcmServicePlan)
                                            .ThenInclude(h => (h.TcmClient))
                                            .ThenInclude(h => (h.Client))
                                            .Where(h => (h.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id
                                               && h.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == clinic.Id
                                               && h.Approved == approved))
                                            .ToListAsync();
                }
                else
                {
                    adendum = await _context.TCMAdendums
                                            .Include(h => h.TcmDomain)
                                            .ThenInclude(h => h.TCMObjetive)
                                            .Include(h => h.TcmServicePlan)
                                            .ThenInclude(h => (h.TcmClient))
                                            .ThenInclude(h => (h.Client))
                                            .Where(h => (h.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id
                                               && h.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == clinic.Id
                                               && h.TcmServicePlan.TcmClient.CaseNumber == tcmClientId
                                               && h.Approved == approved))
                                            .ToListAsync();

                    ViewData["tcmClientId"] = tcmClientId;
                    if (tcmClient != null)
                        ViewData["Id"] = tcmClient.Id;
                }

                ViewData["aview"] = aview;
                return View(adendum);
            }
            if (user_logged.UserType.ToString() == "Manager")
            {
                List<TCMAdendumEntity> adendum = await _context.TCMAdendums
                                                   .Include(h => h.TcmDomain)
                                                   .ThenInclude(h => h.TCMObjetive)
                                                   .Include(h => h.TcmServicePlan)
                                                   .ThenInclude(h => (h.TcmClient))
                                                   .Include(h => h.TcmServicePlan.TcmClient.Client)
                                                   .Include(h => h.TcmServicePlan.TcmClient.Casemanager)
                                                   .Where(h => h.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == clinic.Id
                                                        && h.Approved == approved)
                                                   .ToListAsync();
                ViewData["aview"] = aview;
                return View(adendum);
            }
            if (user_logged.UserType.ToString() == "TCMSupervisor")
            {
                List<TCMAdendumEntity> adendum = await _context.TCMAdendums
                                                   .Include(h => h.TcmDomain)
                                                   .ThenInclude(h => h.TCMObjetive)
                                                   .Include(h => h.TcmServicePlan)
                                                   .ThenInclude(h => (h.TcmClient))
                                                   .Include(h => h.TcmServicePlan.TcmClient.Client)
                                                   .Include(h => h.TcmServicePlan.TcmClient.Casemanager)
                                                   .Where(h => h.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == clinic.Id
                                                        && h.Approved == approved)
                                                   .ToListAsync();
                ViewData["aview"] = aview;
                return View(adendum);
            }

            return View(null);

        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> EditReadOnly(int Id = 0, int origi = 0)
        {
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans
                                                          .Include(f => f.TCMDomain)
                                                          .ThenInclude(f => f.TCMObjetive)
                                                          .Include(f => f.TcmClient)
                                                          .FirstOrDefault(u => u.Id == Id);
            TCMServicePlanViewModel model;

            if (tcmServicePlan != null && tcmServicePlan.Approved != 2)
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        List<TCMClientEntity> tcmClient = _context.TCMClient
                                                       .Include(u => u.Client)
                                                       .Where(u => u.Id == tcmServicePlan.TcmClient.Id)
                                                       .ToList();

                        List<SelectListItem> list_Client = tcmClient.Select(c => new SelectListItem
                        {
                            Text = $"{c.Client.Name}",
                            Value = $"{c.Id}"
                        })
                            .ToList();

                        List<ClinicEntity> clinic = _context.Clinics
                                                         .Where(u => u.Id == user_logged.Clinic.Id)
                                                         .ToList();

                        List<SelectListItem> list_Clinins = clinic.Select(c => new SelectListItem
                        {
                            Text = $"{c.Name}",
                            Value = $"{c.Id}"
                        })
                            .ToList();

                        model = new TCMServicePlanViewModel
                        {
                            ID_TcmClient = tcmServicePlan.TcmClient.Id,
                            TcmClients = list_Client,
                            ID_Clinic = user_logged.Clinic.Id,
                            Clinics = list_Clinins,
                            Status = tcmServicePlan.Status,
                            ID_Status = (tcmServicePlan.Status == StatusType.Open) ? 1 : 2,
                            status = _combosHelper.GetComboClientStatus(),
                            CaseNumber = _context.TCMClient.FirstOrDefault(u => u.Id == tcmServicePlan.TcmClient.Id).CaseNumber,
                            Date_ServicePlan = tcmServicePlan.DateServicePlan,
                            Date_Intake = tcmServicePlan.DateIntake,
                            Date_Assessment = tcmServicePlan.DateAssessment,
                            Date_Certification = tcmServicePlan.DateCertification,
                            strengths = tcmServicePlan.Strengths,
                            weakness = tcmServicePlan.Weakness,
                            dischargerCriteria = tcmServicePlan.DischargerCriteria,
                            Id = Id,
                            TCMDomain = tcmServicePlan.TCMDomain,
                            Approved = tcmServicePlan.Approved,
                            CreatedBy = tcmServicePlan.CreatedBy,
                            CreatedOn = tcmServicePlan.CreatedOn

                        };

                        model.TCMDomain = model.TCMDomain.Where(n => (n.Origin == "Service Plan")).ToList();
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("Index", "TCMServicePlans", new { caseNumber = tcmServicePlan.TcmClient.CaseNumber });
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> EditAdendumReadOnly(int? id, int aview = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMAdendumEntity tcmadendumEntity = await _context.TCMAdendums
                                                              .Include(u => u.TcmServicePlan)
                                                              .ThenInclude(u => u.TCMDomain)
                                                              .ThenInclude(g => g.TCMObjetive.Where(m => m.Origin == "Addendum"))
                                                              .Include(u => u.TcmServicePlan)
                                                              .ThenInclude(g => g.TcmClient)
                                                              .ThenInclude(g => g.Client)
                                                              .FirstOrDefaultAsync(s => s.Id == id);
            if (tcmadendumEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMAdendumViewModel tcmAdendumViewModel = null;

            if (User.IsInRole("TCMSupervisor"))
            {
                List<TCMServicePlanEntity> tcmSerivicePlan = _context.TCMServicePlans
                                                                     .Include(g => g.TcmClient)
                                                                     .ThenInclude(g => g.Client)
                                                                     .Where(c => c.Id == tcmadendumEntity.TcmServicePlan.Id)
                                                                     .ToList();

                List<SelectListItem> list_servicePlan = tcmSerivicePlan.Select(c => new SelectListItem
                {
                    Text = $"{c.TcmClient.CaseNumber}",
                    Value = $"{c.Id}"
                })
                .ToList();

                List<TCMDomainEntity> tcmDomain = _context.TCMDomains
                                                          .Include(g => g.TCMObjetive)
                                                          .Where(c => c.Id == tcmadendumEntity.TcmDomain.Id)
                                                          .ToList();
                List<SelectListItem> list_domain = tcmDomain.Select(c => new SelectListItem
                {
                    Text = $"{c.Code + "-" + c.Name}",
                    Value = $"{c.Id}"
                })
                .ToList();

                tcmAdendumViewModel = new TCMAdendumViewModel
                {
                    Id = tcmadendumEntity.Id,
                    TcmServicePlan = tcmadendumEntity.TcmServicePlan,
                    ID_TcmServicePlan = 0,
                    ListTcmServicePlan = list_servicePlan,
                    ID_TcmDominio = 0,
                    TcmDominio = list_domain,
                    TcmDomain = tcmadendumEntity.TcmDomain,
                    Date_Identified = tcmadendumEntity.TcmDomain.DateIdentified,
                    Long_term = tcmadendumEntity.LongTerm,
                    Needs_Identified = tcmadendumEntity.NeedsIdentified,
                    CreatedBy = tcmadendumEntity.CreatedBy,
                    CreatedOn = tcmadendumEntity.CreatedOn
                };
                ViewData["aview"] = aview;
                return View(tcmAdendumViewModel);
            }

            ViewData["aview"] = aview;
            return View(tcmAdendumViewModel);
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult AddMessageEntity(int id = 0, int origi = 0)
        {
            if (id == 0)
            {
                return View(new TCMMessageViewModel());
            }
            else
            {
                TCMMessageViewModel model = new TCMMessageViewModel()
                {
                    IdTCMServiceplan = id,
                    Origin = origi
                };

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> AddMessageEntity(TCMMessageViewModel messageViewModel)
        {
            if (ModelState.IsValid)
            {
                TCMMessageEntity model = await _converterHelper.ToTCMMessageEntity(messageViewModel, true);
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                model.From = user_logged.UserName;
                model.To = model.TCMServicePlan.CreatedBy;
                _context.Add(model);
                await _context.SaveChangesAsync();
            }

            if (messageViewModel.Origin == 1)
                return RedirectToAction("ServicePlanStarted", new { approved  = 1});

            if (messageViewModel.Origin == 2)
                return RedirectToAction("TCMServicePlanWithReview");
            if (messageViewModel.Origin == 3)
                return RedirectToAction("Notifications", "TCMMessages");

            return RedirectToAction("Index");
        }


        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> TCMServicePlanWithReview()
        {
            if (User.IsInRole("CaseManager"))
            {
                List<TCMServicePlanEntity> salida = await _context.TCMServicePlans
                                                                  .Include(wc => wc.TcmClient)
                                                                  .ThenInclude(wc => wc.Casemanager)
                                                                  .Include(wc => wc.TcmClient)
                                                                  .ThenInclude(wc => wc.Client)
                                                                  .Include(wc => wc.TCMMessages.Where(m => m.Notification == false))
                                                                  .Where(wc => (wc.TcmClient.Casemanager.LinkedUser == User.Identity.Name
                                                                        && wc.Approved == 1
                                                                        && wc.TCMMessages.Count() > 0))
                                                                  .ToListAsync();


                return View(salida);
            }

            if (User.IsInRole("TCMSupervisor"))
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    List<TCMServicePlanEntity> salida = await _context.TCMServicePlans
                                                                  .Include(wc => wc.TcmClient)
                                                                  .ThenInclude(wc => wc.Casemanager)
                                                                  .Include(wc => wc.TcmClient)
                                                                  .ThenInclude(wc => wc.Client)
                                                                  .Include(wc => wc.TCMMessages.Where(m => m.Notification == false))
                                                                  .Where(wc => (wc.TcmClient.Casemanager.Clinic.Id == user_logged.Clinic.Id
                                                                    && wc.Approved == 1
                                                                    && wc.TCMMessages.Count() > 0))
                                                                  .ToListAsync();
                    return View(salida);
                }
            }

            return View();
        }


    }
}
