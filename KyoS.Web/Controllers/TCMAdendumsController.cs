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
    public class TCMAdendumsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IReportHelper _reportHelper;

        public TCMAdendumsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IReportHelper reportHelper)
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

                List<TCMAdendumEntity> adendum = await _context.TCMAdendums
                                                    .Include(h => h.TcmDomain)
                                                    .ThenInclude(h => h.TCMObjetive)
                                                    .Include(h => h.TcmServicePlan)
                                                    .ThenInclude(h => (h.TcmClient))
                                                    .ThenInclude(h => (h.Client))
                                                    .Where(h => (h.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id
                                                    && h.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == clinic.Id))
                                                    .ToListAsync();

                return View(adendum);
            }
            if (user_logged.UserType.ToString() == "Manager")
            {
                List<TCMAdendumEntity> adendum = await _context.TCMAdendums
                                                   .Include(h => h.TcmDomain)
                                                   .Include(h => h.TcmServicePlan)
                                                   .ThenInclude(h => (h.TcmClient))
                                                   .Include(h => h.TcmServicePlan.TcmClient.Client)
                                                   .Include(h => h.TcmServicePlan.TcmClient.Casemanager)
                                                   .Where(h => h.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == clinic.Id)
                                                   .ToListAsync();

                return View(adendum);
            }
            if (user_logged.UserType.ToString() == "TCMSupervisor")
            {
                List<TCMAdendumEntity> adendum = await _context.TCMAdendums
                                                   .Include(h => h.TcmDomain)
                                                   .Include(h => h.TcmServicePlan)
                                                   .ThenInclude(h => (h.TcmClient))
                                                   .Include(h => h.TcmServicePlan.TcmClient.Client)
                                                   .Include(h => h.TcmServicePlan.TcmClient.Casemanager)
                                                   .Where(h => h.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == clinic.Id)
                                                   .ToListAsync();

                return View(adendum);
            }

            return View(null);

        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Create(int id)
        {
            TCMAdendumViewModel model = null;
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                              .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (id != 0)  //para cuando se cree desde la vista de Serviceplan
            {
                TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans.Include(u => u.TcmClient)
                                                             .ThenInclude(u => u.Client)
                                                             .FirstOrDefault(u => u.Id == id);
            
                if (tcmServicePlan != null)
                {
                    if (User.IsInRole("CaseManager"))
                    {
                        if (user_logged.Clinic != null)
                        {

                            IEnumerable<SelectListItem> list_Services = _combosHelper.GetComboServicesNotUsed(tcmServicePlan.Id);

                            /*model = new TCMAdendumViewModel
                            {
                                Date_Identified = DateTime.Today.Date,
                                Services = list_Services,
                                TcmServicePlan = tcmServicePlan,
                                Id_ServicePlan = id,
                            };*/
                            return View(model);
                        }
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                }
            }
            else
            {
                if (User.IsInRole("CaseManager"))
                {
                    
                    if (user_logged.Clinic != null)
                    {
                       
                        model = new TCMAdendumViewModel
                        {
                            Date_Identified = DateTime.Today.Date,
                            ID_TcmDominio = 0,
                            TcmDominio = _combosHelper.GetComboTCMServices(),
                            ID_TcmServicePlan = 0,
                            ListTcmServicePlan = _combosHelper.GetComboServicesPlan(user_logged.Clinic.Id),
                            
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
        public async Task<IActionResult> Create(TCMAdendumViewModel tcmAdendumViewModel)
        {
            UserEntity user_logged = _context.Users
                                           .Include(u => u.Clinic)
                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);
            TCMServiceEntity tcmService = await _context.TCMServices.FirstAsync(d => d.Id == tcmAdendumViewModel.ID_TcmDominio);

            TCMServicePlanEntity tcmServicePlan = await _context.TCMServicePlans
                                    .FirstOrDefaultAsync(s => s.Id == tcmAdendumViewModel.ID_TcmServicePlan);

            TCMDomainEntity tcmDomain = null;

            tcmDomain = new TCMDomainEntity
            {
                     DateIdentified = tcmAdendumViewModel.Date_Identified,
                     LongTerm = tcmAdendumViewModel.Long_term,
                     NeedsIdentified = tcmAdendumViewModel.Needs_Identified,
                     TCMObjetive = null,
                     TcmServicePlan = tcmServicePlan,
                     Name = tcmService.Name,
                     Code = tcmService.Code,
                     
            };
            tcmAdendumViewModel.TcmServicePlan = tcmServicePlan;
            tcmAdendumViewModel.TcmDomain = tcmDomain;

            if (user_logged.UserType.ToString() == "CaseManager")
            {
                if (ModelState.IsValid)
                {
                    TCMAdendumEntity tcmAdendum = await _converterHelper.ToTCMAdendumEntity(tcmAdendumViewModel, true);
                    _context.Add(tcmAdendum);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMAdendumEntity> adendum = await _context.TCMAdendums
                                                   .Include(h => h.TcmDomain)
                                                   .Include(h => h.TcmServicePlan)
                                                   .Include(h => h.TcmServicePlan.TcmClient.Casemanager)
                                                   .Include(h => (h.TcmServicePlan.TcmClient.Client))
                                                   .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_TCMAdendum", adendum) });

                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    // ModelState.AddModelError(string.Empty, "Already exists the TCM Adendums.");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmAdendumViewModel) });
                }

            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            
            return View(tcmAdendumViewModel);
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMAdendumEntity tcmadendumEntity = await _context.TCMAdendums
                                                            .Include(u => u.TcmDomain)
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
                    Long_term = tcmadendumEntity.TcmDomain.LongTerm,
                    Needs_Identified = tcmadendumEntity.TcmDomain.NeedsIdentified
                };
                
                return View(tcmAdendumViewModel);
            }

            return View(tcmAdendumViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Edit(int id, TCMAdendumViewModel tcmAdendumViewModel)
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
           
            tcmAdendumViewModel.TcmServicePlan = tcmServicePlan;
            tcmDomain.NeedsIdentified = tcmAdendumViewModel.Needs_Identified;
            tcmDomain.LongTerm = tcmAdendumViewModel.Long_term;
            tcmDomain.DateIdentified = tcmAdendumViewModel.Date_Identified;
            tcmAdendumViewModel.TcmDomain = tcmDomain;
            
            CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);

            if (user_logged.UserType.ToString() == "CaseManager")
            {
                if (ModelState.IsValid)
                {
                    TCMAdendumEntity tcmAdendumEntity = await _converterHelper.ToTCMAdendumEntity(tcmAdendumViewModel, false);
                    _context.Update(tcmAdendumEntity);

                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMAdendumEntity> tcmAdendums = await _context.TCMAdendums
                                                      .Include(h => h.TcmDomain)
                                                      .Include(h => h.TcmServicePlan)
                                                      .ThenInclude(h => (h.TcmClient))
                                                      .ThenInclude(h => (h.Client))
                                                      .Where(h => (h.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id
                                                      && h.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == caseManager.Clinic.Id))
                                                      .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_TCMAdendum", tcmAdendums) });
                       
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", tcmAdendumViewModel) });
                }
            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            return RedirectToAction("Home/Error404");

        }
    }
}