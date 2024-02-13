using System;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using KyoS.Common.Enums;

namespace KyoS.Web.Models
{
    public class DischargeTypeViewModel
    {
        public bool JoinCommission { get; set; }
        public int IdClient { get; set; }
        public int Origin { get; set; }
        public ServiceType Service { get; set; }


    }
}
