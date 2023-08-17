using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMIntakePainScreenEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMClientEntity TcmClient { get; set; }

        public int TcmClient_FK { get; set; }

        public bool DoYouSuffer { get; set; }

        public bool DidYouUse { get; set; }

        public bool WereYourDrugs { get; set; }

        public bool DoYouFell{ get; set; }

        public bool DoYouBelieve { get; set; }

        public string WhereIs { get; set; }

        public string WhatCauses { get; set; }

        public string DoesYourPainEffect { get; set; }

        public bool AlwayasThere { get; set; }

        public bool ComesAndGoes { get; set; }

        [Range(0, 10, ErrorMessage = "Can only be between 0 .. 10")]
        public int CurrentPainScore { get; set; }

        public string ReferredTo { get; set; }

        [Display(Name = "Date of Referral")]
        [DataType(DataType.Date)]

        public DateTime DateOfReferral { get; set; }

        public string AdmissionedFor { get; set; }

        [Display(Name = "Date of Employee Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureEmployee { get; set; }
    }
}
