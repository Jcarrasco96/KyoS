using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class GeneratedNote_NotePrototype
    {
        public int Id { get; set; }
        public GeneratedNoteEntity GeneratedNote { get; set; }
        public NotePrototypeEntity NotePrototype { get; set; }
        public string LinkedGoal { get; set; }
        public string LinkedObj { get; set; }
    }
}
