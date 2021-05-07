using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Abstract
{
    public abstract class Document : AuditableEntity
    {
        public string FileUrl { get; set; }

        public string FileName { get; set; }
    }
}
