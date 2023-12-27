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
    public class TCMServicePlanReviewsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IRenderHelper _renderHelper;

        public TCMServicePlanReviewsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper, IRenderHelper renderHelper)
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

            if (User.IsInRole("CaseManager"))
            {
                return View(await _context.TCMServicePlanReviews
                                          .Include(f => f.TcmServicePlan)
                                          .ThenInclude(f => f.TcmClient)
                                          .ThenInclude(f => f.Client)
                                          .Include(f => f.TcmServicePlan.TcmClient.Casemanager)
                                          .Where(s => s.TcmServicePlan.TcmClient.Casemanager.LinkedUser == user_logged.UserName)
                                          .OrderBy(f => f.TcmServicePlan.TcmClient.CaseNumber)
                                          .ToListAsync());

            }
            else
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    return View(await _context.TCMServicePlanReviews
                                              .Include(f => f.TcmServicePlan)
                                              .ThenInclude(f => f.TcmClient)
                                              .ThenInclude(f => f.Client)
                                              .Include(f => f.TcmServicePlan.TcmClient.Casemanager)
                                              .Where(s => s.TcmServicePlan.TcmClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName)
                                              .OrderBy(f => f.TcmServicePlan.TcmClient.CaseNumber)
                                              .ToListAsync());

                }
                else
                {
                    if (User.IsInRole("Manager"))
                    {
                        return View(await _context.TCMServicePlanReviews
                                                  .Include(f => f.TcmServicePlan)
                                                  .ThenInclude(f => f.TcmClient)
                                                  .ThenInclude(f => f.Client)
                                                  .Include(f => f.TcmServicePlan.TcmClient.Casemanager)
                                                  .Where(s => s.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == user_logged.Clinic.Id)
                                                  .OrderBy(f => f.TcmServicePlan.TcmClient.CaseNumber)
                                                  .ToListAsync());

                    }
                    else
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                }
            }
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> TCMServicesPlanReviewCandidates(int idError = 0)
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
                                      .Include(f => f.TcmClient)
                                      .ThenInclude(f => f.Client)
                                      .Include(g => g.TcmClient.Casemanager)
                                      .Where(s => (s.TcmClient.Casemanager.Clinic.Id == user_logged.Clinic.Id
                                          && s.TcmClient.Status == StatusType.Open
                                          && s.TcmClient.DataClose > DateTime.Now
                                          && s.Approved == 2
                                          && s.TcmClient.Casemanager.LinkedUser == user_logged.UserName))
                                      .OrderBy(f => f.TcmClient.CaseNumber)
                                      .ToListAsync();
            List<TCMServicePlanReviewEntity> tcmServicePlanReviewList = await _context.TCMServicePlanReviews
                                                                                      .Include(f => f.TcmServicePlan)
                                                                                      .ToListAsync();
            TCMServicePlanEntity tcmServicePlan = null;
            for (int i = 0; i < tcmServicePlanReviewList.Count(); i++)
            {
                tcmServicePlan = tcmServicePlanList.FirstOrDefault(g => g.Id == tcmServicePlanReviewList[i].TcmServicePlan.Id);
                if (tcmServicePlan != null)
                {
                    tcmServicePlanList.Remove(tcmServicePlan);
                }

            }

            return View(tcmServicePlanList);

        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Create(int id, int IdServicePlan)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                      .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (User.IsInRole("CaseManager"))
            {
                TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans
                                                              .Include(b => b.TCMDomain)
                                                              .Include(f => f.TcmClient)
                                                              .Include(f => f.TcmClient.Casemanager)
                                                              .Include(f => f.TcmClient.Client)
                                                              .FirstOrDefault(f => (f.Id == IdServicePlan && f.Approved == 2));
               
                if (tcmServicePlan != null)
                {
                  
                    TCMServicePlanReviewEntity tcmServicePlanReviewEntity = null;

                    tcmServicePlanReviewEntity = new TCMServicePlanReviewEntity
                    {
                        DateServicePlanReview = DateTime.Today.Date,
                        DateOpending = DateTime.Today.Date,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        TcmServicePlan = tcmServicePlan,
                        Recomendation = "",
                        SummaryProgress = "",

                    };

                    TCMServicePlanReviewViewModel model = _converterHelper.ToTCMServicePlanReviewViewModel(tcmServicePlanReviewEntity);
                
                    try
                    {
                        ViewData["origin"] = 1;
                        return View(model);
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                    
                    return RedirectToAction("NotAuthorized", "Account");
                }
                else
                {
                    ViewData["origin"] = 1;
                    return RedirectToAction("TCMIntakeSectionDashboard", "TCMIntakes", new { id = _context.TCMClient.FirstOrDefault(n => n.TcmServicePlan.Id == IdServicePlan).Id, section = 4 });
                }

            }
            return RedirectToAction("NotAuthorized", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Create(TCMServicePlanReviewViewModel tcmServicePlanreviewViewModel, int origin = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMServicePlanReviewEntity tcmServicePlanReviewEntity = await _context.TCMServicePlanReviews
                                                                                .Include(n => n.TcmServicePlan)
                                                                                .FirstOrDefaultAsync(n => n.TcmServicePlan.Id == tcmServicePlanreviewViewModel.IdServicePlan);

                List<TCMDomainEntity> tcmDomainEntity = _context.TCMDomains
                                                                .Include(g => g.TcmServicePlan)
                                                                .Include(g => g.TCMObjetive)
                                                                .Where(g => g.TcmServicePlan.Id == tcmServicePlanreviewViewModel.IdServicePlan)
                                                                .ToList();

                List<TCMServicePlanReviewDomainEntity> tcmDomainReviewList = new List<TCMServicePlanReviewDomainEntity>();
                TCMServicePlanReviewDomainEntity tcmDomainReview = new TCMServicePlanReviewDomainEntity();
               // tcmServicePlanReviewEntity.TCMServicePlanRevDomain = new List<TCMServicePlanReviewDomainEntity>();

                foreach (var item in tcmDomainEntity.ToList())
                {
                    tcmDomainReview.ChangesUpdate = "";
                    tcmDomainReview.TcmDomain = item;
                    tcmDomainReview.Status = SPRStatus.Open;
                    tcmDomainReview.CodeDomain = item.Code;
                    tcmDomainReview.TCMServicePlanRevDomainObjectiive = new List<TCMServicePlanReviewDomainObjectiveEntity>();
                    //tcmDomainReview.TcmDomain = _context.TCMDomains.FirstOrDefault(n => n.Id == item.ID);
                   /* if (item.Status == 0)
                    {
                        tcmDomainReview.Status = StatusType.Open;
                    }
                    else
                    {
                        tcmDomainReview.Status = StatusType.Close;
                    }*/
                    //tcmDomainReview.TCMServicePlanRevDomainObjectiive = new List<TCMServicePlanReviewDomainObjectiveEntity>();
                    TCMServicePlanReviewDomainObjectiveEntity tcmDomainObjectives = new TCMServicePlanReviewDomainObjectiveEntity();
                    foreach (var item_Objective in item.TCMObjetive)
                    {
                        tcmDomainObjectives.IdObjective = item_Objective.IdObjetive;
                        tcmDomainObjectives.Status = item_Objective.Status;
                        
                        tcmDomainObjectives.Origin = item_Objective.Origin;
                        tcmDomainObjectives.ChangesUpdate = "";

                        tcmDomainReview.TCMServicePlanRevDomainObjectiive.Add(tcmDomainObjectives);
                        tcmDomainObjectives = new TCMServicePlanReviewDomainObjectiveEntity();
                    }
                    tcmDomainReviewList.Add(tcmDomainReview);
                    tcmDomainReview = new TCMServicePlanReviewDomainEntity();

                }


                if (tcmServicePlanReviewEntity == null)
                {
                    tcmServicePlanReviewEntity = await _converterHelper.ToTCMServicePlanReviewEntity(tcmServicePlanreviewViewModel, true, user_logged.UserName);

                    tcmServicePlanReviewEntity.TCMServicePlanRevDomain = tcmDomainReviewList;


                    _context.Add(tcmServicePlanReviewEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        if (origin == 1)
                        {
                            TCMServicePlanReviewEntity servicePlanReview = await _context.TCMServicePlanReviews
                                                                                         .Include(f => f.TcmServicePlan)
                                                                                         .ThenInclude(f => f.TcmClient)
                                                                                         .ThenInclude(f => f.Client)
                                                                                         .Include(f => f.TcmServicePlan.TcmClient.Casemanager)
                                                                                         .FirstOrDefaultAsync(s => s.TcmServicePlan.Id == tcmServicePlanreviewViewModel.IdServicePlan);

                            return RedirectToAction("TCMIntakeSectionDashboard", "TCMIntakes", new { id = servicePlanReview.TcmServicePlan.TcmClient.Id, section = 4 });
                        }
                        else
                        {
                            List<TCMServicePlanReviewEntity> servicePlanReview = await _context.TCMServicePlanReviews
                                                                                               .Include(f => f.TcmServicePlan)
                                                                                               .ThenInclude(f => f.TcmClient)
                                                                                               .ThenInclude(f => f.Client)
                                                                                               .Include(f => f.TcmServicePlan.TcmClient.Casemanager)
                                                                                               .Where(s => s.TcmServicePlan.Id == tcmServicePlanreviewViewModel.IdServicePlan)
                                                                                               .OrderBy(f => f.TcmServicePlan.TcmClient.CaseNumber)
                                                                                               .ToListAsync();

                            return RedirectToAction("Index", "TCMServicePlanReviews");
                        }
                        
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError(string.Empty, "The field 'Objectives Changes / Updates' is required");
                        return View(tcmServicePlanreviewViewModel);
                    }
                }
                else
                {
                    if (tcmServicePlanReviewEntity != null)
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the TCM Service Plan Review.");
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmServicePlanreviewViewModel) });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the TCM Service Plan Emty.");
                        return RedirectToAction("Edit", "TCMServicePlans", new { id = tcmServicePlanreviewViewModel.IdServicePlan });
                    }

                }
            }


            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmServicePlanreviewViewModel) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult Edit(int Id, int IdServicePlan, int origin = 0)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .ThenInclude(u => u.Setting)
                                                   .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                TCMServicePlanReviewEntity tcmServicePlanReview = _context.TCMServicePlanReviews
                                                                          .Include(b => b.TCMServicePlanRevDomain)
                                                                          .ThenInclude(f => f.TCMServicePlanRevDomainObjectiive)
                                                                          .Include(f => f.TcmServicePlan)
                                                                          .ThenInclude(f => f.TcmClient)
                                                                          .ThenInclude(f => f.Casemanager)
                                                                          .ThenInclude(f => f.TCMSupervisor)
                                                                          .Include(f => f.TcmServicePlan.TcmClient.Client)
                                                                          .Include(f => f.TcmServicePlan.TCMDomain)
                                                                          .FirstOrDefault(f => (f.TcmServicePlan_FK == IdServicePlan 
                                                                             && f.TcmServicePlan.Approved == 2
                                                                             && f.Id == Id));

                if (tcmServicePlanReview != null)
                {
                    TCMServicePlanReviewViewModel model = _converterHelper.ToTCMServicePlanReviewViewModel(tcmServicePlanReview);
                    model.TCMServicePlanRevDomain = tcmServicePlanReview.TCMServicePlanRevDomain;
                    try
                    {
                        ViewData["origin"] = origin;
                        return View(model);
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }

                }
                else
                {
                    return RedirectToAction("ServicePlanReviewApproved", "TCMServicePlanReviews", new { approved = 0 });
                }
                

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> Edit(TCMServicePlanReviewViewModel tcmServicePlanreviewViewModel, int origin = 0)
        {
            UserEntity user_logged = _context.Users
                                            .Include(u => u.Clinic)
                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMServicePlanReviewEntity tcmServicePlanReviewEntity = _context.TCMServicePlanReviews
                                                                                .Include(n => n.TcmServicePlan)
                                                                                .ThenInclude(n => n.TcmClient)
                                                                                .ThenInclude(n => n.Client)
                                                                                .Include(n => n.TCMMessages)
                                                                                .FirstOrDefault(n => n.Id == tcmServicePlanreviewViewModel.Id);
                tcmServicePlanReviewEntity.Recomendation = tcmServicePlanreviewViewModel.Recomendation;
                tcmServicePlanReviewEntity.SummaryProgress = tcmServicePlanreviewViewModel.SummaryProgress;
                tcmServicePlanReviewEntity.DateServicePlanReview = tcmServicePlanreviewViewModel.DateServicePlanReview;

                tcmServicePlanReviewEntity.ClientHasBeen1 = tcmServicePlanreviewViewModel.ClientHasBeen1;
                tcmServicePlanReviewEntity.ClientContinue = tcmServicePlanreviewViewModel.ClientContinue;
                tcmServicePlanReviewEntity.ClientNoLonger1 = tcmServicePlanreviewViewModel.ClientNoLonger1;
                tcmServicePlanReviewEntity.ClientHasBeen2 = tcmServicePlanreviewViewModel.ClientHasBeen2;
                tcmServicePlanReviewEntity.ClientWillContinue = tcmServicePlanreviewViewModel.ClientWillContinue;
                tcmServicePlanReviewEntity.ClientWillHave = tcmServicePlanreviewViewModel.ClientWillHave;
                tcmServicePlanReviewEntity.ClientNoLonger2 = tcmServicePlanreviewViewModel.ClientNoLonger2;
                tcmServicePlanReviewEntity.TheExpertedReviewDate = tcmServicePlanreviewViewModel.TheExpertedReviewDate;


                tcmServicePlanReviewEntity.DateTCMCaseManagerSignature = tcmServicePlanreviewViewModel.DateTCMCaseManagerSignature;
                tcmServicePlanReviewEntity.DateTCMSupervisorSignature = tcmServicePlanreviewViewModel.DateTCMSupervisorSignature;

                List<TCMMessageEntity> messages = tcmServicePlanReviewEntity.TCMMessages.Where(m => (m.Status == MessageStatus.NotRead && m.Notification == false)).ToList();
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
                        TCMServicePlanReview = tcmServicePlanReviewEntity,
                        TCMAddendum = null,
                        TCMDischarge = null,
                        TCMAssessment = null,
                        Title = "Update on reviewed TCM Service plan review",
                        Text = $"The TCM Service plan review of {tcmServicePlanReviewEntity.TcmServicePlan.TcmClient.Client.Name} on {tcmServicePlanReviewEntity.DateServicePlanReview.ToShortDateString()} was rectified",
                        From = value.To,
                        To = value.From,
                        DateCreated = DateTime.Now,
                        Status = MessageStatus.NotRead,
                        Notification = true
                    };
                    _context.Add(notification);
                }
                _context.Update(tcmServicePlanReviewEntity);
                    try
                    {
                        await _context.SaveChangesAsync();


                        TCMServicePlanReviewEntity servicePlanReview = await _context.TCMServicePlanReviews
                                                                                     .Include(f => f.TcmServicePlan)
                                                                                     .ThenInclude(f => f.TcmClient)
                                                                                     .ThenInclude(f => f.Client)
                                                                                     .Include(f => f.TcmServicePlan.TcmClient.Casemanager)
                                                                                     .FirstOrDefaultAsync(s => s.TcmServicePlan.Id == tcmServicePlanreviewViewModel.IdServicePlan);
                        if (origin == 0)
                        {
                            return RedirectToAction("TCMIntakeSectionDashboard", "TCMIntakes", new { id = servicePlanReview.TcmServicePlan.TcmClient.Id, section = 4 });
                        }
                        else
                        {
                            if (origin == 3)
                            {
                                return RedirectToAction("MessagesOfServicePlanReview", "TCMMessages");
                            }
                            else
                            {
                                if (origin == 4)
                                {
                                    return RedirectToAction("UpdateServicePlanReview", "TCMServicePlanReviews");
                                }
                                else
                                {
                                    return RedirectToAction("ServicePlanReviewApproved", "TCMServicePlanReviews", new { approved = (origin - 1) });
                                }
                            }                        
                        }

                }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
               
                    ModelState.AddModelError(string.Empty, "Already exists the TCM Service Plan Review.");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmServicePlanreviewViewModel) });
                }
            


            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmServicePlanreviewViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> FinishEditing(int id, int origin = 0)
        {
            TCMServicePlanReviewEntity tcmServicePlanReview = _context.TCMServicePlanReviews
                                                                      .Include(n => n.TcmServicePlan)
                                                                      .ThenInclude(n => n.TcmClient)
                                                                      .FirstOrDefault(u => u.Id == id);

            if (tcmServicePlanReview != null)
            {
                if (User.IsInRole("CaseManager"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        tcmServicePlanReview.Approved = 1;
                        _context.Update(tcmServicePlanReview);
                        try
                        {
                            await _context.SaveChangesAsync();
                            if (origin == 0)
                            {
                                return RedirectToAction("TCMIntakeSectionDashboard", "TCMIntakes", new { id = tcmServicePlanReview.TcmServicePlan.TcmClient.Id, section = 4 });
                            }
                            else
                            {
                                return RedirectToAction("ServicePlanReviewApproved", "TCMServicePlanReviews", new { tcmClientId = tcmServicePlanReview.TcmServicePlan.TcmClient.CaseNumber.ToString(), approved = 1 });
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

            return RedirectToAction("Index", "TCMServicePlanReviews");
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
                            DateIdentified = DateTime.Today.Date,
                            Services = list_Services,
                            TcmServicePlan = tcmServicePlan,
                            Id_ServicePlan = id,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now
                        };
                        return View(model);
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
                return RedirectToAction("NotAuthorized", "Account");
            }

            return RedirectToAction("Index", "TCMServicePlans", new { caseNumber = tcmServicePlan.TcmClient.CaseNumber });
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
                ViewData["origi"] = 0;
                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateDomain", model) });

            }

            if (ModelState.IsValid)
            {
                TCMServiceEntity service = _context.TCMServices
                                                   .FirstOrDefault(s => s.Id == tcmDomainViewModel.Id_Service);
                tcmDomainViewModel.Code = service.Code;
                tcmDomainViewModel.Name = service.Name;
                TCMDomainEntity tcmDomainEntity = _context.TCMDomains
                                                          .Include(f => f.TcmServicePlan)
                                                          .FirstOrDefault(g => (g.TcmServicePlan.Id == tcmDomainViewModel.TcmServicePlan.Id
                                                             && g.Code == tcmDomainViewModel.Code));
                if (tcmDomainEntity == null)
                {
                    TCMServicePlanReviewEntity tcmServicePlanReview = await _context.TCMServicePlanReviews
                                                                                    .Include(n => n.TcmServicePlan)
                                                                                    .ThenInclude(n => n.TCMDomain)
                                                                                    .ThenInclude(n => n.TCMObjetive)
                                                                                    .Include(n => n.TCMServicePlanRevDomain)
                                                                                    .ThenInclude(n => n.TCMServicePlanRevDomainObjectiive)
                                                                                    .FirstOrDefaultAsync(n => n.TcmServicePlan.Id == tcmDomainViewModel.Id_ServicePlan);

                    

                    CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
                    tcmDomainEntity = _converterHelper.ToTCMDomainEntity(tcmDomainViewModel, true,"Service Plan Review", user_logged.UserName);

                    TCMServicePlanReviewDomainViewModel TCMServicePlanDomain = new TCMServicePlanReviewDomainViewModel();
                    TCMServicePlanDomain.ChangesUpdate = "";
                    TCMServicePlanDomain.CodeDomain = tcmDomainEntity.Code;
                    TCMServicePlanDomain.TcmDomain = tcmDomainEntity;
                    

                    tcmServicePlanReview.TcmServicePlan.TCMDomain.Add(tcmDomainEntity);
                    tcmServicePlanReview.TCMServicePlanRevDomain.Add(TCMServicePlanDomain);

                    _context.Update(tcmServicePlanReview);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMServicePlanReviewDomainEntity> servicePlanReviewDomain = await _context.TCMServicePlanReviewDomains
                                                                                                  .Include(h => h.TCMServicePlanRevDomainObjectiive)
                                                                                                  .Include(f => f.TcmDomain)
                                                                                                  .ThenInclude(f => f.TcmServicePlan)
                                                                                                  .ThenInclude(f => f.TcmClient)
                                                                                                  .ThenInclude(f => f.Client)
                                                                                                  .Include(f => f.TcmServicePlanReview)
                                                                                                  .Include(t => t.TcmDomain.TcmServicePlan.TcmClient.Casemanager)
                                                                                                  .Include(f => f.TcmServicePlanReview)
                                                                                                  .ThenInclude(t => t.TcmServicePlan)
                                                                                                  .ThenInclude(t => t.TCMSupervisor)
                                                                                                  .ThenInclude(t => t.Clinic)
                                                                                                  .ThenInclude(t => t.Setting)
                                                                                                  .Where(g => (g.TcmServicePlanReview.Id == TCMServicePlanDomain.TcmServicePlanReview.Id))
                                                                                                  .OrderBy(n => n.CodeDomain)
                                                                                                  .ToListAsync();

                        ViewData["origi"] = 0;
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainServicePlanReview", servicePlanReviewDomain) });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ViewData["origi"] = 0;
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateDomain", tcmDomainViewModel) });
                }
            }

            ViewData["origi"] = 0;
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateDomain", tcmDomainViewModel) });
        }


        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateObjetive(int id = 0, int idServicePlanReview = 0)
         {

            TCMObjetiveViewModel model = null;
            TCMDomainEntity tcmdomain = await _context.TCMDomains
                                                .Include(g => g.TCMObjetive)
                                                .Include(h => h.TcmServicePlan)
                                                .ThenInclude(h => h.TCMService)
                                                .ThenInclude(h => h.Stages)
                                                .Include(h => h.TcmServicePlan)
                                                .ThenInclude(h => h.TCMServicePlanReview)
                                                .ThenInclude(h => h.TCMServicePlanRevDomain)
                                                .ThenInclude(h => h.TCMServicePlanRevDomainObjectiive)
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
                        IdObjetive = tcmdomain.TCMObjetive.Count() + 1,
                        StartDate = DateTime.Today.Date,
                        TargetDate = DateTime.Today.Date,
                        EndDate = DateTime.Today.Date,
                        task = "es para que veas el problema del textarea",
                        Origi = 2,
                        IdServicePlanReview = idServicePlanReview
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
                                                        .Include(f => f.TcmServicePlan)
                                                        .ThenInclude(f => f.TcmClient)
                                                        .Include(f => f.TcmServicePlan)
                                                        .ThenInclude(f => f.TCMServicePlanReview)
                                                        .ThenInclude(f => f.TCMServicePlanRevDomain)
                                                        .ThenInclude(f => f.TCMServicePlanRevDomainObjectiive)
                                                        .FirstOrDefault(f => f.Id == tcmObjetiveViewModel.Id_Domain);
                TCMObjetiveEntity tcmObjetiveEntity = await _converterHelper.ToTCMObjetiveEntity(tcmObjetiveViewModel, true, 2, user_logged.UserName);
                tcmObjetiveEntity.Origin = "Service Plan Review";
                tcmObjetiveEntity.TcmDomain = tcmDomain;
                
                if (ModelState.IsValid)
                {
                    TCMServicePlanReviewEntity tcmServicePlanReview = await _context.TCMServicePlanReviews
                                                                                    .Include(n => n.TcmServicePlan)
                                                                                    .ThenInclude(n => n.TCMDomain)
                                                                                    .ThenInclude(n => n.TCMObjetive)
                                                                                    .Include(n => n.TCMServicePlanRevDomain)
                                                                                    .ThenInclude(n => n.TCMServicePlanRevDomainObjectiive)
                                                                                    .FirstOrDefaultAsync(n => n.Id == tcmObjetiveViewModel.IdServicePlanReview);

                    TCMServicePlanReviewDomainObjectiveEntity model;
                    for(int i = 0; i < tcmServicePlanReview.TCMServicePlanRevDomain.Count(); i++)
                    {
                        if (tcmServicePlanReview.TCMServicePlanRevDomain[i].TcmDomain.Id == tcmObjetiveViewModel.Id_Domain)
                        {
                            model = new TCMServicePlanReviewDomainObjectiveEntity
                            {
                                Id = 0,
                                ChangesUpdate = "",
                                DateEndObjective = tcmObjetiveViewModel.EndDate,
                                IdObjective = tcmObjetiveViewModel.IdObjetive,
                                Origin = "Service Plan Review",
                                
                            };
                            if (tcmObjetiveViewModel.Status == 0)
                            {
                                model.Status = StatusType.Open;
                            }
                            else
                            {
                                model.Status = StatusType.Close;
                            }
                            tcmServicePlanReview.TCMServicePlanRevDomain[i].TCMServicePlanRevDomainObjectiive.Add(model);
                            i = tcmServicePlanReview.TCMServicePlanRevDomain.Count();
                            
                        }

                    }

                    
                    _context.Add(tcmObjetiveEntity);
                    _context.Update(tcmServicePlanReview);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMServicePlanReviewDomainEntity> servicePlanReviewDomain = await _context.TCMServicePlanReviewDomains
                                                                                                  .Include(h => h.TCMServicePlanRevDomainObjectiive)
                                                                                                  .Include(f => f.TcmDomain)
                                                                                                  .ThenInclude(f => f.TcmServicePlan)
                                                                                                  .ThenInclude(f => f.TcmClient)
                                                                                                  .ThenInclude(f => f.Client)
                                                                                                  .Include(f => f.TcmServicePlanReview)
                                                                                                  .Include(t => t.TcmDomain.TcmServicePlan.TcmClient.Casemanager)
                                                                                                  .Include(f => f.TcmServicePlanReview)
                                                                                                  .ThenInclude(t => t.TcmServicePlan)
                                                                                                  .ThenInclude(t => t.TCMSupervisor)
                                                                                                  .ThenInclude(t => t.Clinic)
                                                                                                  .ThenInclude(t => t.Setting)
                                                                                                  .Where(g => (g.TcmServicePlanReview.Id == tcmServicePlanReview.Id))
                                                                                                  .OrderBy(n => n.CodeDomain)
                                                                                                  .ToListAsync();

                        ViewData["origi"] = 0;
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainServicePlanReview", servicePlanReviewDomain) });


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
                    ViewData["origi"] = 0;
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateDomain", tcmObjetiveViewModel) });
                }
                ViewData["origi"] = 0;
                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateDomain", tcmObjetiveViewModel) });

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
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
                                                            .Include(u => u.TcmServicePlan)
                                                            .ThenInclude(g => g.TCMService)
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

            if (User.IsInRole("CaseManager") || (User.IsInRole("CaseManager") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                tcmDomainViewModel = _converterHelper.ToTCMDomainViewModel(tcmDomainEntity);
                tcmDomainViewModel.Id_Service = _context.TCMServices.First(n => n.Code == tcmDomainEntity.Code).Id;
            }

            return View(tcmDomainViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
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
                                                           .Include(u => u.TCMServicePlanReview)
                                                           .FirstOrDefault(h => h.Id == tcmDomainViewModel.Id_ServicePlan);

            tcmDomainViewModel.TcmServicePlan = tcmServicePlan;

            if (ModelState.IsValid)
            {
                TCMDomainEntity tcmDomainEntity = _converterHelper.ToTCMDomainEntity(tcmDomainViewModel, false, tcmDomainViewModel.Origin, user_logged.UserName);
                _context.Update(tcmDomainEntity);

                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMServicePlanReviewDomainEntity> servicePlanReviewDomain = await _context.TCMServicePlanReviewDomains
                                                                                                  .Include(h => h.TCMServicePlanRevDomainObjectiive)
                                                                                                  .Include(f => f.TcmDomain)
                                                                                                  .ThenInclude(f => f.TcmServicePlan)
                                                                                                  .ThenInclude(f => f.TcmClient)
                                                                                                  .ThenInclude(f => f.Client)
                                                                                                  .Include(f => f.TcmServicePlanReview)
                                                                                                  .Include(t => t.TcmDomain.TcmServicePlan.TcmClient.Casemanager)
                                                                                                  .Include(f => f.TcmServicePlanReview)
                                                                                                  .ThenInclude(t => t.TcmServicePlan)
                                                                                                  .ThenInclude(t => t.TCMSupervisor)
                                                                                                  .ThenInclude(t => t.Clinic)
                                                                                                  .ThenInclude(t => t.Setting)
                                                                                                  .Where(g => (g.TcmServicePlanReview.Id == tcmServicePlan.TCMServicePlanReview.Id))
                                                                                                  .OrderBy(n => n.CodeDomain)
                                                                                                  .ToListAsync();

                    ViewData["origi"] = 0;
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainServicePlanReview", servicePlanReviewDomain) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }
            ViewData["origi"] = 0;
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditDomain", tcmDomainViewModel) });
        }

        [Authorize(Roles = "Manager, CaseManager, TCMSupervisor")]
        public async Task<IActionResult> EditDomainReadOnly(int? id, int origi = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMDomainEntity tcmDomainEntity = await _context.TCMDomains
                                                            .Include(u => u.TcmServicePlan)
                                                            .ThenInclude(g => g.TcmClient)
                                                            .ThenInclude(g => g.Client)
                                                            .Include(u => u.TcmServicePlan)
                                                            .ThenInclude(g => g.TCMService)
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

            tcmDomainViewModel = _converterHelper.ToTCMDomainViewModel(tcmDomainEntity);
            tcmDomainViewModel.Id_Service = _context.TCMServices.First(n => n.Code == tcmDomainEntity.Code).Id;
            tcmDomainViewModel.Status = tcmDomainEntity.Status;
            ViewData["origi"] = origi;
            return View(tcmDomainViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> EditDomainReadOnly(int id, TCMDomainViewModel tcmDomainViewModel)
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
            TCMServicePlanReviewDomainEntity servciplanR_Domain = _context.TCMServicePlanReviewDomains
                                                                          .Include(n => n.TcmServicePlanReview)
                                                                          .FirstOrDefault(n => n.TcmDomain.Id == tcmDomainViewModel.Id);
            if (tcmDomainViewModel.Status == false)
            {
                if (tcmDomainViewModel.Origin == "Service Plan Review")
                {
                    servciplanR_Domain.Status = SPRStatus.Added;
                }
                else
                {
                    servciplanR_Domain.Status = SPRStatus.Open;
                }
            }
            else
            {
                servciplanR_Domain.Status = SPRStatus.Closed;
            }
            servciplanR_Domain.Status = (tcmDomainViewModel.Status == false) ? SPRStatus.Open : SPRStatus.Closed;
            _context.Update(servciplanR_Domain);
            tcmDomainViewModel.TcmServicePlan = tcmServicePlan;

            if (ModelState.IsValid)
            {
                TCMDomainEntity tcmDomainEntity = _converterHelper.ToTCMDomainEntity(tcmDomainViewModel, false, tcmDomainViewModel.Origin, user_logged.UserName);
                _context.Update(tcmDomainEntity);
               

                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMServicePlanReviewDomainEntity> servicePlanReviewDomain = await _context.TCMServicePlanReviewDomains
                                                                                                   .Include(h => h.TCMServicePlanRevDomainObjectiive)
                                                                                                   .Include(f => f.TcmDomain)
                                                                                                   .ThenInclude(f => f.TcmServicePlan)
                                                                                                   .ThenInclude(f => f.TcmClient)
                                                                                                   .ThenInclude(f => f.Client)
                                                                                                   .Include(f => f.TcmServicePlanReview)
                                                                                                   .Include(t => t.TcmDomain.TcmServicePlan.TcmClient.Casemanager)
                                                                                                   .Include(f => f.TcmServicePlanReview)
                                                                                                   .ThenInclude(t => t.TcmServicePlan)
                                                                                                   .ThenInclude(t => t.TCMSupervisor)
                                                                                                   .ThenInclude(t => t.Clinic)
                                                                                                   .ThenInclude(t => t.Setting)
                                                                                                   .Where(g => (g.TcmServicePlanReview.Id == servciplanR_Domain.TcmServicePlanReview.Id))
                                                                                                   .OrderBy(n => n.CodeDomain)
                                                                                                   .ToListAsync();

                    ViewData["origi"] = 0;
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainServicePlanReview", servicePlanReviewDomain) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }
            ViewData["origi"] = 0;
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditDomain", tcmDomainViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult DeleteDomain(int id = 0)
        {
            if (id > 0)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                DeleteViewModel model = new DeleteViewModel
                {
                    Id_Element = id,
                    Desciption = "Do you want to delete this record?"

                };
                return View(model);
            }
            else
            {
                //Edit
                //return View(new Client_DiagnosticViewModel());
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteDomain(DeleteViewModel DomainViewModel)
        {
            TCMServicePlanReviewDomainEntity TCMServicePlanRevDomain = await _context.TCMServicePlanReviewDomains
                                                                                     .Include(n => n.TCMServicePlanRevDomainObjectiive)
                                                                                     .Include(n => n.TcmServicePlanReview)
                                                                                     .Include(n => n.TcmDomain)                                                                                     
                                                                                     .ThenInclude(n => n.TCMObjetive)
                                                                                     .FirstOrDefaultAsync(n => n.Id == DomainViewModel.Id_Element);

            if (TCMServicePlanRevDomain != null)
            {
                _context.TCMServicePlanReviewDomainObjectives.RemoveRange(TCMServicePlanRevDomain.TCMServicePlanRevDomainObjectiive);
            }

            TCMDomainEntity tcmDomainEntity = await _context.TCMDomains.FirstOrDefaultAsync(s => s.Id == TCMServicePlanRevDomain.TcmDomain.Id);
            if (tcmDomainEntity != null)
            {
                _context.TCMDomains.Remove(tcmDomainEntity);
            }
            if (TCMServicePlanRevDomain == null)
            {
                return RedirectToAction("Home/Error404");
            }

            List<TCMServicePlanReviewDomainEntity> servicePlanReviewDomainList = new List<TCMServicePlanReviewDomainEntity>();
            
            if (ModelState.IsValid)
            {

                try
                {
                    _context.TCMServicePlanReviewDomains.Remove(TCMServicePlanRevDomain);
                    await _context.SaveChangesAsync();
                    
                    servicePlanReviewDomainList = await _context.TCMServicePlanReviewDomains
                                                                .Include(h => h.TCMServicePlanRevDomainObjectiive)
                                                                .Include(f => f.TcmDomain)
                                                                .ThenInclude(f => f.TcmServicePlan)
                                                                .ThenInclude(f => f.TcmClient)
                                                                .ThenInclude(f => f.Client)
                                                                .Include(f => f.TcmServicePlanReview)
                                                                .Include(t => t.TcmDomain.TcmServicePlan.TcmClient.Casemanager)
                                                                .Include(f => f.TcmServicePlanReview)
                                                                .ThenInclude(t => t.TcmServicePlan)
                                                                .ThenInclude(t => t.TCMSupervisor)
                                                                .ThenInclude(t => t.Clinic)
                                                                .ThenInclude(t => t.Setting)
                                                                .Where(g => (g.TcmServicePlanReview.Id == TCMServicePlanRevDomain.TcmServicePlanReview.Id))
                                                                .OrderBy(n => n.CodeDomain)
                                                                .ToListAsync();
                    ViewData["origi"] = 0;
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainServicePlanReview", servicePlanReviewDomainList) });
                }
                catch (Exception)
                {
                    servicePlanReviewDomainList = await _context.TCMServicePlanReviewDomains
                                                                .Include(h => h.TCMServicePlanRevDomainObjectiive)
                                                                .Include(f => f.TcmDomain)
                                                                .ThenInclude(f => f.TcmServicePlan)
                                                                .ThenInclude(f => f.TcmClient)
                                                                .ThenInclude(f => f.Client)
                                                                .Include(f => f.TcmServicePlanReview)
                                                                .Include(t => t.TcmDomain.TcmServicePlan.TcmClient.Casemanager)
                                                                .Include(f => f.TcmServicePlanReview)
                                                                .ThenInclude(t => t.TcmServicePlan)
                                                                .ThenInclude(t => t.TCMSupervisor)
                                                                .ThenInclude(t => t.Clinic)
                                                                .ThenInclude(t => t.Setting)
                                                                .Where(g => (g.TcmServicePlanReview.Id == TCMServicePlanRevDomain.TcmServicePlanReview.Id))
                                                                .OrderBy(n => n.CodeDomain)
                                                                .ToListAsync();
                    ViewData["origi"] = 0;
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainServicePlanReview", servicePlanReviewDomainList) });
                }

               
            }

            servicePlanReviewDomainList = await _context.TCMServicePlanReviewDomains
                                                        .Include(h => h.TCMServicePlanRevDomainObjectiive)
                                                        .Include(f => f.TcmDomain)
                                                        .ThenInclude(f => f.TcmServicePlan)
                                                        .ThenInclude(f => f.TcmClient)
                                                        .ThenInclude(f => f.Client)
                                                        .Include(f => f.TcmServicePlanReview)
                                                        .Include(t => t.TcmDomain.TcmServicePlan.TcmClient.Casemanager)
                                                        .Include(f => f.TcmServicePlanReview)
                                                        .ThenInclude(t => t.TcmServicePlan)
                                                        .ThenInclude(t => t.TCMSupervisor)
                                                        .ThenInclude(t => t.Clinic)
                                                        .ThenInclude(t => t.Setting)
                                                        .Where(g => (g.TcmServicePlanReview.Id == TCMServicePlanRevDomain.TcmServicePlanReview.Id))
                                                        .OrderBy(n => n.CodeDomain)
                                                        .ToListAsync();
            ViewData["origi"] = 0;
            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainServicePlanReview", servicePlanReviewDomainList) });

        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> EditObjetive(int id = 0)
        {
            UserEntity user_logged = await _context.Users

                                                  .Include(u => u.Clinic)
                                                  .ThenInclude(c => c.Setting)

                                                  .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {

                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }


                TCMServicePlanReviewDomainObjectiveEntity tcmObjetiveEntity = await _context.TCMServicePlanReviewDomainObjectives
                                                                                            .Include(n => n.tcmServicePlanReviewDomain)
                                                                                            .ThenInclude(n => n.TcmServicePlanReview)
                                                                                            .Include(n => n.tcmServicePlanReviewDomain)
                                                                                            .ThenInclude(n => n.TcmDomain)
                                                                                            .ThenInclude(n => n.TCMObjetive)
                                                                                            .FirstOrDefaultAsync(s => s.Id == id);

                TCMObjetiveEntity objetiveEntity = _context.TCMObjetives
                                                        .Include(g => g.TcmDomain)
                                                        .ThenInclude(g => g.TcmServicePlan)
                                                        .ThenInclude(g => g.TCMServicePlanReview)
                                                        .FirstOrDefault(n => n.IdObjetive == tcmObjetiveEntity.IdObjective
                                                          && n.TcmDomain.Id == tcmObjetiveEntity.tcmServicePlanReviewDomain.TcmDomain.Id);


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

                TCMObjetiveViewModel model = _converterHelper.ToTCMObjetiveViewModel(objetiveEntity);
                model.ChangesUpdates = tcmObjetiveEntity.ChangesUpdate;
                model.Id_Stage = stage.Id;
                model.Stages = list;
               
                return View(model);

            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager,TCMSupervisor")]
        public async Task<IActionResult> EditObjetive(TCMObjetiveViewModel tcmObjetiveViewModel)
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

                TCMServicePlanReviewDomainObjectiveEntity tcmServicePlanReviewObjetive = await _context.TCMServicePlanReviewDomainObjectives
                                                                                                       .Include(n => n.tcmServicePlanReviewDomain)
                                                                                                       .ThenInclude(n => n.TcmServicePlanReview)
                                                                                                       .FirstOrDefaultAsync(m => m.Id == tcmObjetiveViewModel.Id);

                tcmObjetiveViewModel.Id = tcmObjetiveViewModel.Idd;
                tcmServicePlanReviewObjetive.Status = StatusUtils.GetStatusByIndex(tcmObjetiveViewModel.IdStatus);
                tcmServicePlanReviewObjetive.ChangesUpdate = tcmObjetiveViewModel.ChangesUpdates;
                tcmServicePlanReviewObjetive.DateEndObjective = tcmObjetiveViewModel.EndDate;


                TCMDomainEntity tcmDomain = _context.TCMDomains
                                                    .Include(f => f.TcmServicePlan)
                                                    .ThenInclude(f => f.TcmClient)
                                                    .FirstOrDefault(f => f.Id == tcmObjetiveViewModel.Id_Domain);
                TCMObjetiveEntity tcmObjetiveEntity = await _converterHelper.ToTCMObjetiveEntity(tcmObjetiveViewModel, false, tcmObjetiveViewModel.Origi, user_logged.UserName);
                tcmObjetiveEntity.TcmDomain = tcmDomain;

                if (ModelState.IsValid)
                {
                    _context.Update(tcmObjetiveEntity);
                    _context.Update(tcmServicePlanReviewObjetive);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMServicePlanReviewDomainEntity> servicePlanReviewDomain = await _context.TCMServicePlanReviewDomains
                                                                                                    .Include(h => h.TCMServicePlanRevDomainObjectiive)
                                                                                                    .Include(f => f.TcmDomain)
                                                                                                    .ThenInclude(f => f.TcmServicePlan)
                                                                                                    .ThenInclude(f => f.TcmClient)
                                                                                                    .ThenInclude(f => f.Client)
                                                                                                    .Include(f => f.TcmServicePlanReview)
                                                                                                    .Include(t => t.TcmDomain.TcmServicePlan.TcmClient.Casemanager)
                                                                                                    .Include(t => t.TcmServicePlanReview)
                                                                                                    .ThenInclude(t => t.TcmServicePlan)
                                                                                                    .ThenInclude(t => t.TCMSupervisor)
                                                                                                    .ThenInclude(t => t.Clinic)
                                                                                                    .ThenInclude(t => t.Setting)
                                                                                                    .Where(g => (g.TcmDomain.TcmServicePlan.Id == tcmDomain.TcmServicePlan.Id))
                                                                                                    .OrderBy(n => n.TcmDomain.Code)
                                                                                                    .ToListAsync();

                        ViewData["origi"] = 0;
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainServicePlanReview", servicePlanReviewDomain) });

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
                    ViewData["origi"] = 0;
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjetive", tcmObjetiveViewModel) });
                }
                ViewData["origi"] = 0;
                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjetive", tcmObjetiveViewModel) });

            }
            ViewData["origi"] = 0;
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult DeleteObjetive(int id = 0)
        {
            if (id > 0)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                DeleteViewModel model = new DeleteViewModel
                {
                    Id_Element = id,
                    Desciption = "Do you want to delete this record?"

                };
                return View(model);
            }
            else
            {
                //Edit
                //return View(new Client_DiagnosticViewModel());
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteObjetive(DeleteViewModel ObjectiveViewModel)
        {
            TCMServicePlanReviewDomainObjectiveEntity tcmObjetiveEntity = await _context.TCMServicePlanReviewDomainObjectives
                                                                                         .Include(n => n.tcmServicePlanReviewDomain)
                                                                                         .ThenInclude(n => n.TcmServicePlanReview)
                                                                                         .Include(n => n.tcmServicePlanReviewDomain)
                                                                                         .ThenInclude(n => n.TcmDomain)
                                                                                         .ThenInclude(n => n.TCMObjetive)
                                                                                         .FirstOrDefaultAsync(s => s.Id == ObjectiveViewModel.Id_Element);

            TCMObjetiveEntity tcmObjetive = _context.TCMObjetives
                                                    .FirstOrDefault(n => n.IdObjetive == tcmObjetiveEntity.IdObjective
                                                      && n.TcmDomain.Id == tcmObjetiveEntity.tcmServicePlanReviewDomain.TcmDomain.Id);

            if (tcmObjetiveEntity != null)
            {
                _context.TCMServicePlanReviewDomainObjectives.Remove(tcmObjetiveEntity);
            }

            if (tcmObjetive != null)
            {
                _context.TCMObjetives.Remove(tcmObjetive);
            }

           

            List<TCMServicePlanReviewDomainEntity> servicePlanReviewDomainList = new List<TCMServicePlanReviewDomainEntity>();

            if (ModelState.IsValid)
            {

                try
                {
                    await _context.SaveChangesAsync();

                    servicePlanReviewDomainList = await _context.TCMServicePlanReviewDomains
                                                                .Include(h => h.TCMServicePlanRevDomainObjectiive)
                                                                .Include(f => f.TcmDomain)
                                                                .ThenInclude(f => f.TcmServicePlan)
                                                                .ThenInclude(f => f.TcmClient)
                                                                .ThenInclude(f => f.Client)
                                                                .Include(f => f.TcmServicePlanReview)
                                                                .Include(t => t.TcmDomain.TcmServicePlan.TcmClient.Casemanager)
                                                                .Include(f => f.TcmServicePlanReview)
                                                                .ThenInclude(t => t.TcmServicePlan)
                                                                .ThenInclude(t => t.TCMSupervisor)
                                                                .ThenInclude(t => t.Clinic)
                                                                .ThenInclude(t => t.Setting)
                                                                .Where(g => (g.TcmServicePlanReview.Id == tcmObjetiveEntity.tcmServicePlanReviewDomain.TcmServicePlanReview.Id))
                                                                .OrderBy(n => n.CodeDomain)
                                                                .ToListAsync();
                    ViewData["origi"] = 0;
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainServicePlanReview", servicePlanReviewDomainList) });
                }
                catch (Exception)
                {
                    servicePlanReviewDomainList = await _context.TCMServicePlanReviewDomains
                                                                .Include(h => h.TCMServicePlanRevDomainObjectiive)
                                                                .Include(f => f.TcmDomain)
                                                                .ThenInclude(f => f.TcmServicePlan)
                                                                .ThenInclude(f => f.TcmClient)
                                                                .ThenInclude(f => f.Client)
                                                                .Include(f => f.TcmServicePlanReview)
                                                                .Include(t => t.TcmDomain.TcmServicePlan.TcmClient.Casemanager)
                                                                .Include(f => f.TcmServicePlanReview)
                                                                .ThenInclude(t => t.TcmServicePlan)
                                                                .ThenInclude(t => t.TCMSupervisor)
                                                                .ThenInclude(t => t.Clinic)
                                                                .ThenInclude(t => t.Setting)
                                                                .Where(g => (g.TcmServicePlanReview.Id == tcmObjetiveEntity.tcmServicePlanReviewDomain.TcmServicePlanReview.Id))
                                                                .OrderBy(n => n.CodeDomain)
                                                                .ToListAsync();
                    ViewData["origi"] = 0;
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainServicePlanReview", servicePlanReviewDomainList) });
                }


            }

            servicePlanReviewDomainList = await _context.TCMServicePlanReviewDomains
                                                        .Include(h => h.TCMServicePlanRevDomainObjectiive)
                                                        .Include(f => f.TcmDomain)
                                                        .ThenInclude(f => f.TcmServicePlan)
                                                        .ThenInclude(f => f.TcmClient)
                                                        .ThenInclude(f => f.Client)
                                                        .Include(f => f.TcmServicePlanReview)
                                                        .Include(t => t.TcmDomain.TcmServicePlan.TcmClient.Casemanager)
                                                        .Include(f => f.TcmServicePlanReview)
                                                        .ThenInclude(t => t.TcmServicePlan)
                                                        .ThenInclude(t => t.TCMSupervisor)
                                                        .ThenInclude(t => t.Clinic)
                                                        .ThenInclude(t => t.Setting)
                                                        .Where(g => (g.TcmServicePlanReview.Id == tcmObjetiveEntity.tcmServicePlanReviewDomain.TcmServicePlanReview.Id))
                                                        .OrderBy(n => n.CodeDomain)
                                                        .ToListAsync();
            ViewData["origi"] = 0;
            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainServicePlanReview", servicePlanReviewDomainList) });

        }

        [Authorize(Roles = "TCMSupervisor, Manager, CaseManager")]
        public async Task<IActionResult> EditObjetiveReadOnly(int id = 0, int origi = 0)
        {
            UserEntity user_logged = await _context.Users

                                                  .Include(u => u.Clinic)
                                                  .ThenInclude(c => c.Setting)

                                                  .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor") || User.IsInRole("Manager"))
            {

                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }


                TCMServicePlanReviewDomainObjectiveEntity tcmObjetiveEntity = await _context.TCMServicePlanReviewDomainObjectives
                                                                                            .Include(n => n.tcmServicePlanReviewDomain)
                                                                                            .ThenInclude(n => n.TcmServicePlanReview)
                                                                                            .Include(n => n.tcmServicePlanReviewDomain)
                                                                                            .ThenInclude(n => n.TcmDomain)
                                                                                            .ThenInclude(n => n.TCMObjetive)
                                                                                            .FirstOrDefaultAsync(s => s.Id == id);

                TCMObjetiveEntity objetiveEntity = _context.TCMObjetives
                                                        .Include(g => g.TcmDomain)
                                                        .ThenInclude(g => g.TcmServicePlan)
                                                        .ThenInclude(g => g.TCMServicePlanReview)
                                                        .FirstOrDefault(n => n.IdObjetive == tcmObjetiveEntity.IdObjective
                                                          && n.TcmDomain.Id == tcmObjetiveEntity.tcmServicePlanReviewDomain.TcmDomain.Id);


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

                TCMObjetiveViewModel model = _converterHelper.ToTCMObjetiveViewModel(objetiveEntity);
                model.ChangesUpdates = tcmObjetiveEntity.ChangesUpdate;
                model.Id_Stage = stage.Id;
                model.Stages = list;
                ViewData["origi"] = origi;
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
        public async Task<IActionResult> EditObjetiveReadOnly(TCMObjetiveViewModel tcmObjetiveViewModel)
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

                TCMServicePlanReviewDomainObjectiveEntity tcmServicePlanReviewObjetive = await _context.TCMServicePlanReviewDomainObjectives
                                                                                                       .Include(n => n.tcmServicePlanReviewDomain)
                                                                                                       .ThenInclude(n => n.TcmServicePlanReview)
                                                                                                       .FirstOrDefaultAsync(m => m.Id == tcmObjetiveViewModel.Id);

                tcmObjetiveViewModel.Id = tcmObjetiveViewModel.Idd;
                tcmServicePlanReviewObjetive.Status = StatusUtils.GetStatusByIndex(tcmObjetiveViewModel.IdStatus);
                tcmServicePlanReviewObjetive.ChangesUpdate = tcmObjetiveViewModel.ChangesUpdates;
                tcmServicePlanReviewObjetive.DateEndObjective = tcmObjetiveViewModel.EndDate;


                TCMDomainEntity tcmDomain = _context.TCMDomains
                                                    .Include(f => f.TcmServicePlan)
                                                    .ThenInclude(f => f.TcmClient)
                                                    .FirstOrDefault(f => f.Id == tcmObjetiveViewModel.Id_Domain);
                TCMObjetiveEntity tcmObjetiveEntity = await _converterHelper.ToTCMObjetiveEntity(tcmObjetiveViewModel, false, tcmObjetiveViewModel.Origi, user_logged.UserName);
                tcmObjetiveEntity.TcmDomain = tcmDomain;

                if (ModelState.IsValid)
                {
                    _context.Update(tcmObjetiveEntity);
                    _context.Update(tcmServicePlanReviewObjetive);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMServicePlanReviewDomainEntity> servicePlanReviewDomain = await _context.TCMServicePlanReviewDomains
                                                                                                       .Include(h => h.TCMServicePlanRevDomainObjectiive)
                                                                                                       .Include(f => f.TcmDomain)
                                                                                                       .ThenInclude(f => f.TcmServicePlan)
                                                                                                       .ThenInclude(f => f.TcmClient)
                                                                                                       .ThenInclude(f => f.Client)
                                                                                                       .Include(f => f.TcmServicePlanReview)
                                                                                                       .Include(t => t.TcmDomain.TcmServicePlan.TcmClient.Casemanager)
                                                                                                       .Include(t => t.TcmServicePlanReview)
                                                                                                       .ThenInclude(t => t.TcmServicePlan)
                                                                                                       .ThenInclude(t => t.TCMSupervisor)
                                                                                                       .ThenInclude(t => t.Clinic)
                                                                                                       .ThenInclude(t => t.Setting)
                                                                                                       .Where(g => (g.TcmDomain.TcmServicePlan.TcmClient.Casemanager.LinkedUser == user_logged.UserName))
                                                                                                       .OrderBy(n => n.TcmDomain.Code)
                                                                                                       .ToListAsync();

                        ViewData["origi"] = 0;
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainServicePlanReview", servicePlanReviewDomain) });

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
                    ViewData["origi"] = 0;
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjetive", tcmObjetiveViewModel) });
                }
                ViewData["origi"] = 0;
                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjetive", tcmObjetiveViewModel) });

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> ServicePlanReviewApproved(string tcmClientId = "", int approved = 0)
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
            CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
            TCMClientEntity tcmClient = await _context.TCMClient.FirstOrDefaultAsync(n => n.CaseNumber == tcmClientId);

            if (User.IsInRole("CaseManager") == true)
            {
                List<TCMServicePlanReviewEntity> ServicePlanReview = new List<TCMServicePlanReviewEntity>();
                if (tcmClientId == "")
                {
                    ServicePlanReview = await _context.TCMServicePlanReviews
                                                      .Include(f => f.TcmServicePlan)
                                                      .ThenInclude(f => f.TcmClient)
                                                      .ThenInclude(f => f.Client)
                                                      .Include(f => f.TcmServicePlan.TcmClient.Casemanager)
                                                      .Include(f => f.TCMMessages)
                                                      .Where(s => s.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == user_logged.Clinic.Id
                                                               && s.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id
                                                               && s.Approved == approved)
                                                      .OrderBy(f => f.TcmServicePlan.TcmClient.CaseNumber)
                                                      .ToListAsync();
                }
                else
                {
                    ServicePlanReview = await _context.TCMServicePlanReviews
                                                      .Include(f => f.TcmServicePlan)
                                                      .ThenInclude(f => f.TcmClient)
                                                      .ThenInclude(f => f.Client)
                                                      .Include(f => f.TcmServicePlan.TcmClient.Casemanager)
                                                      .Include(f => f.TCMMessages)
                                                      .Where(s => s.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == user_logged.Clinic.Id
                                                            && s.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id
                                                            && s.TcmServicePlan.TcmClient.CaseNumber == tcmClientId
                                                            && s.Approved == approved)
                                                      .OrderBy(f => f.TcmServicePlan.TcmClient.CaseNumber)
                                                      .ToListAsync();

                    ViewData["tcmClientId"] = tcmClientId;
                    if (tcmClient != null)
                        ViewData["Id"] = tcmClient.Id;
                }


                return View(ServicePlanReview);
            }
            if (User.IsInRole("Manager") == true)
            {
                List<TCMServicePlanReviewEntity> ServicePlanReview = await _context.TCMServicePlanReviews
                                                                                   .Include(f => f.TcmServicePlan)
                                                                                   .ThenInclude(f => f.TcmClient)
                                                                                   .ThenInclude(f => f.Client)
                                                                                   .Include(f => f.TcmServicePlan.TcmClient.Casemanager)
                                                                                   .Include(f => f.TCMMessages)
                                                                                   .Where(s => s.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == user_logged.Clinic.Id
                                                                                        && s.Approved == approved)
                                                                                   .OrderBy(f => f.TcmServicePlan.TcmClient.CaseNumber)
                                                                                   .ToListAsync();

                return View(ServicePlanReview);
            }
            if (User.IsInRole("TCMSupervisor") == true)
            {
                List<TCMServicePlanReviewEntity> ServicePlanReview = await _context.TCMServicePlanReviews
                                                                                   .Include(f => f.TcmServicePlan)
                                                                                   .ThenInclude(f => f.TcmClient)
                                                                                   .ThenInclude(f => f.Client)
                                                                                   .Include(f => f.TcmServicePlan.TcmClient.Casemanager)
                                                                                   .Include(f => f.TCMMessages)
                                                                                   .Where(s => s.TcmServicePlan.TcmClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                                                                        && s.Approved == approved)
                                                                                   .OrderBy(f => f.TcmServicePlan.TcmClient.CaseNumber)
                                                                                   .ToListAsync();

                return View(ServicePlanReview);
            }

            return View(null);

        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public IActionResult EditReadOnly(int Id, int IdServicePlan, int origi = 0)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                        .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor") || User.IsInRole("CaseManager"))
            {
                TCMServicePlanReviewEntity tcmServicePlanReview = _context.TCMServicePlanReviews
                                                                          .Include(b => b.TCMServicePlanRevDomain)
                                                                          .ThenInclude(f => f.TCMServicePlanRevDomainObjectiive)
                                                                          .Include(f => f.TcmServicePlan)
                                                                          .ThenInclude(f => f.TcmClient)
                                                                          .ThenInclude(f => f.Casemanager)
                                                                          .Include(f => f.TcmServicePlan)
                                                                          .ThenInclude(f => f.TCMSupervisor)
                                                                          .ThenInclude(f => f.Clinic)
                                                                          .ThenInclude(f => f.Setting)
                                                                          .Include(f => f.TcmServicePlan.TcmClient.Client)
                                                                          .Include(f => f.TcmServicePlan.TCMDomain)
                                                                          .FirstOrDefault(f => (f.TcmServicePlan_FK == IdServicePlan
                                                                             && f.TcmServicePlan.Approved == 2
                                                                             && f.Id == Id));

                if (tcmServicePlanReview != null)
                {
                    TCMServicePlanReviewViewModel model = _converterHelper.ToTCMServicePlanReviewViewModel(tcmServicePlanReview);
                    model.TCMServicePlanRevDomain = tcmServicePlanReview.TCMServicePlanRevDomain;
                    try
                    {
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }

                }
                else
                {
                    return RedirectToAction("ServicePlanReviewApproved", "TCMServicePlanReviews", new { approved = 1 });
                }


            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> ApproveTCMServicePlanReview(TCMServicePlanReviewViewModel model, int id, int origi = 0)
        {
            TCMServicePlanReviewEntity tcmServicePlanReview = _context.TCMServicePlanReviews
                                                                      .Include(n => n.TcmServicePlan)
                                                                      .ThenInclude(n => n.TcmClient)
                                                                      .FirstOrDefault(u => u.Id == id);

            if (tcmServicePlanReview != null)
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        tcmServicePlanReview.Approved = 2;
                        tcmServicePlanReview.TCMSupervisor = await _context.TCMSupervisors.FirstOrDefaultAsync(n => n.LinkedUser == user_logged.UserName);
                        tcmServicePlanReview.DateTCMCaseManagerSignature = model.DateTCMCaseManagerSignature;
                        tcmServicePlanReview.DateTCMSupervisorSignature = model.DateTCMSupervisorSignature;

                        tcmServicePlanReview.ClientHasBeen1 = model.ClientHasBeen1;
                        tcmServicePlanReview.ClientContinue = model.ClientContinue;
                        tcmServicePlanReview.ClientNoLonger1 = model.ClientNoLonger1;
                        tcmServicePlanReview.ClientHasBeen2 = model.ClientHasBeen2;
                        tcmServicePlanReview.ClientWillContinue = model.ClientWillContinue;
                        tcmServicePlanReview.ClientWillHave = model.ClientWillHave;
                        tcmServicePlanReview.ClientNoLonger2 = model.ClientNoLonger2;
                        tcmServicePlanReview.TheExpertedReviewDate = model.TheExpertedReviewDate;

                        
                        _context.Update(tcmServicePlanReview);
                        TCMClientEntity tcmClient = _context.TCMClient.FirstOrDefault(n => n.Id == tcmServicePlanReview.TcmServicePlan.TcmClient.Id);
                        tcmClient.DataClose = tcmClient.DataClose.AddMonths(6);
                        tcmClient.Period = tcmClient.Period + 6;
                        _context.Update(tcmClient);
                        try
                        {
                            await _context.SaveChangesAsync();
                            if (origi == 0)
                            {
                                return RedirectToAction("ServicePlanReviewApproved", "TCMServicePlanReviews", new { approved = 1 });
                            }
                            if (origi == 1)
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

            return RedirectToAction("Index", "TCMServicePlanReviews");
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
                    IdTCMServiceplanReview = id,
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
                model.To = model.TCMServicePlanReview.CreatedBy;
                _context.Add(model);
                await _context.SaveChangesAsync();
            }

            if (messageViewModel.Origin == 1)
                return RedirectToAction("ServicePlanReviewApproved", new { approved = 1 });

            if (messageViewModel.Origin == 2)
                return RedirectToAction("TCMServicePlanWithReview");
            if (messageViewModel.Origin == 3)
                return RedirectToAction("Notifications", "TCMMessages");

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> ServicePlanReviewReturnTo(int? id, int tcmClientId = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMServicePlanReviewEntity servicePlanReview = await _context.TCMServicePlanReviews.FirstOrDefaultAsync(s => s.Id == id);
            if (servicePlanReview == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                servicePlanReview.Approved = 0;
                _context.TCMServicePlanReviews.Update(servicePlanReview);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("TCMCaseHistory", "TCMClients", new { id = tcmClientId });
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteSPR(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            List<TCMMessageEntity> messageEntity = await _context.TCMMessages
                                                                 .Include(n => n.TCMAddendum)
                                                                 .Where(d => d.TCMServicePlanReview.Id == id)
                                                                 .ToListAsync();

            _context.TCMMessages.RemoveRange(messageEntity);
            await _context.SaveChangesAsync();

            TCMServicePlanReviewEntity servicePlanReview = await _context.TCMServicePlanReviews
                                                                         .Include(n => n.TCMServicePlanRevDomain)
                                                                         .ThenInclude(n => n.TCMServicePlanRevDomainObjectiive)
                                                                         .Include(n => n.TcmServicePlan)
                                                                         .ThenInclude(n => n.TcmClient)
                                                                         .FirstOrDefaultAsync(d => d.Id == id);

           /* List<TCMNoteActivityEntity> noteActivity = await _context.TCMNoteActivity
                                                                     .Include(n => n.TCMNote)
                                                                     .Where(m => m.TCMDomain.Id == )
                                                                     .ToListAsync();

            _context.TCMNoteActivity.RemoveRange(noteActivity);
            await _context.SaveChangesAsync();*/

            _context.TCMServicePlanReviewDomains.RemoveRange(servicePlanReview.TCMServicePlanRevDomain);
            await _context.SaveChangesAsync();

            _context.TCMServicePlanReviews.Remove(servicePlanReview);
            await _context.SaveChangesAsync();

            return RedirectToAction("TCMCaseHistory", "TCMClients", new { id = servicePlanReview.TcmServicePlan.TcmClient.Id });

        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> UpdateServicePlanReview(int id = 0)
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
                return View(await _context.TCMServicePlanReviews
                                          .Include(n => n.TcmServicePlan)
                                          .ThenInclude(n => n.TcmClient)
                                          .ThenInclude(n => n.Casemanager)
                                          .Include(n => n.TcmServicePlan)
                                          .ThenInclude(n => n.TcmClient)
                                          .ThenInclude(n => n.Client)
                                          .ThenInclude(n => n.Clinic)
                                          .ThenInclude(n => n.Setting)

                                          .Include(n => n.TCMServicePlanRevDomain)
                                          .ThenInclude(n => n.TCMServicePlanRevDomainObjectiive)
                                         
                                          .Include(n => n.TCMServicePlanRevDomain)
                                          .ThenInclude(n => n.TcmDomain)

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

    }
}
