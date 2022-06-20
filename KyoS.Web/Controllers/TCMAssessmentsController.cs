using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    public class TCMAssessmentsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IReportHelper _reportHelper;
        private readonly IRenderHelper _renderHelper;

        public TCMAssessmentsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IReportHelper reportHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
            _renderHelper = renderHelper;
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> Index(int idError = 0, int idTCMClient = 0, string origin = "")
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            else
            {
                CaseMannagerEntity caseManager = _context.CaseManagers.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
                    return View(await _context.TCMClient

                                              .Include(f => f.TCMAssessment)
                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clinic)
                                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

                if (User.IsInRole("CaseManager"))
                {

                    ViewData["origin"] = origin.ToString();

                    return View(await _context.TCMClient
                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clinic)
                                              .Include(f => f.TCMAssessment)

                                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id
                                                && (n.Casemanager.Id == caseManager.Id)
                                                 && (n.Id == idTCMClient))
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Create(int id = 0)
        {

            TCMAssessmentViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    model = new TCMAssessmentViewModel
                    {
                        Approved = 0,
                        TcmClient = _context.TCMClient
                                            .Include(n => n.Client)
                                            .FirstOrDefault(n => n.Id == id),
                        AreChild = false,
                        AreChildAddress = "",
                        AreChildCity = "",
                        AreChildName = "",
                        AreChildPhone = "",
                        Caregiver = false,
                        ChildFather = "",
                        ChildMother = "",
                        ClientInput = false,
                        DateAssessment = DateTime.Now,
                        Divorced = false,
                        Family = false,
                        Id = 0,
                        Married = false,
                        MayWe = false,
                        MayWeNA = false,
                        NeverMarried = false,
                        Other = false,
                        OtherExplain = "",
                        PresentingProblems = "",
                        Referring = false,
                        Review = false,
                        School = false,
                        Separated = false,
                        TcmClient_FK = id,
                        Treating = false

                    };

                    return View(model);
                }
            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            model = new TCMAssessmentViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Create(TCMAssessmentViewModel tcmAssessmentViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentEntity tcmAssessmentEntity = _context.TCMAssessment.Find(tcmAssessmentViewModel.Id);
                if (tcmAssessmentEntity == null)
                {
                    tcmAssessmentEntity = await _converterHelper.ToTCMAssessmentEntity(tcmAssessmentViewModel, true, user_logged.UserName);
                    _context.TCMAssessment.Add(tcmAssessmentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        TCMAssessmentEntity tcmDischarge = await _context.TCMAssessment
                                                                         .Include(m => m.TcmClient)
                                                                         .ThenInclude(m => m.Client)
                                                                         .FirstOrDefaultAsync(m => m.TcmClient.Casemanager.LinkedUser == user_logged.UserName
                                                                            && m.TcmClient.Id == tcmAssessmentViewModel.TcmClient_FK);

                        return RedirectToAction("Index", "TCMAssessments", new { idTCMClient = tcmDischarge.TcmClient.Id });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the TCM service.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmAssessmentViewModel) });
                }
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmAssessmentViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Edit(int id = 0)
        {
            TCMAssessmentViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    TCMAssessmentEntity TcmAssessment = _context.TCMAssessment
                                                                .Include(b => b.TcmClient)
                                                                .ThenInclude(b => b.Client)
                                                                .FirstOrDefault(m => m.Id == id);
                    if (TcmAssessment == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToTCMAssessmentViewModel(TcmAssessment);

                        return View(model);
                    }

                }
            }

            model = new TCMAssessmentViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Edit(TCMAssessmentViewModel tcmAssessmentViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentEntity tcmAssessmentEntity = await _converterHelper.ToTCMAssessmentEntity(tcmAssessmentViewModel, false, user_logged.UserName);
                _context.TCMAssessment.Update(tcmAssessmentEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "TCMAssessments", new { idTCMClient = tcmAssessmentEntity.TcmClient.Id });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", tcmAssessmentViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> FinishEditing(int id)
        {
            TCMAssessmentEntity tcmAssessment = _context.TCMAssessment.FirstOrDefault(u => u.Id == id);

            if (tcmAssessment != null)
            {
                if (User.IsInRole("CaseManager"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        tcmAssessment.Approved = 1;
                        _context.Update(tcmAssessment);
                        try
                        {
                            await _context.SaveChangesAsync();

                            return RedirectToAction("Index", "TCMAssessments");
                        }
                        catch (System.Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("Index", "TCMAssessments");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> Approved(int id)
        {
            TCMAssessmentEntity tcmAssessment = _context.TCMAssessment.FirstOrDefault(u => u.Id == id);

            if (tcmAssessment != null)
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        tcmAssessment.Approved = 2;
                        _context.Update(tcmAssessment);
                        try
                        {
                            await _context.SaveChangesAsync();

                            return RedirectToAction("Index", "TCMAssessments");
                        }
                        catch (System.Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("Index", "TCMAssessments");
        }

    }
}
