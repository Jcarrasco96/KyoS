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
            List<SelectListItem> list_Status = new List<SelectListItem>
                                { new SelectListItem { Text = StatusType.Open.ToString(), Value = "1"},
                                  new SelectListItem { Text = StatusType.Close.ToString(), Value = "2"}};
            
            TCMServicePlanReviewEntity tcmServicePlanReviewEntity = null;
            TCMServicePlanReviewDomainEntity tcmServicePlanreviewDomain = null;
            tcmServicePlanReviewEntity = new TCMServicePlanReviewEntity
            {
                DateServicePlanReview = DateTime.Today.Date,
                DateOpending = DateTime.Today.Date,
                TcmServicePlan = tcmServicePlan,
                Recomendation = "",
                SummaryProgress = "",
               
            };
            tcmServicePlanReviewEntity.TCMServicePlanRevDomain = new List<TCMServicePlanReviewDomainEntity>();

            List <TCMDomainObjetiveReview> domainObjetiveReview = new List<TCMDomainObjetiveReview>();
            TCMDomainObjetiveReview temp_Domain = new TCMDomainObjetiveReview();
            TCMObjetiveReview temp_Objetive = new TCMObjetiveReview();
            for (int i = 0; i < tcmDomainList.Count(); i++)
            {
                temp_Domain.ID = tcmDomainList[i].Id;
                temp_Domain.Status = 0;
                temp_Domain.Code = tcmDomainList[i].Code;
                temp_Domain.Name = tcmDomainList[i].Name;
                temp_Domain.Recomendation = "";
                temp_Domain.ObjectiveList = new TCMObjetiveReview [tcmDomainList[i].TCMObjetive.Count()];
                for (int j = 0; j < tcmDomainList[i].TCMObjetive.Count(); j++)
                {
                    temp_Objetive.ID = tcmDomainList[i].TCMObjetive[j].Id ;
                    temp_Objetive.Name = tcmDomainList[i].TCMObjetive[j].Name;
                    temp_Objetive.Status = tcmDomainList[i].TCMObjetive[j].Status;

                    temp_Domain.ObjectiveList[j] = temp_Objetive;
                    temp_Objetive = new TCMObjetiveReview();
                }
                domainObjetiveReview.Add(temp_Domain);
                temp_Domain = new TCMDomainObjetiveReview();
                /*tcmServicePlanreviewDomain = new TCMServicePlanReviewDomainEntity
                {
                    Id = 0,
                    ChangesUpdate = "",
                    Status = StatusType.Open,

                    TcmDomain = tcmDomainList[i],
                };

                tcmServicePlanReviewEntity.TCMServicePlanRevDomain.Add(tcmServicePlanreviewDomain);*/
            }

            if (User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);
                TCMServicePlanReviewViewModel model = _converterHelper.ToTCMServicePlanReviewViewModel(tcmServicePlanReviewEntity);
                model.Domain_List = domainObjetiveReview;
                //model.Domain_ListReview = list_Domain;
                // _context.Add(tcmServicePlanReviewEntity);

                try
                {
                   // _context.SaveChanges();
                    //model.Id = _context.TCMServicePlanReviews.FirstOrDefault(g => g.TcmServicePlan.Id == tcmServicePlanReviewEntity.TcmServicePlan.Id).Id;
                    //return RedirectToAction("Index", "TCMServicePlanReviews");
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
                    _context.Add(tcmServicePlanReviewEntity);
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
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmServicePlanreviewViewModel) });
                }
            }

            
           return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmServicePlanreviewViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> EditDomainReview(int IdObjetive)
        {

            TCMObjetiveEntity tcmObjetive = _context.TCMObjetives
                                                    .Include(f => f.TcmDomain)
                                                    .FirstOrDefault(f => f.Id == IdObjetive); 
            
            TCMServicePlanReviewDomainEntity domainReview = _context.TCMServicePlanReviewDomains
                                                                    .Include(g => g.TcmDomain)
                                                                    .ThenInclude(g => g.TCMObjetive)
                                                                    .FirstOrDefault(g => g.TcmDomain.Id == tcmObjetive.TcmDomain.Id);

            /*List<TCMObjetiveEntity> tcmObjetives = _context.TCMObjetives
                                                           .Where(g => g.TcmDomain.Id == domainReview.TcmDomain.Id)
                                                           .OrderBy(g => g.IdObjetive)
                                                           .ToList();
            domainReview.TcmDomain.TCMObjetive = tcmObjetives;
            TCMServicePlanReviewDomainViewModel model = _converterHelper.ToTCMServicePlanReviewDomainViewModel(domainReview);

            model.IdStatusObjetive = new List<SelectListItem>();

            SelectListItem a = null;
            for (int i = 0; i < tcmObjetives.Count; i++)
            {

                model.IdStatusObjetive.Insert(i, new SelectListItem
                {
                    Text = $"{tcmObjetives[i].Status}",
                    Value = $"{tcmObjetives[i].Id}"

                });

            }*/
            TCMServicePlanReviewDomainViewModel model = _converterHelper.ToTCMServicePlanReviewDomainViewModel(domainReview);
            model.IdStatusObjetive = IdObjetive;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> EditDomainReview(TCMServicePlanReviewDomainViewModel DomainReview)
        {
            UserEntity user_logged = _context.Users
                                               .Include(u => u.Clinic)
                                               .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMServicePlanReviewDomainEntity tcmServicePlanReviewDomainEntity =  _context.TCMServicePlanReviewDomains
                                                                                             .Include(g => g.TcmDomain)
                                                                                             .FirstOrDefault(g => g.Id == DomainReview.Id);
                if (tcmServicePlanReviewDomainEntity != null)
                {
                    DomainReview.TcmDomain = tcmServicePlanReviewDomainEntity.TcmDomain;
                    tcmServicePlanReviewDomainEntity = await _converterHelper.ToTCMServicePlanReviewDomainEntity(DomainReview, false);
                    
                    _context.Update(tcmServicePlanReviewDomainEntity);
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
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", DomainReview) });
                }
            }
            return View(DomainReview);
        }
    }
}
