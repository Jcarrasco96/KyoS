using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class ClassificationEntity
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }
    }
}
