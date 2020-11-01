using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SupervisorsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        public SupervisorsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Supervisors.Include(f => f.Clinic).OrderBy(f => f.Name).ToListAsync());
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

            SupervisorViewModel model = new SupervisorViewModel
            {
                Clinics = _combosHelper.GetComboClinics()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupervisorViewModel supervisorViewModel)
        {
            if (ModelState.IsValid)
            {
                SupervisorEntity supervisor = await _context.Supervisors.FirstOrDefaultAsync(s => s.Name == supervisorViewModel.Name);
                if (supervisor == null)
                {
                    SupervisorEntity supervisorEntity = await _converterHelper.ToSupervisorEntity(supervisorViewModel, true);

                    _context.Add(supervisorEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the supervisor: {supervisorEntity.Name}");
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
            return View(supervisorViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SupervisorEntity supervisorEntity = await _context.Supervisors.FirstOrDefaultAsync(s => s.Id == id);
            if (supervisorEntity == null)
            {
                return NotFound();
            }

            _context.Supervisors.Remove(supervisorEntity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            SupervisorEntity supervisorEntity = await _context.Supervisors.Include(s => s.Clinic).FirstOrDefaultAsync(s => s.Id == id);
            if (supervisorEntity == null)
            {
                return NotFound();
            }

            SupervisorViewModel supervisorViewModel = _converterHelper.ToSupervisorViewModel(supervisorEntity);
            return View(supervisorViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupervisorViewModel supervisorViewModel)
        {
            if (id != supervisorViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                SupervisorEntity supervisorEntity = await _converterHelper.ToSupervisorEntity(supervisorViewModel, false);
                _context.Update(supervisorEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the supervisor: {supervisorEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(supervisorViewModel);
        }
    }
}