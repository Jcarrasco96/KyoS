using KyoS.Common.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Abstract;
using KyoS.Web.Data.Contracts;
using System;

namespace KyoS.Web.Data.Entities
{
    public class SupervisorEntity : Agency
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

        public ClinicEntity Clinic { get; set; }

        public ICollection<NoteEntity> Notes { get; set; }

        public ICollection<NotePEntity> NotesP { get; set; }

        public ICollection<IndividualNoteEntity> IndividualNotes { get; set; }

        public ICollection<GroupNoteEntity> GroupNotes { get; set; }

        public string RaterEducation { get; set; }

        public string RaterFMHCertification { get; set; }
        public List<ReferralFormEntity> ReferralList { get; set; }

        public StatusType Status { get; set; }


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

        public List<SupervisorCertificationEntity> SupervisorCertifications { get; set; }

        [Display(Name = "Credentials")]
        public string Credentials { get; set; }

      
    }
}
