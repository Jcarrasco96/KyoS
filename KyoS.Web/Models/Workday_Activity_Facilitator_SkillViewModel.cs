using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class Workday_Activity_FacilitatorSkillViewModel : Workday_Activity_Facilitator
    {
        public bool activities { get; set; }
        public bool community { get; set; }
        public bool disease { get; set; }
        public bool healthy { get; set; }
        public bool life { get; set; }
        public bool relaxation { get; set; }
        public bool social { get; set; }
        public bool stress { get; set; }
        public bool coping { get; set; }
        public bool am { get; set; }
        public bool pm { get; set; }

    }
}
