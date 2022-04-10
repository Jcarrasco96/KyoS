using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class IntakeConsentPhotographEntity
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }

        public int Client_FK { get; set; }


        [Display(Name = "Date of Legal Guardian Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureLegalGuardian { get; set; }

        [Display(Name = "Date of Person Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignaturePerson { get; set; }

        [Display(Name = "Date of Employee Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureEmployee { get; set; }

        public bool Photograph { get; set; }
        public bool Filmed { get; set; }
        public bool VideoTaped { get; set; }
        public bool Interviwed { get; set; }
        public bool NoneOfTheForegoing { get; set; }
        public string Other { get; set; }

        public bool Publication { get; set; }
        public bool Broadcast { get; set; }
        public bool Markrting { get; set; }
        public bool ByTODocument { get; set; }

        public bool Documents { get; set; }
    }
}
