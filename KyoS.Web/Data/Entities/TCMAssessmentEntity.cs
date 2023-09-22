using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMAssessmentEntity : AuditableEntity
    {
        public int Id { get; set; }

        public int TcmClient_FK { get; set; }

        public TCMClientEntity TcmClient { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateAssessment { get; set; }

        //SECTION 1
        public bool LegalDecisionNone { get; set; }
        public bool LegalDecisionLegal { get; set; }
        public bool LegalDecisionAdLitem { get; set; }
        public bool LegalDecisionParent { get; set; }
        public bool LegalDecisionAttomey { get; set; }
        public bool LegalDecisionOther { get; set; }
        public string LegalDecisionOtherExplain { get; set; }
        public string LegalDecisionName { get; set; }
        public string LegalDecisionPhone { get; set; }
        public string LegalDecisionAddress { get; set; }
        public string LegalDecisionCityStateZip { get; set; }
        public bool NeedOfSpecial { get; set; }
        public string NeedOfSpecialSpecify { get; set; }

        public bool TypeOfAssessmentInitial { get; set; }
        public bool TypeOfAssessmentAnnual { get; set; }
        public bool TypeOfAssessmentSignificant { get; set; }
        public bool TypeOfAssessmentOther { get; set; }
        public string TypeOfAssessmentOtherExplain { get; set; }
        public bool IsClientCurrently { get; set; }
        
        //SECTION 2
        public bool ClientInput { get; set; }
        public bool Family { get; set; }
        public bool Referring { get; set; }
        public bool School { get; set; }
        public bool Treating { get; set; }
        public bool Caregiver { get; set; }
        public bool Review { get; set; }
        public bool Other { get; set; }
        public string OtherExplain { get; set; }
        public List<TCMAssessmentIndividualAgencyEntity> IndividualAgencyList { get; set; }

        //SECTION 3
        public string PresentingProblems { get; set; }
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateOfOnSetPresentingProblem { get; set; }
        public bool PresentingProblemPrevious { get; set; }

        //SECTION 4
        public string ChildMother { get; set; }
        public string ChildFather { get; set; }
        public bool Married { get; set; }
        public bool Divorced { get; set; }
        public bool Separated { get; set; }
        public bool NeverMarried { get; set; }
        public bool AreChild { get; set; }
        public string AreChildName { get; set; }
        public string AreChildPhone { get; set; }
        public string AreChildAddress { get; set; }
        public string AreChildCity { get; set; }
        public YesNoNA MayWe { get; set; }
       
        public List<TCMAssessmentHouseCompositionEntity> HouseCompositionList { get; set; }

        //SECTION 5
        public List<TCMAssessmentPastCurrentServiceEntity> PastCurrentServiceList { get; set; }

        //SECTION 6
        public List<TCMAssessmentMedicationEntity> MedicationList { get; set; }
        public bool HowDoesByFollowing { get; set; }
        public bool HowDoesPill { get; set; }
        public bool HowDoesFamily { get; set; }
        public bool HowDoesCalendar { get; set; }
        public bool HowDoesElectronic { get; set; }
        public bool HowDoesRNHHA { get; set; }
        public bool HowDoesKeeping { get; set; }
        public bool HowDoesDaily { get; set; }
        public bool HowDoesOther { get; set; }
        public string HowDoesOtherExplain { get; set; }

        public bool HowWeelWithNo { get; set; }
        public bool HowWeelWithALot { get; set; }
        public bool HowWeelWithSome { get; set; }
        public bool HowWeelEnable { get; set; }
        public bool HasTheClient { get; set; }
        public string WhatPharmacy { get; set; }
        public string PharmacyPhone { get; set; }
        public string AnyOther { get; set; }


        //SECTION 5
        public string MentalHealth { get; set; }
        public bool DoesDepression { get; set; }
        public bool DoesSelfNeglect { get; set; }
        public bool DoesSleep { get; set; }
        public bool DoesAggressiveness { get; set; }
        public bool DoesSadness { get; set; }
        public bool DoesLoss { get; set; }
        public bool DoesPoor { get; set; }
        public bool DoesHyperactivity { get; set; }
        public bool DoesHopelessness { get; set; }
        public bool DoesLow { get; set; }
        public bool DoesPanic { get; set; }
        public bool DoesImpulsivity { get; set; }
        public bool DoesHelplessness { get; set; }
        public bool DoesAnxiety { get; set; }
        public bool DoesFearfulness { get; set; }
        public bool DoesMood { get; set; }
        public bool DoesNegative { get; set; }
        public bool DoesNervousness { get; set; }
        public bool DoesParanoia { get; set; }
        public bool DoesHallucinations { get; set; }
        public bool DoesWithdrawal { get; set; }
        public bool DoesIrritability { get; set; }
        public bool DoesObsessive { get; set; }
        public bool DoesDelusions { get; set; }
        public List<TCMAssessmentHospitalEntity> HospitalList { get; set; }

        public bool Suicidal { get; set; }
        public bool Homicidal { get; set; }
        public bool AbuseViolence { get; set; }
        public string Provider { get; set; }
        public string DescribeAnyRisk { get; set; }
        public bool HaveYouEverUsedAlcohol { get; set; }
        public List<TCMAssessmentDrugEntity> DrugList { get; set; }

        public bool HaveYouEverBeenToAny { get; set; }
        public string WhenWas { get; set; }
        public string HowManyTimes { get; set; }
        public string DateMostRecent { get; set; }
        public string Outcome { get; set; }

        public bool DoesTheClientFeel { get; set; }
        public string DescribeAnyOther { get; set; }

        //Physical health
        public List<TCMAssessmentMedicalProblemEntity> MedicalProblemList { get; set; }
        public bool Allergy { get; set; }
        public string AllergySpecify { get; set; }

        public bool HasClientUndergone { get; set; }
        public List<TCMAssessmentSurgeryEntity> SurgeryList { get; set; }

        public bool NoHearing { get; set; }
        public bool HearingImpairment { get; set; }
        public bool HearingDifficulty { get; set; }
        public bool Hears { get; set; }
        public bool NoUseful { get; set; }
        public bool HearingNotDetermined { get; set; }

        public bool HasNoImpairment { get; set; }
        public bool VisionImpairment { get; set; }
        public bool HasDifficultySeeingLevel { get; set; }
        public bool HasDifficultySeeingObjetive { get; set; }
        public bool HasNoUsefull { get; set; }
        public bool VisionNotDetermined { get; set; }

        public bool IsClientPregnancy { get; set; }
        public bool IsClientPregnancyNA { get; set; }

        public bool IsSheReceiving { get; set; }
        public bool DoesSheUnderstand { get; set; }
        public string Issues { get; set; }

        public bool AreAllImmunization { get; set; }
        public string AreAllImmunizationExplain { get; set; }

        public bool AreYouPhysician { get; set; }
        public string AreYouPhysicianSpecify { get; set; }
        public string HowActive { get; set; }

        //PHYSICAL HEALTH / MEDICAL / DENTAL (Continued)-
        public string PhysicalExam { get; set; }
        public string DentalExam { get; set; }
        public string LabWorks { get; set; }
        public string PhysicalOther { get; set; }
        public string PapAndHPV { get; set; }
        public string Mammogram { get; set; }
        public string ColonCancer { get; set; }
        public string AdditionalInformation { get; set; }

        //3. VOCATIONAL / EMPLOYMENT
        public string VocationalEmployment { get; set; }
        public bool IsClientCurrentlyEmployed { get; set; }
        public string EmploymentStatus { get; set; }
        public string CurrentEmployer { get; set; }
        public string EmployerAddress { get; set; }
        public string EmployerCityState { get; set; }
        public string EmployerContactPerson { get; set; }
        public string EmployerPhone { get; set; }
        public bool MayWeLeaveSend { get; set; }
        public bool IsTheClientAbleWork { get; set; }
        public bool IsTheClientAbleWorkLimitation { get; set; }
        public bool WouldLikeObtainJob { get; set; }
        public bool WouldLikeObtainJobNotAtThisTime { get; set; }
        public bool DoesClientNeedAssistance { get; set; }
        public string DoesClientNeedAssistanceExplain { get; set; }

        //4. SCHOOL / EDUCATION
        public bool IsClientCurrentlySchool { get; set; }
        public string IsClientCurrentlySchoolExplain { get; set; }
        public string SchoolName { get; set; }
        public string SchoolDistrict { get; set; }
        public string SchoolGrade { get; set; }
        public string SchoolAddress { get; set; }
        public string SchoolCityState { get; set; }
        public bool SchoolProgramRegular { get; set; }
        public bool SchoolProgramESE { get; set; }
        public bool SchoolProgramEBD { get; set; }
        public bool SchoolProgramESOL { get; set; }
        public bool SchoolProgramHHIP { get; set; }
        public bool SchoolProgramOther { get; set; }
        public string SchoolProgramTeacherName { get; set; }
        public string SchoolProgramTeacherPhone { get; set; }

        public bool AcademicPreSchool { get; set; }
        public bool AcademicEelementary { get; set; }
        public bool AcademicMiddle { get; set; }
        public bool AcademicHigh { get; set; }

        public bool BehaviorPreSchool { get; set; }
        public bool BehaviorEelementary { get; set; }
        public bool BehaviorMiddle { get; set; }
        public bool BehaviorHigh { get; set; }

        public bool RelationshipPreSchool { get; set; }
        public bool RelationshipEelementary { get; set; }
        public bool RelationshipMiddle { get; set; }
        public bool RelationshipHigh { get; set; }

        public bool AttendancePreSchool { get; set; }
        public bool AttendanceEelementary { get; set; }
        public bool AttendanceMiddle { get; set; }
        public bool AttendanceHigh { get; set; }

        public bool FailToPreSchool { get; set; }
        public bool FailToEelementary { get; set; }
        public bool FailToMiddle { get; set; }
        public bool FailToHigh { get; set; }

        public bool ParticipationPreSchool { get; set; }
        public bool ParticipationEelementary { get; set; }
        public bool ParticipationMiddle { get; set; }
        public bool ParticipationHigh { get; set; }

        public bool LearningPreSchool { get; set; }
        public bool LearningEelementary { get; set; }
        public bool LearningMiddle { get; set; }
        public bool LearningHigh { get; set; }

        public bool IsClientInvolved { get; set; }
        public string IsClientInvolvedSpecify { get; set; }

        public bool IsThereAnyAide { get; set; }
        public string IsThereAnyAideName { get; set; }
        public string IsThereAnyAidePhone { get; set; }

        public string DescribeClientEducation { get; set; }
        public string DescribeAnySchool { get; set; }

        public bool WhatIsNoSchool { get; set; }
        public bool WhatIsElementary { get; set; }
        public bool WhatIsMiddle { get; set; }
        public bool WhatIsSomeHigh { get; set; }
        public bool WhatIsGED { get; set; }
        public bool WhatIsGraduated { get; set; }
        public bool WhatIsHighSchool { get; set; }
        public bool WhatIsTradeSchool { get; set; }
        public bool WhatIsSomeCollege { get; set; }
        public bool WhatIsCollegeGraduated { get; set; }
        public bool WhatIsGraduatedDegree { get; set; }
        public bool WhatIsUnknown { get; set; }

        public bool IsClientInterested { get; set; }
        public string IfYesWhatArea { get; set; }
        public bool DoesClientNeedAssistanceEducational { get; set; }
        public string DoesClientNeedAssistanceEducationalExplain { get; set; }

        //5 SOCIAL / SUPPORT SYSTEM / RECREATIONAL
        public string DescribeClientCultural { get; set; }
        public bool DoesClientCurrently { get; set; }
        public string DoesClientCurrentlyExplain { get; set; }
        public string WhatActivityThings { get; set; }
        public string Briefly { get; set; }
        public string DescribeClientRelationship { get; set; }
        public string ListAnyNeed { get; set; }

        //6 ACTIVITIES OF DAILY LIVING

        public bool FeedingAssistive { get; set; }
        public bool FeedingIndependent { get; set; }
        public bool FeedingSupervision { get; set; }
        public bool FeedingPhysical { get; set; }
        public bool FeedingTotal { get; set; }

        public bool GroomingAssistive { get; set; }
        public bool GroomingIndependent { get; set; }
        public bool GroomingSupervision { get; set; }
        public bool GroomingPhysical { get; set; }
        public bool GroomingTotal { get; set; }

        public bool BathingAssistive { get; set; }
        public bool BathingIndependent { get; set; }
        public bool BathingSupervision { get; set; }
        public bool BathingPhysical { get; set; }
        public bool BathingTotal { get; set; }

        public bool DressingAssistive { get; set; }
        public bool DressingIndependent { get; set; }
        public bool DressingSupervision { get; set; }
        public bool DressingPhysical { get; set; }
        public bool DressingTotal { get; set; }

        public bool TransferringAssistive { get; set; }
        public bool TransferringIndependent { get; set; }
        public bool TransferringSupervision { get; set; }
        public bool TransferringPhysical { get; set; }
        public bool TransferringTotal { get; set; }

        public bool CookingAssistive { get; set; }
        public bool CookingIndependent { get; set; }
        public bool CookingSupervision { get; set; }
        public bool CookingPhysical { get; set; }
        public bool CookingTotal { get; set; }

        public bool DoingAssistive { get; set; }
        public bool DoingIndependent { get; set; }
        public bool DoingSupervision { get; set; }
        public bool DoingPhysical { get; set; }
        public bool DoingTotal { get; set; }

        public bool MakingAssistive { get; set; }
        public bool MakingIndependent { get; set; }
        public bool MakingSupervision { get; set; }
        public bool MakingPhysical { get; set; }
        public bool MakingTotal { get; set; }

        public bool ShoppingAssistive { get; set; }
        public bool ShoppingIndependent { get; set; }
        public bool ShoppingSupervision { get; set; }
        public bool ShoppingPhysical { get; set; }
        public bool ShoppingTotal { get; set; }

        public string DescribeOtherNeedConcerns { get; set; }

        //7 HOUSING / LIVING ENVIRONMENT

        public string ResidentStatus { get; set; }
        public int NumberOfPersonLiving { get; set; }
        public int NumberOfBedrooms { get; set; }
        public float PersonPorBedrooms { get; set; }
        public string DescribeClientLiving { get; set; }

        public bool Structural { get; set; }
        public bool Electrical { get; set; }
        public bool Poor { get; set; }
        public bool NotHot { get; set; }
        public bool FireHazards { get; set; }

        public bool Tripping { get; set; }
        public bool Unsanitary { get; set; }
        public bool NoAirCondition { get; set; }
        public bool Stairs { get; set; }
        public bool NoTelephone { get; set; }

        public bool ExcessiveCluter { get; set; }
        public bool Insect { get; set; }
        public bool Flooring { get; set; }
        public bool Bathtub { get; set; }
        public bool Appliances { get; set; }

        public string DescribeNeighborhood { get; set; }

        public bool DoesClientFeel { get; set; }
        public string DoesClientFeelExplain { get; set; }

        public bool ContinueToLive { get; set; }
        public bool ContinueToLiveOnly { get; set; }
        public bool PreferToLive { get; set; }
        public bool DoesNotKnow { get; set; }
        public string IfThereAnyHousing { get; set; }

        //8  ECONOMIC / FINANCIAL

        public string MonthlyFamilyIncome { get; set; }
        public string WhatIsTheMainSource { get; set; }
        public string OtherFinancial { get; set; }
        public bool IsTheClientHavingFinancial { get; set; }
        public string IsTheClientHavingFinancialExplain { get; set; }

        //9 BASIC NEEDS

        public bool FoodStampReceive { get; set; }
        public string FoodStampHowOften { get; set; }
        public string FoodStampProvider { get; set; }

        public bool FoodPantryReceive { get; set; }
        public string FoodPantryHowOften { get; set; }
        public string FoodPantryProvider { get; set; }

        public bool HomeDeliveredReceive { get; set; }
        public string HomeDeliveredHowOften { get; set; }
        public string HomeDeliveredProvider { get; set; }

        public bool CongredatedReceive { get; set; }
        public string CongredatedHowOften { get; set; }
        public string CongredatedProvider { get; set; }

        public bool OtherReceive { get; set; }
        public string OtherReceiveExplain { get; set; }
        public string OtherHowOften { get; set; }
        public string OtherProvider { get; set; }

        public string DoesClientBasicNeed { get; set; }

        // 10 TRANSPORTATION

        public bool Walks { get; set; }
        public bool Drives { get; set; }
        public bool TakesABus { get; set; }
        public bool FriendOrFamily { get; set; }
        public bool Staff { get; set; }
        public bool TransportationOther { get; set; }
        public string TransportationOtherExplain { get; set; }

        public bool NeedNoHelp { get; set; }
        public bool NeedSome { get; set; }
        public bool NeedALot { get; set; }
        public bool CantDoItAtAll { get; set; }
        public bool DoesClientTranspotation { get; set; }
        public string DoesClientTranspotationExplain { get; set; }
        //11 LEGAL / IMMIGRATION

        public bool HasClientEverArrest { get; set; }
        public string HasClientEverArrestManyTime { get; set; }
        public string HasClientEverArrestLastTime { get; set; }
        public bool IfYesWereCriminal { get; set; }
        public bool IsThereAnyCurrentLegalProcess { get; set; }
        public bool ProbationOfficer { get; set; }
        public string ProbationOfficerName { get; set; }
        public string ProbationOfficerPhone { get; set; }

        public string CountryOfBirth { get; set; }
        public int YearEnteredUsa { get; set; }
        public bool Citizen { get; set; }
        public bool Resident { get; set; }
        public bool ImmigrationOther { get; set; }
        public string ImmigrationOtherExplain { get; set; }

        public string AdditionalInformationMigration { get; set; }

        //SECTION VI: SUMMARY OF CLIENT’S STRENGHTS AND WEAKNESS

        public string ListClientCurrentPotencialStrngths { get; set; }
        public string ListClientCurrentPotencialWeakness { get; set; }

        //SECTION VII: RECOMMENDED SERVICES 

        public bool RecommendedMentalHealth { get; set; }
        public bool RecommendedPhysicalHealth { get; set; }
        public bool RecommendedVocation { get; set; }
        public bool RecommendedSchool { get; set; }
        public bool RecommendedRecreational { get; set; }
        public bool RecommendedActivities { get; set; }
        public bool RecommendedHousing { get; set; }
        public bool RecommendedEconomic { get; set; }
        public bool RecommendedBasicNeed { get; set; }
        public bool RecommendedTransportation { get; set; }
        public bool RecommendedLegalImmigration { get; set; }
        public bool RecommendedOther { get; set; }
        public string RecommendedOtherSpecify { get; set; }

        //SECTION VII: RECOMMENDED SERVICES 

        public bool AHomeVisit { get; set; }
        public string AHomeVisitOn { get; set; }

        public bool CaseManagerWas { get; set; }
        public string CaseManagerWasDueTo { get; set; }

        public string HoweverOn { get; set; }

        [DataType(DataType.Date)]
        public DateTime HoweverVisitScheduler { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateSignatureCaseManager { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateSignatureTCMSupervisor { get; set; }

        public TCMSupervisorEntity TCMSupervisor { get; set; }

        public TCMDocumentStatus Status { get; set; }
        public int Approved { get; set; }

        public IEnumerable<TCMMessageEntity> TcmMessages { get; set; }
    }
}
