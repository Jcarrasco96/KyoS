using System;
using System.Collections.Generic;
using System.Text;

namespace KyoS.Common.Enums
{
    public enum EmploymentStatus
    {
        EmployetFT,
        EmployetPT,
        Retired,
        Disabled,
        Homemaker,
        Student,
        Unemployed,
        N_A
    }

    public class EmployedUtils
    {
        public static EmploymentStatus GetEmployedByIndex(int index)
        {
            return (index == 0) ? EmploymentStatus.EmployetFT :
                   (index == 1) ? EmploymentStatus.EmployetPT :
                   (index == 2) ? EmploymentStatus.Retired :
                   (index == 3) ? EmploymentStatus.Disabled :
                   (index == 4) ? EmploymentStatus.Homemaker :
                   (index == 5) ? EmploymentStatus.Student :
                   (index == 6) ? EmploymentStatus.Unemployed :
                   (index == 7) ? EmploymentStatus.N_A : EmploymentStatus.EmployetFT;
        }
    }
}
