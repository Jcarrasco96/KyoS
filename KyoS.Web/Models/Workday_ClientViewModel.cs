using KyoS.Web.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Models
{
    public class Workday_ClientViewModel : Workday_Client
    {
        public int Origin { get; set; }
    }
}
