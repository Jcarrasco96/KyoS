namespace KyoS.Web.Models
{
    public class NotesSummary
    {
        public string FacilitatorName { get; set; }

        public int PSRNotStarted { get; set; }
        public int PSREditing { get; set; }
        public int PSRPending { get; set; }
        public int PSRReview { get; set; }

        public int IndNotStarted { get; set; }
        public int IndEditing { get; set; }
        public int IndPending { get; set; }
        public int IndReview { get; set; }

        public int GroupNotStarted { get; set; }
        public int GroupEditing { get; set; }
        public int GroupPending { get; set; }
        public int GroupReview { get; set; }
    }
}
