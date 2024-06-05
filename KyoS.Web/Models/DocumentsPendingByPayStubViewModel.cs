using KyoS.Common.Helpers;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Models
{
    public class DocumentsPendingByPayStubViewModel
    {
        public int Id { get; set; }
        public int AmountNote { get; set; }
        public int Bio { get; set; }
        public int Mtp { get; set; }
        public int MedicalHistory { get; set; }
        public int Fars { get; set; }
        public int Units { get; set; }
        public decimal Amount { get; set; }

        [DataType(DataType.Date)]
        public DateTime DatePayStubClose { get; set; }

        public int CantFacilitators { get; set; }
        public int CantDocumentsAssisstant { get; set; }

        [DataType(DataType.Date)]
        public DateTime DatePayStub { get; set; }

        public List<Workday_Client> WorkdaysClientList { get; set; }
      
        public List<BioEntity> BioList { get; set; }
        public List<MTPEntity> MtpList { get; set; }
        public List<IntakeMedicalHistoryEntity> MedicalHistoryList { get; set; }
        public List<FarsFormEntity> FarsList { get; set; }

        [DataType(DataType.Date)]
        public DateTime DatePayStubPayment { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        [Display(Name = "Status")]
        public int IdStatus { get; set; }
        public IEnumerable<SelectListItem> StatusList { get; set; }

        public int IdFacilitator { get; set; }
        public int IdDocumentAssisstant { get; set; }
        public List<PayStubDetailsEntity> PaystubDetails { get; set; }
        public List<DocumentsByPaystubsDocumentAssisstantViewModel> DocumentsPending { get; set; }
    }
}
