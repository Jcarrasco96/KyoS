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
using KyoS.Common.Helpers;


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
                                                 .Include(n => n.Client_Referred)
                                                 .ThenInclude(n => n.Referred)
                                                 .Include(n => n.List_BehavioralHistory)
                                                 .Include(f => f. Clients_Diagnostics)
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
                        //LicensedPractitioner = user_logged.FullName,
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
                        AdmissionedFor = user_logged.FullName,
                        CodeBill = user_logged.Clinic.CodeBIO
                    };
                    if (model.Client.LegalGuardian == null)
                        model.Client.LegalGuardian = new LegalGuardianEntity();
                    if (model.Client.EmergencyContact == null)
                        model.Client.EmergencyContact = new EmergencyContactEntity();
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    if (model.Client.Doctor == null)
                        model.Client.Doctor = new DoctorEntity();
                    if (model.Client.Client_Referred == null || model.Client.Client_Referred.Count() == 0)
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
                    //calcular las unidades a partir del tiempo de desarrollo del MTP
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
                    bioEntity = await _converterHelper.ToBioEntity(bioViewModel, true, user_logged.UserName);

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
                CodeBill = bioViewModel.CodeBill
            };
            
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult Edit(int id = 0, int origi = 0)
        {
            BioEntity entity = _context.Bio

                                       .Include(m => m.Client)
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
                //calcular las unidades a partir del tiempo de desarrollo del MTP
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

                BioEntity bioEntity = await _converterHelper.ToBioEntity(bioViewModel, false, user_logged.UserName);
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

                                       .Include(b => b.Supervisor)

                                       .Include(b => b.DocumentsAssistant)

                                       .Include(b => b.Client)
                                       .ThenInclude(c => c.Client_Referred)
                                       .ThenInclude(cr => cr.Referred)

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
                                                          .Where(n => n.Bio == null
                                                            && n.Brief == null
                                                            && n.Clinic.Id == user_logged.Clinic.Id
                                                            && n.OnlyTCM == false)
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

        [Authorize(Roles = "Supervisor, Documents_Assistant, Manager, Facilitator")]
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
        public async Task<IActionResult> Approve(int id, int origi = 0)
        {

            BioEntity bio = await _context.Bio.FirstOrDefaultAsync(n => n.Id == id);


            bio.Status = BioStatus.Approved;
            bio.DateSignatureSupervisor = DateTime.Now;
            bio.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.LinkedUser == User.Identity.Name);
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

        [Authorize(Roles = "Supervisor, Manager, Documents_Assistant")]
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
    }
}
