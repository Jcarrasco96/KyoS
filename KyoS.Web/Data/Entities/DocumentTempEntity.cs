using KyoS.Common.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Entities
{
    public class DocumentTempEntity
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The field {0} is mandatory.")]
        public DocumentDescription Description { get; set; }

        public string DocumentPath { get; set; }

        [Display(Name = "Document Name")]
        public string DocumentName { get; set; }

        public DateTime CreatedOn { get; set; }

        public string UserName { get; set; }
    }
}
