using KyoS.Web.Data.Abstract;
using System.Collections.Generic;

namespace KyoS.Web.Data.Entities
{
    public class DoctorEntity : Contact
    {
        public int Id { get; set; }
        public virtual ICollection<ClientEntity> Clients { get; set; }
        public string FaxNumber { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }
}
