using KyoS.Web.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class DateUntilViewModel
    {
        public int IdObjective { get; set; }

        public int IdMtp { get; set; }

        [Display(Name = "Date Active Until")]
        [DataType(DataType.Date)]
        public DateTime DateUntil { get; set; }

    }
}
