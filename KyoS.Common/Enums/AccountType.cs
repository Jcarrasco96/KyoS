namespace KyoS.Common.Enums
{
    public enum AccountType
    {
        Personal_Checking,
        Personal_Saving,
        Company_Checking,
        Company_Saving
    }
    public class AccountTypeUtils
    {
        public static AccountType GetAccountTypeByIndex(int index)
        {
            return (index == 1) ? AccountType.Personal_Checking :
                   (index == 2) ? AccountType.Personal_Saving :
                   (index == 3) ? AccountType.Company_Checking :
                   (index == 4) ? AccountType.Company_Saving : AccountType.Personal_Checking;
        }
    }
  
}
