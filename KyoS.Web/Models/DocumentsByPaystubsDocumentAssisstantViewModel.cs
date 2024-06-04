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
    public class DocumentsByPaystubsDocumentAssisstantViewModel
    {
        public int Id { get; set; }

        public string EmployeeName { get; set; }
        public string ClientName { get; set; }
        public string Document { get; set; }
        public string Status { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateService { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DateBill { get; set; }
        [DataType(DataType.Date)]
        public DateTime? DatePayment { get; set; }
        public int Units { get; set; }
        public decimal Amount { get; set; }

        public decimal Money { get; set; }
        public int IdDocumentAssisstant { get; set; }
        public DocumentsAssistantEntity DocumentAssisstant { get; set; }
    }
}
