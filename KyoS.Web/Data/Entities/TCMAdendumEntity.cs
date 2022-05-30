using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;

namespace KyoS.Web.Data.Entities
{
    public class TCMAdendumEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DateAdendum { get; set; }
        public int Approved { get; set; }


        public TCMServicePlanEntity TcmServicePlan{ get; set; }
        public TCMDomainEntity TcmDomain { get; set; }
    }
}
