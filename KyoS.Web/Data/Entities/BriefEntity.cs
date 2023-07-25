using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class BriefEntity : AuditableEntity
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

        //6-SUMMARY OF FINDINGS 
        public string SumanrOfFindings { get; set; }

        //7 TREATMENT RECOMMENDATIOS:
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
    }
}
