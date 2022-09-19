﻿using KyoS.Common.Enums;
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
                                      .Include(w => w.Days)
                                      .Include(w => w.Days)

                                      .Where(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                && w.Days.Where(d => (d.Service == ServiceType.PSR)).Count() > 0))
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
                                                              .ThenInclude(d => d.TCMDomain)
                                                              .Include(w => w.TCMClient)
                                                              .ThenInclude(d => d.Casemanager)
                                                              .Where(w => (w.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                                && w.TCMClient.Id == idTCMClient))
                                                              .OrderBy(m => m.DateOfService)
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
                        CreatedBy = user_logged.UserName,
                        CreatedOn = DateTime.Now,
                        DateOfService = dateTime,
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
                        TCMNoteActivityTemp = _context.TCMNoteActivityTemp
                                                      .Where(na => na.UserName == user_logged.UserName)
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
                    List<TCMNoteActivityTempEntity> noteActivities = await _context.TCMNoteActivityTemp
                                                                                   .Where(na => na.UserName == user_logged.UserName)
                                                                                   .ToListAsync();
                    TCMNoteActivityEntity noteActivity;
                    foreach (var item in noteActivities)
                    {
                        noteActivity = new TCMNoteActivityEntity()
                        {
                            TCMDomain = await _context.TCMDomains.FirstOrDefaultAsync(n => n.Code == item.TCMDomainCode),
                            CreatedBy = user_logged.UserName,
                            CreatedOn = DateTime.Now,
                            LastModifiedBy = string.Empty,
                            LastModifiedOn = Convert.ToDateTime(null),
                            DescriptionOfService = item.DescriptionOfService,
                            EndTime = item.EndTime,
                            Minutes = item.Minutes,
                            Setting = ServiceTCMNotesUtils.GetCodeByIndex(item.IdSetting),
                            StartTime = item.StartTime,
                            TCMNote = NoteEntity,
                            ServiceName = item.ServiceName,
                            TCMServiceActivity = await _context.TCMServiceActivity.FirstOrDefaultAsync(n => n.Id == item.IdTCMServiceActivity)
                        };
                        _context.TCMNoteActivity.Add(noteActivity);                   
                    }

                    //delete all temporaly items
                    _context.RemoveRange(noteActivities);

                    try
                    {
                        await _context.SaveChangesAsync();

                        return RedirectToAction("Index", "TCMBilling");    
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
                        //redirect to note print report
                        if (TcmNote.Status == NoteStatus.Approved)
                        {
                            return RedirectToAction("PrintNote", new { id = TcmNote.Id });
                        }

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
                
                //tcmNotesEntity.TotalMinutes = GetTotalMinutes(tcmNotesEntity);
                //tcmNotesEntity.TotalUnits = GetTotalUnit(tcmNotesEntity.TotalMinutes);

               /* TCMNoteEntity tcmNote =  _context.TCMNote.FirstOrDefault(n => n.Id == tcmNotesViewModel.Id);

                if (tcmNote == null)
                {
                    ModelState.AddModelError(string.Empty, "Already not exists the TCM note");
                }*/

                TCMNoteEntity tcmNotesEntity = await _converterHelper.ToTCMNoteEntity(tcmNotesViewModel, false, user_logged.UserName);

                List<TCMMessageEntity> messages = tcmNotesEntity.TCMMessages.Where(m => (m.Status == MessageStatus.NotRead && m.Notification == false)).ToList();
                //todos los mensajes no leidos que tiene el Workday_Client de la nota los pongo como leidos
                foreach (TCMMessageEntity value in messages)
                {
                    value.Status = MessageStatus.Read;
                    value.DateRead = DateTime.Now;
                    _context.Update(value);

                    //I generate a notification to supervisor
                    TCMMessageEntity notification = new TCMMessageEntity
                    {
                        TCMNote = tcmNotesEntity,
                        TCMFarsForm = null,
                        TCMServicePlan = null,
                        TCMServicePlanReview = null,
                        TCMAddendum = null,
                        TCMDischarge = null,
                        TCMAssessment = null,
                        Title = "Update on reviewed TCM note",
                        Text = $"The TCM note of {tcmNotesEntity.TCMClient.Client.Name} on {tcmNotesEntity.DateOfService.ToShortDateString()} was rectified",
                        From = value.To,
                        To = value.From,
                        DateCreated = DateTime.Now,
                        Status = MessageStatus.NotRead,
                        Notification = true
                    };
                    _context.Add(notification);
                }
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
                    if (origin == 3)
                    {
                        return RedirectToAction("NotesWithReview");
                    }
                    if (origin == 4)
                    {
                        return RedirectToAction("MessagesOfNotes", "TCMMessages");
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
        public IActionResult CreateNoteActivity(int idNote = 0, int idTCMClient = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMNoteActivityViewModel model;
            IEnumerable<SelectListItem> list_Services = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == idTCMClient).Id);

            TCMNoteEntity note = _context.TCMNote

                                         .Include(n => n.TCMNoteActivity)

                                         .Include(n => n.TCMClient)
                                         .ThenInclude(n => n.Client)

                                         .FirstOrDefault(n => n.Id == idNote);

            DateTime StartTime = note.DateOfService;

            if (note.TCMNoteActivity.Count() > 0)
            {
                StartTime = note.TCMNoteActivity.Last().EndTime.AddMinutes(5);
            }

            if (user_logged.Clinic != null)
            {
                model = new TCMNoteActivityViewModel
                {
                    IdTCMNote = idNote,
                    TCMNote = note,
                    Id = 0,
                    IdTCMDomain = 0,
                    DomainList = list_Services,
                    DescriptionOfService = "",
                    Minutes = 15,
                    IdSetting = 1,
                    SettingList = _combosHelper.GetComboTCMNoteSetting(),
                    Setting = "99",
                    TCMDomain = new TCMDomainEntity(),
                    CreatedBy = user_logged.UserName,
                    CreatedOn = DateTime.Now,
                    IdTCMClient = idTCMClient,
                    DescriptionTemp = "",
                    StartTime = StartTime,
                    EndTime = StartTime.AddMinutes(15),
                    DateOfServiceNote = note.DateOfService,
                    ServiceName = "",
                    IdTCMActivity = 0
                };
                if (model.TCMNote.TCMNoteActivity == null)
                    model.TCMNote.TCMNoteActivity = new List<TCMNoteActivityEntity>();

                return View(model);
            }

            return View(null);
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
                    TcmNotesViewModel.TCMDomain = await _context.TCMDomains.FirstOrDefaultAsync(n => n.Code == _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain).Code);

                    TCMNoteEntity note = await _context.TCMNote
                                                       .FirstOrDefaultAsync(n => n.Id == TcmNotesViewModel.IdTCMNote);
                    TcmNotesViewModel.StartTime = new DateTime(note.DateOfService.Year, note.DateOfService.Month, note.DateOfService.Day, TcmNotesViewModel.StartTime.Hour, TcmNotesViewModel.StartTime.Minute, 0);
                    TcmNotesViewModel.EndTime = new DateTime(note.DateOfService.Year, note.DateOfService.Month, note.DateOfService.Day, TcmNotesViewModel.EndTime.Hour, TcmNotesViewModel.EndTime.Minute, 0);

                    //casemanager overlapping validation
                    List<TCMNoteActivityEntity> noteActivities = await _context.TCMNoteActivity
                                                                               .Where(na => (na.CreatedBy == user_logged.UserName
                                                                                 && na.TCMNote.DateOfService.Date == TcmNotesViewModel.DateOfServiceNote.Date
                                                                                 && ((na.StartTime <= TcmNotesViewModel.StartTime && na.EndTime > TcmNotesViewModel.StartTime)
                                                                                    || (na.StartTime < TcmNotesViewModel.EndTime && na.EndTime >= TcmNotesViewModel.EndTime))))
                                                                               .ToListAsync();

                    if (noteActivities.Count() > 0 || CheckOverlappingMH(TcmNotesViewModel.DateOfServiceNote, TcmNotesViewModel.DateOfServiceNote, _context.TCMClient.Include(n => n.Client).FirstOrDefault(n => n.Id == TcmNotesViewModel.IdTCMClient).Client.Id) == true)
                    {
                        TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                    .Include(n => n.TCMClient)
                                                    .ThenInclude(n => n.Client)
                                                    .FirstOrDefaultAsync(m => m.Id == TcmNotesViewModel.IdTCMNote);

                        TcmNotesViewModel.TCMNote = tcmNote1;
                        TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                        TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id);
                        if (TcmNotesViewModel.IdTCMDomain != 0)
                        {
                            TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                            TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                        }

                        ModelState.AddModelError(string.Empty, $"Error.There are activities created in that time interval");
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditNoteActivity", TcmNotesViewModel) });

                    }

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
            TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id);
            if (TcmNotesViewModel.IdTCMDomain != 0)
            {
                TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivity", TcmNotesViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateNoteActivityTemp(DateTime initDate, int idTCMClient = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMNoteActivityViewModel model;
            IEnumerable<SelectListItem> list_Services = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == idTCMClient).Id);

            if (user_logged.Clinic != null)
            {
                model = new TCMNoteActivityViewModel
                {
                    Id = 0,
                    IdTCMDomain = 0,
                    DomainList = list_Services,
                    DescriptionOfService = "",
                    Minutes = 15,
                    IdSetting = 1,
                    SettingList = _combosHelper.GetComboTCMNoteSetting(),
                    TCMDomain = new TCMDomainEntity(),
                    CreatedBy = user_logged.UserName,
                    CreatedOn = DateTime.Now,
                    IdTCMClient = idTCMClient,
                    DescriptionTemp = "",
                    StartTime = initDate,
                    EndTime = initDate.AddMinutes(15),
                    DateOfServiceNote = initDate
                };

                return View(model);
            }

            return View(null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateNoteActivityTemp(TCMNoteActivityViewModel TcmNotesViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            
            if (ModelState.IsValid)
            {
                TcmNotesViewModel.TCMDomain = await _context.TCMDomains.FirstOrDefaultAsync(n => n.Code == _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain).Code);

                TcmNotesViewModel.StartTime = new DateTime(TcmNotesViewModel.DateOfServiceNote.Year, TcmNotesViewModel.DateOfServiceNote.Month, TcmNotesViewModel.DateOfServiceNote.Day, TcmNotesViewModel.StartTime.Hour, TcmNotesViewModel.StartTime.Minute, 0);
                TcmNotesViewModel.EndTime = new DateTime(TcmNotesViewModel.DateOfServiceNote.Year, TcmNotesViewModel.DateOfServiceNote.Month, TcmNotesViewModel.DateOfServiceNote.Day, TcmNotesViewModel.EndTime.Hour, TcmNotesViewModel.EndTime.Minute, 0);

                TCMNoteActivityTempEntity entity = _converterHelper.ToTCMNoteActivityTempEntity(TcmNotesViewModel, true, user_logged.UserName);

                //casemanager overlapping validation
                List<TCMNoteActivityEntity> noteActivities = await _context.TCMNoteActivity
                                                                           .Where(na => (na.CreatedBy == user_logged.UserName
                                                                             && na.TCMNote.DateOfService.Date == TcmNotesViewModel.DateOfServiceNote.Date
                                                                             && ((na.StartTime <= TcmNotesViewModel.StartTime && na.EndTime > TcmNotesViewModel.StartTime)
                                                                                || (na.StartTime < TcmNotesViewModel.EndTime && na.EndTime >= TcmNotesViewModel.EndTime))))
                                                                           .ToListAsync();
                List<TCMNoteActivityTempEntity> noteActivitiesTemp = await _context.TCMNoteActivityTemp
                                                                                   .Where(na => (na.UserName == user_logged.UserName
                                                                                        && na.DateOfServiceOfNote.Date == TcmNotesViewModel.DateOfServiceNote.Date
                                                                                        && ((na.StartTime <= TcmNotesViewModel.StartTime && na.EndTime > TcmNotesViewModel.StartTime)
                                                                                            || (na.StartTime < TcmNotesViewModel.EndTime && na.EndTime >= TcmNotesViewModel.EndTime))))
                                                                                   .ToListAsync();

                if (noteActivities.Count() > 0 || noteActivitiesTemp.Count() > 0 || CheckOverlappingMH(TcmNotesViewModel.DateOfServiceNote, TcmNotesViewModel.DateOfServiceNote, _context.TCMClient.Include(n => n.Client).FirstOrDefault(n => n.Id == TcmNotesViewModel.IdTCMClient).Client.Id) == true)
                {
                    TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                    TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id);
                    
                    if (TcmNotesViewModel.IdTCMDomain != 0)
                    {
                        TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                        TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                    }

                    ModelState.AddModelError(string.Empty, $"Error.There are activities created in that time interval");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivityTemp", TcmNotesViewModel) });
                }
               
                _context.TCMNoteActivityTemp.Add(entity);
                try
                {
                    await _context.SaveChangesAsync();

                    IEnumerable<TCMNoteActivityTempEntity> NotesActivityList = await _context.TCMNoteActivityTemp

                                                                                             .Where(g => g.UserName == user_logged.UserName)
                                                                                             .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMNoteActivityTemp", NotesActivityList) });

                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
            TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id);
            if (TcmNotesViewModel.IdTCMDomain != 0)
            {
                TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivityTemp", TcmNotesViewModel) });
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
                                                                 .Include(m => m.TCMServiceActivity)
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
        public async Task<IActionResult> EditNoteActivity(TCMNoteActivityViewModel NoteActivityViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                NoteActivityViewModel.TCMDomain = await _context.TCMDomains.FirstOrDefaultAsync(n => n.Code == _context.TCMDomains.FirstOrDefault(d => d.Id == NoteActivityViewModel.IdTCMDomain).Code);

                TCMNoteEntity note = await _context.TCMNote
                                                   .FirstOrDefaultAsync(n => n.Id == NoteActivityViewModel.IdTCMNote);
                NoteActivityViewModel.StartTime = new DateTime(note.DateOfService.Year, note.DateOfService.Month, note.DateOfService.Day, NoteActivityViewModel.StartTime.Hour, NoteActivityViewModel.StartTime.Minute, 0);
                NoteActivityViewModel.EndTime = new DateTime(note.DateOfService.Year, note.DateOfService.Month, note.DateOfService.Day, NoteActivityViewModel.EndTime.Hour, NoteActivityViewModel.EndTime.Minute, 0);

                //casemanager overlapping validation
                List<TCMNoteActivityEntity> noteActivities = await _context.TCMNoteActivity
                                                                           .Where(na => (na.CreatedBy == user_logged.UserName
                                                                             && na.Id != NoteActivityViewModel.Id
                                                                             && na.TCMNote.DateOfService.Date == NoteActivityViewModel.DateOfServiceNote.Date
                                                                             && ((na.StartTime <= NoteActivityViewModel.StartTime && na.EndTime > NoteActivityViewModel.StartTime)
                                                                                || (na.StartTime < NoteActivityViewModel.EndTime && na.EndTime >= NoteActivityViewModel.EndTime))))
                                                                           .ToListAsync();
                if (noteActivities.Count() > 0 || CheckOverlappingMH(NoteActivityViewModel.DateOfServiceNote, NoteActivityViewModel.DateOfServiceNote, _context.TCMClient.Include(n => n.Client).FirstOrDefault(n => n.Id == NoteActivityViewModel.IdTCMClient).Client.Id) == true)
                {
                    TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                .Include(n => n.TCMClient)
                                                .ThenInclude(n => n.Client)
                                                .FirstOrDefaultAsync(m => m.Id == NoteActivityViewModel.IdTCMNote);

                    NoteActivityViewModel.TCMNote = tcmNote1;
                    NoteActivityViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                    NoteActivityViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == NoteActivityViewModel.IdTCMClient).Id);
                    if (NoteActivityViewModel.IdTCMDomain != 0)
                    {
                        TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == NoteActivityViewModel.IdTCMDomain);
                        NoteActivityViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                    }

                    ModelState.AddModelError(string.Empty, $"Error.There are activities created in that time interval");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditNoteActivity", NoteActivityViewModel) });
                   
                }

                TCMNoteActivityEntity IndividualAgencyEntity = await _converterHelper.ToTCMNoteActivityEntity(NoteActivityViewModel, false, user_logged.UserName);
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

                                                                                  .Where(g => g.TCMNote.Id == NoteActivityViewModel.IdTCMNote)
                                                                                  .ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMNoteActivity", NotesActivityList) });
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }
            
            TCMNoteEntity tcmNote = await _context.TCMNote
                                                .Include(n => n.TCMClient)
                                                .ThenInclude(n => n.Client)
                                                .FirstOrDefaultAsync(m => m.Id == NoteActivityViewModel.IdTCMNote);

            NoteActivityViewModel.TCMNote = tcmNote;
            NoteActivityViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
            NoteActivityViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == NoteActivityViewModel.IdTCMClient).Id);
            if (NoteActivityViewModel.IdTCMDomain != 0)
            {
                TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == NoteActivityViewModel.IdTCMDomain);
                NoteActivityViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditNoteActivity", NoteActivityViewModel) });                                 
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> FinishEditingNote(int id, int origin = 0)
        {
            TCMNoteEntity tcmNote = _context.TCMNote
                                            .Include(u => u.TCMClient)
                                            .ThenInclude(u => u.Casemanager)
                                            .Include(u => u.TCMNoteActivity)
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
                                            .Include(u => u.TCMNoteActivity)
                                            .ThenInclude(u => u.TCMDomain)
                                            .Include(u => u.TCMClient)
                                            .ThenInclude(u => u.Casemanager)
                                            .FirstOrDefault(u => u.Id == id);

            if (tcmNote != null)
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    if (user_logged.Clinic != null)
                    {
                        TCMDomainEntity domain = new TCMDomainEntity();
                        foreach (var item in tcmNote.TCMNoteActivity)
                        {
                            domain = await _context.TCMDomains.FindAsync(item.TCMDomain.Id);
                            
                            if (domain != null)
                            {
                                domain.Used = true;
                                _context.Update(domain); 
                                domain = new TCMDomainEntity();
                            }
                        
                        }
                        tcmNote.Status = NoteStatus.Approved;
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
                                return RedirectToAction("NotesStatus", new { status = NoteStatus.Pending });
                            }
                            if (origin == 2)
                            {
                                return RedirectToAction("NotesWithReview");
                            }
                            if (origin == 3)  ///viene de la pagina Notifications
                                return RedirectToAction("Notifications", "TCMMessages");
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

            return RedirectToAction("Edit", new { id = tcmNotesActivity.TCMNote.Id, origin = 2});
        }

        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> DeleteTCMNoteActivityTemp(int id = 0)
        {
            TCMNoteActivityTempEntity tcmNotesActivity = _context.TCMNoteActivityTemp
                                                                 .FirstOrDefault(m => m.Id == id);
            if (tcmNotesActivity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMNoteActivityTemp.Remove(tcmNotesActivity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

            }

            return RedirectToAction("Create", new { dateTime = tcmNotesActivity.DateOfServiceOfNote, tcmNotesActivity.IdTCMClient });
        }

        public JsonResult CalcularMinutes(DateTime start, DateTime end)
        {
            int hora = (end - start).Hours;
            int minutes = (end - start).Minutes + (hora * 60);
            return Json(minutes);
        }

        public JsonResult GetListActivity(int idDomain)
        {
            TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == idDomain);
            List<TCMServiceActivityEntity> activity = new List<TCMServiceActivityEntity>();

            if (domain != null)
            {
                activity = _context.TCMServiceActivity.Where(n => n.TcmService.Code == domain.Code).ToList();
            }

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

        public JsonResult GetSuggestion(int idActivity)
        {
            TCMServiceActivityEntity activity = _context.TCMServiceActivity.FirstOrDefault(o => o.Id == idActivity);
            string text = "Select Service and activity";
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
                                     .Include(w => w.TCMMessages)
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
                                     .Include(w => w.TCMMessages)
                                     .Where(w => (w.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                       && w.Status == status))
                                     .ToListAsync();
            }

            return View(note);
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult EditReadOnly(int id = 0, int origi = 0)
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
                        ViewData["origi"] = origi;
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
        public async Task<IActionResult> EditReadOnly(TCMNoteViewModel tcmNotesViewModel, int origi = 1)
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
                    if (origi == 1)
                    {
                        return RedirectToAction("NotesStatus", new { status = NoteStatus.Pending });
                    }
                    if (origi == 2)
                    {
                        return RedirectToAction("NotesWithReview");
                    }
                    if (origi == 3)
                    {
                        return RedirectToAction("Notifications", "TCMMessages");
                    }

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
                                         .Include(n => n.TCMClient)
                                         .ThenInclude(n => n.Casemanager)
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

            if (note.TCMClient.Casemanager.Clinic.Name == "FLORIDA SOCIAL HEALTH SOLUTIONS")
            {
                Stream stream = _reportHelper.FloridaSocialHSTCMNoteReportSchema1(note);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            if (note.TCMClient.Casemanager.Clinic.Name == "DREAMS MENTAL HEALTH INC")
            {
                Stream stream = _reportHelper.DreamsMentalHealthTCMNoteReportSchema1(note);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            return null;
        }
        
        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public bool CheckOverlappingMH(DateTime start, DateTime end, int idClient)
        {
            List<WorkdayEntity> workday = _context.Workdays
                                                  .Where(n => n.Date == start.Date)
                                                  .ToList();

            List<Workday_Client> workday_Client = _context.Workdays_Clients
                                                          .Where(n => (n.Workday.Date == start.Date
                                                            && n.Client.Id == idClient))
                                                          .ToList();

            if (workday_Client.Count > 0)
            {
                return true;
            }

            return false;
        }

        [Authorize(Roles = "TCMSupervisor, CaseManager")]
        public IActionResult AddMessageEntity(int id = 0, int origi = 0)
        {
            if (id == 0)
            {
                return View(new TCMMessageViewModel());
            }
            else
            {
                TCMMessageViewModel model = new TCMMessageViewModel()
                {
                    IdTCMNote = id,
                    Origin = origi
                };

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor")]
        public async Task<IActionResult> AddMessageEntity(TCMMessageViewModel messageViewModel)
        {
            if (ModelState.IsValid)
            {
                TCMMessageEntity model = await _converterHelper.ToTCMMessageEntity(messageViewModel, true);
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                model.From = user_logged.UserName;
                model.To = model.TCMNote.TCMClient.Casemanager.LinkedUser;
                _context.Add(model);
                await _context.SaveChangesAsync();
            }
            if (messageViewModel.Origin == 1)
                return RedirectToAction("NotesWithReview");
            

            return RedirectToAction("NotesStatus", new { status = NoteStatus.Pending});
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public async Task<IActionResult> NotesWithReview()
        {
            if (User.IsInRole("CaseManager"))
            {
                List<TCMNoteEntity> salida = await _context.TCMNote
                                                           .Include(wc => wc.TCMClient)
                                                           .ThenInclude(wc => wc.Casemanager)
                                                           .Include(wc => wc.TCMClient)
                                                           .ThenInclude(wc => wc.Client)
                                                           .Include(wc => wc.TCMMessages.Where(m => m.Notification == false))
                                                           .Where(wc => (wc.TCMClient.Casemanager.LinkedUser == User.Identity.Name
                                                                && wc.Status == NoteStatus.Pending
                                                                && wc.TCMMessages.Count() > 0))
                                                           .ToListAsync();


                return View(salida);
            }

            if (User.IsInRole("TCMSupervisor") || User.IsInRole("Manager"))
            {
                UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                             .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    List<TCMNoteEntity> salida = await _context.TCMNote
                                                               .Include(wc => wc.TCMClient)
                                                               .ThenInclude(wc => wc.Client)
                                                               .Include(wc => wc.TCMClient.Casemanager)
                                                               .Include(wc => wc.TCMMessages.Where(m => m.Notification == false))
                                                               .Where(wc => (wc.TCMClient.Casemanager.Clinic.Id == user_logged.Clinic.Id
                                                                    && wc.Status == NoteStatus.Pending 
                                                                    && wc.TCMMessages.Count() > 0))
                                                               .ToListAsync();
                    return View(salida);
                }
            }

            return View();
        }


    }
}
