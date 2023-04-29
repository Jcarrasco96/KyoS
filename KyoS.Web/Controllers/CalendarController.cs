﻿using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    public class CalendarController : Controller
    {
        private readonly DataContext _context;
        private readonly ICombosHelper _combosHelper;

        public IConfiguration Configuration { get; }

        public CalendarController(DataContext context, ICombosHelper combosHelper, IConfiguration configuration)
        {
            _context = context;
            _combosHelper = combosHelper;
            Configuration = configuration;
        }

        [Authorize(Roles = "Manager")]
        public IActionResult Index()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                                .ThenInclude(c => c.Setting)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            CalendarCMH model = new CalendarCMH
            {
                IdClient = 0,
                Clients = _combosHelper.GetComboClientsByClinic(user_logged.Clinic.Id, false),
                IdFacilitator = 0,
                Facilitators = _combosHelper.GetComboFacilitatorsByClinic(user_logged.Clinic.Id, false)
            };

            return View(model);
        }

        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Events(string start, string end, int idClient)
        {
            if (idClient != 0)
            {
                DateTime initDate = Convert.ToDateTime(start);
                DateTime finalDate = Convert.ToDateTime(end);

                Task<List<object>> notesTask = NotesByClient(idClient, initDate, finalDate);
                Task<List<object>> mtpsTask = MTPsByClient(idClient, initDate, finalDate);
                Task<List<object>> biosTask = BIOsByClient(idClient, initDate, finalDate);

                await Task.WhenAll(notesTask, mtpsTask, biosTask);
                
                var notes = await notesTask;
                var mtps = await mtpsTask;
                var bios = await biosTask;

                List<object> events = new List<object>();
                events.AddRange(notes);
                events.AddRange(mtps);
                events.AddRange(bios);

                return new JsonResult(events);
            }
            else
            { 
                var events = new List<Workday_Client>();
                return new JsonResult(events);
            }
        }

        #region Utils
        private async Task<List<object>> NotesByClient(int idClient, DateTime initDate, DateTime finalDate)
        {            
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<Workday_Client> listWorkdayClient;

            using (DataContext db = new DataContext(options))
            {
                listWorkdayClient = await db.Workdays_Clients

                                            .Include(wc => wc.Schedule)
                                            .Include(wc => wc.Workday)
                                                  
                                            .Include(wc => wc.IndividualNote)
                                                .ThenInclude(i => i.SubSchedule)

                                            .Where(wc => (wc.Workday.Date >= initDate && wc.Workday.Date <= finalDate && wc.Present == true &&
                                                    wc.Client.Id == idClient))
                                            .ToListAsync();                
            }

            return listWorkdayClient.Select(wc => new
                                    {
                                        title = (wc.Workday.Service == ServiceType.PSR) ? "PSR Therapy" :
                                                    (wc.Workday.Service == ServiceType.Group) ? "Group Therapy" :
                                                        (wc.Workday.Service == ServiceType.Individual) ? "Individual Therapy" : string.Empty,
                                        start = (wc.Workday.Service == ServiceType.Individual) ?
                                                new DateTime(wc.Workday.Date.Year, wc.Workday.Date.Month, wc.Workday.Date.Day,
                                                                (wc.IndividualNote != null && wc.IndividualNote.SubSchedule != null) ? wc.IndividualNote.SubSchedule.InitialTime.Hour : 0, (wc.IndividualNote != null && wc.IndividualNote.SubSchedule != null) ? wc.IndividualNote.SubSchedule.InitialTime.Hour : 0, 0)
                                                                .ToString("yyyy-MM-ddTHH:mm:ssK")
                                                :
                                                new DateTime(wc.Workday.Date.Year, wc.Workday.Date.Month, wc.Workday.Date.Day,
                                                                (wc.Schedule) != null ? wc.Schedule.InitialTime.Hour : 0, (wc.Schedule) != null ? wc.Schedule.InitialTime.Minute : 0, 0)
                                                                .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                        end = (wc.Workday.Service == ServiceType.Individual) ?
                                                new DateTime(wc.Workday.Date.Year, wc.Workday.Date.Month, wc.Workday.Date.Day,
                                                                (wc.IndividualNote != null && wc.IndividualNote.SubSchedule != null) ? wc.IndividualNote.SubSchedule.EndTime.Hour : 0, (wc.IndividualNote != null && wc.IndividualNote.SubSchedule != null) ? wc.IndividualNote.SubSchedule.EndTime.Hour : 0, 0)
                                                                .ToString("yyyy-MM-ddTHH:mm:ssK")
                                                :
                                                new DateTime(wc.Workday.Date.Year, wc.Workday.Date.Month, wc.Workday.Date.Day,
                                                                (wc.Schedule) != null ? wc.Schedule.EndTime.Hour : 0, (wc.Schedule) != null ? wc.Schedule.EndTime.Minute : 0, 0)
                                                                .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                        backgroundColor = (wc.Workday.Service == ServiceType.PSR) ? "#fcf8e3" :
                                                            (wc.Workday.Service == ServiceType.Group) ? "#d9edf7" :
                                                                (wc.Workday.Service == ServiceType.Individual) ? "#dff0d8" : "#dff0d8",
                                        textColor = (wc.Workday.Service == ServiceType.PSR) ? "#9e7d67" :
                                                        (wc.Workday.Service == ServiceType.Group) ? "#487c93" :
                                                            (wc.Workday.Service == ServiceType.Individual) ? "#417c49" : "#417c49",
                                        borderColor = (wc.Workday.Service == ServiceType.PSR) ? "#9e7d67" :
                                                        (wc.Workday.Service == ServiceType.Group) ? "#487c93" :
                                                            (wc.Workday.Service == ServiceType.Individual) ? "#417c49" : "#417c49"
                                    })
                                    .ToList<object>();            
        }

        private async Task<List<object>> MTPsByClient(int idClient, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<MTPEntity> mtpEntity;

            using (DataContext db = new DataContext(options))
            {
                mtpEntity = await db.MTPs                                                 

                                    .Where(m => (m.CreatedOn >= initDate && m.CreatedOn <= finalDate && m.Client.Id == idClient))
                                    .ToListAsync();
            }

            return mtpEntity.Select(m => new
                            {
                                title = "MTP Document",
                                start = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                    m.StartTime.Hour, m.StartTime.Minute, 0)
                                                    .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                end = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                    m.EndTime.Hour, m.EndTime.Minute, 0)
                                                    .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                backgroundColor = "#dff0d8",
                                textColor = "#417c49",
                                borderColor = "#417c49"
                            })
                            .ToList<object>();
        }

        private async Task<List<object>> BIOsByClient(int idClient, DateTime initDate, DateTime finalDate)
        {
            var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer(Configuration.GetConnectionString("KyoSConnection")).Options;
            List<BioEntity> bioEntityList;

            using (DataContext db = new DataContext(options))
            {
                bioEntityList = await db.Bio

                                    .Where(m => (m.CreatedOn >= initDate && m.CreatedOn <= finalDate && m.Client.Id == idClient))
                                    .ToListAsync();
            }

            return bioEntityList.Select(m => new
                                {
                                    title = "BIO Document",
                                    start = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                        m.StartTime.Hour, m.StartTime.Minute, 0)
                                                                        .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    end = new DateTime(m.CreatedOn.Year, m.CreatedOn.Month, m.CreatedOn.Date.Day,
                                                                        m.EndTime.Hour, m.EndTime.Minute, 0)
                                                                        .ToString("yyyy-MM-ddTHH:mm:ssK"),
                                    backgroundColor = "#dff0d8",
                                    textColor = "#417c49",
                                    borderColor = "#417c49"
                                })
                                .ToList<object>();
        }
        #endregion
    }
}
