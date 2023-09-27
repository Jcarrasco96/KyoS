using KyoS.Web.Data.Contracts;
using System.ComponentModel.DataAnnotations;
using System;
using KyoS.Common.Enums;

namespace KyoS.Web.Data.Entities
{
    public class HealthInsuranceTempEntity : AuditableEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [Display(Name = "Approved Date")]
        [DataType(DataType.Date)]
        public DateTime ApprovedDate { get; set; }

        [Display(Name = "Member Id")]
        public string MemberId { get; set; }

        [Display(Name = "Duration time (months)")]
        public int DurationTime { get; set; }

        public int Units { get; set; }

        public bool Active { get; set; }

        public string UserName { get; set; }

        public int IdClient { get; set; }

        [Display(Name = "Authorization Number")]
        public string AuthorizationNumber { get; set; }

        public ServiceAgency Agency { get; set; }
    }
}
