using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class Workday_ClientIndTherapyViewModel
    {
        public  Workday_Client workday_Client { get; set; }

        public string NombreClient { get; set; }
    }
}
