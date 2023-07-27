using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using KyoS.Web.Data.Abstract;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class Client_Referred
    {
        public int Id { get; set; }
        public ClientEntity Client { get; set; }
        public ReferredEntity Referred { get; set; }
        public ServiceAgency Service { get; set; }

        [Display(Name = "Referred Note")]
        public string ReferredNote { get; set; }

        public ReferredType type { get; set; }
    }
}
