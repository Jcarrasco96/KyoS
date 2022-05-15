using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using KyoS.Common.Enums;

namespace KyoS.Web.Controllers
{
    public class FarsFormsController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IReportHelper _reportHelper;
        private readonly DataContext _context;

        public FarsFormsController(IUserHelper userHelper, IConverterHelper converterHelper, ICombosHelper combosHelper, IRenderHelper renderHelper, DataContext context, IReportHelper reportHelper)
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
                if (User.IsInRole("Mannager")|| User.IsInRole("Supervisor"))
                    return View(await _context.Clients

                                              .Include(f => f.FarsFormList)

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());

                if (User.IsInRole("Facilitator"))
                {
                    return View(await _context.Clients

                                              .Include(f => f.FarsFormList)

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public IActionResult Create(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            FarsFormViewModel model;

            if (user_logged.Clinic != null)
            {
                model = new FarsFormViewModel
                {
                   IdClient = id,
                   Client = _context.Clients.Include(n => n.FarsFormList).FirstOrDefault(n => n.Id == id),
                   Id = 0,
                   AbilityScale = 0,
                   AdmissionedFor = user_logged.FullName,
                   ActivitiesScale = 0,
                   AnxietyScale = 0,
                   CognitiveScale = 0,
                   ContractorID = "",
                   Country = "",
                   DangerToOtherScale = 0,
                   DangerToSelfScale = 0,
                   DcfEvaluation = "",
                   DepressionScale = 0,
                   EvaluationDate = DateTime.Now,
                   FamilyEnvironmentScale = 0,
                   FamilyRelationShipsScale = 0,
                   HyperAffectScale = 0,
                   InterpersonalScale = 0,
                   MCOID = "",
                   MedicaidProviderID = "",
                   MedicaidRecipientID = "",
                   MedicalScale = 0,
                   M_GafScore = "",
                   ProviderId = "",
                   ProviderLocal = "",
                   RaterEducation = "",
                   RaterFMHI = "",
                   SecurityScale = 0,
                   SignatureDate = DateTime.Now,
                   SocialScale = 0,
                   SubstanceAbusoHistory = 0,
                   SubstanceScale = 0,
                   ThoughtProcessScale = 0,
                   TraumaticsScale = 0,
                   WorkScale = 0,
                     
                   ContID1 = "",
                   ContID2 = "",
                   ContID3 = "",
                   ProgramEvaluation = "",
                   Status = FarsStatus.Edition,
                   IdSupervisor = 0,
                   Supervisor = new SupervisorEntity()
                };

                if (User.IsInRole("Supervisor"))
                {
                    model.IdSupervisor = _context.Supervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName).Id;
                }

                    if (model.Client.FarsFormList == null)
                    model.Client.FarsFormList = new List<FarsFormEntity>();
                return View(model);
                }
            
            return RedirectToAction("NotAuthorized", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> Create(FarsFormViewModel FarsFormViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                FarsFormEntity farsFormEntity = _context.FarsForm.Find(FarsFormViewModel.Id);
                if (farsFormEntity == null)
                {
                    farsFormEntity = await _converterHelper.ToFarsFormEntity(FarsFormViewModel, true, user_logged.UserName);
                    _context.FarsForm.Add(farsFormEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        return RedirectToAction(nameof(Index));
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Fars.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", FarsFormViewModel) });
                }
            }
            FarsFormViewModel model;
            model = new FarsFormViewModel
            {
                IdClient = FarsFormViewModel.IdClient,
                Client = _context.Clients.Find(FarsFormViewModel.IdClient),
                Id = FarsFormViewModel.Id,
                AbilityScale = FarsFormViewModel.AbilityScale,
                ActivitiesScale = FarsFormViewModel.ActivitiesScale,
                AdmissionedFor = FarsFormViewModel.AdmissionedFor,
                AnxietyScale = FarsFormViewModel.AnxietyScale,
                CognitiveScale = FarsFormViewModel.CognitiveScale,
                ContID1 = FarsFormViewModel.ContID1,
                ContID2 = FarsFormViewModel.ContID2,
                ContID3 = FarsFormViewModel.ContID3,
                ContractorID = FarsFormViewModel.ContractorID,
                Country = FarsFormViewModel.Country,
                DangerToOtherScale = FarsFormViewModel.DangerToOtherScale,
                DangerToSelfScale = FarsFormViewModel.DangerToSelfScale,
                DcfEvaluation = FarsFormViewModel.DcfEvaluation,
                DepressionScale = FarsFormViewModel.DepressionScale,
                EvaluationDate = FarsFormViewModel.EvaluationDate,
                FamilyEnvironmentScale = FarsFormViewModel.FamilyEnvironmentScale,
                FamilyRelationShipsScale = FarsFormViewModel.FamilyRelationShipsScale,
                HyperAffectScale = FarsFormViewModel.HyperAffectScale,
                InterpersonalScale = FarsFormViewModel.InterpersonalScale,
                MCOID = FarsFormViewModel.MCOID,
                MedicaidProviderID = FarsFormViewModel.MedicaidProviderID,
                MedicaidRecipientID = FarsFormViewModel.MedicaidRecipientID,
                MedicalScale = FarsFormViewModel.MedicalScale,
                M_GafScore = FarsFormViewModel.M_GafScore,
                ProgramEvaluation = FarsFormViewModel.ProgramEvaluation,
                ProviderId = FarsFormViewModel.ProviderId,
                ProviderLocal = FarsFormViewModel.ProviderLocal,
                RaterEducation = FarsFormViewModel.RaterEducation,
                RaterFMHI = FarsFormViewModel.RaterFMHI,
                SecurityScale = FarsFormViewModel.SecurityScale,
                SignatureDate = FarsFormViewModel.SignatureDate,
                SocialScale = FarsFormViewModel.SocialScale,
                SubstanceAbusoHistory = FarsFormViewModel.SubstanceAbusoHistory,
                SubstanceScale = FarsFormViewModel.SubstanceScale,
                ThoughtProcessScale = FarsFormViewModel.ThoughtProcessScale,
                TraumaticsScale = FarsFormViewModel.TraumaticsScale,
                WorkScale = FarsFormViewModel.WorkScale
                

            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", FarsFormViewModel) });
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public IActionResult Edit(int id = 0)
        {
            FarsFormViewModel model;

            if (User.IsInRole("Supervisor") || User.IsInRole("Facilitator"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    FarsFormEntity FarsForm = _context.FarsForm
                                                        .Include(m => m.Client)
                                                        .ThenInclude(m => m.FarsFormList)
                                                        .FirstOrDefault(m => m.Id == id);
                    if (FarsForm == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToFarsFormViewModel(FarsForm);

                        return View(model);
                    }

                }
            }

            model = new FarsFormViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> Edit(FarsFormViewModel farsFormViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                FarsFormEntity farsFormEntity = await _converterHelper.ToFarsFormEntity(farsFormViewModel, false, user_logged.Id);
                _context.FarsForm.Update(farsFormEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "FarsForms");
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", farsFormViewModel) });
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            FarsFormEntity farsFormEntity = await _context.FarsForm.FirstOrDefaultAsync(s => s.Id == id);
            if (farsFormEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.FarsForm.Remove(farsFormEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Mannager, Supervisor, Facilitator")]
        public IActionResult PrintFarsForm(int id)
        {
            FarsFormEntity entity = _context.FarsForm

                                            .Include(f => f.Client)
                                            .ThenInclude(c => c.Clinic)

                                            .Include(i => i.Client)
                                            .ThenInclude(c => c.EmergencyContact)

                                            .Include(i => i.Client)
                                            .ThenInclude(c => c.LegalGuardian)

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
                Stream stream = _reportHelper.FloridaSocialHSFarsReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "DREAMS MENTAL HEALTH INC")
            {
                Stream stream = _reportHelper.DreamsMentalHealthFarsReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            return null;
        }

        public async Task<IActionResult> ClientswithoutFARS(int idError = 0)
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<ClientEntity> ClientList = await _context.Clients
                                                          .Include(n => n.FarsFormList)
                                                          .Where(n => n.FarsFormList.Count == 0 && n.Clinic.Id == user_logged.Clinic.Id)
                                                          .ToListAsync();

            return View(ClientList);

        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> FinishEditingFars(int id)
        {
            FarsFormEntity fars = await _context.FarsForm.FirstOrDefaultAsync(n => n.Id == id);
            if (User.IsInRole("Supervisor"))
            {
                fars.Status = FarsStatus.Approved;
            }
            else
            {
                fars.Status = FarsStatus.Pending;
            }

            _context.Update(fars);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ApproveFars(int id)
        {
            FarsFormEntity fars = await _context.FarsForm.FirstOrDefaultAsync(n => n.Id == id);
            fars.Status = FarsStatus.Approved;
            _context.Update(fars);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingFars));
        }

        [Authorize(Roles = "Supervisor, Mannager")]
        public async Task<IActionResult> PendingFars(int idError = 0)
        {
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
                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                if (clinic != null)
                {
                    return View(await _context.FarsForm
                                              .Include(c => c.Client)
                                              .ThenInclude(c => c.Clinic)
                                              .Where(m => (m.Client.Clinic.Id == clinic.Id)
                                                    && m.Status == FarsStatus.Pending)
                                              .OrderBy(m => m.Client.Clinic.Name).ToListAsync());
                    
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

    }
}
