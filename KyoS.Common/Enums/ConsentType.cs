namespace KyoS.Common.Enums
{
    public enum ConsentType
    {
        HURRICANE,
        PCP,
        PSYCHIATRIST,
        EMERGENCY_CONTACT,
        DCF,
        SSA,
        BANK,
        HOUSING_OFFICES,
        POLICE_STATION,
        PHARMACY,
        MEDICAL_INSURANCE,
        CAC,
        LIFELINESS_PROVIDERS,
        TAG_AGENCY,
        STS,
        DONATION_CENTERS,
        LTC,
        INTERNET_SERVICES,
        USCIS

    }
    public class ConsentUtils
    {
        public static ConsentType GetTypeByIndex(int index)
        {
            return (index == 1) ? ConsentType.HURRICANE :
                   (index == 2) ? ConsentType.PCP :
                   (index == 3) ? ConsentType.PSYCHIATRIST :
                   (index == 4) ? ConsentType.EMERGENCY_CONTACT :
                   (index == 5) ? ConsentType.DCF :
                   (index == 6) ? ConsentType.SSA :
                   (index == 7) ? ConsentType.BANK :
                   (index == 8) ? ConsentType.HOUSING_OFFICES :
                   (index == 9) ? ConsentType.POLICE_STATION :
                   (index == 10) ? ConsentType.PHARMACY :
                   (index == 11) ? ConsentType.MEDICAL_INSURANCE :
                   (index == 12) ? ConsentType.CAC :
                   (index == 13) ? ConsentType.LIFELINESS_PROVIDERS :
                   (index == 14) ? ConsentType.TAG_AGENCY :
                   (index == 15) ? ConsentType.STS :
                   (index == 16) ? ConsentType.DONATION_CENTERS :
                   (index == 17) ? ConsentType.LTC :
                   (index == 18) ? ConsentType.INTERNET_SERVICES :
                   (index == 19) ? ConsentType.USCIS : ConsentType.HURRICANE;
        }
      
    }
   
}
