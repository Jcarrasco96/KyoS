﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;

namespace KyoS.Web.Controllers
{
    public class DischargeController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IReportHelper _reportHelper;
        private readonly DataContext _context;

        public DischargeController(IUserHelper userHelper, IConverterHelper converterHelper, ICombosHelper combosHelper, IRenderHelper renderHelper, DataContext context, IReportHelper reportHelper)
        {
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _context = context;
            _renderHelper = renderHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
        }
        
        [Authorize(Roles = "Mannager, Supervisor, Facilitator")]
        public async Task<IActionResult> Index(int idError = 0)
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
                if (User.IsInRole("Mannager"))
                    return View(await _context.Clients

                                              .Include(f => f.Discharge)
                                              .Include(f => f.Clients_Diagnostics)

                                              .Where(n => (n.Clinic.Id == user_logged.Clinic.Id && n.Status == StatusType.Close))
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());

                if (User.IsInRole("Supervisor"))
                {
                    return View(await _context.Clients

                                              .Include(f => f.Discharge)
                                              .Include(f => f.Clients_Diagnostics)

                                              .Where(n => (n.Clinic.Id == user_logged.Clinic.Id && n.Status == StatusType.Close))
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());
                }
                if (User.IsInRole("Facilitator"))
                {
                    return View(await _context.Clients

                                              .Include(f => f.Discharge)
                                              .Include(f => f.Clients_Diagnostics)

                                              .Where(n => (n.Clinic.Id == user_logged.Clinic.Id && n.Status == StatusType.Close))
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor")]
        public IActionResult Create(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            DischargeViewModel model;

            if (User.IsInRole("Supervisor"))
            {
                if (user_logged.Clinic != null)
                {
                    model = new DischargeViewModel
                    {
                        IdClient = id,
                        Client = _context.Clients

                                         .Include(n => n.MedicationList)

                                         .Include(c => c.Clients_Diagnostics)
                                         .ThenInclude(cd => cd.Diagnostic)

                                         .FirstOrDefault(n => n.Id == id),
                        AdmissionedFor = user_logged.FullName,
                        AgencyDischargeClient = false,
                        BriefHistory = "",
                        ClientDeceased = false,
                        ClientDischargeAgainst = false,
                        ClientMoved = false,
                        ClientReferred = false,
                        Client_FK = id,
                        ConditionalDischarge = "",
                        CourseTreatment = "",
                        DateDischarge =DateTime.Now,
                        DateReport = DateTime.Now,
                        FollowDischarge = "",
                        Id = 0,
                        PhysicallyUnstable = false,
                        Planned = false,
                        ReasonDischarge = "",
                        ReferralAgency1 = "",
                        ReferralAgency2 = "",
                        ReferralContactPersonal1 = "",
                        ReferralContactPersonal2 = "",
                        ReferralFor1 = "",
                        ReferralFor2 = "",
                        ReferralHoursOperation1 = "",
                        ReferralHoursOperation2 = "",
                        ReferralPhone1 = "",
                        ReferralPhone2 = "",
                        TreatmentPlanObjCumpl = false,
                        Others = false,
                        Others_Explain = "",
                        Hospitalization = false,
                        DateSignatureEmployee = DateTime.Now,
                        DateSignaturePerson = DateTime.Now,
                        DateSignatureSupervisor = DateTime.Now

                    };
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    return View(model);
                }
            }

            model = new DischargeViewModel
            {
                IdClient = id,
                Client = _context.Clients
                                 .Include(n => n.MedicationList)
                                         
                                 .Include(c => c.Clients_Diagnostics)
                                 .ThenInclude(cd => cd.Diagnostic)

                                 .FirstOrDefault(n => n.Id == id),
                AdmissionedFor = user_logged.FullName,
                AgencyDischargeClient = false,
                BriefHistory = "",
                ClientDeceased = false,
                ClientDischargeAgainst = false,
                ClientMoved = false,
                ClientReferred = false,
                Client_FK = id,
                ConditionalDischarge = "",
                CourseTreatment = "",
                DateDischarge = DateTime.Now,
                DateReport = DateTime.Now,
                FollowDischarge = "",
                Id = 0,
                PhysicallyUnstable = false,
                Planned = false,
                ReasonDischarge = "",
                ReferralAgency1 = "",
                ReferralAgency2 = "",
                ReferralContactPersonal1 = "",
                ReferralContactPersonal2 = "",
                ReferralFor1 = "",
                ReferralFor2 = "",
                ReferralHoursOperation1 = "",
                ReferralHoursOperation2 = "",
                ReferralPhone1 = "",
                ReferralPhone2 = "",
                TreatmentPlanObjCumpl = false,
                Others = false,
                Others_Explain = "",
                Hospitalization = false,
                DateSignatureEmployee = DateTime.Now,
                DateSignaturePerson = DateTime.Now,
                DateSignatureSupervisor = DateTime.Now
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Create(DischargeViewModel DischargeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                DischargeEntity DischargeEntity = _context.Discharge.Find(DischargeViewModel.Id);
                if (DischargeEntity == null)
                {
                    DischargeEntity = await _converterHelper.ToDischargeEntity(DischargeViewModel, true);
                    _context.Discharge.Add(DischargeEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        return RedirectToAction("Index", "Discharge");
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Discharge.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", DischargeViewModel) });
                }
            }
            DischargeViewModel model;
            model = new DischargeViewModel
            {
                IdClient = DischargeViewModel.IdClient,
                Client = _context.Clients.Find(DischargeViewModel.IdClient),
                AdmissionedFor = user_logged.FullName,
                AgencyDischargeClient = DischargeViewModel.AgencyDischargeClient,
                BriefHistory = DischargeViewModel.BriefHistory,
                ClientDeceased = DischargeViewModel.ClientDeceased,
                ClientDischargeAgainst = DischargeViewModel.ClientDischargeAgainst,
                ClientMoved = DischargeViewModel.ClientMoved,
                ClientReferred = DischargeViewModel.ClientReferred,
                Client_FK = DischargeViewModel.Client_FK,
                ConditionalDischarge =DischargeViewModel.ConditionalDischarge,
                CourseTreatment =DischargeViewModel.CourseTreatment,
                DateDischarge = DischargeViewModel.DateDischarge,
                DateReport =DischargeViewModel.DateReport ,
                FollowDischarge = DischargeViewModel.FollowDischarge,
                Id = DischargeViewModel.Id,
                PhysicallyUnstable = DischargeViewModel.PhysicallyUnstable,
                Planned =DischargeViewModel.Planned,
                ReasonDischarge = DischargeViewModel.ReasonDischarge,
                ReferralAgency1 = DischargeViewModel.ReferralAgency1,
                ReferralAgency2 = DischargeViewModel.ReferralAgency2,
                ReferralContactPersonal1 = DischargeViewModel.ReferralContactPersonal1,
                ReferralContactPersonal2 = DischargeViewModel.ReferralContactPersonal2,
                ReferralFor1 = DischargeViewModel.ReferralFor1,
                ReferralFor2 = DischargeViewModel.ReferralFor2,
                ReferralHoursOperation1 = DischargeViewModel.ReferralHoursOperation1,
                ReferralHoursOperation2 = DischargeViewModel.ReferralHoursOperation2,
                ReferralPhone1 = DischargeViewModel.ReferralPhone1,
                ReferralPhone2 = DischargeViewModel.ReferralPhone2,
                TreatmentPlanObjCumpl = DischargeViewModel.TreatmentPlanObjCumpl,
                Others = DischargeViewModel.Others,
                Others_Explain = DischargeViewModel.Others_Explain,
                DateSignatureEmployee = DischargeViewModel.DateSignatureEmployee,
                DateSignaturePerson = DischargeViewModel.DateSignaturePerson,
                DateSignatureSupervisor = DischargeViewModel.DateSignatureSupervisor,
                Hospitalization = DischargeViewModel.Hospitalization
            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", DischargeViewModel) });
        }

        [Authorize(Roles = "Supervisor")]
        public IActionResult Edit(int id = 0)
        {
            DischargeViewModel model;

            if (User.IsInRole("Supervisor"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    DischargeEntity Discharge = _context.Discharge
                                                        .Include(m => m.Client)
                                                        .ThenInclude(m => m.MedicationList)

                                                        .Include(d => d.Client)
                                                        .ThenInclude(c => c.Clients_Diagnostics)                                                        
                                                        .ThenInclude(cd => cd.Diagnostic)

                                                        .FirstOrDefault(m => m.Id == id);
                    if (Discharge == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToDischargeViewModel(Discharge);

                        return View(model);
                    }

                }
            }

            model = new DischargeViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Edit(DischargeViewModel dischargeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                DischargeEntity dischargeEntity = await _converterHelper.ToDischargeEntity(dischargeViewModel, false);
                _context.Discharge.Update(dischargeEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Discharge");
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", dischargeViewModel) });
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DischargeEntity dischargeEntity = await _context.Discharge.FirstOrDefaultAsync(s => s.Id == id);
            if (dischargeEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Discharge.Remove(dischargeEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Mannager, Supervisor, Facilitator")]
        public IActionResult PrintDischarge(int id)
        {
            DischargeEntity entity = _context.Discharge

                                             .Include(d => d.Client)
                                             .ThenInclude(c => c.Clinic)

                                             .Include(d => d.Client)
                                             .ThenInclude(c => c.EmergencyContact)

                                             .Include(d => d.Client)
                                             .ThenInclude(c => c.LegalGuardian)

                                             .Include(d => d.Client)
                                             .ThenInclude(c => c.Clients_Diagnostics)
                                             .ThenInclude(cd => cd.Diagnostic)

                                             .Include(d => d.Client)
                                             .ThenInclude(c => c.MedicationList)

                                             .FirstOrDefault(f => (f.Id == id));
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            //if (entity.Client.Clinic.Name == "DAVILA")
            //{
            //    Stream stream = _reportHelper.FloridaSocialHSIntakeReport(entity);
            //    return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            //}

            if (entity.Client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
            {
                Stream stream = _reportHelper.FloridaSocialHSDischargeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            return null;
        }
    }
}
