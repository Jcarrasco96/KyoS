using AspNetCore.Reporting;
using KyoS.Common.Enums;
using KyoS.Common.Helpers;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    public class TCMNotesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly IImageHelper _imageHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IDateHelper _dateHelper;
        private readonly ITranslateHelper _translateHelper;
        private readonly IWebHostEnvironment _webhostEnvironment;
        private readonly IReportHelper _reportHelper;
        private readonly IRenderHelper _renderHelper;

        public TCMNotesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IDateHelper dateHelper, ITranslateHelper translateHelper, IWebHostEnvironment webHostEnvironment, IImageHelper imageHelper, IReportHelper reportHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _dateHelper = dateHelper;
            _translateHelper = translateHelper;
            _webhostEnvironment = webHostEnvironment;
            _imageHelper = imageHelper;
            _reportHelper = reportHelper;
            _renderHelper = renderHelper;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Index(int id = 0)
        {
            if (id == 1)
            {
                ViewBag.FinishEdition = "Y";
            }


            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            return View(await _context.Weeks

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.TCMNote)
                                      .ThenInclude(wc => wc.TCMClient)

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.TCMNote)
                                      .ThenInclude(g => g.CaseManager)

                                      .Include(w => w.Days)
                                      .ThenInclude(d => d.TCMNote)
                                      
                                      .Where(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                && w.Days.Where(d => (d.Service == ServiceType.PSR && d.TCMNote.Where(wc => wc.CaseManager.LinkedUser == User.Identity.Name).Count() > 0)).Count() > 0))
                                      .ToListAsync());
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> TCMNotesForCase(int idTCMClient = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            ViewData["IdTCMClient"] = idTCMClient;
            ViewData["IdCaseManager"] = _context.CaseManagers.FirstOrDefault(n => n.LinkedUser == user_logged.UserName).Id;
            List<TCMNoteEntity> TcmNoteEntity = await _context.TCMNote
                                                              .Include(w => w.TCMClient)
                                                              .ThenInclude(d => d.Client)

                                                              .Include(w => w.TCMClient)
                                                              .ThenInclude(d => d.Casemanager)
                                                              .Where(w => (w.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                                && w.TCMClient.Id == idTCMClient))
                                                              .ToListAsync();
            return View(TcmNoteEntity);
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Create( int IdTCMClient = 0, int IdCaseManager = 0)
        {

            UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMNoteViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {
                   
                    model = new TCMNoteViewModel
                    {
                        CaseManagerDate = DateTime.Now,
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        DateOfService = DateTime.Now,
                        DocumentationTime = DateTime.Now,
                        Id = 0,
                        IdTCMClient = IdTCMClient,
                        NextStep = "",
                        Origin = 0,
                        Outcome = "",
                        ServiceCode = "T1017",
                        Status = NoteStatus.Edition,
                        TCMNoteActivity = new List<TCMNoteActivityEntity>(),
                        TotalMinutes = 0,
                        TotalUnits = 0,
                        IdCaseManager = IdCaseManager,
                        IdTCMNote = 0,
                       
                        TCMClient =  _context.TCMClient
                                             .Include(n => n.Client)
                                             .FirstOrDefault(n => n.Id == IdTCMClient),
                        CaseManager = _context.CaseManagers
                                              .FirstOrDefault(n => n.Id == IdCaseManager)


                    };
                       
                        return View(model);
                }
            }

            return RedirectToAction("Index", "TCMNotes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Create(TCMNoteViewModel TcmNoteViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMNoteEntity NoteEntity = await _converterHelper.ToTCMNoteEntity(TcmNoteViewModel, false, user_logged.UserName);

                if (NoteEntity.Id == 0)
                {
                   
                    _context.TCMNote.Add(NoteEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("TCMNotesForCase", new { idTCMClient = TcmNoteViewModel.IdTCMClient});
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    NoteEntity.TCMClient = null;
                    _context.TCMNote.Update(NoteEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        return RedirectToAction("TCMIntakeSectionDashboard", new { idTCMClient = TcmNoteViewModel.IdTCMClient });
                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            TcmNoteViewModel.TCMClient = _context.TCMClient.Find(TcmNoteViewModel.Id);

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", TcmNoteViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Edit(int id = 0)
        {
            TCMNoteViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    TCMNoteEntity TcmNote = _context.TCMNote
                                                    .Include(b => b.TCMClient)
                                                    .ThenInclude(b => b.Client)
                                                    .Include(b => b.TCMClient)
                                                    .ThenInclude(b => b.Casemanager)
                                                    .Include(b => b.TCMNoteActivity)
                                                    .Include(b => b.TCMNoteActivity)
                                                    .ThenInclude(b => b.TCMDomain)
                                                    .FirstOrDefault(m => m.Id == id);
                    if (TcmNote == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToTCMNoteViewModel(TcmNote);
                        model.TCMClient = TcmNote.TCMClient;
                        return View(model);
                    }

                }
            }

            model = new TCMNoteViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Edit(TCMNoteViewModel tcmNotesViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMNoteEntity tcmNotesEntity = await _converterHelper.ToTCMNoteEntity(tcmNotesViewModel, false, user_logged.UserName);
                _context.TCMNote.Update(tcmNotesEntity);
                try
                {
                    await _context.SaveChangesAsync();
                   
                    return RedirectToAction("TCMNotesForCase", new { idTCMClient = tcmNotesViewModel.IdTCMClient });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", tcmNotesViewModel) });
        }


        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateNoteActivity(int idNote = 0, int idDomain = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMNoteActivityViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {

                    model = new TCMNoteActivityViewModel
                    {
                        IdTCMNote = idNote,
                        TCMNote = _context.TCMNote
                                                .Include(n => n.TCMNoteActivity)
                                                .Include(n => n.TCMClient)
                                                .ThenInclude(n => n.Client)
                                                .FirstOrDefault(n => n.Id == idNote),
                        Id = 0,
                        DescriptionOfService = "",
                        EndTime = DateTime.Now,
                        Minutes = 0,
                        Setting = "99",
                        StartTime = DateTime.Now,
                        TCMDomain = _context.TCMDomains
                                            .FirstOrDefault(n => n.Id == idDomain),
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now
                    };
                    if (model.TCMNote.TCMNoteActivity == null)
                        model.TCMNote.TCMNoteActivity = new List<TCMNoteActivityEntity>();
                    return View(model);
                }
            }

            model = new TCMNoteActivityViewModel
            {
                IdTCMNote = idNote,
                TCMNote = _context.TCMNote
                                  .Include(n => n.TCMNoteActivity)
                                  .Include(n => n.TCMClient)
                                  .ThenInclude(n => n.Client)
                                  .FirstOrDefault(n => n.Id == idNote),
                Id = 0,
                DescriptionOfService = "",
                EndTime = DateTime.Now,
                Minutes = 0,
                Setting = "99",
                StartTime = DateTime.Now,
                TCMDomain = _context.TCMDomains
                                                .FirstOrDefault(n => n.Id == idDomain),
                CreatedBy = user_logged.UserName,
                CreatedOn = DateTime.Now
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateNoteActivity(TCMNoteActivityViewModel TcmNotesViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMNoteActivityEntity IndividualEntity = _context.TCMNoteActivity.Find(TcmNotesViewModel.Id);
                if (IndividualEntity == null)
                {
                    IndividualEntity = await _converterHelper.ToTCMNoteActivityEntity(TcmNotesViewModel, true, user_logged.UserName);
                    _context.TCMNoteActivity.Add(IndividualEntity);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<TCMNoteActivityEntity> NotesActivityList = await _context.TCMNoteActivity
                                                                                     .Include(g => g.TCMNote)
                                                                                     .ThenInclude(g => g.TCMClient)
                                                                                     .ThenInclude(g => g.Client)
                                                                                     .Include(g => g.TCMNote)
                                                                                     .ThenInclude(g => g.TCMClient)
                                                                                     .ThenInclude(g => g.Casemanager)
                                                                                     .Include(g => g.TCMDomain)
                                                                                     .Where(g => g.TCMNote.Id == TcmNotesViewModel.IdTCMNote)
                                                                                     .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMNoteActivity", NotesActivityList) });

                    }
                    catch (System.Exception ex)
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Already exists the Note.");

                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivity", TcmNotesViewModel) });
                }
            }
            TCMNoteActivityEntity model;
            model = new TCMNoteActivityEntity
            {
                TCMNote = _context.TCMNote.Include(n => n.TCMNoteActivity).FirstOrDefault(n => n.Id == TcmNotesViewModel.IdTCMNote),
                Id = 0,
                DescriptionOfService = TcmNotesViewModel.DescriptionOfService,
                EndTime = TcmNotesViewModel.EndTime,
                Minutes = TcmNotesViewModel.Minutes,
                Setting = TcmNotesViewModel.Setting,
                StartTime = TcmNotesViewModel.StartTime,
                TCMDomain = TcmNotesViewModel.TCMDomain
            };

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Create", model) });
        }


        [Authorize(Roles = "CaseManager")]
        public IActionResult EditNoteActivity(int id = 0)
        {
            TCMNoteActivityViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);

                if (user_logged.Clinic != null)
                {

                    TCMNoteActivityEntity NoteActivity = _context.TCMNoteActivity
                                                                 .Include(m => m.TCMDomain)
                                                                 .Include(m => m.TCMNote)
                                                                 .ThenInclude(m => m.TCMClient)
                                                                 .ThenInclude(m => m.Client)
                                                                 .FirstOrDefault(m => m.Id == id);
                    if (NoteActivity == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {

                        model = _converterHelper.ToTCMNoteActivityViewModel(NoteActivity);

                        return View(model);
                    }

                }
            }

            model = new TCMNoteActivityViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> EditNoteActivity(TCMNoteActivityViewModel IndividualAgencyViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMNoteActivityEntity IndividualAgencyEntity = await _converterHelper.ToTCMNoteActivityEntity(IndividualAgencyViewModel, false, user_logged.UserName);
                _context.TCMNoteActivity.Update(IndividualAgencyEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<TCMNoteActivityEntity> NotesActivityList = await _context.TCMNoteActivity
                                                                                    .Include(g => g.TCMNote)
                                                                                    .ThenInclude(g => g.TCMClient)
                                                                                    .ThenInclude(g => g.Client)
                                                                                    .Include(g => g.TCMNote)
                                                                                    .ThenInclude(g => g.TCMClient)
                                                                                    .ThenInclude(g => g.Casemanager)
                                                                                    .Include(g => g.TCMDomain)
                                                                                    .Where(g => g.TCMNote.Id == IndividualAgencyViewModel.IdTCMNote)
                                                                                    .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMNoteActivity", NotesActivityList) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditNoteActivity", IndividualAgencyViewModel) });
        }


    }
}
