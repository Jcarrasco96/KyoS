using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class DiagnosisTempEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Code { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public string Description { get; set; }        
    }
}
