using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class DiagnosisEntity
    {
        public int Id { get; set; }

        [Display(Name = "Code")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Code { get; set; }

        [Display(Name = "Diagnosis")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Description { get; set; }

        public MTPEntity MTP { get; set; }
    }
}
