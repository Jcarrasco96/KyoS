using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KyoS.Common.Helpers;
using AspNetCore.Reporting;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace KyoS.Web.Controllers
{
    public class CitesController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
       
        public IConfiguration Configuration { get; }

        public CitesController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
        }

        [Authorize(Roles = "Manager, Frontdesk")]
        public async Task<IActionResult> Index(int idError = 0)
        {
            UserEntity user_logged = await _context.Users
                                                  .Include(u => u.Clinic)
                                                  .ThenInclude(c => c.Setting)
                                                  .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }
            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk"))
            {
                return View(await _context.Cites
                                          .Include(c => c.Clinic)
                                          .Include(c => c.Client)
                                          .Include(c => c.Facilitator)
                                          .Include(c => c.Schedule)
                                          .Where(c => c.Clinic.Id == user_logged.Clinic.Id)
                                          .OrderBy(c => c.Date).ToListAsync());
            }
           

            return RedirectToAction("NotAuthorized", "Account");
        }

        [Authorize(Roles = "Frontdesk")]
        public IActionResult CreateModal(int id = 0)
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

            CiteViewModel model = new CiteViewModel();

            if (User.IsInRole("Manager") || User.IsInRole("Frontdesk"))
            {
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
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
                    ClientEntity client = new ClientEntity();
                    client.Name = "-";

                    model = new CiteViewModel
                    {
                        IdClinic = clinic.Id,
                        Clinics = list,                        
                        IdStatus = 0,
                        StatusList = _combosHelper.GetComboSiteStatus(),
                        IdClient = 0,
                        ClientsList = _combosHelper.GetComboClientByIndfacilitator(null, user_logged.Clinic.Id),
                        IdFacilitator = 0,
                        FacilitatorsList = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id, false),
                        IdSchedule = 0,
                        SchedulesList = _combosHelper.GetComboSchedulesByClinicForCites(user_logged.Clinic.Id, ServiceType.Individual, 0),
                        Worday_CLient = null,
                        Copay = 0,
                        Date = DateTime.Today,
                        EventNote = string.Empty,
                        PatientNote = string.Empty,
                        Service = "Therapy Private",
                        Client = client
                        

                    };
                    return View(model);
                }
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Frontdesk")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateModal(CiteViewModel citeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if (ModelState.IsValid)
            {
                CiteEntity cite = await _context.Cites
                                                .FirstOrDefaultAsync(f => (f.Clinic.Id == citeViewModel.IdClinic
                                                                       && f.Date == citeViewModel.Date
                                                                       && ((f.Client.Id == citeViewModel.IdClient)
                                                                       || ( f.Schedule.Id == citeViewModel.IdSchedule
                                                                         && f.Facilitator.Id == citeViewModel.IdFacilitator))));

                if (cite == null && VerifyNotesAtSameTime(citeViewModel.IdClient, citeViewModel.IdSchedule, citeViewModel.Date) == false
                            && VerifyFreeTimeOfFacilitator(citeViewModel.IdFacilitator,citeViewModel.Date,citeViewModel.IdSchedule) == false)
                {
                   
                    cite = await _converterHelper.ToCiteEntity(citeViewModel, true, user_logged.UserName);

                    _context.Add(cite);
                    try
                    {
                        await _context.SaveChangesAsync();

                        List<CiteEntity> cites_List = await _context.Cites
                                                                    .Include(c => c.Clinic)
                                                                    .Include(c => c.Client)
                                                                    .Include(c => c.Facilitator)
                                                                    .Include(c => c.Schedule)
                                                                    .Where(c => c.Clinic.Id == user_logged.Clinic.Id)
                                                                    .OrderBy(c => c.Date)
                                                                    .ToListAsync();

                        return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewCites", cites_List) });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the facilitator: {citeViewModel.Id}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else
                {
                    ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
                    List<SelectListItem> list = new List<SelectListItem>();
                    list.Insert(0, new SelectListItem
                    {
                        Text = clinic.Name,
                        Value = $"{clinic.Id}"
                    });

                    citeViewModel.IdClinic = clinic.Id;
                    citeViewModel.Clinics = list;
                    citeViewModel.StatusList = _combosHelper.GetComboSiteStatus();
                    citeViewModel.ClientsList = _combosHelper.GetComboClientByIndfacilitator(null, user_logged.Clinic.Id);
                    citeViewModel.FacilitatorsList = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id, false);
                    citeViewModel.SchedulesList = _combosHelper.GetComboSchedulesByClinicForCites(user_logged.Clinic.Id, ServiceType.Individual, citeViewModel.IdFacilitator);

                    ViewBag.Creado = "E";
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", citeViewModel) });
                }
            }
            else
            {
                ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = clinic.Name,
                    Value = $"{clinic.Id}"
                });

                citeViewModel.IdClinic = clinic.Id;
                citeViewModel.Clinics = list;
                citeViewModel.StatusList = _combosHelper.GetComboSiteStatus();
                citeViewModel.ClientsList = _combosHelper.GetComboClientByIndfacilitator(null, user_logged.Clinic.Id);
                citeViewModel.FacilitatorsList = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id, false);
                citeViewModel.SchedulesList = _combosHelper.GetComboSchedulesByClinicForCites(user_logged.Clinic.Id, ServiceType.Individual, citeViewModel.IdFacilitator);

            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", citeViewModel) });
        }

        [Authorize(Roles = "Frontdesk")]
        public JsonResult GetClients(int idFacilitator)
        {
            List<ClientEntity> clients = _context.Clients.Where(n => n.IndividualTherapyFacilitator.Id == idFacilitator).ToList();
          
            if (clients.Count == 0)
            {
                clients.Insert(0, new ClientEntity
                {
                    Name = "[First select Facilitator...]",
                    Id = 0
                });
            }
            return Json(new SelectList(clients, "Id", "Name"));
        }

        [Authorize(Roles = "Frontdesk")]
        public async Task<IActionResult> EditModal(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            CiteEntity citeEntity = await _context.Cites
                                                  .Include(c => c.Client)
                                                  .Include(f => f.Facilitator)
                                                  .Include(sc => sc.Schedule)

                                                  .FirstOrDefaultAsync(c => c.Id == id);
            if (citeEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            CiteViewModel citeViewModel = _converterHelper.ToCiteViewModel(citeEntity, user_logged.Clinic.Id);
           
            List<SelectListItem> list = new List<SelectListItem>();
            list.Insert(0, new SelectListItem
            {
                Text = user_logged.Clinic.Name,
                Value = $"{user_logged.Clinic.Id}"
            });
            citeViewModel.Clinics = list;
            return View(citeViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Frontdesk")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditModal(int id, CiteViewModel citeViewModel)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (id != citeViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
               

                CiteEntity citeEntity = await _converterHelper.ToCiteEntity(citeViewModel, false, user_logged.Id);
                _context.Update(citeEntity);
                try
                {
                    await _context.SaveChangesAsync();

                    List<CiteEntity> cite_List = await _context.Cites
                                                               .Include(c => c.Clinic)
                                                               .Include(c => c.Client)
                                                               .Include(c => c.Facilitator)
                                                               .Include(c => c.Schedule)
                                                               .Where(c => c.Clinic.Id == user_logged.Clinic.Id)
                                                               .OrderBy(c => c.Date).ToListAsync();

                    return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_ViewCites", cite_List) });
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the diagnostic: {citeEntity.Id}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            else
            {
                ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);
                List<SelectListItem> list = new List<SelectListItem>();
                list.Insert(0, new SelectListItem
                {
                    Text = clinic.Name,
                    Value = $"{clinic.Id}"
                });

                citeViewModel.IdClinic = clinic.Id;
                citeViewModel.Clinics = list;
                citeViewModel.StatusList = _combosHelper.GetComboSiteStatus();
                citeViewModel.ClientsList = _combosHelper.GetComboClientByIndfacilitator(null, user_logged.Clinic.Id);
                citeViewModel.FacilitatorsList = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id, true);
                citeViewModel.SchedulesList = _combosHelper.GetComboSchedulesByClinicForCites(user_logged.Clinic.Id, ServiceType.Individual, citeViewModel.IdFacilitator);
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "EditModal", citeViewModel) });
        }

        [Authorize(Roles = "Frontdesk")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            CiteEntity citeEntity = await _context.Cites.FirstOrDefaultAsync(c => c.Id == id);
            if (citeEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Cites.Remove(citeEntity);
                await _context.SaveChangesAsync();
            }
            catch (System.Exception)
            {

                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }

        #region Utils Functions     
        private bool VerifyFreeTimeOfFacilitator(int idFacilitator, DateTime date, int idSchedule)
        {
            FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.Id == idFacilitator);
            List<Workday_Client> workday_client = _context.Workdays_Clients
                                                          .Include(n => n.Workday)
                                                          .Include(n => n.Note)
                                                          .Include(n => n.NoteP)
                                                          .Include(n => n.GroupNote)
                                                          .Include(n => n.GroupNote2)
                                                          .Include(n => n.IndividualNote)
                                                          .Include(n => n.Schedule)
                                                          .ThenInclude(n => n.SubSchedules)
                                                          .Where(wc => (wc.Facilitator.Id == idFacilitator
                                                                     && wc.Workday.Date == date))
                                                          .ToList();

            ScheduleEntity schedule = _context.Schedule.FirstOrDefault(n => n.Id == idSchedule);
            foreach (var item in workday_client)
            {
                if (item.Workday.Service == ServiceType.Individual)
                {
                    if (item.IndividualNote != null)
                    {
                        if (item.IndividualNote.SubSchedule.InitialTime.TimeOfDay <= schedule.InitialTime.TimeOfDay && item.IndividualNote.SubSchedule.Schedule.EndTime.TimeOfDay >= schedule.InitialTime.TimeOfDay
                            || item.IndividualNote.SubSchedule.Schedule.InitialTime.TimeOfDay <= schedule.EndTime.TimeOfDay && item.IndividualNote.SubSchedule.Schedule.EndTime.TimeOfDay >= schedule.EndTime.TimeOfDay
                            || item.IndividualNote.SubSchedule.Schedule.InitialTime.TimeOfDay >= schedule.InitialTime.TimeOfDay && item.IndividualNote.SubSchedule.Schedule.InitialTime.TimeOfDay <= schedule.EndTime.TimeOfDay
                            || item.IndividualNote.SubSchedule.Schedule.EndTime.TimeOfDay >= schedule.InitialTime.TimeOfDay && item.IndividualNote.SubSchedule.Schedule.EndTime.TimeOfDay <= schedule.EndTime.TimeOfDay)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (item.Workday.Service == ServiceType.PSR || item.Workday.Service == ServiceType.Group)
                    {
                        if (item.Schedule.InitialTime.TimeOfDay <= schedule.InitialTime.TimeOfDay && item.Schedule.EndTime.TimeOfDay >= schedule.InitialTime.TimeOfDay
                          || item.Schedule.InitialTime.TimeOfDay <= schedule.EndTime.TimeOfDay && item.Schedule.EndTime.TimeOfDay >= schedule.EndTime.TimeOfDay
                          || item.Schedule.InitialTime.TimeOfDay >= schedule.InitialTime.TimeOfDay && item.Schedule.InitialTime.TimeOfDay <= schedule.EndTime.TimeOfDay
                          || item.Schedule.EndTime.TimeOfDay >= schedule.InitialTime.TimeOfDay && item.Schedule.EndTime.TimeOfDay <= schedule.EndTime.TimeOfDay)
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }

        [Authorize(Roles = "Manager")]
        private bool VerifyNotesAtSameTime(int idClient, int idSchedule, DateTime date)
        {
            //PSR notes
            ClientEntity client = _context.Clients.FirstOrDefault(n => n.Id == idClient);
            
            List<Workday_Client> workday_client = _context.Workdays_Clients
                                                   .Include(n => n.Workday)
                                                   .Include(n => n.Note)
                                                   .Include(n => n.NoteP)
                                                   .Include(n => n.GroupNote)
                                                   .Include(n => n.GroupNote2)
                                                   .Include(n => n.IndividualNote)
                                                   .Include(n => n.Schedule)
                                                   .ThenInclude(n => n.SubSchedules)
                                                   .Where(wc => (wc.Client.Id == idClient 
                                                              && wc.Workday.Date == date))
                                                   .ToList();
            ScheduleEntity schedule = _context.Schedule.FirstOrDefault(n => n.Id == idSchedule);
            foreach (var item in workday_client)
            {
                if (item.Workday.Service == ServiceType.Individual)
                {
                    return true;
                }
                else
                {
                    if (item.Workday.Service == ServiceType.PSR || item.Workday.Service == ServiceType.Group)
                    {
                        if (item.Schedule.InitialTime.TimeOfDay <= schedule.InitialTime.TimeOfDay && item.Schedule.EndTime.TimeOfDay >= schedule.InitialTime.TimeOfDay
                          || item.Schedule.InitialTime.TimeOfDay <= schedule.EndTime.TimeOfDay && item.Schedule.EndTime.TimeOfDay >= schedule.EndTime.TimeOfDay
                          || item.Schedule.InitialTime.TimeOfDay >= schedule.InitialTime.TimeOfDay && item.Schedule.InitialTime.TimeOfDay <= schedule.EndTime.TimeOfDay
                          || item.Schedule.EndTime.TimeOfDay >= schedule.InitialTime.TimeOfDay && item.Schedule.EndTime.TimeOfDay <= schedule.EndTime.TimeOfDay)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion

    }
}
