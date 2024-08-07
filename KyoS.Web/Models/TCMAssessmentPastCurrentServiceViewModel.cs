﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMAssessmentPastCurrentServiceViewModel : TCMAssessmentPastCurrentServiceEntity
    {
        public int IdTCMAssessment { get; set; }

        public int IdEffectivess { get; set; }
        public IEnumerable<SelectListItem> EffectivessList { get; set; }
    }
}
