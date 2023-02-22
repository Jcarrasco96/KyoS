using KyoS.Common.Enums;
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
            
            return View(new ClinicViewModel {Id = 0 });
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

                    string pathSignatureClinical = string.Empty;

                    if (clinicViewModel.SignatureFile != null)
                    {
                        pathSignatureClinical = await _imageHelper.UploadImageAsync(clinicViewModel.SignatureFile, "Clinics");
                    }

                    ClinicEntity clinicEntity = _converterHelper.ToClinicEntity(clinicViewModel, path, true, pathSignatureClinical);
                    clinicEntity.Schema = (form["Schema"] == "Schema1") ? SchemaType.Schema1 : (form["Schema"] == "Schema2") ? SchemaType.Schema2 : (form["Schema"] == "Schema3") ? SchemaType.Schema3 : SchemaType.Schema4;
                    clinicEntity.SchemaGroup = (form["SchemaGroup"] == "Schema1") ? SchemaTypeGroup.Schema1 : (form["SchemaGroup"] == "Schema2") ? SchemaTypeGroup.Schema2 : SchemaTypeGroup.Schema3;
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
                return RedirectToAction("Home/Error404");
            }

            ClinicEntity clinicEntity = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == id);
            if (clinicEntity == null)
            {
                return RedirectToAction("Home/Error404");
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
                return RedirectToAction("Home/Error404");
            }

            ClinicEntity clinicEntity = await _context.Clinics.FindAsync(id);
            if (clinicEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ClinicViewModel clinicViewModel = _converterHelper.ToClinicViewModel(clinicEntity);
            ViewBag.Schema = (clinicEntity.Schema == SchemaType.Schema1) ? "1" : (clinicEntity.Schema == SchemaType.Schema2) ? "2" : (clinicEntity.Schema == SchemaType.Schema3) ? "3" : "4";
            ViewBag.SchemaGroup = (clinicEntity.SchemaGroup == SchemaTypeGroup.Schema1) ? "1" : (clinicEntity.SchemaGroup == SchemaTypeGroup.Schema2) ? "2" : "3";
            return View(clinicViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClinicViewModel clinicViewModel, IFormCollection form)
        {
            if (id != clinicViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                string path = clinicViewModel.LogoPath;

                if (clinicViewModel.LogoFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(clinicViewModel.LogoFile, "Clinics");
                }

                string pathSignatureClinical = clinicViewModel.SignaturePath;

                if (clinicViewModel.SignatureFile != null)
                {
                    pathSignatureClinical = await _imageHelper.UploadImageAsync(clinicViewModel.SignatureFile, "Clinics");
                }

                ClinicEntity clinicEntity = _converterHelper.ToClinicEntity(clinicViewModel, path, false, pathSignatureClinical);
                clinicEntity.Schema = (form["Schema"] == "Schema1") ? SchemaType.Schema1 : (form["Schema"] == "Schema2") ? SchemaType.Schema2 : (form["Schema"] == "Schema3") ? SchemaType.Schema3 : SchemaType.Schema4;
                clinicEntity.SchemaGroup = (form["SchemaGroup"] == "Schema1") ? SchemaTypeGroup.Schema1 : (form["SchemaGroup"] == "Schema2") ? SchemaTypeGroup.Schema2 : SchemaTypeGroup.Schema3;
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