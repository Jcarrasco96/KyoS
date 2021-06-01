using KyoS.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Abstract
{
    public abstract class LegalContact : Contact
    {
        public string Country { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        [Display(Name = "Telephone Secondary")]
        public string TelephoneSecondary { get; set; }
        [Display(Name = "Address Line")]
        public string AdressLine2 { get; set; }        
    }
}
