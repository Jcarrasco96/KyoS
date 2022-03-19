﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;

namespace KyoS.Web.Data.Entities
{
    public class TCMClientEntity
    {
        public int Id { get; set; }
       
        public CaseMannagerEntity Casemanager { get; set; }
        public ClientEntity Client { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string CaseNumber { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DateTime DataOpen { get; set; }

        public DateTime DataClose { get; set; }
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int Period { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public StatusType Status { get; set; }

    }
}
