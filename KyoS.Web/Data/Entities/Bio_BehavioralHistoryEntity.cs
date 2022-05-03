using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class Bio_BehavioralHistoryEntity
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }

        [MaxLength(100, ErrorMessage = "The {0} field can not have more than {1} characters.")]
        [Required(ErrorMessage = "The field {0} is mandatory.")]

        public string Problem { get; set; }

        [Display(Name = "Date of the problem")]
        [DataType(DataType.Date)]

        public DateTime Date { get; set; }
    }
}
