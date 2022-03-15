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
                                                         .Where(g => (g.TcmClient.Casemanager.Id == caseManager.Id))
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
                                                        .Where(g => (g.TcmClient.Client.Clinic.Id == clinic.Id))
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
                                                     .Where(g => (g.TcmClient.Client.Clinic.Id == clinic.Id))
                                                     .ToListAsync();
                return View(servicePlan);
            }

            return View(null);

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
        public IActionResult Create_Domain(int id)
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
                        IEnumerable <SelectListItem> list_Services = _combosHelper.GetComboServicesNotUsed(tcmServicePlan.Id);
                            
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
            }

            return RedirectToAction("Index", "TCMServicePlans");
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

        [Authorize(Roles = "TCMSupervisor")]
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
    }
}
