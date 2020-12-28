using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class ClassificationEntity
    {
        public int Id { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        public ICollection<Note_Classification> NotesClassification { get; set; }
        public ICollection<Objetive_Classification> ObjetivesClassification { get; set; }
        public ICollection<Plan_Classification> PlansClassification { get; set; }
    }
}
