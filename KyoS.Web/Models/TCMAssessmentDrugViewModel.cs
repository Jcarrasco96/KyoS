﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMAssessmentDrugViewModel : TCMAssessmentDrugEntity
    {
        public int IdTCMAssessment { get; set; }

        public int IdDrugs { get; set; }
        public IEnumerable<SelectListItem> DrugsList { get; set; }
    }
}
