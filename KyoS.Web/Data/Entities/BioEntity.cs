using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class BioEntity : AuditableEntity
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }

        public int Client_FK { get; set; }

        [Display(Name = "Date of Client Signature")]
        [DataType(DataType.Date)]
        public DateTime DateSignaturePerson { get; set; }

        [Display(Name = "Date of BIO-PSYCHOSOCIAL")]
        [DataType(DataType.Date)]
        public DateTime DateBio { get; set; }

        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]        
        public DateTime StartTime { get; set; }

        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime EndTime { get; set; }

        public string Setting { get; set; }
        public bool CMH { get; set; }
        public bool Priv { get; set; }
        public bool BioH0031HN { get; set; }
        public bool IDAH0031HO { get; set; }
        public bool Code90791 { get; set; }

        //2. CLIENT’S PRESENTING PROBLEM 
        public string PresentingProblem { get; set; }
        public string ClientAssessmentSituation { get; set; }
        public string FamilyAssessmentSituation { get; set; }
        public string FamilyEmotional { get; set; }
        public string LegalAssessment { get; set; }

        //3. BEHAVIORAL HEALTH HISTORY 
        //public List<Bio_BehavioralHistoryEntity> List_BehavioralHistory { get; set; }

        //5-A Mini Mental Status
        public bool Appearance_Disheveled { get; set; }
        public bool Appearance_FairHygiene { get; set; }
        public bool Appearance_Cleaned { get; set; }
        public bool Appearance_WellGroomed { get; set; }
        public bool Appearance_Bizarre { get; set; }

        public bool Motor_Normal { get; set; }
        public bool Motor_Agitated { get; set; }
        public bool Motor_Retardation { get; set; }
        public bool Motor_RestLess { get; set; }
        public bool Motor_Akathisia { get; set; }
        public bool Motor_Tremor { get; set; }
        public bool Motor_Other { get; set; }

        public bool Speech_Normal { get; set; }
        public bool Speech_Loud { get; set; }
        public bool Speech_Mumbled { get; set; }
        public bool Speech_Stutters { get; set; }
        public bool Speech_Pressured { get; set; }
        public bool Speech_Rapid { get; set; }
        public bool Speech_Impoverished { get; set; }
        public bool Speech_Slow { get; set; }
        public bool Speech_Slurred { get; set; }
        public bool Speech_Other { get; set; }

        public bool Affect_Appropriate { get; set; }
        public bool Affect_labile { get; set; }
        public bool Affect_Flat { get; set; }
        public bool Affect_Tearful_Sad { get; set; }
        public bool Affect_Expansive { get; set; }
        public bool Affect_Anxious { get; set; }
        public bool Affect_Blunted { get; set; }
        public bool Affect_Angry { get; set; }
        public bool Affect_Constricted { get; set; }
        public bool Affect_Other { get; set; }
        
        public bool ThoughtProcess_Organized { get; set; }
        public bool ThoughtProcess_Obsessive { get; set; }
        public bool ThoughtProcess_FightIdeas { get; set; }
        public bool ThoughtProcess_Disorganized { get; set; }
        public bool ThoughtProcess_Tangential { get; set; }
        public bool ThoughtProcess_LooseAssociations { get; set; }
        public bool ThoughtProcess_GoalDirected { get; set; }
        public bool ThoughtProcess_Circumstantial { get; set; }
        public bool ThoughtProcess_Other { get; set; }
        public bool ThoughtProcess_Irrational { get; set; }
        public bool ThoughtProcess_Preoccupied { get; set; }
        public bool ThoughtProcess_Rigid { get; set; }
        public bool ThoughtProcess_Blocking { get; set; }

        public bool Mood_Euthymic { get; set; }
        public bool Mood_Depressed { get; set; }
        public bool Mood_Anxious { get; set; }
        public bool Mood_Euphoric { get; set; }
        public bool Mood_Angry { get; set; }
        public bool Mood_Maniac { get; set; }
        public bool Mood_Other { get; set; }

        public bool Judgment_Good { get; set; }
        public bool Judgment_Fair { get; set; }
        public bool Judgment_Poor { get; set; }
        public bool Judgment_Other { get; set; }
        
        public bool Insight_Good { get; set; }
        public bool Insight_Fair { get; set; }
        public bool Insight_Poor { get; set; }
        public bool Insight_Other { get; set; }

        public bool ThoughtContent_Relevant { get; set; }
        public bool ThoughtContent_RealityBased { get; set; }
        public bool ThoughtContent_Hallucinations { get; set; }
        public string ThoughtContent_Hallucinations_Type { get; set; }
        public bool ThoughtContent_Delusions { get; set; }
        public string ThoughtContent_Delusions_Type { get; set; }

        public bool Oriented_FullOriented { get; set; }

        public bool Lacking_Time { get; set; }
        public bool Lacking_Place { get; set; }
        public bool Lacking_Person { get; set; }
        public bool Lacking_Location { get; set; }

        public bool RiskToSelf_Low { get; set; }
        public bool RiskToSelf_Medium { get; set; }
        public bool RiskToSelf_High { get; set; }
        public bool RiskToSelf_Chronic { get; set; }

        public bool RiskToOther_Low { get; set; }
        public bool RiskToOther_Medium { get; set; }
        public bool RiskToOther_High { get; set; }
        public bool RiskToOther_Chronic { get; set; }

        public bool SafetyPlan { get; set; }

        public string Comments { get; set; }
        
        //5-B Mini Mental Status
        public bool ClientDenied { get; set; }

        public bool HaveYouEverThought { get; set; }
        public string HaveYouEverThought_Explain { get; set; }

        public bool DoYouOwn { get; set; }
        public string DoYouOwn_Explain { get; set; }

        public bool DoesClient { get; set; }

        public bool HaveYouEverBeen { get; set; }
        public string HaveYouEverBeen_Explain { get; set; }

        public bool HasTheClient { get; set; }
        public string HasTheClient_Explain { get; set; }

        //5-C
        public bool ClientFamilyAbusoTrauma { get; set; }

        [Display(Name = "Date of Abuse")]
        
        public string DateAbuse { get; set; }
        public string PersonInvolved { get; set; }

        [Display(Name = "Date of Approximate Date Report")]
        
        public string ApproximateDateReport { get; set; }

        public string ApproximateDateReport_Where { get; set; }
        public string RelationShips { get; set; }
        public string Details { get; set; }
        public string Outcome { get; set; }
        public bool AReferral { get; set; }
        public string AReferral_Services { get; set; }
        public string AReferral_When { get; set; }
        public string AReferral_Where { get; set; }
        public bool ObtainRelease { get; set; }
        public bool WhereRecord { get; set; }
        public string WhereRecord_When { get; set; }
        public string WhereRecord_Where { get; set; }

        //6-PHYSICAL HEALTH ASSESSMENT 
        public bool HasTheClientVisitedPhysician { get; set; }
        public string HasTheClientVisitedPhysician_Reason { get; set; }

        public string HasTheClientVisitedPhysician_Date { get; set; }
        public bool DoesTheClientExperience { get; set; }
        public string DoesTheClientExperience_Where { get; set; }

        public string ForHowLong { get; set; }
        public int PleaseRatePain { get; set; }
        public bool HasClientBeenTreatedPain { get; set; }
        public string HasClientBeenTreatedPain_PleaseIncludeService { get; set; }
        public string HasClientBeenTreatedPain_Ifnot { get; set; }
        public string HasClientBeenTreatedPain_Where { get; set; }
        public bool ObtainReleaseInformation { get; set; }

        public bool HasAnIllnes { get; set; }
        public bool EastFewer { get; set; }
        public bool EastFew { get; set; }
        public bool Has3OrMore { get; set; }
        public bool HasTooth { get; set; }
        public bool DoesNotAlways { get; set; }
        public bool EastAlone { get; set; }
        public bool Takes3OrMore { get; set; }
        public bool WithoutWanting { get; set; }
        public bool NotAlwaysPhysically { get; set; }
        //new
        public bool AnyFood { get; set; }
        public bool AnyEating { get; set; }

        public string If6_ReferredTo { get; set; }

        [Display(Name = "Date If 6 or referred")]
        [DataType(DataType.Date)]
        public DateTime? If6_Date { get; set; }
        public Bio_Appetite Appetite { get; set; }
        public Bio_Hydration Hydration { get; set; }
        public Bio_RecentWeightChange RecentWeight { get; set; }

        //7. PSYCHOSOCIAL HISTORY
        public string SubstanceAbuse { get; set; }
        public bool MilitaryServiceHistory { get; set; }
        public string MilitaryServiceHistory_Explain { get; set; }
        public string VocationalAssesment { get; set; }
        public string LegalHistory { get; set; }
        public string PersonalFamilyPsychiatric { get; set; }
        public bool DoesClientRequired { get; set; }
        public string DoesClientRequired_Where { get; set; }
        public bool ObtainReleaseInformation7 { get; set; }

        //8. BACKGROUND, SOCIAL AND EDUCATIONAL 
        public bool IfForeing_Born { get; set; }
        public int IfForeing_AgeArrival { get; set; }
        public int IfForeing_YearArrival { get; set; }
        public string PrimaryLocation { get; set; }
        public string GeneralDescription { get; set; }
        public string AdultCurrentExperience { get; set; }
        public string WhatIsTheClient { get; set; }

        //9. FAMILY EXPERIENCE:
        public string RelationshipWithFamily { get; set; }
        public string Children { get; set; }
        public string MaritalStatus { get; set; }
        public string IfMarried { get; set; }
        public string IfSeparated { get; set; }
        public Bio_IfSexuallyActive IfSexuallyActive { get; set; }

        //10. EDUCATIONAL ASSESSMENT
        public string PleaseProvideGoal { get; set; }
        public bool DoYouHaveAnyReligious { get; set; }
        public string WhatIsYourLanguage { get; set; }
        public bool DoYouHaveAnyVisual { get; set; }
        public string HigHestEducation { get; set; }
        public bool DoYouHaveAnyPhysical { get; set; }
        public bool CanClientFollow { get; set; }

        //11. PROVIDE INTEGRATED SUMMARY BELOW:
        public string ProvideIntegratedSummary { get; set; }

        //12. TREATMENT NEEDS
        public string TreatmentNeeds { get; set; }

        //13 TREATMENT RECOMMENDATIOS:
        public string Treatmentrecomendations { get; set; }

        public SupervisorEntity Supervisor { get; set; }

        [Display(Name = "Date of Licensed Practitioner Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureLicensedPractitioner { get; set; }

        public DocumentsAssistantEntity DocumentsAssistant  { get; set; }

        [Display(Name = "Date of UnlicensedTherapist  Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureUnlicensedTherapist  { get; set; }

        public bool IConcurWhitDiagnistic { get; set; }
        public string AlternativeDiagnosis { get; set; }

        [Display(Name = "Date of Supervisor Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureSupervisor { get; set; }

        public string AdmissionedFor { get; set; }

        public BioStatus Status { get; set; }

        public IEnumerable<MessageEntity> Messages { get; set; }

        public string CodeBill { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BilledDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }

        public bool DeniedBill { get; set; }

        public int Units { get; set; }
        public PayStubEntity PayStub { get; set; }
    }
}
