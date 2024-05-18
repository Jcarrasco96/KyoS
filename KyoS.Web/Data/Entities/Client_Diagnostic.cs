﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class Client_Diagnostic
    {
        public int Id { get; set; }
        public ClientEntity Client { get; set; }
        public DiagnosticEntity Diagnostic { get; set; }
        public bool Principal { get; set; }

        public string Prescriber { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateIdentify { get; set; }
        public bool Active { get; set; }
    }
}
