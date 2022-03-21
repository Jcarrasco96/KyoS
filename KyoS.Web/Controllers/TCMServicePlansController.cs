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
        public async Task<IActionResult> Index(int idError = 0)
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

                servicePlan = await _context.TCMServicePlans
                                                         .Include(h => h.TCMDomain)
                                                         .ThenInclude(h => h.TCMObjetive)
                                                         .Include(g => g.TcmClient)
                                                         .ThenInclude(f => f.Client)
                                                         .Include(t => t.TcmClient.Casemanager)
                                                         .Where(g => (g.TcmClient.Casemanager.Id == caseManager.Id))
                                                         .OrderBy(g => g.TcmClient.CaseNumber)
                                                         .ToListAsync();
                
                  
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
              
            }

            return View(GetOnlyServicePlan(servicePlan, clinic.Id));

        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Create(int id)
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
                            TcmClients = list_Client,
                            ID_Clinic = user_logged.Clinic.Id,
                            Clinics = list_Clinins,
                            ID_Status = 1,
                            status = _combosHelper.GetComboClientStatus(),
                            CaseNumber = _context.TCMClient.FirstOrDefault(u => u.Id == id).CaseNumber,
                            Date_ServicePlan = DateTime.Today.Date,
                            Date_Intake = DateTime.Today.Date,
                            Date_Assessment = DateTime.Today.Date,
                            Date_Certification = DateTime.Today.Date,
                            
                        };
                        return View(model);
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("Index", "TCMServicePlans");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Create(TCMServicePlanViewModel tcmServicePlanViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic) 
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMServicePlanEntity tcmServicePlanEntity = await _context.TCMServicePlans.FindAsync(tcmServicePlanViewModel.ID_TcmClient);
                if (tcmServicePlanEntity == null)
                {
                    tcmServicePlanEntity = await _converterHelper.ToTCMServicePlanEntity(tcmServicePlanViewModel, true);
                    _context.Add(tcmServicePlanEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                       
                        return RedirectToAction("Index", "TCMServicePlans");
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
        public async Task<IActionResult> Edit(int? Id)
        {
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans
                                                          .Include(f => f.TcmClient)
                                                          .FirstOrDefault(u => u.Id == Id);
            TCMServicePlanViewModel model;

            if (tcmServicePlan != null)
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
                            dischargerCriteria = tcmServicePlan.DischargerCriteria
                        };
                        //model = _converterHelper.ToTCMServicePlanViewModel(tcmServicePlan);
                        return View(model);
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("Index", "TCMServicePlans");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Edit(int id, TCMServicePlanViewModel serviceplanViewModel)
        {
            UserEntity user_logged = _context.Users
                                              .Include(u => u.Clinic)
                                              .FirstOrDefault(u => u.UserName == User.Identity.Name);

             if (ModelState.IsValid)
             {
                 
                  TCMServicePlanEntity tcmServicePlanEntity = await _converterHelper.ToTCMServicePlanEntity(serviceplanViewModel, false);
                  _context.Update(tcmServicePlanEntity);
                  try
                  {
                       await _context.SaveChangesAsync();
                     
                       return RedirectToAction("Index", "TCMServicePlans");
                  }
                  catch (System.Exception ex)
                  {
                       ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                  }
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", serviceplanViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> FinishEditing(int id)
        {
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans.Include(u => u.TcmClient)
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

                            return RedirectToAction("Index", "TCMServicePlans");
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

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> AproveServicePlan()
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
            TCMSupervisorEntity tcmSupervisor = await _context.TCMSupervisors.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);

            if (user_logged.UserType.ToString() == "TCMSupervisor")
            {

                List<TCMServicePlanEntity> servicePlan = await _context.TCMServicePlans
                                                    .Include(h => h.TCMDomain)
                                                    .ThenInclude(h => h.TCMObjetive)
                                                    .Include(g => g.TcmClient)
                                                    .ThenInclude(f => f.Client)
                                                    .Include(t => t.TcmClient.Casemanager)
                                                    .Where(g => (g.Approved == 1
                                                        && g.TcmClient.Client.Clinic.Id == clinic.Id))
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
                                                     .Where(g => (g.Approved == 1
                                                        && g.TcmClient.Client.Clinic.Id == clinic.Id))
                                                     .ToListAsync();
                return View(servicePlan);
            }
            return View(null);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> AproveServicePlan(int id)
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
                        _context.Update(tcmServicePlan);
                        try
                        {
                            await _context.SaveChangesAsync();

                            return RedirectToAction("Index", "TCMServicePlans");
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
        public async Task<IActionResult> ClosedServicePlan(int idError = 0)
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
                                                       && g.Status == StatusType.Close))
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
                                                         && g.Status == StatusType.Close))
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
                                                        && g.Status == StatusType.Close))
                                                     .ToListAsync();
                return View(servicePlan);
            }

            return View(null);

        }

        [Authorize(Roles = "Manager, CaseManager")]
        public async Task<IActionResult> EditDomain(int? id)
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

            return View(tcmDomainViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> EditDomain(int id, TCMDomainViewModel tcmDomainViewModel)
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
                TCMDomainEntity tcmDomainEntity = await _converterHelper.ToTCMDomainEntity(tcmDomainViewModel, false);
                _context.Update(tcmDomainEntity);

                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMServicePlanEntity> servicePlan = await _context.TCMServicePlans
                                                        .Include(h => h.TCMDomain)
                                                        .ThenInclude(h => h.TCMObjetive)
                                                        .Include(g => g.TcmClient)
                                                        .ThenInclude(f => f.Client)
                                                        .Include(t => t.TcmClient.Casemanager)
                                                        .Where(g => (g.TcmClient.Casemanager.LinkedUser == user_logged.UserName))
                                                        .OrderBy(g => g.TcmClient.CaseNumber)
                                                        .ToListAsync();
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_TCMServicePlan", GetOnlyServicePlan(servicePlan, user_logged.Clinic.Id)) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditDomain", tcmDomainViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateDomain(int id)
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
                            Date_Identified = DateTime.Today.Date,
                            Services = list_Services,
                            TcmServicePlan = tcmServicePlan,
                            Id_ServicePlan = id,
                        };
                        return View(model);
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
                return RedirectToAction("NotAuthorized", "Account");
            }

            return RedirectToAction("Index", "TCMServicePlans");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateDomain(TCMDomainViewModel tcmDomainViewModel)
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
                    tcmDomainEntity = await _converterHelper.ToTCMDomainEntity(tcmDomainViewModel, true);
                    _context.Add(tcmDomainEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMServicePlanEntity> servicePlan = await _context.TCMServicePlans
                                                        .Include(h => h.TCMDomain)
                                                        .ThenInclude(h => h.TCMObjetive)
                                                        .Include(g => g.TcmClient)
                                                        .ThenInclude(f => f.Client)
                                                        .Include(t => t.TcmClient.Casemanager)
                                                        .Where(g => (g.TcmClient.Casemanager.LinkedUser == user_logged.UserName))
                                                        .OrderBy(g => g.TcmClient.CaseNumber)
                                                        .ToListAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_TCMServicePlan", GetOnlyServicePlan(servicePlan, user_logged.Clinic.Id)) });
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
        public async Task<IActionResult> DeleteDomain(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMDomainEntity tcmDomainEntity = await _context.TCMDomains.FirstOrDefaultAsync(s => s.Id == id);
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
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateObjetive(int id = 0)
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
                        Id_Stage = 0,
                        Stages = _combosHelper.GetComboStagesNotUsed(tcmdomain),
                        Id_Domain = tcmdomain.Id,
                        ID_Objetive = tcmdomain.TCMObjetive.Count() + 1,
                        Start_Date = DateTime.Today.Date,
                        Target_Date = DateTime.Today.Date,
                        End_Date = DateTime.Today.Date,
                        task = "es para que veas el problema del textarea",
                        //long_Term = "es para que veas el problema del textarea",
                    };

                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateObjetive(TCMObjetiveViewModel tcmObjetiveViewModel)
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
                                                        .FirstOrDefault(f => f.Id == tcmObjetiveViewModel.Id_Domain);
                TCMObjetiveEntity tcmObjetiveEntity = await _converterHelper.ToTCMObjetiveEntity(tcmObjetiveViewModel, true);
                tcmObjetiveEntity.TcmDomain = tcmDomain;

                if (ModelState.IsValid)
                {

                    _context.Add(tcmObjetiveEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMServicePlanEntity> servicePlan = await _context.TCMServicePlans
                                                        .Include(h => h.TCMDomain)
                                                        .ThenInclude(h => h.TCMObjetive)
                                                        .Include(g => g.TcmClient)
                                                        .ThenInclude(f => f.Client)
                                                        .Include(t => t.TcmClient.Casemanager)
                                                        .Where(g => (g.TcmClient.Casemanager.LinkedUser == user_logged.UserName))
                                                        .OrderBy(g => g.TcmClient.CaseNumber)
                                                        .ToListAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_TCMServicePlan", GetOnlyServicePlan(servicePlan, user_logged.Clinic.Id)) });
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
        public async Task<IActionResult> EditObjetive(int id = 0)
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
                    // long_Term = objetiveEntity.Long_Term,
                };

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
        public async Task<IActionResult> EditObjetive(TCMObjetiveViewModel tcmObjetiveViewModel)
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
                                                    .FirstOrDefault(f => f.Id == tcmObjetiveViewModel.Id_Domain);
                TCMObjetiveEntity tcmObjetiveEntity = await _converterHelper.ToTCMObjetiveEntity(tcmObjetiveViewModel, false);
                tcmObjetiveEntity.TcmDomain = tcmDomain;

                if (ModelState.IsValid)
                {

                    _context.Update(tcmObjetiveEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMServicePlanEntity> servicePlan = await _context.TCMServicePlans
                                                        .Include(h => h.TCMDomain)
                                                        .ThenInclude(h => h.TCMObjetive)
                                                        .Include(g => g.TcmClient)
                                                        .ThenInclude(f => f.Client)
                                                        .Include(t => t.TcmClient.Casemanager)
                                                        .Where(g => (g.TcmClient.Casemanager.LinkedUser == user_logged.UserName))
                                                        .OrderBy(g => g.TcmClient.CaseNumber)
                                                        .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_TCMServicePlan", GetOnlyServicePlan(servicePlan, user_logged.Clinic.Id)) });
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
        public async Task<IActionResult> DeleteObjetive(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMObjetiveEntity tcmObjetiveEntity = await _context.TCMObjetives.FirstOrDefaultAsync(s => s.Id == id);
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
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }

        public  List<TCMServicePlanEntity> GetOnlyServicePlan(List <TCMServicePlanEntity> servicePlan, int idClinic)
        {
            
            List<TCMAdendumEntity> adendum = null;
            for (int i = 0; i < servicePlan.Count(); i++)
            {
                adendum = _context.TCMAdendums
                                              .Include(h => h.TcmDomain)
                                              .ThenInclude(h => h.TCMObjetive)
                                              .Include(g => g.TcmServicePlan)
                                              .Where(g => (g.TcmServicePlan.TcmClient.Client.Clinic.Id == idClinic
                                                && g.TcmServicePlan.Id == servicePlan[i].Id))
                                              .ToList();
                for (int j = 0; j < adendum.Count; j++)
                {
                    servicePlan[i].TCMDomain.Remove(adendum[j].TcmDomain);
                }
            }
            return servicePlan;
        }

        
    }
}
