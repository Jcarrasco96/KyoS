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


            return View(await _context.TCMServicePlanReviews
                                      .Include(f => f.TcmServicePlan)
                                      .ThenInclude(f => f.TcmClient)
                                      .ThenInclude(f => f.Client)
                                      .Include(f => f.TcmServicePlan.TcmClient.Casemanager)
                                      .Where(s => s.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == user_logged.Clinic.Id)
                                      .OrderBy(f => f.TcmServicePlan.TcmClient.CaseNumber)
                                      .ToListAsync());
           
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
                                          && s.Approved == 2))
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
        public async Task<IActionResult> Create(int id, int IdServicePlan)
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
                    tcmDomainReview.Status = StatusType.Open;
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
                                                                                  .FirstOrDefaultAsync(s => s.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == user_logged.Clinic.Id)
                                                                                  ;

                            return RedirectToAction("TCMIntakeSectionDashboard", "TCMIntakes", new { id = servicePlanReview.TcmServicePlan.TcmClient.Id, section = 4 });
                        }
                        else
                        {
                            List<TCMServicePlanReviewEntity> servicePlanReview = await _context.TCMServicePlanReviews
                                                                                   .Include(f => f.TcmServicePlan)
                                                                                   .ThenInclude(f => f.TcmClient)
                                                                                   .ThenInclude(f => f.Client)
                                                                                   .Include(f => f.TcmServicePlan.TcmClient.Casemanager)
                                                                                   .Where(s => s.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == user_logged.Clinic.Id)
                                                                                   .OrderBy(f => f.TcmServicePlan.TcmClient.CaseNumber)
                                                                                   .ToListAsync();

                            return RedirectToAction("Index", "TCMServicePlanReviews");
                        }
                        
                    }
                    catch (System.Exception ex)
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

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Edit(int Id, int IdServicePlan, int origin = 0)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                        .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("CaseManager"))
            {
                TCMServicePlanReviewEntity tcmServicePlanReview = _context.TCMServicePlanReviews
                                                                          .Include(b => b.TCMServicePlanRevDomain)
                                                                          .ThenInclude(f => f.TCMServicePlanRevDomainObjectiive)
                                                                          .Include(f => f.TcmServicePlan)
                                                                          .ThenInclude(f => f.TcmClient)
                                                                          .ThenInclude(f => f.Casemanager)
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
        [Authorize(Roles = "CaseManager")]
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
                                                                                     .FirstOrDefaultAsync(s => s.TcmServicePlan.TcmClient.Casemanager.Clinic.Id == user_logged.Clinic.Id);
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
                            return RedirectToAction("ServicePlanReviewApproved", "TCMServicePlanReviews", new { approved = (origin - 1) });
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
                            Date_Identified = DateTime.Today.Date,
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
                    TCMServicePlanReviewEntity tcmServicePlanReview = await _context.TCMServicePlanReviews
                                                                                    .Include(n => n.TcmServicePlan)
                                                                                    .ThenInclude(n => n.TCMDomain)
                                                                                    .ThenInclude(n => n.TCMObjetive)
                                                                                    .Include(n => n.TCMServicePlanRevDomain)
                                                                                    .ThenInclude(n => n.TCMServicePlanRevDomainObjectiive)
                                                                                    .FirstOrDefaultAsync(n => n.TcmServicePlan.Id == tcmDomainViewModel.Id_ServicePlan);

                    

                    CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
                    tcmDomainEntity = await _converterHelper.ToTCMDomainEntity(tcmDomainViewModel, true,"Service Plan Review", user_logged.UserName);

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
                                                                                                 .Include(t => t.TcmDomain.TcmServicePlan.TcmClient.Casemanager)
                                                                                                 .Where(g => (g.TcmDomain.TcmServicePlan.TcmClient.Casemanager.LinkedUser == user_logged.UserName))
                                                                                                 .OrderBy(n =>n.TcmDomain.Code)
                                                                                                 .ToListAsync();


                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainServicePlanReview", servicePlanReviewDomain) });
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
                        ID_Objetive = tcmdomain.TCMObjetive.Count() + 1,
                        Start_Date = DateTime.Today.Date,
                        Target_Date = DateTime.Today.Date,
                        End_Date = DateTime.Today.Date,
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
                                DateEndObjective = tcmObjetiveViewModel.End_Date,
                                IdObjective = tcmObjetiveViewModel.ID_Objetive,
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
                                                                                                   .Where(g => (g.TcmDomain.TcmServicePlan.TcmClient.Casemanager.LinkedUser == user_logged.UserName))
                                                                                                   .OrderBy(n => n.TcmDomain.Code)
                                                                                                   .ToListAsync();

                       
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
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateDomain", tcmObjetiveViewModel) });
                }

                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateDomain", tcmObjetiveViewModel) });

            }
            return RedirectToAction("NotAuthorized", "Account");
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
                TCMDomainEntity tcmDomainEntity = await _converterHelper.ToTCMDomainEntity(tcmDomainViewModel, false, tcmDomainViewModel.Origin, user_logged.UserName);
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
                                                                                                 .Where(g => (g.TcmDomain.TcmServicePlan.TcmClient.Casemanager.LinkedUser == user_logged.UserName))
                                                                                                 .ToListAsync();


                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainServicePlanReview", servicePlanReviewDomain) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditDomain", tcmDomainViewModel) });
        }

        [Authorize(Roles = "Manager, CaseManager")]
        public async Task<IActionResult> EditDomainReadOnly(int? id)
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

            tcmDomainViewModel.TcmServicePlan = tcmServicePlan;

            if (ModelState.IsValid)
            {
                TCMDomainEntity tcmDomainEntity = await _converterHelper.ToTCMDomainEntity(tcmDomainViewModel, false, tcmDomainViewModel.Origin, user_logged.UserName);
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
                                                                                                 .Where(g => (g.TcmDomain.TcmServicePlan.TcmClient.Casemanager.LinkedUser == user_logged.UserName))
                                                                                                 .ToListAsync();


                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainServicePlanReview", servicePlanReviewDomain) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditDomain", tcmDomainViewModel) });
        }


        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteDomain(int? id)
        {
            UserEntity user_logged = await _context.Users
                                                .Include(u => u.Clinic)
                                                .ThenInclude(c => c.Setting)
                                                .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans.Include(n => n.TcmClient).Include(n => n.TCMDomain.Where(m => m.Id == id)).FirstOrDefault();
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMServicePlanReviewDomainEntity TCMServicePlanRevDomain = await _context.TCMServicePlanReviewDomains
                                                                                     .Include(n => n.TCMServicePlanRevDomainObjectiive)
                                                                                     .Include(n => n.TcmDomain)
                                                                                     .ThenInclude(n => n.TCMObjetive)
                                                                                     .FirstOrDefaultAsync(n => n.Id == id);

            TCMDomainEntity tcmDomainEntity = await _context.TCMDomains.FirstOrDefaultAsync(s => s.Id == TCMServicePlanRevDomain.TcmDomain.Id);
            if (TCMServicePlanRevDomain == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMServicePlanReviewDomains.Remove(TCMServicePlanRevDomain);
                _context.TCMDomains.Remove(tcmDomainEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1, caseNumber = tcmServicePlan.TcmClient.CaseNumber });
            }

            List<TCMServicePlanReviewDomainEntity> servicePlanReviewDomain = await _context.TCMServicePlanReviewDomains
                                                                                                  .Include(h => h.TCMServicePlanRevDomainObjectiive)
                                                                                                  .Include(f => f.TcmDomain)
                                                                                                  .ThenInclude(f => f.TcmServicePlan)
                                                                                                  .ThenInclude(f => f.TcmClient)
                                                                                                  .ThenInclude(f => f.Client)
                                                                                                  .Include(t => t.TcmDomain.TcmServicePlan.TcmClient.Casemanager)
                                                                                                  .Where(g => (g.TcmDomain.TcmServicePlan.TcmClient.Casemanager.LinkedUser == user_logged.UserName))
                                                                                                  .ToListAsync();

            return RedirectToAction("_ViewDomainServicePlanReview", servicePlanReviewDomain);
            //return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDomainServicePlanReview", servicePlanReviewDomain) });
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

                TCMServicePlanReviewDomainObjectiveEntity tcmServicePlanReviewObjetive = await _context.TCMServicePlanReviewDomainObjectives
                                                                                                       .Include(n => n.tcmServicePlanReviewDomain)
                                                                                                       .ThenInclude(n => n.TcmServicePlanReview)
                                                                                                       .FirstOrDefaultAsync(m => m.Id == tcmObjetiveViewModel.Id);

                tcmObjetiveViewModel.Id = tcmObjetiveViewModel.Idd;
                tcmServicePlanReviewObjetive.Status = StatusUtils.GetStatusByIndex(tcmObjetiveViewModel.IdStatus);
                tcmServicePlanReviewObjetive.ChangesUpdate = tcmObjetiveViewModel.ChangesUpdates;
                tcmServicePlanReviewObjetive.DateEndObjective = tcmObjetiveViewModel.End_Date;


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
                                                                                                    .Where(g => (g.TcmDomain.TcmServicePlan.TcmClient.Casemanager.LinkedUser == user_logged.UserName))
                                                                                                    .OrderBy(n => n.TcmDomain.Code)
                                                                                                    .ToListAsync();


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
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjetive", tcmObjetiveViewModel) });
                }

                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjetive", tcmObjetiveViewModel) });

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteObjetive(int? id, int idObjetive = 0)
        {
            UserEntity user_logged =  _context.Users
                                                .Include(u => u.Clinic)
                                                .ThenInclude(c => c.Setting)
                                                .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMServicePlanReviewDomainObjectiveEntity tcmObjetiveEntity = await _context.TCMServicePlanReviewDomainObjectives
                                                                                        .Include(n => n.tcmServicePlanReviewDomain)
                                                                                        .ThenInclude(n => n.TcmServicePlanReview)
                                                                                        .Include(n => n.tcmServicePlanReviewDomain)
                                                                                        .ThenInclude(n => n.TcmDomain)
                                                                                        .ThenInclude(n => n.TCMObjetive)
                                                                                        .FirstOrDefaultAsync(s => s.Id == id);

           TCMObjetiveEntity tcmObjetive = _context.TCMObjetives
                                                   .FirstOrDefault(n => n.IdObjetive == tcmObjetiveEntity.IdObjective
                                                     && n.TcmDomain.Id == tcmObjetiveEntity.tcmServicePlanReviewDomain.TcmDomain.Id); 

            if (tcmObjetiveEntity == null)
            {
               return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMServicePlanReviewDomainObjectives.Remove(tcmObjetiveEntity);
                _context.TCMObjetives.Remove(tcmObjetive);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1, caseNumber = tcmObjetive.TcmDomain.TcmServicePlan.TcmClient.CaseNumber });
            }

            List<TCMServicePlanReviewDomainEntity> servicePlanReviewDomain = await _context.TCMServicePlanReviewDomains
                                                                                           .Include(h => h.TCMServicePlanRevDomainObjectiive)
                                                                                           .Include(f => f.TcmDomain)
                                                                                           .ThenInclude(f => f.TcmServicePlan)
                                                                                           .ThenInclude(f => f.TcmClient)
                                                                                           .ThenInclude(f => f.Client)
                                                                                           .Include(f => f.TcmServicePlanReview)
                                                                                           .Include(t => t.TcmDomain.TcmServicePlan.TcmClient.Casemanager)
                                                                                           .Where(g => (g.TcmDomain.TcmServicePlan.TcmClient.Casemanager.LinkedUser == user_logged.UserName))
                                                                                           .ToListAsync();
            
            return RedirectToAction("_ViewDomainServicePlanReview", servicePlanReviewDomain);
           
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> EditObjetiveReadOnly(int id = 0)
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
                tcmServicePlanReviewObjetive.DateEndObjective = tcmObjetiveViewModel.End_Date;


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
                                                                                                    .Where(g => (g.TcmDomain.TcmServicePlan.TcmClient.Casemanager.LinkedUser == user_logged.UserName))
                                                                                                    .OrderBy(n => n.TcmDomain.Code)
                                                                                                    .ToListAsync();


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
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjetive", tcmObjetiveViewModel) });
                }

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

            if (user_logged.UserType.ToString() == "CaseManager")
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
            if (user_logged.UserType.ToString() == "Manager")
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
            if (user_logged.UserType.ToString() == "TCMSupervisor")
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

            return View(null);

        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> EditReadOnly(int Id, int IdServicePlan, int origi = 0)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                        .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("TCMSupervisor"))
            {
                TCMServicePlanReviewEntity tcmServicePlanReview = _context.TCMServicePlanReviews
                                                                          .Include(b => b.TCMServicePlanRevDomain)
                                                                          .ThenInclude(f => f.TCMServicePlanRevDomainObjectiive)
                                                                          .Include(f => f.TcmServicePlan)
                                                                          .ThenInclude(f => f.TcmClient)
                                                                          .ThenInclude(f => f.Casemanager)
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
        public async Task<IActionResult> ApproveTCMServicePlanReview(int id, int origi = 0)
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
                        _context.Update(tcmServicePlanReview);
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


    }
}
