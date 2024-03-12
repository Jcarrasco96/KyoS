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
using Microsoft.EntityFrameworkCore;
using System.IO;
using KyoS.Common.Helpers;
using Newtonsoft.Json;

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
        private readonly IOverlapindHelper _overlapingHelper;

        public BiosController(IUserHelper userHelper, IConverterHelper converterHelper, ICombosHelper combosHelper, IRenderHelper renderHelper, DataContext context, IReportHelper reportHelper, IOverlapindHelper overlapingHelper)
        {
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _context = context;
            _renderHelper = renderHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
            _overlapingHelper = overlapingHelper;
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
                    return View(await _context.Clients

                                              .Include(g => g.Bio)
                                              .Include(u => u.Clinic)
                                              .ThenInclude(c => c.Setting)
                                              .Include(g => g.List_BehavioralHistory)

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());

                if (User.IsInRole("Documents_Assistant") )
                    return View(await _context.Clients

                                              .Include(g => g.Bio)
                                              .Include(u => u.Clinic)
                                              .ThenInclude(c => c.Setting)
                                              .Include(g => g.List_BehavioralHistory)

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id && n.Bio.CreatedBy == user_logged.UserName)
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());

                if (User.IsInRole("Facilitator"))
                {
                    return View(await _context.Clients

                                              .Include(g => g.Bio)
                                              .Include(u => u.Clinic)
                                              .ThenInclude(c => c.Setting)
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
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            BioTempEntity bioExist = _context.BioTemp
                                             .FirstOrDefault(b => (b.UserName == user_logged.UserName && b.Url == $"Bios/Create/{id}"));
            BioViewModel model;
            if (bioExist != null)
            {
                model = new BioViewModel
                {
                    IdClient = id,
                    Client_FK = id,
                    Client = _context.Clients.Include(n => n.LegalGuardian)
                                                .Include(n => n.EmergencyContact)
                                                .Include(n => n.MedicationList)
                                                .Include(n => n.Client_Referred)
                                                .ThenInclude(n => n.Referred)
                                                .Include(n => n.List_BehavioralHistory)
                                                .Include(f => f.Clients_Diagnostics)
                                                .ThenInclude(f => f.Diagnostic)
                                                .FirstOrDefault(n => n.Id == id),
                    AdultCurrentExperience = bioExist.AdultCurrentExperience,
                    Affect_Angry = bioExist.Affect_Angry,
                    Affect_Anxious = bioExist.Affect_Anxious,
                    Affect_Appropriate = bioExist.Affect_Appropriate,
                    Affect_Blunted = bioExist.Affect_Blunted,
                    Affect_Constricted = bioExist.Affect_Constricted,
                    Affect_Expansive = bioExist.Affect_Expansive,
                    Affect_Flat = bioExist.Affect_Flat,
                    Affect_labile = bioExist.Affect_labile,
                    Affect_Other = bioExist.Affect_Other,
                    Affect_Tearful_Sad = bioExist.Affect_Tearful_Sad,
                    AlternativeDiagnosis = bioExist.AlternativeDiagnosis,
                    Appearance_Bizarre = bioExist.Appearance_Bizarre,
                    Appearance_Cleaned = bioExist.Appearance_Cleaned,
                    Appearance_Disheveled = bioExist.Appearance_Disheveled,
                    Appearance_FairHygiene = bioExist.Appearance_FairHygiene,
                    Appearance_WellGroomed = bioExist.Appearance_WellGroomed,                    
                    Appetite = bioExist.Appetite,
                    ApproximateDateReport = bioExist.ApproximateDateReport,
                    ApproximateDateReport_Where = bioExist.ApproximateDateReport_Where,
                    AReferral = bioExist.AReferral,
                    AReferral_Services = bioExist.AReferral_Services,
                    AReferral_When = bioExist.AReferral_When,
                    AReferral_Where = bioExist.AReferral_Where,
                    BioH0031HN = bioExist.BioH0031HN,
                    IDAH0031HO = bioExist.IDAH0031HO,
                    CanClientFollow = bioExist.CanClientFollow,
                    Children = bioExist.Children,
                    ClientAssessmentSituation = bioExist.ClientAssessmentSituation,
                    ClientFamilyAbusoTrauma = bioExist.ClientFamilyAbusoTrauma,
                    CMH = bioExist.CMH,
                    Comments = bioExist.Comments,
                    DateAbuse = bioExist.DateAbuse,
                    DateBio = Convert.ToDateTime(bioExist.DateBio),
                    DateSignatureLicensedPractitioner = Convert.ToDateTime(bioExist.DateSignatureLicensedPractitioner),
                    DateSignaturePerson = Convert.ToDateTime(bioExist.DateSignaturePerson),
                    DateSignatureSupervisor = Convert.ToDateTime(bioExist.DateSignatureSupervisor),
                    DateSignatureUnlicensedTherapist = Convert.ToDateTime(bioExist.DateSignatureUnlicensedTherapist),
                    Details = bioExist.Details,
                    DoesClient = bioExist.DoesClient,
                    DoesClientRequired = bioExist.DoesClientRequired,
                    DoesClientRequired_Where = bioExist.DoesClientRequired_Where,
                    DoesNotAlways = bioExist.DoesNotAlways,
                    DoesTheClientExperience = bioExist.DoesTheClientExperience,
                    DoesTheClientExperience_Where = bioExist.DoesTheClientExperience_Where,
                    DoYouHaveAnyPhysical = bioExist.DoYouHaveAnyPhysical,
                    DoYouHaveAnyReligious = bioExist.DoYouHaveAnyReligious,
                    DoYouHaveAnyVisual = bioExist.DoYouHaveAnyVisual,
                    DoYouOwn = bioExist.DoYouOwn,
                    DoYouOwn_Explain = bioExist.DoYouOwn_Explain,
                    EastAlone = bioExist.EastAlone,
                    EastFew = bioExist.EastFew,
                    EastFewer = bioExist.EastFewer,
                    FamilyAssessmentSituation = bioExist.FamilyAssessmentSituation,
                    FamilyEmotional = bioExist.FamilyEmotional,
                    GeneralDescription = bioExist.GeneralDescription,
                    Has3OrMore = bioExist.Has3OrMore,
                    HasAnIllnes = bioExist.HasAnIllnes,
                    HasClientBeenTreatedPain = bioExist.HasClientBeenTreatedPain,
                    HasClientBeenTreatedPain_Ifnot = bioExist.HasClientBeenTreatedPain_Ifnot,
                    HasClientBeenTreatedPain_PleaseIncludeService = bioExist.HasClientBeenTreatedPain_PleaseIncludeService,
                    HasClientBeenTreatedPain_Where = bioExist.HasClientBeenTreatedPain_Where,
                    HasTheClient = bioExist.HasTheClient,
                    HasTheClientVisitedPhysician = bioExist.HasTheClientVisitedPhysician,
                    HasTheClientVisitedPhysician_Date = bioExist.HasTheClientVisitedPhysician_Date,
                    HasTheClientVisitedPhysician_Reason = bioExist.HasTheClientVisitedPhysician_Reason,
                    HasTheClient_Explain = bioExist.HasTheClient_Explain,
                    HasTooth = bioExist.HasTooth,
                    HaveYouEverBeen = bioExist.HaveYouEverBeen,
                    HaveYouEverBeen_Explain = bioExist.HaveYouEverBeen_Explain,
                    HaveYouEverThought = bioExist.HaveYouEverThought,
                    HaveYouEverThought_Explain = bioExist.HaveYouEverThought_Explain,
                    HigHestEducation = bioExist.HigHestEducation,
                    Hydration = Bio_Hydration.Diminished,
                    IConcurWhitDiagnistic = bioExist.IConcurWhitDiagnistic,
                    Id = 0,
                    If6_Date = Convert.ToDateTime(bioExist.If6_Date),
                    If6_ReferredTo = bioExist.If6_ReferredTo,
                    IfForeing_AgeArrival = bioExist.IfForeing_AgeArrival,
                    IfForeing_Born = bioExist.IfForeing_Born,
                    IfForeing_YearArrival = bioExist.IfForeing_YearArrival,
                    IfMarried = bioExist.IfMarried,
                    IfSeparated = bioExist.IfSeparated,
                    IfSexuallyActive = bioExist.IfSexuallyActive,
                    Insight_Fair = bioExist.Insight_Fair,
                    Insight_Good = bioExist.Insight_Good,
                    Insight_Other = bioExist.Insight_Other,
                    Insight_Poor = bioExist.Insight_Poor,
                    Judgment_Fair = bioExist.Judgment_Fair,
                    Judgment_Good = bioExist.Judgment_Good,
                    Judgment_Other = bioExist.Judgment_Other,
                    Judgment_Poor = bioExist.Judgment_Poor,
                    Lacking_Location = bioExist.Lacking_Location,
                    Lacking_Person = bioExist.Lacking_Person,
                    Lacking_Place = bioExist.Lacking_Place,
                    Lacking_Time = bioExist.Lacking_Time,
                    LegalAssessment = bioExist.LegalAssessment,
                    LegalHistory = bioExist.LegalHistory,
                    MaritalStatus = bioExist.MaritalStatus,
                    Mood_Angry = bioExist.Mood_Angry,
                    Mood_Anxious = bioExist.Mood_Anxious,
                    Mood_Depressed = bioExist.Mood_Depressed,
                    Mood_Euphoric = bioExist.Mood_Euphoric,
                    Mood_Euthymic = bioExist.Mood_Euthymic,
                    Mood_Maniac = bioExist.Mood_Maniac,
                    Mood_Other = bioExist.Mood_Other,
                    Motor_Agitated = bioExist.Motor_Agitated,
                    Motor_Akathisia = bioExist.Motor_Akathisia,
                    Motor_Normal = bioExist.Motor_Normal,
                    Motor_Other = bioExist.Motor_Other,
                    Motor_RestLess = bioExist.Motor_RestLess,
                    Motor_Retardation = bioExist.Motor_Retardation,
                    Motor_Tremor = bioExist.Motor_Tremor,
                    NotAlwaysPhysically = bioExist.NotAlwaysPhysically,
                    ObtainRelease = bioExist.ObtainRelease,
                    ObtainReleaseInformation = bioExist.ObtainReleaseInformation,
                    ObtainReleaseInformation7 = bioExist.ObtainReleaseInformation7,
                    Oriented_FullOriented = bioExist.Oriented_FullOriented,
                    Outcome = bioExist.Outcome,
                    PersonalFamilyPsychiatric = bioExist.PersonalFamilyPsychiatric,
                    PersonInvolved = bioExist.PersonInvolved,
                    PleaseProvideGoal = bioExist.PleaseProvideGoal,
                    PleaseRatePain = bioExist.PleaseRatePain,
                    PresentingProblem = bioExist.PresentingProblem,
                    PrimaryLocation = bioExist.PrimaryLocation,
                    Priv = bioExist.Priv,
                    ProvideIntegratedSummary = bioExist.ProvideIntegratedSummary,
                    RecentWeight = bioExist.RecentWeight,
                    RelationShips = bioExist.RelationShips,
                    RelationshipWithFamily = bioExist.RelationshipWithFamily,
                    RiskToOther_Chronic = bioExist.RiskToOther_Chronic,
                    RiskToOther_High = bioExist.RiskToOther_High,
                    RiskToOther_Low = bioExist.RiskToOther_Low,
                    RiskToOther_Medium = bioExist.RiskToOther_Medium,
                    RiskToSelf_Chronic = bioExist.RiskToSelf_Chronic,
                    RiskToSelf_High = bioExist.RiskToSelf_High,
                    RiskToSelf_Low = bioExist.RiskToSelf_Low,
                    RiskToSelf_Medium = bioExist.RiskToSelf_Medium,
                    SafetyPlan = bioExist.SafetyPlan,
                    Setting = bioExist.Setting,
                    Speech_Impoverished = bioExist.Speech_Impoverished,
                    Speech_Loud = bioExist.Speech_Loud,
                    Speech_Mumbled = bioExist.Speech_Mumbled,
                    Speech_Normal = bioExist.Speech_Normal,
                    Speech_Other = bioExist.Speech_Other,
                    Speech_Pressured = bioExist.Speech_Pressured,
                    Speech_Rapid = bioExist.Speech_Rapid,
                    Speech_Slow = bioExist.Speech_Slow,
                    Speech_Slurred = bioExist.Speech_Slurred,
                    Speech_Stutters = bioExist.Speech_Stutters,
                    SubstanceAbuse = bioExist.SubstanceAbuse,
                    Takes3OrMore = bioExist.Takes3OrMore,
                    ThoughtContent_Delusions = bioExist.ThoughtContent_Delusions,
                    ThoughtContent_Delusions_Type = bioExist.ThoughtContent_Delusions_Type,
                    ThoughtContent_Hallucinations = bioExist.ThoughtContent_Hallucinations,
                    ThoughtContent_Hallucinations_Type = bioExist.ThoughtContent_Hallucinations_Type,
                    ThoughtContent_RealityBased = bioExist.ThoughtContent_RealityBased,
                    ThoughtContent_Relevant = bioExist.ThoughtContent_Relevant,
                    ThoughtProcess_Blocking = bioExist.ThoughtProcess_Blocking,
                    ThoughtProcess_Circumstantial = bioExist.ThoughtProcess_Circumstantial,
                    ThoughtProcess_Disorganized = bioExist.ThoughtProcess_Disorganized,
                    ThoughtProcess_FightIdeas = bioExist.ThoughtProcess_FightIdeas,
                    ThoughtProcess_GoalDirected = bioExist.ThoughtProcess_GoalDirected,
                    ThoughtProcess_Irrational = bioExist.ThoughtProcess_Irrational,
                    ThoughtProcess_LooseAssociations = bioExist.ThoughtProcess_LooseAssociations,
                    ThoughtProcess_Obsessive = bioExist.ThoughtProcess_Obsessive,
                    ThoughtProcess_Organized = bioExist.ThoughtProcess_Organized,
                    ThoughtProcess_Other = bioExist.ThoughtProcess_Other,
                    ThoughtProcess_Preoccupied = bioExist.ThoughtProcess_Preoccupied,
                    ThoughtProcess_Rigid = bioExist.ThoughtProcess_Rigid,
                    ThoughtProcess_Tangential = bioExist.ThoughtProcess_Tangential,
                    TreatmentNeeds = bioExist.TreatmentNeeds,
                    Treatmentrecomendations = bioExist.Treatmentrecomendations,
                    WhatIsTheClient = bioExist.WhatIsTheClient,
                    WhatIsYourLanguage = bioExist.WhatIsYourLanguage,
                    WhereRecord = bioExist.WhereRecord,
                    WhereRecord_When = bioExist.WhereRecord_When,
                    WhereRecord_Where = bioExist.WhereRecord_Where,
                    WithoutWanting = bioExist.WithoutWanting,
                    IdAppetite = ((int)bioExist.Appetite) + 1,
                    Appetite_Status = _combosHelper.GetComboBio_Appetite(),
                    IdHydratation = ((int)bioExist.Hydration) + 1,
                    Hydratation_Status = _combosHelper.GetComboBio_Hydration(),
                    IdRecentWeight = ((int)bioExist.RecentWeight) + 1,
                    RecentWeight_Status = _combosHelper.GetComboBio_RecentWeight(),
                    IdIfSexuallyActive = ((int)bioExist.IfSexuallyActive) + 1,
                    IfSexuallyActive_Status = _combosHelper.GetComboBio_IfSexuallyActive(),
                    ClientDenied = bioExist.ClientDenied,
                    ForHowLong = bioExist.ForHowLong,
                    CreatedOn = DateTime.Now,
                    CreatedBy = user_logged.UserName,
                    AdmissionedFor = user_logged.FullName,
                    CodeBill = user_logged.Clinic.CodeBIO,
                    AnyEating = bioExist.AnyEating,
                    AnyFood = bioExist.AnyFood,
                    MilitaryServiceHistory = bioExist.MilitaryServiceHistory,
                    MilitaryServiceHistory_Explain = bioExist.MilitaryServiceHistory_Explain,
                    VocationalAssesment = bioExist.VocationalAssesment,
                    Code90791 = bioExist.Code90791,
                    StartTime = Convert.ToDateTime(bioExist.StartTime),
                    EndTime = Convert.ToDateTime(bioExist.EndTime)
                };

                if (model.Client.LegalGuardian == null)
                    model.Client.LegalGuardian = new LegalGuardianEntity();
                if (model.Client.EmergencyContact == null)
                    model.Client.EmergencyContact = new EmergencyContactEntity();
                if (model.Client.MedicationList == null)
                    model.Client.MedicationList = new List<MedicationEntity>();
                if (model.Client.Doctor == null)
                    model.Client.Doctor = new DoctorEntity();
                if (model.Client.Client_Referred == null || model.Client.Client_Referred.Where(n => n.Service == ServiceAgency.CMH).Count() == 0)
                {
                    Client_Referred client_referred = new Client_Referred();
                    model.Client.Client_Referred = [client_referred];
                    model.ReferralName = "Not have referred";
                }
                else
                {
                    model.ReferralName = model.Client.Client_Referred.Where(n => n.Service == ServiceAgency.CMH).ElementAt(0).Referred.Name;
                }

                model.LegalGuardianName = model.Client.LegalGuardian.Name;
                model.LegalGuardianTelephone = model.Client.LegalGuardian.Telephone;
                model.EmergencyContactName = model.Client.EmergencyContact.Name;
                model.EmergencyContactTelephone = model.Client.EmergencyContact.Telephone;
                model.RelationShipOfEmergencyContact = model.Client.RelationShipOfEmergencyContact.ToString();
            }
            else
            { 
                model = new BioViewModel
                {
                    IdClient = id,
                    Client_FK = id,
                    Client = _context.Clients.Include(n => n.LegalGuardian)
                                                .Include(n => n.EmergencyContact)
                                                .Include(n => n.MedicationList)
                                                .Include(n => n.Client_Referred)
                                                .ThenInclude(n => n.Referred)
                                                .Include(n => n.List_BehavioralHistory)
                                                .Include(f => f.Clients_Diagnostics)
                                                .ThenInclude(f => f.Diagnostic)
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
                    ApproximateDateReport = string.Empty,
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
                    DateAbuse = string.Empty,
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
                    IConcurWhitDiagnistic = true,
                    Id = 0,
                    If6_Date = new DateTime(),
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
                    Setting = "53",
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
                    ForHowLong = "",
                    CreatedOn = DateTime.Now,
                    CreatedBy = user_logged.UserName,
                    AdmissionedFor = user_logged.FullName,
                    CodeBill = user_logged.Clinic.CodeBIO,
                    AnyEating = false,
                    AnyFood = false,
                    MilitaryServiceHistory = false,
                    MilitaryServiceHistory_Explain = string.Empty,
                    VocationalAssesment = string.Empty,
                    Code90791 = false,
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddMinutes(45)
                };

                if (model.Client.LegalGuardian == null)
                    model.Client.LegalGuardian = new LegalGuardianEntity();
                if (model.Client.EmergencyContact == null)
                    model.Client.EmergencyContact = new EmergencyContactEntity();
                if (model.Client.MedicationList == null)
                    model.Client.MedicationList = new List<MedicationEntity>();
                if (model.Client.Doctor == null)
                    model.Client.Doctor = new DoctorEntity();
                if (model.Client.Client_Referred == null || model.Client.Client_Referred.Where(n => n.Service == ServiceAgency.CMH).Count() == 0)
                {
                    Client_Referred client_referred = new Client_Referred();
                    model.Client.Client_Referred = [client_referred];
                    model.ReferralName = "Not have referred";
                }
                else
                {
                    model.ReferralName = model.Client.Client_Referred.Where(n => n.Service == ServiceAgency.CMH).ElementAt(0).Referred.Name;
                }

                model.LegalGuardianName = model.Client.LegalGuardian.Name;
                model.LegalGuardianTelephone = model.Client.LegalGuardian.Telephone;
                model.EmergencyContactName = model.Client.EmergencyContact.Name;
                model.EmergencyContactTelephone = model.Client.EmergencyContact.Telephone;
                model.RelationShipOfEmergencyContact = model.Client.RelationShipOfEmergencyContact.ToString();
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
                    //calcular las unidades a partir del tiempo de desarrollo del BIO
                    int units = (bioViewModel.EndTime.TimeOfDay - bioViewModel.StartTime.TimeOfDay).Minutes / 15;
                    if ((bioViewModel.EndTime.TimeOfDay - bioViewModel.StartTime.TimeOfDay).Minutes % 15 > 7)
                    {
                        units++;
                        bioViewModel.Units = units;
                    }
                    else
                    {
                        bioViewModel.Units = units;
                    }

                    DocumentsAssistantEntity documentAssistant = await _context.DocumentsAssistant.FirstOrDefaultAsync(m => m.LinkedUser == user_logged.UserName);
                    DateTime start = new DateTime(bioViewModel.DateBio.Year, bioViewModel.DateBio.Month, bioViewModel.DateBio.Day, bioViewModel.StartTime.Hour, bioViewModel.StartTime.Minute, bioViewModel.StartTime.Second);
                    bioViewModel.StartTime = start;
                    DateTime end = new DateTime(bioViewModel.DateBio.Year, bioViewModel.DateBio.Month, bioViewModel.DateBio.Day, bioViewModel.EndTime.Hour, bioViewModel.EndTime.Minute, bioViewModel.EndTime.Second);
                    bioViewModel.EndTime = end;
                    bioEntity = await _converterHelper.ToBioEntity(bioViewModel, true, user_logged.UserName);

                    if (User.IsInRole("Documents_Assistant"))
                    {
                        string overlapping = _overlapingHelper.OverlapingDocumentsAssistant(documentAssistant.Id, bioViewModel.StartTime, bioViewModel.EndTime, bioViewModel.Id, DocumentDescription.Bio);
                        if ( overlapping != string.Empty)
                        {
                            ModelState.AddModelError(string.Empty, $"Error. There are documents created in that time interval " + overlapping);
                            bioViewModel.Client = _context.Clients
                                                          .Include(n => n.LegalGuardian)
                                                          .Include(n => n.EmergencyContact)
                                                          .Include(n => n.MedicationList)
                                                          .Include(n => n.Client_Referred)
                                                          .ThenInclude(n => n.Referred)
                                                          .Include(n => n.List_BehavioralHistory)
                                                          .Include(f => f.Clients_Diagnostics)
                                                          .ThenInclude(f => f.Diagnostic) 
                                                          .FirstOrDefault(n => n.Id == bioViewModel.Client_FK);
                            bioViewModel.Appetite_Status = _combosHelper.GetComboBio_Appetite();
                            bioViewModel.IdHydratation = 0;
                            bioViewModel.Hydratation_Status = _combosHelper.GetComboBio_Hydration();
                            bioViewModel.IdRecentWeight = 0;
                            bioViewModel.RecentWeight_Status = _combosHelper.GetComboBio_RecentWeight();
                            bioViewModel.IdIfSexuallyActive = 0;
                            bioViewModel.IfSexuallyActive_Status = _combosHelper.GetComboBio_IfSexuallyActive();

                            return View(bioViewModel);
                        }
                    }

                    if (documentAssistant != null)
                    {
                        bioEntity.DocumentsAssistant = documentAssistant;
                        bioEntity.DateSignatureUnlicensedTherapist = DateTime.Now;
                    }
                    else
                    {
                        bioEntity.DateSignatureLicensedPractitioner = DateTime.Now;
                    }
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
                                                 .Include(n => n.Client_Referred)
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
                ApproximateDateReport = string.Empty,
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
                DateAbuse = string.Empty,
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
                ForHowLong = "",
                CodeBill = bioViewModel.CodeBill,
                Code90791 = false
            };

            return View(bioViewModel);
            //return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult Edit(int id = 0, int origi = 0)
        {
            BioEntity entity = _context.Bio

                                       .Include(m => m.Client)
                                       .Include(n => n.Client.Doctor)
                                       .Include(n => n.Client.LegalGuardian)
                                       .Include(n => n.Client.EmergencyContact)
                                       .Include(n => n.Client.MedicationList)
                                       .Include(n => n.Client.Client_Referred)
                                       .ThenInclude(n => n.Referred)
                                       .Include(n => n.Client.List_BehavioralHistory)
                                       .Include(f => f.Client)
                                       .ThenInclude(f => f.Clients_Diagnostics)
                                       .ThenInclude(f => f.Diagnostic)
                                       
                                       .FirstOrDefault(i => i.Client.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Create", new { id = id });
            }

            BioViewModel model;

            if (User.IsInRole("Supervisor") || User.IsInRole("Documents_Assistant"))
            {
                //redirect to bio print report
                if (entity.Status == BioStatus.Approved)
                {
                    return RedirectToAction("PrintBio", new { id = entity.Id });
                }
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
                    if (model.Client.Client_Referred == null || model.Client.Client_Referred.Where(n => n.Service == ServiceAgency.CMH).Count() == 0)
                    {
                        Client_Referred client_referred = new Client_Referred();
                        model.Client.Client_Referred = new List<Client_Referred>();
                        model.Client.Client_Referred.Add(client_referred);
                        model.ReferralName = "Not have referred";
                    }
                    else
                    {
                        model.ReferralName = model.Client.Client_Referred.Where(n => n.Service == ServiceAgency.CMH).ElementAt(0).Referred.Name;
                    }

                    if (model.Client.FarsFormList == null)
                        model.Client.FarsFormList = new List<FarsFormEntity>();
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    if (model.Client.List_BehavioralHistory == null)
                        model.Client.List_BehavioralHistory = new List<Bio_BehavioralHistoryEntity>();

                    model.LegalGuardianName = model.Client.LegalGuardian.Name;
                    model.LegalGuardianTelephone = model.Client.LegalGuardian.Telephone;
                    model.EmergencyContactName = model.Client.EmergencyContact.Name;
                    model.EmergencyContactTelephone = model.Client.EmergencyContact.Telephone;
                    model.RelationShipOfEmergencyContact = model.Client.RelationShipOfEmergencyContact.ToString();

                    ViewData["origi"] = origi;
                    return View(model);
                }
            }

            model = new BioViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> Edit(BioViewModel bioViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                //calcular las unidades a partir del tiempo de desarrollo del BIO
                int units = (bioViewModel.EndTime.TimeOfDay - bioViewModel.StartTime.TimeOfDay).Minutes / 15;
                if ((bioViewModel.EndTime.TimeOfDay - bioViewModel.StartTime.TimeOfDay).Minutes % 15 > 7)
                {
                    units++;
                    bioViewModel.Units = units;
                }
                else
                {
                    bioViewModel.Units = units;
                }

                DateTime start = new DateTime(bioViewModel.DateBio.Year, bioViewModel.DateBio.Month, bioViewModel.DateBio.Day, bioViewModel.StartTime.Hour, bioViewModel.StartTime.Minute, bioViewModel.StartTime.Second);
                bioViewModel.StartTime = start;
                DateTime end = new DateTime(bioViewModel.DateBio.Year, bioViewModel.DateBio.Month, bioViewModel.DateBio.Day, bioViewModel.EndTime.Hour, bioViewModel.EndTime.Minute, bioViewModel.EndTime.Second);
                bioViewModel.EndTime = end;
                BioEntity bioEntity = await _converterHelper.ToBioEntity(bioViewModel, false, user_logged.UserName);

                if (User.IsInRole("Documents_Assistant"))
                {
                    DocumentsAssistantEntity documentAssistant = _context.DocumentsAssistant.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                    string overlapping = _overlapingHelper.OverlapingDocumentsAssistant(documentAssistant.Id, bioViewModel.StartTime, bioViewModel.EndTime, bioViewModel.Id, DocumentDescription.Bio);
                    if (overlapping != string.Empty)
                    {
                        ModelState.AddModelError(string.Empty, $"Error. There are documents created in that time interval " + overlapping);
                        bioViewModel.Client = _context.Clients
                                                      .Include(n => n.LegalGuardian)
                                                      .Include(n => n.EmergencyContact)
                                                      .Include(n => n.MedicationList)
                                                      .Include(n => n.Client_Referred)
                                                      .ThenInclude(n => n.Referred)
                                                      .Include(n => n.List_BehavioralHistory)
                                                      .Include(f => f.Clients_Diagnostics)
                                                      .ThenInclude(f => f.Diagnostic)
                                                      .FirstOrDefault(n => n.Id == bioViewModel.Client_FK);
                        bioViewModel.Appetite_Status = _combosHelper.GetComboBio_Appetite();
                        bioViewModel.IdHydratation = 0;
                        bioViewModel.Hydratation_Status = _combosHelper.GetComboBio_Hydration();
                        bioViewModel.IdRecentWeight = 0;
                        bioViewModel.RecentWeight_Status = _combosHelper.GetComboBio_RecentWeight();
                        bioViewModel.IdIfSexuallyActive = 0;
                        bioViewModel.IfSexuallyActive_Status = _combosHelper.GetComboBio_IfSexuallyActive();
                        ViewData["origi"] = origi;
                        return View(bioViewModel);
                    }
                }

                _context.Bio.Update(bioEntity);
                try
                {
                    List<MessageEntity> messages = bioEntity.Messages.Where(m => (m.Status == MessageStatus.NotRead && m.Notification == false)).ToList();
                    //todos los mensajes no leidos que tiene el bio los pongo como leidos
                    foreach (MessageEntity value in messages)
                    {
                        value.Status = MessageStatus.Read;
                        value.DateRead = DateTime.Now;
                        _context.Update(value);

                        //I generate a notification to supervisor
                        MessageEntity notification = new MessageEntity
                        {
                            Workday_Client = null,
                            FarsForm = null,
                            MTPReview = null,
                            Addendum = null,
                            Discharge = null,
                            Mtp = null,
                            Bio = bioEntity,
                            Title = "Update on reviewed BIO",
                            Text = $"The BIO document of {bioEntity.Client.Name} that was evaluated on {bioEntity.DateBio.ToShortDateString()} was rectified",
                            From = value.To,
                            To = value.From,
                            DateCreated = DateTime.Now,
                            Status = MessageStatus.NotRead,
                            Notification = true
                        };
                        _context.Add(notification);
                    }

                    await _context.SaveChangesAsync();
                    if(origi == 0)
                    {
                        return RedirectToAction("Index", "Bios");
                    }
                    if (origi == 1)
                    {
                        return RedirectToAction("MessagesOfBio", "Messages");
                    }
                    if (origi == 2)
                    {
                        return RedirectToAction("BioWithReview","Bios");
                    }
                    if (origi == 3)
                    {
                        return RedirectToAction("IndexDocumentsAssistant", "Calendar");
                    }
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return View(bioViewModel);
            // return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", bioViewModel) });
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

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant")]
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
                if (User.IsInRole("Documents_Assistant"))
                {
                    DocumentsAssistantEntity document_Assisstant = _context.DocumentsAssistant.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                    return View(await _context.Clients
                                              .Include(f => f.Clients_Diagnostics)
                                              .Include(g => g.Bio)
                                              .Include(g => g.List_BehavioralHistory)

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id
                                                       && n.Bio.DocumentsAssistant.Id == document_Assisstant.Id)
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

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, Frontdesk")]
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

                                       .Include(b => b.Supervisor)

                                       .Include(b => b.DocumentsAssistant)

                                       .Include(b => b.Client)
                                       .ThenInclude(c => c.Client_Referred)
                                       .ThenInclude(cr => cr.Referred)

                                       .AsSplitQuery()

                                       .FirstOrDefault(i => (i.Id == id));
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }
            
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
            if (entity.Client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
            {
                Stream stream = _reportHelper.CommunityHTCBioReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "PRINCIPLE CARE CENTER INC")
            {
                Stream stream = _reportHelper.PrincipleCCIBioReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "SAPPHIRE MENTAL HEALTH CENTER LLC")
            {
                Stream stream = _reportHelper.SapphireMHCBioReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }            
            if (entity.Client.Clinic.Name == "MEDICAL & REHAB OF HILLSBOROUGH INC")
            {
                Stream stream = _reportHelper.MedicalRehabBioReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "MY FLORIDA CASE MANAGEMENT SERVICES LLC")
            {
                Stream stream = _reportHelper.MyFloridaBioReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "ORION MENTAL HEALTH CENTER LLC")
            {
                Stream stream = _reportHelper.OrionBioReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "ALLIED HEALTH GROUP LLC")
            {
                Stream stream = _reportHelper.AlliedBioReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "YOUR NEIGHBOR MEDICAL GROUP")
            {
                Stream stream = _reportHelper.YourNeighborBioReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            return null;
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, Frontdesk")]
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
            if (User.IsInRole("Facilitator"))
            {
                FacilitatorEntity facilitator = await _context.Facilitators.FirstOrDefaultAsync(n => n.LinkedUser == user_logged.UserName);
                List<ClientEntity> ClientList = await _context.Clients
                                                         .Include(n => n.Bio)
                                                         .Where(n => n.Bio == null
                                                           && n.Brief == null
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
                                                                  .Include(n => n.Bio)
                                                                  .Where(n => n.Bio == null
                                                                           && n.Brief == null
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
                                                                  .Include(n => n.Bio)
                                                                  .Where(n => n.Bio == null
                                                                           && n.Brief == null
                                                                           && n.Clinic.Id == user_logged.Clinic.Id
                                                                           && n.OnlyTCM == false)
                                                                  .ToListAsync();

                    return View(ClientList);
                }
                                
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

        [Authorize(Roles = "Supervisor, Documents_Assistant, Manager, Facilitator, Frontdesk")]
        public IActionResult EditReadOnly(int id = 0, int origi = 0)
        {
            BioEntity entity = _context.Bio

                                       .Include(m => m.Client)
                                       .Include(n => n.Client.LegalGuardian)
                                       .Include(n => n.Client.EmergencyContact)
                                       .Include(n => n.Client.MedicationList)
                                       .Include(n => n.Client.Client_Referred)
                                       .ThenInclude(n => n.Referred)
                                       .Include(n => n.Client.List_BehavioralHistory)
                                       .Include(m => m.Client)
                                       .ThenInclude(n => n.Clients_Diagnostics)
                                       .ThenInclude(n => n.Diagnostic)
                                       .Include(n => n.Client)
                                       .ThenInclude(n => n.Clinic)
                                       .ThenInclude(n => n.Setting)
                                       .FirstOrDefault(i => i.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Create", new { id = id });
            }

            BioViewModel model;

           
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
                   
                    if (model.Client.Client_Referred == null || model.Client.Client_Referred.Where(n => n.Service == ServiceAgency.CMH).Count() == 0)
                    {
                        Client_Referred client_referred = new Client_Referred();
                        model.Client.Client_Referred = new List<Client_Referred>();
                        model.Client.Client_Referred.Add(client_referred);
                        model.ReferralName = "Not have referred";
                    }
                    else
                    {
                        model.ReferralName = model.Client.Client_Referred.Where(n => n.Service == ServiceAgency.CMH).ElementAt(0).Referred.Name;
                    }

                    if (model.Client.FarsFormList == null)
                        model.Client.FarsFormList = new List<FarsFormEntity>();
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    if (model.Client.List_BehavioralHistory == null)
                        model.Client.List_BehavioralHistory = new List<Bio_BehavioralHistoryEntity>();

                    model.LegalGuardianName = model.Client.LegalGuardian.Name;
                    model.LegalGuardianTelephone = model.Client.LegalGuardian.Telephone;
                    model.EmergencyContactName = model.Client.EmergencyContact.Name;
                    model.EmergencyContactTelephone = model.Client.EmergencyContact.Telephone;
                    model.RelationShipOfEmergencyContact = model.Client.RelationShipOfEmergencyContact.ToString();

                    ViewData["origi"] = origi;
                    return View(model);
                }

            ViewData["origi"] = origi;
            model = new BioViewModel();
            return View(model);
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> FinishEditing(int id)
        {
            BioEntity bio = await _context.Bio.FirstOrDefaultAsync(n => n.Id == id);
            if (User.IsInRole("Supervisor"))
            {
                bio.Status = BioStatus.Approved;
                bio.DateSignatureSupervisor = DateTime.Now;
                bio.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.LinkedUser == User.Identity.Name);
            }
            else
            {
                bio.Status = BioStatus.Pending;
            }

            _context.Update(bio);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Approve(BioViewModel model, int id, int origi = 0)
        {

            BioEntity bio = await _context.Bio
                                          .Include(n => n.Client)
                                          .ThenInclude(n => n.Clinic)
                                          .ThenInclude(n => n.Setting)
                                          .FirstOrDefaultAsync(n => n.Id == id);


            bio.Status = BioStatus.Approved;
            bio.DateSignatureSupervisor = model.DateSignatureSupervisor;
            bio.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.LinkedUser == User.Identity.Name);

            if (bio.Client.Clinic.Setting.SupervisorEdit == true)
            {
                //section2
                bio.PresentingProblem = model.PresentingProblem;
                bio.ClientAssessmentSituation = model.ClientAssessmentSituation;
                bio.FamilyAssessmentSituation = model.FamilyAssessmentSituation;
                bio.FamilyEmotional = model.FamilyEmotional;
                bio.LegalAssessment = model.LegalAssessment;

                //section 7
                bio.SubstanceAbuse = model.SubstanceAbuse;
                bio.MilitaryServiceHistory = model.MilitaryServiceHistory;
                bio.VocationalAssesment = model.VocationalAssesment;
                bio.LegalHistory = model.LegalHistory;
                bio.PersonalFamilyPsychiatric = model.PersonalFamilyPsychiatric;
                bio.DoesClientRequired = model.DoesClientRequired;
                bio.DoesClientRequired_Where = model.DoesClientRequired_Where;
                bio.ObtainRelease = model.ObtainRelease;

                //section 12
                bio.TreatmentNeeds = model.TreatmentNeeds;

            }
            _context.Update(bio);

            await _context.SaveChangesAsync();

            if (origi == 1)
            {
                return RedirectToAction(nameof(Pending));
            }
            if (origi == 2)
            {
                return RedirectToAction("Notifications", "Messages");
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Supervisor, Manager, Documents_Assistant, Frontdesk")]
        public async Task<IActionResult> Pending(int idError = 0)
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
                    DocumentsAssistantEntity documentAssistant = _context.DocumentsAssistant.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                    if (User.IsInRole("Documents_Assistant"))
                    {
                        return View(await _context.Bio

                                                  .Include(a => a.Client)
                                                  .ThenInclude(a => a.Clinic)

                                                  .Include(f => f.Messages.Where(m => m.Notification == false))

                                                  .Where(a => (a.Client.Clinic.Id == clinic.Id)
                                                            && a.Status == BioStatus.Pending && (a.DocumentsAssistant.Id == documentAssistant.Id))
                                                  .OrderBy(a => a.Client.Clinic.Name).ToListAsync());

                    }
                    else
                    {
                        return View(await _context.Bio

                                                  .Include(a => a.Client)
                                                  .ThenInclude(a => a.Clinic)

                                                  .Include(f => f.Messages.Where(m => m.Notification == false))

                                                  .Where(a => (a.Client.Clinic.Id == clinic.Id)
                                                            && a.Status == BioStatus.Pending)
                                                  .OrderBy(a => a.Client.Clinic.Name).ToListAsync());

                    }

                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor")]
        public IActionResult AddMessageEntity(int id = 0, int origi = 0)
        {
            if (id == 0)
            {
                return View(new MessageViewModel());
            }
            else
            {
                MessageViewModel model = new MessageViewModel()
                {
                    IdBio = id,
                    Origin = origi
                };

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> AddMessageEntity(MessageViewModel messageViewModel)
        {
            if (ModelState.IsValid)
            {
                MessageEntity model = await _converterHelper.ToMessageEntity(messageViewModel, true);
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                model.From = user_logged.UserName;
                model.To = model.Bio.CreatedBy;
                _context.Add(model);
                await _context.SaveChangesAsync();
            }

            if (messageViewModel.Origin == 1)
                return RedirectToAction("Pending");

            if (messageViewModel.Origin == 2)
                return RedirectToAction("Notifications","Messages");

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Documents_Assistant, Supervisor")]
        public async Task<IActionResult> BioWithReview()
        {
            if (User.IsInRole("Documents_Assistant"))
            {
                List<BioEntity> salida = await _context.Bio
                                                       .Include(wc => wc.Client)
                                                       .Include(wc => wc.DocumentsAssistant)
                                                       .Include(wc => wc.Messages.Where(m => m.Notification == false))
                                                       .Where(wc => (wc.DocumentsAssistant.LinkedUser == User.Identity.Name
                                                               && wc.Status == BioStatus.Pending
                                                               && wc.Messages.Count() > 0))
                                                       .ToListAsync();


                return View(salida);
            }

            if (User.IsInRole("Supervisor"))
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    List<BioEntity> salida = await _context.Bio
                                                      .Include(wc => wc.Client)
                                                      .Include(wc => wc.DocumentsAssistant)
                                                      .Include(wc => wc.Messages.Where(m => m.Notification == false))
                                                      .Where(wc => (wc.Client.Clinic.Id == user_logged.Clinic.Id
                                                              && wc.Status == BioStatus.Pending
                                                              && wc.Messages.Count() > 0))
                                                      .ToListAsync();
                    return View(salida);
                }
            }

            return View();
        }

        [Authorize(Roles = "Manager, Supervisor, Documents_Assistant")]
        public IActionResult AddDiagnostic(int id = 0)
        {
            if (id > 0)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                Client_DiagnosticViewModel model = new Client_DiagnosticViewModel
                {
                    IdDiagnostic = 0,
                    Diagnostics = _combosHelper.GetComboDiagnosticsByClient(id),
                    IdClient = id
                };
                return View(model);
            }
            else
            {
                //Edit
                return View(new Client_DiagnosticViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> AddDiagnostic(Client_DiagnosticViewModel client_diagnosticViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (ModelState.IsValid)
            {
                DiagnosticEntity diagnostic = await _context.Diagnostics.FirstOrDefaultAsync(d => d.Id == client_diagnosticViewModel.IdDiagnostic);
                ClientEntity client = await _context.Clients.FirstOrDefaultAsync(n => n.Id == client_diagnosticViewModel.IdClient);

                Client_Diagnostic client_diagnostic = new Client_Diagnostic
                {
                    Id = 0,
                    Client = client,
                    Diagnostic = diagnostic,
                    Principal = client_diagnosticViewModel.Principal
                   
                };
                
                _context.Add(client_diagnostic);
                try
                {
                    await _context.SaveChangesAsync();
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDiagnostic", _context.Clients_Diagnostics.Include(n => n.Diagnostic).Where(d => (d.Client.Id == client.Id )).ToList()) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the diagnostic: {diagnostic.Code}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddDiagnostic", client_diagnosticViewModel) });
        }

        [Authorize(Roles = "Manager, Supervisor, Documents_Assistant")]
        public IActionResult DeleteDiagnostic(int id = 0)
        {
            if (id > 0)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                DeleteViewModel model = new DeleteViewModel
                {
                    Id_Element = id,
                    Desciption = "Do you want to delete this record?"

                };
                return View(model);
            }
            else
            {
                //Edit
                return View(new Client_DiagnosticViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Supervisor, Documents_Assistant")]
        public async Task<IActionResult> DeleteDiagnostic(int id, DeleteViewModel ClientDiagnosticsViewModel)
        {
            if (ModelState.IsValid)
            {
                Client_Diagnostic client_diagnostic = await _context.Clients_Diagnostics.Include(n => n.Client).FirstOrDefaultAsync(d => d.Id == ClientDiagnosticsViewModel.Id_Element);
                try
                {
                    _context.Clients_Diagnostics.Remove(client_diagnostic);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return RedirectToAction("Index", new { idError = 1 });
                }

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDiagnostic", _context.Clients_Diagnostics.Include(n => n.Diagnostic).Where(d => d.Client.Id == client_diagnostic.Client.Id).ToList()) });
            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDiagnostic", _context.Clients_Diagnostics.Where(d => d.Client.Id == 0).ToList()) });
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
        public async Task<IActionResult> AuditBIO()
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic || !user_logged.Clinic.Setting.MHProblems)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<AuditBIO> auditClient_List = new List<AuditBIO>();
            AuditBIO auditClient = new AuditBIO();

            List<ClientEntity> client_List = new List<ClientEntity>();

            if (User.IsInRole("Manager") || User.IsInRole("Supervisor"))
            {
                client_List = _context.Clients
                                      .Include(m => m.Bio)
                                      .Include(m => m.List_BehavioralHistory)
                                      .Include(m => m.MedicationList)
                                      .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                      .ToList();

            }
            else
            {
                if (User.IsInRole("Facilitator"))
                {
                    FacilitatorEntity facilitator = await _context.Facilitators.FirstOrDefaultAsync(f => f.LinkedUser == user_logged.UserName);

                    client_List = _context.Clients
                                         
                                          .Include(m => m.IndividualTherapyFacilitator)
                                          .Where(n => n.Clinic.Id == user_logged.Clinic.Id
                                              && n.MTPs.Count() > 0 && (n.IdFacilitatorPSR == facilitator.Id || n.IndividualTherapyFacilitator.Id == facilitator.Id || n.IdFacilitatorGroup == facilitator.Id))
                                          .ToList();

                }

            }

            foreach (var item in client_List.OrderBy(n => n.AdmisionDate))
            {
                if (item.Bio == null)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "The client has no BIO";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditBIO();
                }
                else
                {
                    if (item.AdmisionDate > item.Bio.DateBio)
                    {
                        auditClient.NameClient = item.Name;
                        auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                        auditClient.Description = "The admission date is after the BIO date";
                        auditClient.Active = 0;

                        auditClient_List.Add(auditClient);
                        auditClient = new AuditBIO();
                    }
                    if (item.List_BehavioralHistory.Count() == 0)
                    {
                        auditClient.NameClient = item.Name;
                        auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                        auditClient.Description = "The client has no behavioral health history";
                        auditClient.Active = 0;

                        auditClient_List.Add(auditClient);
                        auditClient = new AuditBIO();
                    }
                    if (item.MedicationList.Count() == 0)
                    {
                        auditClient.NameClient = item.Name;
                        auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                        auditClient.Description = "The client has no medication";
                        auditClient.Active = 1;

                        auditClient_List.Add(auditClient);
                        auditClient = new AuditBIO();
                    }
                    if (item.Bio.Status == BioStatus.Edition)
                    {
                        auditClient.NameClient = item.Name;
                        auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                        auditClient.Description = "Bio is edition";
                        auditClient.Active = 1;

                        auditClient_List.Add(auditClient);
                        auditClient = new AuditBIO();
                    }
                    if (item.Bio.Status == BioStatus.Pending)
                    {
                        auditClient.NameClient = item.Name;
                        auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                        auditClient.Description = "Bio is pending";
                        auditClient.Active = 1;

                        auditClient_List.Add(auditClient);
                        auditClient = new AuditBIO();
                    }
                }
         
            }

            return View(auditClient_List);
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult SelectBIOorBrief(int idClient = 0)
        {
            BIOTypeViewModel model = new BIOTypeViewModel()
            {
                IdClient = idClient,
                IdType = 0,
                Types = _combosHelper.GetComboBio_Type()

            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> SelectBIOorBrief(BIOTypeViewModel BioTypeViewModel)
        {
            UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (BioTypeViewModel.IdType == 0)
            {
                return RedirectToAction("Create", "Bios", new { id = BioTypeViewModel.IdClient });
            }
            else
            {
                return RedirectToAction("Create", "Briefs", new { id = BioTypeViewModel.IdClient, origi = 1});
            }
            
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ReturnTo(int? id, int clientId = 0, BioStatus aStatus = BioStatus.Edition)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            BioEntity bioEntity = await _context.Bio.FirstOrDefaultAsync(s => s.Id == id);
            if (bioEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                bioEntity.Status = aStatus;
                _context.Bio.Update(bioEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("ClientHistory", "Clients", new { idClient = clientId });
        }

        #region Bill week
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> BillBIOToday(int id = 0, int week = 0, int origin = 0)
        {
            if (id > 0)
            {
                BioEntity bio = await _context.Bio
                                              .Include(n => n.Client)
                                              .FirstOrDefaultAsync(n => n.Id == id);

                bio.BilledDate = DateTime.Now;
                _context.Update(bio);
                await _context.SaveChangesAsync();

                if (origin == 0)
                {
                    return RedirectToAction("BillingWeek", "Notes", new { id = week });
                }
                else
                {
                    return RedirectToAction("BillingClient", "Notes", new { idClient = bio.Client.Id });
                }
            }


            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> NotBill(int id = 0, int week = 0, int origin = 0)
        {
            if (id > 0)
            {
                BioEntity bio = await _context.Bio
                                              .Include(n => n.Client)
                                              .FirstOrDefaultAsync(n => n.Id == id);

                bio.BilledDate = null;
                _context.Update(bio);
                await _context.SaveChangesAsync();

                if (origin == 0)
                {
                    return RedirectToAction("BillingWeek", "Notes", new { id = week, billed = 1 });
                }
                else
                {
                    return RedirectToAction("BillingClient", "Notes", new { idClient = bio.Client.Id, billed = 1 });
                }
            }

            return RedirectToAction("NotAuthorized", "Account");

        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeniedBillToday(int idBio = 0, int week = 0, int origin = 0)
        {
            if (idBio >= 0)
            {
                BioEntity bio = await _context.Bio
                                              .Include(n => n.Client)
                                              .FirstOrDefaultAsync(wc => wc.Id == idBio);

                bio.DeniedBill = true;
                _context.Update(bio);
                await _context.SaveChangesAsync();

                if (origin == 0)
                {
                    return RedirectToAction("BillingWeek", "Notes", new { id = week, billed = 1 });
                }
                else
                {
                    return RedirectToAction("BillingClient", "Notes", new { idClient = bio.Client.Id, billed = 1 });
                }
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> NotDeniedBill(int idBio = 0, int client = 0, int week = 0)
        {
            if (idBio > 0)
            {
                BioEntity bio = await _context.Bio
                                              .Include(n => n.Client)
                                              .FirstOrDefaultAsync(wc => wc.Id == idBio);

                bio.DeniedBill = false;
                _context.Update(bio);
                await _context.SaveChangesAsync();

                if (client == 0 && week > 0)
                {
                    return RedirectToAction("BillingWeek", "Notes", new { id = week, billed = 1 });
                }
                else
                {
                    return RedirectToAction("BillingClient", "Notes", new { idClient = bio.Client.Id, billed = 1 });
                }


            }

            return RedirectToAction("NotAuthorized", "Account");

        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> NotPaymentReceivedBIO(int id = 0, int week = 0, int origin = 0)
        {
            if (id > 0)
            {
                BioEntity bio = await _context.Bio
                                              .Include(n => n.Client)
                                              .FirstOrDefaultAsync(wc => wc.Id == id);

                bio.PaymentDate = null;
                _context.Update(bio);
                await _context.SaveChangesAsync();

                if (origin == 0)
                {
                    return RedirectToAction("BillingWeek", "Notes", new { id = week, billed = 1 });
                }
                else
                {
                    return RedirectToAction("BillingClient", "Notes", new { idClient = bio.Client.Id, billed = 1 });
                }
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> PaymentReceivedTodayBIO(int id = 0, int week = 0, int origin = 0)
        {
            if (id > 0)
            {
                BioEntity bio = await _context.Bio
                                              .Include(n => n.Client)
                                              .FirstOrDefaultAsync(wc => wc.Id == id);

                bio.PaymentDate = DateTime.Now;
                bio.DeniedBill = false;
                _context.Update(bio);
                await _context.SaveChangesAsync();

                if (origin == 0)
                {
                    return RedirectToAction("BillingWeek", "Notes", new { id = week, billed = 1 });
                }
                else
                {
                    return RedirectToAction("BillingClient", "Notes", new { idClient = bio.Client.Id, billed = 1 });
                }
            }

            return RedirectToAction("NotAuthorized", "Account");
        }
        #endregion

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> AutoSave(string jsonModel)
        {
            UserEntity user_logged = await _context.Users
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            BioTempEntity model = JsonConvert.DeserializeObject<BioTempEntity>(jsonModel);

            BioTempEntity bio = await _context.BioTemp
                                              .FirstOrDefaultAsync(b => (b.UserName == user_logged.UserName && b.Url == model.Url));

            if (bio == null)
            {
                model.UserName = user_logged.UserName;

                _context.Add(model);
                try
                {
                    await _context.SaveChangesAsync();
                    return Json(true);
                }
                catch (Exception)
                {
                    return Json(false);
                }
            }
            else
            {
                bio.CMH = model.CMH;
                bio.Priv = model.Priv;
                bio.Setting = model.Setting;
                bio.BioH0031HN = model.BioH0031HN;
                bio.IDAH0031HO = model.IDAH0031HO;
                bio.Code90791 = model.Code90791;
                bio.DateBio = model.DateBio;
                bio.StartTime = model.StartTime;
                bio.EndTime = model.EndTime;
                bio.PresentingProblem = model.PresentingProblem;
                bio.ClientAssessmentSituation = model.ClientAssessmentSituation;
                bio.FamilyAssessmentSituation = model.FamilyAssessmentSituation;
                bio.FamilyEmotional = model.FamilyEmotional;
                bio.LegalAssessment = model.LegalAssessment;
                bio.Appearance_Disheveled = model.Appearance_Disheveled;
                bio.Appearance_FairHygiene = model.Appearance_FairHygiene;
                bio.Appearance_Cleaned = model.Appearance_Cleaned;
                bio.Appearance_WellGroomed = model.Appearance_WellGroomed;
                bio.Appearance_Bizarre = model.Appearance_Bizarre;
                bio.Motor_Normal = model.Motor_Normal;
                bio.Motor_Agitated = model.Motor_Agitated;
                bio.Motor_Retardation = model.Motor_Retardation;
                bio.Motor_RestLess = model.Motor_RestLess;
                bio.Motor_Akathisia = model.Motor_Akathisia;
                bio.Motor_Tremor = model.Motor_Tremor;
                bio.Motor_Other = model.Motor_Other;
                bio.Speech_Normal = model.Speech_Normal;
                bio.Speech_Loud = model.Speech_Loud;
                bio.Speech_Pressured = model.Speech_Pressured;
                bio.Speech_Impoverished = model.Speech_Impoverished;
                bio.Speech_Slurred = model.Speech_Slurred;
                bio.Speech_Mumbled = model.Speech_Mumbled;
                bio.Speech_Stutters = model.Speech_Stutters;
                bio.Speech_Rapid = model.Speech_Rapid;
                bio.Speech_Slow = model.Speech_Slow;
                bio.Speech_Other = model.Speech_Other;
                bio.Affect_Appropriate = model.Affect_Appropriate;
                bio.Affect_labile = model.Affect_labile;
                bio.Affect_Expansive = model.Affect_Expansive;
                bio.Affect_Blunted = model.Affect_Blunted;
                bio.Affect_Constricted = model.Affect_Constricted;
                bio.Affect_Flat = model.Affect_Flat;
                bio.Affect_Tearful_Sad = model.Affect_Tearful_Sad;
                bio.Affect_Anxious = model.Affect_Anxious;
                bio.Affect_Angry = model.Affect_Angry;
                bio.Affect_Other = model.Affect_Other;
                bio.ThoughtProcess_Organized = model.ThoughtProcess_Organized;
                bio.ThoughtProcess_Disorganized = model.ThoughtProcess_Disorganized;
                bio.ThoughtProcess_GoalDirected = model.ThoughtProcess_GoalDirected;
                bio.ThoughtProcess_Irrational = model.ThoughtProcess_Irrational;
                bio.ThoughtProcess_Rigid = model.ThoughtProcess_Rigid;
                bio.ThoughtProcess_Obsessive = model.ThoughtProcess_Obsessive;
                bio.ThoughtProcess_Tangential = model.ThoughtProcess_Tangential;
                bio.ThoughtProcess_Circumstantial = model.ThoughtProcess_Circumstantial;
                bio.ThoughtProcess_Preoccupied = model.ThoughtProcess_Preoccupied;
                bio.ThoughtProcess_Blocking = model.ThoughtProcess_Blocking;
                bio.ThoughtProcess_FightIdeas = model.ThoughtProcess_FightIdeas;
                bio.ThoughtProcess_LooseAssociations = model.ThoughtProcess_LooseAssociations;
                bio.ThoughtProcess_Other = model.ThoughtProcess_Other;
                bio.Mood_Euthymic = model.Mood_Euthymic;
                bio.Mood_Depressed = model.Mood_Depressed;
                bio.Mood_Anxious = model.Mood_Anxious;
                bio.Mood_Euphoric = model.Mood_Euphoric;
                bio.Mood_Angry = model.Mood_Angry;
                bio.Mood_Maniac = model.Mood_Maniac;
                bio.Mood_Other = model.Mood_Other;
                bio.Judgment_Good = model.Judgment_Good;
                bio.Judgment_Fair = model.Judgment_Fair;
                bio.Judgment_Poor = model.Judgment_Poor;
                bio.Judgment_Other = model.Judgment_Other;
                bio.Insight_Good = model.Insight_Good;
                bio.Insight_Fair = model.Insight_Fair;
                bio.Insight_Poor = model.Insight_Poor;
                bio.Insight_Other = model.Insight_Other;
                bio.ThoughtContent_Relevant = model.ThoughtContent_Relevant;
                bio.ThoughtContent_Hallucinations = model.ThoughtContent_Hallucinations;
                bio.ThoughtContent_Hallucinations_Type = model.ThoughtContent_Hallucinations_Type;
                bio.ThoughtContent_RealityBased = model.ThoughtContent_RealityBased;
                bio.ThoughtContent_Delusions = model.ThoughtContent_Delusions;
                bio.ThoughtContent_Delusions_Type = model.ThoughtContent_Delusions_Type;
                bio.Oriented_FullOriented = model.Oriented_FullOriented;
                bio.Lacking_Time = model.Lacking_Time;
                bio.Lacking_Place = model.Lacking_Place;
                bio.Lacking_Person = model.Lacking_Person;
                bio.Lacking_Location = model.Lacking_Location;
                bio.RiskToSelf_Low = model.RiskToSelf_Low;
                bio.RiskToSelf_Medium = model.RiskToSelf_Medium;
                bio.RiskToSelf_High = model.RiskToSelf_High;
                bio.RiskToSelf_Chronic = model.RiskToSelf_Chronic;
                bio.RiskToOther_Low = model.RiskToOther_Low;
                bio.RiskToOther_Medium = model.RiskToOther_Medium;
                bio.RiskToOther_High = model.RiskToOther_High;
                bio.RiskToOther_Chronic = model.RiskToOther_Chronic;
                bio.SafetyPlan = model.SafetyPlan;
                bio.Comments = model.Comments;
                bio.ClientDenied = model.ClientDenied;
                bio.HaveYouEverThought = model.HaveYouEverThought;
                bio.DoYouOwn = model.DoYouOwn;
                bio.DoesClient = model.DoesClient;
                bio.HaveYouEverBeen = model.HaveYouEverBeen;
                bio.HasTheClient = model.HasTheClient;
                bio.HaveYouEverThought_Explain = model.HaveYouEverThought_Explain;
                bio.DoYouOwn_Explain = model.DoYouOwn_Explain;
                bio.HaveYouEverBeen_Explain = model.HaveYouEverBeen_Explain;
                bio.HasTheClient_Explain = model.HasTheClient_Explain;
                bio.ClientFamilyAbusoTrauma = model.ClientFamilyAbusoTrauma;
                bio.DateAbuse = model.DateAbuse;
                bio.PersonInvolved = model.PersonInvolved;
                bio.ApproximateDateReport = model.ApproximateDateReport;
                bio.ApproximateDateReport_Where = model.ApproximateDateReport_Where;
                bio.RelationShips = model.RelationShips;
                bio.Details = model.Details;
                bio.Outcome = model.Outcome;
                bio.AReferral = model.AReferral;
                bio.AReferral_Services = model.AReferral_Services;
                bio.AReferral_When = model.AReferral_When;
                bio.AReferral_Where = model.AReferral_Where;
                bio.ObtainRelease = model.ObtainRelease;
                bio.WhereRecord = model.WhereRecord;
                bio.WhereRecord_When = model.WhereRecord_When;
                bio.WhereRecord_Where = model.WhereRecord_Where;
                bio.HasTheClientVisitedPhysician = model.HasTheClientVisitedPhysician;
                bio.HasTheClientVisitedPhysician_Reason = model.HasTheClientVisitedPhysician_Reason;
                bio.HasTheClientVisitedPhysician_Date = model.HasTheClientVisitedPhysician_Date;
                bio.DoesTheClientExperience = model.DoesTheClientExperience;
                bio.DoesTheClientExperience_Where = model.DoesTheClientExperience_Where;
                bio.ForHowLong = model.ForHowLong;
                bio.PleaseRatePain = model.PleaseRatePain;
                bio.HasClientBeenTreatedPain = model.HasClientBeenTreatedPain;
                bio.HasClientBeenTreatedPain_PleaseIncludeService = model.HasClientBeenTreatedPain_PleaseIncludeService;
                bio.HasClientBeenTreatedPain_Ifnot = model.HasClientBeenTreatedPain_Ifnot;
                bio.HasClientBeenTreatedPain_Where = model.HasClientBeenTreatedPain_Where;
                bio.ObtainReleaseInformation = model.ObtainReleaseInformation;
                bio.EastFewer = model.EastFewer;
                bio.EastFew = model.EastFew;
                bio.Has3OrMore = model.Has3OrMore;
                bio.HasTooth = model.HasTooth;
                bio.DoesNotAlways = model.DoesNotAlways;
                bio.EastAlone = model.EastAlone;
                bio.Takes3OrMore = model.Takes3OrMore;
                bio.WithoutWanting = model.WithoutWanting;
                bio.NotAlwaysPhysically = model.NotAlwaysPhysically;
                bio.AnyFood = model.AnyFood;
                bio.AnyEating = model.AnyEating;
                bio.If6_ReferredTo = model.If6_ReferredTo;
                bio.If6_Date = model.If6_Date;
                bio.Appetite = model.Appetite;
                bio.Hydration = model.Hydration;
                bio.RecentWeight = model.RecentWeight;
                bio.SubstanceAbuse =  model.SubstanceAbuse;
                bio.MilitaryServiceHistory = model.MilitaryServiceHistory;
                bio.MilitaryServiceHistory_Explain = model.MilitaryServiceHistory_Explain;
                bio.VocationalAssesment = model.VocationalAssesment;
                bio.LegalHistory = model.LegalHistory;
                bio.PersonalFamilyPsychiatric = model.PersonalFamilyPsychiatric;
                bio.DoesClientRequired = model.DoesClientRequired;
                bio.DoesClientRequired_Where = model.DoesClientRequired_Where;
                bio.ObtainReleaseInformation7 = model.ObtainReleaseInformation7;
                bio.IfForeing_Born = model.IfForeing_Born;
                bio.IfForeing_AgeArrival = model.IfForeing_AgeArrival;
                bio.IfForeing_YearArrival = model.IfForeing_YearArrival;
                bio.PrimaryLocation = model.PrimaryLocation;
                bio.GeneralDescription = model.GeneralDescription;
                bio.AdultCurrentExperience = model.AdultCurrentExperience;
                bio.WhatIsTheClient = model.WhatIsTheClient;
                bio.RelationshipWithFamily = model.RelationshipWithFamily;
                bio.Children = model.Children;
                bio.IfMarried = model.IfMarried;
                bio.IfSeparated = model.IfSeparated;
                bio.IfSexuallyActive = model.IfSexuallyActive;
                bio.PleaseProvideGoal = model.PleaseProvideGoal;
                bio.DoYouHaveAnyReligious = model.DoYouHaveAnyReligious;
                bio.DoYouHaveAnyVisual = model.DoYouHaveAnyVisual;
                bio.HigHestEducation = model.HigHestEducation;
                bio.DoYouHaveAnyPhysical = model.DoYouHaveAnyPhysical;
                bio.CanClientFollow = model.CanClientFollow;
                bio.ProvideIntegratedSummary = model.ProvideIntegratedSummary;
                bio.TreatmentNeeds = model.TreatmentNeeds;
                bio.Treatmentrecomendations = model.Treatmentrecomendations;
                bio.IConcurWhitDiagnistic = model.IConcurWhitDiagnistic;
                bio.AlternativeDiagnosis = model.AlternativeDiagnosis;
                if(User.IsInRole("Supervisor"))                
                    bio.DateSignatureLicensedPractitioner = model.DateSignatureLicensedPractitioner;                
                else                
                    bio.DateSignatureUnlicensedTherapist = model.DateSignatureUnlicensedTherapist;                

                _context.Update(bio);
                try
                {
                    await _context.SaveChangesAsync();
                    return Json(false);
                }
                catch (Exception)
                {
                    return Json(false);
                }
            }
        }        
    }
}
