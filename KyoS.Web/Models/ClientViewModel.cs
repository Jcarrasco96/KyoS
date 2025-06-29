﻿using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
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

       /* [Display(Name = "Referred")]
        public int IdReferred { get; set; }
        public IEnumerable<SelectListItem> Referreds { get; set; }    */    

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

        [Display(Name = "Relationship with emergency contact")]
        public int IdRelationshipEC { get; set; }
        public IEnumerable<SelectListItem> RelationshipsEC { get; set; }

        [Display(Name = "MH Therapy")]
        public int IdService { get; set; }
        public IEnumerable<SelectListItem> Services { get; set; }

        public IEnumerable<DiagnosticTempEntity> DiagnosticTemp { get; set; }

        public IEnumerable<ReferredTempEntity> ReferredTemp { get; set; }

        public IEnumerable<DocumentTempEntity> DocumentTemp { get; set; }

        public IEnumerable<HealthInsuranceTempEntity> HealthInsuranceTemp { get; set; }

        //este campo lo uso para saber de que pagina se viene
        public int Origin { get; set; }

        [Display(Name = "Facilitator for individual therapy")]
        public int IdFacilitatorIT { get; set; }
        public IEnumerable<SelectListItem> ITFacilitators { get; set; }

        public int IdTCMClient { get; set; }

        public string FacilitatorPSR { get; set; }

        public string FacilitatorGroup { get; set; }

        //emergency Contact
        public string NameEmergencyContact { get; set; }
        public string AddressEmergencyContact { get; set; }
        public string EmailEmergencyContact { get; set; }
        public string PhoneEmergencyContact { get; set; }
        public string PhoneSecundaryEmergencyContact { get; set; }
        public string AddressLine2EmergencyContact { get; set; }
        public string CountryEmergencyContact { get; set; }
        public string CityEmergencyContact { get; set; }
        public string StateEmergencyContact { get; set; }
        public string ZipCodeEmergencyContact { get; set; }
        public string CreateByEmergencyContact { get; set; }
        public DateTime CreateOnEmergencyContact { get; set; }

        //legalGardian
        public string NameLegalGuardian { get; set; }
        public string AddressLegalGuardian { get; set; }
        public string EmailLegalGuardian { get; set; }
        public string PhoneLegalGuardian { get; set; }
        public string PhoneSecundaryLegalGuardian { get; set; }
        public string AddressLine2LegalGuardian { get; set; }
        public string CountryLegalGuardian { get; set; }
        public string CityLegalGuardian { get; set; }
        public string StateLegalGuardian { get; set; }
        public string ZipCodeLegalGuardian { get; set; }
        public string CreateByLegalGuardian { get; set; }
        public DateTime CreateOnLegalGuardian { get; set; }

        //PrimaryDoctor
        public string NamePrimaryDoctor { get; set; }
        public string AddressPrimaryDoctor { get; set; }
        public string EmailPrimaryDoctor { get; set; }
        public string PhonePrimaryDoctor { get; set; }
        public string CityPrimaryDoctor { get; set; }
        public string StatePrimaryDoctor { get; set; }
        public string ZipCodePrimaryDoctor { get; set; }
        public string FaxNumberPrimaryDoctor { get; set; }
        public string CreateByPrimaryDoctor { get; set; }
        public DateTime CreateOnPrimaryDoctor { get; set; }

        //Psychiatrists
        public string NamePsychiatrists { get; set; }
        public string AddressPsychiatrists { get; set; }
        public string EmailPsychiatrists { get; set; }
        public string PhonePsychiatrists { get; set; }
        public string CityPsychiatrists { get; set; }
        public string StatePsychiatrists { get; set; }
        public string ZipCodePsychiatrists { get; set; }
        public string FaxNumberPsychiatrists { get; set; }
        public string CreateByPsychiatrists { get; set; }
        public DateTime CreateOnPsychiatrists { get; set; }

        [Display(Name = "Date of admission TCM")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime AdmisionDateTCM { get; set; }

        [Display(Name = "Documents Assistant")]
        public int IdDocumentsAssistant { get; set; }
        public IEnumerable<SelectListItem> DocumentsAssistants { get; set; }
        public string TCMName { get; set; }
    }
}
