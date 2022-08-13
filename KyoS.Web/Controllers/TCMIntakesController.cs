using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;


namespace KyoS.Web.Controllers
{
    public class TCMIntakesController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IReportHelper _reportHelper;
        private readonly DataContext _context;
        private readonly IImageHelper _imageHelper;
        private readonly IMimeType _mimeType;

        public TCMIntakesController(IUserHelper userHelper, IConverterHelper converterHelper, ICombosHelper combosHelper, IRenderHelper renderHelper, DataContext context, IReportHelper reportHelper, IImageHelper imageHelper, IMimeType mimeType)
        {
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _context = context;
            _renderHelper = renderHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
            _imageHelper = imageHelper;
            _mimeType = mimeType;
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> Index(int idError = 0, StatusType status = StatusType.Open)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            else
            {
                if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
                {
                    List<TCMClientEntity> tcmClient = await _context.TCMClient
                                                                    .Include(n => n.TCMIntakeForm)
                                                                    .Include(n => n.TcmIntakeConsentForTreatment)
                                                                    .Include(n => n.TcmIntakeConsentForRelease)
                                                                    .Include(n => n.TcmIntakeConsumerRights)
                                                                    .Include(n => n.TcmIntakeAcknowledgementHipa)
                                                                    .Include(n => n.TCMIntakeOrientationChecklist)
                                                                    .Include(n => n.TCMIntakeAdvancedDirective)
                                                                    .Include(n => n.TCMIntakeForeignLanguage)
                                                                    .Include(n => n.TCMIntakeWelcome)
                                                                    .Include(n => n.Client)
                                                                    .ThenInclude(n => n.Clinic)
                                                                    .Include(n => n.Client)
                                                                    .ThenInclude(n => n.Documents)
                                                                    .Include(n => n.Client.IntakeFeeAgreement)
                                                                    .Include(n => n.Client.IntakeMedicalHistory)
                                                                    .Include(n => n.Client.MedicationList)
                                                                    .Include(n => n.Client.Psychiatrist)
                                                                    .Include(n => n.Client.Doctor)
                                                                    .Include(n => n.TCMIntakeNonClinicalLog)
                                                                    .Include(n => n.TCMIntakeMiniMental)
                                                                    .Include(n => n.TCMIntakeCoordinationCare)
                                                                    .Include(n => n.TcmServicePlan)
                                                                    .ThenInclude(n => n.TCMAdendum)
                                                                    .Include(n => n.TcmServicePlan)
                                                                    .ThenInclude(n => n.TCMServicePlanReview)
                                                                    .Include(n => n.TcmServicePlan)
                                                                    .ThenInclude(n => n.TCMDischarge)
                                                                    .Include(n => n.TcmIntakeAppendixJ)
                                                                    .Include(n => n.TcmInterventionLog)
                                                                    .Include(n => n.TCMFarsFormList)
                                                                    .Include(n => n.TCMAssessment)
                                                                    .ThenInclude(n => n.HouseCompositionList)
                                                                    .ThenInclude(n => n.TcmAssessment.MedicationList)
                                                                    .ThenInclude(n => n.TcmAssessment.IndividualAgencyList)
                                                                    .ThenInclude(n => n.TcmAssessment.PastCurrentServiceList)
                                                                    .Include(n => n.TCMNote)
                                                                    .Include(n => n.Casemanager)
                                                                    .Where(n => (n.Client.Clinic.Id == user_logged.Clinic.Id
                                                                       && n.Status == status))
                                                                    .ToListAsync();

                    if (status == StatusType.Open)
                    {
                        ViewData["open"] = "0";
                    }
                    else
                    {
                        ViewData["open"] = "1";
                    }

                    return View(tcmClient);
                }
                if (User.IsInRole("CaseManager"))
                {
                    List<TCMClientEntity> tcmClient = await _context.TCMClient
                                                                    .Include(n => n.TCMIntakeForm)
                                                                    .Include(n => n.TcmIntakeConsentForTreatment)
                                                                    .Include(n => n.TcmIntakeConsentForRelease)
                                                                    .Include(n => n.TcmIntakeConsumerRights)
                                                                    .Include(n => n.TcmIntakeAcknowledgementHipa)
                                                                    .Include(n => n.TCMIntakeOrientationChecklist)
                                                                    .Include(n => n.TCMIntakeAdvancedDirective)
                                                                    .Include(n => n.TCMIntakeForeignLanguage)
                                                                    .Include(n => n.TCMIntakeWelcome)
                                                                    .Include(n => n.Client)
                                                                    .ThenInclude(n => n.Clinic)
                                                                    .Include(n => n.Client)
                                                                    .ThenInclude(n => n.Documents)
                                                                    .Include(n => n.Client.IntakeFeeAgreement)
                                                                    .Include(n => n.Client.IntakeMedicalHistory)
                                                                    .Include(n => n.Client.MedicationList)
                                                                    .Include(n => n.Client.Psychiatrist)
                                                                    .Include(n => n.Client.Doctor)
                                                                    .Include(n => n.TCMIntakeNonClinicalLog)
                                                                    .Include(n => n.TCMIntakeMiniMental)
                                                                    .Include(n => n.TCMIntakeCoordinationCare)
                                                                    .Include(n => n.TcmServicePlan)
                                                                    .ThenInclude(n => n.TCMAdendum)
                                                                    .Include(n => n.TcmServicePlan)
                                                                    .ThenInclude(n => n.TCMServicePlanReview)
                                                                    .Include(n => n.TcmServicePlan)
                                                                    .ThenInclude(n => n.TCMDischarge)
                                                                    .Include(n => n.TcmIntakeAppendixJ)
                                                                    .Include(n => n.TcmInterventionLog)
                                                                    .Include(n => n.TCMFarsFormList)
                                                                    .Include(n => n.TCMAssessment)
                                                                    .ThenInclude(n => n.HouseCompositionList)
                                                                    .ThenInclude(n => n.TcmAssessment.MedicationList)
                                                                    .ThenInclude(n => n.TcmAssessment.IndividualAgencyList)
                                                                    .ThenInclude(n => n.TcmAssessment.PastCurrentServiceList)
                                                                    .Include(n => n.TCMNote)
                                                                    .Include(n => n.Casemanager)
                                                                    .Where(n => (n.Client.Clinic.Id == user_logged.Clinic.Id
                                                                     && n.Casemanager.LinkedUser == user_logged.UserName
                                                                     && n.Status == status))
                                                                    .ToListAsync();

                    if (status == StatusType.Open)
                    {
                        ViewData["open"] = "0";
                    }
                    else
                    {
                        ViewData["open"] = "1";
                    }

                    return View(tcmClient);
                }

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Create(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeFormViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {

                    model = new TCMIntakeFormViewModel
                    {
                        IdTCMClient = id,
                        TcmClient_FK = id,
                        TcmClient = _context.TCMClient
                                            .Include(m => m.Client)
                                            .ThenInclude(m => m.Clients_Diagnostics)
                                            .ThenInclude(m => m.Diagnostic)
                                            .Include(m => m.Client)
                                            .ThenInclude(m => m.Clients_HealthInsurances)
                                            .ThenInclude(m => m.HealthInsurance)
                                            .Include(m => m.Client.LegalGuardian)
                                            .Include(n => n.Client.EmergencyContact)
                                            .Include(n => n.Client.Referred)
                                            .Include(n => n.Client.Doctor)
                                            .Include(n => n.Client.Psychiatrist)
                                            .Include(n => n.TCMIntakeCoordinationCare)
                                            .FirstOrDefault(n => n.Id == id),
                        Agency = "",
                        CaseManagerNotes = "",
                        Elibigility = "",
                        EmploymentStatus = "",
                        Grade = "",
                        IntakeDate = DateTime.Now,
                        IsClientCurrently = false,
                        LTC = false,
                        MMA = false,
                        MonthlyFamilyIncome = "",
                        NeedSpecial = false,
                        NeedSpecial_Specify = "",
                        Other = "",
                        Other_Address = "",
                        Other_City = "",
                        Other_Phone = "",
                        PrimarySourceIncome = "",
                        ResidentialStatus = "",
                        School = "",
                        School_EBD = false,
                        School_ESE = false,
                        School_ESOL = false,
                        School_HHIP = false,
                        School_Other = false,
                        School_Regular = false,
                        SecondaryContact = "",
                        SecondaryContact_Phone = "",
                        SecondaryContact_RelationShip = "",
                        TeacherCounselor_Name = "",
                        TeacherCounselor_Phone = "",
                        TitlePosition = "",
                        EducationLevel = "",
                        InsuranceOther = "",
                        ReligionOrEspiritual = "",
                        CountryOfBirth = "",
                        EmergencyContact = false,
                        StatusOther = false,
                        StatusOther_Explain = "",
                        StatusResident = false,
                        StausCitizen = false,
                        YearEnterUsa = "",
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now

                    };
                    if (model.TcmClient.Client.LegalGuardian == null)
                        model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                    if (model.TcmClient.Client.EmergencyContact == null)
                    {
                        model.TcmClient.Client.EmergencyContact = new EmergencyContactEntity();
                        
                    }
                    return View(model);
                }
            }

            model = new TCMIntakeFormViewModel
            {
                IdTCMClient = id,
                TcmClient_FK = id,
                TcmClient = _context.TCMClient
                                            .Include(m => m.Client)
                                            .ThenInclude(m => m.Clients_Diagnostics)
                                            .ThenInclude(m => m.Diagnostic)
                                            .Include(m => m.Client)
                                            .ThenInclude(m => m.Clients_HealthInsurances)
                                            .ThenInclude(m => m.HealthInsurance)
                                            .Include(m => m.Client.LegalGuardian)
                                            .Include(n => n.Client.EmergencyContact)
                                            .Include(n => n.Client.Referred)
                                            .Include(n => n.Client.Doctor)
                                            .Include(n => n.Client.Psychiatrist)
                                            .FirstOrDefault(n => n.Id == id),
                Agency = "",
                CaseManagerNotes = "",
                Elibigility = "",
                EmploymentStatus = "",
                CreatedBy = user_logged.UserName,
                CreatedOn = DateTime.Now,
                Grade = "",
                IntakeDate = DateTime.Now,
                IsClientCurrently = false,
                LTC = false,
                MMA = false,
                MonthlyFamilyIncome = "",
                NeedSpecial = false,
                NeedSpecial_Specify = "",
                Other = "",
                Other_Address = "",
                Other_City = "",
                Other_Phone = "",
                PrimarySourceIncome = "",
                ResidentialStatus = "",
                School = "",
                School_EBD = false,
                School_ESE = false,
                School_ESOL = false,
                School_HHIP = false,
                School_Other = false,
                School_Regular = false,
                SecondaryContact = "",
                SecondaryContact_Phone = "",
                SecondaryContact_RelationShip = "",
                TeacherCounselor_Name = "",
                TeacherCounselor_Phone = "",
                TitlePosition = "",
                EducationLevel = "",
                InsuranceOther = "",
                ReligionOrEspiritual = "",
                CountryOfBirth = "",
                EmergencyContact = false,
                StatusOther = false,
                StatusOther_Explain = "",
                StatusResident = false,
                StausCitizen = false,
                YearEnterUsa = ""

            };

            if (model.TcmClient.Client.LegalGuardian == null)
                model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
            if (model.TcmClient.Client.EmergencyContact == null)
            {
                model.TcmClient.Client.EmergencyContact = new EmergencyContactEntity();
                
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Create(TCMIntakeFormViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeFormEntity IntakeEntity = _context.TCMIntakeForms.Find(IntakeViewModel.Id);
                if (IntakeEntity == null)
                {
                    IntakeEntity = await _converterHelper.ToTCMIntakeFormEntity(IntakeViewModel, true,user_logged.UserName);
                    _context.TCMIntakeForms.Add(IntakeEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient , section = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the TCM service.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", IntakeViewModel) });
                }
            }
            TCMIntakeFormViewModel model;
            model = new TCMIntakeFormViewModel
            {
                IdTCMClient = IntakeViewModel.IdTCMClient,
                TcmClient_FK = IntakeViewModel.TcmClient_FK,
                TcmClient = _context.TCMClient
                                            .Include(m => m.Client)
                                            .ThenInclude(m => m.Clients_Diagnostics)
                                            .ThenInclude(m => m.Diagnostic)
                                            .Include(m => m.Client)
                                            .ThenInclude(m => m.Clients_HealthInsurances)
                                            .ThenInclude(m => m.HealthInsurance)
                                            .Include(m => m.Client.LegalGuardian)
                                            .Include(n => n.Client.EmergencyContact)
                                            .Include(n => n.Client.Referred)
                                            .Include(n => n.Client.Doctor)
                                            .Include(n => n.Client.Psychiatrist)
                                            .FirstOrDefault(n => n.Id == IntakeViewModel.Id),
                                             
                Agency = "",
                CaseManagerNotes = "",
                Elibigility = "",
                EmploymentStatus = "",
                Grade = "",
                IntakeDate = DateTime.Now,
                IsClientCurrently = false,
                LTC = false,
                MMA = false,
                MonthlyFamilyIncome = "",
                NeedSpecial = false,
                NeedSpecial_Specify = "",
                Other = "",
                Other_Address = "",
                Other_City = "",
                Other_Phone = "",
                PrimarySourceIncome = "",
                ResidentialStatus = "",
                School = "",
                School_EBD = false,
                School_ESE = false,
                School_ESOL = false,
                School_HHIP = false,
                School_Other = false,
                School_Regular = false,
                SecondaryContact = "",
                SecondaryContact_Phone = "",
                SecondaryContact_RelationShip = "",
                TeacherCounselor_Name = "",
                TeacherCounselor_Phone = "",
                TitlePosition = "",
                EducationLevel = "",
                InsuranceOther = "",
                ReligionOrEspiritual = "",
                CountryOfBirth = "",
                EmergencyContact = false,
                StatusOther = false,
                StatusOther_Explain = "",
                StatusResident = false,
                StausCitizen = false,
                YearEnterUsa = ""

            };
            if (model.TcmClient.Client.LegalGuardian == null)
                 model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
            if (model.TcmClient.Client.EmergencyContact == null)
                 model.TcmClient.Client.EmergencyContact = new EmergencyContactEntity();
                
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> TCMIntakeDashboard(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMClientEntity TcmClientEntity = await _context.TCMClient
                                                            .Include(c => c.TCMIntakeForm)
                                                            .Include(c => c.Client)
                                                            .Include(c => c.TcmIntakeConsentForTreatment)
                                                            .Include(n => n.TcmIntakeConsentForRelease.Where(m => m.TcmClient_FK == id))
                                                            .Include(n => n.TcmIntakeConsumerRights)
                                                            .Include(n => n.TcmIntakeAcknowledgementHipa)
                                                            .Include(n => n.TCMIntakeOrientationChecklist)
                                                            .Include(n => n.TCMIntakeAdvancedDirective)
                                                            .Include(n => n.TCMIntakeForeignLanguage)
                                                            .Include(n => n.TCMIntakeWelcome)
                                                            .Include(n => n.Client)
                                                            .ThenInclude(n => n.Documents)
                                                            .Include(n => n.Client.Psychiatrist)
                                                            .Include(n => n.Client.Doctor)
                                                            .Include(n => n.Client.IntakeFeeAgreement)
                                                            .Include(n => n.Client.IntakeMedicalHistory)
                                                            .Include(n => n.Client.MedicationList)
                                                            .Include(n => n.TCMIntakeNonClinicalLog)
                                                            .Include(n => n.TCMIntakeMiniMental)
                                                            .Include(n => n.TCMIntakeCoordinationCare)
                                                            .Include(n => n.TcmServicePlan)
                                                            .ThenInclude(n => n.TCMAdendum)
                                                            .Include(n => n.TcmServicePlan)
                                                            .ThenInclude(n => n.TCMServicePlanReview)
                                                            .Include(n => n.TcmServicePlan)
                                                            .ThenInclude(n => n.TCMDischarge)
                                                            .Include(n => n.TcmIntakeAppendixJ)
                                                            .Include(n => n.TcmInterventionLog)
                                                            .Include(n => n.TCMFarsFormList)
                                                            .Include(n => n.TCMAssessment)
                                                            .ThenInclude(n => n.HouseCompositionList)
                                                            .ThenInclude(n => n.TcmAssessment.MedicationList)
                                                            .ThenInclude(n => n.TcmAssessment.IndividualAgencyList)
                                                            .ThenInclude(n => n.TcmAssessment.PastCurrentServiceList)
                                                            .Include(n => n.TCMNote)
                                                            .FirstOrDefaultAsync(c => c.Id == id);

            List<TCMIntakeConsentForReleaseEntity> listRelease = await _context.TCMIntakeConsentForRelease
                                                                               .Where(m => m.TcmClient_FK == id).ToListAsync();
            List<DocumentEntity> listDocument = await _context.Documents
                                                              .Where(m => m.Client.Id == TcmClientEntity.Client.Id).ToListAsync();

            TcmClientEntity.TcmIntakeConsentForRelease = listRelease;
            TcmClientEntity.Client.Documents = listDocument;
            

            if (TcmClientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            return View(TcmClientEntity);
        }

        [Authorize(Roles = "TCMSupervisor, Manager")]
        public async Task<IActionResult> TCMIntakeDashboardReadOnly(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMClientEntity TcmClientEntity = await _context.TCMClient
                                                            .Include(c => c.TCMIntakeForm)
                                                            .Include(c => c.Client)
                                                            .Include(c => c.TcmIntakeConsentForTreatment)
                                                            .Include(n => n.TcmIntakeConsentForRelease.Where(m => m.TcmClient_FK == id))
                                                            .Include(n => n.TcmIntakeConsumerRights)
                                                            .Include(n => n.TcmIntakeAcknowledgementHipa)
                                                            .Include(n => n.TCMIntakeOrientationChecklist)
                                                            .Include(n => n.TCMIntakeAdvancedDirective)
                                                            .Include(n => n.TCMIntakeForeignLanguage)
                                                            .Include(n => n.TCMIntakeWelcome)
                                                            .Include(n => n.Client)
                                                            .ThenInclude(n => n.Documents)
                                                            .Include(n => n.Client.Psychiatrist)
                                                            .Include(n => n.Client.Doctor)
                                                            .Include(n => n.Client.IntakeFeeAgreement)
                                                            .Include(n => n.Client.IntakeMedicalHistory)
                                                            .Include(n => n.Client.MedicationList)
                                                            .Include(n => n.TCMIntakeNonClinicalLog)
                                                            .Include(n => n.TCMIntakeMiniMental)
                                                            .Include(n => n.TCMIntakeCoordinationCare)
                                                            .Include(n => n.TcmServicePlan)
                                                            .ThenInclude(n => n.TCMAdendum)
                                                            .Include(n => n.TcmServicePlan)
                                                            .ThenInclude(n => n.TCMServicePlanReview)
                                                            .Include(n => n.TcmServicePlan)
                                                            .ThenInclude(n => n.TCMDischarge)
                                                            .Include(n => n.TcmIntakeAppendixJ)
                                                            .Include(n => n.TcmInterventionLog)
                                                            .Include(n => n.TCMFarsFormList)
                                                            .Include(n => n.TCMAssessment)
                                                            .ThenInclude(n => n.HouseCompositionList)
                                                            .ThenInclude(n => n.TcmAssessment.MedicationList)
                                                            .ThenInclude(n => n.TcmAssessment.IndividualAgencyList)
                                                            .ThenInclude(n => n.TcmAssessment.PastCurrentServiceList)
                                                            .Include(n => n.TCMNote)
                                                            .FirstOrDefaultAsync(c => c.Id == id);

            List<TCMIntakeConsentForReleaseEntity> listRelease = await _context.TCMIntakeConsentForRelease
                                                                               .Where(m => m.TcmClient_FK == id).ToListAsync();
            List<DocumentEntity> listDocument = await _context.Documents
                                                              .Where(m => m.Client.Id == TcmClientEntity.Client.Id).ToListAsync();

            TcmClientEntity.TcmIntakeConsentForRelease = listRelease;
            TcmClientEntity.Client.Documents = listDocument;


            if (TcmClientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            return View(TcmClientEntity);
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Edit(int id = 0)
        {
            TCMIntakeFormEntity entity = _context.TCMIntakeForms
                                                 .Include(m => m.TcmClient)
                                                 .ThenInclude(m => m.Client)
                                                 .ThenInclude(m => m.Clients_Diagnostics)
                                                 .ThenInclude(m => m.Diagnostic)
                                                 .Include(m => m.TcmClient)
                                                 .ThenInclude(m => m.Client)
                                                 .ThenInclude(m => m.Clients_HealthInsurances)
                                                 .ThenInclude(m => m.HealthInsurance)
                                                 .Include(m => m.TcmClient.Client.LegalGuardian)
                                                 .Include(n => n.TcmClient.Client.EmergencyContact)
                                                 .Include(n => n.TcmClient.Client.Referred)
                                                 .Include(n => n.TcmClient.Client.Doctor)
                                                 .Include(n => n.TcmClient.Client.Psychiatrist)
                                                 .FirstOrDefault(i => i.TcmClient.Id == id);
            if (entity == null)
            {
                return RedirectToAction("Create", new { id = id });
            }

            TCMIntakeFormEntity model;

            if (User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {
                    model = _converterHelper.ToTCMIntakeFormViewModel(entity);
                    if (model.TcmClient.Client.LegalGuardian == null)
                        model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                    if (model.TcmClient.Client.EmergencyContact == null)
                    {
                        model.TcmClient.Client.EmergencyContact = new EmergencyContactEntity();
                        
                    }

                    return View(model);
                }
            }

            model = new TCMIntakeFormEntity();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Edit(TCMIntakeFormViewModel intakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeFormEntity intakeEntity = await _converterHelper.ToTCMIntakeFormEntity(intakeViewModel, false, user_logged.UserName);
                _context.TCMIntakeForms.Update(intakeEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("TCMIntakeSectionDashboard", new { id = intakeViewModel.IdTCMClient, section = 1});
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }
            
            else
            {
                intakeViewModel.TcmClient = await _context.TCMClient
                                                          .Include(m => m.Client)
                                                          .ThenInclude(m => m.Clients_Diagnostics)
                                                          .ThenInclude(m => m.Diagnostic)
                                                          .Include(m => m.Client)
                                                          .ThenInclude(m => m.Clients_HealthInsurances)
                                                          .ThenInclude(m => m.HealthInsurance)
                                                          .Include(m => m.Client.LegalGuardian)
                                                          .Include(n => n.Client.EmergencyContact)
                                                          .Include(n => n.Client.Referred)
                                                          .Include(n => n.Client.Doctor)
                                                          .Include(n => n.Client.Psychiatrist)
                                                          .FirstOrDefaultAsync(n => n.Id == intakeViewModel.IdTCMClient);
              

            }

            return View(intakeViewModel);
           
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMConsentForTreatment(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeConsentForTreatmentViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeConsentForTreatmentEntity intakeConsent = _context.TCMIntakeConsentForTreatment
                                                                               .Include(n => n.TcmClient)
                                                                               .ThenInclude(n => n.Client)
                                                                               .ThenInclude(n => n.LegalGuardian)
                                                                               .FirstOrDefault(n => n.TcmClient.Id == id);

                    if (intakeConsent == null)
                    {
                        model = new TCMIntakeConsentForTreatmentViewModel
                        {
                            TcmClient = _context.TCMClient
                                                .Include(d => d.Client)
                                                .ThenInclude(d => d.LegalGuardian)
                                                .FirstOrDefault(n => n.Id == id),
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            Aggre = true,
                            Aggre1 = true,
                            AuthorizeRelease = true,
                            AuthorizeStaff = true,
                            Certify = false,
                            Certify1 = true,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
                            Documents = true,
                            Id = 0,
                            Underestand = true,
                            IdTCMClient = id,
                            Client_FK = id,
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeConsent.TcmClient.Client.LegalGuardian == null)
                            intakeConsent.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeConsentForTreatmentViewModel(intakeConsent);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMConsentForTreatment(TCMIntakeConsentForTreatmentViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeConsentForTreatmentEntity IntakeConsentEntity = await _converterHelper.ToTCMIntakeConsentForTreatmentEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeConsentEntity.Id == 0)
                {
                    IntakeConsentEntity.TcmClient = null;
                    _context.TCMIntakeConsentForTreatment.Add(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeConsentEntity.TcmClient = null;
                    _context.TCMIntakeConsentForTreatment.Update(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMConsentForTreatment", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMConsentForRelease(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeConsentForReleaseViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    model = new TCMIntakeConsentForReleaseViewModel
                    {
                        TcmClient = _context.TCMClient
                                            .Include(n => n.Client)
                                            .ThenInclude(n => n.LegalGuardian)
                                            .FirstOrDefault(n => n.Id == id),
                        IdTCMClient = id,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        TcmClient_FK = id,
                        Id = 0,
                        ToRelease = true,
                        ForPurpose_CaseManagement = false,
                        ForPurpose_Other = false,
                        ForPurpose_OtherExplain = "",
                        ForPurpose_Treatment = false,
                        InForm_Facsimile = false,
                        InForm_VerbalInformation = false,
                        InForm_WrittenRecords = false,
                        Discharge = false,
                        SchoolRecord = false,
                        ProgressReports = false,
                        IncidentReport = false,
                        PsychologycalEvaluation = false,
                        History = false,
                        LabWork = false,
                        HospitalRecord = false,
                        Other = false,
                        Other_Explain = "",
                        Documents = true,
                        DateSignatureEmployee = DateTime.Now,
                        DateSignatureLegalGuardian = DateTime.Now,
                        DateSignaturePerson = DateTime.Now,
                        AdmissionedFor = user_logged.FullName,
                        NameOfFacility = "",
                        Address = "",
                        CityStateZip = "",
                        PhoneNo = "",
                        FaxNo = ""
                    };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                
                }
            }

            return RedirectToAction("TCMIntakeDashboard", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMConsentForRelease(TCMIntakeConsentForReleaseViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeConsentForReleaseEntity IntakeConsentEntity = await _converterHelper.ToTCMIntakeConsentForReleaseEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeConsentEntity.Id == 0)
                {
                   
                    _context.TCMIntakeConsentForRelease.Add(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                   
                    _context.TCMIntakeConsentForRelease.Update(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("ListConsentForrelease", new { id = IntakeViewModel.IdTCMClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMConsentForRelease", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMConsumerRights(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeConsumerRightsViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeConsumerRightsEntity intakeConsent = _context.TCMIntakeConsumerRights
                                                                          .Include(n => n.TcmClient)
                                                                          .ThenInclude(n => n.Client)
                                                                          .ThenInclude(n => n.LegalGuardian)
                                                                          .FirstOrDefault(n => n.TcmClient.Id == id);
                    if (intakeConsent == null)
                    {
                        model = new TCMIntakeConsumerRightsViewModel
                        {
                            TcmClient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.LegalGuardian)
                                                .FirstOrDefault(n => n.Id == id),
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            IdTCMClient = id,
                            TcmClient_FK = id,
                            Id = 0,
                            ServedOf = user_logged.FullName,
                            Documents = true,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeConsent.TcmClient.Client.LegalGuardian == null)
                            intakeConsent.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeConsumerRightsViewModel(intakeConsent);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMConsumerRights(TCMIntakeConsumerRightsViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeConsumerRightsEntity IntakeConsumerEntity = await _converterHelper.ToTCMIntakeConsumerRightsEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeConsumerEntity.Id == 0)
                {
                    IntakeConsumerEntity.TcmClient = null;
                    _context.TCMIntakeConsumerRights.Add(IntakeConsumerEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeConsumerEntity.TcmClient = null;
                    _context.TCMIntakeConsumerRights.Update(IntakeConsumerEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMConsumerRights", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMAcknowledgementHippa(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeAcknoewledgementHippaViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeAcknowledgementHippaEntity intakeAck = _context.TCMIntakeAcknowledgement
                                                                            .Include(n => n.TcmClient)
                                                                            .ThenInclude(n => n.Client)
                                                                            .ThenInclude(n => n.LegalGuardian)
                                                                            .FirstOrDefault(n => n.TcmClient.Id == id);

                    if (intakeAck == null)
                    {

                        model = new TCMIntakeAcknoewledgementHippaViewModel
                        {
                            TcmClient = _context.TCMClient
                                                .Include(d => d.Client)
                                                .ThenInclude(d => d.LegalGuardian)
                                                .FirstOrDefault(n => n.Id == id),
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
                            Documents = true,
                            Id = 0,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            IdTCMClient = id,
                            TcmClient_FK = id,
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeAck.TcmClient.Client.LegalGuardian == null)
                            intakeAck.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeAcknoewledgementHippaViewModel(intakeAck);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMAcknowledgementHippa(TCMIntakeAcknoewledgementHippaViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeAcknowledgementHippaEntity IntakeAckNowEntity = await _converterHelper.ToTCMIntakeAcknoewledgementHippaEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeAckNowEntity.Id == 0)
                {
                    IntakeAckNowEntity.TcmClient = null;
                    _context.TCMIntakeAcknowledgement.Add(IntakeAckNowEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeAckNowEntity.TcmClient = null;
                    _context.TCMIntakeAcknowledgement.Update(IntakeAckNowEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMAcknowledgementHippa", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMOrientationCheckList(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeOrientationCheckListViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeOrientationChecklistEntity intakeCheckList = _context.TCMIntakeOrientationCheckList
                                                                                  .Include(n => n.TcmClient)
                                                                                  .ThenInclude(n => n.Client)
                                                                                  .ThenInclude(n => n.LegalGuardian)
                                                                                  .FirstOrDefault(n => n.TcmClient.Id == id);
                    if (intakeCheckList == null)
                    {
                        model = new TCMIntakeOrientationCheckListViewModel
                        {
                            TcmClient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.LegalGuardian)
                                                .FirstOrDefault(n => n.Id == id),
                            IdTcmClient = id,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            TcmClient_FK = id,
                            Id = 0,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
                            Access = true,
                            AgencyExpectation = true,
                            AgencyPolice = true,
                            Code = true,
                            Confidentiality = true,
                            Discharge = true,
                            Education = true,
                            Explanation = true,
                            Fire = true,
                            Identification = true,
                            IndividualPlan = true,
                            Insent = true,
                            Methods = true,
                            PoliceGrievancce = true,
                            PoliceIllicit = true,
                            PoliceTobacco = true,
                            PoliceWeapons = true,
                            Program = true,
                            Purpose = true,
                            Rights = true,
                            Services = true,
                            TheAbove = true,
                            TourFacility = true,
                            Documents = true,
                            AdmissionedFor = user_logged.FullName,
                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeCheckList.TcmClient.Client.LegalGuardian == null)
                            intakeCheckList.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeOrientationChecklistViewModel(intakeCheckList);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMOrientationCheckList(TCMIntakeOrientationCheckListViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeOrientationChecklistEntity IntakeOrientationEntity = await _converterHelper.ToTCMIntakeOrientationChecklistEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeOrientationEntity.Id == 0)
                {
                    IntakeOrientationEntity.TcmClient = null;
                    _context.TCMIntakeOrientationCheckList.Add(IntakeOrientationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTcmClient, section = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeOrientationEntity.TcmClient = null;
                    _context.TCMIntakeOrientationCheckList.Update(IntakeOrientationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTcmClient, section = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMOrientationCheckList", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMIntakeAdvenceDirective(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeAdvancedDirectiveViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeAdvancedDirectiveEntity intakeConsent = _context.TCMIntakeAdvancedDirective
                                                                             .Include(n => n.TcmClient)
                                                                             .ThenInclude(n => n.Client)
                                                                             .ThenInclude(n => n.LegalGuardian)
                                                                             .Include(n => n.TcmClient.Client.EmergencyContact)
                                                                             .FirstOrDefault(n => n.TcmClient.Id == id);
                    if (intakeConsent == null)
                    {
                        model = new TCMIntakeAdvancedDirectiveViewModel
                        {
                            TcmClient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.LegalGuardian)
                                                .FirstOrDefault(n => n.Id == id),
                            IdTCMClient = id,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            TcmClient_FK = id,
                            Id = 0,
                            Documents = true,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeConsent.TcmClient.Client.LegalGuardian == null)
                            intakeConsent.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeAdvancedDirectiveViewModel(intakeConsent);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMIntakeAdvenceDirective(TCMIntakeAdvancedDirectiveViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeAdvancedDirectiveEntity IntakeConsumerEntity = await _converterHelper.ToTCMIntakeAdvancedDirectiveEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeConsumerEntity.Id == 0)
                {
                    IntakeConsumerEntity.TcmClient = null;
                    _context.TCMIntakeAdvancedDirective.Add(IntakeConsumerEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeConsumerEntity.TcmClient = null;
                    _context.TCMIntakeAdvancedDirective.Update(IntakeConsumerEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMIntakeAdvenceDirective", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> ListConsentForrelease(int id = 0)
        {
           
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            else
            {
                List<TCMIntakeConsentForReleaseEntity> listRelease = await _context.TCMIntakeConsentForRelease
                                                                                   .Include(n => n.TcmClient)
                                                                                   .ThenInclude(n => n.Client)
                                                                                   .Where(m => m.TcmClient_FK == id)
                                                                                   .ToListAsync();
                if (listRelease.Count == 0)
                {
                    return RedirectToAction("TCMIntakeSectionDashboard", new { id = id, section = 1 });
                }
                
                return View(listRelease);
            }
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult EditConsentForRelease(int id = 0)
        {
            TCMIntakeConsentForReleaseEntity entity = _context.TCMIntakeConsentForRelease
                                                              .Include(m => m.TcmClient)
                                                              .ThenInclude(m => m.Client)
                                                              .ThenInclude(m => m.Clients_Diagnostics)
                                                              .ThenInclude(m => m.Diagnostic)
                                                              .Include(m => m.TcmClient)
                                                              .ThenInclude(m => m.Client)
                                                              .ThenInclude(m => m.Clients_HealthInsurances)
                                                              .ThenInclude(m => m.HealthInsurance)
                                                              .Include(m => m.TcmClient.Client.LegalGuardian)
                                                              .Include(n => n.TcmClient.Client.EmergencyContact)
                                                              .Include(n => n.TcmClient.Client.Referred)
                                                              .Include(n => n.TcmClient.Client.Doctor)
                                                              .Include(n => n.TcmClient.Client.Psychiatrist)
                                                              .FirstOrDefault(i => i.Id == id);
            if (entity == null)
            {
                return RedirectToAction("EditConsentForRelease", new { id = id });
            }

            TCMIntakeConsentForReleaseEntity model;

            if (User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {
                    model = _converterHelper.ToTCMIntakeConsentForReleaseViewModel(entity);
                    if (model.TcmClient.Client.LegalGuardian == null)
                        model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                    if (model.TcmClient.Client.EmergencyContact == null)
                    {
                        model.TcmClient.Client.EmergencyContact = new EmergencyContactEntity();

                    }

                    return View(model);
                }
            }

            model = new TCMIntakeConsentForReleaseEntity();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> EditConsentForRelease(TCMIntakeConsentForReleaseViewModel intakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeConsentForReleaseEntity intakeEntity = await _converterHelper.ToTCMIntakeConsentForReleaseEntity(intakeViewModel, false, user_logged.UserName);
                _context.TCMIntakeConsentForRelease.Update(intakeEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("ListConsentForrelease", new { id = intakeViewModel.IdTCMClient });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            else
            {
                intakeViewModel.TcmClient = await _context.TCMClient
                                                          .Include(m => m.Client)
                                                          .ThenInclude(m => m.Clients_Diagnostics)
                                                          .ThenInclude(m => m.Diagnostic)
                                                          .Include(m => m.Client)
                                                          .ThenInclude(m => m.Clients_HealthInsurances)
                                                          .ThenInclude(m => m.HealthInsurance)
                                                          .Include(m => m.Client.LegalGuardian)
                                                          .Include(n => n.Client.EmergencyContact)
                                                          .Include(n => n.Client.Referred)
                                                          .Include(n => n.Client.Doctor)
                                                          .Include(n => n.Client.Psychiatrist)
                                                          .FirstOrDefaultAsync(n => n.Id == intakeViewModel.IdTCMClient);


            }

            return View(intakeViewModel);

        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMForeignLanguage(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeForeignLanguageViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeForeignLanguageEntity intakeForeign = _context.TCMIntakeForeignLanguage
                                                                            .Include(n => n.TcmClient)
                                                                            .ThenInclude(n => n.Client)
                                                                            .ThenInclude(n => n.LegalGuardian)
                                                                            .FirstOrDefault(n => n.TcmClient.Id == id);

                    if (intakeForeign == null)
                    {

                        model = new TCMIntakeForeignLanguageViewModel
                        {
                            TcmClient = _context.TCMClient
                                                .Include(d => d.Client)
                                                .ThenInclude(d => d.LegalGuardian)
                                                .FirstOrDefault(n => n.Id == id),
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
                            Documents = true,
                            Id = 0,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            IdTCMClient = id,
                            TcmClient_FK = id,
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeForeign.TcmClient.Client.LegalGuardian == null)
                            intakeForeign.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeForeignLanguageViewModel(intakeForeign);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMForeignLanguage(TCMIntakeForeignLanguageViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeForeignLanguageEntity IntakeForeignEntity = await _converterHelper.ToTCMIntakeForeignLanguageEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeForeignEntity.Id == 0)
                {
                    IntakeForeignEntity.TcmClient = null;
                    _context.TCMIntakeForeignLanguage.Add(IntakeForeignEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeForeignEntity.TcmClient = null;
                    _context.TCMIntakeForeignLanguage.Update(IntakeForeignEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMForeignLanguage", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMWelcome(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeWelcomeViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeWelcomeEntity intakeForeign = _context.TCMIntakeWelcome
                                                                   .Include(n => n.TcmClient)
                                                                   .ThenInclude(n => n.Client)
                                                                   .ThenInclude(n => n.LegalGuardian)
                                                                   .Include(n => n.TcmClient)
                                                                   .ThenInclude(n => n.Casemanager)
                                                                   .ThenInclude(n => n.Clinic)
                                                                   .FirstOrDefault(n => n.TcmClient.Id == id);

                    if (intakeForeign == null)
                    {

                        model = new TCMIntakeWelcomeViewModel
                        {
                            TcmClient = _context.TCMClient
                                                .Include(d => d.Client)
                                                .ThenInclude(d => d.LegalGuardian)
                                                .Include(d => d.Casemanager)
                                                .ThenInclude(d => d.Clinic)
                                                .FirstOrDefault(n => n.Id == id),
                            Date = DateTime.Now,
                            Id = 0,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            IdTCMClient = id,
                            TcmClient_FK = id,
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeForeign.TcmClient.Client.LegalGuardian == null)
                            intakeForeign.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeWelcomeViewModel(intakeForeign);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMWelcome(TCMIntakeWelcomeViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeWelcomeEntity IntakeForeignEntity = await _converterHelper.ToTCMIntakeWelcomeEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeForeignEntity.Id == 0)
                {
                    IntakeForeignEntity.TcmClient = null;
                    _context.TCMIntakeWelcome.Add(IntakeForeignEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeForeignEntity.TcmClient = null;
                    _context.TCMIntakeWelcome.Update(IntakeForeignEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMWelcome", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> ClientsDocuments(int id = 0, int idTCMCLient = 0)
        {
           
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            else
            {
                ClientEntity Client = await _context.Clients
                                                       .Include(n => n.Clinic)
                                                       .Include(n => n.Documents)
                                                       .FirstOrDefaultAsync(n => n.Id == id);
                ClientViewModel model = await _converterHelper.ToClientViewModel(Client, user_logged.Id);
                model.IdTCMClient = idTCMCLient;
                return View(model);
            }
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult AddDocument(int id = 0)
        {
            DocumentViewModel entity = new DocumentViewModel()
            {
                IdDescription = 0,
                Descriptions = _combosHelper.GetComboDocumentDescriptions(),
                Client = _context.Clients.Find(id)
            };
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> AddDocument(int id, DocumentViewModel documentViewModel)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                string documentPath = string.Empty;
                if (documentViewModel.DocumentFile != null)
                {
                    documentPath = await _imageHelper.UploadFileAsync(documentViewModel.DocumentFile, "Clients");
                }

                DocumentEntity document = new DocumentEntity
                    {
                        Id = 0,
                        FileUrl = documentPath,
                        FileName = documentViewModel.DocumentFile.FileName,
                        Description = DocumentUtils.GetDocumentByIndex(documentViewModel.IdDescription),
                        CreatedOn = DateTime.Now,
                        CreatedBy = user_logged.Id,
                        Client = _context.Clients.Find(id)
                    };
                    _context.Add(document);
                    await _context.SaveChangesAsync();

                List<DocumentEntity> documentList = await _context.Documents
                                                                  .Where(c => c.Client.Id == id)
                                                                  .OrderByDescending(n => n.CreatedOn)
                                                                  .ToListAsync();

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDocument", documentList) });
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddDocument", documentViewModel) });
        }

         [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteDocumentTemp(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DocumentEntity document = await _context.Documents
                                                    .Include(n => n.Client)
                                                    .FirstOrDefaultAsync(d => d.Id == id);
            int tempId = document.Client.Id;

            if (document == null)
            {
                return RedirectToAction("Home/Error404");
            }

            _context.Documents.Remove(document);
            
            await _context.SaveChangesAsync();

            List<DocumentEntity> documentList = await _context.Documents
                                                                  .Where(c => c.Client.Id == tempId)
                                                                  .OrderByDescending(n => n.CreatedOn)
                                                                  .ToListAsync();

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDocument", documentList) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> OpenDocument(int id)
        {
            DocumentEntity document = await _context.Documents
                                                        .FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
            {
                return RedirectToAction("Home/Error404");
            }
            string mimeType = _mimeType.GetMimeType(document.FileName);
            return File(document.FileUrl, mimeType);
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMFeeAgreement(int id = 0, int idTCMCLient = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeFeeAgreementViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeFeeAgreementEntity intakefeeAgreement = _context.IntakeFeeAgreement
                                                                            .Include(n => n.Client)
                                                                            .ThenInclude(n => n.LegalGuardian)
                                                                            .FirstOrDefault(n => n.Client.Id == id);
                    if (intakefeeAgreement == null)
                    {
                        model = new IntakeFeeAgreementViewModel
                        {
                            Client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id),
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            Documents = true,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
                            AdmissionedFor = user_logged.FullName,
                            IdTCMClient = idTCMCLient
                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakefeeAgreement.Client.LegalGuardian == null)
                            intakefeeAgreement.Client.LegalGuardian = new LegalGuardianEntity();

                        model = _converterHelper.ToIntakeFeeAgreementViewModel(intakefeeAgreement);
                        model.IdTCMClient = idTCMCLient;
                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMFeeAgreement(IntakeFeeAgreementViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeFeeAgreementEntity IntakefeeAgreementEntity = await _converterHelper.ToIntakeFeeAgreementEntity(IntakeViewModel, false);

                if (IntakefeeAgreementEntity.Id == 0)
                {
                    IntakefeeAgreementEntity.Client = null;
                    _context.IntakeFeeAgreement.Add(IntakefeeAgreementEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 2 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakefeeAgreementEntity.Client = null;
                    _context.IntakeFeeAgreement.Update(IntakefeeAgreementEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 2 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMFeeAgreement", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMNonClinicalLog(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeNonClinicalLogViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeNonClinicalLogEntity intakeNonClinical = _context.TCMIntakeNonClinicalLog
                                                                          .Include(n => n.TcmClient)
                                                                          .ThenInclude(n => n.Client)
                                                                          .ThenInclude(n => n.LegalGuardian)
                                                                          .Include(n => n.TcmClient)
                                                                          .ThenInclude(n => n.Casemanager)
                                                                          .ThenInclude(n => n.Clinic)
                                                                          .FirstOrDefault(n => n.TcmClient.Id == id);

                    if (intakeNonClinical == null)
                    {

                        model = new TCMIntakeNonClinicalLogViewModel
                        {
                            TcmClient = _context.TCMClient
                                                .Include(d => d.Client)
                                                .ThenInclude(d => d.LegalGuardian)
                                                .Include(d => d.Casemanager)
                                                .ThenInclude(d => d.Clinic)
                                                .FirstOrDefault(n => n.Id == id),
                            Date = DateTime.Now,
                            Id = 0,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            IdTCMClient = id,
                            TcmClient_FK = id,
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeNonClinical.TcmClient.Client.LegalGuardian == null)
                            intakeNonClinical.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeNonClinicalLogViewModel(intakeNonClinical);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMNonClinicalLog(TCMIntakeNonClinicalLogViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeNonClinicalLogEntity IntakeNonClinicalEntity = await _converterHelper.ToTCMIntakeNonClinicalLogEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeNonClinicalEntity.Id == 0)
                {
                    IntakeNonClinicalEntity.TcmClient = null;
                    _context.TCMIntakeNonClinicalLog.Add(IntakeNonClinicalEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 2 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeNonClinicalEntity.TcmClient = null;
                    _context.TCMIntakeNonClinicalLog.Update(IntakeNonClinicalEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 2 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMNonClinicalLog", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMMiniMental(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeMiniMentalViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeMiniMentalEntity intakeMiniMental = _context.TCMIntakeMiniMental
                                                                          .Include(n => n.TcmClient)
                                                                          .ThenInclude(n => n.Client)
                                                                          .ThenInclude(n => n.LegalGuardian)
                                                                          .Include(n => n.TcmClient)
                                                                          .ThenInclude(n => n.Casemanager)
                                                                          .ThenInclude(n => n.Clinic)
                                                                          .FirstOrDefault(n => n.TcmClient.Id == id);

                    if (intakeMiniMental == null)
                    {

                        model = new TCMIntakeMiniMentalViewModel
                        {
                            TcmClient = _context.TCMClient
                                                .Include(d => d.Client)
                                                .ThenInclude(d => d.LegalGuardian)
                                                .Include(d => d.Casemanager)
                                                .ThenInclude(d => d.Clinic)
                                                .FirstOrDefault(n => n.Id == id),
                            Date = DateTime.Now,
                            Id = 0,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            IdTCMClient = id,
                            TcmClient_FK = id,
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeMiniMental.TcmClient.Client.LegalGuardian == null)
                            intakeMiniMental.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeMiniMenatalViewModel(intakeMiniMental);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMMiniMental(TCMIntakeMiniMentalViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeMiniMentalEntity IntakeMiniMenatalEntity = await _converterHelper.ToTCMIntakeMiniMenatalEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeMiniMenatalEntity.Id == 0)
                {
                    IntakeMiniMenatalEntity.TcmClient = null;
                    _context.TCMIntakeMiniMental.Add(IntakeMiniMenatalEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeMiniMenatalEntity.TcmClient = null;
                    _context.TCMIntakeMiniMental.Update(IntakeMiniMenatalEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMMiniMental", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMMedicalhistory(int id = 0, int idTCMClient = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeMedicalHistoryViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    IntakeMedicalHistoryEntity intakeMedicalHistory = _context.IntakeMedicalHistory
                                                                            .Include(n => n.Client)
                                                                            .ThenInclude(n => n.LegalGuardian)
                                                                            .FirstOrDefault(n => n.Client.Id == id);
                    if (intakeMedicalHistory == null)
                    {
                        model = new IntakeMedicalHistoryViewModel
                        {
                            Client = _context.Clients.Include(n => n.LegalGuardian).FirstOrDefault(n => n.Id == id),
                            IdClient = id,
                            Client_FK = id,
                            Id = 0,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
                            Documents = true,

                            AddressPhysician = "",
                            AgeFirstTalked = "",
                            AgeFirstWalked = "",
                            AgeToiletTrained = "",
                            AgeWeaned = "",
                            Allergies = false,
                            Allergies_Describe = "",
                            AndOrSoiling = false,
                            Anemia = false,
                            AreYouCurrently = false,
                            AreYouPhysician = false,
                            Arthritis = false,
                            AssumingCertainPositions = false,
                            BackPain = false,
                            BeingConfused = false,
                            BeingDisorientated = false,
                            BirthWeight = "",
                            BlackStools = false,
                            BloodInUrine = false,
                            BloodyStools = false,
                            BottleFedUntilAge = "",
                            BreastFed = false,
                            BurningUrine = false,
                            Calculating = false,
                            Cancer = false,
                            ChestPain = false,
                            ChronicCough = false,
                            ChronicIndigestion = false,
                            City = "",
                            Complications = false,
                            Complications_Explain = "",
                            Comprehending = false,
                            Concentrating = false,
                            Constipation = false,
                            ConvulsionsOrFits = false,
                            CoughingOfBlood = false,
                            DescriptionOfChild = "",
                            Diabetes = false,
                            Diphtheria = false,
                            DoYouSmoke = false,
                            DoYouSmoke_PackPerDay = "",
                            DoYouSmoke_Year = "",
                            EarInfections = false,
                            Epilepsy = false,
                            EyeTrouble = false,
                            Fainting = false,
                            FamilyAsthma = false,
                            FamilyAsthma_ = "",
                            FamilyCancer = false,
                            FamilyCancer_ = "",
                            FamilyDiabetes = false,
                            FamilyDiabetes_ = "",
                            FamilyEpilepsy = false,
                            FamilyEpilepsy_ = "",
                            FamilyGlaucoma = false,
                            FamilyGlaucoma_ = "",
                            FamilyHayFever = false,
                            FamilyHayFever_ = "",
                            FamilyHeartDisease = false,
                            FamilyHeartDisease_ = "",
                            FamilyHighBloodPressure = false,
                            FamilyHighBloodPressure_ = "",
                            FamilyKidneyDisease = false,
                            FamilyKidneyDisease_ = "",
                            FamilyNervousDisorders = false,
                            FamilyNervousDisorders_ = "",
                            FamilyOther = false,
                            FamilyOther_ = "",
                            FamilySyphilis = false,
                            FamilySyphilis_ = "",
                            FamilyTuberculosis = false,
                            FamilyTuberculosis_ = "",
                            FirstYearMedical = "",
                            Fractures = false,
                            FrequentColds = false,
                            FrequentHeadaches = false,
                            FrequentNoseBleeds = false,
                            FrequentSoreThroat = false,
                            FrequentVomiting = false,
                            HaveYouEverBeenPregnant = false,
                            HaveYouEverHadComplications = false,
                            HaveYouEverHadExcessive = false,
                            HaveYouEverHadPainful = false,
                            HaveYouEverHadSpotting = false,
                            HayFever = false,
                            HeadInjury = false,
                            Hearing = false,
                            HearingTrouble = false,
                            HeartPalpitation = false,
                            Hemorrhoids = false,
                            Hepatitis = false,
                            Hernia = false,
                            HighBloodPressure = false,
                            Hoarseness = false,
                            Immunizations = "",
                            InfectiousDisease = false,
                            Jaundice = false,
                            KidneyStones = false,
                            KidneyTrouble = false,
                            Length = "",
                            ListAllCurrentMedications = "",
                            LossOfMemory = false,
                            Mumps = false,
                            Nervousness = false,
                            NightSweats = false,
                            Normal = false,
                            PainfulJoints = false,
                            PainfulMuscles = false,
                            PainfulUrination = false,
                            PerformingCertainMotions = false,
                            Planned = false,
                            Poliomyelitis = false,
                            PrimaryCarePhysician = "",
                            ProblemWithBedWetting = false,
                            Reading = false,
                            RheumaticFever = false,
                            Rheumatism = false,
                            ScarletFever = false,
                            Seeing = false,
                            SeriousInjury = false,
                            ShortnessOfBreath = false,
                            SkinTrouble = false,
                            Speaking = false,
                            State = "",
                            StomachPain = false,
                            Surgery = false,
                            SwellingOfFeet = false,
                            SwollenAnkles = false,
                            Tuberculosis = false,
                            Unplanned = false,
                            VaricoseVeins = false,
                            VenerealDisease = false,
                            VomitingOfBlood = false,
                            Walking = false,
                            WeightLoss = false,
                            WhoopingCough = false,
                            WritingSentence = false,
                            ZipCode = "",
                            AgeOfFirstMenstruation = "",
                            DateOfLastBreastExam = "",
                            DateOfLastPelvic = "",
                            DateOfLastPeriod = "",
                            UsualDurationOfPeriods = "",
                            UsualIntervalBetweenPeriods = "",
                            AdmissionedFor = user_logged.FullName,

                        };
                        if (model.Client.LegalGuardian == null)
                            model.Client.LegalGuardian = new LegalGuardianEntity();
                        model.IdTCMClient = idTCMClient;
                        return View(model);
                    }
                    else
                    {
                        if (intakeMedicalHistory.Client.LegalGuardian == null)
                            intakeMedicalHistory.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToIntakeMedicalHistoryViewModel(intakeMedicalHistory);
                        model.IdTCMClient = idTCMClient;
                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMMedicalhistory(IntakeMedicalHistoryViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeMedicalHistoryEntity IntakeMedicalHistoryEntity = await _converterHelper.ToIntakeMedicalHistoryEntity(IntakeViewModel, false);

                if (IntakeMedicalHistoryEntity.Id == 0)
                {
                    IntakeMedicalHistoryEntity.Client = null;
                    _context.IntakeMedicalHistory.Add(IntakeMedicalHistoryEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeMedicalHistoryEntity.Client = null;
                    _context.IntakeMedicalHistory.Update(IntakeMedicalHistoryEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.Client = _context.Clients.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMMedicalhistory", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMMedication(int id = 0, int idTCMClient = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            MedicationViewModel model;

            if (User.IsInRole("CaseManager"))
            {


                if (user_logged.Clinic != null)
                {

                    model = new MedicationViewModel
                    {
                        IdClient = id,
                        Client = _context.Clients.Include(n => n.MedicationList).FirstOrDefault(n => n.Id == id),
                        Id = 0,
                        Dosage = "",
                        Frequency = "",
                        Name = "",
                        Prescriber = user_logged.FullName

                    };
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    model.IdTCMClient = idTCMClient;
                    return View(model);
                }
            }

            model = new MedicationViewModel
            {
                IdClient = id,
                Client = _context.Clients.Include(n => n.MedicationList).FirstOrDefault(n => n.Id == id),
                Id = 0,
                Dosage = "",
                Frequency = "",
                Name = "",
                Prescriber = ""
            };
            if (model.Client.MedicationList == null)
                model.Client.MedicationList = new List<MedicationEntity>();
            model.IdTCMClient = idTCMClient;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMMedication(MedicationViewModel MedicationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                MedicationEntity medicationEntity = _context.Medication.Find(MedicationViewModel.Id);
                if (medicationEntity == null)
                {
                    medicationEntity = await _converterHelper.ToMedicationEntity(MedicationViewModel, true);
                    _context.Medication.Add(medicationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        return RedirectToAction("CreateTCMMedication", new { id = MedicationViewModel.IdClient, IdTCMClient = MedicationViewModel.IdTCMClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Medication.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMMedication", MedicationViewModel) });
                }
            }
            MedicationViewModel model;
            model = new MedicationViewModel
            {
                IdClient = MedicationViewModel.IdClient,
                Client = _context.Clients.Find(MedicationViewModel.IdClient),
                Id = MedicationViewModel.Id,
                Dosage = MedicationViewModel.Dosage,
                Frequency = MedicationViewModel.Frequency,
                Name = MedicationViewModel.Name,
                Prescriber = MedicationViewModel.Prescriber

            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMMedication", MedicationViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult EditTCMMedication(int id = 0)
        {
            MedicationViewModel model;

            if (User.IsInRole("CaseManager") )
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    MedicationEntity Medication = _context.Medication
                                                         .Include(m => m.Client)
                                                         .ThenInclude(m => m.MedicationList)
                                                         .FirstOrDefault(m => m.Id == id);
                    if (Medication == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToMedicationViewModel(Medication);

                        return View(model);
                    }

                }
            }

            model = new MedicationViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> EditTCMMedication(MedicationViewModel medicationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                MedicationEntity medicationEntity = await _converterHelper.ToMedicationEntity(medicationViewModel, false);
                _context.Medication.Update(medicationEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    return RedirectToAction("CreateTCMMedication", new { id = medicationViewModel.IdClient });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditTCMMedication", medicationViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteTCMMedication(int? id, int origin = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MedicationEntity medicationEntity = await _context.Medication.Include(n => n.Client).FirstOrDefaultAsync(s => s.Id == id);
            if (medicationEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Medication.Remove(medicationEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

             //return RedirectToAction(nameof(Create), new { id = medicationEntity.Client.Id });
             return RedirectToAction("CreateTCMMedication", new { id = medicationEntity.Client.Id });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMCoordinationCare(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeCoordinationCareViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeCoordinationCareEntity intakeCoordination = _context.TCMIntakeCoordinationCare
                                                                                 .Include(n => n.TcmClient)
                                                                                 .ThenInclude(n => n.Client)
                                                                                 .ThenInclude(n => n.LegalGuardian)
                                                                                 .Include(n => n.TcmClient)
                                                                                 .ThenInclude(n => n.Casemanager)
                                                                                 .ThenInclude(n => n.Clinic)
                                                                                 .Include(n => n.TcmClient)
                                                                                 .ThenInclude(n => n.Client)
                                                                                 .ThenInclude(n => n.Doctor)
                                                                                 .FirstOrDefault(n => n.TcmClient.Id == id);

                    if (intakeCoordination == null)
                    {

                        model = new TCMIntakeCoordinationCareViewModel
                        {
                            TcmClient = _context.TCMClient
                                                .Include(d => d.Client)
                                                .ThenInclude(d => d.LegalGuardian)
                                                .Include(d => d.Casemanager)
                                                .ThenInclude(d => d.Clinic)
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.Doctor)
                                                .FirstOrDefault(n => n.Id == id),
                            Date = DateTime.Now,
                            Id = 0,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            IdTCMClient = id,
                            TcmClient_FK = id,
                            AdmissionedFor = user_logged.FullName,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
                            Documents = true,
                            IAuthorize = true,
                            InformationAllBefore = false,
                            InformationElectronic = false,
                            InformationFascimile = false,
                            InformationNonKnown = false,
                            InformationToRelease = false,
                            InformationTorequested = false,
                            InformationVerbal = false,
                            InformationWrited = false,
                            IRefuse = true,
                            PCP = true,
                            Specialist = false,
                            SpecialistText = ""
                            



                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        if (model.TcmClient.Client.Doctor == null)
                            model.TcmClient.Client.Doctor = new DoctorEntity();
                        return View(model);
                    }
                    else
                    {
                        if (intakeCoordination.TcmClient.Client.LegalGuardian == null)
                            intakeCoordination.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        if (intakeCoordination.TcmClient.Client.Doctor == null)
                            intakeCoordination.TcmClient.Client.Doctor = new DoctorEntity();
                        model = _converterHelper.ToTCMIntakeCoordinationCareViewModel(intakeCoordination);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMCoordinationCare(TCMIntakeCoordinationCareViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeCoordinationCareEntity IntakeCoordination = await _converterHelper.ToTCMIntakeCoordinationCareEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeCoordination.Id == 0)
                {
                    IntakeCoordination.TcmClient = null;
                    _context.TCMIntakeCoordinationCare.Add(IntakeCoordination);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeCoordination.TcmClient = null;
                    _context.TCMIntakeCoordinationCare.Update(IntakeCoordination);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMCoordinationCare", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> TCMIntakeSectionDashboard(int id = 0, int section = 0, int origin = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMClientEntity TcmClientEntity = await _context.TCMClient
                                                            .Include(c => c.TCMIntakeForm)
                                                            .Include(c => c.Client)
                                                            .Include(c => c.TcmIntakeConsentForTreatment)
                                                            .Include(n => n.TcmIntakeConsentForRelease.Where(m => m.TcmClient_FK == id))
                                                            .Include(n => n.TcmIntakeConsumerRights)
                                                            .Include(n => n.TcmIntakeAcknowledgementHipa)
                                                            .Include(n => n.TCMIntakeOrientationChecklist)
                                                            .Include(n => n.TCMIntakeAdvancedDirective)
                                                            .Include(n => n.TCMIntakeForeignLanguage)
                                                            .Include(n => n.TCMIntakeWelcome)
                                                            .Include(n => n.Client)
                                                            .ThenInclude(n => n.Documents)
                                                            .Include(n => n.Client.Psychiatrist)
                                                            .Include(n => n.Client.Doctor)
                                                            .Include(n => n.Client.IntakeFeeAgreement)
                                                            .Include(n => n.Client.IntakeMedicalHistory)
                                                            .Include(n => n.Client.MedicationList)
                                                            .Include(n => n.TCMIntakeNonClinicalLog)
                                                            .Include(n => n.TCMIntakeMiniMental)
                                                            .Include(n => n.TCMIntakeCoordinationCare)
                                                            .Include(n => n.TcmServicePlan)
                                                            .ThenInclude(n => n.TCMAdendum)
                                                            .Include(n => n.TcmServicePlan)
                                                            .ThenInclude(n => n.TCMServicePlanReview)
                                                            .Include(n => n.TcmServicePlan)
                                                            .ThenInclude(n => n.TCMDischarge)
                                                            .Include(n => n.TcmIntakeAppendixJ)
                                                            .Include(n => n.TcmInterventionLog)
                                                            .Include(n => n.TCMFarsFormList)
                                                            .Include(n => n.TCMAssessment)
                                                            .ThenInclude(n => n.HouseCompositionList)
                                                            .ThenInclude(n => n.TcmAssessment.MedicationList)
                                                            .ThenInclude(n => n.TcmAssessment.IndividualAgencyList)
                                                            .ThenInclude(n => n.TcmAssessment.PastCurrentServiceList)
                                                            .Include(n => n.TCMNote)
                                                            .FirstOrDefaultAsync(c => c.Id == id);

            List<TCMIntakeConsentForReleaseEntity> listRelease = await _context.TCMIntakeConsentForRelease
                                                                               .Where(m => m.TcmClient_FK == id).ToListAsync();
            List<DocumentEntity> listDocument = await _context.Documents
                                                              .Where(m => m.Client.Id == TcmClientEntity.Client.Id).ToListAsync();

            TcmClientEntity.TcmIntakeConsentForRelease = listRelease;
            TcmClientEntity.Client.Documents = listDocument;

            if (TcmClientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }
            ViewBag.Section = section.ToString();
            ViewData["origin"] = origin;
            return View(TcmClientEntity);
        }

        [Authorize(Roles = "TCMSupervisor, Manager")]
        public async Task<IActionResult> TCMIntakeSectionDashboardReadOnly(int id = 0, int section = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMClientEntity TcmClientEntity = await _context.TCMClient
                                                            .Include(c => c.TCMIntakeForm)
                                                            .Include(c => c.Client)
                                                            .Include(c => c.TcmIntakeConsentForTreatment)
                                                            .Include(n => n.TcmIntakeConsentForRelease.Where(m => m.TcmClient_FK == id))
                                                            .Include(n => n.TcmIntakeConsumerRights)
                                                            .Include(n => n.TcmIntakeAcknowledgementHipa)
                                                            .Include(n => n.TCMIntakeOrientationChecklist)
                                                            .Include(n => n.TCMIntakeAdvancedDirective)
                                                            .Include(n => n.TCMIntakeForeignLanguage)
                                                            .Include(n => n.TCMIntakeWelcome)
                                                            .Include(n => n.Client)
                                                            .ThenInclude(n => n.Documents)
                                                            .Include(n => n.Client.Psychiatrist)
                                                            .Include(n => n.Client.Doctor)
                                                            .Include(n => n.Client.IntakeFeeAgreement)
                                                            .Include(n => n.Client.IntakeMedicalHistory)
                                                            .Include(n => n.Client.MedicationList)
                                                            .Include(n => n.TCMIntakeNonClinicalLog)
                                                            .Include(n => n.TCMIntakeMiniMental)
                                                            .Include(n => n.TCMIntakeCoordinationCare)
                                                            .Include(n => n.TcmServicePlan)
                                                            .ThenInclude(n => n.TCMAdendum)
                                                            .Include(n => n.TcmServicePlan)
                                                            .ThenInclude(n => n.TCMServicePlanReview)
                                                            .Include(n => n.TcmServicePlan)
                                                            .ThenInclude(n => n.TCMDischarge)
                                                            .Include(n => n.TcmIntakeAppendixJ)
                                                            .Include(n => n.TcmInterventionLog)
                                                            .Include(n => n.TCMFarsFormList)
                                                            .Include(n => n.TCMAssessment)
                                                            .ThenInclude(n => n.HouseCompositionList)
                                                            .ThenInclude(n => n.TcmAssessment.MedicationList)
                                                            .ThenInclude(n => n.TcmAssessment.IndividualAgencyList)
                                                            .ThenInclude(n => n.TcmAssessment.PastCurrentServiceList)
                                                            .Include(n => n.TCMNote)
                                                            .FirstOrDefaultAsync(c => c.Id == id);

            List<TCMIntakeConsentForReleaseEntity> listRelease = await _context.TCMIntakeConsentForRelease
                                                                               .Where(m => m.TcmClient_FK == id).ToListAsync();
            List<DocumentEntity> listDocument = await _context.Documents
                                                              .Where(m => m.Client.Id == TcmClientEntity.Client.Id).ToListAsync();

            TcmClientEntity.TcmIntakeConsentForRelease = listRelease;
            TcmClientEntity.Client.Documents = listDocument;

            if (TcmClientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }
            ViewBag.Section = section.ToString();
            return View(TcmClientEntity);
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMAppendixJ(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeAppendixJViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeAppendixJEntity intakeAppendixJ = _context.TCMIntakeAppendixJ
                                                                     .Include(n => n.TcmClient)
                                                                     .ThenInclude(n => n.Client)
                                                                     .FirstOrDefault(n => n.TcmClient.Id == id);
                    if (intakeAppendixJ == null)
                    {
                        model = new TCMIntakeAppendixJViewModel
                        {
                            TcmClient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .FirstOrDefault(n => n.Id == id),
                            IdTCMClient = id,
                            TcmClient_FK = id,
                            Id = 0,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            AdmissionedFor = user_logged.FullName,
                            Approved = 0,
                            Date = DateTime.Now,
                            HasBeen = false,
                            HasHad = false,
                            IsAt = false,
                            IsAwaiting = false,
                            IsExperiencing = false,
                            SupervisorSignatureDate = DateTime.Now,
                            TcmSupervisor = new TCMSupervisorEntity()

                        };
                       
                        return View(model);
                    }
                    else
                    {
                        model = _converterHelper.ToTCMIntakeAppendixJViewModel(intakeAppendixJ);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMAppendixJ(TCMIntakeAppendixJViewModel AppendixJViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeAppendixJEntity AppendixJEntity = await _converterHelper.ToTCMIntakeAppendixJEntity(AppendixJViewModel, false, user_logged.UserName);

                if (AppendixJEntity.Id == 0)
                {
                    AppendixJEntity.TcmClient = null;
                    AppendixJEntity.Approved = 1;
                    _context.TCMIntakeAppendixJ.Add(AppendixJEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = AppendixJViewModel.IdTCMClient, section = 4 });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    AppendixJEntity.TcmClient = null;
                    AppendixJEntity.Approved = 1;
                    _context.TCMIntakeAppendixJ.Update(AppendixJEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = AppendixJViewModel.IdTCMClient , section = 4});
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            AppendixJViewModel.TcmClient = _context.TCMClient.Find(AppendixJViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMAppendixJ", AppendixJViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMInterventionLog(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeInterventionLogViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeInterventionLogEntity interventionLog = _context.TCMIntakeInterventionLog
                                                                             .Include(n => n.TcmClient)
                                                                             .ThenInclude(n => n.Client)
                                                                             .Include(n => n.TcmClient)
                                                                             .ThenInclude(n => n.Casemanager)
                                                                             .Include(n => n.InterventionList)
                                                                             .FirstOrDefault(n => n.InterventionList.Count() > 0);
                    if(interventionLog == null)
                    {
                        model = new TCMIntakeInterventionLogViewModel
                        {
                            TcmClient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .Include(n => n.Casemanager)
                                                .FirstOrDefault(n => n.Id == id),
                            IdTCMClient = id,
                            TcmClient_FK = id,
                            Id = 0,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            InterventionList = new List<TCMIntakeInterventionEntity>()
                  
                        };

                        return View(model);
                    }
                    else
                    {
                        model = _converterHelper.ToTCMIntakeInterventionLogViewModel(interventionLog);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("TCMIntakeSectionDashboard", "Intakes", new { id = id, section = 5 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMInterventionLog(TCMIntakeInterventionLogViewModel interventionLogViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeInterventionLogEntity InterventionLogEntity = await _converterHelper.ToTCMIntakeInterventionLogEntity(interventionLogViewModel, true, user_logged.UserName);

                InterventionLogEntity.TcmClient = null;
                _context.TCMIntakeInterventionLog.Add(InterventionLogEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("TCMIntakeSectionDashboard", new { id = interventionLogViewModel.IdTCMClient, section = 5 });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            interventionLogViewModel.TcmClient = _context.TCMClient.Find(interventionLogViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMInterventionLog", interventionLogViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult EditTCMInterventionLog(int id = 0)
        {
            TCMIntakeInterventionLogViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    TCMIntakeInterventionLogEntity TcmIntervention = _context.TCMIntakeInterventionLog
                                                                             .Include(b => b.TcmClient)
                                                                             .ThenInclude(b => b.Client)
                                                                             .Include(b => b.TcmClient.Casemanager)
                                                                             .Include(b => b.InterventionList)
                                                                             .FirstOrDefault(m => m.Id == id);
                    if (TcmIntervention == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToTCMIntakeInterventionLogViewModel(TcmIntervention);

                        return View(model);
                    }

                }
            }

            model = new TCMIntakeInterventionLogViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> EditTCMInterventionLog(TCMIntakeInterventionLogViewModel tcmInterLogViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeInterventionLogEntity tcmInterLogEntity = await _converterHelper.ToTCMIntakeInterventionLogEntity(tcmInterLogViewModel, false, user_logged.UserName);
                _context.TCMIntakeInterventionLog.Update(tcmInterLogEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    return RedirectToAction("TCMIntakeSectionDashboard", new { id = tcmInterLogEntity.TcmClient_FK, section = 5 });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditTCMInterventionLog", tcmInterLogViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMIntervention(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeInterventionViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeInterventionLogEntity interventionLog = _context.TCMIntakeInterventionLog
                                                                                 .Include(n => n.TcmClient)
                                                                                 .ThenInclude(n => n.Client)
                                                                                 .Include(n => n.TcmClient.Casemanager)
                                                                                 .FirstOrDefault(n => n.Id == id);
                    if (interventionLog != null)
                    {
                        model = new TCMIntakeInterventionViewModel
                        {
                            TcmInterventionLog = interventionLog,
                            Id = 0,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            Activity = "",
                            Date = DateTime.Now,
                            IdInterventionLog = id
                                                      
                        };

                        return View(model);
                    }
                    else
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }

                }
            }

            return RedirectToAction("TCMIntakeSectionDashboard", "Intakes", new { id = id, section = 5 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMIntervention(TCMIntakeInterventionViewModel interventionViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeInterventionEntity InterventionEntity = await _converterHelper.ToTCMIntakeInterventionEntity(interventionViewModel, true, user_logged.UserName);

                _context.TCMIntakeIntervention.Add(InterventionEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMIntakeInterventionEntity> salida = await _context.TCMIntakeIntervention
                                                                             .Include(n => n.TcmInterventionLog)
                                                                             .Where(m => m.TcmInterventionLog.Id == interventionViewModel.IdInterventionLog)
                                                                             .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMIntakeInterventionLIst", salida) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            //interventionLogViewModel.TcmClient = _context.TCMClient.Find(interventionLogViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMInterventionLog", interventionViewModel) });
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> ApendiceJStatus(int approved = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<TCMIntakeAppendixJEntity> apendiceJ = new List<TCMIntakeAppendixJEntity>();

            if (User.IsInRole("Casemanager"))
            {
                apendiceJ = await _context.TCMIntakeAppendixJ
                                     .Include(w => w.TcmClient)
                                     .ThenInclude(d => d.Client)
                                     .ThenInclude(d => d.Clinic)
                                     .Include(w => w.TcmClient)
                                     .ThenInclude(d => d.Casemanager)
                                     .Where(w => (w.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                        && w.Approved == approved
                                        && w.TcmClient.Casemanager.LinkedUser == user_logged.UserName))
                                     .ToListAsync();
            }
            else
            {
                apendiceJ = await _context.TCMIntakeAppendixJ
                                    .Include(w => w.TcmClient)
                                    .ThenInclude(d => d.Client)
                                    .ThenInclude(d => d.Clinic)
                                    .Include(w => w.TcmClient)
                                    .ThenInclude(d => d.Casemanager)
                                    .Where(w => (w.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                       && w.Approved == approved))
                                    .ToListAsync();
            }

            return View(apendiceJ);
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult CreateTCMAppendixJReadOnly(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeAppendixJViewModel model;

            if (User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeAppendixJEntity intakeAppendixJ = _context.TCMIntakeAppendixJ
                                                                     .Include(n => n.TcmClient)
                                                                     .ThenInclude(n => n.Client)
                                                                     .FirstOrDefault(n => n.TcmClient.Id == id);
                    if (intakeAppendixJ == null)
                    {
                        model = new TCMIntakeAppendixJViewModel
                        {
                            TcmClient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .FirstOrDefault(n => n.Id == id),
                            IdTCMClient = id,
                            TcmClient_FK = id,
                            Id = 0,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            AdmissionedFor = user_logged.FullName,
                            Approved = 0,
                            Date = DateTime.Now,
                            HasBeen = false,
                            HasHad = false,
                            IsAt = false,
                            IsAwaiting = false,
                            IsExperiencing = false,
                            SupervisorSignatureDate = DateTime.Now,
                            TcmSupervisor = new TCMSupervisorEntity()

                        };

                        return View(model);
                    }
                    else
                    {
                        model = _converterHelper.ToTCMIntakeAppendixJViewModel(intakeAppendixJ);

                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> AproveAppendixJ(int id)
        {
            TCMIntakeAppendixJEntity tcmAppendixJ = _context.TCMIntakeAppendixJ
                                                            .Include(u => u.TcmClient)
                                                            .ThenInclude(u => u.Client)
                                                            .FirstOrDefault(u => u.Id == id);

            if (tcmAppendixJ != null)
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        tcmAppendixJ.Approved = 2;
                        tcmAppendixJ.TcmSupervisor = await _context.TCMSupervisors.FirstOrDefaultAsync(n => n.LinkedUser == user_logged.UserName);
                        _context.Update(tcmAppendixJ);
                        try
                        {
                            await _context.SaveChangesAsync();

                            return RedirectToAction("ApendiceJStatus", "TCMIntakes", new { approved = 1 });
                        }
                        catch (System.Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("Index", "TCMIntakes");
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> FinishEditingAppendixJ(int id, int origin = 0)
        {
            TCMIntakeAppendixJEntity ApendiceJ = _context.TCMIntakeAppendixJ
                                                         .Include(u => u.TcmClient)
                                                         .ThenInclude(u => u.Client)
                                                         .FirstOrDefault(u => u.Id == id);

            if (ApendiceJ != null)
            {
                if (User.IsInRole("CaseManager"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        ApendiceJ.Approved = 1;
                        _context.Update(ApendiceJ);
                        try
                        {
                            await _context.SaveChangesAsync();
                            if (origin == 0)
                            {
                                return RedirectToAction("TCMIntakeSectionDashboard", "TCMIntakes", new { id = ApendiceJ.TcmClient.Id, section = 4 });
                            }
                            else
                            {
                                return RedirectToAction("ServicePlanStarted", "TCMServicePlans", new { approved = 1 });
                            }
                        }
                        catch (System.Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("Index", "TCMServicePlans");
        }

    }


}
