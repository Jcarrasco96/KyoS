using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMServicePlanReviewDomainViewModel : TCMServicePlanReviewDomainEntity
    {
        
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int IdTcmDomain { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int IdStatusDomain { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int IdStatusObjetive { get; set; }
        
        public IEnumerable<SelectListItem> status { get; set; }

        public int Id_Service { get; set; }

        public int Id_ServicePlanReview { get; set; }
        public string Origin { get; set; }

        public bool StatusBool { get; set; }

    }
}
