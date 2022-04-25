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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Mannager, Supervisor, Facilitator")]
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

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                              .ToListAsync());                
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Mannager")]
        public IActionResult Create(int id = 0)
        {
            
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeScreeningViewModel model;

            if (User.IsInRole("Mannager"))
            {
                if (user_logged.Clinic != null)
                {
                    
                    model = new IntakeScreeningViewModel
                    {
                        IdClient = id,
                        Client_FK = id,
                        Client = _context.Clients.Include(n => n.LegalGuardian).Include(n => n.EmergencyContact).FirstOrDefault(n => n.Id == id),
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
                DateSignatureClient = DateTime.Now,
                DateSignatureWitness = DateTime.Now,
                DateSignatureEmployee = DateTime.Now,
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
        [Authorize(Roles = "Mannager")]
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

        [Authorize(Roles = "Mannager")]
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

            if (User.IsInRole("Mannager"))
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
        [Authorize(Roles = "Mannager")]
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

        [Authorize(Roles = "Mannager")]
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

        [Authorize(Roles = "Mannager")]
        public IActionResult CreateConsentForTreatment(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeConsentForTreatmentViewModel model;

            if (User.IsInRole("Mannager"))
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
                            Client = _context.Clients.Include(d => d.LegalGuardian).FirstOrDefault(n => n.Id == id),
                            Aggre = true,
                            Aggre1 = true,
                            AuthorizeRelease = true,
                            AuthorizeStaff = true,
                            Certify = false,
                            Certify1 = true,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
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
        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> CreateConsentForTreatment(IntakeConsentForTreatmentViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeConsentForTreatmentEntity IntakeConsentEntity = await _converterHelper.ToIntakeConsentForTreatmentEntity(IntakeViewModel, false);
                
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


        [Authorize(Roles = "Mannager")]
        public IActionResult CreateConsentForRelease(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeConsentForReleaseViewModel model;

            if (User.IsInRole("Mannager"))
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
                            Client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id),
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
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
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
        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> CreateConsentForRelease(IntakeConsentForReleaseViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeConsentForReleaseEntity IntakeConsentEntity = await _converterHelper.ToIntakeConsentForReleaseEntity(IntakeViewModel, false);

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


        [Authorize(Roles = "Mannager")]
        public IActionResult CreateConsumerRights(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeConsumerRightsViewModel model;

            if (User.IsInRole("Mannager"))
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
                            Client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id),
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            ServedOf = user_logged.FullName,
                            Documents = true,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
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
        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> CreateConsumerRights(IntakeConsumerRightsViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeConsumerRightsEntity IntakeConsumerEntity = await _converterHelper.ToIntakeConsumerRightsEntity(IntakeViewModel, false);

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
                                                   .ThenInclude(c => c.IntakeScreening)

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

                                                   .FirstOrDefault(i => (i.Id == id));
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
                Stream stream = _reportHelper.FloridaSocialHSIntakeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }            

            return null;
        }

        [Authorize(Roles = "Mannager")]
        public IActionResult CreateAcknowledgementHippa(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeAcknoewledgementHippaViewModel model;

            if (User.IsInRole("Mannager"))
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
                            Client = _context.Clients.Include(d => d.LegalGuardian).FirstOrDefault(n => n.Id == id),
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
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
        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> CreateAcknowledgementHippa(IntakeAcknoewledgementHippaViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeAcknowledgementHippaEntity IntakeAckNowEntity = await _converterHelper.ToIntakeAcknoewledgementHippaEntity(IntakeViewModel, false);

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

        [Authorize(Roles = "Mannager")]
        public IActionResult CreateAccessToServices(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeAccessToServicesViewModel model;

            if (User.IsInRole("Mannager"))
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
                            Client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id),
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            Documents = true,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
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
        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> CreateAccessToServices(IntakeAccessToServicesViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeAccessToServicesEntity IntakeAccessEntity = await _converterHelper.ToIntakeAccessToServicesEntity(IntakeViewModel, false);

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

        [Authorize(Roles = "Mannager")]
        public IActionResult CreateOrientationCheckList(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeOrientationCheckListViewModel model;

            if (User.IsInRole("Mannager"))
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
                            Client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id),
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
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
        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> CreateOrientationCheckList(IntakeOrientationCheckListViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeOrientationChecklistEntity IntakeOrientationEntity = await _converterHelper.ToIntakeOrientationChecklistEntity(IntakeViewModel, false);

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

        [Authorize(Roles = "Mannager")]
        public IActionResult CreateTransportation(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeTransportationViewModel model;

            if (User.IsInRole("Mannager"))
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
                            Client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id),
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            Documents = true,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
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
        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> CreateTransportation(IntakeTransportationViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeTransportationEntity IntakeTransportationEntity = await _converterHelper.ToIntakeTransportationEntity(IntakeViewModel, false);

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


        [Authorize(Roles = "Mannager")]
        public IActionResult CreateConsentPhotograph(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeConsentPhotographViewModel model;

            if (User.IsInRole("Mannager"))
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
                            Client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id),
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
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
        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> CreateConsentPhotograph(IntakeConsentPhotographViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeConsentPhotographEntity IntakeConsentPhotographEntity = await _converterHelper.ToIntakeConsentPhotographEntity(IntakeViewModel, false);

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

        [Authorize(Roles = "Mannager")]
        public IActionResult CreateFeeAgreement(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeFeeAgreementViewModel model;

            if (User.IsInRole("Mannager"))
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
                            Client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id),
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            Documents = true,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
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
        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> CreateFeeAgreement(IntakeFeeAgreementViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeFeeAgreementEntity IntakefeeAgreementEntity = await _converterHelper.ToIntakeFeeAgreementEntity(IntakeViewModel, false);

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

        [Authorize(Roles = "Mannager")]
        public IActionResult CreateTuberculosis(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeTuberculosisViewModel model;

            if (User.IsInRole("Mannager"))
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
                            Client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id),
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,

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
        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> CreateTuberculosis(IntakeTuberculosisViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeTuberculosisEntity IntakeTuberculosisEntity = await _converterHelper.ToIntakeTuberculosisEntity(IntakeViewModel, false);

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

        [Authorize(Roles = "Mannager")]
        public IActionResult CreateMedicalhistory(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeMedicalHistoryViewModel model;

            if (User.IsInRole("Mannager"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeMedicalHistoryEntity intakeMedicalHistory = _context.IntakeMedicalHistory
                                                                            .Include(n => n.Client)
                                                                            .ThenInclude(n => n.LegalGuardian)
                                                                            .FirstOrDefault(n => n.Client.Id == id);
                    if (intakeMedicalHistory == null)
                    {
                        model = new IntakeMedicalHistoryViewModel
                        {
                            Client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id),
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
                            Documents = true,

                            AddressPhysician = "",
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
                            City = "",
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
                            PrimaryCarePhysician = "",
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
                            State = "",
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
                            ZipCode = "",
                            AgeOfFirstMenstruation = "",
                            DateOfLastBreastExam = DateTime.Now,
                            DateOfLastPelvic = DateTime.Now,
                            DateOfLastPeriod = DateTime.Now,
                            UsualDurationOfPeriods = "",
                            UsualIntervalBetweenPeriods = "",
                            AdmissionedFor = user_logged.FullName,

                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeMedicalHistory.Client.LegalGuardian == null)
                            intakeMedicalHistory.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToIntakeMedicalHistoryViewModel(intakeMedicalHistory);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> CreateMedicalhistory(IntakeMedicalHistoryViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeMedicalHistoryEntity IntakeMedicalHistoryEntity = await _converterHelper.ToIntakeMedicalHistoryEntity(IntakeViewModel, false);

                if (IntakeMedicalHistoryEntity.Id == 0)
                {
                    IntakeMedicalHistoryEntity.Client = null;
                    _context.IntakeMedicalHistory.Add(IntakeMedicalHistoryEntity);
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
                    IntakeMedicalHistoryEntity.Client = null;
                    _context.IntakeMedicalHistory.Update(IntakeMedicalHistoryEntity);
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

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateMedicalhistory", IntakeViewModel) });
        }

        [Authorize(Roles = "Mannager")]
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

                                                      .FirstOrDefaultAsync(c => c.Id == id);
            if (clientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }
            
            return View(clientEntity);                    
        }
    }
}
