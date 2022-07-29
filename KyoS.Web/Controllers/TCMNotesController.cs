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
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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
                                                              .Include(w => w.TCMNoteActivity)
                                                              .Include(w => w.TCMClient)
                                                              .ThenInclude(d => d.Casemanager)
                                                              .Where(w => (w.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                                && w.TCMClient.Id == idTCMClient))
                                                              .ToListAsync();
            return View(TcmNoteEntity);
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Create(DateTime dateTime, int IdTCMClient, int origin = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            CaseMannagerEntity caseManager = _context.CaseManagers
                                                     .FirstOrDefault(cm => cm.LinkedUser == user_logged.UserName);

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
                        DateOfService = dateTime,
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
                        IdCaseManager = caseManager.Id,
                        IdTCMNote = 0,

                        TCMClient = _context.TCMClient
                                             .Include(n => n.Client)
                                             .ThenInclude(n => n.Clinic)
                                             .FirstOrDefault(n => n.Id == IdTCMClient),
                        CaseManager = caseManager
                    };
                    ViewData["origin"] = origin;
                    return View(model);
                }
            }

            return RedirectToAction("Index", "TCMNotes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> Create(TCMNoteViewModel TcmNoteViewModel, int origin = 0)
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
                        if (origin == 0)
                        {
                            return RedirectToAction("TCMNotesForCase", new { idTCMClient = TcmNoteViewModel.IdTCMClient });
                        }
                        else
                        {
                            return RedirectToAction("Index", "TCMBilling");
                        }

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
                        if (origin == 0)
                        {
                            return RedirectToAction("TCMIntakeSectionDashboard", new { idTCMClient = TcmNoteViewModel.IdTCMClient });
                        }
                        else
                        {
                            return RedirectToAction("Index", "TCMBilling");
                        }
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
        public IActionResult Edit(int id = 0, int origin = 0)
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
                        ViewData["origin"] = origin;
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
        public async Task<IActionResult> Edit(TCMNoteViewModel tcmNotesViewModel, int origin = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                TCMNoteEntity tcmNotesEntity = await _converterHelper.ToTCMNoteEntity(tcmNotesViewModel, false, user_logged.UserName);
                //tcmNotesEntity.TotalMinutes = GetTotalMinutes(tcmNotesEntity);
                //tcmNotesEntity.TotalUnits = GetTotalUnit(tcmNotesEntity.TotalMinutes);
                _context.TCMNote.Update(tcmNotesEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    if (origin == 0)
                    {
                        return RedirectToAction("TCMNotesForCase", new { idTCMClient = tcmNotesViewModel.IdTCMClient });
                    }
                    if (origin == 1)
                    {
                        return RedirectToAction("NotesStatus", new { status = NoteStatus.Edition });
                    }
                    if (origin == 2)
                    {
                        return RedirectToAction("Index", "TCMBilling");
                    }
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", tcmNotesViewModel) });
        }

        public int GetTotalMinutes(TCMNoteEntity Note)
        {
            int minutes = 0;

            if (Note != null)
            {
                foreach (var item in Note.TCMNoteActivity)
                {
                    minutes = minutes + item.Minutes;
                }

            }
            else
            {
                minutes = 0;
            }
            return minutes;
        }

        public int GetTotalUnit(int minutes = 0)
        {
            int factor = 15;
            int unit = minutes / factor;
            double residuo = minutes % factor;
            if (residuo >= 8)
                unit++;
            return unit;
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateNoteActivity(DateTime startTime, int idNote = 0, int idTCMClient = 0)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMNoteActivityViewModel model;
            IEnumerable<SelectListItem> list_Services = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == idTCMClient).Id);
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
                        IdTCMDomain = 0,
                        DomainList = list_Services,
                        DescriptionOfService = "",
                        Minutes = "",
                        IdSetting = 1,
                        SettingList = _combosHelper.GetComboTCMNoteSetting(),
                        Setting = "99",
                        TCMDomain = new TCMDomainEntity(),
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        IdTCMClient = idTCMClient,
                        DescriptionTemp = "",
                        StartTime = startTime.Date
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
                Minutes = "",
                Setting = "99",
                IdSetting = 1,
                SettingList = _combosHelper.GetComboTCMNoteSetting(),
                StartTime = DateTime.Now,

                CreatedBy = user_logged.UserName,
                CreatedOn = DateTime.Now,
                IdTCMClient = idTCMClient
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
                    TcmNotesViewModel.TCMDomain = await _context.TCMDomains.FirstOrDefaultAsync(n => n.Code == TcmNotesViewModel.TCMDomain.Code);
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
            TCMNoteEntity tcmNote = await _context.TCMNote
                                                     .Include(n => n.TCMClient)
                                                     .ThenInclude(n => n.Client)
                                                     .FirstOrDefaultAsync(m => m.Id == TcmNotesViewModel.IdTCMNote);

            TcmNotesViewModel.TCMNote = tcmNote;
            TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
            TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(TcmNotesViewModel.TCMDomain.Code);
            TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id);
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivity", TcmNotesViewModel) });

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
                IndividualAgencyViewModel.TCMDomain = await _context.TCMDomains.FirstOrDefaultAsync(n => n.Code == IndividualAgencyViewModel.TCMDomain.Code);
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
            else
            {
                TCMNoteEntity tcmNote = await _context.TCMNote
                                                      .Include(n => n.TCMClient)
                                                      .ThenInclude(n => n.Client)
                                                      .Include(n => n.TCMClient)
                                                      .ThenInclude(n => n.TcmServicePlan)
                                                      .ThenInclude(n => n.TCMDomain)
                                                      .FirstOrDefaultAsync(m => m.Id == IndividualAgencyViewModel.IdTCMNote);

                IndividualAgencyViewModel.TCMNote = tcmNote;
                IndividualAgencyViewModel.IdSetting = 0;
                IndividualAgencyViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                IndividualAgencyViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(IndividualAgencyViewModel.TCMDomain.Code);
                IndividualAgencyViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == IndividualAgencyViewModel.IdTCMClient).Id);
                return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditNoteActivity", IndividualAgencyViewModel) });
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditNoteActivity", IndividualAgencyViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> FinishEditingNote(int id, int origin = 0)
        {
            TCMNoteEntity tcmNote = _context.TCMNote
                                            .Include(u => u.TCMClient)
                                            .Include(u => u.TCMNoteActivity)
                                            .Include(u => u.CaseManager)
                                            .FirstOrDefault(u => u.Id == id);

            if (tcmNote != null)
            {
                if (User.IsInRole("CaseManager"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        tcmNote.Status = NoteStatus.Pending;
                        _context.Update(tcmNote);
                        try
                        {
                            await _context.SaveChangesAsync();

                            if (origin == 0)
                            {
                                return RedirectToAction("TCMNotesForCase", new { idTCMClient = tcmNote.TCMClient.Id });
                            }
                            if (origin == 1)
                            {
                                return RedirectToAction("NotesStatus", new { status = NoteStatus.Edition });
                            }

                        }
                        catch (System.Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("TCMNotesForCase");
        }

        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> ApprovedNote(int id, int origin = 0)
        {
            TCMNoteEntity tcmNote = _context.TCMNote
                                            .Include(u => u.TCMClient)
                                            .Include(u => u.TCMNoteActivity)
                                            .Include(u => u.CaseManager)
                                            .FirstOrDefault(u => u.Id == id);

            if (tcmNote != null)
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        tcmNote.Status = NoteStatus.Approved;
                        _context.Update(tcmNote);
                        try
                        {
                            await _context.SaveChangesAsync();
                            if (origin == 0)
                            {
                                return RedirectToAction("TCMNotesForCase", new { idTCMClient = tcmNote.TCMClient.Id });
                            }
                            else
                            {
                                return RedirectToAction("NotesStatus", new { status = NoteStatus.Pending });
                            }

                        }
                        catch (System.Exception ex)
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                    return RedirectToAction("NotAuthorized", "Account");
                }
            }

            return RedirectToAction("TCMNotesForCase");
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteTCMNoteActivity(int id = 0)
        {
            TCMNoteActivityEntity tcmNotesActivity = _context.TCMNoteActivity
                                                             .Include(m => m.TCMNote)
                                                             .FirstOrDefault(m => m.Id == id);
            if (tcmNotesActivity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMNoteActivity.Remove(tcmNotesActivity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

            }

            return RedirectToAction("Edit", new { id = tcmNotesActivity.TCMNote.Id });

        }

        public JsonResult CalcularMinutes(DateTime start, DateTime end)
        {
            int hora = (end - start).Hours;
            int minutes = (end - start).Minutes + (hora * 60);
            return Json(minutes);
        }

        public JsonResult GetListActivity(string codeDomain = "")
        {
            List<TCMServiceActivityEntity> activity = _context.TCMServiceActivity.Where(n => n.TcmService.Code == codeDomain).ToList();

            if (activity.Count == 0)
            {
                activity.Insert(0, new TCMServiceActivityEntity
                {
                    Name = "[First select domain...]",
                    Id = 0
                });
            }
            return Json(new SelectList(activity, "Id", "Name"));
        }

        public JsonResult GetIntervention(int idActivity)
        {
            TCMServiceActivityEntity activity = _context.TCMServiceActivity.FirstOrDefault(o => o.Id == idActivity);
            string text = "Select Service and Acivity";
            if (activity != null)
            {
                text = activity.Description;
            }
            return Json(text);
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public async Task<IActionResult> NotesStatus(NoteStatus status = NoteStatus.Approved)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<TCMNoteEntity> note = new List<TCMNoteEntity>();

            if (User.IsInRole("Casemanager"))
            {
                note = await _context.TCMNote
                                     .Include(w => w.TCMClient)
                                     .ThenInclude(d => d.Client)
                                     .ThenInclude(d => d.Clinic)
                                     .Include(w => w.TCMClient)
                                     .ThenInclude(d => d.Casemanager)
                                     .Include(w => w.TCMNoteActivity)
                                     .Where(w => (w.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                        && w.Status == status
                                        && w.TCMClient.Casemanager.LinkedUser == user_logged.UserName))
                                     .ToListAsync();
            }
            else
            {
                note = await _context.TCMNote
                                    .Include(w => w.TCMClient)
                                    .ThenInclude(d => d.Client)
                                    .ThenInclude(d => d.Clinic)
                                    .Include(w => w.TCMClient)
                                    .ThenInclude(d => d.Casemanager)
                                    .Include(w => w.TCMNoteActivity)
                                    .Where(w => (w.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                       && w.Status == status))
                                    .ToListAsync();
            }

            return View(note);
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult EditReadOnly(int id = 0)
        {
            TCMNoteViewModel model;

            if (User.IsInRole("TCMSupervisor"))
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
        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> EditReadOnly(TCMNoteViewModel tcmNotesViewModel)
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

                    return RedirectToAction("NotesStatus", new { status = NoteStatus.Pending });

                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditReadOnly", tcmNotesViewModel) });
        }

        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public IActionResult PrintNote(int id)
        {
            TCMNoteEntity note = _context.TCMNote
                                         
                                         .Include(n => n.CaseManager)
                                         .ThenInclude(cm => cm.Clinic)

                                         .Include(n => n.TCMClient)
                                         .ThenInclude(c => c.Client)

                                         .Include(n => n.TCMNoteActivity)
                                         .ThenInclude(na => na.TCMDomain)

                                         .FirstOrDefault(n => (n.Id == id && n.Status == NoteStatus.Approved));
            if (note == null)
            {
                return RedirectToAction("Home/Error404");
            }

            if (note.CaseManager.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
            {                
                Stream stream = _reportHelper.FloridaSocialHSTCMNoteReportSchema1(note);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);               
            }

            if (note.CaseManager.Clinic.Name == "DREAMS MENTAL HEALTH INC")
            {
                Stream stream = _reportHelper.DreamsMentalHealthTCMNoteReportSchema1(note);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            return null;
        }
    }
}
