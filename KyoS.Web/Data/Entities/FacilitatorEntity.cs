using KyoS.Common.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;
using System;
using KyoS.Web.Data.Abstract;

namespace KyoS.Web.Data.Entities
{
    public class FacilitatorEntity : Agency
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

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public decimal Money { get; set; }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Date of birth")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Gender")]
        public GenderType Gender { get; set; }

        [Display(Name = "PH")]
        public string PH { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "State")]
        public string State { get; set; }

        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }

        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }
        [Display(Name = "Assigned Date")]
        [DataType(DataType.Date)]
        public DateTime AssignedDate { get; set; }

        public List<FacilitatorCertificationEntity> FacilitatorCertifications { get; set; }

        [Display(Name = "Credentials")]
        public string Credentials { get; set; }
    }
}
