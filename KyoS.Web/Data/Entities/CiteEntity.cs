using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;
using System;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class CiteEntity : AuditableEntity
    {
        public int Id { get; set; }
        public ClientEntity Client { get; set; }
        public FacilitatorEntity Facilitator { get; set; }
        public SubScheduleEntity SubSchedule { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateCite { get; set; }

        public CiteStatus Status { get; set; }

        [Display(Name = "Event Note")]
        public string EventNote { get; set; }

        [Display(Name = "Patient Note")]
        public string PatientNote { get; set; }

        public decimal Copay { get; set; }
        public Workday_Client Worday_CLient { get; set; }        
    }
}
