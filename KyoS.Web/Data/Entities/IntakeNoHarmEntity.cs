using System;
using System.ComponentModel.DataAnnotations;


namespace KyoS.Web.Data.Entities
{
    public class IntakeNoHarmEntity
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }

        public int Client_FK { get; set; }

        [Display(Name = "Date of Person Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignaturePerson { get; set; }

        [Display(Name = "Date of Employee Signature")]
        [DataType(DataType.Date)]
        public DateTime DateSignatureEmployee { get; set; }

        public string AdmissionedFor { get; set; }

        public bool Documents { get; set; }
    }
}
