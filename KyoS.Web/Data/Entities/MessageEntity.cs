using KyoS.Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;

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

        public FarsFormEntity FarsForm { get; set; }

        public MTPReviewEntity MTPReview { get; set; }

        public AdendumEntity Addendum { get; set; }

        public DischargeEntity Discharge { get; set; }

        public MTPEntity Mtp { get; set; }

        public BioEntity Bio { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public bool Notification { get; set; }
    }
}
