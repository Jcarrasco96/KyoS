using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    [Authorize(Roles = "Admin, Mannager, Supervisor")]
    public class ClientsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IImageHelper _imageHelper;

        public ClientsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
            _imageHelper = imageHelper;
        }
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
                return View(await _context.Clients.Include(c => c.Clinic).OrderBy(c => c.Clinic.Name).ToListAsync());
            else
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic == null)
                    return View(await _context.Clients.Include(c => c.Clinic).OrderBy(c => c.Clinic.Name).ToListAsync());

                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                if (clinic != null)
                    return View(await _context.Clients.Include(c => c.Clinic)
                                                      .Where(c => c.Clinic.Id == clinic.Id).OrderBy(c => c.Name).ToListAsync());
                else
                    return View(await _context.Clients.Include(c => c.Clinic).OrderBy(c => c.Clinic.Name).ToListAsync());
            }
        }
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
                        DiagnosticTemp = _context.DiagnosticsTemp
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
        public async Task<IActionResult> Create(ClientViewModel clientViewModel)
        {
            if (ModelState.IsValid)
            {
                ClientEntity client = await _context.Clients.FirstOrDefaultAsync(c => c.Name == clientViewModel.Name);
                if (client == null)
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
                else
                {
                    return RedirectToAction("Create", new { id = 2 });
                }
            }
            return View(clientViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ClientEntity clientEntity = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (clientEntity == null)
            {
                return NotFound();
            }

            _context.Clients.Remove(clientEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ClientEntity clientEntity = await _context.Clients
                                                      .Include(c => c.Clinic)
                                                      .Include(c => c.Doctor)
                                                      .Include(c => c.Psychiatrist)
                                                      .Include(c => c.Referred)
                                                      .Include(c => c.LegalGuardian)
                                                      .Include(c => c.EmergencyContact)
                                                      .FirstOrDefaultAsync(c => c.Id == id);
            if (clientEntity == null)
            {
                return NotFound();
            }

            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.UserName == User.Identity.Name);

            this.DeleteDiagnosticsTemp();
            this.SetDiagnosticsTemp(clientEntity);

            ClientViewModel clientViewModel = _converterHelper.ToClientViewModel(clientEntity, user_logged.Id);            

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

            return View(clientViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientViewModel clientViewModel)
        {
            if (id != clientViewModel.Id)
            {
                return NotFound();
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

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
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

        public async Task<IActionResult> ClientsWithoutMTP()
        {
            if (User.IsInRole("Admin"))
                return View(await _context.Clients
                                          .Include(c => c.Clinic)
                                          .Where(c => c.MTPs.Count == 0)
                                          .OrderBy(c => c.Clinic.Name)
                                          .ToListAsync());
            else
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic == null)
                    return View(null);

                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                if (clinic != null)
                    return View(await _context.Clients.Include(c => c.Clinic)
                                                      .Where(c => (c.Clinic.Id == clinic.Id && c.MTPs.Count == 0))
                                                      .OrderBy(c => c.Name)
                                                      .ToListAsync());
                else
                    return View(null);
            }
        }

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

        public async Task<IActionResult> DeleteDiagnosticTemp(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DiagnosticTempEntity diagnostic = await _context.DiagnosticsTemp.FirstOrDefaultAsync(d => d.Id == id);
            if (diagnostic == null)
            {
                return NotFound();
            }

            _context.DiagnosticsTemp.Remove(diagnostic);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

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
    }
}