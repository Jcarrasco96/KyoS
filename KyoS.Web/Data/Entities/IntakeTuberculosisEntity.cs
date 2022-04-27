using System;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class IntakeTuberculosisEntity
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

        public string AdmissionedFor { get; set; }

        public bool DoYouCurrently { get; set; }
        public bool DoYouBring { get; set; }
        public bool DoYouCough { get; set; }
        public bool DoYouSweat { get; set; }
        public bool DoYouHaveFever { get; set; }
        public bool HaveYouLost { get; set; }
        public bool DoYouHaveChest { get; set; }
        public bool If2OrMore { get; set; }
        public bool HaveYouRecently { get; set; }
        public bool AreYouRecently { get; set; }
        public bool IfYesWhich { get; set; }
        public bool DoYouOr { get; set; }
        public bool HaveYouEverBeen { get; set; }
        public bool HaveYouEverWorked { get; set; }
        public bool HaveYouEverHadOrgan { get; set; }
        public bool HaveYouEverConsidered { get; set; }
        public bool HaveYouEverHadAbnormal { get; set; }
        public bool If3OrMore { get; set; }
        public bool HaveYouEverHadPositive { get; set; }
        public string IfYesWhere { get; set; }
        public string When { get; set; }
        public bool HaveYoyEverBeenTold { get; set; }
        public bool AgencyExpectation { get; set; }
        public bool If1OrMore { get; set; }
        public bool Documents { get; set; }
    }
}
