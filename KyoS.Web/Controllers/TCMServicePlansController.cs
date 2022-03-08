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

        [Authorize(Roles = "Admin, Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> Index(int idError = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if(user_logged.UserType.ToString() != "Admin")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }
                
                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
                
                if (user_logged.UserType.ToString() == "CaseManager")
                {

                     List<TCMServicePlanEntity> servicePlan = await _context.TCMServicePlans
                                                         .Include(g => g.TcmClient)
                                                         .Where(g => (g.TcmClient.Casemanager.Id == caseManager.Id))
                                                         .ToListAsync();
                    return View(servicePlan);
                }
                if (user_logged.UserType.ToString() == "Manager")
                {
                   List<TCMServicePlanEntity> servicePlan = await _context.TCMServicePlans
                                                        .Include(g => g.TcmClient)
                                                        .Where(g => (g.Clinic.Id == clinic.Id))
                                                        .ToListAsync();
                    return View(servicePlan);
                }
                return View(null);

            }

            else
                return View(null);

        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Create(int id)
        {
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans.Include(u => u.TcmClient)
                                                         .FirstOrDefault(u => u.TcmClient.Id == id);
            if(tcmServicePlan == null)
            {
                TCMServicePlanViewModel model;
                if (User.IsInRole("CaseManager"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {

                        model = new TCMServicePlanViewModel
                        {
                           /* IdClinic = user_logged.Clinic.Id,
                            ID_TcmClient = id,*/
                            TcmClient = _context.TCMClient.Include(u => u.Casemanager)
                                                           .Include(d => d.Client)
                                                           .FirstOrDefault(u => u.Id == id),
                            Clinic = _context.Clinics.FirstOrDefault(u => u.Id == user_logged.Clinic.Id),
                            ID_Status = 1,
                            Status = _combosHelper.GetComboClientStatus(),
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
            
            return View(null);
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
                TCMServicePlanEntity tcmServicePlanEntity = await _context.TCMServicePlans
                                                                    .Include(s => s.TcmClient)
                                                                    .FirstOrDefaultAsync(s => s.TcmClient.CaseNumber == tcmServicePlanViewModel.TcmClient.CaseNumber);
                if (tcmServicePlanEntity == null)
                {
                    tcmServicePlanEntity = await _converterHelper.ToTCMServicePlanEntity(tcmServicePlanViewModel, true);
                    _context.Add(tcmServicePlanEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        List<TCMServicePlanEntity> servicePlans = await _context.TCMServicePlans
                                                          .Include(g => g.TcmClient)
                                                          .Where(g => (g.TcmClient.Casemanager.Id == tcmServicePlanViewModel.TcmClient.Casemanager.Id))
                                                          .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_TCMServicePlan", servicePlans) });
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

        public void UpdateTCMServicePlanToNonActive(ClientEntity client)
        {
           /* List<TCMServicePlanEntity> tcmServicePlan_list = _context.TCMServicePlans.Where(m => m.Client == client).ToList();
            if (tcmServicePlan_list.Count() > 0)
            {
                foreach (TCMServicePlanEntity item in tcmServicePlan_list)
                {
                  //  item.Active = false;
                    _context.Update(item);
                }
                _context.SaveChangesAsync();
            }*/
        }
    }
}
