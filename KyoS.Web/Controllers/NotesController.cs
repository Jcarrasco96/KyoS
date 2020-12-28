﻿using KyoS.Web.Data;
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
    public class NotesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        public NotesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Notes.Include(n => n.Activity).ThenInclude(a => a.Theme).OrderBy(n => n.Activity.Theme.Name).ToListAsync());
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

            NoteViewModel model = new NoteViewModel
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
        public async Task<IActionResult> Create(NoteViewModel noteViewModel, IFormCollection form)
        {
            MultiSelectList classification_list;
            if (ModelState.IsValid)
            {
                if ((noteViewModel.IdClient == 0) && (noteViewModel.IdFacilitator == 0))    //only insert into Note table
                {
                    ActivityEntity activityEntity = await _context.Activities.FirstOrDefaultAsync(t => t.Id == noteViewModel.IdActivity);
                    NoteEntity note = await _context.Notes.FirstOrDefaultAsync((n => (n.AnswerClient == noteViewModel.AnswerClient
                                                                                && n.AnswerFacilitator == noteViewModel.AnswerFacilitator
                                                                                && n.Activity.Id == activityEntity.Id)));
                    if (note == null)
                    {
                        NoteEntity noteEntity = await _converterHelper.ToNoteEntity(noteViewModel, true);
                        _context.Add(noteEntity);

                        if (!string.IsNullOrEmpty(form["classifications"]))
                        {
                            string[] classifications = form["classifications"].ToString().Split(',');
                            Note_Classification noteclassification;
                            foreach (string value in classifications)
                            {
                                noteclassification = new Note_Classification()
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
                            NoteEntity noteEntityCreated = await _context.Notes.OrderBy(n => n.Id).LastAsync();

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

            NoteEntity noteEntity = await _context.Notes.FirstOrDefaultAsync(n => n.Id == id);
            if (noteEntity == null)
            {
                return NotFound();
            }

            _context.Notes.Remove(noteEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            NoteEntity noteEntity = await _context.Notes.Include(n => n.Activity)
                                                        .Include(n => n.Classifications)
                                                        .ThenInclude(cl => cl.Classification).FirstOrDefaultAsync(n => n.Id == id);
            if (noteEntity == null)
            {
                return NotFound();
            }

            NoteViewModel noteViewModel = _converterHelper.ToNoteViewModel(noteEntity);

            List<ClassificationEntity> list = new List<ClassificationEntity>();
            ClassificationEntity classification;
            foreach (Note_Classification item in noteViewModel.Classifications)
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
        public async Task<IActionResult> Edit(NoteViewModel noteViewModel, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                NoteEntity noteEntity = await _converterHelper.ToNoteEntity(noteViewModel, false);
                _context.Update(noteEntity);

                NoteEntity original_classifications = await _context.Notes.Include(n => n.Classifications)
                                                                                  .ThenInclude(nc => nc.Classification).FirstOrDefaultAsync(d => d.Id == noteViewModel.Id);
                _context.RemoveRange(original_classifications.Classifications);

                if (!string.IsNullOrEmpty(form["classifications"]))
                {
                    string[] classifications = form["classifications"].ToString().Split(',');
                    Note_Classification noteclassification;
                    foreach (string value in classifications)
                    {
                        noteclassification = new Note_Classification()
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
                Days = _combosHelper.GetComboDays()
            };

            return View(model);
        }
    }
}