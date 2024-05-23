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
using KyoS.Common.Helpers;
using KyoS.Web.Migrations;
using DocumentFormat.OpenXml.Presentation;

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
                if (User.IsInRole("Manager"))
                {
                    List<TCMClientEntity> tcmClient = await _context.TCMClient
                                                                    
                                                                    .Include(n => n.Client)
                                                                    .ThenInclude(n => n.Clinic)
                                                                    .Include(n => n.Casemanager)
                                                                    .AsSplitQuery()
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
                if (User.IsInRole("TCMSupervisor"))
                {
                    List<TCMClientEntity> tcmClient = await _context.TCMClient
                                                                   
                                                                    .Include(n => n.Client)
                                                                    .ThenInclude(n => n.Clinic)
                                                                    .Include(n => n.Casemanager)
                                                                    .AsSplitQuery()
                                                                    .Where(n => (n.Client.Clinic.Id == user_logged.Clinic.Id
                                                                              && n.Status == status
                                                                              && n.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName))
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
                                                                    
                                                                    .Include(n => n.Client)
                                                                    .ThenInclude(n => n.Clinic)
                                                                    .Include(n => n.Casemanager)
                                                                    .AsSplitQuery()
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
        public IActionResult Create(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(m => m.Client)
                                                .ThenInclude(m => m.Clients_Diagnostics)
                                                .ThenInclude(m => m.Diagnostic)
                                                .Include(m => m.Client)
                                                .ThenInclude(m => m.Clients_HealthInsurances.Where(m => m.Active == true))
                                                .ThenInclude(m => m.HealthInsurance)
                                                .Include(m => m.Client.LegalGuardian)
                                                .Include(n => n.Client.EmergencyContact)
                                                .Include(n => n.Client.Client_Referred)
                                                .Include(n => n.Client.Doctor)
                                                .Include(n => n.Client.Psychiatrist)
                                                .Include(n => n.TCMIntakeCoordinationCare)
                                                .Include(n => n.Casemanager)
                                                .FirstOrDefault(n => n.Id == id);
            if (tcmClient.Client.Doctor == null)
                tcmClient.Client.Doctor = new DoctorEntity();
            if (tcmClient.Client.Psychiatrist == null)
                tcmClient.Client.Psychiatrist = new PsychiatristEntity();

            TCMIntakeFormViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {

                    model = new TCMIntakeFormViewModel
                    {
                        IdTCMClient = id,
                        TcmClient_FK = id,
                        TcmClient = tcmClient,
                        Agency = "",
                        CaseManagerNotes = "",
                        Elibigility = "",
                        IdEmployedStatus = 0,
                        EmployedStatus = _combosHelper.GetComboEmployed(),
                        Grade = "",
                        IntakeDate = tcmClient.DataOpen,
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
                        IdResidentialStatus = 0,
                        ResidentialStatusList = _combosHelper.GetComboResidential(),
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
                        CreatedOn = DateTime.Now,
                        IdRelationshipEC = 0,
                        RelationshipsEC = _combosHelper.GetComboRelationships(),
                        IdRelationshipLG = 0,
                        RelationshipsLG = _combosHelper.GetComboRelationships(),
                        HealthPlan = _combosHelper.GetComboActiveInsurancesByClinic(user_logged.Clinic.Id),
                        PCP_Name = tcmClient.Client.Doctor.Name,
                        PCP_Address = tcmClient.Client.Doctor.Address,
                        PCP_Phone = tcmClient.Client.Doctor.Telephone,
                        PCP_CityStateZip = tcmClient.Client.Doctor.City + ", " + tcmClient.Client.Doctor.State + ", " + tcmClient.Client.Doctor.ZipCode,
                        PCP_Place = "",
                        PCP_FaxNumber = tcmClient.Client.Doctor.FaxNumber,
                        Psychiatrist_Name = tcmClient.Client.Psychiatrist.Name,
                        Psychiatrist_Address = tcmClient.Client.Psychiatrist.Address,
                        Psychiatrist_Phone = tcmClient.Client.Psychiatrist.Telephone,
                        Psychiatrist_CityStateZip = tcmClient.Client.Psychiatrist.City + ", "+ tcmClient.Client.Psychiatrist.State + ", " + tcmClient.Client.Psychiatrist.ZipCode
                    };
                    if (tcmClient.Client.LegalGuardian != null)
                    {
                        model.LegalGuardianName = tcmClient.Client.LegalGuardian.Name;
                        model.LegalGuardianAddress = tcmClient.Client.LegalGuardian.Address;
                        model.LegalGuardianTelephone = tcmClient.Client.LegalGuardian.Telephone;
                        model.LegalGuardianCity = tcmClient.Client.LegalGuardian.City;
                        model.LegalGuardianState = tcmClient.Client.LegalGuardian.State;
                        model.LegalGuardianZipCode = tcmClient.Client.LegalGuardian.ZipCode;
                        model.IdRelationshipLG = Convert.ToInt32(tcmClient.Client.RelationShipOfLegalGuardian);
                    }
                    if (tcmClient.Client.EmergencyContact != null)
                    {
                        model.EmergencyContacTelephone = tcmClient.Client.EmergencyContact.Telephone;
                        model.EmergencyContactName = tcmClient.Client.EmergencyContact.Name;
                        model.IdRelationshipEC = Convert.ToInt32(tcmClient.Client.RelationShipOfEmergencyContact);
                    }
                    if (tcmClient.Client.Clients_HealthInsurances.Count() > 0)
                    {
                        model.HealthMemberId = tcmClient.Client.Clients_HealthInsurances.ElementAtOrDefault(0).MemberId;
                        model.IdHealthPlan = tcmClient.Client.Clients_HealthInsurances.ElementAtOrDefault(0).HealthInsurance.Id;
                    }
                    if (model.TcmClient.Client.Clients_Diagnostics.Count() == 0)
                    {
                        Client_Diagnostic diagnostic = new Client_Diagnostic();
                        diagnostic.Diagnostic = new DiagnosticEntity();
                        diagnostic.Diagnostic.Code = "";
                        diagnostic.Diagnostic.Description = "";
                        model.TcmClient.Client.Clients_Diagnostics.Add(diagnostic);
                    }
               
                    ViewData["origi"] = origi;
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
                                    .ThenInclude(m => m.Clients_HealthInsurances.Where(m => m.Active == true))
                                    .ThenInclude(m => m.HealthInsurance)
                                    .Include(m => m.Client.LegalGuardian)
                                    .Include(n => n.Client.EmergencyContact)
                                    .Include(n => n.Client.Client_Referred)
                                    .Include(n => n.Client.Doctor)
                                    .Include(n => n.Client.Psychiatrist)
                                    .Include(n => n.Casemanager)
                                    .FirstOrDefault(n => n.Id == id),
                Agency = "",
                CaseManagerNotes = "",
                Elibigility = "",
                IdEmployedStatus = 0,
                EmployedStatus = _combosHelper.GetComboEmployed(),
                CreatedBy = user_logged.UserName,
                CreatedOn = DateTime.Now,
                Grade = "",
                IntakeDate = tcmClient.DataOpen,
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
                IdResidentialStatus = 0,
                ResidentialStatusList = _combosHelper.GetComboResidential(),
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
                IdRelationshipEC = 0,
                RelationshipsEC = _combosHelper.GetComboRelationships(),
                IdRelationshipLG = 0,
                RelationshipsLG = _combosHelper.GetComboRelationships(),
                PCP_Name = tcmClient.Client.Doctor.Name,
                PCP_Address = tcmClient.Client.Doctor.Address,
                PCP_Phone = tcmClient.Client.Doctor.Telephone,
                PCP_CityStateZip = tcmClient.Client.Doctor.Telephone,
                PCP_Place = "",
                Psychiatrist_Name = tcmClient.Client.Psychiatrist.Name,
                Psychiatrist_Address = tcmClient.Client.Psychiatrist.Address,
                Psychiatrist_Phone = tcmClient.Client.Psychiatrist.Telephone,
                Psychiatrist_CityStateZip = tcmClient.Client.Psychiatrist.Telephone,
                PCP_FaxNumber = tcmClient.Client.Doctor.FaxNumber

            };

            if (tcmClient.Client.LegalGuardian != null)
            {
                model.LegalGuardianName = tcmClient.Client.LegalGuardian.Name;
                model.LegalGuardianAddress = tcmClient.Client.LegalGuardian.Address;
                model.LegalGuardianTelephone = tcmClient.Client.LegalGuardian.Telephone;
                model.LegalGuardianCity = tcmClient.Client.LegalGuardian.City;
                model.LegalGuardianState = tcmClient.Client.LegalGuardian.State;
                model.LegalGuardianZipCode = tcmClient.Client.LegalGuardian.ZipCode;
                model.IdRelationshipLG = Convert.ToInt32(tcmClient.Client.RelationShipOfLegalGuardian);
            }
            if (tcmClient.Client.EmergencyContact != null)
            {
                model.EmergencyContacTelephone = tcmClient.Client.EmergencyContact.Telephone;
                model.EmergencyContactName = tcmClient.Client.EmergencyContact.Name;
                model.IdRelationshipEC = Convert.ToInt32(tcmClient.Client.RelationShipOfEmergencyContact);
            }
            if (tcmClient.Client.Clients_HealthInsurances.Count() > 0)
            {
                model.HealthMemberId = tcmClient.Client.Clients_HealthInsurances.ElementAtOrDefault(0).MemberId;
                model.IdHealthPlan = tcmClient.Client.Clients_HealthInsurances.ElementAtOrDefault(0).HealthInsurance.Id;
            }
            ViewData["origi"] = origi;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Create(TCMIntakeFormViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                EmergencyContactEntity emergency = _context.EmergencyContacts.FirstOrDefault(n => (n.Name == IntakeViewModel.EmergencyContactName
                                                                && n.Telephone == IntakeViewModel.EmergencyContacTelephone));
                
                LegalGuardianEntity legalGuardian = _context.LegalGuardians.FirstOrDefault(n => (n.Name == IntakeViewModel.LegalGuardianName
                                                                && n.Telephone == IntakeViewModel.LegalGuardianTelephone));

                TCMClientEntity tcmclient = _context.TCMClient
                                                    .Include(n => n.Client)
                                                    .FirstOrDefault(m => m.Id == IntakeViewModel.IdTCMClient);

                ClientEntity client = _context.Clients.FirstOrDefault(n => n.Id == tcmclient.Client.Id);

                Client_HealthInsurance client_health = _context.Clients_HealthInsurances
                                                               .FirstOrDefault(n => (n.Client.Id == tcmclient.Client.Id
                                                                        && n.Active == true
                                                                        && n.HealthInsurance.Id == IntakeViewModel.IdHealthPlan));

                if (emergency == null)
                {
                    emergency = new EmergencyContactEntity();
                    emergency.Name = IntakeViewModel.EmergencyContactName;
                    emergency.Telephone = IntakeViewModel.EmergencyContacTelephone;
                    emergency.CreatedOn = DateTime.Now;
                    emergency.CreatedBy = user_logged.Id;
                    _context.EmergencyContacts.Add(emergency);
                    client.EmergencyContact = emergency;
                    client.RelationShipOfEmergencyContact = RelationshipUtils.GetRelationshipByIndex(IntakeViewModel.IdRelationshipEC);
                    _context.Clients.Update(client);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    client.EmergencyContact = emergency;
                    client.RelationShipOfEmergencyContact = RelationshipUtils.GetRelationshipByIndex(IntakeViewModel.IdRelationshipEC);
                    _context.Clients.Update(client);
                    await _context.SaveChangesAsync();
                }

                if (legalGuardian == null && IntakeViewModel.LegalGuardianName != null)
                {
                    legalGuardian = new LegalGuardianEntity();
                    legalGuardian.Name = IntakeViewModel.LegalGuardianName;
                    legalGuardian.Telephone = IntakeViewModel.LegalGuardianTelephone;
                    legalGuardian.Address = IntakeViewModel.LegalGuardianAddress;
                    legalGuardian.City = IntakeViewModel.LegalGuardianCity;
                    legalGuardian.State = IntakeViewModel.LegalGuardianState;
                    legalGuardian.ZipCode = IntakeViewModel.LegalGuardianZipCode;
                    legalGuardian.CreatedOn = DateTime.Now;
                    legalGuardian.CreatedBy = user_logged.Id;
                    _context.LegalGuardians.Add(legalGuardian);
                    client.LegalGuardian = legalGuardian;

                    client.RelationShipOfLegalGuardian = RelationshipUtils.GetRelationshipByIndex(IntakeViewModel.IdRelationshipLG);
                    _context.Clients.Update(client);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    client.LegalGuardian = legalGuardian;
                    client.RelationShipOfEmergencyContact = RelationshipUtils.GetRelationshipByIndex(IntakeViewModel.IdRelationshipEC);
                    _context.Clients.Update(client);
                    await _context.SaveChangesAsync();
                }

                if (client_health != null && client_health.MemberId != IntakeViewModel.HealthMemberId)
                {
                    client_health.MemberId = IntakeViewModel.HealthMemberId;
                    client_health.LastModifiedBy = user_logged.Id;
                    client_health.LastModifiedOn = DateTime.Now;
                    _context.Clients_HealthInsurances.Update(client_health);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    if (client_health == null && IntakeViewModel.IdHealthPlan > 0)
                    {
                        Client_HealthInsurance client_healthActive = _context.Clients_HealthInsurances
                                                                             .FirstOrDefault(n => (n.Client.Id == tcmclient.Client.Id
                                                                                 && n.Active == true));
                        if (client_healthActive != null)
                        {
                            client_healthActive.Active = false;
                            _context.Clients_HealthInsurances.Update(client_healthActive);
                            await _context.SaveChangesAsync();
                        }

                        client_health = new Client_HealthInsurance();
                        client_health.Active = true;
                        client_health.ApprovedDate = DateTime.Now;
                        client_health.Client = client;
                        client_health.CreatedBy = user_logged.Id;
                        client_health.CreatedOn = DateTime.Now;
                        client_health.MemberId = IntakeViewModel.HealthMemberId;
                        client_health.Units = 0;
                        client_health.HealthInsurance = await _context.HealthInsurances.FirstOrDefaultAsync(hi => hi.Id == IntakeViewModel.IdHealthPlan);

                        _context.Clients_HealthInsurances.Add(client_health);
                        await _context.SaveChangesAsync();

                    }
                }

                TCMIntakeFormEntity IntakeEntity = _context.TCMIntakeForms.Find(IntakeViewModel.Id);
                if (IntakeEntity == null)
                {
                    IntakeEntity = await _converterHelper.ToTCMIntakeFormEntity(IntakeViewModel, true,user_logged.UserName);
                    _context.TCMIntakeForms.Add(IntakeEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
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

            IntakeViewModel.TcmClient = _context.TCMClient
                                                .Include(m => m.Client)
                                                .ThenInclude(m => m.Clients_Diagnostics)
                                                .ThenInclude(m => m.Diagnostic)
                                                .Include(m => m.Client)
                                                .ThenInclude(m => m.Clients_HealthInsurances)
                                                .ThenInclude(m => m.HealthInsurance)
                                                .Include(m => m.Client.LegalGuardian)
                                                .Include(n => n.Client.EmergencyContact)
                                                .Include(n => n.Client.Client_Referred)
                                                .Include(n => n.Client.Doctor)
                                                .Include(n => n.Client.Psychiatrist)
                                                .FirstOrDefault(n => n.Id == IntakeViewModel.TcmClient_FK);

            IntakeViewModel.RelationshipsEC = _combosHelper.GetComboRelationships();
            IntakeViewModel.RelationshipsLG = _combosHelper.GetComboRelationships();

            return View(IntakeViewModel);
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> TCMIntakeDashboard(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            TCMClientEntity TcmClientEntity = await _context.TCMClient
                                                            //seccion1
                                                            .Include(c => c.TCMIntakeForm)
                                                            .Include(c => c.TcmIntakeConsentForTreatment)
                                                            .Include(n => n.TcmIntakeConsentForRelease)
                                                            .Include(n => n.TcmIntakeConsumerRights)
                                                            .Include(n => n.TcmIntakeAcknowledgementHipa)
                                                            .Include(n => n.TCMIntakeOrientationChecklist)
                                                            .Include(n => n.TCMIntakeAdvancedDirective)
                                                            .Include(n => n.TCMIntakeForeignLanguage)
                                                            .Include(n => n.TCMIntakeWelcome)
                                                            //seccion2
                                                            .Include(n => n.Client.IntakeFeeAgreement)
                                                            .Include(n => n.TCMIntakeNonClinicalLog)
                                                            //seccion3
                                                            .Include(n => n.TCMIntakeMedicalHistory)
                                                            .Include(n => n.Client.MedicationList)
                                                            .Include(n => n.TCMIntakeMiniMental)
                                                            .Include(n => n.TCMIntakeCoordinationCare)
                                                            //seccion4
                                                            //.Include(n => n.TcmServicePlan)
                                                            //.ThenInclude(n => n.TCMAdendum)
                                                            //.Include(n => n.TcmServicePlan.TCMServicePlanReview)
                                                            //.Include(n => n.TcmIntakeAppendixJ)
                                                            //.Include(n => n.TCMAssessment)
                                                            //seccion5
                                                            .Include(n => n.TcmInterventionLog)
                                                            .Include(n => n.TCMFarsFormList)
                                                            //seccion6
                                                            //.Include(n => n.TCMNote)

                                                            .AsSplitQuery()
                                                            .FirstOrDefaultAsync(c => c.Id == id);

           
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

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            TCMClientEntity TcmClientEntity = await _context.TCMClient
                                                            //seccion1
                                                            .Include(c => c.TCMIntakeForm)
                                                            .Include(c => c.TcmIntakeConsentForTreatment)
                                                            .Include(n => n.TcmIntakeConsentForRelease)
                                                            .Include(n => n.TcmIntakeConsumerRights)
                                                            .Include(n => n.TcmIntakeAcknowledgementHipa)
                                                            .Include(n => n.TCMIntakeOrientationChecklist)
                                                            .Include(n => n.TCMIntakeAdvancedDirective)
                                                            .Include(n => n.TCMIntakeForeignLanguage)
                                                            .Include(n => n.TCMIntakeWelcome)
                                                            //seccion2
                                                            .Include(n => n.Client.IntakeFeeAgreement)
                                                            .Include(n => n.TCMIntakeNonClinicalLog)
                                                            //seccion3
                                                            .Include(n => n.TCMIntakeMedicalHistory)
                                                            .Include(n => n.Client.MedicationList)
                                                            .Include(n => n.TCMIntakeMiniMental)
                                                            .Include(n => n.TCMIntakeCoordinationCare)
                                                            //seccion4
                                                            //.Include(n => n.TcmServicePlan)
                                                            //.ThenInclude(n => n.TCMAdendum)
                                                            //.Include(n => n.TcmServicePlan.TCMServicePlanReview)
                                                            //.Include(n => n.TcmIntakeAppendixJ)
                                                            //.Include(n => n.TCMAssessment)
                                                            //seccion5
                                                            .Include(n => n.TcmInterventionLog)
                                                            .Include(n => n.TCMFarsFormList)
                                                            //seccion6
                                                            //.Include(n => n.TCMNote)

                                                            .AsSplitQuery()

                                                            .FirstOrDefaultAsync(c => c.Id == id);


            if (TcmClientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            return View(TcmClientEntity);
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult Edit(int id = 0, int origi = 0)
        {
            TCMIntakeFormEntity entity = _context.TCMIntakeForms
                                                 .Include(m => m.TcmClient)
                                                 .ThenInclude(m => m.Client)
                                                 .ThenInclude(m => m.Clients_Diagnostics)
                                                 .ThenInclude(m => m.Diagnostic)
                                                 .Include(m => m.TcmClient)
                                                 .ThenInclude(m => m.Client)
                                                 .ThenInclude(m => m.Clients_HealthInsurances.Where(m => m.Active == true))
                                                 .ThenInclude(m => m.HealthInsurance)
                                                 .Include(m => m.TcmClient.Client.LegalGuardian)
                                                 .Include(n => n.TcmClient.Client.EmergencyContact)
                                                 .Include(n => n.TcmClient.Client.Client_Referred)
                                                 .Include(n => n.TcmClient.Client.Doctor)
                                                 .Include(n => n.TcmClient.Client.Psychiatrist)
                                                 .Include(n => n.TcmClient.Casemanager)
                                                 .FirstOrDefault(i => i.TcmClient.Id == id);
            if (entity == null)
            {
                return RedirectToAction("Create", new { id = id });
            }

            TCMIntakeFormViewModel model;

            if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor") || User.IsInRole("Manager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {
                    model = _converterHelper.ToTCMIntakeFormViewModel(entity);

                    if (entity.TcmClient.Client.LegalGuardian != null)
                    {
                        model.LegalGuardianName = entity.TcmClient.Client.LegalGuardian.Name;
                        model.LegalGuardianTelephone = entity.TcmClient.Client.LegalGuardian.Telephone;
                        model.LegalGuardianAddress = entity.TcmClient.Client.LegalGuardian.Address;
                        model.LegalGuardianState = entity.TcmClient.Client.LegalGuardian.State;
                        model.LegalGuardianCity = entity.TcmClient.Client.LegalGuardian.City;
                        model.LegalGuardianZipCode = entity.TcmClient.Client.LegalGuardian.ZipCode;

                    }
                    if (entity.TcmClient.Client.EmergencyContact != null)
                    {
                        model.EmergencyContactName = entity.TcmClient.Client.EmergencyContact.Name;
                        model.EmergencyContacTelephone = entity.TcmClient.Client.EmergencyContact.Telephone;

                    }
                    if (entity.TcmClient.Client.Clients_HealthInsurances.Count > 0)
                    {
                        model.HealthMemberId = entity.TcmClient.Client.Clients_HealthInsurances.ElementAtOrDefault(0).MemberId;
                        model.IdHealthPlan = entity.TcmClient.Client.Clients_HealthInsurances.FirstOrDefault(n => n.Active == true).HealthInsurance.Id;

                    }
                    if (model.TcmClient.Client.Clients_Diagnostics.Count() == 0)
                    {
                        Client_Diagnostic diagnostic = new Client_Diagnostic();
                        diagnostic.Diagnostic = new DiagnosticEntity();
                        diagnostic.Diagnostic.Code = "";
                        diagnostic.Diagnostic.Description = "";
                        model.TcmClient.Client.Clients_Diagnostics.Add(diagnostic);
                    }

                    model.IdRelationshipLG = Convert.ToInt32(entity.TcmClient.Client.RelationShipOfLegalGuardian);
                    model.RelationshipsLG = _combosHelper.GetComboRelationships();

                    model.IdRelationshipEC = Convert.ToInt32(entity.TcmClient.Client.RelationShipOfEmergencyContact);
                    model.RelationshipsEC = _combosHelper.GetComboRelationships();

                    model.HealthPlan = _combosHelper.GetComboActiveInsurancesByClinic(user_logged.Clinic.Id);

                    ViewData["origi"] = origi;
                    return View(model);
                }
            }

            model = new TCMIntakeFormViewModel();
            ViewData["origi"] = origi;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public async Task<IActionResult> Edit(TCMIntakeFormViewModel intakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                EmergencyContactEntity emergency = _context.EmergencyContacts.FirstOrDefault(n => (n.Name == intakeViewModel.EmergencyContactName
                                                               && n.Telephone == intakeViewModel.EmergencyContacTelephone));

                LegalGuardianEntity legalGuardian = _context.LegalGuardians.FirstOrDefault(n => (n.Name == intakeViewModel.LegalGuardianName
                                                               && n.Telephone == intakeViewModel.LegalGuardianTelephone));

                TCMClientEntity tcmclient = _context.TCMClient
                                                    .Include(n => n.Client)
                                                    .FirstOrDefault(m => m.Id == intakeViewModel.IdTCMClient);

                ClientEntity client = _context.Clients.FirstOrDefault(n => n.Id == tcmclient.Client.Id);

                Client_HealthInsurance client_health = _context.Clients_HealthInsurances
                                                               .FirstOrDefault(n => (n.Client.Id == tcmclient.Client.Id
                                                                        && n.Active == true
                                                                        && n.HealthInsurance.Id == intakeViewModel.IdHealthPlan));

                if (emergency == null)
                {
                    emergency = new EmergencyContactEntity();
                    emergency.Name = intakeViewModel.EmergencyContactName;
                    emergency.Telephone = intakeViewModel.EmergencyContacTelephone;
                    emergency.CreatedOn = DateTime.Now;
                    emergency.CreatedBy = user_logged.Id;
                    _context.EmergencyContacts.Add(emergency);
                    client.EmergencyContact = emergency;
                    client.RelationShipOfEmergencyContact = RelationshipUtils.GetRelationshipByIndex(intakeViewModel.IdRelationshipEC);
                    _context.Clients.Update(client);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    client.EmergencyContact = emergency;
                    client.RelationShipOfEmergencyContact = RelationshipUtils.GetRelationshipByIndex(intakeViewModel.IdRelationshipEC);
                    _context.Clients.Update(client);
                    await _context.SaveChangesAsync();
                }

                if (legalGuardian == null && intakeViewModel.LegalGuardianName != null)
                {
                    legalGuardian = new LegalGuardianEntity();
                    legalGuardian.Name = intakeViewModel.LegalGuardianName;
                    legalGuardian.Telephone = intakeViewModel.LegalGuardianTelephone;
                    legalGuardian.Address = intakeViewModel.LegalGuardianAddress;
                    legalGuardian.City = intakeViewModel.LegalGuardianCity;
                    legalGuardian.State = intakeViewModel.LegalGuardianState;
                    legalGuardian.ZipCode = intakeViewModel.LegalGuardianZipCode;
                    legalGuardian.CreatedOn = DateTime.Now;
                    legalGuardian.CreatedBy = user_logged.Id;
                    _context.LegalGuardians.Add(legalGuardian);
                    client.LegalGuardian = legalGuardian;

                    client.RelationShipOfLegalGuardian = RelationshipUtils.GetRelationshipByIndex(intakeViewModel.IdRelationshipLG);
                    _context.Clients.Update(client);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    client.LegalGuardian = legalGuardian;
                    client.RelationShipOfEmergencyContact = RelationshipUtils.GetRelationshipByIndex(intakeViewModel.IdRelationshipEC);
                    _context.Clients.Update(client);
                    await _context.SaveChangesAsync();
                }

                if (client_health != null && client_health.MemberId != intakeViewModel.HealthMemberId)
                {
                    client_health.MemberId = intakeViewModel.HealthMemberId;
                    client_health.LastModifiedBy = user_logged.Id;
                    client_health.LastModifiedOn = DateTime.Now;
                    _context.Clients_HealthInsurances.Update(client_health);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    if (client_health == null && intakeViewModel.IdHealthPlan > 0)
                    {
                        Client_HealthInsurance client_healthActive = _context.Clients_HealthInsurances
                                                                             .FirstOrDefault(n => (n.Client.Id == tcmclient.Client.Id
                                                                                 && n.Active == true));
                        if (client_healthActive != null)
                        {
                            client_healthActive.Active = false;
                            _context.Clients_HealthInsurances.Update(client_healthActive);
                            await _context.SaveChangesAsync();
                        }

                        client_health = new Client_HealthInsurance();
                        client_health.Active = true;
                        client_health.ApprovedDate = DateTime.Now;
                        client_health.Client = client;
                        client_health.CreatedBy = user_logged.Id;
                        client_health.CreatedOn = DateTime.Now;
                        client_health.MemberId = intakeViewModel.HealthMemberId;
                        client_health.Units = 0;
                        client_health.HealthInsurance = await _context.HealthInsurances.FirstOrDefaultAsync(hi => hi.Id == intakeViewModel.IdHealthPlan);

                        _context.Clients_HealthInsurances.Add(client_health);
                        await _context.SaveChangesAsync();

                    }
                }

                TCMIntakeFormEntity intakeEntity = await _converterHelper.ToTCMIntakeFormEntity(intakeViewModel, false, user_logged.UserName);
                _context.TCMIntakeForms.Update(intakeEntity);

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("TCMIntakeSectionDashboard", new { id = intakeViewModel.IdTCMClient, section = 1, origin = origi });
                    
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
                                                          .Include(n => n.Client.Client_Referred)
                                                          .Include(n => n.Client.Doctor)
                                                          .Include(n => n.Client.Psychiatrist)
                                                          .FirstOrDefaultAsync(n => n.Id == intakeViewModel.IdTCMClient);

                intakeViewModel.RelationshipsEC = _combosHelper.GetComboRelationships();
                intakeViewModel.RelationshipsLG = _combosHelper.GetComboRelationships();

            }

            ViewData["origi"] = origi;
            return View(intakeViewModel);
           
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult CreateTCMConsentForTreatment(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeConsentForTreatmentViewModel model;
            TCMIntakeConsentForTreatmentEntity intakeConsent = _context.TCMIntakeConsentForTreatment
                                                                       .Include(n => n.TcmClient)
                                                                       .ThenInclude(n => n.Client)
                                                                       .ThenInclude(n => n.LegalGuardian)
                                                                       .FirstOrDefault(n => n.TcmClient.Id == id);

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(d => d.Client)
                                                .ThenInclude(d => d.LegalGuardian)
                                                .FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                   
                    if (intakeConsent == null)
                    {
                        model = new TCMIntakeConsentForTreatmentViewModel
                        {
                            TcmClient = tcmClient,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            Aggre = true,
                            Aggre1 = true,
                            AuthorizeRelease = true,
                            AuthorizeStaff = true,
                            Certify = false,
                            Certify1 = true,
                            DateSignatureEmployee = tcmClient.DataOpen,
                            DateSignatureLegalGuardian = tcmClient.DataOpen,
                            DateSignaturePerson = tcmClient.DataOpen,
                            Documents = true,
                            Id = 0,
                            Underestand = true,
                            IdTCMClient = id,
                            Client_FK = id,
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        if (intakeConsent.TcmClient.Client.LegalGuardian == null)
                            intakeConsent.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeConsentForTreatmentViewModel(intakeConsent);
                        ViewData["origi"] = origi;
                        return View(model);
                    }

                }
            }
            if (User.IsInRole("TCMSupervisor") || User.IsInRole("Manager"))
            {
                if (user_logged.Clinic != null)
                {

                    if (intakeConsent != null)
                    {
                        if (intakeConsent.TcmClient.Client.LegalGuardian == null)
                            intakeConsent.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeConsentForTreatmentViewModel(intakeConsent);
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                  
                }
            }

            return RedirectToAction("Index", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMConsentForTreatment(TCMIntakeConsentForTreatmentViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeConsentForTreatmentEntity IntakeConsentEntity = _converterHelper.ToTCMIntakeConsentForTreatmentEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeConsentEntity.Id == 0)
                {
                    IntakeConsentEntity.TcmClient = null;
                    _context.TCMIntakeConsentForTreatment.Add(IntakeConsentEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
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

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult CreateTCMConsentForRelease(int id = 0, int origi = 0, string Name = " ", string address = " ", string city = " ", string phone = " ", string fax = " ", int idType = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeConsentForReleaseViewModel model;
            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.LegalGuardian)
                                                .FirstOrDefault(n => n.Id == id);
            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    model = new TCMIntakeConsentForReleaseViewModel
                    {
                        TcmClient = tcmClient,
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
                        DateSignatureEmployee = tcmClient.DataOpen,
                        DateSignatureLegalGuardian = tcmClient.DataOpen,
                        DateSignaturePerson = tcmClient.DataOpen,
                        AdmissionedFor = user_logged.FullName,
                        NameOfFacility = Name,
                        Address = address,
                        CityStateZip = city,
                        PhoneNo = phone,
                        FaxNo = fax,
                        Idtype = idType,
                        ConsentList = _combosHelper.GetComboConsentType(),
                        OtherPurposeRequest = string.Empty,
                        OtherAutorizedInformation = string.Empty,
                        InForm_Electronic = false,
                        InForm_AllofTheAbove = false
                    };
                   if (model.TcmClient.Client.LegalGuardian == null)
                       model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                   
                    ViewData["origi"] = origi;
                    return View(model);
                
                }
            }

            return RedirectToAction("TCMIntakeDashboard", new { id = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMConsentForRelease(TCMIntakeConsentForReleaseViewModel IntakeViewModel, int origi = 0)
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
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
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
                        return RedirectToAction("ListConsentForrelease", new { id = IntakeViewModel.IdTCMClient, origi = origi });
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

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult CreateTCMConsumerRights(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeConsumerRightsViewModel model;
            TCMIntakeConsumerRightsEntity intakeConsent = _context.TCMIntakeConsumerRights
                                                                  .Include(n => n.TcmClient)
                                                                  .ThenInclude(n => n.Client)
                                                                  .ThenInclude(n => n.LegalGuardian)
                                                                  .FirstOrDefault(n => n.TcmClient.Id == id);

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.LegalGuardian)
                                                .FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    
                    if (intakeConsent == null)
                    {
                        model = new TCMIntakeConsumerRightsViewModel
                        {
                            TcmClient = tcmClient,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            IdTCMClient = id,
                            TcmClient_FK = id,
                            Id = 0,
                            ServedOf = user_logged.FullName,
                            Documents = true,
                            DateSignatureEmployee = tcmClient.DataOpen,
                            DateSignatureLegalGuardian = tcmClient.DataOpen,
                            DateSignaturePerson = tcmClient.DataOpen,
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();

                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        if (intakeConsent.TcmClient.Client.LegalGuardian == null)
                            intakeConsent.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeConsumerRightsViewModel(intakeConsent);
                        ViewData["origi"] = origi;
                        return View(model);
                    }

                }
            }
            if (User.IsInRole("TCMSupervisor") || User.IsInRole("Manager"))
            {
                if (user_logged.Clinic != null)
                {
                    if (intakeConsent.TcmClient.Client.LegalGuardian == null)
                        intakeConsent.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                    model = _converterHelper.ToTCMIntakeConsumerRightsViewModel(intakeConsent);
                    ViewData["origi"] = origi;
                    return View(model);
                }
            }

            return RedirectToAction("Index", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMConsumerRights(TCMIntakeConsumerRightsViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeConsumerRightsEntity IntakeConsumerEntity = _converterHelper.ToTCMIntakeConsumerRightsEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeConsumerEntity.Id == 0)
                {
                    IntakeConsumerEntity.TcmClient = null;
                    _context.TCMIntakeConsumerRights.Add(IntakeConsumerEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
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
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
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

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult CreateTCMAcknowledgementHippa(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeAcknoewledgementHippaViewModel model;
            TCMIntakeAcknowledgementHippaEntity intakeAck = _context.TCMIntakeAcknowledgement
                                                                            .Include(n => n.TcmClient)
                                                                            .ThenInclude(n => n.Client)
                                                                            .ThenInclude(n => n.LegalGuardian)
                                                                            .FirstOrDefault(n => n.TcmClient.Id == id);

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(d => d.Client)
                                                .ThenInclude(d => d.LegalGuardian)
                                                .FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    if (intakeAck == null)
                    {

                        model = new TCMIntakeAcknoewledgementHippaViewModel
                        {
                            TcmClient = tcmClient,
                            DateSignatureEmployee = tcmClient.DataOpen,
                            DateSignatureLegalGuardian = tcmClient.DataOpen,
                            DateSignaturePerson = tcmClient.DataOpen,
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
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        if (intakeAck.TcmClient.Client.LegalGuardian == null)
                            intakeAck.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeAcknoewledgementHippaViewModel(intakeAck);
                        ViewData["origi"] = origi;
                        return View(model);
                    }

                }
            }
            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {
                    if (intakeAck.TcmClient.Client.LegalGuardian == null)
                        intakeAck.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                    model = _converterHelper.ToTCMIntakeAcknoewledgementHippaViewModel(intakeAck);
                    ViewData["origi"] = origi;
                    return View(model);

                }
            }

            return RedirectToAction("Index", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMAcknowledgementHippa(TCMIntakeAcknoewledgementHippaViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeAcknowledgementHippaEntity IntakeAckNowEntity = _converterHelper.ToTCMIntakeAcknoewledgementHippaEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeAckNowEntity.Id == 0)
                {
                    IntakeAckNowEntity.TcmClient = null;
                    _context.TCMIntakeAcknowledgement.Add(IntakeAckNowEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
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
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
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

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult CreateTCMOrientationCheckList(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeOrientationCheckListViewModel model;
            TCMIntakeOrientationChecklistEntity intakeCheckList = _context.TCMIntakeOrientationCheckList
                                                                          .Include(n => n.TcmClient)
                                                                          .ThenInclude(n => n.Client)
                                                                          .ThenInclude(n => n.LegalGuardian)
                                                                          .FirstOrDefault(n => n.TcmClient.Id == id);

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.LegalGuardian)
                                                .FirstOrDefault(n => n.Id == id);


            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    if (intakeCheckList == null)
                    {
                        model = new TCMIntakeOrientationCheckListViewModel
                        {
                            TcmClient = tcmClient,
                            IdTcmClient = id,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            TcmClient_FK = id,
                            Id = 0,
                            DateSignatureEmployee = tcmClient.DataOpen,
                            DateSignatureLegalGuardian = tcmClient.DataOpen,
                            DateSignaturePerson = tcmClient.DataOpen,
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
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        if (intakeCheckList.TcmClient.Client.LegalGuardian == null)
                            intakeCheckList.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeOrientationChecklistViewModel(intakeCheckList);
                        ViewData["origi"] = origi;
                        return View(model);
                    }

                }
            }
            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {
                    if (intakeCheckList.TcmClient.Client.LegalGuardian == null)
                        intakeCheckList.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                    model = _converterHelper.ToTCMIntakeOrientationChecklistViewModel(intakeCheckList);
                    ViewData["origi"] = origi;
                    return View(model);
                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMOrientationCheckList(TCMIntakeOrientationCheckListViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeOrientationChecklistEntity IntakeOrientationEntity = _converterHelper.ToTCMIntakeOrientationChecklistEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeOrientationEntity.Id == 0)
                {
                    IntakeOrientationEntity.TcmClient = null;
                    _context.TCMIntakeOrientationCheckList.Add(IntakeOrientationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTcmClient, section = 1, origin = origi });
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
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTcmClient, section = 1, origin = origi });
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

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult CreateTCMIntakeAdvenceDirective(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeAdvancedDirectiveViewModel model;
            TCMIntakeAdvancedDirectiveEntity AdvancedDirective = _context.TCMIntakeAdvancedDirective
                                                                         .Include(n => n.TcmClient)
                                                                         .ThenInclude(n => n.Client)
                                                                         .ThenInclude(n => n.LegalGuardian)
                                                                         .Include(n => n.TcmClient.Client.EmergencyContact)
                                                                         .FirstOrDefault(n => n.TcmClient.Id == id);

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.LegalGuardian)
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.EmergencyContact)
                                                .FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    if (AdvancedDirective == null)
                    {
                        model = new TCMIntakeAdvancedDirectiveViewModel
                        {
                            TcmClient = tcmClient,
                            IdTCMClient = id,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            TcmClient_FK = id,
                            Id = 0,
                            Documents = true,
                            DateSignatureEmployee = tcmClient.DataOpen,
                            DateSignatureLegalGuardian = tcmClient.DataOpen,
                            DateSignaturePerson = tcmClient.DataOpen,
                            AdmissionedFor = user_logged.FullName,
                            IHaveNot = true
                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        if (model.TcmClient.Client.EmergencyContact == null)
                        {
                            model.TcmClient.Client.EmergencyContact = new EmergencyContactEntity();
                            model.TcmClient.Client.EmergencyContact.Name = "N/A";
                        }
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        if (AdvancedDirective.TcmClient.Client.LegalGuardian == null)
                            AdvancedDirective.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        if (AdvancedDirective.TcmClient.Client.EmergencyContact == null)
                        {
                            AdvancedDirective.TcmClient.Client.EmergencyContact = new EmergencyContactEntity();
                            AdvancedDirective.TcmClient.Client.EmergencyContact.Name = "N/A";
                        }
                        model = _converterHelper.ToTCMIntakeAdvancedDirectiveViewModel(AdvancedDirective);
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                }
            }
            if (User.IsInRole("TCMSupervisor") || User.IsInRole("Manager"))
            {
                if (user_logged.Clinic != null)
                {
                    if (AdvancedDirective.TcmClient.Client.LegalGuardian == null)
                        AdvancedDirective.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                    if (AdvancedDirective.TcmClient.Client.EmergencyContact == null)
                    {
                        AdvancedDirective.TcmClient.Client.EmergencyContact = new EmergencyContactEntity();
                        AdvancedDirective.TcmClient.Client.EmergencyContact.Name = "N/A";
                    }
                    model = _converterHelper.ToTCMIntakeAdvancedDirectiveViewModel(AdvancedDirective);
                    ViewData["origi"] = origi;
                    return View(model);
                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMIntakeAdvenceDirective(TCMIntakeAdvancedDirectiveViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeAdvancedDirectiveEntity IntakeAdvancedDirectiveEntity = _converterHelper.ToTCMIntakeAdvancedDirectiveEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeAdvancedDirectiveEntity.Id == 0)
                {
                    IntakeAdvancedDirectiveEntity.TcmClient = null;
                    _context.TCMIntakeAdvancedDirective.Add(IntakeAdvancedDirectiveEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeAdvancedDirectiveEntity.TcmClient = null;
                    _context.TCMIntakeAdvancedDirective.Update(IntakeAdvancedDirectiveEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
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

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public async Task<IActionResult> ListConsentForrelease(int id = 0, int origi = 0)
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
             
                ViewData["origi"] = origi;
                ViewData["idTCMCliente"] = id;
                return View(listRelease);
            }
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult EditConsentForRelease(int id = 0, int origi = 0)
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
                                                              .Include(n => n.TcmClient.Client.Client_Referred)
                                                              .Include(n => n.TcmClient.Client.Doctor)
                                                              .Include(n => n.TcmClient.Client.Psychiatrist)
                                                              .FirstOrDefault(i => i.Id == id);
            if (entity == null)
            {
                return RedirectToAction("EditConsentForRelease", new { id = id });
            }

            TCMIntakeConsentForReleaseEntity model;

            if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor") || User.IsInRole("Manager"))
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
                    ViewData["origi"] = origi;
                    return View(model);
                }
            }

            model = new TCMIntakeConsentForReleaseEntity();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> EditConsentForRelease(TCMIntakeConsentForReleaseViewModel intakeViewModel, int origi = 0)
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
                    return RedirectToAction("ListConsentForrelease", new { id = intakeViewModel.IdTCMClient, origi = origi });
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
                                                          .Include(n => n.Client.Client_Referred)
                                                          .Include(n => n.Client.Doctor)
                                                          .Include(n => n.Client.Psychiatrist)
                                                          .FirstOrDefaultAsync(n => n.Id == intakeViewModel.IdTCMClient);


            }

            ViewData["origi"] = origi;
            return View(intakeViewModel);

        }

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult CreateTCMForeignLanguage(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeForeignLanguageViewModel model;
            TCMIntakeForeignLanguageEntity intakeForeign = _context.TCMIntakeForeignLanguage
                                                                   .Include(n => n.TcmClient)
                                                                   .ThenInclude(n => n.Client)
                                                                   .ThenInclude(n => n.LegalGuardian)
                                                                   .FirstOrDefault(n => n.TcmClient.Id == id);

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(d => d.Client)
                                                .ThenInclude(d => d.LegalGuardian)
                                                .FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    if (intakeForeign == null)
                    {

                        model = new TCMIntakeForeignLanguageViewModel
                        {
                            TcmClient = tcmClient,
                            DateSignatureEmployee = tcmClient.DataOpen,
                            DateSignatureLegalGuardian = tcmClient.DataOpen,
                            DateSignaturePerson = tcmClient.DataOpen,
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
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        if (intakeForeign.TcmClient.Client.LegalGuardian == null)
                            intakeForeign.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeForeignLanguageViewModel(intakeForeign);
                        ViewData["origi"] = origi;
                        return View(model);
                    }

                }
            }
            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {
                    if (intakeForeign.TcmClient.Client.LegalGuardian == null)
                        intakeForeign.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                    model = _converterHelper.ToTCMIntakeForeignLanguageViewModel(intakeForeign);
                    ViewData["origi"] = origi;
                    return View(model);
                }
            }
            return RedirectToAction("Index", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMForeignLanguage(TCMIntakeForeignLanguageViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeForeignLanguageEntity IntakeForeignEntity = _converterHelper.ToTCMIntakeForeignLanguageEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeForeignEntity.Id == 0)
                {
                    IntakeForeignEntity.TcmClient = null;
                    _context.TCMIntakeForeignLanguage.Add(IntakeForeignEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
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
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
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

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult CreateTCMWelcome(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeWelcomeViewModel model;
            TCMIntakeWelcomeEntity intakeForeign = _context.TCMIntakeWelcome
                                                           .Include(n => n.TcmClient)
                                                           .ThenInclude(n => n.Client)
                                                           .ThenInclude(n => n.LegalGuardian)
                                                           .Include(n => n.TcmClient)
                                                           .ThenInclude(n => n.Casemanager)
                                                           .ThenInclude(n => n.Clinic)
                                                           .FirstOrDefault(n => n.TcmClient.Id == id);

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(d => d.Client)
                                                .ThenInclude(d => d.LegalGuardian)
                                                .Include(d => d.Casemanager)
                                                .ThenInclude(d => d.Clinic)
                                                .FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    if (intakeForeign == null)
                    {

                        model = new TCMIntakeWelcomeViewModel
                        {
                            TcmClient = tcmClient,
                            Date = tcmClient.DataOpen,
                            Id = 0,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            IdTCMClient = id,
                            TcmClient_FK = id,
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        if (intakeForeign.TcmClient.Client.LegalGuardian == null)
                            intakeForeign.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeWelcomeViewModel(intakeForeign);
                        ViewData["origi"] = origi;
                        return View(model);
                    }

                }
            }
            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {
                    if (intakeForeign.TcmClient.Client.LegalGuardian == null)
                        intakeForeign.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                    model = _converterHelper.ToTCMIntakeWelcomeViewModel(intakeForeign);
                    ViewData["origi"] = origi;
                    return View(model);
                }
            }
            return RedirectToAction("Index", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMWelcome(TCMIntakeWelcomeViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeWelcomeEntity IntakeForeignEntity = _converterHelper.ToTCMIntakeWelcomeEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeForeignEntity.Id == 0)
                {
                    IntakeForeignEntity.TcmClient = null;
                    _context.TCMIntakeWelcome.Add(IntakeForeignEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
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
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
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

        [Authorize(Roles = "CaseManager,TCMSupervisor, Manager")]
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
                                                    .AsSplitQuery()
                                                    .FirstOrDefaultAsync(n => n.Id == id);

                TCMDocumentViewModel model = new TCMDocumentViewModel()
                {
                    Id = id,
                    IdTCMClient = idTCMCLient,
                    Documents = Client.Documents
                };

                ViewData["createBy"] = user_logged.Id;
                return View(model);
            }
        }

        [Authorize(Roles = "CaseManager,TCMSupervisor, Manager")]
        public IActionResult AddDocument(int id = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            DocumentViewModel entity = new DocumentViewModel()
            {
                IdDescription = 0,
                Descriptions = _combosHelper.GetComboDocumentDescriptions(),
                Client = _context.Clients.Find(id)
            };

            ViewData["createBy"] = user_logged.Id;
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager,TCMSupervisor, Manager")]
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
                ViewData["createBy"] = user_logged.Id;
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDocument", documentList) });
            }

            DocumentViewModel salida = new DocumentViewModel()
            {
                IdDescription = 0,
                Descriptions = _combosHelper.GetComboDocumentDescriptions(),
                Client = _context.Clients.Find(id)
            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddDocument", salida) });
        }

         [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteDocumentTemp(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

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
            ViewData["createBy"] = user_logged.Id;
            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDocument", documentList) });
        }

        [Authorize(Roles = "CaseManager,TCMSupervisor, Manager")]
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

        [Authorize(Roles = "CaseManager,TCMSupervisor")]
        public IActionResult CreateTCMFeeAgreement(int id = 0, int idTCMCLient = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IntakeFeeAgreementViewModel model;

            DateTime date = _context.TCMClient
                                    .Include(d => d.Client)
                                    .ThenInclude(d => d.LegalGuardian)
                                    .Include(d => d.Casemanager)
                                    .ThenInclude(d => d.Clinic)
                                    .FirstOrDefault(n => n.Id == idTCMCLient).DataOpen;

            if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor"))
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
                            DateSignatureEmployee = date,
                            DateSignatureLegalGuardian = date,
                            DateSignaturePerson = date,
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
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> CreateTCMFeeAgreement(IntakeFeeAgreementViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IntakeFeeAgreementEntity IntakefeeAgreementEntity = _converterHelper.ToIntakeFeeAgreementEntity(IntakeViewModel, false);

                if (IntakefeeAgreementEntity.Id == 0)
                {
                    IntakefeeAgreementEntity.Client = null;
                    _context.IntakeFeeAgreement.Add(IntakefeeAgreementEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (User.IsInRole("CaseManager"))
                        {
                            return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 2 });
                        }
                        else
                        {
                            return RedirectToAction("TCMIntakeSectionDashboardReadOnly", new { id = IntakeViewModel.IdTCMClient, section = 2 });
                        }
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
                        if (User.IsInRole("CaseManager"))
                        {
                            return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 2 });
                        }
                        else 
                        {
                            return RedirectToAction("TCMIntakeSectionDashboardReadOnly", new { id = IntakeViewModel.IdTCMClient, section = 2 });
                        }
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

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult CreateTCMNonClinicalLog(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeNonClinicalLogViewModel model;

            if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor") || User.IsInRole("Manager"))
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

                    TCMClientEntity tcmClient = _context.TCMClient
                                                        .Include(d => d.Client)
                                                        .ThenInclude(d => d.LegalGuardian)
                                                        .Include(d => d.Casemanager)
                                                        .ThenInclude(d => d.Clinic)
                                                        .FirstOrDefault(n => n.Id == id);

                    if (intakeNonClinical == null)
                    {

                        model = new TCMIntakeNonClinicalLogViewModel
                        {
                            TcmClient = tcmClient,
                            Date = tcmClient.DataOpen,
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
        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public async Task<IActionResult> CreateTCMNonClinicalLog(TCMIntakeNonClinicalLogViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeNonClinicalLogEntity IntakeNonClinicalEntity = _converterHelper.ToTCMIntakeNonClinicalLogEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeNonClinicalEntity.Id == 0)
                {
                    IntakeNonClinicalEntity.TcmClient = null;
                    _context.TCMIntakeNonClinicalLog.Add(IntakeNonClinicalEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (User.IsInRole("CaseManager"))
                        {
                            return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 2 });
                        }
                        else
                        {
                            return RedirectToAction("TCMIntakeSectionDashboardReadOnly", new { id = IntakeViewModel.IdTCMClient, section = 2 });
                        }
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
                        if (User.IsInRole("CaseManager"))
                        {
                            return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 2 });
                        }
                        else
                        {
                            return RedirectToAction("TCMIntakeSectionDashboardReadOnly", new { id = IntakeViewModel.IdTCMClient, section = 2 });
                        }
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

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult CreateTCMMiniMental(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeMiniMentalViewModel model;

            if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor"))
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

                    TCMClientEntity tcmClient = _context.TCMClient
                                                        .Include(d => d.Client)
                                                        .ThenInclude(d => d.LegalGuardian)
                                                        .Include(d => d.Casemanager)
                                                        .ThenInclude(d => d.Clinic)
                                                        .FirstOrDefault(n => n.Id == id);

                    if (intakeMiniMental == null)
                    {

                        model = new TCMIntakeMiniMentalViewModel
                        {
                            TcmClient = tcmClient,
                            Date = tcmClient.DataOpen,
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
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> CreateTCMMiniMental(TCMIntakeMiniMentalViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeMiniMentalEntity IntakeMiniMenatalEntity = _converterHelper.ToTCMIntakeMiniMenatalEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeMiniMenatalEntity.Id == 0)
                {
                    IntakeMiniMenatalEntity.TcmClient = null;
                    _context.TCMIntakeMiniMental.Add(IntakeMiniMenatalEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (User.IsInRole("CaseManager"))
                        {
                            return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                        }
                        else
                        {
                            return RedirectToAction("TCMIntakeSectionDashboardReadOnly", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                        }
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
                        if (User.IsInRole("CaseManager"))
                        {
                            return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                        }
                        else
                        {
                            return RedirectToAction("TCMIntakeSectionDashboardReadOnly", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                        }
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

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult CreateTCMMedicalhistory(int id = 0, int idTCMClient = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeMedicalHistoryViewModel model;

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.Doctor)
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.LegalGuardian)
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.IntakeMedicalHistory)
                                                .FirstOrDefault(n => n.Id == idTCMClient);

            if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor") || User.IsInRole("Manager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeMedicalHistoryEntity intakeMedicalHistory = _context.TCMIntakeMedicalHistory
                                                                                 .Include(n => n.TCMClient)
                                                                                 .ThenInclude(n => n.Client)
                                                                                 .ThenInclude(n => n.LegalGuardian)
                                                                                 .FirstOrDefault(n => n.TCMClient.Id == idTCMClient);

                    TCMIntakeFormEntity intake = _context.TCMIntakeForms.FirstOrDefault(n => n.TcmClient_FK == tcmClient.Id);
                    DoctorEntity doctor = new DoctorEntity();

                    if (intake != null)
                    {
                        doctor.Address = intake.PCP_Address;
                        doctor.City = intake.PCP_CityStateZip;
                    }
                    if (intakeMedicalHistory == null)
                    {
                        if (tcmClient.Client.IntakeMedicalHistory == null)
                        {
                            model = new TCMIntakeMedicalHistoryViewModel
                            {
                                PrimaryCarePhysician = doctor.Name,
                                AddressPhysician = doctor.Address + doctor.City + doctor.State + doctor.ZipCode,
                               
                                TCMClient = _context.TCMClient.Include(n => n.Client).ThenInclude(n => n.LegalGuardian).FirstOrDefault(n => n.Id == idTCMClient),
                                TCMClient_FK = id,
                                Id = 0,
                                DateSignatureEmployee = tcmClient.DataOpen,
                                DateSignatureLegalGuardian = tcmClient.DataOpen,
                                DateSignaturePerson = tcmClient.DataOpen,
                                Documents = true,
                                
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
                                
                                AgeOfFirstMenstruation = "",
                                DateOfLastBreastExam = "",
                                DateOfLastPelvic = "",
                                DateOfLastPeriod = "",
                                UsualDurationOfPeriods = "",
                                UsualIntervalBetweenPeriods = "",
                                AdmissionedFor = user_logged.FullName

                            };
                            if (model.TCMClient.Client.LegalGuardian == null)
                                model.TCMClient.Client.LegalGuardian = new LegalGuardianEntity();
                            model.IdTCMClient = idTCMClient;
                            ViewData["CaseNumber"] = tcmClient.CaseNumber;
                            return View(model);
                        }
                        else
                        {
                            model = new TCMIntakeMedicalHistoryViewModel
                            {
                                PrimaryCarePhysician = doctor.Name,
                                AddressPhysician = doctor.Address + doctor.City + doctor.State + doctor.ZipCode,

                                TCMClient = _context.TCMClient.Include(n => n.Client).ThenInclude(n => n.LegalGuardian).FirstOrDefault(n => n.Id == idTCMClient),
                                TCMClient_FK = id,
                                Id = 0,
                                DateSignatureEmployee = tcmClient.DataOpen,
                                DateSignatureLegalGuardian = tcmClient.DataOpen,
                                DateSignaturePerson = tcmClient.DataOpen,
                                Documents = true,

                                AgeFirstTalked = tcmClient.Client.IntakeMedicalHistory.AgeFirstTalked,
                                AgeFirstWalked = tcmClient.Client.IntakeMedicalHistory.AgeFirstWalked,
                                AgeToiletTrained = tcmClient.Client.IntakeMedicalHistory.AgeToiletTrained,
                                AgeWeaned = tcmClient.Client.IntakeMedicalHistory.AgeWeaned,
                                Allergies = tcmClient.Client.IntakeMedicalHistory.Allergies,
                                Allergies_Describe = tcmClient.Client.IntakeMedicalHistory.Allergies_Describe,
                                AndOrSoiling = tcmClient.Client.IntakeMedicalHistory.AndOrSoiling,
                                Anemia = tcmClient.Client.IntakeMedicalHistory.Anemia,
                                AreYouCurrently = tcmClient.Client.IntakeMedicalHistory.AreYouCurrently,
                                AreYouPhysician = tcmClient.Client.IntakeMedicalHistory.AreYouPhysician,
                                Arthritis = tcmClient.Client.IntakeMedicalHistory.Arthritis,
                                AssumingCertainPositions = tcmClient.Client.IntakeMedicalHistory.AssumingCertainPositions,
                                BackPain = tcmClient.Client.IntakeMedicalHistory.BackPain,
                                BeingConfused = tcmClient.Client.IntakeMedicalHistory.BeingConfused,
                                BeingDisorientated = tcmClient.Client.IntakeMedicalHistory.BeingDisorientated,
                                BirthWeight = tcmClient.Client.IntakeMedicalHistory.BirthWeight,
                                BlackStools = tcmClient.Client.IntakeMedicalHistory.BlackStools,
                                BloodInUrine = tcmClient.Client.IntakeMedicalHistory.BloodInUrine,
                                BloodyStools = tcmClient.Client.IntakeMedicalHistory.BloodyStools,
                                BottleFedUntilAge = tcmClient.Client.IntakeMedicalHistory.BottleFedUntilAge,
                                BreastFed = tcmClient.Client.IntakeMedicalHistory.BreastFed,
                                BurningUrine = tcmClient.Client.IntakeMedicalHistory.BurningUrine,
                                Calculating = tcmClient.Client.IntakeMedicalHistory.Calculating,
                                Cancer = tcmClient.Client.IntakeMedicalHistory.Cancer,
                                ChestPain = tcmClient.Client.IntakeMedicalHistory.ChestPain,
                                ChronicCough = tcmClient.Client.IntakeMedicalHistory.ChronicCough,
                                ChronicIndigestion = tcmClient.Client.IntakeMedicalHistory.ChronicIndigestion,
                                City = doctor.City,
                                Complications = tcmClient.Client.IntakeMedicalHistory.Complications,
                                Complications_Explain = tcmClient.Client.IntakeMedicalHistory.Complications_Explain,
                                Comprehending = tcmClient.Client.IntakeMedicalHistory.Comprehending,
                                Concentrating = tcmClient.Client.IntakeMedicalHistory.Concentrating,
                                Constipation = tcmClient.Client.IntakeMedicalHistory.Constipation,
                                ConvulsionsOrFits = tcmClient.Client.IntakeMedicalHistory.ConvulsionsOrFits,
                                CoughingOfBlood = tcmClient.Client.IntakeMedicalHistory.CoughingOfBlood,
                                DescriptionOfChild = tcmClient.Client.IntakeMedicalHistory.DescriptionOfChild,
                                Diabetes = tcmClient.Client.IntakeMedicalHistory.Diabetes,
                                Diphtheria = tcmClient.Client.IntakeMedicalHistory.Diphtheria,
                                DoYouSmoke = tcmClient.Client.IntakeMedicalHistory.DoYouSmoke,
                                DoYouSmoke_PackPerDay = tcmClient.Client.IntakeMedicalHistory.DoYouSmoke_PackPerDay,
                                DoYouSmoke_Year = tcmClient.Client.IntakeMedicalHistory.DoYouSmoke_Year,
                                EarInfections = tcmClient.Client.IntakeMedicalHistory.EarInfections,
                                Epilepsy = tcmClient.Client.IntakeMedicalHistory.Epilepsy,
                                EyeTrouble = tcmClient.Client.IntakeMedicalHistory.EyeTrouble,
                                Fainting = tcmClient.Client.IntakeMedicalHistory.Fainting,
                                FamilyAsthma = tcmClient.Client.IntakeMedicalHistory.FamilyAsthma,
                                FamilyAsthma_ = tcmClient.Client.IntakeMedicalHistory.FamilyAsthma_,
                                FamilyCancer = tcmClient.Client.IntakeMedicalHistory.FamilyCancer,
                                FamilyCancer_ = tcmClient.Client.IntakeMedicalHistory.FamilyCancer_,
                                FamilyDiabetes = tcmClient.Client.IntakeMedicalHistory.FamilyDiabetes,
                                FamilyDiabetes_ = tcmClient.Client.IntakeMedicalHistory.FamilyDiabetes_,
                                FamilyEpilepsy = tcmClient.Client.IntakeMedicalHistory.FamilyEpilepsy,
                                FamilyEpilepsy_ = tcmClient.Client.IntakeMedicalHistory.FamilyEpilepsy_,
                                FamilyGlaucoma = tcmClient.Client.IntakeMedicalHistory.FamilyGlaucoma,
                                FamilyGlaucoma_ = tcmClient.Client.IntakeMedicalHistory.FamilyGlaucoma_,
                                FamilyHayFever = tcmClient.Client.IntakeMedicalHistory.FamilyHayFever,
                                FamilyHayFever_ = tcmClient.Client.IntakeMedicalHistory.FamilyHayFever_,
                                FamilyHeartDisease = tcmClient.Client.IntakeMedicalHistory.FamilyHeartDisease,
                                FamilyHeartDisease_ = tcmClient.Client.IntakeMedicalHistory.FamilyHeartDisease_,
                                FamilyHighBloodPressure = tcmClient.Client.IntakeMedicalHistory.FamilyHighBloodPressure,
                                FamilyHighBloodPressure_ = tcmClient.Client.IntakeMedicalHistory.FamilyHighBloodPressure_,
                                FamilyKidneyDisease = tcmClient.Client.IntakeMedicalHistory.FamilyKidneyDisease,
                                FamilyKidneyDisease_ = tcmClient.Client.IntakeMedicalHistory.FamilyHighBloodPressure_,
                                FamilyNervousDisorders = tcmClient.Client.IntakeMedicalHistory.FamilyNervousDisorders,
                                FamilyNervousDisorders_ = tcmClient.Client.IntakeMedicalHistory.FamilyNervousDisorders_,
                                FamilyOther = tcmClient.Client.IntakeMedicalHistory.FamilyOther,
                                FamilyOther_ = tcmClient.Client.IntakeMedicalHistory.FamilyOther_,
                                FamilySyphilis = tcmClient.Client.IntakeMedicalHistory.FamilySyphilis,
                                FamilySyphilis_ = tcmClient.Client.IntakeMedicalHistory.FamilySyphilis_,
                                FamilyTuberculosis = tcmClient.Client.IntakeMedicalHistory.FamilyTuberculosis,
                                FamilyTuberculosis_ = tcmClient.Client.IntakeMedicalHistory.FamilyTuberculosis_,
                                FirstYearMedical = tcmClient.Client.IntakeMedicalHistory.FirstYearMedical,
                                Fractures = tcmClient.Client.IntakeMedicalHistory.Fractures,
                                FrequentColds = tcmClient.Client.IntakeMedicalHistory.FrequentColds,
                                FrequentHeadaches = tcmClient.Client.IntakeMedicalHistory.FrequentHeadaches,
                                FrequentNoseBleeds = tcmClient.Client.IntakeMedicalHistory.FrequentNoseBleeds,
                                FrequentSoreThroat = tcmClient.Client.IntakeMedicalHistory.FrequentSoreThroat,
                                FrequentVomiting = tcmClient.Client.IntakeMedicalHistory.FrequentVomiting,
                                HaveYouEverBeenPregnant = tcmClient.Client.IntakeMedicalHistory.HaveYouEverBeenPregnant,
                                HaveYouEverHadComplications = tcmClient.Client.IntakeMedicalHistory.HaveYouEverHadComplications,
                                HaveYouEverHadExcessive = tcmClient.Client.IntakeMedicalHistory.HaveYouEverHadExcessive,
                                HaveYouEverHadPainful = tcmClient.Client.IntakeMedicalHistory.HaveYouEverHadPainful,
                                HaveYouEverHadSpotting = tcmClient.Client.IntakeMedicalHistory.HaveYouEverHadSpotting,
                                HayFever = tcmClient.Client.IntakeMedicalHistory.HayFever,
                                HeadInjury = tcmClient.Client.IntakeMedicalHistory.HeadInjury,
                                Hearing = tcmClient.Client.IntakeMedicalHistory.Hearing,
                                HearingTrouble = tcmClient.Client.IntakeMedicalHistory.HearingTrouble,
                                HeartPalpitation = tcmClient.Client.IntakeMedicalHistory.HeartPalpitation,
                                Hemorrhoids = tcmClient.Client.IntakeMedicalHistory.Hemorrhoids,
                                Hepatitis = tcmClient.Client.IntakeMedicalHistory.Hepatitis,
                                Hernia = tcmClient.Client.IntakeMedicalHistory.Hernia,
                                HighBloodPressure = tcmClient.Client.IntakeMedicalHistory.HighBloodPressure,
                                Hoarseness = tcmClient.Client.IntakeMedicalHistory.Hoarseness,
                                Immunizations = tcmClient.Client.IntakeMedicalHistory.Immunizations,
                                InfectiousDisease = tcmClient.Client.IntakeMedicalHistory.InfectiousDisease,
                                Jaundice = tcmClient.Client.IntakeMedicalHistory.Jaundice,
                                KidneyStones = tcmClient.Client.IntakeMedicalHistory.KidneyStones,
                                KidneyTrouble = tcmClient.Client.IntakeMedicalHistory.KidneyTrouble,
                                Length = tcmClient.Client.IntakeMedicalHistory.Length,
                                ListAllCurrentMedications = tcmClient.Client.IntakeMedicalHistory.ListAllCurrentMedications,
                                LossOfMemory = tcmClient.Client.IntakeMedicalHistory.LossOfMemory,
                                Mumps = tcmClient.Client.IntakeMedicalHistory.Mumps,
                                Nervousness = tcmClient.Client.IntakeMedicalHistory.Nervousness,
                                NightSweats = tcmClient.Client.IntakeMedicalHistory.NightSweats,
                                Normal = tcmClient.Client.IntakeMedicalHistory.Normal,
                                PainfulJoints = tcmClient.Client.IntakeMedicalHistory.PainfulJoints,
                                PainfulMuscles = tcmClient.Client.IntakeMedicalHistory.PainfulMuscles,
                                PainfulUrination = tcmClient.Client.IntakeMedicalHistory.PainfulUrination,
                                PerformingCertainMotions = tcmClient.Client.IntakeMedicalHistory.PerformingCertainMotions,
                                Planned = tcmClient.Client.IntakeMedicalHistory.Planned,
                                Poliomyelitis = tcmClient.Client.IntakeMedicalHistory.Poliomyelitis,
                                ProblemWithBedWetting = tcmClient.Client.IntakeMedicalHistory.ProblemWithBedWetting,
                                Reading = tcmClient.Client.IntakeMedicalHistory.Reading,
                                RheumaticFever = tcmClient.Client.IntakeMedicalHistory.RheumaticFever,
                                Rheumatism = tcmClient.Client.IntakeMedicalHistory.Rheumatism,
                                ScarletFever = tcmClient.Client.IntakeMedicalHistory.ScarletFever,
                                Seeing = tcmClient.Client.IntakeMedicalHistory.Seeing,
                                SeriousInjury = tcmClient.Client.IntakeMedicalHistory.SeriousInjury,
                                ShortnessOfBreath = tcmClient.Client.IntakeMedicalHistory.ShortnessOfBreath,
                                SkinTrouble = tcmClient.Client.IntakeMedicalHistory.SkinTrouble,
                                Speaking = tcmClient.Client.IntakeMedicalHistory.Speaking,
                                State = doctor.State,
                                StomachPain = tcmClient.Client.IntakeMedicalHistory.StomachPain,
                                Surgery = tcmClient.Client.IntakeMedicalHistory.Surgery,
                                SwellingOfFeet = tcmClient.Client.IntakeMedicalHistory.SwellingOfFeet,
                                SwollenAnkles = tcmClient.Client.IntakeMedicalHistory.SwollenAnkles,
                                Tuberculosis = tcmClient.Client.IntakeMedicalHistory.Tuberculosis,
                                Unplanned = tcmClient.Client.IntakeMedicalHistory.Unplanned,
                                VaricoseVeins = tcmClient.Client.IntakeMedicalHistory.VaricoseVeins,
                                VenerealDisease = tcmClient.Client.IntakeMedicalHistory.VenerealDisease,
                                VomitingOfBlood = tcmClient.Client.IntakeMedicalHistory.VomitingOfBlood,
                                Walking = tcmClient.Client.IntakeMedicalHistory.Walking,
                                WeightLoss = tcmClient.Client.IntakeMedicalHistory.WeightLoss,
                                WhoopingCough = tcmClient.Client.IntakeMedicalHistory.WhoopingCough,
                                WritingSentence = tcmClient.Client.IntakeMedicalHistory.WritingSentence,
                                ZipCode = doctor.ZipCode,
                                AgeOfFirstMenstruation = tcmClient.Client.IntakeMedicalHistory.AgeOfFirstMenstruation,
                                DateOfLastBreastExam = tcmClient.Client.IntakeMedicalHistory.DateOfLastBreastExam,
                                DateOfLastPelvic = tcmClient.Client.IntakeMedicalHistory.DateOfLastPelvic,
                                DateOfLastPeriod = tcmClient.Client.IntakeMedicalHistory.DateOfLastPeriod,
                                UsualDurationOfPeriods = tcmClient.Client.IntakeMedicalHistory.UsualDurationOfPeriods,
                                UsualIntervalBetweenPeriods = tcmClient.Client.IntakeMedicalHistory.UsualIntervalBetweenPeriods,
                                AdmissionedFor = user_logged.FullName

                            };
                            if (model.TCMClient.Client.LegalGuardian == null)
                                model.TCMClient.Client.LegalGuardian = new LegalGuardianEntity();
                            model.IdTCMClient = idTCMClient;
                            ViewData["CaseNumber"] = tcmClient.CaseNumber;
                            return View(model);
                        }
                    }
                    else
                    {
                        if (intakeMedicalHistory.TCMClient.Client.LegalGuardian == null)
                            intakeMedicalHistory.TCMClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeMedicalHistoryViewModel(intakeMedicalHistory);
                        model.IdTCMClient = idTCMClient;
                        ViewData["CaseNumber"] = tcmClient.CaseNumber;
                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public async Task<IActionResult> CreateTCMMedicalhistory(TCMIntakeMedicalHistoryViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeMedicalHistoryEntity IntakeMedicalHistoryEntity = _converterHelper.ToTCMIntakeMedicalHistoryEntity(IntakeViewModel, false, user_logged.UserName);

                if (IntakeMedicalHistoryEntity.Id == 0)
                {
                    //IntakeMedicalHistoryEntity.TCMClient = null;
                    _context.TCMIntakeMedicalHistory.Add(IntakeMedicalHistoryEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (User.IsInRole("CaseManager"))
                        {
                            return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                        }
                        else
                        {
                            return RedirectToAction("TCMIntakeSectionDashboardReadOnly", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    IntakeMedicalHistoryEntity.TCMClient = null;
                    _context.TCMIntakeMedicalHistory.Update(IntakeMedicalHistoryEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (User.IsInRole("CaseManager"))
                        {
                            return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                        }
                        else
                        {
                            return RedirectToAction("TCMIntakeSectionDashboardReadOnly", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.TCMClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMMedicalhistory", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult CreateTCMMedication(int id = 0, int idTCMClient = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(n => n.TCMAssessment)
                                                .ThenInclude(n => n.MedicationList)
                                                .Include(n => n.Client)
                                                .FirstOrDefault(n => n.Id == idTCMClient);

            TCMAssessmentMedicationViewModel model;

            if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {

                    model = new TCMAssessmentMedicationViewModel
                    {
                        Id = 0,
                        Dosage = "",
                        Frequency = "",
                        Name = "",
                        Prescriber = "",
                        TcmAssessment = tcmClient.TCMAssessment,
                        IdTCMAssessment = (tcmClient.TCMAssessment == null)? 0:tcmClient.TCMAssessment.Id,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Today

                    };
                   
                    ViewData["CaseNumber"] = tcmClient.CaseNumber;
                    return View(model);
                }
            }

            model = new TCMAssessmentMedicationViewModel
            {
                IdTCMAssessment = id,
                TcmAssessment = _context.TCMAssessment
                                        .Include(n => n.TcmClient)
                                        .ThenInclude(n => n.Client)
                                        .FirstOrDefault(n => n.Id == id),
                Id = 0,
                Dosage = "",
                Frequency = "",
                Name = "",
                Prescriber = ""
            };
           
            ViewData["CaseNumber"] = tcmClient.CaseNumber;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public async Task<IActionResult> CreateTCMMedication(TCMAssessmentMedicationViewModel MedicationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMAssessmentEntity assessment = await _context.TCMAssessment.FirstOrDefaultAsync(n => n.Id == MedicationViewModel.IdTCMAssessment);

            if (ModelState.IsValid)
            {
                TCMAssessmentMedicationEntity medicationEntity = _context.TCMAssessmentMedication.Find(MedicationViewModel.Id);
                if (medicationEntity == null)
                {
                    medicationEntity = await _converterHelper.ToTCMAssessmenMedicationEntity(MedicationViewModel, true, user_logged.UserName);
                    _context.TCMAssessmentMedication.Add(medicationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (User.IsInRole("CaseManager"))
                        {
                            return RedirectToAction("CreateTCMMedication", new { id = MedicationViewModel.Id, IdTCMClient = assessment.TcmClient_FK });
                        }
                        else
                        {
                            return RedirectToAction("CreateTCMMedicationReadOnly", new { id = MedicationViewModel.Id, IdTCMClient = assessment.TcmClient_FK });
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Medication.");
                    return RedirectToAction("CreateTCMMedication", "MedicationViewModel", new { id = MedicationViewModel.Id, IdTCMClient = assessment.TcmClient_FK });
                   // return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMMedication", MedicationViewModel, new { id = MedicationViewModel.IdClient, IdTCMClient = MedicationViewModel.IdTCMClient }) });
                }
            }
            TCMAssessmentMedicationViewModel model;
            model = new TCMAssessmentMedicationViewModel
            {
                TcmAssessment = _context.TCMAssessment.Find(MedicationViewModel.IdTCMAssessment),
                IdTCMAssessment = MedicationViewModel.IdTCMAssessment,
                Id = MedicationViewModel.Id,
                Dosage = MedicationViewModel.Dosage,
                Frequency = MedicationViewModel.Frequency,
                Name = MedicationViewModel.Name,
                Prescriber = MedicationViewModel.Prescriber

            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMMedication", MedicationViewModel) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult EditTCMMedication(int id = 0)
        {
            TCMAssessmentMedicationViewModel model;
          
            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    TCMAssessmentMedicationEntity Medication = _context.TCMAssessmentMedication
                                                                       .Include(m => m.TcmAssessment)
                                                                       .ThenInclude(m => m.MedicationList)
                                                                       .Include(m => m.TcmAssessment)
                                                                       .ThenInclude(m => m.TcmClient)
                                                                       .FirstOrDefault(m => m.Id == id);
                    if (Medication == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToTCMAssessmentMedicationViewModel(Medication);
                        model.IdTCMAssessment = Medication.TcmAssessment.Id;
                        return View(model);
                    }

                }
            }

            model = new TCMAssessmentMedicationViewModel();
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
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
                    if (User.IsInRole("CaseManager"))
                    {
                        return RedirectToAction("CreateTCMMedication", new { id = medicationViewModel.IdClient, IdTCMClient = medicationViewModel.IdTCMClient });
                    }
                    else
                    {
                        return RedirectToAction("CreateTCMMedicationReadOnly", new { id = medicationViewModel.IdClient, IdTCMClient = medicationViewModel.IdTCMClient });
                    }
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditTCMMedication", medicationViewModel) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
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

            TCMClientEntity tcmClientEntity = await _context.TCMClient
                                                            .Include(n => n.Client)
                                                            .FirstOrDefaultAsync(s => s.Client.Id == medicationEntity.Client.Id);

            //return RedirectToAction(nameof(Create), new { id = medicationEntity.Client.Id });
            return RedirectToAction("CreateTCMMedication", new { id = medicationEntity.Client.Id, idTCMClient = tcmClientEntity.Id });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult CreateTCMCoordinationCare(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeCoordinationCareViewModel model;

            if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor"))
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
                                                                                 .ThenInclude(n => n.TCMIntakeForm)
                                                                                 .AsSplitQuery()
                                                                                 .FirstOrDefault(n => n.TcmClient.Id == id);

                    if (intakeCoordination == null)
                    {
                        TCMClientEntity tcmClient = _context.TCMClient
                                                            .Include(d => d.Client)
                                                            .ThenInclude(d => d.LegalGuardian)
                                                            .Include(d => d.Casemanager)
                                                            .ThenInclude(d => d.Clinic)
                                                            .Include(n => n.TCMIntakeForm)
                                                            .AsSplitQuery()
                                                            .FirstOrDefault(n => n.Id == id);
                        if (tcmClient.TCMIntakeForm == null)
                            tcmClient.TCMIntakeForm = new TCMIntakeFormEntity();

                        model = new TCMIntakeCoordinationCareViewModel
                        {
                            TcmClient = tcmClient,
                            Date = tcmClient.DataOpen,
                            Id = 0,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            IdTCMClient = id,
                            TcmClient_FK = id,
                            AdmissionedFor = user_logged.FullName,
                            DateSignatureEmployee = tcmClient.DataOpen,
                            DateSignatureLegalGuardian = tcmClient.DataOpen,
                            DateSignaturePerson = tcmClient.DataOpen,
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
                            IRefuse = false,
                            PCP = true,
                            Specialist = false,
                            SpecialistText = "",
                            PCP_Name = tcmClient.TCMIntakeForm.PCP_Name,
                            PCP_Address = tcmClient.TCMIntakeForm.PCP_Address,
                            PCP_Phone = tcmClient.TCMIntakeForm.PCP_Phone,
                            PCP_CityStateZip = tcmClient.TCMIntakeForm.PCP_CityStateZip,
                            PCP_FaxNumber = tcmClient.TCMIntakeForm.PCP_FaxNumber,

                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                       
                        return View(model);
                    }
                    else
                    {
                        if (intakeCoordination.TcmClient.Client.LegalGuardian == null)
                            intakeCoordination.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                        
                        model = _converterHelper.ToTCMIntakeCoordinationCareViewModel(intakeCoordination);
                        
                        if (intakeCoordination.TcmClient.TCMIntakeForm != null)
                        {
                            model.PCP_Name = intakeCoordination.TcmClient.TCMIntakeForm.PCP_Name;
                            model.PCP_Address = intakeCoordination.TcmClient.TCMIntakeForm.PCP_Address;
                            model.PCP_Phone = intakeCoordination.TcmClient.TCMIntakeForm.PCP_Phone;
                            model.PCP_CityStateZip = intakeCoordination.TcmClient.TCMIntakeForm.PCP_CityStateZip;
                            model.PCP_FaxNumber = intakeCoordination.TcmClient.TCMIntakeForm.PCP_FaxNumber;
                        }
                        return View(model);
                    }
                }
            }

            return RedirectToAction("Index", "TCMIntakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> CreateTCMCoordinationCare(TCMIntakeCoordinationCareViewModel IntakeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeFormEntity intake = _context.TCMIntakeForms
                                                 .FirstOrDefault(u => u.TcmClient.Id == IntakeViewModel.IdTCMClient);

            if (ModelState.IsValid)
            {
                TCMIntakeCoordinationCareEntity IntakeCoordination = _converterHelper.ToTCMIntakeCoordinationCareEntity(IntakeViewModel, false, user_logged.UserName);

                if (intake != null)
                {
                    intake.PCP_Name = IntakeViewModel.PCP_Name;
                    intake.PCP_Address = IntakeViewModel.PCP_Address;
                    intake.PCP_Phone = IntakeViewModel.PCP_Phone;
                    intake.PCP_FaxNumber = IntakeViewModel.PCP_FaxNumber;
                    intake.PCP_CityStateZip = IntakeViewModel.PCP_CityStateZip;
                    
                    _context.TCMIntakeForms.Update(intake);
                    await _context.SaveChangesAsync();
                }
               
                if (IntakeCoordination.Id == 0)
                {
                    IntakeCoordination.TcmClient = null;
                    _context.TCMIntakeCoordinationCare.Add(IntakeCoordination);
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (User.IsInRole("CaseManager"))
                        {
                            return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                        }
                        else
                        {
                            return RedirectToAction("TCMIntakeSectionDashboardReadOnly", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                        }
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
                        if (User.IsInRole("CaseManager"))
                        {
                            return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                        }
                        else
                        {
                            return RedirectToAction("TCMIntakeSectionDashboardReadOnly", new { id = IntakeViewModel.IdTCMClient, section = 3 });
                        }
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

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> TCMIntakeSectionDashboard(int id = 0, int section = 0, int origin = 0, int error = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Home/Error404");
            }
            if (error == 1)
            {
                ViewBag.Creado = "SPR";
            }
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (section == 1)
            {
                TCMClientEntity TcmClientEntity = await _context.TCMClient
                                                                .Include(c => c.TCMIntakeForm)
                                                                .Include(c => c.TcmIntakeConsentForTreatment)
                                                                .Include(n => n.TcmIntakeConsentForRelease)
                                                                .Include(n => n.TcmIntakeConsumerRights)
                                                                .Include(n => n.TcmIntakeAcknowledgementHipa)
                                                                .Include(n => n.TCMIntakeOrientationChecklist)
                                                                .Include(n => n.TCMIntakeAdvancedDirective)
                                                                .Include(n => n.TCMIntakeForeignLanguage)
                                                                .Include(n => n.TCMIntakeWelcome)
                                                               
                                                                .Include(c => c.Client)
                                                                .Include(n => n.Client.Psychiatrist)
                                                                .Include(n => n.Client.Doctor)
                                                               
                                                                .Include(n => n.TCMIntakeClientSignatureVerification)
                                                                .Include(n => n.TCMIntakeClientIdDocumentVerification)
                                                                .Include(n => n.TCMIntakePainScreen)
                                                                .Include(n => n.TCMIntakeColumbiaSuicide)
                                                                .Include(n => n.TCMIntakeNutritionalScreen)
                                                                .Include(n => n.TCMIntakePersonalWellbeing)
                                                                .AsSplitQuery()
                                                                .FirstOrDefaultAsync(c => c.Id == id);

                if (TcmClientEntity == null)
                {
                    return RedirectToAction("Home/Error404");
                }
                ViewBag.Section = section.ToString();
                ViewData["origin"] = origin;
                return View(TcmClientEntity);
            }
            else
            {
                if (section == 2)
                {
                    TCMClientEntity TcmClientEntity = await _context.TCMClient
                                                                   
                                                                    .Include(n => n.Client)
                                                                    .ThenInclude(n => n.Documents)
                                                                  
                                                                    .Include(n => n.Client.IntakeFeeAgreement)
                                                                    .Include(n => n.TCMIntakeNonClinicalLog)

                                                                    .AsSplitQuery()

                                                                    .FirstOrDefaultAsync(c => c.Id == id);

                    if (TcmClientEntity == null)
                    {
                        return RedirectToAction("Home/Error404");
                    }
                    ViewBag.Section = section.ToString();
                    ViewData["origin"] = origin;
                    return View(TcmClientEntity);
                }
                else
                {
                    if (section == 3)
                    {
                        TCMClientEntity TcmClientEntity = await _context.TCMClient

                                                                       .Include(c => c.Client)
                                                                       
                                                                       .Include(n => n.Client.Psychiatrist)
                                                                       .Include(n => n.Client.Doctor)
                                                                       .Include(n => n.TCMIntakeMedicalHistory)
                                                                       .Include(n => n.TCMAssessment)
                                                                       .ThenInclude(n => n.MedicationList)
                                                                       .Include(n => n.TCMIntakeMiniMental)
                                                                       .Include(n => n.TCMIntakeCoordinationCare)
                                                                       .AsSplitQuery()
                                                                       .FirstOrDefaultAsync(c => c.Id == id);

                        if (TcmClientEntity == null)
                        {
                            return RedirectToAction("Home/Error404");
                        }
                        ViewBag.Section = section.ToString();
                        ViewData["origin"] = origin;
                        return View(TcmClientEntity);
                    }
                    else
                    {
                        if (section == 4)
                        {
                            TCMClientEntity TcmClientEntity = await _context.TCMClient

                                                                            .Include(c => c.Client)
                                                                            .Include(n => n.Client.Psychiatrist)
                                                                            .Include(n => n.Client.Doctor)
                                                                          
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
                                                                            .Include(n => n.TcmIntakeAppendixI)
                                                                            .AsSplitQuery()
                                                                            .FirstOrDefaultAsync(c => c.Id == id);

                            if (TcmClientEntity == null)
                            {
                                return RedirectToAction("Home/Error404");
                            }
                            ViewBag.Section = section.ToString();
                            ViewData["origin"] = origin;
                            return View(TcmClientEntity);
                        }
                        else
                        {
                            if (section == 5)
                            {
                                TCMClientEntity TcmClientEntity = await _context.TCMClient

                                                                               .Include(c => c.Client)
                                                                               .Include(n => n.Client.Psychiatrist)
                                                                               .Include(n => n.Client.Doctor)
                                                                               .Include(n => n.TcmInterventionLog)
                                                                               .ThenInclude(n => n.InterventionList)
                                                                               .Include(n => n.TCMFarsFormList)
                                                                               .AsSplitQuery()
                                                                               .FirstOrDefaultAsync(c => c.Id == id);

                                if (TcmClientEntity == null)
                                {
                                    return RedirectToAction("Home/Error404");
                                }
                                ViewBag.Section = section.ToString();
                                ViewData["origin"] = origin;
                                return View(TcmClientEntity);
                            }
                            else
                            {
                                TCMClientEntity TcmClientEntity = await _context.TCMClient
                                                                                .Include(c => c.Client)
                                                                                .Include(n => n.TCMNote)
                                                                                .FirstOrDefaultAsync(c => c.Id == id);

                                if (TcmClientEntity == null)
                                {
                                    return RedirectToAction("Home/Error404");
                                }
                                ViewBag.Section = section.ToString();
                                ViewData["origin"] = origin;
                                return View(TcmClientEntity);
                            }
                        }
                    }
                }
            }           
        }

        [Authorize(Roles = "TCMSupervisor, Manager")]
        public async Task<IActionResult> TCMIntakeSectionDashboardReadOnly(int id = 0, int section = 0, int origin = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = await _context.Users
                                       .Include(u => u.Clinic)
                                       .ThenInclude(c => c.Setting)
                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (section == 1)
            {
                TCMClientEntity TcmClientEntity = await _context.TCMClient
                                                                .Include(n => n.Client)
                                                                .Include(c => c.TCMIntakeForm)
                                                                .Include(c => c.TcmIntakeConsentForTreatment)
                                                                .Include(n => n.TcmIntakeConsentForRelease.Where(m => m.TcmClient_FK == id))
                                                                .Include(n => n.TcmIntakeConsumerRights)
                                                                .Include(n => n.TcmIntakeAcknowledgementHipa)
                                                                .Include(n => n.TCMIntakeOrientationChecklist)
                                                                .Include(n => n.TCMIntakeAdvancedDirective)
                                                                .Include(n => n.TCMIntakeForeignLanguage)
                                                                .Include(n => n.TCMIntakeWelcome)
                                                                .Include(n => n.TCMIntakeClientSignatureVerification)
                                                                .Include(n => n.TCMIntakeClientIdDocumentVerification)
                                                                .Include(n => n.TCMIntakePainScreen)
                                                                .Include(n => n.TCMIntakeColumbiaSuicide)
                                                                .Include(n => n.TCMIntakeNutritionalScreen)
                                                                .Include(n => n.TCMIntakePersonalWellbeing)
                                                                .Include(n => n.Client.Psychiatrist)
                                                                .Include(n => n.Client.Doctor)
                                                                .AsSplitQuery()
                                                                .FirstOrDefaultAsync(c => c.Id == id);

                if (TcmClientEntity == null)
                {
                    return RedirectToAction("Home/Error404");
                }
                ViewBag.Section = section.ToString();
                ViewData["origin"] = origin;
                return View(TcmClientEntity);
            }
            else
            {
                if (section == 2)
                {
                    TCMClientEntity TcmClientEntity = await _context.TCMClient
                                                                    .Include(n => n.Client)
                                                                    .ThenInclude(n => n.Documents)
                                                                    .Include(n => n.Client.IntakeFeeAgreement)
                                                                    .Include(n => n.TCMIntakeNonClinicalLog)
                                                                    .AsSplitQuery()
                                                                    .FirstOrDefaultAsync(c => c.Id == id);

                    if (TcmClientEntity == null)
                    {
                        return RedirectToAction("Home/Error404");
                    }
                    ViewBag.Section = section.ToString();
                    ViewData["origin"] = origin;
                    return View(TcmClientEntity);
                }
                else
                {
                    if (section == 3)
                    {
                        TCMClientEntity TcmClientEntity = await _context.TCMClient
                                                                        .Include(c => c.Client)
                                                                        .Include(n => n.TCMIntakeMiniMental)
                                                                        .Include(n => n.TCMIntakeCoordinationCare)
                                                                        .Include(n => n.TCMIntakeMedicalHistory)
                                                                        .Include(n => n.TCMAssessment.MedicationList)
                                                                        .AsSplitQuery()
                                                                        .FirstOrDefaultAsync(c => c.Id == id);

                        if (TcmClientEntity == null)
                        {
                            return RedirectToAction("Home/Error404");
                        }
                        ViewBag.Section = section.ToString();
                        ViewData["origin"] = origin;
                        return View(TcmClientEntity);
                    }
                    else
                    {
                        if (section == 4)
                        {
                            TCMClientEntity TcmClientEntity = await _context.TCMClient
                                                                            .Include(c => c.Client)
                                                                            .Include(n => n.TcmServicePlan)
                                                                            .ThenInclude(n => n.TCMAdendum)
                                                                            .Include(n => n.TcmServicePlan)
                                                                            .ThenInclude(n => n.TCMServicePlanReview)
                                                                            .Include(n => n.TcmServicePlan)
                                                                            .ThenInclude(n => n.TCMDischarge)
                                                                            .Include(n => n.TcmIntakeAppendixJ)
                                                                            .Include(n => n.TCMAssessment)
                                                                            .ThenInclude(n => n.HouseCompositionList)
                                                                            .Include(n => n.TCMAssessment)
                                                                            .ThenInclude(n => n.MedicationList)
                                                                            .Include(n => n.TCMAssessment)
                                                                            .ThenInclude(n => n.IndividualAgencyList)
                                                                            .Include(n => n.TCMAssessment)
                                                                            .ThenInclude(n => n.PastCurrentServiceList)
                                                                            .Include(n => n.TcmIntakeAppendixI)
                                                                            .AsSplitQuery()
                                                                            .FirstOrDefaultAsync(c => c.Id == id);

                            if (TcmClientEntity == null)
                            {
                                return RedirectToAction("Home/Error404");
                            }
                            ViewBag.Section = section.ToString();
                            ViewData["origin"] = origin;
                            return View(TcmClientEntity);
                        }
                        else
                        {
                            if (section == 5)
                            {
                                TCMClientEntity TcmClientEntity = await _context.TCMClient
                                                                                .Include(c => c.Client)
                                                                                .Include(n => n.TcmInterventionLog)
                                                                                .ThenInclude(n => n.InterventionList)
                                                                                .Include(n => n.TCMFarsFormList)
                                                                                .AsSplitQuery()
                                                                                .FirstOrDefaultAsync(c => c.Id == id);

                                if (TcmClientEntity == null)
                                {
                                    return RedirectToAction("Home/Error404");
                                }
                                ViewBag.Section = section.ToString();
                                ViewData["origin"] = origin;
                                return View(TcmClientEntity);
                            }
                            else
                            {
                                TCMClientEntity TcmClientEntity = await _context.TCMClient
                                                                                    .Include(c => c.Client)
                                                                                    .Include(n => n.TCMNote)
                                                                                    .AsSplitQuery()
                                                                                    .FirstOrDefaultAsync(c => c.Id == id);

                                if (TcmClientEntity == null)
                                {
                                    return RedirectToAction("Home/Error404");
                                }
                                ViewBag.Section = section.ToString();
                                ViewData["origin"] = origin;
                                return View(TcmClientEntity);
                            }
                        }
                    }
                }
            }
            
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMAppendixJ(int id = 0, int origi = 0)
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
                    TCMClientEntity tcmclient = _context.TCMClient
                                                        .Include(n => n.Client)
                                                        .FirstOrDefault(n => n.Id == id);
                    model = new TCMIntakeAppendixJViewModel
                    {
                        TcmClient = tcmclient,
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
                        IsExperiencing = true,
                        SupervisorSignatureDate = DateTime.Now,
                        TcmSupervisor = new TCMSupervisorEntity(),
                        HasAMental2 = true,
                        HasAMental6 = true,
                        HasRecolated = false,
                        IsEnrolled = true,
                        IsNotReceiving = true,
                        Lacks = true,
                        Meets = true,
                        RequiresOngoing = true,
                        RequiresServices = true,
                        Active = false,
                        DateExpired = tcmclient.DataClose,
                        AppendixType = AppendixJType.Initial,
                        IdType = 0,
                        AppendixJTypes = _combosHelper.GetComboAppendixJType()

                    };
                    ViewData["origi"] = origi;
                    return View(model);

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMAppendixJ(TCMIntakeAppendixJViewModel AppendixJViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                List<TCMIntakeAppendixJEntity> appendixJList = await _context.TCMIntakeAppendixJ
                                                                             .Where(n => n.TcmClient_FK == AppendixJViewModel.TcmClient_FK)
                                                                             .ToListAsync();

                foreach (var item in appendixJList)
                {
                    item.Active = false;
                    item.DateExpired = AppendixJViewModel.Date;
                    _context.TCMIntakeAppendixJ.Update(item);
                }

                TCMIntakeAppendixJEntity AppendixJEntity = await _converterHelper.ToTCMIntakeAppendixJEntity(AppendixJViewModel, false, user_logged.UserName);

                if (AppendixJEntity.Id == 0)
                {
                    AppendixJEntity.TcmClient = await _context.TCMClient.FindAsync(AppendixJViewModel.TcmClient_FK);
                    AppendixJEntity.Approved = 1;
                    _context.TCMIntakeAppendixJ.Add(AppendixJEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (origi == 0)
                        {
                            return RedirectToAction("TCMIntakeSectionDashboard", new { id = AppendixJViewModel.IdTCMClient, section = 4 });
                        }
                        if (origi == 1)
                        {
                            return RedirectToAction("GetCaseNotServicePlan", "TCMClients");
                        }
                        if (origi == 2)
                        {
                            return RedirectToAction("ListAppendixJForTCMClient", "TCMIntakes", new { id = AppendixJViewModel.IdTCMClient, origi = 0});
                        }
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
                        if (origi == 0)
                        {
                            return RedirectToAction("TCMIntakeSectionDashboard", new { id = AppendixJViewModel.IdTCMClient, section = 4 });
                        }
                        if (origi == 1)
                        {
                            return RedirectToAction("GetCaseNotServicePlan", "TCMClients");
                        }
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

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult CreateTCMInterventionLog(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeInterventionLogViewModel model;

            if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeInterventionLogEntity interventionLog = _context.TCMIntakeInterventionLog
                                                                             .Include(n => n.TcmClient)
                                                                             .ThenInclude(n => n.Client)
                                                                             .Include(n => n.TcmClient)
                                                                             .ThenInclude(n => n.Casemanager)
                                                                             .Include(n => n.InterventionList)
                                                                             .FirstOrDefault(n => n.TcmClient.Id == id);
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
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> CreateTCMInterventionLog(TCMIntakeInterventionLogViewModel interventionLogViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeInterventionLogEntity InterventionLogEntity = _converterHelper.ToTCMIntakeInterventionLogEntity(interventionLogViewModel, true, user_logged.UserName);

                _context.TCMIntakeInterventionLog.Add(InterventionLogEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    if (User.IsInRole("CaseManager"))
                    {
                        return RedirectToAction("CreateTCMInterventionLog", new { id = interventionLogViewModel.IdTCMClient, section = 5 });
                    }
                    else
                    {
                        return RedirectToAction("TCMIntakeSectionDashboardReadOnly", new { id = interventionLogViewModel.IdTCMClient, section = 5 });
                    }
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            interventionLogViewModel.TcmClient = _context.TCMClient.Find(interventionLogViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMInterventionLog", interventionLogViewModel) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult EditTCMInterventionLog(int id = 0)
        {
            TCMIntakeInterventionLogViewModel model;

            if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor") || User.IsInRole("Manager")) 
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
        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public async Task<IActionResult> EditTCMInterventionLog(TCMIntakeInterventionLogViewModel tcmInterLogViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeInterventionLogEntity tcmInterLogEntity = _converterHelper.ToTCMIntakeInterventionLogEntity(tcmInterLogViewModel, false, user_logged.UserName);
                _context.TCMIntakeInterventionLog.Update(tcmInterLogEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    if (User.IsInRole("CaseManager"))
                    {
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = tcmInterLogEntity.TcmClient_FK, section = 5 });
                    }
                    else
                    {
                        return RedirectToAction("TCMIntakeSectionDashboardReadOnly", new { id = tcmInterLogEntity.TcmClient_FK, section = 5 });
                    }
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditTCMInterventionLog", tcmInterLogViewModel) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult CreateTCMIntervention(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeInterventionViewModel model;

            if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor") || User.IsInRole("Manager"))
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
        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
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

            if (User.IsInRole("CaseManager"))
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
                if (User.IsInRole("TCMSupervisor"))
                {
                    apendiceJ = await _context.TCMIntakeAppendixJ
                                              .Include(w => w.TcmClient)
                                              .ThenInclude(d => d.Client)
                                              .ThenInclude(d => d.Clinic)
                                              .Include(w => w.TcmClient)
                                              .ThenInclude(d => d.Casemanager)
                                              .Where(w => (w.TcmClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                                        && w.Approved == approved))
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
               
            }

            return View(apendiceJ);
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult CreateTCMAppendixJReadOnly(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeAppendixJViewModel model;
            int edit = 0;

            if (user_logged.Clinic.Setting.TCMSupervisorEdit == false)
            {
                edit = 1;
            }

            if (User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeAppendixJEntity intakeAppendixJ = _context.TCMIntakeAppendixJ
                                                                       .Include(n => n.TcmClient)
                                                                       .ThenInclude(n => n.Client)
                                                                       .FirstOrDefault(n => n.Id == id);
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
                            TcmSupervisor = new TCMSupervisorEntity(),
                            IdType = 0,
                            AppendixJTypes = _combosHelper.GetComboAppendixJType()

                        };

                        ViewData["origi"] = origi;
                        ViewData["edit"] = edit;
                        return View(model);
                    }
                    else
                    {
                        model = _converterHelper.ToTCMIntakeAppendixJViewModel(intakeAppendixJ);
                        ViewData["origi"] = origi;
                        ViewData["edit"] = edit;
                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> AproveAppendixJ(int id, TCMIntakeAppendixJViewModel model, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeAppendixJEntity tcmAppendixJ = await _converterHelper.ToTCMIntakeAppendixJEntity(model, false, user_logged.UserName);
            
            if (tcmAppendixJ != null)
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    List<TCMIntakeAppendixJEntity> list = _context.TCMIntakeAppendixJ
                                                                  .Where(n => n.Active == true 
                                                                           && n.TcmClient_FK == model.TcmClient_FK
                                                                           && n.Id != model.Id)
                                                                  .ToList();
                    foreach (var item in list)
                    {
                        item.Active = false;
                        _context.Update(item);
                    }

                    if (user_logged.Clinic != null)
                    {
                        tcmAppendixJ.Approved = 2;
                        tcmAppendixJ.Active = true;
                        tcmAppendixJ.TcmSupervisor = tcmAppendixJ.TcmClient.Casemanager.TCMSupervisor;
                        
                        _context.Update(tcmAppendixJ);
                        try
                        {
                            await _context.SaveChangesAsync();
                            if (origi == 0)
                            {
                                return RedirectToAction("ApendiceJStatus", "TCMIntakes", new { approved = 1 });
                            }
                            if (origi == 1)
                            {
                                return RedirectToAction("TCMIntakeSectionDashboardReadOnly", new { id = tcmAppendixJ.TcmClient_FK, section = 4 });
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

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMIntakeForm(int id)
        {
            TCMIntakeFormEntity entity = await _context.TCMIntakeForms
                                                       
                                                       .Include(t => t.TcmClient)
                                                       .ThenInclude(c => c.Client)
                                                       .ThenInclude(cl => cl.Clients_Diagnostics)
                                                       .ThenInclude(cd => cd.Diagnostic)

                                                       .Include(t => t.TcmClient)
                                                       .ThenInclude(c => c.Client)
                                                       .ThenInclude(cl => cl.Client_Referred)
                                                       .ThenInclude(cl => cl.Referred)

                                                       .Include(t => t.TcmClient)
                                                       .ThenInclude(c => c.Client)
                                                       .ThenInclude(cl => cl.LegalGuardian)

                                                       .Include(t => t.TcmClient)
                                                       .ThenInclude(c => c.Client)
                                                       .ThenInclude(cl => cl.Clients_HealthInsurances)
                                                       .ThenInclude(ch => ch.HealthInsurance)

                                                       .Include(t => t.TcmClient)
                                                       .ThenInclude(c => c.Client)
                                                       .ThenInclude(cl => cl.EmergencyContact)                                                       

                                                       .Include(t => t.TcmClient)
                                                       .ThenInclude(c => c.Casemanager)
                                                       .ThenInclude(cm => cm.Clinic)

                                                       .AsSplitQuery()
                                                       
                                                       .FirstOrDefaultAsync(t => t.TcmClient.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeFormReport(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMConsentForTreatment(int id)
        {
            TCMIntakeConsentForTreatmentEntity entity = await _context.TCMIntakeConsentForTreatment

                                                                      .Include(t => t.TcmClient)
                                                                      .ThenInclude(c => c.Client)
                                                       
                                                                      .Include(t => t.TcmClient)
                                                                      .ThenInclude(c => c.Client)
                                                                      .ThenInclude(cl => cl.LegalGuardian)

                                                                      .Include(t => t.TcmClient)
                                                                      .ThenInclude(c => c.Casemanager)
                                                                      .ThenInclude(cm => cm.Clinic)

                                                                      .AsSplitQuery()

                                                                      .FirstOrDefaultAsync(t => t.TcmClient.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeConsentForTreatmentReport(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMConsentForRelease(int id)
        {
            TCMIntakeConsentForReleaseEntity entity = await _context.TCMIntakeConsentForRelease

                                                                     .Include(t => t.TcmClient)
                                                                     .ThenInclude(c => c.Client)

                                                                     .Include(t => t.TcmClient)
                                                                     .ThenInclude(c => c.Client)
                                                                     .ThenInclude(cl => cl.LegalGuardian)

                                                                     .Include(t => t.TcmClient)
                                                                     .ThenInclude(c => c.Casemanager)
                                                                     .ThenInclude(cm => cm.Clinic)

                                                                     .AsSplitQuery()

                                                                     .FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeConsentForRelease(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMConsumerRights(int id)
        {
            TCMIntakeConsumerRightsEntity entity = await _context.TCMIntakeConsumerRights

                                                                 .Include(t => t.TcmClient)
                                                                 .ThenInclude(c => c.Client)

                                                                 .Include(t => t.TcmClient)
                                                                 .ThenInclude(c => c.Client)
                                                                 .ThenInclude(cl => cl.LegalGuardian)

                                                                 .Include(t => t.TcmClient)
                                                                 .ThenInclude(c => c.Casemanager)
                                                                 .ThenInclude(cm => cm.Clinic)

                                                                 .AsSplitQuery()

                                                                 .FirstOrDefaultAsync(t => t.TcmClient.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeConsumerRights(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMAdvanceDirective(int id)
        {
            TCMIntakeAdvancedDirectiveEntity entity = await _context.TCMIntakeAdvancedDirective

                                                                    .Include(t => t.TcmClient)
                                                                    .ThenInclude(c => c.Client)

                                                                    .Include(t => t.TcmClient)
                                                                    .ThenInclude(c => c.Client)
                                                                    .ThenInclude(cl => cl.EmergencyContact)

                                                                    .Include(t => t.TcmClient)
                                                                    .ThenInclude(c => c.Casemanager)
                                                                    .ThenInclude(cm => cm.Clinic)

                                                                    .AsSplitQuery()

                                                                    .FirstOrDefaultAsync(t => t.TcmClient.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeAdvancedDirective(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }
        
        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMAcknowledgementHippa(int id)
        {
            TCMIntakeAcknowledgementHippaEntity entity = await _context.TCMIntakeAcknowledgement

                                                                        .Include(t => t.TcmClient)
                                                                        .ThenInclude(c => c.Client)

                                                                        .Include(t => t.TcmClient)
                                                                        .ThenInclude(c => c.Client)
                                                                        .ThenInclude(cl => cl.LegalGuardian)

                                                                        .Include(t => t.TcmClient)
                                                                        .ThenInclude(c => c.Casemanager)
                                                                        .ThenInclude(cm => cm.Clinic)

                                                                        .AsSplitQuery()

                                                                        .FirstOrDefaultAsync(t => t.TcmClient.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeAcknowledgementHippa(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMOrientationChecklist(int id)
        {
            TCMIntakeOrientationChecklistEntity entity = await _context.TCMIntakeOrientationCheckList

                                                                       .Include(t => t.TcmClient)
                                                                       .ThenInclude(c => c.Client)

                                                                       .Include(t => t.TcmClient)
                                                                       .ThenInclude(c => c.Casemanager)
                                                                       .ThenInclude(cm => cm.Clinic)

                                                                       .AsSplitQuery()

                                                                       .FirstOrDefaultAsync(t => t.TcmClient.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeOrientationCheckList(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMForeignLanguage(int id)
        {
            TCMIntakeForeignLanguageEntity entity = await _context.TCMIntakeForeignLanguage

                                                                  .Include(t => t.TcmClient)
                                                                  .ThenInclude(c => c.Client)

                                                                  .Include(t => t.TcmClient)
                                                                  .ThenInclude(c => c.Client)
                                                                  .ThenInclude(cl => cl.LegalGuardian)

                                                                  .Include(t => t.TcmClient)
                                                                  .ThenInclude(c => c.Casemanager)
                                                                  .ThenInclude(cm => cm.Clinic)

                                                                  .AsSplitQuery()

                                                                  .FirstOrDefaultAsync(t => t.TcmClient.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeForeignLanguage(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMCorrespondence(int id)
        {
            TCMIntakeWelcomeEntity entity = await _context.TCMIntakeWelcome

                                                          .Include(t => t.TcmClient)
                                                          .ThenInclude(c => c.Client)                                                          

                                                          .Include(t => t.TcmClient)
                                                          .ThenInclude(c => c.Casemanager)
                                                          .ThenInclude(cm => cm.Clinic)

                                                          .AsSplitQuery()

                                                          .FirstOrDefaultAsync(t => t.TcmClient.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeWelcome(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMClientsSignatureVerificationForm(int id)
        {
            TCMIntakeClientSignatureVerificationEntity entity = await _context.TCMIntakeClientSignatureVerification

                                                                              .Include(t => t.TcmClient)
                                                                              .ThenInclude(c => c.Client)

                                                                              .Include(t => t.TcmClient)
                                                                              .ThenInclude(c => c.Client)
                                                                              .ThenInclude(cl => cl.LegalGuardian)

                                                                              .Include(t => t.TcmClient)
                                                                              .ThenInclude(c => c.Casemanager)
                                                                              .ThenInclude(cm => cm.Clinic)

                                                                              .AsSplitQuery()

                                                                              .FirstOrDefaultAsync(t => t.TcmClient.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeClientSignatureVerification(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMIDDocumentsVerificationForm(int id)
        {
            TCMIntakeClientIdDocumentVerificationEntity entity = await _context.TCMIntakeClientDocumentVerification

                                                                               .Include(t => t.TcmClient)
                                                                               .ThenInclude(c => c.Client)

                                                                               .Include(t => t.TcmClient)
                                                                               .ThenInclude(c => c.Client)
                                                                               .ThenInclude(cl => cl.LegalGuardian)

                                                                               .Include(t => t.TcmClient)
                                                                               .ThenInclude(c => c.Casemanager)
                                                                               .ThenInclude(cm => cm.Clinic)

                                                                               .AsSplitQuery()

                                                                               .FirstOrDefaultAsync(t => t.TcmClient.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeClientDocumentVerification(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMIntakeNutritionalScreen(int id)
        {
            TCMIntakeNutritionalScreenEntity entity = await _context.TCMIntakeNutritionalScreen

                                                                    .Include(t => t.TcmClient)
                                                                    .ThenInclude(c => c.Client)

                                                                    .Include(t => t.TcmClient)
                                                                    .ThenInclude(c => c.Client)
                                                                    .ThenInclude(cl => cl.LegalGuardian)

                                                                    .Include(t => t.TcmClient)
                                                                    .ThenInclude(c => c.Casemanager)
                                                                    .ThenInclude(cm => cm.Clinic)

                                                                    .AsSplitQuery()

                                                                    .FirstOrDefaultAsync(t => t.TcmClient.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeNutritionalScreen(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMIntakePersonalWellbeing(int id)
        {
            TCMIntakePersonalWellbeingEntity entity = await _context.TCMIntakePersonalWellbeing

                                                                    .Include(t => t.TcmClient)
                                                                    .ThenInclude(c => c.Client)

                                                                    .Include(t => t.TcmClient)
                                                                    .ThenInclude(c => c.Client)
                                                                    .ThenInclude(cl => cl.LegalGuardian)

                                                                    .Include(t => t.TcmClient)
                                                                    .ThenInclude(c => c.Casemanager)
                                                                    .ThenInclude(cm => cm.Clinic)

                                                                    .AsSplitQuery()

                                                                    .FirstOrDefaultAsync(t => t.TcmClient.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakePersonalWellbeing(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMIntakeColumbiaSuicide(int id)
        {
            TCMIntakeColumbiaSuicideEntity entity = await _context.TCMIntakeColumbiaSuicide

                                                                  .Include(t => t.TcmClient)
                                                                  .ThenInclude(c => c.Client)

                                                                  .Include(t => t.TcmClient)
                                                                  .ThenInclude(c => c.Client)
                                                                  .ThenInclude(cl => cl.LegalGuardian)

                                                                  .Include(t => t.TcmClient)
                                                                  .ThenInclude(c => c.Casemanager)
                                                                  .ThenInclude(cm => cm.Clinic)

                                                                  .AsSplitQuery()

                                                                  .FirstOrDefaultAsync(t => t.TcmClient.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeColumbiaSuicide(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMAssessment(int id)
        {
            TCMAssessmentEntity entity = await _context.TCMAssessment

                                                        .Include(t => t.TcmClient)
                                                        .ThenInclude(c => c.Client)
                                                        .ThenInclude(cl => cl.LegalGuardian)                                                                                                              

                                                        .Include(t => t.TcmClient)
                                                        .ThenInclude(c => c.Client)
                                                        .ThenInclude(cl => cl.Doctor)

                                                        .Include(t => t.TCMSupervisor)

                                                        .Include(t => t.TcmClient)
                                                        .ThenInclude(c => c.Client)
                                                        .ThenInclude(cl => cl.Client_Referred)
                                                        .ThenInclude(cr => cr.Referred)

                                                        .Include(t => t.TcmClient)
                                                        .ThenInclude(c => c.Client)
                                                        .ThenInclude(cl => cl.Clients_Diagnostics)
                                                        .ThenInclude(cd => cd.Diagnostic)

                                                        .Include(t => t.TcmClient)
                                                        .ThenInclude(c => c.Casemanager)
                                                        .ThenInclude(cm => cm.Clinic)

                                                        .Include(t => t.DrugList)

                                                        .Include(t => t.HospitalList)

                                                        .Include(t => t.HouseCompositionList)

                                                        .Include(t => t.IndividualAgencyList)

                                                        .Include(t => t.MedicalProblemList)

                                                        .Include(t => t.MedicationList)

                                                        .Include(t => t.PastCurrentServiceList)

                                                        .Include(t => t.SurgeryList)

                                                        .Include(t => t.TcmClient)
                                                        .ThenInclude(c => c.Client)
                                                        .ThenInclude(cl => cl.Psychiatrist)

                                                        .AsSplitQuery()

                                                        .FirstOrDefaultAsync(t => t.TcmClient.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeAssessment(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMIntakePainScreen(int id)
        {
            TCMIntakePainScreenEntity entity = await _context.TCMIntakePainScreen

                                                             .Include(t => t.TcmClient)
                                                             .ThenInclude(c => c.Client)

                                                             .Include(t => t.TcmClient)
                                                             .ThenInclude(c => c.Client)
                                                             .ThenInclude(cl => cl.LegalGuardian)

                                                             .Include(t => t.TcmClient)
                                                             .ThenInclude(c => c.Casemanager)
                                                             .ThenInclude(cm => cm.Clinic)

                                                             .AsSplitQuery()

                                                             .FirstOrDefaultAsync(t => t.TcmClient.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakePainScreen(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMAppendixJ(int id)
        {
            TCMIntakeAppendixJEntity entity = await _context.TCMIntakeAppendixJ

                                                            .Include(t => t.TcmClient)
                                                            .ThenInclude(c => c.Client)

                                                            .Include(t => t.TcmClient)
                                                            .ThenInclude(c => c.Casemanager)
                                                            .ThenInclude(cm => cm.Clinic)

                                                            .Include(t => t.TcmSupervisor)

                                                            .AsSplitQuery()

                                                            .FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeAppendixJ(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintDischarge(int id)
        {
            TCMDischargeEntity entity = await _context.TCMDischarge

                                                      .Include(t => t.TcmServicePlan)
                                                      .ThenInclude(s => s.TcmClient)
                                                      .ThenInclude(c => c.Client)

                                                      .Include(t => t.TcmServicePlan)
                                                      .ThenInclude(s => s.TCMDomain)
                                                      
                                                      .Include(t => t.TcmServicePlan)
                                                      .ThenInclude(t => t.TcmClient)
                                                      .ThenInclude(c => c.Casemanager)
                                                      .ThenInclude(cm => cm.Clinic)

                                                      .Include(t => t.TCMSupervisor)

                                                      .Include(t => t.TcmDischargeFollowUp)

                                                      .Include(t => t.TcmDischargeServiceStatus)

                                                      .AsSplitQuery()

                                                      .FirstOrDefaultAsync(t => t.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMDischarge(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMIntakeMiniMental(int id)
        {
            TCMIntakeMiniMentalEntity entity = await _context.TCMIntakeMiniMental
                                                      
                                                             .Include(s => s.TcmClient)
                                                             .ThenInclude(c => c.Client)

                                                             .Include(t => t.TcmClient)
                                                             .ThenInclude(c => c.Casemanager)
                                                             .ThenInclude(cm => cm.Clinic)

                                                             .AsSplitQuery()

                                                             .FirstOrDefaultAsync(t => t.TcmClient.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeMiniMental(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMMedicalHistory(int id)
        {
            TCMIntakeMedicalHistoryEntity entity = await _context.TCMIntakeMedicalHistory
                                                                 .Include(t => t.TCMClient)
                                                                   .ThenInclude(i => i.Client)
                                                                   
                                                                 .Include(t => t.TCMClient)
                                                                   .ThenInclude(i => i.TCMAssessment)
                                                                   .ThenInclude(a => a.MedicationList)

                                                                 .Include(t => t.TCMClient)
                                                                    .ThenInclude(c => c.Casemanager)
                                                                    .ThenInclude(cm => cm.Clinic)

                                                                 .AsSplitQuery()

                                                                 .FirstOrDefaultAsync(i => (i.TCMClient.Id == id));

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeMedicalHistoryReport(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMIntakeCoordinationCare(int id)
        {
            TCMIntakeCoordinationCareEntity entity = await _context.TCMIntakeCoordinationCare

                                                                   .Include(t => t.TcmClient)
                                                                      .ThenInclude(i => i.Client)
                                                                      .ThenInclude(c => c.LegalGuardian)

                                                                   .Include(t => t.TcmClient)
                                                                      .ThenInclude(c => c.TCMIntakeForm) 

                                                                   .Include(t => t.TcmClient)
                                                                      .ThenInclude(c => c.Casemanager)
                                                                      .ThenInclude(cm => cm.Clinic)                                                                   

                                                                   .AsSplitQuery()

                                                                   .FirstOrDefaultAsync(i => (i.TcmClient.Id == id));

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeCoordinationCare(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }
        
        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> PrintTCMInterventionLog(int id)
        {
            TCMIntakeInterventionLogEntity entity = await _context.TCMIntakeInterventionLog
                                                                  .Include(t => t.TcmClient)
                                                                      .ThenInclude(i => i.Client)                                                                      

                                                                   .Include(i => i.InterventionList)                                                                      

                                                                   .Include(t => t.TcmClient)
                                                                      .ThenInclude(c => c.Casemanager)
                                                                      .ThenInclude(cm => cm.Clinic)

                                                                   .AsSplitQuery()

                                                                   .FirstOrDefaultAsync(i => (i.Id == id));

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Stream stream = _reportHelper.TCMIntakeInterventionLog(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public IActionResult CreateTCMIntakeClientSignatureVerification(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeClientSignatureVerificationViewModel model;
            TCMIntakeClientSignatureVerificationEntity clientSignatureverification = _context.TCMIntakeClientSignatureVerification
                                                                                             .Include(n => n.TcmClient)
                                                                                             .ThenInclude(n => n.Client)
                                                                                             .ThenInclude(n => n.LegalGuardian)
                                                                                             .FirstOrDefault(n => n.TcmClient.Id == id);

            TCMClientEntity tcmCLient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.LegalGuardian)
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.EmergencyContact)
                                                .FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    if (clientSignatureverification == null)
                    {
                        model = new TCMIntakeClientSignatureVerificationViewModel
                        {
                            TcmClient = tcmCLient,
                            IdTCMClient = id,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            TcmClient_FK = id,
                            Id = 0,
                            AdmissionedFor = user_logged.FullName,
                            DateSignatureEmployee = tcmCLient.DataOpen,
                            DateSignatureLegalGuardianOrClient = tcmCLient.DataOpen

                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                       
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        if (clientSignatureverification.TcmClient.Client.LegalGuardian == null)
                            clientSignatureverification.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();
                       
                        model = _converterHelper.ToTCMIntakeClientSignatureVerificationViewModel(clientSignatureverification);
                        ViewData["origi"] = origi;
                       
                        return View(model);
                    }

                }
            }
            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {
                    if (clientSignatureverification.TcmClient.Client.LegalGuardian == null)
                        clientSignatureverification.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();

                    model = _converterHelper.ToTCMIntakeClientSignatureVerificationViewModel(clientSignatureverification);
                    ViewData["origi"] = origi;

                    return View(model);
                }
            }
            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMIntakeClientSignatureVerification(TCMIntakeClientSignatureVerificationViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeClientSignatureVerificationEntity clientSignatureVerificationEntity = _converterHelper.ToTCMIntakeClientSignatureVerificationEntity(IntakeViewModel, false, user_logged.UserName);

                if (clientSignatureVerificationEntity.Id == 0)
                {
                    clientSignatureVerificationEntity.TcmClient = null;
                    _context.TCMIntakeClientSignatureVerification.Add(clientSignatureVerificationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    clientSignatureVerificationEntity.TcmClient = null;
                    _context.TCMIntakeClientSignatureVerification.Update(clientSignatureVerificationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMIntakeClientSignatureVerification", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public IActionResult CreateTCMIntakeClientIdDocumentVerification(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeClientIdDocumentVerificationViewModel model;
            TCMIntakeClientIdDocumentVerificationEntity clientIdDocumentationverification = _context.TCMIntakeClientDocumentVerification
                                                                                                    .Include(n => n.TcmClient)
                                                                                                    .ThenInclude(n => n.Client)
                                                                                                    .ThenInclude(n => n.LegalGuardian)
                                                                                                    .FirstOrDefault(n => n.TcmClient.Id == id);

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.LegalGuardian)
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.EmergencyContact)
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.Clients_HealthInsurances)

                                                .FirstOrDefault(n => n.Id == id);

            string idHealthInsurance = string.Empty;
            if(tcmClient.Client.Clients_HealthInsurances.Count() > 0)
            {
                idHealthInsurance = tcmClient.Client.Clients_HealthInsurances.FirstOrDefault(n => n.Active == true).MemberId.ToString();
            }
            
            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    if (clientIdDocumentationverification == null)
                    {
                        model = new TCMIntakeClientIdDocumentVerificationViewModel
                        {
                            TcmClient = tcmClient,
                            IdTCMClient = id,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            TcmClient_FK = id,
                            Id = 0,
                            AdmissionedFor = user_logged.FullName,
                            DateSignatureEmployee = tcmClient.DataOpen,
                            DateSignatureLegalGuardianOrClient = tcmClient.DataOpen,
                            HealthPlan = idHealthInsurance,
                            Id_DriverLicense = string.Empty,
                            MedicaidId = tcmClient.Client.MedicaidID,
                            MedicareCard = tcmClient.Client.MedicareId,
                            Other_Identification = string.Empty,
                            Other_Name = string.Empty,
                            Passport_Resident = string.Empty,
                            Social = tcmClient.Client.SSN

                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();

                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        if (clientIdDocumentationverification.TcmClient.Client.LegalGuardian == null)
                            clientIdDocumentationverification.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();

                        model = _converterHelper.ToTCMIntakeClientIdDocumentVerificationViewModel(clientIdDocumentationverification);
                        ViewData["origi"] = origi;

                        return View(model);
                    }

                }
            }
            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {
                    if (clientIdDocumentationverification.TcmClient.Client.LegalGuardian == null)
                        clientIdDocumentationverification.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();

                    model = _converterHelper.ToTCMIntakeClientIdDocumentVerificationViewModel(clientIdDocumentationverification);
                    ViewData["origi"] = origi;

                    return View(model);
                }
            }
            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMIntakeClientIdDocumentVerification(TCMIntakeClientIdDocumentVerificationViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeClientIdDocumentVerificationEntity clientIdDocumentVerificationEntity = _converterHelper.ToTCMIntakeClientIdDocumentVerificationEntity(IntakeViewModel, false, user_logged.UserName);

                if (clientIdDocumentVerificationEntity.Id == 0)
                {
                    clientIdDocumentVerificationEntity.TcmClient = null;
                    _context.TCMIntakeClientDocumentVerification.Add(clientIdDocumentVerificationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    clientIdDocumentVerificationEntity.TcmClient = null;
                    _context.TCMIntakeClientDocumentVerification.Update(clientIdDocumentVerificationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMIntakeClientIdDocumentVerification", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public IActionResult CreateTCMIntakePainScreen(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakePainScreenViewModel model;
            TCMIntakePainScreenEntity painScreen = _context.TCMIntakePainScreen
                                                           .Include(n => n.TcmClient)
                                                           .ThenInclude(n => n.Client)
                                                           .ThenInclude(n => n.LegalGuardian)
                                                           .FirstOrDefault(n => n.TcmClient.Id == id);

            TCMClientEntity tcmCLient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.LegalGuardian)
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.EmergencyContact)
                                                .FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    if (painScreen == null)
                    {
                        model = new TCMIntakePainScreenViewModel
                        {
                            TcmClient = tcmCLient,
                            IdTCMClient = id,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            TcmClient_FK = id,
                            Id = 0,
                            AdmissionedFor = user_logged.FullName,
                            DateSignatureEmployee = tcmCLient.DataOpen,
                            AlwayasThere = false,
                            ComesAndGoes = false,
                            CurrentPainScore = 0,
                            DidYouUse = false,
                            DoesYourPainEffect = string.Empty,
                            DoYouBelieve = false,
                            DoYouFell = false,
                            DoYouSuffer = false,
                            WereYourDrugs = false,
                            WhatCauses = string.Empty,
                            WhereIs = string.Empty

                        };
                        
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        model = _converterHelper.ToTCMIntakePainScreenViewModel(painScreen);
                        ViewData["origi"] = origi;

                        return View(model);
                    }

                }
            }
            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {
                    model = _converterHelper.ToTCMIntakePainScreenViewModel(painScreen);
                    ViewData["origi"] = origi;

                    return View(model);
                }
            }
            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMIntakePainScreen(TCMIntakePainScreenViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakePainScreenEntity painScreen = _converterHelper.ToTCMIntakePainScreenEntity(IntakeViewModel, false, user_logged.UserName);

                if (painScreen.Id == 0)
                {
                    painScreen.TcmClient = null;
                    _context.TCMIntakePainScreen.Add(painScreen);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    painScreen.TcmClient = null;
                    _context.TCMIntakePainScreen.Update(painScreen);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMIntakePainScreen", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public IActionResult CreateTCMIntakeColumbiaSuicide(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeColumbiaSuicideViewModel model;
            TCMIntakeColumbiaSuicideEntity columbiaCuicide = _context.TCMIntakeColumbiaSuicide
                                                                     .Include(n => n.TcmClient)
                                                                     .ThenInclude(n => n.Client)
                                                                     .ThenInclude(n => n.LegalGuardian)
                                                                     .FirstOrDefault(n => n.TcmClient.Id == id);

            TCMClientEntity tcmCLient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.LegalGuardian)
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.EmergencyContact)
                                                .FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    
                    if (columbiaCuicide == null)
                    {
                        model = new TCMIntakeColumbiaSuicideViewModel
                        {
                            TcmClient = tcmCLient,
                            IdTCMClient = id,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            TcmClient_FK = id,
                            Id = 0,
                            AdmissionedFor = user_logged.FullName,
                            DateSignatureEmployee = tcmCLient.DataOpen,
                            IdHaveYouWishedPastMonth_Value = 0,
                            IdHaveYouWishedLifeTime_Value = 0,
                            IdHaveYouActuallyPastMonth_Value = 0,
                            IdHaveYouActuallyLifeTime_Value = 0,
                            IdHaveYouBeenPastMonth_Value = 0,
                            IdHaveYouBeenLifeTime_Value = 0,
                            IdHaveYouHadPastMonth_Value = 0,
                            IdHaveYouHadLifeTime_Value = 0,
                            IdHaveYouStartedPastMonth_Value = 0,
                            IdHaveYouStartedLifeTime_Value = 0,
                            IdHaveYouEver_Value = 0,
                            IdHaveYouEverIfYes_Value = 0,
                            RiskList = _combosHelper.GetComboRisk()

                        };

                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        model = _converterHelper.ToTCMIntakeColumbiaSuicideViewModel(columbiaCuicide);
                        ViewData["origi"] = origi;

                        return View(model);
                    }

                }
            }
            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {
                    model = _converterHelper.ToTCMIntakeColumbiaSuicideViewModel(columbiaCuicide);
                    ViewData["origi"] = origi;

                    return View(model);
                }
            }
            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMIntakeColumbiaSuicide(TCMIntakeColumbiaSuicideViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeColumbiaSuicideEntity columbiaSuicide = _converterHelper.ToTCMIntakeColumbiaSuicideEntity(IntakeViewModel, false, user_logged.UserName);

                if (columbiaSuicide.Id == 0)
                {
                    columbiaSuicide.TcmClient = null;
                    _context.TCMIntakeColumbiaSuicide.Add(columbiaSuicide);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    columbiaSuicide.TcmClient = null;
                    _context.TCMIntakeColumbiaSuicide.Update(columbiaSuicide);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMIntakeColumbiaCuicide", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public IActionResult CreateTCMIntakePersonalWellbeing(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakePersonalWellbeingViewModel model;
            TCMIntakePersonalWellbeingEntity personalWellbeing = _context.TCMIntakePersonalWellbeing
                                                                         .Include(n => n.TcmClient)
                                                                         .ThenInclude(n => n.Client)
                                                                         .ThenInclude(n => n.LegalGuardian)
                                                                         .FirstOrDefault(n => n.TcmClient.Id == id);

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.LegalGuardian)
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.EmergencyContact)
                                                .FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                   
                    if (personalWellbeing == null)
                    {
                        model = new TCMIntakePersonalWellbeingViewModel
                        {
                            TcmClient = tcmClient,
                            IdTCMClient = id,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            TcmClient_FK = id,
                            Id = 0,
                            AdmissionedFor = user_logged.FullName,
                            DateSignatureEmployee = tcmClient.DataOpen,
                           
                        };

                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        model = _converterHelper.ToTCMIntakePersonalWellbeingViewModel(personalWellbeing);
                        ViewData["origi"] = origi;

                        return View(model);
                    }

                }
            }
            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {
                    model = _converterHelper.ToTCMIntakePersonalWellbeingViewModel(personalWellbeing);
                    ViewData["origi"] = origi;

                    return View(model);
                }
            }
            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMIntakePersonalWellbeing(TCMIntakePersonalWellbeingViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakePersonalWellbeingEntity personalWellbeing = _converterHelper.ToTCMIntakePersonalWellbeingEntity(IntakeViewModel, false, user_logged.UserName);

                if (personalWellbeing.Id == 0)
                {
                    personalWellbeing.TcmClient = null;
                    _context.TCMIntakePersonalWellbeing.Add(personalWellbeing);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    personalWellbeing.TcmClient = null;
                    _context.TCMIntakePersonalWellbeing.Update(personalWellbeing);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMIntakePersonalWellbeing", IntakeViewModel) });
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public IActionResult CreateTCMIntakeNutritionalScreen(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeNutritionalScreenViewModel model;
            TCMIntakeNutritionalScreenEntity nutritionalScreen = _context.TCMIntakeNutritionalScreen
                                                                         .Include(n => n.TcmClient)
                                                                         .ThenInclude(n => n.Client)
                                                                         .ThenInclude(n => n.LegalGuardian)
                                                                         .FirstOrDefault(n => n.TcmClient.Id == id);
            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.LegalGuardian)
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.EmergencyContact)
                                                .FirstOrDefault(n => n.Id == id);
            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    
                    if (nutritionalScreen == null)
                    {
                        model = new TCMIntakeNutritionalScreenViewModel
                        {
                            TcmClient = tcmClient,
                            IdTCMClient = id,
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            TcmClient_FK = id,
                            Id = 0,
                            AdmissionedFor = user_logged.FullName,
                            DateSignatureEmployee = tcmClient.DataOpen,

                        };

                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        model = _converterHelper.ToTCMIntakeNutritionalScreenViewModel(nutritionalScreen);
                        ViewData["origi"] = origi;

                        return View(model);
                    }

                }
            }
            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {
                    model = _converterHelper.ToTCMIntakeNutritionalScreenViewModel(nutritionalScreen);
                    ViewData["origi"] = origi;

                    return View(model);
                }
            }
            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMIntakeNutritionalScreen(TCMIntakeNutritionalScreenViewModel IntakeViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeNutritionalScreenEntity nutritionalScreen = _converterHelper.ToTCMIntakeNutritionalScreenEntity(IntakeViewModel, false, user_logged.UserName);

                if (nutritionalScreen.Id == 0)
                {
                    nutritionalScreen.TcmClient = null;
                    _context.TCMIntakeNutritionalScreen.Add(nutritionalScreen);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    nutritionalScreen.TcmClient = null;
                    _context.TCMIntakeNutritionalScreen.Update(nutritionalScreen);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMIntakeSectionDashboard", new { id = IntakeViewModel.IdTCMClient, section = 1, origin = origi });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            //Preparing Data
            IntakeViewModel.TcmClient = _context.TCMClient.Find(IntakeViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMIntakeNutritionalScreen", IntakeViewModel) });
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> UpdateAppendixJ()
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic || !user_logged.Clinic.Setting.TCMSupervisorEdit)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            else
            {
                if (User.IsInRole("TCMSupervisor"))
                    return View(await _context.TCMClient

                                              .Include(f => f.TcmIntakeAppendixJ)
                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clinic)
                                              .Include(f => f.Casemanager)
                                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id
                                                       && n.Casemanager.TCMSupervisor.Id == _context.TCMSupervisors.FirstOrDefault(m => m.LinkedUser == user_logged.UserName).Id
                                                       && n.TcmIntakeAppendixJ.Where(a => a.Approved == 2).Count() > 0)
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "TCMSupervisor, CaseManager")]
        public async Task<IActionResult> EditAppendixJ(int? id, int origi = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            int edit = 0;

            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMIntakeAppendixJViewModel model = new TCMIntakeAppendixJViewModel();
            TCMIntakeAppendixJEntity entity = new TCMIntakeAppendixJEntity();

            if (User.IsInRole("CaseManager"))
            {
                entity = await _context.TCMIntakeAppendixJ
                                       .Include(c => c.TcmClient)
                                       .ThenInclude(c => c.Client)
                                       .ThenInclude(c => c.Clinic)
                                       .FirstOrDefaultAsync(s => s.Id == id );
            }
            if (User.IsInRole("TCMSupervisor"))
            {
                entity = await _context.TCMIntakeAppendixJ
                                       .Include(c => c.TcmClient)
                                       .ThenInclude(c => c.Client)
                                       .ThenInclude(c => c.Clinic)
                                       .FirstOrDefaultAsync(s => s.Id == id);
                if (user_logged.Clinic.Setting.TCMSupervisorEdit == false)
                {
                    edit = 1;
                }
            }
           
            if (entity.Id == 0)
            {
                return RedirectToAction("Home/Error404");
            }

            ViewData["edit"] = edit;
            ViewData["origi"] = origi;
            model = _converterHelper.ToTCMIntakeAppendixJViewModel(entity);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "TCMSupervisor, CaseManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAppendixJ(int id, TCMIntakeAppendixJViewModel model, int origi = 0)
        {
            if (id != model.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                TCMIntakeAppendixJEntity appendixJ = await _converterHelper.ToTCMIntakeAppendixJEntity(model, false, user_logged.UserName);
                
                if (User.IsInRole("TCMSupervisor"))
                {
                    appendixJ.Approved = 2;
                }
                
                _context.Update(appendixJ);
                await _context.SaveChangesAsync();

                if (origi == 0)
                {
                    return RedirectToAction("ListAppendixJForTCMClient", "TCMIntakes", new { id = appendixJ.TcmClient_FK, origi = origi });
                }
                else
                {

                    return RedirectToAction("UpdateAppendixJ");
                }
                
            }

            return View(model);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteAppendixJ(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMIntakeAppendixJEntity appendixJ = await _context.TCMIntakeAppendixJ
                                                               .Include(n => n.TcmClient)
                                                               .FirstOrDefaultAsync(d => d.Id == id);

            if (appendixJ == null)
            {
                return RedirectToAction("Home/Error404");
            }

            _context.TCMIntakeAppendixJ.Remove(appendixJ);
            await _context.SaveChangesAsync();

            return RedirectToAction("TCMCaseHistory", "TCMClients", new { id = appendixJ.TcmClient.Id });

        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> AppendixJReturnTo(int? id, int tcmClientId = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMIntakeAppendixJEntity appendixJ = await _context.TCMIntakeAppendixJ.FirstOrDefaultAsync(s => s.Id == id);
            if (appendixJ == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                appendixJ.Approved = 1;
                _context.TCMIntakeAppendixJ.Update(appendixJ);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("TCMCaseHistory", "TCMClients", new { id = tcmClientId });
        }

        [Authorize(Roles = "Manager, Frontdesk, TCMSupervisor, CaseManager")]
        public IActionResult AuditConsentForRelease(int idTCMClient = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic || !user_logged.Clinic.Setting.MHProblems)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<AuditTCMConsentForrelease> auditConsent_List = new List<AuditTCMConsentForrelease>();
            AuditTCMConsentForrelease auditConsent = new AuditTCMConsentForrelease();

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(n => n.TcmIntakeConsentForRelease)
                                                .FirstOrDefault(n => n.Id == idTCMClient);
            //apertura
            TCMIntakeConsentForReleaseEntity consentPCP = tcmClient.TcmIntakeConsentForRelease.FirstOrDefault(n => n.ConsentType == ConsentType.PCP);
            if (consentPCP != null)
            {
                auditConsent.Name = "PCP";
                auditConsent.Origin = "Initial";
                auditConsent.Date = consentPCP.DateSignatureEmployee.ToShortDateString();
                if (consentPCP.DateSignatureEmployee.Date == tcmClient.DataOpen.Date)
                {
                    auditConsent.Active = 2;
                }
                else 
                {
                    auditConsent.Active = 1;
                }
            }
            else
            {
                auditConsent.Name = "PCP";
                auditConsent.Origin = "Initial";
                auditConsent.Date = string.Empty;
                auditConsent.Active = 0;
            }
            auditConsent_List.Add(auditConsent);
            auditConsent = new AuditTCMConsentForrelease();

            TCMIntakeConsentForReleaseEntity consentPsy = tcmClient.TcmIntakeConsentForRelease.FirstOrDefault(n => n.ConsentType == ConsentType.PSYCHIATRIST);
            if (consentPsy != null)
            {
                auditConsent.Name = "PSYCHIATRIST";
                auditConsent.Origin = "Initial";
                auditConsent.Date = consentPsy.DateSignatureEmployee.ToShortDateString();

                if (consentPsy.DateSignatureEmployee.Date == tcmClient.DataOpen.Date)
                {
                    auditConsent.Active = 2;
                }
                else
                {
                    auditConsent.Active = 1;
                }
            }
            else
            {
                auditConsent.Name = "PSYCHIATRIST";
                auditConsent.Origin = "Initial";
                auditConsent.Date = string.Empty;
                auditConsent.Active = 0;
            }
            auditConsent_List.Add(auditConsent);
            auditConsent = new AuditTCMConsentForrelease();

            TCMIntakeConsentForReleaseEntity emergencyContact = tcmClient.TcmIntakeConsentForRelease.FirstOrDefault(n => n.ConsentType == ConsentType.EMERGENCY_CONTACT);
            if (emergencyContact != null)
            {
                auditConsent.Name = "EMERGENCY CONTACT";
                auditConsent.Origin = "Initial";
                auditConsent.Date = emergencyContact.DateSignatureEmployee.ToShortDateString();

                if (consentPCP.DateSignatureEmployee.Date == tcmClient.DataOpen.Date)
                {
                    auditConsent.Active = 2;
                }
                else
                {
                    auditConsent.Active = 1;
                }
            }
            else
            {
                auditConsent.Name = "EMERGENCY CONTACT";
                auditConsent.Origin = "Initial";
                auditConsent.Date = string.Empty;
                auditConsent.Active = 0;
            }
            auditConsent_List.Add(auditConsent);
            auditConsent = new AuditTCMConsentForrelease();

            TCMIntakeConsentForReleaseEntity social = tcmClient.TcmIntakeConsentForRelease.FirstOrDefault(n => n.ConsentType == ConsentType.SSA);
            if (social != null)
            {
                auditConsent.Name = "SSA";
                auditConsent.Origin = "Initial";
                auditConsent.Date = social.DateSignatureEmployee.ToShortDateString();

                if (consentPCP.DateSignatureEmployee.Date == tcmClient.DataOpen.Date)
                {
                    auditConsent.Active = 2;
                }
                else
                {
                    auditConsent.Active = 1;
                }
            }
            else
            {
                auditConsent.Name = "SSA";
                auditConsent.Origin = "Initial";
                auditConsent.Date = string.Empty;
                auditConsent.Active = 0;
            }
            auditConsent_List.Add(auditConsent);
            auditConsent = new AuditTCMConsentForrelease();

            TCMIntakeConsentForReleaseEntity DCF = tcmClient.TcmIntakeConsentForRelease.FirstOrDefault(n => n.ConsentType == ConsentType.DCF);
            if (DCF != null)
            {
                auditConsent.Name = "DCF";
                auditConsent.Origin = "Initial";
                auditConsent.Date = DCF.DateSignatureEmployee.ToShortDateString();

                if (consentPCP.DateSignatureEmployee.Date == tcmClient.DataOpen.Date)
                {
                    auditConsent.Active = 2;
                }
                else
                {
                    auditConsent.Active = 1;
                }
            }
            else
            {
                auditConsent.Name = "DCF";
                auditConsent.Origin = "Initial";
                auditConsent.Date = string.Empty;
                auditConsent.Active = 0;
            }
            auditConsent_List.Add(auditConsent);
            auditConsent = new AuditTCMConsentForrelease();

            return View(auditConsent_List);
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> DeleteConsentForRelease(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMIntakeConsentForReleaseEntity consent = await _context.TCMIntakeConsentForRelease
                                                                        .Include(c => c.TcmClient)
                                                                     .FirstOrDefaultAsync(c => c.Id == id);

            if (consent == null)
            {
                return RedirectToAction("Home/Error404");
            }

            _context.TCMIntakeConsentForRelease.Remove(consent);
            await _context.SaveChangesAsync();

            return RedirectToAction("ListConsentForrelease", "TCMIntakes", new { id = consent.TcmClient.Id });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateTCMAppendixI(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeAppendixIViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeAppendixIEntity intakeAppendixI = _context.TCMIntakeAppendixI
                                                                       .Include(n => n.TcmClient)
                                                                       .ThenInclude(n => n.Client)
                                                                       .FirstOrDefault(n => n.TcmClient.Id == id);
                    if (intakeAppendixI == null)
                    {
                        model = new TCMIntakeAppendixIViewModel
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
                            SupervisorSignatureDate = DateTime.Now,
                            TcmSupervisor = new TCMSupervisorEntity(),
                            HasAmental2 = true,
                            HasAmental6 = true,
                            RequiresServices = true,
                            RequiresOngoing = true,
                            Lacks = true,
                            IsNot = true,
                            IsInOut = true,
                            IsEnrolled = true,
                            HasRecolated = true
                        
                        };
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        model = _converterHelper.ToTCMIntakeAppendixIViewModel(intakeAppendixI);
                        ViewData["origi"] = origi;
                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateTCMAppendixI(TCMIntakeAppendixIViewModel AppendixIViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeAppendixIEntity AppendixIEntity = await _converterHelper.ToTCMIntakeAppendixIEntity(AppendixIViewModel, false, user_logged.UserName);

                if (AppendixIEntity.Id == 0)
                {
                    AppendixIEntity.TcmClient = null;
                    AppendixIEntity.Approved = 1;
                    _context.TCMIntakeAppendixI.Add(AppendixIEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (origi == 0)
                        {
                            return RedirectToAction("TCMIntakeSectionDashboard", new { id = AppendixIViewModel.IdTCMClient, section = 4 });
                        }
                        if (origi == 1)
                        {
                            return RedirectToAction("GetCaseNotServicePlan", "TCMClients");
                        }

                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    AppendixIEntity.TcmClient = null;
                    AppendixIEntity.Approved = 1;
                    _context.TCMIntakeAppendixI.Update(AppendixIEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (origi == 0)
                        {
                            return RedirectToAction("TCMIntakeSectionDashboard", new { id = AppendixIViewModel.IdTCMClient, section = 4 });
                        }
                        if (origi == 1)
                        {
                            return RedirectToAction("GetCaseNotServicePlan", "TCMClients");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            AppendixIViewModel.TcmClient = _context.TCMClient.Find(AppendixIViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMAppendixI", AppendixIViewModel) });
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> ApendiceIStatus(int approved = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<TCMIntakeAppendixIEntity> apendiceI = new List<TCMIntakeAppendixIEntity>();

            if (User.IsInRole("CaseManager"))
            {
                apendiceI = await _context.TCMIntakeAppendixI
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
                if (User.IsInRole("TCMSupervisor"))
                {
                    apendiceI = await _context.TCMIntakeAppendixI
                                              .Include(w => w.TcmClient)
                                              .ThenInclude(d => d.Client)
                                              .ThenInclude(d => d.Clinic)
                                              .Include(w => w.TcmClient)
                                              .ThenInclude(d => d.Casemanager)
                                              .Where(w => (w.TcmClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                                        && w.Approved == approved))
                                              .ToListAsync();
                }
                else
                {
                    apendiceI = await _context.TCMIntakeAppendixI
                                             .Include(w => w.TcmClient)
                                             .ThenInclude(d => d.Client)
                                             .ThenInclude(d => d.Clinic)
                                             .Include(w => w.TcmClient)
                                             .ThenInclude(d => d.Casemanager)
                                             .Where(w => (w.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                       && w.Approved == approved))
                                             .ToListAsync();
                }

            }

            return View(apendiceI);
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> AproveAppendixI(int id, TCMIntakeAppendixIViewModel model, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeAppendixIEntity tcmAppendixI = await _converterHelper.ToTCMIntakeAppendixIEntity(model, false, user_logged.UserName);

            if (tcmAppendixI != null)
            {
                if (User.IsInRole("TCMSupervisor"))
                {


                    if (user_logged.Clinic != null)
                    {
                        tcmAppendixI.Approved = 2;
                        tcmAppendixI.TcmSupervisor = tcmAppendixI.TcmClient.Casemanager.TCMSupervisor;

                        _context.Update(tcmAppendixI);
                        try
                        {
                            await _context.SaveChangesAsync();
                            if (origi == 0)
                            {
                                return RedirectToAction("ApendiceIStatus", "TCMIntakes", new { approved = 1 });
                            }
                            if (origi == 1)
                            {
                                return RedirectToAction("TCMIntakeSectionDashboardReadOnly", new { id = tcmAppendixI.TcmClient_FK, section = 4 });
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

            return RedirectToAction("Index", "TCMIntakes");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult CreateTCMAppendixIReadOnly(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeAppendixIViewModel model;

            if (User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeAppendixIEntity intakeAppendixI = _context.TCMIntakeAppendixI
                                                                       .Include(n => n.TcmClient)
                                                                       .ThenInclude(n => n.Client)
                                                                       .FirstOrDefault(n => n.TcmClient.Id == id);
                    if (intakeAppendixI == null)
                    {
                        model = new TCMIntakeAppendixIViewModel
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
                            SupervisorSignatureDate = DateTime.Now,
                            TcmSupervisor = new TCMSupervisorEntity()

                        };

                        ViewData["origi"] = origi;
                        return View(model);
                    }
                    else
                    {
                        model = _converterHelper.ToTCMIntakeAppendixIViewModel(intakeAppendixI);
                        ViewData["origi"] = origi;
                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> UpdateAppendixI()
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic || !user_logged.Clinic.Setting.TCMSupervisorEdit)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            else
            {
                if (User.IsInRole("TCMSupervisor"))
                    return View(await _context.TCMClient

                                              .Include(f => f.TcmIntakeAppendixI)
                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clinic)
                                              .Include(f => f.Casemanager)
                                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id
                                                       && n.Casemanager.TCMSupervisor.Id == _context.TCMSupervisors.FirstOrDefault(m => m.LinkedUser == user_logged.UserName).Id
                                                       && n.TcmIntakeAppendixI.Approved == 2)
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> EditAppendixI(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMIntakeAppendixIEntity entity = await _context.TCMIntakeAppendixI
                                                            .Include(c => c.TcmClient)
                                                            .ThenInclude(c => c.Client)
                                                            .ThenInclude(c => c.Clinic)
                                                            .FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMIntakeAppendixIViewModel model = _converterHelper.ToTCMIntakeAppendixIViewModel(entity);
            ViewData["origi"] = 0;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAppendixI(int id, TCMIntakeAppendixIViewModel model)
        {
            if (id != model.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                TCMIntakeAppendixIEntity appendixI = await _converterHelper.ToTCMIntakeAppendixIEntity(model, false, user_logged.UserName);
                appendixI.Approved = 2;
                _context.Update(appendixI);
                await _context.SaveChangesAsync();

                return RedirectToAction("UpdateAppendixI");
            }

            return View(model);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteAppendixI(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMIntakeAppendixIEntity appendixI = await _context.TCMIntakeAppendixI
                                                               .Include(n => n.TcmClient)
                                                               .FirstOrDefaultAsync(d => d.Id == id);

            if (appendixI == null)
            {
                return RedirectToAction("Home/Error404");
            }

            _context.TCMIntakeAppendixI.Remove(appendixI);
            await _context.SaveChangesAsync();

            return RedirectToAction("TCMCaseHistory", "TCMClients", new { id = appendixI.TcmClient.Id });

        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> AppendixIReturnTo(int? id, int tcmClientId = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMIntakeAppendixIEntity appendixI = await _context.TCMIntakeAppendixI.FirstOrDefaultAsync(s => s.Id == id);
            if (appendixI == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                appendixI.Approved = 0;
                _context.TCMIntakeAppendixI.Update(appendixI);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("TCMCaseHistory", "TCMClients", new { id = tcmClientId });
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> EditAppendixIReadOnly(int? id, int origi = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMIntakeAppendixIEntity entity = await _context.TCMIntakeAppendixI
                                                            .Include(c => c.TcmClient)
                                                            .ThenInclude(c => c.Client)
                                                            .ThenInclude(c => c.Clinic)
                                                            .FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMIntakeAppendixIViewModel model = _converterHelper.ToTCMIntakeAppendixIViewModel(entity);
            ViewData["origi"] = origi;
            return View(model);
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> EditAppendixJReadOnly(int? id, int origi = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMIntakeAppendixJEntity entity = await _context.TCMIntakeAppendixJ
                                                            .Include(c => c.TcmClient)
                                                            .ThenInclude(c => c.Client)
                                                            .ThenInclude(c => c.Clinic)
                                                            .FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMIntakeAppendixJViewModel model = _converterHelper.ToTCMIntakeAppendixJViewModel(entity);
            ViewData["origi"] = origi;
            return View(model);
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult EditTCMIntervention(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeInterventionViewModel model;

            if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor") || User.IsInRole("Manager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeInterventionEntity intervention = _context.TCMIntakeIntervention
                                                                       .Include(n => n.TcmInterventionLog)
                                                                       .ThenInclude(n => n.TcmClient)
                                                                       .ThenInclude(n => n.Client)
                                                                       .Include(n => n.TcmInterventionLog.TcmClient.Casemanager)
                                                                       .FirstOrDefault(n => n.Id == id);
                    if (intervention != null)
                    {
                        model = _converterHelper.ToTCMIntakeInterventionViewModel(intervention);

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
        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public async Task<IActionResult> EditTCMIntervention(TCMIntakeInterventionViewModel interventionViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMIntakeInterventionEntity InterventionEntity = await _converterHelper.ToTCMIntakeInterventionEntity(interventionViewModel, false, user_logged.UserName);

                _context.TCMIntakeIntervention.Update(InterventionEntity);
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

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditTCMInterventionLog", interventionViewModel) });
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> DeleteTCMIntervention(int id = 0)
        {
            TCMIntakeInterventionEntity intervention = _context.TCMIntakeIntervention
                                                               .Include(n => n.TcmInterventionLog)
                                                               
                                                               .FirstOrDefault(m => m.Id == id);
            if (intervention == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMIntakeIntervention.Remove(intervention);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

            }
            return RedirectToAction("EditTCMInterventionLog", "TCMIntakes", new { id = intervention.TcmInterventionLog.Id });
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public IActionResult TCMMiniMentalReadOnly(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeMiniMentalViewModel model;

            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
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

        [Authorize(Roles = "TCMSupervisor, Manager")]
        public IActionResult TCMMedicalhistoryReadOnly(int id = 0, int idTCMClient = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeMedicalHistoryViewModel model;

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.Doctor)
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.LegalGuardian)
                                                .FirstOrDefault(n => n.Id == idTCMClient);

            if (User.IsInRole("TCMSupervisor") || User.IsInRole("Manager"))
            {
                if (user_logged.Clinic != null)
                {
                    TCMIntakeMedicalHistoryEntity intakeMedicalHistory = _context.TCMIntakeMedicalHistory
                                                                                 .Include(n => n.TCMClient)
                                                                                 .ThenInclude(n => n.Client)
                                                                                 .ThenInclude(n => n.LegalGuardian)
                                                                                 .FirstOrDefault(n => n.TCMClient.Id == idTCMClient);
                    DoctorEntity doctor = _context.Clients.FirstOrDefault(n => n.Id == id).Doctor;
                    if (doctor == null)
                    {
                        doctor = new DoctorEntity();
                    }
                    if (intakeMedicalHistory == null)
                    {
                        model = new TCMIntakeMedicalHistoryViewModel
                        {
                            TCMClient = _context.TCMClient.Include(n => n.Client).ThenInclude(n => n.LegalGuardian).FirstOrDefault(n => n.Id == idTCMClient),
                            TCMClient_FK = id,
                            Id = 0,
                            DateSignatureEmployee = DateTime.Now,
                            DateSignatureLegalGuardian = DateTime.Now,
                            DateSignaturePerson = DateTime.Now,
                            Documents = true,

                            AddressPhysician = doctor.Address,
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
                            City = doctor.City,
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
                            PrimaryCarePhysician = doctor.Name,
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
                            State = doctor.State,
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
                            ZipCode = doctor.ZipCode,
                            AgeOfFirstMenstruation = "",
                            DateOfLastBreastExam = "",
                            DateOfLastPelvic = "",
                            DateOfLastPeriod = "",
                            UsualDurationOfPeriods = "",
                            UsualIntervalBetweenPeriods = "",
                            AdmissionedFor = user_logged.FullName

                        };
                        if (model.TCMClient.Client.LegalGuardian == null)
                            model.TCMClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model.IdTCMClient = idTCMClient;
                        ViewData["CaseNumber"] = tcmClient.CaseNumber;
                        return View(model);
                    }
                    else
                    {
                        if (intakeMedicalHistory.TCMClient.Client.LegalGuardian == null)
                            intakeMedicalHistory.TCMClient.Client.LegalGuardian = new LegalGuardianEntity();
                        model = _converterHelper.ToTCMIntakeMedicalHistoryViewModel(intakeMedicalHistory);
                        model.IdTCMClient = idTCMClient;
                        ViewData["CaseNumber"] = tcmClient.CaseNumber;
                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "Intakes");
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public IActionResult TCMCoordinationCareReadOnly(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMIntakeCoordinationCareViewModel model;

            if (User.IsInRole("Manager") || User.IsInRole("TCMSupervisor"))
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
                                                                                 .ThenInclude(n => n.TCMIntakeForm)
                                                                                 .FirstOrDefault(n => n.TcmClient.Id == id);

                    if (intakeCoordination == null)
                    {
                        TCMClientEntity tcmClient = _context.TCMClient
                                                            .Include(d => d.Client)
                                                            .ThenInclude(d => d.LegalGuardian)
                                                            .Include(d => d.Casemanager)
                                                            .ThenInclude(d => d.Clinic)
                                                            .Include(n => n.TCMIntakeForm)
                                                            .FirstOrDefault(n => n.Id == id);
                        if (tcmClient.TCMIntakeForm == null)
                            tcmClient.TCMIntakeForm = new TCMIntakeFormEntity();

                        model = new TCMIntakeCoordinationCareViewModel
                        {
                            TcmClient = tcmClient,
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
                            SpecialistText = "",
                            PCP_Name = tcmClient.TCMIntakeForm.PCP_Name,
                            PCP_Address = tcmClient.TCMIntakeForm.PCP_Address,
                            PCP_Phone = tcmClient.TCMIntakeForm.PCP_Phone,
                            PCP_CityStateZip = tcmClient.TCMIntakeForm.PCP_CityStateZip,
                            PCP_FaxNumber = tcmClient.TCMIntakeForm.PCP_FaxNumber,

                        };
                        if (model.TcmClient.Client.LegalGuardian == null)
                            model.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();

                        return View(model);
                    }
                    else
                    {
                        if (intakeCoordination.TcmClient.Client.LegalGuardian == null)
                            intakeCoordination.TcmClient.Client.LegalGuardian = new LegalGuardianEntity();

                        model = _converterHelper.ToTCMIntakeCoordinationCareViewModel(intakeCoordination);

                        if (intakeCoordination.TcmClient.TCMIntakeForm != null)
                        {
                            model.PCP_Name = intakeCoordination.TcmClient.TCMIntakeForm.PCP_Name;
                            model.PCP_Address = intakeCoordination.TcmClient.TCMIntakeForm.PCP_Address;
                            model.PCP_Phone = intakeCoordination.TcmClient.TCMIntakeForm.PCP_Phone;
                            model.PCP_CityStateZip = intakeCoordination.TcmClient.TCMIntakeForm.PCP_CityStateZip;
                            model.PCP_FaxNumber = intakeCoordination.TcmClient.TCMIntakeForm.PCP_FaxNumber;
                        }
                        return View(model);
                    }

                }
            }

            return RedirectToAction("Index", "TCMIntakes");
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> TCMMedicationList(int idError = 0, int id = 0, int idTCMClient = 0)
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
                List<TCMAssessmentMedicationEntity> medication = await _context.TCMAssessmentMedication
                                                                                  .Include(n => n.TcmAssessment)
                                                                                  .ThenInclude(n => n.TcmClient)
                                                                                  .AsSplitQuery()
                                                                                  .Where(n => n.TcmAssessment.TcmClient.Id == idTCMClient)
                                                                                  .ToListAsync();

                ViewData["idclient"] = idTCMClient;
                return View(medication);
            }           
        }

        [Authorize(Roles = "Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> DeleteTCMMedicationModal(int id = 0)
        {
            TCMAssessmentMedicationEntity medication = _context.TCMAssessmentMedication
                                                               .Include(n => n.TcmAssessment)
                                                               .ThenInclude(n => n.TcmClient)

                                                               .FirstOrDefault(m => m.Id == id);
            if (medication == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMAssessmentMedication.Remove(medication);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

            }
            return RedirectToAction("TCMMedicationList", "TCMIntakes", new { idTCMClient = medication.TcmAssessment.TcmClient_FK });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult CreateTCMMedicationModal(int id = 0, int idTCMClient = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(n => n.TCMAssessment)
                                                .ThenInclude(n => n.MedicationList)
                                                .Include(n => n.Client)
                                                .FirstOrDefault(n => n.Id == idTCMClient);

            TCMAssessmentMedicationViewModel model = new TCMAssessmentMedicationViewModel();

            if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor") || User.IsInRole("Manager"))
            {
                if (user_logged.Clinic != null)
                {

                    model = new TCMAssessmentMedicationViewModel
                    {
                        Id = 0,
                        Dosage = "",
                        Frequency = "",
                        Name = "",
                        Prescriber = "",
                        TcmAssessment = tcmClient.TCMAssessment,
                        IdTCMAssessment = (tcmClient.TCMAssessment == null) ? 0 : tcmClient.TCMAssessment.Id,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Today

                    };

                    return View(model);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public async Task<IActionResult> CreateTCMMedicationModal(TCMAssessmentMedicationViewModel MedicationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMAssessmentEntity assessment = await _context.TCMAssessment.FirstOrDefaultAsync(n => n.Id == MedicationViewModel.IdTCMAssessment);
            TCMAssessmentMedicationEntity medicationEntity = _context.TCMAssessmentMedication.Find(MedicationViewModel.Id);
            if (ModelState.IsValid)
            {
                
                if (medicationEntity == null)
                {
                    medicationEntity = await _converterHelper.ToTCMAssessmenMedicationEntity(MedicationViewModel, true, user_logged.UserName);
                    _context.TCMAssessmentMedication.Add(medicationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMMedicationList", _context.TCMAssessmentMedication.Where(n => n.TcmAssessment.Id == MedicationViewModel.IdTCMAssessment)) });
                        
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Medication.");
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMMedicationList", _context.TCMAssessmentMedication.Where(n => n.TcmAssessment.Id == MedicationViewModel.IdTCMAssessment)) });
                    // return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMMedication", MedicationViewModel, new { id = MedicationViewModel.IdClient, IdTCMClient = MedicationViewModel.IdTCMClient }) });
                }
            }
            TCMAssessmentMedicationViewModel model;
            model = new TCMAssessmentMedicationViewModel
            {
                TcmAssessment = _context.TCMAssessment.Find(MedicationViewModel.IdTCMAssessment),
                IdTCMAssessment = MedicationViewModel.IdTCMAssessment,
                Id = MedicationViewModel.Id,
                Dosage = MedicationViewModel.Dosage,
                Frequency = MedicationViewModel.Frequency,
                Name = MedicationViewModel.Name,
                Prescriber = MedicationViewModel.Prescriber

            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMMedicationList", _context.TCMAssessmentMedication.Where(n => n.TcmAssessment.Id == MedicationViewModel.IdTCMAssessment)) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public IActionResult EditTCMMedicationModal(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMAssessmentMedicationEntity medication = _context.TCMAssessmentMedication
                                                               .Include(n => n.TcmAssessment)
                                                               .ThenInclude(n => n.TcmClient)
                                                               .ThenInclude(n => n.Client)
                                                               .FirstOrDefault(n => n.Id == id);

            TCMAssessmentMedicationViewModel model;

            if (User.IsInRole("CaseManager") || User.IsInRole("TCMSupervisor") || User.IsInRole("Manager"))
            {
                model = _converterHelper.ToTCMAssessmentMedicationViewModel(medication);
                return View(model);
            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public async Task<IActionResult> EditTCMMedicationModal(TCMAssessmentMedicationViewModel MedicationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMAssessmentMedicationEntity medicationEntity = new TCMAssessmentMedicationEntity();
            if (ModelState.IsValid)
            {

                if (MedicationViewModel.Id > 0)
                {
                    medicationEntity = await _converterHelper.ToTCMAssessmenMedicationEntity(MedicationViewModel, false, user_logged.UserName);
                    _context.Update(medicationEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMMedicationList", _context.TCMAssessmentMedication.Where(n => n.TcmAssessment.Id == MedicationViewModel.IdTCMAssessment)) });

                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Medication.");
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMMedicationList", _context.TCMAssessmentMedication.Where(n => n.TcmAssessment.Id == MedicationViewModel.IdTCMAssessment)) });
                    // return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateTCMMedication", MedicationViewModel, new { id = MedicationViewModel.IdClient, IdTCMClient = MedicationViewModel.IdTCMClient }) });
                }
            }
            TCMAssessmentMedicationViewModel model;
            model = new TCMAssessmentMedicationViewModel
            {
                TcmAssessment = _context.TCMAssessment.Find(MedicationViewModel.IdTCMAssessment),
                IdTCMAssessment = MedicationViewModel.IdTCMAssessment,
                Id = MedicationViewModel.Id,
                Dosage = MedicationViewModel.Dosage,
                Frequency = MedicationViewModel.Frequency,
                Name = MedicationViewModel.Name,
                Prescriber = MedicationViewModel.Prescriber

            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMMedicationList", _context.TCMAssessmentMedication.Where(n => n.TcmAssessment.Id == MedicationViewModel.IdTCMAssessment)) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public async Task<IActionResult> ListAppendixJForTCMClient(int id = 0, int origi = 0)
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
                List<TCMIntakeAppendixJEntity> listAppendixJ = await _context.TCMIntakeAppendixJ
                                                                             .Include(n => n.TcmClient)
                                                                             .ThenInclude(n => n.Client)
                                                                             .Where(m => m.TcmClient_FK == id)
                                                                             .ToListAsync();
                if (listAppendixJ.Count == 0)
                {
                    return RedirectToAction("TCMIntakeSectionDashboard", new { id = id, section = 4, origin = origi });
                }

                ViewData["origi"] = origi;
                return View(listAppendixJ);
            }
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult SelectTypeConsentForRelease(int idTcmClient = 0, int origi = 0)
        {
            TCMIntakeConsentForReleaseTypeViewModel model = new TCMIntakeConsentForReleaseTypeViewModel()
            {
                IdTCMClient = idTcmClient,
                IdType = 0,
                origi = origi,
                Types = _combosHelper.GetComboConsentType()

            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> SelectTypeConsentForRelease(TCMIntakeConsentForReleaseTypeViewModel ConsentTypeViewModel)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            
            TCMClientEntity tcmclient = await _context.TCMClient
                                                      .Include(n => n.TCMIntakeForm)
                                                      .Include(n => n.Client)
                                                      .ThenInclude(n => n.EmergencyContact)
                                                      .Include(n => n.TCMAssessment)
                                                      .FirstOrDefaultAsync(n => n.Id == ConsentTypeViewModel.IdTCMClient);

            if (ConsentUtils.GetTypeByIndex(ConsentTypeViewModel.IdType) == ConsentType.PCP)
            {
                if (tcmclient.TCMIntakeForm != null)
                {
                    return RedirectToAction("CreateTCMConsentForRelease", "TCMIntakes", new { id = ConsentTypeViewModel.IdTCMClient, origi = 0, Name = tcmclient.TCMIntakeForm.PCP_Name, address = tcmclient.TCMIntakeForm.PCP_Address, city = tcmclient.TCMIntakeForm.PCP_CityStateZip, phone = tcmclient.TCMIntakeForm.PCP_Phone, fax = tcmclient.TCMIntakeForm.PCP_FaxNumber, idType = ConsentTypeViewModel.IdType });
                }
                else
                {
                    return RedirectToAction("CreateTCMConsentForRelease", "TCMIntakes", new { id = ConsentTypeViewModel.IdTCMClient, origi = ConsentTypeViewModel.origi, idType = ConsentTypeViewModel.IdType });
                }
            } 
            else
            {
                if (ConsentUtils.GetTypeByIndex(ConsentTypeViewModel.IdType) == ConsentType.PSYCHIATRIST)
                {
                    if (tcmclient.TCMIntakeForm != null)
                    {
                        return RedirectToAction("CreateTCMConsentForRelease", "TCMIntakes", new { id = ConsentTypeViewModel.IdTCMClient, origi = 0, Name = tcmclient.TCMIntakeForm.Psychiatrist_Name, address = tcmclient.TCMIntakeForm.Psychiatrist_Address, city = tcmclient.TCMIntakeForm.Psychiatrist_CityStateZip, phone = tcmclient.TCMIntakeForm.Psychiatrist_Phone, fax = " ", idType = ConsentTypeViewModel.IdType });
                    }
                    else
                    {
                        return RedirectToAction("CreateTCMConsentForRelease", "TCMIntakes", new { id = ConsentTypeViewModel.IdTCMClient, origi = ConsentTypeViewModel.origi, idType = ConsentTypeViewModel.IdType });
                    }
                }
                else
                {
                    if (ConsentUtils.GetTypeByIndex(ConsentTypeViewModel.IdType) == ConsentType.EMERGENCY_CONTACT)
                    {
                        if (tcmclient.Client.EmergencyContact != null)
                        {
                            return RedirectToAction("CreateTCMConsentForRelease", "TCMIntakes", new { id = ConsentTypeViewModel.IdTCMClient, origi = 0, Name = tcmclient.Client.EmergencyContact.Name, address = tcmclient.Client.EmergencyContact.Address, city = tcmclient.Client.EmergencyContact.City, phone = tcmclient.Client.EmergencyContact.Telephone, fax = "", idType = ConsentTypeViewModel.IdType });
                        }
                        else
                        {
                            return RedirectToAction("CreateTCMConsentForRelease", "TCMIntakes", new { id = ConsentTypeViewModel.IdTCMClient, origi = ConsentTypeViewModel.origi, idType = ConsentTypeViewModel.IdType });
                        }
                    }
                    else
                    {
                        if (ConsentUtils.GetTypeByIndex(ConsentTypeViewModel.IdType) == ConsentType.DCF)
                        {
                            return RedirectToAction("CreateTCMConsentForRelease", "TCMIntakes", new { id = ConsentTypeViewModel.IdTCMClient, origi = 0, Name = "Department of Children and Families", address = " ", city = "", phone = "", fax = "", idType = ConsentTypeViewModel.IdType });
                        }
                        else
                        {
                            if (ConsentUtils.GetTypeByIndex(ConsentTypeViewModel.IdType) == ConsentType.SSA)
                            {
                                return RedirectToAction("CreateTCMConsentForRelease", "TCMIntakes", new { id = ConsentTypeViewModel.IdTCMClient, origi = 0, Name = "Social Security Administration", address = " ", city = "", phone = "", fax = "", idType = ConsentTypeViewModel.IdType });
                            }
                            if (ConsentUtils.GetTypeByIndex(ConsentTypeViewModel.IdType) == ConsentType.POLICE_STATION)
                            {
                                return RedirectToAction("CreateTCMConsentForRelease", "TCMIntakes", new { id = ConsentTypeViewModel.IdTCMClient, origi = 0, Name = "Miami Dade Police Department", address = " ", city = "", phone = "", fax = "", idType = ConsentTypeViewModel.IdType });
                            }
                            else
                            {
                                if (ConsentUtils.GetTypeByIndex(ConsentTypeViewModel.IdType) == ConsentType.LIFELINESS_PROVIDERS)
                                {
                                    return RedirectToAction("CreateTCMConsentForRelease", "TCMIntakes", new { id = ConsentTypeViewModel.IdTCMClient, origi = 0, Name = "LifeLine Providers", address = " ", city = "", phone = "", fax = "", idType = ConsentTypeViewModel.IdType });
                                }
                                else
                                {
                                    if (ConsentUtils.GetTypeByIndex(ConsentTypeViewModel.IdType) == ConsentType.PHARMACY)
                                    {
                                        if (tcmclient.TCMIntakeForm != null)
                                        {
                                            return RedirectToAction("CreateTCMConsentForRelease", "TCMIntakes", new { id = ConsentTypeViewModel.IdTCMClient, origi = 0, Name = tcmclient.TCMAssessment.WhatPharmacy, address = " ", city = "", phone = tcmclient.TCMAssessment.PharmacyPhone, fax = "", idType = ConsentTypeViewModel.IdType });
                                        }
                                        else
                                        {
                                            return RedirectToAction("CreateTCMConsentForRelease", "TCMIntakes", new { id = ConsentTypeViewModel.IdTCMClient, origi = ConsentTypeViewModel.origi, idType = ConsentTypeViewModel.IdType });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return RedirectToAction("CreateTCMConsentForRelease", "TCMIntakes", new { id = ConsentTypeViewModel.IdTCMClient, origi = ConsentTypeViewModel.origi, idType = ConsentTypeViewModel.IdType });
        }
    }
}
