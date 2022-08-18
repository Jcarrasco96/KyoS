using System;

namespace KyoS.Web.Data.Contracts
{
    public abstract class AuditableEntity : IAuditableEntity
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
    }
}
