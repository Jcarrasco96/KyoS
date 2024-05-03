using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Models
{
    public class BirthDayViewModel
    {
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "BirthDay")]
        [DataType(DataType.Date)]
        public DateTime BirthDay { get; set; }

        [Display(Name = "Gender")]
        public GenderType Gender { get; set; }

        [Display(Name = "Arriving")]
        [DataType(DataType.Date)]
        public DateTime Arriving { get; set; }

        public int Arrived { get; set; }

        public string Program { get; set; }
        public string Person { get; set; }
        public string Code { get; set; }
    }
}
