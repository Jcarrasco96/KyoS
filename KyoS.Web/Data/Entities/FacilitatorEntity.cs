using KyoS.Common.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class FacilitatorEntity
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Display(Name = "Code")]
        public string Codigo { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public StatusType Status { get; set; }

        public string LinkedUser { get; set; }

        [Display(Name = "Signature")]
        public string SignaturePath { get; set; }

        public ClinicEntity Clinic { get; set; }

        public IEnumerable<GroupEntity> Groups { get; set; }

        public ICollection<Workday_Client> Workdays_Clients { get; set; }

        public ICollection<Workday_Activity_Facilitator> Workdays_Activities_Facilitators { get; set; }

        public ICollection<ClientEntity> ClientsFromIndividualTherapy { get; set; }

        public string RaterEducation { get; set; }

        public string RaterFMHCertification { get; set; }

        public List<CiteEntity> CiteList { get; set; }
        public List<ReferralFormEntity> ReferralFormList { get; set; }
        public List<MeetingNotes_Facilitator> SupervisorNotes_FacilitatorList { get; set; }
    }
}
