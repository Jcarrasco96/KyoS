using KyoS.Web.Data.Entities;

namespace KyoS.Web.Models
{
    public class UpdateBillViewModel
    {
        public int IdWeek { get; set; }
        public string DateLong { get; set; }
        public int CountNotesPSR { get; set; }
        public int CountNotesInd { get; set; }
        public int CountNotesGroup { get; set; }


    }
}
