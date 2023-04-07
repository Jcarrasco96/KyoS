using KyoS.Common.Enums;
using KyoS.Common.Helpers;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    public class MTPsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IReportHelper _reportHelper;
        private readonly ITranslateHelper _translateHelper;
        public MTPsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IReportHelper reportHelper, ITranslateHelper translateHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
            _reportHelper = reportHelper;
            _translateHelper = translateHelper;
        }

        [Authorize(Roles = "Supervisor, Manager, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> Index(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            if (User.IsInRole("Admin"))
            {
                return View(await _context.MTPs
                                          .Include(m => m.Client)
                                          .ThenInclude(c => c.Clinic)
                                          .ThenInclude(c => c.Setting)
                                          .Include(m => m.MtpReviewList)
                                          .OrderBy(m => m.Client.Clinic.Name).ToListAsync());
            }
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
                    FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                    if (User.IsInRole("Facilitator"))
                    {
                        return View(await _context.MTPs
                                                  .Include(m => m.MtpReviewList)

                                                  .Include(c => c.Client)
                                                  .ThenInclude(c => c.Clinic)
                                                  .ThenInclude(c => c.Setting)
                                                  .Include(c => c.Client.Group)
                                                  .Where(m => (m.Client.Clinic.Id == clinic.Id
                                                        && (m.Client.IdFacilitatorPSR == facilitator.Id
                                                            || m.Client.IndividualTherapyFacilitator.Id == facilitator.Id
                                                            || m.Client.IdFacilitatorGroup == facilitator.Id)))
                                                  .OrderBy(m => m.Client.Clinic.Name).ToListAsync());
                    }
                    if (User.IsInRole("Manager") || User.IsInRole("Supervisor"))
                    {
                        return View(await _context.MTPs
                                              .Include(m => m.Client)
                                              .ThenInclude(c => c.Clinic)
                                              .ThenInclude(c => c.Setting)
                                              .Include(m => m.MtpReviewList)
                                              .Where(m => m.Client.Clinic.Id == clinic.Id)
                                              .OrderBy(m => m.Client.Clinic.Name).ToListAsync());
                    }
                    if (User.IsInRole("Documents_Assistant"))
                    {
                        return View(await _context.MTPs
                                              .Include(m => m.Client)
                                              .ThenInclude(c => c.Clinic)
                                              .ThenInclude(c => c.Setting)
                                              .Include(m => m.MtpReviewList)
                                              .Where(m => m.Client.Clinic.Id == clinic.Id && m.CreatedBy == user_logged.UserName)
                                              .OrderBy(m => m.Client.Clinic.Name).ToListAsync());
                    }
                    return RedirectToAction("Home/Error404");
                }
                else
                    return View(null);
            }
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult Create(int id = 0, int idClient = 0, bool review = false, int origin = 0)
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
                        ViewBag.Creado = "S";
                    }
                    else
                    {
                        ViewBag.Creado = "N";
                    }
                    
                }
            }

            //this.DeleteGoalsTemp(idClient);    //decide where to delete the temp

            MTPViewModel model = new MTPViewModel();

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    List<SelectListItem> list = _context.Clients.Where(c => c.Id == idClient).Select(c => new SelectListItem
                    {
                        Text = $"{c.Name}",
                        Value = $"{c.Id}"
                    }).ToList();

                    if (list.Count() > 0)
                    {
                        model = new MTPViewModel
                        {
                            IdClient = idClient,
                            Clients = list,
                            MTPDevelopedDate = DateTime.Today,
                            NumberOfMonths = 6,
                            Modality = "PSR",
                            Frecuency = "Four times per week",
                            Setting = "53",
                            Review = review,

                            AdmissionDateMTP = DateTime.Now,
                            DateOfUpdate = DateTime.Now,
                            Psychosocial = true,
                            MedicationCode = "T1015",
                            IndividualCode = "H2019 HR",
                            FamilyCode = "H2019 HR",
                            GroupCode = "90853",
                            PsychosocialCode = "H2017",
                            PsychosocialUnits = 16,
                            PsychosocialFrecuency = "4 times for week",
                            PsychosocialDuration = 6,
                            Substance = false,
                            Legal = false,
                            Health = false,
                            Paint = false,
                            Other = false,
                            Client = _context.Clients
                                             .Include(c => c.Clients_Diagnostics)
                                             .ThenInclude(cd => cd.Diagnostic)
                                             .First(n => n.Id == idClient),
                            AdmissionedFor = user_logged.FullName,
                            GoalTempList = _context.GoalsTemp.Include(m => m.ObjetiveTempList).Where(m => m.IdClient == idClient && m.UserName == user_logged.UserName).ToList()

                    };
                    }
                    else
                    {
                        model = new MTPViewModel
                        {
                            Clients = _combosHelper.GetComboClientsByClinic(user_logged.Clinic.Id),
                            MTPDevelopedDate = DateTime.Today,
                            NumberOfMonths = 6,
                            Modality = "PSR",
                            Frecuency = "Four times per week",
                            Setting = "53",
                            Review = review,

                            AdmissionDateMTP = DateTime.Now,
                            DateOfUpdate = DateTime.Now,
                            Psychosocial = true,
                            MedicationCode = "T1015",
                            IndividualCode = "H2019 HR",
                            FamilyCode = "H2019 HR",
                            GroupCode = "90853",
                            PsychosocialCode = "H2017",
                            PsychosocialUnits = 16,
                            PsychosocialFrecuency = "4 times for week",
                            PsychosocialDuration = 6,
                            Substance = false,
                            Legal = false,
                            Health = false,
                            Paint = false,
                            Other = false,
                            Client = new ClientEntity(),
                            AdmissionedFor = user_logged.FullNameWithDocument,
                            GoalTempList = new List<GoalsTempEntity>()

                        };
                        model.Client.Clients_Diagnostics = new List<Client_Diagnostic>();
                        model.Client.Name = "null";
                    }
                    ViewData["origin"] = origin;
                    return View(model);
                }
            }

            model = new MTPViewModel
            {
                Clients = _combosHelper.GetComboClients(),
                MTPDevelopedDate = DateTime.Today
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> Create(MTPViewModel mtpViewModel, IFormCollection form, int origin = 0)
        {
            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                ClientEntity client = await _context.Clients.FindAsync(mtpViewModel.IdClient);
                DocumentsAssistantEntity documentAssistant = await _context.DocumentsAssistant.FirstOrDefaultAsync(m => m.LinkedUser == user_logged.UserName);
                string gender_problems = string.Empty;

                if (!string.IsNullOrEmpty(mtpViewModel.InitialDischargeCriteria))
                {
                    mtpViewModel.InitialDischargeCriteria = (mtpViewModel.InitialDischargeCriteria.Last() == '.') ? mtpViewModel.InitialDischargeCriteria : $"{mtpViewModel.InitialDischargeCriteria}.";
                    if (this.GenderEvaluation(client.Gender, mtpViewModel.InitialDischargeCriteria))
                    {
                        ModelState.AddModelError(string.Empty, "Error.There are gender issues in: Initial discharge criteria");
                        MTPViewModel model = new MTPViewModel
                        {
                            Clients = _combosHelper.GetComboClientsByClinic(user_logged.Clinic.Id),
                            IdClient = mtpViewModel.IdClient,
                            MTPDevelopedDate = mtpViewModel.MTPDevelopedDate,
                            NumberOfMonths = mtpViewModel.NumberOfMonths,
                            StartTime = mtpViewModel.StartTime,
                            EndTime = mtpViewModel.EndTime,
                            Modality = mtpViewModel.Modality,
                            Frecuency = mtpViewModel.Frecuency,
                            LevelCare = mtpViewModel.LevelCare,
                            InitialDischargeCriteria = mtpViewModel.InitialDischargeCriteria,
                            Setting = form["Setting"].ToString(),
                            Review = mtpViewModel.Review,
                            AdmissionedFor = user_logged.FullNameWithDocument
                           
                        };
                        return View(model);
                    }
                }

                //esto es para cuando el MTP es de group no tener que cambiar mas nada
                if (mtpViewModel.GroupDuration > 0 && mtpViewModel.PsychosocialDuration == 0)
                {
                    mtpViewModel.PsychosocialDuration = mtpViewModel.GroupDuration;
                }                

                MTPEntity mtpEntity = await _converterHelper.ToMTPEntity(mtpViewModel, true, user_logged.UserName);
                mtpEntity.Setting = form["Setting"].ToString();

                //set all mtps of this client non active
                List<MTPEntity> mtp_list = _context.MTPs.Where(m => m.Client == mtpEntity.Client).ToList();
                foreach (MTPEntity item in mtp_list)
                {
                    item.Active = false;
                    _context.Update(item);
                }

                if (documentAssistant != null)
                {
                    mtpEntity.DocumentAssistant = documentAssistant;
                }

                //update Client_Referred table with the news ReferredTemp
                IQueryable<GoalsTempEntity> list_to_delete_Goals = _context.GoalsTemp
                                                                           .Include(n => n.ObjetiveTempList)
                                                                           .Where(n => n.IdClient == mtpViewModel.IdClient);
                GoalEntity goal = new GoalEntity();
                mtpEntity.Goals = new List<GoalEntity>();
                ObjetiveEntity objective = new ObjetiveEntity();
                foreach (GoalsTempEntity item in list_to_delete_Goals)
                {
                    goal = new GoalEntity
                    {
                        Id = 0,
                        AreaOfFocus = item.AreaOfFocus,
                        Name = item.Name,
                        Number = item.Number,
                        MTP = mtpEntity,
                        Service = item.Service,
                        Objetives = new List<ObjetiveEntity>()
                        
                    };

                    foreach (ObjectiveTempEntity product in item.ObjetiveTempList)
                    {
                        objective = new ObjetiveEntity
                        {
                            Id = 0,
                            DateOpened = product.DateOpened,
                            DateResolved = product.DateResolved,
                            DateTarget = product.DateTarget,
                            Description = product.Description,
                            Goal = goal,
                            Intervention = product.Intervention,
                            Objetive = product.Objetive

                        };
                        _context.Add(objective);
                        _context.ObjetivesTemp.Remove(product);
                        objective = new ObjetiveEntity();
                    }

                    //mtpEntity.Goals.ToList().Add(goal);
                    _context.Add(goal);
                    _context.GoalsTemp.Remove(item);
                    goal = new GoalEntity();
                }

                _context.Add(mtpEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    if (origin == 0)
                    {
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    else 
                    {
                        return RedirectToAction("ClientsWithoutMTP", "Clients");
                    }
                    
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the MTP: {mtpEntity.Client.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(mtpViewModel);
        }

        [Authorize(Roles = "Supervisor, Manager, Documents_Assistant")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MTPEntity mtpEntity = await _context.MTPs
                                                .Include(m => m.Client)
                                                .FirstOrDefaultAsync(t => t.Id == id);
            if (mtpEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            //I check the mtp is not in a any note
            List<NoteEntity> notes = await _context.Notes.Where(n => n.MTPId == id).ToListAsync();
            if (notes.Count() > 0)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }
            //I check the client have more than 1 mtp and this mtp is not active, or the client have not any note
            ClientEntity client = await _context.Clients
                                                .Include(c => c.MTPs)
                                                .FirstOrDefaultAsync(c => c.Id == mtpEntity.Client.Id);
            notes = await _context.Notes.Where(n => n.Workday_Cient.Client.Id == client.Id).ToListAsync();
            if ((client.MTPs.Count() == 1 && notes.Count() > 0) || (mtpEntity.Active))
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            _context.MTPs.Remove(mtpEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> Edit(int? id, int origi = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = await _context.Users
                                                  .Include(u => u.Clinic)
                                                  .ThenInclude(c => c.Setting)
                                                  .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            MTPEntity mtpEntity = await _context.MTPs.Include(m => m.Client)
                                                     .ThenInclude(c => c.Clients_Diagnostics)
                                                     .ThenInclude(c => c.Diagnostic)
                                                     .Include(m => m.MtpReviewList)
                                                     .Include(m => m.DocumentAssistant)
                                                     .Include(m => m.Goals.Where(m => m.Adendum == null && m.IdMTPReview == 0))
                                                     .ThenInclude(c => c.Objetives)
                                                     .FirstOrDefaultAsync(m => m.Id == id);
            if (mtpEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MTPViewModel mtpViewModel = _converterHelper.ToMTPViewModel(mtpEntity);

            if (User.IsInRole("Supervisor") || User.IsInRole("Documents_Assistant"))
            {

                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = mtpEntity.Client.Name,
                    Value = $"{mtpEntity.Client.Id}"
                });
                mtpViewModel.Clients = list;
                if (mtpViewModel.Goals == null)
                    mtpViewModel.Goals= new List<GoalEntity>();
                ViewData["origi"] = origi;
                return View(mtpViewModel);
            }

            return RedirectToAction(nameof(Index));

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> Edit(int id, MTPViewModel mtpViewModel, IFormCollection form, int origi = 0)
        {
            if (id != mtpViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (!string.IsNullOrEmpty(mtpViewModel.InitialDischargeCriteria))
                {
                    mtpViewModel.InitialDischargeCriteria = (mtpViewModel.InitialDischargeCriteria.Last() == '.') ? mtpViewModel.InitialDischargeCriteria : $"{mtpViewModel.InitialDischargeCriteria}.";
                }

                //esto es para cuando el MTP es de group no tener que cambiar mas nada
                if (mtpViewModel.GroupDuration > 0 && mtpViewModel.PsychosocialDuration == 0)
                {
                    mtpViewModel.PsychosocialDuration = mtpViewModel.GroupDuration;
                }

                MTPEntity mtpEntity = await _converterHelper.ToMTPEntity(mtpViewModel, false, user_logged.UserName);

                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(mtpViewModel.InitialDischargeCriteria))
                {
                    if (this.GenderEvaluation(mtpEntity.Client.Gender, mtpViewModel.InitialDischargeCriteria))
                    {
                        ModelState.AddModelError(string.Empty, "Error.There are gender issues in: Initial discharge criteria");
                        List<SelectListItem> list = new List<SelectListItem>();
                        list.Insert(0, new SelectListItem
                        {
                            Text = mtpEntity.Client.Name,
                            Value = $"{mtpEntity.Client.Id}"
                        });
                        MTPViewModel model = new MTPViewModel
                        {
                            Clients = list,
                            IdClient = mtpViewModel.IdClient,
                            MTPDevelopedDate = mtpViewModel.MTPDevelopedDate,
                            NumberOfMonths = mtpViewModel.NumberOfMonths,
                            StartTime = mtpViewModel.StartTime,
                            EndTime = mtpViewModel.EndTime,
                            Modality = mtpViewModel.Modality,
                            Frecuency = mtpViewModel.Frecuency,
                            LevelCare = mtpViewModel.LevelCare,
                            InitialDischargeCriteria = mtpViewModel.InitialDischargeCriteria,
                            Setting = form["Setting"].ToString(),
                            AdmissionedFor = mtpViewModel.AdmissionedFor,
                            CreatedBy = mtpViewModel.CreatedBy,
                            CreatedOn = mtpViewModel.CreatedOn,
                            IdDocumentAssistant = mtpViewModel.IdDocumentAssistant
                        };
                        ViewData["origi"] = origi;
                        return View(model);
                    }
                }
                // mtpEntity.MtpReview = await _context.MTPReviews.FirstOrDefaultAsync(u => u.MTP_FK == mtpViewModel.Id);
                if ((User.IsInRole("Supervisor")) || (User.IsInRole("Documents_Assistant"))) //|| ((mtpEntity.MtpReview != null) && (mtpEntity.MtpReview.CreatedBy == user_logged.Id)))
                {
                    mtpEntity.Setting = form["Setting"].ToString();
                    _context.Update(mtpEntity);
                    try
                    {
                        List<MessageEntity> messages = mtpEntity.Messages.Where(m => (m.Status == MessageStatus.NotRead && m.Notification == false)).ToList();
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
                                Discharge = null,
                                Mtp = mtpEntity,
                                Bio = null,
                                Title = "Update on reviewed MTP",
                                Text = $"The MTP document of {mtpEntity.Client.Name} that was evaluated on {mtpEntity.AdmissionDateMTP.ToShortDateString()} was rectified",
                                From = value.To,
                                To = value.From,
                                DateCreated = DateTime.Now,
                                Status = MessageStatus.NotRead,
                                Notification = true
                            };
                            _context.Add(notification);
                        }

                        await _context.SaveChangesAsync();

                        await _context.SaveChangesAsync();
                        if (origi == 0)
                        {
                            return RedirectToAction(nameof(Index));
                        }
                        if (origi == 1)
                        {
                            return RedirectToAction("MessagesOfMTP", "Messages");
                        }
                        if (origi == 2)
                        {
                            return RedirectToAction("MtpWithReview", "MTPs");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the facilitator: {mtpEntity.Client.Name}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
            }
            return View(mtpViewModel);
        }

        [Authorize(Roles = "Supervisor, Manager, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MTPEntity mtpEntity = await _context.MTPs.Include(m => m.Client)
                                                     .ThenInclude(f => f.Clinic)

                                                     .Include(m => m.Client)
                                                     .ThenInclude(c => c.Clients_Diagnostics)
                                                     .ThenInclude(cd => cd.Diagnostic)

                                                     .Include(m => m.Goals.Where(n => n.Adendum == null && n.IdMTPReview == 0))

                                                     .ThenInclude(g => g.Objetives)

                                                     .FirstOrDefaultAsync(m => m.Id == id);
            if (mtpEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            return View(mtpEntity);
        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> UpdateGoals(int? id, int idError = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            MTPEntity mtpEntity = await _context.MTPs

                                                .Include(m => m.Goals.Where(n => n.Adendum == null && n.IdMTPReview == 0))
                                                .ThenInclude(g => g.Objetives)

                                                .Include(m => m.Client)

                                                .FirstOrDefaultAsync(m => m.Id == id);

            if (mtpEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            return View(mtpEntity);
        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> CreateGoal(int? id, int idAdendum)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MTPEntity mtpEntity = await _context.MTPs
                                                .Include(m => m.Client)
                                                .Include(m => m.Goals)
                                                .FirstOrDefaultAsync(m => m.Id == id);
            if (mtpEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GoalViewModel model = new GoalViewModel
            {
                MTP = mtpEntity,
                IdMTP = mtpEntity.Id,
                Number = mtpEntity.Goals.Count() + 1,
                IdService = 0,
                Services = _combosHelper.GetComboServices(),
                IdAdendum = idAdendum
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> CreateGoal(int id, GoalViewModel model)
        {
            if (id != model.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            model.MTP = await _context.MTPs.Include(m => m.Client).FirstOrDefaultAsync(m => m.Id == model.IdMTP);

            if (ModelState.IsValid)
            {
                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(model.Name))
                {
                    model.Name = (model.Name.Last() == '.') ? model.Name : $"{model.Name}.";
                    if (this.GenderEvaluation(model.MTP.Client.Gender, model.Name))
                    {
                        gender_problems = "Name";
                    }
                }
                if (!string.IsNullOrEmpty(model.AreaOfFocus))
                {
                    model.AreaOfFocus = (model.AreaOfFocus.Last() == '.') ? model.AreaOfFocus : $"{model.AreaOfFocus}.";
                    if (this.GenderEvaluation(model.MTP.Client.Gender, model.AreaOfFocus))
                    {
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? "Area of Focus" : $"{gender_problems}, Area of Focus";
                    }
                }
                if (!string.IsNullOrEmpty(gender_problems))     //el goal tiene problemas con el genero
                {
                    ModelState.AddModelError(string.Empty, $"Error.There are gender issues in: {gender_problems}");
                    model.Services = _combosHelper.GetComboServices();
                    return View(model);
                }

                GoalEntity goalEntity = await _converterHelper.ToGoalEntity(model, true);
                _context.Add(goalEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    if (model.IdAdendum == 0)
                        return RedirectToAction("UpdateGoals", new { id = model.IdMTP });
                    else
                        return RedirectToAction("EditAdendum", new { id = model.IdAdendum });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the goal");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            model.Services = _combosHelper.GetComboServices();
            return View(model);
        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> CreateGoalModal(int? id, int idAdendum)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MTPEntity mtpEntity = await _context.MTPs
                                                .Include(m => m.Client)
                                                .Include(m => m.Goals)
                                                .FirstOrDefaultAsync(m => m.Id == id);
            if (mtpEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GoalViewModel model = new GoalViewModel
            {
                MTP = mtpEntity,
                IdMTP = mtpEntity.Id,
                Number = mtpEntity.Goals.Count() + 1,
                IdService = 0,
                Services = _combosHelper.GetComboServices(),
                IdAdendum = idAdendum
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> CreateGoalModal(int id, GoalViewModel model)
        {
            if (id != model.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            model.MTP = await _context.MTPs.Include(m => m.Client).FirstOrDefaultAsync(m => m.Id == model.IdMTP);

            if (ModelState.IsValid)
            {
                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(model.Name))
                {
                    model.Name = (model.Name.Last() == '.') ? model.Name : $"{model.Name}.";
                    if (this.GenderEvaluation(model.MTP.Client.Gender, model.Name))
                    {
                        gender_problems = "Name";
                    }
                }
                if (!string.IsNullOrEmpty(model.AreaOfFocus))
                {
                    model.AreaOfFocus = (model.AreaOfFocus.Last() == '.') ? model.AreaOfFocus : $"{model.AreaOfFocus}.";
                    if (this.GenderEvaluation(model.MTP.Client.Gender, model.AreaOfFocus))
                    {
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? "Area of Focus" : $"{gender_problems}, Area of Focus";
                    }
                }
                if (!string.IsNullOrEmpty(gender_problems))     //el goal tiene problemas con el genero
                {
                    ModelState.AddModelError(string.Empty, $"Error.There are gender issues in: {gender_problems}");
                    model.Services = _combosHelper.GetComboServices();
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateGoalModal", model) });
                    //return View(model);
                }

                GoalEntity goalEntity = await _converterHelper.ToGoalEntity(model, true);
                _context.Add(goalEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    if (model.IdAdendum == 0)
                    {
                        List<GoalEntity> goals = await _context.Goals
                                                           .Include(g => g.Objetives)
                                                           .Where(g => g.MTP.Id == model.IdMTP && g.Adendum == null && g.IdMTPReview == 0)
                                                           .ToListAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoals", goals) });
                    }
                    else
                    {
                        List<GoalEntity> goals = await _context.Goals
                                                           .Include(g => g.Objetives)
                                                           .Include(g => g.MTP)
                                                           .Include(g => g.Adendum)
                                                           .Where(g => g.Adendum.Id == model.IdAdendum)
                                                           .ToListAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoals", goals) });
                    }
                    
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            model.Services = _combosHelper.GetComboServices();
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateGoalModal", model) });
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> CreateGoalMTPReviewModal(int? id, int idReview)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MTPEntity mtpEntity = await _context.MTPs
                                                .Include(m => m.Client)
                                                .Include(m => m.Goals)
                                                .FirstOrDefaultAsync(m => m.Id == id);
            if (mtpEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GoalViewModel model = new GoalViewModel
            {
                MTP = mtpEntity,
                IdMTP = mtpEntity.Id,
                Number = mtpEntity.Goals.Count() + 1,
                IdService = 0,
                Services = _combosHelper.GetComboServices(),
                IdMTPReview = idReview
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> CreateGoalMTPReviewModal(int id, GoalViewModel model)
        {
            if (id != model.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            model.MTP = await _context.MTPs.Include(m => m.Client).FirstOrDefaultAsync(m => m.Id == model.IdMTP);

            if (ModelState.IsValid)
            {
                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(model.Name))
                {
                    model.Name = (model.Name.Last() == '.') ? model.Name : $"{model.Name}.";
                    if (this.GenderEvaluation(model.MTP.Client.Gender, model.Name))
                    {
                        gender_problems = "Name";
                    }
                }
                if (!string.IsNullOrEmpty(model.AreaOfFocus))
                {
                    model.AreaOfFocus = (model.AreaOfFocus.Last() == '.') ? model.AreaOfFocus : $"{model.AreaOfFocus}.";
                    if (this.GenderEvaluation(model.MTP.Client.Gender, model.AreaOfFocus))
                    {
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? "Area of Focus" : $"{gender_problems}, Area of Focus";
                    }
                }
                if (!string.IsNullOrEmpty(gender_problems))     //el goal tiene problemas con el genero
                {
                    ModelState.AddModelError(string.Empty, $"Error.There are gender issues in: {gender_problems}");
                    model.Services = _combosHelper.GetComboServices();
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateGoalMTPReviewModal", model) });
                    //return View(model);
                }

                GoalEntity goalEntity = await _converterHelper.ToGoalEntity(model, true);
                _context.Add(goalEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<GoalEntity> goals = await _context.Goals
                                                           .Include(g => g.Objetives)
                                                           .Include(g => g.MTP)

                                                           .Where(g => g.MTP.Id == model.MTP.Id)
                                                           .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoalsMTPReview", goals, new Dictionary<string, object>() { { "Id", model.IdMTPReview } }) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            model.Services = _combosHelper.GetComboServices();
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateGoalMTPReviewModal", model) });
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> DeleteGoal(int? id, int oringin = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GoalEntity goalEntity = await _context.Goals.Include(g => g.MTP).FirstOrDefaultAsync(g => g.Id == id);
            if (goalEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Goals.Remove(goalEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("UpdateGoals", new { id = goalEntity.MTP.Id, idError = 1 });
            }
            if (oringin == 0)
            {
                return RedirectToAction("UpdateGoals", new { id = goalEntity.MTP.Id });
            }

            return RedirectToAction("Edit", new { id = goalEntity.MTP.Id });

        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> EditGoal(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GoalEntity goalEntity = await _context.Goals.Include(g => g.MTP)
                                                        .ThenInclude(m => m.Client)
                                                        .Include(g => g.Adendum)
                                                        .FirstOrDefaultAsync(d => d.Id == id);

            GoalViewModel model = _converterHelper.ToGoalViewModel(goalEntity);
            if (model == null)
            {
                return RedirectToAction("Home/Error404");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> EditGoal(int id, GoalViewModel model)
        {
            if (id != model.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            model.MTP = await _context.MTPs.Include(m => m.Client).FirstOrDefaultAsync(m => m.Id == model.IdMTP);

            if (ModelState.IsValid)
            {
                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(model.Name))
                {
                    model.Name = (model.Name.Last() == '.') ? model.Name : $"{model.Name}.";
                    if (this.GenderEvaluation(model.MTP.Client.Gender, model.Name))
                    {
                        gender_problems = "Name";
                    }
                }
                if (!string.IsNullOrEmpty(model.AreaOfFocus))
                {
                    model.AreaOfFocus = (model.AreaOfFocus.Last() == '.') ? model.AreaOfFocus : $"{model.AreaOfFocus}.";
                    if (this.GenderEvaluation(model.MTP.Client.Gender, model.AreaOfFocus))
                    {
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? "Area of Focus" : $"{gender_problems}, Area of Focus";
                    }
                }
                if (!string.IsNullOrEmpty(gender_problems))     //el goal tiene problemas con el genero
                {
                    ModelState.AddModelError(string.Empty, $"Error.There are gender issues in: {gender_problems}");
                    model.Services = _combosHelper.GetComboServices();
                    return View(model);
                }

                GoalEntity goalEntity = await _converterHelper.ToGoalEntity(model, false);
                _context.Update(goalEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    if (model.IdAdendum == 0)
                        return RedirectToAction("UpdateGoals", new { id = model.IdMTP });
                    else
                        return RedirectToAction("EditAdendum", new { id = model.IdAdendum });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the goals");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            model.Services = _combosHelper.GetComboServices();
            return View(model);
        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> EditGoalModal(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GoalEntity goalEntity = await _context.Goals.Include(g => g.MTP)
                                                        .ThenInclude(m => m.Client)
                                                        .Include(g => g.Adendum)
                                                        .FirstOrDefaultAsync(d => d.Id == id);

            GoalViewModel model = _converterHelper.ToGoalViewModel(goalEntity);
            if (model == null)
            {
                return RedirectToAction("Home/Error404");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> EditGoalModal(int id, GoalViewModel model)
        {
            if (id != model.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            model.MTP = await _context.MTPs.Include(m => m.Client).FirstOrDefaultAsync(m => m.Id == model.IdMTP);

            if (ModelState.IsValid)
            {
                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(model.Name))
                {
                    model.Name = (model.Name.Last() == '.') ? model.Name : $"{model.Name}.";
                    if (this.GenderEvaluation(model.MTP.Client.Gender, model.Name))
                    {
                        gender_problems = "Name";
                    }
                }
                if (!string.IsNullOrEmpty(model.AreaOfFocus))
                {
                    model.AreaOfFocus = (model.AreaOfFocus.Last() == '.') ? model.AreaOfFocus : $"{model.AreaOfFocus}.";
                    if (this.GenderEvaluation(model.MTP.Client.Gender, model.AreaOfFocus))
                    {
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? "Area of Focus" : $"{gender_problems}, Area of Focus";
                    }
                }
                if (!string.IsNullOrEmpty(gender_problems))     //el goal tiene problemas con el genero
                {
                    ModelState.AddModelError(string.Empty, $"Error.There are gender issues in: {gender_problems}");
                    model.Services = _combosHelper.GetComboServices();
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditGoalModal", model) });
                    //return View(model);
                }

                GoalEntity goalEntity = await _converterHelper.ToGoalEntity(model, false);
                _context.Update(goalEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    if (model.IdAdendum == 0)
                    {
                        List<GoalEntity> goals = await _context.Goals
                                                           .Include(g => g.Objetives)
                                                           .Where(g => g.MTP.Id == model.IdMTP && g.Adendum == null && g.IdMTPReview == 0)
                                                           .ToListAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoals", goals) });
                    }
                    else
                    {
                        List<GoalEntity> goals = await _context.Goals
                                                           .Include(g => g.Objetives)
                                                           .Include(g => g.MTP)
                                                           .Include(g => g.Adendum)
                                                           .Where(g => g.Adendum.Id == model.IdAdendum)
                                                           .ToListAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoals", goals) });
                    }
                    
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the goals");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            model.Services = _combosHelper.GetComboServices();
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditGoalModal", model) });
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> EditGoalMTPReviewModal(int? id, int idMTPReviewOfView)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GoalEntity goalEntity = await _context.Goals

                                                  .Include(g => g.MTP)
                                                  .ThenInclude(m => m.Client)

                                                  .Include(g => g.Adendum)

                                                  .FirstOrDefaultAsync(d => d.Id == id);

            GoalViewModel model = _converterHelper.ToGoalViewModel(goalEntity);
            if (model == null)
            {
                return RedirectToAction("Home/Error404");
            }

            model.IdMTPReviewOfView = idMTPReviewOfView;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> EditGoalMTPReviewModal(int id, GoalViewModel model)
        {
            if (id != model.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            model.MTP = await _context.MTPs.Include(m => m.Client).FirstOrDefaultAsync(m => m.Id == model.IdMTP);

            if (ModelState.IsValid)
            {
                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(model.Name))
                {
                    model.Name = (model.Name.Last() == '.') ? model.Name : $"{model.Name}.";
                    if (this.GenderEvaluation(model.MTP.Client.Gender, model.Name))
                    {
                        gender_problems = "Name";
                    }
                }
                if (!string.IsNullOrEmpty(model.AreaOfFocus))
                {
                    model.AreaOfFocus = (model.AreaOfFocus.Last() == '.') ? model.AreaOfFocus : $"{model.AreaOfFocus}.";
                    if (this.GenderEvaluation(model.MTP.Client.Gender, model.AreaOfFocus))
                    {
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? "Area of Focus" : $"{gender_problems}, Area of Focus";
                    }
                }
                if (!string.IsNullOrEmpty(gender_problems))     //el goal tiene problemas con el genero
                {
                    ModelState.AddModelError(string.Empty, $"Error.There are gender issues in: {gender_problems}");
                    model.Services = _combosHelper.GetComboServices();
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditGoalMTPReviewModal", model) });
                    //return View(model);
                }

                GoalEntity goalEntity = await _converterHelper.ToGoalEntity(model, false);

                if (model.Compliment_IdMTPReview == 0 && model.Compliment == true)
                {
                    goalEntity.Compliment_IdMTPReview = model.IdMTPReviewOfView;
                }
                else
                {
                    goalEntity.Compliment_IdMTPReview = 0;
                }
                _context.Update(goalEntity);

                try
                {
                    await _context.SaveChangesAsync();
                    List<GoalEntity> goals = await _context.Goals

                                                           .Include(g => g.Objetives)

                                                           .Include(g => g.MTP)

                                                           .Where(g => g.MTP.Id == model.IdMTP)
                                                           .OrderBy(g => g.Number)
                                                           .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoalsMTPReview", goals, new Dictionary<string, object>() { { "Id", model.IdMTPReviewOfView } }) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the goals");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            model.Services = _combosHelper.GetComboServices();
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditGoalMTPReviewModal", model) });
        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> UpdateObjectives(int? id, int idError = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            GoalEntity goalEntity = await _context.Goals.Include(g => g.MTP)
                                                     .ThenInclude(m => m.Client)
                                                     .Include(g => g.Objetives).FirstOrDefaultAsync(g => g.Id == id);

            if (goalEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            return View(goalEntity);
        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> CreateObjective(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GoalEntity goalEntity = await _context.Goals.Include(g => g.MTP)
                                                        .ThenInclude(m => m.Client)
                                                        .Include(g => g.Objetives)
                                                        .FirstOrDefaultAsync(m => m.Id == id);
            if (goalEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            string objetive = $"{goalEntity.Number}.{goalEntity.Objetives.Count() + 1}";
            ObjectiveViewModel model = new ObjectiveViewModel
            {
                Goal = goalEntity,
                IdGoal = goalEntity.Id,
                DateOpened = goalEntity.MTP.AdmissionDateMTP,
                DateResolved = goalEntity.MTP.AdmissionDateMTP.AddMonths(Convert.ToInt32(goalEntity.MTP.NumberOfMonths)),
                DateTarget = goalEntity.MTP.AdmissionDateMTP.AddMonths(Convert.ToInt32(goalEntity.MTP.NumberOfMonths)),
                Objetive = objetive
            };

            MultiSelectList classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name");
            ViewData["classification"] = classification_list;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> CreateObjective(ObjectiveViewModel model, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                GoalEntity goal = await _context.Goals
                                                .Include(g => g.MTP)
                                                .ThenInclude(m => m.Client)
                                                .Include(g => g.Objetives)
                                                .FirstOrDefaultAsync(m => m.Id == model.IdGoal);
                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(model.Description))
                {
                    model.Description = (model.Description.Last() == '.') ? model.Description : $"{model.Description}.";
                    if (this.GenderEvaluation(goal.MTP.Client.Gender, model.Description))
                    {
                        gender_problems = "Description";
                    }
                }
                if (!string.IsNullOrEmpty(model.Intervention))
                {
                    model.Intervention = (model.Intervention.Last() == '.') ? model.Intervention : $"{model.Intervention}.";
                    if (this.GenderEvaluation(goal.MTP.Client.Gender, model.Intervention))
                    {
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? "Intervention" : $"{gender_problems}, Intervention";
                    }
                }
                if (!string.IsNullOrEmpty(gender_problems))     //el objective tiene problemas con el genero
                {
                    ModelState.AddModelError(string.Empty, $"Error.There are gender issues in: {gender_problems}");
                    ObjectiveViewModel newmodel = new ObjectiveViewModel
                    {
                        Goal = goal,
                        IdGoal = goal.Id,
                        DateOpened = model.DateOpened,
                        DateResolved = model.DateResolved,
                        DateTarget = model.DateTarget,
                        Objetive = $"{goal.Number}.{goal.Objetives.Count() + 1}",
                        Description = model.Description,
                        Intervention = model.Intervention
                    };
                    return View(newmodel);
                }

                ObjetiveEntity objective = await _converterHelper.ToObjectiveEntity(model, true);
                _context.Add(objective);

                if (!string.IsNullOrEmpty(form["classifications"]))
                {
                    string[] classifications = form["classifications"].ToString().Split(',');
                    Objetive_Classification objclassification;
                    foreach (string value in classifications)
                    {
                        objclassification = new Objetive_Classification()
                        {
                            Objetive = objective,
                            Classification = await _context.Classifications.FindAsync(Convert.ToInt32(value))
                        };
                        _context.Add(objclassification);
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("UpdateObjectives", new { id = model.IdGoal });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the objective");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            GoalEntity goalEntity = await _context.Goals.Include(g => g.MTP)
                                                        .ThenInclude(m => m.Client).FirstOrDefaultAsync(m => m.Id == model.IdGoal);
            model.Goal = goalEntity;
            model.IdGoal = goalEntity.Id;
            MultiSelectList classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name");
            ViewData["classification"] = classification_list;
            return View(model);
        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> CreateObjectiveModal(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GoalEntity goalEntity = await _context.Goals.Include(g => g.MTP)
                                                        .ThenInclude(m => m.Client)
                                                        .Include(g => g.Objetives)
                                                        .FirstOrDefaultAsync(m => m.Id == id);
            if (goalEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            string objetive = $"{goalEntity.Number}.{goalEntity.Objetives.Count() + 1}";
            ObjectiveViewModel model = new ObjectiveViewModel
            {
                Goal = goalEntity,
                IdGoal = goalEntity.Id,
                DateOpened = goalEntity.MTP.AdmissionDateMTP,
                DateResolved = goalEntity.MTP.AdmissionDateMTP.AddMonths(Convert.ToInt32(goalEntity.MTP.NumberOfMonths)),
                DateTarget = goalEntity.MTP.AdmissionDateMTP.AddMonths(Convert.ToInt32(goalEntity.MTP.NumberOfMonths)),
                Objetive = objetive
            };

            MultiSelectList classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name");
            ViewData["classification"] = classification_list;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> CreateObjectiveModal(ObjectiveViewModel model, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                GoalEntity goal = await _context.Goals

                                                .Include(g => g.MTP)
                                                .ThenInclude(m => m.Client)

                                                .Include(g => g.Objetives)

                                                .Include(g => g.Adendum)

                                                .FirstOrDefaultAsync(m => m.Id == model.IdGoal);

                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(model.Description))
                {
                    model.Description = (model.Description.Last() == '.') ? model.Description : $"{model.Description}.";
                    if (this.GenderEvaluation(goal.MTP.Client.Gender, model.Description))
                    {
                        gender_problems = "Description";
                    }
                }
                if (!string.IsNullOrEmpty(model.Intervention))
                {
                    model.Intervention = (model.Intervention.Last() == '.') ? model.Intervention : $"{model.Intervention}.";
                    if (this.GenderEvaluation(goal.MTP.Client.Gender, model.Intervention))
                    {
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? "Intervention" : $"{gender_problems}, Intervention";
                    }
                }
                if (!string.IsNullOrEmpty(gender_problems))     //el objective tiene problemas con el genero
                {
                    ModelState.AddModelError(string.Empty, $"Error.There are gender issues in: {gender_problems}");
                    ObjectiveViewModel newmodel = new ObjectiveViewModel
                    {
                        Goal = goal,
                        IdGoal = goal.Id,
                        DateOpened = model.DateOpened,
                        DateResolved = model.DateResolved,
                        DateTarget = model.DateTarget,
                        Objetive = $"{goal.Number}.{goal.Objetives.Count() + 1}",
                        Description = model.Description,
                        Intervention = model.Intervention
                    };
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateObjectiveModal", newmodel) });
                    //return View(newmodel);
                }

                ObjetiveEntity objective = await _converterHelper.ToObjectiveEntity(model, true);
                _context.Add(objective);

                if (!string.IsNullOrEmpty(form["classifications"]))
                {
                    string[] classifications = form["classifications"].ToString().Split(',');
                    Objetive_Classification objclassification;
                    foreach (string value in classifications)
                    {
                        objclassification = new Objetive_Classification()
                        {
                            Objetive = objective,
                            Classification = await _context.Classifications.FindAsync(Convert.ToInt32(value))
                        };
                        _context.Add(objclassification);
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();

                    if (goal.Adendum == null)
                    {
                        List<GoalEntity> goals = await _context.Goals
                                                           .Include(g => g.Objetives)
                                                           .Where(g => g.MTP.Id == goal.MTP.Id && g.Adendum == null && g.IdMTPReview == 0)
                                                           .ToListAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoals", goals) });
                    }
                    else
                    {
                        List<GoalEntity> goals = await _context.Goals
                                                           .Include(g => g.Objetives)
                                                           .Include(g => g.MTP)
                                                           .Include(g => g.Adendum)
                                                           .Where(g => g.Adendum.Id == goal.Adendum.Id)
                                                           .ToListAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoals", goals) });
                    }
                    
                    
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the objective");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            GoalEntity goalEntity = await _context.Goals.Include(g => g.MTP)
                                                        .ThenInclude(m => m.Client)
                                                        .FirstOrDefaultAsync(m => m.Id == model.IdGoal);
            model.Goal = goalEntity;
            model.IdGoal = goalEntity.Id;
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateObjectiveModal", model) });
        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> CreateObjectiveMTPReviewModal(int? id, int idReview)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GoalEntity goalEntity = await _context.Goals.Include(g => g.MTP)
                                                        .ThenInclude(m => m.Client)
                                                        .Include(g => g.Objetives)
                                                        .FirstOrDefaultAsync(m => m.Id == id);
            if (goalEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            string objetive = $"{goalEntity.Number}.{goalEntity.Objetives.Count() + 1}";
            ObjectiveViewModel model = new ObjectiveViewModel
            {
                Goal = goalEntity,
                IdGoal = goalEntity.Id,
                DateOpened = goalEntity.MTP.AdmissionDateMTP,
                DateResolved = goalEntity.MTP.AdmissionDateMTP.AddMonths(Convert.ToInt32(goalEntity.MTP.NumberOfMonths)),
                DateTarget = goalEntity.MTP.AdmissionDateMTP.AddMonths(Convert.ToInt32(goalEntity.MTP.NumberOfMonths)),
                Objetive = objetive,
                IdMTPReview = idReview
            };

            MultiSelectList classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name");
            ViewData["classification"] = classification_list;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> CreateObjectiveMTPReviewModal(ObjectiveViewModel model, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                GoalEntity goal = await _context.Goals

                                                .Include(g => g.MTP)
                                                .ThenInclude(m => m.Client)

                                                .Include(g => g.Objetives)

                                                .Include(g => g.Adendum)

                                                .FirstOrDefaultAsync(m => m.Id == model.IdGoal);

                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(model.Description))
                {
                    model.Description = (model.Description.Last() == '.') ? model.Description : $"{model.Description}.";
                    if (this.GenderEvaluation(goal.MTP.Client.Gender, model.Description))
                    {
                        gender_problems = "Description";
                    }
                }
                if (!string.IsNullOrEmpty(model.Intervention))
                {
                    model.Intervention = (model.Intervention.Last() == '.') ? model.Intervention : $"{model.Intervention}.";
                    if (this.GenderEvaluation(goal.MTP.Client.Gender, model.Intervention))
                    {
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? "Intervention" : $"{gender_problems}, Intervention";
                    }
                }
                if (!string.IsNullOrEmpty(gender_problems))     //el objective tiene problemas con el genero
                {
                    ModelState.AddModelError(string.Empty, $"Error.There are gender issues in: {gender_problems}");
                    ObjectiveViewModel newmodel = new ObjectiveViewModel
                    {
                        Goal = goal,
                        IdGoal = goal.Id,
                        DateOpened = model.DateOpened,
                        DateResolved = model.DateResolved,
                        DateTarget = model.DateTarget,
                        Objetive = $"{goal.Number}.{goal.Objetives.Count() + 1}",
                        Description = model.Description,
                        Intervention = model.Intervention,
                        Compliment = model.Compliment,
                        Compliment_Date = model.Compliment_Date,
                        Compliment_Explain = model.Compliment_Explain,
                        Compliment_IdMTPReview = model.Compliment_IdMTPReview,
                        IdMTPReview = model.IdMTPReview

                    };
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateObjectiveMTPReviewModal", model) });
                    //return View(newmodel);
                }

                ObjetiveEntity objective = await _converterHelper.ToObjectiveEntity(model, true);
                _context.Add(objective);

                if (!string.IsNullOrEmpty(form["classifications"]))
                {
                    string[] classifications = form["classifications"].ToString().Split(',');
                    Objetive_Classification objclassification;
                    foreach (string value in classifications)
                    {
                        objclassification = new Objetive_Classification()
                        {
                            Objetive = objective,
                            Classification = await _context.Classifications.FindAsync(Convert.ToInt32(value))
                        };
                        _context.Add(objclassification);
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();

                    List<GoalEntity> goals = await _context.Goals

                                                           .Include(g => g.Objetives)

                                                           .Include(g => g.MTP)

                                                           .Where(g => g.MTP.Id == goal.MTP.Id)
                                                           .OrderBy(g => g.Number)
                                                           .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoalsMTPReview", goals, new Dictionary<string, object>() { { "Id", model.IdMTPReview } }) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the objective");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            GoalEntity goalEntity = await _context.Goals.Include(g => g.MTP)
                                                        .ThenInclude(m => m.Client)
                                                        .FirstOrDefaultAsync(m => m.Id == model.IdGoal);
            model.Goal = goalEntity;
            model.IdGoal = goalEntity.Id;
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateObjectiveMTPReviewModal", model) });
        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> DeleteObjective(int? id, int origin = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ObjetiveEntity objectiveEntity = await _context.Objetives
                                                           .Include(o => o.Goal)
                                                           .ThenInclude(g => g.Adendum)
                                                           .Include(o => o.Goal)
                                                           .ThenInclude(g => g.MTP)
                                                           .FirstOrDefaultAsync(o => o.Id == id);
            if (objectiveEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Objetives.Remove(objectiveEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("UpdateObjectives", new { id = objectiveEntity.Goal.Id, idError = 1 });
            }

            if (origin == 0)
            {
                return RedirectToAction("UpdateObjectives", new { objectiveEntity.Goal.Id });
            }
            else
            {
                if (objectiveEntity.Goal.Adendum != null)
                {
                    return RedirectToAction("EditAdendum", new { id = objectiveEntity.Goal.Adendum.Id });
                }
                else
                {
                    return RedirectToAction("Edit", new { id = objectiveEntity.Goal.MTP.Id });
                }
                
            }
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> DeleteObjectiveMTPReview(int? id, int origin = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ObjetiveEntity objectiveEntity = await _context.Objetives
                                                           .Include(o => o.Goal)
                                                           .FirstOrDefaultAsync(o => o.Id == id);
            if (objectiveEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Objetives.Remove(objectiveEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("EditMTPReview", new { id = objectiveEntity.Goal.Id, idError = 1 });
            }

            return RedirectToAction("EditMTPReview", new { id = objectiveEntity.IdMTPReview });

        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> EditObjective(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ObjetiveEntity objectiveEntity = await _context.Objetives.Include(o => o.Goal)
                                                                       .ThenInclude(g => g.MTP)
                                                                       .ThenInclude(m => m.Client)
                                                                       .Include(o => o.Classifications)
                                                                       .ThenInclude(oc => oc.Classification).FirstOrDefaultAsync(d => d.Id == id);
            ObjectiveViewModel model = _converterHelper.ToObjectiveViewModel(objectiveEntity);
            if (model == null)
            {
                return RedirectToAction("Home/Error404");
            }

            List<ClassificationEntity> list = new List<ClassificationEntity>();
            ClassificationEntity classification;
            foreach (Objetive_Classification item in model.Classifications)
            {
                classification = await _context.Classifications.FindAsync(item.Classification.Id);
                list.Add(classification);
            }

            MultiSelectList classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name", list.Select(c => c.Id));
            ViewData["classification"] = classification_list;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> EditObjective(ObjectiveViewModel model, IFormCollection form)
        {
            GoalEntity goal = await _context.Goals
                                            .Include(g => g.MTP)
                                            .ThenInclude(m => m.Client)
                                            .FirstOrDefaultAsync(m => m.Id == model.IdGoal);
            if (ModelState.IsValid)
            {
                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(model.Description))
                {
                    model.Description = (model.Description.Last() == '.') ? model.Description : $"{model.Description}.";
                    if (this.GenderEvaluation(goal.MTP.Client.Gender, model.Description))
                    {
                        gender_problems = "Description";
                    }
                }
                if (!string.IsNullOrEmpty(model.Intervention))
                {
                    model.Intervention = (model.Intervention.Last() == '.') ? model.Intervention : $"{model.Intervention}.";
                    if (this.GenderEvaluation(goal.MTP.Client.Gender, model.Intervention))
                    {
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? "Intervention" : $"{gender_problems}, Intervention";
                    }
                }
                if (!string.IsNullOrEmpty(gender_problems))     //el objective tiene problemas con el genero
                {
                    ModelState.AddModelError(string.Empty, $"Error.There are gender issues in: {gender_problems}");
                    model.Goal = goal;
                    return View(model);
                }

                ObjetiveEntity objective = await _converterHelper.ToObjectiveEntity(model, false);
                _context.Update(objective);

                ObjetiveEntity original_classifications = await _context.Objetives
                                                                        .Include(o => o.Classifications)
                                                                        .ThenInclude(oc => oc.Classification)
                                                                        .FirstOrDefaultAsync(d => d.Id == model.Id);
                _context.RemoveRange(original_classifications.Classifications);

                if (!string.IsNullOrEmpty(form["classifications"]))
                {
                    string[] classifications = form["classifications"].ToString().Split(',');
                    Objetive_Classification objclassification;
                    foreach (string value in classifications)
                    {
                        objclassification = new Objetive_Classification()
                        {
                            Objetive = objective,
                            Classification = await _context.Classifications.FindAsync(Convert.ToInt32(value))
                        };
                        _context.Add(objclassification);
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("UpdateObjectives", new { id = model.IdGoal });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the objective");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            model.Goal = goal;
            return View(model);
        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> EditObjectiveModal(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ObjetiveEntity objectiveEntity = await _context.Objetives

                                                           .Include(o => o.Goal)
                                                           .ThenInclude(g => g.MTP)
                                                           .ThenInclude(m => m.Client)

                                                           .FirstOrDefaultAsync(d => d.Id == id);

            ObjectiveViewModel model = _converterHelper.ToObjectiveViewModel(objectiveEntity);
            if (model == null)
            {
                return RedirectToAction("Home/Error404");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> EditObjectiveModal(ObjectiveViewModel model, IFormCollection form)
        {
            GoalEntity goal = await _context.Goals

                                            .Include(g => g.MTP)
                                            .ThenInclude(m => m.Client)

                                            .Include(g => g.Adendum)

                                            .FirstOrDefaultAsync(m => m.Id == model.IdGoal);
            if (ModelState.IsValid)
            {
                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(model.Description))
                {
                    model.Description = (model.Description.Last() == '.') ? model.Description : $"{model.Description}.";
                    if (this.GenderEvaluation(goal.MTP.Client.Gender, model.Description))
                    {
                        gender_problems = "Description";
                    }
                }
                if (!string.IsNullOrEmpty(model.Intervention))
                {
                    model.Intervention = (model.Intervention.Last() == '.') ? model.Intervention : $"{model.Intervention}.";
                    if (this.GenderEvaluation(goal.MTP.Client.Gender, model.Intervention))
                    {
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? "Intervention" : $"{gender_problems}, Intervention";
                    }
                }
                if (!string.IsNullOrEmpty(gender_problems))     //el objective tiene problemas con el genero
                {
                    ModelState.AddModelError(string.Empty, $"Error.There are gender issues in: {gender_problems}");
                    model.Goal = goal;
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjectiveModal", model) });
                    return View(model);
                }

                ObjetiveEntity objective = await _converterHelper.ToObjectiveEntity(model, false);
                _context.Update(objective);

                ObjetiveEntity original_classifications = await _context.Objetives

                                                                        .Include(o => o.Classifications)
                                                                        .ThenInclude(oc => oc.Classification)

                                                                        .FirstOrDefaultAsync(d => d.Id == model.Id);
                _context.RemoveRange(original_classifications.Classifications);

                if (!string.IsNullOrEmpty(form["classifications"]))
                {
                    string[] classifications = form["classifications"].ToString().Split(',');
                    Objetive_Classification objclassification;
                    foreach (string value in classifications)
                    {
                        objclassification = new Objetive_Classification()
                        {
                            Objetive = objective,
                            Classification = await _context.Classifications.FindAsync(Convert.ToInt32(value))
                        };
                        _context.Add(objclassification);
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();

                    if (goal.Adendum == null)
                    {
                        List<GoalEntity> goals = await _context.Goals
                                                           .Include(g => g.Objetives)
                                                           .Where(g => g.MTP.Id == goal.MTP.Id && g.Adendum == null && g.IdMTPReview == 0)
                                                           .ToListAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoals", goals) });
                    }
                    else
                    {
                        List<GoalEntity> goals = await _context.Goals
                                                           .Include(g => g.Objetives)
                                                           .Include(g => g.MTP)
                                                           .Include(g => g.Adendum)
                                                           .Where(g => g.Adendum.Id == goal.Adendum.Id)
                                                           .ToListAsync();
                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoals", goals) });
                    }
                    
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the objective");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            model.Goal = goal;
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjectiveModal", model) });
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> EditObjectiveMTPReviewModal(int? id, int idMTPReviewOfView)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ObjetiveEntity objectiveEntity = await _context.Objetives

                                                           .Include(o => o.Goal)
                                                           .ThenInclude(g => g.MTP)
                                                           .ThenInclude(m => m.Client)

                                                           .FirstOrDefaultAsync(d => d.Id == id);

            ObjectiveViewModel model = _converterHelper.ToObjectiveViewModel(objectiveEntity);
            model.IdMTPReviewOfView = idMTPReviewOfView;
            if (model == null)
            {
                return RedirectToAction("Home/Error404");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> EditObjectiveMTPReviewModal(ObjectiveViewModel model, IFormCollection form)
        {
            GoalEntity goal = await _context.Goals

                                            .Include(g => g.MTP)
                                            .ThenInclude(m => m.Client)

                                            .Include(g => g.Adendum)

                                            .FirstOrDefaultAsync(m => m.Id == model.IdGoal);
            if (ModelState.IsValid)
            {
                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(model.Description))
                {
                    model.Description = (model.Description.Last() == '.') ? model.Description : $"{model.Description}.";
                    if (this.GenderEvaluation(goal.MTP.Client.Gender, model.Description))
                    {
                        gender_problems = "Description";
                    }
                }
                if (!string.IsNullOrEmpty(model.Intervention))
                {
                    model.Intervention = (model.Intervention.Last() == '.') ? model.Intervention : $"{model.Intervention}.";
                    if (this.GenderEvaluation(goal.MTP.Client.Gender, model.Intervention))
                    {
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? "Intervention" : $"{gender_problems}, Intervention";
                    }
                }
                if (!string.IsNullOrEmpty(gender_problems))     //el objective tiene problemas con el genero
                {
                    ModelState.AddModelError(string.Empty, $"Error.There are gender issues in: {gender_problems}");
                    model.Goal = goal;
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjectiveMTPReviewModal", model) });
                    //return View(model);
                }

                ObjetiveEntity objective = await _converterHelper.ToObjectiveEntity(model, false);
                if (model.Compliment_IdMTPReview == 0 && model.Compliment == true)
                {
                    objective.Compliment_IdMTPReview = model.IdMTPReviewOfView;
                }
                else
                {
                    objective.Compliment_IdMTPReview = 0;
                }

                _context.Update(objective);

                ObjetiveEntity original_classifications = await _context.Objetives

                                                                        .Include(o => o.Classifications)
                                                                        .ThenInclude(oc => oc.Classification)

                                                                        .FirstOrDefaultAsync(d => d.Id == model.Id);
                _context.RemoveRange(original_classifications.Classifications);

                if (!string.IsNullOrEmpty(form["classifications"]))
                {
                    string[] classifications = form["classifications"].ToString().Split(',');
                    Objetive_Classification objclassification;
                    foreach (string value in classifications)
                    {
                        objclassification = new Objetive_Classification()
                        {
                            Objetive = objective,
                            Classification = await _context.Classifications.FindAsync(Convert.ToInt32(value))
                        };
                        _context.Add(objclassification);
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();

                    List<GoalEntity> goals = await _context.Goals

                                                           .Include(g => g.Objetives)

                                                           .Include(g => g.MTP)

                                                           .Where(g => g.MTP.Id == goal.MTP.Id)
                                                           .OrderBy(g => g.Number)
                                                           .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoalsMTPReview", goals, new Dictionary<string, object>() { { "Id", model.IdMTPReviewOfView } }) });

                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the objective");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            model.Goal = goal;
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjectiveMTPReviewModal", model) });
        }


        public IActionResult PrintMTP(int id)
        {
            MTPEntity mtpEntity = _context.MTPs.Include(m => m.Client)
                                               .ThenInclude(c => c.Clinic)

                                               .Include(m => m.Goals)
                                               .ThenInclude(g => g.Objetives)

                                               .Include(m => m.Goals)
                                               .ThenInclude(g => g.Adendum)

                                               .Include(wc => wc.Client)
                                               .ThenInclude(c => c.Clients_Diagnostics)
                                               .ThenInclude(cd => cd.Diagnostic)

                                               .Include(m => m.DocumentAssistant)

                                               .Include(m => m.Supervisor)

                                               .FirstOrDefault(m => (m.Id == id));
            if (mtpEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            if (mtpEntity.Client.Clinic.Name == "DAVILA")
            {
                Stream stream = _reportHelper.DavilaMTPReport(mtpEntity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (mtpEntity.Client.Clinic.Name == "LARKIN BEHAVIOR")
            {
                Stream stream = _reportHelper.LarkinMTPReport(mtpEntity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (mtpEntity.Client.Clinic.Name == "SOL & VIDA")
            {
                Stream stream = _reportHelper.SolAndVidaMTPReport(mtpEntity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (mtpEntity.Client.Clinic.Name == "DREAMS MENTAL HEALTH INC")
            {
                Stream stream = _reportHelper.DreamsMentalHealthMTPReport(mtpEntity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (mtpEntity.Client.Clinic.Name == "ADVANCED GROUP MEDICAL CENTER")
            {
                Stream stream = _reportHelper.AdvancedGroupMCMTPReport(mtpEntity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (mtpEntity.Client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
            {
                Stream stream = _reportHelper.FloridaSocialHSMTPReport(mtpEntity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (mtpEntity.Client.Clinic.Name == "ATLANTIC GROUP MEDICAL CENTER")
            {
                Stream stream = _reportHelper.AtlanticGroupMCMTPReport(mtpEntity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (mtpEntity.Client.Clinic.Name == "DEMO CLINIC SCHEMA 1")
            {
                Stream stream = _reportHelper.DemoClinic1MTPReport(mtpEntity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (mtpEntity.Client.Clinic.Name == "DEMO CLINIC SCHEMA 2")
            {
                Stream stream = _reportHelper.DemoClinic2MTPReport(mtpEntity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (mtpEntity.Client.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
            {
                Stream stream = _reportHelper.CommunityHTCMTPReport(mtpEntity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            return null;
        }

        public void UpdateMTPToNonActive(ClientEntity client)
        {
            List<MTPEntity> mtp_list = _context.MTPs.Where(m => m.Client == client).ToList();
            if (mtp_list.Count() > 0)
            {
                foreach (MTPEntity item in mtp_list)
                {
                    item.Active = false;
                    _context.Update(item);
                }
                _context.SaveChangesAsync();
            }
        }

        [Authorize(Roles = "Supervisor, Manager, Facilitator")]
        public async Task<IActionResult> ExpiredMTP()
        {
            UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                         .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
                return View(null);

            ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
            if (clinic != null)
            {
                List<MTPEntity> mtps = new List<MTPEntity>();
                if (User.IsInRole("Facilitator"))
                {
                    FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                    mtps = await _context.MTPs
                                         .Include(m => m.MtpReviewList)
                                         .Include(m => m.Client)
                                         .ThenInclude(c => c.Clinic)
                                         .Where(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                && m.Active == true && m.Client.Status == StatusType.Open
                                                && (m.Client.IdFacilitatorPSR == facilitator.Id || m.Client.IdFacilitatorGroup == facilitator.Id))).ToListAsync();
                }
                else
                {
                    mtps = await _context.MTPs
                                         .Include(m => m.MtpReviewList)
                                         .Include(m => m.Client)
                                         .ThenInclude(c => c.Clinic)
                                         .Where(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                              && m.Active == true && m.Client.Status == StatusType.Open)).ToListAsync();
                }
                List<MTPEntity> expiredMTPs = new List<MTPEntity>();
                int month = 0;
                foreach (var item in mtps)
                {
                    if (item.MtpReviewList != null)
                    {
                        foreach (var value in item.MtpReviewList)
                        {
                            month = month + value.MonthOfTreatment;

                        }
                    }
                    else
                    {
                        month = 0;
                    }

                    if (item.NumberOfMonths != null)
                    {
                        if (DateTime.Now > item.MTPDevelopedDate.Date.AddMonths(Convert.ToInt32(item.NumberOfMonths + month)))
                        {
                            expiredMTPs.Add(item);
                        }
                    }
                    month = 0;
                }
                return View(expiredMTPs);
            }
            else
                return View(null);
        }

        private bool GenderEvaluation(GenderType gender, string text)
        {
            if (gender == GenderType.Female)
            {
                return text.Contains(" he ") || text.Contains(" He ") || text.Contains(" his ") || text.Contains(" His ") || text.Contains(" him ") ||
                       text.Contains(" him.") || text.Contains("himself") || text.Contains("Himself") || text.Contains(" oldman") || text.Contains(" wife");
            }
            else
            {
                return text.Contains(" she ") || text.Contains(" She ") || text.Contains(" her.") || text.Contains(" her ") || text.Contains(" Her ") ||
                       text.Contains("herself") || text.Contains("Herself") || text.Contains(" oldwoman") || text.Contains(" husband");
            }
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
        public async Task<IActionResult> IndexAdendum(int idError = 0)
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
                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                if (clinic != null)
                {
                    if (User.IsInRole("Facilitator"))
                    {
                        return View(await _context.MTPs
                                                  .Include(m => m.AdendumList)
                                                  .ThenInclude(c => c.Facilitator)

                                                  .Include(m => m.AdendumList)
                                                  .ThenInclude(c => c.Goals)

                                                  .Include(c => c.Client)
                                                  .ThenInclude(c => c.Clinic)

                                                  .Where(m => (m.Client.Clinic.Id == clinic.Id && m.Client.Status == StatusType.Open
                                                        && (m.Client.IdFacilitatorPSR == facilitator.Id 
                                                            || m.Client.IndividualTherapyFacilitator.Id == facilitator.Id
                                                            || m.Client.IdFacilitatorGroup == facilitator.Id)))
                                                  .OrderBy(m => m.Client.Clinic.Name).ToListAsync());

                    }
                    else
                    {
                        return View(await _context.MTPs
                                                  .Include(m => m.AdendumList)
                                                  .ThenInclude(c => c.Facilitator)

                                                  .Include(m => m.AdendumList)
                                                  .ThenInclude(c => c.Goals)

                                                  .Include(c => c.Client)
                                                  .ThenInclude(c => c.Clinic)

                                                  .Where(m => (m.Client.Clinic.Id == clinic.Id && m.Client.Status == StatusType.Open))
                                                  .OrderBy(m => m.Client.Clinic.Name).ToListAsync());

                    }

                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public IActionResult CreateAdendum(int id = 0)
        {

            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.UserName == User.Identity.Name);

            AdendumViewModel model = new AdendumViewModel();

            if (User.IsInRole("Supervisor"))
            {
                model = new AdendumViewModel
                {
                    Mtp = _context.MTPs
                                  .Include(c => c.Client.Clients_Diagnostics)
                                  .ThenInclude(cd => cd.Diagnostic)
                                  .FirstOrDefault(n => n.Id == id),

                    Dateidentified = DateTime.Now,
                    ProblemStatement = "",
                    Duration = 6,
                    Frecuency = "once a week",
                    Id = 0,
                    IdMTP = id,
                    Status = AdendumStatus.Edition,
                    Unit = 4,
                    Facilitator = new FacilitatorEntity(),
                    IdSupervisor = _context.Supervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName).Id,
                    IdFacilitator = 0,
                    Goals = new List<GoalEntity>()
                };
            }
            if (User.IsInRole("Facilitator"))
            {
                model = new AdendumViewModel
                {
                    Mtp = _context.MTPs
                                  .Include(c => c.Client.Clients_Diagnostics)
                                  .ThenInclude(cd => cd.Diagnostic)
                                  .FirstOrDefault(n => n.Id == id),
                    Dateidentified = DateTime.Now,
                    ProblemStatement = "",
                    Duration = 6,
                    Frecuency = "once a week",
                    Id = 0,
                    IdMTP = id,
                    Status = AdendumStatus.Edition,
                    Unit = 4,
                    Supervisor = new SupervisorEntity(),
                    IdFacilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName).Id,
                    IdSupervisor = 0,
                    Goals = new List<GoalEntity>()
                };

                return View(model);
            }

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> CreateAdendum(AdendumViewModel adendumViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                AdendumEntity adendumEntity = _context.Adendums.Find(adendumViewModel.Id);
                MTPEntity mtp = _context.MTPs
                                              .Include(n => n.Client)
                                              .FirstOrDefault(n => n.Id == adendumViewModel.IdMTP);
                if (adendumEntity == null)
                {
                    string gender_problems = string.Empty;
                    if (!string.IsNullOrEmpty(adendumViewModel.ProblemStatement))
                    {
                        if (this.GenderEvaluation(mtp.Client.Gender, adendumViewModel.ProblemStatement))
                        {
                            ModelState.AddModelError(string.Empty, "Error.There are gender issues in: Problem Statement");
                            AdendumViewModel model1 = new AdendumViewModel
                            {
                                CreatedBy = adendumViewModel.CreatedBy,
                                CreatedOn = adendumViewModel.CreatedOn,
                                Dateidentified = adendumViewModel.Dateidentified,
                                Duration = adendumViewModel.Duration,
                                Facilitator = adendumViewModel.Facilitator,
                                Frecuency = adendumViewModel.Frecuency,
                                Goals = adendumViewModel.Goals,
                                Id = adendumViewModel.Id,
                                LastModifiedBy = adendumViewModel.LastModifiedBy,
                                LastModifiedOn = adendumViewModel.LastModifiedOn,

                                ProblemStatement = adendumViewModel.ProblemStatement,
                                Status = adendumViewModel.Status,
                                Supervisor = adendumViewModel.Supervisor,
                                Unit = adendumViewModel.Unit,
                                IdFacilitator = adendumViewModel.IdFacilitator,
                                IdMTP = adendumViewModel.IdMTP,
                                IdSupervisor = adendumViewModel.IdSupervisor,

                                Mtp = _context.MTPs
                                              .Include(c => c.Client.Clients_Diagnostics)
                                              .ThenInclude(cd => cd.Diagnostic)
                                              .FirstOrDefault(n => n.Id == adendumViewModel.IdMTP)


                            };
                            return View(model1);

                        }
                    }
                    adendumEntity = await _converterHelper.ToAdendumEntity(adendumViewModel, true, user_logged.UserName);

                    _context.Adendums.Add(adendumEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        return RedirectToAction("IndexAdendum", "MTPs");
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Adendum.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateAdendum", adendumViewModel) });
                }
            }
            AdendumViewModel model;
            model = new AdendumViewModel
            {
                Mtp = _context.MTPs
                                  .Include(c => c.Client.Clients_Diagnostics)
                                  .ThenInclude(cd => cd.Diagnostic)
                                  .FirstOrDefault(n => n.Id == adendumViewModel.Id),

                Dateidentified = DateTime.Now,
                ProblemStatement = "",
                Duration = 6,
                Frecuency = "once a week",
                Id = 0,
                IdMTP = adendumViewModel.Id,
                Status = AdendumStatus.Edition,
                Unit = 4,
                Facilitator = new FacilitatorEntity(),
                Supervisor = _context.Supervisors.FirstOrDefault(n => n.Name == user_logged.FullName)
            };
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateAdendum", adendumViewModel) });
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public IActionResult EditAdendum(int id = 0, int origin = 0)
        {
            AdendumViewModel model;

            if (User.IsInRole("Supervisor") || User.IsInRole("Facilitator"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    AdendumEntity Adendum = _context.Adendums

                                                    .Include(a => a.Mtp)
                                                    .ThenInclude(m => m.Client)
                                                    .ThenInclude(c => c.Clients_Diagnostics)
                                                    .ThenInclude(cd => cd.Diagnostic)

                                                    .Include(a => a.Goals)
                                                    .ThenInclude(g => g.Objetives)

                                                    .Include(a => a.Goals)
                                                    .ThenInclude(g => g.MTP)

                                                    .Include(a => a.Supervisor)

                                                    .Include(a => a.Facilitator)

                                                    .FirstOrDefault(a => a.Id == id);
                    if (Adendum == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToAdendumViewModel(Adendum);
                        model.Origin = origin;
                        return View(model);
                    }

                }
            }

            model = new AdendumViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> EditAdendum(AdendumViewModel adendumViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {

                AdendumEntity adendumEntity = await _converterHelper.ToAdendumEntity(adendumViewModel, false, user_logged.Id);

                MTPEntity mtp = _context.MTPs
                                              .Include(n => n.Client)
                                              .FirstOrDefault(n => n.Id == adendumViewModel.IdMTP);

                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(adendumViewModel.ProblemStatement))
                {
                    if (this.GenderEvaluation(mtp.Client.Gender, adendumViewModel.ProblemStatement))
                    {
                        ModelState.AddModelError(string.Empty, "Error.There are gender issues in: Problem Statement");
                        AdendumViewModel model1 = new AdendumViewModel
                        {
                            CreatedBy = adendumViewModel.CreatedBy,
                            CreatedOn = adendumViewModel.CreatedOn,
                            Dateidentified = adendumViewModel.Dateidentified,
                            Duration = adendumViewModel.Duration,
                            Facilitator = adendumViewModel.Facilitator,
                            Frecuency = adendumViewModel.Frecuency,
                            Goals = _context.Adendums

                                                    .Include(a => a.Mtp)
                                                    .ThenInclude(m => m.Client)
                                                    .ThenInclude(c => c.Clients_Diagnostics)
                                                    .ThenInclude(cd => cd.Diagnostic)

                                                    .Include(a => a.Goals)
                                                    .ThenInclude(g => g.Objetives)

                                                    .Include(a => a.Goals)
                                                    .ThenInclude(g => g.MTP)

                                                    .Include(a => a.Supervisor)

                                                    .Include(a => a.Facilitator)

                                                    .FirstOrDefault(a => a.Id == adendumViewModel.Id).Goals,
                            Id = adendumViewModel.Id,
                            LastModifiedBy = adendumViewModel.LastModifiedBy,
                            LastModifiedOn = adendumViewModel.LastModifiedOn,

                            ProblemStatement = adendumViewModel.ProblemStatement,
                            Status = adendumViewModel.Status,
                            Supervisor = adendumViewModel.Supervisor,
                            Unit = adendumViewModel.Unit,
                            IdFacilitator = adendumViewModel.IdFacilitator,
                            IdMTP = adendumViewModel.IdMTP,
                            IdSupervisor = adendumViewModel.IdSupervisor,

                            Mtp = _context.MTPs
                                          .Include(c => c.Client.Clients_Diagnostics)
                                          .ThenInclude(cd => cd.Diagnostic)
                                          .FirstOrDefault(n => n.Id == adendumViewModel.IdMTP)


                        };
                        return View(model1);

                    }
                }

                _context.Adendums.Update(adendumEntity);
                try
                {
                    List<MessageEntity> messages = adendumEntity.Messages.Where(m => (m.Status == MessageStatus.NotRead && m.Notification == false)).ToList();
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
                            Addendum = adendumEntity,
                            Discharge = null,
                            Title = "Update on reviewed addendum",
                            Text = $"The addendum of {adendumEntity.Mtp.Client.Name} that was created by {adendumEntity.CreatedBy} was rectified",
                            From = value.To,
                            To = value.From,
                            DateCreated = DateTime.Now,
                            Status = MessageStatus.NotRead,
                            Notification = true
                        };
                        _context.Add(notification);
                    }

                    await _context.SaveChangesAsync();
                    
                    if (adendumViewModel.Origin == 1)
                    {
                        return RedirectToAction("MessagesOfAddendums", "Messages");
                    }
                    return RedirectToAction("IndexAdendum", "MTPs");
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditAdendum", adendumViewModel) });
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> FinishEditingAdendum(int id)
        {
            AdendumEntity adendum = await _context.Adendums.FirstOrDefaultAsync(n => n.Id == id);
            if (User.IsInRole("Supervisor"))
            {
                adendum.Status = AdendumStatus.Approved;
            }
            else
            {
                adendum.Status = AdendumStatus.Pending;
            }

            _context.Update(adendum);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(IndexAdendum));
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ApproveAdendum(int id, int origin = 0)
        {

            AdendumEntity adendum = await _context.Adendums.FirstOrDefaultAsync(n => n.Id == id);


            adendum.Status = AdendumStatus.Approved;
            // adendum.DateOfApprove = DateTime.Now;
            adendum.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.LinkedUser == User.Identity.Name);
            _context.Update(adendum);

            await _context.SaveChangesAsync();

            if (origin == 1)
            {
                return RedirectToAction(nameof(PendingAdendum));
            }

            return RedirectToAction(nameof(IndexAdendum));
        }

        [Authorize(Roles = "Supervisor, Manager, Facilitator")]
        public async Task<IActionResult> PendingAdendum(int idError = 0)
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
                    FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                    if (User.IsInRole("Facilitator"))
                    {
                        return View(await _context.Adendums

                                                  .Include(a => a.Mtp)
                                                  .ThenInclude(a => a.Client)
                                                  .ThenInclude(a => a.Clinic)

                                                  .Include(a => a.Goals)
                                                  .ThenInclude(a => a.Objetives)

                                                  .Include(f => f.Messages.Where(m => m.Notification == false))

                                                  .Where(a => (a.Mtp.Client.Clinic.Id == clinic.Id)
                                                            && a.Status == AdendumStatus.Pending 
                                                            && (a.Mtp.Client.IdFacilitatorPSR == facilitator.Id
                                                               || a.Mtp.Client.IndividualTherapyFacilitator.Id == facilitator.Id
                                                               || a.Mtp.Client.IdFacilitatorGroup == facilitator.Id))
                                                  .OrderBy(a => a.Mtp.Client.Clinic.Name).ToListAsync());

                    }
                    else
                    {
                        return View(await _context.Adendums

                                                  .Include(a => a.Mtp)
                                                  .ThenInclude(a => a.Client)
                                                  .ThenInclude(a => a.Clinic)

                                                  .Include(a => a.Goals)
                                                  .ThenInclude(a => a.Objetives)

                                                  .Include(f => f.Messages.Where(m => m.Notification == false))

                                                  .Where(a => (a.Mtp.Client.Clinic.Id == clinic.Id)
                                                            && a.Status == AdendumStatus.Pending)
                                                  .OrderBy(a => a.Mtp.Client.Clinic.Name).ToListAsync());

                    }

                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ReviewAdendum(int id)
        {
            AdendumEntity adendum = await _context.Adendums.FirstOrDefaultAsync(n => n.Id == id);
            adendum.Status = AdendumStatus.Edition;
            _context.Update(adendum);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingAdendum));
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
        public IActionResult PrintAdendum(int id)
        {
            AdendumEntity entity = _context.Adendums

                                           .Include(a => a.Mtp)
                                           .ThenInclude(m => m.Client)
                                           .ThenInclude(c => c.Clinic)

                                           .Include(a => a.Goals)
                                           .ThenInclude(g => g.Objetives)

                                           .Include(a => a.Facilitator)

                                           .Include(a => a.Supervisor)

                                           .Include(a => a.Mtp)
                                           .ThenInclude(m => m.Client)
                                           .ThenInclude(c => c.LegalGuardian)

                                           .Include(wc => wc.Mtp)
                                           .ThenInclude(m => m.Client)
                                           .ThenInclude(c => c.Clients_Diagnostics)
                                           .ThenInclude(cd => cd.Diagnostic)

                                           .FirstOrDefault(a => (a.Id == id));

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            if (entity.Mtp.Client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
            {
                Stream stream = _reportHelper.FloridaSocialHSAddendumReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Mtp.Client.Clinic.Name == "DREAMS MENTAL HEALTH INC")
            {
                Stream stream = _reportHelper.DreamsMentalHealthAddendumReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            return null;
        }

        [Authorize(Roles = "Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> DeleteGoalOfAddendum(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GoalEntity goalEntity = await _context.Goals

                                                  .Include(g => g.MTP)

                                                  .Include(g => g.Adendum)

                                                  .FirstOrDefaultAsync(g => g.Id == id);
            if (goalEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Goals.Remove(goalEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("EditAdendum", new { id = goalEntity.Adendum.Id });
            }

            if (goalEntity.Adendum == null)
            {
                return RedirectToAction("Edit", new { id = goalEntity.MTP.Id });
            }
            else
            {
                return RedirectToAction("EditAdendum", new { id = goalEntity.Adendum.Id });
            }
            
           
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> DeleteGoalOfMTPreview(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GoalEntity goalEntity = await _context.Goals

                                                  .Include(g => g.MTP)

                                                  .ThenInclude(g => g.MtpReviewList)

                                                  .FirstOrDefaultAsync(g => g.Id == id);
            if (goalEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Goals.Remove(goalEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("EditMTPReview", new { id = goalEntity.IdMTPReview });
            }

            return RedirectToAction("EditMTPReview", new { id = goalEntity.IdMTPReview });
        }

        [Authorize(Roles = "Facilitator, Supervisor")]
        public async Task<IActionResult> EditMTPReview(int? id, int origin = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MTPReviewEntity mtpReviewEntity = await _context.MTPReviews.Include(m => m.Mtp.Client)
                                                                       .ThenInclude(m => m.Clinic)
                                                                       .Include(m => m.Mtp.Goals)
                                                                       .ThenInclude(m => m.Objetives)
                                                                       .FirstOrDefaultAsync(m => m.Id == id);
            if (mtpReviewEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MTPReviewViewModel mtpReviewViewModel = _converterHelper.ToMTPReviewViewModel(mtpReviewEntity);
            mtpReviewViewModel.Origin = origin;
            if (mtpReviewViewModel.Mtp.Client.LegalGuardian == null)
                mtpReviewViewModel.Mtp.Client.LegalGuardian = new LegalGuardianEntity();
            return View(mtpReviewViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Facilitator, Supervisor")]
        public async Task<IActionResult> EditMTPReview(int id, MTPReviewViewModel mtpReviewViewModel)
        {
            if (id != mtpReviewViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                MTPReviewEntity mtpReviewEntity = await _converterHelper.ToMTPReviewEntity(mtpReviewViewModel, false, user_logged.Id);

                _context.Update(mtpReviewEntity);

                try
                {
                    List<MessageEntity> messages = mtpReviewEntity.Messages.Where(m => (m.Status == MessageStatus.NotRead && m.Notification == false)).ToList();
                    //todos los mensajes no leidos que tiene el mtp review los pongo como leidos
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
                            MTPReview = mtpReviewEntity,
                            Addendum = null,
                            Discharge = null,
                            Title = "Update on reviewed MTP Review",
                            Text = $"The MTP review of {mtpReviewEntity.Mtp.Client.Name} that was created by {mtpReviewEntity.CreatedBy} was rectified",
                            From = value.To,
                            To = value.From,
                            DateCreated = DateTime.Now,
                            Status = MessageStatus.NotRead,
                            Notification = true
                        };
                        _context.Add(notification);
                    }

                    await _context.SaveChangesAsync();
                    if (mtpReviewViewModel.Origin == 1)
                    {
                        return RedirectToAction(nameof(PendingMtpReview));
                    }
                    if (mtpReviewViewModel.Origin == 2)
                    {
                        return RedirectToAction(nameof(MTPRinEdit));
                    }
                    if (mtpReviewViewModel.Origin == 3)
                    {
                        return RedirectToAction("MessagesOfMTPReviews", "Messages");
                    }
                    if (mtpReviewViewModel.Origin == 4)
                    {
                        return RedirectToAction("Notifications", "Messages");
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the facilitator: {mtpReviewEntity.Mtp.Client.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(mtpReviewViewModel);
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> FinishEditingMtpReview(int id, int origin = 0)
        {
            MTPReviewEntity MtpReview = await _context.MTPReviews.FirstOrDefaultAsync(n => n.Id == id);
            if (User.IsInRole("Supervisor"))
            {
                MtpReview.Status = AdendumStatus.Approved;
            }
            else
            {
                MtpReview.Status = AdendumStatus.Pending;
            }

            _context.Update(MtpReview);

            await _context.SaveChangesAsync();

            if (origin == 2)
            {
                return RedirectToAction(nameof(MTPRinEdit));
            }

            return RedirectToAction("Index", "MTPs");
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ApproveMTPReview(int id, int origin = 0)
        {
            UserEntity user_logged = await _context.Users
                                                  .Include(u => u.Clinic)
                                                  .ThenInclude(c => c.Setting)
                                                  .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            MTPReviewEntity mtpReview = await _context.MTPReviews.FirstOrDefaultAsync(n => n.Id == id);
            mtpReview.Status = AdendumStatus.Approved;
            mtpReview.LicensedPractitioner = _context.Supervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName).Name;
                
            _context.Update(mtpReview);

            await _context.SaveChangesAsync();

            if (origin == 1)
            {
                return RedirectToAction(nameof(PendingMtpReview));
            }
            if (origin == 2)
            {
                return RedirectToAction(nameof(MTPRinEdit));
            }
            if (origin == 4)
            {
                return RedirectToAction("Notifications", "Messages");
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Supervisor, Manager, Facilitator")]
        public async Task<IActionResult> PendingMtpReview(int idError = 0)
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
                    FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                    if (User.IsInRole("Facilitator"))
                    {
                        return View(await _context.MTPReviews

                                                  .Include(m => m.Mtp)
                                                  .ThenInclude(m => m.Client)
                                                  .ThenInclude(m => m.Clinic)

                                                  .Include(m => m.Mtp.Goals)
                                                  .ThenInclude(m => m.Objetives)

                                                  .Include(f => f.Messages.Where(m => m.Notification == false))

                                                  .Where(m => (m.Mtp.Client.Clinic.Id == clinic.Id)
                                                            && m.Status == AdendumStatus.Pending 
                                                            && m.CreatedBy == user_logged.UserName)
                                                  .ToListAsync());
                    }
                    else
                    {
                        return View(await _context.MTPReviews

                                                  .Include(m => m.Mtp)
                                                  .ThenInclude(m => m.Client)
                                                  .ThenInclude(m => m.Clinic)

                                                  .Include(m => m.Mtp.Goals)
                                                  .ThenInclude(m => m.Objetives)

                                                  .Include(f => f.Messages.Where(m => m.Notification == false))

                                                  .Where(m => (m.Mtp.Client.Clinic.Id == clinic.Id)
                                                      && m.Status == AdendumStatus.Pending)
                                                  .ToListAsync());
                    }
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> ReviewMTPReview(int id)
        {
            MTPReviewEntity mtpReview = await _context.MTPReviews.FirstOrDefaultAsync(n => n.Id == id);
            mtpReview.Status = AdendumStatus.Edition;
            _context.Update(mtpReview);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingMtpReview));
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public IActionResult CreateMTPReview(int id = 0, int origin = 0)
        {

            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.UserName == User.Identity.Name);

            MTPReviewViewModel model = new MTPReviewViewModel();
            MTPEntity mtp = _context.MTPs
                                  .Include(n => n.Client)
                                  .ThenInclude(n => n.LegalGuardian)
                                  .Include(n => n.Client.Clinic)
                                  .Include(n => n.Goals)
                                  .ThenInclude(n => n.Objetives)
                                  .FirstOrDefault(m => m.Id == id);

            if (User.IsInRole("Supervisor"))
            {
                model = new MTPReviewViewModel
                {
                    CreatedOn = DateTime.Now,
                    Therapist = user_logged.FullName,
                    DateClinicalDirector = DateTime.Now,
                    DateLicensedPractitioner = DateTime.Now,
                    DateSignaturePerson = DateTime.Now,
                    DateTherapist = DateTime.Now,
                    ReviewedOn = mtp.AdmissionDateMTP.AddMonths(Convert.ToInt32(mtp.NumberOfMonths)),
                    Status = AdendumStatus.Edition,
                    ACopy = false,
                    ClinicalDirector = mtp.Client.Clinic.ClinicalDirector,
                    CreatedBy = user_logged.UserName,
                    DescribeAnyGoals = "",
                    DescribeClient = "",
                    Documents = true,
                    Id = 0,
                    IdMTP = id,
                    IfCurrent = "",
                    LicensedPractitioner = "",
                    Mtp = mtp,
                    MTP_FK = id,
                    NumberUnit = 4,
                    ServiceCode = "H0032TS",
                    ProviderNumber = "",
                    SpecifyChanges = "",
                    SummaryOfServices = "",
                    TheConsumer = false,
                    TheTreatmentPlan = false,
                    Frecuency = "Four times per week",
                    MonthOfTreatment = 3,
                    Setting = "02",
                    DataOfService = mtp.AdmissionDateMTP.AddMonths(Convert.ToInt32(mtp.NumberOfMonths)),
                    Origin = origin
                };
            }
            if (User.IsInRole("Facilitator"))
            {
                model = new MTPReviewViewModel
                {
                    CreatedOn = DateTime.Now,
                    Therapist = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName).Name,
                    DateClinicalDirector = DateTime.Now,
                    DateLicensedPractitioner = DateTime.Now,
                    DateSignaturePerson = DateTime.Now,
                    DateTherapist = DateTime.Now,
                    ReviewedOn = mtp.AdmissionDateMTP.AddMonths(Convert.ToInt32(mtp.NumberOfMonths)),
                    Status = AdendumStatus.Edition,
                    ACopy = false,
                    ClinicalDirector = mtp.Client.Clinic.ClinicalDirector,
                    CreatedBy = user_logged.UserName,
                    DescribeAnyGoals = "",
                    DescribeClient = "",
                    Documents = true,
                    Id = 0,
                    IdMTP = id,
                    IfCurrent = "",
                    LicensedPractitioner = "",
                    Mtp = mtp,
                    MTP_FK = id,
                    NumberUnit = 4,
                    ServiceCode = "H0032TS",
                    ProviderNumber = "",
                    SpecifyChanges = "",
                    SummaryOfServices = "",
                    TheConsumer = false,
                    TheTreatmentPlan = false,
                    Frecuency = "Four times per week",
                    MonthOfTreatment = 3,
                    Setting = "02",
                    DataOfService = mtp.AdmissionDateMTP.AddMonths(Convert.ToInt32(mtp.NumberOfMonths)),
                    Origin = origin
                };

            }

            if (model.Mtp.Client.LegalGuardian == null)
                model.Mtp.Client.LegalGuardian = new LegalGuardianEntity();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> CreateMTPReview(MTPReviewViewModel reviewViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                MTPReviewEntity reviewEntity = _context.MTPReviews.Find(reviewViewModel.Id);
                if (reviewEntity == null)
                {
                    reviewEntity = await _converterHelper.ToMTPReviewEntity(reviewViewModel, true, reviewViewModel.CreatedBy);

                    _context.MTPReviews.Add(reviewEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        if (reviewViewModel.Origin == 1)
                        {
                            return RedirectToAction(nameof(ExpiredMTP));
                        }

                        if (reviewViewModel.Origin == 2)
                        {
                            return RedirectToAction(nameof(MTPRinEdit));
                        }

                        return RedirectToAction(nameof(Index));
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the MTP Review.");

                    reviewViewModel.Mtp = _context.MTPs
                                                 .Include(n => n.Client)
                                                 .ThenInclude(n => n.LegalGuardian)
                                                 .Include(n => n.Client.Clinic)
                                                 .Include(n => n.Goals)
                                                 .ThenInclude(n => n.Objetives)
                                                 .FirstOrDefault(m => m.Id == reviewViewModel.IdMTP);
                    return View(reviewViewModel);
                }
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateMTPReview", reviewViewModel.Id) });
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
        public IActionResult PrintMTPReview(int id)
        {
            MTPReviewEntity entity = _context.MTPReviews

                                             .Include(a => a.Mtp)
                                             .ThenInclude(m => m.Client)
                                             .ThenInclude(c => c.Clinic)

                                             .Include(a => a.Mtp)
                                             .ThenInclude(m => m.Client)
                                             .ThenInclude(c => c.LegalGuardian)

                                             .Include(wc => wc.Mtp)
                                             .ThenInclude(m => m.Client)
                                             .ThenInclude(c => c.Clients_Diagnostics)
                                             .ThenInclude(cd => cd.Diagnostic)

                                             .FirstOrDefault(a => (a.Id == id));

            if (entity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            if (entity.Mtp.Client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
            {
                Stream stream = _reportHelper.FloridaSocialHSMTPReviewReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (entity.Mtp.Client.Clinic.Name == "DREAMS MENTAL HEALTH INC")
            {
                Stream stream = _reportHelper.DreamsMentalHealthMTPReviewReport(entity);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            return null;
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public JsonResult GetCodeByClient(int idClient)
        {
            ClientEntity client = _context.Clients.FirstOrDefault(c => c.Id == idClient);
            string text = string.Empty;
            if (client != null)
            {
                text = client.Code;
            }
            return Json(text);
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public JsonResult GetDiagnosisByClient(int idClient)
        {
            ClientEntity client = _context.Clients

                                          .Include(c => c.Clients_Diagnostics)
                                          .ThenInclude(cd => cd.Diagnostic)

                                          .FirstOrDefault(c => c.Id == idClient);
            string text = string.Empty;
            if (client != null)
            {
                foreach (var item in client.Clients_Diagnostics)
                {
                    text += $"{item.Diagnostic.Code} - {item.Diagnostic.Description} ";
                }
            }

            return Json(text);
        }

        [Authorize(Roles = "Facilitator, Supervisor")]
        public async Task<IActionResult> MTPRinEdit(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            if (User.IsInRole("Supervisor"))
            {
                return RedirectToAction("PendingMtpReview", "MTPs");
            }
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
                    FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);
                    if (User.IsInRole("Facilitator"))
                    {
                        List<MTPEntity> mtp = await _context.MTPs
                                                  .Include(m => m.MtpReviewList)

                                                  .Include(c => c.Client)
                                                  .ThenInclude(c => c.Clinic)

                                                  .Where(m => (m.Client.Clinic.Id == clinic.Id
                                                        && (m.MtpReviewList.Where(r => r.CreatedBy == user_logged.UserName).Count() > 0)
                                                        ))
                                                  .OrderBy(m => m.Client.Clinic.Name)
                                                  .ToListAsync();

                        List<MTPEntity> mtp1 = new List<MTPEntity>();
                        foreach (var item in mtp)
                        {
                            foreach (var value in item.MtpReviewList)
                            {
                                if (value.Status == AdendumStatus.Edition && mtp1.Where(n => n.Id == value.Mtp.Id).ToList().Count() == 0)
                                    mtp1.Add(item);

                            }
                        }
                        return View(mtp1);
                    }

                    return RedirectToAction("Home/Error404");
                }
                else
                    return View(null);
            }
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
                    IdMTPReview = id,
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
                model.To = model.MTPReview.CreatedBy;
                _context.Add(model);
                await _context.SaveChangesAsync();
            }

            if (messageViewModel.Origin == 1)
                return RedirectToAction("PendingMtpReview");

            if (messageViewModel.Origin == 2)
                return RedirectToAction("MTPRinEdit");

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Supervisor")]
        public IActionResult AddAddendumMessageEntity(int id = 0, int origin = 0)
        {
            if (id == 0)
            {
                return View(new MessageViewModel());
            }
            else
            {
                MessageViewModel model = new MessageViewModel()
                {
                    IdAddendum = id,
                    Origin = origin
                };

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> AddAddendumMessageEntity(MessageViewModel messageViewModel)
        {
            if (ModelState.IsValid)
            {
                MessageEntity model = await _converterHelper.ToMessageEntity(messageViewModel, true);
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                model.From = user_logged.UserName;
                model.To = model.Addendum.CreatedBy;
                _context.Add(model);
                await _context.SaveChangesAsync();
            }

            if (messageViewModel.Origin == 1)
                return RedirectToAction("PendingAdendum");

            return RedirectToAction("IndexAdendum");
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> MTPForClient(int idClient = 0)
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
                    return View(await _context.MTPs

                                              .Include(f => f.Client)

                                              .Where(n => (n.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && n.Client.Id == idClient))
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

                if (User.IsInRole("Documents_Assistant"))
                    return View(await _context.MTPs

                                              .Include(f => f.Client)

                                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && n.Client.Id == idClient)
                                              .OrderBy(f => f.Client.Name)
                                              .ToListAsync());

                if (User.IsInRole("Facilitator"))
                {
                    return View(await _context.MTPs

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

        [Authorize(Roles = "Manager, Supervisor, Facilitator, Documents_Assistant")]
        public async Task<IActionResult> AddendumForClient(int idClient = 0)
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
                    return View(await _context.Adendums

                                              .Include(f => f.Mtp.Client)

                                              .Where(n => (n.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && n.Mtp.Client.Id == idClient))
                                              .OrderBy(f => f.Mtp.Client.Name)
                                              .ToListAsync());

                if (User.IsInRole("Documents_Assistant"))
                    return View(await _context.Adendums

                                              .Include(f => f.Mtp.Client)

                                              .Where(n => n.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && n.Mtp.Client.Id == idClient)
                                              .OrderBy(f => f.Mtp.Client.Name)
                                              .ToListAsync());

                if (User.IsInRole("Facilitator"))
                {
                    return View(await _context.Adendums

                                              .Include(f => f.Mtp.Client)

                                              .Where(n => n.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                && n.Mtp.Client.Id == idClient
                                                && (n.Mtp.Client.IdFacilitatorPSR == facilitator.Id 
                                                    || n.Mtp.Client.IndividualTherapyFacilitator.Id == facilitator.Id
                                                    || n.Mtp.Client.IdFacilitatorGroup == facilitator.Id))
                                              .OrderBy(f => f.Mtp.Client.Name)
                                              .ToListAsync());
                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> EditReadOnly(int? id, int origi = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = await _context.Users
                                                  .Include(u => u.Clinic)
                                                  .ThenInclude(c => c.Setting)
                                                  .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            MTPEntity mtpEntity = await _context.MTPs.Include(m => m.Client)
                                                     .ThenInclude(c => c.Clients_Diagnostics)
                                                     .ThenInclude(c => c.Diagnostic)
                                                     .Include(m => m.MtpReviewList)
                                                     .Include(m => m.DocumentAssistant)
                                                     .Include(m => m.Goals.Where(m => m.Adendum == null && m.IdMTPReview == 0))
                                                     .ThenInclude(c => c.Objetives)
                                                     .FirstOrDefaultAsync(m => m.Id == id);
            if (mtpEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MTPViewModel mtpViewModel = _converterHelper.ToMTPViewModel(mtpEntity);

            if (User.IsInRole("Supervisor") || User.IsInRole("Documents_Assistant"))
            {

                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = mtpEntity.Client.Name,
                    Value = $"{mtpEntity.Client.Id}"
                });
                mtpViewModel.Clients = list;
                if (mtpViewModel.Goals == null)
                    mtpViewModel.Goals = new List<GoalEntity>();

                ViewData["origi"] = origi;
                return View(mtpViewModel);
            }

            return RedirectToAction(nameof(Index));

        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> FinishEditing(int id)
        {
            MTPEntity mtp = await _context.MTPs.FirstOrDefaultAsync(n => n.Id == id);
            if (User.IsInRole("Supervisor"))
            {
                mtp.Status = MTPStatus.Approved;
                mtp.SupervisorDate = DateTime.Now;
                mtp.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.LinkedUser == User.Identity.Name);
            }
            else
            {
                mtp.Status = MTPStatus.Pending;
            }

            _context.Update(mtp);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Approve(int id, int origi = 0)
        {

            MTPEntity mtp = await _context.MTPs.FirstOrDefaultAsync(n => n.Id == id);


            mtp.Status = MTPStatus.Approved;
            mtp.SupervisorDate = DateTime.Now;
            mtp.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.LinkedUser == User.Identity.Name);
            _context.Update(mtp);

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

        [Authorize(Roles = "Supervisor, Manager, Documents_Assistant")]
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
                        return View(await _context.MTPs

                                                  .Include(a => a.Client)
                                                  .ThenInclude(a => a.Clinic)

                                                  .Include(a => a.Goals)
                                                  .ThenInclude(a => a.Objetives)

                                                  .Include(f => f.Messages.Where(m => m.Notification == false))

                                                  .Where(a => (a.Client.Clinic.Id == clinic.Id)
                                                            && a.Status == MTPStatus.Pending && (a.DocumentAssistant.Id == documentAssistant.Id))
                                                  .OrderBy(a => a.Client.Clinic.Name).ToListAsync());

                    }
                    else
                    {
                        return View(await _context.MTPs

                                                  .Include(a => a.Client)
                                                  .ThenInclude(a => a.Clinic)

                                                  .Include(a => a.Goals)
                                                  .ThenInclude(a => a.Objetives)

                                                  .Include(f => f.Messages.Where(m => m.Notification == false))

                                                  .Where(a => (a.Client.Clinic.Id == clinic.Id)
                                                            && a.Status == MTPStatus.Pending)
                                                  .OrderBy(a => a.Client.Clinic.Name).ToListAsync());

                    }

                }
            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Supervisor")]
        public IActionResult AddMessageMtpEntity(int id = 0, int origin = 0)
        {
            if (id == 0)
            {
                return View(new MessageViewModel());
            }
            else
            {
                MessageViewModel model = new MessageViewModel()
                {
                    IdMtp = id,
                    Origin = origin
                };

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> AddMessageMtpEntity(MessageViewModel messageViewModel)
        {
            if (ModelState.IsValid)
            {
                MessageEntity model = await _converterHelper.ToMessageEntity(messageViewModel, true);
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                model.From = user_logged.UserName;
                model.To = model.Mtp.CreatedBy;
                _context.Add(model);
                await _context.SaveChangesAsync();
            }

            if (messageViewModel.Origin == 1)
                return RedirectToAction("Pending");

            if (messageViewModel.Origin == 2)
                return RedirectToAction("MTPRinEdit");

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Documents_Assistant, Supervisor")]
        public async Task<IActionResult> MtpWithReview()
        {
            if (User.IsInRole("Documents_Assistant"))
            {
                List<MTPEntity> salida = await _context.MTPs
                                                       .Include(wc => wc.Client)
                                                       .Include(wc => wc.DocumentAssistant)
                                                       .Include(wc => wc.Messages.Where(m => m.Notification == false))
                                                       .Where(wc => (wc.DocumentAssistant.LinkedUser == User.Identity.Name
                                                               && wc.Status == MTPStatus.Pending
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
                    List<MTPEntity> salida = await _context.MTPs
                                                      .Include(wc => wc.Client)
                                                      .Include(wc => wc.DocumentAssistant)
                                                      .Include(wc => wc.Messages.Where(m => m.Notification == false))
                                                      .Where(wc => (wc.Client.Clinic.Id == user_logged.Clinic.Id
                                                              && wc.Status == MTPStatus.Pending
                                                              && wc.Messages.Count() > 0))
                                                      .ToListAsync();
                    return View(salida);
                }
            }

            return View();
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> CreateGoalModalTemp(DateTime admissionDate, int idClient = 0, int numberMonths = 0)
        {
            if (idClient == 0)
            {
               return RedirectToAction("Create", new { id = 1});
            }

            UserEntity user_logged = await _context.Users
                                                      .Include(u => u.Clinic)
                                                      .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            List<GoalsTempEntity> goaltemp = await _context.GoalsTemp
                                                           .Where(m => (m.UserName == user_logged.UserName
                                                                && m.IdClient == idClient))
                                                           .ToListAsync();

            if (goaltemp == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GoalsTempViewModel model = new GoalsTempViewModel
            {
                Number = goaltemp.Count() + 1,
                IdService = 0,
                Services = _combosHelper.GetComboServices(),
                IdClient = idClient,
                UserName = user_logged.UserName,
                numberMonths = numberMonths,
                AdmissionDate = admissionDate
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> CreateGoalModalTemp(GoalsTempViewModel model)
        {
            UserEntity user_logged = await _context.Users
                                                     .Include(u => u.Clinic)
                                                     .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            ClientEntity client = await _context.Clients.FirstOrDefaultAsync(m => m.Id == model.IdClient);

            if (ModelState.IsValid)
            {
                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(model.Name))
                {
                    model.Name = (model.Name.Last() == '.') ? model.Name : $"{model.Name}.";
                    if (this.GenderEvaluation(client.Gender, model.Name))
                    {
                        gender_problems = "Name";
                    }
                }
                if (!string.IsNullOrEmpty(model.AreaOfFocus))
                {
                    model.AreaOfFocus = (model.AreaOfFocus.Last() == '.') ? model.AreaOfFocus : $"{model.AreaOfFocus}.";
                    if (this.GenderEvaluation(client.Gender, model.AreaOfFocus))
                    {
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? "Area of Focus" : $"{gender_problems}, Area of Focus";
                    }
                }
                if (!string.IsNullOrEmpty(gender_problems))     //el goal tiene problemas con el genero
                {
                    ModelState.AddModelError(string.Empty, $"Error.There are gender issues in: {gender_problems}");
                    model.Services = _combosHelper.GetComboServices();
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateGoalModalTemp", model) });
                    //return View(model);
                }
               
                GoalsTempEntity goalTempEntity = await _converterHelper.ToGoalTempEntity(model, true);
                _context.Add(goalTempEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<GoalsTempEntity> goalsTemp = await _context.GoalsTemp
                                                                    .Include(g => g.ObjetiveTempList)
                                                                    .Where(g => g.IdClient == model.IdClient && g.UserName == user_logged.UserName)
                                                                    .ToListAsync();
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoalsTemp", goalsTemp) });

                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            model.Services = _combosHelper.GetComboServices();
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateGoalModalTemp", model) });
        }

        [Authorize(Roles = "Documents_Assistant, Supervisor")]
        public void DeleteGoalsTemp(int idClient = 0)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);
            //delete all ReferredsTemp
            IQueryable<GoalsTempEntity> list_to_delete = _context.GoalsTemp;
            foreach (GoalsTempEntity item in list_to_delete)
            {
                if (item.UserName == user_logged.UserName && item.IdClient == idClient)
                {
                    _context.GoalsTemp.Remove(item);
                }
            }
            _context.SaveChanges();
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> EditGoalModalTemp(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GoalsTempEntity goalTempEntity = await _context.GoalsTemp
                                                           .FirstOrDefaultAsync(d => d.Id == id);

            GoalsTempViewModel model = _converterHelper.ToGoalTempViewModel(goalTempEntity);
            if (model == null)
            {
                return RedirectToAction("Home/Error404");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> EditGoalModalTemp(int id, GoalsTempViewModel model)
        {
            if (id != model.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);

            ClientEntity client = await _context.Clients.FirstOrDefaultAsync(m => m.Id == model.IdClient);

            if (ModelState.IsValid)
            {
                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(model.Name))
                {
                    model.Name = (model.Name.Last() == '.') ? model.Name : $"{model.Name}.";
                    if (this.GenderEvaluation(client.Gender, model.Name))
                    {
                        gender_problems = "Name";
                    }
                }
                if (!string.IsNullOrEmpty(model.AreaOfFocus))
                {
                    model.AreaOfFocus = (model.AreaOfFocus.Last() == '.') ? model.AreaOfFocus : $"{model.AreaOfFocus}.";
                    if (this.GenderEvaluation(client.Gender, model.AreaOfFocus))
                    {
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? "Area of Focus" : $"{gender_problems}, Area of Focus";
                    }
                }
                if (!string.IsNullOrEmpty(gender_problems))     //el goal tiene problemas con el genero
                {
                    ModelState.AddModelError(string.Empty, $"Error.There are gender issues in: {gender_problems}");
                    model.Services = _combosHelper.GetComboServices();
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditGoalModalTemp", model) });
                    //return View(model);
                }

                GoalsTempEntity goalEntity = await _converterHelper.ToGoalTempEntity(model, false);
                _context.Update(goalEntity);
                try
                {
                    await _context.SaveChangesAsync();
                   
                   List<GoalsTempEntity> goalsTemp = await _context.GoalsTemp
                                                          .Include(g => g.ObjetiveTempList)
                                                          .Where(g => g.IdClient == model.IdClient && g.UserName == user_logged.UserName)
                                                          .ToListAsync();
                   return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoalsTemp", goalsTemp) });
                   
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the goals");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            model.Services = _combosHelper.GetComboServices();
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditGoalModalTemp", model) });
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> CreateObjectiveModalTemp(int? idGoal, int idClient, string nameGoal, int numberGoal, int number, DateTime  admissisonDate)
        {
            if (idGoal == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GoalsTempEntity goalTempEntity = await _context.GoalsTemp
                                                           .Include(g => g.ObjetiveTempList)
                                                           .FirstOrDefaultAsync(m => m.Id == idGoal);

            ClientEntity client = await _context.Clients
                                                .FirstOrDefaultAsync(m => m.Id == idClient);

            if (goalTempEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            string objetive = $"{goalTempEntity.Number}.{goalTempEntity.ObjetiveTempList.Count() + 1}";
            ObjectiveTempViewModel model = new ObjectiveTempViewModel
            {
                GoalTemp = goalTempEntity,
                IdGoal = goalTempEntity.Id,
                DateOpened = admissisonDate,
                DateResolved = admissisonDate.AddMonths(Convert.ToInt32(number)),
                DateTarget = admissisonDate.AddMonths(Convert.ToInt32(number)),
                Objetive = objetive,
                NameGoal = nameGoal,
                NumberGoal = numberGoal
            };

            MultiSelectList classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name");
            ViewData["classification"] = classification_list;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> CreateObjectiveModalTemp(ObjectiveTempViewModel model, IFormCollection form)
        {
            UserEntity user_logged = await _context.Users
                                                     .Include(u => u.Clinic)
                                                     .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                GoalsTempEntity goal = await _context.GoalsTemp
                                                     .Include(g => g.ObjetiveTempList)
                                                     .FirstOrDefaultAsync(m => m.Id == model.IdGoal);
                ClientEntity client = await _context.Clients
                                                    .FirstOrDefaultAsync(m => m.Id == goal.IdClient);

                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(model.Description))
                {
                    model.Description = (model.Description.Last() == '.') ? model.Description : $"{model.Description}.";
                    if (this.GenderEvaluation(client.Gender, model.Description))
                    {
                        gender_problems = "Description";
                    }
                }
                if (!string.IsNullOrEmpty(model.Intervention))
                {
                    model.Intervention = (model.Intervention.Last() == '.') ? model.Intervention : $"{model.Intervention}.";
                    if (this.GenderEvaluation(client.Gender, model.Intervention))
                    {
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? "Intervention" : $"{gender_problems}, Intervention";
                    }
                }
                if (!string.IsNullOrEmpty(gender_problems))     //el objective tiene problemas con el genero
                {
                    ModelState.AddModelError(string.Empty, $"Error.There are gender issues in: {gender_problems}");
                    ObjectiveTempViewModel newmodel = new ObjectiveTempViewModel
                    {
                        GoalTemp = goal,
                        IdGoal = goal.Id,
                        DateOpened = model.DateOpened,
                        DateResolved = model.DateResolved,
                        DateTarget = model.DateTarget,
                        Objetive = $"{goal.Number}.{goal.ObjetiveTempList.Count() + 1}",
                        Description = model.Description,
                        Intervention = model.Intervention
                    };
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateObjectiveModalTemp", newmodel) });
                    //return View(newmodel);
                }

                ObjectiveTempEntity objective = await _converterHelper.ToObjectiveTempEntity(model, true);
                _context.Add(objective);


                try
                {
                    await _context.SaveChangesAsync();

                    List<GoalsTempEntity> goalsTemp = await _context.GoalsTemp
                                                                    .Include(g => g.ObjetiveTempList)
                                                                    .Where(g => g.IdClient == goal.IdClient && g.UserName == user_logged.UserName)
                                                                    .ToListAsync();
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoalsTemp", goalsTemp) });
                  

                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the objective");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            GoalsTempEntity goalTempEntity = await _context.GoalsTemp
                                                           .FirstOrDefaultAsync(m => m.Id == model.IdGoal);
            model.GoalTemp = goalTempEntity;
            model.IdGoal = goalTempEntity.Id;
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateObjectiveModalTemp", model) });
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> DeleteGoalTemp(int? id, int origin = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            GoalsTempEntity goalEntity = await _context.GoalsTemp
                                                       .FirstOrDefaultAsync(g => g.Id == id);
            int idClient = goalEntity.IdClient;
            if (goalEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.GoalsTemp.Remove(goalEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Create", new { idClient = idClient, origin = origin });
            }

            return RedirectToAction("Create", new { idClient = idClient, origin = origin });
           
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> EditObjectiveModalTemp(int? id, string nameGoal, int numberGoal)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ObjectiveTempEntity objectiveEntity = await _context.ObjetivesTemp
                                                                .Include(o => o.GoalTemp)
                                                                .FirstOrDefaultAsync(d => d.Id == id);

            ObjectiveTempViewModel model = _converterHelper.ToObjectiveTempViewModel(objectiveEntity);
            if (model == null)
            {
                return RedirectToAction("Home/Error404");
            }
            model.NameGoal = nameGoal;
            model.NumberGoal = numberGoal;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> EditObjectiveModalTemp(ObjectiveTempViewModel model, IFormCollection form)
        {
            UserEntity user_logged = await _context.Users
                                                     .Include(u => u.Clinic)
                                                     .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            GoalsTempEntity goal = await _context.GoalsTemp
                                                 .FirstOrDefaultAsync(m => m.Id == model.IdGoal);
            ClientEntity client = await _context.Clients
                                                 .FirstOrDefaultAsync(m => m.Id == model.IdGoal);
            if (ModelState.IsValid)
            {
                string gender_problems = string.Empty;
                if (!string.IsNullOrEmpty(model.Description))
                {
                    model.Description = (model.Description.Last() == '.') ? model.Description : $"{model.Description}.";
                    if (this.GenderEvaluation(client.Gender, model.Description))
                    {
                        gender_problems = "Description";
                    }
                }
                if (!string.IsNullOrEmpty(model.Intervention))
                {
                    model.Intervention = (model.Intervention.Last() == '.') ? model.Intervention : $"{model.Intervention}.";
                    if (this.GenderEvaluation(client.Gender, model.Intervention))
                    {
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? "Intervention" : $"{gender_problems}, Intervention";
                    }
                }
                if (!string.IsNullOrEmpty(gender_problems))     //el objective tiene problemas con el genero
                {
                    ModelState.AddModelError(string.Empty, $"Error.There are gender issues in: {gender_problems}");
                    model.GoalTemp = goal;
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjectiveModalTemp", model) });
                   
                }

                ObjectiveTempEntity objectiveTemp = await _converterHelper.ToObjectiveTempEntity(model, false);
                _context.Update(objectiveTemp);

                try
                {
                    await _context.SaveChangesAsync();

                    List<GoalsTempEntity> goalsTemp = await _context.GoalsTemp
                                                                    .Include(g => g.ObjetiveTempList)
                                                                    .Where(g => g.IdClient == goal.IdClient && g.UserName == user_logged.UserName)
                                                                    .ToListAsync();
                    
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoalsTemp", goalsTemp) });
                    

                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the objective");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            model.GoalTemp = goal;
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditObjectiveModalTemp", model) });
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> DeleteObjectiveTemp(int? id, int origin = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ObjectiveTempEntity objectiveEntity = await _context.ObjetivesTemp
                                                                .Include(o => o.GoalTemp)
                                                                .FirstOrDefaultAsync(o => o.Id == id);
            GoalsTempEntity goalTemp = await _context.GoalsTemp
                                                     .FirstOrDefaultAsync(o => o.Id == objectiveEntity.GoalTemp.Id);

            if (objectiveEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.ObjetivesTemp.Remove(objectiveEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Create", new { idClient = goalTemp.IdClient, origin = origin });
            }
                
            return RedirectToAction("Create", new { idClient = goalTemp.IdClient, origin = origin });
        }

        public JsonResult Translate(string text)
        {
            return Json(text = _translateHelper.TranslateText("es", "en", text));
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ReturnTo(int? id, int clientId = 0, MTPStatus aStatus = MTPStatus.Edition)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MTPEntity mtpEntity = await _context.MTPs
                                                .Include(m => m.Client)
                                                .FirstOrDefaultAsync(t => t.Id == id);
            if (mtpEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }
 
            try
            {
                mtpEntity.Status = aStatus;
                _context.MTPs.Update(mtpEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("ClientHistory", "Clients", new { idClient = clientId });
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ReturnAddendumsTo(int? id, int clientId = 0, AdendumStatus aStatus = AdendumStatus.Edition)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            AdendumEntity adendumEntity = await _context.Adendums
                                                        .Include(m => m.Mtp)
                                                        .FirstOrDefaultAsync(t => t.Id == id);
            if (adendumEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                adendumEntity.Status = aStatus;
                _context.Adendums.Update(adendumEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("ClientHistory", "Clients", new { idClient = clientId });
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ReturnMtpReviewTo(int? id, int clientId = 0, AdendumStatus aStatus = AdendumStatus.Edition)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MTPReviewEntity mtprEntity = await _context.MTPReviews
                                                        .Include(m => m.Mtp)
                                                        .FirstOrDefaultAsync(t => t.Id == id);
            if (mtprEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                mtprEntity.Status = aStatus;
                _context.MTPReviews.Update(mtprEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("ClientHistory", "Clients", new { idClient = clientId });
        }

        [Authorize(Roles = "Manager, Supervisor, Facilitator")]
        public async Task<IActionResult> AuditMtp()
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic || !user_logged.Clinic.Setting.MHProblems)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<AuditMtps> auditMtp_List = new List<AuditMtps>();
            AuditMtps auditMtp = new AuditMtps();

            List<ObjetiveEntity> objective_List = _context.Objetives
                                                          .Include(m => m.Goal)
                                                          .ThenInclude(m => m.MTP)
                                                          .ThenInclude(m => m.Client)
                                                          .Include(m => m.IndividualNotes)
                                                          .Include(m => m.Note_Activity)
                                                          .Include(m => m.NoteP_Activity)
                                                          .Where(m => (m.Goal.MTP.Client.Status == StatusType.Close
                                                                && (m.Goal.Service == ServiceType.Individual
                                                                && m.IndividualNotes.Count() == 0))
                                                                ||((m.Goal.Service == ServiceType.PSR
                                                                && m.Note_Activity.Count() == 0
                                                                && m.NoteP_Activity.Count() == 0)))
                                                          .OrderBy(n => n.Goal.MTP.AdmissionDateMTP)
                                                          .ToList();

            foreach (var item in objective_List)
            {
                auditMtp.NameClient = item.Goal.MTP.Client.Name;
                auditMtp.AdmissionDate = item.Goal.MTP.AdmissionDateMTP.ToShortDateString();
                auditMtp.Description = "Has Objective " + item.Objetive + " that are not used";
                auditMtp.Active = 0;
                auditMtp_List.Add(auditMtp);
                auditMtp = new AuditMtps();
            }

            return View(auditMtp_List);
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult DeleteGoalTemp1(int id = 0)
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
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> DeleteGoalTemp1(DeleteViewModel GoalTempViewModel)
        {
            if (ModelState.IsValid)
            {
                GoalsTempEntity goalTemp = await _context.GoalsTemp.FirstAsync(n => n.Id == GoalTempViewModel.Id_Element);
                try
                {
                    _context.GoalsTemp.Remove(goalTemp);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoalsTemp", _context.GoalsTemp.Include(n => n.ObjetiveTempList).Where(d => d.IdClient == goalTemp.IdClient).ToList()) });
                }

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoalsTemp", _context.GoalsTemp.Include(n => n.ObjetiveTempList).Where(d => d.IdClient == goalTemp.IdClient).ToList()) });
            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoalsTemp", _context.GoalsTemp.Include(n => n.ObjetiveTempList).Where(d => d.IdClient == 0).ToList()) });
        }

        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public IActionResult DeleteObjectiveTemp1(int id = 0)
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
        [Authorize(Roles = "Supervisor, Documents_Assistant")]
        public async Task<IActionResult> DeleteObjectiveTemp1(DeleteViewModel ObjectiveTempViewModel)
        {
            if (ModelState.IsValid)
            {
                ObjectiveTempEntity objectiveTemp = await _context.ObjetivesTemp
                                                                  .Include(o => o.GoalTemp)
                                                                  .FirstAsync(n => n.Id == ObjectiveTempViewModel.Id_Element);
                try
                {
                    _context.ObjetivesTemp.Remove(objectiveTemp);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoalsTemp", _context.GoalsTemp.Include(g => g.ObjetiveTempList).Where(d => d.IdClient == objectiveTemp.GoalTemp.IdClient).ToList()) });
                }

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoalsTemp", _context.GoalsTemp.Include(g => g.ObjetiveTempList).Where(d => d.IdClient == objectiveTemp.GoalTemp.IdClient).ToList()) });
            }

            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewGoalsTemp", _context.GoalsTemp.Include(g => g.ObjetiveTempList).Where(d => d.IdClient == 0).ToList()) });
        }


    }
}