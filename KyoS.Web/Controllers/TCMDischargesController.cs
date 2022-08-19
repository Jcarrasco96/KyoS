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
        public async Task<IActionResult> Index(int idError = 0, int idTCMClient = 0)
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

                ViewData["idTCMClient"] = idTCMClient;
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

                ViewData["idTCMClient"] = idTCMClient;
                return View(tcmDischarge);
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Create(int id = 0)
        {

            TCMDischargeViewModel model;
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans
                                                          .FirstOrDefault(f => (f.Id == id && f.Approved == 2));
            if (tcmServicePlan != null)
            {
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
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
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
                                                     .Include(n => n.TCMDomain)
                                                     .Include(n => n.TCMService)
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


                        return View(model);
                    }
                }

            }
            else
            {
                return RedirectToAction("TCMIntakeSectionDashboard", "TCMIntakes", new { id = _context.TCMClient.FirstOrDefault(n => n.TcmServicePlan.Id == id).Id, section = 4 });
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
                    tcmDischargeEntity = await _converterHelper.ToTCMDischargeEntity(tcmDischargeViewModel, true, user_logged.UserName);
                    _context.TCMDischarge.Add(tcmDischargeEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        TCMDischargeEntity tcmDischarge = await _context.TCMDischarge
                                                                       .Include(m => m.TcmServicePlan)
                                                                       .ThenInclude(m => m.TcmClient)
                                                                       .ThenInclude(m => m.Client)
                                                                       .Include(n => n.TcmDischargeFollowUp)
                                                                       .FirstOrDefaultAsync(m => m.TcmServicePlan.TcmClient.Casemanager.LinkedUser == user_logged.UserName
                                                                            && m.TcmServicePlan.Id == tcmDischargeViewModel.IdServicePlan);

                        return RedirectToAction("Index", "TCMDischarges", new { idTCMClient = tcmDischarge.TcmServicePlan.TcmClient.Id});
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
        public IActionResult Edit(int id = 0, int origi  = 0)
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
                                                              .Include(b => b.TcmServicePlan)
                                                              .ThenInclude(b => b.TCMDomain)
                                                              .FirstOrDefault(m => m.Id == id);
                    if (TcmDischarge == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {
                        ViewData["origi"] = origi;
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
        public async Task<IActionResult> Edit(TCMDischargeViewModel tcmDischargeViewModel, int origi  = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMDischargeEntity tcmDischargeEntity = await _converterHelper.ToTCMDischargeEntity(tcmDischargeViewModel, false, user_logged.UserName);

                List<TCMMessageEntity> messages = tcmDischargeEntity.TCMMessages.Where(m => (m.Status == MessageStatus.NotRead && m.Notification == false)).ToList();
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
                        TCMAddendum = null,
                        TCMDischarge = tcmDischargeEntity,
                        TCMAssessment = null,
                        Title = "Update on reviewed TCM Discharge",
                        Text = $"The TCM Discharge of {tcmDischargeEntity.TcmServicePlan.TcmClient.Client.Name} on {tcmDischargeEntity.DischargeDate.ToShortDateString()} was rectified",
                        From = value.To,
                        To = value.From,
                        DateCreated = DateTime.Now,
                        Status = MessageStatus.NotRead,
                        Notification = true
                    };
                    _context.Add(notification);
                }


                _context.TCMDischarge.Update(tcmDischargeEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    if (origi == 0)
                    {
                        return RedirectToAction("Index", "TCMDischarges", new { idTCMClient = tcmDischargeEntity.TcmServicePlan.TcmClient.Id });
                    }
                    else
                    {
                        if (origi == 4)
                        {
                            return RedirectToAction("MessagesOfDischarges", "TCMMessages");
                        }
                        else
                        {
                            return RedirectToAction("TCMDischargeApproved", "TCMDischarges", new { approved = (origi - 1) });
                        }
                    }
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
        public async Task<IActionResult> FinishEditing(int id, int origin = 0)
        {
            TCMDischargeEntity tcmDischarge = _context.TCMDischarge
                                                      .Include(n => n.TcmServicePlan)
                                                      .ThenInclude(n => n.TcmClient)
                                                      .FirstOrDefault(u => u.Id == id);

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

                            if (origin == 0)
                            {
                                return RedirectToAction("TCMIntakeSectionDashboard", "TCMIntakes", new { id = tcmDischarge.TcmServicePlan.TcmClient.Id, section = 4 });
                            }
                            else
                            {
                                return RedirectToAction("TCMDischargeApproved", "TCMDischarges", new { approved = 1 });
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

            return RedirectToAction("Index", "TCMDischarges");
        }


        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMDischargeFolowUp(int idDischarge = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMDischargeFollowUpViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMDischargeEntity tcmDischarge = _context.TCMDischarge
                                                                      .Include(n => n.TcmServicePlan)
                                                                      .ThenInclude(n => n.TcmClient)
                                                                      .FirstOrDefault(n => n.Id == idDischarge);

                    if (tcmDischarge != null)
                    {

                        model = new TCMDischargeFollowUpViewModel
                        {
                            TcmDischarge = _context.TCMDischarge
                                                   .Include(n => n.TcmServicePlan)
                                                   .ThenInclude(n => n.TcmClient)
                                                   .ThenInclude(n => n.Client)
                                                   .FirstOrDefault(n => n.Id == idDischarge),
                            Id = 0,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            IdTCMDischarge = idDischarge,
                            Address_Location = "",
                            NextAppt = "",
                            PhoneNumber = "",
                            ProviderAgency = "",
                            TypeService = ""
                            
                        };

                        return View(model);
                    }
                    else
                    {
                        return RedirectToAction("Index", "TCMDischarges");
                    }

                }
            }

            return RedirectToAction("Index", "TCMDischarges");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMDischargeFolowUp(TCMDischargeFollowUpViewModel DischargeFollowUpViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMDischargeFollowUpEntity dischargeFollowUpEntity = await _converterHelper.ToTCMDischargeFollowUpEntity(DischargeFollowUpViewModel, false, user_logged.UserName);

               
                    _context.TCMDischargeFollowUp.Add(dischargeFollowUpEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMDischargeFollowUpEntity> dischargeFollowUp = await _context.TCMDischargeFollowUp
                                                                                           .Include(h => h.TcmDischarge)
                                                                                           .ThenInclude(f => f.TcmServicePlan)
                                                                                           .ThenInclude(f => f.TcmClient)
                                                                                           .ThenInclude(f => f.Client)
                                                                                           .Where(n => n.TcmDischarge.Id == DischargeFollowUpViewModel.IdTCMDischarge)
                                                                                           .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDischargeFollowUp", dischargeFollowUp) });

                        
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
               
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMDischargeFolowUp", DischargeFollowUpViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> EditTCMDischargeFolowUp(int id = 0)
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


                TCMDischargeFollowUpEntity tcmDischargeFollowUpEntity = await _context.TCMDischargeFollowUp
                                                                                      .Include(n => n.TcmDischarge)
                                                                                      .ThenInclude(n => n.TcmServicePlan)
                                                                                      .ThenInclude(n => n.TcmClient)
                                                                                      .ThenInclude(n => n.Client)
                                                                                      .FirstOrDefaultAsync(s => s.Id == id);


                TCMDischargeFollowUpViewModel model = _converterHelper.ToTCMDischargeFollowUpViewModel(tcmDischargeFollowUpEntity);
               
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
        public async Task<IActionResult> EditTCMDischargeFolowUp(TCMDischargeFollowUpViewModel tcmDischargeFollowUpViewModel)
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

                
                TCMDischargeFollowUpEntity tcmDischargeFollowUpEntity = await _converterHelper.ToTCMDischargeFollowUpEntity(tcmDischargeFollowUpViewModel, false, user_logged.UserName);
               
                if (ModelState.IsValid)
                {
                    _context.Update(tcmDischargeFollowUpEntity);
                   
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMDischargeFollowUpEntity> tcmDischargeFollowUpEntityList = await _context.TCMDischargeFollowUp
                                                                                                 .Include(h => h.TcmDischarge)
                                                                                                 .ThenInclude(f => f.TcmServicePlan)
                                                                                                 .ThenInclude(f => f.TcmClient)
                                                                                                 .ThenInclude(f => f.Client)
                                                                                                 .Where(g => g.TcmDischarge.Id == tcmDischargeFollowUpViewModel.IdTCMDischarge)
                                                                                                 .ToListAsync();
                                                                                                 
                                                                                                 


                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDischargeFollowUp", tcmDischargeFollowUpEntityList) });

                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditTCMDischargeFolowUp", tcmDischargeFollowUpViewModel) });
                }

                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditTCMDischargeFolowUp", tcmDischargeFollowUpViewModel) });

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteDischargeFollowUp(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMDischargeFollowUpEntity tcmDischargeFollowUpEntity = await _context.TCMDischargeFollowUp
                                                                                  .Include(n => n.TcmDischarge)
                                                                                  .FirstOrDefaultAsync(s => s.Id == id);
            if (tcmDischargeFollowUpEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMDischargeFollowUp.Remove(tcmDischargeFollowUpEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1, caseNumber = 0 });
            }

            return RedirectToAction("Edit", new { id = tcmDischargeFollowUpEntity.Id });
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> TCMDischargeApproved(int approved = 0, int idTCMClient = 0)
        {

            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (User.IsInRole("Manager") || (User.IsInRole("TCMSupervisor")))
            {
                List<TCMDischargeEntity> tcmDischarge = await _context.TCMDischarge
                                                                       .Include(m => m.TcmServicePlan)
                                                                       .ThenInclude(m => m.TcmClient)
                                                                       .ThenInclude(m => m.Client)
                                                                       .Include(n => n.TcmDischargeFollowUp)
                                                                       .Where(m => m.Approved == approved
                                                                            && m.TcmServicePlan.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id)
                                                                       .OrderBy(m => m.TcmServicePlan.TcmClient.CaseNumber)
                                                                       .ToListAsync();
                ViewData["idTCMClient"] = idTCMClient;
                return View(tcmDischarge);
            }

            if (User.IsInRole("CaseManager"))
            {

                List<TCMDischargeEntity> tcmDischarge = await _context.TCMDischarge
                                                                       .Include(m => m.TcmServicePlan)
                                                                       .ThenInclude(m => m.TcmClient)
                                                                       .ThenInclude(m => m.Client)
                                                                       .Include(n => n.TcmDischargeFollowUp)
                                                                       .Where(m => m.TcmServicePlan.TcmClient.Casemanager.LinkedUser == user_logged.UserName
                                                                            && m.Approved == approved
                                                                            && m.TcmServicePlan.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id)
                                                                       .OrderBy(m => m.TcmServicePlan.TcmClient.CaseNumber)
                                                                       .ToListAsync();

                ViewData["idTCMClient"] = idTCMClient;
                return View(tcmDischarge);
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult EditReadOnly(int id = 0, int origi = 0)
        {
            TCMDischargeViewModel model;

            if (User.IsInRole("TCMSupervisor"))
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
                                                              .Include(b => b.TcmServicePlan)
                                                              .ThenInclude(b => b.TCMDomain)
                                                              .FirstOrDefault(m => m.Id == id);
                    if (TcmDischarge == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {
                        model = _converterHelper.ToTCMDischargeViewModel(TcmDischarge);
                        ViewData["origi"] = origi;
                        return View(model);

                    }

                }
            }

            model = new TCMDischargeViewModel();
            return View(model);
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> ApproveDischarge(int id)
        {
            TCMDischargeEntity tcmDischarge = _context.TCMDischarge
                                                   .Include(u => u.TcmServicePlan)
                                                   .ThenInclude(u => u.TcmClient)
                                                   .ThenInclude(u => u.Client)
                                                   .FirstOrDefault(u => u.Id == id);

            if (tcmDischarge != null)
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        tcmDischarge.Approved = 2;
                        _context.Update(tcmDischarge);
                        try
                        {
                            await _context.SaveChangesAsync();

                            return RedirectToAction("TCMDischargeApproved", new { approved = 1 });
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

        [Authorize(Roles = "TCMSupervisor, CaseManager")]
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
                    IdTCMDischarge = id,
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
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                model.From = user_logged.UserName;
                model.To = model.TCMDischarge.CreatedBy;
                _context.Add(model);
                await _context.SaveChangesAsync();
            }
            if (messageViewModel.Origin == 1)
                return RedirectToAction("TCMDischargeWithReview");
           
            return RedirectToAction("TCMDischargeApproved", new { approved = 1 });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> TCMDischargeWithReview()
        {
            if (User.IsInRole("CaseManager"))
            {
                List<TCMDischargeEntity> salida = await _context.TCMDischarge
                                                                .Include(wc => wc.TcmServicePlan)
                                                                .ThenInclude(wc => wc.TcmClient)
                                                                .ThenInclude(wc => wc.Client)
                                                                .Include(wc => wc.TcmServicePlan)
                                                                .ThenInclude(wc => wc.TcmClient)
                                                                .ThenInclude(wc => wc.Casemanager)
                                                                .Include(wc => wc.TCMMessages.Where(m => m.Notification == false))
                                                                .Where(wc => (wc.TcmServicePlan.TcmClient.Casemanager.LinkedUser == User.Identity.Name
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
                    List<TCMDischargeEntity> salida = await _context.TCMDischarge
                                                                    .Include(wc => wc.TcmServicePlan)
                                                                    .ThenInclude(wc => wc.TcmClient)
                                                                    .ThenInclude(wc => wc.Client)
                                                                    .Include(wc => wc.TcmServicePlan)
                                                                    .ThenInclude(wc => wc.TcmClient)
                                                                    .ThenInclude(wc => wc.Casemanager)
                                                                    .Include(wc => wc.TCMMessages.Where(m => m.Notification == false))
                                                                    .Where(wc => (wc.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == user_logged.Clinic.Id
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
