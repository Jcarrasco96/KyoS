using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;

namespace KyoS.Web.Data.Entities
{
    public class TCMIntakeNonClinicalLogEntity
    {
        public int Id { get; set; }

        public TCMClientEntity TcmClient { get; set; }

        public int TcmClient_FK { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.Date)]

        public DateTime Date { get; set; }

        public string AdmissionedFor { get; set; }

        public string DateActivity { get; set; }
    }
}
