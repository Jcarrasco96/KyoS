using KyoS.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class ClientEntity : AuditableEntity
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public GenderType Gender { get; set; }

        [Display(Name = "Date of birth")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateOfBirth { get; set; }

        [Display(Name = "Code")]
        public string Code { get; set; }

        [Display(Name = "Medicaid ID")]
        public string MedicaidID { get; set; }

        [Display(Name = "Photo")]
        public string PhotoPath { get; set; }

        public string SSN { get; set; }

        public RaceType Race { get; set; }

        public string FullAddress { get; set; }

        public string AlternativeAddress { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string Country { get; set; }

        public string Email { get; set; }

        public string Telephone { get; set; }

        public string TelephoneSecondary { get; set; }

        public MaritalStatus MaritalStatus { get; set; }

        public EthnicityType Ethnicity { get; set; }

        public virtual PreferredLanguage PreferredLanguage { get; set; }

        public string OtherLanguage { get; set; }

        [Display(Name = "Photo")]
        public string SignPath { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public StatusType Status { get; set; }

        public ClinicEntity Clinic { get; set; }

        public GroupEntity Group { get; set; }

        public ReferredEntity Referred { get; set; }

        public LegalGuardianEntity LegalGuardian { get; set; }

        public EmergencyContactEntity EmergencyContact { get; set; }

        public DoctorEntity Doctor { get; set; }

        public PsychiatristEntity Psychiatrist { get; set; }

        public ICollection<Client_Diagnostic> Clients_Diagnostics { get; set; }

        public ICollection<MTPEntity> MTPs { get; set; }
    
        public ICollection<Workday_Client> Workdays_Clients { get; set; }
    }
}
