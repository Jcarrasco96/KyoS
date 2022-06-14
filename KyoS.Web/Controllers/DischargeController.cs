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
                                                                        && (n.IdFacilitatorPSR == facilitator.Id || n.IndividualTherapyFacilitator.Id == facilitator.Id))
                                                                        || (n.Clinic.Id == user_logged.Clinic.Id && n.Status == StatusType.Open
                                                                        && (n.DischargeList.Count() > 0)
                                                                        && (n.IdFacilitatorPSR == facilitator.Id || n.IndividualTherapyFacilitator.Id == facilitator.Id)))
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

            if (User.IsInRole("Facilitator "))
            {
                if (user_logged.Clinic != null)
                {
                    model = new DischargeViewModel
                    {
                        IdClient = id,
                        Client = _context.Clients

                                         .Include(n => n.MedicationList)

                                         .Include(c => c.Clients_Diagnostics)
                                         .ThenInclude(cd => cd.Diagnostic)

                                         .FirstOrDefault(n => n.Id == id),
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
                        TypeService = service                        
                    };
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    return View(model);
                }
            }

            model = new DischargeViewModel
            {
                IdClient = id,
                Client = _context.Clients
                                 .Include(n => n.MedicationList)
                                         
                                 .Include(c => c.Clients_Diagnostics)
                                 .ThenInclude(cd => cd.Diagnostic)

                                 .FirstOrDefault(n => n.Id == id),
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
                TypeService = service                
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

            if (ModelState.IsValid)
            {
                DischargeEntity DischargeEntity = _context.Discharge.Find(DischargeViewModel.Id);
                if (DischargeEntity == null)
                {
                    DischargeEntity = await _converterHelper.ToDischargeEntity(DischargeViewModel, true, user_logged.UserName);
                    _context.Discharge.Add(DischargeEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        return RedirectToAction("DischargeInEdit", "Discharge");
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
            DischargeViewModel model;
            model = new DischargeViewModel
            {
                IdClient = DischargeViewModel.IdClient,
                Client = _context.Clients.Find(DischargeViewModel.IdClient),
                AdmissionedFor = user_logged.FullName,
                DateDischarge = DischargeViewModel.DateDischarge,
                DateReport =DischargeViewModel.DateReport ,
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
                
                TypeService = DischargeViewModel.TypeService
            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", DischargeViewModel) });
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
                DischargeEntity dischargeEntity = await _converterHelper.ToDischargeEntity(dischargeViewModel, false, user_logged.Id);
                _context.Discharge.Update(dischargeEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    if (dischargeViewModel.Origin == 1)
                    {
                        return RedirectToAction("DischargeInEdit");
                    }
                    if (dischargeViewModel.Origin == 2)
                    {
                        return RedirectToAction("PendingDischarge");
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

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Delete(int? id)
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
                _context.Discharge.Remove(dischargeEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
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

            //if (entity.Client.Clinic.Name == "DAVILA")
            //{
            //    Stream stream = _reportHelper.FloridaSocialHSIntakeReport(entity);
            //    return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            //}

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

            return null;
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
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

            if (User.IsInRole("Facilitator"))
            {
               FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

                List<ClientEntity> clientListPSR = await _context.Clients
                                                               .Include(m => m.DischargeList)
                                                               
                                                               .Where(m => ((m.DischargeList.Count() == 0 || m.DischargeList.Where(n => n.TypeService == ServiceType.PSR).ToList().Count == 0)
                                                                     && m.Clinic.Id == user_logged.Clinic.Id
                                                                     && m.Status == StatusType.Close && m.IdFacilitatorPSR == facilitator.Id)).ToListAsync();
                List<ClientEntity> clientListIND = await _context.Clients
                                                              .Include(m => m.DischargeList)
                                                              .Include(m => m.IndividualTherapyFacilitator)
                                                              .Where(m => ((m.DischargeList.Count() == 0 || m.DischargeList.Where(n => n.TypeService == ServiceType.Individual).ToList().Count == 0)
                                                                    && m.Clinic.Id == user_logged.Clinic.Id
                                                                    && m.Status == StatusType.Close && m.IndividualTherapyFacilitator.Id == facilitator.Id)).ToListAsync();
                List<ClientEntity> clientList = new List<ClientEntity>();

                foreach (var item in clientListPSR)
                {
                    item.Service = ServiceType.PSR;
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
            {
                List<ClientEntity> clientListPSR = await _context.Clients
                                                                .Include(m => m.DischargeList)

                                                                .Where(m => ((m.DischargeList.Count() == 0 || m.DischargeList.Where(n => n.TypeService == ServiceType.PSR).ToList().Count == 0)
                                                                      && m.Clinic.Id == user_logged.Clinic.Id
                                                                      && m.Status == StatusType.Close)).ToListAsync();
                List<ClientEntity> clientListIND = await _context.Clients
                                                              .Include(m => m.DischargeList)

                                                              .Where(m => ((m.DischargeList.Count() == 0 || m.DischargeList.Where(n => n.TypeService == ServiceType.Individual).ToList().Count == 0)
                                                                    && m.Clinic.Id == user_logged.Clinic.Id
                                                                    && m.Status == StatusType.Close 
                                                                    && m.IndividualTherapyFacilitator != null
                                                                    /*m.Workdays_Clients.Where(n => n.Workday.Service == ServiceType.Individual).ToList().Count() > 0*/)).ToListAsync();

                List<ClientEntity> clientList = new List<ClientEntity>();

                foreach (var item in clientListPSR)
                {
                    item.Service = ServiceType.PSR;
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
            DischargeEntity discharge = await _context.Discharge.FirstOrDefaultAsync(n => n.Id == id);
            discharge.Status = DischargeStatus.Approved;
            _context.Update(discharge);

            await _context.SaveChangesAsync();
            
            if(origin == 1)
            {
                return RedirectToAction(nameof(PendingDischarge));
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Supervisor, Manager, Facilitator")]
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
                                                 .Include(c => c.Client)
                                                 .ThenInclude(c => c.Clinic)
                                                 .Where(m => (m.Client.Clinic.Id == clinic.Id)
                                                       && m.Status == DischargeStatus.Pending
                                                       && (m.Client.IdFacilitatorPSR == facilitator.Id || m.Client.IndividualTherapyFacilitator.Id == facilitator.Id))
                                                 .OrderBy(m => m.Client.Clinic.Name).ToListAsync());
                    }

                    if (User.IsInRole("Manager") || User.IsInRole("Supervisor"))
                    {
                        return View(await _context.Discharge
                                                  .Include(c => c.Client)
                                                  .ThenInclude(c => c.Clinic)
                                                  .Where(m => (m.Client.Clinic.Id == clinic.Id)
                                                        && m.Status == DischargeStatus.Pending)
                                                  .OrderBy(m => m.Client.Clinic.Name).ToListAsync());
                    }
                    

                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
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
                List<ClientEntity> clientList = new List<ClientEntity>();

                foreach (var item in clientListPSR)
                {
                    item.Service = ServiceType.PSR;
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
    }
}
