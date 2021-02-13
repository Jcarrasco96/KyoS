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
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NotesPrototypesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;

        public NotesPrototypesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.NotesPrototypes.Include(n => n.Activity).ThenInclude(a => a.Theme).OrderBy(n => n.Activity.Theme.Name).ToListAsync());
        }
        public async Task<IActionResult> Create(int id = 0, int idNote = 0)
        {
            if (id == 1)
            {
                ViewBag.Creado = "Y";
                ViewBag.IdCreado = idNote.ToString();
            }
            else
            {
                if (id == 2)
                {
                    ViewBag.Creado = "E";
                }
                else
                {
                    ViewBag.Creado = "N";
                }
            }

            NotePrototypeViewModel model = new NotePrototypeViewModel
            {
                Activities = _combosHelper.GetComboActivities(),
                Facilitators = _combosHelper.GetComboFacilitators(),
                Clients = _combosHelper.GetComboClients()
            };

            MultiSelectList classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name");
            ViewData["classification"] = classification_list;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NotePrototypeViewModel noteViewModel, IFormCollection form)
        {
            MultiSelectList classification_list;
            if (ModelState.IsValid)
            {
                if ((noteViewModel.IdClient == 0) && (noteViewModel.IdFacilitator == 0))    //only insert into Note table
                {
                    ActivityEntity activityEntity = await _context.Activities.FirstOrDefaultAsync(t => t.Id == noteViewModel.IdActivity);
                    NotePrototypeEntity note = await _context.NotesPrototypes.FirstOrDefaultAsync((n => (n.AnswerClient == noteViewModel.AnswerClient
                                                                                && n.AnswerFacilitator == noteViewModel.AnswerFacilitator
                                                                                && n.Activity.Id == activityEntity.Id)));
                    if (note == null)
                    {
                        NotePrototypeEntity noteEntity = await _converterHelper.ToNotePrototypeEntity(noteViewModel, true);
                        _context.Add(noteEntity);

                        if (!string.IsNullOrEmpty(form["classifications"]))
                        {
                            string[] classifications = form["classifications"].ToString().Split(',');
                            NotePrototype_Classification noteclassification;
                            foreach (string value in classifications)
                            {
                                noteclassification = new NotePrototype_Classification()
                                {
                                    Note = noteEntity,
                                    Classification = await _context.Classifications.FindAsync(Convert.ToInt32(value))
                                };
                                _context.Add(noteclassification);
                            }
                        }

                        try
                        {
                            await _context.SaveChangesAsync();
                            NotePrototypeEntity noteEntityCreated = await _context.NotesPrototypes.OrderBy(n => n.Id).LastAsync();

                            classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name");
                            ViewData["classification"] = classification_list;
                            return RedirectToAction("Create", new { id = 1, idNote = noteEntityCreated.Id });
                        }
                        catch (System.Exception ex)
                        {
                            if (ex.InnerException.Message.Contains("duplicate"))
                            {
                                ModelState.AddModelError(string.Empty, "Already exists the note");
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                            }
                        }
                    }
                    else
                    {
                        classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name");
                        ViewData["classification"] = classification_list;
                        return RedirectToAction("Create", new { id = 2 });
                    }
                }
                else
                {
                    classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name");
                    ViewData["classification"] = classification_list;
                    return View(noteViewModel);
                }
            }
            classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name");
            ViewData["classification"] = classification_list;
            return View(noteViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            NotePrototypeEntity noteEntity = await _context.NotesPrototypes.FirstOrDefaultAsync(n => n.Id == id);
            if (noteEntity == null)
            {
                return NotFound();
            }

            _context.NotesPrototypes.Remove(noteEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            NotePrototypeEntity noteEntity = await _context.NotesPrototypes.Include(n => n.Activity)
                                                        .Include(n => n.Classifications)
                                                        .ThenInclude(cl => cl.Classification).FirstOrDefaultAsync(n => n.Id == id);
            if (noteEntity == null)
            {
                return NotFound();
            }

            NotePrototypeViewModel noteViewModel = _converterHelper.ToNotePrototypeViewModel(noteEntity);

            List<ClassificationEntity> list = new List<ClassificationEntity>();
            ClassificationEntity classification;
            foreach (NotePrototype_Classification item in noteViewModel.Classifications)
            {
                classification = await _context.Classifications.FindAsync(item.Classification.Id);
                list.Add(classification);
            }

            MultiSelectList classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name", list.Select(l => l.Id));
            ViewData["classification"] = classification_list;

            return View(noteViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NotePrototypeViewModel noteViewModel, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                NotePrototypeEntity noteEntity = await _converterHelper.ToNotePrototypeEntity(noteViewModel, false);
                _context.Update(noteEntity);

                NotePrototypeEntity original_classifications = await _context.NotesPrototypes.Include(n => n.Classifications)
                                                                                  .ThenInclude(nc => nc.Classification).FirstOrDefaultAsync(d => d.Id == noteViewModel.Id);
                _context.RemoveRange(original_classifications.Classifications);

                if (!string.IsNullOrEmpty(form["classifications"]))
                {
                    string[] classifications = form["classifications"].ToString().Split(',');
                    NotePrototype_Classification noteclassification;
                    foreach (string value in classifications)
                    {
                        noteclassification = new NotePrototype_Classification()
                        {
                            Note = noteEntity,
                            Classification = await _context.Classifications.FindAsync(Convert.ToInt32(value))
                        };
                        _context.Add(noteclassification);
                    }
                }

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the note");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(noteViewModel);
        }

        public IActionResult GenerateNotes()
        {
            GenerateNotesViewModel model = new GenerateNotesViewModel
            {
                Groups = _combosHelper.GetComboGroups(),
                Days = _combosHelper.GetComboDays(),
                Date = DateTime.Now.Date
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateNotes(GenerateNotesViewModel model)
        {
            if (ModelState.IsValid)
            {
                GroupEntity group = await _context.Groups
                                                  .Include(g => g.Clients)
                                                  .FirstOrDefaultAsync(g => g.Id == model.IdGroup);


            }
            return View(model);
        }
    }
}