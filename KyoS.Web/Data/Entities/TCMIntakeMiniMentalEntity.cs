using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using KyoS.Common.Enums;

namespace KyoS.Web.Data.Entities
{
    public class TCMIntakeMiniMentalEntity
    {
        public int Id { get; set; }

        public TCMClientEntity TcmClient { get; set; }

        public int TcmClient_FK { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.Date)]

        public DateTime Date { get; set; }

        public string AdmissionedFor { get; set; }

        public int OrientationWhat { get; set; }
        public int OrientationWhere { get; set; }

        public int RegistrationName { get; set; }
        public int Attention { get; set; }
        public int Recall { get; set; }
        public int Trials { get; set; }

        public int LanguageName { get; set; }
        public int LanguageRepeat { get; set; }
        public int LanguageFollow { get; set; }
        public int LanguageRead { get; set; }
        public int LanguageWrite { get; set; }
        public int LanguageCopy { get; set; }

        public int TotalScore { get; set; }

    }
}
