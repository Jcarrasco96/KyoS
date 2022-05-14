using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace KyoS.Web.Models
{
    public class GoalViewModel : GoalEntity
    {
        public int IdMTP { get; set; }
        
        public int IdService { get; set; }
        public IEnumerable<SelectListItem> Services { get; set; }

        public int IdAdendum { get; set; }
    }
}
