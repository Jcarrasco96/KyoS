﻿using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class PromotionPhotoViewModel : PromotionPhotosEntity
    {

        [Display(Name = "Photo")]
        public IFormFile PhotoFile { get; set; }
        public int IdPromotion { get; set; }
    }
}
