using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using KyoS.Common.Helpers;

namespace KyoS.Web.Controllers
{
    public class DischargeController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IReportHelper _reportHelper;
        private readonly DataContext _context;

        public DischargeController(IUserHelper userHelper, IConverterHelper converterHelper, ICombosHelper combosHelper, IRenderHelper renderHelper, DataContext context, IReportHelper reportHelper)
        {
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _context = context;
            _renderHelper = renderHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
        }
        
        [Authorize(Roles = "Manager, Supervisor, Facilitator, Frontdesk")]
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
                if (User.IsInRole("Manager") || User.IsInRole("Supervisor"))
                    return View(await _context.Clients

                                              .Include(f => f.DischargeList)
                                              .Include(f => f.Clients_Diagnostics)

                                              .Where(n => (n.Clinic.Id == user_logged.Clinic.Id && n.Status == StatusType.Close)
                                                     || (n.Clinic.Id == user_logged.Clinic.Id && n.Status == StatusType.Open
                                                           && n.DischargeList.Count() > 0))
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());

                if (User.IsInRole("Facilitator"))
                {
                    FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

                    List<ClientEntity> ClientList = await _context.Clients
                                                                  .Include(f => f.DischargeList)
                                                                  .Include(f => f.Clients_Diagnostics)

                                                                  .Where(n => (n.Clinic.Id == user_logged.Clinic.Id && n.Status == StatusType.Close
                                                                        && (n.IdFacilitatorPSR == facilitator.Id || n.IndividualTherapyFacilitator.Id == facilitator.Id || n.IdFacilitatorGroup == facilitator.Id))
                                                                        || (n.Clinic.Id == user_logged.Clinic.Id && n.Status == StatusType.Open
                                                                        && (n.DischargeList.Count() > 0)
                                                                        && (n.IdFacilitatorPSR == facilitator.Id || n.IndividualTherapyFacilitator.Id == facilitator.Id || n.IdFacilitatorGroup == facilitator.Id)))
                                                                  .OrderBy(f => f.Name)
                                                                  .ToListAsync();
                                                                 
                    return View(ClientList);
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Facilitator")]
        public IActionResult Create(int id = 0, ServiceType service = 0, int origin = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            DischargeViewModel model;
            ClientEntity client = _context.Clients

                                        .Include(n => n.MedicationList)

                                        .Include(c => c.Clients_Diagnostics)
                                        .ThenInclude(cd => cd.Diagnostic)

                                        .FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Facilitator"))
            {
                if (user_logged.Clinic != null)
                {
                    model = new DischargeViewModel
                    {
                        IdClient = id,
                        Client = client,
                        AdmissionedFor = user_logged.FullName,
                        Administrative = false,
                        ClientTransferred = false,
                        ClinicalCoherente = false,
                        ClinicalIncoherente = false,
                        ClinicalInRemission = false,
                        ClinicalStable = false,
                        ClinicalUnpredictable = false,
                        ClinicalUnstable = false,
                        CompletedTreatment = false,
                        DateDischarge = DateTime.Now,
                        DateReport = DateTime.Now,
                        DischargeDiagnosis = "",
                        LeftBefore = false,
                        //Messages = ,
                        NonCompliant = false,
                        Other = false,
                        Other_Explain = "",
                        Planned = false,
                        PrognosisFair = false,
                        PrognosisGood = false,
                        PrognosisGuarded = false,
                        PrognosisPoor = false,
                        ProgramClubHouse = false,
                        ProgramGroup = false,
                        ProgramInd = false,
                        ProgramPSR = false,
                        ReferralsTo = "",
                        Termination = false,
                        DateSignatureEmployee = DateTime.Now,
                        DateSignaturePerson = DateTime.Now,
                        DateSignatureSupervisor = DateTime.Now,
                        TypeService = service,
                        Origin = origin,
                        DateAdmissionService = client.AdmisionDate
                    };
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    if (service == ServiceType.PSR)
                    {
                        model.ProgramPSR = true;
                    }
                    if (service == ServiceType.Individual)
                    {
                        model.ProgramInd = true;
                    }
                    if (service == ServiceType.Group)
                    {
                        model.ProgramGroup = true;
                    }

                    return View(model);
                }
            }

            model = new DischargeViewModel
            {
                IdClient = id,
                Client = client,
                AdmissionedFor = user_logged.FullName,
                Client_FK = id,
                DateDischarge = DateTime.Now,
                DateReport = DateTime.Now,
                Id = 0,
                Administrative = false,
                ClientTransferred = false,
                ClinicalCoherente = false,
                ClinicalIncoherente = false,
                ClinicalInRemission = false,
                ClinicalStable = false,
                ClinicalUnpredictable = false,
                ClinicalUnstable = false,
                CompletedTreatment = false,
                DischargeDiagnosis = "",
                LeftBefore = false,
                //Messages = "",
                NonCompliant = false,
                Other = false,
                Other_Explain = "",
                Planned = false,
                PrognosisFair = false,
                PrognosisGood = false,
                PrognosisGuarded = false,
                PrognosisPoor = false,
                ProgramClubHouse = false,
                ProgramGroup = false,
                ProgramInd = false,
                ProgramPSR = false,
                ReferralsTo = "",
                Termination = false,
                DateSignatureEmployee = DateTime.Now,
                DateSignaturePerson = DateTime.Now,
                DateSignatureSupervisor = DateTime.Now,
                TypeService = service,
                Origin = origin,
                DateAdmissionService = client.AdmisionDate
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> Create(DischargeViewModel DischargeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if ((!ModelState.IsValid) || (DischargeViewModel.ProgramPSR == false && DischargeViewModel.ProgramInd == false
                && DischargeViewModel.ProgramGroup == false && DischargeViewModel.ProgramClubHouse == false))
            {
                DischargeViewModel model;
                model = new DischargeViewModel
                {
                    IdClient = DischargeViewModel.IdClient,
                    Client = _context.Clients
                                     .Include(n => n.MedicationList)
                                     .Include(c => c.Clients_Diagnostics)
                                     .ThenInclude(cd => cd.Diagnostic)
                                     .FirstOrDefault(n => n.Id == DischargeViewModel.IdClient),
                    
                    AdmissionedFor = user_logged.FullName,
                    DateDischarge = DischargeViewModel.DateDischarge,
                    DateReport = DischargeViewModel.DateReport,
                    Id = DischargeViewModel.Id,
                    Administrative = DischargeViewModel.Administrative,
                    ClientTransferred = DischargeViewModel.ClientTransferred,
                    ClinicalCoherente = DischargeViewModel.ClinicalCoherente,
                    ClinicalIncoherente = DischargeViewModel.ClinicalIncoherente,
                    ClinicalInRemission = DischargeViewModel.ClinicalInRemission,
                    ClinicalStable = DischargeViewModel.ClinicalStable,
                    ClinicalUnpredictable = DischargeViewModel.ClinicalUnpredictable,
                    ClinicalUnstable = DischargeViewModel.ClinicalUnstable,
                    CompletedTreatment = DischargeViewModel.CompletedTreatment,
                    DischargeDiagnosis = DischargeViewModel.DischargeDiagnosis,
                    LeftBefore = DischargeViewModel.LeftBefore,
                    Messages = DischargeViewModel.Messages,
                    NonCompliant = DischargeViewModel.NonCompliant,
                    Other = DischargeViewModel.Other,
                    Other_Explain = DischargeViewModel.Other_Explain,
                    Planned = DischargeViewModel.Planned,
                    PrognosisFair = DischargeViewModel.PrognosisFair,
                    PrognosisGood = DischargeViewModel.PrognosisGood,
                    PrognosisGuarded = DischargeViewModel.PrognosisGuarded,
                    PrognosisPoor = DischargeViewModel.PrognosisPoor,
                    ProgramClubHouse = DischargeViewModel.ProgramClubHouse,
                    ProgramGroup = DischargeViewModel.ProgramGroup,
                    ProgramInd = DischargeViewModel.ProgramInd,
                    ProgramPSR = DischargeViewModel.ProgramPSR,
                    ReferralsTo = DischargeViewModel.ReferralsTo,
                    Termination = DischargeViewModel.Termination,
                    DateSignatureEmployee = DischargeViewModel.DateSignatureEmployee,
                    DateSignaturePerson = DischargeViewModel.DateSignaturePerson,
                    DateSignatureSupervisor = DischargeViewModel.DateSignatureSupervisor,
                    Status = DischargeStatus.Edition,
                    TypeService = DischargeViewModel.TypeService,
                    DateAdmissionService = DischargeViewModel.DateAdmissionService
                };

                if ((ModelState.IsValid) || (DischargeViewModel.ProgramPSR == false && DischargeViewModel.ProgramInd == false
                    && DischargeViewModel.ProgramGroup == false && DischargeViewModel.ProgramClubHouse == false))
                {
                    ModelState.AddModelError(string.Empty, "You must select the type of therapy in this discharge.");
                }
                return View(model);
                
            }
            else
            {
                DischargeEntity DischargeEntity = _context.Discharge.Find(DischargeViewModel.Id);
                if (DischargeEntity == null)
                {
                    DischargeEntity = await _converterHelper.ToDischargeEntity(DischargeViewModel, true, user_logged.UserName);
                    _context.Discharge.Add(DischargeEntity);

                    // update DateResolved in all Objective for this services
                    
                    List<ObjetiveEntity> listObjective = new List<ObjetiveEntity>();
                    List<ObjetiveEntity> listObjective_Salida = new List<ObjetiveEntity>();
                    listObjective = _context.Objetives
                                            .Where(n => n.Goal.MTP.Client.Id == DischargeEntity.Client_FK
                                                     && n.Goal.Service == DischargeEntity.TypeService)
                                            .ToList();
                    if (listObjective.Count() > 0)
                    {
                        foreach (var item in listObjective)
                        {
                            item.DateResolved = DischargeEntity.DateDischarge;
                            _context.Update(item);
                        }
                        
                    }
                    try
                    {
                        await _context.SaveChangesAsync();

                        if (DischargeViewModel.Origin == 1)
                        {
                            return RedirectToAction("ClientswithoutDischarge");                            
                        }
                        if (DischargeViewModel.Origin == 2)
                        {
                            return RedirectToAction("DischargeOfService");
                        }
                        return RedirectToAction("Index");
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Discharge.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", DischargeViewModel) });
                }
            }

            return RedirectToAction("Create", "Discharge");
            
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public IActionResult Edit(int id = 0, int origin = 0)
        {
            DischargeViewModel model;

            if (User.IsInRole("Supervisor") || User.IsInRole("Facilitator"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    DischargeEntity Discharge = _context.Discharge
                                                        .Include(m => m.Client)
                                                        .ThenInclude(m => m.MedicationList)

                                                        .Include(d => d.Client)
                                                        .ThenInclude(c => c.Clients_Diagnostics)                                                        
                                                        .ThenInclude(cd => cd.Diagnostic)

                                                        .FirstOrDefault(m => m.Id == id);
                    if (Discharge == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {
                        model = _converterHelper.ToDischargeViewModel(Discharge);
                        model.Origin = origin;
                        return View(model);
                    }
                }
            }

            model = new DischargeViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> Edit(DischargeViewModel dischargeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {

                if (dischargeViewModel.ProgramPSR == false && dischargeViewModel.ProgramInd == false
                   && dischargeViewModel.ProgramGroup == false && dischargeViewModel.ProgramClubHouse == false)
                {
                    ModelState.AddModelError(string.Empty, "You must select the type of therapy in this discharge.");
                    dischargeViewModel.Client = _context.Clients
                                                        .Include(n => n.MedicationList)
                                                        .Include(c => c.Clients_Diagnostics)
                                                        .ThenInclude(cd => cd.Diagnostic)
                                                        .FirstOrDefault(n => n.Id == dischargeViewModel.IdClient);
                    return View(dischargeViewModel);
                }
                

                DischargeEntity dischargeEntity = await _converterHelper.ToDischargeEntity(dischargeViewModel, false, user_logged.Id);
                _context.Discharge.Update(dischargeEntity);
                try
                {
                    List<MessageEntity> messages = dischargeEntity.Messages.Where(m => (m.Status == MessageStatus.NotRead && m.Notification == false)).ToList();
                    //todos los mensajes no leidos que tiene el Workday_Client de la nota los pongo como leidos
                    foreach (MessageEntity value in messages)
                    {
                        value.Status = MessageStatus.Read;
                        value.DateRead = DateTime.Now;
                        _context.Update(value);

                        //I generate a notification to supervisor
                        MessageEntity notification = new MessageEntity
                        {
                            Workday_Client = null,
                            FarsForm = null,
                            MTPReview = null,
                            Addendum = null,
                            Discharge = dischargeEntity,
                            Title = "Update on reviewed discharge",
                            Text = $"The discharge document of {dischargeEntity.Client.Name} that was discharged on {dischargeEntity.DateDischarge.ToShortDateString()} was rectified",
                            From = value.To,
                            To = value.From,
                            DateCreated = DateTime.Now,
                            Status = MessageStatus.NotRead,
                            Notification = true
                        };
                        _context.Add(notification);
                    }

                    await _context.SaveChangesAsync();

                    if (dischargeViewModel.Origin == 1)
                    {
                        return RedirectToAction("DischargeInEdit");
                    }
                    if (dischargeViewModel.Origin == 2)
                    {
                        return RedirectToAction("PendingDischarge");
                    }
                    if (dischargeViewModel.Origin == 3)
                    {
                        return RedirectToAction("MessagesOfDischarges", "Messages");
                    }
                    if (dischargeViewModel.Origin == 4)
                    {
                        return RedirectToAction("Notifications", "Messages");
                    }

                    return RedirectToAction("Index");
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", dischargeViewModel) });
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DischargeEntity dischargeEntity = await _context.Discharge
                                                            .Include(d => d.Client)
                                                            .FirstOrDefaultAsync(s => s.Id == id);
            if (dischargeEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            int clientId = dischargeEntity.Client.Id;
            try
            {
                _context.Discharge.Remove(dischargeEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("ClientHistory", "Clients", new { idClient  = clientId});
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Frontdesk")]
        public IActionResult PrintDischarge(int id)
        {
            DischargeEntity entity = _context.Discharge

                                             .Include(d => d.Client)     
                                             .ThenInclude(c => c.Clinic)
                                             
                                             .Include(d => d.Supervisor)
                                             
                                             .FirstOrDefault(f => (f.Id == id));
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }
                        
            if (entity.Client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
            {
                Stream stream = _reportHelper.FloridaSocialHSDischargeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "DREAMS MENTAL HEALTH INC")
            {
                Stream stream = _reportHelper.DreamsMentalHealthDischargeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
            {
                Stream stream = _reportHelper.CommunityHTCDischargeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "PRINCIPLE CARE CENTER INC")
            {
                Stream stream = _reportHelper.PrincipleCCIDischargeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "SAPPHIRE MENTAL HEALTH CENTER LLC")
            {
                Stream stream = _reportHelper.SapphireMHCDischargeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "SOUTH FLORIDA MENTAL HEALTH & RECOVERY")
            {
                Stream stream = _reportHelper.SouthFloridaMHRDischargeReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            return null;
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Frontdesk")]
        public async Task<IActionResult> ClientswithoutDischarge(int idError = 0)
        {
            UserEntity user_logged = await _context.Users

                                                  .Include(u => u.Clinic)
                                                  .ThenInclude(c => c.Setting)

                                                  .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            ClientDischarge clienDischarge = new ClientDischarge();

            if (User.IsInRole("Facilitator"))
            {
                FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

                List<ClientEntity> clientListPSR = await _context.Clients
                                                                 .Where(m => (m.Clinic.Id == user_logged.Clinic.Id
                                                                            && m.Status == StatusType.Close
                                                                            && m.IdFacilitatorPSR == facilitator.Id
                                                                            && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.PSR).Count() > 0
                                                                            && m.DischargeList.Where(d => d.TypeService == ServiceType.PSR).Count() == 0))
                                                                 .ToListAsync();

                List<ClientEntity> clientListIND = await _context.Clients
                                                                 .Where(m => (m.Clinic.Id == user_logged.Clinic.Id
                                                                           && m.Status == StatusType.Close
                                                                           && m.IndividualTherapyFacilitator.Id == facilitator.Id
                                                                           && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.Individual).Count() > 0
                                                                           && m.DischargeList.Where(d => d.TypeService == ServiceType.Individual).Count() == 0))
                                                                 .ToListAsync();
                List<ClientEntity> clientListGroup = await _context.Clients
                                                                   .Where(m => (m.Clinic.Id == user_logged.Clinic.Id
                                                                             && m.Status == StatusType.Close
                                                                             && m.IdFacilitatorGroup == facilitator.Id
                                                                             && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.Group).Count() > 0
                                                                             && m.DischargeList.Where(d => d.TypeService == ServiceType.Group).Count() == 0))
                                                                   .ToListAsync();

                List<ClientDischarge> clientList = new List<ClientDischarge>();
                

                foreach (var item in clientListPSR)
                {
                    clienDischarge.Service = ServiceType.PSR;
                    clienDischarge.FacilitatorName = facilitator.Name;
                    clienDischarge.Id = item.Id;
                    clienDischarge.Name = item.Name;
                    clienDischarge.AdmisionDate = item.AdmisionDate;
                    clienDischarge.Code = item.Code;
                    clienDischarge.Gender = item.Gender;
                    clienDischarge.Status = item.Status;
                    clientList.Add(clienDischarge);
                    clienDischarge = new ClientDischarge();
                }

                foreach (var item in clientListGroup)
                {
                    clienDischarge.Service = ServiceType.Group;
                    clienDischarge.FacilitatorName = facilitator.Name;
                    clienDischarge.Id = item.Id;
                    clienDischarge.Name = item.Name;
                    clienDischarge.AdmisionDate = item.AdmisionDate;
                    clienDischarge.Code = item.Code;
                    clienDischarge.Gender = item.Gender;
                    clienDischarge.Status = item.Status;
                    clientList.Add(clienDischarge);
                    clienDischarge = new ClientDischarge();
                }

                foreach (var item in clientListIND)
                {
                    clienDischarge.Service = ServiceType.Individual;
                    clienDischarge.FacilitatorName = facilitator.Name;
                    clienDischarge.Id = item.Id;
                    clienDischarge.Name = item.Name;
                    clienDischarge.AdmisionDate = item.AdmisionDate;
                    clienDischarge.Code = item.Code;
                    clienDischarge.Gender = item.Gender;
                    clienDischarge.Status = item.Status;
                    clientList.Add(clienDischarge);
                    clienDischarge = new ClientDischarge();

                }

                return View(clientList);
            }
            else
            {
                List<ClientEntity> clientListPSR = await _context.Clients
                                                                
                                                                     .Where(m => (m.Clinic.Id == user_logged.Clinic.Id
                                                                            && m.Status == StatusType.Close
                                                                            && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.PSR).Count() > 0
                                                                            && m.DischargeList.Where(d => d.TypeService == ServiceType.PSR).Count() == 0))
                                                                 .ToListAsync();
                List<ClientEntity> clientListIND = await _context.Clients
                                                                 .Include(n => n.IndividualTherapyFacilitator)
                                                                 .Where(m => (m.Clinic.Id == user_logged.Clinic.Id
                                                                           && m.Status == StatusType.Close
                                                                           && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.Individual).Count() > 0
                                                                           && m.DischargeList.Where(d => d.TypeService == ServiceType.Individual).Count() == 0))
                                                                 .ToListAsync();

                List<ClientEntity> clientListGroup = await _context.Clients

                                                                   .Where(m => (m.Clinic.Id == user_logged.Clinic.Id
                                                                             && m.Status == StatusType.Close
                                                                             && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.Group).Count() > 0
                                                                             && m.DischargeList.Where(d => d.TypeService == ServiceType.Group).Count() == 0))
                                                                   .ToListAsync();

                List<ClientDischarge> clientList = new List<ClientDischarge>();
                FacilitatorEntity facilitator = new FacilitatorEntity();
                foreach (var item in clientListPSR)
                {
                    clienDischarge.Service = ServiceType.PSR;
                    clienDischarge.Id = item.Id;
                    clienDischarge.Name = item.Name;
                    clienDischarge.AdmisionDate = item.AdmisionDate;
                    clienDischarge.Code = item.Code;
                    clienDischarge.Gender = item.Gender;
                    clienDischarge.Status = item.Status;

                    facilitator = _context.Facilitators.FirstOrDefault(n => n.Id == item.IdFacilitatorPSR);
                    if (facilitator != null)
                    {
                        clienDischarge.FacilitatorName = facilitator.Name;
                    }
                    else
                    {
                        clienDischarge.FacilitatorName = string.Empty;
                    }

                    clientList.Add(clienDischarge);
                    facilitator = new FacilitatorEntity();
                    clienDischarge = new ClientDischarge();
                }

                foreach (var item in clientListGroup)
                {
                    clienDischarge.Service = ServiceType.Group;
                    clienDischarge.Id = item.Id;
                    clienDischarge.Name = item.Name;
                    clienDischarge.AdmisionDate = item.AdmisionDate;
                    clienDischarge.Code = item.Code;
                    clienDischarge.Gender = item.Gender;
                    clienDischarge.Status = item.Status;
                    
                    facilitator = _context.Facilitators.FirstOrDefault(n => n.Id == item.IdFacilitatorGroup);
                    
                    if (facilitator != null)
                    {
                        clienDischarge.FacilitatorName = facilitator.Name;
                    }
                    else
                    {
                        clienDischarge.FacilitatorName = string.Empty;                              
                    }

                    clientList.Add(clienDischarge);
                    facilitator = new FacilitatorEntity();
                    clienDischarge = new ClientDischarge();
                }

                foreach (var item in clientListIND)
                {
                    clienDischarge.Service = ServiceType.Individual;
                    clienDischarge.Id = item.Id;
                    clienDischarge.Name = item.Name;
                    clienDischarge.Code = item.Code;
                    clienDischarge.AdmisionDate = item.AdmisionDate;
                    clienDischarge.Gender = item.Gender;
                    clienDischarge.Status = item.Status;
                    
                    facilitator = _context.Facilitators.FirstOrDefault(n => n.Id == item.IndividualTherapyFacilitator.Id);
                    
                    if (facilitator != null)
                    {
                        clienDischarge.FacilitatorName = facilitator.Name;
                    }
                    else
                    {
                        clienDischarge.FacilitatorName = string.Empty;
                    }
                    clientList.Add(clienDischarge);
                    
                    facilitator = new FacilitatorEntity();
                    clienDischarge = new ClientDischarge();
                }

                return View(clientList);
            }
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> FinishEditingDischarge(int id, int origin = 0)
        {
            DischargeEntity discharge = await _context.Discharge.FirstOrDefaultAsync(n => n.Id == id);
            if (User.IsInRole("Supervisor"))
            {
                discharge.Status = DischargeStatus.Approved;
            }
            else
            {
                discharge.Status = DischargeStatus.Pending;
            }

            _context.Update(discharge);

            await _context.SaveChangesAsync();

            if (origin == 1)
            {
                return RedirectToAction("DischargeInEdit");
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ApproveDischarge(int id, int origin = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);


            SupervisorEntity supervisor = await _context.Supervisors.FirstOrDefaultAsync(n => n.LinkedUser == user_logged.UserName);
            DischargeEntity discharge = await _context.Discharge.FirstOrDefaultAsync(n => n.Id == id);
            discharge.Status = DischargeStatus.Approved;
            discharge.Supervisor = supervisor;
            _context.Update(discharge);

            await _context.SaveChangesAsync();
            
            if(origin == 2)
            {
                return RedirectToAction(nameof(PendingDischarge));
            }
            if (origin == 4)
            {
                return RedirectToAction("Notifications", "Messages");
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Supervisor, Manager, Facilitator, Frontdesk")]
        public async Task<IActionResult> PendingDischarge(int idError = 0)
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

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
                        return View(await _context.Discharge

                                                  .Include(d => d.Client)
                                                  .ThenInclude(d => d.Clinic)

                                                  .Include(f => f.Messages.Where(m => m.Notification == false))

                                                  .Where(d => (d.Client.Clinic.Id == clinic.Id)
                                                            && d.Status == DischargeStatus.Pending
                                                            && (d.Client.IdFacilitatorPSR == facilitator.Id || d.Client.IndividualTherapyFacilitator.Id == facilitator.Id || d.Client.IdFacilitatorGroup == facilitator.Id))
                                                  .OrderBy(d => d.Client.Clinic.Name).ToListAsync());
                    }

                    if (User.IsInRole("Manager") || User.IsInRole("Supervisor"))
                    {
                        return View(await _context.Discharge

                                                  .Include(d => d.Client)
                                                  .ThenInclude(d => d.Clinic)

                                                  .Include(f => f.Messages.Where(m => m.Notification == false))

                                                  .Where(d => (d.Client.Clinic.Id == clinic.Id)
                                                            && d.Status == DischargeStatus.Pending)
                                                  .OrderBy(d => d.Client.Clinic.Name).ToListAsync());
                    }
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Frontdesk")]
        public async Task<IActionResult> DischargeInEdit(int idError = 0)
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
                if (User.IsInRole("Manager"))
                    return View(await _context.Discharge

                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clients_Diagnostics)

                                              .Where(n => (n.Status == DischargeStatus.Edition
                                                    && n.Client.Clinic.Id == user_logged.Clinic.Id))
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

                if (User.IsInRole("Supervisor"))
                {
                    return View(await _context.Discharge

                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clients_Diagnostics)

                                              .Where(n => (n.Status == DischargeStatus.Edition
                                                    && n.Client.Clinic.Id == user_logged.Clinic.Id))
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());
                }
                if (User.IsInRole("Facilitator"))
                {
                    return View(await _context.Discharge

                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clients_Diagnostics)

                                              .Where(n => (n.Status == DischargeStatus.Edition
                                                    && n.Client.Clinic.Id == user_logged.Clinic.Id 
                                                    && n.CreatedBy == user_logged.UserName))
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> DischargeOfService(int idError = 0)
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (User.IsInRole("Facilitator"))
            {
                FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

                List<ClientEntity> clientListPSR = await _context.Clients
                                                                 .Include(m => m.DischargeList)

                                                                 .Where(m => ((m.DischargeList.Count() == 0 || m.DischargeList.Where(n => n.TypeService == ServiceType.PSR).ToList().Count == 0)
                                                                     && m.Clinic.Id == user_logged.Clinic.Id
                                                                     && m.Status == StatusType.Open && m.IdFacilitatorPSR == facilitator.Id)).ToListAsync();
                List<ClientEntity> clientListIND = await _context.Clients
                                                                 .Include(m => m.DischargeList)
                                                                 .Include(m => m.IndividualTherapyFacilitator)
                                                                 .Where(m => ((m.DischargeList.Count() == 0 || m.DischargeList.Where(n => n.TypeService == ServiceType.Individual).ToList().Count == 0)
                                                                    && m.Clinic.Id == user_logged.Clinic.Id
                                                                    && m.Status == StatusType.Open && m.IndividualTherapyFacilitator.Id == facilitator.Id)).ToListAsync();
                List<ClientEntity> clientListGroup = await _context.Clients
                                                                   .Include(m => m.DischargeList)

                                                                   .Where(m => ((m.DischargeList.Count() == 0 || m.DischargeList.Where(n => n.TypeService == ServiceType.Group).ToList().Count == 0)
                                                                     && m.Clinic.Id == user_logged.Clinic.Id
                                                                     && m.Status == StatusType.Open && m.IdFacilitatorGroup == facilitator.Id)).ToListAsync();


                List<ClientEntity> clientList = new List<ClientEntity>();

                foreach (var item in clientListPSR)
                {
                    item.Service = ServiceType.PSR;
                    clientList.Add(item);
                }

                foreach (var item in clientListGroup)
                {
                    item.Service = ServiceType.Group;
                    clientList.Add(item);
                }

                ClientEntity client = new ClientEntity();

                foreach (var item in clientListIND)
                {
                    client.Id = item.Id;
                    client.Name = item.Name;
                    client.Code = item.Code;
                    client.AdmisionDate = item.AdmisionDate;
                    client.Status = item.Status;
                    client.Service = ServiceType.Individual;
                    clientList.Add(client);
                    client = new ClientEntity();
                }

                return View(clientList);
            }
           else
                return RedirectToAction("NotAuthorized", "Account");
        
        }

        [Authorize(Roles = "Supervisor")]
        public IActionResult AddMessageEntity(int id = 0, int origin = 0)
        {
            if (id == 0)
            {
                return View(new MessageViewModel());
            }
            else
            {
                MessageViewModel model = new MessageViewModel()
                {
                    IdDischarge = id,
                    Origin = origin
                };

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> AddMessageEntity(MessageViewModel messageViewModel)
        {
            if (ModelState.IsValid)
            {
                MessageEntity model = await _converterHelper.ToMessageEntity(messageViewModel, true);
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                model.From = user_logged.UserName;
                model.To = model.Discharge.CreatedBy;
                _context.Add(model);
                await _context.SaveChangesAsync();
            }

            if (messageViewModel.Origin == 2)
                return RedirectToAction("PendingDischarge");

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, Frontdesk")]
        public async Task<IActionResult> DischargeForClient(int idClient = 0)
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
                FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                if (User.IsInRole("Manager") || User.IsInRole("Supervisor"))
                    return View(await _context.Discharge

                                              .Include(f => f.Client)

                                              .Where(n => (n.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && n.Client.Id == idClient))
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

                if (User.IsInRole("Documents_Assistant"))
                    return View(await _context.Discharge

                                              .Include(f => f.Client)

                                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && n.Client.Id == idClient)
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

                if (User.IsInRole("Facilitator"))
                {
                    return View(await _context.Discharge

                                              .Include(f => f.Client)

                                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id
                                                && n.Client.Id == idClient
                                                && (n.Client.IdFacilitatorPSR == facilitator.Id || n.Client.IndividualTherapyFacilitator.Id == facilitator.Id || n.Client.IdFacilitatorGroup == facilitator.Id))
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ReturnTo(int? id, int clientId = 0, DischargeStatus aStatus = DischargeStatus.Edition)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DischargeEntity dischargeEntity = await _context.Discharge.FirstOrDefaultAsync(s => s.Id == id);
            if (dischargeEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                dischargeEntity.Status = aStatus;
                _context.Discharge.Update(dischargeEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("ClientHistory", "Clients", new { idClient = clientId });
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Frontdesk")]
        public async Task<IActionResult> AuditDischarge()
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic || !user_logged.Clinic.Setting.MHProblems)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<AuditDischarge> auditClient_List = new List<AuditDischarge>();
            AuditDischarge auditClient = new AuditDischarge();

            List<ClientEntity> client_List = new List<ClientEntity>();

            if (!User.IsInRole("Facilitator"))
            {
                client_List = _context.Clients
                                      .Include(m => m.DischargeList)
                                      .Include(m => m.IndividualTherapyFacilitator)
                                      .Include(m => m.Workdays_Clients)
                                      .ThenInclude(m => m.Workday)
                                      .Where(n => (n.Clinic.Id == user_logged.Clinic.Id
                                          && n.Status == StatusType.Close))
                                      .ToList();

            }
            else
            {
                FacilitatorEntity facilitator = await _context.Facilitators.FirstOrDefaultAsync(f => f.LinkedUser == user_logged.UserName);
                client_List = _context.Clients
                                      .Include(m => m.DischargeList)
                                      .Include(m => m.IndividualTherapyFacilitator)
                                      .Include(m => m.Workdays_Clients)
                                      .ThenInclude(m => m.Workday)
                                      .Where(n => (n.Clinic.Id == user_logged.Clinic.Id
                                         && n.Status == StatusType.Close
                                         && (n.IdFacilitatorPSR == facilitator.Id || n.IndividualTherapyFacilitator.Id == facilitator.Id || n.IdFacilitatorGroup == facilitator.Id)))
                                      .ToList();

            }

            int individualTherapy = 0;
            bool HaveIndtherapy = false;
            int PSR = 0;
            int Group = 0;

            foreach (var item in client_List.OrderBy(n => n.AdmisionDate))
            {
                if (item.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.Individual).Count() > 0)
                {
                    individualTherapy = 1;
                    HaveIndtherapy = true;
                }
                else
                {
                    individualTherapy = 0;
                    HaveIndtherapy = false;
                }
                if (item.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.PSR).Count() > 0)
                {
                    PSR = 1;
                }
                else
                {
                    PSR = 0;
                }
                if (item.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.Group).Count() > 0)
                {
                    Group = 1;
                }
                else
                {
                    Group = 0;
                }

                foreach(var discharge in item.DischargeList)
                {
                    if (discharge.TypeService == ServiceType.PSR)
                    {
                        PSR --;
                    }
                    if (discharge.TypeService == ServiceType.Group)
                    {
                        Group --;
                    }
                    if (discharge.TypeService == ServiceType.Individual)
                    {
                        individualTherapy --;
                    }
                }

                if (PSR == 1)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Service = "Missing Discharge (PSR)";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditDischarge();
                }
                if (PSR < 0)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Service = "Duplicate Discharge (PSR)";
                    auditClient.Active = 1;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditDischarge();
                }
                if (Group == 1)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Service = "Missing Discharge (Group)";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditDischarge();
                }
                if (Group < 0)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Service = "Duplicate Discharge (Group)";
                    auditClient.Active = 1;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditDischarge();
                }
                if (individualTherapy == 1)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Service = "Missing Discharge (Individual)";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditDischarge();
                }
                if (individualTherapy < 0)
                {
                    if (HaveIndtherapy == true)
                    {
                        auditClient.NameClient = item.Name;
                        auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                        auditClient.Service = "Duplicate Discharge (Individual)";
                        auditClient.Active = 1;

                        auditClient_List.Add(auditClient);
                        auditClient = new AuditDischarge();
                    }
                    else
                    {
                        auditClient.NameClient = item.Name;
                        auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                        auditClient.Service = "There are discharges without individual notes";
                        auditClient.Active = 1;

                        auditClient_List.Add(auditClient);
                        auditClient = new AuditDischarge();
                    }
                    
                }
            }

            return View(auditClient_List);
        }


    }
}
