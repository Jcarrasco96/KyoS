using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class Workday_Client
    {
        public int Id { get; set; }
        public WorkdayEntity Workday { get; set; }
        public ClientEntity Client { get; set; }
        public FacilitatorEntity Facilitator { get; set; }
        public string Session{ get; set; }
        public bool Present { get; set; }
        public NoteEntity Note { get; set; }
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
    }
}
