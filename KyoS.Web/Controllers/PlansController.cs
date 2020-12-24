using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
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
    public class PlansController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;

        public PlansController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Plans.OrderBy(p => p.Id).ToListAsync());
        }

        public async Task<IActionResult> Create(int id = 0, int idPlan = 0)
        {
            if (id == 1)
            {
                ViewBag.Creado = "Y";
                ViewBag.IdCreado = idPlan.ToString();
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

            MultiSelectList classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name");
            ViewData["classification"] = classification_list;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlanEntity entity, IFormCollection form)
        {
            MultiSelectList classification_list;
            if (ModelState.IsValid)
            {
                PlanEntity plan = await _context.Plans.FirstOrDefaultAsync(p => p.Text == entity.Text);

                if (plan == null)
                {
                    _context.Add(entity);

                    if (!string.IsNullOrEmpty(form["classifications"]))
                    {
                        string[] classifications = form["classifications"].ToString().Split(',');
                        Plan_Classification planclassification;
                        foreach (string value in classifications)
                        {
                            planclassification = new Plan_Classification()
                            {
                                Plan = entity,
                                Classification = await _context.Classifications.FindAsync(Convert.ToInt32(value))
                            };
                            _context.Add(planclassification);
                        }
                    }

                    try
                    {
                            await _context.SaveChangesAsync();
                            PlanEntity planEntityCreated = await _context.Plans.LastAsync();

                            classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name");
                            ViewData["classification"] = classification_list;
                            return RedirectToAction("Create", new { id = 1, idPlan = planEntityCreated.Id });
                    }
                    catch (System.Exception ex)
                    {
                            if (ex.InnerException.Message.Contains("duplicate"))
                            {
                                ModelState.AddModelError(string.Empty, "Already exists the plan");
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
            
            classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name");
            ViewData["classification"] = classification_list;
            return View(entity);               
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PlanEntity planEntity = await _context.Plans.FirstOrDefaultAsync(p => p.Id == id);
            if (planEntity == null)
            {
                return NotFound();
            }

            _context.Plans.Remove(planEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PlanEntity planEntity = await _context.Plans.Include(p => p.Classifications).FirstOrDefaultAsync(p => p.Id == id);
            
            if (planEntity == null)
            {
                return NotFound();
            }

            List<ClassificationEntity> list = await (from cl in _context.Classifications
                                                     join c in planEntity.Classifications on cl.Id equals c.Classification.Id
                                                     select cl).ToListAsync();

            MultiSelectList classification_list = new MultiSelectList(await _context.Classifications.ToListAsync(), "Id", "Name", list.Select(l => l.Id));
            ViewData["classification"] = classification_list;

            return View(planEntity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PlanEntity planEntity, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                _context.Update(planEntity);

                PlanEntity original_classifications = await _context.Plans.Include(p => p.Classifications)
                                                                                  .ThenInclude(pc => pc.Classification).FirstOrDefaultAsync(p => p.Id == planEntity.Id);
                _context.RemoveRange(original_classifications.Classifications);

                if (!string.IsNullOrEmpty(form["classifications"]))
                {
                    string[] classifications = form["classifications"].ToString().Split(',');
                    Plan_Classification planclassification;
                    foreach (string value in classifications)
                    {
                        planclassification = new Plan_Classification()
                        {
                            Plan = planEntity,
                            Classification = await _context.Classifications.FindAsync(Convert.ToInt32(value))
                        };
                        _context.Add(planclassification);
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
                        ModelState.AddModelError(string.Empty, "Already exists the plan");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(planEntity);
        }
    }
}