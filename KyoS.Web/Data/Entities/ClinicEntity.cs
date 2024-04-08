using KyoS.Common.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class ClinicEntity
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Display(Name = "Logo")]
        public string LogoPath { get; set; }

        [Display(Name = "CEO")]
        public string CEO { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Display(Name = "City")]
        public string City { get; set; }

        [Display(Name = "State")]
        public string State { get; set; }

        [Display(Name = "ZipCode")]
        public string ZipCode { get; set; }

        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [Display(Name = "FaxNo")]
        public string FaxNo { get; set; }

        public SchemaType Schema { get; set; }
        public SchemaTypeGroup SchemaGroup { get; set; }

        public ICollection<FacilitatorEntity> Facilitators { get; set; }
        public ICollection<CaseMannagerEntity> CaseManagers { get; set; }
        public ICollection<SupervisorEntity> Supervisors { get; set; }
        public ICollection<ThemeEntity> Themes { get; set; }
        public ICollection<ClientEntity> Clients { get; set; }
        public ICollection<UserEntity> Users { get; set; }
        public ICollection<WeekEntity> Weeks { get; set; }
        public ICollection<TemplateDOCEntity> Templates { get; set; }
        public SettingEntity Setting { get; set; }

        [Display(Name = "Clinical Director")]
        public string ClinicalDirector { get; set; }

        public string ProviderMedicaidId { get; set; }
        public string ProviderTaxId { get; set; }

        [Display(Name = "Signature")]
        public string SignaturePath { get; set; }

        [Display(Name = "Bill code of Ind. Therapy")]
        public string CodeIndTherapy { get; set; }

        [Display(Name = "Bill code of Group Therapy")]
        public string CodeGroupTherapy { get; set; }

        [Display(Name = "Bill code of PSR Therapy")]
        public string CodePSRTherapy { get; set; }

        [Display(Name = "Bill code of MTP")]
        public string CodeMTP { get; set; }

        [Display(Name = "Bill code of BIO")]
        public string CodeBIO { get; set; }

        [Display(Name = "Bill code of MTPR")]
        public string CodeMTPR { get; set; }

        [Display(Name = "Bill code of FARS")]
        public string CodeFARS { get; set; }

        public ICollection<TCMDateBlockedEntity> TCMDateBlockeds { get; set; }

        [Display(Name = "Bill code of TCM")]
        public string CPTCode_TCM { get; set; }

        public string Initials { get; set; }
    }
}
