using KyoS.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class NoteEntity
    {
        public int Id { get; set; }

        [Display(Name = "Client's Answer")]
        [MaxLength(1000, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string AnswerClient { get; set; }

        [Display(Name = "Facilitator's Answer")]
        [MaxLength(1000, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string AnswerFacilitator { get; set; }

        public ActivityEntity Activity { get; set; }

        [Display(Name = "Classification")]
        public NoteClassification Clasificacion { get; set; }
    }
}
