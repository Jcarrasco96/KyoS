using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class MTPExpiredViewModel
    {
        public int Id { get; set; }
        public ClientEntity Client { get; set; }
        
        public int ExpiredDays { get; set; }
        public DateTime DateExpired { get; set; }
        public int TypeDocument { get; set; }
        public int MtpReviewCount { get; set; }

    }
}
