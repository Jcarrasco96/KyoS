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
                List<TCMDomainEntity> tcmDomainList = _context.TCMDomains
                                                                 .Include(b => b.TCMObjetive)
                                                                 .Where(f => (f.TcmServicePlan.Id == IdServicePlan))
                                                                 .OrderBy(f => f.Code)
                                                                 .ToList();
                tcmServicePlan.TCMDomain = tcmDomainList;
                TCMServicePlanReviewEntity tcmServicePlanReviewEntity = null;

                tcmServicePlanReviewEntity = new TCMServicePlanReviewEntity
                {
                    DateServicePlanReview = DateTime.Today.Date,
                    DateOpending = DateTime.Today.Date,
                    TcmServicePlan = tcmServicePlan,
                    Recomendation = "",
                    SummaryProgress = "",

                };

                tcmServicePlanReviewEntity.TCMServicePlanRevDomain = new List<TCMServicePlanReviewDomainEntity>();
                List<TCMDomainObjetiveReview> domainObjetiveReview = new List<TCMDomainObjetiveReview>();

                TCMDomainObjetiveReview temp_Domain = new TCMDomainObjetiveReview();
                TCMObjetiveReview temp_Objetive = new TCMObjetiveReview();

                foreach (TCMDomainEntity item in tcmDomainList)
                {
                    temp_Domain.ID = item.Id;
                    temp_Domain.Status = 0;
                    temp_Domain.Code = item.Code;
                    temp_Domain.Name = item.Name;
                    temp_Domain.Updates_Changes = "";
                    if (_context.TCMAdendums.FirstOrDefault(n => n.TcmDomain.Id == item.Id) != null)
                        temp_Domain.Addendum_ServicePlan = "Service Plan";
                    else
                        temp_Domain.Addendum_ServicePlan = "Addendum";
                    temp_Domain.ObjectiveList = new List<TCMObjetiveReview>();

                    foreach (TCMObjetiveEntity item_objective in item.TCMObjetive)
                    {
                        if (item_objective.Status == 0)//Not insert the objectives closed
                        {
                            temp_Objetive.ID = item_objective.Id;
                            temp_Objetive.Name = item_objective.Name;
                            temp_Objetive.Status = item_objective.Status;
                            temp_Domain.ObjectiveList.Add(temp_Objetive);
                            temp_Objetive = new TCMObjetiveReview();
                        }

                    }
                    if (temp_Domain.ObjectiveList.Count > 0)//Only insert the domian with number of open objectives is more than 0
                    {
                        domainObjetiveReview.Add(temp_Domain);
                        temp_Domain = new TCMDomainObjetiveReview();
                    }

                }

                TCMServicePlanReviewViewModel model = _converterHelper.ToTCMServicePlanReviewViewModel(tcmServicePlanReviewEntity);
                model.Domain_List = domainObjetiveReview;

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
            return RedirectToAction("NotAuthorized", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Create(TCMServicePlanReviewViewModel tcmServicePlanreviewViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic) 
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMServicePlanReviewEntity tcmServicePlanReviewEntity = _context.TCMServicePlanReviews
                                                                                      .Include(g => g.TcmServicePlan.TCMDomain)
                                                                                      .FirstOrDefault(g => g.TcmServicePlan.Id == tcmServicePlanreviewViewModel.TcmServicePlan.Id);
               
                if (tcmServicePlanReviewEntity == null)
                {
                    tcmServicePlanReviewEntity = await _converterHelper.ToTCMServicePlanReviewEntity(tcmServicePlanreviewViewModel, true);

                    TCMServicePlanReviewDomainEntity tcmDomainReview = new TCMServicePlanReviewDomainEntity();
                    tcmServicePlanReviewEntity.TCMServicePlanRevDomain = new List<TCMServicePlanReviewDomainEntity> ();

                    foreach (TCMDomainObjetiveReview item in tcmServicePlanreviewViewModel.Domain_List)
                    {
                        tcmDomainReview.ChangesUpdate = item.Updates_Changes;
                        tcmDomainReview.TcmDomain = _context.TCMDomains.FirstOrDefault(n => n.Id == item.ID);
                        if (item.Status == 0)
                        {
                            tcmDomainReview.Status = StatusType.Open;
                        }
                        else
                        {
                            tcmDomainReview.Status = StatusType.Close;
                        }
                        tcmDomainReview.TCMServicePlanRevDomainObjectiive = new List<TCMServicePlanReviewDomainObjectiveEntity>();
                        TCMServicePlanReviewDomainObjectiveEntity tcmDomainObjectives = new TCMServicePlanReviewDomainObjectiveEntity();
                        foreach(TCMObjetiveReview item_Objective in item.ObjectiveList)
                        {
                            tcmDomainObjectives.IdObjective = item_Objective.ID;
                            if (item_Objective.Status == 0)
                            {
                                tcmDomainObjectives.Status = StatusType.Open;
                            }
                            if (item_Objective.Status == 1)
                            {
                                tcmDomainObjectives.Status = StatusType.Close;
                                tcmDomainObjectives.DateEndObjective = DateTime.Now;
                            }
                            tcmDomainReview.TCMServicePlanRevDomainObjectiive.Add(tcmDomainObjectives);
                            tcmDomainObjectives = new TCMServicePlanReviewDomainObjectiveEntity();
                        }
                        tcmServicePlanReviewEntity.TCMServicePlanRevDomain.Add(tcmDomainReview);
                        tcmDomainReview = new TCMServicePlanReviewDomainEntity();

                    }

                    _context.Add(tcmServicePlanReviewEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        //UPDATE STATUS OF THE OBJECTIVES
                        TCMObjetiveEntity tcmObjetive = new TCMObjetiveEntity();
                        List<TCMObjetiveEntity> tcmObjetives = new List<TCMObjetiveEntity>();
                        for (int i = 0; i < tcmServicePlanreviewViewModel.Domain_List.Count(); i++)
                        {
                            for (int j = 0; j < tcmServicePlanreviewViewModel.Domain_List[i].ObjectiveList.Count(); j++)
                            {
                                tcmObjetive = _context.TCMObjetives.FirstOrDefault(n => n.Id == tcmServicePlanreviewViewModel.Domain_List[i].ObjectiveList[j].ID);
                                tcmObjetive.Status = tcmServicePlanreviewViewModel.Domain_List[i].ObjectiveList[j].Status;
                                if (tcmServicePlanreviewViewModel.Domain_List[i].ObjectiveList[j].Status == 1)
                                {
                                    tcmObjetive.EndDate = DateTime.Now;
                                }
                                _context.TCMObjetives.Update(tcmObjetive);
                                tcmObjetive = new TCMObjetiveEntity();
                                await _context.SaveChangesAsync();
                            }
                        }

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
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the TCM Service Plan Review.");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmServicePlanreviewViewModel) });
                }
            }

            
           return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmServicePlanreviewViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Edit(int Id, int IdServicePlan)
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
                List<TCMDomainEntity> tcmDomainList = _context.TCMDomains
                                                                 .Include(b => b.TCMObjetive)
                                                                 .Where(f => (f.TcmServicePlan.Id == IdServicePlan))
                                                                 .OrderBy(f => f.Code)
                                                                 .ToList();
                tcmServicePlan.TCMDomain = tcmDomainList;

                TCMServicePlanReviewEntity tcmServicePlanReviewEntity = _context.TCMServicePlanReviews
                                                                                .Include(n => n.TCMServicePlanRevDomain)

                                                                                .FirstOrDefault(n => n.Id == Id);
                tcmServicePlanReviewEntity.TcmServicePlan = tcmServicePlan;

                List<TCMDomainObjetiveReview> domainObjetiveReview = new List<TCMDomainObjetiveReview>();

                TCMDomainObjetiveReview temp_Domain = new TCMDomainObjetiveReview();
                TCMObjetiveReview temp_Objetive = new TCMObjetiveReview();
                TCMDomainEntity domain_Temp = new TCMDomainEntity();
                TCMObjetiveEntity objective_Temp = new TCMObjetiveEntity();
                TCMServicePlanReviewDomainEntity item_Temp = new TCMServicePlanReviewDomainEntity();
                foreach (TCMServicePlanReviewDomainEntity item in tcmServicePlanReviewEntity.TCMServicePlanRevDomain)
                {
                    temp_Domain.ID = item.Id;
                    temp_Domain.Status = (item.Status == StatusType.Open) ? 0 : 1;

                    domain_Temp = _context.TCMDomains.FirstOrDefault(f => f.Id == item.TcmDomain.Id);

                    temp_Domain.Code = domain_Temp.Code;
                    temp_Domain.Name = domain_Temp.Name;
                    temp_Domain.Updates_Changes = item.ChangesUpdate;

                    if (_context.TCMAdendums.FirstOrDefault(n => n.TcmDomain.Id == item.TcmDomain.Id) != null)
                        temp_Domain.Addendum_ServicePlan = "Service Plan";
                    else
                        temp_Domain.Addendum_ServicePlan = "Addendum";
                    temp_Domain.ObjectiveList = new List<TCMObjetiveReview>();

                    item_Temp = _context.TCMServicePlanReviewDomains.Include(f => f.TCMServicePlanRevDomainObjectiive)
                                                               .FirstOrDefault(f => f.Id == item.Id);
                    foreach (TCMServicePlanReviewDomainObjectiveEntity item_objective in item_Temp.TCMServicePlanRevDomainObjectiive)
                    {

                        objective_Temp = _context.TCMObjetives.FirstOrDefault(f => f.Id == item_objective.IdObjective);

                        temp_Objetive.ID = item_objective.Id;
                        temp_Objetive.Name = objective_Temp.Name;
                        temp_Objetive.Status = objective_Temp.Status;
                        temp_Domain.ObjectiveList.Add(temp_Objetive);
                        temp_Objetive = new TCMObjetiveReview();


                    }
                    if (temp_Domain.ObjectiveList.Count > 0)
                    {
                        domainObjetiveReview.Add(temp_Domain);
                        temp_Domain = new TCMDomainObjetiveReview();
                    }

                }

                TCMServicePlanReviewViewModel model = _converterHelper.ToTCMServicePlanReviewViewModel(tcmServicePlanReviewEntity);
                model.Domain_List = domainObjetiveReview;
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
            return RedirectToAction("NotAuthorized", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Edit(TCMServicePlanReviewViewModel tcmServicePlanreviewViewModel)
        {
            UserEntity user_logged = _context.Users
                                            .Include(u => u.Clinic)
                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMServicePlanReviewEntity tcmServicePlanReviewEntity = _context.TCMServicePlanReviews
                                                                                .Include(n => n.TcmServicePlan)
                                                                                
                                                                                .FirstOrDefault(n => n.Id == tcmServicePlanreviewViewModel.Id);
                tcmServicePlanReviewEntity.Recomendation = tcmServicePlanreviewViewModel.Recomendation;
                tcmServicePlanReviewEntity.SummaryProgress = tcmServicePlanreviewViewModel.SummaryProgress;
               
                TCMServicePlanReviewDomainEntity tcmDomainReview = new TCMServicePlanReviewDomainEntity();
                    
                foreach (TCMDomainObjetiveReview item in tcmServicePlanreviewViewModel.Domain_List)
                {
                    tcmDomainReview = _context.TCMServicePlanReviewDomains.FirstOrDefault(n => n.Id == item.ID);
                    
                    if (tcmDomainReview != null)
                    {
                        tcmDomainReview.ChangesUpdate = item.Updates_Changes;
                        tcmDomainReview.Id = item.ID;
                        _context.TCMServicePlanReviewDomains.Update(tcmDomainReview);

                        if (item.Status == 0)
                        {
                            tcmDomainReview.Status = StatusType.Open;
                        }
                        else
                        {
                            tcmDomainReview.Status = StatusType.Close;
                        }
                    }
                    else
                    {
                    
                    }
                        
                    TCMServicePlanReviewDomainObjectiveEntity tcmDomainObjectives = new TCMServicePlanReviewDomainObjectiveEntity();
                    foreach (TCMObjetiveReview item_Objective in item.ObjectiveList)
                    {
                        tcmDomainObjectives = _context.TCMServicePlanReviewDomainObjectives.FirstOrDefault(n => n.Id == item_Objective.ID);
                        if (tcmDomainObjectives != null)
                        {
                           
                            if (item_Objective.Status == 0)
                            {
                                tcmDomainObjectives.Status = StatusType.Open;
                            }
                            if (item_Objective.Status == 1)
                            {
                                tcmDomainObjectives.Status = StatusType.Close;
                                tcmDomainObjectives.DateEndObjective = DateTime.Now;
                            }
                           
                            _context.TCMServicePlanReviewDomainObjectives.Update(tcmDomainObjectives);
                            tcmDomainObjectives = new TCMServicePlanReviewDomainObjectiveEntity();
                        }
                        else
                        { 
                        
                        }
                           
                    }
                      
                }

                    _context.Update(tcmServicePlanReviewEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        //UPDATE STATUS OF THE OBJECTIVES
                        TCMObjetiveEntity tcmObjetive = new TCMObjetiveEntity();
                        TCMServicePlanReviewDomainObjectiveEntity tcmObjetive_Review = new TCMServicePlanReviewDomainObjectiveEntity();
                        List<TCMObjetiveEntity> tcmObjetives = new List<TCMObjetiveEntity>();
                        for (int i = 0; i < tcmServicePlanreviewViewModel.Domain_List.Count(); i++)
                        {
                            for (int j = 0; j < tcmServicePlanreviewViewModel.Domain_List[i].ObjectiveList.Count(); j++)
                            {
                                tcmObjetive_Review = _context.TCMServicePlanReviewDomainObjectives.FirstOrDefault(n => n.Id == tcmServicePlanreviewViewModel.Domain_List[i].ObjectiveList[j].ID);
                                tcmObjetive = _context.TCMObjetives.FirstOrDefault(n => n.Id == tcmObjetive_Review.IdObjective);
                                tcmObjetive.Status = tcmServicePlanreviewViewModel.Domain_List[i].ObjectiveList[j].Status;
                                if (tcmServicePlanreviewViewModel.Domain_List[i].ObjectiveList[j].Status == 1)
                                {
                                    tcmObjetive.EndDate = DateTime.Now;
                                }
                                _context.TCMObjetives.Update(tcmObjetive);
                                tcmObjetive = new TCMObjetiveEntity();
                                await _context.SaveChangesAsync();
                            }
                        }

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
        public async Task<IActionResult> FinishEditing(int id)
        {
            TCMServicePlanReviewEntity tcmServicePlanReview = _context.TCMServicePlanReviews.FirstOrDefault(u => u.Id == id);

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

                            return RedirectToAction("Index", "TCMServicePlanReviews");
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
    }
}
