﻿using System;
using System.Collections.Generic;
using System.Text;

namespace KyoS.Common.Enums
{
    public enum ResidentialStatus
    {
        LivingAlone,
        livingWithRelatives,
        livingWithNoRelatives,
        AsistedLivingFacility,
        FosterCare_GroupHome,
        Hospital_NursingHome,
        ResidentialProgram,
        Correctional_Facility,
        Homeless
    }

    public class ResidentialUtils
    {
        public static ResidentialStatus GetResidentialByIndex1(int index)
        {
            return (index == 0) ? ResidentialStatus.LivingAlone :
                   (index == 1) ? ResidentialStatus.livingWithRelatives :
                   (index == 2) ? ResidentialStatus.livingWithNoRelatives :
                   (index == 3) ? ResidentialStatus.AsistedLivingFacility :
                   (index == 4) ? ResidentialStatus.FosterCare_GroupHome :
                   (index == 5) ? ResidentialStatus.Hospital_NursingHome :
                   (index == 6) ? ResidentialStatus.ResidentialProgram :
                   (index == 7) ? ResidentialStatus.Correctional_Facility :
                   (index == 8) ? ResidentialStatus.Homeless : ResidentialStatus.LivingAlone;
        }
    }
}
