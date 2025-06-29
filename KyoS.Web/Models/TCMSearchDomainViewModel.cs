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
    public class TCMSearchDomainViewModel
    {
        //[Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Domain")]
        [Range(0, int.MaxValue, ErrorMessage = "You must select a Domain.")]

        public int IdTCMDomain { get; set; }
        public IEnumerable<SelectListItem> TCMDomainNameList { get; set; }
        public string NameTCMDomain { get; set; }
        public List<TCMDomainEntity> TCMDomains { get; set; }
    }
}
