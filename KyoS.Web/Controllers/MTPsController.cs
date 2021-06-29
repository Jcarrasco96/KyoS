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
        public MTPsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IReportHelper reportHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
            _reportHelper = reportHelper;
        }

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
        public async Task<IActionResult> Index(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            if (User.IsInRole("Admin"))
            {
                ViewBag.Count = _context.Clients.Where(c => c.MTPs.Count == 0)
                                                          .Count().ToString();
                return View(await _context.MTPs.Include(m => m.Client).ThenInclude(c => c.Clinic).
                                                                        OrderBy(m => m.Client.Clinic.Name).ToListAsync());
            }
            else
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic == null)
                    return View(null);

                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                if (clinic != null)
                {
                    ViewBag.Count = _context.Clients.Where(c => (c.MTPs.Count == 0 && c.Clinic.Id == clinic.Id))
                                                          .Count().ToString();
                    return View(await _context.MTPs.Include(m => m.Client).ThenInclude(c => c.Clinic).
                                                                        Where(m => m.Client.Clinic.Id == clinic.Id).
                                                                        OrderBy(m => m.Client.Clinic.Name).ToListAsync());
                }
                else
                    return View(null);
            }
        }

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
        public IActionResult Create(int id = 0, int idClient = 0)
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
                            AdmisionDate = DateTime.Today.Date,
                            MTPDevelopedDate = DateTime.Today,
                            NumberOfMonths = 6,
                            Modality = "PSR",
                            Frecuency = "Four times per week",
                            Setting = "53"
                        };
                    }
                    else
                    { 
                        model = new MTPViewModel
                        {
                            Clients = _combosHelper.GetComboClientsByClinic(user_logged.Clinic.Id),
                            AdmisionDate = DateTime.Today.Date,
                            MTPDevelopedDate = DateTime.Today,
                            NumberOfMonths = 6,
                            Modality = "PSR",
                            Frecuency = "Four times per week",
                            Setting = "53"
                        };
                    }
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
        [Authorize(Roles = "Admin, Supervisor, Mannager")]
        public async Task<IActionResult> Create(MTPViewModel mtpViewModel, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                MTPEntity mtpEntity = await _converterHelper.ToMTPEntity(mtpViewModel, true);
                mtpEntity.Setting = form["Setting"].ToString();

                //set all mtps of this client non active
                List<MTPEntity> mtp_list = _context.MTPs.Where(m => m.Client == mtpEntity.Client).ToList();                
                foreach (MTPEntity item in mtp_list)
                {
                    item.Active = false;
                    _context.Update(item);
                }     

                _context.Add(mtpEntity);
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MTPEntity mtpEntity = await _context.MTPs
                                                .Include(m => m.Client)
                                                .FirstOrDefaultAsync(t => t.Id == id);
            if (mtpEntity == null)
            {
                return NotFound();
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MTPEntity mtpEntity = await _context.MTPs.Include(m => m.Client)
                                                     .FirstOrDefaultAsync(m => m.Id == id);
            if (mtpEntity == null)
            {
                return NotFound();
            }

            MTPViewModel mtpViewModel = _converterHelper.ToMTPViewModel(mtpEntity);

            if (!User.IsInRole("Admin"))
            {
                
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = mtpEntity.Client.Name,
                    Value = $"{mtpEntity.Client.Id}"
                });
                mtpViewModel.Clients = list;
                
            }

            return View(mtpViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Supervisor, Mannager")]
        public async Task<IActionResult> Edit(int id, MTPViewModel mtpViewModel, IFormCollection form)
        {
            if (id != mtpViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                MTPEntity mtpEntity = await _converterHelper.ToMTPEntity(mtpViewModel, false);
                mtpEntity.Setting = form["Setting"].ToString();
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

        [Authorize(Roles = "Admin, Supervisor, Mannager, Facilitator")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MTPEntity mtpEntity = await _context.MTPs.Include(m => m.Client)                                                                 
                                                     .ThenInclude(f => f.Clinic)

                                                     .Include(m => m.Client)
                                                     .ThenInclude(c => c.Clients_Diagnostics)
                                                     .ThenInclude(cd => cd.Diagnostic)

                                                     .Include(m => m.Goals)

                                                     .ThenInclude(g => g.Objetives)

                                                     .FirstOrDefaultAsync(m => m.Id == id);
            if (mtpEntity == null)
            {
                return NotFound();
            }

            return View(mtpEntity);
        }

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
        public async Task<IActionResult> CreateGoal(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MTPEntity mtpEntity = await _context.MTPs
                                                .Include(m => m.Client)
                                                .Include(m => m.Goals)
                                                .FirstOrDefaultAsync(m => m.Id == id);
            if (mtpEntity == null)
            {
                return NotFound();
            }

            GoalViewModel model = new GoalViewModel
            {
                MTP = mtpEntity,
                IdMTP = mtpEntity.Id,
                Number = mtpEntity.Goals.Count() + 1
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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
        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
        public async Task<IActionResult> CreateObjective(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            GoalEntity goalEntity = await _context.Goals.Include(g => g.MTP)
                                                        .ThenInclude(m => m.Client)
                                                        .Include(g => g.Objetives)
                                                        .FirstOrDefaultAsync(m => m.Id == id);
            if (goalEntity == null)
            {
                return NotFound();
            }

            string objetive = $"{goalEntity.Number}.{goalEntity.Objetives.Count() + 1}";
            ObjectiveViewModel model = new ObjectiveViewModel
            {
                Goal = goalEntity,
                IdGoal = goalEntity.Id,
                DateOpened = goalEntity.MTP.MTPDevelopedDate,
                DateResolved = goalEntity.MTP.MTPDevelopedDate.AddMonths(Convert.ToInt32(goalEntity.MTP.NumberOfMonths)),
                DateTarget = goalEntity.MTP.MTPDevelopedDate.AddMonths(Convert.ToInt32(goalEntity.MTP.NumberOfMonths)),
                Objetive = objetive
            };

            MultiSelectList classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name");
            ViewData["classification"] = classification_list;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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
        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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

        public IActionResult PrintMTP(int id)
        {
            MTPEntity mtpEntity = _context.MTPs.Include(m => m.Client)
                                               .ThenInclude(c => c.Clinic)

                                               .Include(m => m.Goals)
                                               .ThenInclude(g => g.Objetives)
                                               
                                               .Include(wc => wc.Client)
                                               .ThenInclude(c => c.Clients_Diagnostics)
                                               .ThenInclude(cd => cd.Diagnostic)

                                               .FirstOrDefault(m => (m.Id == id));
            if (mtpEntity == null)
            {
                return NotFound();
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
            if (mtpEntity.Client.Clinic.Name == "HEALTH & BEAUTY NGB, INC")
            {
                Stream stream = _reportHelper.HealthAndBeautyMTPReport(mtpEntity);
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
    }
}