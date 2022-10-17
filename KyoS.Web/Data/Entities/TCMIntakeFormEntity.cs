using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class TCMIntakeFormEntity : AuditableEntity
    {
        public int Id { get; set; }

        public TCMClientEntity TcmClient { get; set; }

        public int TcmClient_FK { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Intake Date")]
        public DateTime IntakeDate { get; set; }

        public string EducationLevel { get; set; }
        public string ReligionOrEspiritual { get; set; }
        public EmploymentStatus EmploymentStatus { get; set; }
        public ResidentialStatus ResidentialStatus { get; set; }
        public string MonthlyFamilyIncome { get; set; }
        public string PrimarySourceIncome { get; set; }

        public string TitlePosition { get; set; }
        public string Agency { get; set; }
        public bool IsClientCurrently { get; set; }

        public string Elibigility { get; set; }

        public bool MMA { get; set; }
        public bool LTC { get; set; }
        public string InsuranceOther { get; set; }

        public string School { get; set; }
        public string Grade { get; set; }
        public bool School_Regular { get; set; }
        public bool School_ESE { get; set; }
        public bool School_EBD { get; set; }
        public bool School_ESOL { get; set; }
        public bool School_HHIP { get; set; }
        public bool School_Other { get; set; }
        public string  TeacherCounselor_Name { get; set; }
        public string TeacherCounselor_Phone { get; set; }

        public string CountryOfBirth { get; set; }
        public string YearEnterUsa { get; set; }
        public bool StausCitizen { get; set; }
        public bool StatusResident { get; set; }
        public bool StatusOther { get; set; }
        public string StatusOther_Explain { get; set; }

        public string SecondaryContact { get; set; }
        public string SecondaryContact_Phone { get; set; }
        public string SecondaryContact_RelationShip { get; set; }
        public bool EmergencyContact { get; set; }
        public string Other { get; set; }
        public string Other_Phone { get; set; }
        public string Other_Address { get; set; }
        public string Other_City { get; set; }

        public bool NeedSpecial { get; set; }
        public string NeedSpecial_Specify { get; set; }
        public string CaseManagerNotes { get; set; }

        public string Psychiatrist_Name { get; set; }
        public string Psychiatrist_Phone { get; set; }
        public string Psychiatrist_Address { get; set; }
        public string Psychiatrist_CityStateZip { get; set; }

        public string PCP_Name { get; set; }
        public string PCP_Phone { get; set; }
        public string PCP_Place { get; set; }
        public string PCP_Address { get; set; }
        public string PCP_CityStateZip { get; set; }
        public string PCP_FaxNumber { get; set; }
    }
}
