using KyoS.Web.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Models
{
    public class MessageViewModel : MessageEntity
    {
        public int IdWorkdayClient { get; set; }
        public int Origin { get; set; }
    }
}
