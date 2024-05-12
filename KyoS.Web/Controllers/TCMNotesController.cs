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
                                      .Include(w => w.Days)
                                      .Include(w => w.Days)

                                      .Where(w => (w.Clinic.Id == user_logged.Clinic.Id
                                                && w.Days.Where(d => (d.Service == ServiceType.PSR)).Count() > 0))
                                      .ToListAsync());
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public async Task<IActionResult> TCMNotesForCase(int idTCMClient = 0, int origin = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (User.IsInRole("CaseManager"))
            {
                ViewData["IdTCMClient"] = idTCMClient;
                ViewData["IdCaseManager"] = _context.CaseManagers.FirstOrDefault(n => n.LinkedUser == user_logged.UserName).Id;
                ViewData["origin"] = origin;
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
            else
            {
                ViewData["IdTCMClient"] = idTCMClient;
                ViewData["IdCaseManager"] = 0;
                ViewData["origin"] = origin;
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

        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Create(DateTime dateTime, int IdTCMClient, int origin = 0, bool billable = true)
        {

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (_context.TCMDateBlocked.Any(n => n.DateBlocked.Date == dateTime.Date) == true)
            {
                return RedirectToAction("Index", "TCMBilling", new { id = 1});
            }

            CaseMannagerEntity caseManager = _context.CaseManagers
                                                     .FirstOrDefault(cm => cm.LinkedUser == user_logged.UserName);

            TCMNoteViewModel model;

            if (User.IsInRole("CaseManager"))
            {
                TCMNoteEntity note = _context.TCMNote.FirstOrDefault(n => n.TCMClient.Id == IdTCMClient && n.DateOfService.Date == dateTime.Date);
                if (note == null)
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
                                                      .Where(na => na.UserName == user_logged.UserName),
                        Sign = false,
                        CodeBill = user_logged.Clinic.CPTCode_TCM,
                        Billable = billable

                    };
                    ViewData["origin"] = origin;
                    ViewData["available"] = UnitsAvailable(dateTime, IdTCMClient, true);
                    return View(model);
                }
                else
                {
                    return RedirectToAction("Edit", "TCMNotes", new { id = note.Id, origin = 2});
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
                            TCMDomain = await _context.TCMDomains.FirstOrDefaultAsync(n => n.Id == item.IdTCMDomain),
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
                            TCMServiceActivity = await _context.TCMServiceActivity.FirstOrDefaultAsync(n => n.Id == item.IdTCMServiceActivity),
                            Billable = item.Billable
                        };
                        _context.TCMNoteActivity.Add(noteActivity);                   
                    }

                    //delete all temporaly items
                    _context.RemoveRange(noteActivities);

                    try
                    {
                        await _context.SaveChangesAsync();

                        return RedirectToAction("Index", "TCMBilling", new { initDate = TcmNoteViewModel.DateOfService.ToString() });    
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

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult Edit(int id = 0, int origin = 0, int error = 0, string interval = "")
        {
            if (error == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }
            
            TCMNoteViewModel model;
            
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            DateTime dateservice = _context.TCMNote.FirstOrDefault(n => n.Id == id).DateOfService.Date;
            if (user_logged.Clinic.Setting.TCMLockCreateNote.Date <= dateservice)
            {
                return RedirectToAction("Index", "TCMBilling", new {id = 2, initDate = dateservice.ToString() });
            }
          
            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {

                    TCMNoteEntity TcmNote = _context.TCMNote
                                                    .Include(b => b.TCMClient)
                                                    .ThenInclude(b => b.Client)
                                                    .Include(b => b.TCMClient)
                                                    .ThenInclude(b => b.Casemanager)
                                                    .ThenInclude(b => b.TCMSupervisor)
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
                        ViewData["available"] = UnitsAvailable(dateservice, TcmNote.TCMClient.Id);
                        ViewData["interval"] = interval;
                        if (TcmNote.TCMNoteActivity.Count() > 0)
                        {
                            if (TcmNote.TCMNoteActivity.Where(n => n.Billable == false).Count() > 0)
                                model.Billable = false;
                            else
                                model.Billable = true;
                        }
                        else
                        {
                            model.Billable = true;
                        }
                        model.DateOfServiceReference = model.DateOfService;
                        return View(model);
                    }

                }
            }
            else
            {
                if (User.IsInRole("TCMSupervisor") == true && user_logged.Clinic.Setting.TCMSupervisorEdit == true)
                {
                    if (user_logged.Clinic != null)
                    {

                        TCMNoteEntity TcmNote = _context.TCMNote
                                                        .Include(b => b.TCMClient)
                                                        .ThenInclude(b => b.Client)
                                                        .Include(b => b.TCMClient)
                                                        .ThenInclude(b => b.Casemanager)
                                                        .ThenInclude(b => b.TCMSupervisor)
                                                        .Include(b => b.TCMNoteActivity)
                                                        .ThenInclude(b => b.TCMDomain)
                                                        .FirstOrDefault(m => m.Id == id
                                                                && m.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName);

                        if (TcmNote == null)
                        {
                            return RedirectToAction("NotAuthorized", "Account");
                        }
                        else
                        {
                            //redirect to note print report
                            if (TcmNote.Status == NoteStatus.Approved && origin != 7)
                            {
                                return RedirectToAction("PrintNote", new { id = TcmNote.Id });
                            }
                            
                            model = _converterHelper.ToTCMNoteViewModel(TcmNote);
                            if (TcmNote.Status == NoteStatus.Approved)
                            {
                                model.ApprovedDate = TcmNote.ApprovedDate;
                            }
                            model.TCMClient = TcmNote.TCMClient;
                            ViewData["origin"] = origin;
                            ViewData["available"] = UnitsAvailable(dateservice, TcmNote.TCMClient.Id);
                            ViewData["interval"] = interval;
                            if (TcmNote.TCMNoteActivity.Count() > 0)
                            {
                                if (TcmNote.TCMNoteActivity.Where(n => n.Billable == false).Count() > 0)
                                    model.Billable = false;
                                else
                                    model.Billable = true;
                            }
                            else
                            {
                                model.Billable = true;
                            }
                            return View(model);
                        }

                    }
                }
                else
                {
                    TCMNoteEntity TcmNote = _context.TCMNote
                                                       .Include(b => b.TCMClient)
                                                       .ThenInclude(b => b.Client)
                                                       .Include(b => b.TCMClient)
                                                       .ThenInclude(b => b.Casemanager)
                                                       .ThenInclude(b => b.TCMSupervisor)
                                                       .Include(b => b.TCMNoteActivity)
                                                       .ThenInclude(b => b.TCMDomain)
                                                       .FirstOrDefault(m => m.Id == id
                                                               && m.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName);

                    return RedirectToAction("EditReadOnly", new { id = TcmNote.Id, origi = 4 });
                }
            }
            model = new TCMNoteViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> Edit(TCMNoteViewModel tcmNotesViewModel, int origin = 0, string interval = "")
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            //si se cambia el dia de la nota, no puede existir una nota de ese cliente ese dia, si existe que le copie las actividades a mano
            if (_context.TCMNote.Where(n => n.DateOfService == tcmNotesViewModel.DateOfService && n.TCMClient.Id == tcmNotesViewModel.IdTCMClient && n.Id != tcmNotesViewModel.IdTCMNote).Count() > 0)
            {
                ViewData["origin"] = origin;
                ViewData["available"] = UnitsAvailable(tcmNotesViewModel.DateOfService, tcmNotesViewModel.IdTCMClient);
                ViewData["interval"] = interval;
                ViewBag.Delete = "Exists";
                tcmNotesViewModel.TCMNoteActivity = _context.TCMNoteActivity.Where(n => n.TCMNote.Id == tcmNotesViewModel.Id).ToList();
                return View(tcmNotesViewModel);
            }

            if (ModelState.IsValid)
            {
                //reviso al cambiar la fecha de la nota, que para el dia que se esta cambiando no exista overlaping
                if (tcmNotesViewModel.DateOfServiceReference.ToShortDateString() != tcmNotesViewModel.DateOfService.ToShortDateString())
                {
                    List<TCMNoteActivityEntity> listActivity = await _context.TCMNoteActivity.Where(n => n.TCMNote.Id == tcmNotesViewModel.IdTCMNote).ToListAsync();
                    foreach (var item in listActivity)
                    {
                        //casemanager overlapping validation
                        List<TCMNoteActivityEntity> noteActivities = await _context.TCMNoteActivity
                                                                                   .Where(na => (na.CreatedBy == user_logged.UserName
                                                                                     && na.Id != item.Id
                                                                                     && na.TCMNote.DateOfService.Date == tcmNotesViewModel.DateOfService.Date
                                                                                     && ((na.StartTime <= item.StartTime && na.EndTime > item.StartTime)
                                                                                        || (na.StartTime < item.EndTime && na.EndTime >= item.EndTime)
                                                                                        || (na.StartTime >= item.StartTime && na.EndTime <= item.EndTime))))
                                                                                   .ToListAsync();
                        if (noteActivities.Count() > 0)
                        {
                            ViewData["origin"] = origin;
                            ViewData["available"] = UnitsAvailable(tcmNotesViewModel.DateOfService, tcmNotesViewModel.IdTCMClient);
                            ViewData["interval"] = interval;
                            ViewBag.Delete = "Interval";
                            tcmNotesViewModel.TCMNoteActivity = _context.TCMNoteActivity.Where(n => n.TCMNote.Id == tcmNotesViewModel.Id).ToList();
                            return View(tcmNotesViewModel);

                        }

                        //check overlapin Mental Health
                        if (CheckOverlappingMH(item.StartTime, item.EndTime, _context.TCMClient.Include(n => n.Client).FirstOrDefault(n => n.Id == tcmNotesViewModel.IdTCMClient).Client.Id) == true)
                        {
                            ViewData["origin"] = origin;
                            ViewData["available"] = UnitsAvailable(tcmNotesViewModel.DateOfService, tcmNotesViewModel.IdTCMClient);
                            ViewData["interval"] = interval;
                            ViewBag.Delete = "MH";
                            tcmNotesViewModel.TCMNoteActivity = _context.TCMNoteActivity.Where(n => n.TCMNote.Id == tcmNotesViewModel.Id).ToList();
                            return View(tcmNotesViewModel);

                        }

                        //check que no tenga ese tiempo en supervision si esta habilitado en los setting el chequeo en presencia del TCM
                        if (user_logged.Clinic.Setting.TCMSupervisionTimeWithCaseManager == true)
                        {
                            TCMSupervisionTimeEntity supervision = _context.TCMSupervisionTimes
                                                                           .FirstOrDefault(n => n.CaseManager.TCMClients.FirstOrDefault(m => m.Id == tcmNotesViewModel.IdTCMClient) != null
                                                                           && n.DateSupervision.Date == tcmNotesViewModel.DateOfService.Date
                                                                           && ((n.StartTime.TimeOfDay <= item.StartTime.TimeOfDay && n.EndTime.TimeOfDay >= item.StartTime.TimeOfDay)
                                                                            || (n.StartTime.TimeOfDay <= item.EndTime.TimeOfDay && n.EndTime.TimeOfDay >= item.EndTime.TimeOfDay)
                                                                            || (n.StartTime.TimeOfDay <= item.StartTime.TimeOfDay && n.EndTime.TimeOfDay >= item.EndTime.TimeOfDay)
                                                                            || (n.StartTime.TimeOfDay >= item.StartTime.TimeOfDay && n.EndTime.TimeOfDay <= item.EndTime.TimeOfDay)));

                            if (supervision != null)
                            {
                                ViewData["origin"] = origin;
                                ViewData["available"] = UnitsAvailable(tcmNotesViewModel.DateOfService, tcmNotesViewModel.IdTCMClient);
                                ViewData["interval"] = interval;
                                ViewBag.Delete = "Supervision";
                                tcmNotesViewModel.TCMNoteActivity = _context.TCMNoteActivity.Where(n => n.TCMNote.Id == tcmNotesViewModel.Id).ToList();
                                return View(tcmNotesViewModel);

                            }

                        }
                    }

                }

                TCMNoteEntity tcmNotesEntity = await _converterHelper.ToTCMNoteEntity(tcmNotesViewModel, false, user_logged.UserName);
                if (tcmNotesEntity.Status == NoteStatus.Pending)
                {
                    tcmNotesEntity.Sign = true;
                }
                if (User.IsInRole("TCMSupervisor") && tcmNotesEntity.Status == NoteStatus.Approved)
                {
                    tcmNotesEntity.ApprovedDate = tcmNotesViewModel.ApprovedDate;
                    tcmNotesEntity.Sign = true;
                }
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

                List<TCMNoteActivityEntity> tcmNoteActivity = _context.TCMNoteActivity
                                                                      .Where(n => n.TCMNote.Id == tcmNotesViewModel.IdTCMNote)
                                                                      .ToList();
                foreach (var item in tcmNoteActivity)
                {
                    item.StartTime = new DateTime(tcmNotesViewModel.DateOfService.Year, tcmNotesViewModel.DateOfService.Month, tcmNotesViewModel.DateOfService.Day,item.StartTime.Hour,item.StartTime.Minute,0);
                    item.EndTime = new DateTime(tcmNotesViewModel.DateOfService.Year, tcmNotesViewModel.DateOfService.Month, tcmNotesViewModel.DateOfService.Day, item.EndTime.Hour, item.EndTime.Minute, 0);
                    _context.TCMNoteActivity.Update(item);
                }

                _context.TCMNote.Update(tcmNotesEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    if (User.IsInRole("CaseManager"))
                    {
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
                            return RedirectToAction("Index", "TCMBilling", new { initDate = tcmNotesViewModel.DateOfService.ToString() });
                        }
                        if (origin == 3)
                        {
                            return RedirectToAction("NotesWithReview");
                        }
                        if (origin == 4)
                        {
                            return RedirectToAction("MessagesOfNotes", "TCMMessages");
                        }
                        if (origin == 5)
                        {
                            return RedirectToAction("FinishEditingNote", new { id = tcmNotesViewModel.Id, origin = 2 });
                        }
                        if (origin == 6)
                        {
                            return RedirectToAction("NotesStatus", new { status = NoteStatus.Pending });
                        }                       
                    }
                    else
                    {
                        if (User.IsInRole("TCMSupervisor"))
                        {
                            if (origin == 7)
                            {
                                return RedirectToAction("UpdateNote", new { dateInterval = interval, idCaseManager = tcmNotesViewModel.IdCaseManager, idTCMClient = tcmNotesViewModel.IdTCMClient} );
                            }
                            else
                            {
                                return RedirectToAction("NotesStatus", new { status = NoteStatus.Pending });
                            }
                        }
                    }                    
                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }

            }

            ViewData["origin"] = origin;
            ViewData["available"] = UnitsAvailable(tcmNotesViewModel.DateOfService, tcmNotesViewModel.IdTCMClient);
            ViewData["interval"] = interval;
            tcmNotesViewModel.TCMNoteActivity = _context.TCMNoteActivity.Where(n => n.TCMNote.Id == tcmNotesViewModel.Id).ToList();
            return View(tcmNotesViewModel);
            
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
        public IActionResult CreateNoteActivity(int idNote = 0, int idTCMClient = 0, int unitsAvaliable = 0, bool billable = true)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMNoteActivityViewModel model;
           
            TCMNoteEntity note = _context.TCMNote

                                         .Include(n => n.TCMNoteActivity)

                                         .Include(n => n.TCMClient)
                                         .ThenInclude(n => n.Client)

                                         .FirstOrDefault(n => n.Id == idNote);

            DateTime StartTime = note.DateOfService;
            IEnumerable<SelectListItem> list_Services = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == idTCMClient).Id, note.DateOfService);

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
                    IdSetting = 5,
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
                    IdTCMActivity = 0,
                    Units = 1,
                    TimeEnd = StartTime.AddMinutes(15).ToShortTimeString()
                };
                if (model.TCMNote.TCMNoteActivity == null)
                    model.TCMNote.TCMNoteActivity = new List<TCMNoteActivityEntity>();

                ViewData["unitsAvaliable"] = unitsAvaliable;
                return View(model);
            }

            ViewData["unitsAvaliable"] = unitsAvaliable;
            return View(null);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager")]
        public async Task<IActionResult> CreateNoteActivity(TCMNoteActivityViewModel TcmNotesViewModel, int unitsAvaliable = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TcmNotesViewModel.EndTime = TcmNotesViewModel.StartTime.AddMinutes(TcmNotesViewModel.Minutes);

            if (ModelState.IsValid)
            {
                if (TcmNotesViewModel.IdTCMDomain == 0)
                {
                    DateTime open = _context.TCMClient.FirstOrDefault(n => n.Id == TcmNotesViewModel.IdTCMClient).DataOpen;
                    TCMServicePlanEntity sp = _context.TCMServicePlans
                                                      .Include(n => n.TCMServicePlanReview)
                                                      .FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient);
                    DateTime dateServicePlan;
                    if (sp.TCMServicePlanReview != null && sp.TCMServicePlanReview.Approved == 2)
                    {
                        dateServicePlan = sp.TCMServicePlanReview.DateServicePlanReview;
                    }
                    else
                    {
                        dateServicePlan = sp.DateServicePlan;
                    }
                     
                    if (TcmNotesViewModel.DateOfServiceNote < open || TcmNotesViewModel.DateOfServiceNote > dateServicePlan || user_logged.Clinic.Setting.CreateTCMNotesWithoutDomain == false)
                    {
                        TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                        TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, TcmNotesViewModel.DateOfServiceNote);

                        if (TcmNotesViewModel.IdTCMDomain != 0)
                        {
                            TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                            TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                        }

                        ModelState.AddModelError(string.Empty, $"Error. The date is not valid for services without domains.");
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivityTemp", TcmNotesViewModel) });

                    }
                }

                TCMNoteActivityEntity IndividualEntity = _context.TCMNoteActivity.Find(TcmNotesViewModel.Id);
                if (IndividualEntity == null)
                {
                    TcmNotesViewModel.TCMDomain = await _context.TCMDomains.FirstOrDefaultAsync(n => n.Id == TcmNotesViewModel.IdTCMDomain);

                    TCMNoteEntity note = await _context.TCMNote
                                                       .Include(n => n.TCMClient)
                                                       .FirstOrDefaultAsync(n => n.Id == TcmNotesViewModel.IdTCMNote);

                    TcmNotesViewModel.StartTime = new DateTime(note.DateOfService.Year, note.DateOfService.Month, note.DateOfService.Day, TcmNotesViewModel.StartTime.Hour, TcmNotesViewModel.StartTime.Minute, 0);
                    TcmNotesViewModel.EndTime = new DateTime(note.DateOfService.Year, note.DateOfService.Month, note.DateOfService.Day, TcmNotesViewModel.EndTime.Hour, TcmNotesViewModel.EndTime.Minute, 0);

                    //casemanager overlapping validation
                    List<TCMNoteActivityEntity> noteActivities = await _context.TCMNoteActivity
                                                                               .Where(na => (na.CreatedBy == user_logged.UserName
                                                                                 && na.TCMNote.DateOfService.Date == TcmNotesViewModel.DateOfServiceNote.Date
                                                                                 && ((na.StartTime <= TcmNotesViewModel.StartTime && na.EndTime > TcmNotesViewModel.StartTime)
                                                                                    || (na.StartTime < TcmNotesViewModel.EndTime && na.EndTime >= TcmNotesViewModel.EndTime)
                                                                                    || (na.StartTime >= TcmNotesViewModel.StartTime && na.EndTime <= TcmNotesViewModel.EndTime))))
                                                                               .ToListAsync();

                    int cantidad = _context.TCMNoteActivity.Where(n => n.TCMNote.TCMClient.Id == TcmNotesViewModel.IdTCMClient && n.TCMNote.DateOfService.Date == TcmNotesViewModel.DateOfServiceNote.Date).Sum(m => m.Minutes);
                    //Verifico la cantidad de unidades por cliente segun los setting
                    if (user_logged.Clinic.Setting.UnitsForDayForClient < CalculateUnits(TcmNotesViewModel.Minutes + cantidad))
                    {
                        TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                               .Include(n => n.TCMClient)
                                                               .ThenInclude(n => n.Client)
                                                               .FirstOrDefaultAsync(m => m.Id == TcmNotesViewModel.IdTCMNote);

                        TcmNotesViewModel.TCMNote = tcmNote1;
                        TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                        TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, tcmNote1.DateOfService);
                        if (TcmNotesViewModel.IdTCMDomain != 0)
                        {
                            TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                            TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                        }

                        ModelState.AddModelError(string.Empty, $"Error. The amount of units exceeds the setting's value");
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivity", TcmNotesViewModel) });

                    }

                    //Verifico que la actividad este en el rango de fecha segun los setting
                    if (CheckTimeRange(TcmNotesViewModel.StartTime, TcmNotesViewModel.EndTime))
                    {
                        TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                               .Include(n => n.TCMClient)
                                                               .ThenInclude(n => n.Client)
                                                               .FirstOrDefaultAsync(m => m.Id == TcmNotesViewModel.IdTCMNote);

                        TcmNotesViewModel.TCMNote = tcmNote1;
                        TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                        TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, tcmNote1.DateOfService);
                        if (TcmNotesViewModel.IdTCMDomain != 0)
                        {
                            TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                            TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                        }

                        ModelState.AddModelError(string.Empty, $"Error. There are activities created in that authorized time");
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivity", TcmNotesViewModel) });

                    }

                    //Lock tcmnotes for unavaliable units
                    if ((user_logged.Clinic.Setting.LockTCMNoteForUnits == true && (unitsAvaliable - CalculateUnits(TcmNotesViewModel.Minutes)) < 0))
                    {
                        TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                               .Include(n => n.TCMClient)
                                                               .ThenInclude(n => n.Client)
                                                               .FirstOrDefaultAsync(m => m.Id == TcmNotesViewModel.IdTCMNote);

                        TcmNotesViewModel.TCMNote = tcmNote1;
                        TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                        TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, tcmNote1.DateOfService);
                        if (TcmNotesViewModel.IdTCMDomain != 0)
                        {
                            TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                            TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                        }

                        ModelState.AddModelError(string.Empty, $"Error. This note can not be created because the client has no units available");
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivity", TcmNotesViewModel) });

                    }

                    //Verifico la Authorization
                    if (user_logged.Clinic.Setting.LockTCMNoteForUnits  == true && VerifyAuthorization(TcmNotesViewModel.IdTCMClient, TcmNotesViewModel.DateOfServiceNote) == false)
                    {
                        TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                               .Include(n => n.TCMClient)
                                                               .ThenInclude(n => n.Client)
                                                               .FirstOrDefaultAsync(m => m.Id == TcmNotesViewModel.IdTCMNote);

                        TcmNotesViewModel.TCMNote = tcmNote1;
                        TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                        TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, tcmNote1.DateOfService);
                        if (TcmNotesViewModel.IdTCMDomain != 0)
                        {
                            TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                            TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                        }

                        ModelState.AddModelError(string.Empty, $"Error. The client does not have valid authorization for this time");
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivity", TcmNotesViewModel) });

                    }

                    //check overlapin
                    if (noteActivities.Count() > 0)
                    {
                        TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                               .Include(n => n.TCMClient)
                                                               .ThenInclude(n => n.Client)
                                                               .FirstOrDefaultAsync(m => m.Id == TcmNotesViewModel.IdTCMNote);

                        TcmNotesViewModel.TCMNote = tcmNote1;
                        TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                        TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, tcmNote1.DateOfService);
                        if (TcmNotesViewModel.IdTCMDomain != 0)
                        {
                            TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                            TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                        }

                        ModelState.AddModelError(string.Empty, $"Error. There are activities created in that time interval");
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivity", TcmNotesViewModel) });

                    }

                    //check overlapin Mental Health
                    if (CheckOverlappingMH(TcmNotesViewModel.StartTime, TcmNotesViewModel.EndTime, _context.TCMClient.Include(n => n.Client).FirstOrDefault(n => n.Id == TcmNotesViewModel.IdTCMClient).Client.Id) == true)
                    {
                        TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                               .Include(n => n.TCMClient)
                                                               .ThenInclude(n => n.Client)
                                                               .FirstOrDefaultAsync(m => m.Id == TcmNotesViewModel.IdTCMNote);

                        TcmNotesViewModel.TCMNote = tcmNote1;
                        TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                        TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id,tcmNote1.DateOfService);
                        if (TcmNotesViewModel.IdTCMDomain != 0)
                        {
                            TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                            TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                        }

                        ModelState.AddModelError(string.Empty, $"Error. There are activities created in that time interval in other service (Mental Health)");
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivity", TcmNotesViewModel) });

                    }

                    //check que no tenga ese tiempo en supervision si esta habilitado en los setting el chequeo en presencia del TCM
                    if (user_logged.Clinic.Setting.TCMSupervisionTimeWithCaseManager == true)
                    {
                        TCMSupervisionTimeEntity supervision = _context.TCMSupervisionTimes
                                                                       .FirstOrDefault(n => n.CaseManager.TCMClients.FirstOrDefault(m => m.Id == TcmNotesViewModel.IdTCMClient) != null
                                                                       && n.DateSupervision.Date == TcmNotesViewModel.DateOfServiceNote.Date
                                                                       && ((n.StartTime.TimeOfDay <= TcmNotesViewModel.StartTime.TimeOfDay && n.EndTime.TimeOfDay >= TcmNotesViewModel.StartTime.TimeOfDay)
                                                                        || (n.StartTime.TimeOfDay <= TcmNotesViewModel.EndTime.TimeOfDay && n.EndTime.TimeOfDay >= TcmNotesViewModel.EndTime.TimeOfDay)
                                                                        || (n.StartTime.TimeOfDay <= TcmNotesViewModel.StartTime.TimeOfDay && n.EndTime.TimeOfDay >= TcmNotesViewModel.EndTime.TimeOfDay)
                                                                        || (n.StartTime.TimeOfDay >= TcmNotesViewModel.StartTime.TimeOfDay && n.EndTime.TimeOfDay <= TcmNotesViewModel.EndTime.TimeOfDay)));

                        if (supervision != null)
                        {
                            TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                                   .Include(n => n.TCMClient)
                                                                   .ThenInclude(n => n.Client)
                                                                   .FirstOrDefaultAsync(m => m.Id == TcmNotesViewModel.IdTCMNote);

                            TcmNotesViewModel.TCMNote = tcmNote1;
                            TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                            TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, tcmNote1.DateOfService);
                            if (TcmNotesViewModel.IdTCMDomain != 0)
                            {
                                TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                                TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                            }

                            ModelState.AddModelError(string.Empty, $"Error. This time is scheduled for supervision.");
                            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivity", TcmNotesViewModel) });
                        }
                       

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
                        ViewData["Id"] = UnitsAvailable(note.DateOfService, note.TCMClient.Id);
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
            TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, tcmNote.DateOfService);
            if (TcmNotesViewModel.IdTCMDomain != 0)
            {
                TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivity", TcmNotesViewModel) });
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult CreateNoteActivityTemp(DateTime initDate, int idTCMClient = 0, bool billable = true)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMNoteActivityViewModel model;
            IEnumerable<SelectListItem> list_Services = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == idTCMClient).Id, initDate);

            if (user_logged.Clinic != null)
            {
                model = new TCMNoteActivityViewModel
                {
                    Id = 0,
                    IdTCMDomain = 0,
                    DomainList = list_Services,
                    DescriptionOfService = "",
                    Minutes = 15,
                    IdSetting = 5,
                    SettingList = _combosHelper.GetComboTCMNoteSetting(),
                    TCMDomain = new TCMDomainEntity(),
                    CreatedBy = user_logged.UserName,
                    CreatedOn = DateTime.Now,
                    IdTCMClient = idTCMClient,
                    DescriptionTemp = "",
                    StartTime = initDate,
                    TimeEnd = initDate.AddMinutes(15).ToShortTimeString(),
                    DateOfServiceNote = initDate,
                    Units = 1,
                    Billable = billable
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
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            
            if (ModelState.IsValid)
            {
                if (TcmNotesViewModel.IdTCMDomain == 0)
                {
                    DateTime open = _context.TCMClient.FirstOrDefault(n => n.Id == TcmNotesViewModel.IdTCMClient).DataOpen;
                    TCMServicePlanEntity sp = _context.TCMServicePlans
                                                      .Include(n => n.TCMServicePlanReview)
                                                      .FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient);

                    DateTime dateServicePlan;
                    if (sp.TCMServicePlanReview != null && sp.TCMServicePlanReview.Approved == 2)
                    {
                        dateServicePlan = sp.TCMServicePlanReview.DateServicePlanReview;
                    }
                    else
                    {
                        dateServicePlan = sp.DateServicePlan;
                    }

                    if (TcmNotesViewModel.DateOfServiceNote < open || TcmNotesViewModel.DateOfServiceNote > dateServicePlan || user_logged.Clinic.Setting.CreateTCMNotesWithoutDomain == false)
                    {
                        TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                        TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, TcmNotesViewModel.DateOfServiceNote);

                        if (TcmNotesViewModel.IdTCMDomain != 0)
                        {
                            TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                            TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                        }

                        ModelState.AddModelError(string.Empty, $"Error. The date is not valid for services without domains.");
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivityTemp", TcmNotesViewModel) });

                    }
                }


                TcmNotesViewModel.EndTime = TcmNotesViewModel.StartTime.AddMinutes(TcmNotesViewModel.Minutes);
                 
                //TcmNotesViewModel.TCMDomain = await _context.TCMDomains.FirstOrDefaultAsync(n => n.Id == TcmNotesViewModel.IdTCMDomain);

                TcmNotesViewModel.StartTime = new DateTime(TcmNotesViewModel.DateOfServiceNote.Year, TcmNotesViewModel.DateOfServiceNote.Month, TcmNotesViewModel.DateOfServiceNote.Day, TcmNotesViewModel.StartTime.Hour, TcmNotesViewModel.StartTime.Minute, 0);
                TcmNotesViewModel.EndTime = new DateTime(TcmNotesViewModel.DateOfServiceNote.Year, TcmNotesViewModel.DateOfServiceNote.Month, TcmNotesViewModel.DateOfServiceNote.Day, TcmNotesViewModel.EndTime.Hour, TcmNotesViewModel.EndTime.Minute, 0);

                TCMNoteActivityTempEntity entity = _converterHelper.ToTCMNoteActivityTempEntity(TcmNotesViewModel, true, user_logged.UserName);

                //casemanager overlapping validation
                List<TCMNoteActivityEntity> noteActivities = await _context.TCMNoteActivity
                                                                           .Where(na => (na.CreatedBy == user_logged.UserName
                                                                             && na.TCMNote.DateOfService.Date == TcmNotesViewModel.DateOfServiceNote.Date
                                                                             && ((na.StartTime <= TcmNotesViewModel.StartTime && na.EndTime > TcmNotesViewModel.StartTime)
                                                                                || (na.StartTime < TcmNotesViewModel.EndTime && na.EndTime >= TcmNotesViewModel.EndTime)
                                                                                || (na.StartTime >= TcmNotesViewModel.StartTime && na.EndTime <= TcmNotesViewModel.EndTime))))
                                                                           .ToListAsync();
                List<TCMNoteActivityTempEntity> noteActivitiesTemp = await _context.TCMNoteActivityTemp
                                                                                   .Where(na => (na.UserName == user_logged.UserName
                                                                                        && na.DateOfServiceOfNote.Date == TcmNotesViewModel.DateOfServiceNote.Date
                                                                                        && ((na.StartTime <= TcmNotesViewModel.StartTime && na.EndTime > TcmNotesViewModel.StartTime)
                                                                                            || (na.StartTime < TcmNotesViewModel.EndTime && na.EndTime >= TcmNotesViewModel.EndTime)
                                                                                            || (na.StartTime >= TcmNotesViewModel.StartTime && na.EndTime <= TcmNotesViewModel.EndTime))))
                                                                                   .ToListAsync();

                //Verifico la cantidad de unidades por cliente segun los setting
                if (user_logged.Clinic.Setting.UnitsForDayForClient < CalculateUnits(TcmNotesViewModel.Minutes + _context.TCMNoteActivityTemp.Where(n => n.IdTCMClient == TcmNotesViewModel.IdTCMClient && n.DateOfServiceOfNote.Date == TcmNotesViewModel.DateOfServiceNote.Date).Sum(m => m.Minutes)))
                {
                    TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                    TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, TcmNotesViewModel.DateOfServiceNote);

                    if (TcmNotesViewModel.IdTCMDomain != 0)
                    {
                        TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                        TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                    }

                    ModelState.AddModelError(string.Empty, $"Error. The amount of units exceeds the setting's value");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivityTemp", TcmNotesViewModel) });

                }

                //Lock tcmnotes for unavaliable units
                if ((user_logged.Clinic.Setting.LockTCMNoteForUnits == true && (UnitsAvailable(TcmNotesViewModel.DateOfServiceNote, TcmNotesViewModel.IdTCMClient, true) - CalculateUnits(TcmNotesViewModel.Minutes)) < 0))
                {
                    TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                    TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, TcmNotesViewModel.DateOfServiceNote);

                    if (TcmNotesViewModel.IdTCMDomain != 0)
                    {
                        TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                        TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                    }

                    ModelState.AddModelError(string.Empty, $"Error. This note can not be created because the client has no units available");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivityTemp", TcmNotesViewModel) });
                }

                //Lock tcmnotes for valid authorization 
                if ((user_logged.Clinic.Setting.LockTCMNoteForUnits == true && VerifyAuthorization(TcmNotesViewModel.IdTCMClient, TcmNotesViewModel.DateOfServiceNote) == false))
                {
                    TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                    TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, TcmNotesViewModel.DateOfServiceNote);

                    if (TcmNotesViewModel.IdTCMDomain != 0)
                    {
                        TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                        TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                    }

                    ModelState.AddModelError(string.Empty, $"Error. The client does not have valid authorization for this time");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivityTemp", TcmNotesViewModel) });
                }

                //verifico que la actividad este en el rango de fecha segun los setting
                if (CheckTimeRange(TcmNotesViewModel.StartTime, TcmNotesViewModel.EndTime))
                {
                    TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                    TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, TcmNotesViewModel.DateOfServiceNote);

                    if (TcmNotesViewModel.IdTCMDomain != 0)
                    {
                        TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                        TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                    }

                    ModelState.AddModelError(string.Empty, $"Error. There are activities created in that authorized time");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivityTemp", TcmNotesViewModel) });
                }

                //check overlapin
                if (noteActivities.Count() > 0 || noteActivitiesTemp.Count() > 0 )
                {
                    TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                    TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, TcmNotesViewModel.DateOfServiceNote);
                    
                    if (TcmNotesViewModel.IdTCMDomain != 0)
                    {
                        TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                        TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                    }

                    ModelState.AddModelError(string.Empty, $"Error. There are activities created in that time interval");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivityTemp", TcmNotesViewModel) });
                }

                //check overlapin Mental Health
                if (CheckOverlappingMH(TcmNotesViewModel.StartTime, TcmNotesViewModel.EndTime, _context.TCMClient.Include(n => n.Client).FirstOrDefault(n => n.Id == TcmNotesViewModel.IdTCMClient).Client.Id) == true)
                {
                    TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                    TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, TcmNotesViewModel.DateOfServiceNote);

                    if (TcmNotesViewModel.IdTCMDomain != 0)
                    {
                        TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                        TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                    }

                    ModelState.AddModelError(string.Empty, $"Error. There are activities created in that time interval in other service (Mental Helath)");
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivityTemp", TcmNotesViewModel) });
                }

                //check que no tenga ese tiempo en supervision si esta habilitado en los setting el chequeo en presencia del TCM
                if (user_logged.Clinic.Setting.TCMSupervisionTimeWithCaseManager == true)
                {
                    TCMSupervisionTimeEntity supervision = _context.TCMSupervisionTimes
                                                                   .FirstOrDefault(n => n.CaseManager.TCMClients.FirstOrDefault(m => m.Id == TcmNotesViewModel.IdTCMClient) != null
                                                                   && n.DateSupervision.Date == TcmNotesViewModel.DateOfServiceNote.Date
                                                                   && ((n.StartTime.TimeOfDay <= TcmNotesViewModel.StartTime.TimeOfDay && n.EndTime.TimeOfDay >= TcmNotesViewModel.StartTime.TimeOfDay)
                                                                    || (n.StartTime.TimeOfDay <= TcmNotesViewModel.EndTime.TimeOfDay && n.EndTime.TimeOfDay >= TcmNotesViewModel.EndTime.TimeOfDay)
                                                                    || (n.StartTime.TimeOfDay <= TcmNotesViewModel.StartTime.TimeOfDay && n.EndTime.TimeOfDay >= TcmNotesViewModel.EndTime.TimeOfDay)
                                                                    || (n.StartTime.TimeOfDay >= TcmNotesViewModel.StartTime.TimeOfDay && n.EndTime.TimeOfDay <= TcmNotesViewModel.EndTime.TimeOfDay)));

                    if (supervision != null)
                    {
                        TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                        TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, TcmNotesViewModel.DateOfServiceNote);

                        if (TcmNotesViewModel.IdTCMDomain != 0)
                        {
                            TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                            TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                        }

                        ModelState.AddModelError(string.Empty, $"Error. This time is scheduled for supervision.");
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivityTemp", TcmNotesViewModel) });

                    }

                }

                _context.TCMNoteActivityTemp.Add(entity);
                try
                {
                    await _context.SaveChangesAsync();

                    IEnumerable<TCMNoteActivityTempEntity> NotesActivityList = await _context.TCMNoteActivityTemp

                                                                                             .Where(g => g.UserName == user_logged.UserName)
                                                                                             .ToListAsync();
                    ViewData["Id"] = UnitsAvailable(TcmNotesViewModel.DateOfServiceNote, TcmNotesViewModel.IdTCMClient, true);
                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewTCMNoteActivityTemp", NotesActivityList) });

                }
                catch (System.Exception ex)
                {
                    ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                }
            }

            TcmNotesViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
            TcmNotesViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == TcmNotesViewModel.IdTCMClient).Id, TcmNotesViewModel.DateOfServiceNote);
            if (TcmNotesViewModel.IdTCMDomain != 0)
            {
                TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == TcmNotesViewModel.IdTCMDomain);
                TcmNotesViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateNoteActivityTemp", TcmNotesViewModel) });
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public IActionResult EditNoteActivity(int id = 0, int unitsAvaliable = 0)
        {
            TCMNoteActivityViewModel model;

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(n => n.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            int units = 0;
            int residuo = 0;

            if (User.IsInRole("CaseManager"))
            {
                if (user_logged.Clinic != null)
                {

                    TCMNoteActivityEntity NoteActivity = _context.TCMNoteActivity
                                                                 .Include(m => m.TCMDomain)
                                                                 .Include(m => m.TCMNote)
                                                                 .ThenInclude(m => m.TCMClient)
                                                                 .ThenInclude(m => m.Client)
                                                                 .ThenInclude(m => m.Clinic)
                                                                 .ThenInclude(m => m.Setting)
                                                                 .Include(m => m.TCMServiceActivity)
                                                                 .FirstOrDefault(m => m.Id == id);
                    if (NoteActivity == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {
                        if (NoteActivity.TCMDomain == null)
                        {
                            NoteActivity.TCMDomain = new TCMDomainEntity();
                        }
                        if (NoteActivity.TCMServiceActivity == null)
                        {
                            NoteActivity.TCMServiceActivity = new TCMServiceActivityEntity();
                        }
                        model = _converterHelper.ToTCMNoteActivityViewModel(NoteActivity);

                        units = model.Minutes / 15;
                        residuo = model.Minutes % 15;
                        if (residuo > 7)
                            model.Units = units + 1;
                        else
                            model.Units = units;
                        ViewData["unitsAvaliable"] = unitsAvaliable;
                        return View(model);
                    }

                }
            }
            if (User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {

                    TCMNoteActivityEntity NoteActivity = _context.TCMNoteActivity
                                                                 .Include(m => m.TCMDomain)
                                                                 .Include(m => m.TCMNote)
                                                                 .ThenInclude(m => m.TCMClient)
                                                                 .ThenInclude(m => m.Client)
                                                                 .ThenInclude(m => m.Clinic)
                                                                 .ThenInclude(m => m.Setting)
                                                                 .Include(m => m.TCMServiceActivity)
                                                                 .FirstOrDefault(m => m.Id == id
                                                                            && m.TCMNote.TCMClient.Casemanager.Clinic.Setting.TCMSupervisorEdit == true);
                    if (NoteActivity == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {
                        if (NoteActivity.TCMDomain == null)
                        {
                            NoteActivity.TCMDomain = new TCMDomainEntity();
                        }
                        if (NoteActivity.TCMServiceActivity == null)
                        {
                            NoteActivity.TCMServiceActivity = new TCMServiceActivityEntity();
                        }

                        model = _converterHelper.ToTCMNoteActivityViewModel(NoteActivity);

                        units = model.Minutes / 15;
                        residuo = model.Minutes % 15;
                        if (residuo > 7)
                            model.Units = units + 1;
                        else
                            model.Units = units;
                        ViewData["unitsAvaliable"] = unitsAvaliable;
                        return View(model);
                    }

                }
            }

            model = new TCMNoteActivityViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> EditNoteActivity(TCMNoteActivityViewModel NoteActivityViewModel, int unitsAvaliable = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            NoteActivityViewModel.EndTime = NoteActivityViewModel.StartTime.AddMinutes(NoteActivityViewModel.Minutes);

            if (ModelState.IsValid)
            {
                if (NoteActivityViewModel.IdTCMDomain == 0)
                {
                    DateTime open = _context.TCMClient.FirstOrDefault(n => n.Id == NoteActivityViewModel.IdTCMClient).DataOpen;

                    TCMServicePlanEntity sp = _context.TCMServicePlans
                                                      .Include(n => n.TCMServicePlanReview)
                                                      .FirstOrDefault(n => n.TcmClient.Id == NoteActivityViewModel.IdTCMClient);

                    DateTime dateServicePlan;
                    if (sp.TCMServicePlanReview != null && sp.TCMServicePlanReview.Approved == 2)
                    {
                        dateServicePlan = sp.TCMServicePlanReview.DateServicePlanReview;
                    }
                    else
                    {
                        dateServicePlan = sp.DateServicePlan;
                    }

                    if (NoteActivityViewModel.DateOfServiceNote < open || NoteActivityViewModel.DateOfServiceNote > dateServicePlan || user_logged.Clinic.Setting.CreateTCMNotesWithoutDomain == false)
                    {
                        TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                               .Include(n => n.TCMClient)
                                                               .ThenInclude(n => n.Client)
                                                               .FirstOrDefaultAsync(m => m.Id == NoteActivityViewModel.IdTCMNote);

                        NoteActivityViewModel.TCMNote = tcmNote1;
                        NoteActivityViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                        NoteActivityViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == NoteActivityViewModel.IdTCMClient).Id, tcmNote1.DateOfService);
                        if (NoteActivityViewModel.IdTCMDomain != 0)
                        {
                            TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == NoteActivityViewModel.IdTCMDomain);
                            NoteActivityViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                        }

                        ModelState.AddModelError(string.Empty, $"Error. The date is not valid for services without domains.");
                        ViewData["unitsAvaliable"] = unitsAvaliable;
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditNoteActivity", NoteActivityViewModel) });
                    }
                }

                NoteActivityViewModel.TCMDomain = await _context.TCMDomains.FirstOrDefaultAsync(n => n.Id == NoteActivityViewModel.IdTCMDomain);

                TCMNoteEntity note = await _context.TCMNote
                                                   .Include(n => n.TCMClient)
                                                   .FirstOrDefaultAsync(n => n.Id == NoteActivityViewModel.IdTCMNote);

                NoteActivityViewModel.StartTime = new DateTime(note.DateOfService.Year, note.DateOfService.Month, note.DateOfService.Day, NoteActivityViewModel.StartTime.Hour, NoteActivityViewModel.StartTime.Minute, 0);
                NoteActivityViewModel.EndTime = new DateTime(note.DateOfService.Year, note.DateOfService.Month, note.DateOfService.Day, NoteActivityViewModel.EndTime.Hour, NoteActivityViewModel.EndTime.Minute, 0);

                int clientId = _context.TCMClient.Include(n => n.Client).FirstOrDefault(n => n.Id == NoteActivityViewModel.IdTCMClient).Client.Id;

                //Verifico la cantidad de unidades por cliente segun los setting
                if (user_logged.Clinic.Setting.UnitsForDayForClient < CalculateUnits(NoteActivityViewModel.Minutes + _context.TCMNoteActivity.Where(n => n.TCMNote.TCMClient.Id == NoteActivityViewModel.IdTCMClient && n.Id != NoteActivityViewModel.Id && n.TCMNote.DateOfService.Date == NoteActivityViewModel.DateOfServiceNote.Date).Sum(m => m.Minutes)))
                {
                    TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                           .Include(n => n.TCMClient)
                                                           .ThenInclude(n => n.Client)
                                                           .FirstOrDefaultAsync(m => m.Id == NoteActivityViewModel.IdTCMNote);

                    NoteActivityViewModel.TCMNote = tcmNote1;
                    NoteActivityViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                    NoteActivityViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == NoteActivityViewModel.IdTCMClient).Id, tcmNote1.DateOfService);
                    if (NoteActivityViewModel.IdTCMDomain != 0)
                    {
                        TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == NoteActivityViewModel.IdTCMDomain);
                        NoteActivityViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                    }

                    ModelState.AddModelError(string.Empty, $"Error. The amount of units exceeds the setting's value");
                    ViewData["unitsAvaliable"] = unitsAvaliable;
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditNoteActivity", NoteActivityViewModel) });

                }
                
                //Lock tcmnotes for unavaliable units
                if (user_logged.Clinic.Setting.LockTCMNoteForUnits == true)
                {
                    if ((unitsAvaliable - (CalculateUnits(NoteActivityViewModel.Minutes))) < 0)
                    {
                        TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                               .Include(n => n.TCMClient)
                                                               .ThenInclude(n => n.Client)
                                                               .FirstOrDefaultAsync(m => m.Id == NoteActivityViewModel.IdTCMNote);

                        NoteActivityViewModel.TCMNote = tcmNote1;
                        NoteActivityViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                        NoteActivityViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == NoteActivityViewModel.IdTCMClient).Id, tcmNote1.DateOfService);
                        if (NoteActivityViewModel.IdTCMDomain != 0)
                        {
                            TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == NoteActivityViewModel.IdTCMDomain);
                            NoteActivityViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                        }

                        ModelState.AddModelError(string.Empty, $"Error. This note can not be created because the client has no units available");
                        ViewData["unitsAvaliable"] = unitsAvaliable;
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditNoteActivity", NoteActivityViewModel) });
                    }
                    
                }

                //Lock tcmnotes for authorization
                if (user_logged.Clinic.Setting.LockTCMNoteForUnits == true && VerifyAuthorization(NoteActivityViewModel.IdTCMClient, NoteActivityViewModel.DateOfServiceNote) == false)
                {
                    TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                           .Include(n => n.TCMClient)
                                                           .ThenInclude(n => n.Client)
                                                           .FirstOrDefaultAsync(m => m.Id == NoteActivityViewModel.IdTCMNote);

                    NoteActivityViewModel.TCMNote = tcmNote1;
                    NoteActivityViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                    NoteActivityViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == NoteActivityViewModel.IdTCMClient).Id, tcmNote1.DateOfService);
                    if (NoteActivityViewModel.IdTCMDomain != 0)
                    {
                        TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == NoteActivityViewModel.IdTCMDomain);
                        NoteActivityViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                    }

                    ModelState.AddModelError(string.Empty, $"Error. The client does not have valid authorization for this time");
                    ViewData["unitsAvaliable"] = unitsAvaliable;
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditNoteActivity", NoteActivityViewModel) });
                }

                //verifica que la nota este en el rango de fecha segun los setting
                if (CheckTimeRange(NoteActivityViewModel.StartTime, NoteActivityViewModel.EndTime))
                {
                    TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                           .Include(n => n.TCMClient)
                                                           .ThenInclude(n => n.Client)
                                                           .FirstOrDefaultAsync(m => m.Id == NoteActivityViewModel.IdTCMNote);

                    NoteActivityViewModel.TCMNote = tcmNote1;
                    NoteActivityViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                    NoteActivityViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == NoteActivityViewModel.IdTCMClient).Id, tcmNote1.DateOfService);
                    if (NoteActivityViewModel.IdTCMDomain != 0)
                    {
                        TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == NoteActivityViewModel.IdTCMDomain);
                        NoteActivityViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                    }

                    ModelState.AddModelError(string.Empty, $"Error.There are activities created in that authorized time");
                    ViewData["unitsAvaliable"] = unitsAvaliable;
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditNoteActivity", NoteActivityViewModel) });

                }

                //casemanager overlapping validation
                List<TCMNoteActivityEntity> noteActivities = await _context.TCMNoteActivity
                                                                           .Where(na => (na.CreatedBy == user_logged.UserName
                                                                             && na.Id != NoteActivityViewModel.Id
                                                                             && na.TCMNote.DateOfService.Date == NoteActivityViewModel.DateOfServiceNote.Date
                                                                             && ((na.StartTime <= NoteActivityViewModel.StartTime && na.EndTime > NoteActivityViewModel.StartTime)
                                                                                || (na.StartTime < NoteActivityViewModel.EndTime && na.EndTime >= NoteActivityViewModel.EndTime)
                                                                                || (na.StartTime >= NoteActivityViewModel.StartTime && na.EndTime <= NoteActivityViewModel.EndTime))))
                                                                           .ToListAsync();
                //check overlapin
                if (noteActivities.Count() > 0 )
                {
                    TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                           .Include(n => n.TCMClient)
                                                           .ThenInclude(n => n.Client)
                                                           .FirstOrDefaultAsync(m => m.Id == NoteActivityViewModel.IdTCMNote);

                    NoteActivityViewModel.TCMNote = tcmNote1;
                    NoteActivityViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                    NoteActivityViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == NoteActivityViewModel.IdTCMClient).Id, tcmNote1.DateOfService);
                    if (NoteActivityViewModel.IdTCMDomain != 0)
                    {
                        TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == NoteActivityViewModel.IdTCMDomain);
                        NoteActivityViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                    }

                    ModelState.AddModelError(string.Empty, $"Error. There are activities created in that time interval");
                    ViewData["unitsAvaliable"] = unitsAvaliable;
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditNoteActivity", NoteActivityViewModel) });
                   
                }

                //check overlapin Mental Health
                if (CheckOverlappingMH(NoteActivityViewModel.StartTime, NoteActivityViewModel.EndTime, clientId) == true)
                {
                    TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                           .Include(n => n.TCMClient)
                                                           .ThenInclude(n => n.Client)
                                                           .FirstOrDefaultAsync(m => m.Id == NoteActivityViewModel.IdTCMNote);

                    NoteActivityViewModel.TCMNote = tcmNote1;
                    NoteActivityViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                    NoteActivityViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == NoteActivityViewModel.IdTCMClient).Id, tcmNote1.DateOfService);
                    if (NoteActivityViewModel.IdTCMDomain != 0)
                    {
                        TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == NoteActivityViewModel.IdTCMDomain);
                        NoteActivityViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                    }

                    ModelState.AddModelError(string.Empty, $"Error. There are activities created in that time interval in other service (Mental Health)");
                    ViewData["unitsAvaliable"] = unitsAvaliable;
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditNoteActivity", NoteActivityViewModel) });

                }

                //check que no tenga ese tiempo en supervision si esta habilitado en los setting el chequeo en presencia del TCM
                if (user_logged.Clinic.Setting.TCMSupervisionTimeWithCaseManager == true)
                {
                    TCMSupervisionTimeEntity supervision = _context.TCMSupervisionTimes
                                                                   .FirstOrDefault(n => n.CaseManager.TCMClients.FirstOrDefault(m => m.Id == NoteActivityViewModel.IdTCMClient) != null
                                                                   && n.DateSupervision.Date == NoteActivityViewModel.DateOfServiceNote.Date
                                                                   && ((n.StartTime.TimeOfDay <= NoteActivityViewModel.StartTime.TimeOfDay && n.EndTime.TimeOfDay >= NoteActivityViewModel.StartTime.TimeOfDay)
                                                                    || (n.StartTime.TimeOfDay <= NoteActivityViewModel.EndTime.TimeOfDay && n.EndTime.TimeOfDay >= NoteActivityViewModel.EndTime.TimeOfDay)
                                                                    || (n.StartTime.TimeOfDay <= NoteActivityViewModel.StartTime.TimeOfDay && n.EndTime.TimeOfDay >= NoteActivityViewModel.EndTime.TimeOfDay)
                                                                    || (n.StartTime.TimeOfDay >= NoteActivityViewModel.StartTime.TimeOfDay && n.EndTime.TimeOfDay <= NoteActivityViewModel.EndTime.TimeOfDay)));

                    if (supervision != null)
                    {
                        TCMNoteEntity tcmNote1 = await _context.TCMNote
                                                              .Include(n => n.TCMClient)
                                                              .ThenInclude(n => n.Client)
                                                              .FirstOrDefaultAsync(m => m.Id == NoteActivityViewModel.IdTCMNote);

                        NoteActivityViewModel.TCMNote = tcmNote1;
                        NoteActivityViewModel.SettingList = _combosHelper.GetComboTCMNoteSetting();
                        NoteActivityViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == NoteActivityViewModel.IdTCMClient).Id, tcmNote1.DateOfService);
                        if (NoteActivityViewModel.IdTCMDomain != 0)
                        {
                            TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(d => d.Id == NoteActivityViewModel.IdTCMDomain);
                            NoteActivityViewModel.ActivityList = _combosHelper.GetComboTCMNoteActivity(domain.Code);
                        }

                        ModelState.AddModelError(string.Empty, $"Error. This time is scheduled for supervision.");
                        ViewData["unitsAvaliable"] = unitsAvaliable;
                        return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditNoteActivity", NoteActivityViewModel) });
                    }
       
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
                    ViewData["Id"] = UnitsAvailable(note.DateOfService, note.TCMClient.Id);
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
            NoteActivityViewModel.DomainList = _combosHelper.GetComboServicesUsed(_context.TCMServicePlans.FirstOrDefault(n => n.TcmClient.Id == NoteActivityViewModel.IdTCMClient).Id,tcmNote.DateOfService);
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
                        tcmNote.Sign = true;
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
                                            .ThenInclude(u => u.TCMSupervisor)
                                            .ThenInclude(u => u.Clinic)
                                            .ThenInclude(u => u.Setting)
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
                        tcmNote.ApprovedDate = DateTime.Today;
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

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> DeleteTCMNoteActivity(int id = 0)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMNoteActivityEntity tcmNotesActivity = _context.TCMNoteActivity
                                                             .Include(m => m.TCMNote)
                                                             .ThenInclude(m => m.TCMClient)
                                                             .FirstOrDefault(m => m.Id == id);

            if (tcmNotesActivity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && (user_logged.Clinic.Setting.TCMSupervisorEdit == true)))
            {
                try
                {
                    _context.TCMNoteActivity.Remove(tcmNotesActivity);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                }
                ViewData["available"] = UnitsAvailable(tcmNotesActivity.TCMNote.DateOfService, tcmNotesActivity.TCMNote.TCMClient.Id);
                return RedirectToAction("Edit", new { id = tcmNotesActivity.TCMNote.Id, origin = 2 });
            }
            else
            {
                return RedirectToAction("Home/Error404");
            }
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> DeleteTCMNoteActivityTemp(int id = 0)
        {
            UserEntity user_logged = _context.Users
                                            .Include(u => u.Clinic)
                                            .ThenInclude(u => u.Setting)
                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMNoteActivityTempEntity tcmNotesActivity = _context.TCMNoteActivityTemp
                                                                 .FirstOrDefault(m => m.Id == id);
            if (tcmNotesActivity == null)
            {
                return RedirectToAction("Home/Error404");
            }
            if (User.IsInRole("CaseManager") || (User.IsInRole("TCMSupervisor") && (user_logged.Clinic.Setting.TCMSupervisorEdit == true)))
            {
                try
                {
                    _context.TCMNoteActivityTemp.Remove(tcmNotesActivity);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                }
                ViewData["available"] = UnitsAvailable(tcmNotesActivity.DateOfServiceOfNote, tcmNotesActivity.IdTCMClient);
                return RedirectToAction("Create", new { dateTime = tcmNotesActivity.DateOfServiceOfNote, tcmNotesActivity.IdTCMClient });
            }
            else
            {
                return RedirectToAction("Home/Error404");
            }
        }

        public JsonResult CalcularMinutes(DateTime start, DateTime end)
        {
            int hora = (end - start).Hours;
            int minutes = (end - start).Minutes + (hora * 60);
            return Json(minutes);
        }

        public JsonResult CalcularUnits(DateTime start, int minutes)
        {
            int units = minutes / 15;
            int residuo = minutes % 15;

            if(residuo > 7)
             return Json(units+1);
            else
                return Json(units);
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
        public async Task<IActionResult> NotesStatus(NoteStatus status = NoteStatus.Approved, int withoutReview = 0)
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

            if (User.IsInRole("CaseManager"))
            {
                note = await _context.TCMNote
                                   .Include(w => w.TCMClient)
                                   .ThenInclude(d => d.Client)
                                   .ThenInclude(d => d.Clinic)
                                   .ThenInclude(d => d.Setting)
                                   .Include(w => w.TCMClient)
                                   .ThenInclude(d => d.Casemanager)
                                   .Include(w => w.TCMNoteActivity)
                                   .Include(m => m.TCMMessages.Where(m => m.Notification == false))
                                   .AsSplitQuery()
                                   .Where(w => (w.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                             && w.Status == status
                                             && w.TCMClient.Casemanager.LinkedUser == user_logged.UserName))
                                   .ToListAsync();
            }
            else
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    if (withoutReview == 0)
                    {
                        note = await _context.TCMNote
                                            .Include(w => w.TCMClient)
                                            .ThenInclude(d => d.Client)
                                            .ThenInclude(d => d.Clinic)
                                            .ThenInclude(d => d.Setting)
                                            .Include(w => w.TCMClient)
                                            .ThenInclude(d => d.Casemanager)
                                            .Include(w => w.TCMNoteActivity)
                                            .Include(m => m.TCMMessages.Where(m => m.Notification == false))
                                            .AsSplitQuery()
                                            .Where(w => (w.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                      && w.Status == status
                                                      && w.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName))
                                            .ToListAsync();
                    }
                    else
                    {
                        if (withoutReview == 1)
                        {
                            note = await _context.TCMNote
                                                .Include(w => w.TCMClient)
                                                .ThenInclude(d => d.Client)
                                                .ThenInclude(d => d.Clinic)
                                                .ThenInclude(d => d.Setting)
                                                .Include(w => w.TCMClient)
                                                .ThenInclude(d => d.Casemanager)
                                                .Include(w => w.TCMNoteActivity)
                                                .Include(m => m.TCMMessages.Where(m => m.Notification == false))
                                                .AsSplitQuery()
                                                .Where(w => (w.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                   && w.Status == status
                                                   && w.TCMMessages.Count() == 0
                                                   && w.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName))
                                                .ToListAsync();
                        }
                        else
                        {
                            note = await _context.TCMNote
                                                   .Include(w => w.TCMClient)
                                                   .ThenInclude(d => d.Client)
                                                   .ThenInclude(d => d.Clinic)
                                                   .ThenInclude(d => d.Setting)
                                                   .Include(w => w.TCMClient)
                                                   .ThenInclude(d => d.Casemanager)
                                                   .Include(w => w.TCMNoteActivity)
                                                   .Include(m => m.TCMMessages.Where(m => m.Notification == false))
                                                   .AsSplitQuery()
                                                   .Where(w => (w.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                      && w.Status == status
                                                      && w.TCMMessages.Count() > 0
                                                      && w.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName))
                                                   .ToListAsync();
                        }
                    }
                }
                else
                {
                    note = await _context.TCMNote
                                        .Include(w => w.TCMClient)
                                        .ThenInclude(d => d.Client)
                                        .ThenInclude(d => d.Clinic)
                                        .ThenInclude(d => d.Setting)
                                        .Include(w => w.TCMClient)
                                        .ThenInclude(d => d.Casemanager)
                                        .Include(w => w.TCMNoteActivity)
                                        .Include(m => m.TCMMessages.Where(m => m.Notification == false))
                                        .AsSplitQuery()
                                        .Where(w => (w.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                          && w.Status == status))
                                        .ToListAsync();
                }

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
                                                    .ThenInclude(b => b.Clinic)
                                                    .ThenInclude(b => b.Setting)

                                                    .Include(b => b.TCMClient)
                                                    .ThenInclude(b => b.Casemanager)
                                                    .ThenInclude(b => b.Clinic)
                                                    .ThenInclude(b => b.Setting)

                                                    .Include(b => b.TCMClient)
                                                    .ThenInclude(b => b.Casemanager)
                                                    .ThenInclude(b => b.TCMSupervisor)

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
                        model.ApprovedDate = DateTime.Today;
                        ViewData["origi"] = origi;
                        ViewData["available"] = UnitsAvailable(TcmNote.DateOfService, TcmNote.TCMClient.Id);
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
        public async Task<IActionResult> EditReadOnly(TCMNoteViewModel tcmNotesViewModel, int id, int origi = 0)
        {
            TCMNoteEntity tcmNote = _context.TCMNote
                                           .Include(u => u.TCMNoteActivity)
                                           .ThenInclude(u => u.TCMDomain)
                                           .Include(u => u.TCMClient)
                                           .ThenInclude(u => u.Casemanager)
                                           .ThenInclude(u => u.TCMSupervisor)
                                           .ThenInclude(u => u.Clinic)
                                           .ThenInclude(u => u.Setting)
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
                            if (item.TCMDomain != null)
                            {
                                domain = await _context.TCMDomains.FindAsync(item.TCMDomain.Id);

                                if (domain != null)
                                {
                                    domain.Used = true;
                                    _context.Update(domain);
                                    domain = new TCMDomainEntity();
                                }
                            }
                            
                        }

                        tcmNote.Outcome = tcmNotesViewModel.Outcome;
                        tcmNote.NextStep = tcmNotesViewModel.NextStep;
                        tcmNote.Status = NoteStatus.Approved;
                        tcmNote.ApprovedDate = DateTime.Today;
                        _context.Update(tcmNote);

                        try
                        {
                            await _context.SaveChangesAsync();
                            if (origi == 0)
                            {
                                return RedirectToAction("TCMNotesForCase", new { idTCMClient = tcmNote.TCMClient.Id });
                            }
                            if (origi == 1)
                            {
                                return RedirectToAction("NotesStatus", new { status = NoteStatus.Pending });
                            }
                            if (origi == 2)
                            {
                                return RedirectToAction("NotesWithReview");
                            }
                            if (origi == 3)  ///viene de la pagina Notifications
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

            if (note.TCMClient.Casemanager.Clinic.Name == "COMMUNITY HEALTH THERAPY CENTER")
            {
                Stream stream = _reportHelper.CommunityHTCTCMNoteReportSchema1(note);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            if (note.TCMClient.Casemanager.Clinic.Name == "DREAMS MENTAL HEALTH INC")
            {
                Stream stream = _reportHelper.DreamsMentalHealthTCMNoteReportSchema1(note);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            if (note.TCMClient.Casemanager.Clinic.Name == "SAPPHIRE MENTAL HEALTH CENTER LLC")
            {
                Stream stream = _reportHelper.SapphireMHCTCMNoteReportSchema1(note);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            if (note.TCMClient.Casemanager.Clinic.Name == "ORION MENTAL HEALTH CENTER LLC")
            {
                Stream stream = _reportHelper.OrionMHCTCMNoteReportSchema1(note);
                return File(stream, System.Net.Mime.MediaTypeNames.Application.Pdf);
            }

            if (note.TCMClient.Casemanager.Clinic.Name == "MY FLORIDA CASE MANAGEMENT SERVICES LLC")
            {
                Stream stream = _reportHelper.MyFloridaTCMNoteReportSchema1(note);
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
                                                                    && n.Client.Id == idClient
                                                                    && n.Present == true
                                                                    && ((n.Workday.Service != ServiceType.Individual
                                                                        && (n.Schedule.InitialTime.TimeOfDay <= start.TimeOfDay && n.Schedule.EndTime.TimeOfDay >= start.TimeOfDay
                                                                         || n.Schedule.InitialTime.TimeOfDay <= end.TimeOfDay && n.Schedule.EndTime.TimeOfDay >= end.TimeOfDay
                                                                         || start.TimeOfDay <= n.Schedule.InitialTime.TimeOfDay && end.TimeOfDay >= n.Schedule.InitialTime.TimeOfDay
                                                                         || start.TimeOfDay <= n.Schedule.EndTime.TimeOfDay && end.TimeOfDay >= n.Schedule.EndTime.TimeOfDay
                                                                         || n.Schedule.InitialTime.TimeOfDay <= start.TimeOfDay && n.Schedule.EndTime.TimeOfDay >= end.TimeOfDay
                                                                         || start.TimeOfDay <= n.Schedule.InitialTime.TimeOfDay  && end.TimeOfDay >= n.Schedule.EndTime.TimeOfDay))
                                                                     || (n.Workday.Service == ServiceType.Individual
                                                                        && (n.IndividualNote.SubSchedule.InitialTime.TimeOfDay <= start.TimeOfDay && n.IndividualNote.SubSchedule.EndTime.TimeOfDay >= start.TimeOfDay
                                                                         || n.IndividualNote.SubSchedule.InitialTime.TimeOfDay <= end.TimeOfDay && n.IndividualNote.SubSchedule.EndTime.TimeOfDay >= end.TimeOfDay
                                                                         || start.TimeOfDay <= n.IndividualNote.SubSchedule.InitialTime.TimeOfDay && end.TimeOfDay >= n.IndividualNote.SubSchedule.InitialTime.TimeOfDay
                                                                         || start.TimeOfDay <= n.IndividualNote.SubSchedule.EndTime.TimeOfDay && end.TimeOfDay >= n.IndividualNote.SubSchedule.EndTime.TimeOfDay
                                                                         || n.IndividualNote.SubSchedule.InitialTime.TimeOfDay <= start.TimeOfDay && n.IndividualNote.SubSchedule.EndTime.TimeOfDay >= end.TimeOfDay
                                                                         || start.TimeOfDay <= n.IndividualNote.SubSchedule.InitialTime.TimeOfDay && end.TimeOfDay >= n.IndividualNote.SubSchedule.EndTime.TimeOfDay))
                                                                     )))
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

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult UpdateNote(string dateInterval = "", int idCaseManager = 0, int idTCMClient = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMSupervisorEntity tcmSupervisor = _context.TCMSupervisors.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.TCMClinic || !user_logged.Clinic.Setting.TCMSupervisorEdit)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            IQueryable<TCMNoteEntity> query = null;

            if (dateInterval == string.Empty && idCaseManager == 0 && idTCMClient == 0)
            {
                query = _context.TCMNote
                                .Include(wc => wc.TCMNoteActivity)
                                .Include(wc => wc.TCMClient)
                                .ThenInclude(wc => wc.Casemanager)
                                .ThenInclude(wc => wc.TCMSupervisor)
                                .Include(wc => wc.TCMClient)
                                .ThenInclude(wc => wc.Client)
                                .ThenInclude(wc => wc.Clinic)
                                .ThenInclude(wc => wc.Setting)
                                .Where(wc => wc.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                          && wc.TCMClient.Casemanager.TCMSupervisor.LinkedUser == user_logged.UserName
                                          && wc.Status == NoteStatus.Approved 
                                          && wc.DateOfService >= DateTime.Now.AddMonths(-1));

                return View(new ApprovedTCMNotesClinicViewModel
                {
                    DateIterval = $"{DateTime.Now.AddMonths(-1).ToShortDateString()} - {DateTime.Now.AddDays(6).ToShortDateString()}",
                    IdCaseManager = 0,
                    CaseManagers = _combosHelper.GetComboCaseManagersByTCMSupervisor(user_logged.UserName,0),
                    IdTCMClient = 0,
                    TCMClients = _combosHelper.GetComboTCMClientsByCaseManagerByTCMSupervisor(user_logged.UserName, 0),
                    TCMNotes = query.ToList()
                }
                           );
            }

            query = query = _context.TCMNote
                                    .Include(wc => wc.TCMNoteActivity)
                                    .Include(wc => wc.TCMClient)
                                    .ThenInclude(wc => wc.Casemanager)
                                    .ThenInclude(wc => wc.TCMSupervisor)
                                    .Include(wc => wc.TCMClient)
                                    .ThenInclude(wc => wc.Client)
                                    .ThenInclude(wc => wc.Clinic)
                                    .ThenInclude(wc => wc.Setting)
                                    .Where(wc => wc.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id
                                              && wc.Status == NoteStatus.Approved);

            if (dateInterval != string.Empty)
            {
                string[] date = dateInterval.Split(" - ");
                query = query.Where(wc => (wc.DateOfService >= Convert.ToDateTime(date[0]) && wc.DateOfService <= Convert.ToDateTime(date[1])));
            }

            if (idCaseManager != 0)
                query = query.Where(wc => wc.TCMClient.Casemanager.Id == idCaseManager);

            if (idTCMClient != 0)
                query = query.Where(wc => wc.TCMClient.Id == idTCMClient);

            try
            {
                ApprovedTCMNotesClinicViewModel model = new ApprovedTCMNotesClinicViewModel
                {
                    DateIterval = dateInterval,
                    IdCaseManager = idCaseManager,
                    CaseManagers = _combosHelper.GetComboCaseManagersByTCMSupervisor(user_logged.UserName, 0),
                    IdTCMClient = idTCMClient,
                    TCMClients = _combosHelper.GetComboTCMClientsByCaseManagerByTCMSupervisor(user_logged.UserName,0),
                    TCMNotes = query.ToList()
                };
                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(UpdateNote));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult UpdateNote(ApprovedTCMNotesClinicViewModel model)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(UpdateNote), new { dateInterval = model.DateIterval, idCaseManager = model.IdCaseManager, idTCMClient = model.IdTCMClient });
            }

            return View(model);
        }
        public JsonResult GetNeedIdentified(int idDomain)
        {
            TCMDomainEntity domain = _context.TCMDomains.FirstOrDefault(o => o.Id == idDomain);
            string text = "Select Domain";
            if (domain != null)
            {
                text = domain.NeedsIdentified;
            }
            return Json(text);
        }

        public JsonResult CalcularDateEnd(DateTime start, int minutes)
        {
            DateTime end = new DateTime();
            end = start;
            end = end.AddMinutes(minutes);
            string test = end.ToShortTimeString().ToString();
            return Json(test);
        }

        [Authorize(Roles = "TCMSupervisor")]
        public IActionResult EditNoteActivityReadOnly(int id = 0)
        {
            TCMNoteActivityViewModel model;

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(n => n.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            int units = 0;
            int residuo = 0;

            if (User.IsInRole("TCMSupervisor"))
            {
                if (user_logged.Clinic != null)
                {

                    TCMNoteActivityEntity NoteActivity = _context.TCMNoteActivity
                                                                 .Include(m => m.TCMDomain)
                                                                 .Include(m => m.TCMNote)
                                                                 .ThenInclude(m => m.TCMClient)
                                                                 .ThenInclude(m => m.Client)
                                                                 .ThenInclude(m => m.Clinic)
                                                                 .ThenInclude(m => m.Setting)
                                                                 .Include(m => m.TCMServiceActivity)
                                                                 .FirstOrDefault(m => m.Id == id);
                    if (NoteActivity == null)
                    {
                        return RedirectToAction("NotAuthorized", "Account");
                    }
                    else
                    {
                        if (NoteActivity.TCMDomain == null)
                        {
                            NoteActivity.TCMDomain = new TCMDomainEntity();
                        }
                        if (NoteActivity.TCMServiceActivity == null)
                        {
                            NoteActivity.TCMServiceActivity = new TCMServiceActivityEntity();
                        }

                        model = _converterHelper.ToTCMNoteActivityViewModel(NoteActivity);

                        units = model.Minutes / 15;
                        residuo = model.Minutes % 15;
                        if (residuo > 7)
                            model.Units = units + 1;
                        else
                            model.Units = units;

                        return View(model);
                    }

                }
            }

            model = new TCMNoteActivityViewModel();
            return View(model);
        }

        private int UnitsAvailable(DateTime dateService, int idTCMClient = 0, bool isNew = false)
        {
            UserEntity user_logged = _context.Users
                                                   .Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (isNew == false)
            { 
            
            }
            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(n => n.TCMNote)
                                                .ThenInclude(n => n.TCMNoteActivity)
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.Clients_HealthInsurances)
                                                .FirstOrDefault(n => n.Id == idTCMClient
                                                                  && n.Client.Clients_HealthInsurances
                                                                     .Any( m => m.Active == true 
                                                                             && m.Agency == ServiceAgency.TCM
                                                                             && m.Units > 0
                                                                             && m.ApprovedDate <= dateService
                                                                             && m.ExpiredDate >= dateService));

            if (tcmClient != null)
            {
                Client_HealthInsurance clientHealthInsurance = tcmClient.Client.Clients_HealthInsurances.FirstOrDefault(n => n.Active == true && n.Agency == ServiceAgency.TCM);
                List<TCMNoteEntity> notes = tcmClient.TCMNote.Where(n => n.DateOfService.Date >= clientHealthInsurance.ApprovedDate).ToList();
                int count = 0;
                int factor = 15;
                foreach (var item in notes)
                {

                    int unit = item.TCMNoteActivity.Sum(n => n.Minutes) / factor;
                    double residuo = item.TCMNoteActivity.Sum(n => n.Minutes) % factor;
                    if (residuo >= 8)
                        unit++;
                    count += unit;
                }
                if (isNew == false)
                {
                    return clientHealthInsurance.Units - count;
                }
                else
                {
                    List<TCMNoteActivityTempEntity> notesActivity = _context.TCMNoteActivityTemp.Where(n => n.DateOfServiceOfNote.Date >= clientHealthInsurance.ApprovedDate && n.IdTCMClient == idTCMClient).ToList();

                    int unit_tem = notesActivity.Sum(n => n.Minutes) / factor;
                    double residuoTemp = notesActivity.Sum(n => n.Minutes) % factor;
                    if (residuoTemp >= 8)
                        unit_tem++;

                    return clientHealthInsurance.Units - count - unit_tem;
                }
            }
            else
            {
                return 0;
            }


        }

        private int CalculateUnits(int minute = 0)
        {
            if (minute == 0)
            {
                return minute;
            }
            else
            {
                int factor = 15;
                int unit = minute / factor;
                double residuo = minute % factor;
                if (residuo >= 8)
                {
                    unit++;
                }
                
                return unit;
            }
        }
        [Authorize(Roles = "CaseManager, Manager, TCMSupervisor")]
        public bool CheckTimeRange(DateTime start, DateTime end)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(u => u.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (start.TimeOfDay < user_logged.Clinic.Setting.TCMInitialTime.TimeOfDay || start.TimeOfDay >= user_logged.Clinic.Setting.TCMEndTime.TimeOfDay
                || end.TimeOfDay <= user_logged.Clinic.Setting.TCMInitialTime.TimeOfDay || end.TimeOfDay > user_logged.Clinic.Setting.TCMEndTime.TimeOfDay)
            {
                return true;
            }

            return false;
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor, Manager")]
        public async Task<IActionResult> Delete(int id = 0, int origin = 0)
        {
            TCMNoteEntity tcmNotes = _context.TCMNote
                                             .Include(m => m.TCMNoteActivity)
                                             .Include(m => m.TCMMessages)
                                             .Include(m => m.TCMClient)
                                             .FirstOrDefault(m => m.Id == id);
            if (tcmNotes == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try 
            {
                _context.TCMNoteActivity.RemoveRange(tcmNotes.TCMNoteActivity);
                _context.TCMNote.Remove(tcmNotes);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Edit", new { id = id, origin = origin, error = 1 });
            }

            if (User.IsInRole("CaseManager"))
            {
                if (origin == 0)
                {
                    return RedirectToAction("TCMNotesForCase", new { idTCMClient = tcmNotes.TCMClient.Id });
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
                if (origin == 5)
                {
                    return RedirectToAction("FinishEditingNote", new { id = tcmNotes.Id, origin = 2 });
                }
                if (origin == 6)
                {
                    return RedirectToAction("NotesStatus", new { status = NoteStatus.Pending });
                }
            }
            else
            {
                if (User.IsInRole("TCMSupervisor"))
                {
                    if (origin == 7)
                    {
                        return RedirectToAction("UpdateNote");
                    }
                    else
                    {
                        return RedirectToAction("NotesStatus", new { status = NoteStatus.Pending });
                    }
                }
                if (User.IsInRole("Manager"))
                {
                    if (origin == 8)
                    {
                        return RedirectToAction("TCMCaseHistory", "TCMClients", new { id = tcmNotes.TCMClient.Id });
                    }
                    else
                    {
                        return RedirectToAction("NotesStatus", new { status = NoteStatus.Pending });
                    }
                }
            }

            return RedirectToAction("Index", "TCMBilling");
        }

        [Authorize(Roles = "CaseManager, TCMSupervisor")]
        public async Task<IActionResult> Delete1(int id = 0)
        {
            TCMNoteEntity tcmNotes = _context.TCMNote
                                             .Include(m => m.TCMNoteActivity)
                                             .Include(m => m.TCMMessages)
                                             .FirstOrDefault(m => m.Id == id);
            if (tcmNotes == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMNoteActivity.RemoveRange(tcmNotes.TCMNoteActivity);
                _context.TCMNote.Remove(tcmNotes);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

            }
            return RedirectToAction("NotesStatus", "TCMNotes", new { status = NoteStatus.Pending });
        }

        [Authorize(Roles = "Manager, TCMSupervisor")]
        public async Task<IActionResult> ReturnTo(int? id, int tcmClientId = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMNoteEntity note = await _context.TCMNote.FirstOrDefaultAsync(s => s.Id == id);
            if (note == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                note.Status = NoteStatus.Edition;
                _context.TCMNote.Update(note);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction("TCMCaseHistory", "TCMClients", new { id = tcmClientId });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Search(int idService = 0, int idActivityService = 0)
        {
           TCMServiceNoteViewModel model = new TCMServiceNoteViewModel();

            if (idService == 0 || idActivityService == 0)
            {
                model = new TCMServiceNoteViewModel
                {
                    TCMServices = _combosHelper.GetComboTCMServices(),
                    IdTCMService = idService,
                    TCMServicesActivity = _combosHelper.GetComboTCMActivityByService(idService),
                    TCMNoteActivities = _context.TCMNoteActivity
                                                .Include(n => n.TCMNote)
                                                .ThenInclude(n => n.TCMClient)
                                                .ThenInclude(n => n.Casemanager)
                                                .AsSplitQuery()
                                                .ToList(),
                    IdTCMActivity = idActivityService
                };

                return View(model);
            }
            else
            {
                model = new TCMServiceNoteViewModel
                {
                    TCMServices = _combosHelper.GetComboTCMServices(),
                    IdTCMService = idService,
                    TCMServicesActivity = _combosHelper.GetComboTCMActivityByService(idService),
                    TCMNoteActivities = _context.TCMNoteActivity
                                                .Include(n => n.TCMNote)
                                                .ThenInclude(n => n.TCMClient)
                                                .ThenInclude(n => n.Casemanager)
                                                .AsSplitQuery()
                                                .Where(n => n.TCMServiceActivity.Id == idActivityService)
                                                .ToList(),
                    IdTCMActivity = idActivityService
                };

                return View(model);
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Search(TCMServiceNoteViewModel model)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Search), new { idService = model.IdTCMService, idActivityService = model.IdTCMActivity });
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public JsonResult GetActivityByService(int idService)
        {
            List<TCMServiceActivityEntity> activity = _context.TCMServiceActivity.Where(o => o.TcmService.Id == idService).ToList();
            if (activity.Count == 0)
            {
                activity.Insert(0, new TCMServiceActivityEntity
                {
                    Name = "[Select Activity...]",
                    Id = 0
                });
            }
            return Json(new SelectList(activity, "Id", "Name"));
        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Info(int idTCMClient = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            TCMClientInfoViewModel model = new TCMClientInfoViewModel();
            List<TCMClientInfoViewModel> models = new List<TCMClientInfoViewModel>();

            TCMClientEntity tcmClient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.Clients_Diagnostics)
                                                .ThenInclude(n => n.Diagnostic)
                                                .Include(n => n.TCMIntakeForm)
                                                .Include(n => n.TCMAssessment)
                                                .ThenInclude(n => n.MedicationList)
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.EmergencyContact)
                                                .FirstOrDefault(n => n.Id == idTCMClient);

            string Name = string.Empty;
            string Address = string.Empty;
            string Ec = string.Empty;
            string Dx = string.Empty;
            string PCP = string.Empty;
            string PSY = string.Empty;
            string Medication = string.Empty;
            FacilitatorEntity PSR = _context.Facilitators.Find(tcmClient.Client.IdFacilitatorPSR);
            FacilitatorEntity Group = _context.Facilitators.Find(tcmClient.Client.IdFacilitatorGroup);
            string TherapistPSR = string.Empty;
            string TherapistGroup = string.Empty;
            string TherapistInd = string.Empty;
            string Pharmacy = string.Empty;

            string Therapist = "Admission Date: " + tcmClient.Client.AdmisionDate.ToShortDateString();

            if (PSR != null)
            {
                TherapistPSR = PSR.Name;
            }
            else
            {
                TherapistPSR = "None";
            }
            if (Group != null)
            {
                TherapistGroup = Group.Name;
            }
            else
            {
                TherapistGroup = "None";
            }
            if (tcmClient.Client.IndividualTherapyFacilitator != null)
            {
                TherapistInd = tcmClient.Client.IndividualTherapyFacilitator.Name;
            }
            else
            {
                TherapistInd = "None";
            }

            Name = tcmClient.Client.Name;
            Address = tcmClient.Client.FullAddress + ", " + tcmClient.Client.City + ", " + tcmClient.Client.State + ", " + tcmClient.Client.ZipCode;
            Dx = (tcmClient.Client.Clients_Diagnostics != null) ? tcmClient.Client.Clients_Diagnostics.FirstOrDefault(n => n.Principal == true).Diagnostic.Code + ": " + tcmClient.Client.Clients_Diagnostics.FirstOrDefault(n => n.Principal == true).Diagnostic.Description : "Not have Dx";
            PCP = tcmClient.TCMIntakeForm.PCP_Name + ", Address: " + tcmClient.TCMIntakeForm.PCP_Address + ", City-State-ZipCode: " + tcmClient.TCMIntakeForm.PCP_CityStateZip + ", Phone: " + tcmClient.TCMIntakeForm.PCP_Phone + ", Place: " + tcmClient.TCMIntakeForm.PCP_Place;
            PSY = tcmClient.TCMIntakeForm.Psychiatrist_Name + ", Address: " + tcmClient.TCMIntakeForm.Psychiatrist_Address + ", City-State-ZipCode: " + tcmClient.TCMIntakeForm.Psychiatrist_CityStateZip + ", Phone: " + tcmClient.TCMIntakeForm.PCP_Phone;
            Ec = (tcmClient.Client.EmergencyContact != null) ? tcmClient.Client.EmergencyContact.Name + ", Address: " + tcmClient.Client.EmergencyContact.Address + ", Phone: " + tcmClient.Client.EmergencyContact.Telephone + ", RelationShip: " + tcmClient.Client.RelationShipOfEmergencyContact : "Not have";
            Pharmacy = (tcmClient.TCMAssessment != null) ? tcmClient.TCMAssessment.WhatPharmacy + ", Phone: " + tcmClient.TCMAssessment.PharmacyPhone : "None";

            if (tcmClient != null)
            {
                model.Info = "Client";
                model.Description = Name;
                models.Add(model);
                model = new TCMClientInfoViewModel();

                model.Info = "Address";
                model.Description = Address;
                models.Add(model);
                model = new TCMClientInfoViewModel();

                model.Info = "Ec";
                model.Description = Ec;
                models.Add(model);
                model = new TCMClientInfoViewModel();

                model.Info = "Dx";
                model.Description = Dx;
                models.Add(model);
                model = new TCMClientInfoViewModel();

                model.Info = "PCP";
                model.Description = PCP;
                models.Add(model);
                model = new TCMClientInfoViewModel();

                model.Info = "PSY";
                model.Description = PSY;
                models.Add(model);
                model = new TCMClientInfoViewModel();

                if (tcmClient.TCMAssessment != null)
                {
                    if (tcmClient.TCMAssessment.MedicationList.Count() > 0)
                    {
                        int count = 0;
                        foreach (var item in tcmClient.TCMAssessment.MedicationList)
                        {
                            if (count == 0)
                            {
                                Medication += item.Name + " " + item.Dosage + " " + item.Frequency;
                                count++;

                            }
                            else
                            {
                                if ((count+1) == tcmClient.TCMAssessment.MedicationList.Count() )
                                {
                                    Medication += " and " + item.Name + " " + item.Dosage + " " + item.Frequency;
                                    count++;
                                }
                                else
                                {
                                    Medication += ", " + item.Name + " " + item.Dosage + " " + item.Frequency;
                                    count++;
                                    
                                }
                            }
                            
                        }
                        model.Info = "Medication";
                        model.Description = Medication;
                        models.Add(model);
                        model = new TCMClientInfoViewModel();
                    }
                }

                model.Info = "Therapist";
                model.Description = Therapist + ", Service PSR: " + TherapistPSR + ", Service Group therapist: " + TherapistGroup + ", Service Individual therapist: " + TherapistInd;
                models.Add(model);
                model = new TCMClientInfoViewModel();

                model.Info = "Pharmacy";
                model.Description = Pharmacy;
                models.Add(model);
                model = new TCMClientInfoViewModel();

                return View(models);
            }

            return View(null);
        }

        private bool VerifyAuthorization(int idTCMClient, DateTime dateService)
        {
            TCMClientEntity tcmclient = _context.TCMClient
                                                .Include(n => n.Client)
                                                .ThenInclude(n => n.Clients_HealthInsurances)
                                                .ThenInclude(n => n.HealthInsurance)
                                                .FirstOrDefault(n => n.Id == idTCMClient);
            if (tcmclient != null)
            {
                if (tcmclient.Client.Clients_HealthInsurances.Where(n => n.ApprovedDate <= dateService && n.ExpiredDate >= dateService && n.Agency == ServiceAgency.TCM && n.Active == true).Count() > 0)
                { 
                    return true;
                }    
            }
            return false;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchDomain(string NameDomain = "", int idDomain = 0)
        {
            TCMSearchDomainViewModel model = new TCMSearchDomainViewModel();

            if (NameDomain == string.Empty)
            {
                model = new TCMSearchDomainViewModel
                {
                    TCMDomainNameList = _combosHelper.GetComboTCMDomainName(),
                    
                    TCMDomains = _context.TCMDomains
                                         .Include(n => n.TCMObjetive)
                                         .Include(n => n.TcmServicePlan)
                                         .ThenInclude(n => n.TcmClient)
                                         .ThenInclude(n => n.Casemanager)
                                         .AsSplitQuery()
                                         .ToList(),
                    IdTCMDomain = idDomain
                };

                return View(model);
            }
            else
            {
                model = new TCMSearchDomainViewModel
                {
                    TCMDomainNameList = _combosHelper.GetComboTCMDomainName(),

                    TCMDomains = _context.TCMDomains
                                         .Include(n => n.TCMObjetive)
                                         .Include(n => n.TcmServicePlan)
                                         .ThenInclude(n => n.TcmClient)
                                         .ThenInclude(n => n.Casemanager)
                                         .AsSplitQuery()
                                         .Where(n => n.Name == NameDomain)
                                         .ToList(),
                    IdTCMDomain = idDomain
                };

                return View(model);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult SearchDomain(TCMSearchDomainViewModel model)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            string nameTCMDomain = string.Empty;
            nameTCMDomain = (model.IdTCMDomain == 0) ? "Mental Health Behavioral Substance Abuse" : (model.IdTCMDomain == 1) ? "Physical Health Medical/Dental" : (model.IdTCMDomain == 2) ? "Vocational Emplymen job Training" : (model.IdTCMDomain == 3) ? "School Education" : (model.IdTCMDomain == 4) ? "Environmental Recreational Social Support" :
                            (model.IdTCMDomain == 5) ? "Activities of Daily Living" : (model.IdTCMDomain == 6) ? "Housing Shelter" : (model.IdTCMDomain == 7) ? "Economic Financial" : (model.IdTCMDomain == 8) ? "Basic Needs(food, clothing, furniture,etc.)" : (model.IdTCMDomain == 9) ? "Transportation" :
                            (model.IdTCMDomain == 10) ? "Legal Immigration" : "Other(specify)";

            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(SearchDomain), new { NameDomain = nameTCMDomain, idDomain = model.IdTCMDomain });
            }

            return View(model);
        }

    }
}
