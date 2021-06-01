using KyoS.Web.Data.Contracts;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Abstract
{
    public abstract class Contact : AuditableEntity
    {
        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
    }
}
