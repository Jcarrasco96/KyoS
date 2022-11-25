using KyoS.Web.Data.Contracts;
using System.ComponentModel.DataAnnotations;
using System;


namespace KyoS.Web.Data.Entities
{
    public class HealthInsuranceTempEntity : AuditableEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime ApprovedDate { get; set; }
        public string MemberId { get; set; }
        public int DurationTime { get; set; }
        public int Units { get; set; }
        public bool Active { get; set; }
        public string UserName { get; set; }
    }
}
