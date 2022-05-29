using KyoS.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class GoalEntity
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

        public MTPEntity MTP { get; set; }
        public IEnumerable<ObjetiveEntity> Objetives { get; set; }

        public AdendumEntity Adendum { get; set; }

        public bool Compliment { get; set; }
        
        [Display(Name = "Date of compliment")]
        [DataType(DataType.Date)]

        public DateTime Compliment_Date { get; set; }
        
        public string Compliment_Explain { get; set; }
        
        public int Compliment_IdMTPReview { get; set; }

        public int IdMTPReview { get; set; }
    }
}
