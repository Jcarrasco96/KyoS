using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class GoalEntity
    {
        public int Id { get; set; }

        [Display(Name = "Name")]        
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Display(Name = "Area of Focus")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string AreaOfFocus { get; set; }

        public MTPEntity MTP { get; set; }
        public IEnumerable<ObjetiveEntity> Objetives { get; set; }
    }
}
