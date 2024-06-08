﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KyoS.Common.Helpers;
using AspNetCore.Reporting;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using DocumentFormat.OpenXml.Vml.Office;
using DocumentFormat.OpenXml.Office.CustomUI;

namespace KyoS.Web.Controllers
{
    public class ClientsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IMimeType _mimeType;
        private readonly IExportExcellHelper _exportExcelHelper;
        private readonly IFileHelper _fileHelper;
        private readonly IReportHelper _reportHelper;
        private readonly IWebHostEnvironment _webhostEnvironment;
        
        public IConfiguration Configuration { get; }

        public ClientsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper, IMimeType mimeType, IExportExcellHelper exportExcelHelper, IFileHelper fileHelper, IReportHelper reportHelper, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
            _imageHelper = imageHelper;
            _mimeType = mimeType;
            _exportExcelHelper = exportExcelHelper;
            _fileHelper = fileHelper;
            _reportHelper = reportHelper;
            _webhostEnvironment = webHostEnvironment;
            Configuration = configuration;
        }
        
        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, CaseManager, Frontdesk, Biller")]
        public async Task<IActionResult> Index(int idError = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }
            if (User.IsInRole("Manager") || User.IsInRole("Supervisor") || User.IsInRole("Frontdesk") || User.IsInRole("Biller"))
            {
                return View(await _context.Clients
                                          .Include(c => c.Clinic)
                                          .Include(c => c.IndividualTherapyFacilitator)
                                          .Include(c => c.Clients_HealthInsurances)
                                          .ThenInclude(c => c.HealthInsurance)
                                          .Include(c => c.ReferralForm)
                                          .AsSplitQuery()
                                          .Where(c => c.Clinic.Id == user_logged.Clinic.Id)
                                          .OrderBy(c => c.Name).ToListAsync());
            }
            if (User.IsInRole("Documents_Assistant"))
            {
                return View(await _context.Clients
                                          .Include(c => c.Clinic)
                                          .Include(c => c.IndividualTherapyFacilitator)
                                          .Include(c => c.Clients_HealthInsurances)
                                          .ThenInclude(c => c.HealthInsurance)
                                          .Include(c => c.ReferralForm)
                                          .AsSplitQuery()
                                          .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                && (c.Bio.CreatedBy == user_logged.UserName
                                                     || c.MTPs.Where(m => m.CreatedBy == user_logged.UserName).Count() > 0
                                                     || c.DocumentsAssistant.LinkedUser == user_logged.UserName)))
                                          .OrderBy(c => c.Name).ToListAsync());
            }
            if (User.IsInRole("Facilitator"))
            {
                FacilitatorEntity facilitator = await _context.Facilitators.FirstOrDefaultAsync(f => f.LinkedUser == user_logged.UserName);

                return View(await _context.Clients
                                          .Include(c => c.Clinic)
                                          .Include(c => c.IndividualTherapyFacilitator)
                                          .Include(c => c.Clients_HealthInsurances)
                                          .ThenInclude(c => c.HealthInsurance)
                                          .Include(c => c.ReferralForm)
                                          .AsSplitQuery()
                                          .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                && (c.Workdays_Clients.Where(m => m.Facilitator.Id == facilitator.Id).Count() > 0
                                                    || c.IndividualTherapyFacilitator.Id == facilitator.Id
                                                    || c.IdFacilitatorPSR == facilitator.Id
                                                    || c.IdFacilitatorGroup == facilitator.Id)))
                                          .OrderBy(c => c.Name).ToListAsync());
            }
            if (User.IsInRole("CaseManager"))
            {
                CaseMannagerEntity casemanager = await _context.CaseManagers.FirstOrDefaultAsync(f => f.LinkedUser == user_logged.UserName);
                
                TCMClientEntity tcmClient = await _context.TCMClient
                                                          
                                                          .Include(n => n.Client)
                                                          .ThenInclude(n => n.Clinic)
                                                          
                                                          .Include(n => n.Client)
                                                          .ThenInclude(c => c.IndividualTherapyFacilitator)

                                                          .Include(c => c.Client)
                                                          .ThenInclude(c => c.Clients_HealthInsurances)
                                                          .ThenInclude(c => c.HealthInsurance)

                                                          .Include(c => c.Client)
                                                          .ThenInclude(c => c.ReferralForm)

                                                          .AsSplitQuery()

                                                          .FirstOrDefaultAsync(f => f.Casemanager.Id == casemanager.Id);

                return View(tcmClient.Client);
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public IActionResult Create(int id = 0)
        {
            if (id == 1)
            {
                ViewBag.Creado = "Y";
            }
            else
            {
                if (id == 2)
                {
                    ViewBag.Creado = "E";
                }
                else
                {
                    if (id == 3)
                    {
                        ViewBag.Creado = "C";
                    }
                    else
                    {
                        ViewBag.Creado = "N";
                    }
                }
            }

            this.DeleteDiagnosticsTemp();
            this.DeleteDocumentsTemp();
            this.DeleteReferredsTemp();
            this.DeleteHealthInsuranceTemp();

            ClientViewModel model = new ClientViewModel();
            
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic != null)
            {
                ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
                List<SelectListItem> list = new List<SelectListItem>();

                List<ClientEntity> clients = _context.Clients.ToList();
                string maxNumberInc = string.Empty;

                if (clients.Count() > 0)
                {
                    string maxNumber = clients.MaxBy(n => n.Code).Code;
                    int temp = 0;

                    if (Int32.TryParse(maxNumber, out temp) == true)
                    {
                        maxNumberInc = (Convert.ToInt32(temp) + 1).ToString();
                    }
                }
                

                list.Insert(0, new SelectListItem
                {
                    Text = clinic.Name,
                    Value = $"{clinic.Id}"
                });
                model = new ClientViewModel
                {
                    DateOfBirth = DateTime.Today.AddYears(-60),
                    AdmisionDate = DateTime.Today,
                    Clinics = list,
                    IdClinic = clinic.Id,
                    IdGender = 1,
                    GenderList = _combosHelper.GetComboGender(),
                    IdStatus = 1,
                    StatusList = _combosHelper.GetComboClientStatus(),
                    Country = "United States",
                    State = "Florida",
                    City = user_logged.Clinic == null || user_logged.Clinic.City == null || user_logged.Clinic.City == string.Empty ? "Miami" : user_logged.Clinic.City,
                    IdRace = 0,
                    Races = _combosHelper.GetComboRaces(),
                    IdMaritalStatus = 0,
                    Maritals = _combosHelper.GetComboMaritals(),
                    IdEthnicity = 0,
                    Ethnicities = _combosHelper.GetComboEthnicities(),
                    IdPreferredLanguage = 1,
                    Languages = _combosHelper.GetComboLanguages(),
                    IdRelationship = 0,
                    Relationships = _combosHelper.GetComboRelationships(),
                    IdRelationshipEC = 0,
                    RelationshipsEC = _combosHelper.GetComboRelationships(),
                    IdEmergencyContact = 0,
                    EmergencyContacts = _combosHelper.GetComboEmergencyContactsByClinic(user_logged.Id),
                    IdDoctor = 0,
                    Doctors = _combosHelper.GetComboDoctorsByClinic(user_logged.Id),
                    IdPsychiatrist = 0,
                    Psychiatrists = _combosHelper.GetComboPsychiatristsByClinic(user_logged.Id),
                    IdLegalGuardian = 0,
                    LegalsGuardians = _combosHelper.GetComboLegalGuardiansByClinic(user_logged.Id),
                    DiagnosticTemp = _context.DiagnosticsTemp.Where(n => n.UserName == user_logged.UserName && n.IdClient == 0),
                    ReferredTemp = _context.ReferredsTemp.Where(n => n.CreatedBy == user_logged.UserName && n.IdClient == 0),
                    DocumentTemp = _context.DocumentsTemp.Where(n => n.UserName == user_logged.UserName && n.IdClient == 0),
                    OtherLanguage_Read = false,
                    OtherLanguage_Speak = false,
                    OtherLanguage_Understand = false,
                    MedicareId = "",
                    OnlyTCM = false,
                    HealthInsuranceTemp = _context.HealthInsuranceTemp.Where(n => n.UserName == user_logged.UserName && n.IdClient == 0),
                    Annotations = string.Empty,
                    IdService = 0,
                    Services = _combosHelper.GetComboServices(),
                    IdFacilitatorIT = 0,
                    ITFacilitators = _combosHelper.GetComboFacilitators(),
                    IdDocumentsAssistant = 0,
                    DocumentsAssistants = _combosHelper.GetComboDocumentsAssistantByClinic(user_logged.Clinic.Id, false, false),
                    DateOfClose = DateTime.Today.AddYears(1),
                    Code = maxNumberInc
                };
                return View(model);
            }
            else 
            {
                return RedirectToAction("NotAuthorized", "Account");
            }      
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<IActionResult> Create(ClientViewModel clientViewModel)
        {
            if (ModelState.IsValid)
            {
                if (_context.Clients.Where(n => n.Code == clientViewModel.Code).Count() > 0)
                {
                    return RedirectToAction("Create", new { id = 3 });
                }
                string photoPath = string.Empty;
                string signPath = string.Empty;

                if (clientViewModel.PhotoFile != null)
                {
                    photoPath = await _imageHelper.UploadImageAsync(clientViewModel.PhotoFile, "Clients");
                }
                if (clientViewModel.SignFile != null)
                {
                    signPath = await _imageHelper.UploadImageAsync(clientViewModel.SignFile, "Clients");
                }

                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (clientViewModel.FirstName != string.Empty)
                {
                    clientViewModel.Name = clientViewModel.FirstName;
                    if (clientViewModel.LastName != string.Empty)
                    {
                        clientViewModel.Name = clientViewModel.Name + ' ' + clientViewModel.LastName;

                    }
                }

                ClientEntity clientEntity = await _converterHelper.ToClientEntity(clientViewModel, true, photoPath, signPath, user_logged.Id);

                //-------Primary Doctor--------------------------//
                if (clientViewModel.IdDoctor == 0)
                {
                    if (clientViewModel.NamePrimaryDoctor != null && clientViewModel.NamePrimaryDoctor != string.Empty)
                    {
                        DoctorEntity PrimaryDoctor = new DoctorEntity
                        {
                            Name = clientViewModel.NamePrimaryDoctor,
                            Address = clientViewModel.AddressPrimaryDoctor,
                            City = clientViewModel.CityPrimaryDoctor,
                            Email = clientViewModel.EmailPrimaryDoctor,
                            State = clientViewModel.StatePrimaryDoctor,
                            Telephone = clientViewModel.PhonePrimaryDoctor,
                            ZipCode = clientViewModel.ZipCodePrimaryDoctor,
                            FaxNumber = clientViewModel.FaxNumberPsychiatrists,
                            CreatedBy = user_logged.Id,
                            CreatedOn = DateTime.Today,
                            LastModifiedBy = string.Empty,
                            LastModifiedOn = new DateTime(),

                        };
                        clientEntity.Doctor = PrimaryDoctor;

                    }
                    else
                    {
                        clientEntity.Doctor = null;
                    }
                }

                // _context.Add(clientEntity);

                //-------Psychiatrists--------------------------//
                if (clientViewModel.IdPsychiatrist == 0)
                {
                    if (clientViewModel.NamePsychiatrists != null && clientViewModel.NamePsychiatrists != string.Empty)
                    {
                        PsychiatristEntity Psychistrists = new PsychiatristEntity
                        {
                            Name = clientViewModel.NamePsychiatrists,
                            Address = clientViewModel.AddressPsychiatrists,
                            City = clientViewModel.CityPsychiatrists,
                            Email = clientViewModel.EmailPsychiatrists,
                            State = clientViewModel.StatePsychiatrists,
                            Telephone = clientViewModel.PhonePsychiatrists,
                            ZipCode = clientViewModel.ZipCodePsychiatrists,
                            FaxNumber = clientViewModel.FaxNumberPsychiatrists,
                            CreatedBy = user_logged.Id,
                            CreatedOn = DateTime.Today,
                            LastModifiedBy = string.Empty,
                            LastModifiedOn = new DateTime(),

                        };
                        clientEntity.Psychiatrist = Psychistrists;

                    }
                    else
                    {
                        clientEntity.Psychiatrist = null;
                    }
                }

                // _context.Add(clientEntity);


                //-------Legal Guardian Contact--------------------------//
                if (clientViewModel.IdLegalGuardian == 0)
                {
                    if (clientViewModel.NameLegalGuardian != null && clientViewModel.NameLegalGuardian != string.Empty)
                    {
                        LegalGuardianEntity legalGuardian = new LegalGuardianEntity
                        {
                            Name = clientViewModel.NameLegalGuardian,
                            Address = clientViewModel.AddressLegalGuardian,
                            AdressLine2 = clientViewModel.AddressLine2LegalGuardian,
                            City = clientViewModel.CityLegalGuardian,
                            Country = clientViewModel.CountryLegalGuardian,
                            Email = clientViewModel.EmailLegalGuardian,
                            State = clientViewModel.StateLegalGuardian,
                            Telephone = clientViewModel.PhoneLegalGuardian,
                            TelephoneSecondary = clientViewModel.PhoneSecundaryLegalGuardian,
                            ZipCode = clientViewModel.ZipCodeLegalGuardian,
                            CreatedBy = user_logged.Id,
                            CreatedOn = DateTime.Today,
                            LastModifiedBy = string.Empty,
                            LastModifiedOn = new DateTime(),

                        };
                        clientEntity.LegalGuardian = legalGuardian;

                    }
                    else
                    {
                        clientEntity.LegalGuardian = null;
                    }
                }

               // _context.Add(clientEntity);


                //-------Emergency Contact--------------------------//
                if (clientViewModel.IdEmergencyContact == 0)
                {
                    if (clientViewModel.NameEmergencyContact != null && clientViewModel.NameEmergencyContact != string.Empty)
                    {
                        EmergencyContactEntity emergencyContact = new EmergencyContactEntity
                        {
                            Name = clientViewModel.NameEmergencyContact,
                            Address = clientViewModel.AddressEmergencyContact,
                            AdressLine2 = clientViewModel.AddressLine2EmergencyContact,
                            City = clientViewModel.CityEmergencyContact,
                            Country = clientViewModel.CountryEmergencyContact,
                            Email = clientViewModel.EmailEmergencyContact,
                            State = clientViewModel.StateEmergencyContact,
                            Telephone = clientViewModel.PhoneEmergencyContact,
                            TelephoneSecondary = clientViewModel.PhoneSecundaryEmergencyContact,
                            ZipCode = clientViewModel.ZipCodeEmergencyContact,
                            CreatedBy = user_logged.Id,
                            CreatedOn = DateTime.Today,
                            LastModifiedBy = string.Empty,
                            LastModifiedOn = new DateTime(),

                        };
                        clientEntity.EmergencyContact = emergencyContact;

                    }
                    else
                    {
                        clientEntity.EmergencyContact = null;
                    }
                }

                _context.Add(clientEntity);

                //----------update Client_Diagnostic table-------------//
                IQueryable<DiagnosticTempEntity> list_to_delete = _context.DiagnosticsTemp
                                                                          .Where(d => d.UserName == user_logged.UserName
                                                                             && d.IdClient == 0);
                Client_Diagnostic clientDiagnostic;
                foreach (DiagnosticTempEntity item in list_to_delete)
                {
                    clientDiagnostic = new Client_Diagnostic
                    {
                        Client = clientEntity,
                        Diagnostic = await _context.Diagnostics.FirstOrDefaultAsync(d => d.Code == item.Code),
                        Principal = item.Principal,
                        Active = item.Active,
                        DateIdentify = item.DateIdentify,
                        Prescriber = item.Prescriber
                    };
                    _context.Add(clientDiagnostic);
                    _context.DiagnosticsTemp.Remove(item);
                }

                //----------update Documents table---------------------//
                IQueryable<DocumentTempEntity> list_to_delete_doc = _context.DocumentsTemp
                                                                            .Where(d => d.UserName == user_logged.UserName
                                                                               && d.IdClient == 0); 
                DocumentEntity document;
                foreach (DocumentTempEntity item in list_to_delete_doc)
                {
                    document = new DocumentEntity
                    {
                        Client = clientEntity,
                        FileName = item.DocumentName,
                        Description = item.Description,
                        FileUrl = item.DocumentPath,
                        CreatedBy = user_logged.Id,
                        CreatedOn = DateTime.Now
                    };
                    _context.Add(document);
                    _context.DocumentsTemp.Remove(item);
                }

                //update Client_Referred table with the news ReferredTemp
                IQueryable<ReferredTempEntity> list_to_delete_referred = _context.ReferredsTemp
                                                                                 .Where(n => n.CreatedBy == user_logged.UserName
                                                                                    && n.IdClient == 0);
                Client_Referred clientReferred;
                foreach (ReferredTempEntity item in list_to_delete_referred)
                {
                    clientReferred = new Client_Referred
                    {
                        Client = clientEntity,
                        Referred = await _context.Referreds.FirstOrDefaultAsync(d => d.Id == item.IdReferred),
                        Service = item.Service,
                        ReferredNote = item.ReferredNote,
                        type = item.type
                    };
                    _context.Add(clientReferred);
                    _context.ReferredsTemp.Remove(item);
                }

                //----------update Client_HealthInsurance table-------------//
                IQueryable<HealthInsuranceTempEntity> listHealthInsurance_to_delete = _context.HealthInsuranceTemp
                                                                                              .Where(d => d.UserName == user_logged.UserName
                                                                                                && d.IdClient == 0);
                Client_HealthInsurance clientHealthInsurance;
                foreach (HealthInsuranceTempEntity item in listHealthInsurance_to_delete)
                {
                    clientHealthInsurance = new Client_HealthInsurance
                    {
                        Client = clientEntity,
                        HealthInsurance = await _context.HealthInsurances.FirstOrDefaultAsync(d => d.Name == item.Name),
                        Active = item.Active,
                        ApprovedDate = item.ApprovedDate,
                        DurationTime = item.DurationTime,
                        MemberId = item.MemberId,
                        Units = item.Units,
                        AuthorizationNumber = item.AuthorizationNumber,
                        Agency = item.Agency,
                        ExpiredDate = item.ExpiredDate,
                        EffectiveDate = item.EffectiveDate,
                        EndCoverageDate = item.EndCoverageDate,
                        InsuranceType = item.InsuranceType,
                        InsurancePlan = item.InsurancePlan,
                        InsuranceCoverage = item.InsuranceCoverage

                    };
                    _context.Add(clientHealthInsurance);
                    _context.HealthInsuranceTemp.Remove(item);
                }

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Create", new { id = 1 });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the client: {clientEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }                
            }
            return View(clientViewModel);
        }

        [Authorize(Roles = "Manager, Supervisor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ClientEntity clientEntity = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (clientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Clients.Remove(clientEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }
           
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Manager, CaseManager, Frontdesk")]
        public async Task<IActionResult> Edit(int? id, int origin = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ClientEntity clientEntity = await _context.Clients

                                                      .Include(c => c.Clinic)

                                                      .Include(c => c.Doctor)

                                                      .Include(c => c.Psychiatrist)                                                     

                                                      .Include(c => c.LegalGuardian)

                                                      .Include(c => c.EmergencyContact)

                                                      .Include(c => c.IndividualTherapyFacilitator)

                                                      .Include(c => c.Clients_HealthInsurances)

                                                      .Include(c => c.DocumentsAssistant)

                                                      .AsSplitQuery()

                                                      .FirstOrDefaultAsync(c => c.Id == id);
            if (clientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            this.DeleteDiagnosticsTemp(clientEntity);
            this.DeleteDocumentsTemp(clientEntity);
            this.DeleteReferredsTemp(clientEntity);
            this.DeleteHealthInsuranceTemp(clientEntity);

            this.SetDiagnosticsTemp(clientEntity);
            this.SetDocumentsTemp(clientEntity);
            this.SetReferredsTemp(clientEntity);
            this.SetHealthInsuranceTemp(clientEntity);
            
            ClientViewModel clientViewModel = await _converterHelper.ToClientViewModel(clientEntity, user_logged.Id);            

            clientViewModel.Origin = origin;
            List<Workday_Client> worday_clients = _context.Workdays_Clients
                                                          .Include(n => n.Facilitator)
                                                          .Include(n => n.Workday)
                                                          .Where(m => m.Client.Id == clientEntity.Id )
                                                          .ToList();
            if (clientViewModel.IdFacilitatorPSR == 0)
            {
                if (worday_clients.Where(n => n.Workday.Service == ServiceType.PSR).Count() > 0)
                {
                    FacilitatorEntity FacilitatorPSR = worday_clients.Where(n => n.Workday.Service == ServiceType.PSR).OrderByDescending(m => m.Workday.Date).FirstOrDefault().Facilitator;
                    if (FacilitatorPSR != null)
                    {
                        clientViewModel.FacilitatorPSR = FacilitatorPSR.Name;
                        clientViewModel.IdFacilitatorPSR = FacilitatorPSR.Id;
                    }
                }
            }
            else
            {
                clientViewModel.FacilitatorPSR = _context.Facilitators.FirstOrDefault(n => n.Id == clientViewModel.IdFacilitatorPSR).Name;
            }

            if (clientViewModel.IdFacilitatorGroup == 0)
            {
                if (worday_clients.Where(n => n.Workday.Service == ServiceType.Group).Count() > 0)
                {

                    FacilitatorEntity FacilitatorGroup = worday_clients.Where(n => n.Workday.Service == ServiceType.Group).OrderByDescending(m => m.Workday.Date).FirstOrDefault().Facilitator;
                    if (FacilitatorGroup != null)
                    {
                        clientViewModel.FacilitatorGroup = FacilitatorGroup.Name;
                        clientViewModel.IdFacilitatorGroup = FacilitatorGroup.Id;
                    }
                }
            }
            else
            {
                clientViewModel.FacilitatorGroup = _context.Facilitators.FirstOrDefault(n => n.Id == clientViewModel.IdFacilitatorGroup).Name;
            }

            if (clientViewModel.IdTCMClient != 0)
            {
                clientViewModel.AdmisionDateTCM = _context.TCMClient.FirstOrDefault(n => n.Id == clientViewModel.IdTCMClient).DataOpen;
                clientViewModel.TCMName = _context.TCMClient.Include(n => n.Casemanager).FirstOrDefault(n => n.Id == clientViewModel.IdTCMClient).Casemanager.Name;
            }
           
            return View(clientViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, CaseManager, Frontdesk")]
        public async Task<IActionResult> Edit(int id, ClientViewModel clientViewModel)
        {
            if (id != clientViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                string photoPath = clientViewModel.PhotoPath;
                string signPath = clientViewModel.SignPath;

                if (clientViewModel.PhotoFile != null)
                {
                    photoPath = await _imageHelper.UploadImageAsync(clientViewModel.PhotoFile, "Clients");
                }
                if (clientViewModel.SignFile != null)
                {
                    signPath = await _imageHelper.UploadImageAsync(clientViewModel.SignFile, "Clients");
                }

                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);

               
                if (User.IsInRole("CaseManager"))
                {
                    ClientEntity clientEntity = await _context.Clients.FirstOrDefaultAsync(n => n.Id == clientViewModel.Id);

                    clientEntity.FullAddress = clientViewModel.FullAddress;
                    clientEntity.AlternativeAddress = clientViewModel.AlternativeAddress;
                    clientEntity.TelephoneSecondary = clientViewModel.TelephoneSecondary;
                    clientEntity.Email = clientViewModel.Email;
                    clientEntity.Country = clientViewModel.Country;
                    clientEntity.City = clientViewModel.City;
                    clientEntity.State = clientViewModel.State;
                    clientEntity.ZipCode = clientViewModel.ZipCode;
                    clientEntity.Race = RaceUtils.GetRaceByIndex(clientViewModel.IdRace);
                    clientEntity.MaritalStatus = MaritalUtils.GetMaritalByIndex(clientViewModel.IdMaritalStatus);
                    clientEntity.Ethnicity = EthnicityUtils.GetEthnicityByIndex(clientViewModel.IdEthnicity);
                    clientEntity.PreferredLanguage = PreferredLanguageUtils.GetPreferredLanguageByIndex(clientViewModel.IdPreferredLanguage);
                    clientEntity.OtherLanguage = clientViewModel.OtherLanguage;
                    clientEntity.PhotoPath = photoPath;

                    clientEntity.Doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Id == clientViewModel.IdDoctor);
                    clientEntity.Psychiatrist = await _context.Psychiatrists.FirstOrDefaultAsync(p => p.Id == clientViewModel.IdPsychiatrist);
                    //Referred = await _context.Referreds.FirstOrDefaultAsync(r => r.Id == model.IdReferred),
                    clientEntity.LegalGuardian = await _context.LegalGuardians.FirstOrDefaultAsync(lg => lg.Id == clientViewModel.IdLegalGuardian);
                    clientEntity.EmergencyContact = await _context.EmergencyContacts.FirstOrDefaultAsync(ec => ec.Id == clientViewModel.IdEmergencyContact);
                    clientEntity.RelationShipOfEmergencyContact = RelationshipUtils.GetRelationshipByIndex(clientViewModel.IdRelationshipEC);
                    clientEntity.RelationShipOfLegalGuardian = RelationshipUtils.GetRelationshipByIndex(clientViewModel.IdRelationship);

                    clientEntity.LastModifiedBy = user_logged.Id;
                    clientEntity.LastModifiedOn = DateTime.Now;
                    clientEntity.OtherLanguage_Read = clientViewModel.OtherLanguage_Read;
                    clientEntity.OtherLanguage_Speak = clientViewModel.OtherLanguage_Speak;
                    clientEntity.OtherLanguage_Understand = clientViewModel.OtherLanguage_Understand;

                    if (clientEntity.FirstName != string.Empty)
                    {
                        clientEntity.Name = clientEntity.FirstName;
                        if (clientEntity.FirstName != string.Empty)
                        {
                            clientEntity.Name = clientEntity.Name + ' ' + clientEntity.LastName;
                        }
                    }

                    //-------Primary Doctor--------------------------//
                    if (clientViewModel.IdDoctor == 0)
                    {
                        if (clientViewModel.NamePrimaryDoctor != null && clientViewModel.NamePrimaryDoctor != string.Empty)
                        {
                            DoctorEntity PrimaryDoctor = new DoctorEntity
                            {
                                Name = clientViewModel.NamePrimaryDoctor,
                                Address = clientViewModel.AddressPrimaryDoctor,
                                City = clientViewModel.CityPrimaryDoctor,
                                Email = clientViewModel.EmailPrimaryDoctor,
                                State = clientViewModel.StatePrimaryDoctor,
                                Telephone = clientViewModel.PhonePrimaryDoctor,
                                ZipCode = clientViewModel.ZipCodePrimaryDoctor,
                                CreatedBy = user_logged.Id,
                                CreatedOn = DateTime.Today,
                                LastModifiedBy = string.Empty,
                                LastModifiedOn = new DateTime(),

                            };
                            _context.Add(PrimaryDoctor);
                            clientEntity.Doctor = PrimaryDoctor;

                        }
                        else
                        {
                            clientEntity.Doctor = null;
                        }

                    }
                    else
                    {
                        clientEntity.Doctor = _context.Doctors.FirstOrDefault(n => n.Id == clientViewModel.IdDoctor);
                    }

                    //-------Psychiatrists--------------------------//
                    if (clientViewModel.IdPsychiatrist == 0)
                    {
                        if (clientViewModel.NamePsychiatrists != null && clientViewModel.NamePsychiatrists != string.Empty)
                        {
                            PsychiatristEntity Psychiatrists = new PsychiatristEntity
                            {
                                Name = clientViewModel.NamePsychiatrists,
                                Address = clientViewModel.AddressPsychiatrists,
                                City = clientViewModel.CityPsychiatrists,
                                Email = clientViewModel.EmailPsychiatrists,
                                State = clientViewModel.StatePsychiatrists,
                                Telephone = clientViewModel.PhonePsychiatrists,
                                ZipCode = clientViewModel.ZipCodePsychiatrists,
                                FaxNumber = clientViewModel.FaxNumberPsychiatrists,
                                CreatedBy = user_logged.Id,
                                CreatedOn = DateTime.Today,
                                LastModifiedBy = string.Empty,
                                LastModifiedOn = new DateTime(),

                            };
                            _context.Add(Psychiatrists);
                            clientEntity.Psychiatrist = Psychiatrists;

                        }
                        else
                        {
                            clientEntity.Psychiatrist = null;
                        }

                    }
                    else
                    {
                        clientEntity.Psychiatrist = _context.Psychiatrists.FirstOrDefault(n => n.Id == clientViewModel.IdPsychiatrist);
                    }

                    //-------Legal Guardian--------------------------//
                    if (clientViewModel.IdLegalGuardian == 0)
                    {
                        if (clientViewModel.NameLegalGuardian != null && clientViewModel.NameLegalGuardian != string.Empty)
                        {
                            LegalGuardianEntity legalGuardianContact = new LegalGuardianEntity
                            {
                                Name = clientViewModel.NameLegalGuardian,
                                Address = clientViewModel.AddressLegalGuardian,
                                AdressLine2 = clientViewModel.AddressLine2LegalGuardian,
                                City = clientViewModel.CityLegalGuardian,
                                Country = clientViewModel.CountryLegalGuardian,
                                Email = clientViewModel.EmailLegalGuardian,
                                State = clientViewModel.StateLegalGuardian,
                                Telephone = clientViewModel.PhoneLegalGuardian,
                                TelephoneSecondary = clientViewModel.PhoneLegalGuardian,
                                ZipCode = clientViewModel.ZipCodeLegalGuardian,
                                CreatedBy = user_logged.Id,
                                CreatedOn = DateTime.Today,
                                LastModifiedBy = string.Empty,
                                LastModifiedOn = new DateTime(),

                            };
                            _context.Add(legalGuardianContact);
                            clientEntity.LegalGuardian = legalGuardianContact;

                        }
                        else
                        {
                            clientEntity.LegalGuardian = null;
                        }

                    }
                    else
                    {
                        clientEntity.LegalGuardian = _context.LegalGuardians.FirstOrDefault(n => n.Id == clientViewModel.IdLegalGuardian);
                    }

                    //-------Emergency Contact--------------------------//
                    if (clientViewModel.IdEmergencyContact == 0)
                    {
                        if (clientViewModel.NameEmergencyContact != null && clientViewModel.NameEmergencyContact != string.Empty)
                        {
                            EmergencyContactEntity emergencyContact = new EmergencyContactEntity
                            {
                                Name = clientViewModel.NameEmergencyContact,
                                Address = clientViewModel.AddressEmergencyContact,
                                AdressLine2 = clientViewModel.AddressLine2EmergencyContact,
                                City = clientViewModel.CityEmergencyContact,
                                Country = clientViewModel.CountryEmergencyContact,
                                Email = clientViewModel.EmailEmergencyContact,
                                State = clientViewModel.StateEmergencyContact,
                                Telephone = clientViewModel.PhoneEmergencyContact,
                                TelephoneSecondary = clientViewModel.PhoneSecundaryEmergencyContact,
                                ZipCode = clientViewModel.ZipCodeEmergencyContact,
                                CreatedBy = user_logged.Id,
                                CreatedOn = DateTime.Today,
                                LastModifiedBy = string.Empty,
                                LastModifiedOn = new DateTime(),

                            };
                            _context.Add(emergencyContact);
                            clientEntity.EmergencyContact = emergencyContact;

                        }
                        else
                        {
                            clientEntity.EmergencyContact = null;
                        }

                    }
                    else
                    {
                        clientEntity.EmergencyContact = _context.EmergencyContacts.FirstOrDefault(n => n.Id == clientViewModel.IdEmergencyContact);
                    }

                    _context.Update(clientEntity);
                }
                else
                {
                    ClientEntity clientEntity = await _converterHelper.ToClientEntity(clientViewModel, false, photoPath, signPath, user_logged.Id);

                    if (clientViewModel.IdStatus == 2) //the client was closed
                    {
                        _context.Entry(clientEntity).Reference("Group").CurrentValue = null;
                        _context.Entry(clientEntity).Reference("Group").IsModified = true;
                    }

                    //-------Primary Doctor--------------------------//
                    if (clientViewModel.IdDoctor == 0)
                    {
                        if (clientViewModel.NamePrimaryDoctor != null && clientViewModel.NamePrimaryDoctor != string.Empty)
                        {
                            DoctorEntity PrimaryDoctor = new DoctorEntity
                            {
                                Name = clientViewModel.NamePrimaryDoctor,
                                Address = clientViewModel.AddressPrimaryDoctor,
                                City = clientViewModel.CityPrimaryDoctor,
                                Email = clientViewModel.EmailPrimaryDoctor,
                                State = clientViewModel.StatePrimaryDoctor,
                                Telephone = clientViewModel.PhonePrimaryDoctor,
                                ZipCode = clientViewModel.ZipCodePrimaryDoctor,
                                CreatedBy = user_logged.Id,
                                CreatedOn = DateTime.Today,
                                LastModifiedBy = string.Empty,
                                LastModifiedOn = new DateTime(),

                            };
                            _context.Add(PrimaryDoctor);
                            clientEntity.Doctor = PrimaryDoctor;

                        }
                        else
                        {
                            clientEntity.Doctor = null;
                        }

                    }
                    else
                    {
                        clientEntity.Doctor = _context.Doctors.FirstOrDefault(n => n.Id == clientViewModel.IdDoctor);
                    }

                    //-------Psychiatrists--------------------------//
                    if (clientViewModel.IdPsychiatrist == 0)
                    {
                        if (clientViewModel.NamePsychiatrists != null && clientViewModel.NamePsychiatrists != string.Empty)
                        {
                            PsychiatristEntity Psychiatrists = new PsychiatristEntity
                            {
                                Name = clientViewModel.NamePsychiatrists,
                                Address = clientViewModel.AddressPsychiatrists,
                                City = clientViewModel.CityPsychiatrists,
                                Email = clientViewModel.EmailPsychiatrists,
                                State = clientViewModel.StatePsychiatrists,
                                Telephone = clientViewModel.PhonePsychiatrists,
                                ZipCode = clientViewModel.ZipCodePsychiatrists,
                                FaxNumber = clientViewModel.FaxNumberPsychiatrists,
                                CreatedBy = user_logged.Id,
                                CreatedOn = DateTime.Today,
                                LastModifiedBy = string.Empty,
                                LastModifiedOn = new DateTime(),

                            };
                            _context.Add(Psychiatrists);
                            clientEntity.Psychiatrist = Psychiatrists;

                        }
                        else
                        {
                            clientEntity.Psychiatrist = null;
                        }

                    }
                    else
                    {
                        clientEntity.Psychiatrist = _context.Psychiatrists.FirstOrDefault(n => n.Id == clientViewModel.IdPsychiatrist);
                    }

                    //-------Legal Guardian--------------------------//
                    if (clientViewModel.IdLegalGuardian == 0)
                    {
                        if (clientViewModel.NameLegalGuardian != null && clientViewModel.NameLegalGuardian != string.Empty)
                        {
                            LegalGuardianEntity legalGuardianContact = new LegalGuardianEntity
                            {
                                Name = clientViewModel.NameLegalGuardian,
                                Address = clientViewModel.AddressLegalGuardian,
                                AdressLine2 = clientViewModel.AddressLine2LegalGuardian,
                                City = clientViewModel.CityLegalGuardian,
                                Country = clientViewModel.CountryLegalGuardian,
                                Email = clientViewModel.EmailLegalGuardian,
                                State = clientViewModel.StateLegalGuardian,
                                Telephone = clientViewModel.PhoneLegalGuardian,
                                TelephoneSecondary = clientViewModel.PhoneLegalGuardian,
                                ZipCode = clientViewModel.ZipCodeLegalGuardian,
                                CreatedBy = user_logged.Id,
                                CreatedOn = DateTime.Today,
                                LastModifiedBy = string.Empty,
                                LastModifiedOn = new DateTime(),

                            };
                            _context.Add(legalGuardianContact);
                            clientEntity.LegalGuardian = legalGuardianContact;

                        }
                        else
                        {
                            clientEntity.LegalGuardian = null;
                        }

                    }
                    else
                    {
                        clientEntity.LegalGuardian = _context.LegalGuardians.FirstOrDefault(n => n.Id == clientViewModel.IdLegalGuardian);
                    }

                    //-------Emergency Contact--------------------------//
                    if (clientViewModel.IdEmergencyContact == 0)
                    {
                        if (clientViewModel.NameEmergencyContact != null && clientViewModel.NameEmergencyContact != string.Empty)
                        {
                            EmergencyContactEntity emergencyContact = new EmergencyContactEntity
                            {
                                Name = clientViewModel.NameEmergencyContact,
                                Address = clientViewModel.AddressEmergencyContact,
                                AdressLine2 = clientViewModel.AddressLine2EmergencyContact,
                                City = clientViewModel.CityEmergencyContact,
                                Country = clientViewModel.CountryEmergencyContact,
                                Email = clientViewModel.EmailEmergencyContact,
                                State = clientViewModel.StateEmergencyContact,
                                Telephone = clientViewModel.PhoneEmergencyContact,
                                TelephoneSecondary = clientViewModel.PhoneSecundaryEmergencyContact,
                                ZipCode = clientViewModel.ZipCodeEmergencyContact,
                                CreatedBy = user_logged.Id,
                                CreatedOn = DateTime.Today,
                                LastModifiedBy = string.Empty,
                                LastModifiedOn = new DateTime(),

                            };
                            _context.Add(emergencyContact);
                            clientEntity.EmergencyContact = emergencyContact;

                        }
                        else
                        {
                            clientEntity.EmergencyContact = null;
                        }

                    }
                    else
                    {
                        clientEntity.EmergencyContact = _context.EmergencyContacts.FirstOrDefault(n => n.Id == clientViewModel.IdEmergencyContact);
                    }

                    _context.Update(clientEntity);

                    //delete all client diagnostic of this client
                    IEnumerable<Client_Diagnostic> list_to_delete = await _context.Clients_Diagnostics
                                                                                  .Where(cd => cd.Client.Id == clientViewModel.Id)
                                                                                  .ToListAsync();
                    _context.Clients_Diagnostics.RemoveRange(list_to_delete);

                    //update Client_Diagnostic table with the news DiagnosticsTemp
                    IQueryable<DiagnosticTempEntity> listDiagnosticTemp = _context.DiagnosticsTemp
                                                                                  .Where(d => d.UserName == user_logged.UserName
                                                                                    && d.IdClient == clientEntity.Id);
                    Client_Diagnostic clientDiagnostic;
                    foreach (DiagnosticTempEntity item in listDiagnosticTemp)
                    {
                        clientDiagnostic = new Client_Diagnostic
                        {
                            Client = clientEntity,
                            Diagnostic = await _context.Diagnostics.FirstOrDefaultAsync(d => d.Code == item.Code),
                            Principal = item.Principal,
                            Active = item.Active,
                            DateIdentify = item.DateIdentify,
                            Prescriber = item.Prescriber
                        };
                        _context.Add(clientDiagnostic);
                        //_context.DiagnosticsTemp.Remove(item);
                    }

                    //delete all client referred of this client
                    IEnumerable<Client_Referred> listReferred_to_delete = await _context.Clients_Referreds
                                                                                  .Where(cd => cd.Client.Id == clientViewModel.Id)
                                                                                  .ToListAsync();
                    _context.Clients_Referreds.RemoveRange(listReferred_to_delete);

                    //update Client_Referred table with the news ReferredTemp
                    IQueryable<ReferredTempEntity> listReferredTemp = _context.ReferredsTemp
                                                                              .Where(d => d.CreatedBy == user_logged.UserName
                                                                                && d.IdClient == clientEntity.Id);
                    Client_Referred clientReferred;
                    foreach (ReferredTempEntity item1 in listReferredTemp)
                    {
                        clientReferred = new Client_Referred
                        {
                            Client = clientEntity,
                            Referred = await _context.Referreds.FirstOrDefaultAsync(d => d.Id == item1.IdReferred),
                            Service = item1.Service,
                            ReferredNote = item1.ReferredNote,
                            type = item1.type
                        };
                        _context.Add(clientReferred);
                        //_context.ReferredsTemp.Remove(item1);
                    }

                    //delete all client Documents of this client
                    IEnumerable<DocumentEntity> documents_to_delete = await _context.Documents
                                                                                    .Where(cd => cd.Client.Id == clientViewModel.Id)
                                                                                    .ToListAsync();

                    _context.Documents.RemoveRange(documents_to_delete);

                    //update Documents table with the news DocumentsTemp
                    IQueryable<DocumentTempEntity> listDocumentTemp = _context.DocumentsTemp
                                                                              .Where(d => d.UserName == user_logged.UserName
                                                                                && d.IdClient == clientViewModel.Id);
                    DocumentEntity document;
                    foreach (DocumentTempEntity item in listDocumentTemp)
                    {
                        //document = await _context.Documents.FirstOrDefaultAsync(d => d.FileUrl == item.DocumentPath);
                        //if (document == null)
                        // {
                        document = new DocumentEntity
                        {
                            Client = clientEntity,
                            FileName = item.DocumentName,
                            Description = item.Description,
                            FileUrl = item.DocumentPath,
                            CreatedBy = user_logged.Id,
                            CreatedOn = DateTime.Now
                        };
                        _context.Add(document);
                        // }                    
                        //_context.DocumentsTemp.Remove(item);
                    }

                    //delete all client Health Insurance of this client
                    IEnumerable<Client_HealthInsurance> listHealthInsurance_to_delete = await _context.Clients_HealthInsurances
                                                                                                      .Where(cd => cd.Client.Id == clientViewModel.Id)
                                                                                                      .ToListAsync();
                    _context.Clients_HealthInsurances.RemoveRange(listHealthInsurance_to_delete);

                    //update Client_HealthInsurance table with the news HealthInsuranceTemp
                    IQueryable<HealthInsuranceTempEntity> listHealthInsuranceTemp = _context.HealthInsuranceTemp
                                                                                             .Where(d => d.UserName == user_logged.UserName
                                                                                                && d.IdClient == clientEntity.Id);
                    Client_HealthInsurance clientHealthInsurance;
                    foreach (HealthInsuranceTempEntity item in listHealthInsuranceTemp)
                    {
                        clientHealthInsurance = new Client_HealthInsurance
                        {
                            Client = clientEntity,
                            HealthInsurance = await _context.HealthInsurances.FirstOrDefaultAsync(d => d.Name == item.Name),
                            Active = item.Active,
                            ApprovedDate = item.ApprovedDate,
                            DurationTime = item.DurationTime,
                            MemberId = item.MemberId,
                            Units = item.Units,
                            AuthorizationNumber = item.AuthorizationNumber,
                            Agency = item.Agency,
                            ExpiredDate = item.ExpiredDate,
                            EffectiveDate = item.EffectiveDate,
                            EndCoverageDate = item.EndCoverageDate,
                            InsuranceType = item.InsuranceType,
                            InsurancePlan = item.InsurancePlan,
                            InsuranceCoverage = item.InsuranceCoverage

                        };
                        _context.Add(clientHealthInsurance);
                        //_context.HealthInsuranceTemp.Remove(item);
                    }
                }
               
                try
                {
                    await _context.SaveChangesAsync();
                    if (clientViewModel.Origin == 0)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    if (clientViewModel.Origin == 1)
                    {
                        return RedirectToAction(nameof(ClientsWithoutDOC));
                    }
                    if (clientViewModel.Origin == 2)
                    {
                        return RedirectToAction("Clients","TCMClients");
                    }
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the client: {clientViewModel.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(clientViewModel);
        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant, CaseManager, TCMSupervisor, Biller")]
        public async Task<IActionResult> Details(int? id, int origin = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ClientEntity clientEntity = await _context.Clients
                                                      .Include(c => c.Clinic)
                                                      .Include(c => c.Doctor)
                                                      .Include(c => c.Psychiatrist)
                                                      .Include(c => c.LegalGuardian)
                                                      .Include(c => c.EmergencyContact)
                                                      .Include(c => c.IndividualTherapyFacilitator)
                                                      .Include(c => c.Clients_HealthInsurances)
                                                      .FirstOrDefaultAsync(c => c.Id == id);
            if (clientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            this.DeleteDiagnosticsTemp(clientEntity);
            this.DeleteDocumentsTemp(clientEntity);
            this.DeleteReferredsTemp(clientEntity);
            this.DeleteHealthInsuranceTemp(clientEntity);

            this.SetDiagnosticsTemp(clientEntity);
            this.SetDocumentsTemp(clientEntity);
            this.SetReferredsTemp(clientEntity);
            this.SetHealthInsuranceTemp(clientEntity);

            ClientViewModel clientViewModel = await _converterHelper.ToClientViewModel(clientEntity, user_logged.Id);
                        
            clientViewModel.Origin = origin;

            List<Workday_Client> worday_clients = _context.Workdays_Clients
                                                          .Include(n => n.Facilitator)
                                                          .Include(n => n.Workday)
                                                          .Where(m => m.Client.Id == clientEntity.Id)
                                                          .ToList();
            if (clientViewModel.IdFacilitatorPSR == 0)
            {
                if (worday_clients.Where(n => n.Workday.Service == ServiceType.PSR).Count() > 0)
                {
                    clientViewModel.FacilitatorPSR = worday_clients.Where(n => n.Workday.Service == ServiceType.PSR).OrderByDescending(m => m.Workday.Date).FirstOrDefault().Facilitator.Name;
                }
            }
            else
            {
                clientViewModel.FacilitatorPSR = _context.Facilitators.FirstOrDefault(n => n.Id == clientViewModel.IdFacilitatorPSR).Name;
            }

            if (clientViewModel.IdFacilitatorGroup == 0)
            {
                if (worday_clients.Where(n => n.Workday.Service == ServiceType.Group).Count() > 0)
                {
                    clientViewModel.FacilitatorGroup = worday_clients.Where(n => n.Workday.Service == ServiceType.Group).OrderByDescending(m => m.Workday.Date).FirstOrDefault().Facilitator.Name;
                }
            }
            else
            {
                clientViewModel.FacilitatorGroup = _context.Facilitators.FirstOrDefault(n => n.Id == clientViewModel.IdFacilitatorGroup).Name;
            }

            if (clientEntity.EmergencyContact != null)
            {
                clientViewModel.NameEmergencyContact = clientEntity.EmergencyContact.Name;
                clientViewModel.AddressEmergencyContact = clientEntity.EmergencyContact.Address;
                clientViewModel.AddressLine2EmergencyContact = clientEntity.EmergencyContact.Address;
                clientViewModel.CityEmergencyContact = clientEntity.EmergencyContact.City;
                clientViewModel.CountryEmergencyContact = clientEntity.EmergencyContact.Country;
                clientViewModel.EmailEmergencyContact = clientEntity.EmergencyContact.Email;
                clientViewModel.StateEmergencyContact = clientEntity.EmergencyContact.State;
                clientViewModel.PhoneEmergencyContact = clientEntity.EmergencyContact.Telephone;
                clientViewModel.PhoneSecundaryEmergencyContact = clientEntity.EmergencyContact.TelephoneSecondary;
                clientViewModel.ZipCodeEmergencyContact = clientEntity.EmergencyContact.ZipCode;
            }
            else
            {
                clientEntity.EmergencyContact = new EmergencyContactEntity();
            }
            if (clientViewModel.IdTCMClient != 0)
            {
                clientViewModel.AdmisionDateTCM = _context.TCMClient.FirstOrDefault(n => n.Id == clientViewModel.IdTCMClient).DataOpen;
            }
            else
            {
                clientViewModel.AdmisionDateTCM = _context.Clients.FirstOrDefault(n => n.Id == clientViewModel.Id).AdmisionDate;
            }
            return View(clientViewModel);
        }

        [Authorize(Roles = "Supervisor, Manager, Facilitator, Documents_Assistant, Frontdesk")]
        public async Task<IActionResult> ClientsWithoutMTP()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            else
            {
                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                if (clinic != null)
                {
                    if (User.IsInRole("Facilitator"))
                    {
                        FacilitatorEntity facilitator = await _context.Facilitators.FirstOrDefaultAsync(n => n.LinkedUser == user_logged.UserName);
                        return View(await _context.Clients
                                              .Include(c => c.MTPs)
                                              .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                        && c.MTPs.Count == 0
                                                        && c.OnlyTCM == false
                                                        && (c.IdFacilitatorPSR == facilitator.Id
                                                         || c.IdFacilitatorGroup == facilitator.Id
                                                         || c.IndividualTherapyFacilitator.Id == facilitator.Id)))
                                              .ToListAsync());

                    }
                    else
                    {
                        if (User.IsInRole("Documents_Assistant"))
                        {
                            return View(await _context.Clients
                                                      .Include(c => c.MTPs)
                                                      .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                                && c.MTPs.Count == 0
                                                                && c.OnlyTCM == false
                                                                && ((c.DocumentsAssistant != null && c.DocumentsAssistant.LinkedUser == user_logged.UserName)
                                                                    || c.DocumentsAssistant == null)))
                                                      .ToListAsync());

                        }
                        else
                        {
                            return View(await _context.Clients
                                                          .Include(c => c.MTPs)
                                                          .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                                    && c.MTPs.Count == 0
                                                                    && c.OnlyTCM == false))
                                                          .ToListAsync());
                        }
                    }
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, Supervisor, Frontdesk")]
        public async Task<IActionResult> ClientsWithoutDOC()
        {
            if (User.IsInRole("Admin"))
                return View(await _context.Clients
                                          .Include(c => c.Clinic)
                                          .Where(c => (c.MTPs.Count == 0
                                                        && c.OnlyTCM == false))
                                          .OrderBy(c => c.Clinic.Name)
                                          .ToListAsync());
            else
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic == null)
                    return View(null);

                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                if (clinic != null)
                {
                    List<ClientEntity> client = await _context.Clients
                                                              .Include(c => c.Clinic)
                                                              .Include(c => c.Documents)
                                                              .Where(c => (c.Clinic.Id == clinic.Id
                                                                        && c.OnlyTCM == false))
                                                              .OrderBy(c => c.Name)
                                                              .ToListAsync();
                    client = client.Where(c => c.MissingDoc != string.Empty).ToList();
                    return View(client);
                }                    
                else
                    return View(null);
            }
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public IActionResult AddDiagnostic(int id = 0, int idClient = 0)
        {
            ClientEntity client = _context.Clients.Include(n => n.Psychiatrist).FirstOrDefault(n => n.Id == idClient);
            if (id == 0)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                DiagnosticTempViewModel model = new DiagnosticTempViewModel
                {
                    IdDiagnostic = 0,
                    Diagnostics = _combosHelper.GetComboDiagnosticsByClinic(user_logged.Id),
                    UserName = user_logged.UserName,
                    IdClient = idClient,
                    DateIdentify = DateTime.Today,
                    Prescriber = (client == null)? string.Empty : (client.Psychiatrist == null) ? string.Empty : client.Psychiatrist.Name,
                    Active = true
                };
                return View(model);
            }
            else
            {
                //Edit
                return View(new DiagnosticTempViewModel());
            }
        }          

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public async Task<IActionResult> AddDiagnostic(int id, DiagnosticTempViewModel diagnosticTempViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (ModelState.IsValid)
            {
                if (id == 0)
                {
                    DiagnosticEntity diagnostic = await _context.Diagnostics.FirstOrDefaultAsync(d => d.Id == diagnosticTempViewModel.IdDiagnostic);
                    DiagnosticTempEntity diagnosticTemp = new DiagnosticTempEntity 
                    {
                        Id = 0,
                        Code = diagnostic.Code,
                        Description = diagnostic.Description,
                        Principal = diagnosticTempViewModel.Principal,
                        UserName = diagnosticTempViewModel.UserName,
                        IdClient = diagnosticTempViewModel.IdClient,
                        DateIdentify = diagnosticTempViewModel.DateIdentify,
                        Prescriber = diagnosticTempViewModel.Prescriber,
                        Active = diagnosticTempViewModel.Active
                    }; 
                    _context.Add(diagnosticTemp);
                    await _context.SaveChangesAsync();
                }
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDiagnostic", _context.DiagnosticsTemp.Where(d => d.UserName == user_logged.UserName && d.IdClient == diagnosticTempViewModel.IdClient).ToList()) });
            }
            
            DiagnosticTempViewModel model = new DiagnosticTempViewModel
            {
                IdDiagnostic = 0,
                Diagnostics = _combosHelper.GetComboDiagnosticsByClinic(user_logged.Id),
                UserName = diagnosticTempViewModel.UserName,
                IdClient = diagnosticTempViewModel.IdClient,
                DateIdentify = diagnosticTempViewModel.DateIdentify,
                Prescriber = diagnosticTempViewModel.Prescriber,
                Active = diagnosticTempViewModel.Active
            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddDiagnostic", model) });
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public IActionResult AddDocument(int id = 0, int idClient = 0)
        {
            if (id == 0)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);
                DocumentTempViewModel entity = new DocumentTempViewModel()
                {
                    IdDescription = 0,
                    Descriptions = _combosHelper.GetComboDocumentDescriptions(),
                    UserName = user_logged.UserName,
                    IdClient = idClient
                };
                return View(entity);
            }
            else
            {
                return View(new DiagnosticTempViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public async Task<IActionResult> AddDocument(int id, DocumentTempViewModel documentTempViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (ModelState.IsValid)
            {
                string documentPath = string.Empty;
                if (documentTempViewModel.DocumentFile != null)
                {
                    documentPath = await _imageHelper.UploadFileAsync(documentTempViewModel.DocumentFile, "Clients");
                }

                if (id == 0)
                {
                    DocumentTempEntity documentTemp = new DocumentTempEntity
                    {
                        Id = 0,
                        DocumentPath = documentPath,
                        DocumentName = documentTempViewModel.DocumentFile.FileName,
                        Description = DocumentUtils.GetDocumentByIndex(documentTempViewModel.IdDescription),
                        CreatedOn = DateTime.Now,
                        UserName = documentTempViewModel.UserName,
                        IdClient = documentTempViewModel.IdClient
                    };
                    _context.Add(documentTemp);
                    await _context.SaveChangesAsync();
                }
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDocument", _context.DocumentsTemp.Where(d => d.UserName == user_logged.UserName && d.IdClient == documentTempViewModel.IdClient).OrderByDescending(d => d.CreatedOn).ToList()) });
            }
            DocumentTempViewModel salida = new DocumentTempViewModel()
            {
                IdDescription = 0,
                Descriptions = _combosHelper.GetComboDocumentDescriptions(),
                UserName = documentTempViewModel.UserName,
                IdClient = documentTempViewModel.IdClient
            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddDocument", salida) });

        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public async Task<IActionResult> DeleteDiagnosticTemp(int? id)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DiagnosticTempEntity diagnostic = await _context.DiagnosticsTemp.FirstOrDefaultAsync(d => d.Id == id);
            if (diagnostic == null)
            {
                return RedirectToAction("Home/Error404");
            }

            _context.DiagnosticsTemp.Remove(diagnostic);
            await _context.SaveChangesAsync();

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDiagnostic", _context.DiagnosticsTemp.Where(d => d.UserName == user_logged.UserName && d.IdClient == diagnostic.IdClient).ToList()) });
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public async Task<IActionResult> DeleteDocumentTemp(int? id)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DocumentTempEntity documentTemp = await _context.DocumentsTemp.FirstOrDefaultAsync(d => d.Id == id);
            if (documentTemp == null)
            {
                return RedirectToAction("Home/Error404");
            }

           // DocumentEntity document = await _context.Documents.FirstOrDefaultAsync(d => d.FileUrl == documentTemp.DocumentPath);

            _context.DocumentsTemp.Remove(documentTemp);
           // if(document != null)
            //    _context.Documents.Remove(document);

            await _context.SaveChangesAsync();

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDocument", _context.DocumentsTemp.Where(d => d.UserName == user_logged.UserName && d.IdClient == documentTemp.IdClient).OrderByDescending(d => d.CreatedOn).ToList()) });
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public async Task<IActionResult> DeleteReferredTemp(int? id)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ReferredTempEntity referred = await _context.ReferredsTemp.FirstOrDefaultAsync(d => d.Id == id);
            if (referred == null)
            {
                return RedirectToAction("Home/Error404");
            }

            _context.ReferredsTemp.Remove(referred);
            await _context.SaveChangesAsync();

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewReferred", _context.ReferredsTemp.Where(d => d.CreatedBy == user_logged.UserName && d.IdClient == referred.IdClient).ToList()) });
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Frontdesk, TCMSupervisor")]
        public async Task<IActionResult> OpenDocument(int id)
        {
            DocumentTempEntity document = await _context.DocumentsTemp
                                                        .FirstOrDefaultAsync(d => d.Id == id);
            if (document == null)
            {
                return RedirectToAction("Home/Error404");
            }
            string mimeType = _mimeType.GetMimeType(document.DocumentName);
            return File(document.DocumentPath, mimeType);
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public void DeleteDiagnosticsTemp(ClientEntity client = null)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.UserName == User.Identity.Name);
            List<DiagnosticTempEntity> list_to_delete = new List<DiagnosticTempEntity>();
            //delete all DiagnosticsTemp by UserName
            if (client == null)
            {
                list_to_delete = _context.DiagnosticsTemp
                                         .Where(d => d.UserName == user_logged.UserName
                                            && d.IdClient == 0)
                                         .ToList();

            }
            else
            {
                list_to_delete = _context.DiagnosticsTemp
                                         .Where(d => d.UserName == user_logged.UserName 
                                           && d.IdClient == client.Id)
                                         .ToList();

            }
            _context.DiagnosticsTemp.RemoveRange(list_to_delete);
            _context.SaveChanges();
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public void DeleteDocumentsTemp(ClientEntity client = null)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.UserName == User.Identity.Name);
            
            List<DocumentTempEntity> list_to_delete = new List<DocumentTempEntity>();

            //delete all DocumentsTemp by UserName
            if (client == null)
            {
                list_to_delete = _context.DocumentsTemp
                                         .Where(d => d.UserName == user_logged.UserName
                                            && d.IdClient == 0)
                                         .ToList();

            }
            else
            {
                list_to_delete = _context.DocumentsTemp
                                         .Where(d => d.UserName == user_logged.UserName
                                             && d.IdClient == client.Id)
                                         .ToList();

            }
            _context.DocumentsTemp.RemoveRange(list_to_delete);
            _context.SaveChanges();
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public void DeleteReferredsTemp(ClientEntity client = null)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.UserName == User.Identity.Name);
           
            List<ReferredTempEntity> list_to_delete = new List<ReferredTempEntity>();
            //delete all ReferredsTemp by UserName
            if (client == null)
            {
                list_to_delete = _context.ReferredsTemp
                                         .Where(d => d.CreatedBy == user_logged.UserName
                                            && d.IdClient == 0)
                                         .ToList();
            }
            else
            {
                list_to_delete = _context.ReferredsTemp
                                         .Where(d => d.CreatedBy == user_logged.UserName
                                             && d.IdClient == client.Id)
                                         .ToList();
            }
            _context.ReferredsTemp.RemoveRange(list_to_delete);
            _context.SaveChanges();
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public void SetDiagnosticsTemp(ClientEntity client)
        {
            IEnumerable<Client_Diagnostic> clientsDiagnostics = _context.Clients_Diagnostics
                                                                        .Include(cd => cd.Diagnostic)
                                                                        .Where(cd => cd.Client == client).ToList();
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                      .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (clientsDiagnostics.Count() > 0)
            {
                DiagnosticTempEntity diagnostic;
                foreach (var item in clientsDiagnostics)
                {
                    diagnostic = new DiagnosticTempEntity
                    {
                        Code = item.Diagnostic.Code,
                        Description = item.Diagnostic.Description,
                        Principal = item.Principal,
                        UserName = user_logged.UserName,
                        IdClient = client.Id,
                        DateIdentify = item.DateIdentify,
                        Prescriber = item.Prescriber,
                        Active = item.Active
                    };
                    _context.Add(diagnostic);
                }
                _context.SaveChanges();
            }            
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public void SetDocumentsTemp(ClientEntity client)
        {
            IEnumerable<DocumentEntity> documents = _context.Documents                                                            
                                                            .Where(d => d.Client == client)                                                            
                                                            .ToList();
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                      .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (documents.Count() > 0)
            {
                DocumentTempEntity document;
                foreach (var item in documents)
                {
                    document = new DocumentTempEntity
                    {
                        Description = item.Description,
                        DocumentPath = item.FileUrl,
                        DocumentName = item.FileName,
                        CreatedOn = item.CreatedOn,
                        UserName = user_logged.UserName,
                        IdClient = client.Id
                    };
                    _context.Add(document);
                }
                _context.SaveChanges();
            }
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public void SetReferredsTemp(ClientEntity client)
        {
            List<Client_Referred> clientsReferreds = _context.Clients_Referreds
                                                             .Include(cd => cd.Referred)
                                                             .Where(cd => cd.Client == client).ToList();
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (clientsReferreds.Count() > 0)
            {
                ReferredTempEntity referred;
                foreach (var item in clientsReferreds)
                {
                    referred = new ReferredTempEntity
                    {
                        Title = item.Referred.Title,
                        Agency = item.Referred.Agency,
                        Service = item.Service,
                        Name = item.Referred.Name,
                        ReferredNote = item.ReferredNote,
                        Telephone = item.Referred.Telephone,
                        IdReferred = item.Referred.Id,
                        CreatedBy = user_logged.UserName,
                        IdClient = client.Id,
                        type = item.type
                    };
                    _context.Add(referred);
                }
                _context.SaveChanges();
            }
        }

        [Authorize(Roles = "Manager, Supervisor, Frontdesk")]
        public async Task<IActionResult> DocumentForClient()
        {
            if (User.IsInRole("Admin"))
                return View(await _context.Clients
                                          .Include(c => c.Clinic)
                                          .Where(c => (c.MTPs.Count == 0
                                                    && c.OnlyTCM == false))
                                          .OrderBy(c => c.Clinic.Name)
                                          .ToListAsync());
            else
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic == null)
                    return View(null);

                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                if (clinic != null)
                {
                    List<ClientEntity> client = await _context.Clients
                                                              .Include(c => c.Clinic)
                                                              .Include(c => c.MTPs)
                                                              .ThenInclude(cd => cd.AdendumList)
                                                              .Include(c => c.MTPs)
                                                              .ThenInclude(c => c.MtpReviewList)
                                                              .Include(c => c.FarsFormList)
                                                              .Include(c => c.Bio)
                                                              .Include(c => c.DischargeList)
                                                              .Include(c => c.IntakeAccessToServices)
                                                              .Include(c => c.IntakeAcknowledgementHipa)
                                                              .Include(c => c.IntakeConsentForRelease)
                                                              .Include(c => c.IntakeConsentForTreatment)
                                                              .Include(c => c.IntakeConsentPhotograph)
                                                              .Include(c => c.IntakeConsumerRights)
                                                              .Include(c => c.IntakeFeeAgreement)
                                                              .Include(c => c.IntakeMedicalHistory)
                                                              .Include(c => c.IntakeOrientationChecklist)
                                                              .Include(c => c.IntakeScreening)
                                                              .Include(c => c.IntakeTransportation)
                                                              .Include(c => c.IntakeTuberculosis)
                                                              .Where(c => (c.Clinic.Id == clinic.Id
                                                                        && c.OnlyTCM == false))
                                                              .OrderBy(c => c.Name)
                                                              .ToListAsync();
                    //client = client.Where(c => c.MissingDoc != string.Empty).ToList();
                    return View(client);
                }
                else
                    return View(null);
            }
        }

        [Authorize(Roles = "Manager, Facilitator, Supervisor, Documents_Assistant, Frontdesk")]
        public async Task<IActionResult> AllDocuments()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.UserType.ToString() == "Facilitator")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }
                FacilitatorEntity afacilitator = await _context.Facilitators.FirstOrDefaultAsync(n => n.LinkedUser == user_logged.UserName);
                List<ClientEntity> ClientList = await _context.Clients
                                                              .Include(g => g.FarsFormList)
                                                              .Include(g => g.IntakeAccessToServices)
                                                              .Include(g => g.IntakeAcknowledgementHipa)
                                                              .Include(g => g.IntakeConsentForRelease)
                                                              .Include(g => g.IntakeConsentForTreatment)
                                                              .Include(g => g.IntakeConsentPhotograph)
                                                              .Include(g => g.IntakeConsumerRights)
                                                              .Include(g => g.IntakeFeeAgreement)
                                                              .Include(g => g.IntakeMedicalHistory)
                                                              .Include(g => g.IntakeOrientationChecklist)
                                                              .Include(g => g.IntakeScreening)
                                                              .Include(g => g.IntakeTransportation)
                                                              .Include(g => g.IntakeTuberculosis)
                                                              .Include(g => g.DischargeList)
                                                              .Include(g => g.Bio)
                                                              .Include(g => g.Brief)
                                                              .Include(g => g.MedicationList)
                                                              .Include(g => g.MTPs)
                                                              .ThenInclude(g => g.AdendumList)
                                                              .Include(g => g.MTPs)
                                                              .ThenInclude(g => g.MtpReviewList)

                                                              .AsSplitQuery()

                                                              .Where(g => (g.IdFacilitatorPSR == afacilitator.Id
                                                                        && g.OnlyTCM == false))
                                                              .OrderBy(g => g.Name)
                                                              .ToListAsync();

                
                return View(ClientList);
            }
            if (user_logged.UserType.ToString() == "Manager" || user_logged.UserType.ToString() == "Supervisor")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }
                List<ClientEntity> ClientList = await _context.Clients
                                                              .Include(g => g.FarsFormList)
                                                              .Include(g => g.IntakeAccessToServices)
                                                              .Include(g => g.IntakeAcknowledgementHipa)
                                                              .Include(g => g.IntakeConsentForRelease)
                                                              .Include(g => g.IntakeConsentForTreatment)
                                                              .Include(g => g.IntakeConsentPhotograph)
                                                              .Include(g => g.IntakeConsumerRights)
                                                              .Include(g => g.IntakeFeeAgreement)
                                                              .Include(g => g.IntakeMedicalHistory)
                                                              .Include(g => g.IntakeOrientationChecklist)
                                                              .Include(g => g.IntakeScreening)
                                                              .Include(g => g.IntakeTransportation)
                                                              .Include(g => g.IntakeTuberculosis)
                                                              .Include(g => g.DischargeList)
                                                              .Include(g => g.Bio)
                                                              .Include(g => g.Brief)
                                                              .Include(g => g.MedicationList)
                                                              .Include(g => g.MTPs)
                                                              .ThenInclude(g => g.AdendumList)
                                                              .Include(g => g.MTPs)
                                                              .ThenInclude(g => g.MtpReviewList)

                                                              .AsSplitQuery()

                                                              .Where(g => (g.Clinic.Id == user_logged.Clinic.Id
                                                                        && g.OnlyTCM == false))
                                                              .OrderBy(g => g.Name)
                                                              .ToListAsync();

               return View(ClientList);
            }
            if (user_logged.UserType.ToString() == "Documents_Assistant")
            {
                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }
                List<ClientEntity> ClientList = await _context.Clients
                                                              .Include(g => g.FarsFormList)
                                                              .Include(g => g.IntakeAccessToServices)
                                                              .Include(g => g.IntakeAcknowledgementHipa)
                                                              .Include(g => g.IntakeConsentForRelease)
                                                              .Include(g => g.IntakeConsentForTreatment)
                                                              .Include(g => g.IntakeConsentPhotograph)
                                                              .Include(g => g.IntakeConsumerRights)
                                                              .Include(g => g.IntakeFeeAgreement)
                                                              .Include(g => g.IntakeMedicalHistory)
                                                              .Include(g => g.IntakeOrientationChecklist)
                                                              .Include(g => g.IntakeScreening)
                                                              .Include(g => g.IntakeTransportation)
                                                              .Include(g => g.IntakeTuberculosis)
                                                              .Include(g => g.DischargeList)
                                                              .Include(g => g.Bio)
                                                              .Include(g => g.Brief)
                                                              .Include(g => g.MedicationList)
                                                              .Include(g => g.MTPs)
                                                              .ThenInclude(g => g.AdendumList)
                                                              .Include(g => g.MTPs)
                                                              .ThenInclude(g => g.MtpReviewList)

                                                              .AsSplitQuery()

                                                              .Where(g => (g.Clinic.Id == user_logged.Clinic.Id
                                                                        && g.Bio.CreatedBy == user_logged.UserName
                                                                        && g.OnlyTCM == false))
                                                              .OrderBy(g => g.Name)
                                                              .ToListAsync();

                return View(ClientList);
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public IActionResult AddReferred(int id = 0, int idClient = 0)
        {
            if (id == 0)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                ReferredTempViewModel model = new ReferredTempViewModel
                {
                    Id_Referred = 0,
                    Referreds = _combosHelper.GetComboReferredsByClinic(user_logged.Id),
                    IdServiceAgency = 0,
                    ServiceAgency = _combosHelper.GetComboServiceAgency(),
                    CreatedBy = user_logged.UserName,
                    CreatedOn = DateTime.Now,
                    IdClient = idClient,
                    IdType = 0,
                    Types = _combosHelper.GetComboTypeReferred(),

                };
                return View(model);
            }
            else
            {
                //Edit
                return View(new ReferredTempViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public async Task<IActionResult> AddReferred(int id, ReferredTempViewModel referredTempViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (ModelState.IsValid)
            {
                if (id == 0)
                {
                    ReferredEntity referred = await _context.Referreds.FirstOrDefaultAsync(d => d.Id == referredTempViewModel.Id_Referred);
                    ReferredTempEntity referredTemp = new ReferredTempEntity
                    {
                        Id = 0,
                        Title = referred.Title,
                        Agency = referred.Agency,
                        Service = ServiceAgencyUtils.GetServiceAgencyByIndex(referredTempViewModel.IdServiceAgency),
                        Name = referred.Name,
                        ReferredNote = referredTempViewModel.ReferredNote, 
                        Address = referred.Address,
                        Telephone = referred.Telephone,
                        IdReferred = referred.Id,
                        CreatedBy = referredTempViewModel.CreatedBy,
                        CreatedOn = referredTempViewModel.CreatedOn,
                        IdClient = referredTempViewModel.IdClient,
                        type = ReferredUtils.GetTypeReferredByIndex(referredTempViewModel.IdType)

                    };
                    _context.Add(referredTemp);
                    await _context.SaveChangesAsync();
                }
                
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewReferred", _context.ReferredsTemp.Where(r => r.CreatedBy == user_logged.UserName && r.IdClient == referredTempViewModel.IdClient).ToList()) });
            }
                       
            ReferredTempViewModel model = new ReferredTempViewModel
            {
                IdReferred = 0,
                Referreds = _combosHelper.GetComboReferredsByClinic(user_logged.Id),
                IdServiceAgency = 0,
                ServiceAgency = _combosHelper.GetComboServiceAgency(),
                CreatedOn = referredTempViewModel.CreatedOn,
                CreatedBy = referredTempViewModel.CreatedBy,
                IdClient = referredTempViewModel.IdClient,
                type = referredTempViewModel.type
            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddReferred", model) });
        }        

        public JsonResult GetNameReferred(int idReferred)
        {
            ReferredEntity referred = _context.Referreds.FirstOrDefault(o => o.Id == idReferred);
            string text = "Select Referred";
            if (referred != null)
            {
                text = referred.Name;
            }
            return Json(text);
        }

        public JsonResult GetTitleReferred(int idReferred)
        {
            ReferredEntity referred = _context.Referreds.FirstOrDefault(o => o.Id == idReferred);
            string text = "Select Referred";
            if (referred != null)
            {
                text = referred.Title;
            }
            return Json(text);
        }

        public JsonResult GetAgencyReferred(int idReferred)
        {
            ReferredEntity referred = _context.Referreds.FirstOrDefault(o => o.Id == idReferred);
            string text = "Select Referred";
            if (referred != null)
            {
                text = referred.Agency;
            }
            return Json(text);
        }

        public JsonResult GetAddressReferred(int idReferred)
        {
            ReferredEntity referred = _context.Referreds.FirstOrDefault(o => o.Id == idReferred);
            string text = "Select Referred";
            if (referred != null)
            {
                text = referred.Address;
            }
            return Json(text);
        }

        public JsonResult GetPhoneReferred(int idReferred)
        {
            ReferredEntity referred = _context.Referreds.FirstOrDefault(o => o.Id == idReferred);
            string text = "Select Referred";
            if (referred != null)
            {
                text = referred.Telephone;
            }
            return Json(text);
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, Frontdesk, Biller")]
        public async Task<IActionResult> ClientHistory(int idClient = 0, int idError = 0)
        {
            if (idError == 1)
            {
                ViewBag.Error = "Mtpr";
            }
            else
            {
                if (idError == 2)
                {
                    ViewBag.Error = "Addendum";
                }
                else
                {
                    ViewBag.Error = "N";
                }
            }
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            ClientEntity client = await _context.Clients
                                                .Include(w => w.MTPs)
                                                .ThenInclude(w => w.MtpReviewList)

                                                .Include(w => w.MTPs)
                                                .ThenInclude(w => w.AdendumList)
                                                .ThenInclude(w => w.Goals)
                                               
                                                .Include(w => w.Bio)
                                                .Include(w => w.Brief)
                                                .Include(w => w.FarsFormList)
                                                .Include(w => w.DischargeList)

                                                .Include(w => w.Workdays_Clients)
                                                .ThenInclude(w => w.Workday)

                                                .Include(w => w.Workdays_Clients)
                                                .ThenInclude(w => w.Facilitator)

                                                .Include(w => w.Workdays_Clients)
                                                .ThenInclude(w => w.NoteP)

                                                .Include(w => w.Workdays_Clients)
                                                .ThenInclude(w => w.IndividualNote)

                                                .Include(w => w.Workdays_Clients)
                                                .ThenInclude(w => w.Note)

                                                .Include(w => w.Workdays_Clients)
                                                .ThenInclude(w => w.GroupNote)

                                                .Include(w => w.Workdays_Clients)
                                                .ThenInclude(w => w.GroupNote2)

                                                .Include(w => w.Workdays_Clients)
                                                .ThenInclude(w => w.Schedule)

                                                .Include(w => w.IntakeMedicalHistory)
                                                .AsSplitQuery()
                                                .FirstOrDefaultAsync(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                   && w.Id == idClient));



            return View(client);
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, Frontdesk")]
        public async Task<IActionResult> ClientProblemList(int idClient = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic || !user_logged.Clinic.Setting.MHProblems)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            DateTime date = new DateTime();

            ClientEntity client = await _context.Clients
                                                .Include(w => w.MTPs)
                                                .ThenInclude(w => w.MtpReviewList)

                                                .Include(w => w.Bio)
                                                .Include(w => w.Brief)
                                                .Include(w => w.FarsFormList)
                                                .Include(w => w.DischargeList)

                                                .Include(w => w.Workdays_Clients)
                                                .ThenInclude(w => w.Workday)

                                                .Include(w => w.Workdays_Clients)
                                                .ThenInclude(w => w.Facilitator)

                                                .Include(w => w.Workdays_Clients)
                                                .ThenInclude(w => w.NoteP)

                                                .Include(w => w.Workdays_Clients)
                                                .ThenInclude(w => w.IndividualNote)

                                                .Include(w => w.Workdays_Clients)
                                                .ThenInclude(w => w.Note)

                                                .Include(w => w.Workdays_Clients)
                                                .ThenInclude(w => w.GroupNote)

                                                .Include(w => w.Workdays_Clients)
                                                .ThenInclude(w => w.GroupNote2)

                                                .Include(w => w.MTPs)
                                                .ThenInclude(w => w.AdendumList)

                                                .Include(w => w.IntakeMedicalHistory)
                                                .Include(w => w.Doctor)
                                                .Include(w => w.MedicationList)

                                                .FirstOrDefaultAsync(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                   && w.Id == idClient));

            int cant_Fars = 0;
            Problem tempProblem = new Problem();

            List<Problem> problem = new List<Problem>();
            //BIO
            if (client.MTPs.Count() > 0 && (client.Bio != null || client.Brief != null))
            {
                if (client.AdmisionDate > client.MTPs.ElementAtOrDefault(0).AdmissionDateMTP)
                {
                    tempProblem.Name = "MTP Date";
                    tempProblem.Description = "MTP date is prior to admission date";
                    tempProblem.Active = 0;
                }
                else
                {
                    if (client.Bio != null)
                    {
                        if (client.Bio.DateBio > client.MTPs.ElementAtOrDefault(0).AdmissionDateMTP)
                        {
                            tempProblem.Name = "MTP Date";
                            tempProblem.Description = "MTP date is prior to BIO date";
                            tempProblem.Active = 0;
                        }
                        else
                        {
                            tempProblem.Name = "MTP Date";
                            tempProblem.Description = "MTP date is posterior to admision date and BIO date";
                            tempProblem.Active = 2;
                        }
                    }
                    if (client.Brief != null)
                    {
                        if (client.Brief.DateBio > client.MTPs.ElementAtOrDefault(0).AdmissionDateMTP)
                        {
                            tempProblem.Name = "MTP Date";
                            tempProblem.Description = "MTP date is prior to BRIEF date";
                            tempProblem.Active = 0;
                        }
                        else
                        {
                            tempProblem.Name = "MTP Date";
                            tempProblem.Description = "MTP date is posterior to admision date and BRIEF date";
                            tempProblem.Active = 2;
                        }
                    }

                }
                problem.Add(tempProblem);
                cant_Fars++;
                tempProblem = new Problem();
            }
            else
            {
                if (client.MTPs.Count() == 0)
                {
                    tempProblem.Name = "MTP";
                    tempProblem.Description = "MTP doesn't exist";
                    tempProblem.Active = 0;
                }
                else
                {
                    if (client.AdmisionDate > client.MTPs.ElementAtOrDefault(0).AdmissionDateMTP)
                    {
                        tempProblem.Name = "MTP Date";
                        tempProblem.Description = "MTP date is prior to admission date";
                        tempProblem.Active = 0;
                    }
                    else
                    {
                        tempProblem.Name = "MTP Date";
                        tempProblem.Description = "MTP date is posterior to admision date and BIO date";
                        tempProblem.Active = 2;
                    }
                    cant_Fars++;
                }
                problem.Add(tempProblem);
                tempProblem = new Problem();
            }
           
            if (client.Bio != null || client.Brief != null)
            {
                if (client.Bio != null)
                {
                    if (client.AdmisionDate > client.Bio.DateBio)
                    {
                        tempProblem.Name = "BIO Date";
                        tempProblem.Description = "BIO date is prior to admission date";
                        tempProblem.Active = 0;
                    }
                    else
                    {
                        tempProblem.Name = "BIO Date";
                        tempProblem.Description = "BIO date is posterior to admission date";
                        tempProblem.Active = 2;
                    }
                }
                if (client.Brief != null)
                {
                    if (client.AdmisionDate > client.Brief.DateBio)
                    {
                        tempProblem.Name = "BRIEF Date";
                        tempProblem.Description = "BRIEF date is prior to admission date";
                        tempProblem.Active = 0;
                    }
                    else
                    {
                        tempProblem.Name = "BRIEF Date";
                        tempProblem.Description = "BRIEF date is posterior to admission date";
                        tempProblem.Active = 2;
                    }
                }
            }
            else
            {
                tempProblem.Name = "BIO Date";
                tempProblem.Description = "BIO doesn't exist";
                tempProblem.Active = 0;
            }
            problem.Add(tempProblem);
            tempProblem = new Problem();

            //Medical History
            if (client.IntakeMedicalHistory == null)
            {
                tempProblem.Name = "Medical History";
                tempProblem.Description = "The client has no Medical History";
                tempProblem.Active = 0;

                problem.Add(tempProblem);
                tempProblem = new Problem();
            }
            else
            {
                if (client.Doctor == null)
                {
                    tempProblem.Name = "Medical History";
                    tempProblem.Description = "The client has no Doctor";
                    tempProblem.Active = 1;

                    problem.Add(tempProblem);
                    tempProblem = new Problem();
                }
                if (client.MedicationList.Count() == 0)
                {
                    tempProblem.Name = "Medical History"; ;
                    tempProblem.Description = "The client has no Medication list";
                    tempProblem.Active = 1;

                    problem.Add(tempProblem);
                    tempProblem = new Problem();
                }
            }

            //FARS initial
            if (client.FarsFormList.Count() > 0 && client.Bio != null )
            {
                if (client.FarsFormList.Where(n => n.Type == FARSType.Initial).Count() == 0)
                {
                    tempProblem.Name = "Initial FARS";
                    tempProblem.Description = "Initial FARS not exists";
                    tempProblem.Active = 0;

                    problem.Add(tempProblem);
                    tempProblem = new Problem();
                }
                else
                {
                    if (client.FarsFormList.Where(n => n.EvaluationDate == client.Bio.DateBio && n.Type == FARSType.Initial).Count() == 0)
                    {
                        tempProblem.Name = "Initial FARS";
                        tempProblem.Description = "Initial FARS date doesn't match with the BIO document";
                        tempProblem.Active = 0;
                    }
                    else
                    {
                        tempProblem.Name = "Initial FARS";
                        tempProblem.Description = "Initial FARS date match with the BIO document";
                        tempProblem.Active = 2;
                    }
                    problem.Add(tempProblem);
                    tempProblem = new Problem();
                }
              
            }

            //MTP
            bool mtpr = false;
            if (client.MTPs.Count() > 0 && client.MTPs.FirstOrDefault(n => n.Active == true).MtpReviewList.Count() > 0)
            {
                if (client.MTPs.FirstOrDefault(n => n.Active == true).AdmissionDateMTP.AddMonths(client.MTPs.FirstOrDefault(n => n.Active == true).NumberOfMonths.Value) < client.MTPs.FirstOrDefault(n => n.Active == true).MtpReviewList.Min(n => n.DataOfService))
                {
                    tempProblem.Name = "MTP Review";
                    tempProblem.Description = "MTP Review date is out of term";
                    tempProblem.Active = 0;
                }
                else
                {
                    tempProblem.Name = "MTP Review";
                    tempProblem.Description = "MTP Review date is in term";
                    tempProblem.Active = 2;
                }
                problem.Add(tempProblem);
                cant_Fars++;
                mtpr = true;
            }
            
            tempProblem = new Problem();

            if (client.Status == StatusType.Close)
            {
                if (client.MTPs.Count() > 0)
                {
                    
                    if (client.MTPs.ElementAtOrDefault(0).MtpReviewList.Count() > 0)
                    {
                        if (client.MTPs.ElementAtOrDefault(0).DateOfUpdate > client.DateOfClose || client.MTPs.ElementAtOrDefault(0).MtpReviewList.ElementAtOrDefault(0).ReviewedOn > client.DateOfClose)
                        {
                            tempProblem.Name = "Date of Close";
                            tempProblem.Description = "Client ends therapy before expiration (MTPR)";
                            tempProblem.Active = 1;
                        }
                        else
                        {
                            if (client.DateOfClose == date)
                            {
                                tempProblem.Name = "Date of Close";
                                tempProblem.Description = "Date of close is out of term";
                                tempProblem.Active = 0;
                            }
                            else
                            {
                                tempProblem.Name = "Date of Close";
                                tempProblem.Description = "Date of close is in term";
                                tempProblem.Active = 2;
                            }
                            
                        }
                    }
                    else
                    {
                        if (client.MTPs.ElementAtOrDefault(0).DateOfUpdate > client.DateOfClose)
                        {
                            tempProblem.Name = "Date of Close";
                            tempProblem.Description = "Client ends therapy before expiration (MTP)";
                            tempProblem.Active = 1;
                        }
                        else
                        {
                            if (client.DateOfClose == date)
                            {
                                tempProblem.Name = "Date of Close";
                                tempProblem.Description = "Date of close is out of term";
                                tempProblem.Active = 0;
                            }
                            else
                            {
                                tempProblem.Name = "Date of Close";
                                tempProblem.Description = "Date of close is in term";
                                tempProblem.Active = 2;
                            }
                            
                        }
                    }
                    problem.Add(tempProblem);
                }
            }
            tempProblem = new Problem();

            if (client.Bio != null)
            {
                if (client.Bio.Setting == string.Empty)
                {
                    tempProblem.Name = "BIO";
                    tempProblem.Description = "Not exists setting in the BIO";
                    tempProblem.Active = 1;
                    problem.Add(tempProblem);
                    tempProblem = new Problem();
                }

                if (client.MTPs.Count() > 0)
                {
                    foreach (var item in client.MTPs)
                    {
                        if (item.Setting == string.Empty)
                        {
                            tempProblem.Name = "MTP";
                            tempProblem.Description = "Not exists setting in the MTP";
                            tempProblem.Active = 1;
                            problem.Add(tempProblem);
                            tempProblem = new Problem();
                        }
                        if (item.Setting != client.Bio.Setting)
                        {
                            tempProblem.Name = "Setting";
                            tempProblem.Description = "Review setting in the documents(BIO, MTP)";
                            tempProblem.Active = 1;
                            problem.Add(tempProblem);
                            tempProblem = new Problem();
                        }
                    }
                }
            }
            
           

            if (client.Brief != null)
            {
                if (client.Brief.Setting == string.Empty)
                {
                    tempProblem.Name = "Brief";
                    tempProblem.Description = "Not exists setting in the Brief";
                    tempProblem.Active = 1;
                    problem.Add(tempProblem);
                    tempProblem = new Problem();
                }

                if (client.MTPs.Count() > 0)
                {
                    foreach (var item in client.MTPs)
                    {
                        if (item.Setting == string.Empty)
                        {
                            tempProblem.Name = "MTP";
                            tempProblem.Description = "Not exists setting in the MTP";
                            tempProblem.Active = 1;
                            problem.Add(tempProblem);
                            tempProblem = new Problem();
                        }
                        if (item.Setting != client.Bio.Setting)
                        {
                            tempProblem.Name = "Setting";
                            tempProblem.Description = "Review setting in the documents(Brief, MTP)";
                            tempProblem.Active = 1;
                            problem.Add(tempProblem);
                            tempProblem = new Problem();
                        }
                    }
                }
            }

            //Discharge
            bool dischage_ind = false;
            bool dischage_psr = false;
            bool dischage_group = false;
            if (client.Status == StatusType.Close)
            {
                int cant_Discharge = 0;
                if (client.Workdays_Clients.Where(n => n.Note != null).Count() > 0 || client.Workdays_Clients.Where(n => n.NoteP != null).Count() > 0)
                {
                    cant_Discharge++;
                    dischage_psr = true;
                    cant_Fars++;
                }
                if (client.Workdays_Clients.Where(n => n.IndividualNote != null).Count() > 0)
                {
                    cant_Discharge++;
                    dischage_ind = true;
                    cant_Fars++;
                }
                if (client.Workdays_Clients.Where(n => n.GroupNote != null).Count() > 0 || client.Workdays_Clients.Where(n => n.GroupNote2 != null).Count() > 0)
                {
                    cant_Discharge++;
                    dischage_group = true;
                    cant_Fars++;
                }

                if (client.DischargeList.Count() != cant_Discharge)
                {
                    tempProblem.Name = "Discharge";
                    tempProblem.Description = "The amount of discharge is not correct";
                    tempProblem.Active = 0;
                    problem.Add(tempProblem);
                    tempProblem = new Problem();

                    if (client.DischargeList.Count() > 0)
                    {
                        bool salida = false;
                        foreach (var item in client.DischargeList)
                        {
                            if (item.DateDischarge != client.DateOfClose)
                            {
                                tempProblem.Name = "Discharge";
                                tempProblem.Description = "Discharge date doesn't match with date of close";
                                tempProblem.Active = 1;
                                salida = true;
                                break;
                            }
                        }
                        if (salida == false)
                        {
                            tempProblem.Name = "Discharge";
                            tempProblem.Description = "Discharge date match with date of close";
                            tempProblem.Active = 2;
                        }
                        problem.Add(tempProblem);
                        tempProblem = new Problem();
                    }
                }
                else
                {
                    if (client.DischargeList.Count() > 0)
                    {
                        bool salida = false;
                        foreach (var item in client.DischargeList)
                        {
                            if (item.DateDischarge != client.DateOfClose)
                            {
                                tempProblem.Name = "Discharge";
                                tempProblem.Description = "Discharge date doesn't match with date of close";
                                tempProblem.Active = 0;
                                salida = true;
                                break;
                            }
                        }
                        if (salida == false)
                        {
                            tempProblem.Name = "Discharge";
                            tempProblem.Description = "Discharge date match with date of close";
                            tempProblem.Active = 2;
                        }
                        problem.Add(tempProblem);
                        tempProblem = new Problem();
                    }
                }
               
            }

            bool dischargeEdition = false;
            bool dischargePending = false;
            foreach (var item in client.DischargeList)
            {
                if (item.Status == DischargeStatus.Edition)
                {
                    dischargeEdition = true;
                }
                if (item.Status == DischargeStatus.Pending)
                {
                    dischargePending = true;
                }
            }

            if (dischargeEdition == true)
            {
                tempProblem.Name = "Discharge";
                tempProblem.Description = "Discharge in edition";
                tempProblem.Active = 1;
                problem.Add(tempProblem);
                tempProblem = new Problem();
            }

            if (dischargePending == true)
            {
                tempProblem.Name = "Discharge";
                tempProblem.Description = "Pending for approval discharge";
                tempProblem.Active = 1;
                problem.Add(tempProblem);
                tempProblem = new Problem();
            }

            //FARS continued
            cant_Fars += client.MTPs.Sum(n => n.AdendumList.Count());

            if (client.FarsFormList.Count() != cant_Fars)
            {
                tempProblem.Name = "Amount of FARS";
                tempProblem.Description = "The amount of FARS is not correct (must be " + cant_Fars + ")";
                tempProblem.Active = 0;
            }
            else
            {
                tempProblem.Name = "Amount of FARS";
                tempProblem.Description = "The amount of FARS is correct";
                tempProblem.Active = 2;
            }
            problem.Add(tempProblem);
            tempProblem = new Problem();

            bool farsEdition = false;
            bool farsPending = false;
            bool fars_mtpr = false;
            bool fars_d_psr = false;
            bool fars_d_ind = false;
            bool fars_d_group = false;
            foreach (var item in client.FarsFormList)
            {
                if (mtpr == true && item.Type == FARSType.MtpReview)
                {
                    if (client.MTPs.FirstOrDefault(n => n.Active == true).MtpReviewList.FirstOrDefault().DataOfService == item.EvaluationDate)
                    {
                        fars_mtpr = true;
                    }
                }
                if (dischage_psr == true && item.Type == FARSType.Discharge_PSR)
                {
                    if (client.DischargeList.Where(n => n.TypeService == ServiceType.PSR && n.DateDischarge == item.EvaluationDate).Count() > 0)
                    {
                        fars_d_psr = true;
                    }
                }
                if (dischage_group == true && item.Type == FARSType.Discharge_Group)
                {
                    if (client.DischargeList.Where(n => n.TypeService == ServiceType.Group && n.DateDischarge == item.EvaluationDate).Count() > 0)
                    {
                        fars_d_group = true;
                    }
                }
                if (dischage_ind == true && item.Type == FARSType.Discharge_Ind)
                {
                    if (client.DischargeList.Where(n => n.TypeService == ServiceType.Individual && n.DateDischarge == item.EvaluationDate).Count() > 0)
                    {
                        fars_d_ind = true;
                    }
                }
                if (item.Status == FarsStatus.Edition)
                {
                    farsEdition = true;
                }
                if (item.Status == FarsStatus.Pending)
                {
                    farsPending = true;
                }
            }

            if (mtpr == true && fars_mtpr == false)
            {
                if (client.MTPs.Sum(n => n.MtpReviewList.Count()) > client.FarsFormList.Where(n => n.Type == FARSType.MtpReview).Count())
                {
                    foreach (var item in client.MTPs)
                    {
                        foreach (var element in item.MtpReviewList)
                        {
                            if (client.FarsFormList.Exists(n => n.EvaluationDate == element.DataOfService && n.Type == FARSType.MtpReview) == true)
                            {

                            }
                            else
                            {
                                tempProblem.Name = "FARS";
                                tempProblem.Description = "FARS with incompatible date (MTPR)  created for (" + element.Therapist + ")";
                                tempProblem.Active = 1;
                                problem.Add(tempProblem);
                                tempProblem = new Problem();

                            }
                        }
                    }

                }

            }



            if (client.MTPs.Sum(n => n.AdendumList.Count()) > client.FarsFormList.Where(n => n.Type == FARSType.Addendums).Count())
            {
                int cant_Addendums = client.MTPs.Sum(n => n.AdendumList.Count()) - client.FarsFormList.Where(n => n.Type == FARSType.Addendums).Count();
                tempProblem.Name = "FARS";
                tempProblem.Description = "The amount of FARS does not match with Addendums (" + cant_Addendums + ")";
                tempProblem.Active = 0;
                problem.Add(tempProblem);
                tempProblem = new Problem();

            }
            else
            {
                foreach (var item in client.MTPs)
                {
                    foreach (var element in item.AdendumList)
                    {
                        if (client.FarsFormList.Exists(n => n.EvaluationDate == element.Dateidentified && n.Type == FARSType.Addendums) == true)
                        {

                        }
                        else
                        {
                            tempProblem.Name = "FARS";
                            if (element.Facilitator != null)
                            {
                                tempProblem.Description = "FARS with incompatible date (addendum)  created for (" + element.Facilitator.Name + ")";
                            }
                            else
                            {
                                tempProblem.Description = "FARS with incompatible date (addendum)  created for (" + element.CreatedBy + ")";
                            }
                            tempProblem.Active = 1;
                            problem.Add(tempProblem);
                            tempProblem = new Problem();
                        }

                    }
                }
            }

            if (dischage_psr == true)
            {
                if (client.FarsFormList.Where(n => n.Type == FARSType.Discharge_PSR).Count() == 0)
                {
                    tempProblem.Name = "FARS";
                    tempProblem.Description = "Client without FARS (Discharge PSR)";
                    tempProblem.Active = 0;
                    problem.Add(tempProblem);
                    tempProblem = new Problem();
                }
                else
                {
                    if (dischage_psr == true && fars_d_psr == false)
                    {
                        tempProblem.Name = "FARS";
                        tempProblem.Description = "FARS with incompatible date (Discharge PSR)";
                        tempProblem.Active = 1;
                        problem.Add(tempProblem);
                        tempProblem = new Problem();
                    }
                }
            }

            if (dischage_group == true)
            {
                if (client.FarsFormList.Where(n => n.Type == FARSType.Discharge_Group).Count() == 0)
                {
                    tempProblem.Name = "FARS";
                    tempProblem.Description = "Client without FARS (Discharge Group)";
                    tempProblem.Active = 0;
                    problem.Add(tempProblem);
                    tempProblem = new Problem();
                }
                else
                {
                    if (dischage_group == true && fars_d_group == false)
                    {
                        tempProblem.Name = "FARS";
                        tempProblem.Description = "FARS with incompatible date (Discharge Group)";
                        tempProblem.Active = 1;
                        problem.Add(tempProblem);
                        tempProblem = new Problem();
                    }
                }
            }

            if (dischage_ind == true)
            {
                if (client.FarsFormList.Where(n => n.Type == FARSType.Discharge_Ind).Count() == 0)
                {
                    tempProblem.Name = "FARS";
                    tempProblem.Description = "Client without FARS (Discharge Ind.)";
                    tempProblem.Active = 0;
                    problem.Add(tempProblem);
                    tempProblem = new Problem();
                }
                else
                {
                    if (dischage_ind == true && fars_d_ind == false)
                    {
                        tempProblem.Name = "FARS";
                        tempProblem.Description = "FARS with incompatible date (Discharge Ind.)";
                        tempProblem.Active = 1;
                        problem.Add(tempProblem);
                        tempProblem = new Problem();
                    }
                }
            }

            if (farsEdition == true)
            {
                tempProblem.Name = "FARS";
                tempProblem.Description = "FARS in edition";
                tempProblem.Active = 1;
                problem.Add(tempProblem);
                tempProblem = new Problem();
            }

            if (farsPending == true)
            {
                tempProblem.Name = "FARS";
                tempProblem.Description = "Pending for approval FARS";
                tempProblem.Active = 1;
                problem.Add(tempProblem);
                tempProblem = new Problem();
            }

            if (client.MTPs.Count() > 0 && client.Bio != null)
            {
                if (client.Workdays_Clients.Where(d => d.Workday.Date <= client.MTPs.ElementAtOrDefault(0).AdmissionDateMTP).Count() > 0 || client.Workdays_Clients.Where(d => d.Workday.Date <= client.Bio.DateBio).Count() > 0)
                {
                    tempProblem.Name = "Notes";
                    tempProblem.Description = "Notes are overdue (before)";
                    tempProblem.Active = 0;
                }
                else
                {
                    tempProblem.Name = "Notes";
                    tempProblem.Description = "Notes are in term (before)";
                    tempProblem.Active = 2;
                }
                problem.Add(tempProblem);
                tempProblem = new Problem();
            }

            if (client.Status == StatusType.Close)
            {
                if (client.Workdays_Clients.Where(d => d.Workday.Date > client.DateOfClose).Count() > 0)
                {
                    tempProblem.Name = "Notes";
                    tempProblem.Description = "Notes are overdue (after)";
                    tempProblem.Active = 0;
                }
                else
                {
                    tempProblem.Name = "Notes";
                    tempProblem.Description = "Notes are in term (after)";
                    tempProblem.Active = 2;
                }
            }
            else
            {
                tempProblem.Name = "Notes";
                tempProblem.Description = "Notes are in term (after)";
                tempProblem.Active = 2;
            }

            problem.Add(tempProblem);
            tempProblem = new Problem();

            List<Workday_Client> not_started_list;
            not_started_list = await _context.Workdays_Clients
                                             .Include(wc => wc.Note)
                                             .Include(wc => wc.NoteP)
                                             .Include(wc => wc.GroupNote)
                                             .Include(wc => wc.GroupNote2)
                                             .Include(wc => wc.Workday)
                                             .Where(wc => (wc.Client.Id == idClient
                                                 && wc.Present == true
                                                 && (wc.Workday.Service == ServiceType.PSR
                                                    || wc.Workday.Service == ServiceType.Group)))
                                             .ToListAsync();
            not_started_list = not_started_list.Where(wc => (wc.Note == null && wc.NoteP == null && wc.Workday.Service == ServiceType.PSR) 
                                                    || ( wc.Workday.Service == ServiceType.Group && wc.GroupNote == null && wc.GroupNote2 == null))
                                               .ToList();

            if (not_started_list.Count() > 0)
            {
                tempProblem.Name = "Notes";
                tempProblem.Description = "Not started Notes (" + not_started_list.Count() + ")";
                tempProblem.Active = 0;
            }
            else
            {
                tempProblem.Name = "Notes";
                tempProblem.Description = "Not started Notes (0)";
                tempProblem.Active = 2;
            }

            problem.Add(tempProblem);
            tempProblem = new Problem();

            bool noteEdition = false;
            bool notePending = false;
            int editionCountNote = 0;
            int pendingCountNote = 0;
            foreach (var item in client.Workdays_Clients)
            {
                if (item.Note != null)
                {
                    if (item.Note.Status == NoteStatus.Edition)
                    {
                        noteEdition = true;
                        editionCountNote++;
                    }
                }
                if (item.NoteP != null)
                {
                    if (item.NoteP.Status == NoteStatus.Edition)
                    {
                        noteEdition = true;
                        editionCountNote++;
                    }
                }
                if (item.IndividualNote != null)
                {
                    if (item.IndividualNote.Status == NoteStatus.Edition)
                    {
                        noteEdition = true;
                        editionCountNote++;
                    }
                }
                if (item.GroupNote != null)
                {
                    if (item.GroupNote.Status == NoteStatus.Edition)
                    {
                        noteEdition = true;
                        editionCountNote++;
                    }
                }
                if (item.GroupNote2 != null)
                {
                    if (item.GroupNote2.Status == NoteStatus.Edition)
                    {
                        noteEdition = true;
                        editionCountNote++;
                    }
                }

                if (item.Note != null)
                {
                    if (item.Note.Status == NoteStatus.Pending)
                    {
                        notePending = true;
                        pendingCountNote++;
                    }
                }
                if (item.NoteP != null)
                {
                    if (item.NoteP.Status == NoteStatus.Pending)
                    {
                        notePending = true;
                        pendingCountNote++;
                    }
                }
                if (item.IndividualNote != null)
                {
                    if (item.IndividualNote.Status == NoteStatus.Pending)
                    {
                        notePending = true;
                        pendingCountNote++;
                    }
                }
                if (item.GroupNote != null)
                {
                    if (item.GroupNote.Status == NoteStatus.Pending)
                    {
                        notePending = true;
                        pendingCountNote++;
                    }
                }
                if (item.GroupNote2 != null)
                {
                    if (item.GroupNote2.Status == NoteStatus.Pending)
                    {
                        notePending = true;
                        pendingCountNote++;
                    }
                }
            }

            if (noteEdition == true)
            {
                tempProblem.Name = "Notes";
                tempProblem.Description = "Notes in edition (" + editionCountNote + ")";
                tempProblem.Active = 0;
                problem.Add(tempProblem);
                tempProblem = new Problem();
            } 

            if (notePending == true)
            {
                tempProblem.Name = "Notes";
                tempProblem.Description = "Pending for approval Notes (" + pendingCountNote + ")";
                tempProblem.Active = 0;
                problem.Add(tempProblem);
                tempProblem = new Problem();
            }

            
            tempProblem = new Problem();

            if (client.Workdays_Clients.Count(n => n.Present == false && n.BilledDate != null) > 0 || client.Workdays_Clients.Count(n => n.Present == true && n.BilledDate == null) > 0)
            {
                if (client.Workdays_Clients.Count(n => n.Present == false && n.BilledDate != null) > 0)
                {
                    tempProblem.Name = "Billed";
                    tempProblem.Description = "There are billed clients who have been absent (" + client.Workdays_Clients.Count(n => n.Present == false && n.BilledDate != null) + ")";
                    tempProblem.Active = 0;
                    problem.Add(tempProblem);
                    tempProblem = new Problem();
                }
                if (client.Workdays_Clients.Count(n => n.Present == true && n.BilledDate == null) > 0)
                {
                    tempProblem.Name = "Billed";
                    tempProblem.Description = "There are therapies not billed ("+ client.Workdays_Clients.Count(n => n.Present == true && n.BilledDate == null) + ")";
                    tempProblem.Active = 0;
                    problem.Add(tempProblem);
                    tempProblem = new Problem();
                }
            }
            else
            {
                if (client.Workdays_Clients.Count(n => n.BilledDate != null && n.PaymentDate == null) > 0)
                {
                    tempProblem.Name = "Billed";
                    tempProblem.Description = "There is billed therapies without paying ";
                    tempProblem.Active = 1;
                    problem.Add(tempProblem);
                    tempProblem = new Problem();
                }
                else
                {
                    tempProblem.Name = "Billed";
                    tempProblem.Description = "All therapies are billed and paid";
                    tempProblem.Active = 2;
                    problem.Add(tempProblem);
                    tempProblem = new Problem();
                }
            }

            return View(problem);
        }

        [Authorize(Roles = "Manager, Supervisor, Frontdesk")]
        public void DeleteHealthInsuranceTemp(ClientEntity client = null)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.UserName == User.Identity.Name);

            List<HealthInsuranceTempEntity> list_to_delete = new List<HealthInsuranceTempEntity>();

            //delete all HealthInsuranceTemp by UserName
            if (client == null)
            {
                list_to_delete = _context.HealthInsuranceTemp
                                         .Where(d => d.UserName == user_logged.UserName
                                            && d.IdClient == 0)
                                         .ToList();
            }
            else
            {
                list_to_delete = _context.HealthInsuranceTemp
                                         .Where(d => d.UserName == user_logged.UserName
                                            && d.IdClient == client.Id)
                                         .ToList();
            }
            
            _context.HealthInsuranceTemp.RemoveRange(list_to_delete);
            _context.SaveChanges();
        }

        [Authorize(Roles = "Manager, Supervisor, Frontdesk")]
        public void SetHealthInsuranceTemp(ClientEntity client)
        {
            IEnumerable<Client_HealthInsurance> clientsHealthinsurance = _context.Clients_HealthInsurances
                                                                                 .Include(cd => cd.HealthInsurance)
                                                                                 .Where(cd => cd.Client == client).ToList();
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                      .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (clientsHealthinsurance.Count() > 0)
            {
                HealthInsuranceTempEntity healthInsuranceTemp;
                foreach (var item in clientsHealthinsurance)
                {
                    healthInsuranceTemp = new HealthInsuranceTempEntity
                    {
                        Active = item.Active,
                        ApprovedDate = item.ApprovedDate,
                        DurationTime = item.DurationTime,
                        MemberId = item.MemberId,
                        Units = item.Units,
                        Name = item.HealthInsurance.Name,
                        UserName = user_logged.UserName,
                        IdClient = client.Id,
                        AuthorizationNumber = item.AuthorizationNumber,
                        Agency = item.Agency,
                        ExpiredDate = item.ExpiredDate,
                        EffectiveDate = item.EffectiveDate,
                        EndCoverageDate = item.EndCoverageDate,
                        InsuranceType = item.InsuranceType,
                        InsurancePlan = item.InsurancePlan,                       
                        InsuranceCoverage = item.InsuranceCoverage

                    };
                    _context.Add(healthInsuranceTemp);
                }
                _context.SaveChanges();
            }
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public async Task<IActionResult> DeleteHealthInsurance_Temp(int? id)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            HealthInsuranceTempEntity healthInsurance = await _context.HealthInsuranceTemp.FirstOrDefaultAsync(d => d.Id == id);
            if (healthInsurance == null)
            {
                return RedirectToAction("Home/Error404");
            }

            _context.HealthInsuranceTemp.Remove(healthInsurance);
            await _context.SaveChangesAsync();

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHealthInsurance", _context.HealthInsuranceTemp.Where(d => d.UserName == user_logged.UserName && d.IdClient == healthInsurance.IdClient).ToList()) });
        }

        public async Task<IActionResult> AddHealthInsuranceClient(int idClient = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (idClient != 0)
            {
                ClientEntity client = await _context.Clients
                                                    .FirstOrDefaultAsync(u => u.Id == idClient);

                 HealthInsuranceTempViewModel entity = new HealthInsuranceTempViewModel()
                {
                    ApprovedDate = DateTime.Now,
                    DurationTime = 12,
                    IdhealthInsurance = 0,
                    HealthInsurance = _combosHelper.GetComboActiveInsurancesByClinic(user_logged.Clinic.Id),
                    ClientName = client.Name,
                    MemberId = string.Empty,
                    Units = 0,
                    IdClient = idClient,
                    AuthorizationNumber = "Not Need",
                    IdAgencyService = 0,
                    AgencyServices = _combosHelper.GetComboServiceAgency(),
                    ExpiredDate = DateTime.Today.AddMonths(3),
                    EffectiveDate = DateTime.Today,
                    EndCoverageDate = DateTime.Today.AddMonths(12),
                    IdInsuranceType = 0,
                    InsuranceTypes = _combosHelper.GetComboInsuranceType(),
                    IdInsurancePlanType = 0,
                    InsurancePlanTypes = _combosHelper.GetComboInsurancePlanType(),
                    IdInsuranceCoverageType = 0,
                    InsuranceCoverageTypes = _combosHelper.GetComboInsuranceCoverageType()
                 };
                return View(entity);
            }
            else
            {
                HealthInsuranceTempViewModel entity = new HealthInsuranceTempViewModel()
                {
                    ApprovedDate = DateTime.Now,
                    DurationTime = 12,
                    IdhealthInsurance = 0,
                    HealthInsurance = _combosHelper.GetComboActiveInsurancesByClinic(user_logged.Clinic.Id),
                    ClientName = string.Empty,
                    MemberId = string.Empty,
                    Units = 0,
                    IdClient = idClient,
                    AuthorizationNumber = string.Empty,
                    IdAgencyService = 0,
                    AgencyServices = _combosHelper.GetComboServiceAgency(),
                    ExpiredDate = DateTime.Today.AddMonths(3),
                    EffectiveDate = DateTime.Today,
                    EndCoverageDate = DateTime.Today.AddMonths(12),
                    IdInsuranceType = 0,
                    InsuranceTypes = _combosHelper.GetComboInsuranceType(),
                    IdInsurancePlanType = 0,
                    InsurancePlanTypes = _combosHelper.GetComboInsurancePlanType(),
                    IdInsuranceCoverageType = 0,
                    InsuranceCoverageTypes = _combosHelper.GetComboInsuranceCoverageType()
                };
                return View(entity);
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHealthInsuranceClient(int id, HealthInsuranceTempViewModel HealthInsuranceModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                if (id == 0)
                {
                    List<HealthInsuranceTempEntity> healthInsuranceTempList = await _context.HealthInsuranceTemp
                                                                                            .Where(n => n.UserName == user_logged.UserName
                                                                                                && n.IdClient == HealthInsuranceModel.IdClient)
                                                                                            .ToListAsync();

                    foreach (var item in healthInsuranceTempList)
                    {
                        if (item.Agency == ServiceAgencyUtils.GetServiceAgencyByIndex(HealthInsuranceModel.IdAgencyService))
                        {
                            item.Active = false;
                            _context.Update(item);
                        }
                        else
                        {
                            _context.Update(item);
                        }
                    }

                    HealthInsuranceEntity healthInsurance = await _context.HealthInsurances.FirstOrDefaultAsync(n => n.Id == HealthInsuranceModel.IdhealthInsurance);
                    HealthInsuranceTempEntity healthInsuranceTemp = new HealthInsuranceTempEntity
                    {
                        Id = 0,
                        UserName = user_logged.UserName,
                        ApprovedDate = HealthInsuranceModel.ApprovedDate,
                        Active = true,
                        DurationTime = HealthInsuranceModel.DurationTime,
                        MemberId = HealthInsuranceModel.MemberId,
                        Units = HealthInsuranceModel.Units,
                        Name = healthInsurance.Name,
                        IdClient = HealthInsuranceModel.IdClient,
                        AuthorizationNumber = HealthInsuranceModel.AuthorizationNumber,
                        Agency = ServiceAgencyUtils.GetServiceAgencyByIndex(HealthInsuranceModel.IdAgencyService),
                        ExpiredDate = HealthInsuranceModel.ExpiredDate,
                        EffectiveDate = HealthInsuranceModel.EffectiveDate,
                        EndCoverageDate = HealthInsuranceModel.EndCoverageDate,
                        InsuranceType = InsuranceUtils.GetInsuranceTypeByIndex(HealthInsuranceModel.IdInsuranceType),
                        InsurancePlan = InsurancePlanUtils.GetInsurancePlanTypeByIndex(HealthInsuranceModel.IdInsurancePlanType),
                        InsuranceCoverage = InsuranceCoverageUtils.GetInsuranceCoverageTypeByIndex(HealthInsuranceModel.IdInsuranceCoverageType)
                    };
                    _context.Add(healthInsuranceTemp);
                    await _context.SaveChangesAsync();

                    List<HealthInsuranceTempEntity> list = await _context.HealthInsuranceTemp.Where(n => n.IdClient == HealthInsuranceModel.IdClient && n.UserName == user_logged.UserName)
                                                                         .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHealthInsurance", list )});

                }
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHealthInsurance", _context.HealthInsuranceTemp.Where(m => m.IdClient == id).ToList()) });
            }

            HealthInsuranceTempViewModel model = new HealthInsuranceTempViewModel
            {
                ApprovedDate = DateTime.Now,
                DurationTime = 12,
                IdhealthInsurance = 0,
                HealthInsurance = _combosHelper.GetComboActiveInsurancesByClinic(user_logged.Clinic.Id),
                ClientName = HealthInsuranceModel.Name,
                MemberId = "",
                Units = 0,
                IdClient = HealthInsuranceModel.IdClient,
                AuthorizationNumber = HealthInsuranceModel.AuthorizationNumber,
                ExpiredDate = DateTime.Today.AddMonths(3),
                EffectiveDate = HealthInsuranceModel.EffectiveDate,
                EndCoverageDate = HealthInsuranceModel.EndCoverageDate,
                IdInsuranceType = 0,
                InsuranceTypes = _combosHelper.GetComboInsuranceType(),
                IdInsurancePlanType = 0,
                InsurancePlanTypes = _combosHelper.GetComboInsurancePlanType(),
                IdInsuranceCoverageType = 0,
                InsuranceCoverageTypes = _combosHelper.GetComboInsuranceCoverageType()
            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddHealthInsuranceClient", model) });
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Frontdesk")]
        public IActionResult AuditClientNotUsed()
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic || !user_logged.Clinic.Setting.MHProblems)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<AuditClientNotUsed> auditClient_List = new List<AuditClientNotUsed>();
            AuditClientNotUsed auditClient = new AuditClientNotUsed();

            List<ClientEntity> client_List = _context.Clients
                                                     .Include(m => m.MTPs)
                                                     .Include(m => m.Workdays_Clients)
                                                     .Where(n => (n.Clinic.Id == user_logged.Clinic.Id 
                                                        && n.Workdays_Clients.Count() == 0 
                                                        && n.MTPs.Count() > 0
                                                        && n.Status == StatusType.Close))
                                                     .ToList();
            
            foreach (var item in client_List)
            {
                auditClient.Name = item.Name;
                auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                auditClient.Description = "The client have not Notes";
                auditClient.Active = 0;

                auditClient_List.Add(auditClient);
                auditClient = new AuditClientNotUsed();

            }

            //Clients without PCP, PSY, EC

            List<ClientEntity> client_List1 = _context.Clients
                                                      .Include(m => m.Doctor)
                                                      .Include(m => m.Psychiatrist)
                                                      .Include(m => m.EmergencyContact)
                                                      .OrderBy(n => n.Name)                                         
                                                      .ToList();


            foreach (var item in client_List1)
            {
                if (item.Doctor == null)
                {
                    auditClient.Name = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "The client have not PCP";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditClientNotUsed();
                }

                if (item.Psychiatrist == null)
                {
                    auditClient.Name = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "The client have not PSY";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditClientNotUsed();
                }

                if (item.EmergencyContact == null)
                {
                    auditClient.Name = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "The client have not EC";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditClientNotUsed();
                }
            }


            List<ClientEntity> client_Diagnostics_List = _context.Clients
                                                                 .Include(m => m.Clients_Diagnostics)
                                                                 .Where(n => (n.Clinic.Id == user_logged.Clinic.Id))
                                                                 .OrderBy(n => n.Name)
                                                                 .ToList();

            foreach (var item in client_Diagnostics_List)
            {
                if (item.Clients_Diagnostics.Count() == 0)
                {
                    auditClient.Name = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "The client haven't a diagnostic code";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditClientNotUsed();
                }
                else
                {
                    if (item.Clients_Diagnostics.Where(n => n.Principal == true).Count() == 0)
                    {
                        auditClient.Name = item.Name;
                        auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                        auditClient.Description = "The client haven't a principal diagnostic code";
                        auditClient.Active = 1;

                        auditClient_List.Add(auditClient);
                        auditClient = new AuditClientNotUsed();
                    }
                }
                
            }

            return View(auditClient_List);
        }

        [Authorize(Roles = "Manager")]
        public IActionResult EditOnlyTypeFARS(int id = 0)
        {
            FarsFormEntity fars = _context.FarsForm.Include(n => n.Client).FirstOrDefault(n => n.Id == id);

            if (fars != null)
            {
                FarsFormViewModel model = new FarsFormViewModel()
                {
                    Id = id,
                    IdClient = fars.Client.Id,
                    IdType = Convert.ToInt32(fars.Type),
                    FarsType = _combosHelper.GetComboFARSType()
                };
                return View(model);
            }

            return View(null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditOnlyTypeFARS(FarsFormViewModel farsViewModel)
        {
            FarsFormEntity fars = await _context.FarsForm.FindAsync(farsViewModel.Id);
            if (fars != null)
            {
                fars.Type = FARSUtils.GetypeByIndex(farsViewModel.IdType);

                _context.Update(fars);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction("ClientHistory","Clients", new { idClient = farsViewModel .IdClient});
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public IActionResult EXCELallClient()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            List<ClientEntity> clients = new List<ClientEntity>();
            string date = "Date Report: " + DateTime.Today.ToLongDateString();

            clients = _context.Clients
                              .Include(w => w.Clients_Diagnostics)
                              .ThenInclude(w => w.Diagnostic)

                              .Include(w => w.Clients_HealthInsurances)
                              .ThenInclude(w => w.HealthInsurance)

                              //.Include(w => w.Workdays_Clients)
                              //.ThenInclude(w => w.Workday)

                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                              .OrderBy(n => n.Name)
                              .ToList();

            byte[] content = _exportExcelHelper.ExportAllClients(clients, date);

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ALL_CLIENTS.xlsx");
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<IActionResult> Signatures()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                        .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            
            return View(await _context.Clients

                                      .Include(c => c.Clinic)
                                      .Include(c => c.Clients_HealthInsurances)
                                        .ThenInclude(c => c.HealthInsurance)
                                      .Include(c => c.LegalGuardian)

                                      .AsSplitQuery()

                                      .Where(c => c.Clinic.Id == user_logged.Clinic.Id)
                                      .OrderBy(c => c.Name).ToListAsync());       
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<IActionResult> EditSignature(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ClientEntity clientEntity = await _context.Clients
                                                      .FirstOrDefaultAsync(c => c.Id == id);

            if (clientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);            

            ClientViewModel clientViewModel = await _converterHelper.ToClientViewModel(clientEntity, user_logged.Id);

            return View(clientViewModel);
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<JsonResult> SaveClientSignature(string id, string dataUrl)
        {
            string signPath = await _imageHelper.UploadSignatureAsync(dataUrl, "Clients"); 

            ClientEntity client = await _context.Clients
                                                .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(id));
            if (client != null)
            {
                client.SignPath = signPath;
                _context.Update(client);
                await _context.SaveChangesAsync();
            }

            return Json(new { redirectToUrl = Url.Action("Signatures", "Clients") });
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<IActionResult> Documentation()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                        .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            
            return View(await _context.Clients
                
                                      .Include(c => c.IndividualTherapyFacilitator)
                                      .Include(c => c.Clients_HealthInsurances)
                                          .ThenInclude(c => c.HealthInsurance)

                                      .AsSplitQuery()

                                      .Where(c => c.Clinic.Id == user_logged.Clinic.Id)
                                      .OrderBy(c => c.Name).ToListAsync());                     
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<IActionResult> DownloadDocumentation(int id)
        {
            ClientEntity client = await _context.Clients

                                                .Include(c => c.Clinic)

                                                .Include(c => c.EmergencyContact)                                                  

                                                .Include(c => c.LegalGuardian)                                                   

                                                .Include(c => c.IntakeScreening)
                                                .Include(c => c.IntakeConsentForTreatment)
                                                .Include(c => c.IntakeConsentForRelease)
                                                .Include(c => c.IntakeConsumerRights)
                                                .Include(c => c.IntakeAcknowledgementHipa)
                                                .Include(c => c.IntakeAccessToServices)
                                                .Include(c => c.IntakeOrientationChecklist)
                                                .Include(c => c.IntakeTransportation)
                                                .Include(c => c.IntakeConsentPhotograph)
                                                .Include(c => c.IntakeFeeAgreement)
                                                .Include(c => c.IntakeTuberculosis)

                                                .Include(c => c.IntakeMedicalHistory)

                                                .Include(c => c.Bio)
                                                .Include(c => c.Brief)

                                                .Include(c => c.FarsFormList)

                                                .Include(c => c.MTPs)
                                                    .ThenInclude(m => m.MtpReviewList)

                                                .Include(c => c.MTPs)
                                                    .ThenInclude(m => m.AdendumList)

                                                .Include(c => c.MTPs)
                                                    .ThenInclude(m => m.Goals)
                                                        .ThenInclude(g => g.Objetives)

                                                .Include(c => c.MTPs)
                                                    .ThenInclude(m => m.Supervisor)

                                                .Include(c => c.MTPs)
                                                    .ThenInclude(m => m.DocumentAssistant)

                                                .Include(c => c.DischargeList)

                                                .Include(c => c.IntakeMedicalHistory)

                                                .Include(c => c.MedicationList)

                                                .Include(c => c.List_BehavioralHistory)

                                                .Include(c => c.Clients_Diagnostics)
                                                    .ThenInclude(cd => cd.Diagnostic)

                                                .Include(c => c.Client_Referred)
                                                    .ThenInclude(cr => cr.Referred)

                                                .AsSplitQuery()

                                                .FirstOrDefaultAsync(c => (c.Id == id));                                                                                       
                                                                          
            if (client == null)
            {
                return RedirectToAction("Home/Error404");
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            //Intake
            if (ExistIntake(client))
            {
                if (client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")                
                   stream = _reportHelper.FloridaSocialHSIntakeReport(client.IntakeScreening);                              
                
                if (client.Clinic.Name == "DREAMS MENTAL HEALTH INC")                
                   stream = _reportHelper.DreamsMentalHealthIntakeReport(client.IntakeScreening);                   
                
                if (client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")                
                   stream = _reportHelper.CommunityHTCIntakeReport(client.IntakeScreening);

                if (client.Clinic.Name == "MEDICAL & REHAB OF HILLSBOROUGH INC")
                    stream = _reportHelper.MedicalRehabIntakeReport(client.IntakeScreening);

                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Intake.pdf"));
            }

            //Bio
            if (ExistBio(client))
            {
                if (client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")                
                    stream = _reportHelper.FloridaSocialHSBioReport(client.Bio);
                
                if (client.Clinic.Name == "DREAMS MENTAL HEALTH INC")                
                    stream = _reportHelper.DreamsMentalHealthBioReport(client.Bio);
                
                if (client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")                
                    stream = _reportHelper.CommunityHTCBioReport(client.Bio);

                if (client.Clinic.Name == "MEDICAL & REHAB OF HILLSBOROUGH INC")
                    stream = _reportHelper.MedicalRehabBioReport(client.Bio);

                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Bio-Psychosocial Assesssment.pdf"));
            }

            //Brief
            if (ExistBrief(client))
            {
                if (client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")                
                    stream = _reportHelper.FloridaSocialHSBriefReport(client.Brief);
                
                if (client.Clinic.Name == "DREAMS MENTAL HEALTH INC")                
                    stream = _reportHelper.DreamsMentalHealthBriefReport(client.Brief);
                
                if (client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")                
                    stream = _reportHelper.CommunityHTCBriefReport(client.Brief);

                if (client.Clinic.Name == "MEDICAL & REHAB OF HILLSBOROUGH INC")
                    stream = _reportHelper.MedicalRehabBriefReport(client.Brief);

                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Brief Behavioral Health Status Examination.pdf"));
            }

            //Medical History
            if (ExistMedicalHistory(client))
            {
                if (client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")                
                    stream = _reportHelper.FloridaSocialHSMedicalHistoryReport(client.IntakeMedicalHistory);
                
                if (client.Clinic.Name == "DREAMS MENTAL HEALTH INC")                
                    stream = _reportHelper.DreamsMentalHealthMedicalHistoryReport(client.IntakeMedicalHistory);
                
                if (client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")                
                    stream = _reportHelper.CommunityHTCMedicalHistoryReport(client.IntakeMedicalHistory);

                if (client.Clinic.Name == "MEDICAL & REHAB OF HILLSBOROUGH INC")
                    stream = _reportHelper.MedicalRehabMedicalHistoryReport(client.IntakeMedicalHistory);

                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Medical History.pdf"));
            }

            //Fars Form list
            if (ExistFars(client))
            {
                foreach (var fars in client.FarsFormList)
                {
                    if (fars.Status == FarsStatus.Approved)
                    {
                        if (client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")                        
                            stream = _reportHelper.FloridaSocialHSFarsReport(fars);
                        
                        if (client.Clinic.Name == "DREAMS MENTAL HEALTH INC")                        
                            stream = _reportHelper.DreamsMentalHealthFarsReport(fars);
                        
                        if (client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")                        
                            stream = _reportHelper.CommunityHTCFarsReport(fars);

                        if (client.Clinic.Name == "MEDICAL & REHAB OF HILLSBOROUGH INC")
                            stream = _reportHelper.MedicalRehabFarsReport(fars);

                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Fars_{fars.Type.ToString()}_{fars.Id}.pdf"));
                    }                    
                }
            }

            //Discharge list
            if (ExistDischarges(client))
            {
                foreach (var discharge in client.DischargeList)
                {
                    if (discharge.Status == DischargeStatus.Approved)
                    {
                        if (client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
                        {
                            if (!discharge.JoinCommission)
                                stream = _reportHelper.FloridaSocialHSDischargeReport(discharge);
                            else
                                stream = _reportHelper.FloridaSocialHSDischargeJCReport(discharge);
                        }
                        if (client.Clinic.Name == "DREAMS MENTAL HEALTH INC")
                        {
                            if (!discharge.JoinCommission)
                                stream = _reportHelper.DreamsMentalHealthDischargeReport(discharge);
                            else
                                stream = _reportHelper.DreamsMentalHealthDischargeJCReport(discharge);
                        }
                        if (client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
                        {
                            if (!discharge.JoinCommission)
                                stream = _reportHelper.CommunityHTCDischargeReport(discharge);
                            else
                                stream = _reportHelper.CommunityHTCDischargeJCReport(discharge);
                        }
                        if (client.Clinic.Name == "MEDICAL & REHAB OF HILLSBOROUGH INC")
                        {
                            if (!discharge.JoinCommission)
                                stream = _reportHelper.MedicalRehabDischargeReport(discharge);
                            else
                                stream = _reportHelper.MedicalRehabDischargeJCReport(discharge);                            
                        }                            

                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Discharge_{discharge.TypeService.ToString()}_{discharge.Id}.pdf"));
                    }                    
                }
            }

            //MTP, Review, Addendum
            if (ExistMTP(client))
            {
                foreach (var mtp in client.MTPs)
                {
                    if (mtp.Status == MTPStatus.Approved)
                    {
                        if (client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
                        {
                            stream = _reportHelper.FloridaSocialHSMTPReport(mtp);
                            fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Mtp_{mtp.Id}.pdf"));
                            foreach (var review in mtp.MtpReviewList)
                            {
                                if (review.Status == AdendumStatus.Approved)
                                {
                                    stream = _reportHelper.FloridaSocialHSMTPReviewReport(review);
                                    fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"MtpReview_of_MTP{mtp.Id}_{review.Id}.pdf"));
                                }                                
                            }
                            foreach (var adendum in mtp.AdendumList)
                            {
                                if (adendum.Status == AdendumStatus.Approved)
                                {
                                    stream = _reportHelper.FloridaSocialHSAddendumReport(adendum);
                                    fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Addendum_of_MTP{mtp.Id}_{adendum.Id}.pdf"));
                                }                                
                            }
                        }
                        if (client.Clinic.Name == "DREAMS MENTAL HEALTH INC")
                        {
                            stream = _reportHelper.DreamsMentalHealthMTPReport(mtp);
                            fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Mtp_{mtp.Id}.pdf"));
                            foreach (var review in mtp.MtpReviewList)
                            {
                                if (review.Status == AdendumStatus.Approved)
                                {
                                    stream = _reportHelper.DreamsMentalHealthMTPReviewReport(review);
                                    fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"MtpReview_of_MTP{mtp.Id}_{review.Id}.pdf"));
                                }                                
                            }
                            foreach (var adendum in mtp.AdendumList)
                            {
                                if (adendum.Status == AdendumStatus.Approved)
                                {
                                    stream = _reportHelper.DreamsMentalHealthAddendumReport(adendum);
                                    fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Addendum_of_MTP{mtp.Id}_{adendum.Id}.pdf"));
                                }                                
                            }
                        }
                        if (client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
                        {
                            stream = _reportHelper.CommunityHTCMTPReport(mtp);
                            fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Mtp_{mtp.Id}.pdf"));
                            foreach (var review in mtp.MtpReviewList)
                            {
                                if (review.Status == AdendumStatus.Approved)
                                {
                                    stream = _reportHelper.CommunityHTCMTPReviewReport(review);
                                    fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"MtpReview_of_MTP{mtp.Id}_{review.Id}.pdf"));
                                }                                
                            }
                            foreach (var adendum in mtp.AdendumList)
                            {
                                if (adendum.Status == AdendumStatus.Approved)
                                {
                                    stream = _reportHelper.CommunityHTCAddendumReport(adendum);
                                    fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Addendum_of_MTP{mtp.Id}_{adendum.Id}.pdf"));
                                }                                
                            }
                        }
                        if (client.Clinic.Name == "MEDICAL & REHAB OF HILLSBOROUGH INC")
                        {
                            stream = _reportHelper.MedicalRehabMTPReport(mtp);
                            fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Mtp_{mtp.Id}.pdf"));
                            foreach (var review in mtp.MtpReviewList)
                            {
                                if (review.Status == AdendumStatus.Approved)
                                {
                                    stream = _reportHelper.MedicalRehabMTPReviewReport(review);
                                    fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"MtpReview_of_MTP{mtp.Id}_{review.Id}.pdf"));
                                }
                            }
                            foreach (var adendum in mtp.AdendumList)
                            {
                                if (adendum.Status == AdendumStatus.Approved)
                                {
                                    stream = _reportHelper.MedicalRehabAddendumReport(adendum);
                                    fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Addendum_of_MTP{mtp.Id}_{adendum.Id}.pdf"));
                                }
                            }
                        }
                    }                                     
                }
            }

            return File(_fileHelper.Zip(fileContentList), "application/zip", $"{client.Name}_Documents.zip");
        }

        [Authorize(Roles = "Manager, Frontdesk")]        
        public async Task<IActionResult> DownloadApprovedNotesSimultaneous(int id)
        {
            ClientEntity client = await _context.Clients
                                                .FirstOrDefaultAsync(c => c.Id == id);

            if (client == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Task<List<FileContentResult>> fileContent1Task = Schemas124PSRNotes(id);
            Task<List<FileContentResult>> fileContent2Task = Schema3PSRNotes(id);
            Task<List<FileContentResult>> fileContent3Task = Schema1GroupNotes(id);
            Task<List<FileContentResult>> fileContent4Task = Schema2GroupNotes(id);
            Task<List<FileContentResult>> fileContent5Task = Schema1IndNotes(id);

            await Task.WhenAll(fileContent1Task, fileContent2Task, fileContent3Task, fileContent4Task, fileContent5Task);
            var fileContent1 = await fileContent1Task;
            var fileContent2 = await fileContent2Task;
            var fileContent3 = await fileContent3Task;
            var fileContent4 = await fileContent4Task;
            var fileContent5 = await fileContent5Task;

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            fileContentList.AddRange(fileContent1);
            fileContentList.AddRange(fileContent2);
            fileContentList.AddRange(fileContent3);
            fileContentList.AddRange(fileContent4);
            fileContentList.AddRange(fileContent5);

            return File(_fileHelper.Zip(fileContentList), "application/zip", $"{client.Name}_Notes.zip");
        }

        public async Task<IActionResult> DownloadDocumentationUploadedByUser(int id)
        {
            ClientEntity client = await _context.Clients

                                                .Include(c => c.Documents)                                       

                                                .FirstOrDefaultAsync(c => (c.Id == id));

            if (client == null)
            {
                return RedirectToAction("Home/Error404");
            }

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            string path;
            foreach (var document in client.Documents)
            {
                try
                {
                    path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(document.FileUrl)}");
                    fileContentList.Add(File(_fileHelper.FileToByteArray(path), _mimeType.GetMimeType(document.FileName), document.FileName));
                }
                finally { }
            }            
            
            return File(_fileHelper.Zip(fileContentList), "application/zip", $"{client.Name}_Documents uploaded by user.zip");
        }

        #region Utils
        private bool ExistIntake(ClientEntity client)
        {
            return (client.IntakeScreening != null && client.IntakeConsentForTreatment != null && client.IntakeConsentForRelease != null &&
                        client.IntakeConsumerRights != null && client.IntakeAcknowledgementHipa != null && client.IntakeAccessToServices != null &&
                            client.IntakeOrientationChecklist != null && client.IntakeTransportation != null && client.IntakeConsentPhotograph != null &&
                                client.IntakeFeeAgreement != null && client.IntakeTuberculosis != null);                                      
        }
        private bool ExistBio(ClientEntity client)
        {
            return ((client.Bio != null) && (client.Bio.Status == BioStatus.Approved));
        }
        private bool ExistBrief(ClientEntity client)
        {
            return ((client.Brief != null) && (client.Brief.Status == BioStatus.Approved));
        }
        private bool ExistMTP(ClientEntity client)
        {
            return (client.MTPs.Count > 0);
        }
        private bool ExistFars(ClientEntity client)
        {
            return (client.FarsFormList.Count() > 0);
        }
        private bool ExistDischarges(ClientEntity client)
        {
            return (client.DischargeList.Count() > 0);
        }
        private bool ExistMedicalHistory(ClientEntity client)
        {
            return (client.IntakeMedicalHistory != null);
        }
        private async Task<List<FileContentResult>> Schemas124PSRNotes(int idClient)
        {
            List<Workday_Client> workdayClientList = new List<Workday_Client>();
            //PSR schema 1, 2 y 4
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                workdayClientList = await db.Workdays_Clients

                                            .Include(wc => wc.Facilitator)

                                            .Include(wc => wc.Client)
                                                .ThenInclude(c => c.MTPs)
                                                    .ThenInclude(m => m.Goals)
                                                        .ThenInclude(g => g.Objetives)

                                            .Include(wc => wc.Client)
                                                .ThenInclude(c => c.Clients_Diagnostics)
                                                    .ThenInclude(cd => cd.Diagnostic)

                                            .Include(wc => wc.Note)
                                                .ThenInclude(n => n.Supervisor)
                                                    .ThenInclude(s => s.Clinic)

                                            .Include(wc => wc.Note)
                                                .ThenInclude(n => n.Notes_Activities)
                                                    .ThenInclude(na => na.Activity)
                                                        .ThenInclude(a => a.Theme)

                                            .Include(wc => wc.Note)
                                                .ThenInclude(n => n.Notes_Activities)
                                                    .ThenInclude(na => na.Objetive)
                                                        .ThenInclude(o => o.Goal)

                                            .Include(wc => wc.Workday)

                                            .AsSplitQuery()

                                            .Where(wc => (wc.Client.Id == idClient && (wc.Note != null && wc.Note.Status == NoteStatus.Approved)))
                                            .ToListAsync();
            }            

            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;

            foreach (var workdayClient in workdayClientList)
            {
                if (workdayClient.Note.Supervisor.Clinic.Name == "DAVILA")
                {
                    if (workdayClient.Note.Schema == SchemaType.Schema1)
                    {
                        fileContentList.Add(DavilaNoteReportFCRSchema1(workdayClient));
                    }
                    if (workdayClient.Note.Schema == SchemaType.Schema4)
                    {
                        stream = _reportHelper.DavilaNoteReportSchema4(workdayClient);
                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"PSR/PSR_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf"));
                    }
                }
                if (workdayClient.Note.Supervisor.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
                {
                    fileContentList.Add(FloridaSocialHSNoteReportFCRSchema2(workdayClient));
                }
            }

            return fileContentList;
        }
        private async Task<List<FileContentResult>> Schema3PSRNotes(int idClient)
        {
            List<Workday_Client> workdayClientList = new List<Workday_Client>();
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            using (DataContext db = new DataContext(options))
            {
                workdayClientList = await db.Workdays_Clients

                                            .Include(wc => wc.Facilitator)

                                            .Include(wc => wc.Client)
                                                .ThenInclude(c => c.Clients_Diagnostics)
                                                    .ThenInclude(cd => cd.Diagnostic)

                                            .Include(wc => wc.NoteP)
                                            .ThenInclude(n => n.Supervisor)
                                                .ThenInclude(s => s.Clinic)

                                            .Include(wc => wc.NoteP)
                                            .ThenInclude(n => n.NotesP_Activities)
                                                .ThenInclude(na => na.Activity)
                                                    .ThenInclude(a => a.Theme)

                                            .Include(wc => wc.NoteP)
                                            .ThenInclude(n => n.NotesP_Activities)
                                                .ThenInclude(na => na.Objetive)
                                                    .ThenInclude(o => o.Goal)

                                            .Include(wc => wc.Workday)
                                            .ThenInclude(w => w.Workdays_Activities_Facilitators)
                                                .ThenInclude(waf => waf.Facilitator)

                                            .AsSplitQuery()

                                            .Where(wc => (wc.Client.Id == idClient && (wc.NoteP != null && wc.NoteP.Status == NoteStatus.Approved)))
                                            .ToListAsync();
            }
            
            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;
            foreach (var workdayClient in workdayClientList)
            {
                if (workdayClient.NoteP.Supervisor.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
                {
                    if (workdayClient.NoteP.Schema == SchemaType.Schema3)
                    {
                        if (!workdayClient.SharedSession)
                            stream = _reportHelper.FloridaSocialHSNoteReportSchema3(workdayClient);
                        else
                            stream = _reportHelper.FloridaSocialHSNoteReportSchema3SS(workdayClient);
                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"PSR/PSR_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf"));
                    }
                }
                if (workdayClient.NoteP.Supervisor.Clinic.Name == "DREAMS MENTAL HEALTH INC")
                {
                    if (workdayClient.NoteP.Schema == SchemaType.Schema3)
                    {
                        if (!workdayClient.SharedSession)
                            stream = _reportHelper.DreamsMentalHealthNoteReportSchema3(workdayClient);
                        else
                            stream = _reportHelper.DreamsMentalHealthNoteReportSchema3SS(workdayClient);
                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"PSR/PSR_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf"));
                    }
                }
                if (workdayClient.NoteP.Supervisor.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
                {
                    if (workdayClient.NoteP.Schema == SchemaType.Schema3)
                    {
                        if (!workdayClient.SharedSession)
                            stream = _reportHelper.CommunityHTCNoteReportSchema3(workdayClient);
                        else
                            stream = _reportHelper.CommunityHTCNoteReportSchema3SS(workdayClient);
                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"PSR/PSR_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf"));
                    }
                }
                if (workdayClient.NoteP.Supervisor.Clinic.Name == "MEDICAL & REHAB OF HILLSBOROUGH INC")
                {
                    if (workdayClient.NoteP.Schema == SchemaType.Schema3)
                    {
                        if (!workdayClient.SharedSession)
                            stream = _reportHelper.MedicalRehabNoteReportSchema3(workdayClient);
                        else
                            stream = _reportHelper.MedicalRehabNoteReportSchema3SS(workdayClient);
                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"PSR/PSR_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf"));
                    }
                }
            }
            return fileContentList;
        }
        private async Task<List<FileContentResult>> Schema1GroupNotes(int idClient)
        {
            List<Workday_Client> workdayClientList = new List<Workday_Client>();
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            //Group schema 1
            using (DataContext db = new DataContext(options))
            {
                workdayClientList = await db.Workdays_Clients

                                            .Include(wc => wc.Facilitator)

                                            .Include(wc => wc.Client)
                                                .ThenInclude(c => c.MTPs)
                                                    .ThenInclude(m => m.Goals)
                                                        .ThenInclude(g => g.Objetives)

                                            .Include(wc => wc.GroupNote)
                                                .ThenInclude(n => n.Supervisor)
                                                    .ThenInclude(s => s.Clinic)

                                            .Include(wc => wc.GroupNote)
                                                .ThenInclude(n => n.GroupNotes_Activities)
                                                    .ThenInclude(na => na.Activity)
                                                        .ThenInclude(a => a.Theme)

                                            .Include(wc => wc.GroupNote)
                                                .ThenInclude(n => n.GroupNotes_Activities)
                                                    .ThenInclude(na => na.Objetive)
                                                        .ThenInclude(o => o.Goal)

                                            .Include(wc => wc.Workday)

                                            .AsSplitQuery()

                                            .Where(wc => (wc.Client.Id == idClient && (wc.GroupNote != null && wc.GroupNote.Status == NoteStatus.Approved)))
                                            .ToListAsync();
            }
            
            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;
            foreach (var workdayClient in workdayClientList)
            {
                if (workdayClient.GroupNote.Supervisor.Clinic.Name == "DAVILA")
                    stream = _reportHelper.DavilaGroupNoteReportSchema1(workdayClient);

                if (workdayClient.GroupNote.Supervisor.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
                    stream = _reportHelper.FloridaSocialHSGroupNoteReportSchema1(workdayClient);

                if (workdayClient.GroupNote.Supervisor.Clinic.Name == "DREAMS MENTAL HEALTH INC")
                    stream = _reportHelper.DreamsMentalHealthGroupNoteReportSchema1(workdayClient);

                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Group/Group_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf"));
            }
            return fileContentList;
        }
        private async Task<List<FileContentResult>> Schema2GroupNotes(int idClient)
        {
            List<Workday_Client> workdayClientList = new List<Workday_Client>();
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            //Group schema 2
            using (DataContext db = new DataContext(options))
            {
                workdayClientList = await db.Workdays_Clients

                                            .Include(wc => wc.Facilitator)

                                            .Include(wc => wc.Client)
                                            .ThenInclude(c => c.MTPs)
                                                .ThenInclude(m => m.Goals)
                                                    .ThenInclude(g => g.Objetives)

                                            .Include(wc => wc.GroupNote2)
                                            .ThenInclude(n => n.Supervisor)
                                                .ThenInclude(s => s.Clinic)

                                            .Include(wc => wc.GroupNote2)
                                            .ThenInclude(n => n.GroupNotes2_Activities)
                                                .ThenInclude(na => na.Activity)
                                                    .ThenInclude(a => a.Theme)

                                            .Include(wc => wc.GroupNote2)
                                            .ThenInclude(n => n.GroupNotes2_Activities)
                                                .ThenInclude(na => na.Objetive)
                                                    .ThenInclude(o => o.Goal)

                                            .Include(wc => wc.Workday)

                                            .Include(wc => wc.Schedule)

                                            .Include(wc => wc.Client)
                                            .ThenInclude(c => c.Clients_Diagnostics)
                                                .ThenInclude(cd => cd.Diagnostic)

                                            .AsSplitQuery()

                                            .Where(wc => (wc.Client.Id == idClient && (wc.GroupNote2 != null && wc.GroupNote2.Status == NoteStatus.Approved)))
                                            .ToListAsync();
            }
            
            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;
            foreach (var workdayClient in workdayClientList)
            {
                if (workdayClient.GroupNote2.Supervisor.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
                    stream = _reportHelper.FloridaSocialHSGroupNoteReportSchema3(workdayClient);

                if (workdayClient.GroupNote2.Supervisor.Clinic.Name == "DREAMS MENTAL HEALTH INC")
                    stream = _reportHelper.DreamsMentalHealthGroupNoteReportSchema3(workdayClient);

                if (workdayClient.GroupNote2.Supervisor.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
                    stream = _reportHelper.CommunityHTCGroupNoteReportSchema3(workdayClient);

                if (workdayClient.GroupNote2.Supervisor.Clinic.Name == "MEDICAL & REHAB OF HILLSBOROUGH INC")
                    stream = _reportHelper.MedicalRehabGroupNoteReportSchema3(workdayClient);

                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Group/Group_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf"));
            }
            return fileContentList;
        }
        private async Task<List<FileContentResult>> Schema1IndNotes(int idClient)
        {
            List<Workday_Client> workdayClientList = new List<Workday_Client>();
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            //Individual schema 1
            using (DataContext db = new DataContext(options))
            {
                workdayClientList = await db.Workdays_Clients

                                            .Include(wc => wc.Facilitator)

                                            .Include(wc => wc.Client)
                                            .ThenInclude(c => c.MTPs)
                                                .ThenInclude(m => m.Goals)
                                                    .ThenInclude(g => g.Objetives)

                                            .Include(wc => wc.Client)
                                            .ThenInclude(c => c.Clients_Diagnostics)
                                                .ThenInclude(cd => cd.Diagnostic)

                                            .Include(wc => wc.IndividualNote)
                                            .ThenInclude(n => n.Supervisor)
                                                .ThenInclude(s => s.Clinic)

                                            .Include(wc => wc.IndividualNote)
                                            .ThenInclude(n => n.Objective)

                                            .Include(wc => wc.Workday)

                                            .AsSplitQuery()

                                            .Where(wc => (wc.Client.Id == idClient && (wc.IndividualNote != null && wc.IndividualNote.Status == NoteStatus.Approved)))
                                            .ToListAsync();
            }
            
            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;
            foreach (var workdayClient in workdayClientList)
            {
                if (workdayClient.IndividualNote.Supervisor.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
                    stream = _reportHelper.FloridaSocialHSIndNoteReportSchema1(workdayClient);

                if (workdayClient.IndividualNote.Supervisor.Clinic.Name == "DREAMS MENTAL HEALTH INC")
                    stream = _reportHelper.DreamsMentalHealthIndNoteReportSchema1(workdayClient);

                if (workdayClient.IndividualNote.Supervisor.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
                    stream = _reportHelper.CommunityHTCIndNoteReportSchema1(workdayClient);

                if (workdayClient.IndividualNote.Supervisor.Clinic.Name == "MEDICAL & REHAB OF HILLSBOROUGH INC")
                    stream = _reportHelper.MedicalRehabIndNoteReportSchema1(workdayClient);

                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Individual/Ind_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf"));
            }
            return fileContentList;
        }
        #endregion

        #region Davila RDLC reports
        private IActionResult DavilaNoteReportSchema1(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteDAVILA0");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal = string.Empty;
            var goal_text = string.Empty;
            var num_of_obj = string.Empty;
            var obj_text = string.Empty;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                        goal_text = item.Objetive.Goal.Name;
                        num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                        obj_text = item.Objetive.Description;
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();
            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("num_of_goal", num_of_goal);
            parameters.Add("goal_text", goal_text);
            parameters.Add("num_of_obj", num_of_obj);
            parameters.Add("obj_text", obj_text);
            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf");
        }
        private FileContentResult DavilaNoteReportFCRSchema1(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteDAVILA0");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal = string.Empty;
            var goal_text = string.Empty;
            var num_of_obj = string.Empty;
            var obj_text = string.Empty;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                        goal_text = item.Objetive.Goal.Name;
                        num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                        obj_text = item.Objetive.Description;
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();
            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("num_of_goal", num_of_goal);
            parameters.Add("goal_text", goal_text);
            parameters.Add("num_of_obj", num_of_obj);
            parameters.Add("obj_text", obj_text);
            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf", $"PSR/PSR_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf");
        }
        #endregion

        #region Florida Social Health Solution RDLC reports        
        private IActionResult FloridaSocialHSNoteReportSchema2(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteFloridaSocialHS1");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal1 = string.Empty;
            var goal_text1 = string.Empty;
            bool goal1 = false;
            var num_of_goal2 = string.Empty;
            var goal_text2 = string.Empty;
            bool goal2 = false;
            var num_of_goal3 = string.Empty;
            var goal_text3 = string.Empty;
            bool goal3 = false;
            var num_of_goal4 = string.Empty;
            var goal_text4 = string.Empty;
            bool goal4 = false;
            var num_of_goal5 = string.Empty;
            var goal_text5 = string.Empty;
            bool goal5 = false;
            var goal_obj_activity1 = string.Empty;
            var goal_obj_activity2 = string.Empty;
            var goal_obj_activity3 = string.Empty;
            var goal_obj_activity4 = string.Empty;

            MTPEntity mtp;
            if (workdayClient.Note.MTPId == null)   //la nota no tiene mtp relacionado, entonces se usa el primero que esté
                mtp = workdayClient.Client.MTPs.FirstOrDefault();
            else                                    //la nota tiene mtp relacionado    
                mtp = _context.MTPs.FirstOrDefault(m => m.Id == workdayClient.Note.MTPId);

            foreach (GoalEntity item in mtp.Goals.OrderBy(g => g.Number))
            {
                if (i == 0)
                {
                    num_of_goal1 = $"GOAL #{item.Number}:";
                    goal_text1 = item.Name;
                }
                if (i == 1)
                {
                    num_of_goal2 = $"GOAL #{item.Number}:";
                    goal_text2 = item.Name;
                }
                if (i == 2)
                {
                    num_of_goal3 = $"GOAL #{item.Number}:";
                    goal_text3 = item.Name;
                }
                if (i == 3)
                {
                    num_of_goal4 = $"GOAL #{item.Number}:";
                    goal_text4 = item.Name;
                }
                if (i == 4)
                {
                    num_of_goal5 = $"GOAL #{item.Number}:";
                    goal_text5 = item.Name;
                }
                i = ++i;
            }

            i = 0;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity1 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity2 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity3 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity4 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("num_of_goal1", num_of_goal1);
            parameters.Add("goal_text1", goal_text1);
            parameters.Add("goal1", goal1.ToString());
            parameters.Add("goal_obj_activity1", goal_obj_activity1);
            parameters.Add("num_of_goal2", num_of_goal2);
            parameters.Add("goal_text2", goal_text2);
            parameters.Add("goal2", goal2.ToString());
            parameters.Add("goal_obj_activity2", goal_obj_activity2);
            parameters.Add("num_of_goal3", num_of_goal3);
            parameters.Add("goal_text3", goal_text3);
            parameters.Add("goal3", goal3.ToString());
            parameters.Add("goal_obj_activity3", goal_obj_activity3);
            parameters.Add("num_of_goal4", num_of_goal4);
            parameters.Add("goal_text4", goal_text4);
            parameters.Add("goal4", goal4.ToString());
            parameters.Add("goal_obj_activity4", goal_obj_activity4);
            parameters.Add("num_of_goal5", num_of_goal5);
            parameters.Add("goal_text5", goal_text5);
            parameters.Add("goal5", goal5.ToString());
            parameters.Add("setting", setting);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf");
        }
        private FileContentResult FloridaSocialHSNoteReportFCRSchema2(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteFloridaSocialHS1");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal1 = string.Empty;
            var goal_text1 = string.Empty;
            bool goal1 = false;
            var num_of_goal2 = string.Empty;
            var goal_text2 = string.Empty;
            bool goal2 = false;
            var num_of_goal3 = string.Empty;
            var goal_text3 = string.Empty;
            bool goal3 = false;
            var num_of_goal4 = string.Empty;
            var goal_text4 = string.Empty;
            bool goal4 = false;
            var num_of_goal5 = string.Empty;
            var goal_text5 = string.Empty;
            bool goal5 = false;
            var goal_obj_activity1 = string.Empty;
            var goal_obj_activity2 = string.Empty;
            var goal_obj_activity3 = string.Empty;
            var goal_obj_activity4 = string.Empty;

            MTPEntity mtp;
            if (workdayClient.Note.MTPId == null)   //la nota no tiene mtp relacionado, entonces se usa el primero que esté
                mtp = workdayClient.Client.MTPs.FirstOrDefault();
            else                                    //la nota tiene mtp relacionado    
                mtp = _context.MTPs.FirstOrDefault(m => m.Id == workdayClient.Note.MTPId);

            foreach (GoalEntity item in mtp.Goals.OrderBy(g => g.Number))
            {
                if (i == 0)
                {
                    num_of_goal1 = $"GOAL #{item.Number}:";
                    goal_text1 = item.Name;
                }
                if (i == 1)
                {
                    num_of_goal2 = $"GOAL #{item.Number}:";
                    goal_text2 = item.Name;
                }
                if (i == 2)
                {
                    num_of_goal3 = $"GOAL #{item.Number}:";
                    goal_text3 = item.Name;
                }
                if (i == 3)
                {
                    num_of_goal4 = $"GOAL #{item.Number}:";
                    goal_text4 = item.Name;
                }
                if (i == 4)
                {
                    num_of_goal5 = $"GOAL #{item.Number}:";
                    goal_text5 = item.Name;
                }
                i = ++i;
            }

            i = 0;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity1 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity2 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity3 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity4 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("num_of_goal1", num_of_goal1);
            parameters.Add("goal_text1", goal_text1);
            parameters.Add("goal1", goal1.ToString());
            parameters.Add("goal_obj_activity1", goal_obj_activity1);
            parameters.Add("num_of_goal2", num_of_goal2);
            parameters.Add("goal_text2", goal_text2);
            parameters.Add("goal2", goal2.ToString());
            parameters.Add("goal_obj_activity2", goal_obj_activity2);
            parameters.Add("num_of_goal3", num_of_goal3);
            parameters.Add("goal_text3", goal_text3);
            parameters.Add("goal3", goal3.ToString());
            parameters.Add("goal_obj_activity3", goal_obj_activity3);
            parameters.Add("num_of_goal4", num_of_goal4);
            parameters.Add("goal_text4", goal_text4);
            parameters.Add("goal4", goal4.ToString());
            parameters.Add("goal_obj_activity4", goal_obj_activity4);
            parameters.Add("num_of_goal5", num_of_goal5);
            parameters.Add("goal_text5", goal_text5);
            parameters.Add("goal5", goal5.ToString());
            parameters.Add("setting", setting);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf", $"PSR/PSR_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf");
        }
        #endregion

        public async Task<IActionResult> EditHealthInsuranceClient(int id = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (id != 0)
            {
                HealthInsuranceTempEntity healthInsuranceTempEntity = await _context.HealthInsuranceTemp
                                                                                    .FirstOrDefaultAsync(u => u.Id == id);

                HealthInsuranceTempViewModel entity = new HealthInsuranceTempViewModel()
                {
                    ApprovedDate = healthInsuranceTempEntity.ApprovedDate,
                    DurationTime = healthInsuranceTempEntity.DurationTime,
                    IdhealthInsurance = _context.HealthInsurances.FirstOrDefault(n => n.Name == healthInsuranceTempEntity.Name).Id,
                    HealthInsurance = _combosHelper.GetComboActiveInsurancesByClinic(user_logged.Clinic.Id),
                    ClientName = _context.Clients.FirstOrDefault(n => n.Id == healthInsuranceTempEntity.IdClient).Name,
                    MemberId = healthInsuranceTempEntity.MemberId,
                    Units = healthInsuranceTempEntity.Units,
                    IdClient = healthInsuranceTempEntity.IdClient,
                    AuthorizationNumber = healthInsuranceTempEntity.AuthorizationNumber,
                    Id = id,
                    Name = healthInsuranceTempEntity.Name,
                    Active = healthInsuranceTempEntity.Active,
                    UserName = user_logged.UserName,
                    IdAgencyService = (healthInsuranceTempEntity.Agency == ServiceAgency.CMH)? 0 : 1,
                    AgencyServices = _combosHelper.GetComboServiceAgency(),
                    ExpiredDate = healthInsuranceTempEntity.ExpiredDate,
                    EffectiveDate = healthInsuranceTempEntity.EffectiveDate,
                    EndCoverageDate = healthInsuranceTempEntity.EndCoverageDate,
                    IdInsuranceType = (healthInsuranceTempEntity.InsuranceType == InsuranceType.Medicaid)? 0 : (healthInsuranceTempEntity.InsuranceType == InsuranceType.Medicare) ? 1 : (healthInsuranceTempEntity.InsuranceType == InsuranceType.Comercial) ? 2 : 3,
                    InsuranceTypes = _combosHelper.GetComboInsuranceType(),
                    IdInsurancePlanType = (healthInsuranceTempEntity.InsurancePlan == InsurancePlanType.Full_Medicaid) ? 0 : (healthInsuranceTempEntity.InsurancePlan == InsurancePlanType.Medicare_Part_AB) ? 1 : 2,
                    InsurancePlanTypes = _combosHelper.GetComboInsurancePlanType(),
                    IdInsuranceCoverageType = (healthInsuranceTempEntity.InsuranceCoverage == InsuranceCoverageType.Full_Medicaid) ? 0 : (healthInsuranceTempEntity.InsuranceCoverage == InsuranceCoverageType.MMA_Capitated) ? 1 : (healthInsuranceTempEntity.InsuranceCoverage == InsuranceCoverageType.Dual_Special_Needs_Plan) ? 2 : (healthInsuranceTempEntity.InsuranceCoverage == InsuranceCoverageType.Medicare_Special_Needs) ? 3 : (healthInsuranceTempEntity.InsuranceCoverage == InsuranceCoverageType.Medicare_Advantage_Plan) ? 4 : (healthInsuranceTempEntity.InsuranceCoverage == InsuranceCoverageType.HMO) ? 5 : (healthInsuranceTempEntity.InsuranceCoverage == InsuranceCoverageType.PPO) ? 6 : (healthInsuranceTempEntity.InsuranceCoverage == InsuranceCoverageType.EPO) ? 7 : 8,
                    InsuranceCoverageTypes = _combosHelper.GetComboInsuranceCoverageType()

                };
                return View(entity);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHealthInsuranceClient(int id, HealthInsuranceTempViewModel HealthInsuranceModel)
        {
            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                if (id != 0)
                {
                    
                    HealthInsuranceTempEntity healthInsuranceTemp = new HealthInsuranceTempEntity
                    {
                        Id = HealthInsuranceModel.Id,
                        UserName = HealthInsuranceModel.UserName,
                        ApprovedDate = HealthInsuranceModel.ApprovedDate,
                        Active = HealthInsuranceModel.Active,
                        DurationTime = HealthInsuranceModel.DurationTime,
                        MemberId = HealthInsuranceModel.MemberId,
                        Units = HealthInsuranceModel.Units,
                        Name = HealthInsuranceModel.Name,
                        IdClient = HealthInsuranceModel.IdClient,
                        AuthorizationNumber = HealthInsuranceModel.AuthorizationNumber,
                        Agency = ServiceAgencyUtils.GetServiceAgencyByIndex(HealthInsuranceModel.IdAgencyService),
                        ExpiredDate = HealthInsuranceModel.ExpiredDate,
                        EffectiveDate = HealthInsuranceModel.EffectiveDate,
                        EndCoverageDate = HealthInsuranceModel.EndCoverageDate,
                        InsuranceType = InsuranceUtils.GetInsuranceTypeByIndex(HealthInsuranceModel.IdInsuranceType),
                        InsurancePlan = InsurancePlanUtils.GetInsurancePlanTypeByIndex(HealthInsuranceModel.IdInsurancePlanType),
                        InsuranceCoverage = InsuranceCoverageUtils.GetInsuranceCoverageTypeByIndex(HealthInsuranceModel.IdInsuranceCoverageType)
                    };
                    _context.Update(healthInsuranceTemp);
                    await _context.SaveChangesAsync();

                    List<HealthInsuranceTempEntity> list = await _context.HealthInsuranceTemp
                                                                         .Where(n => n.IdClient == HealthInsuranceModel.IdClient
                                                                            && n.UserName == HealthInsuranceModel.UserName)
                                                                         .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHealthInsurance", list) });

                }
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHealthInsurance", _context.HealthInsuranceTemp.Where(m => m.IdClient == HealthInsuranceModel.IdClient && m.UserName == HealthInsuranceModel.UserName).ToList()) });
            }

            
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditHealthInsuranceClient", HealthInsuranceModel) });
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public IActionResult DeleteHealthInsuranceTempModal(int id = 0)
        {
            if (id > 0)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                DeleteViewModel model = new DeleteViewModel
                {
                    Id_Element = id,
                    Desciption = "Do you want to delete this record?"

                };
                return View(model);
            }
            else
            {
                //Edit
                //return View(new Client_DiagnosticViewModel());
                return null;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<IActionResult> DeleteHealthInsuranceTempModal(DeleteViewModel HealthInsuranceModel)
        {
            if (ModelState.IsValid)
            {
                HealthInsuranceTempEntity healthInsurance = await _context.HealthInsuranceTemp
                                                                          .FirstAsync(n => n.Id == HealthInsuranceModel.Id_Element);
                try
                {
                    _context.HealthInsuranceTemp.Remove(healthInsurance);
                    await _context.SaveChangesAsync();

                    List<HealthInsuranceTempEntity> list = await _context.HealthInsuranceTemp
                                                                        .Where(n => n.IdClient == healthInsurance.IdClient
                                                                                 && n.UserName == healthInsurance.UserName)
                                                                        .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHealthInsurance", list) });
                }
                catch (Exception)
                {
                    List<HealthInsuranceTempEntity> list = await _context.HealthInsuranceTemp
                                                                         .Where(n => n.IdClient == healthInsurance.IdClient
                                                                                  && n.UserName == healthInsurance.UserName)
                                                                         .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHealthInsurance", list) });
                   
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditHealthInsuranceClient", HealthInsuranceModel) });
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<IActionResult> AuthorizationClients(int idError = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }


            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk"))
            {
                List<ClientEntity> list = await _context.Clients
                                                        .Include(n => n.Clients_HealthInsurances)
                                                        .ThenInclude(n => n.HealthInsurance)
                                                        .Where(n => n.Clinic.Id == user_logged.Clinic.Id
                                                                 && n.Status == StatusType.Open
                                                                 && n.OnlyTCM == false)
                                                        .ToListAsync();
                List<AuthorizationViewModel> authorizations = new List<AuthorizationViewModel>();
                AuthorizationViewModel authorization = new AuthorizationViewModel();

                foreach (var item in list)
                {
                    if (item.Clients_HealthInsurances.Where(n => n.Agency == ServiceAgency.CMH).Count() == 0)
                    {
                        authorization.IdClientHealthInsurance = 0;
                        authorization.IdClient = item.Id;                       
                        authorization.TCMClientName = item.Name;
                        authorization.HealthInsurance = "Empty";
                        authorization.Status = item.Status;
                        authorization.DateOpen = item.AdmisionDate;
                        authorization.Agency = "MH";
                        authorization.Info = 0;

                        authorizations.Add(authorization);
                        authorization = new AuthorizationViewModel();
                    }
                    else
                    {
                        if (item.Clients_HealthInsurances.Where(n => n.Agency == ServiceAgency.CMH && n.Active == true).Count() > 0)
                        {
                            foreach (var item1 in item.Clients_HealthInsurances.Where(n => n.Agency == ServiceAgency.CMH && n.Active == true && n.HealthInsurance.NeedAuthorization == true))
                            {
                                if (item1.ExpiredDate.Date < DateTime.Today.Date)
                                {
                                    authorization.IdClientHealthInsurance = 0;
                                    authorization.IdClient = item.Id;
                                    authorization.TCMClientName = item.Name;
                                    authorization.HealthInsurance = item1.HealthInsurance.Name;
                                    authorization.Status = item.Status;
                                    authorization.DateOpen = item.AdmisionDate;
                                    authorization.Agency = "MH";
                                    authorization.Info = 0;
                                    authorization.ExpiratedDate = item1.ExpiredDate;
                                    authorization.EffectiveDate = item1.ApprovedDate;

                                    authorizations.Add(authorization);
                                    authorization = new AuthorizationViewModel();
                                }
                                else
                                {
                                    if (item1.ExpiredDate.Date <= DateTime.Today.Date.AddDays(30))
                                    {
                                        authorization.IdClientHealthInsurance = 0;
                                        authorization.IdClient = item.Id;                                        
                                        authorization.TCMClientName = item.Name;                                        
                                        authorization.HealthInsurance = item1.HealthInsurance.Name;
                                        authorization.Status = item.Status;
                                        authorization.DateOpen = item.AdmisionDate;
                                        authorization.Agency = "MH";
                                        authorization.Info = 1;
                                        authorization.ExpiratedDate = item1.ExpiredDate;
                                        authorization.EffectiveDate = item1.ApprovedDate;

                                        authorizations.Add(authorization);
                                        authorization = new AuthorizationViewModel();
                                    }
                                }
                            }
                        }
                        else
                        {
                            authorization.IdClientHealthInsurance = 0;
                            authorization.IdClient = item.Id;                            
                            authorization.TCMClientName = item.Name;
                            authorization.HealthInsurance = "Empty";
                            authorization.Status = item.Status;
                            authorization.DateOpen = item.AdmisionDate;
                            authorization.Agency = "MH";
                            authorization.Info = 0;

                            authorizations.Add(authorization);
                            authorization = new AuthorizationViewModel();
                        }

                    }

                }

                return View(authorizations);
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<IActionResult> BirthDayClients(int month = 0)
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                        .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            string monthName = string.Empty;
            if (month != 0)
            {
                monthName = (month == 1) ? "January" : (month == 2) ? "February" : (month == 3) ? "March" : (month == 4) ? "April" : (month == 5) ? "May" :
                                (month == 6) ? "June" : (month == 7) ? "July" : (month == 8) ? "August" : (month == 9) ? "September" : (month == 10) ? "October" :
                                    (month == 11) ? "November" : (month == 12) ? "December" : string.Empty;
            }
            else
            {
                monthName = (DateTime.Today.Month == 1) ? "January" : (DateTime.Today.Month == 2) ? "February" : (DateTime.Today.Month == 3) ? "March" : 
                                (DateTime.Today.Month == 4) ? "April" : (DateTime.Today.Month == 5) ? "May" : (DateTime.Today.Month == 6) ? "June" : 
                                    (DateTime.Today.Month == 7) ? "July" : (DateTime.Today.Month == 8) ? "August" : (DateTime.Today.Month == 9) ? "September" : 
                                        (DateTime.Today.Month == 10) ? "October" : (DateTime.Today.Month == 11) ? "November" : 
                                            (DateTime.Today.Month == 12) ? "December" : string.Empty;
            }
            
            ViewData["month"] = monthName;

            List<ClientEntity> clients = new List<ClientEntity>();
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity>();
            List<SupervisorEntity> supervisors = new List<SupervisorEntity>();
            List<CaseMannagerEntity> TCMs = new List<CaseMannagerEntity>();
            List<DocumentsAssistantEntity> documentAssistants = new List<DocumentsAssistantEntity>();
            List<TCMSupervisorEntity> supervisorTCMs = new List<TCMSupervisorEntity>();

            if (month == 0)
            {
                clients = await _context.Clients
                                        .Where(c => (c.DateOfBirth.Month == DateTime.Today.Month && c.Status == StatusType.Open))
                                        .ToListAsync();
                facilitators = await _context.Facilitators
                                             .Where(c => (c.DateOfBirth.Month == DateTime.Today.Month && c.Status == StatusType.Open))
                                             .ToListAsync();
                supervisors = await _context.Supervisors
                                            .Where(c => (c.DateOfBirth.Month == DateTime.Today.Month && c.Status == StatusType.Open))
                                            .ToListAsync();
                documentAssistants = await _context.DocumentsAssistant
                                                   .Where(c => (c.DateOfBirth.Month == DateTime.Today.Month && c.Status == StatusType.Open))
                                                   .ToListAsync();
                TCMs = await _context.CaseManagers
                                     .Where(c => (c.DateOfBirth.Month == DateTime.Today.Month && c.Status == StatusType.Open))
                                     .ToListAsync();
                supervisorTCMs = await _context.TCMSupervisors
                                               .Where(c => (c.DateOfBirth.Month == DateTime.Today.Month && c.Status == StatusType.Open))
                                               .ToListAsync();
            }
            else
            {
                clients = await _context.Clients
                                        .Where(c => (c.DateOfBirth.Month == month && c.Status == StatusType.Open))
                                        .ToListAsync();
                facilitators = await _context.Facilitators
                                            .Where(c => (c.DateOfBirth.Month == month && c.Status == StatusType.Open))
                                            .ToListAsync();
                supervisors = await _context.Supervisors
                                            .Where(c => (c.DateOfBirth.Month == month && c.Status == StatusType.Open))
                                            .ToListAsync();
                documentAssistants = await _context.DocumentsAssistant
                                                   .Where(c => (c.DateOfBirth.Month == month && c.Status == StatusType.Open))
                                                   .ToListAsync();
                TCMs = await _context.CaseManagers
                                     .Where(c => (c.DateOfBirth.Month == month && c.Status == StatusType.Open))
                                     .ToListAsync();
                supervisorTCMs = await _context.TCMSupervisors
                                               .Where(c => (c.DateOfBirth.Month == month && c.Status == StatusType.Open))
                                               .ToListAsync();
            }            

            List<BirthDayViewModel> salida = new List<BirthDayViewModel>();
            BirthDayViewModel temp = new BirthDayViewModel();
            
            // Client
            foreach (var item in clients)
            {
                temp.Name = item.Name;
                temp.BirthDay = item.DateOfBirth;
                temp.Arriving = item.AdmisionDate;
                temp.Program = item.Service.ToString();
                temp.Arrived = DateTime.Today.Subtract(item.AdmisionDate).Days;
                temp.Person = "Client";
                temp.Gender = item.Gender;
                temp.Code = item.Code;

                salida.Add(temp);
                temp = new BirthDayViewModel();
            }

            // facilitator
            foreach (var item in facilitators)
            {
                temp.Name = item.Name;
                temp.BirthDay = item.DateOfBirth;
                temp.Arriving = item.HiringDate;
                temp.Program = "MH";
                temp.Arrived = DateTime.Today.Subtract(item.HiringDate).Days;
                temp.Person = "Facilitator";
                temp.Gender = item.Gender;
                temp.Code = item.Codigo;

                salida.Add(temp);
                temp = new BirthDayViewModel();
            }

            // Supervisor
            foreach (var item in supervisors)
            {
                temp.Name = item.Name;
                temp.BirthDay = item.DateOfBirth;
                temp.Arriving = item.HiringDate;
                temp.Program = "MH";
                temp.Arrived = DateTime.Today.Subtract(item.HiringDate).Days;
                temp.Person = "Supervisor";
                temp.Gender = item.Gender;
                temp.Code = item.Code;

                salida.Add(temp);
                temp = new BirthDayViewModel();
            }

            // Document Assistant
            foreach (var item in documentAssistants)
            {
                temp.Name = item.Name;
                temp.BirthDay = item.DateOfBirth;
                temp.Arriving = item.HiringDate;
                temp.Program = "MH";
                temp.Arrived = DateTime.Today.Subtract(item.HiringDate).Days;
                temp.Person = "Doc_Assistant";
                temp.Gender = item.Gender;
                temp.Code = item.Code;

                salida.Add(temp);
                temp = new BirthDayViewModel();
            }

            // TCMs
            foreach (var item in TCMs)
            {
                temp.Name = item.Name;
                temp.BirthDay = item.DateOfBirth;
                temp.Arriving = item.HiringDate;
                temp.Program = "MH";
                temp.Arrived = DateTime.Today.Subtract(item.HiringDate).Days;
                temp.Person = "TCM";
                temp.Gender = item.Gender;
                temp.Code = item.ProviderNumber;

                salida.Add(temp);
                temp = new BirthDayViewModel();
            }

            // TCM Supervisor
            foreach (var item in supervisorTCMs)
            {
                temp.Name = item.Name;
                temp.BirthDay = item.DateOfBirth;
                temp.Arriving = item.HiringDate;
                temp.Program = "MH";
                temp.Arrived = DateTime.Today.Subtract(item.HiringDate).Days;
                temp.Person = "TCM Supervisor";
                temp.Gender = item.Gender;
                temp.Code = item.Code;

                salida.Add(temp);
                temp = new BirthDayViewModel();
            }

            return View(salida);
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> ActiveClients(int warning = 0)
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                        .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<ClientEntity> clients = new List<ClientEntity>();

            if (User.IsInRole("Manager") || User.IsInRole("Supervisor"))
            {
                clients = await _context.Clients
                                        .Include(n => n.Clients_HealthInsurances)
                                        .ThenInclude(n => n.HealthInsurance)
                                        .Include(n => n.IndividualTherapyFacilitator)
                                        .AsSplitQuery()
                                        .Where(c => c.Status == StatusType.Open
                                                 && c.Clinic.Id == user_logged.Clinic.Id)
                                        .ToListAsync();

            }
            
            if (User.IsInRole("Facilitator"))
            {
                FacilitatorEntity facilitator = await _context.Facilitators.FirstOrDefaultAsync(f => f.LinkedUser == user_logged.UserName);

                clients = await _context.Clients
                                        .Include(n => n.Clients_HealthInsurances)
                                        .ThenInclude(n => n.HealthInsurance)
                                        .Include(n => n.IndividualTherapyFacilitator)
                                        .AsSplitQuery()
                                        .Where(c => c.Status == StatusType.Open
                                                 && c.Clinic.Id == user_logged.Clinic.Id
                                                 && (c.Workdays_Clients.Where(m => m.Facilitator.Id == facilitator.Id).Count() > 0
                                                    || c.IndividualTherapyFacilitator.Id == facilitator.Id
                                                    || c.IdFacilitatorPSR == facilitator.Id
                                                    || c.IdFacilitatorGroup == facilitator.Id))
                                        .ToListAsync();

            }

            if (User.IsInRole("Documents_Assistant"))
            {
                clients = await _context.Clients
                                        .Include(n => n.Clients_HealthInsurances)
                                        .ThenInclude(n => n.HealthInsurance)
                                        .Include(n => n.IndividualTherapyFacilitator)
                                        .AsSplitQuery()
                                        .Where(c => c.Status == StatusType.Open
                                                 && c.Clinic.Id == user_logged.Clinic.Id)
                                        .ToListAsync();

            }

            List<ClientActivedViewModel> salida = new List<ClientActivedViewModel>();
            ClientActivedViewModel temp = new ClientActivedViewModel();
            DateTime date = new DateTime();
            List<ObjetiveEntity> objectives = new List<ObjetiveEntity>();
            ObjetiveEntity objective = new ObjetiveEntity();

            DateTime dateInd = new DateTime();
            List<ObjetiveEntity> objectivesInd = new List<ObjetiveEntity>();
            ObjetiveEntity objectiveInd = new ObjetiveEntity();

            foreach (var item in clients)
            {
                objectives = _context.Objetives
                                     .Include(n => n.Goal)
                                     .ThenInclude(n => n.Adendum)
                                     .Include(n => n.Goal)
                                     .ThenInclude(n => n.MTP)
                                     .AsSplitQuery()
                                     .Where(g => g.Compliment == false 
                                              && g.Goal.MTP.Client.Id == item.Id 
                                              && g.Goal.MTP.Active == true 
                                              && g.Goal.Compliment == false
                                              && g.Goal.Service == item.Service)
                                     .ToList();
                objectivesInd = _context.Objetives
                                        .Include(n => n.Goal)
                                        .ThenInclude(n => n.Adendum)
                                        .Include(n => n.Goal)
                                        .ThenInclude(n => n.MTP)
                                        .AsSplitQuery()
                                        .Where(g => g.Compliment == false
                                                 && g.Goal.MTP.Client.Id == item.Id
                                                 && g.Goal.MTP.Active == true
                                                 && g.Goal.Compliment == false
                                                 && g.Goal.Service == ServiceType.Individual)
                                     .ToList();
                if (objectives.Count() > 0)
                {
                    date = objectives.Max(n => n.DateResolved);

                    objective = objectives.FirstOrDefault(n => n.DateResolved == date);

                    temp.Days = (date - DateTime.Today).Days;
                    temp.Name = item.Name;
                    temp.Clients_HealthInsurances = item.Clients_HealthInsurances;
                    temp.Service = item.Service;
                    temp.IndividualTherapyFacilitator = item.IndividualTherapyFacilitator;
                    temp.AdmisionDate = item.AdmisionDate;
                    temp.Code = item.Code;
                    temp.Gender = item.Gender;
                    temp.MtpId = objective.Goal.MTP.Id;

                    if (objective.Goal.Adendum != null)
                    {
                        temp.DocumentType = "Addendum";
                    }
                    else
                    {
                        if (objective.IdMTPReview > 0)
                        {
                            temp.DocumentType = "MTPR";
                        }
                        else
                        {
                            temp.DocumentType = "MTP";
                        }
                    }

                    //---------para calcular dias de ind-------
                    if (objectivesInd != null && objectivesInd.Count() > 0)
                    {
                        dateInd = objectivesInd.Max(n => n.DateResolved);
                        objectiveInd = objectivesInd.FirstOrDefault(n => n.DateResolved == dateInd);
                        temp.DaysInd = (dateInd - DateTime.Today).Days;

                        if (objectiveInd.Goal.Adendum != null)
                        {
                            temp.DocumentTypeInd = "Addendum";
                        }
                        else
                        {
                            if (objectiveInd.IdMTPReview > 0)
                            {
                                temp.DocumentTypeInd = "MTPR";
                            }
                            else
                            {
                                temp.DocumentTypeInd = "MTP";
                            }
                        }
                    }
                    else
                    {
                        temp.DaysInd = -1000;
                        temp.DocumentTypeInd = "N_Doc";
                    }


                    if (warning == 1)
                    {
                        if ((temp.Days < 15 && temp.Days > 0) || (temp.DaysInd < 15 && temp.DaysInd > 0))
                        {
                            salida.Add(temp);
                        }
                       
                    }
                    else
                    {
                        if (warning == 2)
                        {
                            if ((temp.Days <= 0 && temp.Days != -1000) || (temp.DaysInd <= 0 && temp.DaysInd != -1000))
                            {
                                salida.Add(temp);
                            }
                        }
                        else
                        {
                            salida.Add(temp);
                        }
                    }
                    temp = new ClientActivedViewModel();
                    objectives = new List<ObjetiveEntity>();
                    objective = new ObjetiveEntity();
                    date = new DateTime();
                    objectivesInd = new List<ObjetiveEntity>();
                    objectiveInd = new ObjetiveEntity();
                    dateInd = new DateTime();
                }
               else
                {
                    if (warning == 0)
                    {
                        temp.Days = -1000;
                        temp.Name = item.Name;
                        temp.Clients_HealthInsurances = item.Clients_HealthInsurances;
                        temp.Service = item.Service;
                        temp.IndividualTherapyFacilitator = item.IndividualTherapyFacilitator;
                        temp.AdmisionDate = item.AdmisionDate;
                        temp.Code = item.Code;
                        temp.Gender = item.Gender;
                        temp.DocumentType = "N_Doc";
                        temp.MtpId = 0;
                        temp.DaysInd = -1000;
                        temp.DocumentTypeInd = "N_Doc";

                        salida.Add(temp);
                        temp = new ClientActivedViewModel();
                        objectives = new List<ObjetiveEntity>();
                        objective = new ObjetiveEntity();
                        date = new DateTime();
                        objectivesInd = new List<ObjetiveEntity>();
                        objectiveInd = new ObjetiveEntity();
                        dateInd = new DateTime();
                    }
                    
                }
            }

            return View(salida);
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<IActionResult> EditSignatureLegalGuardian(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            LegalGuardianEntity legalGuardian = await _context.LegalGuardians
                                                              .FirstOrDefaultAsync(c => c.Id == id);

            if (legalGuardian == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            LegalGuardianViewModel LegalViewModel = _converterHelper.ToLegalGuardianViewModel(legalGuardian);

            return View(LegalViewModel);
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<JsonResult> SaveSignatureLegalGuardian(string id, string dataUrl)
        {
            string signPath = await _imageHelper.UploadSignatureAsync(dataUrl, "LegalGuardian");

            LegalGuardianEntity legalGuardian = await _context.LegalGuardians
                                                              .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(id));
            if (legalGuardian != null)
            {
                legalGuardian.SignPath = signPath;
                _context.Update(legalGuardian);
                await _context.SaveChangesAsync();
            }

            return Json(new { redirectToUrl = Url.Action("Signatures", "Clients") });
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> ReferralAccept(int? id)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            if (User.IsInRole("Supervisor"))
            {
                ReferralFormEntity Referral = await _context.ReferralForms
                                                            .Include(g => g.Client)
                                                            .Include(g => g.Facilitator)
                                                            .Include(g => g.Supervisor)
                                                            .FirstOrDefaultAsync(g => g.Id == id
                                                                                   && g.Supervisor.LinkedUser == user_logged.UserName);
                if (Referral == null)
                {
                    return RedirectToAction("Home/Error404");
                }

                ReferralFormViewModel ReferralViewModel = _converterHelper.ToReferralViewModel(Referral);
                return View(ReferralViewModel);
            }
            if (User.IsInRole("Facilitator"))
            {
                ReferralFormEntity Referral = await _context.ReferralForms
                                                            .Include(g => g.Client)
                                                            .FirstOrDefaultAsync(g => g.Id == id
                                                                                   && g.Facilitator.LinkedUser == user_logged.UserName);
                if (Referral == null)
                {
                    return RedirectToAction("Home/Error404");
                }

                ReferralFormViewModel ReferralViewModel = _converterHelper.ToReferralViewModel(Referral);
                return View(ReferralViewModel);
            }
            if (User.IsInRole("Manager"))
            {
                ReferralFormEntity Referral = await _context.ReferralForms
                                                            .Include(g => g.Client)
                                                            .FirstOrDefaultAsync(g => g.Id == id);
                if (Referral == null)
                {
                    return RedirectToAction("Home/Error404");
                }

                ReferralFormViewModel ReferralViewModel = _converterHelper.ToReferralViewModel(Referral);
                return View(ReferralViewModel);
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [HttpPost]
        [Authorize(Roles = "Supervisor, Facilitator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReferralAccept(ReferralFormViewModel model)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            ReferralFormEntity Referral = _context.ReferralForms.FirstOrDefault(n => n.Id == model.Id);
            if (User.IsInRole("Facilitator"))
            {
                Referral.FacilitatorSign = true;
                Referral.CaseAcceptedFacilitator = model.CaseAcceptedFacilitator;
                _context.ReferralForms.Update(Referral);
            }
            if (User.IsInRole("Supervisor"))
            {
                Referral.SupervisorSign = true;
                Referral.CaseAcceptedSupervisor = model.CaseAcceptedSupervisor;
                _context.ReferralForms.Update(Referral);
            }
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("ReferralPending", "Clients");

            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.InnerException.Message);
            }

            return View(model);
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public IActionResult CreateReferralForm(int idClient = 0)
        {
            ClientEntity client = _context.Clients
                                          .Include(d => d.Clients_Diagnostics)
                                          .ThenInclude(d => d.Diagnostic)
                                          .Include(d => d.LegalGuardian)
                                          .Include(d => d.Client_Referred)
                                          .ThenInclude(d => d.Referred)
                                          .Include(d => d.Clients_HealthInsurances)
                                          .ThenInclude(d => d.HealthInsurance)

                                          .FirstOrDefault(n => n.Id == idClient);

            ReferralFormViewModel salida;

            Client_Diagnostic client_Diagnostic = new Client_Diagnostic();
            if (client.Clients_Diagnostics != null)
            {
                client_Diagnostic = client.Clients_Diagnostics
                                          .FirstOrDefault(n => n.Principal == true);
            }

            ReferredEntity Referred = new ReferredEntity();
            if (client.Client_Referred.Count() > 0)
            {
                Referred = client.Client_Referred
                                 .FirstOrDefault(n => n.Service == ServiceAgency.TCM)
                                 .Referred;
            }

            Client_HealthInsurance Client_HealtInsurance = new Client_HealthInsurance();
            if (client.Clients_HealthInsurances.Count() > 0)
            {
                Client_HealtInsurance = client.Clients_HealthInsurances
                                              .FirstOrDefault(n => n.Active == true
                                                                && n.Agency == ServiceAgency.TCM);
            }

            LegalGuardianEntity legalGuardian = new LegalGuardianEntity();
            if (client.LegalGuardian != null)
            {
                legalGuardian = client.LegalGuardian;
            }

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    salida = new ReferralFormViewModel
                    {
                        //Client
                        Address = client.FullAddress,
                        SecondaryPhone = client.TelephoneSecondary,
                        SSN = client.SSN,
                        DateOfBirth = client.DateOfBirth,
                        MedicaidId = client.MedicaidID,
                        Gender = (client.Gender == GenderType.Female) ? "Female" : "Male",
                        HMO = string.Empty,
                        PrimaryPhone = client.Telephone,
                        //audit
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        //tcmClient
                        CaseNumber = client.Code,
                        NameClient = client.Name,
                        //Referred
                        //AssignedTo = .Name,
                        CaseAcceptedFacilitator = false,
                        CaseAcceptedSupervisor = false,
                        Comments = string.Empty,
                        DateAssigned = client.AdmisionDate,
                        FacilitatorSign = false,
                        SupervisorSign = false,
                        //NameSupervisor = client.Casemanager.TCMSupervisor.Name,
                        Program = "Mental Health Targeted Case Management",
                        //Health Insurance
                        AuthorizedDate = Client_HealtInsurance != null ? Client_HealtInsurance.ApprovedDate : new DateTime(),
                        ExperatedDate = Client_HealtInsurance != null ? Client_HealtInsurance.ApprovedDate.AddMonths(Client_HealtInsurance.DurationTime) : new DateTime(),
                        UnitsApproved = Client_HealtInsurance != null ? Client_HealtInsurance.Units : 0,
                        //Legal Guardian
                        LegalGuardianName = legalGuardian.Name,
                        LegalGuardianPhone = legalGuardian.Telephone,
                        //Diagnostic
                        Dx = client_Diagnostic.Diagnostic.Code,
                        Dx_Description = client_Diagnostic.Diagnostic.Description,
                        //Referred
                        ReferredBy_Name = Referred.Name,
                        ReferredBy_Phone = Referred.Telephone,
                        ReferredBy_Title = Referred.Title,
                        Client = client,
                        IdClient = client.Id,
                        FacilitatorList = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id, false,false),
                        SupervisorList = _combosHelper.GetComboSupervisorByClinic(user_logged.Clinic.Id, true),
                        AssignedBy = user_logged.FullName
                    };

                    return View(salida);
                }
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [HttpPost]
        [Authorize(Roles = "Manager, Frontdesk")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateReferralForm(ReferralFormViewModel model)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk"))
            {
                if (ModelState.IsValid)
                {
                    ReferralFormEntity Referral = _converterHelper.ToReferralEntity(model, true, user_logged.UserName);
                    _context.Add(Referral);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Index", "Clients");
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }


                }
                else
                {
                    return View(model);
                }
            }
            else
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            return View(model);
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public IActionResult ReferralFormReadOnly(int idReferral = 0)
        {
            ReferralFormEntity referral = _context.ReferralForms
                                                  .Include(d => d.Facilitator)
                                                  .Include(d => d.Supervisor)
                                                  .Include(d => d.Client)
                                                  .FirstOrDefault(n => n.Id == idReferral);
            if (referral != null)
            {
                ReferralFormViewModel salida = _converterHelper.ToReferralViewModel(referral);
                return View(salida);

            }


            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> ReferralPending(int idError = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }
            if (User.IsInRole("Supervisor"))
            {
                return View(await _context.ReferralForms
                                          .Include(c => c.Facilitator)
                                          .Include(c => c.Supervisor)
                                          .Include(c => c.Client)
                                          .Where(c => c.Supervisor.LinkedUser == user_logged.UserName
                                                   && c.SupervisorSign == false)
                                          .OrderBy(c => c.Client.Name).ToListAsync());
            }
            if (User.IsInRole("Facilitator"))
            {
                return View(await _context.ReferralForms
                                          .Include(c => c.Facilitator)
                                          .Include(c => c.Supervisor)
                                          .Include(c => c.Client)
                                          .Where(c => c.Facilitator.LinkedUser == user_logged.UserName
                                                   && c.FacilitatorSign == false)
                                          .OrderBy(c => c.Client.Name).ToListAsync());
            }
           

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public IActionResult DetailsPCP(int id = 0)
        {
            if (id != 0)
            {
                DoctorEntity doctor = _context.Doctors.FirstOrDefault(u => u.Id == id);

                DoctorEntity model = new DoctorViewModel
                {
                    Name = doctor.Name,
                    Email = doctor.Email,
                    FaxNumber = doctor.FaxNumber,
                    Telephone = doctor.Telephone,
                    ZipCode = doctor.ZipCode,
                    Address = doctor.Address,
                    City = doctor.City,
                    State = doctor.State
                };
                return View(model);
            }
            else
            {
                return View(new DoctorViewModel());
            }
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public IActionResult DetailsPSY(int id = 0)
        {
            if (id != 0)
            {
                PsychiatristEntity psy = _context.Psychiatrists.FirstOrDefault(u => u.Id == id);

                PsychiatristViewModel model = new PsychiatristViewModel
                {
                    Name = psy.Name,
                    Email = psy.Email,
                    FaxNumber = psy.FaxNumber,
                    Telephone = psy.Telephone,
                    ZipCode = psy.ZipCode,
                    Address = psy.Address,
                    City = psy.City,
                    State = psy.State
                };
                return View(model);
            }
            else
            {
                return View(new PsychiatristViewModel());
            }
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public IActionResult DetailsEC(int id = 0)
        {
            if (id != 0)
            {
                EmergencyContactEntity ec = _context.EmergencyContacts.FirstOrDefault(u => u.Id == id);

                EmergencyContactViewModel model = new EmergencyContactViewModel
                {
                    Name = ec.Name,
                    Email = ec.Email,
                    Telephone = ec.Telephone,
                    ZipCode = ec.ZipCode,
                    Address = ec.Address,
                    City = ec.City,
                    State = ec.State
                };
                return View(model);
            }
            else
            {
                return View(new EmergencyContactViewModel());
            }
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager, Frontdesk")]
        public IActionResult DetailsLG(int id = 0)
        {
            if (id != 0)
            {
                LegalGuardianEntity lg = _context.LegalGuardians.FirstOrDefault(u => u.Id == id);

                LegalGuardianViewModel model = new LegalGuardianViewModel
                {
                    Name = lg.Name,
                    Email = lg.Email,
                    Telephone = lg.Telephone,
                    ZipCode = lg.ZipCode,
                    Address = lg.Address,
                    City = lg.City,
                    State = lg.State
                };
                return View(model);
            }
            else
            {
                return View(new LegalGuardianViewModel());
            }
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ClientWithoutCase(int idError = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }
            if (User.IsInRole("Manager") )
            {
                List<ClientEntity> clients_Total = _context.Clients
                                                           .Include(n => n.Clients_HealthInsurances)
                                                           .ThenInclude(n => n.HealthInsurance)
                                                           .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                                     && c.Status == StatusType.Open
                                                                     && c.OnlyTCM == true))
                                                           .OrderBy(c => c.Name)
                                                           .ToList();

                List<TCMClientEntity> clients_Open = _context.TCMClient
                                                             .Include(n => n.Client)
                                                             .Where(c => (c.Client.Clinic.Id == user_logged.Clinic.Id
                                                                && c.Status == StatusType.Open))
                                                             .ToList();

                foreach (var item in clients_Open)
                {
                    if (item.Client != null)
                    {
                        if (clients_Total.Exists(c => c.Id == item.Client.Id))
                            clients_Total.Remove(item.Client);
                    }
                }
                return View(clients_Total);
            }
            

            return RedirectToAction("NotAuthorized", "Account");
        }


        public async Task<IActionResult> EditDxTempClient(int id = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (id != 0)
            {
                DiagnosticTempEntity DxTempEntity = await _context.DiagnosticsTemp
                                                                  .FirstOrDefaultAsync(u => u.Id == id);

                DiagnosticTempViewModel model = new DiagnosticTempViewModel
                {
                   Code = DxTempEntity.Code,
                   DateIdentify = DxTempEntity.DateIdentify,
                   Description = DxTempEntity.Description,
                   IdClient = DxTempEntity.IdClient,
                   IdDiagnostic = _context.Diagnostics.FirstOrDefault(n => n.Code == DxTempEntity.Code && n.Description == DxTempEntity.Description).Id,
                   Prescriber = (DxTempEntity.Prescriber != null)? DxTempEntity.Prescriber : string.Empty,
                   Principal = DxTempEntity.Principal,
                   Diagnostics = _combosHelper.GetComboDiagnosticsByClinic(user_logged.Id),
                   UserName = DxTempEntity.UserName,
                   Active = DxTempEntity.Active,
                   Id = id

                };
                return View(model);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDxTempClient(int id, DiagnosticTempViewModel model)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                if (id != 0)
                {
                    DiagnosticEntity diagnostic = await _context.Diagnostics.FirstOrDefaultAsync(d => d.Id == model.IdDiagnostic);
                    DiagnosticTempEntity dxTemp = new DiagnosticTempEntity
                    {
                        Id = model.Id,
                        Active = model.Active,
                        Code = diagnostic.Code,
                        Description = diagnostic.Description,
                        DateIdentify = model.DateIdentify,
                        Prescriber = model.Prescriber,
                        Principal = model.Principal,
                        IdClient = model.IdClient,
                        UserName = model.UserName
                        
                        
                    };
                    _context.Update(dxTemp);
                    await _context.SaveChangesAsync();

                    List<DiagnosticTempEntity> list = await _context.DiagnosticsTemp
                                                                    .Where(n => n.IdClient == model.IdClient
                                                                             && n.UserName == model.UserName)
                                                                    .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDiagnostic", list) });

                }
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDiagnostic", _context.DiagnosticsTemp.Where(m => m.IdClient == model.IdClient && m.UserName == model.UserName).ToList()) });
            }


            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditDxTempClient", model) });
        }

        public async Task<IActionResult> DuplicateHealthInsuranceClient(int id = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (id != 0)
            {
                HealthInsuranceTempEntity healthInsuranceTempEntity = await _context.HealthInsuranceTemp
                                                                                    .FirstOrDefaultAsync(u => u.Id == id);

                HealthInsuranceTempViewModel entity = new HealthInsuranceTempViewModel()
                {
                    ApprovedDate = healthInsuranceTempEntity.ApprovedDate,
                    DurationTime = healthInsuranceTempEntity.DurationTime,
                    IdhealthInsurance = _context.HealthInsurances.FirstOrDefault(n => n.Name == healthInsuranceTempEntity.Name).Id,
                    HealthInsurance = _combosHelper.GetComboActiveInsurancesByClinic(user_logged.Clinic.Id),
                    ClientName = _context.Clients.FirstOrDefault(n => n.Id == healthInsuranceTempEntity.IdClient).Name,
                    MemberId = healthInsuranceTempEntity.MemberId,
                    Units = healthInsuranceTempEntity.Units,
                    IdClient = healthInsuranceTempEntity.IdClient,
                    AuthorizationNumber = healthInsuranceTempEntity.AuthorizationNumber,
                    Id = id,
                    Name = healthInsuranceTempEntity.Name,
                    Active = healthInsuranceTempEntity.Active,
                    UserName = user_logged.UserName,
                    IdAgencyService = (healthInsuranceTempEntity.Agency == ServiceAgency.CMH) ? 0 : 1,
                    AgencyServices = _combosHelper.GetComboServiceAgency(),
                    ExpiredDate = healthInsuranceTempEntity.ExpiredDate,
                    EffectiveDate = healthInsuranceTempEntity.EffectiveDate,
                    EndCoverageDate = healthInsuranceTempEntity.EndCoverageDate,
                    IdInsuranceType = (healthInsuranceTempEntity.InsuranceType == InsuranceType.Medicaid) ? 0 : (healthInsuranceTempEntity.InsuranceType == InsuranceType.Medicare) ? 1 : (healthInsuranceTempEntity.InsuranceType == InsuranceType.Comercial) ? 2 : 3,
                    InsuranceTypes = _combosHelper.GetComboInsuranceType(),
                    IdInsurancePlanType = (healthInsuranceTempEntity.InsurancePlan == InsurancePlanType.Full_Medicaid) ? 0 : (healthInsuranceTempEntity.InsurancePlan == InsurancePlanType.Medicare_Part_AB) ? 1 : 2,
                    InsurancePlanTypes = _combosHelper.GetComboInsurancePlanType(),
                    IdInsuranceCoverageType = (healthInsuranceTempEntity.InsuranceCoverage == InsuranceCoverageType.Full_Medicaid) ? 0 : (healthInsuranceTempEntity.InsuranceCoverage == InsuranceCoverageType.MMA_Capitated) ? 1 : (healthInsuranceTempEntity.InsuranceCoverage == InsuranceCoverageType.Dual_Special_Needs_Plan) ? 2 : (healthInsuranceTempEntity.InsuranceCoverage == InsuranceCoverageType.Medicare_Special_Needs) ? 3 : (healthInsuranceTempEntity.InsuranceCoverage == InsuranceCoverageType.Medicare_Advantage_Plan) ? 4 : (healthInsuranceTempEntity.InsuranceCoverage == InsuranceCoverageType.HMO) ? 5 : (healthInsuranceTempEntity.InsuranceCoverage == InsuranceCoverageType.PPO) ? 6 : (healthInsuranceTempEntity.InsuranceCoverage == InsuranceCoverageType.EPO) ? 7 : 8,
                    InsuranceCoverageTypes = _combosHelper.GetComboInsuranceCoverageType()

                };
                return View(entity);
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuplicateHealthInsuranceClient(int id, HealthInsuranceTempViewModel HealthInsuranceModel)
        {
            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                if (id != 0)
                {

                    HealthInsuranceTempEntity healthInsuranceTemp = new HealthInsuranceTempEntity
                    {
                        Id = 0,
                        UserName = HealthInsuranceModel.UserName,
                        ApprovedDate = HealthInsuranceModel.ApprovedDate,
                        Active = HealthInsuranceModel.Active,
                        DurationTime = HealthInsuranceModel.DurationTime,
                        MemberId = HealthInsuranceModel.MemberId,
                        Units = HealthInsuranceModel.Units,
                        Name = HealthInsuranceModel.Name,
                        IdClient = HealthInsuranceModel.IdClient,
                        AuthorizationNumber = HealthInsuranceModel.AuthorizationNumber,
                        Agency = ServiceAgencyUtils.GetServiceAgencyByIndex(HealthInsuranceModel.IdAgencyService),
                        ExpiredDate = HealthInsuranceModel.ExpiredDate,
                        EffectiveDate = HealthInsuranceModel.EffectiveDate,
                        EndCoverageDate = HealthInsuranceModel.EndCoverageDate,
                        InsuranceType = InsuranceUtils.GetInsuranceTypeByIndex(HealthInsuranceModel.IdInsuranceType),
                        InsurancePlan = InsurancePlanUtils.GetInsurancePlanTypeByIndex(HealthInsuranceModel.IdInsurancePlanType),
                        InsuranceCoverage = InsuranceCoverageUtils.GetInsuranceCoverageTypeByIndex(HealthInsuranceModel.IdInsuranceCoverageType)
                    };
                    _context.Add(healthInsuranceTemp);
                    await _context.SaveChangesAsync();

                    List<HealthInsuranceTempEntity> list = await _context.HealthInsuranceTemp
                                                                         .Where(n => n.IdClient == HealthInsuranceModel.IdClient
                                                                            && n.UserName == HealthInsuranceModel.UserName)
                                                                         .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHealthInsurance", list) });

                }
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHealthInsurance", _context.HealthInsuranceTemp.Where(m => m.IdClient == HealthInsuranceModel.IdClient && m.UserName == HealthInsuranceModel.UserName).ToList()) });
            }


            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditHealthInsuranceClient", HealthInsuranceModel) });
        }

        public async Task<IActionResult> UpdateName()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                       
            if (User.IsInRole("Manager") )
            {
                List<ClientEntity> listClient = await _context.Clients.ToListAsync();
                List<ClientEntity> listClientSalida = new List<ClientEntity>();
                ClientEntity Client = new ClientEntity();
                string[] array = new string[0];
                char[] delimitador = { '.' };
               
                foreach (var item in listClient)
                {
                    Client = item;
                    array = item.Name.Split(delimitador);
                    if (array.Length > 2)
                    {
                        Client.FirstName = array[0];
                        for (int i = 1; i < array.Length; i++)
                        {
                            if (i == 1)
                            {
                                Client.LastName = array[i];
                            }
                            else
                            {
                                Client.LastName += ' ' + array[i];
                            }
                        }
                    }
                    else
                    {
                        if (array.Length == 2)
                        {
                            Client.FirstName = array[0] + '.';
                            Client.LastName = array[1];
                        }
                        else
                        {
                            if (array.Length == 1)
                            {
                                array = item.Name.Split(' ');
                                if (array.Length > 1)
                                {
                                    for (int i = 0; i < array.Length; i++)
                                    {
                                        if (i == 0)
                                        {
                                            Client.FirstName = array[i];
                                        }
                                        else
                                        {
                                            if (i == 1)
                                            {
                                                Client.LastName = array[i];
                                            }
                                            else
                                            {
                                                Client.LastName += ' ' + array[i];
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    listClientSalida.Add(Client);
                    Client = new ClientEntity();
                }
                _context.UpdateRange(listClientSalida);
                await _context.SaveChangesAsync();
                              
            }

            return RedirectToAction("Index", "Clients");
        }

    }
}