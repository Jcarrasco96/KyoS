using System;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace KyoS.Web.Models
{
    public class TCMIntakeConsentForReleaseTypeViewModel
    {
        public int IdTCMClient { get; set; }

        public int IdType { get; set; }
        public int origi { get; set; }

        public IEnumerable<SelectListItem> Types { get; set; }


    }
}
