using KyoS.Web.Data.Abstract;
using System.Collections.Generic;

namespace KyoS.Web.Data.Entities
{
    public class ReferredEntity : Contact
    {
        public int Id{ get; set; }

        public string ReferredNote { get; set; }

        public ICollection<ClientEntity> Clients { get; set; }
    }
}
