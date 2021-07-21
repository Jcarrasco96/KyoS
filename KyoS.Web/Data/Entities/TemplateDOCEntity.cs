using KyoS.Common.Enums;
using KyoS.Web.Data.Abstract;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class TemplateDOCEntity : Document
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DocumentDescription Description { get; set; }

        public ClinicEntity Clinic { get; set; }
    }
}
