using KyoS.Common.Enums;
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
    [Authorize(Roles = "Manager, Frontdesk")]
    public class TemplatesDOCController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IMimeType _mimeType;

        public TemplatesDOCController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IImageHelper imageHelper, IMimeType mimeType)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
            _imageHelper = imageHelper;
            _mimeType = mimeType;
        }

        public async Task<IActionResult> Index()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null)
            {
                return RedirectToAction("Home/Error404");
            }

            return View(await _context.TemplatesDOC
                                      .Include(t => t.Clinic)
                                      .Where(t => t.Clinic.Id == user_logged.Clinic.Id)
                                      .OrderByDescending(t => t.CreatedOn)
                                      .ToListAsync());
        }

        public IActionResult AddTemplate(int id = 0)
        {
            TemplateDOCViewModel entity = new TemplateDOCViewModel()
            {
                IdDescription = 0,
                Descriptions = _combosHelper.GetComboDocumentDescriptions()
            };
            return View(entity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTemplate(int id, TemplateDOCViewModel templateViewModel)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                string documentPath = string.Empty;
                if (templateViewModel.DocumentFile != null)
                {
                    documentPath = await _imageHelper.UploadFileAsync(templateViewModel.DocumentFile, "Templates");
                }

                if (id == 0)
                {
                    TemplateDOCEntity template = new TemplateDOCEntity
                    {
                        Id = 0,
                        FileUrl = documentPath,
                        FileName = templateViewModel.DocumentFile.FileName,
                        Description = DocumentUtils.GetDocumentByIndex(templateViewModel.IdDescription),
                        Clinic = user_logged.Clinic,
                        CreatedOn = DateTime.Now
                    };
                    _context.Add(template);
                    await _context.SaveChangesAsync();
                }
                return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTemplate", _context.TemplatesDOC.OrderByDescending(d => d.CreatedOn).ToList()) });
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "AddTemplate", templateViewModel) });
        }

        public async Task<IActionResult> OpenDocument(int id)
        {
            TemplateDOCEntity template = await _context.TemplatesDOC
                                                       .FirstOrDefaultAsync(t => t.Id == id);
            if (template == null)
            {
                return RedirectToAction("Home/Error404");
            }
            string mimeType = _mimeType.GetMimeType(template.FileName);
            return File(template.FileUrl, mimeType);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TemplateDOCEntity template = await _context.TemplatesDOC.FirstOrDefaultAsync(c => c.Id == id);
            if (template == null)
            {
                return RedirectToAction("Home/Error404");
            }

            _context.TemplatesDOC.Remove(template);
            await _context.SaveChangesAsync();
            return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTemplate", _context.TemplatesDOC.OrderByDescending(d => d.CreatedOn).ToList()) });
        }
    }
}