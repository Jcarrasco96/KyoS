using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMFarsFormEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMClientEntity TCMClient { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string ContractorID { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string DcfEvaluation { get; set; }

        [Display(Name = "Date of Evaluation")]
        [DataType(DataType.Date)]
        public DateTime EvaluationDate{ get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string ProviderId { get; set; }

        public string M_GafScore { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string RaterEducation { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string RaterFMHI { get; set; }

        [RegularExpression("[0-1]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int SubstanceAbusoHistory { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int DepressionScale{ get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int AnxietyScale { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int HyperAffectScale { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int ThoughtProcessScale { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int CognitiveScale { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int MedicalScale { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int TraumaticsScale { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int SubstanceScale { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int InterpersonalScale { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int FamilyRelationShipsScale { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int FamilyEnvironmentScale { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int SocialScale { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int WorkScale { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int ActivitiesScale { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int AbilityScale { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int DangerToSelfScale { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int DangerToOtherScale { get; set; }

        [RegularExpression("[1-9]{1,1}", ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory and with a value between 1 and 9")]
        public int SecurityScale { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string ContID1 { get; set; }

        public string ContID2 { get; set; }

        public string ContID3 { get; set; }

        public string ProviderLocal { get; set; }

        public string MedicaidRecipientID { get; set; }

        public string MedicaidProviderID { get; set; }

        public string MCOID { get; set; }

        public string Country { get; set; }

        [Display(Name = "Date of Signature")]
        [DataType(DataType.Date)]
        public DateTime SignatureDate { get; set; }

        public string AdmissionedFor { get; set; }

        public string ProgramEvaluation { get; set; }

        public TCMSupervisorEntity TCMSupervisor{ get; set; }

        public FarsStatus Status { get; set; }

        public IEnumerable<TCMMessageEntity> TcmMessages { get; set; }
    }
}
