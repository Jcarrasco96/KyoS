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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


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
        public async Task<IActionResult> Index()
        {
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
        public async Task<IActionResult> Present(int id)
        {
            return View(await _context.Workdays_Clients.Include(wc => wc.Workday)
                                                       .Include(wc => wc.Client)
                                                       .ThenInclude(c => c.Group)
                                                       .ThenInclude(g => g.Facilitator)
                                                       .FirstOrDefaultAsync(wc => wc.Id == id));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Facilitator")]
        public async Task<IActionResult> Present(int id, IFormCollection form)
        {
            Workday_Client model = await _context.Workdays_Clients.Include(wc => wc.Workday)
                                                       .Include(wc => wc.Client)
                                                       .ThenInclude(c => c.Group)
                                                       .ThenInclude(g => g.Facilitator)
                                                       .FirstOrDefaultAsync(wc => wc.Id == id);

            if (model == null)
            {
                return NotFound();
            }

            switch (form["Present"])
            {
                case "present":
                    {
                        model.Present = true;
                        break;
                    }
                case "nopresent":
                    {
                        model.Present = false;
                        break;
                    }
                default:
                    break;
            }

            _context.Update(model);
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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

            return View(model);
        }

        public async Task<IActionResult> EditNote(int id)
        {
            Workday_Client workday_Client = await _context.Workdays_Clients.Include(wc => wc.Workday)
                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Clinic)
                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Group)
                                                                           .ThenInclude(g => g.Facilitator)
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
            List<ThemeEntity> topics;
            List<SelectListItem> list1 = new List<SelectListItem>();
            List<SelectListItem> list2 = new List<SelectListItem>();
            List<SelectListItem> list3 = new List<SelectListItem>();
            List<SelectListItem> list4 = new List<SelectListItem>();

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

            if (note == null)   //la nota no está creada
            {
                noteViewModel = new NoteViewModel
                {
                    Id = id,
                    Status = NoteStatus.Pending,    //es solo generico para la visualizacion del btn FinishEditing
                    IdTopic1 = (list1.Count != 0) ? topics[0].Id : 0,
                    Topics1 = (list1.Count != 0) ? list1 : _combosHelper.GetComboThemesByClinic(workday_Client.Client.Clinic.Id),
                    Activities1 = (list1.Count != 0) ? _combosHelper.GetComboActivitiesByTheme(topics[0].Id) : null,

                    IdTopic2 = (list2.Count != 0) ? topics[1].Id : 0,
                    Topics2 = (list2.Count != 0) ? list2 : _combosHelper.GetComboThemesByClinic(workday_Client.Client.Clinic.Id),
                    Activities2 = (list2.Count != 0) ? _combosHelper.GetComboActivitiesByTheme(topics[1].Id) : null,

                    IdTopic3 = (list3.Count != 0) ? topics[2].Id : 0,
                    Topics3 = (list3.Count != 0) ? list3 : _combosHelper.GetComboThemesByClinic(workday_Client.Client.Clinic.Id),
                    Activities3 = (list3.Count != 0) ? _combosHelper.GetComboActivitiesByTheme(topics[2].Id) : null,

                    IdTopic4 = (list4.Count != 0) ? topics[3].Id : 0,
                    Topics4 = (list4.Count != 0) ? list4 : _combosHelper.GetComboThemesByClinic(workday_Client.Client.Clinic.Id),
                    Activities4 = (list4.Count != 0) ? _combosHelper.GetComboActivitiesByTheme(topics[3].Id) : null,

                    Workday_Cient = workday_Client
                };
            }
            else
            {
                List<Note_Activity> note_Activity = await _context.Notes_Activities
                                                                  .Include(na => na.Activity)
                                                                  .ThenInclude(a => a.Theme).Where(na => na.Note.Id == note.Id).ToListAsync();
                noteViewModel = new NoteViewModel
                {
                    Id = id,
                    Workday_Cient = workday_Client,
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

                    IdTopic1 = (note_Activity.Count > 0) ? note_Activity[0].Activity.Theme.Id : 0,
                    Topics1 = (list1.Count != 0) ? list1 : _combosHelper.GetComboThemesByClinic(workday_Client.Client.Clinic.Id),
                    IdActivity1 = (note_Activity.Count > 0) ? note_Activity[0].Activity.Id : 0,
                    Activities1 = _combosHelper.GetComboActivitiesByTheme((note_Activity.Count > 0) ? note_Activity[0].Activity.Theme.Id : 0),
                    AnswerClient1 = note_Activity[0].AnswerClient,
                    AnswerFacilitator1 = note_Activity[0].AnswerFacilitator,

                    IdTopic2 = (note_Activity.Count > 1) ? note_Activity[1].Activity.Theme.Id : 0,
                    Topics2 = (list2.Count != 0) ? list2 : _combosHelper.GetComboThemesByClinic(workday_Client.Client.Clinic.Id),
                    IdActivity2 = (note_Activity.Count > 1) ? note_Activity[1].Activity.Id : 0,
                    Activities2 = _combosHelper.GetComboActivitiesByTheme((note_Activity.Count > 1) ? note_Activity[1].Activity.Theme.Id : 0),
                    AnswerClient2 = note_Activity[1].AnswerClient,
                    AnswerFacilitator2 = note_Activity[1].AnswerFacilitator,

                    IdTopic3 = (note_Activity.Count > 2) ? note_Activity[2].Activity.Theme.Id : 0,
                    Topics3 = (list3.Count != 0) ? list3 : _combosHelper.GetComboThemesByClinic(workday_Client.Client.Clinic.Id),
                    IdActivity3 = (note_Activity.Count > 2) ? note_Activity[2].Activity.Id : 0,
                    Activities3 = _combosHelper.GetComboActivitiesByTheme((note_Activity.Count > 2) ? note_Activity[2].Activity.Theme.Id : 0),
                    AnswerClient3 = note_Activity[2].AnswerClient,
                    AnswerFacilitator3 = note_Activity[2].AnswerFacilitator,

                    IdTopic4 = (note_Activity.Count > 3) ? note_Activity[3].Activity.Theme.Id : 0,
                    Topics4 = (list4.Count != 0) ? list4 : _combosHelper.GetComboThemesByClinic(workday_Client.Client.Clinic.Id),
                    IdActivity4 = (note_Activity.Count > 3) ? note_Activity[3].Activity.Id : 0,
                    Activities4 = _combosHelper.GetComboActivitiesByTheme((note_Activity.Count > 3) ? note_Activity[3].Activity.Theme.Id : 0),
                    AnswerClient4 = note_Activity[3].AnswerClient,
                    AnswerFacilitator4 = note_Activity[3].AnswerFacilitator
                };
            }
            return View(noteViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Facilitator")]
        public async Task<IActionResult> EditNote(NoteViewModel model)
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
                NoteEntity note = await _context.Notes.Include(n => n.Workday_Cient).FirstOrDefaultAsync(n => n.Workday_Cient.Id == model.Id);
                Note_Activity note_Activity;
                if (note == null)   //la nota no está creada
                {
                    noteEntity = await _converterHelper.ToNoteEntity(model, true);
                    _context.Add(noteEntity);
                    note_Activity = new Note_Activity
                    {
                        Note = noteEntity,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity1),
                        AnswerClient = model.AnswerClient1,
                        AnswerFacilitator = model.AnswerFacilitator1
                    };
                    _context.Add(note_Activity);
                    note_Activity = new Note_Activity
                    {
                        Note = noteEntity,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity2),
                        AnswerClient = model.AnswerClient2,
                        AnswerFacilitator = model.AnswerFacilitator2
                    };
                    _context.Add(note_Activity);
                    note_Activity = new Note_Activity
                    {
                        Note = noteEntity,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity3),
                        AnswerClient = model.AnswerClient3,
                        AnswerFacilitator = model.AnswerFacilitator3
                    };
                    _context.Add(note_Activity);
                    note_Activity = new Note_Activity
                    {
                        Note = noteEntity,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity4),
                        AnswerClient = model.AnswerClient4,
                        AnswerFacilitator = model.AnswerFacilitator4
                    };
                    _context.Add(note_Activity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
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
                        AnswerFacilitator = model.AnswerFacilitator1
                    };
                    _context.Add(note_Activity);
                    note_Activity = new Note_Activity
                    {
                        Note = note,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity2),
                        AnswerClient = model.AnswerClient2,
                        AnswerFacilitator = model.AnswerFacilitator2
                    };
                    _context.Add(note_Activity);
                    note_Activity = new Note_Activity
                    {
                        Note = note,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity3),
                        AnswerClient = model.AnswerClient3,
                        AnswerFacilitator = model.AnswerFacilitator3
                    };
                    _context.Add(note_Activity);
                    note_Activity = new Note_Activity
                    {
                        Note = note,
                        Activity = _context.Activities.FirstOrDefault(a => a.Id == model.IdActivity4),
                        AnswerClient = model.AnswerClient4,
                        AnswerFacilitator = model.AnswerFacilitator4
                    };
                    _context.Add(note_Activity);

                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> FinishEditing(int id)
        {
            Workday_Client workday_Client = await _context.Workdays_Clients.FirstOrDefaultAsync(wc => wc.Id == id);
            if (workday_Client == null)
            {
                return NotFound();
            }

            NoteEntity note = await _context.Notes.Include(n => n.Workday_Cient).FirstOrDefaultAsync(n => n.Workday_Cient.Id == id);
            note.Status = NoteStatus.Pending;
            _context.Update(note);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin, Facilitator")]
        public JsonResult GetActivityList(int idTheme)
        {
            List<ActivityEntity> activities = _context.Activities.Where(a => a.Theme.Id == idTheme).ToList();

            return Json(new SelectList(activities, "Id", "Name"));
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
        public async Task<IActionResult> ApproveNote(int id)
        {
            Workday_Client workday_Client = await _context.Workdays_Clients.Include(wc => wc.Workday)
                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Clinic)
                                                                           .Include(wc => wc.Client)
                                                                           .ThenInclude(c => c.Group)
                                                                           .ThenInclude(g => g.Facilitator)
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
                                                                .ThenInclude(a => a.Theme).Where(na => na.Note.Id == note.Id).ToListAsync();
            noteViewModel = new NoteViewModel
            {
                Id = id,
                Workday_Cient = workday_Client,
                PlanNote = note.PlanNote,

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

                Topic2 = note_Activity[1].Activity.Theme.Name,
                Activity2 = note_Activity[1].Activity.Name,
                AnswerClient2 = note_Activity[1].AnswerClient,
                AnswerFacilitator2 = note_Activity[1].AnswerFacilitator,

                Topic3 = note_Activity[2].Activity.Theme.Name,
                Activity3 = note_Activity[2].Activity.Name,
                AnswerClient3 = note_Activity[2].AnswerClient,
                AnswerFacilitator3 = note_Activity[2].AnswerFacilitator,

                Topic4 = note_Activity[3].Activity.Theme.Name,
                Activity4 = note_Activity[3].Activity.Name,
                AnswerClient4 = note_Activity[3].AnswerClient,
                AnswerFacilitator4 = note_Activity[3].AnswerFacilitator
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

            NoteEntity note = await _context.Notes.FirstOrDefaultAsync(n => n.Workday_Cient.Id == model.Id);
            note.Status = NoteStatus.Approved;
            note.Supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.LinkedUser == User.Identity.Name);
            _context.Update(note);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(NotesSupervision));
        }
    }
}