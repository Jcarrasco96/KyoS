﻿using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClinicsController : Controller
    {
        private readonly DataContext _context;
        private readonly IImageHelper _imageHelper;
        private readonly IConverterHelper _converterHelper;

        public ClinicsController(DataContext context, IImageHelper imageHelper, IConverterHelper converterHelper)
        {
            _context = context;
            _imageHelper = imageHelper;
            _converterHelper = converterHelper;
        }

        public async Task<IActionResult> Index(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            return View(await _context.Clinics.OrderBy(t => t.Name).ToListAsync());
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
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClinicViewModel clinicViewModel, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Name == clinicViewModel.Name);
                if (clinic == null)
                {
                    string path = string.Empty;

                    if (clinicViewModel.LogoFile != null)
                    {
                        path = await _imageHelper.UploadImageAsync(clinicViewModel.LogoFile, "Clinics");
                    }

                    ClinicEntity clinicEntity = _converterHelper.ToClinicEntity(clinicViewModel, path, true);
                    clinicEntity.Schema = (form["Schema"] == "Schema1") ? SchemaType.Schema1 : SchemaType.Schema2;
                    _context.Add(clinicEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the clinic: {clinicEntity.Name}");
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
            return View(clinicViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ClinicEntity clinicEntity = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == id);
            if (clinicEntity == null)
            {
                return NotFound();
            }

            try
            {
                _context.Clinics.Remove(clinicEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }
            
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ClinicEntity clinicEntity = await _context.Clinics.FindAsync(id);
            if (clinicEntity == null)
            {
                return NotFound();
            }

            ClinicViewModel clinicViewModel = _converterHelper.ToClinicViewModel(clinicEntity);
            ViewBag.Schema = (clinicEntity.Schema == 0) ? "1" : "2";
            return View(clinicViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClinicViewModel clinicViewModel, IFormCollection form)
        {
            if (id != clinicViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                string path = clinicViewModel.LogoPath;

                if (clinicViewModel.LogoFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(clinicViewModel.LogoFile, "Clinics");
                }

                ClinicEntity clinicEntity = _converterHelper.ToClinicEntity(clinicViewModel, path, false);
                clinicEntity.Schema = (form["Schema"] == "Schema1") ? SchemaType.Schema1 : SchemaType.Schema2;
                _context.Update(clinicEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the clinic: {clinicEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(clinicViewModel);
        }
    }
}