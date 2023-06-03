using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
    public class DocumentsAssistantsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IImageHelper _imageHelper;
        public DocumentsAssistantsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IImageHelper imageHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _imageHelper = imageHelper;
        }
        
        public async Task<IActionResult> Index(int idError = 0)
        {
            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }

            if (User.IsInRole("Admin"))
                return View(await _context.Supervisors.Include(f => f.Clinic).OrderBy(f => f.Name).ToListAsync());
            else
            {
                UserEntity user_logged = _context.Users

                                                 .Include(u => u.Clinic)
                                                 .ThenInclude(c => c.Setting)

                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }               
                         
                return View(await _context.DocumentsAssistant
                                          .Include(f => f.Clinic)
                                          .Where(s => s.Clinic.Id == user_logged.Clinic.Id)
                                          .OrderBy(f => f.Name).ToListAsync());             
            }
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

            DocumentsAssistantViewModel model;

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = clinic.Name,
                        Value = $"{clinic.Id}"
                    });
                    model = new DocumentsAssistantViewModel
                    {
                        Clinics = list,
                        IdClinic = clinic.Id,
                        UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Documents_Assistant, user_logged.Clinic.Id)
                    };                    
                    return View(model);
                }
            }
            
            model = new DocumentsAssistantViewModel
            {
                 Clinics = _combosHelper.GetComboClinics(),
                 UserList = _combosHelper.GetComboUserNamesByRolesClinic(UserType.Documents_Assistant, 0)
            };
            return View(model);                        
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocumentsAssistantViewModel documentAssistantViewModel)
        {
            if (ModelState.IsValid)
            {
                DocumentsAssistantEntity supervisor = await _context.DocumentsAssistant.FirstOrDefaultAsync(s => s.Name == documentAssistantViewModel.Name);
                if (supervisor == null)
                {
                    if (documentAssistantViewModel.IdUser == "0")
                    {
                        ModelState.AddModelError(string.Empty, "You must select a linked user");
                        return View(documentAssistantViewModel);
                    }

                    string path = string.Empty;
                    if (documentAssistantViewModel.SignatureFile != null)
                    {
                        path = await _imageHelper.UploadImageAsync(documentAssistantViewModel.SignatureFile, "Signatures");
                    }

                    DocumentsAssistantEntity documentsEntity = await _converterHelper.ToDocumentsAssistantEntity(documentAssistantViewModel, path, true);

                    _context.Add(documentsEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the documents assistant: {documentsEntity.Name}");
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
            return View(documentAssistantViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DocumentsAssistantEntity documentsAssistantEntity = await _context.DocumentsAssistant.FirstOrDefaultAsync(s => s.Id == id);
            if (documentsAssistantEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }        

            try
            {
                _context.DocumentsAssistant.Remove(documentsAssistantEntity);
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

            DocumentsAssistantEntity dosumentAssistantEntity = await _context.DocumentsAssistant.Include(s => s.Clinic).FirstOrDefaultAsync(s => s.Id == id);
            if (dosumentAssistantEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DocumentsAssistantViewModel documentsAssistantViewModel;

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
                documentsAssistantViewModel = _converterHelper.ToDocumentsAssistantViewModel(dosumentAssistantEntity, user_logged.Clinic.Id);
                if (user_logged.Clinic != null)
                {
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = user_logged.Clinic.Name,
                        Value = $"{user_logged.Clinic.Id}"
                    });
                    documentsAssistantViewModel.Clinics = list;
                }
            }            
            else
                documentsAssistantViewModel = _converterHelper.ToDocumentsAssistantViewModel(dosumentAssistantEntity, 0);

            return View(documentsAssistantViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DocumentsAssistantViewModel documentsAssistantViewModel)
        {
            if (id != documentsAssistantViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                string path = documentsAssistantViewModel.SignaturePath;
                if (documentsAssistantViewModel.SignatureFile != null)
                {
                    path = await _imageHelper.UploadImageAsync(documentsAssistantViewModel.SignatureFile, "Signatures");
                }
                DocumentsAssistantEntity documentsAssitantEntity = await _converterHelper.ToDocumentsAssistantEntity(documentsAssistantViewModel, path, false);
                _context.Update(documentsAssitantEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the documents assistant: {documentsAssitantEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(documentsAssistantViewModel);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Signatures()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }


            return View(await _context.DocumentsAssistant
                                          .Include(f => f.Clinic)
                                          .Where(s => s.Clinic.Id == user_logged.Clinic.Id)
                                          .OrderBy(f => f.Name).ToListAsync());
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> EditSignature(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DocumentsAssistantEntity assistantEntity = await _context.DocumentsAssistant
                                                                     .Include(n => n.Clinic)
                                                                     .FirstOrDefaultAsync(c => c.Id == id);

            if (assistantEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            DocumentsAssistantViewModel documentViewModel = _converterHelper.ToDocumentsAssistantViewModel(assistantEntity, assistantEntity.Clinic.Id);

            return View(documentViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> SaveDocumentAssistantSignature(string id, string dataUrl)
        {
            string signPath = await _imageHelper.UploadSignatureAsync(dataUrl, "Assistants");

            DocumentsAssistantEntity assistant = await _context.DocumentsAssistant
                                                               .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(id));
            if (assistant != null)
            {
                assistant.SignaturePath = signPath;
                _context.Update(assistant);
                await _context.SaveChangesAsync();
            }

            return Json(new { redirectToUrl = Url.Action("Signatures", "DocumentsAssistants") });
        }
    }
}