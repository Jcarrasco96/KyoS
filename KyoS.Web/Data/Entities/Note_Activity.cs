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
        public string AnswerClient { get; set; }

        [Display(Name = "Facilitator's Answer")]        
        public string AnswerFacilitator { get; set; }

        public ObjetiveEntity Objetive { get; set; }
    }
}
