namespace KyoS.Web.Data.Entities
{
    public class NotePrototype_DailySession_Client_Plan
    {
        public int Id { get; set; }
        public NotePrototypeEntity Note { get; set; }
        public DailySessionEntity DailySession { get; set; }
        public ClientEntity Client { get; set; }
        public PlanEntity Plan { get; set; }
    }
}
