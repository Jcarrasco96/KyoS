using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;

namespace KyoS.Web.Data.Entities
{
    public class PromotionEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }
        
        public string Location { get; set; }
        public string Promotion { get; set; }
        public string Room { get; set; }

        [Display(Name = "Open Date")]
        [DataType(DataType.Date)]

        public DateTime OpenDate { get; set; }

        [Display(Name = "Close Date")]
        [DataType(DataType.Date)]

        public DateTime CloseDate { get; set; }

        public string LinkReferred { get; set; }
        public string Contact { get; set; }
        public string Description { get; set; }

        public decimal Precio { get; set; }
        public bool Active { get; set; }
        public ICollection<PromotionPhotosEntity> Photos { get; set; }
    }
}
