using KyoS.Web.Data.Abstract;
using System.Collections.Generic;

namespace KyoS.Web.Data.Entities
{
    public class EmergencyContactEntity : LegalContact
    {
        public int Id { get; set; }
        public virtual ICollection<ClientEntity> Clients { get; set; }
    }
}
