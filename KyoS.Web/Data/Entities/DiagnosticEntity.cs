using KyoS.Web.Data.Contracts;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class DiagnosticEntity : AuditableEntity
    {
        public int Id { get; set; }

        [Display(Name = "Code")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Code { get; set; }

        [Display(Name = "Diagnostic")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Description { get; set; }
    }
}
