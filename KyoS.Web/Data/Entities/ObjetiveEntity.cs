using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class ObjetiveEntity
    {
        public int Id { get; set; }

        [Display(Name = "Objetive")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Objetive { get; set; }

        [Display(Name = "Description")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Description { get; set; }

        [Display(Name = "Date Opened")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = false)]
        public DateTime DateOpened { get; set; }

        [Display(Name = "Date Target")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = false)]
        public DateTime DateTarget { get; set; }

        [Display(Name = "Date Resolved")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}", ApplyFormatInEditMode = false)]
        public DateTime DateResolved { get; set; }

        [Display(Name = "Intervention")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Intervention { get; set; }

        public GoalEntity Goal { get; set; }

        public IEnumerable<Objetive_Classification> Classifications { get; set; }
        public IEnumerable<IndividualNoteEntity> IndividualNotes { get; set; }
    }
}
