using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class SettingManagerViewModel
    {
        public int IdClinic { get; set; }

        //CLinic
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

        [Display(Name = "Bill code of TCM")]
        public string CPTCode_TCM { get; set; }

        //setting
            //MH
        public bool MHClassificationOfGoals { get; set; }
        public bool MHProblems { get; set; }
        public bool SupervisorEdit { get; set; }
        public bool BillSemanalMH { get; set; }
        public bool IndNoteForAppointment { get; set; }
        public bool DischargeJoinCommission { get; set; }

        //TCM
        [DataType(DataType.Time)]
        public DateTime TCMInitialTime { get; set; }

        [DataType(DataType.Time)]
        public DateTime TCMEndTime { get; set; }
        public bool LockTCMNoteForUnits { get; set; }

        public int UnitsForDayForClient { get; set; }
       
        public bool CreateNotesTCMWithServiceplanInEdition { get; set; }
        public bool TCMSupervisorEdit { get; set; }

        public bool TCMSupervisionTimeWithCaseManager { get; set; }

        public bool DocumentAssistant_Intake { get; set; }

        public bool CreateTCMNotesWithoutDomain { get; set; }
        public int IdFiltroPayStub { get; set; }

        public IEnumerable<SelectListItem> FiltroPayStubs { get; set; }
        public bool MTPmultipleSignatures { get; set; }

        [DataType(DataType.Date)]
        public DateTime TCMLockCreateNote { get; set; }
    }
}
