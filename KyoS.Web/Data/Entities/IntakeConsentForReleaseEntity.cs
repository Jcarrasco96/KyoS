﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;

namespace KyoS.Web.Data.Entities
{
    public class IntakeConsentForReleaseEntity
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }

        public int Client_FK { get; set; }


        [Display(Name = "Date of Legal Guardian Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureLegalGuardian { get; set; }

        [Display(Name = "Date of Person Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignaturePerson { get; set; }

        [Display(Name = "Date of Employee Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureEmployee { get; set; }

        public bool ToRelease { get; set; }

        public bool InForm_VerbalInformation { get; set; }
        public bool InForm_Facsimile { get; set; }
        public bool InForm_WrittenRecords { get; set; }

        public bool ForPurpose_Treatment { get; set; }
        public bool ForPurpose_CaseManagement { get; set; }
        public bool ForPurpose_Other { get; set; }
        public string ForPurpose_OtherExplain { get; set; }

        public bool Discaherge { get; set; }
        public bool SchoolRecord { get; set; }
        public bool ProgressReports { get; set; }
        public bool IncidentReport { get; set; }
        public bool PsychologycalEvaluation { get; set; }
        public bool History { get; set; }
        public bool LabWork { get; set; }
        public bool HospitalRecord { get; set; }
        public bool Other { get; set; }
        public string Other_Explain { get; set; }
        public bool Documents { get; set; }
    }
}
