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
    [Authorize(Roles = "Admin, Supervisor, Mannager")]
    public class MTPsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        public MTPsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Admin"))
                return View(await _context.MTPs.Include(m => m.Client).ThenInclude(c => c.Clinic).
                                                                        OrderBy(m => m.Client.Clinic.Name).ToListAsync());
            else
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic == null)
                    return View(await _context.MTPs.Include(m => m.Client).ThenInclude(c => c.Clinic).
                                                                        OrderBy(m => m.Client.Clinic.Name).ToListAsync());

                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                if (clinic != null)
                    return View(await _context.MTPs.Include(m => m.Client).ThenInclude(c => c.Clinic).
                                                                        Where(m => m.Client.Clinic.Id == clinic.Id).
                                                                        OrderBy(m => m.Client.Clinic.Name).ToListAsync());
                else
                    return View(await _context.MTPs.Include(m => m.Client).ThenInclude(c => c.Clinic).
                                                                        OrderBy(m => m.Client.Clinic.Name).ToListAsync());
            }
        }

        public async Task<IActionResult> Create(int id = 0)
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
                    ViewBag.Creado = "N";
                }
            }

            //delete all DiagnosisTemp
            IQueryable<DiagnosisTempEntity> list_to_delete = _context.DiagnosesTemp;
            foreach (DiagnosisTempEntity item in list_to_delete)
            {
                _context.DiagnosesTemp.Remove(item);
            }
            await _context.SaveChangesAsync();

            MTPViewModel model = new MTPViewModel();

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    model = new MTPViewModel
                    {
                        Clients = _combosHelper.GetComboClientsByClinic(user_logged.Clinic.Id),
                        AdmisionDate = DateTime.Today,
                        MTPDevelopedDate = DateTime.Today
                    };
                    return View(model);
                }
            }

            model = new MTPViewModel
            {
                Clients = _combosHelper.GetComboClients(),
                AdmisionDate = DateTime.Today,
                MTPDevelopedDate = DateTime.Today
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MTPViewModel mtpViewModel)
        {
            if (ModelState.IsValid)
            {
                MTPEntity mtpEntity = await _converterHelper.ToMTPEntity(mtpViewModel, true);
                _context.Add(mtpEntity);

                IQueryable<DiagnosisTempEntity> list_to_delete = _context.DiagnosesTemp;
                DiagnosisEntity diagnosis;
                foreach (DiagnosisTempEntity item in list_to_delete)
                {
                    diagnosis = new DiagnosisEntity
                    {
                        Code = item.Code,
                        Description = item.Description,
                        MTP = mtpEntity
                    };
                    _context.Add(diagnosis);
                    _context.DiagnosesTemp.Remove(item);
                }

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Create", new { id = 1 });
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

        public IActionResult AddDiagnosis(int id = 0)
        {
            if (id == 0)
            {
                return View(new DiagnosisTempEntity());
            }
            else
            {
                //Edit
                return View(new DiagnosisEntity());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDiagnosis(int id, DiagnosisTempEntity diagnosisTempModel)
        {
            if (ModelState.IsValid)
            {
                if (id == 0)
                {
                    _context.Add(diagnosisTempModel);
                    await _context.SaveChangesAsync();
                }
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewDiagnosis", _context.DiagnosesTemp.ToList()) });
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddDiagnosis", diagnosisTempModel) });
        }

        public IActionResult AddDiagnosisEntity(int id = 0)
        {
            if (id == 0)
            {
                return View(new DiagnosisViewModel());
            }
            else
            {
                DiagnosisViewModel model = new DiagnosisViewModel()
                {
                    IdMTP = id
                };

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDiagnosisEntity(int id, DiagnosisViewModel diagnosisViewModel)
        {
            if (ModelState.IsValid)
            {
                DiagnosisEntity model = await _converterHelper.ToDiagnosisEntity(diagnosisViewModel, true);
                _context.Add(model);
                await _context.SaveChangesAsync();

                //return RedirectToAction($"{nameof(UpdateDiagnosis)}/{id}");
                return RedirectToAction("UpdateDiagnosis", new { id });
            }
            return RedirectToAction("UpdateDiagnosis", new { id });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MTPEntity mtpEntity = await _context.MTPs.FirstOrDefaultAsync(t => t.Id == id);
            if (mtpEntity == null)
            {
                return NotFound();
            }

            _context.MTPs.Remove(mtpEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MTPEntity mtpEntity = await _context.MTPs.Include(m => m.Client)
                                                     .Include(m => m.Diagnosis).FirstOrDefaultAsync(m => m.Id == id);
            if (mtpEntity == null)
            {
                return NotFound();
            }

            MTPViewModel mtpViewModel = _converterHelper.ToMTPViewModel(mtpEntity);

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    mtpViewModel.Clients = _combosHelper.GetComboClientsByClinic(user_logged.Clinic.Id);
                }
            }

            return View(mtpViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MTPViewModel mtpViewModel)
        {
            if (id != mtpViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                MTPEntity mtpEntity = await _converterHelper.ToMTPEntity(mtpViewModel, false);
                _context.Update(mtpEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
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
            return View(mtpViewModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MTPEntity mtpEntity = await _context.MTPs.Include(m => m.Client)                                                                 
                                                                 .ThenInclude(f => f.Clinic)
                                                                 .Include(m => m.Diagnosis)
                                                                 .Include(m => m.Goals)
                                                                 .ThenInclude(g => g.Objetives)
                                                                 .FirstOrDefaultAsync(m => m.Id == id);
            if (mtpEntity == null)
            {
                return NotFound();
            }

            return View(mtpEntity);
        }

        public async Task<IActionResult> UpdateDiagnosis(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MTPEntity mtpEntity = await _context.MTPs.Include(m => m.Diagnosis)
                                                     .Include(m => m.Client).FirstOrDefaultAsync(m => m.Id == id);

            if (mtpEntity == null)
            {
                return NotFound();
            }

            return View(mtpEntity);
        }

        public async Task<IActionResult> EditDiagnosis(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DiagnosisEntity diagnosisEntity = await _context.Diagnoses.Include(d => d.MTP)
                                                                      .ThenInclude(m => m.Client).FirstOrDefaultAsync(d => d.Id == id);
            DiagnosisViewModel model = _converterHelper.ToDiagnosisViewModel(diagnosisEntity);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDiagnosis(int id, DiagnosisViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            model.MTP = await _context.MTPs.Include(m => m.Client).FirstOrDefaultAsync(m => m.Id == model.IdMTP);

            if (ModelState.IsValid)
            {
                DiagnosisEntity diagnosisEntity = await _converterHelper.ToDiagnosisEntity(model, false);
                _context.Update(diagnosisEntity);
                try
                {
                    await _context.SaveChangesAsync();                    
                    return RedirectToAction("UpdateDiagnosis", new { id = model.IdMTP });                    
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the diagnosis");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteDiagnosis(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DiagnosisEntity diagnosisEntity = await _context.Diagnoses.Include(d => d.MTP).FirstOrDefaultAsync(d => d.Id == id);
            if (diagnosisEntity == null)
            {
                return NotFound();
            }

            _context.Diagnoses.Remove(diagnosisEntity);
            await _context.SaveChangesAsync();
            //return RedirectToAction($"{nameof(UpdateDiagnosis)}/{diagnosisEntity.MTP.Id}");
            return RedirectToAction("UpdateDiagnosis", new { diagnosisEntity.MTP.Id });
        }

        public async Task<IActionResult> UpdateGoals(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MTPEntity mtpEntity = await _context.MTPs.Include(m => m.Goals)
                                                     .Include(m => m.Client).FirstOrDefaultAsync(m => m.Id == id);

            if (mtpEntity == null)
            {
                return NotFound();
            }

            return View(mtpEntity);
        }

        public async Task<IActionResult> CreateGoal(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MTPEntity mtpEntity = await _context.MTPs.Include(m => m.Client).FirstOrDefaultAsync(m => m.Id == id);
            if (mtpEntity == null)
            {
                return NotFound();
            }

            GoalViewModel model = new GoalViewModel
            {
                MTP = mtpEntity,
                IdMTP = mtpEntity.Id
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGoal(int id, GoalViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            model.MTP = await _context.MTPs.Include(m => m.Client).FirstOrDefaultAsync(m => m.Id == model.IdMTP);

            if (ModelState.IsValid)
            {
                GoalEntity goalEntity = await _converterHelper.ToGoalEntity(model, true);
                _context.Add(goalEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    //return RedirectToAction($"{nameof(UpdateGoals)}/{model.IdMTP}");
                    return RedirectToAction("UpdateGoals", new { id = model.IdMTP });
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
            return View(model);
        }

        public async Task<IActionResult> DeleteGoal(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GoalEntity goalEntity = await _context.Goals.Include(g => g.MTP).FirstOrDefaultAsync(g => g.Id == id);
            if (goalEntity == null)
            {
                return NotFound();
            }

            _context.Goals.Remove(goalEntity);
            await _context.SaveChangesAsync();
            //return RedirectToAction($"{nameof(UpdateGoals)}/{goalEntity.MTP.Id}");
            return RedirectToAction("UpdateGoals", new { id = goalEntity.MTP.Id });
        }

        public async Task<IActionResult> EditGoal(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GoalEntity goalEntity = await _context.Goals.Include(g => g.MTP)
                                                                       .ThenInclude(m => m.Client).FirstOrDefaultAsync(d => d.Id == id);
            GoalViewModel model = _converterHelper.ToGoalViewModel(goalEntity);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGoal(int id, GoalViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            model.MTP = await _context.MTPs.Include(m => m.Client).FirstOrDefaultAsync(m => m.Id == model.IdMTP);

            if (ModelState.IsValid)
            {
                GoalEntity goalEntity = await _converterHelper.ToGoalEntity(model, false);
                _context.Update(goalEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    //return RedirectToAction($"{nameof(UpdateGoals)}/{model.IdMTP}");
                    return RedirectToAction("UpdateGoals", new { id = model.IdMTP });
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
            return View(model);
        }

        public async Task<IActionResult> UpdateObjectives(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GoalEntity goalEntity = await _context.Goals.Include(g => g.MTP)
                                                     .ThenInclude(m => m.Client)
                                                     .Include(g => g.Objetives).FirstOrDefaultAsync(g => g.Id == id);

            if (goalEntity == null)
            {
                return NotFound();
            }

            return View(goalEntity);
        }

        public async Task<IActionResult> CreateObjective(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GoalEntity goalEntity = await _context.Goals.Include(g => g.MTP)
                                                        .ThenInclude(m => m.Client).FirstOrDefaultAsync(m => m.Id == id);
            if (goalEntity == null)
            {
                return NotFound();
            }

            ObjectiveViewModel model = new ObjectiveViewModel
            {
                Goal = goalEntity,
                IdGoal = goalEntity.Id,
                DateOpened = DateTime.Now,
                DateResolved = DateTime.Now,
                DateTarget = DateTime.Now
            };

            MultiSelectList classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name");
            ViewData["classification"] = classification_list;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateObjective(ObjectiveViewModel model, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
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
                    //return RedirectToAction($"{nameof(UpdateObjectives)}/{model.IdGoal}");
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

        public async Task<IActionResult> DeleteObjective(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ObjetiveEntity objectiveEntity = await _context.Objetives.Include(o => o.Goal).FirstOrDefaultAsync(o => o.Id == id);
            if (objectiveEntity == null)
            {
                return NotFound();
            }

            _context.Objetives.Remove(objectiveEntity);
            await _context.SaveChangesAsync();
            //return RedirectToAction($"{nameof(UpdateObjectives)}/{objectiveEntity.Goal.Id}");
            return RedirectToAction("UpdateObjectives", new { objectiveEntity.Goal.Id });
        }

        public async Task<IActionResult> EditObjective(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ObjetiveEntity objectiveEntity = await _context.Objetives.Include(o => o.Goal)
                                                                       .ThenInclude(g => g.MTP)
                                                                       .ThenInclude(m => m.Client)
                                                                       .Include(o => o.Classifications)
                                                                       .ThenInclude(oc => oc.Classification).FirstOrDefaultAsync(d => d.Id == id);
            ObjectiveViewModel model = _converterHelper.ToObjectiveViewModel(objectiveEntity);
            if (model == null)
            {
                return NotFound();
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
        public async Task<IActionResult> EditObjective(ObjectiveViewModel model, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                ObjetiveEntity objective = await _converterHelper.ToObjectiveEntity(model, false);
                _context.Update(objective);

                ObjetiveEntity original_classifications = await _context.Objetives.Include(o => o.Classifications)
                                                                                  .ThenInclude(oc => oc.Classification).FirstOrDefaultAsync(d => d.Id == model.Id);
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
                    //return RedirectToAction($"{nameof(UpdateObjectives)}/{model.IdGoal}");
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
            return View(model);
        }
    }
}