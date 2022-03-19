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


            return View(await _context.TCMServicePlans
                                      .Include(f => f.TcmClient)
                                      .ThenInclude(f => f.Client)
                                      .Include(g => g.TcmClient.Casemanager)
                                      .Where(s => (s.TcmClient.Casemanager.Clinic.Id == user_logged.Clinic.Id
                                          && s.TcmClient.Status == StatusType.Open 
                                          && s.TcmClient.DataClose > DateTime.Now
                                          && s.Approved ==2 ))
                                      .OrderBy(f => f.TcmClient.CaseNumber)
                                      .ToListAsync());

        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Create(int id)
        {
            TCMServicePlanEntity tcmServicePlan = _context.TCMServicePlans
                                                             .Include(b => b.TCMDomain)
                                                             .Include(f => f.TcmClient)
                                                             .Include(f => f.TcmClient.Casemanager)
                                                             .Include(f => f.TcmClient.Client)
                                                             .FirstOrDefault(f => (f.Id == id && f.Approved == 2));
            List<TCMDomainEntity> tcmDomainList = _context.TCMDomains
                                                             .Include(b => b.TCMObjetive)
                                                             .Where(f => (f.TcmServicePlan.Id == id))
                                                             .OrderBy(f => f.Code)
                                                             .ToList();
            tcmServicePlan.TCMDomain = tcmDomainList;

            TCMServicePlanReviewViewModel model;
            if (User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {
                    List<SelectListItem> list_Status = new List<SelectListItem>
                                { new SelectListItem { Text = StatusType.Open.ToString(), Value = "1"},
                                  new SelectListItem { Text = StatusType.Close.ToString(), Value = "2"}};
                    model = new TCMServicePlanReviewViewModel
                    {
                        IdStatus = 1,
                        DateServicePlanReview = DateTime.Today.Date,
                        DateOpending = DateTime.Today.Date,
                        TcmServicePlan = tcmServicePlan,
                        TCMServicePlanRevDomain = null,
                        Recomendation = "",
                        SummaryProgress = "",
                        StatusList = list_Status,

                    };
                    return View(model);
                }
                return RedirectToAction("NotAuthorized", "Account");

            }
            return RedirectToAction("NotAuthorized", "Account");
        }
    }
}
