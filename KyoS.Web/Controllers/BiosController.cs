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
    public class BiosController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IReportHelper _reportHelper;
        private readonly DataContext _context;

        public BiosController(IUserHelper userHelper, IConverterHelper converterHelper, ICombosHelper combosHelper, IRenderHelper renderHelper, DataContext context, IReportHelper reportHelper)
        {
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _context = context;
            _renderHelper = renderHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant")]
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
                if (User.IsInRole("Manager")|| User.IsInRole("Supervisor"))
                    return View(await _context.Clients

                                              .Include(f => f.Clients_Diagnostics)
                                              .Include(g => g.Bio)
                                              .Include(g => g.List_BehavioralHistory)

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());

                if (User.IsInRole("Documents_Assistant") )
                    return View(await _context.Clients

                                              .Include(f => f.Clients_Diagnostics)
                                              .Include(g => g.Bio)
                                              .Include(g => g.List_BehavioralHistory)

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id && n.Bio.CreatedBy == user_logged.UserName)
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());

                if (User.IsInRole("Facilitator"))
                {
                    return View(await _context.Clients

                                              .Include(f => f.Clients_Diagnostics)
                                              .Include(g => g.Bio)
                                              .Include(g => g.List_BehavioralHistory)
                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult Create(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            BioViewModel model = new BioViewModel();

            if (User.IsInRole("Supervisor") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {

                    model = new BioViewModel
                    {
                        IdClient = id,
                        Client_FK = id,
                        Client = _context.Clients.Include(n => n.LegalGuardian)
                                                 .Include(n => n.EmergencyContact)
                                                 .Include(n => n.MedicationList)
                                                 .Include(n => n.Referred)
                                                 .Include(n => n.List_BehavioralHistory)
                                                 .FirstOrDefault(n => n.Id == id),
                        AdultCurrentExperience = "",
                        Affect_Angry = false,
                        Affect_Anxious = false,
                        Affect_Appropriate = false,
                        Affect_Blunted = false,
                        Affect_Constricted = false,
                        Affect_Expansive = false,
                        Affect_Flat = false,
                        Affect_labile = false,
                        Affect_Other = false,
                        Affect_Tearful_Sad = false,
                        AlternativeDiagnosis = "",
                        Appearance_Bizarre = false,
                        Appearance_Cleaned = false,
                        Appearance_Disheveled = false,
                        Appearance_FairHygiene = false,
                        Appearance_WellGroomed = false,
                        Appetite = Bio_Appetite.Diminished,
                        ApproximateDateReport = DateTime.Now,
                        ApproximateDateReport_Where = "",
                        AReferral = false,
                        AReferral_Services = "",
                        AReferral_When = "",
                        AReferral_Where = "",
                        BioH0031HN = false,
                        IDAH0031HO = false,
                        CanClientFollow = false,
                        Children = "",
                        ClientAssessmentSituation = "",
                        ClientFamilyAbusoTrauma = false,
                        CMH = false,
                        Comments = "",
                        DateAbuse = DateTime.Now,
                        DateBio = DateTime.Now,
                        DateSignatureLicensedPractitioner = DateTime.Now,
                        DateSignaturePerson = DateTime.Now,
                        DateSignatureSupervisor = DateTime.Now,
                        DateSignatureUnlicensedTherapist = DateTime.Now,
                        Details = "",
                        DoesClient = false,
                        DoesClientRequired = false,
                        DoesClientRequired_Where = "",
                        DoesNotAlways = false,
                        DoesTheClientExperience = false,
                        DoesTheClientExperience_Where = "",
                        DoYouHaveAnyPhysical = false,
                        DoYouHaveAnyReligious = false,
                        DoYouHaveAnyVisual = false,
                        DoYouOwn = false,
                        DoYouOwn_Explain = "",
                        EastAlone = false,
                        EastFew = false,
                        EastFewer = false,
                        FamilyAssessmentSituation = "",
                        FamilyEmotional = "",
                        GeneralDescription = "",
                        Has3OrMore = false,
                        HasAnIllnes = false,
                        HasClientBeenTreatedPain = false,
                        HasClientBeenTreatedPain_Ifnot = "",
                        HasClientBeenTreatedPain_PleaseIncludeService = "",
                        HasClientBeenTreatedPain_Where = "",
                        HasTheClient = false,
                        HasTheClientVisitedPhysician = false,
                        HasTheClientVisitedPhysician_Date = "",
                        HasTheClientVisitedPhysician_Reason = "",
                        HasTheClient_Explain = "",
                        HasTooth = false,
                        HaveYouEverBeen = false,
                        HaveYouEverBeen_Explain = "",
                        HaveYouEverThought = false,
                        HaveYouEverThought_Explain = "",
                        HigHestEducation = "",
                        Hydration = Bio_Hydration.Diminished,
                        IConcurWhitDiagnistic = false,
                        Id = 0,
                        If6_Date = DateTime.Now,
                        If6_ReferredTo = "",
                        IfForeing_AgeArrival = 0,
                        IfForeing_Born = false,
                        IfForeing_YearArrival = 0,
                        IfMarried = "",
                        IfSeparated = "",
                        IfSexuallyActive = Bio_IfSexuallyActive.N_A,
                        Insight_Fair = false,
                        Insight_Good = false,
                        Insight_Other = false,
                        Insight_Poor = false,
                        Judgment_Fair = false,
                        Judgment_Good = false,
                        Judgment_Other = false,
                        Judgment_Poor = false,
                        Lacking_Location = false,
                        Lacking_Person = false,
                        Lacking_Place = false,
                        Lacking_Time = false,
                        LegalAssessment = "",
                        LegalHistory = "",
                        LicensedPractitioner = user_logged.FullName,
                        MaritalStatus = "",
                        Mood_Angry = false,
                        Mood_Anxious = false,
                        Mood_Depressed = false,
                        Mood_Euphoric = false,
                        Mood_Euthymic = false,
                        Mood_Maniac = false,
                        Mood_Other = false,
                        Motor_Agitated = false,
                        Motor_Akathisia = false,
                        Motor_Normal = false,
                        Motor_Other = false,
                        Motor_RestLess = false,
                        Motor_Retardation = false,
                        Motor_Tremor = false,
                        NotAlwaysPhysically = false,
                        ObtainRelease = false,
                        ObtainReleaseInformation = false,
                        ObtainReleaseInformation7 = false,
                        Oriented_FullOriented = false,
                        Outcome = "",
                        PersonalFamilyPsychiatric = "",
                        PersonInvolved = "",
                        PleaseProvideGoal = "",
                        PleaseRatePain = 0,
                        PresentingProblem = "",
                        PrimaryLocation = "",
                        Priv = false,
                        ProvideIntegratedSummary = "",
                        RecentWeight = Bio_RecentWeightChange.Gained,
                        RelationShips = "",
                        RelationshipWithFamily = "",
                        RiskToOther_Chronic = false,
                        RiskToOther_High = false,
                        RiskToOther_Low = false,
                        RiskToOther_Medium = false,
                        RiskToSelf_Chronic = false,
                        RiskToSelf_High = false,
                        RiskToSelf_Low = false,
                        RiskToSelf_Medium = false,
                        SafetyPlan = false,
                        Setting = "02",
                        Speech_Impoverished = false,
                        Speech_Loud = false,
                        Speech_Mumbled = false,
                        Speech_Normal = false,
                        Speech_Other = false,
                        Speech_Pressured = false,
                        Speech_Rapid = false,
                        Speech_Slow = false,
                        Speech_Slurred = false,
                        Speech_Stutters = false,
                        SubstanceAbuse = "",
                        Takes3OrMore = false,
                        ThoughtContent_Delusions = false,
                        ThoughtContent_Delusions_Type = "",
                        ThoughtContent_Hallucinations = false,
                        ThoughtContent_Hallucinations_Type = "",
                        ThoughtContent_RealityBased = false,
                        ThoughtContent_Relevant = false,
                        ThoughtProcess_Blocking = false,
                        ThoughtProcess_Circumstantial = false,
                        ThoughtProcess_Disorganized = false,
                        ThoughtProcess_FightIdeas = false,
                        ThoughtProcess_GoalDirected = false,
                        ThoughtProcess_Irrational = false,
                        ThoughtProcess_LooseAssociations = false,
                        ThoughtProcess_Obsessive = false,
                        ThoughtProcess_Organized = false,
                        ThoughtProcess_Other = false,
                        ThoughtProcess_Preoccupied = false,
                        ThoughtProcess_Rigid = false,
                        ThoughtProcess_Tangential = false,
                        TreatmentNeeds = "",
                        Treatmentrecomendations = "",
                        UnlicensedTherapist = "",
                        WhatIsTheClient = "",
                        WhatIsYourLanguage = "",
                        WhereRecord = false,
                        WhereRecord_When = "",
                        WhereRecord_Where = "",
                        WithoutWanting = false,
                        IdAppetite = 0,
                        Appetite_Status = _combosHelper.GetComboBio_Appetite(),
                        IdHydratation = 0,
                        Hydratation_Status = _combosHelper.GetComboBio_Hydration(),
                        IdRecentWeight = 0,
                        RecentWeight_Status = _combosHelper.GetComboBio_RecentWeight(),
                        IdIfSexuallyActive = 0,
                        IfSexuallyActive_Status = _combosHelper.GetComboBio_IfSexuallyActive(),
                        ClientDenied = false,
                        /*StartTime = DateTime.Now,
                        EndTime = DateTime.Now,*/
                        ForHowLong = "",
                        CreatedOn = DateTime.Now,
                        CreatedBy = user_logged.UserName,
                        AdmissionedFor = user_logged.FullName
                    };
                    if (model.Client.LegalGuardian == null)
                        model.Client.LegalGuardian = new LegalGuardianEntity();
                    if (model.Client.EmergencyContact == null)
                        model.Client.EmergencyContact = new EmergencyContactEntity();
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    if (model.Client.Doctor == null)
                        model.Client.Doctor = new DoctorEntity();
                    if (model.Client.Referred == null)
                        model.Client.Referred = new ReferredEntity();

                    model.ReferralName = model.Client.Referred.Name;
                    model.LegalGuardianName = model.Client.LegalGuardian.Name;
                    model.LegalGuardianTelephone = model.Client.LegalGuardian.Telephone;
                    model.EmergencyContactName = model.Client.EmergencyContact.Name;
                    model.EmergencyContactTelephone = model.Client.EmergencyContact.Telephone;
                    model.RelationShipOfEmergencyContact = model.Client.RelationShipOfEmergencyContact.ToString();

                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> Create(BioViewModel bioViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                BioEntity bioEntity = _context.Bio.Find(bioViewModel.Id);
                if (bioEntity == null)
                {
                    bioEntity = await _converterHelper.ToBioEntity(bioViewModel, true, user_logged.UserName);
                    _context.Bio.Add(bioEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Index", "Bios");
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the BIO.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", bioViewModel) });
                }
            }
            BioViewModel model;
            model = new BioViewModel
            {
                IdClient = bioViewModel.IdClient,
                Client = _context.Clients.Include(n => n.LegalGuardian)
                                                 .Include(n => n.EmergencyContact)
                                                 .Include(n => n.MedicationList)
                                                 .Include(n => n.Referred)
                                                 .Include(n => n.List_BehavioralHistory)
                                                 .FirstOrDefault(n => n.Id == bioViewModel.IdClient),
                AdultCurrentExperience = "",
                Affect_Angry = false,
                Affect_Anxious = false,
                Affect_Appropriate = false,
                Affect_Blunted = false,
                Affect_Constricted = false,
                Affect_Expansive = false,
                Affect_Flat = false,
                Affect_labile = false,
                Affect_Other = false,
                Affect_Tearful_Sad = false,
                AlternativeDiagnosis = "",
                Appearance_Bizarre = false,
                Appearance_Cleaned = false,
                Appearance_Disheveled = false,
                Appearance_FairHygiene = false,
                Appearance_WellGroomed = false,
                Appetite = Bio_Appetite.Diminished,
                ApproximateDateReport = DateTime.Now,
                ApproximateDateReport_Where = "",
                AReferral = false,
                AReferral_Services = "",
                AReferral_When = "",
                AReferral_Where = "",
                BioH0031HN = false,
                IDAH0031HO = false,
                CanClientFollow = false,
                Children = "",
                ClientAssessmentSituation = "",
                ClientFamilyAbusoTrauma = false,
                CMH = false,
                Comments = "",
                DateAbuse = DateTime.Now,
                DateBio = DateTime.Now,
                DateSignatureLicensedPractitioner = DateTime.Now,
                DateSignaturePerson = DateTime.Now,
                DateSignatureSupervisor = DateTime.Now,
                DateSignatureUnlicensedTherapist = DateTime.Now,
                Details = "",
                DoesClient = false,
                DoesClientRequired = false,
                DoesClientRequired_Where = "",
                DoesNotAlways = false,
                DoesTheClientExperience = false,
                DoesTheClientExperience_Where = "",
                DoYouHaveAnyPhysical = false,
                DoYouHaveAnyReligious = false,
                DoYouHaveAnyVisual = false,
                DoYouOwn = false,
                DoYouOwn_Explain = "",
                EastAlone = false,
                EastFew = false,
                EastFewer = false,
                FamilyAssessmentSituation = "",
                FamilyEmotional = "",
                GeneralDescription = "",
                Has3OrMore = false,
                HasAnIllnes = false,
                HasClientBeenTreatedPain = false,
                HasClientBeenTreatedPain_Ifnot = "",
                HasClientBeenTreatedPain_PleaseIncludeService = "",
                HasClientBeenTreatedPain_Where = "",
                HasTheClient = false,
                HasTheClientVisitedPhysician = false,
                HasTheClientVisitedPhysician_Date = "",
                HasTheClientVisitedPhysician_Reason = "",
                HasTheClient_Explain = "",
                HasTooth = false,
                HaveYouEverBeen = false,
                HaveYouEverBeen_Explain = "",
                HaveYouEverThought = false,
                HaveYouEverThought_Explain = "",
                HigHestEducation = "",
                Hydration = Bio_Hydration.Diminished,
                IConcurWhitDiagnistic = false,
                Id = 0,
                If6_Date = DateTime.Now,
                If6_ReferredTo = "",
                IfForeing_AgeArrival = 0,
                IfForeing_Born = false,
                IfForeing_YearArrival = 0,
                IfMarried = "",
                IfSeparated = "",
                IfSexuallyActive = Bio_IfSexuallyActive.N_A,
                Insight_Fair = false,
                Insight_Good = false,
                Insight_Other = false,
                Insight_Poor = false,
                Judgment_Fair = false,
                Judgment_Good = false,
                Judgment_Other = false,
                Judgment_Poor = false,
                Lacking_Location = false,
                Lacking_Person = false,
                Lacking_Place = false,
                Lacking_Time = false,
                LegalAssessment = "",
                LegalHistory = "",
                LicensedPractitioner = "",
                MaritalStatus = "",
                Mood_Angry = false,
                Mood_Anxious = false,
                Mood_Depressed = false,
                Mood_Euphoric = false,
                Mood_Euthymic = false,
                Mood_Maniac = false,
                Mood_Other = false,
                Motor_Agitated = false,
                Motor_Akathisia = false,
                Motor_Normal = false,
                Motor_Other = false,
                Motor_RestLess = false,
                Motor_Retardation = false,
                Motor_Tremor = false,
                NotAlwaysPhysically = false,
                ObtainRelease = false,
                ObtainReleaseInformation = false,
                ObtainReleaseInformation7 = false,
                Oriented_FullOriented = false,
                Outcome = "",
                PersonalFamilyPsychiatric = "",
                PersonInvolved = "",
                PleaseProvideGoal = "",
                PleaseRatePain = 0,
                PresentingProblem = "",
                PrimaryLocation = "",
                Priv = false,
                ProvideIntegratedSummary = "",
                RecentWeight = Bio_RecentWeightChange.Gained,
                RelationShips = "",
                RelationshipWithFamily = "",
                RiskToOther_Chronic = false,
                RiskToOther_High = false,
                RiskToOther_Low = false,
                RiskToOther_Medium = false,
                RiskToSelf_Chronic = false,
                RiskToSelf_High = false,
                RiskToSelf_Low = false,
                RiskToSelf_Medium = false,
                SafetyPlan = false,
                Setting = "",
                Speech_Impoverished = false,
                Speech_Loud = false,
                Speech_Mumbled = false,
                Speech_Normal = false,
                Speech_Other = false,
                Speech_Pressured = false,
                Speech_Rapid = false,
                Speech_Slow = false,
                Speech_Slurred = false,
                Speech_Stutters = false,
                SubstanceAbuse = "",
                Takes3OrMore = false,
                ThoughtContent_Delusions = false,
                ThoughtContent_Delusions_Type = "",
                ThoughtContent_Hallucinations = false,
                ThoughtContent_Hallucinations_Type = "",
                ThoughtContent_RealityBased = false,
                ThoughtContent_Relevant = false,
                ThoughtProcess_Blocking = false,
                ThoughtProcess_Circumstantial = false,
                ThoughtProcess_Disorganized = false,
                ThoughtProcess_FightIdeas = false,
                ThoughtProcess_GoalDirected = false,
                ThoughtProcess_Irrational = false,
                ThoughtProcess_LooseAssociations = false,
                ThoughtProcess_Obsessive = false,
                ThoughtProcess_Organized = false,
                ThoughtProcess_Other = false,
                ThoughtProcess_Preoccupied = false,
                ThoughtProcess_Rigid = false,
                ThoughtProcess_Tangential = false,
                TreatmentNeeds = "",
                Treatmentrecomendations = "",
                UnlicensedTherapist = "",
                WhatIsTheClient = "",
                WhatIsYourLanguage = "",
                WhereRecord = false,
                WhereRecord_When = "",
                WhereRecord_Where = "",
                WithoutWanting = false,
                IdAppetite = 0,
                Appetite_Status = _combosHelper.GetComboBio_Appetite(),
                IdHydratation = 0,
                Hydratation_Status = _combosHelper.GetComboBio_Hydration(),
                IdRecentWeight = 0,
                RecentWeight_Status = _combosHelper.GetComboBio_RecentWeight(),
                IdIfSexuallyActive = 0,
                IfSexuallyActive_Status = _combosHelper.GetComboBio_IfSexuallyActive(),
                ClientDenied = false,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
                ForHowLong = ""
            };
            
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult Edit(int id = 0)
        {
            BioEntity entity = _context.Bio

                                       .Include(m => m.Client)
                                       .Include(n => n.Client.LegalGuardian)
                                       .Include(n => n.Client.EmergencyContact)
                                       .Include(n => n.Client.MedicationList)
                                       .Include(n => n.Client.Referred)
                                       .Include(n => n.Client.List_BehavioralHistory)

                                       .FirstOrDefault(i => i.Client.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Create", new { id = id });
            }

            BioViewModel model;

            if (User.IsInRole("Supervisor") || User.IsInRole("Documents_Assistant"))
            {
                UserEntity user_logged = _context.Users

                                                 .Include(u => u.Clinic)

                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {
                    model = _converterHelper.ToBioViewModel(entity);
                    if (model.Client.LegalGuardian == null)
                        model.Client.LegalGuardian = new LegalGuardianEntity();
                    if (model.Client.EmergencyContact == null)
                        model.Client.EmergencyContact = new EmergencyContactEntity();
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    if (model.Client.Doctor == null)
                        model.Client.Doctor = new DoctorEntity();
                    if (model.Client.Referred == null)
                        model.Client.Referred = new ReferredEntity();
                    if (model.Client.FarsFormList == null)
                        model.Client.FarsFormList = new List<FarsFormEntity>();
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    if (model.Client.List_BehavioralHistory == null)
                        model.Client.List_BehavioralHistory = new List<Bio_BehavioralHistoryEntity>();

                    model.ReferralName = model.Client.Referred.Name;
                    model.LegalGuardianName = model.Client.LegalGuardian.Name;
                    model.LegalGuardianTelephone = model.Client.LegalGuardian.Telephone;
                    model.EmergencyContactName = model.Client.EmergencyContact.Name;
                    model.EmergencyContactTelephone = model.Client.EmergencyContact.Telephone;
                    model.RelationShipOfEmergencyContact = model.Client.RelationShipOfEmergencyContact.ToString();

                    return View(model);
                }
            }

            model = new BioViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> Edit(BioViewModel bioViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                BioEntity bioEntity = await _converterHelper.ToBioEntity(bioViewModel, false, user_logged.UserName);
                _context.Bio.Update(bioEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Bios");
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", bioViewModel) });
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult CreateBehavioral(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            Bio_BehavioralHistoryViewModel model;

            if (User.IsInRole("Supervisor") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {

                    model = new Bio_BehavioralHistoryViewModel
                    {
                        IdClient = id,
                        Client = _context.Clients.Include(n => n.List_BehavioralHistory).FirstOrDefault(n => n.Id == id),
                        Date = "",
                        Id = 0,
                        Problem = ""
                    };
                    if (model.Client.List_BehavioralHistory == null)
                        model.Client.List_BehavioralHistory = new List<Bio_BehavioralHistoryEntity>();
                    return View(model);
                }
            }

            model = new Bio_BehavioralHistoryViewModel
            {
                IdClient = id,
                Client = _context.Clients.Include(n => n.List_BehavioralHistory).FirstOrDefault(n => n.Id == id),
                Date = "",
                Id = 0,
                Problem = ""
            };
            
            return View(model);
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult CreateBehavioralModal(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            Bio_BehavioralHistoryViewModel model;

            if (User.IsInRole("Supervisor") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {

                    model = new Bio_BehavioralHistoryViewModel
                    {
                        IdClient = id,
                        Client = _context.Clients.Include(n => n.List_BehavioralHistory).FirstOrDefault(n => n.Id == id),
                        Date = "",
                        Id = 0,
                        Problem = ""
                    };
                    if (model.Client.List_BehavioralHistory == null)
                        model.Client.List_BehavioralHistory = new List<Bio_BehavioralHistoryEntity>();
                    return View(model);
                }
            }

            model = new Bio_BehavioralHistoryViewModel
            {
                IdClient = id,
                Client = _context.Clients.Include(n => n.List_BehavioralHistory).FirstOrDefault(n => n.Id == id),
                Date = "",
                Id = 0,
                Problem = ""
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> CreateBehavioral(Bio_BehavioralHistoryViewModel bioViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                Bio_BehavioralHistoryEntity bioEntity = await _converterHelper.ToBio_BehaviorEntity(bioViewModel, true);
                _context.Bio_BehavioralHistory.Add(bioEntity);
                try
                {
                    await _context.SaveChangesAsync();
                        
                    return RedirectToAction("CreateBehavioral", new { id = bioViewModel.IdClient });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
                
            }
            Bio_BehavioralHistoryEntity model;
            model = new Bio_BehavioralHistoryEntity
            {
               
                Date = "",
                Id = 0,
                Problem = ""
            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> CreateBehavioralModal(Bio_BehavioralHistoryViewModel bioViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                
                Bio_BehavioralHistoryEntity bioEntity = await _converterHelper.ToBio_BehaviorEntity(bioViewModel, true);
                _context.Bio_BehavioralHistory.Add(bioEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<Bio_BehavioralHistoryEntity> bio = await _context.Bio_BehavioralHistory
                                                                          .Include(g => g.Client)
                                                                          .Where(g => g.Client.Id == bioViewModel.IdClient)
                                                                          .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewBehavioralHealth", bio) });
                        
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
               
            }
            Bio_BehavioralHistoryEntity model;
            model = new Bio_BehavioralHistoryEntity
            {

                Date = "",
                Id = 0,
                Problem = ""
            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
        public async Task<IActionResult> IndexBehavioralHealthHistory(int idError = 0)
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
                if (User.IsInRole("Manager") || User.IsInRole("Supervisor"))
                    return View(await _context.Clients

                                              .Include(f => f.Clients_Diagnostics)
                                              .Include(g => g.Bio)
                                              .Include(g => g.List_BehavioralHistory)

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());

                if (User.IsInRole("Facilitator"))
                {
                    return View(await _context.Clients
                                              .Include(f => f.Clients_Diagnostics)
                                              .Include(g => g.Bio)
                                              .Include(g => g.List_BehavioralHistory)

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult EditBehavioral(int id = 0)
        {
            Bio_BehavioralHistoryViewModel model;

            if (User.IsInRole("Supervisor") || User.IsInRole("Documents_Assistant"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    Bio_BehavioralHistoryEntity Behavioral = _context.Bio_BehavioralHistory
                                                                     .Include(m => m.Client)
                                                                     .ThenInclude(m => m.List_BehavioralHistory)
                                                                     .FirstOrDefault(m => m.Id == id);
                    if (Behavioral == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToBio_BehaviorViewModel(Behavioral);

                        return View(model);
                    }

                }
            }

            model = new Bio_BehavioralHistoryViewModel();
            return View(model);
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult EditBehavioralModal(int id = 0)
        {
            Bio_BehavioralHistoryViewModel model;

            if (User.IsInRole("Supervisor") || User.IsInRole("Documents_Assistant"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    Bio_BehavioralHistoryEntity Behavioral = _context.Bio_BehavioralHistory
                                                                     .Include(m => m.Client)
                                                                     .ThenInclude(m => m.List_BehavioralHistory)
                                                                     .FirstOrDefault(m => m.Id == id);
                    if (Behavioral == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToBio_BehaviorViewModel(Behavioral);

                        return View(model);
                    }

                }
            }

            model = new Bio_BehavioralHistoryViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> EditBehavioral(Bio_BehavioralHistoryViewModel behavioralViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                Bio_BehavioralHistoryEntity behavioralEntity = await _converterHelper.ToBio_BehaviorEntity(behavioralViewModel, false);
                _context.Bio_BehavioralHistory.Update(behavioralEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("IndexBehavioralHealthHistory", "Bios");

                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditBehavioral", behavioralViewModel) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> EditBehavioralModal(Bio_BehavioralHistoryViewModel behavioralViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                Bio_BehavioralHistoryEntity behavioralEntity = await _converterHelper.ToBio_BehaviorEntity(behavioralViewModel, false);
                _context.Bio_BehavioralHistory.Update(behavioralEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<Bio_BehavioralHistoryEntity> bio = await _context.Bio_BehavioralHistory
                                                                                .Include(g => g.Client)
                                                                                .Where(g => g.Client.Id == behavioralViewModel.IdClient)
                                                                                .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewBehavioralHealth", bio) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditBehavioralModal", behavioralViewModel) });
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> DeleteBehavioral(int? id, int origin = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Bio_BehavioralHistoryEntity Bio_BehavioralEntity = await _context.Bio_BehavioralHistory
                                                                             .Include(n => n.Client)
                                                                             .FirstOrDefaultAsync(s => s.Id == id);
            if (Bio_BehavioralEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Bio_BehavioralHistory.Remove(Bio_BehavioralEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(IndexBehavioralHealthHistory));            
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant")]
        public IActionResult PrintBio(int id)
        {
            BioEntity entity = _context.Bio

                                       .Include(b => b.Client)
                                       .ThenInclude(c => c.Clinic)

                                       .Include(b => b.Client)
                                       .ThenInclude(c => c.EmergencyContact)

                                       .Include(b => b.Client)
                                       .ThenInclude(c => c.LegalGuardian)

                                       .Include(b => b.Client)
                                       .ThenInclude(c => c.List_BehavioralHistory)

                                       .Include(b => b.Client)
                                       .ThenInclude(c => c.MedicationList)

                                       .Include(b => b.Client)
                                       .ThenInclude(c => c.Clients_Diagnostics)
                                       .ThenInclude(cd => cd.Diagnostic)

                                       .Include(b => b.Client)
                                       .ThenInclude(c => c.Doctor)

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
                Stream stream = _reportHelper.FloridaSocialHSBioReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "DREAMS MENTAL HEALTH INC")
            {
                Stream stream = _reportHelper.DreamsMentalHealthBioReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            return null;
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> ClientswithoutBIO(int idError = 0)
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
                                                          .Include(n => n.Bio)
                                                          .Where(n => n.Bio == null && n.Clinic.Id == user_logged.Clinic.Id)
                                                          .ToListAsync();

            return View(ClientList);

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

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult EditMedicationModal(int id = 0)
        {
            MedicationViewModel model;

            if (User.IsInRole("Documents_Assistant") || User.IsInRole("Supervisor"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    MedicationEntity Medication = _context.Medication
                                                         .Include(m => m.Client)
                                                         .ThenInclude(m => m.MedicationList)
                                                         .FirstOrDefault(m => m.Id == id);
                    if (Medication == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToMedicationViewModel(Medication);

                        return View(model);
                    }

                }
            }

            model = new MedicationViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> EditMedicationModal(MedicationViewModel medicationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                MedicationEntity medicationEntity = await _converterHelper.ToMedicationEntity(medicationViewModel, false);
                _context.Medication.Update(medicationEntity);
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

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditMedicationModal", medicationViewModel) });
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> DeleteMedication(int? id, int origin = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MedicationEntity medicationEntity = await _context.Medication
                                                              .Include(n => n.Client)
                                                              .FirstOrDefaultAsync(s => s.Id == id);
            if (medicationEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Medication.Remove(medicationEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            if (origin == 1)
                return RedirectToAction(nameof(Index));
            else
            {
                List<MedicationEntity> medication = await _context.Medication
                                                                         .Include(g => g.Client)
                                                                         .Where(g => g.Client.Id == medicationEntity.Client.Id)
                                                                         .ToListAsync();

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewMedication", medication) });
            }
        }
    }
}
