using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class ClientViewModel : ClientEntity
    {
        [Display(Name = "Photo")]
        public IFormFile PhotoFile { get; set; }

        [Display(Name = "Sign")]
        public IFormFile SignFile { get; set; }

        [Display(Name = "Clinic")]
        public int IdClinic { get; set; }
        public IEnumerable<SelectListItem> Clinics { get; set; }        

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Status")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a status.")]
        public int IdStatus { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Gender")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a gender.")]
        public int IdGender { get; set; }
        public IEnumerable<SelectListItem> GenderList { get; set; }

        [Display(Name = "Race")]
        public int IdRace { get; set; }
        public IEnumerable<SelectListItem> Races { get; set; }

        [Display(Name = "Marital Status")]
        public int IdMaritalStatus { get; set; }
        public IEnumerable<SelectListItem> Maritals { get; set; }

        [Display(Name = "Ethnicity")]
        public int IdEthnicity { get; set; }
        public IEnumerable<SelectListItem> Ethnicities { get; set; }

        [Display(Name = "PreferredLanguage")]
        public int IdPreferredLanguage { get; set; }
        public IEnumerable<SelectListItem> Languages { get; set; }

        [Display(Name = "Referred")]
        public int IdReferred { get; set; }
        public IEnumerable<SelectListItem> Referreds { get; set; }        

        [Display(Name = "Emergency Contact")]
        public int IdEmergencyContact { get; set; }
        public IEnumerable<SelectListItem> EmergencyContacts { get; set; }

        [Display(Name = "Doctor")]
        public int IdDoctor { get; set; }
        public IEnumerable<SelectListItem> Doctors { get; set; }
        
        [Display(Name = "Psychiatrist")]
        public int IdPsychiatrist { get; set; }
        public IEnumerable<SelectListItem> Psychiatrists { get; set; }

        [Display(Name = "Legal Guardian")]
        public int IdLegalGuardian { get; set; }
        public IEnumerable<SelectListItem> LegalsGuardians { get; set; }

        [Display(Name = "Relationship with legal guardian")]
        public int IdRelationship { get; set; }
        public IEnumerable<SelectListItem> Relationships { get; set; }

        public IEnumerable<DiagnosticTempEntity> DiagnosticTemp { get; set; }

        public IEnumerable<DocumentTempEntity> DocumentTemp { get; set; }
    }
}
