using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class Workday_Client
    {
        public int Id { get; set; }
        public WorkdayEntity Workday { get; set; }
        public ClientEntity Client { get; set; }
        public FacilitatorEntity Facilitator { get; set; }
        public string Session { get; set; }
        public bool Present { get; set; }
        public NoteEntity Note { get; set; }
        public NotePEntity NoteP { get; set; }
        public IndividualNoteEntity IndividualNote { get; set; }
        public GroupNoteEntity GroupNote { get; set; }
        public IEnumerable<MessageEntity> Messages { get; set; }
        public string CauseOfNotPresent { get; set; }
        public string ClientName
        {
            get
            {
                if (Client == null)
                {
                    return string.Empty;
                }
                return Client.Name;
            }
        }
        [DataType(DataType.Date)]
        public DateTime? BilledDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }
        public int? GroupSize { get; set; }
        public bool SharedSession { get; set; }
        public bool DeniedBill { get; set; }
        public bool Hold { get; set; }
        public string CodeBill { get; set; }
        public GroupNote2Entity GroupNote2 { get; set; }
        public ScheduleEntity Schedule { get; set; }

    }
}
