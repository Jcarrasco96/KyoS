using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class MTPEntity : AuditableEntity
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }        

        [Display(Name = "Treatment plan developed date")]
        [DataType(DataType.Date)]
        
        public DateTime MTPDevelopedDate { get; set; }

        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime StartTime { get; set; }

        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        [DisplayFormat(DataFormatString = "{0:hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime EndTime { get; set; }

        [Display(Name = "Level of care")]
        public string LevelCare { get; set; }

        [Display(Name = "Initial discharge criteria")]
        public string InitialDischargeCriteria { get; set; }

        public IEnumerable<GoalEntity> Goals { get; set; }

        [Display(Name = "Modality")]
        public string Modality { get; set; }

        [Display(Name = "Frecuency")]
        public string Frecuency { get; set; }

        [Display(Name = "Months of treatment")]
        public int? NumberOfMonths { get; set; }

        public string Setting { get; set; }

        public bool Active { get; set; }

        public List<AdendumEntity> AdendumList { get; set; }

        public List<MTPReviewEntity> MtpReviewList { get; set; }


        public string ClientStrengths { get; set; }

        public string ClientLimitation { get; set; }

        public string RationaleForUpdate { get; set; }

        [Display(Name = "MTP Admission date")]
        [DataType(DataType.Date)]

        public DateTime AdmissionDateMTP { get; set; }

        [Display(Name = "MTP update date")]
        [DataType(DataType.Date)]

        public DateTime DateOfUpdate { get; set; }

        public bool Medication { get; set; }
        public string MedicationCode { get; set; }
        public int MedicationUnits { get; set; }
        public string MedicationFrecuency { get; set; }
        public int MedicationDuration { get; set; }

        public bool Individual { get; set; }
        public string IndividualCode { get; set; }
        public int IndividualUnits { get; set; }
        public string IndividualFrecuency { get; set; }
        public int IndividualDuration { get; set; }

        public bool Family { get; set; }
        public string FamilyCode { get; set; }
        public int FamilyUnits { get; set; }
        public string FamilyFrecuency { get; set; }
        public int FamilyDuration { get; set; }

        public bool Psychosocial { get; set; }
        public string PsychosocialCode { get; set; }
        public int PsychosocialUnits { get; set; }
        public string PsychosocialFrecuency { get; set; }
        public int PsychosocialDuration { get; set; }

        public bool Group { get; set; }
        public string GroupCode { get; set; }
        public int GroupUnits { get; set; }
        public string GroupFrecuency { get; set; }
        public int GroupDuration { get; set; }

        public string AdditionalRecommended { get; set; }

        public bool Substance { get; set; }
        public string SubstanceWhere { get; set; }

        public bool Legal { get; set; }
        public string LegalWhere { get; set; }

        public bool Health { get; set; }
        public string HealthWhere { get; set; }

        public bool Paint { get; set; }
        public string PaintWhere { get; set; }

        public bool Other { get; set; }
        public string OtherWhere { get; set; }

        public string AdmissionedFor { get; set; }

        [Display(Name = "MTP Supervisor date")]
        [DataType(DataType.Date)]

        public DateTime SupervisorDate { get; set; }

        public DocumentsAssistantEntity DocumentAssistant { get; set; }
        public SupervisorEntity Supervisor { get; set; }
        public MTPStatus Status { get; set; }

        public IEnumerable<MessageEntity> Messages { get; set; }

        public string CodeBill { get; set; }

        [DataType(DataType.Date)]
        public DateTime? BilledDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }

        public bool DeniedBill { get; set; }

        public int Units { get; set; }
    }
}
