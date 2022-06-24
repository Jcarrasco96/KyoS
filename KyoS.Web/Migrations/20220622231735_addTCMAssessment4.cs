using Microsoft.EntityFrameworkCore.Migrations;

namespace KyoS.Web.Migrations
{
    public partial class addTCMAssessment4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AHomeVisit",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AHomeVisitOn",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AcademicEelementary",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AcademicHigh",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AcademicMiddle",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AcademicPreSchool",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalInformation",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalInformationMigration",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Appliances",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AttendanceEelementary",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AttendanceHigh",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AttendanceMiddle",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AttendancePreSchool",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BathingAssistive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BathingIndependent",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BathingPhysical",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BathingSupervision",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BathingTotal",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Bathtub",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BehaviorEelementary",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BehaviorHigh",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BehaviorMiddle",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "BehaviorPreSchool",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Briefly",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CaseManagerWas",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CaseManagerWasDueTo",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Citizen",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ColonCancer",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "CongredatedHowOften",
                table: "TCMAssessment",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "CongredatedProvider",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CongredatedReceive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ContinueToLive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ContinueToLiveOnly",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CookingAssistive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CookingIndependent",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CookingPhysical",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CookingSupervision",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CookingTotal",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "CountryOfBirth",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentEmployer",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DentalExam",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescribeAnySchool",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescribeClientCultural",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescribeClientEducation",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescribeClientLiving",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescribeClientRelationship",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescribeNeighborhood",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescribeOtherNeedConcerns",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoesClientBasicNeed",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DoesClientCurrently",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DoesClientCurrentlyExplain",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DoesClientFeel",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DoesClientFeelExplain",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DoesClientNeedAssistance",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoesClientNeedAssistanceEducational",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DoesClientNeedAssistanceEducationalExplain",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoesClientNeedAssistanceExplain",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DoesNotKnow",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoingAssistive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoingIndependent",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoingPhysical",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoingSupervision",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DoingTotal",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DressingAssistive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DressingIndependent",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DressingPhysical",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DressingSupervision",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DressingTotal",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Drives",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Electrical",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EmployerAddress",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployerCityState",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployerContactPerson",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployerPhone",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmploymentStatus",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ExcessiveCluter",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FailToEelementary",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FailToHigh",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FailToMiddle",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FailToPreSchool",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FeedingAssistive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FeedingIndependent",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FeedingPhysical",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FeedingSupervision",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FeedingTotal",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FireHazards",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Flooring",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "FoodPantryHowOften",
                table: "TCMAssessment",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "FoodPantryProvider",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FoodPantryReceive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "FoodStampHowOften",
                table: "TCMAssessment",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "FoodStampProvider",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FoodStampReceive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FriendOrFamily",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GroomingAssistive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GroomingIndependent",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GroomingPhysical",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GroomingSupervision",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "GroomingTotal",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasClientEverArrest",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HasClientEverArrestLastTime",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HasClientEverArrestManyTime",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "HomeDeliveredHowOften",
                table: "TCMAssessment",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "HomeDeliveredProvider",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HomeDeliveredReceive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IfThereAnyHousing",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IfYesWereCriminal",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IfYesWhatArea",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ImmigrationOther",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ImmigrationOtherExplain",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Insect",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsClientCurrentlyEmployed",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsClientCurrentlySchool",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IsClientCurrentlySchoolExplain",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsClientInterested",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsClientInvolved",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IsClientInvolvedSpecify",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTheClientAbleWork",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTheClientAbleWorkLimitation",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTheClientHavingFinancial",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IsTheClientHavingFinancialExplain",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsThereAnyAide",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "IsThereAnyAideName",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IsThereAnyAidePhone",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsThereAnyCurrentLegalProcess",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LabWorks",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LearningEelementary",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LearningHigh",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LearningMiddle",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "LearningPreSchool",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ListAnyNeed",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ListClientCurrentPotencialStrngths",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ListClientCurrentPotencialWeakness",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MakingAssistive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MakingIndependent",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MakingPhysical",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MakingSupervision",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MakingTotal",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Mammogram",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MayWeLeaveSend",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MonthlyFamilyIncome",
                table: "TCMAssessment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "NoAirCondition",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NoTelephone",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NotHot",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfBedrooms",
                table: "TCMAssessment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumberOfPersonLiving",
                table: "TCMAssessment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OtherFinancial",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "OtherHowOften",
                table: "TCMAssessment",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "OtherProvider",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OtherReceive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PapAndHPV",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ParticipationEelementary",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ParticipationHigh",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ParticipationMiddle",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ParticipationPreSchool",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<float>(
                name: "PersonPorBedrooms",
                table: "TCMAssessment",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "PhysicalExam",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhysicalOther",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Poor",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PreferToLive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ProbationOfficer",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProbationOfficerName",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProbationOfficerPhone",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RecommendedActivities",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecommendedBasicNeed",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecommendedEconomic",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecommendedHousing",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecommendedLegalImmigration",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecommendedMentalHealth",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecommendedOther",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecommendedOtherSpecify",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecommendedPhysicalHealth",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecommendedRecreational",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecommendedSchool",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecommendedTransportation",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RecommendedVocation",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RelationshipEelementary",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RelationshipHigh",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RelationshipMiddle",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "RelationshipPreSchool",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Resident",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ResidentStatus",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolAddress",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolCityState",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolDistrict",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolGrade",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolName",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SchoolProgramEBD",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SchoolProgramESE",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SchoolProgramESOL",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SchoolProgramHHIP",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SchoolProgramOther",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SchoolProgramRegular",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SchoolProgramTeacherName",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolProgramTeacherPhone",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShoppingAssistive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShoppingIndependent",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShoppingPhysical",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShoppingSupervision",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ShoppingTotal",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Staff",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Stairs",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Structural",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TakesABus",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TransferringAssistive",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TransferringIndependent",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TransferringPhysical",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TransferringSupervision",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TransferringTotal",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "TransportationOther",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TransportationOtherExplain",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Tripping",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Unsanitary",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "VocationalEmployment",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Walks",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WhatActivityThings",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WhatIsCollegeGraduated",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatIsElementary",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatIsGED",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatIsGraduated",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatIsGraduatedDegree",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatIsHighSchool",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatIsMiddle",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatIsNoSchool",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatIsSomeCollege",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatIsSomeHigh",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WhatIsTheMainSource",
                table: "TCMAssessment",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WhatIsTradeSchool",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatIsUnknown",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WouldLikeObtainJob",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WouldLikeObtainJobNotAtThisTime",
                table: "TCMAssessment",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "YearEnteredUsa",
                table: "TCMAssessment",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AHomeVisit",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "AHomeVisitOn",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "AcademicEelementary",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "AcademicHigh",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "AcademicMiddle",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "AcademicPreSchool",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "AdditionalInformation",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "AdditionalInformationMigration",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Appliances",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "AttendanceEelementary",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "AttendanceHigh",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "AttendanceMiddle",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "AttendancePreSchool",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "BathingAssistive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "BathingIndependent",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "BathingPhysical",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "BathingSupervision",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "BathingTotal",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Bathtub",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "BehaviorEelementary",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "BehaviorHigh",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "BehaviorMiddle",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "BehaviorPreSchool",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Briefly",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "CaseManagerWas",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "CaseManagerWasDueTo",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Citizen",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ColonCancer",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "CongredatedHowOften",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "CongredatedProvider",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "CongredatedReceive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ContinueToLive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ContinueToLiveOnly",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "CookingAssistive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "CookingIndependent",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "CookingPhysical",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "CookingSupervision",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "CookingTotal",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "CountryOfBirth",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "CurrentEmployer",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DentalExam",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DescribeAnySchool",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DescribeClientCultural",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DescribeClientEducation",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DescribeClientLiving",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DescribeClientRelationship",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DescribeNeighborhood",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DescribeOtherNeedConcerns",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesClientBasicNeed",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesClientCurrently",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesClientCurrentlyExplain",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesClientFeel",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesClientFeelExplain",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesClientNeedAssistance",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesClientNeedAssistanceEducational",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesClientNeedAssistanceEducationalExplain",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesClientNeedAssistanceExplain",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoesNotKnow",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoingAssistive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoingIndependent",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoingPhysical",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoingSupervision",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DoingTotal",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DressingAssistive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DressingIndependent",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DressingPhysical",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DressingSupervision",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "DressingTotal",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Drives",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Electrical",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "EmployerAddress",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "EmployerCityState",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "EmployerContactPerson",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "EmployerPhone",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "EmploymentStatus",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ExcessiveCluter",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FailToEelementary",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FailToHigh",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FailToMiddle",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FailToPreSchool",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FeedingAssistive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FeedingIndependent",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FeedingPhysical",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FeedingSupervision",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FeedingTotal",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FireHazards",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Flooring",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FoodPantryHowOften",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FoodPantryProvider",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FoodPantryReceive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FoodStampHowOften",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FoodStampProvider",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FoodStampReceive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "FriendOrFamily",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "GroomingAssistive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "GroomingIndependent",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "GroomingPhysical",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "GroomingSupervision",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "GroomingTotal",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HasClientEverArrest",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HasClientEverArrestLastTime",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HasClientEverArrestManyTime",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HomeDeliveredHowOften",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HomeDeliveredProvider",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "HomeDeliveredReceive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IfThereAnyHousing",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IfYesWereCriminal",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IfYesWhatArea",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ImmigrationOther",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ImmigrationOtherExplain",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Insect",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsClientCurrentlyEmployed",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsClientCurrentlySchool",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsClientCurrentlySchoolExplain",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsClientInterested",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsClientInvolved",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsClientInvolvedSpecify",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsTheClientAbleWork",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsTheClientAbleWorkLimitation",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsTheClientHavingFinancial",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsTheClientHavingFinancialExplain",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsThereAnyAide",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsThereAnyAideName",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsThereAnyAidePhone",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "IsThereAnyCurrentLegalProcess",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "LabWorks",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "LearningEelementary",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "LearningHigh",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "LearningMiddle",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "LearningPreSchool",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ListAnyNeed",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ListClientCurrentPotencialStrngths",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ListClientCurrentPotencialWeakness",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "MakingAssistive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "MakingIndependent",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "MakingPhysical",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "MakingSupervision",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "MakingTotal",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Mammogram",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "MayWeLeaveSend",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "MonthlyFamilyIncome",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "NoAirCondition",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "NoTelephone",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "NotHot",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "NumberOfBedrooms",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "NumberOfPersonLiving",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "OtherFinancial",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "OtherHowOften",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "OtherProvider",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "OtherReceive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "PapAndHPV",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ParticipationEelementary",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ParticipationHigh",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ParticipationMiddle",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ParticipationPreSchool",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "PersonPorBedrooms",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "PhysicalExam",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "PhysicalOther",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Poor",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "PreferToLive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ProbationOfficer",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ProbationOfficerName",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ProbationOfficerPhone",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RecommendedActivities",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RecommendedBasicNeed",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RecommendedEconomic",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RecommendedHousing",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RecommendedLegalImmigration",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RecommendedMentalHealth",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RecommendedOther",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RecommendedOtherSpecify",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RecommendedPhysicalHealth",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RecommendedRecreational",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RecommendedSchool",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RecommendedTransportation",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RecommendedVocation",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RelationshipEelementary",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RelationshipHigh",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RelationshipMiddle",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "RelationshipPreSchool",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Resident",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ResidentStatus",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "SchoolAddress",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "SchoolCityState",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "SchoolDistrict",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "SchoolGrade",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "SchoolName",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "SchoolProgramEBD",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "SchoolProgramESE",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "SchoolProgramESOL",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "SchoolProgramHHIP",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "SchoolProgramOther",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "SchoolProgramRegular",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "SchoolProgramTeacherName",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "SchoolProgramTeacherPhone",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ShoppingAssistive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ShoppingIndependent",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ShoppingPhysical",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ShoppingSupervision",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "ShoppingTotal",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Staff",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Stairs",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Structural",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "TakesABus",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "TransferringAssistive",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "TransferringIndependent",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "TransferringPhysical",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "TransferringSupervision",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "TransferringTotal",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "TransportationOther",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "TransportationOtherExplain",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Tripping",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Unsanitary",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "VocationalEmployment",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "Walks",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WhatActivityThings",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WhatIsCollegeGraduated",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WhatIsElementary",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WhatIsGED",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WhatIsGraduated",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WhatIsGraduatedDegree",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WhatIsHighSchool",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WhatIsMiddle",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WhatIsNoSchool",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WhatIsSomeCollege",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WhatIsSomeHigh",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WhatIsTheMainSource",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WhatIsTradeSchool",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WhatIsUnknown",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WouldLikeObtainJob",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "WouldLikeObtainJobNotAtThisTime",
                table: "TCMAssessment");

            migrationBuilder.DropColumn(
                name: "YearEnteredUsa",
                table: "TCMAssessment");
        }
    }
}
