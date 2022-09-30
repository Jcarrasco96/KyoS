using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KyoS.Web.Models
{
    public class GoalsTempViewModel : GoalsTempEntity
    {
        public int IdService { get; set; }
        public IEnumerable<SelectListItem> Services { get; set; }
    }
}
