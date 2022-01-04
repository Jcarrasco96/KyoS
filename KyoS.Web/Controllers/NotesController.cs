using AspNetCore.Reporting;
using KyoS.Common.Enums;
using KyoS.Common.Helpers;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    public class NotesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly IImageHelper _imageHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IDateHelper _dateHelper;
        private readonly ITranslateHelper _translateHelper;
        private readonly IWebHostEnvironment _webhostEnvironment;
        private readonly IReportHelper _reportHelper;
        private readonly IRenderHelper _renderHelper;

        public NotesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IDateHelper dateHelper, ITranslateHelper translateHelper, IWebHostEnvironment webHostEnvironment, IImageHelper imageHelper, IReportHelper reportHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _dateHelper = dateHelper;
            _translateHelper = translateHelper;
            _webhostEnvironment = webHostEnvironment;
            _imageHelper = imageHelper;
            _reportHelper = reportHelper;
            _renderHelper = renderHelper;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }
        
        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> Index(int id = 0)
        {
            if (id == 1)
            {
                ViewBag.FinishEdition = "Y";
            }

            
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
            {
                return View(await _context.Weeks
                                          .Include(w => w.Clinic)
                                          .Include(w => w.Days)
                                          .ThenInclude(d => d.Workdays_Clients).ToListAsync());
            }

            return View(await _context.Weeks
                                      .Include(w => w.Days)                                            
                                      .ThenInclude(d => d.Workdays_Clients)                                            
                                      .ThenInclude(wc => wc.Client)                          

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Clients)
                                      .ThenInclude(g => g.Facilitator)

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Clients)
                                      .ThenInclude(wc => wc.Note)

                                      .Where(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                && w.Days.Where(d => (d.Service == ServiceType.PSR && d.Workdays_Clients.Where(wc => wc.Facilitator.LinkedUser == User.Identity.Name).Count() > 0)).Count() > 0))
                                      .ToListAsync());            
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> IndividualNotes(int id = 0)
        {
            if (id == 1)
            {
                ViewBag.FinishEdition = "Y";
            }


            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
            {
                return View(await _context.Weeks
                                          .Include(w => w.Clinic)
                                          .Include(w => w.Days)
                                          .ThenInclude(d => d.Workdays_Clients).ToListAsync());
            }

            return View(await _context.Weeks
                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Clients)
                                      .ThenInclude(wc => wc.Client)
                                      
                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Clients)
                                      .ThenInclude(g => g.Facilitator)

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Clients)
                                      .ThenInclude(wc => wc.IndividualNote)

                                      .Where(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                && w.Days.Where(d => (d.Service == ServiceType.Individual && d.Workdays_Clients.Where(wc => wc.Facilitator.LinkedUser == User.Identity.Name).Count() > 0)).Count() > 0))
                                      .ToListAsync());
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> GroupNotes(int id = 0)
        {
            if (id == 1)
            {
                ViewBag.FinishEdition = "Y";
            }


            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
            {
                return View(await _context.Weeks
                                          .Include(w => w.Clinic)
                                          .Include(w => w.Days)
                                          .ThenInclude(d => d.Workdays_Clients).ToListAsync());
            }

            return View(await _context.Weeks
                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Clients)
                                      .ThenInclude(wc => wc.Client)                                      

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Clients)
                                      .ThenInclude(g => g.Facilitator)

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Clients)
                                      .ThenInclude(wc => wc.GroupNote)

                                      .Where(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                && w.Days.Where(d => (d.Service == ServiceType.Group && d.Workdays_Clients.Where(wc => wc.Facilitator.LinkedUser == User.Identity.Name).Count() > 0)).Count() > 0))                                               
                                            
                                      .ToListAsync());
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> Present(int id, int origin = 0)
        {
            Workday_Client workdayClient = await _context.Workdays_Clients
                                                         .Include(wc => wc.Workday)
                                                         .Include(wc => wc.Client)
                                                         .ThenInclude(c => c.Group)

                                                         .Include(g => g.Facilitator)
                                                         .FirstOrDefaultAsync(wc => wc.Id == id);
            Workday_ClientViewModel model = _converterHelper.ToWorkdayClientViewModel(workdayClient);
            model.Origin = origin;
            model.CauseOfNotPresent = (string.IsNullOrEmpty(workdayClient.CauseOfNotPresent)) ?
                                            "The client was absent to PSR session today, because of a personal matter." : workdayClient.CauseOfNotPresent;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> Present(Workday_ClientViewModel model, IFormCollection form)
        {
            Workday_Client entity = await _context.Workdays_Clients
                                                  .Include(wc => wc.Workday)
                                                  .Include(wc => wc.Client)
                                                  .ThenInclude(c => c.Group)
                                                  .Include(wc => wc.Facilitator)
                                                  .FirstOrDefaultAsync(wc => wc.Id == model.Id);

            if (entity == null)
            {
                return NotFound();
            }

            switch (form["Present"])
            {
                case "present":
                    {
                        entity.Present = true;
                        entity.CauseOfNotPresent = string.Empty;
                        break;
                    }
                case "nopresent":
                    {
                        entity.Present = false;
                        entity.CauseOfNotPresent = model.CauseOfNotPresent;
                        break;
                    }
                default:
                    break;
            }

            _context.Update(entity);
            try
            {
                await _context.SaveChangesAsync();
                if (model.Origin == 0)
                    return RedirectToAction(nameof(Index));
                if (model.Origin == 1)
                    return RedirectToAction(nameof(NotStartedNotes));
                if (model.Origin == 2)
                    return RedirectToAction(nameof(NotesInEdit));
                if (model.Origin == 3)
                    return RedirectToAction(nameof(NotStartedIndNotes));
                if (model.Origin == 4)
                    return RedirectToAction(nameof(IndividualNotes));
                if (model.Origin == 5)
                    return RedirectToAction(nameof(IndNotesInEdit));
                if (model.Origin == 6)
                    return RedirectToAction(nameof(GroupNotes));
                if (model.Origin == 7)
                    return RedirectToAction(nameof(NotStartedGroupNotes));
                if (model.Origin == 8)
                    return RedirectToAction(nameof(GroupNotesInEdit));
            }
            catch (System.Exception ex)
            {
                if (ex.InnerException.Message.Contains("duplicate"))
                {
                    ModelState.AddModelError(string.Empty, $"Already exists the element");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            return View(_converterHelper.ToWorkdayClientViewModel(entity));
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> EditNote(int id, int error = 0, int origin = 0, string errorText = "")
        {
            NoteViewModel noteViewModel;
            List<ThemeEntity> topics;
            List<SelectListItem> list1 = new List<SelectListItem>();
            List<SelectListItem> list2 = new List<SelectListItem>();
            List<SelectListItem> list3 = new List<SelectListItem>();
            List<SelectListItem> list4 = new List<SelectListItem>();
            
            //la nota no tiene linkeado ningun goal
            if (error == 1)  
                ViewBag.Error = "0";

            //la nota no esta completa, faltan campos por editar
            if (error == 2)
                ViewBag.Error = "2";

            //la nota tiene problemas con el genero
            if (error == 4)
            {
                ViewBag.Error = "4";
                ViewBag.errorText = errorText;
            }

            //el cliente seleccionado tiene una nota ya creada de otro servicio en ese mismo horario
            if (error == 5)
            {
                ViewBag.Error = "5";
            }

            Workday_Client workday_Client = await _context.Workdays_Clients.Include(wc => wc.Workday)
                                                                           .ThenInclude(w => w.Workdays_Activities_Facilitators)
                                                                           .ThenInclude(waf => waf.Activity)
                                                                           .ThenInclude(a => a.Theme)

                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Clinic)

                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Group)

                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.MTPs)

                                                                           .Include(wc => wc.Facilitator)
                                                                           .FirstOrDefaultAsync(wc => wc.Id == id);

            if (workday_Client == null)
            {
                return NotFound();
            }

            FacilitatorEntity facilitator_logged = _context.Facilitators
                                                           .FirstOrDefault(f => f.LinkedUser == User.Identity.Name);

            //el dia no tiene actividad asociada para el facilitator logueado por lo tanto no se puede crear la nota
            if (workday_Client.Workday.Workdays_Activities_Facilitators.Where(waf => waf.Facilitator == facilitator_logged).Count() == 0)
            {
                ViewBag.Error = "1";
                noteViewModel = new NoteViewModel
                {
                    Id = workday_Client.Workday.Id,
                };
                return View(noteViewModel);
            }

            //el cliente no tiene mtp activos
            if (workday_Client.Client.MTPs.Where(m => m.Active == true).Count() == 0)
            {
                ViewBag.Error = "3";
                noteViewModel = new NoteViewModel
                {
                    Id = workday_Client.Workday.Id,
                };
                return View(noteViewModel);
            }

            NoteEntity note = await _context.Notes.Include(n => n.Workday_Cient)
                                                  .ThenInclude(wc => wc.Client)
                                                  .ThenInclude(c => c.Group)

                                                  .Include(n => n.Workday_Cient)
                                                  .ThenInclude(g => g.Facilitator)

                                                  .Include(n => n.Notes_Activities)
                                                  .ThenInclude(na => na.Activity)
                                                  .FirstOrDefaultAsync(n => n.Workday_Cient.Id == id);

            topics = await _context.Themes.Where(t => t.Clinic.Id == workday_Client.Client.Clinic.Id)
                                          .ToListAsync();
            topics = topics.Where(t => t.Day.ToString() == workday_Client.Workday.Day)
                                            .ToList();

            int index = 0;
            foreach (ThemeEntity value in topics)
            {
                if (index == 0)
                {
                    list1.Insert(index, new SelectListItem
                    {
                        Text = value.Name,
                        Value = $"{value.Id}"
                    });
                    index = ++index;
                    continue;
                }
                if (index == 1)
                {
                    list2.Insert(0, new SelectListItem
                    {
                        Text = value.Name,
                        Value = $"{value.Id}"
                    });
                    index = ++index;
                    continue;
                }
                if (index == 2)
                {
                    list3.Insert(0, new SelectListItem
                    {
                        Text = value.Name,
                        Value = $"{value.Id}"
                    });
                    index = ++index;
                    continue;
                }
                if (index == 3)
                {
                    list4.Insert(0, new SelectListItem
                    {
                        Text = value.Name,
                        Value = $"{value.Id}"
                    });
                    index = ++index;
                    continue;
                }
            }

            //-----------se selecciona el primer MTP activo que tenga el cliente-----------//
            MTPEntity mtp = _context.MTPs.FirstOrDefault(m => (m.Client.Id == workday_Client.Client.Id && m.Active == true));

            List<Workday_Activity_Facilitator> activities = workday_Client.Workday
                                                                          .Workdays_Activities_Facilitators
                                                                          .Where(waf => waf.Facilitator == facilitator_logged)
                                                                          .ToList();

            if (note == null)   //la nota no está creada
            {
                IEnumerable<SelectListItem> goals = null;
                IEnumerable<SelectListItem> objs = null;
                if (mtp != null)
                {
                    goals = _combosHelper.GetComboGoals(mtp.Id);
                    objs = _combosHelper.GetComboObjetives(0);
                }
                else
                {
                    goals = _combosHelper.GetComboGoals(0);
                    objs = _combosHelper.GetComboObjetives(0);
                }                

                noteViewModel = new NoteViewModel
                {
                    Id = id,
                    Status = NoteStatus.Pending,    //es solo generico para la visualizacion del btn FinishEditing
                    Origin = origin,
                    Schema = workday_Client.Client.Clinic.Schema,
                    //IdTopic1 = (activities.Count > 0) ? activities[0].Activity.Theme.Id : 0,
                    Topic1 = (activities.Count > 0) ? activities[0].Activity.Theme.Name : string.Empty,
                    IdActivity1 = (activities.Count > 0) ? activities[0].Activity.Id : 0,
                    Activity1 = (activities.Count > 0) ? activities[0].Activity.Name : string.Empty,
                    Goals1 = goals,
                    Objetives1 = objs,

                    //IdTopic2 = (activities.Count > 1) ? activities[1].Activity.Theme.Id : 0,
                    Topic2 = (activities.Count > 1) ? activities[1].Activity.Theme.Name : string.Empty,
                    IdActivity2 = (activities.Count > 1) ? activities[1].Activity.Id : 0,
                    Activity2 = (activities.Count > 1) ? activities[1].Activity.Name : string.Empty,
                    Goals2 = goals,
                    Objetives2 = objs,

                    //IdTopic3 = (activities.Count > 2) ? activities[2].Activity.Theme.Id : 0,
                    Topic3 = (activities.Count > 2) ? activities[2].Activity.Theme.Name : string.Empty,
                    IdActivity3 = (activities.Count > 2) ? activities[2].Activity.Id : 0,
                    Activity3 = (activities.Count > 2) ? activities[2].Activity.Name : string.Empty,
                    Goals3 = goals,
                    Objetives3 = objs,

                    //IdTopic4 = (activities.Count > 3) ? activities[3].Activity.Theme.Id : 0,
                    Topic4 = (activities.Count > 3) ? activities[3].Activity.Theme.Name : string.Empty,
                    IdActivity4 = (activities.Count > 3) ? activities[3].Activity.Id : 0,
                    Activity4 = (activities.Count > 3) ? activities[3].Activity.Name : string.Empty,
                    Goals4 = goals,
                    Objetives4 = objs,

                    Workday_Cient = workday_Client
                };
            }
            else
            {
                List<Note_Activity> note_Activity = await _context.Notes_Activities

                                                                  .Include(na => na.Activity)
                                                                  .ThenInclude(a => a.Theme)

                                                                  .Include(na => na.Objetive)
                                                                  .ThenInclude(o => o.Goal)

                                                                  .Where(na => na.Note.Id == note.Id)
                                                                  .ToListAsync();

                IEnumerable<SelectListItem> goals = null;
                IEnumerable<SelectListItem> objs = null;
                if (mtp != null)
                {
                    goals = _combosHelper.GetComboGoals(mtp.Id);
                    objs = _combosHelper.GetComboObjetives(0);
                }
                else
                {
                    goals = _combosHelper.GetComboGoals(0);
                    objs = _combosHelper.GetComboObjetives(0);
                }

                noteViewModel = new NoteViewModel
                {
                    Id = id,
                    Origin = origin,
                    Workday_Cient = workday_Client,
                    Schema = note.Schema,
                    PlanNote = note.PlanNote,
                    Status = note.Status,

                    OrientedX3 = note.OrientedX3,
                    NotTime = note.NotTime,
                    NotPlace = note.NotPlace,
                    NotPerson = note.NotPerson,
                    Present = note.Present,
                    Adequate = note.Adequate,
                    Limited = note.Limited,
                    Impaired = note.Impaired,
                    Faulty = note.Faulty,
                    Euthymic = note.Euthymic,
                    Congruent = note.Congruent,
                    Negativistic = note.Negativistic,
                    Depressed = note.Depressed,
                    Euphoric = note.Euphoric,
                    Optimistic = note.Optimistic,
                    Anxious = note.Anxious,
                    Hostile = note.Hostile,
                    Withdrawn = note.Withdrawn,
                    Irritable = note.Irritable,
                    Dramatized = note.Dramatized,
                    AdequateAC = note.AdequateAC,
                    Inadequate = note.Inadequate,
                    Fair = note.Fair,
                    Unmotivated = note.Unmotivated,
                    Motivated = note.Motivated,
                    Guarded = note.Guarded,
                    Normal = note.Normal,
                    ShortSpanned = note.ShortSpanned,
                    MildlyImpaired = note.MildlyImpaired,
                    SeverelyImpaired = note.SeverelyImpaired,

                    //IdTopic1 = (activities.Count > 0) ? activities[0].Activity.Theme.Id : 0,
                    Topic1 = (activities.Count > 0) ? activities[0].Activity.Theme.Name : string.Empty,
                    IdActivity1 = (activities.Count > 0) ? activities[0].Activity.Id : 0,
                    Activity1 = (activities.Count > 0) ? activities[0].Activity.Name : string.Empty,
                    AnswerClient1 = note_Activity[0].AnswerClient,
                    AnswerFacilitator1 = note_Activity[0].AnswerFacilitator,
                    IdGoal1 = ((note_Activity.Count > 0) && (note_Activity[0].Objetive != null)) ? note_Activity[0].Objetive.Goal.Id : 0,
                    Goals1 = goals,
                    IdObjetive1 = ((note_Activity.Count > 0) && (note_Activity[0].Objetive != null)) ? note_Activity[0].Objetive.Id : 0,
                    //Paso el IdGoal1 como parametro
                    Objetives1 = _combosHelper.GetComboObjetives(((note_Activity.Count > 0) && (note_Activity[0].Objetive != null)) 
                                                                        ? note_Activity[0].Objetive.Goal.Id : 0),
                    Intervention1 = ((note_Activity.Count > 0) && (note_Activity[0].Objetive != null)) ? note_Activity[0].Objetive.Intervention : string.Empty,

                    //IdTopic2 = (activities.Count > 1) ? activities[1].Activity.Theme.Id : 0,
                    Topic2 = (activities.Count > 1) ? activities[1].Activity.Theme.Name : string.Empty,
                    IdActivity2 = (activities.Count > 1) ? activities[1].Activity.Id : 0,
                    Activity2 = (activities.Count > 1) ? activities[1].Activity.Name : string.Empty,
                    AnswerClient2 = note_Activity[1].AnswerClient,
                    AnswerFacilitator2 = note_Activity[1].AnswerFacilitator,
                    IdGoal2 = ((note_Activity.Count > 1) && (note_Activity[1].Objetive != null)) ? note_Activity[1].Objetive.Goal.Id : 0,
                    Goals2 = goals,
                    IdObjetive2 = ((note_Activity.Count > 1) && (note_Activity[1].Objetive != null)) ? note_Activity[1].Objetive.Id : 0,
                    //Paso el IdGoal2 como parametro
                    Objetives2 = _combosHelper.GetComboObjetives(((note_Activity.Count > 1) && (note_Activity[1].Objetive != null))
                                                                        ? note_Activity[1].Objetive.Goal.Id : 0),
                    Intervention2 = ((note_Activity.Count > 1) && (note_Activity[1].Objetive != null)) ? note_Activity[1].Objetive.Intervention : string.Empty,

                    //IdTopic3 = (activities.Count > 2) ? activities[2].Activity.Theme.Id : 0,
                    Topic3 = (activities.Count > 2) ? activities[2].Activity.Theme.Name : string.Empty,
                    IdActivity3 = (activities.Count > 2) ? activities[2].Activity.Id : 0,
                    Activity3 = (activities.Count > 2) ? activities[2].Activity.Name : string.Empty,
                    AnswerClient3 = note_Activity[2].AnswerClient,
                    AnswerFacilitator3 = note_Activity[2].AnswerFacilitator,
                    IdGoal3 = ((note_Activity.Count > 2) && (note_Activity[2].Objetive != null)) ? note_Activity[2].Objetive.Goal.Id : 0,
                    Goals3 = goals,
                    IdObjetive3 = ((note_Activity.Count > 2) && (note_Activity[2].Objetive != null)) ? note_Activity[2].Objetive.Id : 0,
                    //Paso el IdGoal3 como parametro
                    Objetives3 = _combosHelper.GetComboObjetives(((note_Activity.Count > 2) && (note_Activity[2].Objetive != null))
                                                                        ? note_Activity[2].Objetive.Goal.Id : 0),
                    Intervention3 = ((note_Activity.Count > 2) && (note_Activity[2].Objetive != null)) ? note_Activity[2].Objetive.Intervention : string.Empty,

                    //IdTopic4 = (activities.Count > 3) ? activities[3].Activity.Theme.Id : 0,
                    Topic4 = (activities.Count > 3) ? activities[3].Activity.Theme.Name : string.Empty,
                    IdActivity4 = (activities.Count > 3) ? activities[3].Activity.Id : 0,
                    Activity4 = (activities.Count > 3) ? activities[3].Activity.Name : string.Empty,
                    AnswerClient4 = note_Activity[3].AnswerClient,
                    AnswerFacilitator4 = note_Activity[3].AnswerFacilitator,
                    IdGoal4 = ((note_Activity.Count > 3) && (note_Activity[3].Objetive != null)) ? note_Activity[3].Objetive.Goal.Id : 0,
                    Goals4 = goals,
                    IdObjetive4 = ((note_Activity.Count > 3) && (note_Activity[3].Objetive != null)) ? note_Activity[3].Objetive.Id : 0,
                    //Paso el IdGoal4 como parametro
                    Objetives4 = _combosHelper.GetComboObjetives(((note_Activity.Count > 3) && (note_Activity[3].Objetive != null))
                                                                        ? note_Activity[3].Objetive.Goal.Id : 0),
                    Intervention4 = ((note_Activity.Count > 3) && (note_Activity[3].Objetive != null)) ? note_Activity[3].Objetive.Intervention : string.Empty,
                };
            }
            return View(noteViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> EditNote(NoteViewModel model, IFormCollection form)
        {
            Workday_Client workday_Client = await _context.Workdays_Clients.Include(wc => wc.Workday)

                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Clinic)

                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Group)

                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.MTPs)

                                                                           .Include(wc => wc.Facilitator)
                                                                           
                                                                           .FirstOrDefaultAsync(wc => wc.Id == model.Id);
            if (workday_Client == null)
            {
                return NotFound();
            }

            NoteEntity noteEntity;
            if (ModelState.IsValid)
            {
                NoteEntity note = await _context.Notes.Include(n => n.Workday_Cient)
                                                      .ThenInclude(wc => wc.Messages)
                                                      .FirstOrDefaultAsync(n => n.Workday_Cient.Id == model.Id);
                Note_Activity note_Activity;
                string progress;
                if (note == null)   //la nota no está creada
                {
                    //Verify the client is not present in other services of notes at the same time
                    if (this.VerifyNotesAtSameTime(workday_Client.Client.Id, workday_Client.Session, workday_Client.Workday.Date))
                    {
                        return RedirectToAction(nameof(EditNote), new { id = model.Id, error = 5, origin = model.Origin });
                    }

                    //actualizo el progress seleccionado en el plan
                    progress = (form["Progress"] == "SignificantProgress") ? "Significant progress " :
                                (form["Progress"] == "ModerateProgress") ? "Moderate progress " :
                                 (form["Progress"] == "MinimalProgress") ? "Minimal progress " :
                                  (form["Progress"] == "NoProgress") ? "No progress " :
                                   (form["Progress"] == "Regression") ? "Regression " :
                                    (form["Progress"] == "Decompensating") ? "Decompensating " :
                                     (form["Progress"] == "Unable") ? "Unable "
                                      : string.Empty;
                    progress = $"{progress}{RandomGenerator()}";
                    model.PlanNote = (model.PlanNote.Trim().Last() == '.') ? $"{progress}{model.PlanNote.Trim()}" : $"{progress}{model.PlanNote.Trim()}.";

                    noteEntity = await _converterHelper.ToNoteEntity(model, true);      
                    //Update plan progress
                    noteEntity.SignificantProgress = (form["Progress"] == "SignificantProgress") ? true : false;
                    noteEntity.ModerateProgress = (form["Progress"] == "ModerateProgress") ? true : false;
                    noteEntity.MinimalProgress = (form["Progress"] == "MinimalProgress") ? true : false;
                    noteEntity.NoProgress = (form["Progress"] == "NoProgress") ? true : false;
                    noteEntity.Regression = (form["Progress"] == "Regression") ? true : false;
                    noteEntity.Decompensating = (form["Progress"] == "Decompensating") ? true : false;
                    noteEntity.UnableToDetermine = (form["Progress"] == "Unable") ? true : false;
                    noteEntity.Setting = workday_Client.Client.MTPs.FirstOrDefault().Setting;

                    //vinculo el mtp activo del cliente a la nota que se creará
                    Workday_Client workday_client = await _context.Workdays_Clients
                                                                  .Include(wd => wd.Client)
                                                                  .FirstOrDefaultAsync(wd => wd.Id == model.Id);
                    MTPEntity mtp = await _context.MTPs.FirstOrDefaultAsync(m => (m.Client.Id == workday_client.Client.Id && m.Active == true));
                    if(mtp != null)
                        noteEntity.MTPId = mtp.Id;

                    _context.Add(noteEntity);
                    note_Activity = new Note_Activity
                    {
                        Note = noteEntity,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity1),
                        AnswerClient = model.AnswerClient1.Trim(),
                        AnswerFacilitator = (model.AnswerFacilitator1.Trim().Last() == '.') ? model.AnswerFacilitator1.Trim() : $"{model.AnswerFacilitator1.Trim()}.",
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive1),
                    };
                    _context.Add(note_Activity);
                    note_Activity = new Note_Activity
                    {
                        Note = noteEntity,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity2),
                        AnswerClient = (model.AnswerClient2 != null) ? model.AnswerClient2.Trim() : string.Empty,
                        AnswerFacilitator = (model.AnswerFacilitator2 != null) ? ((model.AnswerFacilitator2.Trim().Last() == '.') ? model.AnswerFacilitator2.Trim() : $"{model.AnswerFacilitator2.Trim()}.") : string.Empty,
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive2),
                    };
                    _context.Add(note_Activity);
                    note_Activity = new Note_Activity
                    {
                        Note = noteEntity,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity3),
                        AnswerClient = (model.AnswerClient3 != null) ? model.AnswerClient3.Trim() : string.Empty,
                        AnswerFacilitator = (model.AnswerFacilitator3 != null) ? ((model.AnswerFacilitator3.Trim().Last() == '.') ? model.AnswerFacilitator3.Trim() : $"{model.AnswerFacilitator3.Trim()}.") : string.Empty,
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive3),
                    };
                    _context.Add(note_Activity);
                    note_Activity = new Note_Activity
                    {
                        Note = noteEntity,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity4),
                        AnswerClient = (model.AnswerClient4 != null) ? model.AnswerClient4.Trim() : string.Empty,
                        AnswerFacilitator = (model.AnswerFacilitator4 != null) ? ((model.AnswerFacilitator4.Trim().Last() == '.') ? model.AnswerFacilitator4.Trim() : $"{model.AnswerFacilitator4.Trim()}.") : string.Empty,
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive4),
                    };
                    _context.Add(note_Activity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        if(model.Origin == 0)
                            return RedirectToAction(nameof(Index));
                        if (model.Origin == 1)
                            return RedirectToAction(nameof(NotStartedNotes));
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, "Already exists the element");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else    //la nota está creada y sólo se debe actualizar
                {
                    note.PlanNote = (model.PlanNote.Trim().Last() == '.') ? model.PlanNote.Trim() : $"{model.PlanNote.Trim()}.";
                    note.OrientedX3 = model.OrientedX3;
                    note.NotTime = model.NotTime;
                    note.NotPlace = model.NotPlace;
                    note.NotPerson = model.NotPerson;
                    note.Present = model.Present;
                    note.Adequate = model.Adequate;
                    note.Limited = model.Limited;
                    note.Impaired = model.Impaired;
                    note.Faulty = model.Faulty;
                    note.Euthymic = model.Euthymic;
                    note.Congruent = model.Congruent;
                    note.Negativistic = model.Negativistic;
                    note.Depressed = model.Depressed;
                    note.Euphoric = model.Euphoric;
                    note.Optimistic = model.Optimistic;
                    note.Anxious = model.Anxious;
                    note.Hostile = model.Hostile;
                    note.Withdrawn = model.Withdrawn;
                    note.Irritable = model.Irritable;
                    note.Dramatized = model.Dramatized;
                    note.AdequateAC = model.AdequateAC;
                    note.Inadequate = model.Inadequate;
                    note.Fair = model.Fair;
                    note.Unmotivated = model.Unmotivated;
                    note.Motivated = model.Motivated;
                    note.Guarded = model.Guarded;
                    note.Normal = model.Normal;
                    note.ShortSpanned = model.ShortSpanned;
                    note.MildlyImpaired = model.MildlyImpaired;
                    note.SeverelyImpaired = model.SeverelyImpaired;

                    //actualizo el mtp activo del cliente a la nota que se creará
                    Workday_Client workday_client = await _context.Workdays_Clients
                                                                  .Include(wd => wd.Client)
                                                                  .FirstOrDefaultAsync(wd => wd.Id == model.Id);
                    MTPEntity mtp = await _context.MTPs.FirstOrDefaultAsync(m => (m.Client.Id == workday_client.Client.Id && m.Active == true));
                    if (mtp != null)
                        note.MTPId = mtp.Id;

                    _context.Update(note);
                    List<Note_Activity> noteActivities_list = await _context.Notes_Activities
                                                                            .Where(na => na.Note.Id == note.Id)
                                                                            .ToListAsync();
                    foreach (Note_Activity item in noteActivities_list)
                    {
                        _context.Remove(item);
                    }

                    note_Activity = new Note_Activity
                    {
                        Note = note,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity1),
                        AnswerClient = model.AnswerClient1.Trim(),
                        AnswerFacilitator = (model.AnswerFacilitator1.Trim().Last() == '.') ? model.AnswerFacilitator1.Trim() : $"{model.AnswerFacilitator1.Trim()}.",
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive1),
                    };
                    _context.Add(note_Activity);                    
                    await _context.SaveChangesAsync();

                    note_Activity = new Note_Activity
                    {
                        Note = note,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity2),
                        AnswerClient = (model.AnswerClient2 != null) ? model.AnswerClient2.Trim() : string.Empty,
                        AnswerFacilitator = (model.AnswerFacilitator2 != null) ? ((model.AnswerFacilitator2.Trim().Last() == '.') ? model.AnswerFacilitator2.Trim() : $"{model.AnswerFacilitator2.Trim()}.") : string.Empty,
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive2),
                    };
                    _context.Add(note_Activity);
                    await _context.SaveChangesAsync();

                    note_Activity = new Note_Activity
                    {
                        Note = note,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity3),
                        AnswerClient = (model.AnswerClient3 != null) ? model.AnswerClient3.Trim() : string.Empty,
                        AnswerFacilitator = (model.AnswerFacilitator3 != null) ? ((model.AnswerFacilitator3.Trim().Last() == '.') ? model.AnswerFacilitator3.Trim() : $"{model.AnswerFacilitator3.Trim()}.") : string.Empty,
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive3),
                    };
                    _context.Add(note_Activity);
                    await _context.SaveChangesAsync();

                    note_Activity = new Note_Activity
                    {
                        Note = note,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity4),
                        AnswerClient = (model.AnswerClient4 != null) ? model.AnswerClient4.Trim() : string.Empty,
                        AnswerFacilitator = (model.AnswerFacilitator4 != null) ? ((model.AnswerFacilitator4.Trim().Last() == '.') ? model.AnswerFacilitator4.Trim() : $"{model.AnswerFacilitator4.Trim()}.") : string.Empty,
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive4),
                    };
                    _context.Add(note_Activity);                                     

                    //todos los mensajes que tiene el Workday_Client de la nota los pongo como leidos
                    foreach (MessageEntity value in note.Workday_Cient.Messages)
                    {
                        value.Status = MessageStatus.Read;
                        value.DateRead = DateTime.Now;
                        _context.Update(value);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        if (model.Origin == 0)
                            return RedirectToAction(nameof(Index));
                        if (model.Origin == 1)
                            return RedirectToAction(nameof(NotStartedNotes));
                        if (model.Origin == 2)
                            return RedirectToAction(nameof(NotesInEdit));
                        if (model.Origin == 3)
                            return RedirectToAction(nameof(PendingNotes));
                        if (model.Origin == 4)
                            return RedirectToAction(nameof(NotesWithReview));
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, "Already exists the element");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> EditIndNote(int id, int error = 0, int origin = 0, string errorText = "")
        {
            IndividualNoteViewModel individualNoteViewModel;
            
            //la nota no tiene linkeado ningun goal
            if (error == 1)
                ViewBag.Error = "0";

            //la nota no esta completa, faltan campos por editar
            if (error == 2)
                ViewBag.Error = "2";

            //la nota tiene problemas con el genero
            if (error == 4)
            {
                ViewBag.Error = "4";
                ViewBag.errorText = errorText;
            }

            //el cliente seleccionado tiene una nota ya creada en ese mismo horario
            if (error == 5)
            {
                ViewBag.Error = "5";                
            }

            Workday_Client workday_Client = await _context.Workdays_Clients.Include(wc => wc.Workday)
                                                                           
                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Clinic)

                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.MTPs)

                                                                           .Include(wc => wc.Facilitator)

                                                                           .Include(wc => wc.Workday)
                                                                           .ThenInclude(w => w.Week)

                                                                           .FirstOrDefaultAsync(wc => wc.Id == id);

            if (workday_Client == null)
            {
                return NotFound();
            }

            FacilitatorEntity facilitator_logged = _context.Facilitators
                                                           .FirstOrDefault(f => f.LinkedUser == User.Identity.Name);

            //el cliente no tiene mtp activos
            if(workday_Client.Client != null)
            { 
                if (workday_Client.Client.MTPs.Where(m => m.Active == true).Count() == 0)
                {
                    ViewBag.Error = "3";
                    individualNoteViewModel = new IndividualNoteViewModel
                    {
                        Id = workday_Client.Workday.Id,
                    };
                    return View(individualNoteViewModel);
                }
            }

            IndividualNoteEntity note = await _context.IndividualNotes

                                                      .Include(n => n.Workday_Cient)
                                                      .ThenInclude(wc => wc.Client)
                                                      
                                                      .Include(n => n.Workday_Cient)
                                                      .ThenInclude(g => g.Facilitator)

                                                      .Include(n => n.Objective)
                                                      .ThenInclude(o => o.Goal)

                                                      .FirstOrDefaultAsync(n => n.Workday_Cient.Id == id);

            //-----------se selecciona el primer MTP activo que tenga el cliente-----------//
            MTPEntity mtp = null;
            if (workday_Client.Client != null)
            {
                mtp = _context.MTPs
                              .FirstOrDefault(m => (m.Client.Id == workday_Client.Client.Id && m.Active == true));
            }            

            IEnumerable<SelectListItem> goals = null;
            IEnumerable<SelectListItem> objs = null;
            if (mtp != null)
            {
                goals = _combosHelper.GetComboGoals(mtp.Id);
                objs = _combosHelper.GetComboObjetives(0);
            }
            else
            {
                goals = _combosHelper.GetComboGoals(0);
                objs = _combosHelper.GetComboObjetives(0);
            }

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);            

            if (note == null)   //la nota no está creada
            {
                individualNoteViewModel = new IndividualNoteViewModel
                {
                    Id = id,
                    Status = NoteStatus.Pending,    //es solo generico para la visualizacion del btn FinishEditing
                    Origin = origin,
                    Goals1 = goals,
                    Objetives1 = objs,
                    Workday_Cient = workday_Client,
                    Clients = _combosHelper.GetComboClientsForIndNotes(user_logged.Clinic.Id, workday_Client.Workday.Week.Id),
                    IdClient = 0
                };
            }
            else
            {
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = note.Workday_Cient.Client.Name,
                    Value = $"{note.Workday_Cient.Client.Id}"
                });

                individualNoteViewModel = new IndividualNoteViewModel
                {
                    Id = id,
                    Workday_Cient = workday_Client,
                    IdClient = note.Workday_Cient.Client.Id,
                    Clients   = list,
                    Origin = origin,
                    SubjectiveData = note.SubjectiveData,
                    ObjectiveData = note.ObjectiveData,
                    Assessment = note.Assessment,
                    PlanNote = note.PlanNote,
                    Status = note.Status,

                    Groomed = note.Groomed,
                    Unkempt = note.Unkempt,
                    Disheveled = note.Disheveled,
                    Meticulous = note.Meticulous,
                    Overbuild = note.Overbuild,
                    Other = note.Other,
                    Clear = note.Clear,
                    Pressured = note.Pressured,
                    Slurred = note.Slurred,
                    Slow = note.Slow,
                    Impaired = note.Impaired,
                    Poverty = note.Poverty,
                    Euthymic = note.Euthymic,
                    Depressed = note.Depressed,
                    Anxious = note.Anxious,
                    Fearful = note.Fearful,
                    Irritable = note.Irritable,
                    Labile = note.Labile,
                    WNL = note.WNL,
                    Guarded = note.Guarded,
                    Withdrawn = note.Withdrawn,
                    Hostile = note.Hostile,
                    Restless = note.Restless,
                    Impulsive = note.Impulsive,
                    WNL_Cognition = note.WNL_Cognition,
                    Blocked = note.Blocked,
                    Obsessive = note.Obsessive,
                    Paranoid = note.Paranoid,
                    Scattered = note.Scattered,
                    Psychotic = note.Psychotic,
                    Exceptional = note.Exceptional,
                    Steady = note.Steady,
                    Slow_Progress = note.Slow_Progress,
                    Regressing = note.Regressing,
                    Stable = note.Stable,
                    Maintain = note.Maintain,
                    CBT = note.CBT,
                    Psychodynamic = note.Psychodynamic,
                    BehaviorModification = note.BehaviorModification,
                    Other_Intervention = note.Other_Intervention,             

                    IdGoal1 = (note.Objective == null) ? 0 : note.Objective.Goal.Id,
                    Goals1 = goals,
                    IdObjetive1 = (note.Objective == null) ? 0 : note.Objective.Id,
                    //Paso el IdGoal1 como parametro
                    Objetives1 = _combosHelper.GetComboObjetives((note.Objective == null) ? 0 : note.Objective.Goal.Id),
                    Intervention1 = (note.Objective == null) ? string.Empty : note.Objective.Intervention
                };
            }

            return View(individualNoteViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> EditIndNote(IndividualNoteViewModel model, IFormCollection form)
        {
            Workday_Client workday_Client = await _context.Workdays_Clients

                                                          .Include(wc => wc.Workday)

                                                          .Include(wc => wc.Client)
                                                          .ThenInclude(c => c.Clinic)                                                                          

                                                          .Include(wc => wc.Client)
                                                          .ThenInclude(c => c.MTPs)

                                                          .Include(wc => wc.Facilitator)

                                                          .FirstOrDefaultAsync(wc => wc.Id == model.Id);
            if (workday_Client == null)
            {
                return NotFound();
            }

            IndividualNoteEntity individualNoteEntity;
            if (ModelState.IsValid)
            {
                IndividualNoteEntity note = await _context.IndividualNotes

                                                          .Include(n => n.Workday_Cient)
                                                          .ThenInclude(wc => wc.Messages)

                                                          .Include(n => n.Objective)

                                                          .FirstOrDefaultAsync(n => n.Workday_Cient.Id == model.Id);
               
                if (note == null)   //the note is not exist
                {
                    //Verify the client is not present in other services of notes at the same time
                    if (this.VerifyNotesAtSameTime(model.IdClient, workday_Client.Session, workday_Client.Workday.Date))
                    {
                        return RedirectToAction(nameof(EditIndNote), new { id = model.Id, error = 5, origin = model.Origin });
                    }

                    model.PlanNote = (model.PlanNote.Trim().Last() == '.') ? model.PlanNote.Trim() : $"{model.PlanNote.Trim()}.";
                    model.SubjectiveData = (model.SubjectiveData.Trim().Last() == '.') ? model.SubjectiveData.Trim() : $"{model.SubjectiveData.Trim()}.";
                    model.ObjectiveData = (model.ObjectiveData.Trim().Last() == '.') ? model.ObjectiveData.Trim() : $"{model.ObjectiveData.Trim()}.";
                    model.Assessment = (model.Assessment.Trim().Last() == '.') ? model.Assessment.Trim() : $"{model.Assessment.Trim()}.";

                    individualNoteEntity = await _converterHelper.ToIndividualNoteEntity(model, true);
                    //Update plan progress
                    individualNoteEntity.Exceptional = (form["Progress"] == "Exceptional") ? true : false;
                    individualNoteEntity.Steady = (form["Progress"] == "Steady") ? true : false;
                    individualNoteEntity.Slow_Progress = (form["Progress"] == "Slow") ? true : false;
                    individualNoteEntity.Regressing = (form["Progress"] == "Regressing") ? true : false;
                    individualNoteEntity.Stable = (form["Progress"] == "Stable") ? true : false;
                    individualNoteEntity.Maintain = (form["Progress"] == "Maintain") ? true : false;

                    //vinculo el mtp activo del cliente a la nota que se creará
                    MTPEntity mtp = await _context.MTPs.FirstOrDefaultAsync(m => (m.Client.Id == model.IdClient && m.Active == true));
                    if (mtp != null)
                        individualNoteEntity.MTPId = mtp.Id;

                    //Update selected client in Workday_Client
                    Workday_Client workday_client = await _context.Workdays_Clients                                                                 
                                                                  .FirstOrDefaultAsync(wd => wd.Id == model.Id);
                    workday_client.Client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == model.IdClient);
                    _context.Update(workday_client);

                    //Create individual note
                    _context.Add(individualNoteEntity);
                    
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (model.Origin == 0)
                            return RedirectToAction(nameof(IndividualNotes));
                        if (model.Origin == 1)
                            return RedirectToAction(nameof(NotStartedIndNotes));
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, "Already exists the element");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else    //the note exist, and must be updated
                {
                    note.PlanNote = (model.PlanNote.Trim().Last() == '.') ? model.PlanNote.Trim() : $"{model.PlanNote.Trim()}.";
                    note.SubjectiveData = (model.SubjectiveData.Trim().Last() == '.') ? model.SubjectiveData.Trim() : $"{model.SubjectiveData.Trim()}.";
                    note.ObjectiveData = (model.ObjectiveData.Trim().Last() == '.') ? model.ObjectiveData.Trim() : $"{model.ObjectiveData.Trim()}.";
                    note.Assessment = (model.Assessment.Trim().Last() == '.') ? model.Assessment.Trim() : $"{model.Assessment.Trim()}.";

                    note.Groomed = model.Groomed;
                    note.Unkempt = model.Unkempt;
                    note.Disheveled = model.Disheveled;
                    note.Meticulous = model.Meticulous;
                    note.Overbuild = model.Overbuild;
                    note.Other = model.Other;
                    note.Clear = model.Clear;
                    note.Pressured = model.Pressured;
                    note.Slurred = model.Slurred;
                    note.Slow = model.Slow;
                    note.Impaired = model.Impaired;
                    note.Poverty = model.Poverty;
                    note.Euthymic = model.Euthymic;
                    note.Depressed = model.Depressed;
                    note.Anxious = model.Anxious;
                    note.Fearful = model.Fearful;
                    note.Irritable = model.Irritable;
                    note.Labile = model.Labile;
                    note.WNL = model.WNL;
                    note.Guarded = model.Guarded;
                    note.Withdrawn = model.Withdrawn;
                    note.Hostile = model.Hostile;
                    note.Restless = model.Restless;
                    note.Impulsive = model.Impulsive;
                    note.WNL_Cognition = model.WNL_Cognition;
                    note.Blocked = model.Blocked;
                    note.Obsessive = model.Obsessive;
                    note.Paranoid = model.Paranoid;
                    note.Scattered = model.Scattered;
                    note.Psychotic = model.Psychotic;           
                    note.CBT = model.CBT;
                    note.Psychodynamic = model.Psychodynamic;
                    note.BehaviorModification = model.BehaviorModification;
                    note.Other_Intervention = model.Other_Intervention;               

                    note.Objective = (model.IdObjetive1 != 0) ? await _context.Objetives.FirstOrDefaultAsync(o => o.Id == model.IdObjetive1) : null;

                    //vinculo el mtp activo del cliente a la nota que se creará
                    MTPEntity mtp = await _context.MTPs.FirstOrDefaultAsync(m => (m.Client.Id == model.IdClient && m.Active == true));
                    if (mtp != null)
                        note.MTPId = mtp.Id;

                    _context.Update(note);

                    //Update selected client in Workday_Client
                    Workday_Client workday_client = await _context.Workdays_Clients
                                                                  .FirstOrDefaultAsync(wd => wd.Id == model.Id);
                    workday_client.Client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == model.IdClient);
                    _context.Update(workday_client);                    
                    
                    //todos los mensajes que tiene el Workday_Client de la nota los pongo como leidos
                    foreach (MessageEntity value in note.Workday_Cient.Messages)
                    {
                        value.Status = MessageStatus.Read;
                        value.DateRead = DateTime.Now;
                        _context.Update(value);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        if (model.Origin == 0)
                            return RedirectToAction(nameof(IndividualNotes));
                        if (model.Origin == 1)
                            return RedirectToAction(nameof(NotStartedIndNotes));
                        if (model.Origin == 2)
                            return RedirectToAction(nameof(IndNotesInEdit));
                        if (model.Origin == 3)
                            return RedirectToAction(nameof(PendingIndNotes));
                        if (model.Origin == 4)
                            return RedirectToAction(nameof(IndNotesWithReview));
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, "Already exists the element");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> EditGroupNote(int id, int error = 0, int origin = 0, string errorText = "")
        {
            GroupNoteViewModel noteViewModel;
            
            //la nota no tiene linkeado ningun goal
            if (error == 1)
                ViewBag.Error = "0";

            //la nota no esta completa, faltan campos por editar
            if (error == 2)
                ViewBag.Error = "2";

            //la nota tiene problemas con el genero
            if (error == 4)
            {
                ViewBag.Error = "4";
                ViewBag.errorText = errorText;
            }

            //el cliente seleccionado tiene una nota ya creada de otro servicio en ese mismo horario
            if (error == 5)
            {
                ViewBag.Error = "5";
            }

            Workday_Client workday_Client = await _context.Workdays_Clients

                                                          .Include(wc => wc.Workday)
                                                          .ThenInclude(w => w.Workdays_Activities_Facilitators)
                                                          .ThenInclude(waf => waf.Activity)
                                                          .ThenInclude(a => a.Theme)

                                                          .Include(wc => wc.Client)
                                                          .ThenInclude(c => c.Clinic)

                                                          .Include(wc => wc.Client)
                                                          .ThenInclude(c => c.Group)

                                                          .Include(wc => wc.Client)
                                                          .ThenInclude(c => c.MTPs)

                                                          .Include(wc => wc.Facilitator)

                                                          .FirstOrDefaultAsync(wc => wc.Id == id);

            if (workday_Client == null)
            {
                return NotFound();
            }

            FacilitatorEntity facilitator_logged = _context.Facilitators
                                                           .FirstOrDefault(f => f.LinkedUser == User.Identity.Name);

            //el dia no tiene actividad asociada para el facilitator logueado por lo tanto no se puede crear la nota
            if (workday_Client.Workday.Workdays_Activities_Facilitators.Where(waf => waf.Facilitator == facilitator_logged).Count() == 0)
            {
                ViewBag.Error = "1";
                noteViewModel = new GroupNoteViewModel
                {
                    Id = workday_Client.Workday.Id,
                };
                return View(noteViewModel);
            }

            //el cliente no tiene mtp activos
            if (workday_Client.Client.MTPs.Where(m => m.Active == true).Count() == 0)
            {
                ViewBag.Error = "3";
                noteViewModel = new GroupNoteViewModel
                {
                    Id = workday_Client.Workday.Id,
                };
                return View(noteViewModel);
            }

            GroupNoteEntity note = await _context.GroupNotes

                                                 .Include(n => n.Workday_Cient)
                                                 .ThenInclude(wc => wc.Client)
                                                 .ThenInclude(c => c.Group)

                                                 .Include(n => n.Workday_Cient)
                                                 .ThenInclude(g => g.Facilitator)

                                                 .Include(n => n.GroupNotes_Activities)
                                                 .ThenInclude(na => na.Activity)

                                                 .FirstOrDefaultAsync(n => n.Workday_Cient.Id == id);            

            //-----------se selecciona el primer MTP activo que tenga el cliente-----------//
            MTPEntity mtp = _context.MTPs.FirstOrDefault(m => (m.Client.Id == workday_Client.Client.Id && m.Active == true));

            List<Workday_Activity_Facilitator> activities = workday_Client.Workday
                                                                          .Workdays_Activities_Facilitators
                                                                          .Where(waf => waf.Facilitator == facilitator_logged)
                                                                          .ToList();

            if (note == null)   //la nota no está creada
            {
                IEnumerable<SelectListItem> goals = null;
                IEnumerable<SelectListItem> objs = null;
                if (mtp != null)
                {
                    goals = _combosHelper.GetComboGoals(mtp.Id);
                    objs = _combosHelper.GetComboObjetives(0);
                }
                else
                {
                    goals = _combosHelper.GetComboGoals(0);
                    objs = _combosHelper.GetComboObjetives(0);
                }

                noteViewModel = new GroupNoteViewModel
                {
                    Id = id,
                    Status = NoteStatus.Pending,    //es solo generico para la visualizacion del btn FinishEditing
                    Origin = origin,                   

                    //IdTopic1 = (activities.Count > 0) ? activities[0].Activity.Theme.Id : 0,
                    Topic1 = (activities.Count > 0) ? activities[0].Activity.Theme.Name : string.Empty,
                    IdActivity1 = (activities.Count > 0) ? activities[0].Activity.Id : 0,
                    Activity1 = (activities.Count > 0) ? activities[0].Activity.Name : string.Empty,
                    Goals1 = goals,
                    Objetives1 = objs,

                    //IdTopic2 = (activities.Count > 1) ? activities[1].Activity.Theme.Id : 0,
                    Topic2 = (activities.Count > 1) ? activities[1].Activity.Theme.Name : string.Empty,
                    IdActivity2 = (activities.Count > 1) ? activities[1].Activity.Id : 0,
                    Activity2 = (activities.Count > 1) ? activities[1].Activity.Name : string.Empty,
                    Goals2 = goals,
                    Objetives2 = objs,                    

                    Workday_Cient = workday_Client
                };
            }
            else
            {
                List<GroupNote_Activity> note_Activity = await _context.GroupNotes_Activities

                                                                       .Include(na => na.Activity)
                                                                       .ThenInclude(a => a.Theme)

                                                                       .Include(na => na.Objetive)
                                                                       .ThenInclude(o => o.Goal)

                                                                       .Where(na => na.GroupNote.Id == note.Id)
                                                                       .ToListAsync();

                IEnumerable<SelectListItem> goals = null;
                IEnumerable<SelectListItem> objs = null;
                if (mtp != null)
                {
                    goals = _combosHelper.GetComboGoals(mtp.Id);
                    objs = _combosHelper.GetComboObjetives(0);
                }
                else
                {
                    goals = _combosHelper.GetComboGoals(0);
                    objs = _combosHelper.GetComboObjetives(0);
                }

                noteViewModel = new GroupNoteViewModel
                {
                    Id = id,
                    Origin = origin,
                    Workday_Cient = workday_Client,
                    PlanNote = note.PlanNote,
                    Status = note.Status,

                    Groomed = note.Groomed,
                    Unkempt = note.Unkempt,
                    Disheveled = note.Disheveled,
                    Meticulous = note.Meticulous,
                    Overbuild = note.Overbuild,
                    Other = note.Other,
                    Clear = note.Clear,
                    Pressured = note.Pressured,
                    Slurred = note.Slurred,
                    Slow = note.Slow,
                    Impaired = note.Impaired,
                    Poverty = note.Poverty,
                    Euthymic = note.Euthymic,
                    Depressed = note.Depressed,
                    Anxious = note.Anxious,
                    Fearful = note.Fearful,
                    Irritable = note.Irritable,
                    Labile = note.Labile,
                    WNL = note.WNL,
                    Guarded = note.Guarded,
                    Withdrawn = note.Withdrawn,
                    Hostile = note.Hostile,
                    Restless = note.Restless,
                    Impulsive = note.Impulsive,
                    WNL_Cognition = note.WNL_Cognition,
                    Blocked = note.Blocked,
                    Obsessive = note.Obsessive,
                    Paranoid = note.Paranoid,
                    Scattered = note.Scattered,
                    Psychotic = note.Psychotic,
                    Exceptional = note.Exceptional,
                    Steady = note.Steady,
                    Slow_Progress = note.Slow_Progress,
                    Regressing = note.Regressing,
                    Stable = note.Stable,
                    Maintain = note.Maintain,
                    CBT = note.CBT,
                    Psychodynamic = note.Psychodynamic,
                    BehaviorModification = note.BehaviorModification,
                    Other_Intervention = note.Other_Intervention,

                    Topic1 = (activities.Count > 0) ? activities[0].Activity.Theme.Name : string.Empty,
                    IdActivity1 = (activities.Count > 0) ? activities[0].Activity.Id : 0,
                    Activity1 = (activities.Count > 0) ? activities[0].Activity.Name : string.Empty,
                    AnswerClient1 = note_Activity[0].AnswerClient,
                    AnswerFacilitator1 = note_Activity[0].AnswerFacilitator,
                    IdGoal1 = ((note_Activity.Count > 0) && (note_Activity[0].Objetive != null)) ? note_Activity[0].Objetive.Goal.Id : 0,
                    Goals1 = goals,
                    IdObjetive1 = ((note_Activity.Count > 0) && (note_Activity[0].Objetive != null)) ? note_Activity[0].Objetive.Id : 0,
                    //Paso el IdGoal1 como parametro
                    Objetives1 = _combosHelper.GetComboObjetives(((note_Activity.Count > 0) && (note_Activity[0].Objetive != null)) ? note_Activity[0].Objetive.Goal.Id : 0),
                    Intervention1 = ((note_Activity.Count > 0) && (note_Activity[0].Objetive != null)) ? note_Activity[0].Objetive.Intervention : string.Empty,

                    Topic2 = (activities.Count > 1) ? activities[1].Activity.Theme.Name : string.Empty,
                    IdActivity2 = (activities.Count > 1) ? activities[1].Activity.Id : 0,
                    Activity2 = (activities.Count > 1) ? activities[1].Activity.Name : string.Empty,
                    AnswerClient2 = note_Activity[1].AnswerClient,
                    AnswerFacilitator2 = note_Activity[1].AnswerFacilitator,
                    IdGoal2 = ((note_Activity.Count > 1) && (note_Activity[1].Objetive != null)) ? note_Activity[1].Objetive.Goal.Id : 0,
                    Goals2 = goals,
                    IdObjetive2 = ((note_Activity.Count > 1) && (note_Activity[1].Objetive != null)) ? note_Activity[1].Objetive.Id : 0,
                    //Paso el IdGoal2 como parametro
                    Objetives2 = _combosHelper.GetComboObjetives(((note_Activity.Count > 1) && (note_Activity[1].Objetive != null)) ? note_Activity[1].Objetive.Goal.Id : 0),
                    Intervention2 = ((note_Activity.Count > 1) && (note_Activity[1].Objetive != null)) ? note_Activity[1].Objetive.Intervention : string.Empty                    
                };
            }
            return View(noteViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> EditGroupNote(GroupNoteViewModel model, IFormCollection form)
        {
            Workday_Client workday_Client = await _context.Workdays_Clients.Include(wc => wc.Workday)

                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Clinic)

                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Group)

                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.MTPs)

                                                                           .Include(wc => wc.Facilitator)

                                                                           .FirstOrDefaultAsync(wc => wc.Id == model.Id);
            if (workday_Client == null)
            {
                return NotFound();
            }
                        
            if (ModelState.IsValid)
            {
                GroupNoteEntity note = await _context.GroupNotes
                                                
                                                     .Include(n => n.Workday_Cient)
                                                     .ThenInclude(wc => wc.Messages)
                                                
                                                     .FirstOrDefaultAsync(n => n.Workday_Cient.Id == model.Id);
                
                GroupNote_Activity note_Activity;
                GroupNoteEntity groupNoteEntity;
                if (note == null)   //la nota no está creada
                {
                    //Verify the client is not present in other services of notes at the same time
                    if (this.VerifyNotesAtSameTime(workday_Client.Client.Id, workday_Client.Session, workday_Client.Workday.Date))
                    {
                        return RedirectToAction(nameof(EditGroupNote), new { id = model.Id, error = 5, origin = model.Origin });
                    }

                    groupNoteEntity = await _converterHelper.ToGroupNoteEntity(model, true);

                    //Update plan progress
                    groupNoteEntity.Exceptional = (form["Progress"] == "Exceptional") ? true : false;
                    groupNoteEntity.Steady = (form["Progress"] == "Steady") ? true : false;
                    groupNoteEntity.Slow_Progress = (form["Progress"] == "Slow") ? true : false;
                    groupNoteEntity.Regressing = (form["Progress"] == "Regressing") ? true : false;
                    groupNoteEntity.Stable = (form["Progress"] == "Stable") ? true : false;
                    groupNoteEntity.Maintain = (form["Progress"] == "Maintain") ? true : false;            

                    //vinculo el mtp activo del cliente a la nota que se creará
                    MTPEntity mtp = await _context.MTPs.FirstOrDefaultAsync(m => (m.Client.Id == workday_Client.Client.Id && m.Active == true));
                    if (mtp != null)
                        groupNoteEntity.MTPId = mtp.Id;

                    _context.Add(groupNoteEntity);

                    note_Activity = new GroupNote_Activity
                    {
                        GroupNote = groupNoteEntity,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity1),
                        AnswerClient = model.AnswerClient1.Trim(),
                        AnswerFacilitator = (model.AnswerFacilitator1.Trim().Last() == '.') ? model.AnswerFacilitator1.Trim() : $"{model.AnswerFacilitator1.Trim()}.",
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive1),
                    };
                    _context.Add(note_Activity);
                    note_Activity = new GroupNote_Activity
                    {
                        GroupNote = groupNoteEntity,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity2),
                        AnswerClient = (model.AnswerClient2 != null) ? model.AnswerClient2.Trim() : string.Empty,
                        AnswerFacilitator = (model.AnswerFacilitator2 != null) ? ((model.AnswerFacilitator2.Trim().Last() == '.') ? model.AnswerFacilitator2.Trim() : $"{model.AnswerFacilitator2.Trim()}.") : string.Empty,
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive2),
                    };
                    _context.Add(note_Activity);                   
                    
                    try
                    {
                        await _context.SaveChangesAsync();
                        if (model.Origin == 0)
                            return RedirectToAction(nameof(GroupNotes));
                        if (model.Origin == 1)
                            return RedirectToAction(nameof(NotStartedGroupNotes));
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, "Already exists the element");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else    //la nota está creada y sólo se debe actualizar
                {
                    note.PlanNote = (model.PlanNote.Trim().Last() == '.') ? model.PlanNote.Trim() : $"{model.PlanNote.Trim()}.";

                    note.Groomed = model.Groomed;
                    note.Unkempt = model.Unkempt;
                    note.Disheveled = model.Disheveled;
                    note.Meticulous = model.Meticulous;
                    note.Overbuild = model.Overbuild;
                    note.Other = model.Other;
                    note.Clear = model.Clear;
                    note.Pressured = model.Pressured;
                    note.Slurred = model.Slurred;
                    note.Slow = model.Slow;
                    note.Impaired = model.Impaired;
                    note.Poverty = model.Poverty;
                    note.Euthymic = model.Euthymic;
                    note.Depressed = model.Depressed;
                    note.Anxious = model.Anxious;
                    note.Fearful = model.Fearful;
                    note.Irritable = model.Irritable;
                    note.Labile = model.Labile;
                    note.WNL = model.WNL;
                    note.Guarded = model.Guarded;
                    note.Withdrawn = model.Withdrawn;
                    note.Hostile = model.Hostile;
                    note.Restless = model.Restless;
                    note.Impulsive = model.Impulsive;
                    note.WNL_Cognition = model.WNL_Cognition;
                    note.Blocked = model.Blocked;
                    note.Obsessive = model.Obsessive;
                    note.Paranoid = model.Paranoid;
                    note.Scattered = model.Scattered;
                    note.Psychotic = model.Psychotic;
                    note.CBT = model.CBT;
                    note.Psychodynamic = model.Psychodynamic;
                    note.BehaviorModification = model.BehaviorModification;
                    note.Other_Intervention = model.Other_Intervention;

                    //actualizo el mtp activo del cliente a la nota que se creará                   
                    MTPEntity mtp = await _context.MTPs.FirstOrDefaultAsync(m => (m.Client.Id == workday_Client.Client.Id && m.Active == true));
                    if (mtp != null)
                        note.MTPId = mtp.Id;

                    _context.Update(note);

                    List<GroupNote_Activity> noteActivities_list = await _context.GroupNotes_Activities
                                                                                 .Where(na => na.GroupNote.Id == note.Id)
                                                                                 .ToListAsync();
                    
                    _context.RemoveRange(noteActivities_list);                    

                    note_Activity = new GroupNote_Activity
                    {
                        GroupNote = note,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity1),
                        AnswerClient = model.AnswerClient1.Trim(),
                        AnswerFacilitator = (model.AnswerFacilitator1.Trim().Last() == '.') ? model.AnswerFacilitator1.Trim() : $"{model.AnswerFacilitator1.Trim()}.",
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive1),
                    };
                    _context.Add(note_Activity);
                    await _context.SaveChangesAsync();

                    note_Activity = new GroupNote_Activity
                    {
                        GroupNote = note,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity2),
                        AnswerClient = (model.AnswerClient2 != null) ? model.AnswerClient2.Trim() : string.Empty,
                        AnswerFacilitator = (model.AnswerFacilitator2 != null) ? ((model.AnswerFacilitator2.Trim().Last() == '.') ? model.AnswerFacilitator2.Trim() : $"{model.AnswerFacilitator2.Trim()}.") : string.Empty,
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive2),
                    };
                    _context.Add(note_Activity);
                    
                    //todos los mensajes que tiene el Workday_Client de la nota los pongo como leidos
                    foreach (MessageEntity value in note.Workday_Cient.Messages)
                    {
                        value.Status = MessageStatus.Read;
                        value.DateRead = DateTime.Now;
                        _context.Update(value);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        if (model.Origin == 0)
                            return RedirectToAction(nameof(GroupNotes));
                        if (model.Origin == 1)
                            return RedirectToAction(nameof(NotStartedGroupNotes));
                        if (model.Origin == 2)
                            return RedirectToAction(nameof(GroupNotesInEdit));
                        if (model.Origin == 3)
                            return RedirectToAction(nameof(PendingGroupNotes));
                        if (model.Origin == 4)
                            return RedirectToAction(nameof(GroupNotesWithReview));
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, "Already exists the element");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> FinishEditing(int id, int origin = 0)
        {
            Workday_Client workday_Client = await _context.Workdays_Clients
                                                          .Include(wc => wc.Client)
                                                          .FirstOrDefaultAsync(wc => wc.Id == id);
            if (workday_Client == null)
            {
                return NotFound();
            }

            NoteEntity note = await _context.Notes.Include(n => n.Workday_Cient)
                                                  .ThenInclude(wc => wc.Facilitator)
                                                  .ThenInclude(f => f.Clinic)
                                                  .Include(n => n.Notes_Activities)
                                                  .ThenInclude(na => na.Objetive)
                                                  .FirstOrDefaultAsync(n => n.Workday_Cient.Id == id);

            bool exist = false;
            bool complete = true;
            string gender_problems = string.Empty;
            int index = 1;
            foreach (Note_Activity item in note.Notes_Activities)
            {
                if (item.Objetive != null)
                    exist = true;
                if (string.IsNullOrEmpty(item.AnswerClient) || string.IsNullOrEmpty(item.AnswerFacilitator))
                    complete = false;
                if(!string.IsNullOrEmpty(item.AnswerClient))
                { 
                  if(this.GenderEvaluation(workday_Client.Client.Gender, item.AnswerClient))
                    gender_problems = string.IsNullOrEmpty(gender_problems) ? $"Client Answer #{index}" : $"{gender_problems}, Client Answer #{index}";
                }
                if (!string.IsNullOrEmpty(item.AnswerFacilitator))
                {
                  if(this.GenderEvaluation(workday_Client.Client.Gender, item.AnswerFacilitator))
                    gender_problems = string.IsNullOrEmpty(gender_problems) ? $"Facilitator Answer #{index}" : $"{gender_problems}, Facilitator Answer #{index}";
                }                
                index ++;
            }

            if (this.GenderEvaluation(workday_Client.Client.Gender, note.PlanNote))
                gender_problems = string.IsNullOrEmpty(gender_problems) ? $"Plan" : $"{gender_problems}, Plan";

            if (!exist)     //la nota no tiene goal relacionado
            {
                if(origin == 0)
                    return RedirectToAction(nameof(EditNote), new {id =  id, error = 1, origin = 0});                
                if (origin == 2)
                    return RedirectToAction(nameof(EditNote), new {id = id, error = 1, origin = 2});
            }

            if (note.Schema == Common.Enums.SchemaType.Schema2) //se debe validar que las respuestas a las 4 activiades esten completas
            {
                if (!complete)     //la nota no esta completa
                {
                    if (origin == 0)
                        return RedirectToAction(nameof(EditNote), new { id = id, error = 2, origin = 0 });
                    if (origin == 2)
                        return RedirectToAction(nameof(EditNote), new { id = id, error = 2, origin = 2 });
                }
            }

            if (!string.IsNullOrEmpty(gender_problems))     //la nota tiene problemas con el genero
            {
                if (origin == 0)
                    return RedirectToAction(nameof(EditNote), new { id = id, error = 4, origin = 0, errorText = gender_problems});
                if (origin == 2)
                    return RedirectToAction(nameof(EditNote), new { id = id, error = 4, origin = 2, errorText = gender_problems});
            }
            
            note.Status = NoteStatus.Pending;
            _context.Update(note);

            await _context.SaveChangesAsync();
            if (origin == 2)
                return RedirectToAction(nameof(NotesInEdit), new { id = 1 });
            else
                return RedirectToAction(nameof(Index), new { id = 1});                        
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> FinishEditingIN(int id, int origin = 0)
        {
            Workday_Client workday_Client = await _context.Workdays_Clients
                                                          .Include(wc => wc.Client)
                                                          .FirstOrDefaultAsync(wc => wc.Id == id);
            if (workday_Client == null)
            {
                return NotFound();
            }

            IndividualNoteEntity note = await _context.IndividualNotes

                                                      .Include(n => n.Objective)

                                                      .Include(n => n.Workday_Cient)
                                                      .ThenInclude(wc => wc.Facilitator)
                                                      .ThenInclude(f => f.Clinic)
                                                      
                                                      .FirstOrDefaultAsync(n => n.Workday_Cient.Id == id);

            string gender_problems = string.Empty;
                      
            if (note.Objective == null) //la nota no tiene goal relacionado
            { 
                if (origin == 0)
                    return RedirectToAction(nameof(EditIndNote), new { id = id, error = 1, origin = 0 });
                if (origin == 2)
                    return RedirectToAction(nameof(EditIndNote), new { id = id, error = 1, origin = 2 });
            }                        
            
            if (this.GenderEvaluation(workday_Client.Client.Gender, note.SubjectiveData))
                gender_problems = string.IsNullOrEmpty(gender_problems) ? $"Subjective Data" : $"{gender_problems}, Subjective Data";
            if (this.GenderEvaluation(workday_Client.Client.Gender, note.ObjectiveData))
                gender_problems = string.IsNullOrEmpty(gender_problems) ? $"Objective Data" : $"{gender_problems}, Objective Data";
            if (this.GenderEvaluation(workday_Client.Client.Gender, note.Assessment))
                gender_problems = string.IsNullOrEmpty(gender_problems) ? $"Assessment" : $"{gender_problems}, Assessment";
            if (this.GenderEvaluation(workday_Client.Client.Gender, note.PlanNote))
                gender_problems = string.IsNullOrEmpty(gender_problems) ? $"Plan" : $"{gender_problems}, Plan";
            
            if (!string.IsNullOrEmpty(gender_problems))     //la nota tiene problemas con el genero
            {
                if (origin == 0)
                    return RedirectToAction(nameof(EditIndNote), new { id = id, error = 4, origin = 0, errorText = gender_problems });
                if (origin == 2)
                    return RedirectToAction(nameof(EditIndNote), new { id = id, error = 4, origin = 2, errorText = gender_problems });
            }

            note.Status = NoteStatus.Pending;
            _context.Update(note);

            await _context.SaveChangesAsync();
            if (origin == 2)
                return RedirectToAction(nameof(IndNotesInEdit), new { id = 1 });
            else
                return RedirectToAction(nameof(IndividualNotes), new { id = 1 });
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> FinishEditingGroup(int id, int origin = 0)
        {
            Workday_Client workday_Client = await _context.Workdays_Clients
                                                          .Include(wc => wc.Client)
                                                          .FirstOrDefaultAsync(wc => wc.Id == id);
            if (workday_Client == null)
            {
                return NotFound();
            }

            GroupNoteEntity note = await _context.GroupNotes

                                                 .Include(n => n.Workday_Cient)
                                                 .ThenInclude(wc => wc.Facilitator)
                                                 .ThenInclude(f => f.Clinic)

                                                  .Include(n => n.GroupNotes_Activities)
                                                  .ThenInclude(na => na.Objetive)

                                                  .FirstOrDefaultAsync(n => n.Workday_Cient.Id == id);

            bool exist = false;
            bool complete = true;
            string gender_problems = string.Empty;
            int index = 1;
            foreach (GroupNote_Activity item in note.GroupNotes_Activities)
            {
                if (item.Objetive != null)
                    exist = true;
                if (string.IsNullOrEmpty(item.AnswerClient) || string.IsNullOrEmpty(item.AnswerFacilitator))
                    complete = false;
                if (!string.IsNullOrEmpty(item.AnswerClient))
                {
                    if (this.GenderEvaluation(workday_Client.Client.Gender, item.AnswerClient))
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? $"Client Answer #{index}" : $"{gender_problems}, Client Answer #{index}";
                }
                if (!string.IsNullOrEmpty(item.AnswerFacilitator))
                {
                    if (this.GenderEvaluation(workday_Client.Client.Gender, item.AnswerFacilitator))
                        gender_problems = string.IsNullOrEmpty(gender_problems) ? $"Facilitator Answer #{index}" : $"{gender_problems}, Facilitator Answer #{index}";
                }
                index++;
            }

            if (this.GenderEvaluation(workday_Client.Client.Gender, note.PlanNote))
                gender_problems = string.IsNullOrEmpty(gender_problems) ? $"Plan" : $"{gender_problems}, Plan";

            if (!exist)     //la nota no tiene goal relacionado
            {
                if (origin == 0)
                    return RedirectToAction(nameof(EditGroupNote), new { id = id, error = 1, origin = 0 });
                if (origin == 2)
                    return RedirectToAction(nameof(EditGroupNote), new { id = id, error = 1, origin = 2 });
            }            

            if (!string.IsNullOrEmpty(gender_problems))     //la nota tiene problemas con el genero
            {
                if (origin == 0)
                    return RedirectToAction(nameof(EditGroupNote), new { id = id, error = 4, origin = 0, errorText = gender_problems });
                if (origin == 2)
                    return RedirectToAction(nameof(EditGroupNote), new { id = id, error = 4, origin = 2, errorText = gender_problems });
            }

            note.Status = NoteStatus.Pending;
            _context.Update(note);

            await _context.SaveChangesAsync();
            if (origin == 2)
                return RedirectToAction(nameof(GroupNotesInEdit), new { id = 1 });
            else
                return RedirectToAction(nameof(GroupNotes), new { id = 1 });
        }

        [Authorize(Roles = "Facilitator")]
        public JsonResult GetActivityList(int idTheme)
        {
            List<ActivityEntity> activities = _context.Activities.Where(a => a.Theme.Id == idTheme).ToList();

            return Json(new SelectList(activities, "Id", "Name"));
        }

        [Authorize(Roles = "Facilitator")]
        public JsonResult GetObjetiveList(int idGoal)
        {
            List<ObjetiveEntity> objetives = _context.Objetives.Where(o => o.Goal.Id == idGoal).ToList();
            if (objetives.Count == 0)
            {
                objetives.Insert(0, new ObjetiveEntity
                {
                    Objetive = "[First select goal...]",
                    Id = 0
                });
            }
            return Json(new SelectList(objetives, "Id", "Objetive"));
        }

        [Authorize(Roles = "Facilitator")]
        public JsonResult GetIntervention(int idObjetive)
        {
            ObjetiveEntity objetive = _context.Objetives.FirstOrDefault(o => o.Id == idObjetive);
            string text = "Select goal and objective";
            if (objetive != null)
            {
                text = objetive.Intervention;
            }
            return Json(text);
        }

        [Authorize(Roles = "Facilitator")]
        public JsonResult GetGoalsList(int idClient)
        {
            MTPEntity mtp = _context.MTPs
                                    .FirstOrDefault(m => (m.Client.Id == idClient && m.Active == true));
            
            List<SelectListItem> list = new List<SelectListItem>();

            if (mtp != null)
            {
                list = _context.Goals.Where(g => g.MTP.Id == mtp.Id).Select(g => new SelectListItem
                {
                    Text = $"{g.Number}",
                    Value = $"{g.Id}"
                }).ToList();
                list.Insert(0, new SelectListItem
                {
                    Text = "[Select goal...]",
                    Value = "0"
                });
            }
            else
            {
                list.Add(new SelectListItem
                {
                    Text = "[Select goal...]",
                    Value = "0"
                });
            }         

            return Json(new SelectList(list, "Value", "Text"));
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> NotesSupervision()
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            List<WeekEntity> weeks = (await _context.Weeks
                                                    
                                                    .Include(w => w.Days)
                                                    .ThenInclude(d => d.Workdays_Clients)
                                                    .ThenInclude(wc => wc.Client)
                                                    .ThenInclude(c => c.Group)

                                                    .Include(w => w.Days)
                                                    .ThenInclude(d => d.Workdays_Clients)
                                                    .ThenInclude(g => g.Facilitator)

                                                    .Include(w => w.Days)
                                                    .ThenInclude(d => d.Workdays_Clients)
                                                    .ThenInclude(wc => wc.Note)

                                                    .Where(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                              && w.Days.Where(d => (d.Service == ServiceType.PSR && d.Workdays_Clients.Where(wc => wc.Note.Status == NoteStatus.Pending).Count() > 0)).Count() > 0))
                                                    .ToListAsync());
            
            return View(weeks);
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> IndNotesSupervision()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            List<WeekEntity> weeks = (await _context.Weeks

                                                    .Include(w => w.Days)
                                                    .ThenInclude(d => d.Workdays_Clients)
                                                    .ThenInclude(wc => wc.Client)                                                          

                                                    .Include(w => w.Days)
                                                    .ThenInclude(d => d.Workdays_Clients)
                                                    .ThenInclude(g => g.Facilitator)

                                                    .Include(w => w.Days)
                                                    .ThenInclude(d => d.Workdays_Clients)
                                                    .ThenInclude(wc => wc.IndividualNote)
                                                    
                                                    .Where(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                              && w.Days.Where(d => (d.Service == ServiceType.Individual && d.Workdays_Clients.Where(wc => wc.IndividualNote.Status == NoteStatus.Pending).Count() > 0)).Count() > 0))
                                                    .ToListAsync());

            return View(weeks);
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> GroupNotesSupervision()
        {
            UserEntity user_logged = await _context.Users

                                                    .Include(u => u.Clinic)

                                                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            List<WeekEntity> weeks = (await _context.Weeks

                                                    .Include(w => w.Days)
                                                    .ThenInclude(d => d.Workdays_Clients)
                                                    .ThenInclude(wc => wc.Client)
                                                    .ThenInclude(c => c.Group)

                                                    .Include(w => w.Days)
                                                    .ThenInclude(d => d.Workdays_Clients)
                                                    .ThenInclude(g => g.Facilitator)

                                                    .Include(w => w.Days)
                                                    .ThenInclude(d => d.Workdays_Clients)
                                                    .ThenInclude(wc => wc.GroupNote)

                                                    .Where(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                              && w.Days.Where(d => (d.Service == ServiceType.Group && d.Workdays_Clients.Where(wc => wc.GroupNote.Status == NoteStatus.Pending).Count() > 0)).Count() > 0))
                                                    .ToListAsync());

            return View(weeks);
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> ApproveNote(int id, int origin = 0)
        {
            Workday_Client workday_Client = await _context.Workdays_Clients.Include(wc => wc.Workday)

                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Clinic)

                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Group)

                                                                           .Include(wc => wc.Facilitator)
                                                                           
                                                                           .FirstOrDefaultAsync(wc => wc.Id == id);

            if (workday_Client == null)
            {
                return NotFound();
            }

            NoteEntity note = await _context.Notes.Include(n => n.Workday_Cient)
                                                  .ThenInclude(wc => wc.Client)
                                                  .ThenInclude(c => c.Group)
                                                  .ThenInclude(g => g.Facilitator)

                                                  .Include(n => n.Notes_Activities)
                                                  .ThenInclude(na => na.Activity)

                                                  .FirstOrDefaultAsync(n => n.Workday_Cient.Id == id);

            NoteViewModel noteViewModel = null;
                        
            List<Note_Activity> note_Activity = await _context.Notes_Activities
                                                                .Include(na => na.Activity)
                                                                .ThenInclude(a => a.Theme)

                                                                .Include(n => n.Objetive)
                                                                .ThenInclude(o => o.Goal)

                                                                .Where(na => na.Note.Id == note.Id).ToListAsync();
            
            if ((note.Schema == Common.Enums.SchemaType.Schema1) || (note.Schema == Common.Enums.SchemaType.Schema2))
            {
                noteViewModel = new NoteViewModel
                {
                    Id = id,
                    Workday_Cient = workday_Client,
                    PlanNote = note.PlanNote,
                    Origin = origin,
                    Schema = note.Schema,

                    OrientedX3 = note.OrientedX3,
                    NotTime = note.NotTime,
                    NotPlace = note.NotPlace,
                    NotPerson = note.NotPerson,
                    Present = note.Present,
                    Adequate = note.Adequate,
                    Limited = note.Limited,
                    Impaired = note.Impaired,
                    Faulty = note.Faulty,
                    Euthymic = note.Euthymic,
                    Congruent = note.Congruent,
                    Negativistic = note.Negativistic,
                    Depressed = note.Depressed,
                    Euphoric = note.Euphoric,
                    Optimistic = note.Optimistic,
                    Anxious = note.Anxious,
                    Hostile = note.Hostile,
                    Withdrawn = note.Withdrawn,
                    Irritable = note.Irritable,
                    Dramatized = note.Dramatized,
                    AdequateAC = note.AdequateAC,
                    Inadequate = note.Inadequate,
                    Fair = note.Fair,
                    Unmotivated = note.Unmotivated,
                    Motivated = note.Motivated,
                    Guarded = note.Guarded,
                    Normal = note.Normal,
                    ShortSpanned = note.ShortSpanned,
                    MildlyImpaired = note.MildlyImpaired,
                    SeverelyImpaired = note.SeverelyImpaired,

                    Topic1 = note_Activity[0].Activity.Theme.Name,
                    Activity1 = note_Activity[0].Activity.Name,
                    AnswerClient1 = note_Activity[0].AnswerClient,
                    AnswerFacilitator1 = note_Activity[0].AnswerFacilitator,
                    Goal1 = (note_Activity[0].Objetive != null) ? note_Activity[0].Objetive.Goal.Number.ToString() : string.Empty,
                    Objetive1 = (note_Activity[0].Objetive != null) ? note_Activity[0].Objetive.Objetive : string.Empty,

                    Topic2 = note_Activity[1].Activity.Theme.Name,
                    Activity2 = note_Activity[1].Activity.Name,
                    AnswerClient2 = note_Activity[1].AnswerClient,
                    AnswerFacilitator2 = note_Activity[1].AnswerFacilitator,
                    Goal2 = (note_Activity[1].Objetive != null) ? note_Activity[1].Objetive.Goal.Number.ToString() : string.Empty,
                    Objetive2 = (note_Activity[1].Objetive != null) ? note_Activity[1].Objetive.Objetive : string.Empty,

                    Topic3 = note_Activity[2].Activity.Theme.Name,
                    Activity3 = note_Activity[2].Activity.Name,
                    AnswerClient3 = note_Activity[2].AnswerClient,
                    AnswerFacilitator3 = note_Activity[2].AnswerFacilitator,
                    Goal3 = (note_Activity[2].Objetive != null) ? note_Activity[2].Objetive.Goal.Number.ToString() : string.Empty,
                    Objetive3 = (note_Activity[2].Objetive != null) ? note_Activity[2].Objetive.Objetive : string.Empty,

                    Topic4 = note_Activity[3].Activity.Theme.Name,
                    Activity4 = note_Activity[3].Activity.Name,
                    AnswerClient4 = note_Activity[3].AnswerClient,
                    AnswerFacilitator4 = note_Activity[3].AnswerFacilitator,
                    Goal4 = (note_Activity[3].Objetive != null) ? note_Activity[3].Objetive.Goal.Number.ToString() : string.Empty,
                    Objetive4 = (note_Activity[3].Objetive != null) ? note_Activity[3].Objetive.Objetive : string.Empty,
                };
            }
            if (note.Schema == Common.Enums.SchemaType.Schema4)
            {
                noteViewModel = new NoteViewModel
                {
                    Id = id,
                    Workday_Cient = workday_Client,
                    PlanNote = note.PlanNote,
                    Origin = origin,
                    Schema = note.Schema,

                    OrientedX3 = note.OrientedX3,
                    NotTime = note.NotTime,
                    NotPlace = note.NotPlace,
                    NotPerson = note.NotPerson,
                    Present = note.Present,
                    Adequate = note.Adequate,
                    Limited = note.Limited,
                    Impaired = note.Impaired,
                    Faulty = note.Faulty,
                    Euthymic = note.Euthymic,
                    Congruent = note.Congruent,
                    Negativistic = note.Negativistic,
                    Depressed = note.Depressed,
                    Euphoric = note.Euphoric,
                    Optimistic = note.Optimistic,
                    Anxious = note.Anxious,
                    Hostile = note.Hostile,
                    Withdrawn = note.Withdrawn,
                    Irritable = note.Irritable,
                    Dramatized = note.Dramatized,
                    AdequateAC = note.AdequateAC,
                    Inadequate = note.Inadequate,
                    Fair = note.Fair,
                    Unmotivated = note.Unmotivated,
                    Motivated = note.Motivated,
                    Guarded = note.Guarded,
                    Normal = note.Normal,
                    ShortSpanned = note.ShortSpanned,
                    MildlyImpaired = note.MildlyImpaired,
                    SeverelyImpaired = note.SeverelyImpaired,

                    Topic1 = note_Activity[0].Activity.Theme.Name,
                    Activity1 = note_Activity[0].Activity.Name,
                    AnswerClient1 = note_Activity[0].AnswerClient,
                    AnswerFacilitator1 = note_Activity[0].AnswerFacilitator,
                    Goal1 = (note_Activity[0].Objetive != null) ? note_Activity[0].Objetive.Goal.Number.ToString() : string.Empty,
                    Objetive1 = (note_Activity[0].Objetive != null) ? note_Activity[0].Objetive.Objetive : string.Empty,

                    Topic2 = note_Activity[1].Activity.Theme.Name,
                    Activity2 = note_Activity[1].Activity.Name,
                    AnswerClient2 = note_Activity[1].AnswerClient,
                    AnswerFacilitator2 = note_Activity[1].AnswerFacilitator,
                    Goal2 = (note_Activity[1].Objetive != null) ? note_Activity[1].Objetive.Goal.Number.ToString() : string.Empty,
                    Objetive2 = (note_Activity[1].Objetive != null) ? note_Activity[1].Objetive.Objetive : string.Empty,

                    Topic3 = note_Activity[2].Activity.Theme.Name,
                    Activity3 = note_Activity[2].Activity.Name,
                    AnswerClient3 = note_Activity[2].AnswerClient,
                    AnswerFacilitator3 = note_Activity[2].AnswerFacilitator,
                    Goal3 = (note_Activity[2].Objetive != null) ? note_Activity[2].Objetive.Goal.Number.ToString() : string.Empty,
                    Objetive3 = (note_Activity[2].Objetive != null) ? note_Activity[2].Objetive.Objetive : string.Empty                    
                };
            }            
            
            return View(noteViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> ApproveNote(NoteViewModel model)
        {
            if (model == null)
            {
                return NotFound();
            }

            NoteEntity note = await _context.Notes
                                            .Include(n => n.Notes_Activities)
                                            .FirstOrDefaultAsync(n => n.Workday_Cient.Id == model.Id);

            
            note.Status = NoteStatus.Approved;
            note.DateOfApprove = DateTime.Now;
            note.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.LinkedUser == User.Identity.Name);
            _context.Update(note);

            await _context.SaveChangesAsync();           
                
            if (model.Origin == 3)  ///viene de la pagina PendingNotes
                return RedirectToAction(nameof(PendingNotes));
            if (model.Origin == 4)  ///viene de la pagina NotesWithReview
                return RedirectToAction(nameof(NotesWithReview));

            return RedirectToAction(nameof(NotesSupervision));
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> ApproveIndNote(int id, int origin = 0)
        {
            Workday_Client workday_Client = await _context.Workdays_Clients.Include(wc => wc.Workday)

                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Clinic)

                                                                           .Include(wc => wc.Client)                                                                           

                                                                           .Include(wc => wc.Facilitator)

                                                                           .FirstOrDefaultAsync(wc => wc.Id == id);

            if (workday_Client == null)
            {
                return NotFound();
            }

            IndividualNoteEntity note = await _context.IndividualNotes
                                                      
                                                      .Include(n => n.Workday_Cient)
                                                      .ThenInclude(wc => wc.Client)                                                  

                                                      .Include(n => n.Workday_Cient)
                                                      .ThenInclude(wc => wc.Facilitator)   
                                                      
                                                      .Include(n => n.Objective)
                                                      .ThenInclude(o => o.Goal)

                                                      .FirstOrDefaultAsync(n => n.Workday_Cient.Id == id);

            IndividualNoteViewModel individualNoteViewModel = null;


            individualNoteViewModel = new IndividualNoteViewModel
            {
                Id = id,
                Workday_Cient = workday_Client,
                SubjectiveData = note.SubjectiveData,
                ObjectiveData = note.ObjectiveData,
                Assessment = note.Assessment,
                PlanNote = note.PlanNote,
                Origin = origin,

                Groomed = note.Groomed,
                Unkempt = note.Unkempt,
                Disheveled = note.Disheveled,
                Meticulous = note.Meticulous,
                Overbuild = note.Overbuild,
                Other = note.Other,
                Clear = note.Clear,
                Pressured = note.Pressured,
                Slurred = note.Slurred,
                Slow = note.Slow,
                Impaired = note.Impaired,
                Poverty = note.Poverty,
                Euthymic = note.Euthymic,
                Depressed = note.Depressed,
                Anxious = note.Anxious,
                Fearful = note.Fearful,
                Irritable = note.Irritable,
                Labile = note.Labile,
                WNL = note.WNL,
                Guarded = note.Guarded,
                Withdrawn = note.Withdrawn,
                Hostile = note.Hostile,
                Restless = note.Restless,
                Impulsive = note.Impulsive,
                WNL_Cognition = note.WNL_Cognition,
                Blocked = note.Blocked,
                Obsessive = note.Obsessive,
                Paranoid = note.Paranoid,
                Scattered = note.Scattered,
                Psychotic = note.Psychotic,
                Exceptional = note.Exceptional,
                Steady = note.Steady,
                Slow_Progress = note.Slow_Progress,
                Regressing = note.Regressing,
                Stable = note.Stable,
                Maintain = note.Maintain,
                CBT = note.CBT,
                Psychodynamic = note.Psychodynamic,
                BehaviorModification = note.BehaviorModification,
                Other_Intervention = note.Other_Intervention,

                Goal1 = (note.Objective == null) ? string.Empty : note.Objective.Goal.Number.ToString(),
                Objetive1 = (note.Objective == null) ? string.Empty : note.Objective.Objetive,                
                Intervention1 = (note.Objective == null) ? string.Empty : note.Objective.Intervention
            };           

            return View(individualNoteViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> ApproveIndNote(IndividualNoteViewModel model)
        {
            if (model == null)
            {
                return NotFound();
            }

            IndividualNoteEntity note = await _context.IndividualNotes                                            
                                                      .FirstOrDefaultAsync(n => n.Workday_Cient.Id == model.Id);


            note.Status = NoteStatus.Approved;
            note.DateOfApprove = DateTime.Now;
            note.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.LinkedUser == User.Identity.Name);
            _context.Update(note);

            await _context.SaveChangesAsync();

            if (model.Origin == 5)  ///viene de la pagina PendingIndNotes
                return RedirectToAction(nameof(PendingIndNotes));
            if (model.Origin == 6)  ///viene de la pagina NotesWithReview
                return RedirectToAction(nameof(IndNotesWithReview));

            return RedirectToAction(nameof(IndNotesSupervision));
        }

        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> ApproveGroupNote(int id, int origin = 0)
        {
            Workday_Client workday_Client = await _context.Workdays_Clients.Include(wc => wc.Workday)

                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Clinic)

                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Group)

                                                                           .Include(wc => wc.Facilitator)

                                                                           .FirstOrDefaultAsync(wc => wc.Id == id);

            if (workday_Client == null)
            {
                return NotFound();
            }

            GroupNoteEntity note = await _context.GroupNotes

                                                 .Include(n => n.Workday_Cient)
                                                 .ThenInclude(wc => wc.Client)
                                                 .ThenInclude(c => c.Group)
                                                 .ThenInclude(g => g.Facilitator)

                                                 .Include(n => n.GroupNotes_Activities)
                                                 .ThenInclude(na => na.Activity)

                                                 .FirstOrDefaultAsync(n => n.Workday_Cient.Id == id);

            GroupNoteViewModel noteViewModel = null;

            List<GroupNote_Activity> note_Activity = await _context.GroupNotes_Activities
                                                                   
                                                                   .Include(na => na.Activity)
                                                                   .ThenInclude(a => a.Theme)

                                                                   .Include(n => n.Objetive)
                                                                   .ThenInclude(o => o.Goal)

                                                                   .Where(na => na.GroupNote.Id == note.Id)
                                                                   .ToListAsync();

            noteViewModel = new GroupNoteViewModel
            {
                Id = id,
                Workday_Cient = workday_Client,
                PlanNote = note.PlanNote,
                Origin = origin,

                Groomed = note.Groomed,
                Unkempt = note.Unkempt,
                Disheveled = note.Disheveled,
                Meticulous = note.Meticulous,
                Overbuild = note.Overbuild,
                Other = note.Other,
                Clear = note.Clear,
                Pressured = note.Pressured,
                Slurred = note.Slurred,
                Slow = note.Slow,
                Impaired = note.Impaired,
                Poverty = note.Poverty,
                Euthymic = note.Euthymic,
                Depressed = note.Depressed,
                Anxious = note.Anxious,
                Fearful = note.Fearful,
                Irritable = note.Irritable,
                Labile = note.Labile,
                WNL = note.WNL,
                Guarded = note.Guarded,
                Withdrawn = note.Withdrawn,
                Hostile = note.Hostile,
                Restless = note.Restless,
                Impulsive = note.Impulsive,
                WNL_Cognition = note.WNL_Cognition,
                Blocked = note.Blocked,
                Obsessive = note.Obsessive,
                Paranoid = note.Paranoid,
                Scattered = note.Scattered,
                Psychotic = note.Psychotic,
                Exceptional = note.Exceptional,
                Steady = note.Steady,
                Slow_Progress = note.Slow_Progress,
                Regressing = note.Regressing,
                Stable = note.Stable,
                Maintain = note.Maintain,
                CBT = note.CBT,
                Psychodynamic = note.Psychodynamic,
                BehaviorModification = note.BehaviorModification,
                Other_Intervention = note.Other_Intervention,

                Topic1 = note_Activity[0].Activity.Theme.Name,
                Activity1 = note_Activity[0].Activity.Name,
                AnswerClient1 = note_Activity[0].AnswerClient,
                AnswerFacilitator1 = note_Activity[0].AnswerFacilitator,
                Goal1 = (note_Activity[0].Objetive != null) ? note_Activity[0].Objetive.Goal.Number.ToString() : string.Empty,
                Objetive1 = (note_Activity[0].Objetive != null) ? note_Activity[0].Objetive.Objetive : string.Empty,

                Topic2 = note_Activity[1].Activity.Theme.Name,
                Activity2 = note_Activity[1].Activity.Name,
                AnswerClient2 = note_Activity[1].AnswerClient,
                AnswerFacilitator2 = note_Activity[1].AnswerFacilitator,
                Goal2 = (note_Activity[1].Objetive != null) ? note_Activity[1].Objetive.Goal.Number.ToString() : string.Empty,
                Objetive2 = (note_Activity[1].Objetive != null) ? note_Activity[1].Objetive.Objetive : string.Empty                
            };           

            return View(noteViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Facilitator")]
        public async Task<IActionResult> ApproveGroupNote(GroupNoteViewModel model)
        {
            if (model == null)
            {
                return NotFound();
            }

            GroupNoteEntity note = await _context.GroupNotes                                                 

                                                 .FirstOrDefaultAsync(n => n.Workday_Cient.Id == model.Id);


            note.Status = NoteStatus.Approved;
            note.DateOfApprove = DateTime.Now;
            note.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.LinkedUser == User.Identity.Name);
            _context.Update(note);

            await _context.SaveChangesAsync();

            if (model.Origin == 7)  //viene de la pagina PendingGroupNotes
                return RedirectToAction(nameof(PendingGroupNotes));
            if (model.Origin == 8)  //viene de la pagina GroupNotesWithReview
                return RedirectToAction(nameof(GroupNotesWithReview));

            return RedirectToAction(nameof(GroupNotesSupervision));
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> PrintNotes(int id)
        {
            WorkdayEntity workday = await _context.Workdays.FirstOrDefaultAsync(w => w.Id == id);

            if (workday == null)
            {
                return NotFound();
            }

            PrintNotesViewModel noteViewModel = new PrintNotesViewModel
            {
                DateOfPrint = workday.Date
            };

            return View(noteViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> PrintNotes(PrintNotesViewModel model, IFormCollection form)
        {
            var meridian = (form["classifications"] == "First") ? "AM" : "PM";
            List<Workday_Client> list = await _context.Workdays_Clients
                                                      .Include(wc => wc.Client)
                                                      .ThenInclude(c => c.Group)

                                                      .Include(wc => wc.Facilitator)
                                                      
                                                      .Include(wc => wc.Note)
                                                      .ThenInclude(n => n.Supervisor)

                                                      .Include(wc => wc.Note)
                                                      .ThenInclude(n => n.Notes_Activities)

                                                      .Where(wc => (wc.Workday.Date == model.DateOfPrint 
                                                          && wc.Client.Group.Facilitator.LinkedUser == User.Identity.Name
                                                          && wc.Session == meridian
                                                          && wc.Note.Status == NoteStatus.Approved)).ToListAsync();
            if (list.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "There are not a approved notes on that date");
            }

            //WorkdayEntity workday = await _context.Workdays.FirstOrDefaultAsync(w => w.Id == id);

            //if (workday == null)
            //{
            //    return NotFound();
            //}

            //PrintNotesViewModel noteViewModel = new PrintNotesViewModel
            //{
            //    DateOfPrint = workday.Date
            //};

            return View(model);
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> PrintWorkdaysNotes(int id)
        {
            WorkdayEntity workday = await _context.Workdays

                                                  .Include(w => w.Workdays_Clients)     
                                                  .ThenInclude(wc => wc.Facilitator)

                                                  .FirstOrDefaultAsync(w => w.Id == id);

            if (workday == null)
            {
                return NotFound();
            }

            IEnumerable<Workday_Client> workdayClientList = workday.Workdays_Clients
                                                                   .Where(wc => wc.Facilitator.LinkedUser == User.Identity.Name
                                                                             && wc.Workday.Id == workday.Id);
            Workday_Client workdayClient;
            List<FileContentResult> fileContentList = new List<FileContentResult>();
            foreach (var item in workdayClientList)
            {
                workdayClient = _context.Workdays_Clients

                                        .Include(wc => wc.Facilitator)

                                        .Include(wc => wc.Client)
                                        .ThenInclude(c => c.MTPs)
                                        .ThenInclude(m => m.Goals)
                                        .ThenInclude(g => g.Objetives)

                                        .Include(wc => wc.Client)
                                        .ThenInclude(c => c.Clients_Diagnostics)
                                        .ThenInclude(cd => cd.Diagnostic)

                                        .Include(wc => wc.Note)
                                        .ThenInclude(n => n.Supervisor)
                                        .ThenInclude(s => s.Clinic)

                                        .Include(wc => wc.Note)
                                        .ThenInclude(n => n.Notes_Activities)
                                        .ThenInclude(na => na.Activity)
                                        .ThenInclude(a => a.Theme)

                                        .Include(wc => wc.Note)
                                        .ThenInclude(n => n.Notes_Activities)
                                        .ThenInclude(na => na.Objetive)
                                        .ThenInclude(o => o.Goal)

                                        .Include(wc => wc.Note)
                                        .ThenInclude(n => n.Notes_Activities)

                                        .Include(wc => wc.Workday)

                                        .FirstOrDefault(wc => (wc.Id == item.Id));

                if ((workdayClient.Note != null) && (workdayClient.Note.Status == NoteStatus.Approved))
                {
                    if (workdayClient.Note.Supervisor.Clinic.Name == "DAVILA")
                    {
                        if (workdayClient.Note.Schema == Common.Enums.SchemaType.Schema1)
                        {
                            fileContentList.Add(DavilaNoteReportFCRSchema1(workdayClient));
                        }
                        if (workdayClient.Note.Schema == Common.Enums.SchemaType.Schema4)
                        {
                            Stream stream = _reportHelper.DavilaNoteReportSchema4(workdayClient); 
                            fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"{workdayClient.Client.Name}.pdf"));
                        }
                    }
                    if (workdayClient.Note.Supervisor.Clinic.Name == "LARKIN BEHAVIOR")
                    {
                        if (workdayClient.Note.Schema == Common.Enums.SchemaType.Schema1)
                        {
                            fileContentList.Add(LarkinNoteReportFCRSchema1(workdayClient));
                        }
                        if (workdayClient.Note.Schema == Common.Enums.SchemaType.Schema2)
                        {
                            fileContentList.Add(LarkinNoteReportFCRSchema2(workdayClient));
                        }                        
                    }
                    if (workdayClient.Note.Supervisor.Clinic.Name == "SOL & VIDA")
                    {
                        fileContentList.Add(SolAndVidaNoteReportFCRSchema1(workdayClient));
                    }
                    if (workdayClient.Note.Supervisor.Clinic.Name == "HEALTH & BEAUTY NGB, INC")
                    {
                        fileContentList.Add(HealthAndBeautyNoteReportFCRSchema2(workdayClient));                        
                    }
                    if (workdayClient.Note.Supervisor.Clinic.Name == "ADVANCED GROUP MEDICAL CENTER")
                    {
                        fileContentList.Add(AdvancedGroupMCNoteReportFCRSchema2(workdayClient));                        
                    }
                    if (workdayClient.Note.Supervisor.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
                    {
                        fileContentList.Add(FloridaSocialHSNoteReportFCRSchema2(workdayClient));                        
                    }
                    if (workdayClient.Note.Supervisor.Clinic.Name == "ATLANTIC GROUP MEDICAL CENTER")
                    {
                        fileContentList.Add(AtlanticGroupMCNoteReportFCRSchema2(workdayClient));                        
                    }
                    if (workdayClient.Note.Supervisor.Clinic.Name == "DEMO CLINIC SCHEMA 1")
                    {
                        fileContentList.Add(DemoClinic1NoteReportFCRSchema1(workdayClient));                        
                    }
                    if (workdayClient.Note.Supervisor.Clinic.Name == "DEMO CLINIC SCHEMA 2")
                    {
                        fileContentList.Add(DemoClinic2NoteReportFCRSchema2(workdayClient));                        
                    }
                }                
            }

            return this.ZipFile(fileContentList, $"{workday.Date.ToShortDateString()}.zip");
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> PrintWorkdaysIndNotes(int id)
        {
            WorkdayEntity workday = await _context.Workdays

                                                  .Include(w => w.Workdays_Clients)
                                                  .ThenInclude(wc => wc.Facilitator)

                                                  .FirstOrDefaultAsync(w => w.Id == id);

            if (workday == null)
            {
                return NotFound();
            }

            IEnumerable<Workday_Client> workdayClientList = workday.Workdays_Clients
                                                                   .Where(wc => wc.Facilitator.LinkedUser == User.Identity.Name
                                                                             && wc.Workday.Id == workday.Id);
            Workday_Client workdayClient;
            List<FileContentResult> fileContentList = new List<FileContentResult>();
            foreach (var item in workdayClientList)
            {
                workdayClient = _context.Workdays_Clients

                                        .Include(wc => wc.Facilitator)

                                        .Include(wc => wc.Client)
                                        .ThenInclude(c => c.MTPs)
                                        .ThenInclude(m => m.Goals)
                                        .ThenInclude(g => g.Objetives)

                                        .Include(wc => wc.Client)
                                        .ThenInclude(c => c.Clients_Diagnostics)
                                        .ThenInclude(cd => cd.Diagnostic)

                                        .Include(wc => wc.IndividualNote)
                                        .ThenInclude(n => n.Supervisor)
                                        .ThenInclude(s => s.Clinic)

                                        .Include(wc => wc.IndividualNote)
                                        .ThenInclude(n => n.Objective)

                                        .Include(wc => wc.Workday)

                                        .FirstOrDefault(wc => (wc.Id == item.Id && wc.IndividualNote.Status == NoteStatus.Approved));

                if (workdayClient != null)
                {
                    if (workdayClient.IndividualNote.Supervisor.Clinic.Name == "DAVILA")
                    {                       
                       Stream stream = _reportHelper.DavilaIndNoteReportSchema1(workdayClient);
                       fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"{workdayClient.Client.Name}.pdf"));                        
                    }                    
                }
            }

            return this.ZipFile(fileContentList, $"{workday.Date.ToShortDateString()}.zip");
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> PrintWorkdaysGroupNotes(int id)
        {
            WorkdayEntity workday = await _context.Workdays

                                                  .Include(w => w.Workdays_Clients)
                                                  .ThenInclude(wc => wc.Facilitator)

                                                  .FirstOrDefaultAsync(w => w.Id == id);

            if (workday == null)
            {
                return NotFound();
            }

            IEnumerable<Workday_Client> workdayClientList = workday.Workdays_Clients
                                                                   .Where(wc => wc.Facilitator.LinkedUser == User.Identity.Name
                                                                             && wc.Workday.Id == workday.Id);

            Workday_Client workdayClient;
            List<FileContentResult> fileContentList = new List<FileContentResult>();
            foreach (var item in workdayClientList)
            {
                workdayClient = _context.Workdays_Clients

                                        .Include(wc => wc.Facilitator)

                                        .Include(wc => wc.Client)
                                        .ThenInclude(c => c.MTPs)
                                        .ThenInclude(m => m.Goals)
                                        .ThenInclude(g => g.Objetives)

                                        .Include(wc => wc.GroupNote)
                                        .ThenInclude(n => n.Supervisor)
                                        .ThenInclude(s => s.Clinic)

                                        .Include(wc => wc.GroupNote)
                                        .ThenInclude(n => n.GroupNotes_Activities)
                                        .ThenInclude(na => na.Activity)
                                        .ThenInclude(a => a.Theme)

                                        .Include(wc => wc.GroupNote)
                                        .ThenInclude(n => n.GroupNotes_Activities)
                                        .ThenInclude(na => na.Objetive)
                                        .ThenInclude(o => o.Goal)

                                        .Include(wc => wc.Workday)

                                        .FirstOrDefault(wc => (wc.Id == item.Id && wc.GroupNote.Status == NoteStatus.Approved));               

                if (workdayClient != null)
                {
                    if (workdayClient.GroupNote.Supervisor.Clinic.Name == "DAVILA")
                    {
                        Stream stream = _reportHelper.DavilaGroupNoteReportSchema1(workdayClient);
                        fileContentList.Add(File(_reportHelper.ConvertStreamToByteArray(stream), "application/pdf", $"{workdayClient.Client.Name}.pdf"));
                    }
                }
            }

            return this.ZipFile(fileContentList, $"{workday.Date.ToShortDateString()}.zip");
        }

        [Authorize(Roles = "Facilitator, Mannager")]        
        public IActionResult PrintNote(int id)
        {
            Workday_Client workdayClient = _context.Workdays_Clients
                                                    .Include(wc => wc.Facilitator)

                                                    .Include(wc => wc.Client)
                                                    .ThenInclude(c => c.MTPs)
                                                    .ThenInclude(m => m.Goals)
                                                    .ThenInclude(g => g.Objetives)

                                                    .Include(wc => wc.Client)
                                                    .ThenInclude(c => c.Clients_Diagnostics)
                                                    .ThenInclude(cd => cd.Diagnostic)                                              

                                                    .Include(wc => wc.Note)
                                                    .ThenInclude(n => n.Supervisor)
                                                    .ThenInclude(s => s.Clinic)

                                                    .Include(wc => wc.Note)
                                                    .ThenInclude(n => n.Notes_Activities)
                                                    .ThenInclude(na => na.Activity)
                                                    .ThenInclude(a => a.Theme)

                                                    .Include(wc => wc.Note)
                                                    .ThenInclude(n => n.Notes_Activities)
                                                    .ThenInclude(na => na.Objetive)
                                                    .ThenInclude(o => o.Goal)

                                                    .Include(wc => wc.Note)
                                                    .ThenInclude(n => n.Notes_Activities)

                                                    .Include(wc => wc.Workday)
                                                    .FirstOrDefault(wc => (wc.Id == id && wc.Note.Status == NoteStatus.Approved));
            if (workdayClient == null)
            {
                return NotFound();
            }            
            
            if (workdayClient.Note.Supervisor.Clinic.Name == "DAVILA")
            {
                if (workdayClient.Note.Schema == Common.Enums.SchemaType.Schema1)
                {
                    return DavilaNoteReportSchema1(workdayClient);
                }
                if (workdayClient.Note.Schema == Common.Enums.SchemaType.Schema4)
                {
                    Stream stream = _reportHelper.DavilaNoteReportSchema4(workdayClient);
                    return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
                }
            }

            if (workdayClient.Note.Supervisor.Clinic.Name == "LARKIN BEHAVIOR")
            {
                if (workdayClient.Note.Schema == Common.Enums.SchemaType.Schema1)
                {
                    return LarkinNoteReportSchema1(workdayClient);
                }
                if (workdayClient.Note.Schema == Common.Enums.SchemaType.Schema2)
                {
                    return LarkinNoteReportSchema2(workdayClient);
                }
            }

            if (workdayClient.Note.Supervisor.Clinic.Name == "SOL & VIDA")
            {
                return SolAndVidaNoteReportSchema1(workdayClient);
            }

            if (workdayClient.Note.Supervisor.Clinic.Name == "HEALTH & BEAUTY NGB, INC")
            {
                return HealthAndBeautyNoteReportSchema2(workdayClient);
            }

            if (workdayClient.Note.Supervisor.Clinic.Name == "ADVANCED GROUP MEDICAL CENTER")
            {
                return AdvancedGroupMCNoteReportSchema2(workdayClient);
            }

            if (workdayClient.Note.Supervisor.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
            {
                return FloridaSocialHSNoteReportSchema2(workdayClient);
            }

            if (workdayClient.Note.Supervisor.Clinic.Name == "ATLANTIC GROUP MEDICAL CENTER")
            {
                return AtlanticGroupMCNoteReportSchema2(workdayClient);
            }

            if (workdayClient.Note.Supervisor.Clinic.Name == "DEMO CLINIC SCHEMA 1")
            {
                return DemoClinic1NoteReportSchema1(workdayClient);
            }

            if (workdayClient.Note.Supervisor.Clinic.Name == "DEMO CLINIC SCHEMA 2")
            {
                return DemoClinic2NoteReportSchema2(workdayClient);
            }

            return null;
        }

        [Authorize(Roles = "Facilitator, Mannager")]
        public IActionResult PrintIndNote(int id)
        {
            Workday_Client workdayClient = _context.Workdays_Clients

                                                    .Include(wc => wc.Facilitator)

                                                    .Include(wc => wc.Client)
                                                    .ThenInclude(c => c.MTPs)
                                                    .ThenInclude(m => m.Goals)
                                                    .ThenInclude(g => g.Objetives)

                                                    .Include(wc => wc.Client)
                                                    .ThenInclude(c => c.Clients_Diagnostics)
                                                    .ThenInclude(cd => cd.Diagnostic)

                                                    .Include(wc => wc.IndividualNote)
                                                    .ThenInclude(n => n.Supervisor)
                                                    .ThenInclude(s => s.Clinic)

                                                    .Include(wc => wc.IndividualNote)
                                                    .ThenInclude(n => n.Objective)

                                                    .Include(wc => wc.Workday)

                                                    .FirstOrDefault(wc => (wc.Id == id && wc.IndividualNote.Status == NoteStatus.Approved));
            if (workdayClient == null)
            {
                return NotFound();
            }

            if (workdayClient.IndividualNote.Supervisor.Clinic.Name == "DAVILA")
            {
                Stream stream = _reportHelper.DavilaIndNoteReportSchema1(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
                
            }
            
            return null;
        }

        [Authorize(Roles = "Facilitator, Mannager")]
        public IActionResult PrintGroupNote(int id)
        {
            Workday_Client workdayClient = _context.Workdays_Clients

                                                   .Include(wc => wc.Facilitator)

                                                   .Include(wc => wc.Client)
                                                   .ThenInclude(c => c.MTPs)
                                                   .ThenInclude(m => m.Goals)
                                                   .ThenInclude(g => g.Objetives)                                                   

                                                   .Include(wc => wc.GroupNote)
                                                   .ThenInclude(n => n.Supervisor)
                                                   .ThenInclude(s => s.Clinic)

                                                   .Include(wc => wc.GroupNote)
                                                   .ThenInclude(n => n.GroupNotes_Activities)
                                                   .ThenInclude(na => na.Activity)
                                                   .ThenInclude(a => a.Theme)

                                                   .Include(wc => wc.GroupNote)
                                                   .ThenInclude(n => n.GroupNotes_Activities)
                                                   .ThenInclude(na => na.Objetive)
                                                   .ThenInclude(o => o.Goal)

                                                   .Include(wc => wc.Workday)

                                                   .FirstOrDefault(wc => (wc.Id == id && wc.GroupNote.Status == NoteStatus.Approved));
            if (workdayClient == null)
            {
                return NotFound();
            }

            if (workdayClient.GroupNote.Supervisor.Clinic.Name == "DAVILA")
            {
                Stream stream = _reportHelper.DavilaGroupNoteReportSchema1(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }            

            return null;
        }

        #region Davila        
        private IActionResult DavilaNoteReportSchema1(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteDAVILA0");
            Dictionary<string, string> parameters = new Dictionary<string, string>();            
            
            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal = string.Empty;
            var goal_text = string.Empty;
            var num_of_obj = string.Empty;
            var obj_text = string.Empty;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                        goal_text = item.Objetive.Goal.Name;
                        num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                        obj_text = item.Objetive.Description;
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();
            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("num_of_goal", num_of_goal);
            parameters.Add("goal_text", goal_text);
            parameters.Add("num_of_obj", num_of_obj);
            parameters.Add("obj_text", obj_text);
            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf"/*,
                        $"NoteOf_{workdayClient.Client.Name}_{workdayClient.Workday.Date.ToShortDateString()}.pdf"*/);
        }

        private FileContentResult DavilaNoteReportFCRSchema1(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteDAVILA0");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal = string.Empty;
            var goal_text = string.Empty;
            var num_of_obj = string.Empty;
            var obj_text = string.Empty;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                        goal_text = item.Objetive.Goal.Name;
                        num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                        obj_text = item.Objetive.Description;
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();
            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("num_of_goal", num_of_goal);
            parameters.Add("goal_text", goal_text);
            parameters.Add("num_of_obj", num_of_obj);
            parameters.Add("obj_text", obj_text);
            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf", $"{workdayClient.Client.Name}.pdf");
        }
        #endregion

        #region Larkin
        private IActionResult LarkinNoteReportSchema1(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);            
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteLARKINBEHAVIOR0");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal = string.Empty;
            var goal_text = string.Empty;
            var num_of_obj = string.Empty;
            var obj_text = string.Empty;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                        goal_text = item.Objetive.Goal.Name;
                        num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                        obj_text = item.Objetive.Description;
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();
            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("num_of_goal", num_of_goal);
            parameters.Add("goal_text", goal_text);
            parameters.Add("num_of_obj", num_of_obj);
            parameters.Add("obj_text", obj_text);
            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf");
        }

        private IActionResult LarkinNoteReportSchema2(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);            
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteLARKINBEHAVIOR1");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal1 = string.Empty;
            var goal_text1 = string.Empty;
            bool goal1 = false;
            var num_of_goal2 = string.Empty;
            var goal_text2 = string.Empty;
            bool goal2 = false;
            var num_of_goal3 = string.Empty;
            var goal_text3 = string.Empty;
            bool goal3 = false;
            var num_of_goal4 = string.Empty;
            var goal_text4 = string.Empty;
            bool goal4 = false;
            var num_of_goal5 = string.Empty;
            var goal_text5 = string.Empty;
            bool goal5 = false;
            var goal_obj_activity1 = string.Empty;
            var goal_obj_activity2 = string.Empty;
            var goal_obj_activity3 = string.Empty;
            var goal_obj_activity4 = string.Empty;

            MTPEntity mtp;
            if (workdayClient.Note.MTPId == null)   //la nota no tiene mtp relacionado, entonces se usa el primero que esté
                mtp = workdayClient.Client.MTPs.FirstOrDefault();
            else                                    //la nota tiene mtp relacionado    
                mtp = _context.MTPs.FirstOrDefault(m => m.Id == workdayClient.Note.MTPId);

            foreach (GoalEntity item in mtp.Goals.OrderBy(g => g.Number))
            {
                if (i == 0)
                {
                    num_of_goal1 = $"GOAL #{item.Number}:";
                    goal_text1 = item.Name;
                }
                if (i == 1)
                {
                    num_of_goal2 = $"GOAL #{item.Number}:";
                    goal_text2 = item.Name;
                }
                if (i == 2)
                {
                    num_of_goal3 = $"GOAL #{item.Number}:";
                    goal_text3 = item.Name;
                }
                if (i == 3)
                {
                    num_of_goal4 = $"GOAL #{item.Number}:";
                    goal_text4 = item.Name;
                }
                if (i == 4)
                {
                    num_of_goal5 = $"GOAL #{item.Number}:";
                    goal_text5 = item.Name;
                }
                i = ++i;
            }

            i = 0;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity1 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity2 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity3 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity4 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("num_of_goal1", num_of_goal1);
            parameters.Add("goal_text1", goal_text1);
            parameters.Add("goal1", goal1.ToString());
            parameters.Add("goal_obj_activity1", goal_obj_activity1);
            parameters.Add("num_of_goal2", num_of_goal2);
            parameters.Add("goal_text2", goal_text2);
            parameters.Add("goal2", goal2.ToString());
            parameters.Add("goal_obj_activity2", goal_obj_activity2);
            parameters.Add("num_of_goal3", num_of_goal3);
            parameters.Add("goal_text3", goal_text3);
            parameters.Add("goal3", goal3.ToString());
            parameters.Add("goal_obj_activity3", goal_obj_activity3);
            parameters.Add("num_of_goal4", num_of_goal4);
            parameters.Add("goal_text4", goal_text4);
            parameters.Add("goal4", goal4.ToString());
            parameters.Add("goal_obj_activity4", goal_obj_activity4);
            parameters.Add("num_of_goal5", num_of_goal5);
            parameters.Add("goal_text5", goal_text5);
            parameters.Add("goal5", goal5.ToString());
            parameters.Add("setting", setting);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf");
        }

        private FileContentResult LarkinNoteReportFCRSchema1(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteLARKINBEHAVIOR0");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal = string.Empty;
            var goal_text = string.Empty;
            var num_of_obj = string.Empty;
            var obj_text = string.Empty;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                        goal_text = item.Objetive.Goal.Name;
                        num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                        obj_text = item.Objetive.Description;
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();
            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("num_of_goal", num_of_goal);
            parameters.Add("goal_text", goal_text);
            parameters.Add("num_of_obj", num_of_obj);
            parameters.Add("obj_text", obj_text);
            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf", $"{workdayClient.Client.Name}.pdf");
        }

        private FileContentResult LarkinNoteReportFCRSchema2(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteLARKINBEHAVIOR1");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal1 = string.Empty;
            var goal_text1 = string.Empty;
            bool goal1 = false;
            var num_of_goal2 = string.Empty;
            var goal_text2 = string.Empty;
            bool goal2 = false;
            var num_of_goal3 = string.Empty;
            var goal_text3 = string.Empty;
            bool goal3 = false;
            var num_of_goal4 = string.Empty;
            var goal_text4 = string.Empty;
            bool goal4 = false;
            var num_of_goal5 = string.Empty;
            var goal_text5 = string.Empty;
            bool goal5 = false;
            var goal_obj_activity1 = string.Empty;
            var goal_obj_activity2 = string.Empty;
            var goal_obj_activity3 = string.Empty;
            var goal_obj_activity4 = string.Empty;

            MTPEntity mtp;
            if (workdayClient.Note.MTPId == null)   //la nota no tiene mtp relacionado, entonces se usa el primero que esté
                mtp = workdayClient.Client.MTPs.FirstOrDefault();
            else                                    //la nota tiene mtp relacionado    
                mtp = _context.MTPs.FirstOrDefault(m => m.Id == workdayClient.Note.MTPId);

            foreach (GoalEntity item in mtp.Goals.OrderBy(g => g.Number))
            {
                if (i == 0)
                {
                    num_of_goal1 = $"GOAL #{item.Number}:";
                    goal_text1 = item.Name;
                }
                if (i == 1)
                {
                    num_of_goal2 = $"GOAL #{item.Number}:";
                    goal_text2 = item.Name;
                }
                if (i == 2)
                {
                    num_of_goal3 = $"GOAL #{item.Number}:";
                    goal_text3 = item.Name;
                }
                if (i == 3)
                {
                    num_of_goal4 = $"GOAL #{item.Number}:";
                    goal_text4 = item.Name;
                }
                if (i == 4)
                {
                    num_of_goal5 = $"GOAL #{item.Number}:";
                    goal_text5 = item.Name;
                }
                i = ++i;
            }

            i = 0;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity1 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity2 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity3 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity4 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("num_of_goal1", num_of_goal1);
            parameters.Add("goal_text1", goal_text1);
            parameters.Add("goal1", goal1.ToString());
            parameters.Add("goal_obj_activity1", goal_obj_activity1);
            parameters.Add("num_of_goal2", num_of_goal2);
            parameters.Add("goal_text2", goal_text2);
            parameters.Add("goal2", goal2.ToString());
            parameters.Add("goal_obj_activity2", goal_obj_activity2);
            parameters.Add("num_of_goal3", num_of_goal3);
            parameters.Add("goal_text3", goal_text3);
            parameters.Add("goal3", goal3.ToString());
            parameters.Add("goal_obj_activity3", goal_obj_activity3);
            parameters.Add("num_of_goal4", num_of_goal4);
            parameters.Add("goal_text4", goal_text4);
            parameters.Add("goal4", goal4.ToString());
            parameters.Add("goal_obj_activity4", goal_obj_activity4);
            parameters.Add("num_of_goal5", num_of_goal5);
            parameters.Add("goal_text5", goal_text5);
            parameters.Add("goal5", goal5.ToString());
            parameters.Add("setting", setting);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf", $"{workdayClient.Client.Name}.pdf");
        }
        #endregion

        #region SolAndVida        
        private IActionResult SolAndVidaNoteReportSchema1(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteSolAndVida0");
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            
            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal = string.Empty;
            var goal_text = string.Empty;
            var num_of_obj = string.Empty;
            var obj_text = string.Empty;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                        goal_text = item.Objetive.Goal.Name;
                        num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                        obj_text = item.Objetive.Description;
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();
            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("num_of_goal", num_of_goal);
            parameters.Add("goal_text", goal_text);
            parameters.Add("num_of_obj", num_of_obj);
            parameters.Add("obj_text", obj_text);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("setting", setting);
            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf");
        }

        private FileContentResult SolAndVidaNoteReportFCRSchema1(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteSolAndVida0");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal = string.Empty;
            var goal_text = string.Empty;
            var num_of_obj = string.Empty;
            var obj_text = string.Empty;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                        goal_text = item.Objetive.Goal.Name;
                        num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                        obj_text = item.Objetive.Description;
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();
            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("num_of_goal", num_of_goal);
            parameters.Add("goal_text", goal_text);
            parameters.Add("num_of_obj", num_of_obj);
            parameters.Add("obj_text", obj_text);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("setting", setting);
            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf", $"{workdayClient.Client.Name}.pdf");
        }
        #endregion

        #region HealthAndBeauty        
        private IActionResult HealthAndBeautyNoteReportSchema2(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteHealthAndBeauty1");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal1 = string.Empty;
            var goal_text1 = string.Empty;
            bool goal1 = false;
            var num_of_goal2 = string.Empty;
            var goal_text2 = string.Empty;
            bool goal2 = false;
            var num_of_goal3 = string.Empty;
            var goal_text3 = string.Empty;
            bool goal3 = false;
            var num_of_goal4 = string.Empty;
            var goal_text4 = string.Empty;
            bool goal4 = false;
            var num_of_goal5 = string.Empty;
            var goal_text5 = string.Empty;
            bool goal5 = false;
            var goal_obj_activity1 = string.Empty;
            var goal_obj_activity2 = string.Empty;
            var goal_obj_activity3 = string.Empty;
            var goal_obj_activity4 = string.Empty;

            MTPEntity mtp;
            if (workdayClient.Note.MTPId == null)   //la nota no tiene mtp relacionado, entonces se usa el primero que esté
                mtp = workdayClient.Client.MTPs.FirstOrDefault();
            else                                    //la nota tiene mtp relacionado    
                mtp = _context.MTPs.FirstOrDefault(m => m.Id == workdayClient.Note.MTPId);

            foreach (GoalEntity item in mtp.Goals.OrderBy(g => g.Number))
            {
                if (i == 0)
                {
                    num_of_goal1 = $"GOAL #{item.Number}:";
                    goal_text1 = item.Name;
                }
                if (i == 1)
                {
                    num_of_goal2 = $"GOAL #{item.Number}:";
                    goal_text2 = item.Name;
                }
                if (i == 2)
                {
                    num_of_goal3 = $"GOAL #{item.Number}:";
                    goal_text3 = item.Name;
                }
                if (i == 3)
                {
                    num_of_goal4 = $"GOAL #{item.Number}:";
                    goal_text4 = item.Name;
                }
                if (i == 4)
                {
                    num_of_goal5 = $"GOAL #{item.Number}:";
                    goal_text5 = item.Name;
                }
                i = ++i;
            }

            i = 0;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity1 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity2 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity3 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity4 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("num_of_goal1", num_of_goal1);
            parameters.Add("goal_text1", goal_text1);
            parameters.Add("goal1", goal1.ToString());
            parameters.Add("goal_obj_activity1", goal_obj_activity1);
            parameters.Add("num_of_goal2", num_of_goal2);
            parameters.Add("goal_text2", goal_text2);
            parameters.Add("goal2", goal2.ToString());
            parameters.Add("goal_obj_activity2", goal_obj_activity2);
            parameters.Add("num_of_goal3", num_of_goal3);
            parameters.Add("goal_text3", goal_text3);
            parameters.Add("goal3", goal3.ToString());
            parameters.Add("goal_obj_activity3", goal_obj_activity3);
            parameters.Add("num_of_goal4", num_of_goal4);
            parameters.Add("goal_text4", goal_text4);
            parameters.Add("goal4", goal4.ToString());
            parameters.Add("goal_obj_activity4", goal_obj_activity4);
            parameters.Add("num_of_goal5", num_of_goal5);
            parameters.Add("goal_text5", goal_text5);
            parameters.Add("goal5", goal5.ToString());
            parameters.Add("setting", setting);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf");
        }

        private FileContentResult HealthAndBeautyNoteReportFCRSchema2(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteHealthAndBeauty1");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal1 = string.Empty;
            var goal_text1 = string.Empty;
            bool goal1 = false;
            var num_of_goal2 = string.Empty;
            var goal_text2 = string.Empty;
            bool goal2 = false;
            var num_of_goal3 = string.Empty;
            var goal_text3 = string.Empty;
            bool goal3 = false;
            var num_of_goal4 = string.Empty;
            var goal_text4 = string.Empty;
            bool goal4 = false;
            var num_of_goal5 = string.Empty;
            var goal_text5 = string.Empty;
            bool goal5 = false;
            var goal_obj_activity1 = string.Empty;
            var goal_obj_activity2 = string.Empty;
            var goal_obj_activity3 = string.Empty;
            var goal_obj_activity4 = string.Empty;

            MTPEntity mtp;
            if (workdayClient.Note.MTPId == null)   //la nota no tiene mtp relacionado, entonces se usa el primero que esté
                mtp = workdayClient.Client.MTPs.FirstOrDefault();
            else                                    //la nota tiene mtp relacionado    
                mtp = _context.MTPs.FirstOrDefault(m => m.Id == workdayClient.Note.MTPId);

            foreach (GoalEntity item in mtp.Goals.OrderBy(g => g.Number))
            {
                if (i == 0)
                {
                    num_of_goal1 = $"GOAL #{item.Number}:";
                    goal_text1 = item.Name;
                }
                if (i == 1)
                {
                    num_of_goal2 = $"GOAL #{item.Number}:";
                    goal_text2 = item.Name;
                }
                if (i == 2)
                {
                    num_of_goal3 = $"GOAL #{item.Number}:";
                    goal_text3 = item.Name;
                }
                if (i == 3)
                {
                    num_of_goal4 = $"GOAL #{item.Number}:";
                    goal_text4 = item.Name;
                }
                if (i == 4)
                {
                    num_of_goal5 = $"GOAL #{item.Number}:";
                    goal_text5 = item.Name;
                }
                i = ++i;
            }

            i = 0;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity1 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity2 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity3 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity4 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("num_of_goal1", num_of_goal1);
            parameters.Add("goal_text1", goal_text1);
            parameters.Add("goal1", goal1.ToString());
            parameters.Add("goal_obj_activity1", goal_obj_activity1);
            parameters.Add("num_of_goal2", num_of_goal2);
            parameters.Add("goal_text2", goal_text2);
            parameters.Add("goal2", goal2.ToString());
            parameters.Add("goal_obj_activity2", goal_obj_activity2);
            parameters.Add("num_of_goal3", num_of_goal3);
            parameters.Add("goal_text3", goal_text3);
            parameters.Add("goal3", goal3.ToString());
            parameters.Add("goal_obj_activity3", goal_obj_activity3);
            parameters.Add("num_of_goal4", num_of_goal4);
            parameters.Add("goal_text4", goal_text4);
            parameters.Add("goal4", goal4.ToString());
            parameters.Add("goal_obj_activity4", goal_obj_activity4);
            parameters.Add("num_of_goal5", num_of_goal5);
            parameters.Add("goal_text5", goal_text5);
            parameters.Add("goal5", goal5.ToString());
            parameters.Add("setting", setting);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf", $"{workdayClient.Client.Name}.pdf");
        }
        #endregion

        #region Advanced      
        private IActionResult AdvancedGroupMCNoteReportSchema2(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteAdvancedGroupMC1");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal1 = string.Empty;
            var goal_text1 = string.Empty;
            bool goal1 = false;
            var num_of_goal2 = string.Empty;
            var goal_text2 = string.Empty;
            bool goal2 = false;
            var num_of_goal3 = string.Empty;
            var goal_text3 = string.Empty;
            bool goal3 = false;
            var num_of_goal4 = string.Empty;
            var goal_text4 = string.Empty;
            bool goal4 = false;
            var num_of_goal5 = string.Empty;
            var goal_text5 = string.Empty;
            bool goal5 = false;
            var goal_obj_activity1 = string.Empty;
            var goal_obj_activity2 = string.Empty;
            var goal_obj_activity3 = string.Empty;
            var goal_obj_activity4 = string.Empty;

            MTPEntity mtp;
            if (workdayClient.Note.MTPId == null)   //la nota no tiene mtp relacionado, entonces se usa el primero que esté
                mtp = workdayClient.Client.MTPs.FirstOrDefault();
            else                                    //la nota tiene mtp relacionado    
                mtp = _context.MTPs.FirstOrDefault(m => m.Id == workdayClient.Note.MTPId);

            foreach (GoalEntity item in mtp.Goals.OrderBy(g => g.Number))
            {
                if (i == 0)
                {
                    num_of_goal1 = $"GOAL #{item.Number}:";
                    goal_text1 = item.Name;
                }
                if (i == 1)
                {
                    num_of_goal2 = $"GOAL #{item.Number}:";
                    goal_text2 = item.Name;
                }
                if (i == 2)
                {
                    num_of_goal3 = $"GOAL #{item.Number}:";
                    goal_text3 = item.Name;
                }
                if (i == 3)
                {
                    num_of_goal4 = $"GOAL #{item.Number}:";
                    goal_text4 = item.Name;
                }
                if (i == 4)
                {
                    num_of_goal5 = $"GOAL #{item.Number}:";
                    goal_text5 = item.Name;
                }
                i = ++i;
            }

            i = 0;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity1 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity2 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity3 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity4 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("num_of_goal1", num_of_goal1);
            parameters.Add("goal_text1", goal_text1);
            parameters.Add("goal1", goal1.ToString());
            parameters.Add("goal_obj_activity1", goal_obj_activity1);
            parameters.Add("num_of_goal2", num_of_goal2);
            parameters.Add("goal_text2", goal_text2);
            parameters.Add("goal2", goal2.ToString());
            parameters.Add("goal_obj_activity2", goal_obj_activity2);
            parameters.Add("num_of_goal3", num_of_goal3);
            parameters.Add("goal_text3", goal_text3);
            parameters.Add("goal3", goal3.ToString());
            parameters.Add("goal_obj_activity3", goal_obj_activity3);
            parameters.Add("num_of_goal4", num_of_goal4);
            parameters.Add("goal_text4", goal_text4);
            parameters.Add("goal4", goal4.ToString());
            parameters.Add("goal_obj_activity4", goal_obj_activity4);
            parameters.Add("num_of_goal5", num_of_goal5);
            parameters.Add("goal_text5", goal_text5);
            parameters.Add("goal5", goal5.ToString());
            parameters.Add("setting", setting);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf");
        }

        private FileContentResult AdvancedGroupMCNoteReportFCRSchema2(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteAdvancedGroupMC1");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal1 = string.Empty;
            var goal_text1 = string.Empty;
            bool goal1 = false;
            var num_of_goal2 = string.Empty;
            var goal_text2 = string.Empty;
            bool goal2 = false;
            var num_of_goal3 = string.Empty;
            var goal_text3 = string.Empty;
            bool goal3 = false;
            var num_of_goal4 = string.Empty;
            var goal_text4 = string.Empty;
            bool goal4 = false;
            var num_of_goal5 = string.Empty;
            var goal_text5 = string.Empty;
            bool goal5 = false;
            var goal_obj_activity1 = string.Empty;
            var goal_obj_activity2 = string.Empty;
            var goal_obj_activity3 = string.Empty;
            var goal_obj_activity4 = string.Empty;

            MTPEntity mtp;
            if (workdayClient.Note.MTPId == null)   //la nota no tiene mtp relacionado, entonces se usa el primero que esté
                mtp = workdayClient.Client.MTPs.FirstOrDefault();
            else                                    //la nota tiene mtp relacionado    
                mtp = _context.MTPs.FirstOrDefault(m => m.Id == workdayClient.Note.MTPId);

            foreach (GoalEntity item in mtp.Goals.OrderBy(g => g.Number))
            {
                if (i == 0)
                {
                    num_of_goal1 = $"GOAL #{item.Number}:";
                    goal_text1 = item.Name;
                }
                if (i == 1)
                {
                    num_of_goal2 = $"GOAL #{item.Number}:";
                    goal_text2 = item.Name;
                }
                if (i == 2)
                {
                    num_of_goal3 = $"GOAL #{item.Number}:";
                    goal_text3 = item.Name;
                }
                if (i == 3)
                {
                    num_of_goal4 = $"GOAL #{item.Number}:";
                    goal_text4 = item.Name;
                }
                if (i == 4)
                {
                    num_of_goal5 = $"GOAL #{item.Number}:";
                    goal_text5 = item.Name;
                }
                i = ++i;
            }

            i = 0;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity1 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity2 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity3 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity4 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("num_of_goal1", num_of_goal1);
            parameters.Add("goal_text1", goal_text1);
            parameters.Add("goal1", goal1.ToString());
            parameters.Add("goal_obj_activity1", goal_obj_activity1);
            parameters.Add("num_of_goal2", num_of_goal2);
            parameters.Add("goal_text2", goal_text2);
            parameters.Add("goal2", goal2.ToString());
            parameters.Add("goal_obj_activity2", goal_obj_activity2);
            parameters.Add("num_of_goal3", num_of_goal3);
            parameters.Add("goal_text3", goal_text3);
            parameters.Add("goal3", goal3.ToString());
            parameters.Add("goal_obj_activity3", goal_obj_activity3);
            parameters.Add("num_of_goal4", num_of_goal4);
            parameters.Add("goal_text4", goal_text4);
            parameters.Add("goal4", goal4.ToString());
            parameters.Add("goal_obj_activity4", goal_obj_activity4);
            parameters.Add("num_of_goal5", num_of_goal5);
            parameters.Add("goal_text5", goal_text5);
            parameters.Add("goal5", goal5.ToString());
            parameters.Add("setting", setting);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf", $"{workdayClient.Client.Name}.pdf");
        }
        #endregion

        #region Florida Social Health Solution        
        private IActionResult FloridaSocialHSNoteReportSchema2(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteFloridaSocialHS1");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal1 = string.Empty;
            var goal_text1 = string.Empty;
            bool goal1 = false;
            var num_of_goal2 = string.Empty;
            var goal_text2 = string.Empty;
            bool goal2 = false;
            var num_of_goal3 = string.Empty;
            var goal_text3 = string.Empty;
            bool goal3 = false;
            var num_of_goal4 = string.Empty;
            var goal_text4 = string.Empty;
            bool goal4 = false;
            var num_of_goal5 = string.Empty;
            var goal_text5 = string.Empty;
            bool goal5 = false;
            var goal_obj_activity1 = string.Empty;
            var goal_obj_activity2 = string.Empty;
            var goal_obj_activity3 = string.Empty;
            var goal_obj_activity4 = string.Empty;

            MTPEntity mtp;
            if (workdayClient.Note.MTPId == null)   //la nota no tiene mtp relacionado, entonces se usa el primero que esté
                mtp = workdayClient.Client.MTPs.FirstOrDefault();
            else                                    //la nota tiene mtp relacionado    
                mtp = _context.MTPs.FirstOrDefault(m => m.Id == workdayClient.Note.MTPId);

            foreach (GoalEntity item in mtp.Goals.OrderBy(g => g.Number))
            {
                if (i == 0)
                {
                    num_of_goal1 = $"GOAL #{item.Number}:";
                    goal_text1 = item.Name;
                }
                if (i == 1)
                {
                    num_of_goal2 = $"GOAL #{item.Number}:";
                    goal_text2 = item.Name;
                }
                if (i == 2)
                {
                    num_of_goal3 = $"GOAL #{item.Number}:";
                    goal_text3 = item.Name;
                }
                if (i == 3)
                {
                    num_of_goal4 = $"GOAL #{item.Number}:";
                    goal_text4 = item.Name;
                }
                if (i == 4)
                {
                    num_of_goal5 = $"GOAL #{item.Number}:";
                    goal_text5 = item.Name;
                }
                i = ++i;
            }

            i = 0;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity1 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity2 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity3 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity4 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("num_of_goal1", num_of_goal1);
            parameters.Add("goal_text1", goal_text1);
            parameters.Add("goal1", goal1.ToString());
            parameters.Add("goal_obj_activity1", goal_obj_activity1);
            parameters.Add("num_of_goal2", num_of_goal2);
            parameters.Add("goal_text2", goal_text2);
            parameters.Add("goal2", goal2.ToString());
            parameters.Add("goal_obj_activity2", goal_obj_activity2);
            parameters.Add("num_of_goal3", num_of_goal3);
            parameters.Add("goal_text3", goal_text3);
            parameters.Add("goal3", goal3.ToString());
            parameters.Add("goal_obj_activity3", goal_obj_activity3);
            parameters.Add("num_of_goal4", num_of_goal4);
            parameters.Add("goal_text4", goal_text4);
            parameters.Add("goal4", goal4.ToString());
            parameters.Add("goal_obj_activity4", goal_obj_activity4);
            parameters.Add("num_of_goal5", num_of_goal5);
            parameters.Add("goal_text5", goal_text5);
            parameters.Add("goal5", goal5.ToString());
            parameters.Add("setting", setting);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf");
        }

        private FileContentResult FloridaSocialHSNoteReportFCRSchema2(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteFloridaSocialHS1");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal1 = string.Empty;
            var goal_text1 = string.Empty;
            bool goal1 = false;
            var num_of_goal2 = string.Empty;
            var goal_text2 = string.Empty;
            bool goal2 = false;
            var num_of_goal3 = string.Empty;
            var goal_text3 = string.Empty;
            bool goal3 = false;
            var num_of_goal4 = string.Empty;
            var goal_text4 = string.Empty;
            bool goal4 = false;
            var num_of_goal5 = string.Empty;
            var goal_text5 = string.Empty;
            bool goal5 = false;
            var goal_obj_activity1 = string.Empty;
            var goal_obj_activity2 = string.Empty;
            var goal_obj_activity3 = string.Empty;
            var goal_obj_activity4 = string.Empty;

            MTPEntity mtp;
            if (workdayClient.Note.MTPId == null)   //la nota no tiene mtp relacionado, entonces se usa el primero que esté
                mtp = workdayClient.Client.MTPs.FirstOrDefault();
            else                                    //la nota tiene mtp relacionado    
                mtp = _context.MTPs.FirstOrDefault(m => m.Id == workdayClient.Note.MTPId);

            foreach (GoalEntity item in mtp.Goals.OrderBy(g => g.Number))
            {
                if (i == 0)
                {
                    num_of_goal1 = $"GOAL #{item.Number}:";
                    goal_text1 = item.Name;
                }
                if (i == 1)
                {
                    num_of_goal2 = $"GOAL #{item.Number}:";
                    goal_text2 = item.Name;
                }
                if (i == 2)
                {
                    num_of_goal3 = $"GOAL #{item.Number}:";
                    goal_text3 = item.Name;
                }
                if (i == 3)
                {
                    num_of_goal4 = $"GOAL #{item.Number}:";
                    goal_text4 = item.Name;
                }
                if (i == 4)
                {
                    num_of_goal5 = $"GOAL #{item.Number}:";
                    goal_text5 = item.Name;
                }
                i = ++i;
            }

            i = 0;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity1 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity2 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity3 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity4 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("num_of_goal1", num_of_goal1);
            parameters.Add("goal_text1", goal_text1);
            parameters.Add("goal1", goal1.ToString());
            parameters.Add("goal_obj_activity1", goal_obj_activity1);
            parameters.Add("num_of_goal2", num_of_goal2);
            parameters.Add("goal_text2", goal_text2);
            parameters.Add("goal2", goal2.ToString());
            parameters.Add("goal_obj_activity2", goal_obj_activity2);
            parameters.Add("num_of_goal3", num_of_goal3);
            parameters.Add("goal_text3", goal_text3);
            parameters.Add("goal3", goal3.ToString());
            parameters.Add("goal_obj_activity3", goal_obj_activity3);
            parameters.Add("num_of_goal4", num_of_goal4);
            parameters.Add("goal_text4", goal_text4);
            parameters.Add("goal4", goal4.ToString());
            parameters.Add("goal_obj_activity4", goal_obj_activity4);
            parameters.Add("num_of_goal5", num_of_goal5);
            parameters.Add("goal_text5", goal_text5);
            parameters.Add("goal5", goal5.ToString());
            parameters.Add("setting", setting);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf", $"{workdayClient.Client.Name}.pdf");
        }
        #endregion

        #region Atlantic
        private IActionResult AtlanticGroupMCNoteReportSchema2(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteAtlanticGroupMC1");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal1 = string.Empty;
            var goal_text1 = string.Empty;
            bool goal1 = false;
            var num_of_goal2 = string.Empty;
            var goal_text2 = string.Empty;
            bool goal2 = false;
            var num_of_goal3 = string.Empty;
            var goal_text3 = string.Empty;
            bool goal3 = false;
            var num_of_goal4 = string.Empty;
            var goal_text4 = string.Empty;
            bool goal4 = false;
            var num_of_goal5 = string.Empty;
            var goal_text5 = string.Empty;
            bool goal5 = false;
            var goal_obj_activity1 = string.Empty;
            var goal_obj_activity2 = string.Empty;
            var goal_obj_activity3 = string.Empty;
            var goal_obj_activity4 = string.Empty;

            MTPEntity mtp;
            if (workdayClient.Note.MTPId == null)   //la nota no tiene mtp relacionado, entonces se usa el primero que esté
                mtp = workdayClient.Client.MTPs.FirstOrDefault();
            else                                    //la nota tiene mtp relacionado    
                mtp = _context.MTPs.FirstOrDefault(m => m.Id == workdayClient.Note.MTPId);

            foreach (GoalEntity item in mtp.Goals.OrderBy(g => g.Number))
            {
                if (i == 0)
                {
                    num_of_goal1 = $"GOAL #{item.Number}:";
                    goal_text1 = item.Name;
                }
                if (i == 1)
                {
                    num_of_goal2 = $"GOAL #{item.Number}:";
                    goal_text2 = item.Name;
                }
                if (i == 2)
                {
                    num_of_goal3 = $"GOAL #{item.Number}:";
                    goal_text3 = item.Name;
                }
                if (i == 3)
                {
                    num_of_goal4 = $"GOAL #{item.Number}:";
                    goal_text4 = item.Name;
                }
                if (i == 4)
                {
                    num_of_goal5 = $"GOAL #{item.Number}:";
                    goal_text5 = item.Name;
                }
                i = ++i;
            }

            i = 0;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity1 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity2 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity3 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity4 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("num_of_goal1", num_of_goal1);
            parameters.Add("goal_text1", goal_text1);
            parameters.Add("goal1", goal1.ToString());
            parameters.Add("goal_obj_activity1", goal_obj_activity1);
            parameters.Add("num_of_goal2", num_of_goal2);
            parameters.Add("goal_text2", goal_text2);
            parameters.Add("goal2", goal2.ToString());
            parameters.Add("goal_obj_activity2", goal_obj_activity2);
            parameters.Add("num_of_goal3", num_of_goal3);
            parameters.Add("goal_text3", goal_text3);
            parameters.Add("goal3", goal3.ToString());
            parameters.Add("goal_obj_activity3", goal_obj_activity3);
            parameters.Add("num_of_goal4", num_of_goal4);
            parameters.Add("goal_text4", goal_text4);
            parameters.Add("goal4", goal4.ToString());
            parameters.Add("goal_obj_activity4", goal_obj_activity4);
            parameters.Add("num_of_goal5", num_of_goal5);
            parameters.Add("goal_text5", goal_text5);
            parameters.Add("goal5", goal5.ToString());
            parameters.Add("setting", setting);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf");
        }

        private FileContentResult AtlanticGroupMCNoteReportFCRSchema2(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteAtlanticGroupMC1");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal1 = string.Empty;
            var goal_text1 = string.Empty;
            bool goal1 = false;
            var num_of_goal2 = string.Empty;
            var goal_text2 = string.Empty;
            bool goal2 = false;
            var num_of_goal3 = string.Empty;
            var goal_text3 = string.Empty;
            bool goal3 = false;
            var num_of_goal4 = string.Empty;
            var goal_text4 = string.Empty;
            bool goal4 = false;
            var num_of_goal5 = string.Empty;
            var goal_text5 = string.Empty;
            bool goal5 = false;
            var goal_obj_activity1 = string.Empty;
            var goal_obj_activity2 = string.Empty;
            var goal_obj_activity3 = string.Empty;
            var goal_obj_activity4 = string.Empty;

            MTPEntity mtp;
            if (workdayClient.Note.MTPId == null)   //la nota no tiene mtp relacionado, entonces se usa el primero que esté
                mtp = workdayClient.Client.MTPs.FirstOrDefault();
            else                                    //la nota tiene mtp relacionado    
                mtp = _context.MTPs.FirstOrDefault(m => m.Id == workdayClient.Note.MTPId);

            foreach (GoalEntity item in mtp.Goals.OrderBy(g => g.Number))
            {
                if (i == 0)
                {
                    num_of_goal1 = $"GOAL #{item.Number}:";
                    goal_text1 = item.Name;
                }
                if (i == 1)
                {
                    num_of_goal2 = $"GOAL #{item.Number}:";
                    goal_text2 = item.Name;
                }
                if (i == 2)
                {
                    num_of_goal3 = $"GOAL #{item.Number}:";
                    goal_text3 = item.Name;
                }
                if (i == 3)
                {
                    num_of_goal4 = $"GOAL #{item.Number}:";
                    goal_text4 = item.Name;
                }
                if (i == 4)
                {
                    num_of_goal5 = $"GOAL #{item.Number}:";
                    goal_text5 = item.Name;
                }
                i = ++i;
            }

            i = 0;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity1 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity2 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity3 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity4 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("num_of_goal1", num_of_goal1);
            parameters.Add("goal_text1", goal_text1);
            parameters.Add("goal1", goal1.ToString());
            parameters.Add("goal_obj_activity1", goal_obj_activity1);
            parameters.Add("num_of_goal2", num_of_goal2);
            parameters.Add("goal_text2", goal_text2);
            parameters.Add("goal2", goal2.ToString());
            parameters.Add("goal_obj_activity2", goal_obj_activity2);
            parameters.Add("num_of_goal3", num_of_goal3);
            parameters.Add("goal_text3", goal_text3);
            parameters.Add("goal3", goal3.ToString());
            parameters.Add("goal_obj_activity3", goal_obj_activity3);
            parameters.Add("num_of_goal4", num_of_goal4);
            parameters.Add("goal_text4", goal_text4);
            parameters.Add("goal4", goal4.ToString());
            parameters.Add("goal_obj_activity4", goal_obj_activity4);
            parameters.Add("num_of_goal5", num_of_goal5);
            parameters.Add("goal_text5", goal_text5);
            parameters.Add("goal5", goal5.ToString());
            parameters.Add("setting", setting);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf", $"{workdayClient.Client.Name}.pdf");
        }
        #endregion

        #region Demo
        private IActionResult DemoClinic1NoteReportSchema1(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteDemoClinic0");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal = string.Empty;
            var goal_text = string.Empty;
            var num_of_obj = string.Empty;
            var obj_text = string.Empty;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                        goal_text = item.Objetive.Goal.Name;
                        num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                        obj_text = item.Objetive.Description;
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();
            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("num_of_goal", num_of_goal);
            parameters.Add("goal_text", goal_text);
            parameters.Add("num_of_obj", num_of_obj);
            parameters.Add("obj_text", obj_text);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("setting", setting);
            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf");
        }

        private FileContentResult DemoClinic1NoteReportFCRSchema1(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteDemoClinic0");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal = string.Empty;
            var goal_text = string.Empty;
            var num_of_obj = string.Empty;
            var obj_text = string.Empty;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                        goal_text = item.Objetive.Goal.Name;
                        num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                        obj_text = item.Objetive.Description;
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (num_of_goal == string.Empty)
                    {
                        if (item.Objetive != null)
                        {
                            num_of_goal = $"GOAL #{item.Objetive.Goal.Number}:";
                            goal_text = item.Objetive.Goal.Name;
                            num_of_obj = $"OBJ {item.Objetive.Objetive}:";
                            obj_text = item.Objetive.Description;
                        }
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();
            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("num_of_goal", num_of_goal);
            parameters.Add("goal_text", goal_text);
            parameters.Add("num_of_obj", num_of_obj);
            parameters.Add("obj_text", obj_text);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("setting", setting);
            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf", $"{workdayClient.Client.Name}.pdf");
        }

        private IActionResult DemoClinic2NoteReportSchema2(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteDemoClinic1");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal1 = string.Empty;
            var goal_text1 = string.Empty;
            bool goal1 = false;
            var num_of_goal2 = string.Empty;
            var goal_text2 = string.Empty;
            bool goal2 = false;
            var num_of_goal3 = string.Empty;
            var goal_text3 = string.Empty;
            bool goal3 = false;
            var num_of_goal4 = string.Empty;
            var goal_text4 = string.Empty;
            bool goal4 = false;
            var num_of_goal5 = string.Empty;
            var goal_text5 = string.Empty;
            bool goal5 = false;
            var goal_obj_activity1 = string.Empty;
            var goal_obj_activity2 = string.Empty;
            var goal_obj_activity3 = string.Empty;
            var goal_obj_activity4 = string.Empty;

            MTPEntity mtp;
            if (workdayClient.Note.MTPId == null)   //la nota no tiene mtp relacionado, entonces se usa el primero que esté
                mtp = workdayClient.Client.MTPs.FirstOrDefault();
            else                                    //la nota tiene mtp relacionado    
                mtp = _context.MTPs.FirstOrDefault(m => m.Id == workdayClient.Note.MTPId);

            foreach (GoalEntity item in mtp.Goals.OrderBy(g => g.Number))
            {
                if (i == 0)
                {
                    num_of_goal1 = $"GOAL #{item.Number}:";
                    goal_text1 = item.Name;
                }
                if (i == 1)
                {
                    num_of_goal2 = $"GOAL #{item.Number}:";
                    goal_text2 = item.Name;
                }
                if (i == 2)
                {
                    num_of_goal3 = $"GOAL #{item.Number}:";
                    goal_text3 = item.Name;
                }
                if (i == 3)
                {
                    num_of_goal4 = $"GOAL #{item.Number}:";
                    goal_text4 = item.Name;
                }
                if (i == 4)
                {
                    num_of_goal5 = $"GOAL #{item.Number}:";
                    goal_text5 = item.Name;
                }
                i = ++i;
            }

            i = 0;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity1 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity2 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity3 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity4 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("num_of_goal1", num_of_goal1);
            parameters.Add("goal_text1", goal_text1);
            parameters.Add("goal1", goal1.ToString());
            parameters.Add("goal_obj_activity1", goal_obj_activity1);
            parameters.Add("num_of_goal2", num_of_goal2);
            parameters.Add("goal_text2", goal_text2);
            parameters.Add("goal2", goal2.ToString());
            parameters.Add("goal_obj_activity2", goal_obj_activity2);
            parameters.Add("num_of_goal3", num_of_goal3);
            parameters.Add("goal_text3", goal_text3);
            parameters.Add("goal3", goal3.ToString());
            parameters.Add("goal_obj_activity3", goal_obj_activity3);
            parameters.Add("num_of_goal4", num_of_goal4);
            parameters.Add("goal_text4", goal_text4);
            parameters.Add("goal4", goal4.ToString());
            parameters.Add("goal_obj_activity4", goal_obj_activity4);
            parameters.Add("num_of_goal5", num_of_goal5);
            parameters.Add("goal_text5", goal_text5);
            parameters.Add("goal5", goal5.ToString());
            parameters.Add("setting", setting);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf"/*,
                        $"NoteOf_{workdayClient.Client.Name}_{workdayClient.Workday.Date.ToShortDateString()}.pdf"*/);
        }

        private FileContentResult DemoClinic2NoteReportFCRSchema2(Workday_Client workdayClient)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNoteDemoClinic1");
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClient.Note.Supervisor.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Note.Supervisor.SignaturePath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }
            if (!string.IsNullOrEmpty(workdayClient.Facilitator.SignaturePath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClient.Facilitator.SignaturePath)}");
                stream2 = _imageHelper.ImageToByteArray(path);
            }

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            List<Note_Activity> notesactivities1 = new List<Note_Activity>();
            List<ActivityEntity> activities1 = new List<ActivityEntity>();
            List<ThemeEntity> themes1 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities2 = new List<Note_Activity>();
            List<ActivityEntity> activities2 = new List<ActivityEntity>();
            List<ThemeEntity> themes2 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities3 = new List<Note_Activity>();
            List<ActivityEntity> activities3 = new List<ActivityEntity>();
            List<ThemeEntity> themes3 = new List<ThemeEntity>();
            List<Note_Activity> notesactivities4 = new List<Note_Activity>();
            List<ActivityEntity> activities4 = new List<ActivityEntity>();
            List<ThemeEntity> themes4 = new List<ThemeEntity>();

            int i = 0;
            var num_of_goal1 = string.Empty;
            var goal_text1 = string.Empty;
            bool goal1 = false;
            var num_of_goal2 = string.Empty;
            var goal_text2 = string.Empty;
            bool goal2 = false;
            var num_of_goal3 = string.Empty;
            var goal_text3 = string.Empty;
            bool goal3 = false;
            var num_of_goal4 = string.Empty;
            var goal_text4 = string.Empty;
            bool goal4 = false;
            var num_of_goal5 = string.Empty;
            var goal_text5 = string.Empty;
            bool goal5 = false;
            var goal_obj_activity1 = string.Empty;
            var goal_obj_activity2 = string.Empty;
            var goal_obj_activity3 = string.Empty;
            var goal_obj_activity4 = string.Empty;

            MTPEntity mtp;
            if (workdayClient.Note.MTPId == null)   //la nota no tiene mtp relacionado, entonces se usa el primero que esté
                mtp = workdayClient.Client.MTPs.FirstOrDefault();
            else                                    //la nota tiene mtp relacionado    
                mtp = _context.MTPs.FirstOrDefault(m => m.Id == workdayClient.Note.MTPId);

            foreach (GoalEntity item in mtp.Goals.OrderBy(g => g.Number))
            {
                if (i == 0)
                {
                    num_of_goal1 = $"GOAL #{item.Number}:";
                    goal_text1 = item.Name;
                }
                if (i == 1)
                {
                    num_of_goal2 = $"GOAL #{item.Number}:";
                    goal_text2 = item.Name;
                }
                if (i == 2)
                {
                    num_of_goal3 = $"GOAL #{item.Number}:";
                    goal_text3 = item.Name;
                }
                if (i == 3)
                {
                    num_of_goal4 = $"GOAL #{item.Number}:";
                    goal_text4 = item.Name;
                }
                if (i == 4)
                {
                    num_of_goal5 = $"GOAL #{item.Number}:";
                    goal_text5 = item.Name;
                }
                i = ++i;
            }

            i = 0;
            foreach (Note_Activity item in workdayClient.Note.Notes_Activities)
            {
                if (i == 0)
                {
                    notesactivities1 = new List<Note_Activity> { item };
                    activities1 = new List<ActivityEntity> { item.Activity };
                    themes1 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity1 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 1)
                {
                    notesactivities2 = new List<Note_Activity> { item };
                    activities2 = new List<ActivityEntity> { item.Activity };
                    themes2 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity2 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 2)
                {
                    notesactivities3 = new List<Note_Activity> { item };
                    activities3 = new List<ActivityEntity> { item.Activity };
                    themes3 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity3 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                if (i == 3)
                {
                    notesactivities4 = new List<Note_Activity> { item };
                    activities4 = new List<ActivityEntity> { item.Activity };
                    themes4 = new List<ThemeEntity> { item.Activity.Theme };
                    if (item.Objetive != null)
                    {
                        if (item.Objetive.Goal.Number == 1)
                            goal1 = true;
                        if (item.Objetive.Goal.Number == 2)
                            goal2 = true;
                        if (item.Objetive.Goal.Number == 3)
                            goal3 = true;
                        if (item.Objetive.Goal.Number == 4)
                            goal4 = true;
                        if (item.Objetive.Goal.Number == 5)
                            goal5 = true;
                        goal_obj_activity4 = $"(Goal #{item.Objetive.Goal.Number}, Obj#{item.Objetive.Objetive}) ";
                    }
                }
                i = ++i;
            }

            report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsNotes", notes);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsSupervisors", supervisors);
            report.AddDataSource("dsNotesActivities1", notesactivities1);
            report.AddDataSource("dsActivities1", activities1);
            report.AddDataSource("dsThemes1", themes1);
            report.AddDataSource("dsNotesActivities2", notesactivities2);
            report.AddDataSource("dsActivities2", activities2);
            report.AddDataSource("dsThemes2", themes2);
            report.AddDataSource("dsNotesActivities3", notesactivities3);
            report.AddDataSource("dsActivities3", activities3);
            report.AddDataSource("dsThemes3", themes3);
            report.AddDataSource("dsNotesActivities4", notesactivities4);
            report.AddDataSource("dsActivities4", activities4);
            report.AddDataSource("dsThemes4", themes4);
            report.AddDataSource("dsImages", images);

            var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();

            i = 0;
            var diagnostic = string.Empty;
            foreach (var item in workdayClient.Client.Clients_Diagnostics)
            {
                if (i == 0)
                    diagnostic = item.Diagnostic.Code;
                else
                    diagnostic = $"{diagnostic}, {item.Diagnostic.Code}";
                i = ++i;
            }

            var setting = $"Setting: {workdayClient.Note.Setting}";

            parameters.Add("date", date);
            parameters.Add("dateFacilitator", dateFacilitator);
            parameters.Add("dateSupervisor", dateSupervisor);
            parameters.Add("diagnosis", diagnostic);
            parameters.Add("num_of_goal1", num_of_goal1);
            parameters.Add("goal_text1", goal_text1);
            parameters.Add("goal1", goal1.ToString());
            parameters.Add("goal_obj_activity1", goal_obj_activity1);
            parameters.Add("num_of_goal2", num_of_goal2);
            parameters.Add("goal_text2", goal_text2);
            parameters.Add("goal2", goal2.ToString());
            parameters.Add("goal_obj_activity2", goal_obj_activity2);
            parameters.Add("num_of_goal3", num_of_goal3);
            parameters.Add("goal_text3", goal_text3);
            parameters.Add("goal3", goal3.ToString());
            parameters.Add("goal_obj_activity3", goal_obj_activity3);
            parameters.Add("num_of_goal4", num_of_goal4);
            parameters.Add("goal_text4", goal_text4);
            parameters.Add("goal4", goal4.ToString());
            parameters.Add("goal_obj_activity4", goal_obj_activity4);
            parameters.Add("num_of_goal5", num_of_goal5);
            parameters.Add("goal_text5", goal_text5);
            parameters.Add("goal5", goal5.ToString());
            parameters.Add("setting", setting);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, "application/pdf", $"{workdayClient.Client.Name}.pdf");
        }
        #endregion

        [Authorize(Roles = "Facilitator, Supervisor")]
        public async Task<IActionResult> MTPView(int id)
        {
            Workday_Client workday_Client = await _context.Workdays_Clients
                                                          .Include(wc => wc.Client)
                                                          .FirstOrDefaultAsync(wc => wc.Id == id);

            if (workday_Client == null)
            {
                return NotFound();
            }

            MTPEntity mtp = await _context.MTPs
                                          .FirstOrDefaultAsync(m => (m.Client.Id == workday_Client.Client.Id && m.Active == true));
            if (mtp == null)
            {
                return NotFound();
            }
            
            return RedirectToAction("Details", "MTPs", new {id = mtp.Id});
        }

        public JsonResult Translate(string text)
        {
            return Json(text = _translateHelper.TranslateText("es", "en", text));            
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> NotStartedNotes()
        {           
          return View(await _context.Workdays_Clients.Include(wc => wc.Note)
                                                     .Include(wc => wc.Facilitator)
                                                     .Include(wc => wc.Client)
                                                     .Include(wc => wc.Workday)
                                                     .ThenInclude(w => w.Week)
                                                     .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                      && wc.Note == null && wc.Present == true
                                                                      && wc.Workday.Service == ServiceType.PSR))
                                                     .ToListAsync());                        
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> NotStartedIndNotes()
        {
            return View(await _context.Workdays_Clients.Include(wc => wc.IndividualNote)
                                                       .Include(wc => wc.Facilitator)
                                                       .Include(wc => wc.Client)
                                                       .Include(wc => wc.Workday)
                                                       .ThenInclude(w => w.Week)
                                                       .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                        && wc.IndividualNote == null && wc.Present == true
                                                                        && wc.Workday.Service == ServiceType.Individual))                                                       
                                                       .ToListAsync());
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> NotStartedGroupNotes()
        {
            return View(await _context.Workdays_Clients
                                      
                                      .Include(wc => wc.GroupNote)
                                      
                                      .Include(wc => wc.Facilitator)
                                      
                                      .Include(wc => wc.Client)
                                      
                                      .Include(wc => wc.Workday)
                                      .ThenInclude(w => w.Week)

                                      .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                 && wc.GroupNote == null && wc.Present == true
                                                 && wc.Workday.Service == ServiceType.Group))
                                      .ToListAsync());
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> NotesInEdit(int id  = 0)
        {
            if (id == 1)
            {
                ViewBag.FinishEdition = "Y";
            }
            return View(await _context.Workdays_Clients.Include(wc => wc.Note)
                                                       .Include(wc => wc.Facilitator)
                                                       .Include(wc => wc.Client)
                                                       .Include(wc => wc.Workday)
                                                       .ThenInclude(w => w.Week)
                                                       .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                        && wc.Note.Status == NoteStatus.Edition
                                                                        && wc.Workday.Service == ServiceType.PSR))
                                                       .ToListAsync());
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> IndNotesInEdit(int id = 0)
        {
            if (id == 1)
            {
                ViewBag.FinishEdition = "Y";
            }
            return View(await _context.Workdays_Clients.Include(wc => wc.IndividualNote)

                                                       .Include(wc => wc.Facilitator)

                                                       .Include(wc => wc.Client)

                                                       .Include(wc => wc.Workday)
                                                       .ThenInclude(w => w.Week)

                                                       .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                        && wc.IndividualNote.Status == NoteStatus.Edition
                                                                        && wc.Workday.Service == ServiceType.Individual))
                                                       .ToListAsync());
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> GroupNotesInEdit(int id = 0)
        {
            if (id == 1)
            {
                ViewBag.FinishEdition = "Y";
            }
            return View(await _context.Workdays_Clients.Include(wc => wc.IndividualNote)

                                                       .Include(wc => wc.Facilitator)

                                                       .Include(wc => wc.Client)

                                                       .Include(wc => wc.Workday)
                                                       .ThenInclude(w => w.Week)

                                                       .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                  && wc.GroupNote.Status == NoteStatus.Edition
                                                                  && wc.Workday.Service == ServiceType.Group))
                                                       .ToListAsync());
        }

        [Authorize(Roles = "Facilitator, Supervisor")]
        public async Task<IActionResult> PendingNotes(int id = 0)
        {
            if (User.IsInRole("Facilitator"))
            {
                return View(await _context.Workdays_Clients.Include(wc => wc.Note)

                                                           .Include(wc => wc.Facilitator)

                                                           .Include(wc => wc.Client)

                                                           .Include(wc => wc.Workday)
                                                           .ThenInclude(w => w.Week)

                                                           .Include(wc => wc.Messages)

                                                           .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                      && wc.Note.Status == NoteStatus.Pending
                                                                      && wc.Workday.Service == ServiceType.PSR))
                                                           .ToListAsync());
            }

            if (User.IsInRole("Supervisor"))
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    return View(await _context.Workdays_Clients.Include(wc => wc.Note)

                                                               .Include(wc => wc.Facilitator)
                                                               .ThenInclude(f => f.Clinic)

                                                               .Include(wc => wc.Client)

                                                               .Include(wc => wc.Workday)
                                                               .ThenInclude(w => w.Week)

                                                               .Include(wc => wc.Messages)

                                                               .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                          && wc.Note.Status == NoteStatus.Pending
                                                                          && wc.Workday.Service == ServiceType.PSR))
                                                               .ToListAsync());
                }
            }

            return View();
        }

        [Authorize(Roles = "Facilitator, Supervisor")]
        public async Task<IActionResult> PendingIndNotes(int id = 0)
        {
            if (User.IsInRole("Facilitator"))
            {
                return View(await _context.Workdays_Clients.Include(wc => wc.Note)

                                                           .Include(wc => wc.Facilitator)

                                                           .Include(wc => wc.Client)

                                                           .Include(wc => wc.Workday)
                                                           .ThenInclude(w => w.Week)

                                                           .Include(wc => wc.Messages)

                                                           .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                      && wc.IndividualNote.Status == NoteStatus.Pending
                                                                      && wc.Workday.Service == ServiceType.Individual))
                                                           .ToListAsync());
            }

            if (User.IsInRole("Supervisor"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    return View(await _context.Workdays_Clients.Include(wc => wc.Note)

                                                               .Include(wc => wc.Facilitator)
                                                               .ThenInclude(f => f.Clinic)

                                                               .Include(wc => wc.Client)

                                                               .Include(wc => wc.Workday)
                                                               .ThenInclude(w => w.Week)

                                                               .Include(wc => wc.Messages)

                                                               .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                          && wc.IndividualNote.Status == NoteStatus.Pending
                                                                          && wc.Workday.Service == ServiceType.Individual))
                                                               .ToListAsync());
                }
            }

            return View();
        }

        [Authorize(Roles = "Facilitator, Supervisor")]
        public async Task<IActionResult> PendingGroupNotes(int id = 0)
        {
            if (User.IsInRole("Facilitator"))
            {
                return View(await _context.Workdays_Clients.Include(wc => wc.Note)

                                                           .Include(wc => wc.Facilitator)

                                                           .Include(wc => wc.Client)

                                                           .Include(wc => wc.Workday)
                                                           .ThenInclude(w => w.Week)

                                                           .Include(wc => wc.Messages)

                                                           .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                      && wc.GroupNote.Status == NoteStatus.Pending
                                                                      && wc.Workday.Service == ServiceType.Group))
                                                           .ToListAsync());
            }

            if (User.IsInRole("Supervisor"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    return View(await _context.Workdays_Clients.Include(wc => wc.Note)

                                                               .Include(wc => wc.Facilitator)
                                                               .ThenInclude(f => f.Clinic)

                                                               .Include(wc => wc.Client)

                                                               .Include(wc => wc.Workday)
                                                               .ThenInclude(w => w.Week)

                                                               .Include(wc => wc.Messages)

                                                               .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                          && wc.GroupNote.Status == NoteStatus.Pending
                                                                          && wc.Workday.Service == ServiceType.Group))
                                                               .ToListAsync());
                }
            }

            return View();
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> ApprovedNotes(int id = 0)
        {
            return View(await _context.Workdays_Clients

                                      .Include(wc => wc.Note)
                                                       
                                      .Include(wc => wc.Facilitator)
                                                       
                                      .Include(wc => wc.Client)
                                                       
                                      .Include(wc => wc.Workday)                                                       
                                      .ThenInclude(w => w.Week)
                                                       
                                      .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                 && wc.Note.Status == NoteStatus.Approved
                                                 && wc.Workday.Service == ServiceType.PSR))
                                                       
                                      .ToListAsync());
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> ApprovedIndNotes(int id = 0)
        {
            return View(await _context.Workdays_Clients

                                      .Include(wc => wc.IndividualNote)
                                                       
                                      .Include(wc => wc.Facilitator)
                                                       
                                      .Include(wc => wc.Client)
                                                       
                                      .Include(wc => wc.Workday)                                                       
                                      .ThenInclude(w => w.Week)
                                                       
                                      .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name           
                                                 && wc.IndividualNote.Status == NoteStatus.Approved
                                                 && wc.Workday.Service == ServiceType.Individual))
                                      .ToListAsync());
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> ApprovedGroupNotes(int id = 0)
        {
            return View(await _context.Workdays_Clients

                                      .Include(wc => wc.GroupNote)

                                      .Include(wc => wc.Facilitator)

                                      .Include(wc => wc.Client)

                                      .Include(wc => wc.Workday)
                                      .ThenInclude(w => w.Week)

                                      .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                 && wc.GroupNote.Status == NoteStatus.Approved
                                                 && wc.Workday.Service == ServiceType.Group))
                                      .ToListAsync());
        }

        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> ApprovedNotesClinic(int id = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            return View(await _context.Workdays_Clients

                                      .Include(wc => wc.Note)

                                      .Include(wc => wc. IndividualNote)

                                      .Include(wc => wc.Facilitator)

                                      .Include(wc => wc.Client)
                                      
                                      .Include(wc => wc.Workday)
                                      .ThenInclude(w => w.Week)
                                      
                                      .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                 && (wc.Note.Status == NoteStatus.Approved || wc.IndividualNote.Status == NoteStatus.Approved)))
                                      .ToListAsync());
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
                    IdWorkdayClient = id,
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
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                model.From = user_logged.UserName;
                model.To = model.Workday_Client.Facilitator.LinkedUser;
                _context.Add(model);
                await _context.SaveChangesAsync();                
            }
            if(messageViewModel.Origin == 3)
                return RedirectToAction("PendingNotes");
            if (messageViewModel.Origin == 4)
                return RedirectToAction("NotesWithReview");
            if (messageViewModel.Origin == 5)
                return RedirectToAction("PendingIndNotes");
            if (messageViewModel.Origin == 6)
                return RedirectToAction("IndNotesWithReview");
            if (messageViewModel.Origin == 7)
                return RedirectToAction("PendingGroupNotes");
            if (messageViewModel.Origin == 8)
                return RedirectToAction("GroupNotesWithReview");
            if (messageViewModel.Origin == 1)
                return RedirectToAction("IndNotesSupervision");

            return RedirectToAction("NotesSupervision");
        }

        [Authorize(Roles = "Facilitator, Supervisor")]
        public async Task<IActionResult> NotesWithReview(int id = 0)
        {
            if (User.IsInRole("Facilitator"))
            {
                return View(await _context.Workdays_Clients.Include(wc => wc.Note)

                                                           .Include(wc => wc.Facilitator)

                                                           .Include(wc => wc.Client)

                                                           .Include(wc => wc.Workday)
                                                           .ThenInclude(w => w.Week)

                                                           .Include(wc => wc.Messages)

                                                           .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                               && wc.Note.Status == NoteStatus.Pending
                                                               && wc.Messages.Count() > 0
                                                               && wc.Workday.Service == ServiceType.PSR))
                                                           .ToListAsync());
            }

            if (User.IsInRole("Supervisor"))
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    return View(await _context.Workdays_Clients.Include(wc => wc.Note)

                                                               .Include(wc => wc.Facilitator)
                                                               .ThenInclude(f => f.Clinic)

                                                               .Include(wc => wc.Client)

                                                               .Include(wc => wc.Workday)
                                                               .ThenInclude(w => w.Week)

                                                               .Include(wc => wc.Messages)

                                                               .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                   && wc.Note.Status == NoteStatus.Pending
                                                                   && wc.Messages.Count() > 0
                                                                   && wc.Workday.Service == ServiceType.PSR))
                                                               .ToListAsync());
                }
            }

            return View();
        }

        [Authorize(Roles = "Facilitator, Supervisor")]
        public async Task<IActionResult> IndNotesWithReview(int id = 0)
        {
            if (User.IsInRole("Facilitator"))
            {
                return View(await _context.Workdays_Clients
                                          .Include(wc => wc.IndividualNote)

                                          .Include(wc => wc.Facilitator)

                                          .Include(wc => wc.Client)

                                          .Include(wc => wc.Workday)
                                          .ThenInclude(w => w.Week)

                                          .Include(wc => wc.Messages)

                                          .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                     && wc.IndividualNote.Status == NoteStatus.Pending
                                                     && wc.Messages.Count() > 0
                                                     && wc.Workday.Service == ServiceType.Individual))
                                          .ToListAsync());
            }

            if (User.IsInRole("Supervisor"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {
                    return View(await _context.Workdays_Clients
                                              .Include(wc => wc.IndividualNote)

                                              .Include(wc => wc.Facilitator)
                                              .ThenInclude(f => f.Clinic)

                                              .Include(wc => wc.Client)

                                              .Include(wc => wc.Workday)
                                              .ThenInclude(w => w.Week)

                                              .Include(wc => wc.Messages)

                                              .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                         && wc.IndividualNote.Status == NoteStatus.Pending
                                                         && wc.Messages.Count() > 0
                                                         && wc.Workday.Service == ServiceType.Individual))
                                              .ToListAsync());
                }
            }

            return View();
        }

        [Authorize(Roles = "Facilitator, Supervisor")]
        public async Task<IActionResult> GroupNotesWithReview(int id = 0)
        {
            if (User.IsInRole("Facilitator"))
            {
                return View(await _context.Workdays_Clients

                                          .Include(wc => wc.GroupNote)

                                          .Include(wc => wc.Facilitator)

                                          .Include(wc => wc.Client)

                                          .Include(wc => wc.Workday)
                                          .ThenInclude(w => w.Week)

                                          .Include(wc => wc.Messages)

                                          .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                     && wc.GroupNote.Status == NoteStatus.Pending
                                                     && wc.Messages.Count() > 0
                                                     && wc.Workday.Service == ServiceType.Group))
                                          .ToListAsync());
            }

            if (User.IsInRole("Supervisor"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {
                    return View(await _context.Workdays_Clients
                                              .Include(wc => wc.IndividualNote)

                                              .Include(wc => wc.Facilitator)
                                              .ThenInclude(f => f.Clinic)

                                              .Include(wc => wc.Client)

                                              .Include(wc => wc.Workday)
                                              .ThenInclude(w => w.Week)

                                              .Include(wc => wc.Messages)

                                              .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                         && wc.GroupNote.Status == NoteStatus.Pending
                                                         && wc.Messages.Count() > 0
                                                         && wc.Workday.Service == ServiceType.Group))
                                              .ToListAsync());
                }
            }

            return View();
        }

        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> NotNotesSummary()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            List<FacilitatorEntity> facilitators = await _context.Facilitators
                                                                 .Where(f => f.Clinic.Id == user_logged.Clinic.Id) 
                                                                 .ToListAsync();
            List<NotesSummary> notStarted = new List<NotesSummary>();
            foreach (FacilitatorEntity item in facilitators)
            {
                int psr_cant = await _context.Workdays_Clients
                                             .CountAsync(wc => (wc.Facilitator.Id == item.Id 
                                                             && wc.Note == null && wc.Present == true
                                                             && wc.Workday.Service == ServiceType.PSR));

                int ind_cant = await _context.Workdays_Clients
                                             .CountAsync(wc => (wc.Facilitator.Id == item.Id
                                                             && wc.IndividualNote == null && wc.Present == true
                                                             && wc.Workday.Service == ServiceType.Individual));

                int group_cant = await _context.Workdays_Clients
                                               .CountAsync(wc => (wc.Facilitator.Id == item.Id
                                                               && wc.Note == null && wc.Present == true
                                                               && wc.Workday.Service == ServiceType.Group));

                notStarted.Add(new NotesSummary {FacilitatorName = item.Name, PSRNotStarted = psr_cant, IndNotStarted = ind_cant, GroupNotStarted = group_cant });
            }
            
            return View(notStarted);
        }

        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> EditingSummary()
        {
            UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                         .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            List<FacilitatorEntity> facilitators = await _context.Facilitators
                                                                 .Where(f => f.Clinic.Id == user_logged.Clinic.Id)
                                                                 .ToListAsync();
            
            List<NotesSummary> editing = new List<NotesSummary>();
            foreach (FacilitatorEntity item in facilitators)
            {
                int psr_cant = await _context.Workdays_Clients
                                             .CountAsync(wc => (wc.Facilitator.Id == item.Id
                                                             && wc.Note.Status == NoteStatus.Edition
                                                             && wc.Workday.Service == ServiceType.PSR));

                int ind_cant = await _context.Workdays_Clients
                                             .CountAsync(wc => (wc.Facilitator.Id == item.Id
                                                             && wc.IndividualNote.Status == NoteStatus.Edition
                                                             && wc.Workday.Service == ServiceType.Individual));

                int group_cant = await _context.Workdays_Clients
                                               .CountAsync(wc => (wc.Facilitator.Id == item.Id
                                                             && wc.Note.Status == NoteStatus.Edition
                                                             && wc.Workday.Service == ServiceType.Group));

                editing.Add(new NotesSummary { FacilitatorName = item.Name, PSREditing = psr_cant, IndEditing = ind_cant, GroupEditing = group_cant });
            }

            return View(editing);
        }

        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> PendingSummary()
        {
            UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                         .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            List<FacilitatorEntity> facilitators = await _context.Facilitators
                                                                 .Where(f => f.Clinic.Id == user_logged.Clinic.Id)
                                                                 .ToListAsync();
            
            List<NotesSummary> pending = new List<NotesSummary>();
            foreach (FacilitatorEntity item in facilitators)
            {
                int psr_cant = await _context.Workdays_Clients
                                             .CountAsync(wc => (wc.Facilitator.Id == item.Id
                                                             && wc.Note.Status == NoteStatus.Pending 
                                                             && wc.Workday.Service == ServiceType.PSR));

                int ind_cant = await _context.Workdays_Clients
                                             .CountAsync(wc => (wc.Facilitator.Id == item.Id
                                                             && wc.IndividualNote.Status == NoteStatus.Pending
                                                             && wc.Workday.Service == ServiceType.Individual));

                int group_cant = await _context.Workdays_Clients
                                               .CountAsync(wc => (wc.Facilitator.Id == item.Id
                                                               && wc.Note.Status == NoteStatus.Pending
                                                               && wc.Workday.Service == ServiceType.Group));

                pending.Add(new NotesSummary { FacilitatorName = item.Name, PSRPending = psr_cant, IndPending = ind_cant, GroupPending = group_cant });
            }

            return View(pending);
        }

        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> ReviewSummary()
        {
            UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                         .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            List<FacilitatorEntity> facilitators = await _context.Facilitators
                                                                 .Where(f => f.Clinic.Id == user_logged.Clinic.Id)
                                                                 .ToListAsync();
           
            List<NotesSummary> review = new List<NotesSummary>();
            foreach (FacilitatorEntity item in facilitators)
            {
                int psr_cant = await _context.Workdays_Clients.CountAsync(wc => (wc.Facilitator.Id == item.Id
                                                                              && wc.Note.Status == NoteStatus.Pending 
                                                                              && wc.Messages.Count() > 0
                                                                              && wc.Workday.Service == ServiceType.PSR));

                int ind_cant = await _context.Workdays_Clients.CountAsync(wc => (wc.Facilitator.Id == item.Id
                                                                              && wc.IndividualNote.Status == NoteStatus.Pending
                                                                              && wc.Messages.Count() > 0
                                                                              && wc.Workday.Service == ServiceType.Individual));

                int group_cant = await _context.Workdays_Clients.CountAsync(wc => (wc.Facilitator.Id == item.Id
                                                                                && wc.Note.Status == NoteStatus.Pending
                                                                                && wc.Messages.Count() > 0
                                                                                && wc.Workday.Service == ServiceType.Group));

                review.Add(new NotesSummary { FacilitatorName = item.Name, PSRReview = psr_cant, IndReview = ind_cant, GroupReview = group_cant});
            }

            return View(review);
        }

        public string RandomGenerator()
        {
            Random random = new Random();
            int value = random.Next(1, 5);
            string text = (value == 1) ? "detected. " :
                           (value == 2) ? "observed. " :
                            (value == 3) ? "reached. " :
                             (value == 4) ? "attained. " :
                              (value == 5) ? "done. " : string.Empty;
            return text;
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> NotPresentNotes(int id = 0)
        {
            return View(await _context.Workdays_Clients.Include(wc => wc.Note)
                                                       .Include(wc => wc.Facilitator)
                                                       .Include(wc => wc.Client)
                                                       .Include(wc => wc.Workday)
                                                       .ThenInclude(w => w.Week)
                                                       .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                  && wc.Present == false
                                                                  && wc.Workday.Service == ServiceType.PSR))
                                                       .ToListAsync());
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> NotPresentIndNotes(int id = 0)
        {
            return View(await _context.Workdays_Clients

                                      .Include(wc => wc.IndividualNote)

                                      .Include(wc => wc.Facilitator)

                                      .Include(wc => wc.Client)

                                      .Include(wc => wc.Workday)
                                      .ThenInclude(w => w.Week)

                                      .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                 && wc.Present == false
                                                 && wc.Workday.Service == ServiceType.Individual))
                                      .ToListAsync());
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> NotPresentGroupNotes(int id = 0)
        {
            return View(await _context.Workdays_Clients

                                      .Include(wc => wc.GroupNote)

                                      .Include(wc => wc.Facilitator)

                                      .Include(wc => wc.Client)

                                      .Include(wc => wc.Workday)
                                      .ThenInclude(w => w.Week)

                                      .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                 && wc.Present == false
                                                 && wc.Workday.Service == ServiceType.Group))
                                      .ToListAsync());
        }

        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> NotPresentNotesClinic(int id = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            return View(await _context.Workdays_Clients.Include(wc => wc.Note)

                                                       .Include(wc => wc.Facilitator)

                                                       .Include(wc => wc.Client)

                                                       .Include(wc => wc.Workday)
                                                       .ThenInclude(w => w.Week)

                                                       .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                  && wc.Present == false))
                                                       .ToListAsync());
        }

        [Authorize(Roles = "Facilitator, Mannager")]
        public IActionResult PrintAbsenceNote(int id)
        {
            Workday_Client workdayClient = _context.Workdays_Clients
                                                          .Include(wc => wc.Facilitator)
                                                          .ThenInclude(c => c.Clinic)

                                                          .Include(wc => wc.Client)
                                                          .ThenInclude(c => c.Clinic)

                                                          .Include(wc => wc.Client)
                                                          .ThenInclude(c => c.Group)

                                                          .Include(wc => wc.Workday)
                                                          .FirstOrDefault(wc => wc.Id == id);
            if (workdayClient == null)
            {
                return NotFound();
            }

            if (workdayClient.Client.Clinic.Name == "DAVILA")
            {
                Stream stream = _reportHelper.DavilaAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Client.Clinic.Name == "LARKIN BEHAVIOR")
            {
                Stream stream = _reportHelper.LarkinAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Client.Clinic.Name == "SOL & VIDA")
            {
                Stream stream = _reportHelper.SolAndVidaAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Client.Clinic.Name == "HEALTH & BEAUTY NGB, INC")
            {
                Stream stream = _reportHelper.HealthAndBeautyAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Client.Clinic.Name == "ADVANCED GROUP MEDICAL CENTER")
            {
                Stream stream = _reportHelper.AdvancedGroupMCAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Client.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
            {
                Stream stream = _reportHelper.FloridaSocialHSAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Client.Clinic.Name == "ATLANTIC GROUP MEDICAL CENTER")
            {
                Stream stream = _reportHelper.AtlanticGroupMCAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Client.Clinic.Name == "DEMO CLINIC SCHEMA 1")
            {
                Stream stream = _reportHelper.DemoClinic1AbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Client.Clinic.Name == "DEMO CLINIC SCHEMA 2")
            {
                Stream stream = _reportHelper.DemoClinic2AbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            return null;
        }

        [Authorize(Roles = "Facilitator, Mannager")]
        public IActionResult PrintAbsenceIndNote(int id)
        {
            Workday_Client workdayClient = _context.Workdays_Clients

                                                   .Include(wc => wc.Facilitator)
                                                   .ThenInclude(c => c.Clinic)

                                                   .Include(wc => wc.Client)
                                                   .ThenInclude(c => c.Clinic)

                                                   .Include(wc => wc.Client)
                                                   .ThenInclude(c => c.Group)

                                                   .Include(wc => wc.Workday)

                                                   .FirstOrDefault(wc => wc.Id == id);
            if (workdayClient == null)
            {
                return NotFound();
            }

            if (workdayClient.Facilitator.Clinic.Name == "DAVILA")
            {
                Stream stream = _reportHelper.DavilaAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Facilitator.Clinic.Name == "LARKIN BEHAVIOR")
            {
                Stream stream = _reportHelper.LarkinAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Facilitator.Clinic.Name == "SOL & VIDA")
            {
                Stream stream = _reportHelper.SolAndVidaAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Facilitator.Clinic.Name == "HEALTH & BEAUTY NGB, INC")
            {
                Stream stream = _reportHelper.HealthAndBeautyAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Facilitator.Clinic.Name == "ADVANCED GROUP MEDICAL CENTER")
            {
                Stream stream = _reportHelper.AdvancedGroupMCAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Facilitator.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
            {
                Stream stream = _reportHelper.FloridaSocialHSAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Facilitator.Clinic.Name == "ATLANTIC GROUP MEDICAL CENTER")
            {
                Stream stream = _reportHelper.AtlanticGroupMCAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Facilitator.Clinic.Name == "DEMO CLINIC SCHEMA 1")
            {
                Stream stream = _reportHelper.DemoClinic1AbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Facilitator.Clinic.Name == "DEMO CLINIC SCHEMA 2")
            {
                Stream stream = _reportHelper.DemoClinic2AbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            return null;
        }

        [Authorize(Roles = "Facilitator, Mannager")]
        public IActionResult PrintAbsenceGroupNote(int id)
        {
            Workday_Client workdayClient = _context.Workdays_Clients

                                                   .Include(wc => wc.Facilitator)
                                                   .ThenInclude(c => c.Clinic)

                                                   .Include(wc => wc.Client)
                                                   .ThenInclude(c => c.Clinic)

                                                   .Include(wc => wc.Client)
                                                   .ThenInclude(c => c.Group)

                                                   .Include(wc => wc.Workday)

                                                   .FirstOrDefault(wc => wc.Id == id);
            if (workdayClient == null)
            {
                return NotFound();
            }

            if (workdayClient.Facilitator.Clinic.Name == "DAVILA")
            {
                Stream stream = _reportHelper.DavilaAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Facilitator.Clinic.Name == "LARKIN BEHAVIOR")
            {
                Stream stream = _reportHelper.LarkinAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Facilitator.Clinic.Name == "SOL & VIDA")
            {
                Stream stream = _reportHelper.SolAndVidaAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Facilitator.Clinic.Name == "HEALTH & BEAUTY NGB, INC")
            {
                Stream stream = _reportHelper.HealthAndBeautyAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Facilitator.Clinic.Name == "ADVANCED GROUP MEDICAL CENTER")
            {
                Stream stream = _reportHelper.AdvancedGroupMCAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Facilitator.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
            {
                Stream stream = _reportHelper.FloridaSocialHSAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Facilitator.Clinic.Name == "ATLANTIC GROUP MEDICAL CENTER")
            {
                Stream stream = _reportHelper.AtlanticGroupMCAbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Facilitator.Clinic.Name == "DEMO CLINIC SCHEMA 1")
            {
                Stream stream = _reportHelper.DemoClinic1AbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }
            if (workdayClient.Facilitator.Clinic.Name == "DEMO CLINIC SCHEMA 2")
            {
                Stream stream = _reportHelper.DemoClinic2AbsenceNoteReport(workdayClient);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            return null;
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> PrintIndividualSign(int id, int idWeek)
        {
            List<Workday_Client> workdayClientList = await _context.Workdays_Clients

                                                            .Include(wc => wc.Facilitator)
                                                            .ThenInclude(c => c.Clinic)

                                                            .Include(wc => wc.Client)                                                  

                                                            .Include(wc => wc.Workday)

                                                            .Where(wc => (wc.Client.Id == id && wc.Workday.Week.Id == idWeek)).ToListAsync();
            if (workdayClientList.Count() == 0)
            {
                return NotFound();
            }

            Stream stream = _reportHelper.PrintIndividualSign(workdayClientList);            
            return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);            
        }

        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> BillingReport()
        {                     
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
            {
                return NotFound();
            }
            
            return View(await _context.Weeks
                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Clients)
                                      .ThenInclude(wc => wc.Client)
                                      .ThenInclude(c => c.Group)

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Clients)
                                      .ThenInclude(g => g.Facilitator)

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Clients)
                                      .ThenInclude(wc => wc.Note)

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Clients)
                                      .ThenInclude(wc => wc.IndividualNote)

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Clients)
                                      .ThenInclude(wc => wc.Client)
                                      .ThenInclude(c => c.MTPs)

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.Workdays_Clients)
                                      .ThenInclude(wc => wc.Client)
                                      .ThenInclude(c => c.Clients_Diagnostics)
                                      .ThenInclude(cd => cd.Diagnostic)      
                                            
                                      .Include(w => w.Clinic)

                                      .Where(w => (w.Clinic.Id == user_logged.Clinic.Id))
                                      .ToListAsync());            
        }

        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> BillingWeek(int id)
        {
            UserEntity user_logged = await _context.Users

                                                   .Include(u => u.Clinic)

                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
            {
                return NotFound();
            }

            return View(await _context.Workdays_Clients
                                     
                                      .Include(wc => wc.Client)
                                      .ThenInclude(c => c.MTPs)
                                      
                                      .Include(wc => wc.Facilitator)

                                      .Include(wc => wc.Note)

                                      .Include(wc => wc.IndividualNote)                                      

                                      .Include(wc => wc.Client)
                                      .ThenInclude(c => c.Clients_Diagnostics)
                                      .ThenInclude(cd => cd.Diagnostic)

                                      .Include(wc => wc.Workday)
                                      .ThenInclude(w => w.Week)

                                      .Where(wc => (wc.Workday.Week.Clinic.Id == user_logged.Clinic.Id 
                                                 && wc.Workday.Week.Id == id
                                                 && wc.Present == true
                                                 && wc.Client != null))
                                      .ToListAsync());
        }

        [Authorize(Roles = "Mannager")]
        public IActionResult BillNote(int id)
        {
            BillViewModel model = new BillViewModel { Id = id, BilledDate = DateTime.Now };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> BillNote(BillViewModel model)
        {
            if (ModelState.IsValid)
            {
                Workday_Client workday_client = await _context.Workdays_Clients

                                                              .Include(wc => wc.Workday)
                                                              .ThenInclude(w => w.Week)
                                                              .ThenInclude(we => we.Clinic)

                                                              .Where(wc => wc.Id == model.Id)
                                                              .FirstOrDefaultAsync();
                workday_client.BilledDate = model.BilledDate;
                _context.Update(workday_client);
                await _context.SaveChangesAsync();

                List<Workday_Client> workdays_clients = await _context.Workdays_Clients

                                                                      .Include(wc => wc.Client)
                                                                      .ThenInclude(c => c.MTPs)

                                                                      .Include(wc => wc.Facilitator)

                                                                      .Include(wc => wc.Note)

                                                                      .Include(wc => wc.IndividualNote)

                                                                      .Include(wc => wc.Client)
                                                                      .ThenInclude(c => c.Clients_Diagnostics)
                                                                      .ThenInclude(cd => cd.Diagnostic)

                                                                      .Include(wc => wc.Workday)
                                                                      .ThenInclude(w => w.Week)

                                                                      .Where(wc => (wc.Workday.Week.Clinic.Id == workday_client.Workday.Week.Clinic.Id
                                                                                 && wc.Workday.Week.Id == workday_client.Workday.Week.Id
                                                                                 && wc.Present == true
                                                                                 && wc.Client != null))
                                                                      .ToListAsync();

                workdays_clients = workdays_clients.OrderBy(wc => wc.Facilitator.Name).ToList();

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_BillingWeek", workdays_clients) });
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "BillNote", model) });
        }

        [Authorize(Roles = "Mannager")]
        public IActionResult PaymentReceived(int id)
        {
            PaymentReceivedViewModel model = new PaymentReceivedViewModel { Id = id, PaymentDate = DateTime.Now };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> PaymentReceived(PaymentReceivedViewModel model)
        {
            if (ModelState.IsValid)
            {
                Workday_Client workday_client = await _context.Workdays_Clients

                                                              .Include(wc => wc.Workday)
                                                              .ThenInclude(w => w.Week)
                                                              .ThenInclude(we => we.Clinic)

                                                              .Where(wc => wc.Id == model.Id)
                                                              .FirstOrDefaultAsync();
                workday_client.PaymentDate = model.PaymentDate;
                _context.Update(workday_client);
                await _context.SaveChangesAsync();

                List<Workday_Client> workdays_clients = await _context.Workdays_Clients

                                                                      .Include(wc => wc.Client)
                                                                      .ThenInclude(c => c.MTPs)

                                                                      .Include(wc => wc.Facilitator)

                                                                      .Include(wc => wc.Note)

                                                                      .Include(wc => wc.IndividualNote)

                                                                      .Include(wc => wc.Client)
                                                                      .ThenInclude(c => c.Clients_Diagnostics)
                                                                      .ThenInclude(cd => cd.Diagnostic)

                                                                      .Include(wc => wc.Workday)
                                                                      .ThenInclude(w => w.Week)

                                                                      .Where(wc => (wc.Workday.Week.Clinic.Id == workday_client.Workday.Week.Clinic.Id
                                                                                 && wc.Workday.Week.Id == workday_client.Workday.Week.Id
                                                                                 && wc.Present == true
                                                                                 && wc.Client != null))
                                                                      .ToListAsync();

                workdays_clients = workdays_clients.OrderBy(wc => wc.Facilitator.Name).ToList();

                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_BillingWeek", workdays_clients) });
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "PaymentReceived", model) });
        }

        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> NotesSummaryDetails()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
            {
                return NotFound();
            }
            return View(await _context.Weeks.Include(w => w.Days)
                                            .ThenInclude(d => d.Workdays_Clients)
                                            .ThenInclude(wc => wc.Client)
                                            .ThenInclude(c => c.Group)

                                            .Include(w => w.Days)
                                            .ThenInclude(d => d.Workdays_Clients)
                                            .ThenInclude(g => g.Facilitator)

                                            .Include(w => w.Days)
                                            .ThenInclude(d => d.Workdays_Clients)
                                            .ThenInclude(wc => wc.Note)

                                            .Where(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                      && w.Days.Where(d => d.Service == ServiceType.PSR).Count() > 0))
                                            .ToListAsync());
        }

        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> IndNotesSummaryDetails()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
            {
                return NotFound();
            }
            return View(await _context.Weeks.Include(w => w.Days)
                                            .ThenInclude(d => d.Workdays_Clients)
                                            .ThenInclude(wc => wc.Client)                                            

                                            .Include(w => w.Days)
                                            .ThenInclude(d => d.Workdays_Clients)
                                            .ThenInclude(g => g.Facilitator)

                                            .Include(w => w.Days)
                                            .ThenInclude(d => d.Workdays_Clients)
                                            .ThenInclude(wc => wc.IndividualNote)

                                            .Where(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                      && w.Days.Where(d => d.Service == ServiceType.Individual).Count() > 0))
                                            .ToListAsync());
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> IndividualSignInSheet()
        {
            UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                         .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
                return View(null);

            return View(await _context.Weeks.Include(w => w.Days)
                                            .ThenInclude(d => d.Workdays_Clients)
                                            .ThenInclude(wc => wc.Client)
                                            .ThenInclude(c => c.Group)

                                            .Include(w => w.Days)
                                            .ThenInclude(d => d.Workdays_Clients)
                                            .ThenInclude(g => g.Facilitator)

                                            .Include(w => w.Days)
                                            .ThenInclude(d => d.Workdays_Clients)
                                            .ThenInclude(wc => wc.Note)

                                            .Include(w => w.Days)
                                            .ThenInclude(d => d.Workdays_Clients)
                                            .ThenInclude(wc => wc.Client)
                                            .ThenInclude(c => c.MTPs)

                                            .Include(w => w.Days)
                                            .ThenInclude(d => d.Workdays_Clients)
                                            .ThenInclude(wc => wc.Client)
                                            .ThenInclude(c => c.Clients_Diagnostics)
                                            .ThenInclude(cd => cd.Diagnostic)

                                            .Where(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                      && w.Days.Where(d => d.Service == ServiceType.PSR).Count() > 0))
                                            .ToListAsync());
        }

        #region Utils funtions
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

        public FileResult ZipFile(List<FileContentResult> fileContentList, string zipName)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    foreach (FileContentResult file in fileContentList)
                    {
                        var entry = archive.CreateEntry(file.FileDownloadName, CompressionLevel.Fastest);
                        using (var zipStream = entry.Open())
                        {
                            zipStream.Write(file.FileContents, 0, file.FileContents.Length);
                        }
                    }
                }
                return File(ms.ToArray(), "application/zip", zipName);
            }
        }

        private bool VerifyNotesAtSameTime(int idClient, string session, DateTime date)
        {
            //Individual notes
            if (session == "8.00 - 9.00 AM" || session == "9.05 - 10.05 AM" || session == "10.15 - 11.15 AM" || session == "11.20 - 12.20 PM")
            {
                if (_context.Workdays_Clients
                            .Where(wc => (wc.Client.Id == idClient && wc.Session == "AM" && wc.Workday.Date == date))
                            .Count() > 0)
                    return true;
                return false;

            }
            if (session == "12.45 - 1.45 PM" || session == "1.50 - 2.50 PM" || session == "3.00 - 4.00 PM" || session == "4.05 - 5.05 PM")
            {
                if (_context.Workdays_Clients
                            .Where(wc => (wc.Client.Id == idClient && wc.Session == "PM" && wc.Workday.Date == date))
                            .Count() > 0)
                    return true;
                return false;
            }

            //PSR notes
            if (session == "AM")
            {
                if (_context.Workdays_Clients
                            .Where(wc => (wc.Client.Id == idClient && wc.Session == "8.00 - 9.00 AM" && wc.Workday.Date == date))
                            .Count() > 0 
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "9.05 - 10.05 AM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "10.15 - 11.15 AM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "11.20 - 12.20 PM" && wc.Workday.Date == date))
                                  .Count() > 0)
                    return true;
                return false;
            }
            if (session == "PM")
            {
                if (_context.Workdays_Clients
                            .Where(wc => (wc.Client.Id == idClient && wc.Session == "12.45 - 1.45 PM" && wc.Workday.Date == date))
                            .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "1.50 - 2.50 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "3.00 - 4.00 PM" && wc.Workday.Date == date))
                                  .Count() > 0
                       || _context.Workdays_Clients
                                  .Where(wc => (wc.Client.Id == idClient && wc.Session == "4.05 - 5.05 PM" && wc.Workday.Date == date))
                                  .Count() > 0)
                    return true;
                return false;
            }

            return true;
        }
        #endregion

    }
}