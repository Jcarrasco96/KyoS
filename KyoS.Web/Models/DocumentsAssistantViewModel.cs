using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Models
{
    public class DocumentsAssistantViewModel : DocumentsAssistantEntity
    {
        //internal object TCM_Active;

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Clinic")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a clinic.")]
        public int IdClinic { get; set; }
        public IEnumerable<SelectListItem> Clinics { get; set; }

        [Display(Name = "Linked user")]              
        public string IdUser { get; set; }
        public IEnumerable<SelectListItem> UserList { get; set; }

        [Display(Name = "Signature")]
        public IFormFile SignatureFile { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Gender")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a gender.")]
        public int IdGender { get; set; }
        public IEnumerable<SelectListItem> GenderList { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Payment Methods")]
        [Range(0, int.MaxValue, ErrorMessage = "You must select a payment method.")]
        public int IdPaymentMethod { get; set; }
        public IEnumerable<SelectListItem> PaymentMethodList { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Account Type")]
        [Range(0, int.MaxValue, ErrorMessage = "You must select a account type.")]
        public int IdAccountType { get; set; }
        public IEnumerable<SelectListItem> AccountTypeList { get; set; }

        public List<DocumentAssistantCertificationEntity> DocumentAssistantCertificationIdealList { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Status")]
        [Range(1, int.MaxValue, ErrorMessage = "You must select a status.")]
        public int IdStatus { get; set; }

        public IEnumerable<SelectListItem> StatusList { get; set; }
    }
}
