using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
    public class TCMDischargesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IRenderHelper _renderHelper;

        public TCMDischargesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
            _renderHelper = renderHelper;
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> Index(int idError = 0)
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

            if (User.IsInRole("Manager")|| (User.IsInRole("TCMSupervisor")))
            {
                List<TCMDischargeEntity> tcmDischarge = await _context.TCMDischarge
                                                                       .Include(m => m.TcmServicePlan)
                                                                       .ThenInclude(m => m.TcmClient)
                                                                       .ThenInclude(m => m.Client)
                                                                       .Include(n => n.TcmDischargeFollowUp)
                                                                       .Where(m => m.Approved >= 1)
                                                                       .OrderBy(m => m.TcmServicePlan.TcmClient.CaseNumber)
                                                                       .ToListAsync();

                return View(tcmDischarge);
            }

            if (User.IsInRole("CaseManager"))
            {
                
                List<TCMDischargeEntity> tcmDischarge = await _context.TCMDischarge
                                                                       .Include(m => m.TcmServicePlan)
                                                                       .ThenInclude(m => m.TcmClient)
                                                                       .ThenInclude(m => m.Client)
                                                                       .Include(n => n.TcmDischargeFollowUp)
                                                                       .Where(m => m.TcmServicePlan.TcmClient.Casemanager.LinkedUser == user_logged.UserName)
                                                                       .OrderBy(m => m.TcmServicePlan.TcmClient.CaseNumber)
                                                                       .ToListAsync();

                return View(tcmDischarge);
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Create(int id = 0)
        {
            
            TCMDischargeViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    model = new TCMDischargeViewModel
                    {
                        AdministrativeDischarge = false,
                        AdministrativeDischarge_Explain = "",
                        AllServiceInPlace = false,
                        ClientLeftVoluntarily = false,
                        ClientMovedOutArea = false,
                        DischargeDate = DateTime.Now,
                        IdServicePlan = id,
                        LackOfProgress = false,
                        NonComplianceWithAgencyRules = false,
                        Other = false,
                        Other_Explain = "",
                        PresentProblems = "",
                        ProgressToward = "",
                        Referred = false,
                        StaffingDate = DateTime.Now,
                        StaffSignatureDate = DateTime.Now,
                        SupervisorSignatureDate = DateTime.Now,
                        TcmDischargeFollowUp = new List<TCMDischargeFollowUpEntity>(),
                        TcmServicePlan = _context.TCMServicePlans
                                                 .Include(n => n.TcmClient)
                                                 .ThenInclude(n => n.Client)
                                                 .FirstOrDefault(n => n.Id == id),
                        TcmServices = _context.TCMServices.ToList(),
                        TcmDischargeServiceStatus = new List<TCMDischargeServiceStatusEntity>()
                        
                    };
                    TCMDischargeEntity tcmDischarge = _context.TCMDischarge.FirstOrDefault(n => n.TcmServicePlan.Id == id);
                    if (tcmDischarge == null)
                    {
                        model.Id = 0;
                    }
                    else
                    {
                        model.Id = tcmDischarge.Id;
                    }
                    TCMDischargeServiceStatusEntity ServiceStatus = new TCMDischargeServiceStatusEntity();

                    foreach (var item in model.TcmServices)
                    {
                        ServiceStatus.CodeService = item.Code;
                        ServiceStatus.NameService = item.Name;
                        ServiceStatus.Status = false;
                        ServiceStatus.Id = 0;
                        model.TcmDischargeServiceStatus.Add(ServiceStatus);
                        ServiceStatus = new TCMDischargeServiceStatusEntity();
                    }
                    TCMDischargeFollowUpEntity tcmDischargeFollowUp = new TCMDischargeFollowUpEntity();
                    tcmDischargeFollowUp.ProviderAgency = "";
                    tcmDischargeFollowUp.TypeService = "";
                    tcmDischargeFollowUp.Address_Location = "";
                    tcmDischargeFollowUp.PhoneNumber = "";
                    tcmDischargeFollowUp.NextAppt = "";
                    model.TcmDischargeFollowUp.Add(tcmDischargeFollowUp);
                    //model.TcmDischargeFollowUp.Insert(0,tcmDischargeFollowUp);
                    return View(model);
                }
            }

            model = new TCMDischargeViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Create(TCMDischargeViewModel tcmDischargeViewModel)
         {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMDischargeEntity tcmDischargeEntity = _context.TCMDischarge.Find(tcmDischargeViewModel.Id);
                if (tcmDischargeEntity == null)
                {
                    tcmDischargeEntity = await _converterHelper.ToTCMDischargeEntity(tcmDischargeViewModel, true);
                    _context.TCMDischarge.Add(tcmDischargeEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        List<TCMDischargeEntity> tcmDischarge = await _context.TCMDischarge
                                                                       .Include(m => m.TcmServicePlan)
                                                                       .ThenInclude(m => m.TcmClient)
                                                                       .ThenInclude(m => m.Client)
                                                                       .Include(n => n.TcmDischargeFollowUp)
                                                                       .Where(m => m.TcmServicePlan.TcmClient.Casemanager.LinkedUser == user_logged.UserName)
                                                                       .OrderBy(m => m.TcmServicePlan.TcmClient.CaseNumber)
                                                                       .ToListAsync();

                        return RedirectToAction("Index", "TCMDischarges");
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the TCM service.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmDischargeViewModel) });
                }
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmDischargeViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Edit(int id = 0)
        {
            TCMDischargeViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    TCMDischargeEntity TcmDischarge = _context.TCMDischarge
                                                              .Include(m => m.TcmDischargeFollowUp)
                                                              .Include(n => n.TcmDischargeServiceStatus)
                                                              .Include(b => b.TcmServicePlan)
                                                              .ThenInclude(b => b.TcmClient)
                                                              .ThenInclude(b => b.Client)
                                                              .FirstOrDefault(m => m.Id == id);
                    if (TcmDischarge == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {
                        
                        model = _converterHelper.ToTCMDischargeViewModel(TcmDischarge);
                        
                        return View(model);
                    }
                   
                }
            }

            model = new TCMDischargeViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Edit(TCMDischargeViewModel tcmDischargeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                    TCMDischargeEntity tcmDischargeEntity = await _converterHelper.ToTCMDischargeEntity(tcmDischargeViewModel, false);
                    _context.TCMDischarge.Update(tcmDischargeEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                       
                    return RedirectToAction("Index", "TCMDischarges");
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
               
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", tcmDischargeViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> TCMDischargeCandidates(int idError = 0)
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

            List<TCMServicePlanEntity> tcmServicePlanList = await _context.TCMServicePlans
                                                                          .Include(f => f.TCMService)
                                                                          .Include(f => f.TCMDomain)
                                                                          .Include(f => f.TcmClient)
                                                                          .ThenInclude(f => f.Client)
                                                                          .Include(g => g.TcmClient.Casemanager)
                                                                          .Where(s => (s.TcmClient.Casemanager.Clinic.Id == user_logged.Clinic.Id
                                                                                 && s.TcmClient.Status == StatusType.Open
                                                                                     && s.TcmClient.DataClose > DateTime.Now
                                                                                          && s.Approved == 2))
                                                                          .OrderBy(f => f.TcmClient.CaseNumber)
                                                                          .ToListAsync();
            List<TCMDischargeEntity> tcmDischargeList = await _context.TCMDischarge
                                                                      .Include(f => f.TcmServicePlan)
                                                                      .ToListAsync();
            TCMDischargeEntity tcmDiscaharge = new TCMDischargeEntity();
            for (int i = 0; i < tcmDischargeList.Count(); i++)
            {
                tcmDiscaharge.TcmServicePlan = tcmServicePlanList.FirstOrDefault(g => g.Id == tcmDischargeList[i].TcmServicePlan.Id);
                if (tcmDiscaharge != null)
                {
                    tcmServicePlanList.Remove(tcmDiscaharge.TcmServicePlan);
                }

            }

            return View(tcmServicePlanList);

        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> FinishEditing(int id)
        {
            TCMDischargeEntity tcmDischarge = _context.TCMDischarge.FirstOrDefault(u => u.Id == id);

            if (tcmDischarge != null)
            {
                if (User.IsInRole("CaseManager"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        tcmDischarge.Approved = 1;
                        _context.Update(tcmDischarge);
                        try
                        {
                            await _context.SaveChangesAsync();

                            return RedirectToAction("Index", "TCMDischarges");
                        }
                        catch (System.Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("Index", "TCMDischarges");
        }
    }
}
