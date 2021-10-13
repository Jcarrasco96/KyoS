using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Mannager")]
    public class HealthInsurancesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IMimeType _mimeType;

        public HealthInsurancesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper, IMimeType mimeType)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
            _imageHelper = imageHelper;
            _mimeType = mimeType;
        }

        public async Task<IActionResult> Index(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null)
            {
                return NotFound();
            }

            return View(await _context.HealthInsurances
                                      .Where(hi => hi.Clinic.Id == user_logged.Clinic.Id).OrderBy(c => c.Name).ToListAsync());
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

            HealthInsuranceViewModel entity = new HealthInsuranceViewModel()
            {
                SignedDate = DateTime.Now,
                DurationTime = 12,
                Active = true
            };
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HealthInsuranceViewModel healthInsuranceViewModel)
        {
            if (ModelState.IsValid)
            {
                HealthInsuranceEntity insurance = await _context.HealthInsurances.FirstOrDefaultAsync(c => c.Name == healthInsuranceViewModel.Name);
                if (insurance == null)
                {
                    string documentPath = string.Empty;
                    if (healthInsuranceViewModel.DocumentFile != null)
                    {
                        documentPath = await _imageHelper.UploadFileAsync(healthInsuranceViewModel.DocumentFile, "Insurances");
                    }

                    UserEntity user_logged = _context.Users
                                                     .Include(u => u.Clinic)
                                                     .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    healthInsuranceViewModel.IdClinic = user_logged.Clinic.Id;

                    HealthInsuranceEntity healthEnsuranceEntity = await _converterHelper.ToHealthInsuranceEntity(healthInsuranceViewModel, true, user_logged.Id, documentPath);
                    _context.Add(healthEnsuranceEntity);

                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the health insurance: {healthEnsuranceEntity.Name}");
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
            return View(healthInsuranceViewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            HealthInsuranceEntity entity = await _context.HealthInsurances
                                                         .Include(hi => hi.Clinic)
                                                         .FirstOrDefaultAsync(hi => hi.Id == id);
            if (entity == null)
            {
                return NotFound();
            }

            HealthInsuranceViewModel healthInsuranceViewModel = _converterHelper.ToHealthInsuranceViewModel(entity);            
            return View(healthInsuranceViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HealthInsuranceViewModel healthInsuranceViewModel)
        {
            if (id != healthInsuranceViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                string path = healthInsuranceViewModel.DocumentPath;

                if (healthInsuranceViewModel.DocumentFile != null)
                {
                    path = await _imageHelper.UploadFileAsync(healthInsuranceViewModel.DocumentFile, "Insurances");
                }

                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                HealthInsuranceEntity healthInsuranceEntity = await _converterHelper.ToHealthInsuranceEntity(healthInsuranceViewModel, false, user_logged.Id, path);
                _context.Update(healthInsuranceEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the health insurance: {healthInsuranceEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(healthInsuranceViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            HealthInsuranceEntity healthInsurancesEntity = await _context.HealthInsurances.FirstOrDefaultAsync(hi => hi.Id == id);
            if (healthInsurancesEntity == null)
            {
                return NotFound();
            }

            try
            {
                _context.HealthInsurances.Remove(healthInsurancesEntity);
                await _context.SaveChangesAsync();
            }
            catch (System.Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> OpenDocument(int id)
        {
            HealthInsuranceEntity entity = await _context.HealthInsurances
                                                         .FirstOrDefaultAsync(t => t.Id == id);
            if (entity == null)
            {
                return NotFound();
            }
            string mimeType = _mimeType.GetMimeType(entity.DocumentPath);
            return File(entity.DocumentPath, mimeType);
        }
    }
}