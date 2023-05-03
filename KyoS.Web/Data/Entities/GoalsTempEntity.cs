using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class GoalsTempEntity
    {
        public int Id { get; set; }

        [Display(Name = "Number of Goal")]
        [Range(1, 10, ErrorMessage = "You must select a valid number of goal.")]
        public int Number { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Display(Name = "Area of Focus")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string AreaOfFocus { get; set; }

        public ServiceType Service { get; set; }

        public int IdClient { get; set; }

        public string UserName { get; set; }

        public List<ObjectiveTempEntity> ObjetiveTempList { get; set; }

        public int numberMonths { get; set; }

        [DataType(DataType.Date)]
        public DateTime AdmissionDate { get; set; }

        public int TypeDocument { get; set; }
    }
}
