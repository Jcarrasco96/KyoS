using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;
using KyoS.Web.Data.Abstract;

namespace KyoS.Web.Data.Entities
{
    public class EligibilityEntity : Document
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }        

        [Display(Name = "Eligibility date")]
        [DataType(DataType.Date)]
        
        public DateTime EligibilityDate { get; set; }

        [Display(Name = "Date Up")]
        [DataType(DataType.Date)]

        public DateTime DateUp { get; set; }

        public string AdmissionedFor { get; set; }

        public bool Exists { get; set; }

    }
}
