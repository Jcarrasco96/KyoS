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

        public int IdFarsForm { get; set; }

        public int IdMTPReview { get; set; }

        public int IdAddendum { get; set; }

        public int IdDischarge { get; set; }

        public int IdMtp { get; set; }

        public int IdBio { get; set; }

        public int Origin { get; set; }

        public int IdBrief { get; set; }
    }
}
