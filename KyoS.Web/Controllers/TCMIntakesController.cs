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

        public TCMIntakesController(IUserHelper userHelper, IConverterHelper converterHelper, ICombosHelper combosHelper, IRenderHelper renderHelper, DataContext context, IReportHelper reportHelper)
        {
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _context = context;
            _renderHelper = renderHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
        public async Task<IActionResult> Index(int idError = 0)
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
                                                          .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id)
                                                          .ToListAsync();
                return View(tcmClient);
            }
        }

        [Authorize(Roles = "Manager")]
        public IActionResult Create(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeFormViewModel model;

            if (User.IsInRole("Manager"))
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
        [Authorize(Roles = "Manager")]
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
                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTCMClient });
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

        [Authorize(Roles = "Manager")]
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
                                                            .FirstOrDefaultAsync(c => c.Id == id);
            List<TCMIntakeConsentForReleaseEntity> listRelease = await _context.TCMIntakeConsentForRelease
                                                                               .Where(m => m.TcmClient_FK == id).ToListAsync();
            TcmClientEntity.TcmIntakeConsentForRelease = listRelease;

            if (TcmClientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            return View(TcmClientEntity);
        }

        [Authorize(Roles = "Manager")]
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

            if (User.IsInRole("Manager"))
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
        [Authorize(Roles = "Manager")]
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
                    return RedirectToAction("TCMIntakeDashboard", new { id = intakeViewModel.IdTCMClient });
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

        [Authorize(Roles = "Manager")]
        public IActionResult CreateTCMConsentForTreatment(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeConsentForTreatmentViewModel model;

            if (User.IsInRole("Manager"))
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
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateTCMConsentForTreatment(TCMIntakeConsentForTreatmentViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeConsentForTreatmentEntity IntakeConsentEntity = await _converterHelper.ToTCMIntakeConsentForTreatmentEntity(IntakeViewModel, false);

                if (IntakeConsentEntity.Id == 0)
                {
                    IntakeConsentEntity.TcmClient = null;
                    _context.TCMIntakeConsentForTreatment.Add(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTCMClient });
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

                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTCMClient });
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

        [Authorize(Roles = "Manager")]
        public IActionResult CreateTCMConsentForRelease(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeConsentForReleaseViewModel model;

            if (User.IsInRole("Manager"))
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
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateTCMConsentForRelease(TCMIntakeConsentForReleaseViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeConsentForReleaseEntity IntakeConsentEntity = await _converterHelper.ToTCMIntakeConsentForReleaseEntity(IntakeViewModel, false);

                if (IntakeConsentEntity.Id == 0)
                {
                   
                    _context.TCMIntakeConsentForRelease.Add(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTCMClient });
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
                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTCMClient });
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

        [Authorize(Roles = "Manager")]
        public IActionResult CreateTCMConsumerRights(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeConsumerRightsViewModel model;

            if (User.IsInRole("Manager"))
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
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateTCMConsumerRights(TCMIntakeConsumerRightsViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeConsumerRightsEntity IntakeConsumerEntity = await _converterHelper.ToTCMIntakeConsumerRightsEntity(IntakeViewModel, false);

                if (IntakeConsumerEntity.Id == 0)
                {
                    IntakeConsumerEntity.TcmClient = null;
                    _context.TCMIntakeConsumerRights.Add(IntakeConsumerEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTCMClient });
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
                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTCMClient });
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

        [Authorize(Roles = "Manager")]
        public IActionResult CreateTCMAcknowledgementHippa(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeAcknoewledgementHippaViewModel model;

            if (User.IsInRole("Manager"))
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
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateTCMAcknowledgementHippa(TCMIntakeAcknoewledgementHippaViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeAcknowledgementHippaEntity IntakeAckNowEntity = await _converterHelper.ToTCMIntakeAcknoewledgementHippaEntity(IntakeViewModel, false);

                if (IntakeAckNowEntity.Id == 0)
                {
                    IntakeAckNowEntity.TcmClient = null;
                    _context.TCMIntakeAcknowledgement.Add(IntakeAckNowEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTCMClient });
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
                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTCMClient });
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

        [Authorize(Roles = "Manager")]
        public IActionResult CreateTCMOrientationCheckList(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeOrientationCheckListViewModel model;

            if (User.IsInRole("Manager"))
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
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateTCMOrientationCheckList(TCMIntakeOrientationCheckListViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeOrientationChecklistEntity IntakeOrientationEntity = await _converterHelper.ToTCMIntakeOrientationChecklistEntity(IntakeViewModel, false);

                if (IntakeOrientationEntity.Id == 0)
                {
                    IntakeOrientationEntity.TcmClient = null;
                    _context.TCMIntakeOrientationCheckList.Add(IntakeOrientationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTcmClient });
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
                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTcmClient });
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

        [Authorize(Roles = "Manager")]
        public IActionResult CreateTCMIntakeAdvenceDirective(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeAdvancedDirectiveViewModel model;

            if (User.IsInRole("Manager"))
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
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateTCMIntakeAdvenceDirective(TCMIntakeAdvancedDirectiveViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeAdvancedDirectiveEntity IntakeConsumerEntity = await _converterHelper.ToTCMIntakeAdvancedDirectiveEntity(IntakeViewModel, false);

                if (IntakeConsumerEntity.Id == 0)
                {
                    IntakeConsumerEntity.TcmClient = null;
                    _context.TCMIntakeAdvancedDirective.Add(IntakeConsumerEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTCMClient });
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
                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTCMClient});
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

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
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
                return View(listRelease);
            }
        }

        [Authorize(Roles = "Manager")]
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

            if (User.IsInRole("Manager"))
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
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditConsentForRelease(TCMIntakeConsentForReleaseViewModel intakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeConsentForReleaseEntity intakeEntity = await _converterHelper.ToTCMIntakeConsentForReleaseEntity(intakeViewModel, false);
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

        [Authorize(Roles = "Manager")]
        public IActionResult CreateTCMForeignLanguage(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeForeignLanguageViewModel model;

            if (User.IsInRole("Manager"))
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
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateTCMForeignLanguage(TCMIntakeForeignLanguageViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeForeignLanguageEntity IntakeForeignEntity = await _converterHelper.ToTCMIntakeForeignLanguageEntity(IntakeViewModel, false);

                if (IntakeForeignEntity.Id == 0)
                {
                    IntakeForeignEntity.TcmClient = null;
                    _context.TCMIntakeForeignLanguage.Add(IntakeForeignEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTCMClient });
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
                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTCMClient });
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

        [Authorize(Roles = "Manager")]
        public IActionResult CreateTCMWelcome(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeWelcomeViewModel model;

            if (User.IsInRole("Manager"))
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
                                                .FirstOrDefault(n => n.Id == id),
                            Date = DateTime.Now,
                            Id = 0,
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
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateTCMWelcome(TCMIntakeWelcomeViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeWelcomeEntity IntakeForeignEntity = await _converterHelper.ToTCMIntakeWelcomeEntity(IntakeViewModel, false);

                if (IntakeForeignEntity.Id == 0)
                {
                    IntakeForeignEntity.TcmClient = null;
                    _context.TCMIntakeWelcome.Add(IntakeForeignEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTCMClient });
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
                        return RedirectToAction("TCMIntakeDashboard", new { id = IntakeViewModel.IdTCMClient });
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

    }
}
