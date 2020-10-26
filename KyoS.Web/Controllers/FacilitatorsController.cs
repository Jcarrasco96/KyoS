using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    public class FacilitatorsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        public FacilitatorsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Facilitators.Include(f => f.Clinic).OrderBy(f => f.Name).ToListAsync());
        }

        public IActionResult Create(int id = 0)
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

            FacilitatorViewModel model = new FacilitatorViewModel
            {
                Clinics = _combosHelper.GetComboClinics()                
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FacilitatorViewModel facilitatorViewModel)
        {
            if (ModelState.IsValid)
            {
                FacilitatorEntity facilitator = await _context.Facilitators.FirstOrDefaultAsync(f => f.Name == facilitatorViewModel.Name);
                if (facilitator == null)
                {
                    FacilitatorEntity facilitatorEntity = await _converterHelper.ToFacilitatorEntity(facilitatorViewModel, true);

                    _context.Add(facilitatorEntity);
                    try
                    {
                        await _context.SaveChangesAsync();                        
                        return RedirectToAction("Create", new { id = 1});
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the facilitator: {facilitatorEntity.Name}");
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
            return View(facilitatorViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            FacilitatorEntity facilitatorEntity = await _context.Facilitators.FirstOrDefaultAsync(t => t.Id == id);
            if (facilitatorEntity == null)
            {
                return NotFound();
            }

            _context.Facilitators.Remove(facilitatorEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            FacilitatorEntity facilitatorEntity = await _context.Facilitators.Include(f => f.Clinic).FirstOrDefaultAsync(f => f.Id == id);
            if (facilitatorEntity == null)
            {
                return NotFound();
            }

            FacilitatorViewModel facilitatorViewModel = _converterHelper.ToFacilitatorViewModel(facilitatorEntity);
            return View(facilitatorViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FacilitatorViewModel facilitatorViewModel)
        {
            if (id != facilitatorViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                FacilitatorEntity facilitatorEntity = await _converterHelper.ToFacilitatorEntity(facilitatorViewModel, false);
                _context.Update(facilitatorEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the facilitator: {facilitatorEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(facilitatorViewModel);
        }
    }
}