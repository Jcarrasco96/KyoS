using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class BillingReport1ViewModel
    {
        
        public int IdFacilitator { get; set; }
        public IEnumerable<SelectListItem> Facilitators { get; set; }

        public int IdClient { get; set; }
        public IEnumerable<SelectListItem> Clients { get; set; }

        public List<WeekEntity> Weeks { get; set; }

        public int IdWeek { get; set; }
        public IEnumerable<SelectListItem> WeeksListName { get; set; }
    }
}