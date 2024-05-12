using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using KyoS.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMServicePlanReviewViewModel : TCMServicePlanReviewEntity
    {
        
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Status")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a status.")]
        
        public int IdServicePlan { get; set; }

       //public List <SelectListItem> Domain_ListReview { get; set; }

        public List <TCMDomainObjetiveReview> Domain_List { get; set; }

        public IEnumerable<SelectListItem> StatusListDomain { get; set; }
        public IEnumerable<SelectListItem> StatusListObjetive { get; set; }
        public List<TCMServicePlanReviewDomainEntity> _TCMServicePlanRevDomain { get; set; }

        [Display(Name = "Date Initial Certification")]
        [DataType(DataType.Date)]
        public DateTime DateInitialCertification { get; set; }
    }
}
