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
using KyoS.Common.Helpers;

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
                                                     .Where(g => (g.TcmClient.Client.Clinic.Id == clinic.Id
                                                            && g.TcmClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName))
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
                        string strendths = string.Empty;
                        string weakness = string.Empty;
                        TCMAssessmentEntity assessment = _context.TCMAssessment.FirstOrDefault(n => n.TcmClient.Id == id);
                        if (assessment != null)
                        {
                            strendths = assessment.ListClientCurrentPotencialStrngths;
                            weakness = assessment.ListClientCurrentPotencialWeakness;
                        }
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
                            DateServicePlan = DateTime.Now.Date,
                            DateIntake = DateTime.Now.Date,
                            DateAssessment = DateTime.Now.Date,
                            DateCertification = DateTime.Now.Date,
                            Strengths = strendths,
                            Weakness = weakness

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
                            return RedirectToAction("TCMIntakeSectionDashboard","TCMIntakes", new { id = tcmServicePlanViewModel.ID_TcmClient, section = 4 }); ;
                        }
                        else
                        {
                            if (origin == 4)
                            {
                                return RedirectToAction("GetCaseNotServicePlan", "TCMClients");
                            }
                            else
                            {
                                return RedirectToAction("ServicePlanStarted", "TCMServicePlans", new { approved = (origin - 1) });
                            }
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

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult Edit(int Id = 0, int origin = 0)
        {
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans
                                                          .Include(f => f.TCMDomain)
                                                          .ThenInclude(f => f.TCMObjetive)
                                                          .Include(f => f.TcmClient)
                                                          .ThenInclude(f => f.Client)
                                                          .ThenInclude(f => f.Clinic)
                                                          .ThenInclude(f => f.Setting)
                                                          .Include(n => n.TcmClient)
                                                          .ThenInclude(n => n.Casemanager)
                                                          .ThenInclude(n => n.TCMSupervisor)
                                                          .FirstOrDefault(u => u.Id == Id);

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(m => m.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMServicePlanViewModel model;

            if ((tcmServicePlan != null && tcmServicePlan.Approved != 2 && User.IsInRole("CaseManager"))
                || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true && tcmServicePlan.Approved == 2))
            {
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

                    TCMAssessmentEntity assessment = _context.TCMAssessment.FirstOrDefault(n => n.TcmClient.Id == tcmServicePlan.TcmClient.Id);
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
                        DateServicePlan = tcmServicePlan.DateServicePlan,
                        DateIntake = tcmServicePlan.DateIntake,
                        DateAssessment = tcmServicePlan.DateAssessment,
                        DateCertification = tcmServicePlan.DateCertification,
                        Strengths = tcmServicePlan.Strengths,
                        Weakness = tcmServicePlan.Weakness,
                        DischargerCriteria = tcmServicePlan.DischargerCriteria,
                        Id = Id,
                        TCMDomain = tcmServicePlan.TCMDomain,
                        Approved = tcmServicePlan.Approved,
                        CreatedBy = tcmServicePlan.CreatedBy,
                        CreatedOn = tcmServicePlan.CreatedOn,
                        TcmClient = tcmServicePlan.TcmClient,
                        DateSupervisorSignature = tcmServicePlan.DateSupervisorSignature,
                        Domain1 = assessment.RecommendedMentalHealth,
                        Domain2 = assessment.RecommendedPhysicalHealth,
                        Domain3 = assessment.RecommendedVocation,
                        Domain4 = assessment.RecommendedSchool,
                        Domain5 = assessment.RecommendedRecreational,
                        Domain6 = assessment.RecommendedActivities,
                        Domain7 = assessment.RecommendedHousing,
                        Domain8 = assessment.RecommendedEconomic,
                        Domain9 = assessment.RecommendedBasicNeed,
                        Domain10 = assessment.RecommendedTransportation,
                        Domain11 = assessment.RecommendedLegalImmigration,
                        Domain12 = assessment.RecommendedOther,
                        DateTcmSignature = tcmServicePlan.DateTcmSignature
                    };

                    ViewData["origin"] = origin;
                    model.TCMDomain = model.TCMDomain.Where(n => (n.Origin == "Service Plan")).ToList();
                    return View(model);
                }
                return RedirectToAction("NotAuthorized", "Account");
            }

            return RedirectToAction("Index", "TCMServicePlans", new { caseNumber = tcmServicePlan.TcmClient.CaseNumber });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> Edit(int id, TCMServicePlanViewModel serviceplanViewModel, int origin = 0)
        {
            UserEntity user_logged = _context.Users
                                              .Include(u => u.Clinic)
                                              .ThenInclude(u => u.Setting)
                                              .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if ((serviceplanViewModel != null && serviceplanViewModel.Approved != 2 && User.IsInRole("CaseManager"))
               || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true && serviceplanViewModel.Approved == 2))
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

                if (User.IsInRole("TCMSupervisor"))
                {
                    tcmServicePlanEntity.DateSupervisorSignature = serviceplanViewModel.DateSupervisorSignature;
                }
                _context.Update(tcmServicePlanEntity);
                  try
                  {
                       await _context.SaveChangesAsync();

                    if (origin == 0)
                    {
                        return RedirectToAction("TCMIntakeSectionDashboard", "TCMIntakes", new { id = tcmServicePlanEntity.TcmClient.Id, section = 4 });
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
                                if (origin == 5)
                                {
                                    return RedirectToAction("GetCaseNotServicePlan", "TCMClients");
                                }
                                else
                                {
                                    if (origin == 6)
                                    {
                                        return RedirectToAction("UpdateServicePlan", "TCMServicePlans");
                                    }
                                    else
                                    {
                                        return RedirectToAction("ServicePlanStarted", "TCMServicePlans", new { approved = (origin - 1) });
                                    }
                                }
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
                                if (origin == 5)
                                {
                                    return RedirectToAction("GetCaseNotServicePlan", "TCMClients");
                                }
                                else
                                {
                                    return RedirectToAction("ServicePlanStarted", "TCMServicePlans", new { approved = 1 });
                                }
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
        public async Task<IActionResult> AproveServicePlan(int id, TCMServicePlanViewModel model, int origi = 0)
        {
            TCMServicePlanEntity tcmServicePlan = new TCMServicePlanEntity();

            if (tcmServicePlan != null)
            { 
                if (User.IsInRole("TCMSupervisor"))
                {
                    UserEntity user_logged = _context.Users
                                                     .Include(u => u.Clinic)
                                                     .ThenInclude(u => u.Setting)
                                                     .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        if (user_logged.Clinic.Setting.TCMSupervisorEdit == true)
                        {
                            tcmServicePlan = await _converterHelper.ToTCMServicePlanEntity(model, false, user_logged.UserName);
                            tcmServicePlan.Approved = 2;
                            tcmServicePlan.TCMSupervisor = await _context.TCMSupervisors.FirstOrDefaultAsync(n => n.LinkedUser == user_logged.UserName);
                        }
                        else
                        {
                            tcmServicePlan = _context.TCMServicePlans
                                                     .Include(u => u.TcmClient)
                                                     .ThenInclude(u => u.Client)
                                                     .FirstOrDefault(u => u.Id == id);

                            tcmServicePlan.Approved = 2;
                            tcmServicePlan.TCMSupervisor = await _context.TCMSupervisors.FirstOrDefaultAsync(n => n.LinkedUser == user_logged.UserName);
                            tcmServicePlan.DateSupervisorSignature = model.DateSupervisorSignature;
                            tcmServicePlan.DateServicePlan = model.DateServicePlan;
                        }
                        
                        _context.Update(tcmServicePlan);
                        try
                        {
                            await _context.SaveChangesAsync();
                            if (origi == 0)
                            {
                                return RedirectToAction("Index", "TCMServicePlans");
                            }
                            if (origi == 1)
                            {
                                return RedirectToAction("ServicePlanStarted", "TCMServicePlans", new { approved = 1 });
                            }
                            if (origi == 3)
                            {
                                return RedirectToAction("Notifications", "TCMMessages");
                            }
                            if (origi == 4)
                            {
                                return RedirectToAction("TCMIntakeSectionDashboardReadOnly", "TCMIntakes", new { id = tcmServicePlan.TcmClient.Id, section = 4 });
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
                                                               && g.Approved == 2
                                                               && g.TcmClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName))
                                                     .ToListAsync();
                return View(servicePlan);
            }

            return View(null);

        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
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
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMDomainViewModel tcmDomainViewModel = null;

            if (User.IsInRole("CaseManager"))
            {
                tcmDomainViewModel = _converterHelper.ToTCMDomainViewModel(tcmDomainEntity);

                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = tcmDomainEntity.Code + ' '+ tcmDomainEntity.Name,
                    Value = $"{tcmDomainEntity.Id}"
                });
                tcmDomainViewModel.Services = list;
            }

            if (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true)
            {
                tcmDomainViewModel = _converterHelper.ToTCMDomainViewModel(tcmDomainEntity);

                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = tcmDomainEntity.Code + ' ' + tcmDomainEntity.Name,
                    Value = $"{tcmDomainEntity.Id}"
                });
                tcmDomainViewModel.Services = list;
            }

            ViewData["origi"] = origi;
            ViewData["aview"] = aview;
            ViewData["idAddendum"] = idAddendum;
            
            return View(tcmDomainViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
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
                TCMDomainEntity tcmDomainEntity = _converterHelper.ToTCMDomainEntity(tcmDomainViewModel, false, tcmDomainViewModel.Origin, user_logged.UserName);
                _context.Update(tcmDomainEntity);

                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMDomainEntity> domainList = await _context.TCMDomains
                                                                     .Include(h => h.TCMObjetive)
                                                                     .Include(g => g.TcmServicePlan)
                                                                     .ThenInclude(g => g.TcmClient)
                                                                     .ThenInclude(g => g.Client)
                                                                     .ThenInclude(g => g.Clinic)
                                                                     .ThenInclude(g => g.Setting)
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
                        /*TCMAdendumEntity tcmadendumEntity = await _context.TCMAdendums
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
                                                                         .Include(u => u.TcmServicePlan)
                                                                         .ThenInclude(g => g.TcmClient)
                                                                         .ThenInclude(g => g.Casemanager)
                                                                         .ThenInclude(g => g.TCMSupervisor)
                                                                         .FirstOrDefaultAsync(s => s.Id == idAddendum);

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainsAddendum", _converterHelper.ToTCMAdendumViewModel(tcmadendumEntity)) });
                        */
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

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditDomain", tcmDomainViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateDomain(int id = 0, int origin = 0, int aview = 0)
        {
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans
                                                          .Include(u => u.TcmClient)
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

                        IEnumerable<SelectListItem> list_Services = _combosHelper.GetComboServicesAssessment(tcmServicePlan.Id);

                        model = new TCMDomainViewModel
                        {
                            DateIdentified = DateTime.Today,
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

            if (ModelState.IsValid)
            {
                TCMServiceEntity service = _context.TCMServices
                                                   .FirstOrDefault(s => s.Id == tcmDomainViewModel.Id_Service);
                tcmDomainViewModel.Code = service.Code;
                tcmDomainViewModel.Name = service.Name;

                TCMDomainEntity tcmDomainEntity = _context.TCMDomains
                                                          .Include(f => f.TcmServicePlan)
                                                          .FirstOrDefault(g => (g.TcmServicePlan.Id == tcmDomainViewModel.TcmServicePlan.Id
                                                                             && g.Code == service.Code));
                if (tcmDomainEntity == null)
                {
                    CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
                    tcmDomainEntity = _converterHelper.ToTCMDomainEntity(tcmDomainViewModel, true,"Service Plan", user_logged.UserName);

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
                    ModelState.AddModelError("Error", "The service already exists in the services plan");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateDomain", tcmDomainViewModel) });
                }
            }
            
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateDomain", tcmDomainViewModel) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
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

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> CreateObjetive(int id = 0, int Origin = 0, int idAddendum = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMObjetiveViewModel model = null;
            TCMDomainEntity tcmdomain = await _context.TCMDomains
                                                      .Include(g => g.TCMObjetive)
                                                      .Include(h => h.TcmServicePlan)
                                                      .ThenInclude(h => h.TCMService)
                                                      .ThenInclude(h => h.Stages)
                                                      .Include(h => h.TcmServicePlan)
                                                      .ThenInclude(h => h.TcmClient)
                                                      .FirstOrDefaultAsync(m => m.Id == id);

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") == true && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
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
                        IdObjetive = tcmdomain.TCMObjetive.Count() + 1,
                        StartDate = tcmdomain.TcmServicePlan.DateServicePlan,
                        TargetDate = tcmdomain.TcmServicePlan.DateServicePlan.AddMonths(tcmdomain.TcmServicePlan.TcmClient.Period),
                        //EndDate = DateTime.Today.Date,
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
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> CreateObjetive(TCMObjetiveViewModel tcmObjetiveViewModel, int Origin, int idAddendum = 0)
        {
            UserEntity user_logged = await _context.Users

                                                 .Include(u => u.Clinic)
                                                 .ThenInclude(c => c.Setting)

                                                 .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") == true && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {

                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }                

                if (ModelState.IsValid)
                {
                    TCMDomainEntity tcmDomain = _context.TCMDomains

                                                        .Include(f => f.TCMObjetive)

                                                        .Include(f => f.TcmServicePlan)
                                                        .ThenInclude(f => f.TcmClient)

                                                        .FirstOrDefault(f => f.Id == tcmObjetiveViewModel.Id_Domain);

                    TCMStageEntity stage = _context.TCMStages
                                                   .FirstOrDefault(s => s.Id == tcmObjetiveViewModel.Id_Stage);

                    tcmObjetiveViewModel.name = stage.Name;
                    TCMObjetiveEntity tcmObjetiveEntity = await _converterHelper.ToTCMObjetiveEntity(tcmObjetiveViewModel, true, Origin, user_logged.UserName);
                    tcmObjetiveEntity.TcmDomain = tcmDomain;

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
                
                TCMDomainEntity tcmdomain = await _context.TCMDomains
                                                              
                                                            .Include(g => g.TCMObjetive)
                                                              
                                                            .Include(g => g.TcmServicePlan)
                                                              
                                                            .FirstOrDefaultAsync(m => m.Id == tcmObjetiveViewModel.Id_Domain);
                tcmObjetiveViewModel.TcmDomain = tcmdomain;
                tcmObjetiveViewModel.Id_Stage = 0;
                tcmObjetiveViewModel.Stages = _combosHelper.GetComboStagesNotUsed(tcmdomain);
                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateObjetive", tcmObjetiveViewModel) });                
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

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
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
                    IdObjetive = objetiveEntity.IdObjetive,
                    StartDate = objetiveEntity.StartDate,
                    TargetDate = objetiveEntity.TargetDate,
                    EndDate = objetiveEntity.EndDate,
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
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> EditObjetive(TCMObjetiveViewModel tcmObjetiveViewModel, int origi = 0, int idAddendum = 0) 
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
               
                if (ModelState.IsValid)
                {
                    TCMDomainEntity tcmDomain = _context.TCMDomains
                                                        .Include(f => f.TcmServicePlan)
                                                        .ThenInclude(f => f.TcmClient)
                                                        .ThenInclude(f => f.Casemanager)
                                                        .FirstOrDefault(f => f.Id == tcmObjetiveViewModel.Id_Domain);

                    TCMStageEntity stage = _context.TCMStages
                                                   .FirstOrDefault(s => s.Id == tcmObjetiveViewModel.Id_Stage);

                    tcmObjetiveViewModel.name = stage.Name;

                    TCMObjetiveEntity tcmObjetiveEntity = await _converterHelper.ToTCMObjetiveEntity(tcmObjetiveViewModel, false, origi, user_logged.UserName);
                    tcmObjetiveEntity.TcmDomain = tcmDomain;

                    _context.Update(tcmObjetiveEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        if (origi == 0)
                        {
                            List<TCMDomainEntity> domainList = await _context.TCMDomains
                                                                             .Include(h => h.TCMObjetive)
                                                                             .Include(h => h.TcmServicePlan)
                                                                             .ThenInclude(h => h.TcmClient)
                                                                             .ThenInclude(h => h.Client)
                                                                             .ThenInclude(h => h.Clinic)
                                                                             .ThenInclude(h => h.Setting)
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
                            if (user_logged.Clinic.Setting.TCMSupervisorEdit == true)
                            {
                                ViewData["editSupervisor"] = 1;
                            }
                            else
                            {
                                ViewData["editSupervisor"] = 0;
                            }
                            
                            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainsAddendum", tcAddendumEntity.TcmDomain) });
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                
                TCMDomainEntity tcmdomain = await _context.TCMDomains

                                                          .Include(g => g.TCMObjetive)
                                                          .Include(g => g.TcmServicePlan)

                                                          .FirstOrDefaultAsync(m => m.Id == tcmObjetiveViewModel.Id_Domain);

                tcmObjetiveViewModel.TcmDomain = tcmdomain;
                tcmObjetiveViewModel.Stages = _combosHelper.GetComboStagesNotUsed(tcmdomain);
                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjetive", tcmObjetiveViewModel) });
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
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

            if (tcmServicePlan != null)
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
                                                .Include(h => h.TCMMessages)
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
                                                .Include(h => h.TCMMessages)
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
                if (user_logged.UserType.ToString() == "Manager" )
                {
                    List<TCMAdendumEntity> adendum = await _context.TCMAdendums
                                                       .Include(h => h.TcmDomain)
                                                       .ThenInclude(h => h.TCMObjetive)
                                                       .Include(h => h.TcmServicePlan)
                                                       .ThenInclude(h => (h.TcmClient))
                                                       .Include(h => h.TcmServicePlan.TcmClient.Client)
                                                       .Include(h => h.TcmServicePlan.TcmClient.Casemanager)
                                                       .Include(h => h.TCMMessages)
                                                       .Where(h => h.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == clinic.Id)
                                                       .ToListAsync();
                    ViewData["tcmClientId"] = caseNumber;
                    if (tcmClient != null)
                        ViewData["Id"] = tcmClient.Id;
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
                                                       .Include(h => h.TCMMessages)
                                                       .Where(h => h.TcmServicePlan.TcmClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName)
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
                                                                   .Include(h => h.TCMMessages)
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
        public IActionResult CreateAdendum(string caseNumber = "", int idTcmClient = 0)
        {
            TCMAdendumViewModel model = null;
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                              .FirstOrDefault(u => u.UserName == User.Identity.Name);
            CaseMannagerEntity caseManager = _context.CaseManagers.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == idTcmClient);
            if (tcmServicePlan == null || tcmServicePlan.Approved < 2)
              tcmServicePlan = new TCMServicePlanEntity();
              
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
                        TcmDominio = _combosHelper.GetComboServicesNotUsed(tcmServicePlan.Id),
                        ID_TcmServicePlan = 0,
                        ListTcmServicePlan = _combosHelper.GetComboServicesPlan(user_logged.Clinic.Id, caseManager.Id, caseNumber),
                        

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

            if(ModelState.IsValid)
            {
                if (user_logged.UserType.ToString() == "CaseManager")
                {                    
                        TCMServiceEntity tcmService = await _context.TCMServices.FirstAsync(d => d.Id == tcmAdendumViewModel.ID_TcmDominio);

                        TCMServicePlanEntity tcmServicePlan = await _context.TCMServicePlans
                                                                            .FirstOrDefaultAsync(s => s.Id == tcmAdendumViewModel.ID_TcmServicePlan);

                        TCMDomainEntity tcmDomain = _context.TCMDomains.FirstOrDefault(n => n.Code == tcmService.Code
                                                                                         && n.TcmServicePlan.Id == tcmAdendumViewModel.ID_TcmServicePlan);

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
                                                                           .Include(h => h.TCMMessages)
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

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
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
                                                              .Include(u => u.TcmDomain)
                                                              .Include(u => u.TcmServicePlan)
                                                              .ThenInclude(g => g.TcmClient)
                                                              .ThenInclude(g => g.Casemanager)
                                                              .ThenInclude(g => g.TCMSupervisor)
                                                              .FirstOrDefaultAsync(s => s.Id == id);
            if (tcmadendumEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMAdendumViewModel tcmAdendumViewModel = null;

            if (User.IsInRole("CaseManager") || (user_logged.Clinic.Setting.TCMSupervisorEdit == true))
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
                    CreatedOn = tcmadendumEntity.CreatedOn,
                    Approved = tcmadendumEntity.Approved,
                    DateTCMSign = tcmadendumEntity.DateTCMSign,
                    DateTCMSupervisorSign = tcmadendumEntity.DateTCMSupervisorSign
                };
                ViewData["aview"] = aview;
                if (user_logged.Clinic.Setting.TCMSupervisorEdit == true)
                {
                    ViewData["editSupervisor"] = 1;
                }
                else
                {
                    ViewData["editSupervisor"] = 0;
                }
                
                return View(tcmAdendumViewModel);
            }

            ViewData["aview"] = aview;
            return View(tcmAdendumViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> EditAdendum(int? id, TCMAdendumViewModel tcmAdendumViewModel, int aview = 0)
        {
            if (id != tcmAdendumViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
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

            if (user_logged.UserType.ToString() == "CaseManager" || user_logged.Clinic.Setting.TCMSupervisorEdit == true)
            {
                if (ModelState.IsValid)
                {
                    TCMAdendumEntity tcmAdendumEntity = await _converterHelper.ToTCMAdendumEntity(tcmAdendumViewModel, false, user_logged.UserName);

                    if (user_logged.UserType.ToString() == "CaseManager")
                    {
                        List<TCMMessageEntity> messages = tcmAdendumEntity.TCMMessages.Where(m => (m.Status == MessageStatus.NotRead && m.Notification == false)).ToList();
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
                                TCMServicePlan = null,
                                TCMServicePlanReview = null,
                                TCMAddendum = tcmAdendumEntity,
                                TCMDischarge = null,
                                TCMAssessment = null,
                                Title = "Update on reviewed TCM Service plan review",
                                Text = $"The TCM Service plan review of {tcmAdendumEntity.TcmServicePlan.TcmClient.Client.Name} on {tcmAdendumEntity.DateAdendum.ToShortDateString()} was rectified",
                                From = value.To,
                                To = value.From,
                                DateCreated = DateTime.Now,
                                Status = MessageStatus.NotRead,
                                Notification = true
                            };
                            _context.Add(notification);
                        }
                    }
                    
                    _context.Update(tcmAdendumEntity);

                    try
                    {
                        await _context.SaveChangesAsync();
                        ViewData["aview"] = aview;
                        if (aview == 0)
                        {
                            return RedirectToAction("Adendum", "TCMServicePlans", new { caseNumber = tcmServicePlan.TcmClient.CaseNumber, aview = aview });
                        }
                        if (aview == 1)
                        {
                            return RedirectToAction("Adendum", "TCMServicePlans", new { aview = aview });
                        }
                        if (aview == 2)
                        {
                            return RedirectToAction("AdendumApproved", "TCMServicePlans", new { aview = aview });
                        }
                        if (aview == 3)
                        {
                            return RedirectToAction("MessagesOfAddendum", "TCMMessages");
                        }
                        if (aview == 5)
                        {
                            return RedirectToAction("UpdateAddendum", "TCMServicePlans");
                        }
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
        public async Task<IActionResult> AproveAdendum(TCMAdendumViewModel model, int id, int aview = 0)
        {
            TCMAdendumEntity tcmAdendum = _context.TCMAdendums
                                                  .Include(u => u.TcmServicePlan.TcmClient)
                                                  .ThenInclude(u => u.Client)
                                                  .FirstOrDefault(u => u.Id == id);

            if (tcmAdendum != null)
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    UserEntity user_logged = _context.Users
                                                     .Include(u => u.Clinic)
                                                     .ThenInclude(u => u.Setting)
                                                     .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        tcmAdendum.Approved = 2;
                        tcmAdendum.DateTCMSupervisorSign = model.DateTCMSupervisorSign;
                        if (user_logged.Clinic.Setting.TCMSupervisorEdit == true)
                        {
                            tcmAdendum.DateAdendum = model.Date_Identified;
                            tcmAdendum.NeedsIdentified = model.Needs_Identified;
                            tcmAdendum.LongTerm = model.Long_term;
                            tcmAdendum.DateTCMSign = model.DateTCMSign;
                        }
                        _context.Update(tcmAdendum);
                        try
                        {
                            await _context.SaveChangesAsync();
                            if(aview == 0)
                            {
                                return RedirectToAction("AdendumApproved", "TCMServicePlans", new { approved = 1 });
                            }
                            if (aview == 1)
                            {
                                return RedirectToAction("Adendum", "TCMServicePlans", new { aview = aview });
                            }
                            if (aview == 2)
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
            TCMSupervisorEntity tcmSupervisor = await _context.TCMSupervisors.Include(n => n.CaseManagerList).FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
            List<TCMServicePlanEntity> servicePlan = new List<TCMServicePlanEntity>();

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
                                                         .Include(h => h.TCMMessages)
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
                                                .Include(h => h.TCMMessages)
                                                .Where(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                    && g.TcmClient.CaseNumber == caseNumber
                                                    && g.Approved == approved))
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
                                             .Include(h => h.TCMMessages)
                                             .Where(g => (g.TcmClient.Client.Clinic.Id == clinic.Id
                                                       && g.Approved == approved))
                                             .OrderBy(g => g.TcmClient.CaseNumber)
                                             .ToListAsync();
            }
            if (user_logged.UserType.ToString() == "TCMSupervisor")
            {
                List<TCMServicePlanEntity> aux = new List<TCMServicePlanEntity>();

                aux = await _context.TCMServicePlans
                                    .Include(h => h.TCMDomain)
                                    .ThenInclude(h => h.TCMObjetive)
                                    .Include(g => g.TcmClient)
                                    .ThenInclude(f => f.Client)
                                    .Include(t => t.TcmClient.Casemanager)
                                    .Include(g => g.TcmClient)
                                    .ThenInclude(f => f.Client)
                                    .ThenInclude(g => g.Clinic)
                                    .ThenInclude(f => f.Setting)
                                    .Include(h => h.TCMMessages)
                                    .Where(g => g.Approved == approved
                                      && g.TcmClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName)
                                    .OrderBy(g => g.TcmClient.CaseNumber)
                                    .ToListAsync();
                foreach (var item in tcmSupervisor.CaseManagerList)
                {
                    servicePlan.AddRange(aux.Where(n => n.TcmClient.Casemanager.Id == item.Id).ToList());
                }
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
                                            .Include(h => h.TCMMessages)
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
                                            .Include(h => h.TCMMessages)
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
                                                   .Include(h => h.TCMMessages)
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
                                                   .Include(h => h.TCMMessages)
                                                   .Where(h => h.TcmServicePlan.TcmClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                                        && h.Approved == approved)
                                                   .ToListAsync();
                ViewData["aview"] = aview;
                return View(adendum);
            }

            return View(null);

        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult EditReadOnly(int Id = 0, int origi = 0)
        {
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans
                                                          .Include(f => f.TCMDomain)
                                                          .ThenInclude(f => f.TCMObjetive)
                                                          .Include(f => f.TcmClient)
                                                          .ThenInclude(f => f.Client)
                                                          .ThenInclude(f => f.Clinic)
                                                          .ThenInclude(f => f.Setting)
                                                          .Include(f => f.TcmClient)
                                                          .ThenInclude(f => f.Casemanager)
                                                          .ThenInclude(f => f.TCMSupervisor)
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
                        TCMAssessmentEntity assessment = _context.TCMAssessment.FirstOrDefault(n => n.TcmClient.Id == tcmServicePlan.TcmClient.Id);
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
                            DateServicePlan = tcmServicePlan.DateServicePlan,
                            DateIntake = tcmServicePlan.DateIntake,
                            DateAssessment = tcmServicePlan.DateAssessment,
                            DateCertification = tcmServicePlan.DateCertification,
                            Strengths = tcmServicePlan.Strengths,
                            Weakness = tcmServicePlan.Weakness,
                            DischargerCriteria = tcmServicePlan.DischargerCriteria,
                            Id = Id,
                            TCMDomain = tcmServicePlan.TCMDomain,
                            Approved = tcmServicePlan.Approved,
                            CreatedBy = tcmServicePlan.CreatedBy,
                            CreatedOn = tcmServicePlan.CreatedOn,
                            TcmClient = tcmServicePlan.TcmClient,
                            DateSupervisorSignature = tcmServicePlan.DateSupervisorSignature,
                            Domain1 = assessment.RecommendedMentalHealth,
                            Domain2 = assessment.RecommendedPhysicalHealth,
                            Domain3 = assessment.RecommendedVocation,
                            Domain4 = assessment.RecommendedSchool,
                            Domain5 = assessment.RecommendedRecreational,
                            Domain6 = assessment.RecommendedActivities,
                            Domain7 = assessment.RecommendedHousing,
                            Domain8 = assessment.RecommendedEconomic,
                            Domain9 = assessment.RecommendedBasicNeed,
                            Domain10 = assessment.RecommendedTransportation,
                            Domain11 = assessment.RecommendedLegalImmigration,
                            Domain12 = assessment.RecommendedOther,
                            DateTcmSignature = tcmServicePlan.DateTcmSignature

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
                                                              .Include(u => u.TcmDomain)
                                                              .Include(u => u.TcmServicePlan)
                                                              .ThenInclude(u => u.TcmClient)
                                                              .ThenInclude(u => u.Casemanager)
                                                              .ThenInclude(u => u.TCMSupervisor)
                                                              .FirstOrDefaultAsync(s => s.Id == id);
            if (tcmadendumEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
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
                    CreatedOn = tcmadendumEntity.CreatedOn,
                    DateTCMSign = tcmadendumEntity.DateTCMSign,
                    DateTCMSupervisorSign = tcmadendumEntity.DateTCMSupervisorSign
                };
                ViewData["aview"] = aview;
                if (user_logged.Clinic.Setting.TCMSupervisorEdit == true)
                {
                    ViewData["editSupervisor"] = 1;
                }
                else
                {
                    ViewData["editSupervisor"] = 0;
                }
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

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult AddMessageAddendumEntity(int id = 0, int origi = 0)
        {
            if (id == 0)
            {
                return View(new TCMMessageViewModel());
            }
            else
            {
                TCMMessageViewModel model = new TCMMessageViewModel()
                {
                    IdTCMAddendum = id,
                    Origin = origi
                };

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> AddMessageAddendumEntity(TCMMessageViewModel messageViewModel)
        {
            if (ModelState.IsValid)
            {
                TCMMessageEntity model = await _converterHelper.ToTCMMessageEntity(messageViewModel, true);
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                model.From = user_logged.UserName;
                model.To = model.TCMAddendum.CreatedBy;
                _context.Add(model);
                await _context.SaveChangesAsync();
            }

            if (messageViewModel.Origin == 1)
                return RedirectToAction("Adendum", new { aview = messageViewModel.Origin });
            if (messageViewModel.Origin == 2)
                return RedirectToAction("Notifications", "TCMMessages");
            
            return RedirectToAction("AdendumApproved", new { approved = 1 });

        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public IActionResult PrintServicePlan(int id)
        {
            TCMServicePlanEntity servicePlan = _context.TCMServicePlans                                                         

                                                       .Include(sp => sp.TcmClient)
                                                       .ThenInclude(c => c.Client)
                                                       .ThenInclude(cl => cl.Clients_Diagnostics)
                                                       .ThenInclude(d => d.Diagnostic)

                                                       .Include(sp => sp.TCMSupervisor)
                                                       .ThenInclude(s => s.Clinic)

                                                       .Include(sp => sp.TcmClient)
                                                       .ThenInclude(c => c.Casemanager)

                                                       .Include(sp => sp.TCMDomain.Where(d => d.Origin == "Service Plan"))
                                                       .ThenInclude(d => d.TCMObjetive)

                                                       .FirstOrDefault(sp => (sp.Id == id && sp.Approved == 2));
            if (servicePlan == null)
            {
                return RedirectToAction("Home/Error404");
            }

            if (servicePlan.TCMSupervisor.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
            {
                Stream stream = _reportHelper.FloridaSocialHSTCMServicePlan(servicePlan);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            if (servicePlan.TCMSupervisor.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
            {
                Stream stream = _reportHelper.CommunityHTCTCMServicePlan(servicePlan);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            if (servicePlan.TCMSupervisor.Clinic.Name == "DREAMS MENTAL HEALTH INC")
            {
                Stream stream = _reportHelper.DreamsMentalHealthTCMServicePlan(servicePlan);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            if (servicePlan.TCMSupervisor.Clinic.Name == "SAPPHIRE MENTAL HEALTH CENTER LLC")
            {
                Stream stream = _reportHelper.SapphireMHCTCMServicePlan(servicePlan);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            if (servicePlan.TCMSupervisor.Clinic.Name == "ORION MENTAL HEALTH CENTER LLC")
            {
                Stream stream = _reportHelper.OrionMHCTCMServicePlan(servicePlan);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            if (servicePlan.TCMSupervisor.Clinic.Name == "MY FLORIDA CASE MANAGEMENT SERVICES LLC")
            {
                Stream stream = _reportHelper.MyFloridaTCMServicePlan(servicePlan);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            return null;
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> UpdateServicePlan(int id = 0)
        {
            if (id == 1)
            {
                ViewBag.FinishEdition = "Y";
            }

            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            TCMSupervisorEntity tcmSupervisor = _context.TCMSupervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic || !user_logged.Clinic.Setting.TCMSupervisorEdit)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (User.IsInRole("TCMSupervisor"))
            {
                return View(await _context.TCMServicePlans
                                          .Include(n => n.TcmClient)
                                          .ThenInclude(n => n.Casemanager)
                                          .Include(n => n.TcmClient)
                                          .ThenInclude(n => n.Client)
                                          .ThenInclude(n => n.Clinic)
                                          .ThenInclude(n => n.Setting)

                                          .Include(n => n.TCMDomain)
                                          .ThenInclude(n => n.TCMObjetive)

                                          .Where(w => (w.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && w.TcmClient.Casemanager.TCMSupervisor.Id == tcmSupervisor.Id
                                                    && w.Approved == 2))
                                          .ToListAsync());
            }
            else
            {
                return View();
            }
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> EditObjetiveReadOnly(int id = 0, int origi = 0, int idAddendum = 0)
        {
            UserEntity user_logged = await _context.Users

                                                  .Include(u => u.Clinic)
                                                  .ThenInclude(c => c.Setting)

                                                  .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if ( User.IsInRole("TCMSupervisor"))
            {

                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                TCMObjetiveEntity objetiveEntity = await _context.TCMObjetives
                                                                 .Include(g => g.TcmDomain)
                                                                 .ThenInclude(g => g.TcmServicePlan)
                                                                 .ThenInclude(g => g.TcmClient)
                                                                 .ThenInclude(g => g.Client)
                                                                 .ThenInclude(g => g.Clinic)
                                                                 .ThenInclude(g => g.Setting)
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
                    IdObjetive = objetiveEntity.IdObjetive,
                    StartDate = objetiveEntity.StartDate,
                    TargetDate = objetiveEntity.TargetDate,
                    EndDate = objetiveEntity.EndDate,
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
        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> EditObjetiveReadOnly(TCMObjetiveViewModel tcmObjetiveViewModel, int origi = 0, int idAddendum = 0)
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (User.IsInRole("TCMSupervisor"))
            {

                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                if (ModelState.IsValid)
                {
                    TCMDomainEntity tcmDomain = _context.TCMDomains
                                                        .Include(f => f.TcmServicePlan)
                                                        .ThenInclude(f => f.TcmClient)
                                                        .ThenInclude(f => f.Casemanager)
                                                        .Include(f => f.TcmServicePlan)
                                                        .ThenInclude(f => f.TcmClient)
                                                        .ThenInclude(f => f.Client)
                                                        .ThenInclude(f => f.Clinic)
                                                        .ThenInclude(f => f.Setting)
                                                        .FirstOrDefault(f => f.Id == tcmObjetiveViewModel.Id_Domain);

                    TCMStageEntity stage = _context.TCMStages
                                                   .FirstOrDefault(s => s.Id == tcmObjetiveViewModel.Id_Stage);

                    tcmObjetiveViewModel.name = stage.Name;

                    TCMObjetiveEntity tcmObjetiveEntity = await _converterHelper.ToTCMObjetiveEntity(tcmObjetiveViewModel, false, origi, user_logged.UserName);
                    tcmObjetiveEntity.TcmDomain = tcmDomain;

                    _context.Update(tcmObjetiveEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        if (origi == 0)
                        {
                            List<TCMDomainEntity> domainList = await _context.TCMDomains
                                                                             .Include(h => h.TCMObjetive)
                                                                             .Include(h => h.TcmServicePlan)
                                                                             .ThenInclude(h => h.TcmClient)
                                                                             .ThenInclude(h => h.Client)
                                                                             .ThenInclude(h => h.Clinic)
                                                                             .ThenInclude(h => h.Setting)
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

                TCMDomainEntity tcmdomain = await _context.TCMDomains

                                                          .Include(g => g.TCMObjetive)
                                                          .Include(g => g.TcmServicePlan)
                                                          .ThenInclude(g => g.TcmClient)
                                                          .ThenInclude(g => g.Client)
                                                          .ThenInclude(g => g.Clinic)
                                                          .ThenInclude(g => g.Setting)

                                                          .FirstOrDefaultAsync(m => m.Id == tcmObjetiveViewModel.Id_Domain);

                tcmObjetiveViewModel.TcmDomain = tcmdomain;
                tcmObjetiveViewModel.Stages = _combosHelper.GetComboStagesNotUsed(tcmdomain);
                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjetiveReadOnly", tcmObjetiveViewModel) });
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> EditDomainReadOnly(int? id, int origi = 0, int aview = 0, int idAddendum = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMDomainEntity tcmDomainEntity = await _context.TCMDomains
                                                            .Include(u => u.TcmServicePlan)
                                                            .ThenInclude(g => g.TcmClient)
                                                            .ThenInclude(g => g.Client)
                                                            .ThenInclude(g => g.Clinic)
                                                            .ThenInclude(g => g.Setting)
                                                            .FirstOrDefaultAsync(s => s.Id == id);
            if (tcmDomainEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMDomainViewModel tcmDomainViewModel = null;
                        
            if (User.IsInRole("TCMSupervisor"))
            {
                tcmDomainViewModel = _converterHelper.ToTCMDomainViewModel(tcmDomainEntity);

                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = tcmDomainEntity.Code + ' ' + tcmDomainEntity.Name,
                    Value = $"{tcmDomainEntity.Id}"
                });
                tcmDomainViewModel.Services = list;
            }

            ViewData["origi"] = origi;
            ViewData["aview"] = aview;
            ViewData["idAddendum"] = idAddendum;

            return View(tcmDomainViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> EditDomainReadOnly(int id, TCMDomainViewModel tcmDomainViewModel, int origi = 0, int aview = 0, int idAddendum = 0)
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
                TCMDomainEntity tcmDomainEntity = _converterHelper.ToTCMDomainEntity(tcmDomainViewModel, false, tcmDomainViewModel.Origin, user_logged.UserName);
                _context.Update(tcmDomainEntity);

                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMDomainEntity> domainList = await _context.TCMDomains
                                                                     .Include(h => h.TCMObjetive)
                                                                     .Include(g => g.TcmServicePlan)
                                                                     .ThenInclude(g => g.TcmClient)
                                                                     .ThenInclude(g => g.Client)
                                                                     .ThenInclude(g => g.Clinic)
                                                                     .ThenInclude(g => g.Setting)
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

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditDomainReadOnly", tcmDomainViewModel) });
        }

        [Authorize(Roles = "TCMSupervisor, Manager, CaseManager")]
        public IActionResult Details(int Id = 0, int origi = 0)
        {
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans
                                                          .Include(f => f.TCMDomain)
                                                          .ThenInclude(f => f.TCMObjetive)
                                                          .Include(f => f.TcmClient)
                                                          .ThenInclude(f => f.Client)
                                                          .ThenInclude(f => f.Clinic)
                                                          .ThenInclude(f => f.Setting)
                                                          .Include(f => f.TcmClient)
                                                          .ThenInclude(f => f.Casemanager)
                                                          .ThenInclude(f => f.TCMSupervisor)
                                                          .FirstOrDefault(u => u.Id == Id);
            TCMServicePlanViewModel model;

            if (tcmServicePlan != null && tcmServicePlan.Approved == 2)
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
                            DateServicePlan = tcmServicePlan.DateServicePlan,
                            DateIntake = tcmServicePlan.DateIntake,
                            DateAssessment = tcmServicePlan.DateAssessment,
                            DateCertification = tcmServicePlan.DateCertification,
                            Strengths = tcmServicePlan.Strengths,
                            Weakness = tcmServicePlan.Weakness,
                            DischargerCriteria = tcmServicePlan.DischargerCriteria,
                            Id = Id,
                            TCMDomain = tcmServicePlan.TCMDomain,
                            Approved = tcmServicePlan.Approved,
                            CreatedBy = tcmServicePlan.CreatedBy,
                            CreatedOn = tcmServicePlan.CreatedOn,
                            TcmClient = tcmServicePlan.TcmClient,
                            DateSupervisorSignature = tcmServicePlan.DateSupervisorSignature

                        };

                        model.TCMDomain = model.TCMDomain.Where(n => (n.Origin == "Service Plan")).ToList();
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    return RedirectToAction("NotAuthorized", "Account");
               
            }

            return RedirectToAction("Index", "TCMServicePlans", new { caseNumber = tcmServicePlan.TcmClient.CaseNumber });
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> UpdateAddendum(int id = 0)
        {
            if (id == 1)
            {
                ViewBag.FinishEdition = "Y";
            }

            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            TCMSupervisorEntity tcmSupervisor = _context.TCMSupervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic || !user_logged.Clinic.Setting.TCMSupervisorEdit)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (User.IsInRole("TCMSupervisor"))
            {
                return View(await _context.TCMAdendums
                                          .Include(n => n.TcmServicePlan)
                                          .ThenInclude(n => n.TcmClient)
                                          .ThenInclude(n => n.Casemanager)
                                          .ThenInclude(n => n.TCMSupervisor)
                                          .Include(n => n.TcmServicePlan)
                                          .ThenInclude(n => n.TcmClient)
                                          .ThenInclude(n => n.Client)
                                          .ThenInclude(n => n.Clinic)
                                          .ThenInclude(n => n.Setting)

                                          .Include(n => n.TcmDomain)
                                          .ThenInclude(n => n.TCMObjetive)

                                          .Where(w => (w.TcmServicePlan.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && w.TcmServicePlan.TcmClient.Casemanager.TCMSupervisor.Id == tcmSupervisor.Id
                                                    && w.Approved == 2))
                                          .ToListAsync());
            }
            else
            {
                return View();
            }
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public IActionResult PrintAdendum(int id)
        {
            TCMAdendumEntity adendum = _context.TCMAdendums

                                               .Include(t => t.TcmServicePlan)
                                               .ThenInclude(tc => tc.TcmClient)
                                               .ThenInclude(c => c.Client)

                                               .Include(t => t.TcmDomain)
                                               .ThenInclude(tc => tc.TCMObjetive)

                                               .Include(t => t.TcmServicePlan)
                                               .ThenInclude(sp => sp.TCMSupervisor)
                                               .ThenInclude(s => s.Clinic)

                                               .Include(t => t.TcmServicePlan)
                                               .ThenInclude(sp => sp.TcmClient)
                                               .ThenInclude(c => c.Casemanager)

                                               .FirstOrDefault(sp => (sp.Id == id && sp.Approved == 2));
            if (adendum == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMAdendum(adendum);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> ReturnTo(int? id, int tcmClientId = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMServicePlanEntity serviceplan = await _context.TCMServicePlans.FirstOrDefaultAsync(s => s.Id == id);
            if (serviceplan == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                serviceplan.Approved = 0;
                _context.TCMServicePlans.Update(serviceplan);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("TCMCaseHistory", "TCMClients", new { id = tcmClientId });
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> AddendumReturnTo(int? id, int tcmClientId = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMAdendumEntity addendum = await _context.TCMAdendums.FirstOrDefaultAsync(s => s.Id == id);
            if (addendum == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                addendum.Approved = 0;
                _context.TCMAdendums.Update(addendum);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("TCMCaseHistory", "TCMClients", new { id = tcmClientId });
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMServicePlanEntity servicePlan = await _context.TCMServicePlans
                                                             .Include(n => n.TcmClient)
                                                             .FirstOrDefaultAsync(d => d.Id == id);

            if (servicePlan == null)
            {
                return RedirectToAction("Home/Error404");
            }

            List<TCMMessageEntity> messageEntity = await _context.TCMMessages
                                                                 .Include(n => n.TCMServicePlan)
                                                                 .Where(d => d.TCMServicePlan.Id == id)
                                                                 .ToListAsync();

            _context.TCMMessages.RemoveRange(messageEntity);
            await _context.SaveChangesAsync();

            List<TCMDomainEntity> domains = await _context.TCMDomains
                                                          .Include(n => n.TcmServicePlan)
                                                          .Include(n => n.TCMObjetive)
                                                          .Where(d => d.TcmServicePlan.Id == id)
                                                          .ToListAsync();

            foreach (var item in domains)
            {
                _context.TCMObjetives.RemoveRange(item.TCMObjetive);
                await _context.SaveChangesAsync();
            }
           
            List<TCMNoteEntity> notes = await _context.TCMNote
                                                      .Include(n => n.TCMClient)
                                                      .Include(n => n.TCMNoteActivity)
                                                      .Where(d => d.TCMClient.Id == servicePlan.TcmClient.Id)
                                                      .ToListAsync();

            foreach (var item in notes)
            {
                _context.TCMNoteActivity.RemoveRange(item.TCMNoteActivity);
                await _context.SaveChangesAsync();
            }

            _context.TCMNote.RemoveRange(notes);
            await _context.SaveChangesAsync();

            _context.TCMDomains.RemoveRange(domains);
            await _context.SaveChangesAsync();

            TCMDischargeEntity discharge = await _context.TCMDischarge
                                                         .FirstOrDefaultAsync(d => d.TcmServicePlan.Id == id);

            _context.TCMDischarge.Remove(discharge);
            await _context.SaveChangesAsync();

            List<TCMAdendumEntity> addendums = await _context.TCMAdendums
                                                             .Where(d => d.TcmServicePlan.Id == id)
                                                             .ToListAsync();

            _context.TCMAdendums.RemoveRange(addendums);
            await _context.SaveChangesAsync();

            TCMServicePlanReviewEntity servicePlanReview = await _context.TCMServicePlanReviews
                                                                         .FirstOrDefaultAsync(d => d.TcmServicePlan.Id == id);

            _context.TCMServicePlanReviews.Remove(servicePlanReview);
            await _context.SaveChangesAsync();

            _context.TCMServicePlans.Remove(servicePlan);
            await _context.SaveChangesAsync();

            return RedirectToAction("TCMCaseHistory", "TCMClients", new { id = servicePlan.TcmClient.Id });

        }
       
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteAddendum(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

           List<TCMMessageEntity> messageEntity = await _context.TCMMessages
                                                                .Include(n => n.TCMAddendum)
                                                                .Where(d => d.TCMAddendum.Id == id)
                                                                .ToListAsync();

            _context.TCMMessages.RemoveRange(messageEntity);
            await _context.SaveChangesAsync();
                       
            TCMAdendumEntity addendum = await _context.TCMAdendums
                                                      .Include(n => n.TcmDomain)
                                                      .Include(n => n.TcmServicePlan)
                                                      .ThenInclude(n => n.TcmClient)
                                                      .FirstOrDefaultAsync(d => d.Id == id);

            /*List<TCMNoteActivityEntity> noteActivity = await _context.TCMNoteActivity
                                                                     .Include(n => n.TCMNote)
                                                                     .Where(m => m.TCMDomain.Id == addendum.TcmDomain.Id)
                                                                     .ToListAsync();

            _context.TCMNoteActivity.RemoveRange(noteActivity);
            await _context.SaveChangesAsync();*/

            _context.TCMDomains.RemoveRange(addendum.TcmDomain);
            await _context.SaveChangesAsync();

            _context.TCMAdendums.Remove(addendum);
            await _context.SaveChangesAsync();

            return RedirectToAction("TCMCaseHistory", "TCMClients", new { id = addendum.TcmServicePlan.TcmClient.Id });

        }

        [Authorize(Roles = "Manager, Frontdesk, TCMSupervisor, CaseManager")]
        public IActionResult AuditTCMServicePlan(int idTCMClient = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic || !user_logged.Clinic.Setting.MHProblems)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<AuditTCMServicePlan> auditServicePlan_List = new List<AuditTCMServicePlan>();
            AuditTCMServicePlan auditServicePlan = new AuditTCMServicePlan();

            TCMAssessmentEntity assessment = _context.TCMAssessment
                                                     .Include(n => n.TcmClient)
                                                     .FirstOrDefault(n => n.TcmClient_FK == idTCMClient);
            List<TCMDomainEntity> domains = _context.TCMDomains
                                                    .Include(n => n.TcmServicePlan)
                                                    .ThenInclude(n => n.TcmClient)
                                                    .Where(n => n.TcmServicePlan.TcmClient.Id == idTCMClient)
                                                    .ToList();
            //apertura

            if (assessment != null)
            {
                if (assessment.RecommendedMentalHealth == true)
                {
                    TCMDomainEntity domain = domains.FirstOrDefault(n => n.Code == "01");
                    if (domain != null)
                    {
                        auditServicePlan.Name = "01-Mental Health";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 2;
                    }
                    else
                    {
                        auditServicePlan.Name = "01-Mental Health";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 0;
                    }
                    auditServicePlan_List.Add(auditServicePlan);
                    auditServicePlan = new AuditTCMServicePlan();
                }
                if (assessment.RecommendedPhysicalHealth == true)
                {
                    TCMDomainEntity domain = domains.FirstOrDefault(n => n.Code == "02");
                    if (domain != null)
                    {
                        auditServicePlan.Name = "02-Physical Health";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 2;
                    }
                    else
                    {
                        auditServicePlan.Name = "02-Physical Health";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 0;
                    }
                    auditServicePlan_List.Add(auditServicePlan);
                    auditServicePlan = new AuditTCMServicePlan();
                }
                if (assessment.RecommendedVocation == true)
                {
                    TCMDomainEntity domain = domains.FirstOrDefault(n => n.Code == "03");
                    if (domain != null)
                    {
                        auditServicePlan.Name = "03-Vocational";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 2;
                    }
                    else
                    {
                        auditServicePlan.Name = "03-Vocational";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 0;
                    }
                    auditServicePlan_List.Add(auditServicePlan);
                    auditServicePlan = new AuditTCMServicePlan();
                }
                if (assessment.RecommendedSchool == true)
                {
                    TCMDomainEntity domain = domains.FirstOrDefault(n => n.Code == "04");
                    if (domain != null)
                    {
                        auditServicePlan.Name = "04-School";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 2;
                    }
                    else
                    {
                        auditServicePlan.Name = "04-School";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 0;
                    }
                    auditServicePlan_List.Add(auditServicePlan);
                    auditServicePlan = new AuditTCMServicePlan();
                }
                if (assessment.RecommendedRecreational == true)
                {
                    TCMDomainEntity domain = domains.FirstOrDefault(n => n.Code == "05");
                    if (domain != null)
                    {
                        auditServicePlan.Name = "05-Recreational ";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 2;
                    }
                    else
                    {
                        auditServicePlan.Name = "05-Recreational ";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 0;
                    }
                    auditServicePlan_List.Add(auditServicePlan);
                    auditServicePlan = new AuditTCMServicePlan();
                }
                if (assessment.RecommendedActivities == true)
                {
                    TCMDomainEntity domain = domains.FirstOrDefault(n => n.Code == "06");
                    if (domain != null)
                    {
                        auditServicePlan.Name = "06-Activities of Daily Living ";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 2;
                    }
                    else
                    {
                        auditServicePlan.Name = "06-Activities of Daily Living ";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 0;
                    }
                    auditServicePlan_List.Add(auditServicePlan);
                    auditServicePlan = new AuditTCMServicePlan();
                }
                if (assessment.RecommendedHousing == true)
                {
                    TCMDomainEntity domain = domains.FirstOrDefault(n => n.Code == "07");
                    if (domain != null)
                    {
                        auditServicePlan.Name = "07-Housing";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 2;
                    }
                    else
                    {
                        auditServicePlan.Name = "07-Housing";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 0;
                    }
                    auditServicePlan_List.Add(auditServicePlan);
                    auditServicePlan = new AuditTCMServicePlan();
                }
                if (assessment.RecommendedEconomic == true)
                {
                    TCMDomainEntity domain = domains.FirstOrDefault(n => n.Code == "08");
                    if (domain != null)
                    {
                        auditServicePlan.Name = "08-Economic";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 2;
                    }
                    else
                    {
                        auditServicePlan.Name = "08-Economic";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 0;
                    }
                    auditServicePlan_List.Add(auditServicePlan);
                    auditServicePlan = new AuditTCMServicePlan();
                }
                if (assessment.RecommendedBasicNeed == true)
                {
                    TCMDomainEntity domain = domains.FirstOrDefault(n => n.Code == "09");
                    if (domain != null)
                    {
                        auditServicePlan.Name = "09-Basic Needs";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 2;
                    }
                    else
                    {
                        auditServicePlan.Name = "09-Basic Needs";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 0;
                    }
                    auditServicePlan_List.Add(auditServicePlan);
                    auditServicePlan = new AuditTCMServicePlan();
                }
                if (assessment.RecommendedTransportation == true)
                {
                    TCMDomainEntity domain = domains.FirstOrDefault(n => n.Code == "10");
                    if (domain != null)
                    {
                        auditServicePlan.Name = "10-Transportation";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 2;
                    }
                    else
                    {
                        auditServicePlan.Name = "10-Transportation";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 0;
                    }
                    auditServicePlan_List.Add(auditServicePlan);
                    auditServicePlan = new AuditTCMServicePlan();
                }
                if (assessment.RecommendedLegalImmigration == true)
                {
                    TCMDomainEntity domain = domains.FirstOrDefault(n => n.Code == "11");
                    if (domain != null)
                    {
                        auditServicePlan.Name = "11-Legal";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 2;
                    }
                    else
                    {
                        auditServicePlan.Name = "11-Legal";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 0;
                    }
                    auditServicePlan_List.Add(auditServicePlan);
                    auditServicePlan = new AuditTCMServicePlan();
                }
                if (assessment.RecommendedOther == true)
                {
                    TCMDomainEntity domain = domains.FirstOrDefault(n => n.Code == "12");
                    if (domain != null)
                    {
                        auditServicePlan.Name = "12-Other";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 2;
                    }
                    else
                    {
                        auditServicePlan.Name = "12-Other";
                        auditServicePlan.Description = "Assessment";
                        auditServicePlan.Date = domain.DateIdentified.ToShortDateString();
                        auditServicePlan.Active = 0;
                    }
                    auditServicePlan_List.Add(auditServicePlan);
                    auditServicePlan = new AuditTCMServicePlan();
                }
            }
           
            return View(auditServicePlan_List);
        }
    }
}
