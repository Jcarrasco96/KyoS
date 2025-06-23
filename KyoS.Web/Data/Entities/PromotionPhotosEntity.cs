using System;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;

namespace KyoS.Web.Data.Entities
{
    public class PromotionPhotosEntity
    {
        public int Id { get; set; }

        public PromotionEntity Promotion { get; set; }

        [Display(Name = "Photo")]
        public string PhotoPath { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }
    }
}
