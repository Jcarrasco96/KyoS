using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class Note_Activity
    {
        public int Id { get; set; }
        public NoteEntity Note { get; set; }
        public ActivityEntity Activity { get; set; }
        
        [Display(Name = "Client's Answer")]
        [MaxLength(1000, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        public string AnswerClient { get; set; }

        [Display(Name = "Facilitator's Answer")]
        [MaxLength(1000, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        public string AnswerFacilitator { get; set; }
    }
}
