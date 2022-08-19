using KyoS.Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class TCMMessageEntity
    {
        public int Id { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime? DateRead { get; set; }

        public string Title { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Text { get; set; }

        public MessageStatus Status { get; set; }

        public TCMFarsFormEntity TCMFarsForm { get; set; }

        public TCMNoteEntity TCMNote { get; set; }

        public TCMAssessmentEntity TCMAssessment { get; set; }

        public TCMServicePlanEntity TCMServicePlan { get; set; }

        public TCMServicePlanReviewEntity TCMServicePlanReview { get; set; }

        public TCMAdendumEntity TCMAddendum { get; set; }

        public TCMDischargeEntity TCMDischarge { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public bool Notification { get; set; }
    }
}
