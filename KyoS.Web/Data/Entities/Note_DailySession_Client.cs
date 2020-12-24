namespace KyoS.Web.Data.Entities
{
    public class Note_DailySession_Client
    {
        public int Id { get; set; }
        public NoteEntity Note { get; set; }
        public DailySessionEntity DailySession { get; set; }
        public ClientEntity Client { get; set; }
        public PlanEntity Plan { get; set; }
    }
}
