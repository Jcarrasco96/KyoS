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

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            else
            {
                if (User.IsInRole("Mannager"))
                    return View(await _context.IntakeScreenings
                                              .Include(f => f.Client)
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

                if (User.IsInRole("Facilitator"))
                {
                    


                    return View(await _context.IntakeScreenings
                                              .Include(f => f.Client)
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

            IntakeScreeningViewModel model;

            if (User.IsInRole("Mannager"))
            {
                

                if (user_logged.Clinic != null)
                {
                    
                    model = new IntakeScreeningViewModel
                    {
                        IdClient = id,
                        Client = _context.Clients.Find(id),
                        InformationGatheredBy = user_logged.FullName,
                        DateAdmision = DateTime.Now,
                        ClientIsStatus = IntakeClientIsStatus.Clean,
                        BehaviorIsStatus = IntakeBehaviorIsStatus.Normal,
                        SpeechIsStatus = IntakeSpeechIsStatus.Normal,
                        DoesClientKnowHisName = true,
                        DoesClientKnowTodayDate = true,
                        DoesClientKnowWhereIs = true,
                        DoesClientKnowTimeOfDay = true,
                        DateSignatureClient = DateTime.Now,
                        DateSignatureWitness = DateTime.Now,
                        IdClientIs = 1,
                        ClientIs_Status = _combosHelper.GetComboIntake_ClientIs(),
                        IdBehaviorIs = 1,
                        BehaviorIs_Status = _combosHelper.GetComboIntake_BehaviorIs(),
                        IdSpeechIs = 1,
                        SpeechIs_Status = _combosHelper.GetComboIntake_SpeechIs(),
                        

                    };
                    return View(model);
                }
            }

            model = new IntakeScreeningViewModel
            {
                IdClient = id,
                Client = _context.Clients.Find(id),
                InformationGatheredBy = user_logged.FullName,
                DateAdmision = DateTime.Now,
                ClientIsStatus = IntakeClientIsStatus.Clean,
                BehaviorIsStatus = IntakeBehaviorIsStatus.Normal,
                SpeechIsStatus = IntakeSpeechIsStatus.Normal,
                DoesClientKnowHisName = true,
                DoesClientKnowTodayDate = true,
                DoesClientKnowWhereIs = true,
                DoesClientKnowTimeOfDay = true,
                DateSignatureClient = DateTime.Now,
                DateSignatureWitness = DateTime.Now,
                IdClientIs = 1,
                ClientIs_Status = _combosHelper.GetComboIntake_ClientIs(),
                IdBehaviorIs = 1,
                BehaviorIs_Status = _combosHelper.GetComboIntake_BehaviorIs(),
                IdSpeechIs = 1,
                SpeechIs_Status = _combosHelper.GetComboIntake_SpeechIs(),
            };
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
                       
                        return RedirectToAction("Index", "Intakes");
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
                Client = _context.Clients.Find(IntakeViewModel.IdClient),
                InformationGatheredBy = user_logged.FullName,
                DateAdmision = DateTime.Now,
                ClientIsStatus = IntakeClientIsStatus.Clean,
                BehaviorIsStatus = IntakeBehaviorIsStatus.Normal,
                SpeechIsStatus = IntakeSpeechIsStatus.Normal,
                DoesClientKnowHisName = true,
                DoesClientKnowTodayDate = true,
                DoesClientKnowWhereIs = true,
                DoesClientKnowTimeOfDay = true,
                DateSignatureClient = DateTime.Now,
                DateSignatureWitness = DateTime.Now,
                IdClientIs = IntakeViewModel.IdClientIs,
                ClientIs_Status = _combosHelper.GetComboIntake_ClientIs(),
                IdBehaviorIs = IntakeViewModel.IdBehaviorIs,
                BehaviorIs_Status = _combosHelper.GetComboIntake_BehaviorIs(),
                IdSpeechIs = IntakeViewModel.IdSpeechIs,
                SpeechIs_Status = _combosHelper.GetComboIntake_SpeechIs(),
            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", IntakeViewModel) });
        }

        [Authorize(Roles = "Mannager")]
        public IActionResult Edit(int id = 0)
        {
            IntakeScreeningViewModel model;

            if (User.IsInRole("Mannager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    IntakeScreeningEntity Intake = _context.IntakeScreenings
                                                                 .Include(m => m.Client)
                                                                 .FirstOrDefault(m => m.Id == id);
                    if (Intake == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToIntakeViewModel(Intake);

                        return View(model);
                    }

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

                    return RedirectToAction("Index", "Intakes");
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", intakeViewModel) });
        }

        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> IntakeCandidates(int idError = 0)
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
                                                          .Include(n => n.IntakeScreening)
                                                          .Where(n => n.IntakeScreening == null && n.Clinic.Id == user_logged.Clinic.Id)
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
                            Certify = true,
                            Certify1 = true,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
                            Documents = true,
                            Id = 0,
                            Underestand = true,
                            IdClient = id,
                            Client_FK = id
                            
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

                        return RedirectToAction("Index", "Intakes");
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

                        return RedirectToAction("Index", "Intakes");
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
                                                                            .FirstOrDefault(n => n.Client.Id == id);
                    if (intakeConsent == null)
                    {
                        model = new IntakeConsentForReleaseViewModel
                        {
                            Client = _context.Clients.FirstOrDefault(n => n.Id == id),
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            ToRelease = false,
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

                        };

                        return View(model);
                    }
                    else
                    {
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

                        return RedirectToAction("Index", "Intakes");
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

                        return RedirectToAction("Index", "Intakes");
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
                                                                            .FirstOrDefault(n => n.Client.Id == id);
                    if (intakeConsent == null)
                    {
                        model = new IntakeConsumerRightsViewModel
                        {
                            Client = _context.Clients.FirstOrDefault(n => n.Id == id),
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            ServedOf = user_logged.FullName,
                            Documents = true,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
                            
                        };

                        return View(model);
                    }
                    else
                    {
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

                        return RedirectToAction("Index", "Intakes");
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

                        return RedirectToAction("Index", "Intakes");
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
                                                   .ThenInclude(c => c.IntakeConsentForRelease)

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.IntakeConsumerRights)

                                                   .Include(i => i.Client)
                                                   .ThenInclude(c => c.IntakeConsentForRelease)

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
                                                                            .FirstOrDefault(n => n.Client.Id == id);
                    if (intakeConsentPhotograph == null)
                    {
                        model = new IntakeConsentPhotographViewModel
                        {
                            Client = _context.Clients.FirstOrDefault(n => n.Id == id),
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
                            NoneOfTheForegoing = true,
                            Other = "",
                            Publication = true,
                            Broadcast = true,
                            Markrting = true,
                            ByTODocument = true,

                            Documents = true,
                        };

                        return View(model);
                    }
                    else
                    {
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

                        return RedirectToAction("Index", "Intakes");
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

                        return RedirectToAction("Index", "Intakes");
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
    }
}
