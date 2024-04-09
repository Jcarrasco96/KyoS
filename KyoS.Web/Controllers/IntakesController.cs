﻿using System;
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
using Microsoft.EntityFrameworkCore;
using System.IO;
using KyoS.Common.Helpers;

namespace KyoS.Web.Controllers
{    
    public class IntakesController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IReportHelper _reportHelper;
        private readonly DataContext _context;

        public IntakesController(IUserHelper userHelper, IConverterHelper converterHelper, ICombosHelper combosHelper, IRenderHelper renderHelper, DataContext context, IReportHelper reportHelper)
        {
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _context = context;
            _renderHelper = renderHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, Frontdesk")]
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
                if (User.IsInRole("Manager") || User.IsInRole("Supervisor") || User.IsInRole("Frontdesk"))
                {
                    return View(await _context.Clients

                                              .Include(n => n.IntakeScreening)
                                              .Include(n => n.IntakeConsentForTreatment)
                                              .Include(n => n.IntakeConsentForRelease)
                                              .Include(n => n.IntakeConsumerRights)
                                              .Include(n => n.IntakeAcknowledgementHipa)
                                              .Include(n => n.IntakeAccessToServices)
                                              .Include(n => n.IntakeOrientationChecklist)
                                              .Include(n => n.IntakeTransportation)
                                              .Include(n => n.IntakeConsentPhotograph)
                                              .Include(n => n.IntakeFeeAgreement)
                                              .Include(n => n.IntakeTuberculosis)
                                              .Include(n => n.IntakeMedicalHistory)
                                              .Include(n => n.IntakeConsentForTelehealth)
                                              .Include(n => n.IntakeNoDuplicateService)
                                              .Include(n => n.IntakeAdvancedDirective)
                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                              .ToListAsync());
                }
                else
                {
                    if (User.IsInRole("Facilitator"))
                    {
                        FacilitatorEntity facilitator = await _context.Facilitators.FirstOrDefaultAsync(f => f.LinkedUser == user_logged.UserName);
                        return View(await _context.Clients

                                              .Include(n => n.IntakeScreening)
                                              .Include(n => n.IntakeConsentForTreatment)
                                              .Include(n => n.IntakeConsentForRelease)
                                              .Include(n => n.IntakeConsumerRights)
                                              .Include(n => n.IntakeAcknowledgementHipa)
                                              .Include(n => n.IntakeAccessToServices)
                                              .Include(n => n.IntakeOrientationChecklist)
                                              .Include(n => n.IntakeTransportation)
                                              .Include(n => n.IntakeConsentPhotograph)
                                              .Include(n => n.IntakeFeeAgreement)
                                              .Include(n => n.IntakeTuberculosis)
                                              .Include(n => n.IntakeMedicalHistory)
                                              .Include(n => n.IntakeConsentForTelehealth)
                                              .Include(n => n.IntakeNoDuplicateService)
                                              .Include(n => n.IntakeAdvancedDirective)
                                              .Where(n => (n.Clinic.Id == user_logged.Clinic.Id
                                                  && n.Workdays_Clients.Where(m => m.Facilitator.Id == facilitator.Id).Count() > 0))
                                              .ToListAsync());
                    }
                    else
                    {
                        if (User.IsInRole("Documents_Assistant"))
                        {
                            DocumentsAssistantEntity doc_assistant = await _context.DocumentsAssistant.FirstOrDefaultAsync(f => f.LinkedUser == user_logged.UserName);
                            return View(await _context.Clients

                                                  .Include(n => n.IntakeScreening)
                                                  .Include(n => n.IntakeConsentForTreatment)
                                                  .Include(n => n.IntakeConsentForRelease)
                                                  .Include(n => n.IntakeConsumerRights)
                                                  .Include(n => n.IntakeAcknowledgementHipa)
                                                  .Include(n => n.IntakeAccessToServices)
                                                  .Include(n => n.IntakeOrientationChecklist)
                                                  .Include(n => n.IntakeTransportation)
                                                  .Include(n => n.IntakeConsentPhotograph)
                                                  .Include(n => n.IntakeFeeAgreement)
                                                  .Include(n => n.IntakeTuberculosis)
                                                  .Include(n => n.IntakeMedicalHistory)
                                                  .Include(n => n.Bio)
                                                  .Include(n => n.IntakeConsentForTelehealth)
                                                  .Include(n => n.IntakeNoDuplicateService)
                                                  .Include(n => n.IntakeAdvancedDirective)
                                                  .Where(n => (n.Clinic.Id == user_logged.Clinic.Id
                                                            && (n.Bio.DocumentsAssistant.Id == doc_assistant.Id
                                                            || n.IntakeAccessToServices.AdmissionedFor == user_logged.FullName
                                                            || n.IntakeAcknowledgementHipa.AdmissionedFor == user_logged.FullName
                                                            || n.IntakeConsentForRelease.AdmissionedFor == user_logged.FullName
                                                            || n.IntakeConsentForTreatment.AdmissionedFor == user_logged.FullName
                                                            || n.IntakeConsentPhotograph.AdmissionedFor == user_logged.FullName
                                                            || n.IntakeConsumerRights.AdmissionedFor == user_logged.FullName
                                                            || n.IntakeFeeAgreement.AdmissionedFor == user_logged.FullName
                                                            || n.IntakeMedicalHistory.AdmissionedFor == user_logged.FullName
                                                            || n.IntakeOrientationChecklist.AdmissionedFor == user_logged.FullName
                                                            || n.IntakeScreening.InformationGatheredBy == user_logged.FullName
                                                            || n.IntakeTransportation.AdmissionedFor == user_logged.FullName
                                                            || n.IntakeTuberculosis.AdmissionedFor == user_logged.FullName)))
                                                  .ToListAsync());
                        }
                    }
                }
                return View(null);         
            }            
        }

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult Create(int id = 0)
        {
            
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeScreeningViewModel model;
            ClientEntity client = _context.Clients.Include(n => n.LegalGuardian).Include(n => n.EmergencyContact).FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Manager"))
            {
                if (user_logged.Clinic != null)
                {
                    
                    model = new IntakeScreeningViewModel
                    {
                        IdClient = id,
                        Client_FK = id,
                        Client = client,
                        InformationGatheredBy = user_logged.FullName,
                        ClientIsStatus = IntakeClientIsStatus.Clean,
                        BehaviorIsStatus = IntakeBehaviorIsStatus.Normal,
                        SpeechIsStatus = IntakeSpeechIsStatus.Normal,
                        DoesClientKnowHisName = true,
                        DoesClientKnowTodayDate = true,
                        DoesClientKnowWhereIs = true,
                        DoesClientKnowTimeOfDay = true,
                        DateSignatureClient = client.AdmisionDate,
                        DateSignatureWitness = client.AdmisionDate,
                        DateSignatureEmployee = client.AdmisionDate,
                        IdClientIs = 0,
                        ClientIs_Status = _combosHelper.GetComboIntake_ClientIs(),
                        IdBehaviorIs = 0,
                        BehaviorIs_Status = _combosHelper.GetComboIntake_BehaviorIs(),
                        IdSpeechIs = 0,
                        SpeechIs_Status = _combosHelper.GetComboIntake_SpeechIs(),
                        EmergencyContact = true,
                        
                    };
                    if (model.Client.LegalGuardian == null)
                        model.Client.LegalGuardian = new LegalGuardianEntity();
                    if (model.Client.EmergencyContact == null)
                    {
                        model.Client.EmergencyContact = new EmergencyContactEntity();
                        model.EmergencyContact = false;
                    } 
                    return View(model);
                }
            }

            model = new IntakeScreeningViewModel
            {
                IdClient = id,
                Client = _context.Clients.Include(n => n.LegalGuardian).Include(n => n.EmergencyContact).FirstOrDefault(n => n.Id == id),
                InformationGatheredBy = user_logged.FullName,
                ClientIsStatus = IntakeClientIsStatus.Clean,
                BehaviorIsStatus = IntakeBehaviorIsStatus.Normal,
                SpeechIsStatus = IntakeSpeechIsStatus.Normal,
                DoesClientKnowHisName = true,
                DoesClientKnowTodayDate = true,
                DoesClientKnowWhereIs = true,
                DoesClientKnowTimeOfDay = true,
                DateSignatureClient = client.AdmisionDate,
                DateSignatureWitness = client.AdmisionDate,
                DateSignatureEmployee = client.AdmisionDate,
                IdClientIs = 0,
                ClientIs_Status = _combosHelper.GetComboIntake_ClientIs(),
                IdBehaviorIs = 0,
                BehaviorIs_Status = _combosHelper.GetComboIntake_BehaviorIs(),
                IdSpeechIs = 0,
                SpeechIs_Status = _combosHelper.GetComboIntake_SpeechIs(),
                EmergencyContact = true,
            };

            if (model.Client.LegalGuardian == null)
                model.Client.LegalGuardian = new LegalGuardianEntity();
            if (model.Client.EmergencyContact == null)
            {
                model.Client.EmergencyContact = new EmergencyContactEntity();
                model.EmergencyContact = false;
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> Create(IntakeScreeningViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeScreeningEntity IntakeEntity = _context.IntakeScreenings.Find(IntakeViewModel.Id);
                if (IntakeEntity == null)
                {
                    IntakeEntity = await _converterHelper.ToIntakeEntity(IntakeViewModel, true);
                    _context.IntakeScreenings.Add(IntakeEntity);
                    try
                    {
                        await _context.SaveChangesAsync();                       
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient});
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the TCM service.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", IntakeViewModel) });
                }
            }
            IntakeScreeningViewModel model;
            model = new IntakeScreeningViewModel
            {
                IdClient = IntakeViewModel.IdClient,
                Client = _context.Clients.Include(n => n.LegalGuardian).Include(n => n.EmergencyContact).FirstOrDefault(n => n.Id == IntakeViewModel.IdClient),
                InformationGatheredBy = user_logged.FullName,
                ClientIsStatus = IntakeClientIsStatus.Clean,
                BehaviorIsStatus = IntakeBehaviorIsStatus.Normal,
                SpeechIsStatus = IntakeSpeechIsStatus.Normal,
                DoesClientKnowHisName = true,
                DoesClientKnowTodayDate = true,
                DoesClientKnowWhereIs = true,
                DoesClientKnowTimeOfDay = true,
                DateSignatureClient = DateTime.Now,
                DateSignatureWitness = DateTime.Now,
                DateSignatureEmployee = DateTime.Now,
                IdClientIs = IntakeViewModel.IdClientIs,
                ClientIs_Status = _combosHelper.GetComboIntake_ClientIs(),
                IdBehaviorIs = IntakeViewModel.IdBehaviorIs,
                BehaviorIs_Status = _combosHelper.GetComboIntake_BehaviorIs(),
                IdSpeechIs = IntakeViewModel.IdSpeechIs,
                SpeechIs_Status = _combosHelper.GetComboIntake_SpeechIs(),
                EmergencyContact = true,
            };
            if (model.Client.LegalGuardian == null)
                model.Client.LegalGuardian = new LegalGuardianEntity();
            if (model.Client.EmergencyContact == null)
            {
                model.Client.EmergencyContact = new EmergencyContactEntity();
                model.EmergencyContact = false;
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
        }

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult Edit(int id = 0)
        {
            IntakeScreeningEntity entity = _context.IntakeScreenings

                                                   .Include(m => m.Client)
                                                   .ThenInclude(m => m.LegalGuardian)
                                                   .Include(n => n.Client.EmergencyContact)

                                                   .FirstOrDefault(i => i.Client.Id == id);
            if (entity == null)
            {
                return RedirectToAction("Create", new {id = id});
            }

            IntakeScreeningViewModel model;

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk") || User.IsInRole("Documents_Assistant"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {
                        model = _converterHelper.ToIntakeViewModel(entity);
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        if (model.Client.EmergencyContact == null)
                        {
                            model.Client.EmergencyContact = new EmergencyContactEntity();
                            model.EmergencyContact = false;
                        }

                        return View(model);   
                }
            }

            model = new IntakeScreeningViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> Edit(IntakeScreeningViewModel intakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeScreeningEntity intakeEntity = await _converterHelper.ToIntakeEntity(intakeViewModel, false);
                _context.IntakeScreenings.Update(intakeEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("IntakeDashboard", new { id = intakeViewModel.IdClient });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", intakeViewModel) });
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            IntakeScreeningEntity intakeEntity = await _context.IntakeScreenings.FirstOrDefaultAsync(s => s.Id == id);
            if (intakeEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.IntakeScreenings.Remove(intakeEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult CreateConsentForTreatment(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeConsentForTreatmentViewModel model;
            ClientEntity client = _context.Clients.Include(d => d.LegalGuardian).FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeConsentForTreatmentEntity intakeConsent = _context.IntakeConsentForTreatment
                                                                            .Include(n => n.Client)
                                                                            .ThenInclude(n => n.LegalGuardian)
                                                                            .FirstOrDefault(n => n.Client.Id == id);
                    
                    if (intakeConsent == null)
                    {
                        model = new IntakeConsentForTreatmentViewModel
                        {
                            Client = client,
                            Aggre = true,
                            Aggre1 = true,
                            AuthorizeRelease = true,
                            AuthorizeStaff = true,
                            Certify = false,
                            Certify1 = true,
                            DateSignatureEmployee = client.AdmisionDate,
                            DateSignatureLegalGuardian = client.AdmisionDate,
                            DateSignaturePerson = client.AdmisionDate,
                            Documents = true,
                            Id = 0,
                            Underestand = true,
                            IdClient = id,
                            Client_FK = id,
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeConsent.Client.LegalGuardian == null)
                            intakeConsent.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToIntakeConsentForTreatmentViewModel(intakeConsent);

                        return View(model);
                    }
                    
                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> CreateConsentForTreatment(IntakeConsentForTreatmentViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeConsentForTreatmentEntity IntakeConsentEntity = _converterHelper.ToIntakeConsentForTreatmentEntity(IntakeViewModel, false);
                
                if (IntakeConsentEntity.Id == 0)
                {
                    IntakeConsentEntity.Client = null;
                    _context.IntakeConsentForTreatment.Add(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeConsentEntity.Client = null;
                    _context.IntakeConsentForTreatment.Update(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);
            
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateConsentForTreatment", IntakeViewModel) });
        }


        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult CreateConsentForRelease(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeConsentForReleaseViewModel model;
            ClientEntity client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeConsentForReleaseEntity intakeConsent = _context.IntakeConsentForRelease
                                                                            .Include(n => n.Client)
                                                                            .ThenInclude(n => n.LegalGuardian)
                                                                            .FirstOrDefault(n => n.Client.Id == id);
                    if (intakeConsent == null)
                    {
                        model = new IntakeConsentForReleaseViewModel
                        {
                            Client = client,
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            ToRelease = true,
                            ForPurpose_CaseManagement = false,
                            ForPurpose_Other = false,
                            ForPurpose_OtherExplain = "",
                            ForPurpose_Treatment = false,
                            InForm_Facsimile = false,
                            InForm_VerbalInformation = false,
                            InForm_WrittenRecords = false,
                            Discaherge = false,
                            SchoolRecord = false,
                            ProgressReports = false,
                            IncidentReport = false,
                            PsychologycalEvaluation = false,
                            History = false,
                            LabWork = false,
                            HospitalRecord = false,
                            Other = false,
                            Other_Explain = "",
                            Documents = true,
                            DateSignatureEmployee = client.AdmisionDate,
                            DateSignatureLegalGuardian = client.AdmisionDate,
                            DateSignaturePerson = client.AdmisionDate,
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeConsent.Client.LegalGuardian == null)
                            intakeConsent.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToIntakeConsentForReleaseViewModel(intakeConsent);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> CreateConsentForRelease(IntakeConsentForReleaseViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeConsentForReleaseEntity IntakeConsentEntity = _converterHelper.ToIntakeConsentForReleaseEntity(IntakeViewModel, false);

                if (IntakeConsentEntity.Id == 0)
                {
                    IntakeConsentEntity.Client = null;
                    _context.IntakeConsentForRelease.Add(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeConsentEntity.Client = null;
                    _context.IntakeConsentForRelease.Update(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync(); 
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);
            
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateConsentForRelease", IntakeViewModel) });
        }


        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult CreateConsumerRights(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeConsumerRightsViewModel model;
            ClientEntity client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeConsumerRightsEntity intakeConsent = _context.IntakeConsumerRights
                                                                            .Include(n => n.Client)
                                                                            .ThenInclude(n => n.LegalGuardian)
                                                                            .FirstOrDefault(n => n.Client.Id == id);
                    if (intakeConsent == null)
                    {
                        model = new IntakeConsumerRightsViewModel
                        {
                            Client = client,
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            ServedOf = user_logged.FullName,
                            Documents = true,
                            DateSignatureEmployee = client.AdmisionDate,
                            DateSignatureLegalGuardian = client.AdmisionDate,
                            DateSignaturePerson = client.AdmisionDate,
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeConsent.Client.LegalGuardian == null)
                            intakeConsent.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToIntakeConsumerRightsViewModel(intakeConsent);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> CreateConsumerRights(IntakeConsumerRightsViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeConsumerRightsEntity IntakeConsumerEntity = _converterHelper.ToIntakeConsumerRightsEntity(IntakeViewModel, false);

                if (IntakeConsumerEntity.Id == 0)
                {
                    IntakeConsumerEntity.Client = null;
                    _context.IntakeConsumerRights.Add(IntakeConsumerEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeConsumerEntity.Client = null;
                    _context.IntakeConsumerRights.Update(IntakeConsumerEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateConsumerRights", IntakeViewModel) });
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, Frontdesk")]
        public IActionResult PrintIntake(int id)
        {
            IntakeScreeningEntity entity = _context.IntakeScreenings

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.Clinic)

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.EmergencyContact)

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.LegalGuardian)

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.IntakeConsentForTreatment)

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.IntakeConsentForRelease)

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.IntakeConsumerRights)

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.IntakeAcknowledgementHipa)

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.IntakeAccessToServices)

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.IntakeOrientationChecklist)

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.IntakeFeeAgreement)

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.IntakeTuberculosis)

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.IntakeTransportation)

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.IntakeConsentPhotograph)

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.IntakeMedicalHistory)

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.DischargeList)

                                                   .AsSplitQuery()

                                                   .FirstOrDefault(i => (i.Id == id));
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            if (entity.Client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
            {
                Stream stream = _reportHelper.FloridaSocialHSIntakeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "DREAMS MENTAL HEALTH INC")
            {
                Stream stream = _reportHelper.DreamsMentalHealthIntakeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
            {
                Stream stream = _reportHelper.CommunityHTCIntakeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "PRINCIPLE CARE CENTER INC")
            {
                Stream stream = _reportHelper.PrincipleCCIIntakeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "SAPPHIRE MENTAL HEALTH CENTER LLC")
            {
                Stream stream = _reportHelper.SapphireMHCIntakeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }            
            if (entity.Client.Clinic.Name == "MEDICAL & REHAB OF HILLSBOROUGH INC")
            {
                Stream stream = _reportHelper.MedicalRehabIntakeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "MY FLORIDA CASE MANAGEMENT SERVICES LLC")
            {
                Stream stream = _reportHelper.MyFloridaIntakeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "ORION MENTAL HEALTH CENTER LLC")
            {
                Stream stream = _reportHelper.OrionIntakeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "ALLIED HEALTH GROUP LLC")
            {
                Stream stream = _reportHelper.AlliedIntakeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "YOUR NEIGHBOR MEDICAL GROUP")
            {
                Stream stream = _reportHelper.YourNeighborIntakeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            return null;
        }

        [Authorize(Roles = "Manager, Supervisor, Documents_Assistant, Frontdesk")]
        public IActionResult PrintMedicalHistory(int id)
        {
            IntakeMedicalHistoryEntity entity = _context.IntakeMedicalHistory

                                                        .Include(i => i.Client)    
                                                            .ThenInclude(c => c.Clinic)
                                                        .Include(i => i.Client)
                                                            .ThenInclude(c => c.IntakeTuberculosis)
                                                        .Include(i => i.Client)
                                                            .ThenInclude(c => c.MedicationList)

                                                        .AsSplitQuery()

                                                        .FirstOrDefault(i => (i.Client.Id == id));
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }            

            if (entity.Client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
            {
                Stream stream = _reportHelper.FloridaSocialHSMedicalHistoryReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "DREAMS MENTAL HEALTH INC")
            {
                Stream stream = _reportHelper.DreamsMentalHealthMedicalHistoryReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
            {
                Stream stream = _reportHelper.CommunityHTCMedicalHistoryReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "PRINCIPLE CARE CENTER INC")
            {
                Stream stream = _reportHelper.PrincipleCCIMedicalHistoryReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "SAPPHIRE MENTAL HEALTH CENTER LLC")
            {
                Stream stream = _reportHelper.SapphireMHCMedicalHistoryReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }            
            if (entity.Client.Clinic.Name == "MEDICAL & REHAB OF HILLSBOROUGH INC")
            {
                Stream stream = _reportHelper.MedicalRehabMedicalHistoryReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "MY FLORIDA CASE MANAGEMENT SERVICES LLC")
            {
                Stream stream = _reportHelper.MyFloridaMedicalHistoryReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "ORION MENTAL HEALTH CENTER LLC")
            {
                Stream stream = _reportHelper.OrionMedicalHistoryReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "ALLIED HEALTH GROUP LLC")
            {
                Stream stream = _reportHelper.AlliedMedicalHistoryReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "YOUR NEIGHBOR MEDICAL GROUP")
            {
                Stream stream = _reportHelper.YourNeighborMedicalHistoryReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            return null;
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, Frontdesk")]
        public async Task<IActionResult> PrintIDDocumentsVerificationForm(int id)
        {
            IntakeClientIdDocumentVerificationEntity entity = await _context.IntakeClientDocumentVerification
                                                                               
                                                                            .Include(i => i.Client)
                                                                            
                                                                            .Include(c => c.Client)
                                                                            .ThenInclude(cl => cl.LegalGuardian)

                                                                            .Include(i => i.Client)
                                                                            .ThenInclude(cm => cm.Clinic)

                                                                            .FirstOrDefaultAsync(t => t.Client.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.IntakeClientDocumentVerification(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, Frontdesk")]
        public async Task<IActionResult> PrintForeignLangAcknowledgement(int id)
        {
            IntakeForeignLanguageEntity entity = await _context.IntakeForeignLanguage
                                                                                 
                                                               .Include(c => c.Client)
                                                                  
                                                               .Include(c => c.Client)
                                                               .ThenInclude(cl => cl.LegalGuardian)

                                                               .Include(c => c.Client)                                                                  
                                                               .ThenInclude(cm => cm.Clinic)

                                                               .FirstOrDefaultAsync(c => c.Client.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.IntakeForeignLanguage(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, Frontdesk")]
        public async Task<IActionResult> PrintAdvancedDirective(int id)
        {
            IntakeAdvancedDirectiveEntity entity = await _context.IntakeAdvancedDirective

                                                                 .Include(c => c.Client)

                                                                 .Include(c => c.Client)
                                                                 .ThenInclude(cl => cl.LegalGuardian)

                                                                 .Include(c => c.Client)
                                                                 .ThenInclude(cl => cl.EmergencyContact)

                                                                 .Include(c => c.Client)
                                                                 .ThenInclude(cm => cm.Clinic)

                                                                 .FirstOrDefaultAsync(c => c.Client.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.IntakeAdvancedDirective(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, Frontdesk")]
        public async Task<IActionResult> PrintTelehealthConsent(int id)
        {
            IntakeConsentForTelehealthEntity entity = await _context.IntakeConsentForTelehealth

                                                                    .Include(c => c.Client)
                                                                 
                                                                    .Include(c => c.Client)
                                                                    .ThenInclude(cm => cm.Clinic)

                                                                    .FirstOrDefaultAsync(c => c.Client.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.IntakeTelehealthConsent(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, Frontdesk")]
        public async Task<IActionResult> PrintNoDuplicateService(int id)
        {
            IntakeNoDuplicateServiceEntity entity = await _context.IntakeNoDuplicateService

                                                                  .Include(c => c.Client)

                                                                  .Include(c => c.Client)
                                                                  .ThenInclude(cm => cm.Clinic)

                                                                  .Include(c => c.Client)
                                                                  .ThenInclude(cm => cm.LegalGuardian)

                                                                  .FirstOrDefaultAsync(c => c.Client.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.IntakeNoDuplicateService(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }        

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult CreateAcknowledgementHippa(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeAcknoewledgementHippaViewModel model;
            ClientEntity client = _context.Clients.Include(d => d.LegalGuardian).FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeAcknowledgementHippaEntity intakeAck = _context.IntakeAcknowledgement
                                                                            .Include(n => n.Client)
                                                                            .ThenInclude(n => n.LegalGuardian)
                                                                            .FirstOrDefault(n => n.Client.Id == id);

                    if (intakeAck == null)
                    {

                        model = new IntakeAcknoewledgementHippaViewModel
                        {
                            Client = client,
                            DateSignatureEmployee = client.AdmisionDate,
                            DateSignatureLegalGuardian = client.AdmisionDate,
                            DateSignaturePerson = client.AdmisionDate,
                            Documents = true,
                            Id = 0,
                            IdClient = id,
                            Client_FK = id,
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeAck.Client.LegalGuardian == null)
                            intakeAck.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToIntakeAcknoewledgementHippaViewModel(intakeAck);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> CreateAcknowledgementHippa(IntakeAcknoewledgementHippaViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeAcknowledgementHippaEntity IntakeAckNowEntity = _converterHelper.ToIntakeAcknoewledgementHippaEntity(IntakeViewModel, false);

                if (IntakeAckNowEntity.Id == 0)
                {
                    IntakeAckNowEntity.Client = null;
                    _context.IntakeAcknowledgement.Add(IntakeAckNowEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeAckNowEntity.Client = null;
                    _context.IntakeAcknowledgement.Update(IntakeAckNowEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateAcknowledgementHippa", IntakeViewModel) });
        }

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult CreateAccessToServices(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeAccessToServicesViewModel model;
            ClientEntity client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeAccessToServicesEntity intakeAccess = _context.IntakeAccessToServices
                                                                            .Include(n => n.Client)
                                                                            .ThenInclude(n => n.LegalGuardian)
                                                                            .FirstOrDefault(n => n.Client.Id == id);
                    if (intakeAccess == null)
                    {
                        model = new IntakeAccessToServicesViewModel
                        {
                            Client = client,
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            Documents = true,
                            DateSignatureEmployee = client.AdmisionDate,
                            DateSignatureLegalGuardian = client.AdmisionDate,
                            DateSignaturePerson = client.AdmisionDate,
                            AdmissionedFor = user_logged.FullName,

                        };
                        
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        
                        if (intakeAccess.Client.LegalGuardian == null)
                            intakeAccess.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToIntakeAccessToServicesViewModel(intakeAccess);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> CreateAccessToServices(IntakeAccessToServicesViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeAccessToServicesEntity IntakeAccessEntity = _converterHelper.ToIntakeAccessToServicesEntity(IntakeViewModel, false);

                if (IntakeAccessEntity.Id == 0)
                {
                    IntakeAccessEntity.Client = null;
                    _context.IntakeAccessToServices.Add(IntakeAccessEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeAccessEntity.Client = null;
                    _context.IntakeAccessToServices.Update(IntakeAccessEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateAccessToServices", IntakeViewModel) });
        }

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult CreateOrientationCheckList(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeOrientationCheckListViewModel model;
            ClientEntity client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeOrientationChecklistEntity intakeCheckList = _context.IntakeOrientationCheckList
                                                                            .Include(n => n.Client)
                                                                            .ThenInclude(n => n.LegalGuardian)
                                                                            .FirstOrDefault(n => n.Client.Id == id);
                    if (intakeCheckList == null)
                    {
                        model = new IntakeOrientationCheckListViewModel
                        {
                            Client = client,
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            DateSignatureEmployee = client.AdmisionDate,
                            DateSignatureLegalGuardian = client.AdmisionDate,
                            DateSignaturePerson = client.AdmisionDate,
                            Access = true,
                            AgencyExpectation = true,
                            AgencyPolice = true,
                            Code = true,
                            Confidentiality = true,
                            Discharge = true,
                            Education = true,
                            Explanation = true,
                            Fire = true,
                            Identification = true,
                            IndividualPlan = true,
                            Insent = true,
                            Methods = true,
                            PoliceGrievancce = true,
                            PoliceIllicit = true,
                            PoliceTobacco = true,
                            PoliceWeapons = true,
                            Program = true,
                            Purpose = true,
                            Rights = true,
                            Services = true,
                            TheAbove = true,
                            TourFacility = true,
                            Documents = true,
                            AdmissionedFor = user_logged.FullName,
                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeCheckList.Client.LegalGuardian == null)
                            intakeCheckList.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToIntakeOrientationChecklistViewModel(intakeCheckList);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> CreateOrientationCheckList(IntakeOrientationCheckListViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeOrientationChecklistEntity IntakeOrientationEntity = _converterHelper.ToIntakeOrientationChecklistEntity(IntakeViewModel, false);

                if (IntakeOrientationEntity.Id == 0)
                {
                    IntakeOrientationEntity.Client = null;
                    _context.IntakeOrientationCheckList.Add(IntakeOrientationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeOrientationEntity.Client = null;
                    _context.IntakeOrientationCheckList.Update(IntakeOrientationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateOrientationCheckList", IntakeViewModel) });
        }

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult CreateTransportation(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeTransportationViewModel model;
            ClientEntity client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeTransportationEntity intakeTransportation = _context.IntakeTransportation
                                                                            .Include(n => n.Client)
                                                                            .ThenInclude(n => n.LegalGuardian)
                                                                            .FirstOrDefault(n => n.Client.Id == id);
                    if (intakeTransportation == null)
                    {
                        model = new IntakeTransportationViewModel
                        {
                            Client = client,
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            Documents = true,
                            DateSignatureEmployee = client.AdmisionDate,
                            DateSignatureLegalGuardian = client.AdmisionDate,
                            DateSignaturePerson = client.AdmisionDate,
                            AdmissionedFor = user_logged.FullName,

                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeTransportation.Client.LegalGuardian == null)
                            intakeTransportation.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToIntakeTransportationViewModel(intakeTransportation);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> CreateTransportation(IntakeTransportationViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeTransportationEntity IntakeTransportationEntity = _converterHelper.ToIntakeTransportationEntity(IntakeViewModel, false);

                if (IntakeTransportationEntity.Id == 0)
                {
                    IntakeTransportationEntity.Client = null;
                    _context.IntakeTransportation.Add(IntakeTransportationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeTransportationEntity.Client = null;
                    _context.IntakeTransportation.Update(IntakeTransportationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTransportation", IntakeViewModel) });
        }


        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult CreateConsentPhotograph(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeConsentPhotographViewModel model;
            ClientEntity client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeConsentPhotographEntity intakeConsentPhotograph = _context.IntakeConsentPhotograph
                                                                            .Include(n => n.Client)
                                                                            .ThenInclude(n => n.LegalGuardian)
                                                                            .FirstOrDefault(n => n.Client.Id == id);
                    if (intakeConsentPhotograph == null)
                    {
                        model = new IntakeConsentPhotographViewModel
                        {
                            Client = client,
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            DateSignatureEmployee = client.AdmisionDate,
                            DateSignatureLegalGuardian = client.AdmisionDate,
                            DateSignaturePerson = client.AdmisionDate,
                            Photograph = true,
                            Filmed = true,
                            VideoTaped = true,
                            Interviwed = true,
                            NoneOfTheForegoing = false,
                            Other = "",
                            Publication = true,
                            Broadcast = true,
                            Markrting = true,
                            ByTODocument = true,

                            Documents = true,
                            AdmissionedFor = user_logged.FullName,
                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeConsentPhotograph.Client.LegalGuardian == null)
                            intakeConsentPhotograph.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToIntakeConsentPhotographViewModel(intakeConsentPhotograph);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> CreateConsentPhotograph(IntakeConsentPhotographViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeConsentPhotographEntity IntakeConsentPhotographEntity = _converterHelper.ToIntakeConsentPhotographEntity(IntakeViewModel, false);

                if (IntakeConsentPhotographEntity.Id == 0)
                {
                    IntakeConsentPhotographEntity.Client = null;
                    _context.IntakeConsentPhotograph.Add(IntakeConsentPhotographEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeConsentPhotographEntity.Client = null;
                    _context.IntakeConsentPhotograph.Update(IntakeConsentPhotographEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateConsentPhotograph", IntakeViewModel) });
        }

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult CreateFeeAgreement(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeFeeAgreementViewModel model;
            ClientEntity client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeFeeAgreementEntity intakefeeAgreement = _context.IntakeFeeAgreement
                                                                            .Include(n => n.Client)
                                                                            .ThenInclude(n => n.LegalGuardian)
                                                                            .FirstOrDefault(n => n.Client.Id == id);
                    if (intakefeeAgreement == null)
                    {
                        model = new IntakeFeeAgreementViewModel
                        {
                            Client = client,
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            Documents = true,
                            DateSignatureEmployee = client.AdmisionDate,
                            DateSignatureLegalGuardian = client.AdmisionDate,
                            DateSignaturePerson = client.AdmisionDate,
                            AdmissionedFor = user_logged.FullName,

                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakefeeAgreement.Client.LegalGuardian == null)
                            intakefeeAgreement.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToIntakeFeeAgreementViewModel(intakefeeAgreement);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> CreateFeeAgreement(IntakeFeeAgreementViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeFeeAgreementEntity IntakefeeAgreementEntity = _converterHelper.ToIntakeFeeAgreementEntity(IntakeViewModel, false);

                if (IntakefeeAgreementEntity.Id == 0)
                {
                    IntakefeeAgreementEntity.Client = null;
                    _context.IntakeFeeAgreement.Add(IntakefeeAgreementEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakefeeAgreementEntity.Client = null;
                    _context.IntakeFeeAgreement.Update(IntakefeeAgreementEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateFeeAgreement", IntakeViewModel) });
        }

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult CreateTuberculosis(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeTuberculosisViewModel model;
            ClientEntity client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeTuberculosisEntity intakeTuberculosis = _context.IntakeTuberculosis
                                                                            .Include(n => n.Client)
                                                                            .ThenInclude(n => n.LegalGuardian)
                                                                            .FirstOrDefault(n => n.Client.Id == id);
                    if (intakeTuberculosis == null)
                    {
                        model = new IntakeTuberculosisViewModel
                        {
                            Client = client,
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            DateSignatureEmployee = client.AdmisionDate,
                            DateSignatureLegalGuardian = client.AdmisionDate,
                            DateSignaturePerson = client.AdmisionDate,

                            DoYouCurrently  = false,
                            DoYouBring = false,
                            DoYouCough = false,
                            DoYouSweat = false,
                            DoYouHaveFever = false,
                            HaveYouLost = false,
                            DoYouHaveChest = false,
                            If2OrMore = false,
                        
                            HaveYouRecently = false,
                            AreYouRecently = false,
                            IfYesWhich = false,
                            DoYouOr = false,
                            HaveYouEverBeen = false,
                            HaveYouEverWorked = false,
                            HaveYouEverHadOrgan = false,
                            HaveYouEverConsidered = false,
                            HaveYouEverHadAbnormal = false,
                            If3OrMore = false,

                            HaveYouEverHadPositive = false,
                            IfYesWhere = "",
                            When = "",
                            HaveYoyEverBeenTold = false,
                            AgencyExpectation = false,
                            If1OrMore = false,

                            Documents = true,
                            AdmissionedFor = user_logged.FullName,
                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeTuberculosis.Client.LegalGuardian == null)
                            intakeTuberculosis.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToIntakeTuberculosisViewModel(intakeTuberculosis);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> CreateTuberculosis(IntakeTuberculosisViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeTuberculosisEntity IntakeTuberculosisEntity = _converterHelper.ToIntakeTuberculosisEntity(IntakeViewModel, false);

                if (IntakeTuberculosisEntity.Id == 0)
                {
                    IntakeTuberculosisEntity.Client = null;
                    _context.IntakeTuberculosis.Add(IntakeTuberculosisEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeTuberculosisEntity.Client = null;
                    _context.IntakeTuberculosis.Update(IntakeTuberculosisEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTuberculosis", IntakeViewModel) });
        }

        [Authorize(Roles = "Documents_Assistant, Supervisor, Documents_Assistant")]
        public IActionResult CreateMedicalhistory(int id = 0, int origin = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeMedicalHistoryViewModel model;
            ClientEntity client = _context.Clients
                                          .Include(n => n.LegalGuardian)
                                          .Include(n => n.Doctor)
                                          .Include(n => n.MedicationList)
                                          .FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Documents_Assistant") || User.IsInRole("Supervisor"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeMedicalHistoryEntity intakeMedicalHistory = _context.IntakeMedicalHistory
                                                                              .Include(n => n.Client)
                                                                              .ThenInclude(n => n.LegalGuardian)
                                                                              .Include(n => n.Client)
                                                                              .ThenInclude(n => n.Doctor)
                                                                              .Include(n => n.Client)
                                                                              .ThenInclude(n => n.MedicationList)
                                                                              .FirstOrDefault(n => n.Client.Id == id);

                    DoctorEntity doctor = _context.Clients.FirstOrDefault(n => n.Id == id).Doctor;
                    if (doctor == null)
                    {
                        doctor = new DoctorEntity();
                    }
                    if (intakeMedicalHistory == null)
                    {
                        model = new IntakeMedicalHistoryViewModel
                        {
                            Client = client,
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            DateSignatureEmployee = client.AdmisionDate,
                            DateSignatureLegalGuardian = client.AdmisionDate,
                            DateSignaturePerson = client.AdmisionDate,
                            Documents = true,

                            AddressPhysician = doctor.Address,
                            AgeFirstTalked = "",
                            AgeFirstWalked = "",
                            AgeToiletTrained = "",
                            AgeWeaned = "",
                            Allergies = false,
                            Allergies_Describe = "",
                            AndOrSoiling = false,
                            Anemia = false,
                            AreYouCurrently = false,
                            AreYouPhysician = false,
                            Arthritis = false,
                            AssumingCertainPositions = false,
                            BackPain = false,
                            BeingConfused = false,
                            BeingDisorientated = false,
                            BirthWeight = "",
                            BlackStools = false,
                            BloodInUrine = false,
                            BloodyStools = false,
                            BottleFedUntilAge = "",
                            BreastFed = false,
                            BurningUrine = false,
                            Calculating = false,
                            Cancer = false,
                            ChestPain = false,
                            ChronicCough = false,
                            ChronicIndigestion = false,
                            City = doctor.City,
                            Complications = false,
                            Complications_Explain = "",
                            Comprehending = false,
                            Concentrating = false,
                            Constipation = false,
                            ConvulsionsOrFits = false,
                            CoughingOfBlood = false,
                            DescriptionOfChild = "",
                            Diabetes = false,
                            Diphtheria = false,
                            DoYouSmoke = false,
                            DoYouSmoke_PackPerDay = "",
                            DoYouSmoke_Year = "",
                            EarInfections = false,
                            Epilepsy = false,
                            EyeTrouble = false,
                            Fainting = false,
                            FamilyAsthma = false,
                            FamilyAsthma_ = "",
                            FamilyCancer = false,
                            FamilyCancer_ = "",
                            FamilyDiabetes = false,
                            FamilyDiabetes_ = "",
                            FamilyEpilepsy = false,
                            FamilyEpilepsy_ = "",
                            FamilyGlaucoma = false,
                            FamilyGlaucoma_ = "",
                            FamilyHayFever = false,
                            FamilyHayFever_ = "",
                            FamilyHeartDisease = false,
                            FamilyHeartDisease_ = "",
                            FamilyHighBloodPressure = false,
                            FamilyHighBloodPressure_ = "",
                            FamilyKidneyDisease = false,
                            FamilyKidneyDisease_ = "",
                            FamilyNervousDisorders = false,
                            FamilyNervousDisorders_ = "",
                            FamilyOther = false,
                            FamilyOther_ = "",
                            FamilySyphilis = false,
                            FamilySyphilis_ = "",
                            FamilyTuberculosis = false,
                            FamilyTuberculosis_ = "",
                            FirstYearMedical = "",
                            Fractures = false,
                            FrequentColds = false,
                            FrequentHeadaches = false,
                            FrequentNoseBleeds = false,
                            FrequentSoreThroat = false,
                            FrequentVomiting = false,
                            HaveYouEverBeenPregnant = false,
                            HaveYouEverHadComplications = false,
                            HaveYouEverHadExcessive = false,
                            HaveYouEverHadPainful = false,
                            HaveYouEverHadSpotting = false,
                            HayFever = false,
                            HeadInjury = false,
                            Hearing = false,
                            HearingTrouble = false,
                            HeartPalpitation = false,
                            Hemorrhoids = false,
                            Hepatitis = false,
                            Hernia = false,
                            HighBloodPressure = false,
                            Hoarseness = false,
                            Immunizations = "",
                            InfectiousDisease = false,
                            Jaundice = false,
                            KidneyStones = false,
                            KidneyTrouble = false,
                            Length = "",
                            ListAllCurrentMedications = "",
                            LossOfMemory = false,
                            Mumps = false,
                            Nervousness = false,
                            NightSweats = false,
                            Normal = false,
                            PainfulJoints = false,
                            PainfulMuscles = false,
                            PainfulUrination = false,
                            PerformingCertainMotions = false,
                            Planned = false,
                            Poliomyelitis = false,
                            PrimaryCarePhysician = doctor.Name,
                            ProblemWithBedWetting = false,
                            Reading = false,
                            RheumaticFever = false,
                            Rheumatism = false,
                            ScarletFever = false,
                            Seeing = false,
                            SeriousInjury = false,
                            ShortnessOfBreath = false,
                            SkinTrouble = false,
                            Speaking = false,
                            State = doctor.State,
                            StomachPain = false,
                            Surgery = false,
                            SwellingOfFeet = false,
                            SwollenAnkles = false,
                            Tuberculosis = false,
                            Unplanned = false,
                            VaricoseVeins = false,
                            VenerealDisease = false,
                            VomitingOfBlood = false,
                            Walking = false,
                            WeightLoss = false,
                            WhoopingCough = false,
                            WritingSentence = false,
                            ZipCode = doctor.ZipCode,
                            AgeOfFirstMenstruation = "",
                            DateOfLastBreastExam = "",
                            DateOfLastPelvic = "",
                            DateOfLastPeriod = "",
                            UsualDurationOfPeriods = "",
                            UsualIntervalBetweenPeriods = "",
                            AdmissionedFor = user_logged.FullName,
                            IdDoctor = 0,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Today,
                            LastModifiedBy = string.Empty,
                            LastModifiedOn = null,
                            StartTime = DateTime.Now,
                            EndTime = DateTime.Now.AddMinutes(30)

                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        if (client.Doctor != null)
                        {
                            model.PrimaryCarePhysician = client.Doctor.Name;
                            model.AddressPhysician = client.Doctor.Address;
                            model.City = client.Doctor.City;
                            model.State = client.Doctor.State;
                            model.ZipCode = client.Doctor.ZipCode;
                            model.IdDoctor = client.Doctor.Id;
                        }
                        ViewData["origin"] = origin;
                        return View(model);
                    }
                    else
                    {
                        if (intakeMedicalHistory.Client.LegalGuardian == null)
                            intakeMedicalHistory.Client.LegalGuardian = new LegalGuardianEntity();
                       
                        model = _converterHelper.ToIntakeMedicalHistoryViewModel(intakeMedicalHistory);
                        ViewData["origin"] = origin;
                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Desktop");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Documents_Assistant, Supervisor, Documents_Assistant")]
        public async Task<IActionResult> CreateMedicalhistory(IntakeMedicalHistoryViewModel IntakeViewModel, int origin = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeMedicalHistoryEntity IntakeMedicalHistoryEntity;

                if (IntakeViewModel.Id == 0)
                {
                    DateTime start = new DateTime(IntakeViewModel.DateSignatureEmployee.Year, IntakeViewModel.DateSignatureEmployee.Month, IntakeViewModel.DateSignatureEmployee.Day, IntakeViewModel.StartTime.Hour, IntakeViewModel.StartTime.Minute, IntakeViewModel.StartTime.Second);
                    IntakeViewModel.StartTime = start;
                    DateTime end = new DateTime(IntakeViewModel.DateSignatureEmployee.Year, IntakeViewModel.DateSignatureEmployee.Month, IntakeViewModel.DateSignatureEmployee.Day, IntakeViewModel.EndTime.Hour, IntakeViewModel.EndTime.Minute, IntakeViewModel.EndTime.Second);
                    IntakeViewModel.EndTime = end;
                    IntakeMedicalHistoryEntity = _converterHelper.ToIntakeMedicalHistoryEntity(IntakeViewModel, true, user_logged.UserName);
                    IntakeMedicalHistoryEntity.Client = null;
                    _context.IntakeMedicalHistory.Add(IntakeMedicalHistoryEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (origin == 1)
                        {
                            return RedirectToAction("ClientswithoutMedicalHistory");
                        }
                        else
                        {
                            if (origin == 2)
                            {
                                return RedirectToAction("IndexDocumentsAssistant", "Calendar");
                            }
                            else
                            {
                                return RedirectToAction("MedicalHistory");
                            }
                        }
                        
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeMedicalHistoryEntity = _converterHelper.ToIntakeMedicalHistoryEntity(IntakeViewModel, false, user_logged.UserName);
                    IntakeMedicalHistoryEntity.Client = null;
                    _context.IntakeMedicalHistory.Update(IntakeMedicalHistoryEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (origin == 1)
                        {
                            return RedirectToAction("ClientswithoutMedicalHistory");
                        }
                        else
                        {
                            return RedirectToAction("MedicalHistory");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateMedicalhistory", IntakeViewModel) });
        }

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> IntakeDashboard(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Home/Error404");
            }

            ClientEntity clientEntity = await _context.Clients

                                                      .Include(c => c.IntakeConsentForTreatment)
                                                      .Include(c => c.IntakeAccessToServices)
                                                      .Include(c => c.IntakeAcknowledgementHipa)
                                                      .Include(c => c.IntakeConsentForRelease)
                                                      .Include(c => c.IntakeConsentPhotograph)
                                                      .Include(c => c.IntakeConsumerRights)
                                                      .Include(c => c.IntakeFeeAgreement)
                                                      .Include(c => c.IntakeOrientationChecklist)
                                                      .Include(c => c.IntakeScreening)
                                                      .Include(c => c.IntakeTransportation)
                                                      .Include(c => c.IntakeTuberculosis)
                                                      .Include(c => c.IntakeMedicalHistory)
                                                      .Include(c => c.Clinic)
                                                      .Include(n => n.IntakeConsentForTelehealth)
                                                      .Include(n => n.IntakeNoDuplicateService)
                                                      .Include(n => n.IntakeAdvancedDirective)
                                                      .Include(n => n.IntakeClientIdDocumentVerification)
                                                      .Include(n => n.IntakeForeignLanguage)
                                                      .Include(n => n.IntakeNoHarm)

                                                      .FirstOrDefaultAsync(c => c.Id == id);
            if (clientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }
            
            return View(clientEntity);                    
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, Frontdesk")]
        public async Task<IActionResult> ClientswithoutMedicalHistory(int idError = 0)
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (User.IsInRole("Facilitator"))
            {
                FacilitatorEntity facilitator = await _context.Facilitators.FirstOrDefaultAsync(n => n.LinkedUser == user_logged.UserName);
                List<ClientEntity> ClientList = await _context.Clients
                                                              .Include(n => n.IntakeMedicalHistory)
                                                              .Where(n => n.IntakeMedicalHistory == null
                                                                       && n.Clinic.Id == user_logged.Clinic.Id
                                                                       && n.OnlyTCM == false
                                                                       && (n.IdFacilitatorPSR == facilitator.Id
                                                                        || n.IdFacilitatorGroup == facilitator.Id
                                                                        || n.IndividualTherapyFacilitator.Id == facilitator.Id))
                                                              .ToListAsync();

                return View(ClientList);
            }
            else
            {
                if (User.IsInRole("Documents_Assistant"))
                {
                    List<ClientEntity> ClientList = await _context.Clients
                                                                  .Include(n => n.IntakeMedicalHistory)
                                                                  .Where(n => n.IntakeMedicalHistory == null
                                                                           && n.Clinic.Id == user_logged.Clinic.Id
                                                                           && n.OnlyTCM == false
                                                                           && ((n.DocumentsAssistant != null && n.DocumentsAssistant.LinkedUser == user_logged.UserName)
                                                                             || n.DocumentsAssistant == null))
                                                                  .ToListAsync();
                    return View(ClientList);
                }
                else
                {
                    List<ClientEntity> ClientList = await _context.Clients
                                                                  .Include(n => n.IntakeMedicalHistory)
                                                                  .Where(n => n.IntakeMedicalHistory == null
                                                                           && n.Clinic.Id == user_logged.Clinic.Id
                                                                           && n.OnlyTCM == false)
                                                                  .ToListAsync();
                    return View(ClientList);
                }
            
            }
           

        }

        [Authorize(Roles = "Manager, Supervisor, Documents_Assistant, Frontdesk")]
        public async Task<IActionResult> MedicalHistory(int idError = 0)
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
                if(User.IsInRole("Documents_Assistant"))
                {
                    return View(await _context.Clients
                                          .Include(n => n.IntakeMedicalHistory)
                                          .Include(n => n.Clinic)
                                          .ThenInclude(n => n.Setting)
                                          .Where(n => n.Clinic.Id == user_logged.Clinic.Id
                                            && n.IntakeMedicalHistory.AdmissionedFor == user_logged.FullName)
                                          .ToListAsync());
                }
                else
                    return View(await _context.Clients
                                          .Include(n => n.IntakeMedicalHistory)
                                          .Include(n => n.Clinic)
                                          .ThenInclude(n => n.Setting)
                                          .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                          .ToListAsync());

            }
        }

        [Authorize(Roles = "Manager, Supervisor, Documents_Assistant, Facilitator, Frontdesk")]
        public async Task<IActionResult> IntakeDashboardPrint(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Home/Error404");
            }

            ClientEntity clientEntity = await _context.Clients

                                                      .Include(c => c.IntakeConsentForTreatment)
                                                      .Include(c => c.IntakeAccessToServices)
                                                      .Include(c => c.IntakeAcknowledgementHipa)
                                                      .Include(c => c.IntakeConsentForRelease)
                                                      .Include(c => c.IntakeConsentPhotograph)
                                                      .Include(c => c.IntakeConsumerRights)
                                                      .Include(c => c.IntakeFeeAgreement)
                                                      .Include(c => c.IntakeOrientationChecklist)
                                                      .Include(c => c.IntakeScreening)
                                                      .Include(c => c.IntakeTransportation)
                                                      .Include(c => c.IntakeTuberculosis)
                                                      .Include(c => c.IntakeMedicalHistory)
                                                      .Include(c => c.Clinic)
                                                      .Include(c => c.IntakeNoHarm)

                                                      .FirstOrDefaultAsync(c => c.Id == id);
            if (clientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            return View(clientEntity);
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> AuditIntake()
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic || !user_logged.Clinic.Setting.MHProblems)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<AuditIntake> auditClient_List = new List<AuditIntake>();
            AuditIntake auditClient = new AuditIntake();

            List<ClientEntity> client_List = new List<ClientEntity>();

            if (!User.IsInRole("Facilitator"))
            {
                client_List = _context.Clients
                                      .Include(m => m.IntakeAccessToServices)
                                      .Include(m => m.IntakeAcknowledgementHipa)
                                      .Include(m => m.IntakeConsentForRelease)
                                      .Include(m => m.IntakeConsentForTreatment)
                                      .Include(m => m.IntakeConsentPhotograph)
                                      .Include(m => m.IntakeConsumerRights)
                                      .Include(m => m.IntakeFeeAgreement)
                                      .Include(m => m.IntakeMedicalHistory)
                                      .Include(m => m.IntakeOrientationChecklist)
                                      .Include(m => m.IntakeScreening)
                                      .Include(m => m.IntakeTransportation)
                                      .Include(m => m.IntakeTuberculosis)
                                      .Where(n => (n.Clinic.Id == user_logged.Clinic.Id))
                                      .ToList();
            }
            else
            {
                FacilitatorEntity facilitator = await _context.Facilitators.FirstOrDefaultAsync(f => f.LinkedUser == user_logged.UserName);
                client_List = _context.Clients
                                       .Include(m => m.IntakeAccessToServices)
                                       .Include(m => m.IntakeAcknowledgementHipa)
                                       .Include(m => m.IntakeConsentForRelease)
                                       .Include(m => m.IntakeConsentForTreatment)
                                       .Include(m => m.IntakeConsentPhotograph)
                                       .Include(m => m.IntakeConsumerRights)
                                       .Include(m => m.IntakeFeeAgreement)
                                       .Include(m => m.IntakeMedicalHistory)
                                       .Include(m => m.IntakeOrientationChecklist)
                                       .Include(m => m.IntakeScreening)
                                       .Include(m => m.IntakeTransportation)
                                       .Include(m => m.IntakeTuberculosis)
                                       .Where(n => (n.Clinic.Id == user_logged.Clinic.Id
                                        && (n.IdFacilitatorPSR == facilitator.Id || n.IndividualTherapyFacilitator.Id == facilitator.Id || n.IdFacilitatorGroup == facilitator.Id)))
                                       .ToList();
            }
           

           

            foreach (var item in client_List.OrderBy(n => n.AdmisionDate))
            {
                if (item.IntakeAccessToServices == null)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "Missing Intake Access To Services";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditIntake();
                }
                if (item.IntakeAcknowledgementHipa == null)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "Missing Intake Acknowledgement Hipa";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditIntake();
                }
                if (item.IntakeConsentForRelease == null)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "Missing Intake Consent For Release";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditIntake();
                }
                if (item.IntakeConsentForTreatment == null)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "Missing Intake Consent For Treatment";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditIntake();
                }
                if (item.IntakeConsentPhotograph == null)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "Missing Intake Consent Photograph";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditIntake();
                }
                if (item.IntakeConsumerRights == null)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "Missing Intake Consumer Rights";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditIntake();
                }
                if (item.IntakeFeeAgreement == null)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "Missing Intake Fee Agreement";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditIntake();
                }
                if (item.IntakeMedicalHistory == null)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "Missing Intake Medical History";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditIntake();
                }
                if (item.IntakeOrientationChecklist == null)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "Missing Intake Orientation Checklist";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditIntake();
                }
                if (item.IntakeScreening == null)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "Missing Intake Screening";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditIntake();
                }
                if (item.IntakeTransportation == null)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "Missing Intake Transportation";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditIntake();
                }
                if (item.IntakeTuberculosis == null)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "Missing Intake Tuberculosis";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditIntake();
                }
            }

            return View(auditClient_List);
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> ClientswithoutIntake(int idError = 0)
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (User.IsInRole("Documents_Assistant"))
            {
                List<ClientEntity> ClientList = await _context.Clients
                                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id
                                                                       && n.IntakeScreening == null
                                                                       && n.IntakeConsentForTreatment == null
                                                                       && n.IntakeConsentForRelease == null
                                                                       && n.IntakeConsumerRights == null
                                                                       && n.IntakeAcknowledgementHipa == null
                                                                       && n.IntakeAccessToServices == null
                                                                       && n.IntakeOrientationChecklist == null
                                                                       && n.IntakeTransportation == null
                                                                       && n.IntakeConsentPhotograph == null
                                                                       && n.IntakeFeeAgreement == null
                                                                       && n.IntakeTuberculosis == null
                                                                       && n.OnlyTCM == false
                                                                       && ((n.DocumentsAssistant != null && n.DocumentsAssistant.LinkedUser == user_logged.UserName)
                                                                         || n.DocumentsAssistant == null))
                                                             .ToListAsync();

                return View(ClientList);

            }
            else
            {
                List<ClientEntity> ClientList = await _context.Clients
                                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id
                                                                       && n.IntakeScreening == null
                                                                       && n.IntakeConsentForTreatment == null
                                                                       && n.IntakeConsentForRelease == null
                                                                       && n.IntakeConsumerRights == null
                                                                       && n.IntakeAcknowledgementHipa == null
                                                                       && n.IntakeAccessToServices == null
                                                                       && n.IntakeOrientationChecklist == null
                                                                       && n.IntakeTransportation == null
                                                                       && n.IntakeConsentPhotograph == null
                                                                       && n.IntakeFeeAgreement == null
                                                                       && n.IntakeTuberculosis == null
                                                                       && n.OnlyTCM == false)
                                                              .ToListAsync();

                return View(ClientList);
            }
            
            

        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult CreateMedicationModal(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            MedicationViewModel model;

            if (User.IsInRole("Documents_Assistant") || User.IsInRole("Supervisor"))
            {


                if (user_logged.Clinic != null)
                {

                    model = new MedicationViewModel
                    {
                        IdClient = id,
                        Client = _context.Clients.Include(n => n.MedicationList).FirstOrDefault(n => n.Id == id),
                        Id = 0,
                        Dosage = "",
                        Frequency = "",
                        Name = "",
                        Prescriber = ""

                    };
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    return View(model);
                }
            }

            model = new MedicationViewModel
            {
                IdClient = id,
                Client = _context.Clients.Include(n => n.MedicationList).FirstOrDefault(n => n.Id == id),
                Id = 0,
                Dosage = "",
                Frequency = "",
                Name = "",
                Prescriber = ""
            };
            if (model.Client.MedicationList == null)
                model.Client.MedicationList = new List<MedicationEntity>();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> CreateMedicationModal(MedicationViewModel medicationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {

                MedicationEntity medicationEntity = await _converterHelper.ToMedicationEntity(medicationViewModel, true);
                _context.Medication.Add(medicationEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    List<MedicationEntity> medication = await _context.Medication
                                                                      .Include(g => g.Client)
                                                                      .Where(g => g.Client.Id == medicationViewModel.IdClient)
                                                                      .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewMedication", medication) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }
            MedicationViewModel model;
            model = new MedicationViewModel
            {
                IdClient = medicationViewModel.IdClient,
                Client = _context.Clients.Find(medicationViewModel.IdClient),
                Id = medicationViewModel.Id,
                Dosage = medicationViewModel.Dosage,
                Frequency = medicationViewModel.Frequency,
                Name = medicationViewModel.Name,
                Prescriber = medicationViewModel.Prescriber

            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateMedicationModal", medicationViewModel) });
        }

        public IActionResult AuditMedicalHistory()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic || !user_logged.Clinic.Setting.MHProblems)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<AuditMedicalHistory> auditClient_List = new List<AuditMedicalHistory>();
            AuditMedicalHistory auditClient = new AuditMedicalHistory();

            List<ClientEntity> client_List = _context.Clients
                                                     .Include(m => m.Doctor)
                                                     .Include(m => m.IntakeMedicalHistory)
                                                     .Include(m => m.MedicationList)
                                                     .Where(n => (n.Clinic.Id == user_logged.Clinic.Id))
                                                     .ToList();

            foreach (var item in client_List)
            {
                if (item.IntakeMedicalHistory == null)
                {
                    auditClient.Name = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "The client has no Medical History";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditMedicalHistory();
                }
                else
                {
                    if (item.Doctor == null)
                    {
                        auditClient.Name = item.Name;
                        auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                        auditClient.Description = "The client has no Doctor";
                        auditClient.Active = 1;

                        auditClient_List.Add(auditClient);
                        auditClient = new AuditMedicalHistory();
                    }
                    if (item.MedicationList.Count() == 0)
                    {
                        auditClient.Name = item.Name;
                        auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                        auditClient.Description = "The client has no Medication list";
                        auditClient.Active = 1;

                        auditClient_List.Add(auditClient);
                        auditClient = new AuditMedicalHistory();
                    }
                }
               
            }

            return View(auditClient_List);
        }

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult CreateConsentForTelehealth(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeConsentForTelehealthViewModel model;
            ClientEntity client = _context.Clients.Include(d => d.LegalGuardian).FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeConsentForTelehealthEntity intakeConsent = _context.IntakeConsentForTelehealth
                                                                             .Include(n => n.Client)
                                                                             .ThenInclude(n => n.LegalGuardian)
                                                                             .FirstOrDefault(n => n.Client.Id == id);

                    if (intakeConsent == null)
                    {
                        model = new IntakeConsentForTelehealthViewModel
                        {
                            Client = client,
                            DateSignatureEmployee = client.AdmisionDate,
                            DateSignatureLegalGuardian = client.AdmisionDate,
                            DateSignaturePerson = client.AdmisionDate,
                            Id = 0,
                            IdClient = id,
                            Client_FK = id,
                            AdmissionedFor = user_logged.FullName,
                            IConsentToReceive = true,
                            Documents = true

                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeConsent.Client.LegalGuardian == null)
                            intakeConsent.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToIntakeConsentForTelehealthViewModel(intakeConsent);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> CreateConsentForTelehealth(IntakeConsentForTelehealthViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeConsentForTelehealthEntity IntakeConsentEntity = _converterHelper.ToIntakeConsentForTelehealthEntity(IntakeViewModel, false);

                if (IntakeConsentEntity.Id == 0)
                {
                    IntakeConsentEntity.Client = null;
                    _context.IntakeConsentForTelehealth.Add(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeConsentEntity.Client = null;
                    _context.IntakeConsentForTelehealth.Update(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateConsentForTelehealth", IntakeViewModel) });
        }

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult CreateNoDuplicateService(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeNoDuplicateServiceViewModel model;
            ClientEntity client = _context.Clients.Include(d => d.LegalGuardian).FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeNoDuplicateServiceEntity intakeConsent = _context.IntakeNoDuplicateService
                                                                           .Include(n => n.Client)
                                                                           .ThenInclude(n => n.LegalGuardian)
                                                                           .FirstOrDefault(n => n.Client.Id == id);

                    if (intakeConsent == null)
                    {
                        model = new IntakeNoDuplicateServiceViewModel
                        {
                            Client = client,
                            DateSignatureEmployee = client.AdmisionDate,
                            DateSignatureLegalGuardian = client.AdmisionDate,
                            DateSignaturePerson = client.AdmisionDate,
                            Id = 0,
                            IdClient = id,
                            Client_FK = id,
                            AdmissionedFor = user_logged.FullName,
                            Documents = true
                         
                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeConsent.Client.LegalGuardian == null)
                            intakeConsent.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToIntakeNoDuplicateServiceViewModel(intakeConsent);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> CreateNoDuplicateService(IntakeNoDuplicateServiceViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeNoDuplicateServiceEntity IntakeConsentEntity = _converterHelper.ToIntakeNoDuplicateServiceEntity(IntakeViewModel, false);

                if (IntakeConsentEntity.Id == 0)
                {
                    IntakeConsentEntity.Client = null;
                    _context.IntakeNoDuplicateService.Add(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeConsentEntity.Client = null;
                    _context.IntakeNoDuplicateService.Update(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoDuplicateService", IntakeViewModel) });
        }

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult CreateIntakeAdvenceDirective(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeAdvancedDirectiveViewModel model;
            IntakeAdvancedDirectiveEntity AdvancedDirective = _context.IntakeAdvancedDirective
                                                                       .Include(n => n.Client)
                                                                       .ThenInclude(n => n.LegalGuardian)
                                                                       .Include(n => n.Client.EmergencyContact)
                                                                       .FirstOrDefault(n => n.Client.Id == id);

            ClientEntity client = _context.Clients.Include(d => d.LegalGuardian).FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Frontdesk") || User.IsInRole("Documents_Assistant") || User.IsInRole("Manager"))
            {
                if (user_logged.Clinic != null)
                {
                    if (AdvancedDirective == null)
                    {
                        model = new IntakeAdvancedDirectiveViewModel
                        {
                            Client = _context.Clients
                                             .Include(n => n.LegalGuardian)
                                             .Include(n => n.EmergencyContact)
                                             .FirstOrDefault(n => n.Id == id),
                            IdClient = id,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            Client_FK = id,
                            Id = 0,
                            Documents = true,
                            DateSignatureEmployee = client.AdmisionDate,
                            DateSignatureLegalGuardian = client.AdmisionDate,
                            DateSignaturePerson = client.AdmisionDate,
                            AdmissionedFor = user_logged.FullName,
                            IHave = true

                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        if (model.Client.EmergencyContact == null)
                        {
                            model.Client.EmergencyContact = new EmergencyContactEntity();
                            model.Client.EmergencyContact.Name = "N/A";
                        }
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        if (AdvancedDirective.Client.LegalGuardian == null)
                            AdvancedDirective.Client.LegalGuardian = new LegalGuardianEntity();
                        if (AdvancedDirective.Client.EmergencyContact == null)
                        {
                            AdvancedDirective.Client.EmergencyContact = new EmergencyContactEntity();
                            AdvancedDirective.Client.EmergencyContact.Name = "N/A";
                        }
                        model = _converterHelper.ToIntakeAdvancedDirectiveViewModel(AdvancedDirective);
                        ViewData["origi"] = origi;
                        return View(model);
                    }

                }
            }
           
            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> CreateIntakeAdvenceDirective(IntakeAdvancedDirectiveViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                           .Include(u => u.Clinic)
                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeAdvancedDirectiveEntity IntakeConsentEntity = _converterHelper.ToIntakeAdvancedDirectiveEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeConsentEntity.Id == 0)
                {
                    IntakeConsentEntity.Client = null;
                    _context.IntakeAdvancedDirective.Add(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeConsentEntity.Client = null;
                    _context.IntakeAdvancedDirective.Update(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateIntakeAdvenceDirective", IntakeViewModel) });
        }

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult CreateIntakeClientIdDocumentVerification(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeClientIdDocumentVerificationViewModel model;
            IntakeClientIdDocumentVerificationEntity clientIdDocumentationverification = _context.IntakeClientDocumentVerification
                                                                                                 .Include(n => n.Client)
                                                                                                 .ThenInclude(n => n.LegalGuardian)
                                                                                                 .FirstOrDefault(n => n.Client.Id == id);

            ClientEntity client = _context.Clients
                                          .Include(d => d.LegalGuardian)
                                          .Include(d => d.Clients_HealthInsurances)
                                          .ThenInclude(d => d.HealthInsurance)
                                          .FirstOrDefault(n => n.Id == id);

            if (user_logged.Clinic != null)
            {
                if (clientIdDocumentationverification == null)
                {
                    model = new IntakeClientIdDocumentVerificationViewModel
                    {
                        Client = _context.Clients
                                         .Include(n => n.LegalGuardian)
                                         .Include(n => n.EmergencyContact)
                                         .FirstOrDefault(n => n.Id == id),
                        IdClient = id,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        Client_FK = id,
                        Id = 0,
                        AdmissionedFor = user_logged.FullName,
                        DateSignatureEmployee = client.AdmisionDate,
                        DateSignatureLegalGuardianOrClient = client.AdmisionDate,
                        HealthPlan = (client.Clients_HealthInsurances.FirstOrDefault(n => n.Active == true) != null) ? client.Clients_HealthInsurances.FirstOrDefault(n => n.Active == true).HealthInsurance.Name : string.Empty,
                        Id_DriverLicense = string.Empty,
                        MedicaidId = client.MedicaidID,
                        MedicareCard = client.MedicareId,
                        Other_Identification = string.Empty,
                        Other_Name = string.Empty,
                        Passport_Resident = string.Empty,
                        Social = client.SSN

                    };
                    if (model.Client.LegalGuardian == null)
                        model.Client.LegalGuardian = new LegalGuardianEntity();

                    ViewData["origi"] = origi;
                    return View(model);
                }
                else
                {
                    if (clientIdDocumentationverification.Client.LegalGuardian == null)
                        clientIdDocumentationverification.Client.LegalGuardian = new LegalGuardianEntity();

                    model = _converterHelper.ToIntakeClientIdDocumentVerificationViewModel(clientIdDocumentationverification);
                    ViewData["origi"] = origi;

                    return View(model);
                }

            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> CreateIntakeClientIdDocumentVerification(IntakeClientIdDocumentVerificationViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeClientIdDocumentVerificationEntity clientIdDocumentVerificationEntity = _converterHelper.ToIntakeClientIdDocumentVerificationEntity(IntakeViewModel, false, user_logged.UserName);

                if (clientIdDocumentVerificationEntity.Id == 0)
                {
                    clientIdDocumentVerificationEntity.Client = null;
                    _context.IntakeClientDocumentVerification.Add(clientIdDocumentVerificationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    clientIdDocumentVerificationEntity.Client = null;
                    _context.IntakeClientDocumentVerification.Update(clientIdDocumentVerificationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateIntakeClientIdDocumentVerification", IntakeViewModel) });
        }

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult CreateForeignLanguage(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeForeignLanguageViewModel model;
            IntakeForeignLanguageEntity intakeForeign = _context.IntakeForeignLanguage
                                                                .Include(n => n.Client)
                                                                .ThenInclude(n => n.LegalGuardian)
                                                                .FirstOrDefault(n => n.Client.Id == id);
            ClientEntity client = _context.Clients.Include(d => d.LegalGuardian).FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {
                    if (intakeForeign == null)
                    {

                        model = new IntakeForeignLanguageViewModel
                        {
                            Client = _context.Clients
                                             .Include(d => d.LegalGuardian)
                                             .FirstOrDefault(n => n.Id == id),
                            DateSignatureEmployee = client.AdmisionDate,
                            DateSignatureLegalGuardian = client.AdmisionDate,
                            DateSignaturePerson = client.AdmisionDate,
                            Documents = true,
                            Id = 0,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            IdClient = id,
                            Client_FK = id,
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        if (intakeForeign.Client.LegalGuardian == null)
                            intakeForeign.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToIntakeForeignLanguageViewModel(intakeForeign);
                        ViewData["origi"] = origi;
                        return View(model);
                    }

                }
            }
           
            return RedirectToAction("Index", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> CreateForeignLanguage(IntakeForeignLanguageViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeForeignLanguageEntity IntakeForeignEntity = _converterHelper.ToIntakeForeignLanguageEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeForeignEntity.Id == 0)
                {
                    IntakeForeignEntity.Client = null;
                    _context.IntakeForeignLanguage.Add(IntakeForeignEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeForeignEntity.Client = null;
                    _context.IntakeForeignLanguage.Update(IntakeForeignEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateForeignLanguage", IntakeViewModel) });
        }

        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public IActionResult CreateIntakeNoHarm(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeNoHarmViewModel model;
            ClientEntity client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeNoHarmEntity intakeNoHarm = _context.IntakeNoHarm
                                                              .FirstOrDefault(n => n.Client.Id == id);
                    if (intakeNoHarm == null)
                    {
                        model = new IntakeNoHarmViewModel
                        {
                            Client = client,
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            Documents = true,
                            DateSignatureEmployee = client.AdmisionDate,
                            DateSignaturePerson = client.AdmisionDate,
                            AdmissionedFor = user_logged.FullName,

                        };

                      
                        return View(model);
                    }
                    else
                    {
                        model = _converterHelper.ToIntakeNoHarmViewModel(intakeNoHarm);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> CreateIntakeNoHarm(IntakeNoHarmViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeNoHarmEntity IntakeNoHarmEntity = _converterHelper.ToIntakeNoHarmEntity(IntakeViewModel, false);

                if (IntakeNoHarmEntity.Id == 0)
                {
                    IntakeNoHarmEntity.Client = null;
                    _context.IntakeNoHarm.Add(IntakeNoHarmEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeNoHarmEntity.Client = null;
                    _context.IntakeNoHarm.Update(IntakeNoHarmEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("IntakeDashboard", new { id = IntakeViewModel.IdClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateIntakeNoHarm", IntakeViewModel) });
        }

    }
}
