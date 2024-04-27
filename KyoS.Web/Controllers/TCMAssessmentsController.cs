using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Vml.Office;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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
                if (User.IsInRole("Manager") )
                    return View(await _context.TCMClient

                                              .Include(f => f.TCMAssessment)
                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clinic)
                                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());
                if (User.IsInRole("TCMSupervisor"))
                    return View(await _context.TCMClient

                                              .Include(f => f.TCMAssessment)
                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clinic)
                                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && n.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName)
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
        public async Task<IActionResult> Create(int id = 0, int origi = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            TCMClientEntity tcmClient = await _context.TCMClient
                                                      .Include(n => n.Client)
                                                      .ThenInclude(n => n.Client_Referred)
                                                      .ThenInclude(n => n.Referred)
                                                      .Include(n => n.TCMIntakeForm)
                                                      .Include(n => n.Casemanager)
                                                      .ThenInclude(n => n.TCMSupervisor)
                                                      .Include(n => n.Client)
                                                      .ThenInclude(n => n.MedicationList)
                                                      .AsSplitQuery()
                                                      .FirstOrDefaultAsync(n => n.Id == id);

            TCMAssessmentViewModel model;            
            
            if (tcmClient.TCMIntakeForm == null)
                tcmClient.TCMIntakeForm = new TCMIntakeFormEntity();

            model = new TCMAssessmentViewModel
            {
                Approved = 0,
                TcmClient = tcmClient,
                AreChild = YesNoNAType.NA,
                IdYesNoNAAreChild = 2,
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
                EmploymentStatuss = _combosHelper.GetComboEmployed(),
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
                ResidentStatuss = _combosHelper.GetComboResidential(),
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
                FrecuencyActiveList = _combosHelper.GetComboFrecuencyActive(),
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
                IsSheReceiving = false,
                Issues = "",
                LastModifiedBy = "",
                LastModifiedOn = DateTime.Now,
                LegalDecisionAddress = "",
                LegalDecisionAdLitem = false,
                LegalDecisionAttomey = false,
                LegalDecisionCityStateZip = "",
                LegalDecisionLegal = false,
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
                TypeOfAssessmentInitial = true,
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Create(TCMAssessmentViewModel tcmAssessmentViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);         

            if (ModelState.IsValid)
            {
                TCMAssessmentEntity tcmAssessmentEntity = _context.TCMAssessment.Find(tcmAssessmentViewModel.Id);
                if (tcmAssessmentEntity == null)
                {
                    tcmAssessmentViewModel.IdYesNoNAPregnancy = 1;
                    tcmAssessmentEntity = await _converterHelper.ToTCMAssessmentEntity(tcmAssessmentViewModel, true, user_logged.UserName);
                    tcmAssessmentEntity.Approved = 0;
                    _context.TCMAssessment.Add(tcmAssessmentEntity);

                    TCMClientEntity tcmclient = _context.TCMClient
                                                        .Include(n => n.Client)
                                                        .ThenInclude(n => n.MedicationList)
                                                        .Include(n => n.Client)
                                                        .ThenInclude(n => n.Doctor)
                                                        .Include(n => n.Client)
                                                        .ThenInclude(n => n.Psychiatrist)
                                                        .Include(n => n.Client)
                                                        .ThenInclude(n => n.EmergencyContact)
                                                        .Include(n => n.Client)
                                                        .ThenInclude(n => n.IndividualTherapyFacilitator)
                                                        .FirstOrDefault(n => n.Id == tcmAssessmentViewModel.TcmClient_FK);

                    tcmAssessmentEntity.MedicationList = new List<TCMAssessmentMedicationEntity>();
                    foreach (var item in tcmclient.Client.MedicationList)
                    {
                        if (tcmAssessmentEntity.MedicationList.Where(n => n.Name == item.Name).Count() == 0)
                        {
                            TCMAssessmentMedicationEntity medicine = new TCMAssessmentMedicationEntity()
                            {
                                Name = item.Name,
                                Dosage = item.Dosage,
                                Frequency = item.Frequency,
                                Prescriber = item.Prescriber,
                                ReasonPurpose = string.Empty
                            };
                            tcmAssessmentEntity.MedicationList.Add(medicine);
                        }
                    }

                    //llenar la tabla IndividualAgency
                    tcmAssessmentEntity.IndividualAgencyList = new List<TCMAssessmentIndividualAgencyEntity>();
                    if (tcmclient.Client.Doctor != null)
                    {
                        TCMAssessmentIndividualAgencyEntity indAgency = new TCMAssessmentIndividualAgencyEntity()
                        {
                            Name = tcmclient.Client.Doctor.Name,
                            Agency = user_logged.Clinic.Initials,
                            RelationShip = "PCP",
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now                            
                        };
                        tcmAssessmentEntity.IndividualAgencyList.Add(indAgency);
                    }
                   
                    if (tcmclient.Client.Psychiatrist != null)
                    {
                        TCMAssessmentIndividualAgencyEntity indAgency = new TCMAssessmentIndividualAgencyEntity()
                        {
                            Name = tcmclient.Client.Psychiatrist.Name,
                            Agency = user_logged.Clinic.Initials,
                            RelationShip = "PSY",
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now
                        };
                        tcmAssessmentEntity.IndividualAgencyList.Add(indAgency);
                    }

                    if (tcmclient.Client.EmergencyContact != null)
                    {
                        TCMAssessmentIndividualAgencyEntity indAgency = new TCMAssessmentIndividualAgencyEntity()
                        {
                            Name = tcmclient.Client.EmergencyContact.Name,
                            Agency = user_logged.Clinic.Initials,
                            RelationShip = RelationshipUtils.GetRelationshipByIndex(Convert.ToInt32(tcmAssessmentEntity.TcmClient.Client.RelationShipOfEmergencyContact)).ToString(),
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now
                        };
                        tcmAssessmentEntity.IndividualAgencyList.Add(indAgency);
                    }

                    if (tcmclient.Client.IndividualTherapyFacilitator != null)
                    {
                        TCMAssessmentIndividualAgencyEntity indAgency = new TCMAssessmentIndividualAgencyEntity()
                        {
                            Name = tcmclient.Client.IndividualTherapyFacilitator.Name,
                            Agency = user_logged.Clinic.Initials,
                            RelationShip = "Indivicual Therapist",
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now
                        };
                        tcmAssessmentEntity.IndividualAgencyList.Add(indAgency);
                    }

                    if (tcmclient.Client.IdFacilitatorPSR != 0)
                    {
                        TCMAssessmentIndividualAgencyEntity indAgency = new TCMAssessmentIndividualAgencyEntity()
                        {
                            Name = _context.Facilitators.Find(tcmclient.Client.IdFacilitatorPSR).Name,
                            Agency = user_logged.Clinic.Initials,
                            RelationShip = "PSR Therapist",
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now
                        };
                        tcmAssessmentEntity.IndividualAgencyList.Add(indAgency);
                    }

                    if (tcmclient.Client.IdFacilitatorGroup != 0)
                    {
                        TCMAssessmentIndividualAgencyEntity indAgency = new TCMAssessmentIndividualAgencyEntity()
                        {
                            Name = _context.Facilitators.Find(tcmclient.Client.IdFacilitatorGroup).Name,
                            Agency = user_logged.Clinic.Name,
                            RelationShip = "PSR Therapist",
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now
                        };
                        tcmAssessmentEntity.IndividualAgencyList.Add(indAgency);
                    }

                    //llenar la tabla Past and current service
                    tcmAssessmentEntity.PastCurrentServiceList = new List<TCMAssessmentPastCurrentServiceEntity>();
                    if (tcmclient.Client.MedicareId != null && tcmclient.Client.MedicareId != string.Empty)
                    {
                        TCMAssessmentPastCurrentServiceEntity pastCurrent = new TCMAssessmentPastCurrentServiceEntity()
                        {
                            TypeService = "MEDICARE",
                            ProviderAgency = "SSA",
                            DateReceived = "Update Date",
                            Efectiveness = EffectivenessType.Effective,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now
                        };
                        tcmAssessmentEntity.PastCurrentServiceList.Add(pastCurrent);
                    }
                    if (tcmclient.Client.MedicaidID != null && tcmclient.Client.MedicaidID != string.Empty)
                    {
                        TCMAssessmentPastCurrentServiceEntity pastCurrent = new TCMAssessmentPastCurrentServiceEntity()
                        {
                            TypeService = "MEDICAID",
                            ProviderAgency = "SSA",
                            DateReceived = "Update Date",
                            Efectiveness = EffectivenessType.Effective,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now
                        };
                        tcmAssessmentEntity.PastCurrentServiceList.Add(pastCurrent);
                    }
                    if (tcmclient.Client.SSN != null && tcmclient.Client.SSN != string.Empty)
                    {
                        TCMAssessmentPastCurrentServiceEntity pastCurrent = new TCMAssessmentPastCurrentServiceEntity()
                        {
                            TypeService = "SSI",
                            ProviderAgency = "SSA",
                            DateReceived = "Update Date",
                            Efectiveness = EffectivenessType.Effective,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now
                        };
                        tcmAssessmentEntity.PastCurrentServiceList.Add(pastCurrent);
                    }
                    if (tcmclient != null)
                    {
                        TCMAssessmentPastCurrentServiceEntity pastCurrent = new TCMAssessmentPastCurrentServiceEntity()
                        {
                            TypeService = "SNAP",
                            ProviderAgency = "DCF",
                            DateReceived = "Update Date",
                            Efectiveness = EffectivenessType.Effective,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now
                        };
                        tcmAssessmentEntity.PastCurrentServiceList.Add(pastCurrent);
                    }
                    if (tcmclient.Client.Doctor != null)
                    {
                        TCMAssessmentPastCurrentServiceEntity pastCurrent = new TCMAssessmentPastCurrentServiceEntity()
                        {
                            TypeService = "PCP",
                            ProviderAgency = user_logged.Clinic.Initials,
                            DateReceived = "Update Date",
                            Efectiveness = EffectivenessType.Effective,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now
                        };
                        tcmAssessmentEntity.PastCurrentServiceList.Add(pastCurrent);
                    }

                    if (tcmclient.Client.Psychiatrist != null)
                    {
                        TCMAssessmentPastCurrentServiceEntity pastCurrent = new TCMAssessmentPastCurrentServiceEntity()
                        {
                            TypeService = "PSY",
                            ProviderAgency = user_logged.Clinic.Initials,
                            DateReceived = "Update Date",
                            Efectiveness = EffectivenessType.Effective,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now
                        };
                        tcmAssessmentEntity.PastCurrentServiceList.Add(pastCurrent);
                    }
                    if (tcmclient.Client.OnlyTCM == false)
                    {
                        TCMAssessmentPastCurrentServiceEntity pastCurrent = new TCMAssessmentPastCurrentServiceEntity()
                        {
                            TypeService = "MENTAL HEALTH THERAPY",
                            ProviderAgency = user_logged.Clinic.Initials,
                            DateReceived = tcmclient.Client.AdmisionDate.Date.ToShortDateString(),
                            Efectiveness = EffectivenessType.Effective,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now
                        };
                        tcmAssessmentEntity.PastCurrentServiceList.Add(pastCurrent);
                    }

                    //llenar la tabla House Composition
                    tcmAssessmentEntity.HouseCompositionList = new List<TCMAssessmentHouseCompositionEntity>();
                    TCMAssessmentHouseCompositionEntity houseComposition = new TCMAssessmentHouseCompositionEntity()
                    {
                        Name = tcmclient.Client.Name,
                        Age = DateTime.Now.Year - tcmclient.Client.DateOfBirth.Year,
                        RelationShip = "Self",
                        Supporting = "Limited",
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now
                    };
                    tcmAssessmentEntity.HouseCompositionList.Add(houseComposition);

                    try
                    {
                        await _context.SaveChangesAsync();
                        if (origi == 0)
                        {
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
        public async Task<IActionResult> Edit(int id = 0, int origi = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(u => u.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            TCMAssessmentViewModel model;
            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && user_logged.Clinic.Setting.TCMSupervisorEdit == true))
            {
                TCMAssessmentTempEntity assessmentExist = await _context.TCMAssessmentTemp
                                                                        .FirstOrDefaultAsync(a => (a.UserName == user_logged.UserName
                                                                                                && a.Url == $"TCMAssessments/Edit/{id}"));

                TCMAssessmentEntity TcmAssessment = await _context.TCMAssessment
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
                                                                      .ThenInclude(b => b.Casemanager)
                                                                      .ThenInclude(b => b.TCMSupervisor)
                                                                      .Include(b => b.TcmClient)
                                                                      .ThenInclude(b => b.TCMIntakeForm)
                                                                      .AsSplitQuery()
                                                                      .FirstOrDefaultAsync(m => m.Id == id);

                if (TcmAssessment == null)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }

                if (assessmentExist != null)
                {                    
                    model = new TCMAssessmentViewModel
                    {
                        Id = TcmAssessment.Id,
                        TcmClient = TcmAssessment.TcmClient,
                        CreatedBy = TcmAssessment.CreatedBy,
                        CreatedOn = TcmAssessment.CreatedOn,
                        LastModifiedBy = TcmAssessment.LastModifiedBy,
                        LastModifiedOn = TcmAssessment.LastModifiedOn,
                        TcmClient_FK = TcmAssessment.TcmClient_FK,
                        IndividualAgencyList = TcmAssessment.IndividualAgencyList,
                        HouseCompositionList = TcmAssessment.HouseCompositionList,
                        PastCurrentServiceList = TcmAssessment.PastCurrentServiceList,
                        MedicationList = TcmAssessment.MedicationList,
                        HospitalList = TcmAssessment.HospitalList,
                        DrugList = TcmAssessment.DrugList,
                        MedicalProblemList = TcmAssessment.MedicalProblemList,
                        SurgeryList = TcmAssessment.SurgeryList,                        
                        TCMSupervisor = TcmAssessment.TCMSupervisor,
                        Client_Referred_List = TcmAssessment.TcmClient.Client.Client_Referred.ToList(),
                        Psychiatrist_Name = (TcmAssessment.TcmClient.TCMIntakeForm == null) ? string.Empty : TcmAssessment.TcmClient.TCMIntakeForm.Psychiatrist_Name,
                        Psychiatrist_Address = (TcmAssessment.TcmClient.TCMIntakeForm == null) ? string.Empty : TcmAssessment.TcmClient.TCMIntakeForm.Psychiatrist_Address,
                        Psychiatrist_CityStateZip = (TcmAssessment.TcmClient.TCMIntakeForm == null) ? string.Empty : TcmAssessment.TcmClient.TCMIntakeForm.Psychiatrist_CityStateZip,
                        Psychiatrist_Phone = (TcmAssessment.TcmClient.TCMIntakeForm == null) ? string.Empty : TcmAssessment.TcmClient.TCMIntakeForm.Psychiatrist_Phone,
                        PCP_Name = (TcmAssessment.TcmClient.TCMIntakeForm == null) ? string.Empty : TcmAssessment.TcmClient.TCMIntakeForm.PCP_Name,
                        PCP_Address = (TcmAssessment.TcmClient.TCMIntakeForm == null) ? string.Empty : TcmAssessment.TcmClient.TCMIntakeForm.PCP_Address,
                        PCP_CityStateZip = (TcmAssessment.TcmClient.TCMIntakeForm == null) ? string.Empty : TcmAssessment.TcmClient.TCMIntakeForm.PCP_CityStateZip,
                        PCP_Phone = (TcmAssessment.TcmClient.TCMIntakeForm == null) ? string.Empty : TcmAssessment.TcmClient.TCMIntakeForm.PCP_Phone,
                        Approved = assessmentExist.Approved,                        
                        AreChild = assessmentExist.AreChild,
                        IdYesNoNAAreChild = (int)assessmentExist.AreChild,
                        AreChildAddress = assessmentExist.AreChildAddress,
                        AreChildCity = assessmentExist.AreChildCity,
                        AreChildName = assessmentExist.AreChildName,
                        AreChildPhone = assessmentExist.AreChildPhone,
                        Caregiver = assessmentExist.Caregiver,
                        ChildFather = assessmentExist.ChildFather,
                        ChildMother = assessmentExist.ChildMother,
                        ClientInput = assessmentExist.ClientInput,
                        DateAssessment = Convert.ToDateTime(assessmentExist.DateAssessment),
                        Divorced = assessmentExist.Divorced,
                        Family = assessmentExist.Family,                        
                        Married = assessmentExist.Married,
                        IdYesNoNAWe = (int)assessmentExist.MayWe,
                        YesNoNAs = _combosHelper.GetComboYesNoNA(),
                        NeverMarried = assessmentExist.NeverMarried,
                        Other = assessmentExist.Other,
                        OtherExplain = assessmentExist.OtherExplain,
                        PresentingProblems = assessmentExist.PresentingProblems,
                        Referring = assessmentExist.Referring,
                        Review = assessmentExist.Review,
                        School = assessmentExist.School,
                        Separated = assessmentExist.Separated,                        
                        Treating = assessmentExist.Treating,
                        AcademicEelementary = assessmentExist.AcademicEelementary,
                        AcademicHigh = assessmentExist.AcademicHigh,
                        AcademicMiddle = assessmentExist.AcademicMiddle,
                        AcademicPreSchool = assessmentExist.AcademicPreSchool,
                        AdditionalInformation = assessmentExist.AdditionalInformation,
                        AdditionalInformationMigration = assessmentExist.AdditionalInformationMigration,
                        AHomeVisit = assessmentExist.AHomeVisit,
                        AHomeVisitOn = assessmentExist.AHomeVisitOn,
                        Appliances = assessmentExist.Appliances,
                        AttendanceEelementary = assessmentExist.AttendanceEelementary,
                        AttendanceHigh = assessmentExist.AttendanceHigh,
                        AttendanceMiddle = assessmentExist.AttendanceMiddle,
                        AttendancePreSchool = assessmentExist.AttendancePreSchool,
                        BathingAssistive = assessmentExist.BathingAssistive,
                        BathingIndependent = assessmentExist.BathingIndependent,
                        BathingPhysical = assessmentExist.BathingPhysical,
                        BathingSupervision = assessmentExist.BathingSupervision,
                        BathingTotal = assessmentExist.BathingTotal,
                        Bathtub = assessmentExist.Bathtub,
                        BehaviorEelementary = assessmentExist.BehaviorEelementary,
                        BehaviorHigh = assessmentExist.BehaviorHigh,
                        BehaviorMiddle = assessmentExist.BehaviorMiddle,
                        BehaviorPreSchool = assessmentExist.BehaviorPreSchool,
                        Briefly = assessmentExist.Briefly,
                        CaseManagerWas = assessmentExist.CaseManagerWas,
                        CaseManagerWasDueTo = assessmentExist.CaseManagerWasDueTo,
                        Citizen = assessmentExist.Citizen,
                        ColonCancer = assessmentExist.ColonCancer,
                        CongredatedHowOften = assessmentExist.CongredatedHowOften,
                        CongredatedProvider = assessmentExist.CongredatedProvider,
                        CongredatedReceive = assessmentExist.CongredatedReceive,
                        ContinueToLive = assessmentExist.ContinueToLive,
                        ContinueToLiveOnly = assessmentExist.ContinueToLiveOnly,
                        CookingAssistive = assessmentExist.CookingAssistive,
                        CookingIndependent = assessmentExist.CookingIndependent,
                        CookingPhysical = assessmentExist.CookingPhysical,
                        CookingSupervision = assessmentExist.CookingSupervision,
                        CookingTotal = assessmentExist.CookingTotal,
                        CountryOfBirth = assessmentExist.CountryOfBirth,
                        CurrentEmployer = assessmentExist.CurrentEmployer,
                        DentalExam = assessmentExist.DentalExam,
                        DescribeAnySchool = assessmentExist.DescribeAnySchool,
                        DescribeClientCultural = assessmentExist.DescribeClientCultural,
                        DescribeClientEducation = assessmentExist.DescribeClientEducation,
                        DescribeClientLiving = assessmentExist.DescribeClientLiving,
                        DescribeClientRelationship = assessmentExist.DescribeClientRelationship,
                        DescribeNeighborhood = assessmentExist.DescribeNeighborhood,
                        DescribeOtherNeedConcerns = assessmentExist.DescribeOtherNeedConcerns,
                        DoesClientBasicNeed = assessmentExist.DoesClientBasicNeed,
                        DoesClientCurrently = assessmentExist.DoesClientCurrently,
                        DoesClientCurrentlyExplain = assessmentExist.DoesClientCurrentlyExplain,
                        DoesClientFeel = assessmentExist.DoesClientFeel,
                        DoesClientFeelExplain = assessmentExist.DoesClientFeelExplain,
                        DoesClientNeedAssistance = assessmentExist.DoesClientNeedAssistance,
                        DoesClientNeedAssistanceEducational = assessmentExist.DoesClientNeedAssistanceEducational,
                        DoesClientNeedAssistanceEducationalExplain = assessmentExist.DoesClientNeedAssistanceEducationalExplain,
                        DoesClientNeedAssistanceExplain = assessmentExist.DoesClientNeedAssistanceExplain,
                        DoesNotKnow = assessmentExist.DoesNotKnow,
                        DoingAssistive = assessmentExist.DoingAssistive,
                        DoingIndependent = assessmentExist.DoingIndependent,
                        DoingPhysical = assessmentExist.DoingPhysical,
                        DoingSupervision = assessmentExist.DoingSupervision,
                        DoingTotal = assessmentExist.DoingTotal,
                        DressingAssistive = assessmentExist.DressingAssistive,
                        DressingIndependent = assessmentExist.DressingIndependent,
                        DressingPhysical = assessmentExist.DressingPhysical,
                        DressingSupervision = assessmentExist.DressingSupervision,
                        DressingTotal = assessmentExist.DressingTotal,
                        Drives = assessmentExist.Drives,
                        Electrical = assessmentExist.Electrical,
                        EmployerAddress = assessmentExist.EmployerAddress,
                        EmployerCityState = assessmentExist.EmployerCityState,
                        EmployerContactPerson = assessmentExist.EmployerContactPerson,
                        EmployerPhone = assessmentExist.EmployerPhone,
                        IdEmploymentStatus = (int)assessmentExist.EmploymentStatus,
                        EmploymentStatuss = _combosHelper.GetComboEmployed(),
                        ExcessiveCluter = assessmentExist.ExcessiveCluter,
                        FailToEelementary = assessmentExist.FailToEelementary,
                        FailToHigh = assessmentExist.FailToHigh,
                        FailToMiddle = assessmentExist.FailToMiddle,
                        FailToPreSchool = assessmentExist.FailToPreSchool,
                        FeedingAssistive = assessmentExist.FeedingAssistive,
                        FeedingIndependent = assessmentExist.FeedingIndependent,
                        FeedingPhysical = assessmentExist.FeedingPhysical,
                        FeedingSupervision = assessmentExist.FeedingSupervision,
                        FeedingTotal = assessmentExist.FeedingTotal,
                        FireHazards = assessmentExist.FireHazards,
                        Flooring = assessmentExist.Flooring,
                        FoodPantryHowOften = assessmentExist.FoodPantryHowOften,
                        FoodPantryProvider = assessmentExist.FoodPantryProvider,
                        FoodPantryReceive = assessmentExist.FoodPantryReceive,
                        FoodStampHowOften = assessmentExist.FoodStampHowOften,
                        FoodStampProvider = assessmentExist.FoodStampProvider,
                        FoodStampReceive = assessmentExist.FoodStampReceive,
                        FriendOrFamily = assessmentExist.FriendOrFamily,
                        GroomingAssistive = assessmentExist.GroomingAssistive,
                        GroomingIndependent = assessmentExist.GroomingIndependent,
                        GroomingPhysical = assessmentExist.GroomingPhysical,
                        GroomingSupervision = assessmentExist.GroomingSupervision,
                        GroomingTotal = assessmentExist.GroomingTotal,
                        HasClientEverArrest = assessmentExist.HasClientEverArrest,
                        HasClientEverArrestLastTime = assessmentExist.HasClientEverArrestLastTime,
                        HasClientEverArrestManyTime = assessmentExist.HasClientEverArrestManyTime,
                        HomeDeliveredHowOften = assessmentExist.HomeDeliveredHowOften,
                        HomeDeliveredProvider = assessmentExist.HomeDeliveredProvider,
                        HomeDeliveredReceive = assessmentExist.HomeDeliveredReceive,
                        IfThereAnyHousing = assessmentExist.IfThereAnyHousing,
                        IfYesWereCriminal = assessmentExist.IfYesWereCriminal,
                        IfYesWhatArea = assessmentExist.IfYesWhatArea,
                        ImmigrationOther = assessmentExist.ImmigrationOther,
                        ImmigrationOtherExplain = assessmentExist.ImmigrationOtherExplain,
                        Insect = assessmentExist.Insect,
                        IsClientCurrentlyEmployed = assessmentExist.IsClientCurrentlyEmployed,
                        IsClientCurrentlySchool = assessmentExist.IsClientCurrentlySchool,
                        IsClientCurrentlySchoolExplain = assessmentExist.IsClientCurrentlySchoolExplain,
                        IsClientInterested = assessmentExist.IsClientInterested,
                        IsClientInvolved = assessmentExist.IsClientInvolved,
                        IsClientInvolvedSpecify = assessmentExist.IsClientInvolvedSpecify,
                        IsTheClientAbleWork = assessmentExist.IsTheClientAbleWork,
                        IsTheClientAbleWorkLimitation = assessmentExist.IsTheClientAbleWorkLimitation,
                        IsTheClientHavingFinancial = assessmentExist.IsTheClientHavingFinancial,
                        IsTheClientHavingFinancialExplain = assessmentExist.IsTheClientHavingFinancialExplain,
                        IsThereAnyAide = assessmentExist.IsThereAnyAide,
                        IsThereAnyAideName = assessmentExist.IsThereAnyAideName,
                        IsThereAnyAidePhone = assessmentExist.IsThereAnyAidePhone,
                        IsThereAnyCurrentLegalProcess = assessmentExist.IsThereAnyCurrentLegalProcess,
                        LabWorks = assessmentExist.LabWorks,
                        LearningEelementary = assessmentExist.LearningEelementary,
                        LearningHigh = assessmentExist.LearningHigh,
                        LearningMiddle = assessmentExist.LearningMiddle,
                        LearningPreSchool = assessmentExist.LearningPreSchool,
                        MakingAssistive = assessmentExist.MakingAssistive,
                        MakingIndependent = assessmentExist.MakingIndependent,
                        MakingPhysical = assessmentExist.MakingPhysical,
                        MakingSupervision = assessmentExist.MakingSupervision,
                        MakingTotal = assessmentExist.MakingTotal,
                        Mammogram = assessmentExist.Mammogram,
                        MayWeLeaveSend = assessmentExist.MayWeLeaveSend,
                        MonthlyFamilyIncome = assessmentExist.MonthlyFamilyIncome,
                        NoAirCondition = assessmentExist.NoAirCondition,
                        NoTelephone = assessmentExist.NoTelephone,
                        NotHot = assessmentExist.NotHot,
                        NumberOfBedrooms = assessmentExist.NumberOfBedrooms,
                        NumberOfPersonLiving = assessmentExist.NumberOfPersonLiving,
                        OtherFinancial = assessmentExist.OtherFinancial,
                        OtherHowOften = assessmentExist.OtherHowOften,
                        OtherProvider = assessmentExist.OtherProvider,
                        OtherReceive = assessmentExist.OtherReceive,
                        PapAndHPV = assessmentExist.PapAndHPV,
                        ParticipationEelementary = assessmentExist.ParticipationEelementary,
                        ParticipationHigh = assessmentExist.ParticipationHigh,
                        ParticipationMiddle = assessmentExist.ParticipationMiddle,
                        ParticipationPreSchool = assessmentExist.ParticipationPreSchool,
                        PersonPorBedrooms = assessmentExist.PersonPorBedrooms,
                        PhysicalExam = assessmentExist.PhysicalExam,
                        PhysicalOther = assessmentExist.PhysicalOther,
                        PreferToLive = assessmentExist.PreferToLive,
                        Poor = assessmentExist.Poor,
                        ProbationOfficer = assessmentExist.ProbationOfficer,
                        ProbationOfficerName = assessmentExist.ProbationOfficerName,
                        ProbationOfficerPhone = assessmentExist.ProbationOfficerPhone,
                        RecommendedActivities = assessmentExist.RecommendedActivities,
                        RecommendedBasicNeed = assessmentExist.RecommendedBasicNeed,
                        RecommendedEconomic = assessmentExist.RecommendedEconomic,
                        RecommendedHousing = assessmentExist.RecommendedHousing,
                        RecommendedLegalImmigration = assessmentExist.RecommendedLegalImmigration,
                        RecommendedMentalHealth = assessmentExist.RecommendedMentalHealth,
                        RecommendedOther = assessmentExist.RecommendedOther,
                        RecommendedOtherSpecify = assessmentExist.RecommendedOtherSpecify,
                        RecommendedPhysicalHealth = assessmentExist.RecommendedPhysicalHealth,
                        RecommendedRecreational = assessmentExist.RecommendedRecreational,
                        RecommendedSchool = assessmentExist.RecommendedSchool,
                        RecommendedTransportation = assessmentExist.RecommendedTransportation,
                        RecommendedVocation = assessmentExist.RecommendedVocation,
                        RelationshipEelementary = assessmentExist.RelationshipEelementary,
                        RelationshipHigh = assessmentExist.RelationshipHigh,
                        RelationshipMiddle = assessmentExist.RelationshipMiddle,
                        RelationshipPreSchool = assessmentExist.RelationshipPreSchool,
                        Resident = assessmentExist.Resident,
                        IdResidentStatus = (int)assessmentExist.ResidentStatus,
                        ResidentStatuss = _combosHelper.GetComboResidential(),
                        SchoolAddress = assessmentExist.SchoolAddress,
                        SchoolCityState = assessmentExist.SchoolCityState,
                        SchoolDistrict = assessmentExist.SchoolDistrict,
                        SchoolGrade = assessmentExist.SchoolGrade,
                        SchoolName = assessmentExist.SchoolName,
                        SchoolProgramEBD = assessmentExist.SchoolProgramEBD,
                        SchoolProgramESE = assessmentExist.SchoolProgramESE,
                        SchoolProgramESOL = assessmentExist.SchoolProgramESOL,
                        SchoolProgramHHIP = assessmentExist.SchoolProgramHHIP,
                        SchoolProgramOther = assessmentExist.SchoolProgramOther,
                        SchoolProgramRegular = assessmentExist.SchoolProgramRegular,
                        SchoolProgramTeacherName = assessmentExist.SchoolProgramTeacherName,
                        SchoolProgramTeacherPhone = assessmentExist.SchoolProgramTeacherPhone,
                        ShoppingAssistive = assessmentExist.ShoppingAssistive,
                        ShoppingIndependent = assessmentExist.ShoppingIndependent,
                        ShoppingPhysical = assessmentExist.ShoppingPhysical,
                        ShoppingSupervision = assessmentExist.ShoppingSupervision,
                        ShoppingTotal = assessmentExist.ShoppingTotal,
                        Staff = assessmentExist.Staff,
                        Stairs = assessmentExist.Stairs,
                        Structural = assessmentExist.Structural,
                        TakesABus = assessmentExist.TakesABus,
                        TransferringAssistive = assessmentExist.TransferringAssistive,
                        TransferringIndependent = assessmentExist.TransferringIndependent,
                        TransferringPhysical = assessmentExist.TransferringPhysical,
                        TransferringSupervision = assessmentExist.TransferringSupervision,
                        TransferringTotal = assessmentExist.TransferringTotal,
                        TransportationOther = assessmentExist.TransportationOther,
                        TransportationOtherExplain = assessmentExist.TransportationOtherExplain,
                        Tripping = assessmentExist.Tripping,
                        Unsanitary = assessmentExist.Unsanitary,
                        VocationalEmployment = assessmentExist.VocationalEmployment,
                        Walks = assessmentExist.Walks,
                        WhatActivityThings = assessmentExist.WhatActivityThings,
                        WhatIsCollegeGraduated = assessmentExist.WhatIsCollegeGraduated,
                        WhatIsElementary = assessmentExist.WhatIsElementary,
                        WhatIsGED = assessmentExist.WhatIsGED,
                        WhatIsGraduated = assessmentExist.WhatIsGraduated,
                        WhatIsGraduatedDegree = assessmentExist.WhatIsGraduatedDegree,
                        WhatIsHighSchool = assessmentExist.WhatIsHighSchool,
                        WhatIsMiddle = assessmentExist.WhatIsMiddle,
                        WhatIsNoSchool = assessmentExist.WhatIsNoSchool,
                        WhatIsSomeCollege = assessmentExist.WhatIsSomeCollege,
                        WhatIsSomeHigh = assessmentExist.WhatIsSomeHigh,
                        WhatIsTheMainSource = assessmentExist.WhatIsTheMainSource,
                        WhatIsTradeSchool = assessmentExist.WhatIsTradeSchool,
                        WhatIsUnknown = assessmentExist.WhatIsUnknown,
                        WouldLikeObtainJob = assessmentExist.WouldLikeObtainJob,
                        WouldLikeObtainJobNotAtThisTime = assessmentExist.WouldLikeObtainJobNotAtThisTime,
                        YearEnteredUsa = assessmentExist.YearEnteredUsa,
                        HoweverVisitScheduler = Convert.ToDateTime(assessmentExist.HoweverVisitScheduler),
                        HoweverOn = assessmentExist.HoweverOn,
                        AbuseViolence = assessmentExist.AbuseViolence,
                        Allergy = assessmentExist.Allergy,
                        AllergySpecify = assessmentExist.AllergySpecify,
                        AnyOther = assessmentExist.AnyOther,
                        AreAllImmunization = assessmentExist.AreAllImmunization,
                        AreAllImmunizationExplain = assessmentExist.AreAllImmunizationExplain,
                        AreYouPhysician = assessmentExist.AreYouPhysician,
                        AreYouPhysicianSpecify = assessmentExist.AreYouPhysicianSpecify,
                        CantDoItAtAll = assessmentExist.CantDoItAtAll,
                        DateOfOnSetPresentingProblem = Convert.ToDateTime(assessmentExist.DateOfOnSetPresentingProblem),
                        DateMostRecent = assessmentExist.DateMostRecent,
                        DateSignatureCaseManager = Convert.ToDateTime(assessmentExist.DateSignatureCaseManager),
                        DateSignatureTCMSupervisor = Convert.ToDateTime(assessmentExist.DateSignatureTCMSupervisor),
                        DescribeAnyOther = assessmentExist.DescribeAnyOther,
                        DescribeAnyRisk = assessmentExist.DescribeAnyRisk,
                        DoesAggressiveness = assessmentExist.DoesAggressiveness,
                        DoesAnxiety = assessmentExist.DoesAnxiety,
                        DoesClientTranspotation = assessmentExist.DoesClientTranspotation,
                        DoesClientTranspotationExplain = assessmentExist.DoesClientTranspotationExplain,
                        DoesDelusions = assessmentExist.DoesDelusions,
                        DoesDepression = assessmentExist.DoesDepression,
                        DoesFearfulness = assessmentExist.DoesFearfulness,
                        DoesHallucinations = assessmentExist.DoesHallucinations,
                        DoesHelplessness = assessmentExist.DoesHelplessness,
                        DoesHopelessness = assessmentExist.DoesHopelessness,
                        DoesHyperactivity = assessmentExist.DoesHyperactivity,
                        DoesImpulsivity = assessmentExist.DoesImpulsivity,
                        DoesIrritability = assessmentExist.DoesIrritability,
                        DoesLoss = assessmentExist.DoesLoss,
                        DoesLow = assessmentExist.DoesLow,
                        DoesMood = assessmentExist.DoesMood,
                        DoesNegative = assessmentExist.DoesNegative,
                        DoesNervousness = assessmentExist.DoesNervousness,
                        DoesObsessive = assessmentExist.DoesObsessive,
                        DoesPanic = assessmentExist.DoesPanic,
                        Suicidal = assessmentExist.Suicidal,
                        Status = TCMDocumentStatus.Edition,
                        DoesParanoia = assessmentExist.DoesParanoia,
                        IdYesNoNAPregnancy = (int)assessmentExist.IsClientPregnancy,
                        IdFrecuencyActive = (int)assessmentExist.HowActive,
                        FrecuencyActiveList = _combosHelper.GetComboFrecuencyActive(),                        
                        DoesPoor = assessmentExist.DoesPoor,
                        DoesSadness = assessmentExist.DoesSadness,
                        DoesSelfNeglect = assessmentExist.DoesSelfNeglect,
                        DoesSheUnderstand = assessmentExist.DoesSheUnderstand,
                        DoesSleep = assessmentExist.DoesSleep,
                        DoesTheClientFeel = assessmentExist.DoesTheClientFeel,
                        DoesWithdrawal = assessmentExist.DoesWithdrawal,                        
                        HasClientUndergone = assessmentExist.HasClientUndergone,
                        HasDifficultySeeingLevel = assessmentExist.HasDifficultySeeingLevel,
                        HasDifficultySeeingObjetive = assessmentExist.HasDifficultySeeingObjetive,
                        HasNoImpairment = assessmentExist.HasNoImpairment,
                        HasNoUsefull = assessmentExist.HasNoUsefull,
                        HasTheClient = assessmentExist.HasTheClient,
                        HaveYouEverBeenToAny = assessmentExist.HaveYouEverBeenToAny,
                        HaveYouEverUsedAlcohol = assessmentExist.HaveYouEverUsedAlcohol,
                        HearingDifficulty = assessmentExist.HearingDifficulty,
                        HearingImpairment = assessmentExist.HearingImpairment,
                        HearingNotDetermined = assessmentExist.HearingNotDetermined,
                        Hears = assessmentExist.Hears,
                        Homicidal = assessmentExist.Homicidal,                        
                        HowDoesByFollowing = assessmentExist.HowDoesByFollowing,
                        HowDoesCalendar = assessmentExist.HowDoesCalendar,
                        HowDoesDaily = assessmentExist.HowDoesDaily,
                        HowDoesElectronic = assessmentExist.HowDoesElectronic,
                        HowDoesFamily = assessmentExist.HowDoesFamily,
                        HowDoesKeeping = assessmentExist.HowDoesKeeping,
                        HowDoesOther = assessmentExist.HowDoesOther,
                        HowDoesOtherExplain = assessmentExist.HowDoesOtherExplain,
                        HowDoesPill = assessmentExist.HowDoesPill,
                        HowDoesRNHHA = assessmentExist.HowDoesRNHHA,
                        HowManyTimes = assessmentExist.HowManyTimes,
                        HowWeelEnable = assessmentExist.HowWeelEnable,
                        HowWeelWithALot = assessmentExist.HowWeelWithALot,
                        HowWeelWithNo = assessmentExist.HowWeelWithNo,
                        HowWeelWithSome = assessmentExist.HowWeelWithSome,                        
                        IsClientCurrently = assessmentExist.IsClientCurrently,
                        IsSheReceiving = assessmentExist.IsSheReceiving,
                        Issues = assessmentExist.Issues,
                        LegalDecisionAddress = assessmentExist.LegalDecisionAddress,
                        LegalDecisionAdLitem = assessmentExist.LegalDecisionAdLitem,
                        LegalDecisionAttomey = assessmentExist.LegalDecisionAttomey,
                        LegalDecisionCityStateZip = assessmentExist.LegalDecisionCityStateZip,
                        LegalDecisionLegal = assessmentExist.LegalDecisionLegal,
                        LegalDecisionName = assessmentExist.LegalDecisionName,
                        LegalDecisionNone = assessmentExist.LegalDecisionNone,
                        LegalDecisionOther = assessmentExist.LegalDecisionOther,
                        LegalDecisionOtherExplain = assessmentExist.LegalDecisionOtherExplain,
                        LegalDecisionParent = assessmentExist.LegalDecisionParent,
                        LegalDecisionPhone = assessmentExist.LegalDecisionPhone,                        
                        MentalHealth = assessmentExist.MentalHealth,
                        NeedALot = assessmentExist.NeedALot,
                        NeedNoHelp = assessmentExist.NeedNoHelp,
                        NeedOfSpecial = assessmentExist.NeedOfSpecial,
                        NeedOfSpecialSpecify = assessmentExist.NeedOfSpecialSpecify,
                        NeedSome = assessmentExist.NeedSome,
                        NoHearing = assessmentExist.NoHearing,
                        NoUseful = assessmentExist.NoUseful,
                        OtherReceiveExplain = assessmentExist.OtherReceiveExplain,
                        Outcome = assessmentExist.Outcome,                        
                        PharmacyPhone = assessmentExist.PharmacyPhone,
                        PresentingProblemPrevious = assessmentExist.PresentingProblemPrevious,
                        Provider = assessmentExist.Provider,                        
                        TypeOfAssessmentAnnual = assessmentExist.TypeOfAssessmentAnnual,
                        TypeOfAssessmentInitial = assessmentExist.TypeOfAssessmentInitial,
                        TypeOfAssessmentOther = assessmentExist.TypeOfAssessmentOther,
                        TypeOfAssessmentOtherExplain = assessmentExist.TypeOfAssessmentOtherExplain,
                        TypeOfAssessmentSignificant = assessmentExist.TypeOfAssessmentSignificant,
                        VisionImpairment = assessmentExist.VisionImpairment,
                        VisionNotDetermined = assessmentExist.VisionNotDetermined,
                        WhatPharmacy = assessmentExist.WhatPharmacy,
                        WhenWas = assessmentExist.WhenWas,
                        ListAnyNeed = assessmentExist.ListAnyNeed,
                        ListClientCurrentPotencialStrngths = assessmentExist.ListClientCurrentPotencialStrngths,
                        ListClientCurrentPotencialWeakness = assessmentExist.ListClientCurrentPotencialWeakness
                    };
                }
                else
                {             
                    model = _converterHelper.ToTCMAssessmentViewModel(TcmAssessment);                                   
                }
                ViewData["origi"] = origi;
                return View(model);
            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> Edit(TCMAssessmentViewModel tcmAssessmentViewModel, int origi = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMAssessmentEntity tcmAssessmentEntity = await _converterHelper.ToTCMAssessmentEntity(tcmAssessmentViewModel, false, user_logged.UserName);
                if (tcmAssessmentEntity.Approved != 1)
                {
                    tcmAssessmentEntity.Approved = 0;
                }
                if (origi == 5)
                {
                    tcmAssessmentEntity.Approved = 2;
                }
                List<TCMMessageEntity> messages =  tcmAssessmentEntity.TcmMessages
                                                                      .Where(m => (m.Status == MessageStatus.NotRead && m.Notification == false))
                                                                      .ToList();
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

                //Remove the assessment on TCMAssessmentTemp
                TCMAssessmentTempEntity assessmentExist = await _context.TCMAssessmentTemp
                                                                        .FirstOrDefaultAsync(a => (a.UserName == user_logged.UserName
                                                                                          && a.Url == $"TCMAssessments/Edit/{tcmAssessmentViewModel.Id}"));
                if (assessmentExist != null)
                {
                    _context.TCMAssessmentTemp.Remove(assessmentExist);
                }                

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
                    if (origi == 5)
                    {
                        return RedirectToAction("UpdateAssessment", "TCMAssessments");
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
            TCMAssessmentEntity tcmAssessment = _context.TCMAssessment.FirstOrDefault(u => u.Id == id);

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
                        EffectivessList = _combosHelper.GetComboEffectiveness(),
                        DateReceived = string.Empty,
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
                EffectivessList = _combosHelper.GetComboEffectiveness(),
                DateReceived = string.Empty,
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

                    if (_context.Medication.Where(n => n.Name == MedicationEntity.Name).Count() == 0)
                    {
                        MedicationEntity medicine = new MedicationEntity()
                        {
                            Name = MedicationEntity.Name,
                            Dosage = MedicationEntity.Dosage,
                            Frequency = MedicationEntity.Frequency,
                            Prescriber = MedicationEntity.Prescriber,
                            ReasonPurpose = MedicationEntity.ReasonPurpose,
                            CreatedOn = MedicationEntity.CreatedOn,
                            CreatedBy = MedicationEntity.CreatedBy,
                            LastModifiedBy = MedicationEntity.LastModifiedBy,
                            LastModifiedOn = MedicationEntity.LastModifiedOn,
                            Client = _context.TCMClient.Include(n => n.Client).FirstOrDefault(n => n.TCMAssessment.Id == MedicationViewModel.IdTCMAssessment).Client
                        };
                        _context.Medication.Add(medicine);
                    }

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
                        model.OldName = MedicationEntity.Name;
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
                MedicationEntity medication = _context.Medication
                                                      .FirstOrDefault(n => n.Name == MedicationViewModel.OldName
                                                                        && n.Client.Id == _context.TCMClient
                                                                                                  .Include(n => n.Client)
                                                                                                  .FirstOrDefault(n => n.TCMAssessment.Id == MedicationViewModel.IdTCMAssessment).Client.Id);
                if (medication != null)
                {
                    medication.Name = MedicationEntity.Name;
                    medication.Dosage = MedicationEntity.Dosage;
                    medication.Frequency = MedicationEntity.Frequency;
                    medication.Prescriber = MedicationEntity.Prescriber;
                    medication.ReasonPurpose = MedicationEntity.ReasonPurpose;
                    medication.CreatedOn = MedicationEntity.CreatedOn;
                    medication.CreatedBy = MedicationEntity.CreatedBy;
                    medication.LastModifiedBy = MedicationEntity.LastModifiedBy;
                    medication.LastModifiedOn = MedicationEntity.LastModifiedOn;
                    _context.Medication.Update(medication);
                }

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
                        DrugsList = _combosHelper.GetComboDrugs()
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
                DrugsList = _combosHelper.GetComboDrugs()

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
                        Date = string.Empty,
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
                Date = string.Empty,
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
                                                                        .Include(m => m.TcmClient)
                                                                        .ThenInclude(m => m.Casemanager)
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
                                                                        .Include(m => m.TcmClient)
                                                                        .ThenInclude(m => m.Casemanager)
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
                                                                         .Include(m => m.TcmClient)
                                                                         .ThenInclude(m => m.Casemanager)
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

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public IActionResult EditReadOnly(int id = 0, int origi = 0)
        {
            TCMAssessmentViewModel model;

            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor") || User.IsInRole("CaseManager"))
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
                                                                .Include(b => b.TcmClient)
                                                                 .ThenInclude(b => b.Client)
                                                                 .ThenInclude(b => b.Client_Referred)
                                                                 .ThenInclude(b => b.Referred)
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
                                                                .Include(b => b.TcmClient)
                                                                .ThenInclude(b => b.Casemanager)
                                                                .ThenInclude(b => b.TCMSupervisor)
                                                                .Include(b => b.TcmClient)
                                                                .ThenInclude(b => b.TCMIntakeForm)
                                                                .AsSplitQuery()
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
                        CreatedOn = createdDate,
                        City = referredTempViewModel.City,
                        State = referredTempViewModel.State,
                        ZidCode = referredTempViewModel.ZidCode

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
                if (User.IsInRole("TCMSupervisor"))
                    return View(await _context.TCMClient

                                              .Include(f => f.TCMAssessment)
                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clinic)
                                              .Include(f => f.Casemanager)
                                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id
                                                       && n.Casemanager.TCMSupervisor.Id == _context.TCMSupervisors.FirstOrDefault(m => m.LinkedUser == user_logged.UserName).Id
                                                       && n.TCMAssessment.Approved == 2)
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        public IActionResult Details(int id = 0, int origi = 0)
        {
            TCMAssessmentViewModel model;

            
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
                                                                .Include(b => b.TcmClient)
                                                                .ThenInclude(b => b.Casemanager)
                                                                .ThenInclude(b => b.TCMSupervisor)
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

            model = new TCMAssessmentViewModel();
            return View(model);
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> ReturnTo(int? id, int tcmClientId = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMAssessmentEntity assessmentEntity = await _context.TCMAssessment.FirstOrDefaultAsync(s => s.Id == id);
            if (assessmentEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                assessmentEntity.Approved = 0;
                _context.TCMAssessment.Update(assessmentEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("TCMCaseHistory", "TCMClients", new { id = tcmClientId });
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMAssessmentEntity assessment = await _context.TCMAssessment
                                                           .Include(n => n.TcmClient)
                                                           .FirstOrDefaultAsync(d => d.Id == id);

            if (assessment == null)
            {
                return RedirectToAction("Home/Error404");
            }
           
            _context.TCMAssessment.Remove(assessment);
            await _context.SaveChangesAsync();

            return RedirectToAction("TCMCaseHistory", "TCMClients", new { id = assessment.TcmClient.Id });
            
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> AutoSave(string jsonModel)
        {
            UserEntity user_logged = await _context.Users                                                   
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            TCMAssessmentTempEntity model = JsonConvert.DeserializeObject<TCMAssessmentTempEntity>(jsonModel);

            TCMAssessmentTempEntity assessment = await _context.TCMAssessmentTemp
                                                               .FirstOrDefaultAsync(a => (a.UserName == user_logged.UserName && a.Url == model.Url));

            if (assessment == null)
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
                assessment.DateAssessment = model.DateAssessment;
                assessment.LegalDecisionNone = model.LegalDecisionNone;
                assessment.LegalDecisionLegal = model.LegalDecisionLegal;
                assessment.LegalDecisionAdLitem = model.LegalDecisionAdLitem;
                assessment.LegalDecisionParent = model.LegalDecisionParent;
                assessment.LegalDecisionAttomey = model.LegalDecisionAttomey;
                assessment.LegalDecisionOther = model.LegalDecisionOther;
                assessment.LegalDecisionOtherExplain = model.LegalDecisionOtherExplain;
                assessment.NeedOfSpecial = model.NeedOfSpecial;
                assessment.NeedOfSpecialSpecify = model.NeedOfSpecialSpecify;
                assessment.TypeOfAssessmentInitial = model.TypeOfAssessmentInitial;
                assessment.TypeOfAssessmentAnnual = model.TypeOfAssessmentAnnual;
                assessment.TypeOfAssessmentSignificant = model.TypeOfAssessmentSignificant;
                assessment.TypeOfAssessmentOther = model.TypeOfAssessmentOther;
                assessment.TypeOfAssessmentOtherExplain = model.TypeOfAssessmentOtherExplain;
                assessment.IsClientCurrently = model.IsClientCurrently;
                assessment.ClientInput = model.ClientInput;
                assessment.Family = model.Family;
                assessment.Referring = model.Referring;
                assessment.School = model.School;
                assessment.Treating = model.Treating;
                assessment.Caregiver = model.Caregiver;
                assessment.Review = model.Review;
                assessment.Other = model.Other;
                assessment.OtherExplain = model.OtherExplain;
                assessment.PresentingProblems = model.PresentingProblems;
                assessment.DateOfOnSetPresentingProblem = model.DateOfOnSetPresentingProblem;
                assessment.PresentingProblemPrevious = model.PresentingProblemPrevious;
                assessment.ChildMother = model.ChildMother;
                assessment.ChildFather = model.ChildFather;
                assessment.Married = model.Married;
                assessment.Divorced = model.Divorced;
                assessment.Separated = model.Separated;
                assessment.NeverMarried = model.NeverMarried;
                assessment.AreChild = model.AreChild;
                assessment.AreChildName = model.AreChildName;
                assessment.AreChildPhone = model.AreChildPhone;
                assessment.AreChildAddress = model.AreChildAddress;
                assessment.AreChildCity = model.AreChildCity;
                assessment.MayWe = model.MayWe;
                assessment.HowDoesByFollowing = model.HowDoesByFollowing;
                assessment.HowDoesPill = model.HowDoesPill;
                assessment.HowDoesFamily = model.HowDoesFamily;
                assessment.HowDoesCalendar = model.HowDoesCalendar;
                assessment.HowDoesElectronic = model.HowDoesElectronic;
                assessment.HowDoesRNHHA = model.HowDoesRNHHA;
                assessment.HowDoesKeeping = model.HowDoesKeeping;
                assessment.HowDoesDaily = model.HowDoesDaily;
                assessment.HowDoesOther = model.HowDoesOther;
                assessment.HowDoesOtherExplain = model.HowDoesOtherExplain;
                assessment.HowWeelWithNo = model.HowWeelWithNo;
                assessment.HowWeelWithALot = model.HowWeelWithALot;
                assessment.HowWeelWithSome = model.HowWeelWithSome;
                assessment.HowWeelEnable = model.HowWeelEnable;
                assessment.HasTheClient = model.HasTheClient;
                assessment.WhatPharmacy = model.WhatPharmacy;
                assessment.PharmacyPhone = model.PharmacyPhone;
                assessment.AnyOther = model.AnyOther;
                assessment.MentalHealth = model.MentalHealth;
                assessment.Suicidal = model.Suicidal;
                assessment.Homicidal = model.Homicidal;
                assessment.AbuseViolence = model.AbuseViolence;
                assessment.Provider = model.Provider;
                assessment.DescribeAnyRisk = model.DescribeAnyRisk;
                assessment.HaveYouEverUsedAlcohol = model.HaveYouEverUsedAlcohol;
                assessment.HaveYouEverBeenToAny = model.HaveYouEverBeenToAny;
                assessment.WhenWas = model.WhenWas;
                assessment.HowManyTimes = model.HowManyTimes;
                assessment.DateMostRecent = model.DateMostRecent;
                assessment.Outcome = model.Outcome;
                assessment.DoesTheClientFeel = model.DoesTheClientFeel;
                assessment.DescribeAnyOther = model.DescribeAnyOther;
                assessment.Allergy = model.Allergy;
                assessment.AllergySpecify = model.AllergySpecify;
                assessment.HasClientUndergone = model.HasClientUndergone;
                assessment.NoHearing = model.NoHearing;
                assessment.HasNoImpairment = model.HasNoImpairment;
                assessment.HearingImpairment = model.HearingImpairment;
                assessment.VisionImpairment = model.VisionImpairment;
                assessment.HearingDifficulty = model.HearingDifficulty;
                assessment.HasDifficultySeeingLevel = model.HasDifficultySeeingLevel;
                assessment.Hears = model.Hears;
                assessment.HasDifficultySeeingObjetive = model.HasDifficultySeeingObjetive;
                assessment.NoUseful = model.NoUseful;
                assessment.HasNoUsefull = model.HasNoUsefull;
                assessment.HearingNotDetermined = model.HearingNotDetermined;
                assessment.VisionNotDetermined = model.VisionNotDetermined;
                assessment.IsClientPregnancy = model.IsClientPregnancy;
                assessment.IsSheReceiving = model.IsSheReceiving;
                assessment.DoesSheUnderstand = model.DoesSheUnderstand;
                assessment.Issues = model.Issues;
                assessment.AreAllImmunization = model.AreAllImmunization;
                assessment.AreAllImmunizationExplain = model.AreAllImmunizationExplain;
                assessment.AreYouPhysician = model.AreYouPhysician;
                assessment.AreYouPhysicianSpecify = model.AreYouPhysicianSpecify;
                assessment.HowActive = model.HowActive;
                assessment.PhysicalExam = model.PhysicalExam;
                assessment.PapAndHPV = model.PapAndHPV;
                assessment.DentalExam = model.DentalExam;
                assessment.Mammogram = model.Mammogram;
                assessment.LabWorks = model.LabWorks;
                assessment.ColonCancer = model.ColonCancer;
                assessment.PhysicalOther = model.PhysicalOther;
                assessment.AdditionalInformation = model.AdditionalInformation;
                assessment.VocationalEmployment = model.VocationalEmployment;
                assessment.IsClientCurrentlyEmployed = model.IsClientCurrentlyEmployed;
                assessment.EmploymentStatus = model.EmploymentStatus;
                assessment.CurrentEmployer = model.CurrentEmployer;
                assessment.EmployerAddress = model.EmployerAddress;
                assessment.EmployerCityState = model.EmployerCityState;
                assessment.EmployerContactPerson = model.EmployerContactPerson;
                assessment.EmployerPhone = model.EmployerPhone;
                assessment.MayWeLeaveSend = model.MayWeLeaveSend;
                assessment.IsTheClientAbleWork = model.IsTheClientAbleWork;
                assessment.IsTheClientAbleWorkLimitation = model.IsTheClientAbleWorkLimitation;
                assessment.WouldLikeObtainJob = model.WouldLikeObtainJob;
                assessment.WouldLikeObtainJobNotAtThisTime = model.WouldLikeObtainJobNotAtThisTime;
                assessment.DoesClientNeedAssistance = model.DoesClientNeedAssistance;
                assessment.DoesClientNeedAssistanceExplain = model.DoesClientNeedAssistanceExplain;
                assessment.IsClientCurrentlySchool = model.IsClientCurrentlySchool;
                assessment.IsClientCurrentlySchoolExplain = model.IsClientCurrentlySchoolExplain;
                assessment.SchoolName = model.SchoolName;
                assessment.SchoolDistrict = model.SchoolDistrict;
                assessment.SchoolGrade = model.SchoolGrade;
                assessment.SchoolAddress = model.SchoolAddress;
                assessment.SchoolCityState = model.SchoolCityState;
                assessment.SchoolProgramRegular = model.SchoolProgramRegular;
                assessment.SchoolProgramESE = model.SchoolProgramESE;
                assessment.SchoolProgramEBD = model.SchoolProgramEBD;
                assessment.SchoolProgramESOL = model.SchoolProgramESOL;
                assessment.SchoolProgramHHIP = model.SchoolProgramHHIP;
                assessment.SchoolProgramOther = model.SchoolProgramOther;
                assessment.SchoolProgramTeacherName = model.SchoolProgramTeacherName;
                assessment.SchoolProgramTeacherPhone = model.SchoolProgramTeacherPhone;
                assessment.AcademicPreSchool = model.AcademicPreSchool;
                assessment.AcademicEelementary = model.AcademicEelementary;
                assessment.AcademicMiddle = model.AcademicMiddle;
                assessment.AcademicHigh = model.AcademicHigh;
                assessment.BehaviorPreSchool = model.BehaviorPreSchool;
                assessment.BehaviorEelementary = model.BehaviorEelementary;
                assessment.BehaviorMiddle = model.BehaviorMiddle;
                assessment.BehaviorHigh = model.BehaviorHigh;
                assessment.RelationshipPreSchool = model.RelationshipPreSchool;
                assessment.RelationshipEelementary = model.RelationshipEelementary;
                assessment.RelationshipMiddle = model.RelationshipMiddle;
                assessment.RelationshipHigh = model.RelationshipHigh;
                assessment.AttendancePreSchool = model.AttendancePreSchool;
                assessment.AttendanceEelementary = model.AttendanceEelementary;
                assessment.AttendanceMiddle = model.AttendanceMiddle;
                assessment.AttendanceHigh = model.AttendanceHigh;
                assessment.FailToPreSchool = model.FailToPreSchool;
                assessment.FailToEelementary = model.FailToEelementary;
                assessment.FailToMiddle = model.FailToMiddle;
                assessment.FailToHigh = model.FailToHigh;
                assessment.ParticipationPreSchool = model.ParticipationPreSchool;
                assessment.ParticipationEelementary = model.ParticipationEelementary;
                assessment.ParticipationMiddle = model.ParticipationMiddle;
                assessment.ParticipationHigh = model.ParticipationHigh;
                assessment.LearningPreSchool = model.LearningPreSchool;
                assessment.LearningEelementary = model.LearningEelementary;
                assessment.LearningMiddle = model.LearningMiddle;
                assessment.LearningHigh = model.LearningHigh;
                assessment.IsClientInvolved = model.IsClientInvolved;
                assessment.IsClientInvolvedSpecify = model.IsClientInvolvedSpecify;
                assessment.IsThereAnyAide = model.IsThereAnyAide;
                assessment.IsThereAnyAideName = model.IsThereAnyAideName;
                assessment.IsThereAnyAidePhone = model.IsThereAnyAidePhone;
                assessment.DescribeClientEducation = model.DescribeClientEducation;
                assessment.DescribeAnySchool = model.DescribeAnySchool;
                assessment.WhatIsNoSchool = model.WhatIsNoSchool;
                assessment.WhatIsGED = model.WhatIsGED;
                assessment.WhatIsSomeCollege = model.WhatIsSomeCollege;
                assessment.WhatIsElementary = model.WhatIsElementary;
                assessment.WhatIsGraduated = model.WhatIsGraduated;
                assessment.WhatIsCollegeGraduated = model.WhatIsCollegeGraduated;
                assessment.WhatIsMiddle = model.WhatIsMiddle;
                assessment.WhatIsHighSchool = model.WhatIsHighSchool;
                assessment.WhatIsGraduatedDegree = model.WhatIsGraduatedDegree;
                assessment.WhatIsSomeHigh = model.WhatIsSomeHigh;
                assessment.WhatIsTradeSchool = model.WhatIsTradeSchool;
                assessment.WhatIsUnknown = model.WhatIsUnknown;
                assessment.IsClientInterested = model.IsClientInterested;
                assessment.IfYesWhatArea = model.IfYesWhatArea;
                assessment.DoesClientNeedAssistanceEducational = model.DoesClientNeedAssistanceEducational;
                assessment.DoesClientNeedAssistanceEducationalExplain = model.DoesClientNeedAssistanceEducationalExplain;
                assessment.DescribeClientCultural = model.DescribeClientCultural;
                assessment.DoesClientCurrently = model.DoesClientCurrently;
                assessment.DoesClientCurrentlyExplain = model.DoesClientCurrentlyExplain;
                assessment.WhatActivityThings = model.WhatActivityThings;
                assessment.Briefly = model.Briefly;
                assessment.DescribeClientRelationship = model.DescribeClientRelationship;
                assessment.ListAnyNeed = model.ListAnyNeed;
                assessment.FeedingAssistive = model.FeedingAssistive;
                assessment.FeedingIndependent = model.FeedingIndependent;
                assessment.FeedingSupervision = model.FeedingSupervision;
                assessment.FeedingPhysical = model.FeedingPhysical;
                assessment.FeedingTotal = model.FeedingTotal;
                assessment.GroomingAssistive = model.GroomingAssistive;
                assessment.GroomingIndependent = model.GroomingIndependent;
                assessment.GroomingSupervision = model.GroomingSupervision;
                assessment.GroomingPhysical = model.GroomingPhysical;
                assessment.GroomingTotal = model.GroomingTotal;
                assessment.BathingAssistive = model.BathingAssistive;
                assessment.BathingIndependent = model.BathingIndependent;
                assessment.BathingSupervision = model.BathingSupervision;
                assessment.BathingPhysical = model.BathingPhysical;
                assessment.BathingTotal = model.BathingTotal;
                assessment.DressingAssistive = model.DressingAssistive;
                assessment.DressingIndependent = model.DressingIndependent;
                assessment.DressingSupervision = model.DressingSupervision;
                assessment.DressingPhysical = model.DressingPhysical;
                assessment.DressingTotal = model.DressingTotal;
                assessment.TransferringAssistive = model.TransferringAssistive;
                assessment.TransferringIndependent = model.TransferringIndependent;
                assessment.TransferringSupervision = model.TransferringSupervision;
                assessment.TransferringPhysical = model.TransferringPhysical;
                assessment.TransferringTotal = model.TransferringTotal;
                assessment.CookingAssistive = model.CookingAssistive;
                assessment.CookingIndependent = model.CookingIndependent;
                assessment.CookingSupervision = model.CookingSupervision;
                assessment.CookingPhysical = model.CookingPhysical;
                assessment.CookingTotal = model.CookingTotal;
                assessment.DoingAssistive = model.DoingAssistive;
                assessment.DoingIndependent = model.DoingIndependent;
                assessment.DoingSupervision = model.DoingSupervision;
                assessment.DoingPhysical = model.DoingPhysical;
                assessment.DoingTotal = model.DoingTotal;
                assessment.MakingAssistive = model.MakingAssistive;
                assessment.MakingIndependent = model.MakingIndependent;
                assessment.MakingSupervision = model.MakingSupervision;
                assessment.MakingPhysical = model.MakingPhysical;
                assessment.MakingTotal = model.MakingTotal;
                assessment.ShoppingAssistive = model.ShoppingAssistive;
                assessment.ShoppingIndependent = model.ShoppingIndependent;
                assessment.ShoppingSupervision = model.ShoppingSupervision;
                assessment.ShoppingPhysical = model.ShoppingPhysical;
                assessment.ShoppingTotal = model.ShoppingTotal;
                assessment.DescribeOtherNeedConcerns = model.DescribeOtherNeedConcerns;
                assessment.ResidentStatus = model.ResidentStatus;
                assessment.NumberOfPersonLiving = model.NumberOfPersonLiving;
                assessment.NumberOfBedrooms = model.NumberOfBedrooms;
                assessment.PersonPorBedrooms = model.PersonPorBedrooms;
                assessment.DescribeClientLiving = model.DescribeClientLiving;
                assessment.Structural = model.Structural;
                assessment.Tripping = model.Tripping;
                assessment.ExcessiveCluter = model.ExcessiveCluter;
                assessment.Electrical = model.Electrical;
                assessment.Unsanitary = model.Unsanitary;
                assessment.Insect = model.Insect;
                assessment.Poor = model.Poor;
                assessment.Stairs = model.Stairs;
                assessment.Flooring = model.Flooring;
                assessment.NotHot = model.NotHot;
                assessment.NoAirCondition = model.NoAirCondition;
                assessment.Bathtub = model.Bathtub;
                assessment.FireHazards = model.FireHazards;
                assessment.NoTelephone = model.NoTelephone;
                assessment.Appliances = model.Appliances;
                assessment.DescribeNeighborhood = model.DescribeNeighborhood;
                assessment.DoesClientFeel = model.DoesClientFeel;
                assessment.DoesClientFeelExplain = model.DoesClientFeelExplain;
                assessment.ContinueToLive = model.ContinueToLive;
                assessment.ContinueToLiveOnly = model.ContinueToLiveOnly;
                assessment.PreferToLive = model.PreferToLive;
                assessment.DoesNotKnow = model.DoesNotKnow;
                assessment.IfThereAnyHousing = model.IfThereAnyHousing;
                assessment.MonthlyFamilyIncome = model.MonthlyFamilyIncome;
                assessment.WhatIsTheMainSource = model.WhatIsTheMainSource;
                assessment.OtherFinancial = model.OtherFinancial;
                assessment.IsTheClientHavingFinancial = model.IsTheClientHavingFinancial;
                assessment.IsTheClientHavingFinancialExplain = model.IsTheClientHavingFinancialExplain;
                assessment.FoodStampReceive = model.FoodStampReceive;
                assessment.FoodStampHowOften = model.FoodStampHowOften;
                assessment.FoodStampProvider = model.FoodStampProvider;
                assessment.FoodPantryReceive = model.FoodPantryReceive;
                assessment.FoodPantryHowOften = model.FoodPantryHowOften;
                assessment.FoodPantryProvider = model.FoodPantryProvider;
                assessment.HomeDeliveredReceive = model.HomeDeliveredReceive;
                assessment.HomeDeliveredHowOften = model.HomeDeliveredHowOften;
                assessment.HomeDeliveredProvider = model.HomeDeliveredProvider;
                assessment.CongredatedReceive = model.CongredatedReceive;
                assessment.CongredatedHowOften = model.CongredatedHowOften;
                assessment.CongredatedProvider = model.CongredatedProvider;
                assessment.OtherReceiveExplain = model.OtherReceiveExplain;
                assessment.OtherReceive = model.OtherReceive;
                assessment.OtherHowOften = model.OtherHowOften;
                assessment.OtherProvider = model.OtherProvider;
                assessment.DoesClientBasicNeed = model.DoesClientBasicNeed;
                assessment.Walks = model.Walks;
                assessment.FriendOrFamily = model.FriendOrFamily;
                assessment.Drives = model.Drives;
                assessment.Staff = model.Staff;
                assessment.TakesABus = model.TakesABus;
                assessment.TransportationOther = model.TransportationOther;
                assessment.TransportationOtherExplain = model.TransportationOtherExplain;
                assessment.NeedNoHelp = model.NeedNoHelp;
                assessment.NeedALot = model.NeedALot;
                assessment.NeedSome = model.NeedSome;
                assessment.CantDoItAtAll = model.CantDoItAtAll;
                assessment.DoesClientTranspotation = model.DoesClientTranspotation;
                assessment.DoesClientTranspotationExplain = model.DoesClientTranspotationExplain;                
                assessment.HasClientEverArrest = model.HasClientEverArrest;
                assessment.HasClientEverArrestManyTime = model.HasClientEverArrestManyTime;
                assessment.HasClientEverArrestLastTime = model.HasClientEverArrestLastTime;
                assessment.IfYesWereCriminal = model.IfYesWereCriminal;
                assessment.IsThereAnyCurrentLegalProcess = model.IsThereAnyCurrentLegalProcess;
                assessment.ProbationOfficer = model.ProbationOfficer;
                assessment.ProbationOfficerName = model.ProbationOfficerName;
                assessment.ProbationOfficerPhone = model.ProbationOfficerPhone;
                assessment.CountryOfBirth = model.CountryOfBirth;
                assessment.YearEnteredUsa = model.YearEnteredUsa;
                assessment.Citizen = model.Citizen;
                assessment.Resident = model.Resident;
                assessment.ImmigrationOther = model.ImmigrationOther;
                assessment.ImmigrationOtherExplain = model.ImmigrationOtherExplain;
                assessment.AdditionalInformationMigration = model.AdditionalInformationMigration;
                assessment.ListClientCurrentPotencialStrngths = model.ListClientCurrentPotencialStrngths;
                assessment.ListClientCurrentPotencialWeakness = model.ListClientCurrentPotencialWeakness;
                assessment.RecommendedMentalHealth = model.RecommendedMentalHealth;
                assessment.RecommendedHousing = model.RecommendedHousing;
                assessment.RecommendedPhysicalHealth = model.RecommendedPhysicalHealth;
                assessment.RecommendedEconomic = model.RecommendedEconomic;
                assessment.RecommendedVocation = model.RecommendedVocation;
                assessment.RecommendedBasicNeed = model.RecommendedBasicNeed;
                assessment.RecommendedSchool = model.RecommendedSchool;
                assessment.RecommendedTransportation = model.RecommendedTransportation;
                assessment.RecommendedRecreational = model.RecommendedRecreational;
                assessment.RecommendedLegalImmigration = model.RecommendedLegalImmigration;
                assessment.RecommendedActivities = model.RecommendedActivities;
                assessment.RecommendedOther = model.RecommendedOther;
                assessment.RecommendedOtherSpecify = model.RecommendedOtherSpecify;
                assessment.AHomeVisit = model.AHomeVisit;
                assessment.AHomeVisitOn = model.AHomeVisitOn;
                assessment.CaseManagerWas = model.CaseManagerWas;
                assessment.CaseManagerWasDueTo = model.CaseManagerWasDueTo;
                assessment.HoweverOn = model.HoweverOn;
                assessment.HoweverVisitScheduler = model.HoweverVisitScheduler;
                assessment.DateSignatureCaseManager = model.DateSignatureCaseManager;
                assessment.DateSignatureTCMSupervisor = model.DateSignatureTCMSupervisor;

                _context.Update(assessment);
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

        [Authorize(Roles = "CaseManager")]
        public IActionResult DeleteIndAgency(int id = 0)
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
                //return View(new Client_DiagnosticViewModel());
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteIndAgency(DeleteViewModel model)
        {
            TCMAssessmentIndividualAgencyEntity entity = await _context.TCMAssessmentIndividualAgency
                                                                       .Include(n => n.TcmAssessment)
                                                                       .FirstOrDefaultAsync(n => n.Id == model.Id_Element);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.TCMAssessmentIndividualAgency.Remove(entity);
                    await _context.SaveChangesAsync();

                  
                    List<TCMAssessmentIndividualAgencyEntity> salida = await _context.TCMAssessmentIndividualAgency
                                                                                .Include(g => g.TcmAssessment)
                                                                                .Where(g => g.TcmAssessment.Id == entity.TcmAssessment.Id)
                                                                                .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewIndividualAgency", salida) });
                }
                catch (Exception)
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewIndividualAgency", _context.TCMAssessmentIndividualAgency.Include(n => n.TcmAssessment).Where(n => n.TcmAssessment.Id == entity.TcmAssessment.Id).ToList()) });
                }

               
            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewIndividualAgency", _context.TCMAssessmentIndividualAgency.Include(n => n.TcmAssessment).Where(n => n.TcmAssessment.Id == entity.TcmAssessment.Id).ToList()) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult DeleteHouseComposition(int id = 0)
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
                //return View(new Client_DiagnosticViewModel());
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteHouseComposition(DeleteViewModel model)
        {
            TCMAssessmentHouseCompositionEntity entity = await _context.TCMAssessmentHouseComposition
                                                                       .Include(n => n.TcmAssessment)
                                                                       .FirstOrDefaultAsync(n => n.Id == model.Id_Element);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.TCMAssessmentHouseComposition.Remove(entity);
                    await _context.SaveChangesAsync();

                    List<TCMAssessmentHouseCompositionEntity> salida = await _context.TCMAssessmentHouseComposition
                                                                                     .Include(g => g.TcmAssessment)
                                                                                     .Where(g => g.TcmAssessment.Id == entity.TcmAssessment.Id)
                                                                                     .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHouseComposition", salida) });
                }
                catch (Exception)
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHouseComposition", _context.TCMAssessmentHouseComposition.Include(n => n.TcmAssessment).Where(n => n.TcmAssessment.Id == entity.TcmAssessment.Id).ToList()) });
                }


            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHouseComposition", _context.TCMAssessmentHouseComposition.Include(n => n.TcmAssessment).Where(n => n.TcmAssessment.Id == entity.TcmAssessment.Id).ToList()) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult DeletePastCurrentService(int id = 0)
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
                //return View(new Client_DiagnosticViewModel());
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeletePastCurrentService(DeleteViewModel model)
        {
            TCMAssessmentPastCurrentServiceEntity entity = await _context.TCMAssessmentPastCurrentService
                                                                         .Include(n => n.TcmAssessment)
                                                                         .FirstOrDefaultAsync(n => n.Id == model.Id_Element);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.TCMAssessmentPastCurrentService.Remove(entity);
                    await _context.SaveChangesAsync();

                    List<TCMAssessmentPastCurrentServiceEntity> salida = await _context.TCMAssessmentPastCurrentService
                                                                                       .Include(g => g.TcmAssessment)
                                                                                       .Where(g => g.TcmAssessment.Id == entity.TcmAssessment.Id)
                                                                                       .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewPastCurrent", salida) });
                }
                catch (Exception)
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewPastCurrent", _context.TCMAssessmentPastCurrentService.Include(n => n.TcmAssessment).Where(n => n.TcmAssessment.Id == entity.TcmAssessment.Id).ToList()) });
                }


            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewPastCurrent", _context.TCMAssessmentPastCurrentService.Include(n => n.TcmAssessment).Where(n => n.TcmAssessment.Id == entity.TcmAssessment.Id).ToList()) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult DeleteMedication(int id = 0)
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
                //return View(new Client_DiagnosticViewModel());
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteMedication(DeleteViewModel model)
        {
            TCMAssessmentMedicationEntity entity = await _context.TCMAssessmentMedication
                                                                 .Include(n => n.TcmAssessment)
                                                                 .FirstOrDefaultAsync(n => n.Id == model.Id_Element);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.TCMAssessmentMedication.Remove(entity);
                    await _context.SaveChangesAsync();

                    List<TCMAssessmentMedicationEntity> salida = await _context.TCMAssessmentMedication
                                                                               .Include(g => g.TcmAssessment)
                                                                               .Where(g => g.TcmAssessment.Id == entity.TcmAssessment.Id)
                                                                               .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewMedication", salida) });
                }
                catch (Exception)
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewMedication", _context.TCMAssessmentMedication.Include(n => n.TcmAssessment).Where(n => n.TcmAssessment.Id == entity.TcmAssessment.Id).ToList()) });
                }


            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewMedication", _context.TCMAssessmentMedication.Include(n => n.TcmAssessment).Where(n => n.TcmAssessment.Id == entity.TcmAssessment.Id).ToList()) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult DeleteHospital(int id = 0)
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
                //return View(new Client_DiagnosticViewModel());
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteHospital(DeleteViewModel model)
        {
            TCMAssessmentHospitalEntity entity = await _context.TCMAssessmentHospital
                                                               .Include(n => n.TcmAssessment)
                                                               .FirstOrDefaultAsync(n => n.Id == model.Id_Element);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.TCMAssessmentHospital.Remove(entity);
                    await _context.SaveChangesAsync();

                    List<TCMAssessmentHospitalEntity> salida = await _context.TCMAssessmentHospital
                                                                             .Include(g => g.TcmAssessment)
                                                                             .Where(g => g.TcmAssessment.Id == entity.TcmAssessment.Id)
                                                                             .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHospital", salida) });
                }
                catch (Exception)
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHospital", _context.TCMAssessmentHospital.Include(n => n.TcmAssessment).Where(n => n.TcmAssessment.Id == entity.TcmAssessment.Id).ToList()) });
                }


            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHospital", _context.TCMAssessmentHospital.Include(n => n.TcmAssessment).Where(n => n.TcmAssessment.Id == entity.TcmAssessment.Id).ToList()) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult DeleteDrug(int id = 0)
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
                //return View(new Client_DiagnosticViewModel());
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteDrug(DeleteViewModel model)
        {
            TCMAssessmentDrugEntity entity = await _context.TCMAssessmentDrug
                                                           .Include(n => n.TcmAssessment)
                                                           .FirstOrDefaultAsync(n => n.Id == model.Id_Element);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.TCMAssessmentDrug.Remove(entity);
                    await _context.SaveChangesAsync();

                    List<TCMAssessmentDrugEntity> salida = await _context.TCMAssessmentDrug
                                                                         .Include(g => g.TcmAssessment)
                                                                         .Where(g => g.TcmAssessment.Id == entity.TcmAssessment.Id)
                                                                         .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDrug", salida) });
                }
                catch (Exception)
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDrug", _context.TCMAssessmentDrug.Include(n => n.TcmAssessment).Where(n => n.TcmAssessment.Id == entity.TcmAssessment.Id).ToList()) });
                }


            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDrug", _context.TCMAssessmentDrug.Include(n => n.TcmAssessment).Where(n => n.TcmAssessment.Id == entity.TcmAssessment.Id).ToList()) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult DeleteMedicalProblem(int id = 0)
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
                //return View(new Client_DiagnosticViewModel());
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteMedicalProblem(DeleteViewModel model)
        {
            TCMAssessmentMedicalProblemEntity entity = await _context.TCMAssessmentMedicalProblem
                                                                     .Include(n => n.TcmAssessment)
                                                                     .FirstOrDefaultAsync(n => n.Id == model.Id_Element);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.TCMAssessmentMedicalProblem.Remove(entity);
                    await _context.SaveChangesAsync();

                    List<TCMAssessmentMedicalProblemEntity> salida = await _context.TCMAssessmentMedicalProblem
                                                                                   .Include(g => g.TcmAssessment)
                                                                                   .Where(g => g.TcmAssessment.Id == entity.TcmAssessment.Id)
                                                                                   .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewMedicalProblem", salida) });
                }
                catch (Exception)
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewMedicalProblem", _context.TCMAssessmentMedicalProblem.Include(n => n.TcmAssessment).Where(n => n.TcmAssessment.Id == entity.TcmAssessment.Id).ToList()) });
                }


            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewMedicalProblem", _context.TCMAssessmentMedicalProblem.Include(n => n.TcmAssessment).Where(n => n.TcmAssessment.Id == entity.TcmAssessment.Id).ToList()) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult DeleteSurgery(int id = 0)
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
                //return View(new Client_DiagnosticViewModel());
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteSurgery(DeleteViewModel model)
        {
            TCMAssessmentSurgeryEntity entity = await _context.TCMAssessmentSurgery
                                                                     .Include(n => n.TcmAssessment)
                                                                     .FirstOrDefaultAsync(n => n.Id == model.Id_Element);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.TCMAssessmentSurgery.Remove(entity);
                    await _context.SaveChangesAsync();

                    List<TCMAssessmentSurgeryEntity> salida = await _context.TCMAssessmentSurgery
                                                                            .Include(g => g.TcmAssessment)
                                                                            .Where(g => g.TcmAssessment.Id == entity.TcmAssessment.Id)
                                                                            .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSurgery", salida) });
                }
                catch (Exception)
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSurgery", _context.TCMAssessmentSurgery.Include(n => n.TcmAssessment).Where(n => n.TcmAssessment.Id == entity.TcmAssessment.Id).ToList()) });
                }


            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewSurgery", _context.TCMAssessmentSurgery.Include(n => n.TcmAssessment).Where(n => n.TcmAssessment.Id == entity.TcmAssessment.Id).ToList()) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult DeleteReferred1(int id = 0)
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
                //return View(new Client_DiagnosticViewModel());
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteReferred1(DeleteViewModel model)
        {
            Client_Referred entity = await _context.Clients_Referreds
                                                   .Include(n => n.Client)
                                                   .Include(n => n.Referred)
                                                   .FirstOrDefaultAsync(n => n.Id == model.Id_Element);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Clients_Referreds.Remove(entity);
                    await _context.SaveChangesAsync();

                    List<Client_Referred> clientList = await _context.Clients_Referreds
                                                                     .Include(m => m.Referred)
                                                                     .Where(n => n.Client.Id == entity.Client.Id && n.Service == ServiceAgency.TCM)
                                                                     .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewReferred", clientList) });
                }
                catch (Exception)
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewReferred", _context.Clients_Referreds.Include(n => n.Referred).Where(n => n.Client.Id == entity.Client.Id && n.Service == ServiceAgency.TCM).ToList()) });
                }


            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewReferred", _context.Clients_Referreds.Include(n => n.Referred).Where(n => n.Client.Id == entity.Client.Id && n.Service == ServiceAgency.TCM).ToList()) });
        }

    }
}
