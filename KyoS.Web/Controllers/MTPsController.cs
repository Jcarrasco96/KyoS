using AspNetCore.Reporting;
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
using System.Reflection;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
        public async Task<IActionResult> Index()
        {
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
        public async Task<IActionResult> Create(int id = 0, int idClient = 0)
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
                            AdmisionDate = DateTime.Today,
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
                            AdmisionDate = DateTime.Today,
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
        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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
        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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
        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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

        [Authorize(Roles = "Admin, Supervisor, Mannager")]
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
                DateOpened = goalEntity.MTP.AdmisionDate,
                DateResolved = goalEntity.MTP.AdmisionDate.AddMonths(Convert.ToInt32(goalEntity.MTP.NumberOfMonths)),
                DateTarget = goalEntity.MTP.AdmisionDate.AddMonths(Convert.ToInt32(goalEntity.MTP.NumberOfMonths)),
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
                                               
                                               .Include(m => m.Diagnosis)

                                               .FirstOrDefault(m => (m.Id == id));
            if (mtpEntity == null)
            {
                return NotFound();
            }

            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\MTPs\\{1}.rdlc", fileDirPath, $"rptMTP{mtpEntity.Client.Clinic.Name}");
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            System.Text.Encoding.GetEncoding("windows-1252");
            LocalReport report = new LocalReport(rdlcFilePath);

            //datasource
            List<ClientEntity> clients = new List<ClientEntity> { mtpEntity.Client };
            List<MTPEntity> mtps = new List<MTPEntity> { mtpEntity };
            List<DiagnosisEntity> diagnosis = mtpEntity.Diagnosis.ToList();
            List<GoalEntity> goals1 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives1 = new List<ObjetiveEntity>();
            List<GoalEntity> goals2 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives2 = new List<ObjetiveEntity>();
            List<GoalEntity> goals3 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives3 = new List<ObjetiveEntity>();
            List<GoalEntity> goals4 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives4 = new List<ObjetiveEntity>();
            List<GoalEntity> goals5 = new List<GoalEntity>();
            List<ObjetiveEntity> objetives5 = new List<ObjetiveEntity>();

            int i = 0;
            
            foreach (GoalEntity item in mtpEntity.Goals)
            {
                if (i == 0)
                {
                    goals1 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives1 = item.Objetives.ToList();
                    }
                }
                if (i == 1)
                {
                    goals2 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives2 = item.Objetives.ToList();
                    }
                }
                if (i == 2)
                {
                    goals3 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives3 = item.Objetives.ToList();
                    }
                }
                if (i == 3)
                {
                    goals4 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives4 = item.Objetives.ToList();
                    }
                }
                if (i == 4)
                {
                    goals5 = new List<GoalEntity> { item };
                    if (item.Objetives != null)
                    {
                        objetives5 = item.Objetives.ToList();
                    }
                }
                i = ++i;
            }          

            //report.AddDataSource("dsWorkdays_Clients", workdaysclients);
            report.AddDataSource("dsClients", clients);
            report.AddDataSource("dsMTPs", mtps);
            report.AddDataSource("dsDiagnosis", diagnosis);
            report.AddDataSource("dsGoals1", goals1);
            report.AddDataSource("dsObjetives1", objetives1);
            report.AddDataSource("dsGoals2", goals2);
            report.AddDataSource("dsObjetives2", objetives2);
            report.AddDataSource("dsGoals3", goals3);
            report.AddDataSource("dsObjetives3", objetives3);
            report.AddDataSource("dsGoals4", goals4);
            report.AddDataSource("dsObjetives4", objetives4);
            report.AddDataSource("dsGoals5", goals5);
            report.AddDataSource("dsObjetives5", objetives5);
            
            //var date = $"{workdayClient.Workday.Date.DayOfWeek}, {workdayClient.Workday.Date.ToShortDateString()}";
            //var dateFacilitator = workdayClient.Workday.Date.ToShortDateString();
            //var dateSupervisor = workdayClient.Note.DateOfApprove.Value.ToShortDateString();
            //parameters.Add("date", date);
            //parameters.Add("dateFacilitator", dateFacilitator);
            //parameters.Add("dateSupervisor", dateSupervisor);
            //parameters.Add("num_of_goal", num_of_goal);
            //parameters.Add("goal_text", goal_text);
            //parameters.Add("num_of_obj", num_of_obj);
            //parameters.Add("obj_text", obj_text);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, System.Net.Mime.MediaTypeNames.Application.Octet,
                        $"{mtpEntity.Client.Name} - MTP.pdf");
        }
    }
}