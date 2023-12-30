using KyoS.Common.Enums;
using KyoS.Common.Helpers;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    public class IncidentReportController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IReportHelper _reportHelper;
        private readonly DataContext _context;

        public IncidentReportController(IUserHelper userHelper, IConverterHelper converterHelper, ICombosHelper combosHelper, IRenderHelper renderHelper, DataContext context, IReportHelper reportHelper)
        {
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _context = context;
            _renderHelper = renderHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Frontdesk, Documents_Assistant")]
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

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            else
            {
                if (User.IsInRole("Manager") || User.IsInRole("Supervisor") || User.IsInRole("Frontdesk"))
                    return View(await _context.Clients

                                              .Include(f => f.IncidentReport)
                                              .Include(f => f.Clinic)
                                              .ThenInclude(f => f.Setting)

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id
                                                       && n.IncidentReport.Count() > 0)
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());

                if (User.IsInRole("Facilitator"))
                {
                    FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

                    List<ClientEntity> clientList = await _context.Clients
                                                                  .Include(f => f.IncidentReport)
                                                                  .Include(f => f.Clinic)
                                                                  .ThenInclude(f => f.Setting)
                                                                  .Where(n => (n.IdFacilitatorGroup == facilitator.Id
                                                                           || n.IdFacilitatorPSR == facilitator.Id
                                                                           || n.IndividualTherapyFacilitator.Id == facilitator.Id)
                                                                           && n.IncidentReport.Count() > 0)
                                                                  .OrderBy(f => f.Name)
                                                                  .ToListAsync();

                    return View(clientList);
                }
                if (User.IsInRole("Documents_Assistant"))
                {
                    DocumentsAssistantEntity documentAssisstant = _context.DocumentsAssistant
                                                                          .FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

                    List<ClientEntity> clientList = await _context.Clients
                                                                  .Include(f => f.IncidentReport)
                                                                  .Include(f => f.Clinic)
                                                                  .ThenInclude(f => f.Setting)
                                                                  .Where(n => n.Bio.DocumentsAssistant.Id == documentAssisstant.Id
                                                                           && n.IncidentReport.Count() > 0)
                                                                  .OrderBy(f => f.Name)
                                                                  .ToListAsync();

                    return View(clientList);
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Facilitator, Documents_Assistant, Supervisor")]
        public IActionResult Create(int idClient = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IncidentReportViewModel model = new IncidentReportViewModel();
            ClientEntity client = _context.Clients
                                          .FirstOrDefault(n => n.Id == idClient);

            if (User.IsInRole("Facilitator"))
            {
                FacilitatorEntity facilitator = _context.Facilitators
                                                        .FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                if (user_logged.Clinic != null)
                {
                    model = new IncidentReportViewModel
                    {
                        IdClient = idClient,
                        Clients = _combosHelper.GetComboActiveClientsByClinic(user_logged.Clinic.Id),
                        Client = client,
                        Id = 0,
                        Facilitator = facilitator,
                        IdFacilitator = facilitator.Id,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Today,
                        AdmissionFor = user_logged.FullName,
                        DateIncident = DateTime.Today,
                        DateReport = DateTime.Today,
                        DateSignatureEmployee = DateTime.Today,
                        DescriptionIncident = string.Empty,
                        IdDocumentAssisstant = 0,
                        IdSupervisor = 0,
                        Injured = false,
                        Injured_Description = string.Empty,
                        Location = string.Empty,
                        TimeIncident = DateTime.Today,
                        Witnesses = false,
                        Witnesses_Contact = string.Empty

                    };

                    return View(model);
                }
            }
            if (User.IsInRole("Documents_Assistant"))
            {
                DocumentsAssistantEntity documentsAssistant = _context.DocumentsAssistant
                                                                      .FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                if (user_logged.Clinic != null)
                {
                    model = new IncidentReportViewModel
                    {
                        IdClient = idClient,
                        Client = client,
                        Id = 0,
                        IdFacilitator = 0,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Today,
                        DocumentAssisstant = documentsAssistant,
                        IdDocumentAssisstant = documentsAssistant.Id,
                        AdmissionFor = user_logged.FullName,
                        DateIncident = DateTime.Today,
                        DateReport = DateTime.Today,
                        DateSignatureEmployee = DateTime.Today,
                        DescriptionIncident = string.Empty,
                        IdSupervisor = 0,
                        Injured = false,
                        Injured_Description = string.Empty,
                        Location = string.Empty,
                        Supervisor = new SupervisorEntity(),
                        TimeIncident = DateTime.Today,
                        Witnesses = false,
                        Witnesses_Contact = string.Empty

                    };

                    return View(model);
                }
            }
           
            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Facilitator, Documents_Assistant")]
        public async Task<IActionResult> Create(IncidentReportViewModel IncidentViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IncidentReportEntity IncidentReprot = _context.IncidentReport.Find(IncidentViewModel.Id);
                if (IncidentReprot == null)
                {
                    IncidentReprot = await _converterHelper.ToIncidentReportEntity(IncidentViewModel, true, user_logged.UserName);
                   
                    _context.IncidentReport.Add(IncidentReprot);

                    try
                    {
                        await _context.SaveChangesAsync();

                        return RedirectToAction("Index");
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Safety Plan.");

                    return RedirectToAction("Index");
                }


            }
            else
            {
                IncidentViewModel.Client = await _context.Clients
                                                         .FirstOrDefaultAsync(n => n.Id == IncidentViewModel.IdClient);
                IncidentViewModel.Facilitator = await _context.Facilitators
                                                              .FirstOrDefaultAsync(n => n.Id == IncidentViewModel.IdFacilitator);
                IncidentViewModel.DocumentAssisstant = await _context.DocumentsAssistant
                                                                     .FirstOrDefaultAsync(n => n.Id == IncidentViewModel.IdDocumentAssisstant);
                return View(IncidentViewModel);
            }

            return RedirectToAction("Create", "IncidentReport");

        }

        [Authorize(Roles = "Facilitator, Supervisor, Documents_Assistant")]
        public IActionResult Edit(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IncidentReportEntity incident = _context.IncidentReport
                                                    .Include(n => n.Client)
                                                    .Include(n => n.Facilitator)
                                                    .Include(n => n.Supervisor)
                                                    .Include(n => n.DocumentAssisstant)
                                                    .FirstOrDefault(n => n.Id == id);

            IncidentReportViewModel model = _converterHelper.ToIncidentReportViewModel(incident);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Facilitator, Supervisor, Documents_Assistant")]
        public async Task<IActionResult> Edit(IncidentReportViewModel incidentViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                IncidentReportEntity incidentEntity = await _converterHelper.ToIncidentReportEntity(incidentViewModel, false, user_logged.UserName);
                incidentEntity.Client = null;
                _context.IncidentReport.Update(incidentEntity);

                try
                {
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index");
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }
            else
            {
                incidentViewModel.Client = _context.Clients
                                                 .FirstOrDefault(n => n.Id == incidentViewModel.IdClient);

                incidentViewModel.Facilitator = await _context.Facilitators.FirstOrDefaultAsync(n => n.Id == incidentViewModel.IdFacilitator);
                incidentViewModel.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(n => n.Id == incidentViewModel.IdSupervisor);
                incidentViewModel.DocumentAssisstant = await _context.DocumentsAssistant.FirstOrDefaultAsync(n => n.Id == incidentViewModel.IdDocumentAssisstant);


                return View(incidentViewModel);
            }

            return RedirectToAction("Edit", "SafetyPlan");

        }

        [Authorize(Roles = "Manager, Facilitator, Supervisor, Documents_Assistant, Frontdesk")]
        public IActionResult EditReadOnly(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IncidentReportEntity incidentReport = _context.IncidentReport
                                                          .Include(n => n.Client)
                                                          .Include(n => n.Facilitator)
                                                          .Include(n => n.Supervisor)
                                                          .Include(n => n.DocumentAssisstant)
                                                          .FirstOrDefault(n => n.Id == id);

            IncidentReportViewModel model = _converterHelper.ToIncidentReportViewModel(incidentReport);

            return View(model);
        }

        [Authorize(Roles = "Facilitator, Documents_Assistant")]
        public IActionResult SelectClient()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            IncidentReportViewModel model = new IncidentReportViewModel()
            {
                IdClient = 0,
                Clients = _combosHelper.GetComboActiveClientsByClinic(user_logged.Clinic.Id)

            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Facilitator, Documents_Assistant")]
        public async Task<IActionResult> SelectClient(IncidentReportViewModel incidentReportViewModel)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(u => u.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (incidentReportViewModel.IdClient > 0)
            {
                return RedirectToAction("Create", "IncidentReport", new { idClient = incidentReportViewModel.IdClient });
            }
            else
            {
                return RedirectToAction("SelectClient");
            }

        }


    }
}
