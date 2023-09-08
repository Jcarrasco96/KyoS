using KyoS.Web.Data.Abstract;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class LegalGuardianEntity : LegalContact
    {
        public int Id { get; set; }
        public virtual ICollection<ClientEntity> Clients { get; set; }

        [Display(Name = "Sign")]
        public string SignPath { get; set; }
    }
}
