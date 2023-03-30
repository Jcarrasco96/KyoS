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
using KyoS.Web.Data.Contracts;
using KyoS.Common.Helpers;

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

        public ClientsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper, IMimeType mimeType, IExportExcellHelper exportExcelHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
            _imageHelper = imageHelper;
            _mimeType = mimeType;
            _exportExcelHelper = exportExcelHelper;
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
                                                    || c.IndividualTherapyFacilitator.Id == facilitator.Id)))
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
                    ViewBag.Creado = "N";
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
                        HealthInsuranceTemp = _context.HealthInsuranceTemp.Where(n => n.UserName == user_logged.UserName && n.IdClient == 0)
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
                        Units = item.Units
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

                                                     // .Include(c => c.Client_Referred)

                                                      .Include(c => c.LegalGuardian)

                                                      .Include(c => c.EmergencyContact)

                                                      .Include(c => c.IndividualTherapyFacilitator)                                                      

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

            if (!User.IsInRole("Admin"))
            {
                if (user_logged.Clinic != null)
                {
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = user_logged.Clinic.Name,
                        Value = $"{user_logged.Clinic.Id}"
                    });
                    clientViewModel.Clinics = list;
                }
            }
            clientViewModel.Origin = origin;
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
                        Units = item.Units
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

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, CaseManager")]
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
                                                      .Include(c => c.Client_Referred)
                                                      .Include(c => c.LegalGuardian)
                                                      .Include(c => c.EmergencyContact)
                                                      .Include(c => c.IndividualTherapyFacilitator)
                                                      .FirstOrDefaultAsync(c => c.Id == id);
            if (clientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            this.DeleteDiagnosticsTemp();
            this.DeleteDocumentsTemp();
            this.DeleteReferredsTemp();

            this.SetDiagnosticsTemp(clientEntity);
            this.SetDocumentsTemp(clientEntity);
            this.SetReferredsTemp(clientEntity);

            ClientViewModel clientViewModel = await _converterHelper.ToClientViewModel(clientEntity, user_logged.Id);
                        
            if (user_logged.Clinic != null)
            {
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = user_logged.Clinic.Name,
                    Value = $"{user_logged.Clinic.Id}"
                });
                clientViewModel.Clinics = list;
            }
            
            clientViewModel.Origin = origin;
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

            //FARS
            if (client.FarsFormList.Count() > 0 && client.Bio != null )
            {
                if (client.FarsFormList.Where(n => n.EvaluationDate == client.Bio.DateBio ).Count() == 0)
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
                            tempProblem.Active = 0;
                        }
                        else
                        {
                            if (client.DateOfClose == null || client.DateOfClose == date)
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
                            tempProblem.Active = 0;
                        }
                        else
                        {
                            if (client.DateOfClose == null || client.DateOfClose == date)
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
                }
                if (client.Workdays_Clients.Where(n => n.IndividualNote != null).Count() > 0)
                {
                    cant_Discharge++;
                    dischage_ind = true;
                }
                if (client.Workdays_Clients.Where(n => n.GroupNote != null).Count() > 0)
                {
                    cant_Discharge++;
                    dischage_group = true;
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
               
                cant_Fars++;

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
                tempProblem.Name = "FARS";
                tempProblem.Description = "FARS with incompatible date (MTPR)";
                tempProblem.Active = 1;
                problem.Add(tempProblem);
                tempProblem = new Problem();
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
                                                    .Where(wc => (wc.Client.Id == idClient
                                                               && wc.Present == true
                                                               && wc.Workday.Service == ServiceType.PSR)).ToListAsync();
            not_started_list = not_started_list.Where(wc => (wc.Note == null && wc.NoteP == null)).ToList();

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
                        IdClient = client.Id

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
                    MemberId = "",
                    Units = 0,
                    IdClient = idClient
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
                    ClientName ="",
                    MemberId = "",
                    Units = 0,
                    IdClient = idClient
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
                        IdClient = HealthInsuranceModel.IdClient
                    };
                    _context.Add(healthInsuranceTemp);
                    await _context.SaveChangesAsync();
                }
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewHealthInsurance", _context.HealthInsuranceTemp.Where(d => d.UserName == user_logged.UserName).ToList()) });
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
                IdClient = HealthInsuranceModel.IdClient
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
            var encodedImage = dataUrl.Split(',')[1];
            var decodedImage = Convert.FromBase64String(encodedImage);

            string guid = Guid.NewGuid().ToString();
            string file = $"{guid}.png";
            string path = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\images\\Clients", file);

            System.IO.File.WriteAllBytes(path, decodedImage);           

            ClientEntity client = await _context.Clients
                                                .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(id));
            if (client != null)
            {
                client.SignPath = $"~/images/Clients/{file}";
                _context.Update(client);
                await _context.SaveChangesAsync();
            }

            return Json(new { redirectToUrl = Url.Action("Signatures", "Clients") });
        }
    }
}