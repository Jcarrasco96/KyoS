﻿using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Helpers;

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

        [Authorize(Roles = "Frontdesk")]
        public IActionResult CreateModal(string appointmentDate, int id = 0, int facilitatorId = 0)
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

            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic != null)
            {
                DateTime realDate = (appointmentDate != null) ? Convert.ToDateTime(appointmentDate) : DateTime.Now;
                CiteViewModel model = new CiteViewModel
                {
                    IdFacilitator = (facilitatorId != 0) ? facilitatorId : 0,
                    StatusList = _combosHelper.GetComboSiteStatus(),
                    ClientsList = _combosHelper.GetComboClientByIndfacilitator(facilitatorId),
                    FacilitatorsList = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id, false),
                    SubSchedulesList = _combosHelper.GetComboSchedulesByClinicForCites(user_logged.Clinic.Id, ServiceType.Individual, (facilitatorId != 0) ? facilitatorId : 0, realDate),
                    Copay = 0,
                    DateCite = realDate,
                    Service = "Therapy Private"
                };
                return View(model);
            }

            return View(null);
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
                                                .FirstOrDefaultAsync(f => (f.DateCite == citeViewModel.DateCite
                                                                       && ((f.Client.Id == citeViewModel.IdClient)
                                                                       || (f.SubSchedule.Id == citeViewModel.IdSubSchedule && f.Facilitator.Id == citeViewModel.IdFacilitator))));

                if (cite == null && VerifyNotesAtSameTime(citeViewModel.IdClient, citeViewModel.IdSubSchedule, citeViewModel.DateCite) == false
                            && VerifyFreeTimeOfFacilitator(citeViewModel.IdFacilitator, citeViewModel.DateCite, citeViewModel.IdSubSchedule) == false)
                {

                    cite = await _converterHelper.ToCiteEntity(citeViewModel, true, user_logged.UserName);

                    _context.Add(cite);
                    try
                    {
                        await _context.SaveChangesAsync();

                        return Json(new { isValid = true, html = citeViewModel.IdFacilitator.ToString() });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the appoitment: {citeViewModel.Id}");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                        }
                    }
                }
                else
                {
                    citeViewModel.StatusList = _combosHelper.GetComboSiteStatus();
                    citeViewModel.ClientsList = _combosHelper.GetComboClientByIndfacilitator(citeViewModel.IdFacilitator);
                    citeViewModel.FacilitatorsList = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id, false);
                    citeViewModel.SubSchedulesList = _combosHelper.GetComboSchedulesByClinicForCites(user_logged.Clinic.Id, ServiceType.Individual, citeViewModel.IdFacilitator, citeViewModel.DateCite);

                    ViewBag.Creado = "E";
                    return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", citeViewModel) });
                }
            }
            else
            {
                citeViewModel.DateCite = citeViewModel.DateCite;
                citeViewModel.StatusList = _combosHelper.GetComboSiteStatus();
                citeViewModel.ClientsList = _combosHelper.GetComboClientByIndfacilitator(citeViewModel.IdFacilitator);
                citeViewModel.FacilitatorsList = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id, false);
                citeViewModel.SubSchedulesList = _combosHelper.GetComboSchedulesByClinicForCites(user_logged.Clinic.Id, ServiceType.Individual, citeViewModel.IdFacilitator, citeViewModel.DateCite);
            }

            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "CreateModal", citeViewModel) });
        }

        [Authorize(Roles = "Frontdesk")]
        public JsonResult GetClients(int Facilitator)
        {
            List<SelectListItem> ClientsList = _combosHelper.GetComboClientByIndfacilitator(Facilitator).ToList();

            return Json(new SelectList(ClientsList, "Value", "Text"));
        }

        [Authorize(Roles = "Frontdesk")]
        public JsonResult GetSubSchedule(int Facilitator, DateTime DateOfAppoitment)
        {
            UserEntity user_logged = _context.Users
                                            .Include(u => u.Clinic)
                                            .FirstOrDefault(u => u.UserName == User.Identity.Name);

            List<SelectListItem> subschedules = _combosHelper.GetComboSchedulesByClinicForCites(user_logged.Clinic.Id, ServiceType.Individual, Facilitator, DateOfAppoitment)
                                                             .ToList();

            return Json(new SelectList(subschedules, "Value", "Text"));
        }

        [Authorize(Roles = "Frontdesk")]
        public async Task<JsonResult> GetDateOfBirth(int ClientId)
        {
            ClientEntity client = await _context.Clients
                                                .FirstOrDefaultAsync(c => c.Id == ClientId);

            return Json((client != null) ? client.DateOfBirth.ToShortDateString() : string.Empty);
        }

        [Authorize(Roles = "Frontdesk")]
        public async Task<JsonResult> GetTelephone(int ClientId)
        {
            ClientEntity client = await _context.Clients
                                                .FirstOrDefaultAsync(c => c.Id == ClientId);

            return Json((client != null) ? client.Telephone : string.Empty);
        }

        [Authorize(Roles = "Frontdesk")]
        public async Task<JsonResult> GetFullAddress(int ClientId)
        {
            ClientEntity client = await _context.Clients
                                                .FirstOrDefaultAsync(c => c.Id == ClientId);

            return Json((client != null) ? client.FullAddress : string.Empty);
        }        

        [Authorize(Roles = "Frontdesk")]
        public async Task<IActionResult> Edit(int? id, int idError = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
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
                                                  .Include(sc => sc.SubSchedule)

                                                  .FirstOrDefaultAsync(c => c.Id == id);
            if (citeEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            CiteViewModel citeViewModel = _converterHelper.ToCiteViewModel(citeEntity, user_logged.Clinic.Id);

            return View(citeViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Frontdesk")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CiteViewModel citeViewModel)
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
                    return Json(new { isValid = true, html = citeViewModel.IdFacilitator.ToString() });
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
                citeViewModel.StatusList = _combosHelper.GetComboSiteStatus();
                citeViewModel.ClientsList = _combosHelper.GetComboClientByIndfacilitator(citeViewModel.IdFacilitator);
                citeViewModel.FacilitatorsList = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id, true);
                citeViewModel.SubSchedulesList = _combosHelper.GetComboSchedulesByClinicForCites(user_logged.Clinic.Id, ServiceType.Individual, citeViewModel.IdFacilitator, citeViewModel.DateCite);
            }
            return Json(new { isValid = false, html = _renderHelper.RenderRazorViewToString(this, "Edit", citeViewModel) });
        }        

        #region Utils Functions     

        [Authorize(Roles = "Manager, Frontdesk")]
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
                                                                     && wc.Workday.Date.Date == date.Date))
                                                          .ToList();

            SubScheduleEntity subSchedule = _context.SubSchedule.FirstOrDefault(n => n.Id == idSchedule);
            foreach (var item in workday_client)
            {
                if (item.Workday.Service == ServiceType.Individual)
                {
                    if (item.IndividualNote != null)
                    {
                        if ((item.IndividualNote.SubSchedule.InitialTime.TimeOfDay <= subSchedule.InitialTime.TimeOfDay && item.IndividualNote.SubSchedule.EndTime.TimeOfDay >= subSchedule.InitialTime.TimeOfDay)
                            || (item.IndividualNote.SubSchedule.InitialTime.TimeOfDay <= subSchedule.EndTime.TimeOfDay && item.IndividualNote.SubSchedule.EndTime.TimeOfDay >= subSchedule.EndTime.TimeOfDay)
                            || (item.IndividualNote.SubSchedule.InitialTime.TimeOfDay >= subSchedule.InitialTime.TimeOfDay && item.IndividualNote.SubSchedule.InitialTime.TimeOfDay <= subSchedule.EndTime.TimeOfDay)
                            || (item.IndividualNote.SubSchedule.EndTime.TimeOfDay >= subSchedule.InitialTime.TimeOfDay && item.IndividualNote.SubSchedule.EndTime.TimeOfDay <= subSchedule.EndTime.TimeOfDay))
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (item.Workday.Service == ServiceType.PSR || item.Workday.Service == ServiceType.Group)
                    {
                        if ((item.Schedule.InitialTime.TimeOfDay <= subSchedule.InitialTime.TimeOfDay && item.Schedule.EndTime.TimeOfDay >= subSchedule.InitialTime.TimeOfDay)
                          || (item.Schedule.InitialTime.TimeOfDay <= subSchedule.EndTime.TimeOfDay && item.Schedule.EndTime.TimeOfDay >= subSchedule.EndTime.TimeOfDay)
                          || (item.Schedule.InitialTime.TimeOfDay >= subSchedule.InitialTime.TimeOfDay && item.Schedule.InitialTime.TimeOfDay <= subSchedule.EndTime.TimeOfDay)
                          || (item.Schedule.EndTime.TimeOfDay >= subSchedule.InitialTime.TimeOfDay && item.Schedule.EndTime.TimeOfDay <= subSchedule.EndTime.TimeOfDay))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        [Authorize(Roles = "Manager, Frontdesk")]
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
            SubScheduleEntity subSchedule = _context.SubSchedule.FirstOrDefault(n => n.Id == idSchedule);
            foreach (var item in workday_client)
            {
                if (item.Workday.Service == ServiceType.Individual && item.Client != null)
                {
                    return true;
                }
                else
                {
                    if (item.Workday.Service == ServiceType.PSR || item.Workday.Service == ServiceType.Group)
                    {
                        if ((item.Schedule.InitialTime.TimeOfDay <= subSchedule.InitialTime.TimeOfDay && item.Schedule.EndTime.TimeOfDay >= subSchedule.InitialTime.TimeOfDay)
                          || (item.Schedule.InitialTime.TimeOfDay <= subSchedule.EndTime.TimeOfDay && item.Schedule.EndTime.TimeOfDay >= subSchedule.EndTime.TimeOfDay)
                          || (item.Schedule.InitialTime.TimeOfDay >= subSchedule.InitialTime.TimeOfDay && item.Schedule.InitialTime.TimeOfDay <= subSchedule.EndTime.TimeOfDay)
                          || (item.Schedule.EndTime.TimeOfDay >= subSchedule.InitialTime.TimeOfDay && item.Schedule.EndTime.TimeOfDay <= subSchedule.EndTime.TimeOfDay))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion


        [Authorize(Roles = "Manager, Supervisor, Facilitator, Frontdesk")]
        public IActionResult AuditCites(DateTime date)
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic || !user_logged.Clinic.Setting.MHProblems)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            List<AuditCites> audit_List = new List<AuditCites>();
            AuditCites audit = new AuditCites();
            List<CiteEntity> cites_List = new List<CiteEntity>();

            if (User.IsInRole("Facilitator"))
            {
                cites_List = _context.Cites
                                     .Include(m => m.SubSchedule)
                                     .Include(m => m.Client)
                                     .Where(n => (n.Client.Clinic.Id == user_logged.Clinic.Id
                                              && (n.DateCite.Date < date.Date)
                                              && (n.Facilitator.LinkedUser == user_logged.UserName)
                                              && (n.Status == CiteStatus.S
                                               || (n.Status == CiteStatus.C)
                                               || (n.Status == CiteStatus.R)
                                               || (n.Status == CiteStatus.AR)
                                               || (n.Status == CiteStatus.CO))))
                                     .ToList();

            }
            else
            {
                cites_List = _context.Cites
                                     .Include(m => m.SubSchedule)
                                     .Include(m => m.Client)
                                     .Where(n => (n.Client.Clinic.Id == user_logged.Clinic.Id
                                              && (n.DateCite.Date < date.Date)
                                              && (n.Status == CiteStatus.S
                                               || (n.Status == CiteStatus.C)
                                               || (n.Status == CiteStatus.R)
                                               || (n.Status == CiteStatus.AR)
                                               || (n.Status == CiteStatus.CO))))
                                     .ToList();

            }

            foreach (var item in cites_List)
            {
                audit.ClientName = item.Client.Name;
                audit.Date = item.DateCite;
                audit.Status = item.Status.ToString();
                audit.Description = "Check this appointment";
                audit.Active = 1;

                audit_List.Add(audit);
                audit = new AuditCites();
            }

            return View(audit_List);
        }

    }
}
