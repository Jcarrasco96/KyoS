using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    public class TCMAssessmentsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IReportHelper _reportHelper;
        private readonly IRenderHelper _renderHelper;

        public TCMAssessmentsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IReportHelper reportHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
            _renderHelper = renderHelper;
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
                if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
                    return View(await _context.TCMClient

                                              .Include(f => f.TCMAssessment)
                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clinic)
                                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

                if (User.IsInRole("CaseManager"))
                {

                    ViewData["origin"] = origin.ToString();
                    if (idTCMClient == 0)
                    {
                        List<TCMClientEntity> AssessmentList =  await _context.TCMClient
                                                                        .Include(f => f.Client)
                                                                        .ThenInclude(f => f.Clinic)
                                                                        .Include(f => f.TCMAssessment)
                                                                        .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id
                                                                            && (n.Casemanager.Id == caseManager.Id))
                                                                        .OrderBy(f => f.Client.Name)
                                                                        .ToListAsync();
                        return View(AssessmentList);
                    }
                    else
                    {
                        return View(await _context.TCMClient
                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clinic)
                                              .Include(f => f.TCMAssessment)

                                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id
                                                && (n.Casemanager.Id == caseManager.Id)
                                                 && (n.Id == idTCMClient))
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());
                    }
                    
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Create(int id = 0, int origi = 0)
        {

            TCMAssessmentViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {
                    TCMClientEntity tcmClient = _context.TCMClient
                                                        .Include(n => n.Client)
                                                        .ThenInclude(n => n.Client_Referred)
                                                        .ThenInclude(n => n.Referred)
                                                        .Include(n => n.TCMIntakeForm)
                                                        .FirstOrDefault(n => n.Id == id);

                    if (tcmClient.TCMIntakeForm == null)
                        tcmClient.TCMIntakeForm = new TCMIntakeFormEntity();

                    model = new TCMAssessmentViewModel
                    {
                        Approved = 0,
                        TcmClient = tcmClient,
                        AreChild = false,
                        AreChildAddress = "",
                        AreChildCity = "",
                        AreChildName = "",
                        AreChildPhone = "",
                        Caregiver = false,
                        ChildFather = "",
                        ChildMother = "",
                        ClientInput = false,
                        DateAssessment = DateTime.Now,
                        Divorced = false,
                        Family = false,
                        Id = 0,
                        Married = false,
                        IdYesNoNAWe = 0,
                        YesNoNAs = _combosHelper.GetComboYesNoNA(),
                        NeverMarried = false,
                        Other = false,
                        OtherExplain = "",
                        PresentingProblems = "",
                        Referring = false,
                        Review = false,
                        School = false,
                        Separated = false,
                        TcmClient_FK = id,
                        Treating = false,

                        AcademicEelementary = false,
                        AcademicHigh = false,
                        AcademicMiddle = false,
                        AcademicPreSchool = false,
                        AdditionalInformation = "",
                        AdditionalInformationMigration = "",
                        AHomeVisit = false,
                        AHomeVisitOn = "",
                        Appliances = false,
                        AttendanceEelementary = false,
                        AttendanceHigh = false,
                        AttendanceMiddle = false,
                        AttendancePreSchool = false,
                        BathingAssistive = false,
                        BathingIndependent = false,
                        BathingPhysical = false,
                        BathingSupervision = false,
                        BathingTotal = false,
                        Bathtub = false,
                        BehaviorEelementary = false,
                        BehaviorHigh = false,
                        BehaviorMiddle = false,
                        BehaviorPreSchool = false,
                        Briefly = "",
                        CaseManagerWas = false,
                        CaseManagerWasDueTo = "",
                        Citizen = false,
                        ColonCancer = "",
                        CongredatedHowOften = string.Empty,
                        CongredatedProvider = "",
                        CongredatedReceive = false,
                        ContinueToLive = false,
                        ContinueToLiveOnly = false,
                        CookingAssistive = false,
                        CookingIndependent = false,
                        CookingPhysical = false,
                        CookingSupervision = false,
                        CookingTotal = false,
                        CountryOfBirth = "",
                        CurrentEmployer = "",
                        DentalExam = "",
                        DescribeAnySchool = "",
                        DescribeClientCultural = "",
                        DescribeClientEducation = "",
                        DescribeClientLiving = "",
                        DescribeClientRelationship = "",
                        DescribeNeighborhood = "",
                        DescribeOtherNeedConcerns = "",
                        DoesClientBasicNeed = "",
                        DoesClientCurrently = false,
                        DoesClientCurrentlyExplain = "",
                        DoesClientFeel = false,
                        DoesClientFeelExplain = "",
                        DoesClientNeedAssistance = false,
                        DoesClientNeedAssistanceEducational = false,
                        DoesClientNeedAssistanceEducationalExplain = "",
                        DoesClientNeedAssistanceExplain = "",
                        DoesNotKnow = false,
                        DoingAssistive = false,
                        DoingIndependent = false,
                        DoingPhysical = false,
                        DoingSupervision = false,
                        DoingTotal = false,
                        DressingAssistive = false,
                        DressingIndependent = false,
                        DressingPhysical = false,
                        DressingSupervision = false,
                        DressingTotal = false,
                        Drives = false,
                        Electrical = false,
                        EmployerAddress = "",
                        EmployerCityState = "",
                        EmployerContactPerson = "",
                        EmployerPhone = "",
                        EmploymentStatus = "",
                        ExcessiveCluter = false,
                        FailToEelementary = false,
                        FailToHigh = false,
                        FailToMiddle = false,
                        FailToPreSchool = false,
                        FeedingAssistive = false,
                        FeedingIndependent = false,
                        FeedingPhysical = false,
                        FeedingSupervision = false,
                        FeedingTotal = false,
                        FireHazards = false,
                        Flooring = false,
                        FoodPantryHowOften = string.Empty,
                        FoodPantryProvider = "",
                        FoodPantryReceive = false,
                        FoodStampHowOften = string.Empty,
                        FoodStampProvider = "",
                        FoodStampReceive = false,
                        FriendOrFamily = false,
                        GroomingAssistive = false,
                        GroomingIndependent = false,
                        GroomingPhysical = false,
                        GroomingSupervision = false,
                        GroomingTotal = false,
                        HasClientEverArrest = false,
                        HasClientEverArrestLastTime = "",
                        HasClientEverArrestManyTime = "",
                        HomeDeliveredHowOften = string.Empty,
                        HomeDeliveredProvider = "",
                        HomeDeliveredReceive = false,
                        IfThereAnyHousing = "",
                        IfYesWereCriminal = false,
                        IfYesWhatArea = "",
                        ImmigrationOther = false,
                        ImmigrationOtherExplain = "",
                        Insect = false,
                        IsClientCurrentlyEmployed = false,
                        IsClientCurrentlySchool = false,
                        IsClientCurrentlySchoolExplain = "",
                        IsClientInterested = false,
                        IsClientInvolved = false,
                        IsClientInvolvedSpecify = "",
                        IsTheClientAbleWork = false,
                        IsTheClientAbleWorkLimitation = false,
                        IsTheClientHavingFinancial = false,
                        IsTheClientHavingFinancialExplain = "",
                        IsThereAnyAide = false,
                        IsThereAnyAideName = "",
                        IsThereAnyAidePhone = "",
                        IsThereAnyCurrentLegalProcess = false,
                        LabWorks = "",
                        LearningEelementary = false,
                        LearningHigh = false,
                        LearningMiddle = false,
                        LearningPreSchool = false,
                        ListAnyNeed = "",
                        ListClientCurrentPotencialStrngths = "",
                        ListClientCurrentPotencialWeakness = "",
                        MakingAssistive = false,
                        MakingIndependent = false,
                        MakingPhysical = false,
                        MakingSupervision = false,
                        MakingTotal = false,
                        Mammogram = "",
                        MayWeLeaveSend = false,
                        MonthlyFamilyIncome = string.Empty,
                        NoAirCondition = false,
                        NoTelephone = false,
                        NotHot = false,
                        NumberOfBedrooms = 0,
                        NumberOfPersonLiving = 0,
                        OtherFinancial = "",
                        OtherHowOften = string.Empty,
                        OtherProvider = "",
                        OtherReceive = false,
                        PapAndHPV = "",
                        ParticipationEelementary = false,
                        ParticipationHigh = false,
                        ParticipationMiddle = false,
                        ParticipationPreSchool = false,
                        PersonPorBedrooms = 0,
                        PhysicalExam = "",
                        PhysicalOther = "",
                        PreferToLive = false,
                        Poor = false,
                        ProbationOfficer = false,
                        ProbationOfficerName = "",
                        ProbationOfficerPhone = "",
                        RecommendedActivities = false,
                        RecommendedBasicNeed = false,
                        RecommendedEconomic = false,
                        RecommendedHousing = false,
                        RecommendedLegalImmigration = false,
                        RecommendedMentalHealth = false,
                        RecommendedOther = false,
                        RecommendedOtherSpecify = "",
                        RecommendedPhysicalHealth = false,
                        RecommendedRecreational = false,
                        RecommendedSchool = false,
                        RecommendedTransportation = false,
                        RecommendedVocation = false,
                        RelationshipEelementary = false,
                        RelationshipHigh = false,
                        RelationshipMiddle = false,
                        RelationshipPreSchool = false,
                        Resident = false,
                        ResidentStatus = "",
                        SchoolAddress = "",
                        SchoolCityState = "",
                        SchoolDistrict = "",
                        SchoolGrade = "",
                        SchoolName = "",
                        SchoolProgramEBD = false,
                        SchoolProgramESE = false,
                        SchoolProgramESOL = false,
                        SchoolProgramHHIP = false,
                        SchoolProgramOther = false,
                        SchoolProgramRegular = false,
                        SchoolProgramTeacherName = "",
                        SchoolProgramTeacherPhone = "",
                        ShoppingAssistive = false,
                        ShoppingIndependent = false,
                        ShoppingPhysical = false,
                        ShoppingSupervision = false,
                        ShoppingTotal = false,
                        Staff = false,
                        Stairs = false,
                        Structural = false,
                        TakesABus = false,
                        TransferringAssistive = false,
                        TransferringIndependent = false,
                        TransferringPhysical = false,
                        TransferringSupervision = false,
                        TransferringTotal = false,
                        TransportationOther = false,
                        TransportationOtherExplain = "",
                        Tripping = false,
                        Unsanitary = false,
                        VocationalEmployment = "",
                        Walks = false,
                        WhatActivityThings = "",
                        WhatIsCollegeGraduated = false,
                        WhatIsElementary = false,
                        WhatIsGED = false,
                        WhatIsGraduated = false,
                        WhatIsGraduatedDegree = false,
                        WhatIsHighSchool = false,
                        WhatIsMiddle = false,
                        WhatIsNoSchool = false,
                        WhatIsSomeCollege = false,
                        WhatIsSomeHigh = false,
                        WhatIsTheMainSource = "",
                        WhatIsTradeSchool = false,
                        WhatIsUnknown = false,
                        WouldLikeObtainJob = false,
                        WouldLikeObtainJobNotAtThisTime = false,
                        YearEnteredUsa = 0,
                        HoweverVisitScheduler = DateTime.Now,
                        HoweverOn = "",
                        AbuseViolence = false,
                        Allergy = false,
                        AllergySpecify = "",
                        AnyOther = "",
                        AreAllImmunization = false,
                        AreAllImmunizationExplain = "",
                        AreYouPhysician = false,
                        AreYouPhysicianSpecify = "",
                        CantDoItAtAll = false,
                        DateOfOnSetPresentingProblem = DateTime.Now,
                        DateMostRecent = "",
                        DateSignatureCaseManager = DateTime.Now,
                        DateSignatureTCMSupervisor = DateTime.Now,
                        DescribeAnyOther = "",
                        DescribeAnyRisk = "",
                        DoesAggressiveness = false,
                        DoesAnxiety = false,
                        DoesClientTranspotation = false,
                        DoesClientTranspotationExplain = "",
                        DoesDelusions = false,
                        DoesDepression = false,
                        DoesFearfulness = false,
                        DoesHallucinations = false,
                        DoesHelplessness = false,
                        DoesHopelessness = false,
                        DoesHyperactivity = false,
                        DoesImpulsivity = false,
                        DoesIrritability = false,
                        DoesLoss = false,
                        DoesLow = false,
                        DoesMood = false,
                        DoesNegative = false,
                        DoesNervousness = false,
                        DoesObsessive = false,
                        DoesPanic = false,
                        Suicidal = false,
                        Status = TCMDocumentStatus.Edition,
                        DoesParanoia = false,
                        HowActive = "",
                        TCMSupervisor = new TCMSupervisorEntity(),
                        DoesPoor = false,
                        DoesSadness = false,
                        DoesSelfNeglect = false,
                        DoesSheUnderstand = false,
                        DoesSleep = false,
                        DoesTheClientFeel = false,
                        DoesWithdrawal = false,
                        DrugList = new List<TCMAssessmentDrugEntity>(),
                        HasClientUndergone = false,
                        HasDifficultySeeingLevel = false,
                        HasDifficultySeeingObjetive = false,
                        HasNoImpairment = false,
                        HasNoUsefull = false,
                        HasTheClient = false,
                        HaveYouEverBeenToAny = false,
                        HaveYouEverUsedAlcohol = false,
                        HearingDifficulty = false,
                        HearingImpairment = false,
                        HearingNotDetermined = false,
                        Hears = false,
                        Homicidal = false,
                        HospitalList = new List<TCMAssessmentHospitalEntity>(),
                        HouseCompositionList = new List<TCMAssessmentHouseCompositionEntity>(),
                        HowDoesByFollowing = false,
                        HowDoesCalendar = false,
                        HowDoesDaily = false,
                        HowDoesElectronic = false,
                        HowDoesFamily = false,
                        HowDoesKeeping = false,
                        HowDoesOther = false,
                        HowDoesOtherExplain = "",
                        HowDoesPill = false,
                        HowDoesRNHHA = false,
                        HowManyTimes = "",
                        HowWeelEnable = false,
                        HowWeelWithALot = false,
                        HowWeelWithNo = false,
                        HowWeelWithSome = false,
                        IndividualAgencyList = new List<TCMAssessmentIndividualAgencyEntity>(),
                        IsClientCurrently = false,
                        IsClientPregnancy = false,
                        IsClientPregnancyNA = false,
                        IsSheReceiving = false,
                        Issues = "",
                        LastModifiedBy = "",
                        LastModifiedOn = DateTime.Now,
                        LegalDecisionAddress = "",
                        LegalDecisionAdLitem = false,
                        LegalDecisionAttomey = false,
                        LegalDecisionCityStateZip = "",
                        LegalDecisionLegal =  false,
                        LegalDecisionName = "",
                        LegalDecisionNone = false,
                        LegalDecisionOther = false,
                        LegalDecisionOtherExplain = "",
                        LegalDecisionParent = false,
                        LegalDecisionPhone = "",
                        MedicalProblemList = new List<TCMAssessmentMedicalProblemEntity>(),
                        MedicationList = new List<TCMAssessmentMedicationEntity>(),
                        MentalHealth = "",
                        NeedALot = false,
                        NeedNoHelp = false,
                        NeedOfSpecial = false,
                        NeedOfSpecialSpecify = "",
                        NeedSome = false,
                        NoHearing = false,
                        NoUseful = false,
                        OtherReceiveExplain = "",
                        Outcome = "",
                        PastCurrentServiceList = new List<TCMAssessmentPastCurrentServiceEntity>(),
                        PharmacyPhone = "",
                        PresentingProblemPrevious = false,
                        Provider = "",
                        SurgeryList = new List<TCMAssessmentSurgeryEntity>(),
                        TypeOfAssessmentAnnual = false,
                        TypeOfAssessmentInitial = false,
                        TypeOfAssessmentOther = false,
                        TypeOfAssessmentOtherExplain = "",
                        TypeOfAssessmentSignificant = false,
                        VisionImpairment = false,
                        VisionNotDetermined = false,
                        WhatPharmacy = "",
                        WhenWas = "",
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        Client_Referred_List = new List<Client_Referred>(),
                        Psychiatrist_Name = tcmClient.TCMIntakeForm.Psychiatrist_Name,
                        Psychiatrist_Address = tcmClient.TCMIntakeForm.Psychiatrist_Address,
                        Psychiatrist_CityStateZip = tcmClient.TCMIntakeForm.Psychiatrist_CityStateZip,
                        Psychiatrist_Phone = tcmClient.TCMIntakeForm.Psychiatrist_Phone,
                        PCP_Name = tcmClient.TCMIntakeForm.PCP_Name,
                        PCP_Address = tcmClient.TCMIntakeForm.PCP_Address,
                        PCP_CityStateZip = tcmClient.TCMIntakeForm.PCP_CityStateZip,
                        PCP_Phone = tcmClient.TCMIntakeForm.PCP_Phone

                    };
                    if (model.IndividualAgencyList == null)
                        model.IndividualAgencyList = new List<TCMAssessmentIndividualAgencyEntity>();

                    ViewData["origi"] = origi;
                    return View(model);
                }
            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            model = new TCMAssessmentViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Create(TCMAssessmentViewModel tcmAssessmentViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeFormEntity intake = _context.TCMIntakeForms
                                                .FirstOrDefault(u => u.TcmClient.Id == tcmAssessmentViewModel.TcmClient_FK);

            if (ModelState.IsValid)
            {
                if (intake != null)
                {
                    intake.Psychiatrist_Name = tcmAssessmentViewModel.Psychiatrist_Name;
                    intake.Psychiatrist_Address = tcmAssessmentViewModel.Psychiatrist_Address;
                    intake.Psychiatrist_Phone = tcmAssessmentViewModel.Psychiatrist_Phone;
                    intake.Psychiatrist_CityStateZip = tcmAssessmentViewModel.Psychiatrist_CityStateZip;

                    intake.PCP_Name = tcmAssessmentViewModel.PCP_Name;
                    intake.PCP_Address = tcmAssessmentViewModel.PCP_Address;
                    intake.PCP_Phone = tcmAssessmentViewModel.PCP_Phone;
                    intake.PCP_CityStateZip = tcmAssessmentViewModel.PCP_CityStateZip;

                    _context.TCMIntakeForms.Update(intake);
                    await _context.SaveChangesAsync();
                }

                TCMAssessmentEntity tcmAssessmentEntity = _context.TCMAssessment.Find(tcmAssessmentViewModel.Id);
                if (tcmAssessmentEntity == null)
                {
                    tcmAssessmentEntity = await _converterHelper.ToTCMAssessmentEntity(tcmAssessmentViewModel, true, user_logged.UserName);
                    tcmAssessmentEntity.Approved = 0;
                    _context.TCMAssessment.Add(tcmAssessmentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (origi == 0)
                        {
                            TCMAssessmentEntity tcmAssessment = await _context.TCMAssessment
                                                                              .Include(m => m.TcmClient)
                                                                              .ThenInclude(m => m.Client)
                                                                              .Include(m => m.IndividualAgencyList)
                                                                              .Include(m => m.HouseCompositionList)
                                                                              .Include(m => m.MedicationList)
                                                                              .Include(m => m.PastCurrentServiceList)
                                                                              .FirstOrDefaultAsync(m => m.TcmClient.Casemanager.LinkedUser == user_logged.UserName
                                                                                && m.TcmClient.Id == tcmAssessmentViewModel.TcmClient_FK);

                            return RedirectToAction("TCMIntakeSectionDashboard", "TCMIntakes", new { id = tcmAssessmentEntity.TcmClient_FK, section = 4 });
                        }
                        if (origi == 1)
                        {
                            return RedirectToAction("GetCaseNotServicePlan", "TCMClients");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the TCM service.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmAssessmentViewModel) });
                }
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", tcmAssessmentViewModel) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult Edit(int id = 0, int origi = 0)
        {
            TCMAssessmentViewModel model;
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
            
                if (user_logged.Clinic != null)
                {

                    TCMAssessmentEntity TcmAssessment = _context.TCMAssessment
                                                                .Include(b => b.TcmClient)
                                                                .ThenInclude(b => b.Client)
                                                                .ThenInclude(b => b.Clients_Diagnostics)
                                                                .ThenInclude(b => b.Diagnostic)
                                                                .Include(b => b.TcmClient.Client.Client_Referred)
                                                                .ThenInclude(n => n.Referred)
                                                                .Include(b => b.TcmClient.Client.Doctor)
                                                                .Include(b => b.TcmClient.Client.Psychiatrist)
                                                                .Include(b => b.IndividualAgencyList)
                                                                .Include(b => b.HouseCompositionList)
                                                                .Include(b => b.MedicationList)
                                                                .Include(b => b.PastCurrentServiceList)
                                                                .Include(b => b.HospitalList)
                                                                .Include(b => b.DrugList)
                                                                .Include(b => b.MedicalProblemList)
                                                                .Include(b => b.SurgeryList)
                                                                .Include(b => b.TcmClient)
                                                                .ThenInclude(b => b.TCMIntakeForm)
                                                                .FirstOrDefault(m => m.Id == id);
                    if (TcmAssessment == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToTCMAssessmentViewModel(TcmAssessment);

                        if (TcmAssessment.TcmClient.TCMIntakeForm != null)
                        {
                            model.Psychiatrist_Name = model.TcmClient.TCMIntakeForm.Psychiatrist_Name;
                            model.Psychiatrist_Address = model.TcmClient.TCMIntakeForm.Psychiatrist_Address;
                            model.Psychiatrist_Phone = model.TcmClient.TCMIntakeForm.Psychiatrist_Phone;
                            model.Psychiatrist_CityStateZip = model.TcmClient.TCMIntakeForm.Psychiatrist_CityStateZip;

                            model.PCP_Name = model.TcmClient.TCMIntakeForm.PCP_Name;
                            model.PCP_Address = model.TcmClient.TCMIntakeForm.PCP_Address;
                            model.PCP_Phone = model.TcmClient.TCMIntakeForm.PCP_Phone;
                            model.PCP_CityStateZip = model.TcmClient.TCMIntakeForm.PCP_CityStateZip;
                        }
                        ViewData["origi"] = origi;
                        return View(model);
                    }

                }
            }

            model = new TCMAssessmentViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> Edit(TCMAssessmentViewModel tcmAssessmentViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeFormEntity intake = _context.TCMIntakeForms
                                                .FirstOrDefault(u => u.TcmClient.Id == tcmAssessmentViewModel.TcmClient_FK);

            if (ModelState.IsValid)
            {
                if (intake != null)
                {
                    intake.Psychiatrist_Name = tcmAssessmentViewModel.Psychiatrist_Name;
                    intake.Psychiatrist_Address = tcmAssessmentViewModel.Psychiatrist_Address;
                    intake.Psychiatrist_Phone = tcmAssessmentViewModel.Psychiatrist_Phone;
                    intake.Psychiatrist_CityStateZip = tcmAssessmentViewModel.Psychiatrist_CityStateZip;

                    intake.PCP_Name = tcmAssessmentViewModel.PCP_Name;
                    intake.PCP_Address = tcmAssessmentViewModel.PCP_Address;
                    intake.PCP_Phone = tcmAssessmentViewModel.PCP_Phone;
                    intake.PCP_CityStateZip = tcmAssessmentViewModel.PCP_CityStateZip;

                    _context.TCMIntakeForms.Update(intake);
                    await _context.SaveChangesAsync();
                }

                TCMAssessmentEntity tcmAssessmentEntity = await _converterHelper.ToTCMAssessmentEntity(tcmAssessmentViewModel, false, user_logged.UserName);
                if (tcmAssessmentEntity.Approved != 1)
                {
                    tcmAssessmentEntity.Approved = 0;
                }
                if (origi == 3)
                {
                    tcmAssessmentEntity.Approved = 2;
                }
                List<TCMMessageEntity> messages = tcmAssessmentEntity.TcmMessages.Where(m => (m.Status == MessageStatus.NotRead && m.Notification == false)).ToList();
                //todos los mensajes no leidos que tiene el Workday_Client de la nota los pongo como leidos
                foreach (TCMMessageEntity value in messages)
                {
                    value.Status = MessageStatus.Read;
                    value.DateRead = DateTime.Now;
                    _context.Update(value);

                    //I generate a notification to supervisor
                    TCMMessageEntity notification = new TCMMessageEntity
                    {
                        TCMNote = null,
                        TCMFarsForm = null,
                        TCMServicePlan = null,
                        TCMServicePlanReview = null,
                        TCMAddendum = null,
                        TCMDischarge = null,
                        TCMAssessment = tcmAssessmentEntity,
                        Title = "Update on reviewed TCM Assessment",
                        Text = $"The TCM Assessment of {tcmAssessmentEntity.TcmClient.Client.Name} on {tcmAssessmentEntity.DateAssessment.ToShortDateString()} was rectified",
                        From = value.To,
                        To = value.From,
                        DateCreated = DateTime.Now,
                        Status = MessageStatus.NotRead,
                        Notification = true
                    };
                    _context.Add(notification);
                }

                _context.TCMAssessment.Update(tcmAssessmentEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    if (origi == 0)
                    {
                        return RedirectToAction("TCMIntakeSectionDashboard", "TCMIntakes", new { id = tcmAssessmentEntity.TcmClient_FK, section = 4 });
                    }
                    if (origi == 1)
                    {
                        return RedirectToAction("MessagesOfAssessment", "TCMMessages");
                    }
                    if (origi == 2)
                    {
                        return RedirectToAction("GetCaseNotServicePlan", "TCMClients");
                    }
                    if (origi == 3)
                    {
                        return RedirectToAction("Index", "DeskTop");
                    }
                    if (origi == 4)
                    {
                        return RedirectToAction("TCMAssessmentApproved", "TCMAssessments", new { approved = 0});
                    }
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", tcmAssessmentViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> FinishEditing(int id)
        {
            TCMAssessmentEntity tcmAssessment = _context.TCMAssessment.FirstOrDefault(u => u.TcmClient.Id == id);

            if (tcmAssessment != null)
            {
                if (User.IsInRole("CaseManager"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        tcmAssessment.Approved = 1;
                        _context.Update(tcmAssessment);
                        try
                        {
                            await _context.SaveChangesAsync();

                            return RedirectToAction("TCMIntakeSectionDashboard", "TCMIntakes", new { id = tcmAssessment.TcmClient_FK, section = 4 });
                        }
                        catch (System.Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("Index", "TCMAssessments");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> ApproveAssessments(int id, TCMAssessmentViewModel model, int origi = 0)
        {
            TCMAssessmentEntity tcmAssessment = new TCMAssessmentEntity();

            if (tcmAssessment != null)
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    UserEntity user_logged = _context.Users
                                                     .Include(u => u.Clinic)
                                                     .ThenInclude(u => u.Setting)
                                                     .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        if (user_logged.Clinic.Setting.TCMSupervisorEdit == true)
                        {
                            tcmAssessment = await _converterHelper.ToTCMAssessmentEntity(model, false, user_logged.UserName);
                            tcmAssessment.Approved = 2;
                            tcmAssessment.TCMSupervisor = await _context.TCMSupervisors.FirstOrDefaultAsync(n => n.LinkedUser == user_logged.UserName);
                            _context.Update(tcmAssessment);
                        }
                        else
                        {
                            tcmAssessment = _context.TCMAssessment
                                                    .Include(n => n.TcmClient)
                                                    .FirstOrDefault(u => u.Id == id);
                            tcmAssessment.Approved = 2;
                            tcmAssessment.TCMSupervisor = await _context.TCMSupervisors.FirstOrDefaultAsync(n => n.LinkedUser == user_logged.UserName);
                            _context.Update(tcmAssessment);
                        }

                        
                        try
                        {
                            await _context.SaveChangesAsync();
                            if (origi == 0)
                            {
                                return RedirectToAction("TCMAssessmentApproved", "TCMAssessments", new { approved = 1 });
                            }
                            if (origi == 1)
                            {
                                return RedirectToAction("Notifications", "TCMMessages");
                            }
                            if (origi == 2)
                            {
                                return RedirectToAction("TCMIntakeSectionDashboardReadOnly", "TCMIntakes", new { id = tcmAssessment.TcmClient.Id, section = 4 });
                            }
                            
                        }
                        catch (System.Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("Index", "TCMAssessments");
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult CreateIndividualAgencyModal(int idAssessment = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMAssessmentIndividualAgencyViewModel model;

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                if (user_logged.Clinic != null)
                {

                    model = new TCMAssessmentIndividualAgencyViewModel
                    {
                        IdTCMAssessment = idAssessment,
                        TcmAssessment = _context.TCMAssessment
                                                .Include(n => n.IndividualAgencyList)
                                                .Include(n => n.TcmClient)
                                                .ThenInclude(n => n.Client)
                                                .FirstOrDefault(n => n.Id == idAssessment),
                        Id = 0,
                        Agency = "",
                        Name = "",
                        RelationShip ="",
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now
                    };
                    if (model.TcmAssessment.IndividualAgencyList == null)
                        model.TcmAssessment.IndividualAgencyList = new List<TCMAssessmentIndividualAgencyEntity>();
                    return View(model);
                }
            }

            model = new TCMAssessmentIndividualAgencyViewModel
            {
                IdTCMAssessment = idAssessment,
                TcmAssessment = _context.TCMAssessment
                                        .Include(n => n.IndividualAgencyList)
                                        .Include(n => n.TcmClient)
                                        .ThenInclude(n => n.Client)
                                        .FirstOrDefault(n => n.Id == idAssessment),
                Id = 0,
                Agency = "",
                Name = "",
                RelationShip = "",
                CreatedBy = user_logged.UserName,
                CreatedOn = DateTime.Now
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> CreateIndividualAgencyModal(TCMAssessmentIndividualAgencyViewModel IndividualAgencyViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentIndividualAgencyEntity IndividualEntity = _context.TCMAssessmentIndividualAgency.Find(IndividualAgencyViewModel.Id);
                if (IndividualEntity == null)
                {
                    IndividualEntity = await _converterHelper.ToTCMAssessmenIndividualAgencyEntity(IndividualAgencyViewModel, true,user_logged.UserName);
                    _context.TCMAssessmentIndividualAgency.Add(IndividualEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMAssessmentIndividualAgencyEntity> IndividualAgency = await _context.TCMAssessmentIndividualAgency
                                                                                .Include(g => g.TcmAssessment)
                                                                                .Where(g => g.TcmAssessment.Id == IndividualAgencyViewModel.IdTCMAssessment)
                                                                                .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewIndividualAgency", IndividualAgency) });

                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the BIO.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateIndividualAgencyModal", IndividualAgencyViewModel) });
                }
            }
            TCMAssessmentIndividualAgencyEntity model;
            model = new TCMAssessmentIndividualAgencyEntity
            {
                TcmAssessment = _context.TCMAssessment.Include(n => n.IndividualAgencyList).FirstOrDefault(n => n.Id == IndividualAgencyViewModel.IdTCMAssessment),
                Id = 0,
                Agency = IndividualAgencyViewModel.Agency,
                Name = IndividualAgencyViewModel.Name,
                RelationShip = IndividualAgencyViewModel.RelationShip
                
            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
        }


        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult EditIndividualAgencyModal(int id = 0)
        {
            TCMAssessmentIndividualAgencyViewModel model;
            
            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .ThenInclude(n => n.Setting)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                if (user_logged.Clinic != null)
                {

                    TCMAssessmentIndividualAgencyEntity IndividualAgency = _context.TCMAssessmentIndividualAgency
                                                                                   .Include(m => m.TcmAssessment)
                                                                                   .ThenInclude(m => m.TcmClient)
                                                                                   .ThenInclude(m => m.Client)
                                                                                   .FirstOrDefault(m => m.Id == id);
                    if (IndividualAgency == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToTCMAssessmentIndividualAgencyViewModel(IndividualAgency);

                        return View(model);
                    }

                }
            }

            model = new TCMAssessmentIndividualAgencyViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> EditIndividualAgencyModal(TCMAssessmentIndividualAgencyViewModel IndividualAgencyViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentIndividualAgencyEntity IndividualAgencyEntity = await _converterHelper.ToTCMAssessmenIndividualAgencyEntity(IndividualAgencyViewModel, false, user_logged.UserName);
                _context.TCMAssessmentIndividualAgency.Update(IndividualAgencyEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMAssessmentIndividualAgencyEntity> IndividualAgency = await _context.TCMAssessmentIndividualAgency
                                                                                .Include(g => g.TcmAssessment)
                                                                                .Where(g => g.TcmAssessment.Id == IndividualAgencyViewModel.IdTCMAssessment)
                                                                                .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewIndividualAgency", IndividualAgency) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditIndividualAgencyModal", IndividualAgencyViewModel) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult CreateHouseCompositionModal(int idAssessment = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(n => n.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMAssessmentHouseCompositionViewModel model;

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                if (user_logged.Clinic != null)
                {

                    model = new TCMAssessmentHouseCompositionViewModel
                    {
                        IdTCMAssessment = idAssessment,
                        TcmAssessment = _context.TCMAssessment
                                                .Include(n => n.HouseCompositionList)
                                                .Include(n => n.TcmClient)
                                                .ThenInclude(n => n.Client)
                                                .FirstOrDefault(n => n.Id == idAssessment),
                        Id = 0,
                        Name = "",
                        RelationShip = "",
                        Supporting = "",
                        Age = 0,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now
                    };
                    if (model.TcmAssessment.HouseCompositionList == null)
                        model.TcmAssessment.HouseCompositionList = new List<TCMAssessmentHouseCompositionEntity>();
                    return View(model);
                }
            }

            model = new TCMAssessmentHouseCompositionViewModel
            {
                IdTCMAssessment = idAssessment,
                TcmAssessment = _context.TCMAssessment
                                        .Include(n => n.HouseCompositionList)
                                        .Include(n => n.TcmClient)
                                        .ThenInclude(n => n.Client)
                                        .FirstOrDefault(n => n.Id == idAssessment),
                Id = 0,
                Supporting = "",
                Name = "",
                RelationShip = ""
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> CreateHouseCompositionModal(TCMAssessmentHouseCompositionViewModel HouseCompositionViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentHouseCompositionEntity HouseCompositionEntity = _context.TCMAssessmentHouseComposition.Find(HouseCompositionViewModel.Id);
                if (HouseCompositionEntity == null)
                {
                    HouseCompositionEntity = await _converterHelper.ToTCMAssessmentHouseCompositionEntity(HouseCompositionViewModel, true, user_logged.UserName);
                    _context.TCMAssessmentHouseComposition.Add(HouseCompositionEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMAssessmentHouseCompositionEntity> HouseCompositionList = await _context.TCMAssessmentHouseComposition
                                                                                .Include(g => g.TcmAssessment)
                                                                                .Where(g => g.TcmAssessment.Id == HouseCompositionViewModel.IdTCMAssessment)
                                                                                .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHouseComposition", HouseCompositionList) });

                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Assessment.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateHouseCompositionModal", HouseCompositionViewModel) });
                }
            }
            TCMAssessmentHouseCompositionEntity model;
            model = new TCMAssessmentHouseCompositionEntity
            {
                TcmAssessment = _context.TCMAssessment.Include(n => n.IndividualAgencyList).FirstOrDefault(n => n.Id == HouseCompositionViewModel.IdTCMAssessment),
                Id = 0,
                Supporting = HouseCompositionViewModel.Supporting,
                Name = HouseCompositionViewModel.Name,
                RelationShip = HouseCompositionViewModel.RelationShip

            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateHouseCompositionModal", model) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult EditHouseCompositionModal(int id = 0)
        {
            TCMAssessmentHouseCompositionViewModel model;
            
            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .ThenInclude(u => u.Setting)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                
                if (user_logged.Clinic != null)
                {

                    TCMAssessmentHouseCompositionEntity HouseComposition = _context.TCMAssessmentHouseComposition
                                                                                   .Include(m => m.TcmAssessment)
                                                                                   .ThenInclude(m => m.TcmClient)
                                                                                   .ThenInclude(m => m.Client)
                                                                                   .FirstOrDefault(m => m.Id == id);
                    if (HouseComposition == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToTCMAssessmentHouseCompositionViewModel(HouseComposition);

                        return View(model);
                    }

                }
            }

            model = new TCMAssessmentHouseCompositionViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> EditHouseCompositionModal(TCMAssessmentHouseCompositionViewModel HouseCompositionViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentHouseCompositionEntity HouseCompositionEntity = await _converterHelper.ToTCMAssessmentHouseCompositionEntity(HouseCompositionViewModel, false, user_logged.UserName);
                _context.TCMAssessmentHouseComposition.Update(HouseCompositionEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMAssessmentHouseCompositionEntity> HouseCompositionList = await _context.TCMAssessmentHouseComposition
                                                                                .Include(g => g.TcmAssessment)
                                                                                .Where(g => g.TcmAssessment.Id == HouseCompositionViewModel.IdTCMAssessment)
                                                                                .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHouseComposition", HouseCompositionList) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditHouseCompositionModal", HouseCompositionViewModel) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult CreatePastCurrentModal(int idAssessment = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMAssessmentPastCurrentServiceViewModel model;

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                if (user_logged.Clinic != null)
                {
                    model = new TCMAssessmentPastCurrentServiceViewModel
                    {
                        IdTCMAssessment = idAssessment,
                        TcmAssessment = _context.TCMAssessment
                                                .Include(n => n.PastCurrentServiceList)
                                                .Include(n => n.TcmClient)
                                                .ThenInclude(n => n.Client)
                                                .FirstOrDefault(n => n.Id == idAssessment),
                        Id = 0,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        Efectiveness = "",
                        DateReceived = DateTime.Now,
                        ProviderAgency = "",
                        TypeService = ""
                    };
                    if (model.TcmAssessment.PastCurrentServiceList == null)
                        model.TcmAssessment.PastCurrentServiceList = new List<TCMAssessmentPastCurrentServiceEntity>();
                    return View(model);
                }
            }

            model = new TCMAssessmentPastCurrentServiceViewModel
            {
                IdTCMAssessment = idAssessment,
                TcmAssessment = _context.TCMAssessment
                                        .Include(n => n.PastCurrentServiceList)
                                        .Include(n => n.TcmClient)
                                        .ThenInclude(n => n.Client)
                                        .FirstOrDefault(n => n.Id == idAssessment),
                Id = 0,
                CreatedBy = user_logged.UserName,
                CreatedOn = DateTime.Now,
                Efectiveness = "",
                DateReceived = DateTime.Now,
                ProviderAgency = "",
                TypeService = ""
                
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> CreatePastCurrentModal(TCMAssessmentPastCurrentServiceViewModel PastCurrentViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentPastCurrentServiceEntity PastCurrentEntity = _context.TCMAssessmentPastCurrentService.Find(PastCurrentViewModel.Id);
                if (PastCurrentEntity == null)
                {
                    PastCurrentEntity = await _converterHelper.ToTCMAssessmenPastCurrentServiceEntity(PastCurrentViewModel, true, user_logged.UserName);
                    _context.TCMAssessmentPastCurrentService.Add(PastCurrentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMAssessmentPastCurrentServiceEntity> PastCurrentList = await _context.TCMAssessmentPastCurrentService
                                                                                                    .Include(g => g.TcmAssessment)
                                                                                                    .Where(g => g.TcmAssessment.Id == PastCurrentViewModel.IdTCMAssessment)
                                                                                                    .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewPastCurrent", PastCurrentList) });

                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Assessment.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateHouseCompositionModal", PastCurrentViewModel) });
                }
            }
            TCMAssessmentPastCurrentServiceEntity model;
            model = new TCMAssessmentPastCurrentServiceEntity
            {
                TcmAssessment = _context.TCMAssessment.Include(n => n.IndividualAgencyList).FirstOrDefault(n => n.Id == PastCurrentViewModel.IdTCMAssessment),
                Id = 0,
                DateReceived = PastCurrentViewModel.DateReceived,
                TypeService = PastCurrentViewModel.TypeService,
                ProviderAgency = PastCurrentViewModel.ProviderAgency,
                Efectiveness = PastCurrentViewModel.Efectiveness

            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreatePastCurrentModal", model) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult EditPastCurrentModal(int id = 0)
        {
            TCMAssessmentPastCurrentServiceViewModel model;

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                if (user_logged.Clinic != null)
                {
                    TCMAssessmentPastCurrentServiceEntity PastCurrent = _context.TCMAssessmentPastCurrentService
                                                                                   .Include(m => m.TcmAssessment)
                                                                                   .ThenInclude(m => m.TcmClient)
                                                                                   .ThenInclude(m => m.Client)
                                                                                   .FirstOrDefault(m => m.Id == id);
                    if (PastCurrent == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {
                        model = _converterHelper.ToTCMAssessmentPastCurrentServiceViewModel(PastCurrent);

                        return View(model);
                    }

                }
            }

            model = new TCMAssessmentPastCurrentServiceViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> EditPastCurrentModal(TCMAssessmentPastCurrentServiceViewModel PastCurrentViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentPastCurrentServiceEntity PastCurrentEntity = await _converterHelper.ToTCMAssessmenPastCurrentServiceEntity(PastCurrentViewModel, false, user_logged.UserName);
                _context.TCMAssessmentPastCurrentService.Update(PastCurrentEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMAssessmentPastCurrentServiceEntity> PastCurrentList = await _context.TCMAssessmentPastCurrentService
                                                                                .Include(g => g.TcmAssessment)
                                                                                .Where(g => g.TcmAssessment.Id == PastCurrentViewModel.IdTCMAssessment)
                                                                                .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewPastCurrent", PastCurrentList) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditPastCurrentModal", PastCurrentViewModel) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult CreateMedicationModal(int idAssessment = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(n => n.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMAssessmentMedicationViewModel model;

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                if (user_logged.Clinic != null)
                {

                    model = new TCMAssessmentMedicationViewModel
                    {
                        IdTCMAssessment = idAssessment,
                        TcmAssessment = _context.TCMAssessment
                                                .Include(n => n.MedicationList)
                                                .Include(n => n.TcmClient)
                                                .ThenInclude(n => n.Client)
                                                .FirstOrDefault(n => n.Id == idAssessment),
                        Id = 0,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        Name = "",
                        Dosage = "",
                        Prescriber = "",
                        Frequency = "",
                        ReasonPurpose = ""
                    };
                    if (model.TcmAssessment.MedicationList == null)
                        model.TcmAssessment.MedicationList = new List<TCMAssessmentMedicationEntity>();
                    return View(model);
                }
            }

            model = new TCMAssessmentMedicationViewModel
            {
                IdTCMAssessment = idAssessment,
                TcmAssessment = _context.TCMAssessment
                                        .Include(n => n.MedicationList)
                                        .Include(n => n.TcmClient)
                                        .ThenInclude(n => n.Client)
                                        .FirstOrDefault(n => n.Id == idAssessment),
                Id = 0,
                CreatedBy = user_logged.UserName,
                CreatedOn = DateTime.Now,
                Name = "",
                Dosage = "",
                Prescriber = "",
                Frequency = "",
                ReasonPurpose = ""

            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> CreateMedicationModal(TCMAssessmentMedicationViewModel MedicationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentMedicationEntity MedicationEntity = _context.TCMAssessmentMedication.Find(MedicationViewModel.Id);
                if (MedicationEntity == null)
                {
                    MedicationEntity = await _converterHelper.ToTCMAssessmenMedicationEntity(MedicationViewModel, true, user_logged.UserName);
                    _context.TCMAssessmentMedication.Add(MedicationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMAssessmentMedicationEntity> MedicationList = await _context.TCMAssessmentMedication
                                                                                            .Include(g => g.TcmAssessment)
                                                                                            .Where(g => g.TcmAssessment.Id == MedicationViewModel.IdTCMAssessment)
                                                                                            .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewMedication", MedicationList) });

                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Assessment.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateMedicationModal", MedicationViewModel) });
                }
            }
            TCMAssessmentMedicationEntity model;
            model = new TCMAssessmentMedicationEntity
            {
                TcmAssessment = _context.TCMAssessment.Include(n => n.MedicationList).FirstOrDefault(n => n.Id == MedicationViewModel.IdTCMAssessment),
                Id = 0,
                Dosage = MedicationViewModel.Dosage,
                Frequency = MedicationViewModel.Frequency,
                Name = MedicationViewModel.Name,
                Prescriber = MedicationViewModel.Prescriber,
                ReasonPurpose = MedicationViewModel.ReasonPurpose

            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateMedicationModal", model) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult EditMedicationModal(int id = 0)
        {
            TCMAssessmentMedicationViewModel model;

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
               

                if (user_logged.Clinic != null)
                {

                    TCMAssessmentMedicationEntity MedicationEntity = _context.TCMAssessmentMedication
                                                                        .Include(m => m.TcmAssessment)
                                                                        .ThenInclude(m => m.TcmClient)
                                                                        .ThenInclude(m => m.Client)
                                                                        .FirstOrDefault(m => m.Id == id);
                    if (MedicationEntity == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToTCMAssessmentMedicationViewModel(MedicationEntity);

                        return View(model);
                    }

                }
            }

            model = new TCMAssessmentMedicationViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> EditMedicationModal(TCMAssessmentMedicationViewModel MedicationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentMedicationEntity MedicationEntity = await _converterHelper.ToTCMAssessmenMedicationEntity(MedicationViewModel, false, user_logged.UserName);
                _context.TCMAssessmentMedication.Update(MedicationEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMAssessmentMedicationEntity> MedicationList = await _context.TCMAssessmentMedication
                                                                                .Include(g => g.TcmAssessment)
                                                                                .Where(g => g.TcmAssessment.Id == MedicationViewModel.IdTCMAssessment)
                                                                                .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewMedication", MedicationList) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditMedicationModal", MedicationViewModel) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult CreateHospitalModal(int idAssessment = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMAssessmentHospitalViewModel model;

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                if (user_logged.Clinic != null)
                {

                    model = new TCMAssessmentHospitalViewModel
                    {
                        IdTCMAssessment = idAssessment,
                        TcmAssessment = _context.TCMAssessment
                                                .Include(n => n.HospitalList)
                                                .Include(n => n.TcmClient)
                                                .ThenInclude(n => n.Client)
                                                .FirstOrDefault(n => n.Id == idAssessment),
                        Id = 0,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        Name = "",
                        Date = DateTime.Now,
                        Reason = ""
                    };
                    if (model.TcmAssessment.HospitalList == null)
                        model.TcmAssessment.HospitalList = new List<TCMAssessmentHospitalEntity>();
                    return View(model);
                }
            }

            model = new TCMAssessmentHospitalViewModel
            {
                IdTCMAssessment = idAssessment,
                TcmAssessment = _context.TCMAssessment
                                        .Include(n => n.HospitalList)
                                        .Include(n => n.TcmClient)
                                        .ThenInclude(n => n.Client)
                                        .FirstOrDefault(n => n.Id == idAssessment),
                Id = 0,
                CreatedBy = user_logged.UserName,
                CreatedOn = DateTime.Now,
                Name = "",
                Date = DateTime.Now,
                Reason = ""

            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> CreateHospitalModal(TCMAssessmentHospitalViewModel HospitalViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentHospitalEntity HospitalEntity = _context.TCMAssessmentHospital.Find(HospitalViewModel.Id);
                if (HospitalEntity == null)
                {
                    HospitalEntity = await _converterHelper.ToTCMAssessmentHospitalEntity(HospitalViewModel, true, user_logged.UserName);
                    _context.TCMAssessmentHospital.Add(HospitalEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMAssessmentHospitalEntity> HospitalList = await _context.TCMAssessmentHospital
                                                                                         .Include(g => g.TcmAssessment)
                                                                                         .Where(g => g.TcmAssessment.Id == HospitalViewModel.IdTCMAssessment)
                                                                                         .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHospital", HospitalList) });

                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Assessment.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateHospitalModal", HospitalViewModel) });
                }
            }
            TCMAssessmentHospitalEntity model;
            model = new TCMAssessmentHospitalEntity
            {
                TcmAssessment = _context.TCMAssessment.Include(n => n.HospitalList).FirstOrDefault(n => n.Id == HospitalViewModel.IdTCMAssessment),
                Id = 0,
                Name = HospitalViewModel.Name,
                Date = HospitalViewModel.Date,
                Reason = HospitalViewModel.Reason
            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateHospitalModal", model) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult EditHospitalModal(int id = 0)
        {
            TCMAssessmentHospitalViewModel model;

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                if (user_logged.Clinic != null)
                {

                    TCMAssessmentHospitalEntity HospitalEntity = _context.TCMAssessmentHospital
                                                                           .Include(m => m.TcmAssessment)
                                                                           .ThenInclude(m => m.TcmClient)
                                                                           .ThenInclude(m => m.Client)
                                                                           .FirstOrDefault(m => m.Id == id);
                    if (HospitalEntity == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToTCMAssessmentHospitalViewModel(HospitalEntity);

                        return View(model);
                    }

                }
            }

            model = new TCMAssessmentHospitalViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> EditHospitalModal(TCMAssessmentHospitalViewModel HospitalViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentHospitalEntity HospitalEntity = await _converterHelper.ToTCMAssessmentHospitalEntity(HospitalViewModel, false, user_logged.UserName);
                _context.TCMAssessmentHospital.Update(HospitalEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMAssessmentHospitalEntity> HospitalList = await _context.TCMAssessmentHospital
                                                                                .Include(g => g.TcmAssessment)
                                                                                .Where(g => g.TcmAssessment.Id == HospitalViewModel.IdTCMAssessment)
                                                                                .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHospital", HospitalList) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditHospitalModal", HospitalViewModel) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult CreateDrugModal(int idAssessment = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMAssessmentDrugViewModel model;

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                if (user_logged.Clinic != null)
                {

                    model = new TCMAssessmentDrugViewModel
                    {
                        IdTCMAssessment = idAssessment,
                        TcmAssessment = _context.TCMAssessment
                                                .Include(n => n.DrugList)
                                                .Include(n => n.TcmClient)
                                                .ThenInclude(n => n.Client)
                                                .FirstOrDefault(n => n.Id == idAssessment),
                        Id = 0,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        Age = 0,
                        Frequency = "",
                        DateBegin = DateTime.Now,
                        LastTimeUsed = "",
                        SustanceName = ""
                    };
                    if (model.TcmAssessment.DrugList == null)
                        model.TcmAssessment.DrugList = new List<TCMAssessmentDrugEntity>();
                    return View(model);
                }
            }

            model = new TCMAssessmentDrugViewModel
            {
                IdTCMAssessment = idAssessment,
                TcmAssessment = _context.TCMAssessment
                                        .Include(n => n.DrugList)
                                        .Include(n => n.TcmClient)
                                        .ThenInclude(n => n.Client)
                                        .FirstOrDefault(n => n.Id == idAssessment),
                Id = 0,
                CreatedBy = user_logged.UserName,
                CreatedOn = DateTime.Now,
                Age = 0,
                Frequency = "",
                DateBegin = DateTime.Now,
                LastTimeUsed = "",
                SustanceName = ""

            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> CreateDrugModal(TCMAssessmentDrugViewModel DrugViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentDrugEntity DrugEntity = _context.TCMAssessmentDrug.Find(DrugViewModel.Id);
                if (DrugEntity == null)
                {
                    DrugEntity = await _converterHelper.ToTCMAssessmentDrugEntity(DrugViewModel, true, user_logged.UserName);
                    _context.TCMAssessmentDrug.Add(DrugEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMAssessmentDrugEntity> DrugList = await _context.TCMAssessmentDrug
                                                                                   .Include(g => g.TcmAssessment)
                                                                                   .Where(g => g.TcmAssessment.Id == DrugViewModel.IdTCMAssessment)
                                                                                   .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDrug", DrugList) });

                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Assessment.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateDrugModal", DrugViewModel) });
                }
            }
            TCMAssessmentDrugEntity model;
            model = new TCMAssessmentDrugEntity
            {
                TcmAssessment = _context.TCMAssessment.Include(n => n.DrugList).FirstOrDefault(n => n.Id == DrugViewModel.IdTCMAssessment),
                Id = 0,
                Age = DrugViewModel.Age,
                SustanceName = DrugViewModel.SustanceName,
                LastTimeUsed = DrugViewModel.LastTimeUsed,
                DateBegin = DrugViewModel.DateBegin,
                Frequency = DrugViewModel.Frequency
            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateDrugModal", model) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult EditDrugModal(int id = 0)
        {
            TCMAssessmentDrugViewModel model;

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                if (user_logged.Clinic != null)
                {

                    TCMAssessmentDrugEntity DrugEntity = _context.TCMAssessmentDrug
                                                                           .Include(m => m.TcmAssessment)
                                                                           .ThenInclude(m => m.TcmClient)
                                                                           .ThenInclude(m => m.Client)
                                                                           .FirstOrDefault(m => m.Id == id);
                    if (DrugEntity == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToTCMAssessmentDrugViewModel(DrugEntity);

                        return View(model);
                    }

                }
            }

            model = new TCMAssessmentDrugViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> EditDrugModal(TCMAssessmentDrugViewModel DrugViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentDrugEntity DrugEntity = await _converterHelper.ToTCMAssessmentDrugEntity(DrugViewModel, false, user_logged.UserName);
                _context.TCMAssessmentDrug.Update(DrugEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMAssessmentDrugEntity> DruglList = await _context.TCMAssessmentDrug
                                                                                .Include(g => g.TcmAssessment)
                                                                                .Where(g => g.TcmAssessment.Id == DrugViewModel.IdTCMAssessment)
                                                                                .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDrug", DruglList) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditDrugModal", DrugViewModel) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult CreateMedicalProblemModal(int idAssessment = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMAssessmentMedicalProblemViewModel model;

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                if (user_logged.Clinic != null)
                {

                    model = new TCMAssessmentMedicalProblemViewModel
                    {
                        IdTCMAssessment = idAssessment,
                        TcmAssessment = _context.TCMAssessment
                                                .Include(n => n.MedicalProblemList)
                                                .Include(n => n.TcmClient)
                                                .ThenInclude(n => n.Client)
                                                .FirstOrDefault(n => n.Id == idAssessment),
                        Id = 0,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        Client = false,
                        Comments = "",
                        Family = false,
                        MedicalProblem = ""
                    };
                    if (model.TcmAssessment.MedicalProblemList == null)
                        model.TcmAssessment.MedicalProblemList = new List<TCMAssessmentMedicalProblemEntity>();
                    return View(model);
                }
            }

            model = new TCMAssessmentMedicalProblemViewModel
            {
                IdTCMAssessment = idAssessment,
                TcmAssessment = _context.TCMAssessment
                                        .Include(n => n.MedicalProblemList)
                                        .Include(n => n.TcmClient)
                                        .ThenInclude(n => n.Client)
                                        .FirstOrDefault(n => n.Id == idAssessment),
                Id = 0,
                CreatedBy = user_logged.UserName,
                CreatedOn = DateTime.Now,
                Client = false,
                Comments = "",
                Family = false,
                MedicalProblem = ""

            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> CreateMedicalProblemModal(TCMAssessmentMedicalProblemViewModel MedicalProblemViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentMedicalProblemEntity MedicalProblemEntity = _context.TCMAssessmentMedicalProblem.Find(MedicalProblemViewModel.Id);
                if (MedicalProblemEntity == null)
                {
                    MedicalProblemEntity = await _converterHelper.ToTCMAssessmentMedicalProblemEntity(MedicalProblemViewModel, true, user_logged.UserName);
                    _context.TCMAssessmentMedicalProblem.Add(MedicalProblemEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMAssessmentMedicalProblemEntity> MedicalProblemList = await _context.TCMAssessmentMedicalProblem
                                                                                                   .Include(g => g.TcmAssessment)
                                                                                                   .Where(g => g.TcmAssessment.Id == MedicalProblemViewModel.IdTCMAssessment)
                                                                                                   .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewMedicalProblem", MedicalProblemList) });

                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Assessment.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateMedicalProblemModal", MedicalProblemViewModel) });
                }
            }
            TCMAssessmentMedicalProblemEntity model;
            model = new TCMAssessmentMedicalProblemEntity
            {
                TcmAssessment = _context.TCMAssessment.Include(n => n.MedicalProblemList).FirstOrDefault(n => n.Id == MedicalProblemViewModel.IdTCMAssessment),
                Id = 0,
                Client = MedicalProblemViewModel.Client,
                Comments = MedicalProblemViewModel.Comments,
                Family = MedicalProblemViewModel.Family,
                MedicalProblem = MedicalProblemViewModel.MedicalProblem
            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateMedicalProblemModal", model) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult EditMedicalProblemModal(int id = 0)
        {
            TCMAssessmentMedicalProblemViewModel model;

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                if (user_logged.Clinic != null)
                {

                    TCMAssessmentMedicalProblemEntity MedicalProblemEntity = _context.TCMAssessmentMedicalProblem
                                                                                     .Include(m => m.TcmAssessment)
                                                                                     .ThenInclude(m => m.TcmClient)
                                                                                     .ThenInclude(m => m.Client)
                                                                                     .FirstOrDefault(m => m.Id == id);
                    if (MedicalProblemEntity == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToTCMAssessmentMedicalProblemViewModel(MedicalProblemEntity);

                        return View(model);
                    }

                }
            }

            model = new TCMAssessmentMedicalProblemViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> EditMedicalProblemModal(TCMAssessmentMedicalProblemViewModel DrugViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentMedicalProblemEntity MedicalProblemEntity = await _converterHelper.ToTCMAssessmentMedicalProblemEntity(DrugViewModel, false, user_logged.UserName);
                _context.TCMAssessmentMedicalProblem.Update(MedicalProblemEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMAssessmentMedicalProblemEntity> MedicalProblemList = await _context.TCMAssessmentMedicalProblem
                                                                                .Include(g => g.TcmAssessment)
                                                                                .Where(g => g.TcmAssessment.Id == DrugViewModel.IdTCMAssessment)
                                                                                .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewMedicalProblem", MedicalProblemList) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditMedicalProblemModal", DrugViewModel) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult CreateSurgeryModal(int idAssessment = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMAssessmentSurgeryViewModel model;

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                if (user_logged.Clinic != null)
                {

                    model = new TCMAssessmentSurgeryViewModel
                    {
                        IdTCMAssessment = idAssessment,
                        TcmAssessment = _context.TCMAssessment
                                                .Include(n => n.SurgeryList)
                                                .Include(n => n.TcmClient)
                                                .ThenInclude(n => n.Client)
                                                .FirstOrDefault(n => n.Id == idAssessment),
                        Id = 0,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        Date = DateTime.Now,
                        Hospital = "",
                        Outcome = "",
                        TypeSurgery = ""
                    };
                    if (model.TcmAssessment.MedicalProblemList == null)
                        model.TcmAssessment.MedicalProblemList = new List<TCMAssessmentMedicalProblemEntity>();
                    return View(model);
                }
            }

            model = new TCMAssessmentSurgeryViewModel
            {
                IdTCMAssessment = idAssessment,
                TcmAssessment = _context.TCMAssessment
                                        .Include(n => n.SurgeryList)
                                        .Include(n => n.TcmClient)
                                        .ThenInclude(n => n.Client)
                                        .FirstOrDefault(n => n.Id == idAssessment),
                Id = 0,
                CreatedBy = user_logged.UserName,
                CreatedOn = DateTime.Now,
                Date = DateTime.Now,
                Hospital = "",
                Outcome = "",
                TypeSurgery = ""

            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> CreateSurgeryModal(TCMAssessmentSurgeryViewModel SurgeryViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentSurgeryEntity SurgeryEntity = _context.TCMAssessmentSurgery.Find(SurgeryViewModel.Id);
                if (SurgeryEntity == null)
                {
                    SurgeryEntity = await _converterHelper.ToTCMAssessmentSurgeryEntity(SurgeryViewModel, true, user_logged.UserName);
                    _context.TCMAssessmentSurgery.Add(SurgeryEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMAssessmentSurgeryEntity> SurgeryList = await _context.TCMAssessmentSurgery
                                                                                                   .Include(g => g.TcmAssessment)
                                                                                                   .Where(g => g.TcmAssessment.Id == SurgeryViewModel.IdTCMAssessment)
                                                                                                   .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSurgery", SurgeryList) });

                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Assessment.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateSurgeryModal", SurgeryViewModel) });
                }
            }
            TCMAssessmentSurgeryEntity model;
            model = new TCMAssessmentSurgeryEntity
            {
                TcmAssessment = _context.TCMAssessment.Include(n => n.SurgeryList).FirstOrDefault(n => n.Id == SurgeryViewModel.IdTCMAssessment),
                Id = 0,
                Date = SurgeryViewModel.Date,
                Hospital = SurgeryViewModel.Hospital,
                Outcome = SurgeryViewModel.Outcome,
                TypeSurgery = SurgeryViewModel.TypeSurgery
            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateSurgeryModal", model) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult EditSurgeryModal(int id = 0)
        {
            TCMAssessmentSurgeryViewModel model;

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                if (user_logged.Clinic != null)
                {

                    TCMAssessmentSurgeryEntity SurgeryEntity = _context.TCMAssessmentSurgery
                                                                                     .Include(m => m.TcmAssessment)
                                                                                     .ThenInclude(m => m.TcmClient)
                                                                                     .ThenInclude(m => m.Client)
                                                                                     .FirstOrDefault(m => m.Id == id);
                    if (SurgeryEntity == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToTCMAssessmentSurgeryViewModel(SurgeryEntity);

                        return View(model);
                    }

                }
            }

            model = new TCMAssessmentSurgeryViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> EditSurgeryModal(TCMAssessmentSurgeryViewModel SurgeryViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentSurgeryEntity SurgeryEntity = await _converterHelper.ToTCMAssessmentSurgeryEntity(SurgeryViewModel, false, user_logged.UserName);
                _context.TCMAssessmentSurgery.Update(SurgeryEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMAssessmentSurgeryEntity> SurgeryList = await _context.TCMAssessmentSurgery
                                                                                .Include(g => g.TcmAssessment)
                                                                                .Where(g => g.TcmAssessment.Id == SurgeryViewModel.IdTCMAssessment)
                                                                                .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSurgery", SurgeryList) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditSurgeryModal", SurgeryViewModel) });
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> TCMAssessmentApproved(int approved = 0)
        {

            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (User.IsInRole("Manager"))
            {
                List<TCMAssessmentEntity> tcmAssessment = await _context.TCMAssessment
                                                                        .Include(m => m.TcmClient)
                                                                        .ThenInclude(m => m.Client)
                                                                        .Include(m => m.TcmMessages)
                                                                        .Where(m => m.Approved == approved
                                                                                 && m.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id)
                                                                        .OrderBy(m => m.TcmClient.CaseNumber)
                                                                        .ToListAsync();
                ViewData["approved"] = approved;

                return View(tcmAssessment);
            }

            if (User.IsInRole("TCMSupervisor"))
            {
                List<TCMAssessmentEntity> tcmAssessment = await _context.TCMAssessment
                                                                        .Include(m => m.TcmClient)
                                                                        .ThenInclude(m => m.Client)
                                                                        .Include(m => m.TcmMessages)
                                                                        .Where(m => m.Approved == approved
                                                                                 && m.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                                                 && m.TcmClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName)
                                                                        .OrderBy(m => m.TcmClient.CaseNumber)
                                                                        .ToListAsync();

                ViewData["approved"] = approved;
                return View(tcmAssessment);
            }

            if (User.IsInRole("CaseManager"))
            {
                List<TCMAssessmentEntity> tcmAssessment = await _context.TCMAssessment
                                                                         .Include(m => m.TcmClient)
                                                                         .ThenInclude(m => m.Client)
                                                                         .Include(m => m.TcmMessages)
                                                                         .Where(m => m.Approved == approved
                                                                             && m.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                                             && m.TcmClient.Casemanager.LinkedUser == user_logged.UserName)
                                                                         .OrderBy(m => m.TcmClient.CaseNumber)
                                                                         .ToListAsync();
                ViewData["approved"] = approved;
                return View(tcmAssessment);

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult EditReadOnly(int id = 0, int origi = 0)
        {
            TCMAssessmentViewModel model;

            if (User.IsInRole("TCMSupervisor"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    TCMAssessmentEntity TcmAssessment = _context.TCMAssessment
                                                                .Include(b => b.TcmClient)
                                                                .ThenInclude(b => b.Client)
                                                                .ThenInclude(b => b.Clients_Diagnostics)
                                                                .ThenInclude(b => b.Diagnostic)
                                                                .Include(b => b.TcmClient.Client.Client_Referred)
                                                                .Include(b => b.TcmClient.Client.Doctor)
                                                                .Include(b => b.TcmClient.Client.Psychiatrist)
                                                                .Include(b => b.IndividualAgencyList)
                                                                .Include(b => b.HouseCompositionList)
                                                                .Include(b => b.MedicationList)
                                                                .Include(b => b.PastCurrentServiceList)
                                                                .Include(b => b.HospitalList)
                                                                .Include(b => b.DrugList)
                                                                .Include(b => b.MedicalProblemList)
                                                                .Include(b => b.SurgeryList)
                                                                .Include(b => b.TcmClient)
                                                                .ThenInclude(b => b.Client)
                                                                .ThenInclude(b => b.Clinic)
                                                                .ThenInclude(b => b.Setting)
                                                                .FirstOrDefault(m => m.Id == id);
                    if (TcmAssessment == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {
                        model = _converterHelper.ToTCMAssessmentViewModel(TcmAssessment);

                        ViewData["origi"] = origi;
                        return View(model);
                       
                    }

                }
            }

            model = new TCMAssessmentViewModel();
            return View(model);
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult AddMessageEntity(int id = 0, int origi = 0)
        {
            if (id == 0)
            {
                return View(new TCMMessageViewModel());
            }
            else
            {
                TCMMessageViewModel model = new TCMMessageViewModel()
                {
                    IdTCMAssessment = id,
                    Origin = origi
                };

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> AddMessageEntity(TCMMessageViewModel messageViewModel)
        {
            if (ModelState.IsValid)
            {
                TCMMessageEntity model = await _converterHelper.ToTCMMessageEntity(messageViewModel, true);
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                model.From = user_logged.UserName;
                model.To = model.TCMAssessment.CreatedBy;
                _context.Add(model);
                await _context.SaveChangesAsync();
            }

            if (messageViewModel.Origin == 1)
                return RedirectToAction("TCMAssessmentApproved", new { approved = 1 });

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult AddReferred(int id = 0)
        {
            if (id > 0)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                ReferredTempViewModel model = new ReferredTempViewModel
                {
                    Id_Referred = 0,
                    Referreds = _combosHelper.GetComboReferredsByClinic(user_logged.Id),
                    IdServiceAgency = 1,
                    ServiceAgency = _combosHelper.GetComboServiceAgency()
                };
                return View(model);
            }
            else
            {
                //Edit
                return View(new ReferredTempViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> AddReferred(int id, ReferredTempViewModel referredTempViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                if (referredTempViewModel.Name != null && referredTempViewModel.Name != string.Empty)
                {
                    DateTime createdDate = DateTime.Now;
                    ReferredViewModel referred = new ReferredViewModel
                    {
                        Id = 0,
                        Address = referredTempViewModel.Address,
                        Agency = referredTempViewModel.Agency,
                        Email = referredTempViewModel.Email,
                        Name = referredTempViewModel.Name,
                        Telephone = referredTempViewModel.Telephone,
                        Title = referredTempViewModel.Title,
                        CreatedBy = user_logged.Id,
                        CreatedOn = createdDate

                    };

                    //_context.Add(referred);
                    ReferredEntity referredEntity = _converterHelper.ToReferredEntity(referred, true, user_logged.Id);

                    Client_Referred Client_referred = new Client_Referred
                    {
                        Id = 0,
                        Client = await _context.Clients.FirstOrDefaultAsync(d => d.Id == id),
                        Referred = referred,//await _context.Referreds.FirstOrDefaultAsync(d => d.CreatedOn == createdDate),
                        ReferredNote = referredTempViewModel.ReferredNote,
                        Service = ServiceAgency.TCM

                    };

                    _context.Add(Client_referred);

                }
                else
                {

                    Client_Referred Client_referred = new Client_Referred
                    {
                        Id = 0,
                        Client = await _context.Clients.FirstOrDefaultAsync(d => d.Id == id),
                        Referred = await _context.Referreds.FirstOrDefaultAsync(d => d.Id == referredTempViewModel.Id_Referred),
                        ReferredNote = referredTempViewModel.ReferredNote,
                        Service = ServiceAgency.TCM

                    };
                    _context.Add(Client_referred);
                }
                await _context.SaveChangesAsync();

                List<Client_Referred> clientList = await _context.Clients_Referreds
                                                           .Include(m => m.Referred)
                                                           .Where(n => n.Client.Id == id && n.Service == ServiceAgency.TCM)
                                                           .ToListAsync();
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewReferred", clientList) });
            }
            else
            {
                if (referredTempViewModel.Id_Referred > 0)
                {
                    Client_Referred Client_referred = new Client_Referred
                    {
                        Id = 0,
                        Client = await _context.Clients.FirstOrDefaultAsync(d => d.Id == id),
                        Referred = await _context.Referreds.FirstOrDefaultAsync(d => d.Id == referredTempViewModel.Id_Referred),
                        ReferredNote = referredTempViewModel.ReferredNote,
                        Service = ServiceAgency.TCM

                    };
                    _context.Add(Client_referred);
                    await _context.SaveChangesAsync();

                    List<Client_Referred> clientList = await _context.Clients_Referreds
                                                               .Include(m => m.Referred)
                                                               .Where(n => n.Client.Id == id && n.Service == ServiceAgency.TCM)
                                                               .ToListAsync();
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewReferred", clientList) });
                }
            }

            ReferredTempViewModel model = new ReferredTempViewModel
            {
                IdReferred = 0,
                Referreds = _combosHelper.GetComboReferredsByClinic(user_logged.Id),
                IdServiceAgency = 0,
                ServiceAgency = _combosHelper.GetComboServiceAgency()
            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddReferred", model) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteReferred(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Client_Referred referred = await _context.Clients_Referreds.Include( n => n.Client ).FirstOrDefaultAsync(d => d.Id == id);
            
            if (referred == null)
            {
                return RedirectToAction("Home/Error404");
            }
            int id_client = referred.Client.Id;

            _context.Clients_Referreds.Remove(referred);
            await _context.SaveChangesAsync();

            List<Client_Referred> clientList = await _context.Clients_Referreds
                                                          .Include(m => m.Referred)
                                                          .Where(n => n.Client.Id == id_client && n.Service == ServiceAgency.TCM)
                                                          .ToListAsync();
            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewReferred", clientList) });
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> UpdateAssessment()
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic || !user_logged.Clinic.Setting.TCMSupervisorEdit)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            else
            {
                CaseMannagerEntity caseManager = _context.CaseManagers.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                if (User.IsInRole("TCMSupervisor"))
                    return View(await _context.TCMClient

                                              .Include(f => f.TCMAssessment)
                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clinic)
                                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id
                                                       && n.Casemanager.TCMSupervisor.Id == _context.TCMSupervisors.FirstOrDefault(m => m.LinkedUser == user_logged.UserName).Id
                                                       && n.TCMAssessment.Approved == 2)
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

    }
}
