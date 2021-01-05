﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class Workday_Client
    {
        public int Id { get; set; }
        public WorkdayEntity Workday { get; set; }
        public ClientEntity Client { get; set; }
        public string Session 
        { 
            get 
            { 
                return this.Client.Group.Meridian; 
            } 
        }
    }
}
