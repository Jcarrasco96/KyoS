using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMIntakeNutritionalScreenEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMClientEntity TcmClient { get; set; }

        public int TcmClient_FK { get; set; }

        public bool ClientHasIllnes { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int ClientHasIllnes_Value { get; set; }

        public bool ClientHasHistory { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int ClientHasHistory_Value { get; set; }

        public bool ClientEatsFewer { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int ClientEatsFewer_Value { get; set; }

        public bool ClientEatsFew { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int ClientEatsFew_Value { get; set; }

        public bool ClientHasTooth { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int ClientHasTooth_Value { get; set; }

        public bool ClientEatsAlone { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int ClientEatsAlone_Value { get; set; }

        public bool ClientTakes { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int ClientTakes_Value { get; set; }

        public bool WithoutWanting { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int WithoutWanting_Value { get; set; }

        public bool ClientAlwaysHungry { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int ClientAlwaysHungry_Value { get; set; }

        public bool ClientAlwaysThirsty { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int ClientAlwaysThirsty_Value { get; set; }

        public bool ClientVomits { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int ClientVomits_Value { get; set; }

        public bool ClientDiarrhea { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int ClientDiarrhea_Value { get; set; }

        public bool ClientBinges { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int ClientBinges_Value { get; set; }

        public bool ClientAppetiteGood { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int ClientAppetiteGood_Value { get; set; }

        public bool ClientAppetiteFair { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int ClientAppetiteFair_Value { get; set; }

        public bool ClientAppetitepoor { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int ClientAppetitepoor_Value { get; set; }

        public bool ClientFoodAllergies { get; set; }
        [Range(0, 100, ErrorMessage = "Can only be between 0 .. 100")]
        public int ClientFoodAllergies_Value { get; set; }

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
