using KyoS.Common.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;
using System;

namespace KyoS.Web.Data.Abstract
{
    public abstract class Agency : AuditableEntity
    {
        [DataType(DataType.Date)]
        public DateTime HiringDate { get; set; }

        public string CredentialExpirationDate { get; set; }
        public string NPI { get; set; }
        public string SSN { get; set; }
        public string MedicaidProviderID { get; set; }
        public string MedicareProviderID { get; set; }
        public string DEALicense { get; set; }
        public string CAQH { get; set; }
        public string CompanyName { get; set; }
        public string CompanyEIN { get; set; }
        public string FinancialInstitutionsName { get; set; }
        public AccountType AccountType { get; set; }
        public string Routing { get; set; }
        public string AccountNumber { get; set; }
    }
}
