using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
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
        public IActionResult Create(int id = 0, int idNote = 0)
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
                Clients = _combosHelper.GetComboClients(),
                Classifications = _combosHelper.GetComboClassifications()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NoteViewModel noteViewModel)
        {
            if (ModelState.IsValid)
            {
                if ((noteViewModel.IdClient == 0) && (noteViewModel.IdFacilitator == 0))    //only insert into Note table
                {
                    ActivityEntity activityEntity = await _context.Activities.FirstOrDefaultAsync(t => t.Id == noteViewModel.IdActivity);
                    NoteClassification classification = NoteClassificationUtils.GetClassificationByIndex(noteViewModel.IdClassification);
                    NoteEntity note = await _context.Notes.FirstOrDefaultAsync((n => (n.AnswerClient == noteViewModel.AnswerClient 
                                                                                && n.AnswerFacilitator == noteViewModel.AnswerFacilitator
                                                                                && n.Clasificacion == classification
                                                                                && n.Activity.Id == activityEntity.Id)));
                    if (note == null)
                    {
                        NoteEntity noteEntity = await _converterHelper.ToNoteEntity(noteViewModel, true);
                        _context.Add(noteEntity);
                        try
                        {
                            await _context.SaveChangesAsync();
                            NoteEntity noteEntityCreated = await _context.Notes.LastAsync();
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
                        return RedirectToAction("Create", new { id = 2 });
                    }
                }
                else
                    return View(noteViewModel);
            }
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

            NoteEntity noteEntity = await _context.Notes.Include(n => n.Activity).FirstOrDefaultAsync(n => n.Id == id);
            if (noteEntity == null)
            {
                return NotFound();
            }

            NoteViewModel noteViewModel = _converterHelper.ToNoteViewModel(noteEntity);
            return View(noteViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, NoteViewModel noteViewModel)
        {
            if (id != noteViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                NoteEntity noteEntity = await _converterHelper.ToNoteEntity(noteViewModel, false);
                _context.Update(noteEntity);
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
    }
}