using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    public class PromotionController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IMimeType _mimeType;
        private readonly IExportExcellHelper _exportExcelHelper;
        private readonly IFileHelper _fileHelper;
        private readonly IReportHelper _reportHelper;
        private readonly IWebHostEnvironment _webhostEnvironment;

        public IConfiguration Configuration { get; }

        public PromotionController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper, IMimeType mimeType, IExportExcellHelper exportExcelHelper, IFileHelper fileHelper, IReportHelper reportHelper, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
            _imageHelper = imageHelper;
            _mimeType = mimeType;
            _exportExcelHelper = exportExcelHelper;
            _fileHelper = fileHelper;
            _reportHelper = reportHelper;
            _webhostEnvironment = webHostEnvironment;
            Configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            UserEntity user_logged = await  _context.Users
                                                    .Include(u => u.Clinic)
                                                    .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
          

            if (User.IsInRole("Admin"))
            {
                return View(await _context.Promotions
                                          .Include(i => i.Photos)
                                          .ToListAsync());
            }

            return View(null);
        }

        [Authorize(Roles = "Admin")]
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

            PromotionViewModel model;

            if (User.IsInRole("Admin"))
            {
                model = new PromotionViewModel
                {
                    Active = true,
                    Name ="",
                    Description = string.Empty,
                    Contact = string.Empty,
                    LinkReferred = string.Empty,
                    OpenDate = DateTime.Today,
                    CloseDate = DateTime.Today,
                    Precio = 0,
                    Location = string.Empty,
                    Promotion = string.Empty,
                    Room = string.Empty

                };
                return View(model);
            }

           
            return View(null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(PromotionViewModel model)
        {
            if (ModelState.IsValid)
            {

                PromotionEntity entity = _converterHelper.ToPromotionEntity(model, true);

                _context.Add(entity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<PromotionEntity> promotions = await _context.Promotions.Include(n => n.Photos).ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewPromotions", promotions) });
                   
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the Case Mannager: {entity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }

        }
            else
            {
                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
                
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id = 0)
        {
            PromotionViewModel model;
            PromotionEntity entity = _context.Promotions.FirstOrDefault(n => n.Id == id);

            if (User.IsInRole("Admin") && entity != null)
            {
                model = _converterHelper.ToPromotionViewModel(entity);
                return View(model);
            }


            return View(null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(PromotionViewModel model)
        {
            if (ModelState.IsValid)
            {

                PromotionEntity entity = _converterHelper.ToPromotionEntity(model, false);

                _context.Update(entity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<PromotionEntity> promotions = await _context.Promotions.Include(n => n.Photos).ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewPromotions", promotions) });

                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the Case Mannager: {entity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }

            }
            else
            {
                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", model) });

            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", model) });
        }


        [Authorize(Roles = "Admin")]
        public IActionResult AddPhoto(int id = 0, int idPromo = 0)
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

            PromotionPhotoViewModel model;

            if (User.IsInRole("Admin"))
            {
                model = new PromotionPhotoViewModel
                {
                    PhotoPath = string.Empty,
                    Description = string.Empty,
                    IdPromotion = idPromo

                };
                return View(model);
            }


            return View(null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddPhoto(PromotionPhotoViewModel model)
        {
            if (ModelState.IsValid)
            {
                string photoPath = string.Empty;
               
                if (model.PhotoFile != null)
                {
                    photoPath = await _imageHelper.UploadImageAsync(model.PhotoFile, "Promotions");
                }
               

                PromotionPhotosEntity entity = _converterHelper.ToPromotionPhotoEntity(model, true, photoPath);

                _context.Add(entity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<PromotionEntity> promotions = await _context.Promotions.Include(n => n.Photos).ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewPromotions", promotions) });

                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the Case Mannager: {entity.Description}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }

            }
            else
            {
                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddPhoto", model) });

            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddPhoto", model) });
        }

        [Authorize(Roles = "Admin")]
        public IActionResult EditPhoto(int id = 0, int idPromo = 0)
        {

            PromotionPhotosEntity entity = _context.PromotionPhotos
                                                   .Include(n => n.Promotion)
                                                   .FirstOrDefault(n => n.Id == id);

            PromotionPhotoViewModel model;

            if (User.IsInRole("Admin") && entity != null)
            {
                model = _converterHelper.ToPromotionPhotoViewModel(entity);
                return View(model);
            }

            return View(null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditPhoto(PromotionPhotoViewModel model)
        {
            if (ModelState.IsValid)
            {
                string photoPath = model.PhotoPath;

                if (model.PhotoFile != null)
                {
                    photoPath = await _imageHelper.UploadImageAsync(model.PhotoFile, "Promotions");
                }


                PromotionPhotosEntity entity = _converterHelper.ToPromotionPhotoEntity(model, false, photoPath);

                _context.Update(entity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<PromotionEntity> promotions = await _context.Promotions.Include(n => n.Photos).ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewPromotions", promotions) });

                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the Case Mannager: {entity.Description}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }

            }
            else
            {
                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditPhoto", model) });

            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditPhoto", model) });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            PromotionPhotosEntity Photo = await _context.PromotionPhotos
                                                        .Include(d => d.Promotion)
                                                        .FirstOrDefaultAsync(s => s.Id == id);
            if (Photo == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.PromotionPhotos.Remove(Photo);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("Index", "Promotion");
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Details(int id = 0)
        {

            PromotionPhotosEntity entity = _context.PromotionPhotos
                                                   .Include(n => n.Promotion)
                                                   .FirstOrDefault(n => n.Id == id);

            PromotionShowViewModel model = new PromotionShowViewModel();

            if (User.IsInRole("Admin") && entity != null)
            {
                model.Location = entity.Promotion.Location;
                model.Precio = entity.Promotion.Precio;
                model.Promotion = entity.Promotion.Promotion;
                model.IdPromotion = entity.Promotion.Id;
                model.Room = entity.Promotion.Room;
                model.Name = entity.Promotion.Name;
                model.CloseDate = entity.Promotion.CloseDate;
                model.OpenDate = entity.Promotion.OpenDate;
                model.LinkReferred = entity.Promotion.LinkReferred;
                model.Contact = entity.Promotion.Contact;
                model.Description = entity.Promotion.Description;
                model.Photos = _context.Promotions.Include(m => m.Photos).FirstOrDefault(n => n.Id == entity.Promotion.Id).Photos;
                model.PhotoPath = entity.PhotoPath;

                return View(model);
            }

            return View(null);
        }

    }
}
