using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;
using KyoS.Common.Helpers;


namespace KyoS.Web.Controllers
{
    public class BriefsController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IReportHelper _reportHelper;
        private readonly DataContext _context;

        public BriefsController(IUserHelper userHelper, IConverterHelper converterHelper, ICombosHelper combosHelper, IRenderHelper renderHelper, DataContext context, IReportHelper reportHelper)
        {
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _context = context;
            _renderHelper = renderHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, Frontdesk")]
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
            List<ClientEntity> client = new List<ClientEntity>();

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }
            else
            {
                if (User.IsInRole("Manager") || User.IsInRole("Supervisor"))
                {
                    client = await _context.Clients

                                               .Include(g => g.Brief)
                                               .Include(u => u.Clinic)
                                               .ThenInclude(c => c.Setting)
                                               .Include(g => g.List_BehavioralHistory)

                                               .Where(n => n.Clinic.Id == user_logged.Clinic.Id
                                                        && n.Brief != null)
                                               .OrderBy(f => f.Name)
                                               .ToListAsync();
                }

                if (User.IsInRole("Documents_Assistant"))
                {
                    client = await _context.Clients

                                           .Include(g => g.Brief)
                                           .Include(u => u.Clinic)
                                           .ThenInclude(c => c.Setting)
                                           .Include(g => g.List_BehavioralHistory)

                                           .Where(n => n.Clinic.Id == user_logged.Clinic.Id && n.Brief.CreatedBy == user_logged.UserName)
                                           .OrderBy(f => f.Name)
                                           .ToListAsync();

                }

                if (User.IsInRole("Facilitator"))
                {
                    FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(m => m.LinkedUser == user_logged.UserName);
                    client = await _context.Clients

                                           .Include(g => g.Brief)
                                           .Include(u => u.Clinic)
                                           .ThenInclude(c => c.Setting)
                                           .Include(g => g.List_BehavioralHistory)
                                           .Where(n => n.Clinic.Id == user_logged.Clinic.Id
                                            && (n.IdFacilitatorGroup == facilitator.Id
                                             || n.IdFacilitatorPSR == facilitator.Id
                                             || n.IndividualTherapyFacilitator.Id == facilitator.Id)
                                              && n.Brief != null)
                                           .OrderBy(f => f.Name)
                                           .ToListAsync();
                }
                return View(client);
            }            
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult Create(int id = 0, int origi = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            BriefViewModel model = new BriefViewModel();

            if (User.IsInRole("Supervisor") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {

                    model = new BriefViewModel
                    {
                        IdClient = id,
                        Client_FK = id,
                        Client = _context.Clients.Include(n => n.LegalGuardian)
                                                 .Include(n => n.EmergencyContact)
                                                 .Include(n => n.MedicationList)
                                                 .Include(n => n.Client_Referred)
                                                 .ThenInclude(n => n.Referred)
                                                 .Include(n => n.List_BehavioralHistory)
                                                 .Include(f => f. Clients_Diagnostics)
                                                 .ThenInclude(f => f.Diagnostic)
                                                 .FirstOrDefault(n => n.Id == id),
                        Affect_Angry = false,
                        Affect_Anxious = false,
                        Affect_Appropriate = false,
                        Affect_Blunted = false,
                        Affect_Constricted = false,
                        Affect_Expansive = false,
                        Affect_Flat = false,
                        Affect_labile = false,
                        Affect_Other = false,
                        Affect_Tearful_Sad = false,
                        AlternativeDiagnosis = "",
                        Appearance_Bizarre = false,
                        Appearance_Cleaned = false,
                        Appearance_Disheveled = false,
                        Appearance_FairHygiene = false,
                        Appearance_WellGroomed = false,
                        BioH0031HN = false,
                        IDAH0031HO = false,
                        ClientAssessmentSituation = "",
                        CMH = false,
                        Comments = "",
                        DateBio = DateTime.Now,
                        DateSignatureLicensedPractitioner = DateTime.Now,
                        DateSignaturePerson = DateTime.Now,
                        DateSignatureSupervisor = DateTime.Now,
                        DateSignatureUnlicensedTherapist = DateTime.Now,
                        DoesClient = false,
                        DoYouOwn = false,
                        DoYouOwn_Explain = "",
                        FamilyAssessmentSituation = "",
                        FamilyEmotional = "",
                        HasTheClient = false,
                        HasTheClient_Explain = "",
                        HaveYouEverBeen = false,
                        HaveYouEverBeen_Explain = "",
                        HaveYouEverThought = false,
                        HaveYouEverThought_Explain = "",
                        IConcurWhitDiagnistic = false,
                        Id = 0,
                        Insight_Fair = false,
                        Insight_Good = false,
                        Insight_Other = false,
                        Insight_Poor = false,
                        Judgment_Fair = false,
                        Judgment_Good = false,
                        Judgment_Other = false,
                        Judgment_Poor = false,
                        Lacking_Location = false,
                        Lacking_Person = false,
                        Lacking_Place = false,
                        Lacking_Time = false,
                        LegalAssessment = "",
                        //LicensedPractitioner = user_logged.FullName,
                        Mood_Angry = false,
                        Mood_Anxious = false,
                        Mood_Depressed = false,
                        Mood_Euphoric = false,
                        Mood_Euthymic = false,
                        Mood_Maniac = false,
                        Mood_Other = false,
                        Motor_Agitated = false,
                        Motor_Akathisia = false,
                        Motor_Normal = false,
                        Motor_Other = false,
                        Motor_RestLess = false,
                        Motor_Retardation = false,
                        Motor_Tremor = false,
                        Oriented_FullOriented = false,
                        PresentingProblem = "",
                        Priv = false,
                        RiskToOther_Chronic = false,
                        RiskToOther_High = false,
                        RiskToOther_Low = false,
                        RiskToOther_Medium = false,
                        RiskToSelf_Chronic = false,
                        RiskToSelf_High = false,
                        RiskToSelf_Low = false,
                        RiskToSelf_Medium = false,
                        SafetyPlan = false,
                        Setting = "53",
                        Speech_Impoverished = false,
                        Speech_Loud = false,
                        Speech_Mumbled = false,
                        Speech_Normal = false,
                        Speech_Other = false,
                        Speech_Pressured = false,
                        Speech_Rapid = false,
                        Speech_Slow = false,
                        Speech_Slurred = false,
                        Speech_Stutters = false,
                        ThoughtContent_Delusions = false,
                        ThoughtContent_Delusions_Type = "",
                        ThoughtContent_Hallucinations = false,
                        ThoughtContent_Hallucinations_Type = "",
                        ThoughtContent_RealityBased = false,
                        ThoughtContent_Relevant = false,
                        ThoughtProcess_Blocking = false,
                        ThoughtProcess_Circumstantial = false,
                        ThoughtProcess_Disorganized = false,
                        ThoughtProcess_FightIdeas = false,
                        ThoughtProcess_GoalDirected = false,
                        ThoughtProcess_Irrational = false,
                        ThoughtProcess_LooseAssociations = false,
                        ThoughtProcess_Obsessive = false,
                        ThoughtProcess_Organized = false,
                        ThoughtProcess_Other = false,
                        ThoughtProcess_Preoccupied = false,
                        ThoughtProcess_Rigid = false,
                        ThoughtProcess_Tangential = false,
                        Treatmentrecomendations = "",
                        ClientDenied = false,
                        /*StartTime = DateTime.Now,
                        EndTime = DateTime.Now,*/
                        CreatedOn = DateTime.Now,
                        CreatedBy = user_logged.UserName,
                        AdmissionedFor = user_logged.FullName,
                        Code90791 = false
                    };
                    if (model.Client.LegalGuardian == null)
                        model.Client.LegalGuardian = new LegalGuardianEntity();
                    if (model.Client.EmergencyContact == null)
                        model.Client.EmergencyContact = new EmergencyContactEntity();
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    if (model.Client.Doctor == null)
                        model.Client.Doctor = new DoctorEntity();
                    if (model.Client.Client_Referred == null || model.Client.Client_Referred.Count() == 0)
                    {
                        Client_Referred client_referred = new Client_Referred();
                        model.Client.Client_Referred = new List<Client_Referred>();
                        model.Client.Client_Referred.Add(client_referred);
                        model.ReferralName = "Not have referred";
                    }
                    else
                    {
                        model.ReferralName = model.Client.Client_Referred.Where(n => n.Service == ServiceAgency.CMH).ElementAt(0).Referred.Name;
                    }
                   
                    model.LegalGuardianName = model.Client.LegalGuardian.Name;
                    model.LegalGuardianTelephone = model.Client.LegalGuardian.Telephone;
                    model.EmergencyContactName = model.Client.EmergencyContact.Name;
                    model.EmergencyContactTelephone = model.Client.EmergencyContact.Telephone;
                    model.RelationShipOfEmergencyContact = model.Client.RelationShipOfEmergencyContact.ToString();

                    ViewData["Origi"] = origi;
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> Create(BriefViewModel briefViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                BriefEntity briefEntity = _context.Brief.Find(briefViewModel.Id);
                if (briefEntity == null)
                {
                    DocumentsAssistantEntity documentAssistant = await _context.DocumentsAssistant.FirstOrDefaultAsync(m => m.LinkedUser == user_logged.UserName);
                    briefEntity = await _converterHelper.ToBriefEntity(briefViewModel, true, user_logged.UserName);

                    if (documentAssistant != null)
                    {
                        briefEntity.DocumentsAssistant = documentAssistant;
                        briefEntity.DateSignatureUnlicensedTherapist = DateTime.Now;
                    }
                    else
                    {
                        briefEntity.DateSignatureLicensedPractitioner = DateTime.Now;
                    }
                    _context.Brief.Add(briefEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (origi == 0)
                        {
                            return RedirectToAction("Index", "Briefs");
                        }
                        if (origi == 1)
                        {
                            return RedirectToAction("ClientswithoutBIO", "Bios");
                        }
                        
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the BRIEF.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", briefViewModel) });
                }
            }
            BriefViewModel model;
            model = new BriefViewModel
            {
                IdClient = briefViewModel.IdClient,
                Client = _context.Clients.Include(n => n.LegalGuardian)
                                                 .Include(n => n.EmergencyContact)
                                                 .Include(n => n.MedicationList)
                                                 .Include(n => n.Client_Referred)
                                                 .Include(n => n.List_BehavioralHistory)
                                                 .FirstOrDefault(n => n.Id == briefViewModel.IdClient),
                Affect_Angry = false,
                Affect_Anxious = false,
                Affect_Appropriate = false,
                Affect_Blunted = false,
                Affect_Constricted = false,
                Affect_Expansive = false,
                Affect_Flat = false,
                Affect_labile = false,
                Affect_Other = false,
                Affect_Tearful_Sad = false,
                AlternativeDiagnosis = "",
                Appearance_Bizarre = false,
                Appearance_Cleaned = false,
                Appearance_Disheveled = false,
                Appearance_FairHygiene = false,
                Appearance_WellGroomed = false,
                BioH0031HN = false,
                IDAH0031HO = false,
                ClientAssessmentSituation = "",
                CMH = false,
                Comments = "",
                DateBio = DateTime.Now,
                DateSignatureLicensedPractitioner = DateTime.Now,
                DateSignaturePerson = DateTime.Now,
                DateSignatureSupervisor = DateTime.Now,
                DateSignatureUnlicensedTherapist = DateTime.Now,
                DoesClient = false,
                DoYouOwn = false,
                DoYouOwn_Explain = "",
                FamilyAssessmentSituation = "",
                FamilyEmotional = "",
                HasTheClient = false,
                HasTheClient_Explain = "",
                HaveYouEverBeen = false,
                HaveYouEverBeen_Explain = "",
                HaveYouEverThought = false,
                HaveYouEverThought_Explain = "",
                IConcurWhitDiagnistic = false,
                Id = 0,
                Insight_Fair = false,
                Insight_Good = false,
                Insight_Other = false,
                Insight_Poor = false,
                Judgment_Fair = false,
                Judgment_Good = false,
                Judgment_Other = false,
                Judgment_Poor = false,
                Lacking_Location = false,
                Lacking_Person = false,
                Lacking_Place = false,
                Lacking_Time = false,
                LegalAssessment = "",
                Mood_Angry = false,
                Mood_Anxious = false,
                Mood_Depressed = false,
                Mood_Euphoric = false,
                Mood_Euthymic = false,
                Mood_Maniac = false,
                Mood_Other = false,
                Motor_Agitated = false,
                Motor_Akathisia = false,
                Motor_Normal = false,
                Motor_Other = false,
                Motor_RestLess = false,
                Motor_Retardation = false,
                Motor_Tremor = false,
                Oriented_FullOriented = false,
                PresentingProblem = "",
                Priv = false,
                RiskToOther_Chronic = false,
                RiskToOther_High = false,
                RiskToOther_Low = false,
                RiskToOther_Medium = false,
                RiskToSelf_Chronic = false,
                RiskToSelf_High = false,
                RiskToSelf_Low = false,
                RiskToSelf_Medium = false,
                SafetyPlan = false,
                Setting = "",
                Speech_Impoverished = false,
                Speech_Loud = false,
                Speech_Mumbled = false,
                Speech_Normal = false,
                Speech_Other = false,
                Speech_Pressured = false,
                Speech_Rapid = false,
                Speech_Slow = false,
                Speech_Slurred = false,
                Speech_Stutters = false,
                ThoughtContent_Delusions = false,
                ThoughtContent_Delusions_Type = "",
                ThoughtContent_Hallucinations = false,
                ThoughtContent_Hallucinations_Type = "",
                ThoughtContent_RealityBased = false,
                ThoughtContent_Relevant = false,
                ThoughtProcess_Blocking = false,
                ThoughtProcess_Circumstantial = false,
                ThoughtProcess_Disorganized = false,
                ThoughtProcess_FightIdeas = false,
                ThoughtProcess_GoalDirected = false,
                ThoughtProcess_Irrational = false,
                ThoughtProcess_LooseAssociations = false,
                ThoughtProcess_Obsessive = false,
                ThoughtProcess_Organized = false,
                ThoughtProcess_Other = false,
                ThoughtProcess_Preoccupied = false,
                ThoughtProcess_Rigid = false,
                ThoughtProcess_Tangential = false,
                Treatmentrecomendations = "",
                ClientDenied = false,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now,
                Code90791 = false

            };
            
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult Edit(int id = 0, int origi = 0)
        {
            BriefEntity entity = _context.Brief

                                         .Include(m => m.Client)
                                         .Include(n => n.Client.LegalGuardian)
                                         .Include(n => n.Client.EmergencyContact)
                                         .Include(n => n.Client.MedicationList)
                                         .Include(n => n.Client.Client_Referred)
                                         .ThenInclude(n => n.Referred)
                                         .Include(n => n.Client.List_BehavioralHistory)
                                         .Include(f => f.Client)
                                         .ThenInclude(f => f.Clients_Diagnostics)
                                         .ThenInclude(f => f.Diagnostic)
                                         .FirstOrDefault(i => i.Client.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Create", new { id = id });
            }

            BriefViewModel model;

            if (User.IsInRole("Supervisor") || User.IsInRole("Documents_Assistant"))
            {
                UserEntity user_logged = _context.Users

                                                 .Include(u => u.Clinic)

                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {
                    model = _converterHelper.ToBriefViewModel(entity);
                    if (model.Client.LegalGuardian == null)
                        model.Client.LegalGuardian = new LegalGuardianEntity();
                    if (model.Client.EmergencyContact == null)
                        model.Client.EmergencyContact = new EmergencyContactEntity();
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    if (model.Client.Doctor == null)
                        model.Client.Doctor = new DoctorEntity();
                    if (model.Client.Client_Referred == null || model.Client.Client_Referred.Where(n => n.Service == ServiceAgency.CMH).Count() == 0)
                    {
                        Client_Referred client_referred = new Client_Referred();
                        model.Client.Client_Referred = new List<Client_Referred>();
                        model.Client.Client_Referred.Add(client_referred);
                        model.ReferralName = "Not have referred";
                    }
                    else
                    {
                        model.ReferralName = model.Client.Client_Referred.Where(n => n.Service == ServiceAgency.CMH).ElementAt(0).Referred.Name;
                    }

                    if (model.Client.FarsFormList == null)
                        model.Client.FarsFormList = new List<FarsFormEntity>();
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    if (model.Client.List_BehavioralHistory == null)
                        model.Client.List_BehavioralHistory = new List<Bio_BehavioralHistoryEntity>();

                    model.LegalGuardianName = model.Client.LegalGuardian.Name;
                    model.LegalGuardianTelephone = model.Client.LegalGuardian.Telephone;
                    model.EmergencyContactName = model.Client.EmergencyContact.Name;
                    model.EmergencyContactTelephone = model.Client.EmergencyContact.Telephone;
                    model.RelationShipOfEmergencyContact = model.Client.RelationShipOfEmergencyContact.ToString();

                    ViewData["origi"] = origi;
                    return View(model);
                }
            }

            model = new BriefViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> Edit(BriefViewModel briefViewModel, int origi = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                BriefEntity briefEntity = await _converterHelper.ToBriefEntity(briefViewModel, false, user_logged.UserName);
                _context.Brief.Update(briefEntity);
                try
                {
                    List<MessageEntity> messages = briefEntity.Messages.Where(m => (m.Status == MessageStatus.NotRead && m.Notification == false)).ToList();
                    //todos los mensajes no leidos que tiene el bio los pongo como leidos
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
                            Discharge = null,
                            Mtp = null,
                            Bio = null,
                            Brief = briefEntity,
                            Title = "Update on reviewed Brief",
                            Text = $"The Brief document of {briefEntity.Client.Name} that was evaluated on {briefEntity.DateBio.ToShortDateString()} was rectified",
                            From = value.To,
                            To = value.From,
                            DateCreated = DateTime.Now,
                            Status = MessageStatus.NotRead,
                            Notification = true
                        };
                        _context.Add(notification);
                    }

                    await _context.SaveChangesAsync();
                    if(origi == 0)
                    {
                        return RedirectToAction("Index", "Briefs");
                    }
                    if (origi == 1)
                    {
                        return RedirectToAction("MessagesOfBrief", "Messages");
                    }
                    if (origi == 2)
                    {
                        return RedirectToAction("BriefWithReview", "Briefs");
                    }
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", briefViewModel) });
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult CreateBehavioralModal(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            Bio_BehavioralHistoryViewModel model;

            if (User.IsInRole("Supervisor") || User.IsInRole("Documents_Assistant"))
            {
                if (user_logged.Clinic != null)
                {

                    model = new Bio_BehavioralHistoryViewModel
                    {
                        IdClient = id,
                        Client = _context.Clients.Include(n => n.List_BehavioralHistory).FirstOrDefault(n => n.Id == id),
                        Date = "",
                        Id = 0,
                        Problem = ""
                    };
                    if (model.Client.List_BehavioralHistory == null)
                        model.Client.List_BehavioralHistory = new List<Bio_BehavioralHistoryEntity>();
                    return View(model);
                }
            }

            model = new Bio_BehavioralHistoryViewModel
            {
                IdClient = id,
                Client = _context.Clients.Include(n => n.List_BehavioralHistory).FirstOrDefault(n => n.Id == id),
                Date = "",
                Id = 0,
                Problem = ""
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> CreateBehavioralModal(Bio_BehavioralHistoryViewModel bioViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                
                Bio_BehavioralHistoryEntity bioEntity = await _converterHelper.ToBio_BehaviorEntity(bioViewModel, true);
                _context.Bio_BehavioralHistory.Add(bioEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<Bio_BehavioralHistoryEntity> bio = await _context.Bio_BehavioralHistory
                                                                          .Include(g => g.Client)
                                                                          .Where(g => g.Client.Id == bioViewModel.IdClient)
                                                                          .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewBehavioralHealth", bio) });
                        
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
               
            }
            Bio_BehavioralHistoryEntity model;
            model = new Bio_BehavioralHistoryEntity
            {

                Date = "",
                Id = 0,
                Problem = ""
            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
        public async Task<IActionResult> IndexBehavioralHealthHistory(int idError = 0)
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

                                              .Include(f => f.Clients_Diagnostics)
                                              .Include(g => g.Brief)
                                              .Include(g => g.List_BehavioralHistory)

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());

                if (User.IsInRole("Facilitator"))
                {
                    return View(await _context.Clients
                                              .Include(f => f.Clients_Diagnostics)
                                              .Include(g => g.Brief)
                                              .Include(g => g.List_BehavioralHistory)

                                              .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                              .OrderBy(f => f.Name)
                                              .ToListAsync());
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult EditBehavioralModal(int id = 0)
        {
            Bio_BehavioralHistoryViewModel model;

            if (User.IsInRole("Supervisor") || User.IsInRole("Documents_Assistant"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    Bio_BehavioralHistoryEntity Behavioral = _context.Bio_BehavioralHistory
                                                                     .Include(m => m.Client)
                                                                     .ThenInclude(m => m.List_BehavioralHistory)
                                                                     .FirstOrDefault(m => m.Id == id);
                    if (Behavioral == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToBio_BehaviorViewModel(Behavioral);

                        return View(model);
                    }

                }
            }

            model = new Bio_BehavioralHistoryViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> EditBehavioralModal(Bio_BehavioralHistoryViewModel behavioralViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                Bio_BehavioralHistoryEntity behavioralEntity = await _converterHelper.ToBio_BehaviorEntity(behavioralViewModel, false);
                _context.Bio_BehavioralHistory.Update(behavioralEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<Bio_BehavioralHistoryEntity> bio = await _context.Bio_BehavioralHistory
                                                                                .Include(g => g.Client)
                                                                                .Where(g => g.Client.Id == behavioralViewModel.IdClient)
                                                                                .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewBehavioralHealth", bio) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditBehavioralModal", behavioralViewModel) });
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> DeleteBehavioral(int? id, int origin = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            Bio_BehavioralHistoryEntity Bio_BehavioralEntity = await _context.Bio_BehavioralHistory
                                                                             .Include(n => n.Client)
                                                                             .FirstOrDefaultAsync(s => s.Id == id);
            if (Bio_BehavioralEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Bio_BehavioralHistory.Remove(Bio_BehavioralEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(IndexBehavioralHealthHistory));            
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant, Frontdesk")]
        public IActionResult PrintBrief(int id)
        {
            BriefEntity entity = _context.Brief

                                         .Include(b => b.Client)
                                            .ThenInclude(c => c.Clinic)

                                         .Include(b => b.Client)
                                            .ThenInclude(c => c.EmergencyContact)

                                         .Include(b => b.Client)
                                            .ThenInclude(c => c.LegalGuardian)

                                         .Include(b => b.Client)
                                            .ThenInclude(c => c.List_BehavioralHistory)

                                         .Include(b => b.Client)
                                            .ThenInclude(c => c.MedicationList)

                                         .Include(b => b.Client)
                                            .ThenInclude(c => c.Clients_Diagnostics)
                                                .ThenInclude(cd => cd.Diagnostic)

                                         .Include(b => b.Client)
                                            .ThenInclude(c => c.Doctor)

                                         .Include(b => b.Supervisor)

                                         .Include(b => b.DocumentsAssistant)

                                         .Include(b => b.Client)
                                            .ThenInclude(c => c.Client_Referred)
                                                .ThenInclude(cr => cr.Referred)

                                         .FirstOrDefault(i => (i.Id == id));
            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            if (entity.Client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
            {
                Stream stream = _reportHelper.FloridaSocialHSBriefReport(entity);                
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "DREAMS MENTAL HEALTH INC")
            {
                Stream stream = _reportHelper.DreamsMentalHealthBriefReport(entity);                
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
            {
                Stream stream = _reportHelper.CommunityHTCBriefReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "PRINCIPLE CARE CENTER INC")
            {
                Stream stream = _reportHelper.PrincipleCCIBriefReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "SAPPHIRE MENTAL HEALTH CENTER LLC")
            {
                Stream stream = _reportHelper.SapphireMHCBriefReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Client.Clinic.Name == "SOUTH FLORIDA MENTAL HEALTH & RECOVERY")
            {
                Stream stream = _reportHelper.SouthFloridaMHRBriefReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            return null;
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult CreateMedicationModal(int id = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            MedicationViewModel model;

            if (User.IsInRole("Documents_Assistant") || User.IsInRole("Supervisor"))
            {


                if (user_logged.Clinic != null)
                {

                    model = new MedicationViewModel
                    {
                        IdClient = id,
                        Client = _context.Clients.Include(n => n.MedicationList).FirstOrDefault(n => n.Id == id),
                        Id = 0,
                        Dosage = "",
                        Frequency = "",
                        Name = "",
                        Prescriber = ""

                    };
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    return View(model);
                }
            }

            model = new MedicationViewModel
            {
                IdClient = id,
                Client = _context.Clients.Include(n => n.MedicationList).FirstOrDefault(n => n.Id == id),
                Id = 0,
                Dosage = "",
                Frequency = "",
                Name = "",
                Prescriber = ""
            };
            if (model.Client.MedicationList == null)
                model.Client.MedicationList = new List<MedicationEntity>();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> CreateMedicationModal(MedicationViewModel medicationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                
                MedicationEntity medicationEntity = await _converterHelper.ToMedicationEntity(medicationViewModel, true);
                _context.Medication.Add(medicationEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    List<MedicationEntity> medication = await _context.Medication
                                                                      .Include(g => g.Client)
                                                                      .Where(g => g.Client.Id == medicationViewModel.IdClient)
                                                                      .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewMedication", medication) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
                
            }
            MedicationViewModel model;
            model = new MedicationViewModel
            {
                IdClient = medicationViewModel.IdClient,
                Client = _context.Clients.Find(medicationViewModel.IdClient),
                Id = medicationViewModel.Id,
                Dosage = medicationViewModel.Dosage,
                Frequency = medicationViewModel.Frequency,
                Name = medicationViewModel.Name,
                Prescriber = medicationViewModel.Prescriber

            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateMedicationModal", medicationViewModel) });
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult EditMedicationModal(int id = 0)
        {
            MedicationViewModel model;

            if (User.IsInRole("Documents_Assistant") || User.IsInRole("Supervisor"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    MedicationEntity Medication = _context.Medication
                                                         .Include(m => m.Client)
                                                         .ThenInclude(m => m.MedicationList)
                                                         .FirstOrDefault(m => m.Id == id);
                    if (Medication == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToMedicationViewModel(Medication);

                        return View(model);
                    }

                }
            }

            model = new MedicationViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> EditMedicationModal(MedicationViewModel medicationViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                MedicationEntity medicationEntity = await _converterHelper.ToMedicationEntity(medicationViewModel, false);
                _context.Medication.Update(medicationEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<MedicationEntity> medication = await _context.Medication
                                                                      .Include(g => g.Client)
                                                                      .Where(g => g.Client.Id == medicationViewModel.IdClient)
                                                                      .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewMedication", medication) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditMedicationModal", medicationViewModel) });
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> DeleteMedication(int? id, int origin = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MedicationEntity medicationEntity = await _context.Medication
                                                              .Include(n => n.Client)
                                                              .FirstOrDefaultAsync(s => s.Id == id);
            if (medicationEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Medication.Remove(medicationEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            if (origin == 1)
                return RedirectToAction(nameof(Index));
            else
            {
                List<MedicationEntity> medication = await _context.Medication
                                                                  .Include(g => g.Client)
                                                                  .Where(g => g.Client.Id == medicationEntity.Client.Id)
                                                                  .ToListAsync();

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewMedication", medication) });
            }
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant, Manager, Facilitator, Frontdesk")]
        public IActionResult EditReadOnly(int id = 0, int origi = 0)
        {
            BriefEntity entity = _context.Brief

                                         .Include(m => m.Client)
                                         .Include(n => n.Client.LegalGuardian)
                                         .Include(n => n.Client.EmergencyContact)
                                         .Include(n => n.Client.MedicationList)
                                         .Include(n => n.Client.Client_Referred)
                                         .ThenInclude(n => n.Referred)
                                         .Include(n => n.Client.List_BehavioralHistory)
                                         .Include(m => m.Client)
                                         .ThenInclude(n => n.Clients_Diagnostics)
                                         .ThenInclude(n => n.Diagnostic)

                                         .FirstOrDefault(i => i.Id == id);

            if (entity == null)
            {
                return RedirectToAction("Create", new { id = id });
            }

            BriefViewModel model;

           
                UserEntity user_logged = _context.Users

                                                 .Include(u => u.Clinic)

                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {
                    model = _converterHelper.ToBriefViewModel(entity);
                    if (model.Client.LegalGuardian == null)
                        model.Client.LegalGuardian = new LegalGuardianEntity();
                    if (model.Client.EmergencyContact == null)
                        model.Client.EmergencyContact = new EmergencyContactEntity();
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    if (model.Client.Doctor == null)
                        model.Client.Doctor = new DoctorEntity();
                   
                    if (model.Client.Client_Referred == null || model.Client.Client_Referred.Where(n => n.Service == ServiceAgency.CMH).Count() == 0)
                    {
                        Client_Referred client_referred = new Client_Referred();
                        model.Client.Client_Referred = new List<Client_Referred>();
                        model.Client.Client_Referred.Add(client_referred);
                        model.ReferralName = "Not have referred";
                    }
                    else
                    {
                        model.ReferralName = model.Client.Client_Referred.Where(n => n.Service == ServiceAgency.CMH).ElementAt(0).Referred.Name;
                    }

                    if (model.Client.FarsFormList == null)
                        model.Client.FarsFormList = new List<FarsFormEntity>();
                    if (model.Client.MedicationList == null)
                        model.Client.MedicationList = new List<MedicationEntity>();
                    if (model.Client.List_BehavioralHistory == null)
                        model.Client.List_BehavioralHistory = new List<Bio_BehavioralHistoryEntity>();

                    model.LegalGuardianName = model.Client.LegalGuardian.Name;
                    model.LegalGuardianTelephone = model.Client.LegalGuardian.Telephone;
                    model.EmergencyContactName = model.Client.EmergencyContact.Name;
                    model.EmergencyContactTelephone = model.Client.EmergencyContact.Telephone;
                    model.RelationShipOfEmergencyContact = model.Client.RelationShipOfEmergencyContact.ToString();

                    ViewData["origi"] = origi;
                    return View(model);
                }

            ViewData["origi"] = origi;
            model = new BriefViewModel();
            return View(model);
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> FinishEditing(int id)
        {
            BriefEntity brief = await _context.Brief.FirstOrDefaultAsync(n => n.Id == id);
            if (User.IsInRole("Supervisor"))
            {
                brief.Status = BioStatus.Approved;
                brief.DateSignatureSupervisor = DateTime.Now;
                brief.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.LinkedUser == User.Identity.Name);
            }
            else
            {
                brief.Status = BioStatus.Pending;
            }

            _context.Update(brief);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Approve(int id, int origi = 0)
        {

            BriefEntity brief = await _context.Brief.FirstOrDefaultAsync(n => n.Id == id);


            brief.Status = BioStatus.Approved;
            brief.DateSignatureSupervisor = DateTime.Now;
            brief.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.LinkedUser == User.Identity.Name);
            _context.Update(brief);

            await _context.SaveChangesAsync();

            if (origi == 1)
            {
                return RedirectToAction(nameof(Pending));
            }
            if (origi == 2)
            {
                return RedirectToAction("Notifications", "Messages");
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Supervisor, Manager, Documents_Assistant, Frontdesk")]
        public async Task<IActionResult> Pending(int idError = 0)
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
                    DocumentsAssistantEntity documentAssistant = _context.DocumentsAssistant.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                    if (User.IsInRole("Documents_Assistant"))
                    {
                        return View(await _context.Brief

                                                  .Include(a => a.Client)
                                                  .ThenInclude(a => a.Clinic)

                                                  .Include(f => f.Messages.Where(m => m.Notification == false))

                                                  .Where(a => (a.Client.Clinic.Id == clinic.Id)
                                                            && a.Status == BioStatus.Pending && (a.DocumentsAssistant.Id == documentAssistant.Id))
                                                  .OrderBy(a => a.Client.Clinic.Name).ToListAsync());

                    }
                    else
                    {
                        return View(await _context.Brief

                                                  .Include(a => a.Client)
                                                  .ThenInclude(a => a.Clinic)

                                                  .Include(f => f.Messages.Where(m => m.Notification == false))

                                                  .Where(a => (a.Client.Clinic.Id == clinic.Id)
                                                            && a.Status == BioStatus.Pending)
                                                  .OrderBy(a => a.Client.Clinic.Name).ToListAsync());

                    }

                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor")]
        public IActionResult AddMessageEntity(int id = 0, int origi = 0)
        {
            if (id == 0)
            {
                return View(new MessageViewModel());
            }
            else
            {
                MessageViewModel model = new MessageViewModel()
                {
                    IdBrief = id,
                    Origin = origi
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
                model.To = model.Brief.CreatedBy;
                _context.Add(model);
                await _context.SaveChangesAsync();
            }

            if (messageViewModel.Origin == 1)
                return RedirectToAction("Pending");

            if (messageViewModel.Origin == 2)
                return RedirectToAction("Notifications","Messages");

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Documents_Assistant, Supervisor")]
        public async Task<IActionResult> BriefWithReview()
        {
            if (User.IsInRole("Documents_Assistant"))
            {
                List<BriefEntity> salida = await _context.Brief
                                                         .Include(wc => wc.Client)
                                                         .Include(wc => wc.DocumentsAssistant)
                                                         .Include(wc => wc.Messages.Where(m => m.Notification == false))
                                                         .Where(wc => (wc.DocumentsAssistant.LinkedUser == User.Identity.Name
                                                               && wc.Status == BioStatus.Pending
                                                               && wc.Messages.Count() > 0))
                                                         .ToListAsync();


                return View(salida);
            }

            if (User.IsInRole("Supervisor"))
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    List<BriefEntity> salida = await _context.Brief
                                                             .Include(wc => wc.Client)
                                                             .Include(wc => wc.DocumentsAssistant)
                                                             .Include(wc => wc.Messages.Where(m => m.Notification == false))
                                                             .Where(wc => (wc.Client.Clinic.Id == user_logged.Clinic.Id
                                                                 && wc.Status == BioStatus.Pending
                                                                 && wc.Messages.Count() > 0))
                                                             .ToListAsync();
                    return View(salida);
                }
            }

            return View();
        }

        [Authorize(Roles = "Manager, Supervisor, Documents_Assistant")]
        public IActionResult AddDiagnostic(int id = 0)
        {
            if (id > 0)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                Client_DiagnosticViewModel model = new Client_DiagnosticViewModel
                {
                    IdDiagnostic = 0,
                    Diagnostics = _combosHelper.GetComboDiagnosticsByClient(id),
                    IdClient = id
                };
                return View(model);
            }
            else
            {
                //Edit
                return View(new Client_DiagnosticViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> AddDiagnostic(Client_DiagnosticViewModel client_diagnosticViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (ModelState.IsValid)
            {
                DiagnosticEntity diagnostic = await _context.Diagnostics.FirstOrDefaultAsync(d => d.Id == client_diagnosticViewModel.IdDiagnostic);
                ClientEntity client = await _context.Clients.FirstOrDefaultAsync(n => n.Id == client_diagnosticViewModel.IdClient);

                Client_Diagnostic client_diagnostic = new Client_Diagnostic
                {
                    Id = 0,
                    Client = client,
                    Diagnostic = diagnostic,
                    Principal = client_diagnosticViewModel.Principal
                   
                };
                
                _context.Add(client_diagnostic);
                try
                {
                    await _context.SaveChangesAsync();
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDiagnostic", _context.Clients_Diagnostics.Include(n => n.Diagnostic).Where(d => (d.Client.Id == client.Id )).ToList()) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the diagnostic: {diagnostic.Code}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddDiagnostic", client_diagnosticViewModel) });
        }

        [Authorize(Roles = "Manager, Supervisor, Documents_Assistant")]
        public IActionResult DeleteDiagnostic(int id = 0)
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
                return View(new Client_DiagnosticViewModel());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager, Supervisor, Documents_Assistant")]
        public async Task<IActionResult> DeleteDiagnostic(int id, DeleteViewModel ClientDiagnosticsViewModel)
        {
            if (ModelState.IsValid)
            {
                Client_Diagnostic client_diagnostic = await _context.Clients_Diagnostics.Include(n => n.Client).FirstOrDefaultAsync(d => d.Id == ClientDiagnosticsViewModel.Id_Element);
                try
                {
                    _context.Clients_Diagnostics.Remove(client_diagnostic);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return RedirectToAction("Index", new { idError = 1 });
                }

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDiagnostic", _context.Clients_Diagnostics.Include(n => n.Diagnostic).Where(d => d.Client.Id == client_diagnostic.Client.Id).ToList()) });
            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDiagnostic", _context.Clients_Diagnostics.Where(d => d.Client.Id == 0).ToList()) });
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Frontdesk")]
        public async Task<IActionResult> AuditBrief()
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic || !user_logged.Clinic.Setting.MHProblems)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<AuditBIO> auditClient_List = new List<AuditBIO>();
            AuditBIO auditClient = new AuditBIO();

            List<ClientEntity> client_List = new List<ClientEntity>();

            if (User.IsInRole("Manager") || User.IsInRole("Supervisor"))
            {
                client_List = _context.Clients
                                      .Include(m => m.Bio)
                                      .Include(m => m.Brief)
                                      .Include(m => m.List_BehavioralHistory)
                                      .Include(m => m.MedicationList)
                                      .Where(n => n.Clinic.Id == user_logged.Clinic.Id)
                                      .ToList();

            }
            else
            {
                if (User.IsInRole("Facilitator"))
                {
                    FacilitatorEntity facilitator = await _context.Facilitators.FirstOrDefaultAsync(f => f.LinkedUser == user_logged.UserName);

                    client_List = _context.Clients
                                         
                                          .Include(m => m.IndividualTherapyFacilitator)
                                          .Where(n => n.Clinic.Id == user_logged.Clinic.Id
                                              && n.MTPs.Count() > 0 && (n.IdFacilitatorPSR == facilitator.Id || n.IndividualTherapyFacilitator.Id == facilitator.Id || n.IdFacilitatorGroup == facilitator.Id))
                                          .ToList();

                }

            }

            foreach (var item in client_List.OrderBy(n => n.AdmisionDate))
            {
                if (item.Brief == null && item.Bio == null)
                {
                    auditClient.NameClient = item.Name;
                    auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                    auditClient.Description = "The client has no BRIEF";
                    auditClient.Active = 0;

                    auditClient_List.Add(auditClient);
                    auditClient = new AuditBIO();
                }
                else
                {
                    if (item.Brief != null)
                    {
                        if (item.AdmisionDate > item.Brief.DateBio)
                        {
                            auditClient.NameClient = item.Name;
                            auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                            auditClient.Description = "The admission date is after the BRIEF date";
                            auditClient.Active = 0;

                            auditClient_List.Add(auditClient);
                            auditClient = new AuditBIO();
                        }
                        if (item.List_BehavioralHistory.Count() == 0)
                        {
                            auditClient.NameClient = item.Name;
                            auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                            auditClient.Description = "The client has no behavioral health history";
                            auditClient.Active = 0;

                            auditClient_List.Add(auditClient);
                            auditClient = new AuditBIO();
                        }
                        if (item.MedicationList.Count() == 0)
                        {
                            auditClient.NameClient = item.Name;
                            auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                            auditClient.Description = "The client has no medication";
                            auditClient.Active = 1;

                            auditClient_List.Add(auditClient);
                            auditClient = new AuditBIO();
                        }
                        if (item.Brief.Status == BioStatus.Edition)
                        {
                            auditClient.NameClient = item.Name;
                            auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                            auditClient.Description = "Brief is edition";
                            auditClient.Active = 1;

                            auditClient_List.Add(auditClient);
                            auditClient = new AuditBIO();
                        }
                        if (item.Brief.Status == BioStatus.Pending)
                        {
                            auditClient.NameClient = item.Name;
                            auditClient.AdmissionDate = item.AdmisionDate.ToShortDateString();
                            auditClient.Description = "Brief is pending";
                            auditClient.Active = 1;

                            auditClient_List.Add(auditClient);
                            auditClient = new AuditBIO();
                        }
                    }
                    
                }
         
            }

            return View(auditClient_List);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ReturnTo(int? id, int clientId = 0, BioStatus aStatus = BioStatus.Edition)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            BriefEntity briefEntity = await _context.Brief.FirstOrDefaultAsync(s => s.Id == id);
            if (briefEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                briefEntity.Status = aStatus;
                _context.Brief.Update(briefEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("ClientHistory", "Clients", new { idClient = clientId });
        }

    }
}
