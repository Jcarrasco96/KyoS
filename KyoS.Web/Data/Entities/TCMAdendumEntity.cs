﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMAdendumEntity : AuditableEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateAdendum { get; set; }
        public int Approved { get; set; }


        public TCMServicePlanEntity TcmServicePlan{ get; set; }
        public TCMDomainEntity TcmDomain { get; set; }
         
        public string NeedsIdentified { get; set; }
        public string LongTerm { get; set; }
    }
}
