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
    public class SafetyPlanController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IReportHelper _reportHelper;
        private readonly DataContext _context;

        public SafetyPlanController(IUserHelper userHelper, IConverterHelper converterHelper, ICombosHelper combosHelper, IRenderHelper renderHelper, DataContext context, IReportHelper reportHelper)
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

                                              .Include(f => f.SafetyPlanList)
                                              .Include(f => f.Clinic)
                                              .ThenInclude(f => f.Setting)

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id
                                                       && n.SafetyPlanList.Count() > 0)
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());

                if (User.IsInRole("Facilitator"))
                {
                    FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

                    List<ClientEntity> clientList = await _context.Clients
                                                                  .Include(f => f.SafetyPlanList)
                                                                  .Include(f => f.Clinic)
                                                                  .ThenInclude(f => f.Setting)
                                                                  .Where(n => (n.SafetyPlanList.Where(m => m.Facilitator.Id == facilitator.Id).Count() > 0
                                                                           || n.IdFacilitatorGroup == facilitator.Id
                                                                           || n.IdFacilitatorPSR == facilitator.Id
                                                                           || n.IndividualTherapyFacilitator.Id == facilitator.Id)
                                                                           && n.SafetyPlanList.Count() > 0)
                                                                  .OrderBy(f => f.Name)
                                                                  .ToListAsync();

                    return View(clientList);
                }
                if (User.IsInRole("Documents_Assistant"))
                {
                    DocumentsAssistantEntity documentAssisstant = _context.DocumentsAssistant
                                                                          .FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

                    List<ClientEntity> clientList = await _context.Clients
                                                                  .Include(f => f.SafetyPlanList)
                                                                  .Include(f => f.Clinic)
                                                                  .ThenInclude(f => f.Setting)
                                                                  .Where(n => n.Bio.DocumentsAssistant.Id == documentAssisstant.Id
                                                                           && n.SafetyPlanList.Count() > 0)
                                                                  .OrderBy(f => f.Name)
                                                                  .ToListAsync();

                    return View(clientList);
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Facilitator, Documents_Assistant")]
        public IActionResult Create(int id = 0, int origin = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            SafetyPlanViewModel model;
            ClientEntity client = _context.Clients
                                          .FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Facilitator"))
            {
                FacilitatorEntity facilitator = _context.Facilitators
                                                        .FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                if (user_logged.Clinic != null)
                {
                    model = new SafetyPlanViewModel
                    {
                        IdClient = id,
                        Client = client,
                        AdviceIwould = string.Empty,
                        Id = 0,
                        Client_FK = client.Id,
                        DateSignatureClient = DateTime.Today,
                        DateSignatureFacilitator = DateTime.Today,
                        Documents = true,
                        Facilitator = facilitator,
                        IdFacilitator = facilitator.Id,
                        PeopleIcanCall = string.Empty,
                        ThingsThat = string.Empty,
                        WarningSignsOfCrisis = string.Empty,
                        WaysToDistract = string.Empty,
                        WaysToKeepmyselfSafe = string.Empty,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Today,
                        DateDocument = DateTime.Today

                    };
                    ViewData["origin"] = origin;
                    return View(model);
                }
            }
            if (User.IsInRole("Documents_Assistant"))
            {
                DocumentsAssistantEntity documentsAssistant = _context.DocumentsAssistant
                                                                      .FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                if (user_logged.Clinic != null)
                {
                    model = new SafetyPlanViewModel
                    {
                        IdClient = id,
                        Client = client,
                        AdviceIwould = string.Empty,
                        Id = 0,
                        Client_FK = client.Id,
                        DateSignatureClient = DateTime.Today,
                        DateSignatureFacilitator = DateTime.Today,
                        Documents = true,
                        IdFacilitator = 0,
                        PeopleIcanCall = string.Empty,
                        ThingsThat = string.Empty,
                        WarningSignsOfCrisis = string.Empty,
                        WaysToDistract = string.Empty,
                        WaysToKeepmyselfSafe = string.Empty,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Today,
                        DateDocument = DateTime.Today,
                        DocumentAssisstant = documentsAssistant,
                        IdDocumentAssisstant = documentsAssistant.Id

                    };
                    ViewData["origin"] = origin;
                    return View(model);
                }
            }
            ViewData["origin"] = origin;
            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Facilitator, Supervisor, Documents_Assistant")]
        public async Task<IActionResult> Create(SafetyPlanViewModel safetyViewModel, int origin = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                SafetyPlanEntity safetyPlanEntity = _context.SafetyPlan.Find(safetyViewModel.Id);
                if (safetyPlanEntity == null)
                {
                    safetyPlanEntity = await _converterHelper.ToSafetyPlanEntity(safetyViewModel, true, user_logged.UserName);
                   // safetyPlanEntity.Client = null;
                    _context.SafetyPlan.Add(safetyPlanEntity);

                    try
                    {
                        await _context.SaveChangesAsync();

                        if (origin == 0)
                        {
                            return RedirectToAction("ClientswithoutSafetyPlan");
                        }
                        else
                        {
                            return RedirectToAction("Index");
                        }
                        
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Safety Plan.");

                    return RedirectToAction("ClientswithoutSafetyPlan");
                }


            }
            else
            {
                SafetyPlanViewModel model;
                model = new SafetyPlanViewModel
                {
                    IdClient = safetyViewModel.IdClient,
                    Client = _context.Clients
                                     .FirstOrDefault(n => n.Id == safetyViewModel.IdClient),

                    Client_FK = safetyViewModel.IdClient,
                    AdviceIwould = safetyViewModel.AdviceIwould,
                    DateSignatureClient = safetyViewModel.DateSignatureClient,
                    DateSignatureFacilitator = safetyViewModel.DateSignatureFacilitator,
                    Documents = safetyViewModel.Documents,
                    Facilitator = await _context.Facilitators.FirstOrDefaultAsync(n => n.Id == safetyViewModel.IdFacilitator),
                    PeopleIcanCall = safetyViewModel.PeopleIcanCall,
                    ThingsThat = safetyViewModel.ThingsThat,
                    WarningSignsOfCrisis = safetyViewModel.WarningSignsOfCrisis,
                    WaysToDistract = safetyViewModel.WaysToDistract,
                    WaysToKeepmyselfSafe = safetyViewModel.WaysToKeepmyselfSafe

                };
                ViewData["origin"] = origin;
                return View(model);
            }

            return RedirectToAction("Create", "SafetyPlan");

        }

        [Authorize(Roles = "Facilitator, Supervisor, Documents_Assistant")]
        public IActionResult Edit(int id = 0, int origin = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            SafetyPlanEntity safetyPlan = _context.SafetyPlan
                                                  .Include(n => n.Client)
                                                  .Include(n => n.Facilitator)
                                                  .Include(n => n.Supervisor)
                                                  .FirstOrDefault(n => n.Id == id);

            SafetyPlanViewModel model = _converterHelper.ToSafetyPlanViewModel(safetyPlan);

            ViewData["origin"] = origin;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Facilitator, , Documents_Assistant")]
        public async Task<IActionResult> Edit(SafetyPlanViewModel safetyViewModel, int origin = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                SafetyPlanEntity safetyPlanEntity = await _converterHelper.ToSafetyPlanEntity(safetyViewModel, false, user_logged.UserName);
                safetyPlanEntity.Client = null;
                _context.SafetyPlan.Update(safetyPlanEntity);

                try
                {
                    await _context.SaveChangesAsync();
                    if (origin == 0)
                    {
                        return RedirectToAction("EditionSafetyPlan");
                    }
                    if (origin == 1)
                    {
                        return RedirectToAction("Index");
                    }
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }
            else
            {
                safetyViewModel.Client = _context.Clients
                                                 .FirstOrDefault(n => n.Id == safetyViewModel.IdClient);

                safetyViewModel.Facilitator = await _context.Facilitators.FirstOrDefaultAsync(n => n.Id == safetyViewModel.IdFacilitator);
                safetyViewModel.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(n => n.Id == safetyViewModel.IdSupervisor);
                    

                return View(safetyViewModel);
            }

            return RedirectToAction("Edit", "SafetyPlan");

        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Frontdesk, Documents_Assistant")]
        public async Task<IActionResult> ClientswithoutSafetyPlan(int idError = 0, int all = 0)
        {
            UserEntity user_logged = await _context.Users

                                                  .Include(u => u.Clinic)
                                                  .ThenInclude(c => c.Setting)

                                                  .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<ClientEntity> clientlist = new List<ClientEntity>();

            if (User.IsInRole("Manager") || User.IsInRole("Supervisor") || User.IsInRole("Frontdesk"))
            {
                if (all == 0)
                {
                    clientlist = await _context.Clients
                                               .Include(n => n.IndividualTherapyFacilitator)
                                               .Where(n => n.SafetyPlanList.Count() == 0 
                                                        && n.OnlyTCM == false
                                                        && n.Clinic.Id == user_logged.Clinic.Id)
                                               .AsSplitQuery()
                                               .ToListAsync();

                    return View(clientlist);
                }
                else
                {
                    if (all == 1)
                    {
                        clientlist = await _context.Clients
                                                   .Include(n => n.IndividualTherapyFacilitator)
                                                   .Where(n => n.SafetyPlanList.Count() == 0
                                                            && n.Status == StatusType.Open
                                                            && n.OnlyTCM == false
                                                            && n.Clinic.Id == user_logged.Clinic.Id)
                                                   .AsSplitQuery()
                                                   .ToListAsync();

                        return View(clientlist);
                    }
                    else
                    {
                        clientlist = await _context.Clients
                                                 .Include(n => n.IndividualTherapyFacilitator)
                                                 .Where(n => n.SafetyPlanList.Count() == 0
                                                          && n.Status == StatusType.Close
                                                          && n.OnlyTCM == false
                                                          && n.Clinic.Id == user_logged.Clinic.Id)
                                                 .AsSplitQuery()
                                                 .ToListAsync();

                        return View(clientlist);
                    }
                }
               
            }
            if (User.IsInRole("Facilitator"))
            {
                FacilitatorEntity facilitator = _context.Facilitators
                                                        .FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                if (all == 0)
                {
                    clientlist = await _context.Clients
                                           .Include(n => n.IndividualTherapyFacilitator)
                                           .Where(n => n.SafetyPlanList.Count() == 0
                                                    && n.OnlyTCM == false
                                                    && n.IndividualTherapyFacilitator.Id == facilitator.Id)
                                           .AsSplitQuery()
                                           .ToListAsync();

                    return View(clientlist);
                }
                else
                {
                    if (all == 1)
                    {
                        clientlist = await _context.Clients
                                               .Include(n => n.IndividualTherapyFacilitator)
                                               .Where(n => n.SafetyPlanList.Count() == 0
                                                        && n.OnlyTCM == false
                                                        && n.IndividualTherapyFacilitator.Id == facilitator.Id
                                                        && n.Status == StatusType.Open)
                                               .AsSplitQuery()
                                               .ToListAsync();

                        return View(clientlist);
                    }
                    else
                    {
                        clientlist = await _context.Clients
                                                   .Include(n => n.IndividualTherapyFacilitator)
                                                   .Where(n => n.SafetyPlanList.Count() == 0
                                                            && n.OnlyTCM == false
                                                            && n.IndividualTherapyFacilitator.Id == facilitator.Id
                                                            && n.Status == StatusType.Close)
                                                   .AsSplitQuery()
                                                   .ToListAsync();

                        return View(clientlist);
                    }
                }
                
            }
            if (User.IsInRole("Documents_Assistant"))
            {
                DocumentsAssistantEntity documentAssisstant = _context.DocumentsAssistant
                                                                      .FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                if (all == 0)
                {
                    clientlist = await _context.Clients
                                               .Include(n => n.IndividualTherapyFacilitator)
                                               .Where(n => n.SafetyPlanList.Count() == 0
                                                        && n.OnlyTCM == false
                                                        && n.Bio.DocumentsAssistant.Id == documentAssisstant.Id)
                                               .AsSplitQuery()
                                               .ToListAsync();

                    return View(clientlist);
                }
                else
                {
                    if (all == 1)
                    {
                        clientlist = await _context.Clients
                                                   .Include(n => n.IndividualTherapyFacilitator)
                                                   .Where(n => n.SafetyPlanList.Count() == 0
                                                            && n.OnlyTCM == false
                                                            && n.Bio.DocumentsAssistant.Id == documentAssisstant.Id
                                                            && n.Status == StatusType.Open)
                                                   .AsSplitQuery()
                                                   .ToListAsync();

                        return View(clientlist);
                    }
                    else
                    {
                        clientlist = await _context.Clients
                                                   .Include(n => n.IndividualTherapyFacilitator)
                                                   .Where(n => n.SafetyPlanList.Count() == 0
                                                            && n.OnlyTCM == false
                                                            && n.Bio.DocumentsAssistant.Id == documentAssisstant.Id
                                                            && n.Status == StatusType.Close)
                                                   .AsSplitQuery()
                                                   .ToListAsync();

                        return View(clientlist);
                    }
                }
                
            }

            return View(clientlist);
        }

        [Authorize(Roles = "Supervisor, Manager, Facilitator, Documents_Assistant, Frontdesk")]
        public async Task<IActionResult> EditionSafetyPlan(int idError = 0)
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
                    if (User.IsInRole("Manager") || User.IsInRole("Supervisor") || User.IsInRole("Frontdesk"))
                    {
                        return View(await _context.SafetyPlan
                                                  .Include(f => f.Client)
                                                  .ThenInclude(f => f.Clinic)
                                                  .Include(f => f.Facilitator)
                                                  .Include(f => f.Supervisor)

                                                  .Where(f => (f.Client.Clinic.Id == clinic.Id)
                                                            && f.Status == SafetyPlanStatus.Edition)
                                                  .ToListAsync());
                    }
                    else
                    {
                        return View(await _context.SafetyPlan
                                                  .Include(f => f.Client)
                                                  .ThenInclude(f => f.Clinic)
                                                  .Include(f => f.Facilitator)
                                                  .Include(f => f.Supervisor)
                                                  .Where(f => (f.Client.Clinic.Id == clinic.Id)
                                                            && f.Status == SafetyPlanStatus.Edition
                                                            && f.CreatedBy == user_logged.UserName)
                                                  .ToListAsync());
                    }

                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> FinishEditingSafetyPlan(int id)
        {
            SafetyPlanEntity safetyplan = await _context.SafetyPlan.FirstOrDefaultAsync(n => n.Id == id);
            if (User.IsInRole("Supervisor"))
            {
                safetyplan.Status = SafetyPlanStatus.Approved;
            }
            else
            {
                safetyplan.Status = SafetyPlanStatus.Pending;  //the FARS no have approved for Supervisor
            }

            _context.Update(safetyplan);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(EditionSafetyPlan));
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

            SafetyPlanEntity safetyPlan = _context.SafetyPlan
                                                  .Include(n => n.Client)
                                                  .Include(n => n.Facilitator)
                                                  .Include(n => n.Supervisor)
                                                  .FirstOrDefault(n => n.Id == id);

            SafetyPlanViewModel model = _converterHelper.ToSafetyPlanViewModel(safetyPlan);

            return View(model);
        }

        [Authorize(Roles = "Manager, Facilitator, Supervisor, Documents_Assistant")]
        public IActionResult PrintSafetyPlan(int id)
        {
            SafetyPlanEntity entity = _context.SafetyPlan

                                              .Include(s => s.Client)
                                              .ThenInclude(s => s.Clinic)           
                                              
                                              .Include(s => s.Facilitator)

                                              .Include(s => s.DocumentAssisstant)

                                              .Include(s => s.Supervisor)                                       

                                              .FirstOrDefault(s => (s.Id == id));
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            
            Stream stream = _reportHelper.SafetyPlanReport(entity);
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);         
        }

        [Authorize(Roles = "Supervisor, Manager, Facilitator, Documents_Assistant, Frontdesk")]
        public async Task<IActionResult> PendingSafetyPlan(int idError = 0)
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
                    if (User.IsInRole("Manager") || User.IsInRole("Supervisor") || User.IsInRole("Frontdesk"))
                    {
                        return View(await _context.SafetyPlan
                                                  .Include(f => f.Client)
                                                  .ThenInclude(f => f.Clinic)
                                                  .Include(f => f.Facilitator)
                                                  .Include(f => f.Supervisor)

                                                  .Where(f => (f.Client.Clinic.Id == clinic.Id)
                                                            && f.Status == SafetyPlanStatus.Pending)
                                                  .ToListAsync());
                    }
                    else
                    {
                        return View(await _context.SafetyPlan
                                                  .Include(f => f.Client)
                                                  .ThenInclude(f => f.Clinic)
                                                  .Include(f => f.Facilitator)
                                                  .Include(f => f.Supervisor)
                                                  .Where(f => (f.Client.Clinic.Id == clinic.Id)
                                                            && f.Status == SafetyPlanStatus.Pending
                                                            && f.CreatedBy == user_logged.UserName)
                                                  .ToListAsync());
                    }

                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor")]
        public IActionResult Approve(int id = 0)
        {
            if (id == 0)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            SafetyPlanEntity safetyPlan = _context.SafetyPlan
                                                  .Include(n => n.Client)
                                                  .Include(n => n.Facilitator)
                                                  .Include(n => n.Supervisor)
                                                  .FirstOrDefault(n => n.Id == id);

            SafetyPlanViewModel model = _converterHelper.ToSafetyPlanViewModel(safetyPlan);

            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Approve(SafetyPlanViewModel safetyViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                SafetyPlanEntity safetyPlanEntity = await _converterHelper.ToSafetyPlanEntity(safetyViewModel, false, user_logged.UserName);
                safetyPlanEntity.Client = null;
                safetyPlanEntity.Status = SafetyPlanStatus.Approved;
                _context.SafetyPlan.Update(safetyPlanEntity);

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("PendingSafetyPlan");
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }
            else
            {
                safetyViewModel.Client = _context.Clients
                                                 .FirstOrDefault(n => n.Id == safetyViewModel.IdClient);

                safetyViewModel.Facilitator = await _context.Facilitators.FirstOrDefaultAsync(n => n.Id == safetyViewModel.IdFacilitator);
                safetyViewModel.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(n => n.Id == safetyViewModel.IdSupervisor);


                return View(safetyViewModel);
            }

            return RedirectToAction("Approve", "SafetyPlan");

        }

    }
}
