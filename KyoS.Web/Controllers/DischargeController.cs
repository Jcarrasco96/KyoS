using Microsoft.AspNetCore.Mvc;
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
        [Authorize(Roles = "Mannager")]
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

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            else
            {
                if (User.IsInRole("Mannager"))
                    return View(await _context.Discharge
                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clients_Diagnostics)
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

                if (User.IsInRole("Facilitator"))
                {



                    return View(await _context.Discharge
                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clients_Diagnostics)
                                              //.Where(f => )
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Mannager")]
        public IActionResult Create(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            DischargeViewModel model;

            if (User.IsInRole("Mannager"))
            {


                if (user_logged.Clinic != null)
                {

                    model = new DischargeViewModel
                    {
                        IdClient = id,
                        Client = _context.Clients.Include(n => n.MedicationList).FirstOrDefault(n => n.Id == id),
                        AdmissionedFor = user_logged.FullName,
                        AgencyDischargeClient = true,
                        BriefHistory = "",
                        ClientDeceased = true,
                        ClientDischargeAgainst = true,
                        ClientMoved = true,
                        ClientReferred = true,
                        Client_FK = id,
                        ConditionalDischarge = "",
                        CourseTreatment = "",
                        DateDischarge =DateTime.Now,
                        DateReport = DateTime.Now,
                        FollowDischarge = "",
                        Id = 0,
                        PhysicallyUnstable = true,
                        Planned = true,
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
                        TreatmentPlanObjCumpl = true,
                        Others = false,
                        Hospitalization = false,
                        DateSignatureEmployee = DateTime.Now,
                        DateSignaturePerson = DateTime.Now,

                    };
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    return View(model);
                }
            }

            model = new DischargeViewModel
            {
                IdClient = id,
                Client = _context.Clients.Include(n => n.MedicationList).FirstOrDefault(n => n.Id == id),
                AdmissionedFor = user_logged.FullName,
                AgencyDischargeClient = true,
                BriefHistory = "",
                ClientDeceased = true,
                ClientDischargeAgainst = true,
                ClientMoved = true,
                ClientReferred = true,
                Client_FK = id,
                ConditionalDischarge = "",
                CourseTreatment = "",
                DateDischarge = DateTime.Now,
                DateReport = DateTime.Now,
                FollowDischarge = "",
                Id = 0,
                PhysicallyUnstable = true,
                Planned = true,
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
                TreatmentPlanObjCumpl = true,
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Mannager")]
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
                

            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", DischargeViewModel) });
        }

        [Authorize(Roles = "Mannager")]
        public IActionResult Edit(int id = 0)
        {
            DischargeViewModel model;

            if (User.IsInRole("Mannager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    DischargeEntity Discharge = _context.Discharge
                                                                 .Include(m => m.Client)
                                                                 .ThenInclude(m => m.MedicationList)
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
        [Authorize(Roles = "Mannager")]
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

        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> DischargeCandidates(int idError = 0)
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null /*|| !user_logged.Clinic.Setting.TCMClinic*/)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            List<ClientEntity> ClientList = await _context.Clients
                                                          .Include(n => n.Discharge)
                                                          .Where(n => n.Discharge == null && n.Status == StatusType.Close)
                                                          .ToListAsync();

            return View(ClientList);

        }

        [Authorize(Roles = "Mannager")]
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
    }
}
