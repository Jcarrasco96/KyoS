using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMSubServiceStepEntity : AuditableEntity
    {
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Name { get; set; }

        public int Orden { get; set; }
        public TCMSubServiceEntity TcmSubService { get; set; }

        public bool Active { get; set; }
        public int Units { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }
    }
}
