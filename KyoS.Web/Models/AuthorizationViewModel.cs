using KyoS.Common.Enums;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class AuthorizationViewModel
    {
        public int IdClientHealthInsurance { get; set; }
        public int IdClient { get; set; }
        public int IdTCMClient { get; set; }

        public string Agency { get; set; }
        public string TCMClientName { get; set; }
        public string CaseManagerName { get; set; }
        public string HealthInsurance { get; set; }

        public StatusType Status { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime DateOpen { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime ExpiratedDate { get; set; }
        
        public int Info { get; set; }

        [DataType(DataType.Date)]
        public DateTime EffectiveDate { get; set; }

    }
}
