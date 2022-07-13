using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMServiceActivityEntity : AuditableEntity
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public int Unit { get; set; }

        public bool Status { get; set; }

        public int Approved { get; set; }
        public int Frecuency { get; set; }

        public TCMServiceEntity TcmService { get; set; }
    }
}
