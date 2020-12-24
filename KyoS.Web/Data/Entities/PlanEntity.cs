using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class PlanEntity
    {
        public int Id { get; set; }

        [Display(Name = "Text")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Text { get; set; }

        public ICollection<Plan_Classification> Classifications { get; set; }
    }
}
