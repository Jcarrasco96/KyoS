using System;
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
        
        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, CaseManager")]
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
            if (User.IsInRole("Manager") || User.IsInRole("Supervisor"))
            {
                return View(await _context.Clients
                                          .Include(c => c.Clinic)
                                          .Include(c => c.IndividualTherapyFacilitator)
                                          .Include(c => c.Clients_HealthInsurances)
                                          .ThenInclude(c => c.HealthInsurance)
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
                                          .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                && (c.Bio.CreatedBy == user_logged.UserName
                                                     || c.MTPs.Where(m => m.CreatedBy == user_logged.UserName).Count() > 0)))
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
                                                          .FirstOrDefaultAsync(f => f.Casemanager.Id == casemanager.Id);

                return View(tcmClient.Client);
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, Supervisor")]
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

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
                    List<SelectListItem> list = new List<SelectListItem>();
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
                        City = "Miami",
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
                        //IdReferred = 0,
                        //Referreds = _combosHelper.GetComboReferredsByClinic(user_logged.Id),
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
                        ITFacilitators = _combosHelper.GetComboFacilitators()

                    };
                    return View(model);
                }
            }

            model = new ClientViewModel
            {
                DateOfBirth = DateTime.Today.AddYears(-60),
                Clinics = _combosHelper.GetComboClinics(),
                IdGender = 1,
                GenderList = _combosHelper.GetComboGender(),
                IdStatus = 1,
                StatusList = _combosHelper.GetComboClientStatus()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Supervisor")]
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

                ClientEntity clientEntity = await _converterHelper.ToClientEntity(clientViewModel, true, photoPath, signPath, user_logged.Id);

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
                        Principal = item.Principal
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
                        ReferredNote = item.ReferredNote
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
                        AuthorizationNumber = item.AuthorizationNumber
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

        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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

            //if (clientEntity.EmergencyContact != null)
            //{
            //    clientViewModel.NameEmergencyContact = clientEntity.EmergencyContact.Name;
            //    clientViewModel.AddressEmergencyContact = clientEntity.EmergencyContact.Address;
            //    clientViewModel.AddressLine2EmergencyContact = clientEntity.EmergencyContact.Address;
            //    clientViewModel.CityEmergencyContact = clientEntity.EmergencyContact.City;
            //    clientViewModel.CountryEmergencyContact = clientEntity.EmergencyContact.Country;
            //    clientViewModel.EmailEmergencyContact = clientEntity.EmergencyContact.Email;
            //    clientViewModel.StateEmergencyContact = clientEntity.EmergencyContact.State;
            //    clientViewModel.PhoneEmergencyContact = clientEntity.EmergencyContact.Telephone;
            //    clientViewModel.PhoneSecundaryEmergencyContact = clientEntity.EmergencyContact.TelephoneSecondary;
            //    clientViewModel.ZipCodeEmergencyContact = clientEntity.EmergencyContact.ZipCode;
            //    clientViewModel.CreateByEmergencyContact = clientEntity.EmergencyContact.CreatedBy;
            //    clientViewModel.CreateOnEmergencyContact = clientEntity.EmergencyContact.CreatedOn;

            //}
            //else
            //{
            //    clientEntity.EmergencyContact = new EmergencyContactEntity();
            //}

            return View(clientViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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

                ClientEntity clientEntity = await _converterHelper.ToClientEntity(clientViewModel, false, photoPath, signPath, user_logged.Id);
                if (clientViewModel.IdStatus == 2) //the client was closed
                {
                    _context.Entry(clientEntity).Reference("Group").CurrentValue = null;
                    _context.Entry(clientEntity).Reference("Group").IsModified = true;
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
                    /*if (clientViewModel.NameEmergencyContact != string.Empty)
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
                        //_context.Update(emergencyContact);
                        

                    }*/
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
                        Principal = item.Principal
                    };
                    _context.Add(clientDiagnostic);
                    _context.DiagnosticsTemp.Remove(item);
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
                        ReferredNote = item1.ReferredNote
                    };
                    _context.Add(clientReferred);
                    _context.ReferredsTemp.Remove(item1);
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
                    _context.DocumentsTemp.Remove(item);
                }

                //delete all client Health Insurance of this client
                IEnumerable<Client_HealthInsurance> listHealthInsurance_to_delete = await _context.Clients_HealthInsurances
                                                                                                  .Where(cd => cd.Client.Id == clientViewModel.Id)
                                                                                                  .ToListAsync();
                _context.Clients_HealthInsurances.RemoveRange(listHealthInsurance_to_delete);

                //update Client_HealthInsurance table with the news HealthInsuranceTemp
                IQueryable <HealthInsuranceTempEntity> listHealthInsuranceTemp = _context.HealthInsuranceTemp
                                                                                         .Where(d => d.UserName == user_logged.UserName
                                                                                            && d.IdClient == clientEntity.Id);
                Client_HealthInsurance clientHealthInsurance ;
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
                        AuthorizationNumber = item.AuthorizationNumber
                    };
                    _context.Add(clientHealthInsurance);
                    _context.HealthInsuranceTemp.Remove(item);
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

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant, CaseManager")]
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

            return View(clientViewModel);
        }

        [Authorize(Roles = "Supervisor, Manager, Facilitator, Documents_Assistant")]
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
                    return View(await _context.Clients
                                              .Include(c => c.MTPs)
                                              .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                        && c.MTPs.Count == 0
                                                        && c.OnlyTCM == false))
                                              .ToListAsync());

                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, Supervisor")]
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

        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
        public IActionResult AddDiagnostic(int id = 0, int idClient = 0)
        {
            if (id == 0)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                DiagnosticTempViewModel model = new DiagnosticTempViewModel
                {
                    IdDiagnostic = 0,
                    Diagnostics = _combosHelper.GetComboDiagnosticsByClinic(user_logged.Id),
                    UserName = user_logged.UserName,
                    IdClient = idClient
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
        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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
                        IdClient = diagnosticTempViewModel.IdClient
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
                IdClient = diagnosticTempViewModel.IdClient
            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddDiagnostic", model) });
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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
        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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

        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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

        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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

        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
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

        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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

        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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

        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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

        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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
                        IdClient = client.Id
                    };
                    _context.Add(diagnostic);
                }
                _context.SaveChanges();
            }            
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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

        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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
                        IdClient = client.Id
                    };
                    _context.Add(referred);
                }
                _context.SaveChanges();
            }
        }

        [Authorize(Roles = "Manager, Supervisor")]
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

        [Authorize(Roles = "Manager, Facilitator, Supervisor, Documents_Assistant")]
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
                                                              .Where(g => (g.Clinic.Id == user_logged.Clinic.Id
                                                                        && g.Bio.CreatedBy == user_logged.UserName
                                                                        && g.OnlyTCM == false))
                                                              .OrderBy(g => g.Name)
                                                              .ToListAsync();

                return View(ClientList);
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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
                    IdClient = idClient

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
        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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
                        IdClient = referredTempViewModel.IdClient

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
                IdClient = referredTempViewModel.IdClient
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

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> ClientHistory(int idClient = 0)
        {
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

                                                .FirstOrDefaultAsync(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                   && w.Id == idClient));



            return View(client);
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant")]
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
                        if (client.MTPs.ElementAtOrDefault(0).AdmissionDateMTP.AddMonths(client.MTPs.ElementAtOrDefault(0).NumberOfMonths.Value + client.MTPs.ElementAtOrDefault(0).MtpReviewList.ElementAtOrDefault(0).MonthOfTreatment) > client.DateOfClose)
                        {
                            tempProblem.Name = "Date of Close";
                            tempProblem.Description = "Date of close is out of term";
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
                        if (client.MTPs.ElementAtOrDefault(0).AdmissionDateMTP.AddMonths(client.MTPs.ElementAtOrDefault(0).NumberOfMonths.Value) > client.DateOfClose)
                        {
                            tempProblem.Name = "Date of Close";
                            tempProblem.Description = "Date of close is out of term";
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
                tempProblem.Description = "The amount of FARS does not match with Addendums (" + cant_Addendums +")";
                tempProblem.Active = 1;
                problem.Add(tempProblem);
                tempProblem = new Problem();

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

            if (dischage_psr == true && fars_d_psr == false)
            {
                tempProblem.Name = "FARS";
                tempProblem.Description = "FARS with incompatible date (Discharge PSR)";
                tempProblem.Active = 1;
                problem.Add(tempProblem);
                tempProblem = new Problem();
            }
            if (dischage_group == true && fars_d_group == false)
            {
                tempProblem.Name = "FARS";
                tempProblem.Description = "FARS with incompatible date (Discharge Group)";
                tempProblem.Active = 1;
                problem.Add(tempProblem);
                tempProblem = new Problem();
            }
            if (dischage_ind == true && fars_d_ind == false)
            {
                tempProblem.Name = "FARS";
                tempProblem.Description = "FARS with incompatible date (Discharge Ind.)";
                tempProblem.Active = 1;
                problem.Add(tempProblem);
                tempProblem = new Problem();
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

        [Authorize(Roles = "Manager, Supervisor")]
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

        [Authorize(Roles = "Manager, Supervisor")]
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
                        AuthorizationNumber = item.AuthorizationNumber

                    };
                    _context.Add(healthInsuranceTemp);
                }
                _context.SaveChanges();
            }
        }

        [Authorize(Roles = "Manager, Supervisor, CaseManager")]
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
                    AuthorizationNumber = string.Empty
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
                    AuthorizationNumber = string.Empty
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
                        item.Active = false;
                        _context.Update(item);
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
                        AuthorizationNumber = HealthInsuranceModel.AuthorizationNumber
                    };
                    _context.Add(healthInsuranceTemp);
                    await _context.SaveChangesAsync();

                    List<HealthInsuranceTempEntity> list = await _context.HealthInsuranceTemp.Where(n => n.IdClient == HealthInsuranceModel.IdClient)
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
                AuthorizationNumber = HealthInsuranceModel.AuthorizationNumber
            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddHealthInsuranceClient", model) });
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
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

            List<ClientEntity> client_Diagnostics_List = _context.Clients
                                                                 .Include(m => m.Clients_Diagnostics)
                                                                 .Where(n => (n.Clinic.Id == user_logged.Clinic.Id))
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

        [Authorize(Roles = "Manager")]
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

        [Authorize(Roles = "Manager")]
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
                                      .Include(c => c.IndividualTherapyFacilitator)
                                      .Include(c => c.Clients_HealthInsurances)
                                        .ThenInclude(c => c.HealthInsurance)

                                      .Where(c => c.Clinic.Id == user_logged.Clinic.Id)
                                      .OrderBy(c => c.Name).ToListAsync());       
        }

        [Authorize(Roles = "Manager")]
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

        [Authorize(Roles = "Manager")]
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

        [Authorize(Roles = "Manager")]
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

                                      .Where(c => c.Clinic.Id == user_logged.Clinic.Id)
                                      .OrderBy(c => c.Name).ToListAsync());                     
        }

        [Authorize(Roles = "Manager")]
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
                            stream = _reportHelper.FloridaSocialHSDischargeReport(discharge);
                        
                        if (client.Clinic.Name == "DREAMS MENTAL HEALTH INC")                        
                            stream = _reportHelper.DreamsMentalHealthDischargeReport(discharge);
                        
                        if (client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")                        
                            stream = _reportHelper.CommunityHTCDischargeReport(discharge);
                        
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
                    }                                     
                }
            }

            return File(_fileHelper.Zip(fileContentList), "application/zip", $"{client.Name}_Documents.zip");
        }

        //[Authorize(Roles = "Manager")]
        //public async Task<IActionResult> DownloadApprovedNotes(int id)
        //{
        //    ClientEntity client = await _context.Clients
        //                                        .FirstOrDefaultAsync(c => c.Id == id);

        //    if (client == null)
        //    {
        //        return RedirectToAction("Home/Error404");
        //    }

        //    //PSR schema 1, 2 y 4
        //    List<Workday_Client> workdayClientList = await _context.Workdays_Clients

        //                                                           .Include(wc => wc.Facilitator)

        //                                                           .Include(wc => wc.Client)
        //                                                                .ThenInclude(c => c.MTPs)
        //                                                                    .ThenInclude(m => m.Goals)
        //                                                                        .ThenInclude(g => g.Objetives)

        //                                                           .Include(wc => wc.Client)
        //                                                                .ThenInclude(c => c.Clients_Diagnostics)
        //                                                                    .ThenInclude(cd => cd.Diagnostic)

        //                                                           .Include(wc => wc.Note)
        //                                                                .ThenInclude(n => n.Supervisor)
        //                                                                    .ThenInclude(s => s.Clinic)

        //                                                           .Include(wc => wc.Note)
        //                                                                .ThenInclude(n => n.Notes_Activities)
        //                                                                    .ThenInclude(na => na.Activity)
        //                                                                        .ThenInclude(a => a.Theme)

        //                                                           .Include(wc => wc.Note)
        //                                                                .ThenInclude(n => n.Notes_Activities)
        //                                                                    .ThenInclude(na => na.Objetive)
        //                                                                        .ThenInclude(o => o.Goal)

        //                                                           .Include(wc => wc.Workday)

        //                                                           .Where(wc => (wc.Client.Id == id && (wc.Note != null && wc.Note.Status == NoteStatus.Approved)))
        //                                                           .ToListAsync();
            
        //    List<FileContentResult> fileContentList = new List<FileContentResult>();
        //    Stream stream = null;

        //    foreach (var workdayClient in workdayClientList)
        //    {
        //        if (workdayClient.Note.Supervisor.Clinic.Name == "DAVILA")
        //        {
        //            if (workdayClient.Note.Schema == SchemaType.Schema1)
        //            {
        //                fileContentList.Add(DavilaNoteReportFCRSchema1(workdayClient));
        //            }
        //            if (workdayClient.Note.Schema == SchemaType.Schema4)
        //            {
        //                stream = _reportHelper.DavilaNoteReportSchema4(workdayClient);
        //                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"PSR/PSR_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf"));
        //            }                    
        //        }
        //        if (workdayClient.Note.Supervisor.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
        //        {
        //            fileContentList.Add(FloridaSocialHSNoteReportFCRSchema2(workdayClient));
        //        }
        //    }

        //    //PSR schema 3
        //    workdayClientList = await _context.Workdays_Clients

        //                                      .Include(wc => wc.Facilitator)

        //                                      .Include(wc => wc.Client)

        //                                      .Include(wc => wc.NoteP)
        //                                        .ThenInclude(n => n.Supervisor)
        //                                            .ThenInclude(s => s.Clinic)

        //                                      .Include(wc => wc.NoteP)
        //                                        .ThenInclude(n => n.NotesP_Activities)
        //                                            .ThenInclude(na => na.Activity)
        //                                                .ThenInclude(a => a.Theme)

        //                                      .Include(wc => wc.NoteP)
        //                                        .ThenInclude(n => n.NotesP_Activities)
        //                                            .ThenInclude(na => na.Objetive)
        //                                                .ThenInclude(o => o.Goal)

        //                                      .Include(wc => wc.Workday)
        //                                        .ThenInclude(w => w.Workdays_Activities_Facilitators)
        //                                            .ThenInclude(waf => waf.Facilitator)

        //                                      .Where(wc => (wc.Client.Id == id && (wc.NoteP != null && wc.NoteP.Status == NoteStatus.Approved)))
        //                                      .ToListAsync();

        //    foreach (var workdayClient in workdayClientList)
        //    {
        //        if (workdayClient.NoteP.Supervisor.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
        //        {
        //            if (workdayClient.NoteP.Schema == SchemaType.Schema3)
        //            {                        
        //                if (!workdayClient.SharedSession)
        //                    stream = _reportHelper.FloridaSocialHSNoteReportSchema3(workdayClient);
        //                else
        //                    stream = _reportHelper.FloridaSocialHSNoteReportSchema3SS(workdayClient);
        //                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"PSR/PSR_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf"));
        //            }
        //        }
        //        if (workdayClient.NoteP.Supervisor.Clinic.Name == "DREAMS MENTAL HEALTH INC")
        //        {
        //            if (workdayClient.NoteP.Schema == SchemaType.Schema3)
        //            {
        //                if (!workdayClient.SharedSession)
        //                    stream = _reportHelper.DreamsMentalHealthNoteReportSchema3(workdayClient);
        //                else
        //                    stream = _reportHelper.DreamsMentalHealthNoteReportSchema3SS(workdayClient);
        //                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"PSR/PSR_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf"));
        //            }
        //        }
        //        if (workdayClient.NoteP.Supervisor.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
        //        {
        //            if (workdayClient.NoteP.Schema == SchemaType.Schema3)
        //            {
        //                if (!workdayClient.SharedSession)
        //                    stream = _reportHelper.CommunityHTCNoteReportSchema3(workdayClient);
        //                else
        //                    stream = _reportHelper.CommunityHTCNoteReportSchema3SS(workdayClient);
        //                fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"PSR/PSR_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf"));
        //            }
        //        }
        //    }

        //    //Group schema 1
        //    workdayClientList = await _context.Workdays_Clients

        //                                      .Include(wc => wc.Facilitator)

        //                                      .Include(wc => wc.Client)
        //                                          .ThenInclude(c => c.MTPs)
        //                                              .ThenInclude(m => m.Goals)
        //                                                  .ThenInclude(g => g.Objetives)

        //                                      .Include(wc => wc.GroupNote)
        //                                          .ThenInclude(n => n.Supervisor)
        //                                              .ThenInclude(s => s.Clinic)

        //                                      .Include(wc => wc.GroupNote)
        //                                          .ThenInclude(n => n.GroupNotes_Activities)
        //                                              .ThenInclude(na => na.Activity)
        //                                                  .ThenInclude(a => a.Theme)

        //                                      .Include(wc => wc.GroupNote)
        //                                          .ThenInclude(n => n.GroupNotes_Activities)
        //                                              .ThenInclude(na => na.Objetive)
        //                                                  .ThenInclude(o => o.Goal)

        //                                      .Include(wc => wc.Workday)

        //                                      .Where(wc => (wc.Client.Id == id && (wc.GroupNote != null && wc.GroupNote.Status == NoteStatus.Approved)))
        //                                      .ToListAsync();

        //    foreach (var workdayClient in workdayClientList)
        //    {
        //        if (workdayClient.GroupNote.Supervisor.Clinic.Name == "DAVILA")                
        //            stream = _reportHelper.DavilaGroupNoteReportSchema1(workdayClient);                    
                
        //        if (workdayClient.GroupNote.Supervisor.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")                
        //            stream = _reportHelper.FloridaSocialHSGroupNoteReportSchema1(workdayClient);                    
                
        //        if (workdayClient.GroupNote.Supervisor.Clinic.Name == "DREAMS MENTAL HEALTH INC")                
        //            stream = _reportHelper.DreamsMentalHealthGroupNoteReportSchema1(workdayClient);                    
                
        //        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Group/Group_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf"));
        //    }

        //    //Group schema 2
        //    workdayClientList = await _context.Workdays_Clients

        //                                      .Include(wc => wc.Facilitator)

        //                                      .Include(wc => wc.Client)
        //                                        .ThenInclude(c => c.MTPs)
        //                                            .ThenInclude(m => m.Goals)
        //                                                .ThenInclude(g => g.Objetives)

        //                                      .Include(wc => wc.GroupNote2)
        //                                        .ThenInclude(n => n.Supervisor)
        //                                            .ThenInclude(s => s.Clinic)

        //                                      .Include(wc => wc.GroupNote2)
        //                                        .ThenInclude(n => n.GroupNotes2_Activities)
        //                                            .ThenInclude(na => na.Activity)
        //                                                .ThenInclude(a => a.Theme)

        //                                      .Include(wc => wc.GroupNote2)
        //                                        .ThenInclude(n => n.GroupNotes2_Activities)
        //                                            .ThenInclude(na => na.Objetive)
        //                                                .ThenInclude(o => o.Goal)

        //                                      .Include(wc => wc.Workday)

        //                                      .Include(wc => wc.Schedule)

        //                                      .Where(wc => (wc.Client.Id == id && (wc.GroupNote2 != null && wc.GroupNote2.Status == NoteStatus.Approved)))
        //                                      .ToListAsync();

        //    foreach (var workdayClient in workdayClientList)
        //    {
        //        if (workdayClient.GroupNote2.Supervisor.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
        //            stream = _reportHelper.FloridaSocialHSGroupNoteReportSchema3(workdayClient);

        //        if (workdayClient.GroupNote2.Supervisor.Clinic.Name == "DREAMS MENTAL HEALTH INC")
        //            stream = _reportHelper.DreamsMentalHealthGroupNoteReportSchema3(workdayClient);

        //        if (workdayClient.GroupNote2.Supervisor.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")                
        //            stream = _reportHelper.CommunityHTCGroupNoteReportSchema3(workdayClient);                  
                
        //        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Group/Group_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf"));
        //    }

        //    //Individual schema 1
        //    workdayClientList = await _context.Workdays_Clients

        //                                      .Include(wc => wc.Facilitator)

        //                                      .Include(wc => wc.Client)
        //                                        .ThenInclude(c => c.MTPs)
        //                                            .ThenInclude(m => m.Goals)
        //                                                .ThenInclude(g => g.Objetives)

        //                                      .Include(wc => wc.Client)
        //                                        .ThenInclude(c => c.Clients_Diagnostics)
        //                                            .ThenInclude(cd => cd.Diagnostic)

        //                                      .Include(wc => wc.IndividualNote)
        //                                        .ThenInclude(n => n.Supervisor)
        //                                            .ThenInclude(s => s.Clinic)

        //                                      .Include(wc => wc.IndividualNote)
        //                                        .ThenInclude(n => n.Objective)

        //                                      .Include(wc => wc.Workday)

        //                                      .Where(wc => (wc.Client.Id == id && (wc.IndividualNote != null && wc.IndividualNote.Status == NoteStatus.Approved)))
        //                                      .ToListAsync();

        //    foreach (var workdayClient in workdayClientList)
        //    {
        //        if (workdayClient.IndividualNote.Supervisor.Clinic.Name == "DAVILA")                
        //            stream = _reportHelper.DavilaIndNoteReportSchema1(workdayClient);                    
                
        //        if (workdayClient.IndividualNote.Supervisor.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")                
        //            stream = _reportHelper.FloridaSocialHSIndNoteReportSchema1(workdayClient);                    
                
        //        if (workdayClient.IndividualNote.Supervisor.Clinic.Name == "DREAMS MENTAL HEALTH INC")                
        //            stream = _reportHelper.DreamsMentalHealthIndNoteReportSchema1(workdayClient);                   
                
        //        if (workdayClient.IndividualNote.Supervisor.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")                
        //            stream = _reportHelper.CommunityHTCIndNoteReportSchema1(workdayClient);                    
                
        //        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"Individual/Ind_{workdayClient.Workday.Date.Month}_{workdayClient.Workday.Date.Day}_{workdayClient.Workday.Date.Year}.pdf"));
        //    }

        //    return File(_fileHelper.Zip(fileContentList), "application/zip", $"{client.Name}_Notes.zip");
        //}

        [Authorize(Roles = "Manager")]
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

                                                                   .Where(wc => (wc.Client.Id == idClient && (wc.IndividualNote != null && wc.IndividualNote.Status == NoteStatus.Approved)))
                                                                   .ToListAsync();
            }
            
            List<FileContentResult> fileContentList = new List<FileContentResult>();
            Stream stream = null;
            foreach (var workdayClient in workdayClientList)
            {
                if (workdayClient.IndividualNote.Supervisor.Clinic.Name == "DAVILA")
                    stream = _reportHelper.DavilaIndNoteReportSchema1(workdayClient);

                if (workdayClient.IndividualNote.Supervisor.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
                    stream = _reportHelper.FloridaSocialHSIndNoteReportSchema1(workdayClient);

                if (workdayClient.IndividualNote.Supervisor.Clinic.Name == "DREAMS MENTAL HEALTH INC")
                    stream = _reportHelper.DreamsMentalHealthIndNoteReportSchema1(workdayClient);

                if (workdayClient.IndividualNote.Supervisor.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
                    stream = _reportHelper.CommunityHTCIndNoteReportSchema1(workdayClient);

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
                    UserName = user_logged.UserName
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
                        AuthorizationNumber = HealthInsuranceModel.AuthorizationNumber
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

        [Authorize(Roles = "Manager")]
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
        [Authorize(Roles = "Manager")]
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

        [Authorize(Roles = "Manager")]
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
           
            string mounth = string.Empty;
            if (DateTime.Today.Month == 1)
            {
                mounth = "January";
            }
            if (DateTime.Today.Month == 2)
            {
                mounth = "February";
            }
            if (DateTime.Today.Month == 3)
            {
                mounth = "March";
            }
            if (DateTime.Today.Month == 4)
            {
                mounth = "April";
            }
            if (DateTime.Today.Month == 5)
            {
                mounth = "May";
            }
            if (DateTime.Today.Month == 6)
            {
                mounth = "June";
            }
            if (DateTime.Today.Month == 7)
            {
                mounth = "July";
            }
            if (DateTime.Today.Month == 8)
            {
                mounth = "August";
            }
            if (DateTime.Today.Month == 9)
            {
                mounth = "September";
            }
            if (DateTime.Today.Month == 10)
            {
                mounth = "October";
            }
            if (DateTime.Today.Month == 11)
            {
                mounth = "November";
            }
            if (DateTime.Today.Month == 12)
            {
                mounth = "December";
            }
            ViewData["mounth"] = mounth;

            if (User.IsInRole("Manager"))
            {
                return View(await _context.Clients
                                          .Include(n => n.Clients_HealthInsurances)
                                          .ThenInclude(n => n.HealthInsurance)
                                          .Where(n => n.Clinic.Id == user_logged.Clinic.Id
                                                   && n.Service == ServiceType.PSR
                                                   && n.Status == StatusType.Open
                                                  && (n.Clients_HealthInsurances == null
                                                   || n.Clients_HealthInsurances.Where(m => m.Active == true
                                                             && m.ApprovedDate.AddMonths(m.DurationTime) > DateTime.Today.AddDays(15)).Count() == 0))
                                          .ToListAsync());
            }

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager")]
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
                        
            if(month == 0)
                return View(await _context.Clients
                                          .Where(c => (c.DateOfBirth.Month == DateTime.Today.Month && c.Status == StatusType.Open))
                                          .ToListAsync());          
            else
                return View(await _context.Clients
                                          .Where(c => (c.DateOfBirth.Month == month && c.Status == StatusType.Open))
                                          .ToListAsync());
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
        public async Task<IActionResult> ActiveClients()
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
                                        .Where(c => c.Status == StatusType.Open
                                                 && c.Clinic.Id == user_logged.Clinic.Id
                                                 && (c.Workdays_Clients.Where(m => m.Facilitator.Id == facilitator.Id).Count() > 0
                                                    || c.IndividualTherapyFacilitator.Id == facilitator.Id
                                                    || c.IdFacilitatorPSR == facilitator.Id
                                                    || c.IdFacilitatorGroup == facilitator.Id))
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

                   

                    salida.Add(temp);
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

            return View(salida);
        }
    }
}