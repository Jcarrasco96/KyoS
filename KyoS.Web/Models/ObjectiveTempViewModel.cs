using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data.Entities;

namespace KyoS.Web.Models
{
    public class ObjectiveTempViewModel : ObjectiveTempEntity
    {
        public int IdGoal { get; set; }
        public int NumberGoal { get; set; }
        public string NameGoal { get; set; }
    }
}
