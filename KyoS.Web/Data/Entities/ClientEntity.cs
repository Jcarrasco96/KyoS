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

        [Display(Name = "Full Address")]
        public string FullAddress { get; set; }

        [Display(Name = "Alternative Address")]
        public string AlternativeAddress { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string ZipCode { get; set; }

        public string Country { get; set; }

        public string Email { get; set; }

        public string Telephone { get; set; }

        [Display(Name = "Telephone Secondary")]
        public string TelephoneSecondary { get; set; }

        [Display(Name = "Marital Status")]
        public MaritalStatus MaritalStatus { get; set; }

        public EthnicityType Ethnicity { get; set; }

        [Display(Name = "Preferred Language")]
        public virtual PreferredLanguage PreferredLanguage { get; set; }

        [Display(Name = "Other Language")]
        public string OtherLanguage { get; set; }

        [Display(Name = "Sign")]
        public string SignPath { get; set; }

        public RelationshipType RelationShipOfLegalGuardian { get; set; }

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

        public ICollection<DocumentEntity> Documents { get; set; }

        public ICollection<Client_HealthInsurance> Clients_HealthInsurances { get; set; }

        public string MissingDoc 
        {
            get
            {               
                string missingDoc = string.Empty;
                bool pe = false;
                bool intake = false;
                bool bio = false;
                bool mtp = false;
                bool fars = false;
                bool consent = false;
                if (this.Documents != null)
                { 
                    foreach (var item in this.Documents)
                    {
                        if (item.Description == DocumentDescription.Psychiatrist_evaluation)
                            pe = true;
                        if (item.Description == DocumentDescription.Intake)
                            intake = true;
                        if (item.Description == DocumentDescription.Bio)
                            bio = true;
                        if (item.Description == DocumentDescription.MTP)
                            mtp = true;
                        if (item.Description == DocumentDescription.Fars)
                            fars = true;
                        if (item.Description == DocumentDescription.Consent)
                            consent = true;
                    }

                    missingDoc = (!pe) ? "Psychiatrist_evaluation," : string.Empty;
                    missingDoc = (!intake) ? $"{missingDoc} Intake," : missingDoc;
                    missingDoc = (!bio) ? $"{missingDoc} Bio," : missingDoc;
                    missingDoc = (!mtp) ? $"{missingDoc} MTP," : missingDoc;
                    missingDoc = (!fars) ? $"{missingDoc} Fars," : missingDoc;
                    missingDoc = (!consent) ? $"{missingDoc} Consent" : missingDoc;

                    if (missingDoc.EndsWith(','))
                        missingDoc.Remove(missingDoc.Length - 1);
                }
                return missingDoc;      
            } 
        }        
    }
}
