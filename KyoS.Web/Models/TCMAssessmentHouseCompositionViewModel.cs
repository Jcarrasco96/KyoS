using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMAssessmentHouseCompositionViewModel : TCMAssessmentHouseCompositionEntity
    {
        public int IdTCMAssessment { get; set; }
    }
}
