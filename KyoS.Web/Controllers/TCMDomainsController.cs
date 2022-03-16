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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    public class TCMDomainsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IRenderHelper _renderHelper;

        public TCMDomainsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
            _renderHelper = renderHelper;
        }

        [Authorize(Roles = "Manager, CaseManager")]
        public async Task<IActionResult> Index(int idError = 0)
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
        public async Task<IActionResult> Edit(int? id)
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
        public async Task<IActionResult> Edit(int id, TCMDomainViewModel tcmDomainViewModel)
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

                    List<TCMDomainEntity> tcmDomain = await _context.TCMDomains
                                                            .Include(f => f.TCMObjetive)
                                                            .Include(g => g.TcmServicePlan)
                                                            .ThenInclude(g => g.TcmClient)
                                                            .ThenInclude(g => g.Client)
                                                            .Where(s => s.TcmServicePlan.TcmClient.Casemanager.LinkedUser == user_logged.UserName)
                                                            .OrderBy(f => f.Code)
                                                            .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMDomain", tcmDomain) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", tcmDomainViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Create(int id)
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
        public async Task<IActionResult> Create(TCMDomainViewModel tcmDomainViewModel)
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

                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmDomainViewModel) });
                
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
                                                        .Include(g => g.TcmClient)
                                                        .ThenInclude(t => t.Casemanager)
                                                        .Where(d => d.TcmClient.Casemanager.Id == caseManager.Id)
                                                        .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "index", servicePlan) });
                        //return RedirectToAction("Index", "TCMDomains");
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    // ModelState.AddModelError(string.Empty, "Already exists the TCM service.");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmDomainViewModel) });
                }
            }

            return View(tcmDomainViewModel);
            // return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create_Domain", tcmDomainViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Delete(int? id)
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
    }
}
