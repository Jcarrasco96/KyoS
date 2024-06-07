using KyoS.Web.Data.Contracts;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace KyoS.Web.Data.Entities
{
    public class HealthInsuranceEntity : AuditableEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Display(Name = "Signed date")]
        [DataType(DataType.Date)]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime SignedDate { get; set; }

        [Display(Name = "Duration time (months)")]
        public int? DurationTime { get; set; }

        public string DocumentPath { get; set; }

        public bool Active { get; set; }

        public ClinicEntity Clinic { get; set; }
        public bool NeedAuthorization { get; set; }
        public IEnumerable<Client_HealthInsurance> Client_HealthInsurances { get; set; }

        //CPT Codes
        public string CPTcode_PSR { get; set; }
        public string CPTcode_Ind { get; set; }
        public string CPTcode_Group { get; set; }
        public string CPTcode_MTP { get; set; }
        public string CPTcode_BIO { get; set; }
        public string CPTcode_MTPR { get; set; }
        public string CPTcode_FARS_MH { get; set; }

        public string CPTcode_TCM { get; set; }
    }
}
