using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using System.ComponentModel.DataAnnotations;
using KyoS.Web.Data.Contracts;

namespace KyoS.Web.Data.Entities
{
    public class IntakeMedicalHistoryEntity
    {
        public int Id { get; set; }

        public ClientEntity Client { get; set; }

        public int Client_FK { get; set; }


        [Display(Name = "Date of Legal Guardian Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureLegalGuardian { get; set; }

        [Display(Name = "Date of Person Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignaturePerson { get; set; }

        [Display(Name = "Date of Employee Signature")]
        [DataType(DataType.Date)]

        public DateTime DateSignatureEmployee { get; set; }

        public string PrimaryCarePhysician { get; set; }
        public string AddressPhysician { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        //Colum 1 sheet 1
        public bool Diphtheria { get; set; }
        public bool Mumps { get; set; }
        public bool Poliomyelitis { get; set; }
        public bool RheumaticFever { get; set; }
        public bool WhoopingCough { get; set; }
        public bool Tuberculosis { get; set; }
        public bool ScarletFever { get; set; }
        public bool Hepatitis { get; set; }
        public bool HighBloodPressure { get; set; }
        public bool KidneyTrouble { get; set; }
        public bool KidneyStones { get; set; }
        public bool BloodInUrine { get; set; }
        public bool BurningUrine { get; set; }
        public bool PainfulUrination { get; set; }
        public bool EyeTrouble  { get; set; }
        public bool HearingTrouble { get; set; }
        public bool Fractures { get; set; }
        public bool EarInfections { get; set; }
        public bool FrequentNoseBleeds { get; set; }
        public bool FrequentSoreThroat { get; set; }
        public bool Hoarseness { get; set; }
        public bool Allergies { get; set; }
        public string Allergies_Describe { get; set; }

        //Colum 2 sheet 1
        public bool StomachPain { get; set; }
        public bool BlackStools { get; set; }
        public bool NightSweats { get; set; }
        public bool FrequentVomiting { get; set; }
        public bool SkinTrouble  { get; set; }
        public bool PainfulMuscles { get; set; }
        public bool PainfulJoints { get; set; }
        public bool BackPain { get; set; }
        public bool SeriousInjury { get; set; }
        public bool Surgery { get; set; }
        public bool Arthritis { get; set; }
        public bool Hemorrhoids { get; set; }
        public bool WeightLoss { get; set; }
        public bool FrequentHeadaches { get; set; }
        public bool Fainting { get; set; }
        public bool ConvulsionsOrFits { get; set; }
        public bool LossOfMemory { get; set; }
        public bool Nervousness { get; set; }
        public bool ChronicCough { get; set; }
        public bool CoughingOfBlood { get; set; }
        public bool VenerealDisease { get; set; }

        //Colum 3 sheet 1
        public bool FrequentColds { get; set; }
        public bool HeartPalpitation { get; set; }
        public bool ChestPain { get; set; }
        public bool ShortnessOfBreath { get; set; }
        public bool SwellingOfFeet { get; set; }
        public bool SwollenAnkles { get; set; }
        public bool ChronicIndigestion { get; set; }
        public bool VomitingOfBlood { get; set; }
        public bool Jaundice { get; set; }
        public bool Constipation { get; set; }
        public bool BloodyStools { get; set; }
        public bool Cancer { get; set; }
        public bool Diabetes { get; set; }
        public bool HayFever { get; set; }
        public bool Hernia { get; set; }
        public bool HeadInjury { get; set; }
        public bool Rheumatism { get; set; }
        public bool Epilepsy { get; set; }
        public bool VaricoseVeins { get; set; }
        public bool Anemia { get; set; }
        public bool InfectiousDisease { get; set; }

        //sheet 2
        public bool FamilyDiabetes { get; set; }
        public string FamilyDiabetes_ { get; set; }
        public bool FamilyCancer { get; set; }
        public string FamilyCancer_ { get; set; }
        public bool FamilyTuberculosis { get; set; }
        public string FamilyTuberculosis_ { get; set; }
        public bool FamilyHeartDisease { get; set; }
        public string FamilyHeartDisease_ { get; set; }
        public bool FamilyKidneyDisease { get; set; }
        public string FamilyKidneyDisease_ { get; set; }
        public bool FamilyHighBloodPressure { get; set; }
        public string FamilyHighBloodPressure_ { get; set; }
        public bool FamilyHayFever { get; set; }
        public string FamilyHayFever_ { get; set; }
        public bool FamilyAsthma { get; set; }
        public string FamilyAsthma_ { get; set; }
        public bool FamilyEpilepsy { get; set; }
        public string FamilyEpilepsy_ { get; set; }
        public bool FamilyGlaucoma { get; set; }
        public string FamilyGlaucoma_ { get; set; }
        public bool FamilySyphilis { get; set; }
        public string FamilySyphilis_ { get; set; }
        public bool FamilyNervousDisorders { get; set; }
        public string FamilyNervousDisorders_ { get; set; }
        public bool FamilyOther { get; set; }
        public string FamilyOther_ { get; set; }

        public bool HaveYouEverBeenPregnant { get; set; }
        public bool HaveYouEverHadComplications { get; set; }
        public bool HaveYouEverHadPainful { get; set; }
        public bool HaveYouEverHadExcessive { get; set; }
        public bool HaveYouEverHadSpotting { get; set; }
        public bool AreYouCurrently { get; set; }

        public string AgeOfFirstMenstruation { get; set; }
        public string UsualDurationOfPeriods { get; set; }
        
        [Display(Name = "Date of last pelvic exam (PAP)")]
        [DataType(DataType.Date)]

        public DateTime DateOfLastPelvic { get; set; }

        public string UsualIntervalBetweenPeriods { get; set; }

        [Display(Name = "Date of last period")]
        [DataType(DataType.Date)]

        public DateTime DateOfLastPeriod { get; set; }

        [Display(Name = "Date of last breast exam")]
        [DataType(DataType.Date)]
        public DateTime DateOfLastBreastExam { get; set; }

        public string AdmissionedFor { get; set; }        

        //sheet 3
        public bool AreYouPhysician { get; set; }
        public bool DoYouSmoke { get; set; }
        public string DoYouSmoke_PackPerDay { get; set; }
        public string DoYouSmoke_Year { get; set; }

        public string ListAllCurrentMedications  { get; set; }

        public bool PerformingCertainMotions { get; set; }
        public bool AssumingCertainPositions { get; set; }
        public bool Hearing { get; set; }
        public bool Seeing { get; set; }
        public bool Speaking { get; set; }

        public bool Reading { get; set; }
        public bool Concentrating { get; set; }
        public bool Comprehending { get; set; }
        public bool BeingConfused { get; set; }

        public bool BeingDisorientated { get; set; }
        public bool Calculating { get; set; }
        public bool WritingSentence { get; set; }
        public bool Walking { get; set; }


        public bool InformationProvided { get; set; }
        public bool Planned { get; set; }
        public bool Unplanned { get; set; }
        
        public bool Normal { get; set; }
        public bool Complications { get; set; }
        public string Complications_Explain { get; set; }

        public string BirthWeight { get; set; }
        public string Length { get; set; }

        public bool BreastFed { get; set; }

        public string BottleFedUntilAge { get; set; }
        public string AgeWeaned { get; set; }

        public string FirstYearMedical { get; set; }

        public string AgeFirstWalked { get; set; }
        public string AgeFirstTalked { get; set; }
        public string AgeToiletTrained { get; set; }

        public string DescriptionOfChild { get; set; }

        public bool ProblemWithBedWetting { get; set; }
        public bool AndOrSoiling { get; set; }

        public string Immunizations { get; set; }


        public bool Documents { get; set; }
    }
}
