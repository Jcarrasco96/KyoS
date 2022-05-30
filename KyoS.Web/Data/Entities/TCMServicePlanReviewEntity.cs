using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class TCMServicePlanReviewEntity
    {
        public int Id { get; set; }

        [Display(Name = "Start Time")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{mm/dd/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime DateServicePlanReview { get; set; }

        [Display(Name = "Start Time")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{mm/dd/yyyy}", ApplyFormatInEditMode = false)]
        public DateTime DateOpending { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string SummaryProgress { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Recomendation { get; set; }

        public int Approved { get; set; }

        public TCMServicePlanEntity TcmServicePlan { get; set; }
        public List<TCMServicePlanReviewDomainEntity> TCMServicePlanRevDomain { get; set; }
    }
}
