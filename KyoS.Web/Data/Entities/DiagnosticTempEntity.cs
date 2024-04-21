using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using System;
using System.Collections.Generic;

using KyoS.Web.Data.Contracts;


namespace KyoS.Web.Data.Entities
{
    public class DiagnosticTempEntity
    {
        public int Id { get; set; }

        public string Code { get; set; }
        
        public string Description { get; set; }

        public bool Principal { get; set; }

        public string UserName { get; set; }

        public int IdClient { get; set; }

        [Display(Name = "Prescribe")]
        public string Prescriber { get; set; }

        [Display(Name = "Prescription date")]
        [DataType(DataType.Date)]
        public DateTime DateIdentify { get; set; }
        public bool Active { get; set; }
    }
}
