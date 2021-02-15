using AspNetCore.Reporting;
using KyoS.Common.Enums;
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
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace KyoS.Web.Controllers
{
    public class NotesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IDateHelper _dateHelper;
        public NotesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IDateHelper dateHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _dateHelper = dateHelper;
        }
        [Authorize(Roles = "Admin, Facilitator")]
        public async Task<IActionResult> Index(int id = 0)
        {
            if (id == 1)
            {
                ViewBag.FinishEdition = "Y";
            }

            if (User.IsInRole("Admin"))
            {
                return View(await _context.Weeks.Include(w => w.Clinic)
                                                .Include(w => w.Days)
                                                .ThenInclude(d => d.Workdays_Clients).ToListAsync());
            }
            else
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic == null)
                {
                    return View(await _context.Weeks.Include(w => w.Clinic)
                                                .Include(w => w.Days)
                                                .ThenInclude(d => d.Workdays_Clients).ToListAsync());
                }
                return View(await _context.Weeks.Include(w => w.Days)
                                                .ThenInclude(d => d.Workdays_Clients)
                                                .ThenInclude(wc => wc.Client)
                                                .ThenInclude(c => c.Group)
                                                .ThenInclude(g => g.Facilitator)

                                                .Include(w => w.Days)
                                                .ThenInclude(d => d.Workdays_Clients)
                                                .ThenInclude(wc => wc.Note)

                                                .Where(w => (w.Clinic.Id == user_logged.Clinic.Id))
                                                .ToListAsync());
            }
        }

        [Authorize(Roles = "Admin, Facilitator")]
        public async Task<IActionResult> Present(int id, int origin = 0)
        {
            Workday_Client workdayClient = await _context.Workdays_Clients.Include(wc => wc.Workday)
                                                       .Include(wc => wc.Client)
                                                       .ThenInclude(c => c.Group)
                                                       .ThenInclude(g => g.Facilitator)
                                                       .FirstOrDefaultAsync(wc => wc.Id == id);
            Workday_ClientViewModel model = _converterHelper.ToWorkdayClientViewModel(workdayClient);
            model.Origin = origin;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Facilitator")]
        public async Task<IActionResult> Present(Workday_ClientViewModel model, IFormCollection form)
        {
            Workday_Client entity = await _context.Workdays_Clients.Include(wc => wc.Workday)
                                                       .Include(wc => wc.Client)
                                                       .ThenInclude(c => c.Group)
                                                       .ThenInclude(g => g.Facilitator)
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
                        break;
                    }
                case "nopresent":
                    {
                        entity.Present = false;
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

        [Authorize(Roles = "Admin, Facilitator")]
        public async Task<IActionResult> EditNote(int id, int error = 0, int origin = 0)
        {
            NoteViewModel noteViewModel;
            List<ThemeEntity> topics;
            List<SelectListItem> list1 = new List<SelectListItem>();
            List<SelectListItem> list2 = new List<SelectListItem>();
            List<SelectListItem> list3 = new List<SelectListItem>();
            List<SelectListItem> list4 = new List<SelectListItem>();

            //la nota no tiene linkeado ningun goal
            if (error == 1)  
                ViewBag.Error = "O";

            Workday_Client workday_Client = await _context.Workdays_Clients.Include(wc => wc.Workday)
                                                                           .ThenInclude(w => w.Workdays_Activities_Facilitators)
                                                                           .ThenInclude(waf => waf.Activity)
                                                                           .ThenInclude(a => a.Theme)

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

            //el dia no tiene actividad asociada por lo tanto no se puede crear la nota
            if (workday_Client.Workday.Workdays_Activities_Facilitators.Count == 0)
            {
                ViewBag.Error = "1";
                noteViewModel = new NoteViewModel
                {
                    Id = workday_Client.Workday.Id,
                };
                return View(noteViewModel);
            }

            NoteEntity note = await _context.Notes.Include(n => n.Workday_Cient)
                                                  .ThenInclude(wc => wc.Client)
                                                  .ThenInclude(c => c.Group)
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

            MTPEntity mtp = await _context.MTPs.Include(m => m.Goals).FirstOrDefaultAsync(m => m.Client.Id == workday_Client.Client.Id);

            List<Workday_Activity_Facilitator> activities = workday_Client.Workday.Workdays_Activities_Facilitators.ToList();

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
                                                                  .Where(na => na.Note.Id == note.Id).ToListAsync();

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
                    Schema = workday_Client.Client.Clinic.Schema,
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
                                                                        ? note_Activity[3].Objetive.Goal.Id : 0)
                };
            }
            return View(noteViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Facilitator")]
        public async Task<IActionResult> EditNote(NoteViewModel model, IFormCollection form)
        {
            Workday_Client workday_Client = await _context.Workdays_Clients.Include(wc => wc.Workday)
                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Clinic)
                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Group)
                                                                           .ThenInclude(g => g.Facilitator)
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
                    model.PlanNote = $"{progress}{model.PlanNote}";

                    noteEntity = await _converterHelper.ToNoteEntity(model, true);
                    _context.Add(noteEntity);
                    note_Activity = new Note_Activity
                    {
                        Note = noteEntity,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity1),
                        AnswerClient = model.AnswerClient1,
                        AnswerFacilitator = model.AnswerFacilitator1,
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive1),
                    };
                    _context.Add(note_Activity);
                    note_Activity = new Note_Activity
                    {
                        Note = noteEntity,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity2),
                        AnswerClient = model.AnswerClient2,
                        AnswerFacilitator = model.AnswerFacilitator2,
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive2),
                    };
                    _context.Add(note_Activity);
                    note_Activity = new Note_Activity
                    {
                        Note = noteEntity,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity3),
                        AnswerClient = model.AnswerClient3,
                        AnswerFacilitator = model.AnswerFacilitator3,
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive3),
                    };
                    _context.Add(note_Activity);
                    note_Activity = new Note_Activity
                    {
                        Note = noteEntity,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity4),
                        AnswerClient = model.AnswerClient4,
                        AnswerFacilitator = model.AnswerFacilitator4,
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
                    note.PlanNote = model.PlanNote;
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
                        AnswerClient = model.AnswerClient1,
                        AnswerFacilitator = model.AnswerFacilitator1,
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive1),
                    };
                    _context.Add(note_Activity);
                    note_Activity = new Note_Activity
                    {
                        Note = note,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity2),
                        AnswerClient = model.AnswerClient2,
                        AnswerFacilitator = model.AnswerFacilitator2,
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive2),
                    };
                    _context.Add(note_Activity);
                    note_Activity = new Note_Activity
                    {
                        Note = note,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity3),
                        AnswerClient = model.AnswerClient3,
                        AnswerFacilitator = model.AnswerFacilitator3,
                        Objetive = _context.Objetives.FirstOrDefault(o => o.Id == model.IdObjetive3),
                    };
                    _context.Add(note_Activity);
                    note_Activity = new Note_Activity
                    {
                        Note = note,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity4),
                        AnswerClient = model.AnswerClient4,
                        AnswerFacilitator = model.AnswerFacilitator4,
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

        [Authorize(Roles = "Admin, Facilitator")]
        public async Task<IActionResult> FinishEditing(int id, int origin = 0)
        {
            Workday_Client workday_Client = await _context.Workdays_Clients.FirstOrDefaultAsync(wc => wc.Id == id);
            if (workday_Client == null)
            {
                return NotFound();
            }

            NoteEntity note = await _context.Notes.Include(n => n.Workday_Cient)
                                                  .Include(n => n.Notes_Activities)
                                                  .ThenInclude(na => na.Objetive)
                                                  .FirstOrDefaultAsync(n => n.Workday_Cient.Id == id);

            bool exist = false;
            foreach (Note_Activity item in note.Notes_Activities)
            {
                if (item.Objetive != null)
                    exist = true;
            }

            if (!exist)     //la nota no tiene goal relacionado
            {
                if(origin == 0)
                    return RedirectToAction(nameof(EditNote), new {id =  id, error = 1, origin = 0});                
                if (origin == 2)
                    return RedirectToAction(nameof(EditNote), new {id = id, error = 1, origin = 2});
            }
            
            note.Status = NoteStatus.Pending;
            _context.Update(note);

            await _context.SaveChangesAsync();
            if (origin == 2)
                return RedirectToAction(nameof(NotesInEdit), new { id = 1 });
            else
                return RedirectToAction(nameof(Index), new { id = 1});                        
        }

        [Authorize(Roles = "Admin, Facilitator")]
        public JsonResult GetActivityList(int idTheme)
        {
            List<ActivityEntity> activities = _context.Activities.Where(a => a.Theme.Id == idTheme).ToList();

            return Json(new SelectList(activities, "Id", "Name"));
        }

        [Authorize(Roles = "Admin, Facilitator")]
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

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> NotesSupervision()
        {
            UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                            .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            List<WeekEntity> weeks = (await _context.Weeks.Include(w => w.Days)
                                            .ThenInclude(d => d.Workdays_Clients)
                                            .ThenInclude(wc => wc.Client)
                                            .ThenInclude(c => c.Group)
                                            .ThenInclude(g => g.Facilitator)
                                            .Include(w => w.Days)
                                            .ThenInclude(d => d.Workdays_Clients)
                                            .ThenInclude(wc => wc.Note)
                                            .Where(w => (w.Clinic.Id == user_logged.Clinic.Id)).ToListAsync());
            
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

            NoteViewModel noteViewModel;
                        
            List<Note_Activity> note_Activity = await _context.Notes_Activities
                                                                .Include(na => na.Activity)
                                                                .ThenInclude(a => a.Theme)
                                                                .Include(n => n.Objetive)
                                                                .ThenInclude(o => o.Goal)
                                                                .Where(na => na.Note.Id == note.Id).ToListAsync();
            noteViewModel = new NoteViewModel
            {
                Id = id,
                Workday_Cient = workday_Client,
                PlanNote = note.PlanNote,
                Origin = origin,
                Schema = workday_Client.Client.Clinic.Schema,

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

        [Authorize(Roles = "Admin, Facilitator")]
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
        [Authorize(Roles = "Admin, Facilitator")]
        public async Task<IActionResult> PrintNotes(PrintNotesViewModel model, IFormCollection form)
        {
            var meridian = (form["classifications"] == "First") ? "AM" : "PM";
            List<Workday_Client> list = await _context.Workdays_Clients
                                                      .Include(wc => wc.Client)
                                                      .ThenInclude(c => c.Group)
                                                      .ThenInclude(g => g.Facilitator)
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

        [Authorize(Roles = "Admin, Facilitator")]
        public IActionResult PrintNote(int id)
        {
            Workday_Client workdayClient = _context.Workdays_Clients
                                                          .Include(wc => wc.Facilitator)

                                                          .Include(wc => wc.Client)
                                                          .ThenInclude(c => c.MTPs)
                                                          .ThenInclude(m => m.Diagnosis)

                                                          .Include(wc => wc.Client)
                                                          .ThenInclude(c => c.MTPs)
                                                          .ThenInclude(m => m.Goals)

                                                          .Include(wc => wc.Client)
                                                          .ThenInclude(c => c.MTPs)
                                                          .ThenInclude(m => m.Goals)
                                                          .ThenInclude(g => g.Objetives)

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
                                                          .FirstOrDefault(wc => (wc.Id == id
                                                                               && wc.Note.Status == NoteStatus.Approved));
            if (workdayClient == null)
            {
                return NotFound();
            }            

            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Notes\\{1}.rdlc", fileDirPath, $"rptNote{workdayClient.Note.Supervisor.Clinic.Name}");
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            System.Text.Encoding.GetEncoding("windows-1252");
            LocalReport report = new LocalReport(rdlcFilePath);

            //datasource
            List<Workday_Client> workdaysclients = new List<Workday_Client> { workdayClient };
            List<ClientEntity> clients = new List<ClientEntity> { workdayClient.Client };
            List<NoteEntity> notes = new List<NoteEntity> { workdayClient.Note };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClient.Facilitator };
            List<SupervisorEntity> supervisors = new List<SupervisorEntity> { workdayClient.Note.Supervisor };

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
            return File(result.MainStream, System.Net.Mime.MediaTypeNames.Application.Octet,
                        $"NoteOf_{workdayClient.Client.Name}_{workdayClient.Workday.Date.ToShortDateString()}.pdf");
        }

        [Authorize(Roles = "Admin, Facilitator, Supervisor")]
        public async Task<IActionResult> MTPView(int id)
        {
            Workday_Client workday_Client = await _context.Workdays_Clients.Include(wc => wc.Client)
                                                                           .FirstOrDefaultAsync(wc => wc.Id == id);

            if (workday_Client == null)
            {
                return NotFound();
            }

            MTPEntity mtp = await _context.MTPs.FirstOrDefaultAsync(m => m.Client.Id == workday_Client.Client.Id);
            if (mtp == null)
            {
                return NotFound();
            }
            
            return RedirectToAction("Details", "MTPs", new {id = mtp.Id});
        }

        public JsonResult Translate(string text)
        {
            var toLanguage = "en";//English
            var fromLanguage = "es";//Spanish
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={fromLanguage}&tl={toLanguage}&dt=t&q={HttpUtility.UrlEncode(text)}";
            var webClient = new WebClient
            {
                Encoding = System.Text.Encoding.UTF8
            };
            try
            {
                var result = webClient.DownloadString(url);
                result = result.Substring(4, result.IndexOf("\"", 4, StringComparison.Ordinal) - 4);
                return Json(text = result);
            }
            catch
            {
                return Json(text = "Error. It's not possible to translate");
            }
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
                                                                      && wc.Note == null))
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
                                                                        && wc.Note.Status == NoteStatus.Edition))
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
                                                       .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                  && wc.Note.Status == NoteStatus.Pending))
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
                                                       .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                  && wc.Note.Status == NoteStatus.Pending))
                                                       .ToListAsync());
                }
            }

            return View();
        }
        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> ApprovedNotes(int id = 0)
        {
            return View(await _context.Workdays_Clients.Include(wc => wc.Note)
                                                       .Include(wc => wc.Facilitator)
                                                       .Include(wc => wc.Client)
                                                       .Include(wc => wc.Workday)
                                                       .ThenInclude(w => w.Week)
                                                       .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                  && wc.Note.Status == NoteStatus.Approved))
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
                                                                  && wc.Messages.Count() > 0))
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
                                                                   && wc.Messages.Count() > 0))
                                                               .ToListAsync());
                }
            }

            return View();
        }

        [Authorize(Roles = "Mannager")]
        public async Task<IActionResult> NotNotesSummary()
        {
            UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                         .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            List<FacilitatorEntity> facilitators = await _context.Facilitators
                                                                 .Where(f => f.Clinic.Id == user_logged.Clinic.Id) 
                                                                 .ToListAsync();
            int cant;
            List<NotesSummary> notStarted = new List<NotesSummary>();
            foreach (FacilitatorEntity item in facilitators)
            {
                cant = await _context.Workdays_Clients.CountAsync(wc => (wc.Facilitator.Id == item.Id 
                                                                      && wc.Note == null && wc.Facilitator.Status == StatusType.Open));

                notStarted.Add(new NotesSummary {FacilitatorName = item.Name, NotStarted = cant});
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
            int cant;
            List<NotesSummary> editing = new List<NotesSummary>();
            foreach (FacilitatorEntity item in facilitators)
            {
                cant = await _context.Workdays_Clients.CountAsync(wc => (wc.Facilitator.Id == item.Id
                                                                      && wc.Note.Status == NoteStatus.Edition && wc.Facilitator.Status == StatusType.Open));

                editing.Add(new NotesSummary { FacilitatorName = item.Name, Editing = cant });
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
            int cant;
            List<NotesSummary> pending = new List<NotesSummary>();
            foreach (FacilitatorEntity item in facilitators)
            {
                cant = await _context.Workdays_Clients.CountAsync(wc => (wc.Facilitator.Id == item.Id
                                                                      && wc.Note.Status == NoteStatus.Pending && wc.Facilitator.Status == StatusType.Open));

                pending.Add(new NotesSummary { FacilitatorName = item.Name, Editing = cant });
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
            int cant;
            List<NotesSummary> review = new List<NotesSummary>();
            foreach (FacilitatorEntity item in facilitators)
            {
                cant = await _context.Workdays_Clients.CountAsync(wc => (wc.Facilitator.Id == item.Id
                                                                      && wc.Note.Status == NoteStatus.Pending 
                                                                      && wc.Facilitator.Status == StatusType.Open
                                                                      && wc.Messages.Count() > 0));

                review.Add(new NotesSummary { FacilitatorName = item.Name, Editing = cant });
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
    }
}