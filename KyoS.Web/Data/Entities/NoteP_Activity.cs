using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class NoteP_Activity
    {
        public int Id { get; set; }

        public NotePEntity NoteP { get; set; }

        public ActivityEntity Activity { get; set; }

        //Client's response
        public bool Cooperative { get; set; }
        public bool Assertive { get; set; }
        public bool Passive { get; set; }
        public bool Variable { get; set; }
        public bool Uninterested { get; set; }
        public bool EngagedActive { get; set; }
        public bool Distractible { get; set; }
        public bool Confused { get; set; }
        public bool Aggresive { get; set; }
        public bool Resistant { get; set; }
        public bool Other { get; set; }

        public ObjetiveEntity Objetive { get; set; }
    }
}
