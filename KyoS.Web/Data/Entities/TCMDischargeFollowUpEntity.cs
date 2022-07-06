using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMDischargeFollowUpEntity : AuditableEntity
    {
        public int Id { get; set; }

        [Display(Name = "Provider Agency")]
        public string ProviderAgency { get; set; }

        [Display(Name = "Type of Service")]
        public string TypeService { get; set; }

        [Display(Name = "Address/Location")]
        public string Address_Location { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Address/Location")]
        public string NextAppt { get; set; }
        
        public TCMDischargeEntity TcmDischarge { get; set; }
    }
}
