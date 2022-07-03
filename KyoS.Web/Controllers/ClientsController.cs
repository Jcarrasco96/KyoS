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

        public ClientsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper, IMimeType mimeType)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
            _imageHelper = imageHelper;
            _mimeType = mimeType;
        }
        
        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant")]
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

            return View(await _context.Clients
                                      .Include(c => c.Clinic)
                                      .Include(c => c.IndividualTherapyFacilitator)
                                      .Where(c => c.Clinic.Id == user_logged.Clinic.Id)
                                      .OrderBy(c => c.Name).ToListAsync());
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
                        IdReferred = 0,
                        Referreds = _combosHelper.GetComboReferredsByClinic(user_logged.Id),
                        IdEmergencyContact = 0,
                        EmergencyContacts = _combosHelper.GetComboEmergencyContactsByClinic(user_logged.Id),
                        IdDoctor = 0,
                        Doctors = _combosHelper.GetComboDoctorsByClinic(user_logged.Id),
                        IdPsychiatrist = 0,
                        Psychiatrists = _combosHelper.GetComboPsychiatristsByClinic(user_logged.Id),
                        IdLegalGuardian = 0,
                        LegalsGuardians = _combosHelper.GetComboLegalGuardiansByClinic(user_logged.Id),
                        DiagnosticTemp = _context.DiagnosticsTemp,
                        DocumentTemp = _context.DocumentsTemp,
                        OtherLanguage_Read = false,
                        OtherLanguage_Speak = false,
                        OtherLanguage_Understand = false,
                        MedicareId = ""
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
                /*ClientEntity client = await _context.Clients.FirstOrDefaultAsync(c => c.Name == clientViewModel.Name);
                if (client == null)
                {*/
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
                    IQueryable<DiagnosticTempEntity> list_to_delete = _context.DiagnosticsTemp;
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

                    //----------update Documents table-------------//
                    IQueryable<DocumentTempEntity> list_to_delete_doc = _context.DocumentsTemp;
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
                /*}
                else
                {
                    return RedirectToAction("Create", new { id = 2 });
                }*/
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

        [Authorize(Roles = "Manager, Supervisor")]
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
                                                      .Include(c => c.Referred)
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

            this.SetDiagnosticsTemp(clientEntity);
            this.SetDocumentsTemp(clientEntity);

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
        [Authorize(Roles = "Manager, Supervisor")]
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
                _context.Update(clientEntity);

                //delete all client diagnostic of this client
                IEnumerable<Client_Diagnostic> list_to_delete = await _context.Clients_Diagnostics
                                                                              .Where(cd => cd.Client.Id == clientViewModel.Id)
                                                                              .ToListAsync();
                _context.Clients_Diagnostics.RemoveRange(list_to_delete);

                //update Client_Diagnostic table with the news DiagnosticsTemp
                IQueryable<DiagnosticTempEntity> listDiagnosticTemp = _context.DiagnosticsTemp;
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

                //update Documents table with the news DocumentsTemp
                IQueryable<DocumentTempEntity> listDocumentTemp = _context.DocumentsTemp;
                DocumentEntity document;
                foreach (DocumentTempEntity item in listDocumentTemp)
                {
                    document = await _context.Documents.FirstOrDefaultAsync(d => d.FileUrl == item.DocumentPath);
                    if (document == null)
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
                    }                    
                    _context.DocumentsTemp.Remove(item);
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

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant")]
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
                                                      .Include(c => c.Referred)
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

            this.SetDiagnosticsTemp(clientEntity);
            this.SetDocumentsTemp(clientEntity);

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
                                                        && c.MTPs.Count == 0))
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
                                          .Where(c => c.MTPs.Count == 0)
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
                                                              .Where(c => (c.Clinic.Id == clinic.Id))
                                                              .OrderBy(c => c.Name)
                                                              .ToListAsync();
                    client = client.Where(c => c.MissingDoc != string.Empty).ToList();
                    return View(client);
                }                    
                else
                    return View(null);
            }
        }

        [Authorize(Roles = "Manager, Supervisor")]
        public IActionResult AddDiagnostic(int id = 0)
        {
            if (id == 0)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                DiagnosticTempViewModel model = new DiagnosticTempViewModel
                {
                    IdDiagnostic = 0,
                    Diagnostics = _combosHelper.GetComboDiagnosticsByClinic(user_logged.Id)
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
        [Authorize(Roles = "Manager, Supervisor")]
        public async Task<IActionResult> AddDiagnostic(int id, DiagnosticTempViewModel diagnosticTempViewModel)
        {
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
                        Principal = diagnosticTempViewModel.Principal                        
                    }; 
                    _context.Add(diagnosticTemp);
                    await _context.SaveChangesAsync();
                }
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDiagnostic", _context.DiagnosticsTemp.ToList()) });
            }

            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            DiagnosticTempViewModel model = new DiagnosticTempViewModel
            {
                IdDiagnostic = 0,
                Diagnostics = _combosHelper.GetComboDiagnosticsByClinic(user_logged.Id)
            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddDiagnostic", model) });
        }

        [Authorize(Roles = "Manager, Supervisor")]
        public IActionResult AddDocument(int id = 0)
        {
            DocumentTempViewModel entity = new DocumentTempViewModel()
            { 
                IdDescription = 0,
                Descriptions = _combosHelper.GetComboDocumentDescriptions()
            };
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Supervisor")]
        public async Task<IActionResult> AddDocument(int id, DocumentTempViewModel documentTempViewModel)
        {
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
                        CreatedOn = DateTime.Now
                    };
                    _context.Add(documentTemp);
                    await _context.SaveChangesAsync();
                }
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDocument", _context.DocumentsTemp.OrderByDescending(d => d.CreatedOn).ToList()) });
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddDocument", documentTempViewModel) });
        }

        [Authorize(Roles = "Manager, Supervisor")]
        public async Task<IActionResult> DeleteDiagnosticTemp(int? id)
        {
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

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDiagnostic", _context.DiagnosticsTemp.ToList()) });
        }

        [Authorize(Roles = "Manager, Supervisor")]
        public async Task<IActionResult> DeleteDocumentTemp(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DocumentTempEntity documentTemp = await _context.DocumentsTemp.FirstOrDefaultAsync(d => d.Id == id);
            if (documentTemp == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DocumentEntity document = await _context.Documents.FirstOrDefaultAsync(d => d.FileUrl == documentTemp.DocumentPath);

            _context.DocumentsTemp.Remove(documentTemp);
            if(document != null)
                _context.Documents.Remove(document);

            await _context.SaveChangesAsync();

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDocument", _context.DocumentsTemp.OrderByDescending(d => d.CreatedOn).ToList()) });
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

        [Authorize(Roles = "Manager, Supervisor")]
        public void DeleteDiagnosticsTemp()
        {
            //delete all DiagnosticsTemp
            IQueryable<DiagnosticTempEntity> list_to_delete = _context.DiagnosticsTemp;
            foreach (DiagnosticTempEntity item in list_to_delete)
            {
                _context.DiagnosticsTemp.Remove(item);
            }
            _context.SaveChanges();
        }

        [Authorize(Roles = "Manager, Supervisor")]
        public void SetDiagnosticsTemp(ClientEntity client)
        {
            IEnumerable<Client_Diagnostic> clientsDiagnostics = _context.Clients_Diagnostics
                                                                        .Include(cd => cd.Diagnostic)
                                                                        .Where(cd => cd.Client == client).ToList();

            if (clientsDiagnostics.Count() > 0)
            {
                DiagnosticTempEntity diagnostic;
                foreach (var item in clientsDiagnostics)
                {
                    diagnostic = new DiagnosticTempEntity
                    {
                        Code = item.Diagnostic.Code,
                        Description = item.Diagnostic.Description,
                        Principal = item.Principal
                    };
                    _context.Add(diagnostic);
                }
                _context.SaveChanges();
            }            
        }

        [Authorize(Roles = "Manager, Supervisor")]
        public void SetDocumentsTemp(ClientEntity client)
        {
            IEnumerable<DocumentEntity> documents = _context.Documents                                                            
                                                            .Where(d => d.Client == client)                                                            
                                                            .ToList();

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
                        CreatedOn = item.CreatedOn
                    };
                    _context.Add(document);
                }
                _context.SaveChanges();
            }
        }

        [Authorize(Roles = "Manager, Supervisor")]
        public void DeleteDocumentsTemp()
        {
            //delete all DiagnosticsTemp
            IQueryable<DocumentTempEntity> list_to_delete = _context.DocumentsTemp;
            foreach (DocumentTempEntity item in list_to_delete)
            {
                _context.DocumentsTemp.Remove(item);
            }
            _context.SaveChanges();
        }

        [Authorize(Roles = "Manager, Supervisor")]
        public async Task<IActionResult> DocumentForClient()
        {
            if (User.IsInRole("Admin"))
                return View(await _context.Clients
                                          .Include(c => c.Clinic)
                                          .Where(c => c.MTPs.Count == 0)
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
                                                              .Where(c => (c.Clinic.Id == clinic.Id))
                                                              .OrderBy(c => c.Name)
                                                              .ToListAsync();
                    //client = client.Where(c => c.MissingDoc != string.Empty).ToList();
                    return View(client);
                }
                else
                    return View(null);
            }
        }
    }
}