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
    public class TCMFarsFormsController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IReportHelper _reportHelper;
        private readonly DataContext _context;

        public TCMFarsFormsController(IUserHelper userHelper, IConverterHelper converterHelper, ICombosHelper combosHelper, IRenderHelper renderHelper, DataContext context, IReportHelper reportHelper)
        {
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _context = context;
            _renderHelper = renderHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
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
                if (User.IsInRole("Manager")|| User.IsInRole("TCMSupervisor"))
                    return View(await _context.TCMClient

                                              .Include(f => f.TCMFarsFormList)

                                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

                if (User.IsInRole("CaseManager"))
                {

                    ViewData["origin"] = origin.ToString();
                    if (idTCMClient == 0)
                    {
                         List<TCMClientEntity> farsList = await _context.TCMClient
                                                                        .Include(f => f.Client)
                                                                        .Include(f => f.TCMFarsFormList)
                                                                        .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id
                                                                            && (n.Casemanager.Id == caseManager.Id))
                                                                        .OrderBy(f => f.Client.Name)
                                                                        .ToListAsync();
                        return View(farsList);

                    }
                    else
                    {
                        List<TCMClientEntity> farsList = await _context.TCMClient
                                                                       .Include(f => f.Client)
                                                                       .Include(f => f.TCMFarsFormList)
                                                                       .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id
                                                                         && (n.Casemanager.Id == caseManager.Id)
                                                                         && (n.Id == idTCMClient))
                                                                       .OrderBy(f => f.Client.Name)
                                                                       .ToListAsync();
                        return View(farsList);

                    }
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "TCMSupervisor, CaseManager")]
        public IActionResult Create(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);
            TCMClientEntity TCMClient = _context.TCMClient.Include(n => n.TCMFarsFormList)
                                                  .Include(n => n.Client.Clinic)
                                                    .FirstOrDefault(n => n.Id == id);
            TCMFarsFormViewModel model;

            if (user_logged.Clinic != null)
            {
                model = new TCMFarsFormViewModel
                {
                   IdTCMClient = id,
                   CreatedBy = user_logged.UserName,
                   CreatedOn = DateTime.Now,
                   TCMClient = TCMClient,
                   Id = 0,
                   AbilityScale = 0,
                   AdmissionedFor = user_logged.FullName,
                   ActivitiesScale = 0,
                   AnxietyScale = 0,
                   CognitiveScale = 0,
                   ContractorID = TCMClient.Client.Clinic.ProviderTaxId,
                   Country = "13",
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
                   MedicaidProviderID = TCMClient.Client.Clinic.ProviderMedicaidId,
                   MedicaidRecipientID = TCMClient.Client.MedicaidID,
                   MedicalScale = 0,
                   M_GafScore = "",
                   ProviderId = TCMClient.Client.Clinic.ProviderTaxId,
                   ProviderLocal = TCMClient.Client.Clinic.Address + ", " + TCMClient.Client.Clinic.City + ", " + TCMClient.Client.Clinic.State + ", "+ TCMClient.Client.Clinic.ZipCode,
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
                     
                   ContID1 = TCMClient.Client.Clinic.ProviderTaxId,
                   ContID2 = "",
                   ContID3 = "",
                   ProgramEvaluation = "",
                   Status = FarsStatus.Edition,
                   IdTCMSupervisor = 0,
                   TCMSupervisor  = new TCMSupervisorEntity()
                };

                TCMSupervisorEntity tcmSupervisor = _context.TCMSupervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                if (User.IsInRole("TCMSupervisor") && tcmSupervisor != null)
                {
                    model.TCMSupervisor = tcmSupervisor;
                    model.IdTCMSupervisor = tcmSupervisor.Id;
                    model.RaterEducation = tcmSupervisor.RaterEducation;
                    model.RaterFMHI = tcmSupervisor.RaterFMHCertification;
                }

                if (model.TCMClient.TCMFarsFormList == null)
                    model.TCMClient.TCMFarsFormList = new List<TCMFarsFormEntity>();
                return View(model);
                }
            
            return RedirectToAction("NotAuthorized", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor, CaseManager")]
        public async Task<IActionResult> Create(TCMFarsFormViewModel FarsFormViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMFarsFormEntity farsFormEntity = _context.TCMFarsForm.Find(FarsFormViewModel.Id);
                if (farsFormEntity == null)
                {
                    farsFormEntity = await _converterHelper.ToTCMFarsFormEntity(FarsFormViewModel, true, user_logged.UserName);
                    _context.TCMFarsForm.Add(farsFormEntity);
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
                    ModelState.AddModelError(string.Empty, "Already exists the TCM Fars.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", FarsFormViewModel) });
                }
            }
            TCMFarsFormViewModel model;
            model = new TCMFarsFormViewModel
            {
                IdTCMClient = FarsFormViewModel.IdTCMClient,
                TCMClient = _context.TCMClient.Find(FarsFormViewModel.IdTCMClient),
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

        [Authorize(Roles = "TCMSupervisor, CaseManager")]
        public IActionResult Edit(int id = 0, int origin = 0)
        {
            TCMFarsFormViewModel model;

            if (User.IsInRole("TCMSupervisor") || User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    TCMFarsFormEntity tcmFarsForm = _context.TCMFarsForm

                                                      .Include(m => m.TCMClient)
                                                      .ThenInclude(m => m.TCMFarsFormList)

                                                      .Include(m => m.TCMClient)
                                                      .ThenInclude(m => m.Client)

                                                      .FirstOrDefault(m => m.Id == id);
                    if (tcmFarsForm == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToTCMFarsFormViewModel(tcmFarsForm);
                        model.Origin = origin;
                        return View(model);
                    }

                }
            }

            model = new TCMFarsFormViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor, CaseManager")]
        public async Task<IActionResult> Edit(TCMFarsFormViewModel farsFormViewModel, string origin = "")
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMFarsFormEntity farsFormEntity = await _converterHelper.ToTCMFarsFormEntity(farsFormViewModel, false, user_logged.Id);
                _context.TCMFarsForm.Update(farsFormEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    if (farsFormViewModel.Origin == 1)
                    {
                        return RedirectToAction("Index", new { idTCMClient = farsFormEntity.TCMClient.Id, origin = origin });
                    }

                    return RedirectToAction("Index", new { idTCMClient = farsFormEntity.TCMClient.Id, origin = origin });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", farsFormViewModel) });
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMFarsFormEntity farsFormEntity = await _context.TCMFarsForm.FirstOrDefaultAsync(s => s.Id == id);
            if (farsFormEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMFarsForm.Remove(farsFormEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
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

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
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
            CaseMannagerEntity casemanager = _context.CaseManagers.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
            if (User.IsInRole("Facilitator"))
            {
                
                List<TCMClientEntity> tcmClientList = await _context.TCMClient
                                                                    .Include(n => n.TCMFarsFormList)
                                                                    .Where(n => n.TCMFarsFormList.Count == 0 && n.Client.Clinic.Id == user_logged.Clinic.Id
                                                                        && (n.Casemanager.Id == casemanager.Id))
                                                                    .ToListAsync();

                return View(tcmClientList);
            }
            else
            {
                List<TCMClientEntity> TCMClientList = await _context.TCMClient
                                                                    .Include(n => n.TCMFarsFormList)
                                                                    .Where(n => n.TCMFarsFormList.Count == 0 && n.Client.Clinic.Id == user_logged.Clinic.Id)
                                                                    .ToListAsync();

                return View(TCMClientList);
            }
            

        }

        [Authorize(Roles = "TCMSupervisor, CaseManager")]
        public async Task<IActionResult> FinishEditingFars(int id, string origin="")
        {
            TCMFarsFormEntity fars = await _context.TCMFarsForm
                                                   .Include(n => n.TCMClient) 
                                                   .FirstOrDefaultAsync(n => n.Id == id);
            if (User.IsInRole("TCMSupervisor"))
            {
                fars.Status = FarsStatus.Approved;
            }
            else
            {
                fars.Status = FarsStatus.Pending;
            }

            _context.Update(fars);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { idTCMClient = fars.TCMClient.Id, origin = origin });
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> ApproveFars(int id)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            TCMFarsFormEntity tcmfars = await _context.TCMFarsForm.FirstOrDefaultAsync(n => n.Id == id);
            tcmfars.Status = FarsStatus.Approved;
            tcmfars.TCMSupervisor = await _context.TCMSupervisors.FirstOrDefaultAsync(n => n.LinkedUser == user_logged.UserName);
            _context.Update(tcmfars);

            await _context.SaveChangesAsync();
            
            return RedirectToAction("TCMFarsApproved", "TCMFarsForms", new { status = FarsStatus.Pending });
           
        }

        [Authorize(Roles = "TCMSupervisor, CaseManager")]
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
                    return View(await _context.TCMFarsForm
                                              .Include(c => c.TCMClient)
                                              .ThenInclude(c => c.Client)
                                              .ThenInclude(c => c.Clinic)
                                              .Where(m => (m.TCMClient.Client.Clinic.Id == clinic.Id)
                                                    && m.Status == FarsStatus.Pending)
                                              .OrderBy(m => m.TCMClient.Client.Clinic.Name).ToListAsync());
                    
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult AddMessageEntity(int id = 0, int origin = 0)
        {
            if (id == 0)
            {
                return View(new MessageViewModel());
            }
            else
            {
                MessageViewModel model = new MessageViewModel()
                {
                    IdFarsForm = id,
                    Origin = origin
                };

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> AddMessageEntity(MessageViewModel messageViewModel)
        {
            if (ModelState.IsValid)
            {
                MessageEntity model = await _converterHelper.ToMessageEntity(messageViewModel, true);
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                model.From = user_logged.UserName;
                model.To = model.FarsForm.CreatedBy;
                _context.Add(model);
                await _context.SaveChangesAsync();
            }
            
            if (messageViewModel.Origin == 1)
                return RedirectToAction("PendingFars");

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> TCMFarsApproved(FarsStatus status)
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
                List<TCMFarsFormEntity> tcmFars = await _context.TCMFarsForm
                                                                .Include(m => m.TCMClient)
                                                                .ThenInclude(m => m.Client)
                                                                .Where(m => m.Status == status
                                                                            && m.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)
                                                                .OrderBy(m => m.TCMClient.CaseNumber)
                                                                .ToListAsync();
               
                return View(tcmFars);
            }

            if (User.IsInRole("CaseManager"))
            {
                List<TCMFarsFormEntity> tcmFars = await _context.TCMFarsForm
                                                                .Include(m => m.TCMClient)
                                                                .ThenInclude(m => m.Client)
                                                                .Where(m => m.Status == status
                                                                    && m.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                                    && m.TCMClient.Casemanager.LinkedUser == user_logged.UserName)
                                                                .OrderBy(m => m.TCMClient.CaseNumber)
                                                                .ToListAsync();

                return View(tcmFars);

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult EditReadOnly(int id = 0)
        {
            TCMFarsFormViewModel model;

            if (User.IsInRole("TCMSupervisor") || User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    TCMFarsFormEntity tcmFarsForm = _context.TCMFarsForm

                                                      .Include(m => m.TCMClient)
                                                      .ThenInclude(m => m.TCMFarsFormList)

                                                      .Include(m => m.TCMClient)
                                                      .ThenInclude(m => m.Client)

                                                      .FirstOrDefault(m => m.Id == id);
                    if (tcmFarsForm == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToTCMFarsFormViewModel(tcmFarsForm);
                        return View(model);
                    }

                }
            }

            model = new TCMFarsFormViewModel();
            return View(model);
        }

    }
}
