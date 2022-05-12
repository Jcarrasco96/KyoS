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

        [Authorize(Roles = "Supervisor, Mannager, Facilitator")]
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
                                          .Include(m => m.MtpReview)
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

                    return View(await _context.MTPs
                                              .Include(m => m.Client)
                                              .ThenInclude(c => c.Clinic)
                                              .Include(m => m.MtpReview)
                                              .Where(m => m.Client.Clinic.Id == clinic.Id)
                                              .OrderBy(m => m.Client.Clinic.Name).ToListAsync());
                }
                else
                    return View(null);
            }
        }

        [Authorize(Roles = "Supervisor, Mannager, Facilitator")]
        public IActionResult Create(int id = 0, int idClient = 0, bool review = false)
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
                            MTPDevelopedDate = DateTime.Today,
                            NumberOfMonths = 6,
                            Modality = "PSR",
                            Frecuency = "Four times per week",
                            Setting = "53",
                            Review = review
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
                            Review = review
                        };
                    }
                    
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
        [Authorize(Roles = "Supervisor, Mannager, Facilitator")]
        public async Task<IActionResult> Create(MTPViewModel mtpViewModel, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                ClientEntity client = await _context.Clients.FindAsync(mtpViewModel.IdClient);
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
                            Review = mtpViewModel.Review
                        };
                        return View(model);
                    }
                }

                MTPEntity mtpEntity = await _converterHelper.ToMTPEntity(mtpViewModel, true);
                mtpEntity.Setting = form["Setting"].ToString();

                //set all mtps of this client non active
                List<MTPEntity> mtp_list = _context.MTPs.Where(m => m.Client == mtpEntity.Client).ToList();
                foreach (MTPEntity item in mtp_list)
                {
                    item.Active = false;
                    _context.Update(item);
                }

                if(mtpViewModel.Review == true)
                {
                    mtpEntity.MtpReview = new MTPReviewEntity();
                    mtpEntity.MtpReview.CreatedBy = user_logged.Id;
                    mtpEntity.MtpReview.CreatedOn = DateTime.Now;
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

        [Authorize(Roles = "Supervisor, Mannager")]
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

        [Authorize(Roles = "Supervisor, Mannager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MTPEntity mtpEntity = await _context.MTPs.Include(m => m.Client)
                                                     .FirstOrDefaultAsync(m => m.Id == id);
            if (mtpEntity == null)
            {
                return RedirectToAction("Home/Error404");
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
        [Authorize(Roles = "Supervisor, Mannager")]
        public async Task<IActionResult> Edit(int id, MTPViewModel mtpViewModel, IFormCollection form)
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

                MTPEntity mtpEntity = await _converterHelper.ToMTPEntity(mtpViewModel, false);

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
                            Setting = form["Setting"].ToString()
                        };
                        return View(model);
                    }
                }

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

        [Authorize(Roles = "Supervisor, Mannager, Facilitator")]
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

                                                     .Include(m => m.Goals)

                                                     .ThenInclude(g => g.Objetives)

                                                     .FirstOrDefaultAsync(m => m.Id == id);
            if (mtpEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            return View(mtpEntity);
        }

        [Authorize(Roles = "Supervisor, Mannager, Facilitator")]
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

                                                .Include(m => m.Goals)
                                                .ThenInclude(g => g.Objetives)

                                                .Include(m => m.Client)

                                                .FirstOrDefaultAsync(m => m.Id == id);

            if (mtpEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            return View(mtpEntity);
        }

        [Authorize(Roles = "Supervisor, Mannager, Facilitator")]
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
        [Authorize(Roles = "Supervisor, Mannager, Facilitator")]
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
                    if(model.IdAdendum == 0)
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
        public async Task<IActionResult> DeleteGoal(int? id)
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

            return RedirectToAction("UpdateGoals", new { id = goalEntity.MTP.Id });
        }

        [Authorize(Roles = "Supervisor, Mannager, Facilitator")]
        public async Task<IActionResult> EditGoal(int? id, int idAdendum)
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
        [Authorize(Roles = "Supervisor, Mannager, Facilitator")]
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
                    if(model.IdAdendum == 0)
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

        [Authorize(Roles = "Supervisor, Mannager, Facilitator")]
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

        [Authorize(Roles = "Supervisor, Mannager, Facilitator")]
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
        [Authorize(Roles = "Supervisor, Mannager, Facilitator")]
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

        [Authorize(Roles = "Supervisor, Mannager")]
        public async Task<IActionResult> DeleteObjective(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ObjetiveEntity objectiveEntity = await _context.Objetives.Include(o => o.Goal).FirstOrDefaultAsync(o => o.Id == id);
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

            return RedirectToAction("UpdateObjectives", new { objectiveEntity.Goal.Id });
        }

        [Authorize(Roles = "Supervisor, Mannager, Facilitator")]
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
        [Authorize(Roles = "Supervisor, Mannager, Facilitator")]
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

        [Authorize(Roles = "Supervisor, Mannager")]
        public async Task<IActionResult> ExpiredMTP()
        {
            UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                         .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
                return View(null);

            ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
            if (clinic != null)
            {
                List<MTPEntity> mtps = await _context.MTPs
                                                     .Include(m => m.Client)
                                                     .ThenInclude(c => c.Clinic)
                                                     .Where(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                                && m.Active == true && m.Client.Status == StatusType.Open)).ToListAsync();
                List<MTPEntity> expiredMTPs = new List<MTPEntity>();
                foreach (var item in mtps)
                {
                    if (item.NumberOfMonths != null)
                    {
                        if (DateTime.Now > item.MTPDevelopedDate.Date.AddMonths(Convert.ToInt32(item.NumberOfMonths)))
                        {
                            expiredMTPs.Add(item);
                        }
                    }
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

        [Authorize(Roles = "Mannager, Supervisor, Facilitator")]
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
                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                if (clinic != null)
                {
                    return View(await _context.MTPs
                                              .Include(m => m.AdendumList)
                                              .ThenInclude(c => c.Facilitator)
                                              .Include(c => c.Client)
                                              .ThenInclude(c => c.Clinic)
                                              .Where(m => m.Client.Clinic.Id == clinic.Id)
                                              .OrderBy(m => m.Client.Clinic.Name).ToListAsync());

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
                    IdFacilitator = _context.Facilitators.FirstOrDefault(n => n.Name == user_logged.FullName).Id,
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
                if (adendumEntity == null)
                {
                    
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
        public IActionResult EditAdendum(int id = 0)
        {
            AdendumViewModel model;

            if (User.IsInRole("Supervisor")|| User.IsInRole("Facilitator"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    AdendumEntity Adendum = _context.Adendums
                                                      .Include(m => m.Mtp)
                                                      .ThenInclude(m => m.Client)
                                                      .ThenInclude(c => c.Clients_Diagnostics)
                                                      .ThenInclude(cd => cd.Diagnostic)
                                                      .Include(m => m.Goals)
                                                      .ThenInclude(cd => cd.Objetives)
                                                      .Include(m => m.Supervisor)
                                                      .Include(m => m.Facilitator)
                                                      .FirstOrDefault(m => m.Id == id);
                    if (Adendum == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToAdendumViewModel(Adendum);

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
                _context.Adendums.Update(adendumEntity);
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
        public async Task<IActionResult> ApproveAdendum(int id)
        {
            AdendumEntity adendum = await _context.Adendums.FirstOrDefaultAsync(n => n.Id == id);
            adendum.Status = AdendumStatus.Approved;
            _context.Update(adendum);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PendingAdendum));
        }

        [Authorize(Roles = "Supervisor, Mannager")]
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
                    return View(await _context.Adendums
                                              .Include(c => c.Mtp)
                                              .ThenInclude(c => c.Client)
                                              .ThenInclude(c => c.Clinic)
                                              .Include(c => c.Goals)
                                              .ThenInclude(c => c.Objetives)
                                              .Where(m => (m.Mtp.Client.Clinic.Id == clinic.Id)
                                                    && m.Status == AdendumStatus.Pending)
                                              .OrderBy(m => m.Mtp.Client.Clinic.Name).ToListAsync());

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

        [Authorize(Roles = "Supervisor, Mannager")]
        public async Task<IActionResult> EditMTPReview(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MTPReviewEntity mtpReviewEntity = await _context.MTPReviews.Include(m => m.Mtp.Client)
                                                                       .ThenInclude(m => m.Clinic)
                                                                       .FirstOrDefaultAsync(m => m.Id == id);
            if (mtpReviewEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MTPReviewViewModel mtpReviewViewModel = _converterHelper.ToMTPReviewViewModel(mtpReviewEntity);

            return View(mtpReviewViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Supervisor, Mannager")]
        public async Task<IActionResult> EditMTPReview(int id, MTPReviewViewModel mtpReviewViewModel, IFormCollection form)
        {
            if (id != mtpReviewViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

                MTPReviewEntity mtpReviewEntity = await _converterHelper.ToMTPReviewEntity(mtpReviewViewModel, false, user_logged.Id);
                if (mtpReviewEntity != null)
                {
                    mtpReviewEntity.Mtp.StartTime = mtpReviewViewModel.Mtp.StartTime;
                    mtpReviewEntity.Mtp.EndTime = mtpReviewViewModel.Mtp.EndTime;
                    mtpReviewEntity.Mtp.Setting = mtpReviewViewModel.Mtp.Setting;
                    mtpReviewEntity.Mtp.MTPDevelopedDate = mtpReviewViewModel.Mtp.MTPDevelopedDate;
                }
                

                string gender_problems = string.Empty;
               
                _context.Update(mtpReviewEntity);
                
                try
                {
                    await _context.SaveChangesAsync();
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
    }
}