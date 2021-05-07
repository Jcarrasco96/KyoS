using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Abstract
{
    public abstract class Contact : AuditableEntity
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
    }
}
