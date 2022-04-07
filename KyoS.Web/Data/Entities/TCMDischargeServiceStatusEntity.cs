using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class TCMDischargeServiceStatusEntity
    {
        public int Id { get; set; }

        public string CodeService { get; set; }

        public string NameService { get; set; }

        public bool Status { get; set; }

        public TCMDischargeEntity TcmDischarge { get; set; }
    }
}
