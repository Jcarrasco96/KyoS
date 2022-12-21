using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using KyoS.Common.Enums;
using KyoS.Common.Helpers;

namespace KyoS.Web.Controllers
{
    public class FarsFormsController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IReportHelper _reportHelper;
        private readonly DataContext _context;

        public FarsFormsController(IUserHelper userHelper, IConverterHelper converterHelper, ICombosHelper combosHelper, IRenderHelper renderHelper, DataContext context, IReportHelper reportHelper)
        {
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _context = context;
            _renderHelper = renderHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant")]
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
                FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                if (User.IsInRole("Manager")|| User.IsInRole("Supervisor"))
                    return View(await _context.Clients

                                              .Include(f => f.FarsFormList)

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());

                if (User.IsInRole("Documents_Assistant"))
                    return View(await _context.Clients

                                              .Include(f => f.FarsFormList)

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id && n.FarsFormList.Where(m => m.CreatedBy == user_logged.UserName).Count()>0)
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());

                if (User.IsInRole("Facilitator"))
                {
                    return View(await _context.Clients

                                              .Include(f => f.FarsFormList)

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id
                                                && (n.IdFacilitatorPSR == facilitator.Id || n.IndividualTherapyFacilitator.Id == facilitator.Id))
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public IActionResult Create(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);
            ClientEntity client = _context.Clients.Include(n => n.FarsFormList)
                                                  .Include(n => n.Clinic)
                                                    .FirstOrDefault(n => n.Id == id);
            FarsFormViewModel model;

            if (user_logged.Clinic != null)
            {
                model = new FarsFormViewModel
                {
                   IdClient = id,
                   Client = client,
                   Id = 0,
                   AbilityScale = 0,
                   AdmissionedFor = user_logged.FullName,
                   ActivitiesScale = 0,
                   AnxietyScale = 0,
                   CognitiveScale = 0,
                   ContractorID = client.Clinic.ProviderMedicaidId,
                   Country = "13",
                   DangerToOtherScale = 0,
                   DangerToSelfScale = 0,
                   DcfEvaluation = "",
                   DepressionScale = 0,
                   EvaluationDate = DateTime.Now,
                   FamilyEnvironmentScale = 0,
                   FamilyRelationShipsScale = 0,
                   HyperAffectScale = 0,
                   InterpersonalScale = 0,
                   MCOID = "",
                   MedicaidProviderID = client.Clinic.ProviderMedicaidId,
                   MedicaidRecipientID = client.MedicaidID,
                   MedicalScale = 0,
                   M_GafScore = "",
                   ProviderId = client.Clinic.ProviderTaxId,
                   ProviderLocal = client.Clinic.Address + ", " + client.Clinic.City + ", " + client.Clinic.State + ", "+ client.Clinic.ZipCode,
                   RaterEducation = "",
                   RaterFMHI = "",
                   SecurityScale = 0,
                   SignatureDate = DateTime.Now,
                   SocialScale = 0,
                   SubstanceAbusoHistory = 0,
                   SubstanceScale = 0,
                   ThoughtProcessScale = 0,
                   TraumaticsScale = 0,
                   WorkScale = 0,
                     
                   ContID1 = client.Clinic.ProviderTaxId,
                   ContID2 = "",
                   ContID3 = "",
                   ProgramEvaluation = "",
                   Status = FarsStatus.Edition,
                   IdSupervisor = 0,
                   Supervisor = new SupervisorEntity()
                };

                SupervisorEntity supervisor = _context.Supervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                if (User.IsInRole("Supervisor") && supervisor != null)
                {
                    model.Supervisor = supervisor;
                    model.IdSupervisor = supervisor.Id;
                    model.RaterEducation = supervisor.RaterEducation;
                    model.RaterFMHI = supervisor.RaterFMHCertification;
                }

                DocumentsAssistantEntity documentsAssistant = _context.DocumentsAssistant.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                if (User.IsInRole("Documents_Assistant") && documentsAssistant != null)
                {
                    model.RaterEducation = documentsAssistant.RaterEducation;
                    model.RaterFMHI = documentsAssistant.RaterFMHCertification;
                }

                FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                if (User.IsInRole("facilitator") && facilitator != null)
                {
                    model.RaterEducation = facilitator.RaterEducation;
                    model.RaterFMHI = facilitator.RaterFMHCertification;
                }

                if (model.Client.FarsFormList == null)
                    model.Client.FarsFormList = new List<FarsFormEntity>();
                return View(model);
                }
            
            return RedirectToAction("NotAuthorized", "Account");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> Create(FarsFormViewModel FarsFormViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                FarsFormEntity farsFormEntity = _context.FarsForm.Find(FarsFormViewModel.Id);
                if (farsFormEntity == null)
                {
                    farsFormEntity = await _converterHelper.ToFarsFormEntity(FarsFormViewModel, true, user_logged.UserName);
                    _context.FarsForm.Add(farsFormEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        return RedirectToAction(nameof(Index));
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Fars.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", FarsFormViewModel) });
                }
            }
            FarsFormViewModel model;
            model = new FarsFormViewModel
            {
                IdClient = FarsFormViewModel.IdClient,
                Client = _context.Clients.Find(FarsFormViewModel.IdClient),
                Id = FarsFormViewModel.Id,
                AbilityScale = FarsFormViewModel.AbilityScale,
                ActivitiesScale = FarsFormViewModel.ActivitiesScale,
                AdmissionedFor = FarsFormViewModel.AdmissionedFor,
                AnxietyScale = FarsFormViewModel.AnxietyScale,
                CognitiveScale = FarsFormViewModel.CognitiveScale,
                ContID1 = FarsFormViewModel.ContID1,
                ContID2 = FarsFormViewModel.ContID2,
                ContID3 = FarsFormViewModel.ContID3,
                ContractorID = FarsFormViewModel.ContractorID,
                Country = FarsFormViewModel.Country,
                DangerToOtherScale = FarsFormViewModel.DangerToOtherScale,
                DangerToSelfScale = FarsFormViewModel.DangerToSelfScale,
                DcfEvaluation = FarsFormViewModel.DcfEvaluation,
                DepressionScale = FarsFormViewModel.DepressionScale,
                EvaluationDate = FarsFormViewModel.EvaluationDate,
                FamilyEnvironmentScale = FarsFormViewModel.FamilyEnvironmentScale,
                FamilyRelationShipsScale = FarsFormViewModel.FamilyRelationShipsScale,
                HyperAffectScale = FarsFormViewModel.HyperAffectScale,
                InterpersonalScale = FarsFormViewModel.InterpersonalScale,
                MCOID = FarsFormViewModel.MCOID,
                MedicaidProviderID = FarsFormViewModel.MedicaidProviderID,
                MedicaidRecipientID = FarsFormViewModel.MedicaidRecipientID,
                MedicalScale = FarsFormViewModel.MedicalScale,
                M_GafScore = FarsFormViewModel.M_GafScore,
                ProgramEvaluation = FarsFormViewModel.ProgramEvaluation,
                ProviderId = FarsFormViewModel.ProviderId,
                ProviderLocal = FarsFormViewModel.ProviderLocal,
                RaterEducation = FarsFormViewModel.RaterEducation,
                RaterFMHI = FarsFormViewModel.RaterFMHI,
                SecurityScale = FarsFormViewModel.SecurityScale,
                SignatureDate = FarsFormViewModel.SignatureDate,
                SocialScale = FarsFormViewModel.SocialScale,
                SubstanceAbusoHistory = FarsFormViewModel.SubstanceAbusoHistory,
                SubstanceScale = FarsFormViewModel.SubstanceScale,
                ThoughtProcessScale = FarsFormViewModel.ThoughtProcessScale,
                TraumaticsScale = FarsFormViewModel.TraumaticsScale,
                WorkScale = FarsFormViewModel.WorkScale
                

            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", FarsFormViewModel) });
        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public IActionResult Edit(int id = 0, int origin = 0)
        {
            FarsFormViewModel model;

            if (User.IsInRole("Supervisor") || User.IsInRole("Facilitator") || User.IsInRole("Documents_Assistant"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    FarsFormEntity FarsForm = _context.FarsForm

                                                      .Include(m => m.Client)
                                                      .ThenInclude(m => m.FarsFormList)

                                                      .FirstOrDefault(m => m.Id == id);
                    if (FarsForm == null)
                    {
                        return RedirectToAction("Home/Error404");
                    }
                    else
                    {

                        model = _converterHelper.ToFarsFormViewModel(FarsForm);
                        model.Origin = origin;
                        return View(model);
                    }

                }
            }

            model = new FarsFormViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> Edit(FarsFormViewModel farsFormViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                FarsFormEntity farsFormEntity = await _converterHelper.ToFarsFormEntity(farsFormViewModel, false, user_logged.Id);
                _context.FarsForm.Update(farsFormEntity);
                try
                {
                    List<MessageEntity> messages = farsFormEntity.Messages.Where(m => (m.Status == MessageStatus.NotRead && m.Notification == false)).ToList();
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
                            FarsForm = farsFormEntity,
                            MTPReview = null,
                            Addendum = null,
                            Discharge = null,
                            Title = "Update on reviewed FARS Forms",
                            Text = $"The FARS Forms document of {farsFormEntity.Client.Name} that was evaluated on {farsFormEntity.EvaluationDate.ToShortDateString()} was rectified",
                            From = value.To,
                            To = value.From,
                            DateCreated = DateTime.Now,
                            Status = MessageStatus.NotRead,
                            Notification = true
                        };
                        _context.Add(notification);
                    }

                    await _context.SaveChangesAsync();

                    if (farsFormViewModel.Origin == 1)
                    {
                        return RedirectToAction(nameof(PendingFars));
                    }
                    if (farsFormViewModel.Origin == 2)
                    {
                        return RedirectToAction("MessagesOfFars", "Messages");
                    }
                    if (farsFormViewModel.Origin == 3)
                    {
                        return RedirectToAction("Notifications", "Messages");
                    }
                    if (farsFormViewModel.Origin == 4)
                    {
                        return RedirectToAction(nameof(EditionFars));
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", farsFormViewModel) });
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int? id, int clientId = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            FarsFormEntity farsFormEntity = await _context.FarsForm.FirstOrDefaultAsync(s => s.Id == id);
            if (farsFormEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.FarsForm.Remove(farsFormEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("ClientHistory", "Clients", new { idClient = clientId });
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant")]
        public IActionResult PrintFarsForm(int id)
        {
            FarsFormEntity entity = _context.FarsForm

                                            .Include(f => f.Client)
                                            .ThenInclude(c => c.Clinic)

                                            .Include(i => i.Client)
                                            .ThenInclude(c => c.EmergencyContact)

                                            .Include(i => i.Client)
                                            .ThenInclude(c => c.LegalGuardian)

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
                Stream stream = _reportHelper.FloridaSocialHSFarsReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "DREAMS MENTAL HEALTH INC")
            {
                Stream stream = _reportHelper.DreamsMentalHealthFarsReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            return null;
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> ClientswithoutFARS(int idError = 0)
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
            if (User.IsInRole("Facilitator"))
            {
                
                List<ClientEntity> ClientList = await _context.Clients
                                                              .Include(n => n.FarsFormList)
                                                              .Where(n => n.FarsFormList.Count == 0 
                                                                 && n.Clinic.Id == user_logged.Clinic.Id
                                                                 && (n.IdFacilitatorPSR == facilitator.Id 
                                                                    || n.IndividualTherapyFacilitator.Id == facilitator.Id)
                                                                 && n.OnlyTCM == false)
                                                              .ToListAsync();

                return View(ClientList);
            }
            else
            {
                List<ClientEntity> ClientList = await _context.Clients
                                                          .Include(n => n.FarsFormList)
                                                          .Where(n => n.FarsFormList.Count == 0 
                                                              && n.Clinic.Id == user_logged.Clinic.Id
                                                              && n.OnlyTCM == false)
                                                          .ToListAsync();

                return View(ClientList);
            }
            

        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> FinishEditingFars(int id)
        {
            FarsFormEntity fars = await _context.FarsForm.FirstOrDefaultAsync(n => n.Id == id);
            if (User.IsInRole("Supervisor"))
            {
                fars.Status = FarsStatus.Approved;
            }
            else
            {
                fars.Status = FarsStatus.Approved;  //the FARS no have approved for Supervisor
            }

            _context.Update(fars);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(EditionFars));
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ApproveFars(int id, int origin = 0)
        {
            FarsFormEntity fars = await _context.FarsForm.FirstOrDefaultAsync(n => n.Id == id);
            fars.Status = FarsStatus.Approved;
            _context.Update(fars);

            await _context.SaveChangesAsync();
            
            if (origin == 1)
            {
                return RedirectToAction(nameof(PendingFars));
            }
            if (origin == 3)
            {
                return RedirectToAction("Notifications", "Messages");
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Supervisor, Manager")]
        public async Task<IActionResult> PendingFars(int idError = 0)
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
                    return View(await _context.FarsForm

                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clinic)

                                              .Include(f => f.Messages.Where(m => m.Notification == false))

                                              .Where(f => (f.Client.Clinic.Id == clinic.Id)
                                                        && f.Status == FarsStatus.Pending)
                                              .ToListAsync());                    
                }
            }
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
                    IdFarsForm = id,
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
                model.To = model.FarsForm.CreatedBy;
                _context.Add(model);
                await _context.SaveChangesAsync();
            }
            
            if (messageViewModel.Origin == 1)
                return RedirectToAction("PendingFars");

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> FarsForClient(int idClient = 0)
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
                    return View(await _context.FarsForm

                                              .Include(f => f.Client)

                                              .Where(n => (n.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && n.Client.Id == idClient))
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

                if (User.IsInRole("Documents_Assistant"))
                    return View(await _context.FarsForm

                                              .Include(f => f.Client)

                                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id 
                                                    && n.Client.Id == idClient)
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

                if (User.IsInRole("Facilitator"))
                {
                    return View(await _context.FarsForm

                                              .Include(f => f.Client)

                                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id
                                                && n.Client.Id == idClient
                                                && (n.Client.IdFacilitatorPSR == facilitator.Id || n.Client.IndividualTherapyFacilitator.Id == facilitator.Id))
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor, Manager, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> EditionFars(int idError = 0)
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
                    if (User.IsInRole("Manager"))
                    {
                        return View(await _context.FarsForm

                                              .Include(f => f.Client)
                                              .ThenInclude(f => f.Clinic)

                                              .Include(f => f.Messages.Where(m => m.Notification == false))

                                              .Where(f => (f.Client.Clinic.Id == clinic.Id)
                                                        && f.Status == FarsStatus.Edition)
                                              .ToListAsync());
                    }
                    else
                    {
                        return View(await _context.FarsForm

                                                  .Include(f => f.Client)
                                                  .ThenInclude(f => f.Clinic)

                                                  .Include(f => f.Messages.Where(m => m.Notification == false))

                                                  .Where(f => (f.Client.Clinic.Id == clinic.Id)
                                                            && f.Status == FarsStatus.Edition
                                                            && f.CreatedBy == user_logged.UserName)
                                                  .ToListAsync());
                    }
                    
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ReturnTo(int? id, int clientId = 0, FarsStatus aStatus = FarsStatus.Edition)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            FarsFormEntity farsFormEntity = await _context.FarsForm.FirstOrDefaultAsync(s => s.Id == id);
            if (farsFormEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                farsFormEntity.Status = aStatus;
                _context.FarsForm.Update(farsFormEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("ClientHistory", "Clients", new { idClient = clientId });
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
        public async Task<IActionResult> AuditFARS()
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic || !user_logged.Clinic.Setting.MHProblems)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<AuditFARS> auditClient_List = new List<AuditFARS>();
            AuditFARS auditClient = new AuditFARS();

            List<ClientEntity> client_List = new List<ClientEntity>();

            if (User.IsInRole("Manager") || User.IsInRole("Supervisor"))
            {
                client_List = _context.Clients
                                      .Include(m => m.MTPs)
                                      .ThenInclude(m => m.MtpReviewList)
                                      .Include(m => m.MTPs)
                                      .ThenInclude(m => m.AdendumList)
                                      .Include(m => m.FarsFormList)
                                      .Include(m => m.DischargeList)
                                      .Include(m => m.IndividualTherapyFacilitator)
                                      .Where(n => n.Clinic.Id == user_logged.Clinic.Id
                                            && n.MTPs.Count() > 0)
                                      .ToList();

            }
            else
            {
                if (User.IsInRole("Facilitator"))
                {
                    FacilitatorEntity facilitator = await _context.Facilitators.FirstOrDefaultAsync(f => f.LinkedUser == user_logged.UserName);

                    client_List = _context.Clients
                                          .Include(m => m.MTPs)
                                          .ThenInclude(m => m.MtpReviewList)
                                          .Include(m => m.MTPs)
                                          .ThenInclude(m => m.AdendumList)
                                          .Include(m => m.FarsFormList)
                                          .Include(m => m.DischargeList)
                                          .Include(m => m.IndividualTherapyFacilitator)
                                          .Where(n => n.Clinic.Id == user_logged.Clinic.Id
                                              && n.MTPs.Count() > 0 && (n.IdFacilitatorPSR == facilitator.Id || n.IndividualTherapyFacilitator.Id == facilitator.Id))
                                          .ToList();

                }

            }

            MTPEntity mtp = new MTPEntity();
            List<MTPReviewEntity> review = new List<MTPReviewEntity>();
            int individualTherapy = 0;
            int admission = 1;
            int close = 1;
            int resto = 0;

            foreach (var item in client_List.OrderBy(n => n.AdmisionDate))
            {
                if (item.IndividualTherapyFacilitator != null)
                {
                    individualTherapy = 1;
                }
                else
                {
                    individualTherapy = 0;
                }

                mtp = item.MTPs.FirstOrDefault(n => n.Active == true);
                review = mtp.MtpReviewList;

                if (item.Status == StatusType.Open)
                {
                    resto = (admission + review.Count()) - item.FarsFormList.Count();
                    if (resto > 0)
                    {
                        auditClient.NameClient = item.Name;
                        auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                        auditClient.Description = "Missing FARS (" + resto + ")";
                        auditClient.Active = 0;

                        auditClient_List.Add(auditClient);
                        auditClient = new AuditFARS();
                        resto = 0;
                    }
                }
                else
                {
                    resto = (admission + review.Count() + individualTherapy + close) - item.FarsFormList.Count();
                    if (resto > 0)
                    {
                        auditClient.NameClient = item.Name;
                        auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                        auditClient.Description = "Missing FARS ( " + resto + " )";
                        auditClient.Active = 0;

                        auditClient_List.Add(auditClient);
                        auditClient = new AuditFARS();
                        resto = 0;
                    }
                }
               
            }

            return View(auditClient_List);
        }

    }
}
