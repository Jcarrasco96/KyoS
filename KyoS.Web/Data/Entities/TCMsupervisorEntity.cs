using KyoS.Common.Enums;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;
using System.Collections.Generic;

namespace KyoS.Web.Data.Entities
{
    public class TCMSupervisorEntity : AuditableEntity
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Display(Name = "Firm")]
        public string Firm { get; set; }

        [Display(Name = "Code")]
        public string Code { get; set; }

        public string LinkedUser { get; set; }

        [Display(Name = "Signature")]
        public string SignaturePath { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public StatusType Status { get; set; }

        public ClinicEntity Clinic { get; set; }

        public string RaterEducation { get; set; }

        public string RaterFMHCertification { get; set; }

        public List<CaseMannagerEntity> CaseManagerList { get; set; }

    }
}
