using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class AdendumEntity : AuditableEntity
    {
        public int Id { get; set; }

        public MTPEntity Mtp { get; set; }

        public List<GoalEntity> Goals { get; set; }

        [Display(Name = "Date Identified")]
        [DataType(DataType.Date)]

        public DateTime Dateidentified { get; set; }

        [Display(Name = "Problem Statement")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]

        public string ProblemStatement { get; set; }

        [Display(Name = "Unit")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]

        public int Unit { get; set; }

        [Display(Name = "Duration")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]

        public int Duration { get; set; }

        [Display(Name = "Frecuency")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]

        public string Frecuency { get; set; }

        public FacilitatorEntity Facilitator { get; set; }

        public SupervisorEntity Supervisor { get; set; }

        public AdendumStatus Status { get; set; }

        public IEnumerable<MessageEntity> Messages { get; set; }
        
        [DataType(DataType.Date)]

        public DateTime DateOfApprove { get; set; }
    }
}
