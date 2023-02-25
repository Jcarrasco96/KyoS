using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class SubScheduleViewModel : SubScheduleEntity
    {
        public int IdSchedule { get; set; }
    }
}
