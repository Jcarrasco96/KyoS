﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class TCMSubServiceStepViewModel : TCMSubServiceStepEntity
    {
        public int Id_TCMService{ get; set; }
        public int Id_TCMSubService { get; set; }

    }
}
 