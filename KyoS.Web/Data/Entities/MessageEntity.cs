using KyoS.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class MessageEntity
    {
        public int Id { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime? DateRead { get; set; }

        public string Title { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Text { get; set; }

        public MessageStatus Status { get; set; }

        public Workday_Client Workday_Client { get; set; }
    }
}
