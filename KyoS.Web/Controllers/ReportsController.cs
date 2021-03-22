﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspNetCore.Reporting;
using KyoS.Common.Helpers;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    public class ReportsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IDateHelper _dateHelper;
        private readonly IWebHostEnvironment _webhostEnvironment;
        private readonly IImageHelper _imageHelper;

        public ReportsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IDateHelper dateHelper, IWebHostEnvironment webHostEnvironment, IImageHelper imageHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _dateHelper = dateHelper;
            _webhostEnvironment = webHostEnvironment;
            _imageHelper = imageHelper;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> DailyAssistance()
        {
            UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                         .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user_logged.Clinic == null)
                return View(null);

            return View(await _context.Weeks.Include(w => w.Days)
                                                .ThenInclude(d => d.Workdays_Clients)
                                                .ThenInclude(wc => wc.Client)
                                                .ThenInclude(c => c.Group)

                                                .Include(w => w.Days)
                                                .ThenInclude(d => d.Workdays_Clients)
                                                .ThenInclude(g => g.Facilitator)

                                                .Include(w => w.Days)
                                                .ThenInclude(d => d.Workdays_Clients)
                                                .ThenInclude(wc => wc.Note)

                                                .Where(w => (w.Clinic.Id == user_logged.Clinic.Id))
                                                .ToListAsync());            
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> PrintDailyAssistance(int id)
        {
            UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
                                                         .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            List<Workday_Client> workdayClientList = await _context.Workdays_Clients
                                                               .Include(wc => wc.Facilitator)
                                                               .ThenInclude(f => f.Clinic)

                                                               .Include(wc => wc.Workday)

                                                               .Include(wc => wc.Client)

                                                               .Where(wc => (wc.Workday.Id == id && wc.Facilitator.LinkedUser == User.Identity.Name))
                                                               .ToListAsync();
            if (workdayClientList.Count() == 0)
            {
                return NotFound();
            }

            List<Workday_Client> am_list = workdayClientList.Where(wc => wc.Session == "AM").ToList();
            List<Workday_Client> pm_list = workdayClientList.Where(wc => wc.Session == "PM").ToList();

            if(am_list.Count() != 0)
                return DailyAssistanceReport(am_list);
            if (pm_list.Count() != 0)
                return DailyAssistanceReport(pm_list);

            return null;
        }

        private IActionResult DailyAssistanceSolAndVidaReport(List<Workday_Client> workdayClientList)
        {
            throw new NotImplementedException();
        }

        private IActionResult DailyAssistanceReport(List<Workday_Client> workdayClientList)
        {
            //report
            string mimetype = "";
            string fileDirPath = Assembly.GetExecutingAssembly().Location.Replace("KyoS.Web.dll", string.Empty);
            string rdlcFilePath = string.Format("{0}Reports\\Generics\\{1}.rdlc", fileDirPath, $"rptDailyAssistance");
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            System.Text.Encoding.GetEncoding("windows-1252");
            LocalReport report = new LocalReport(rdlcFilePath);

            //signatures images 
            byte[] stream1 = null;
            byte[] stream2 = null;
            string path;
            if (!string.IsNullOrEmpty(workdayClientList.First().Facilitator.Clinic.LogoPath))
            {
                path = string.Format($"{_webhostEnvironment.WebRootPath}{_imageHelper.TrimPath(workdayClientList.First().Facilitator.Clinic.LogoPath)}");
                stream1 = _imageHelper.ImageToByteArray(path);
            }            

            //datasource            
            List<ClinicEntity> clinics = new List<ClinicEntity> { workdayClientList.First().Facilitator.Clinic };
            List<FacilitatorEntity> facilitators = new List<FacilitatorEntity> { workdayClientList.First().Facilitator };
            List<ImageArray> images = new List<ImageArray> { new ImageArray { ImageStream1 = stream1, ImageStream2 = stream2 } };

            report.AddDataSource("dsWorkdays_Clients", workdayClientList);
            report.AddDataSource("dsClinics", clinics);
            report.AddDataSource("dsFacilitators", facilitators);
            report.AddDataSource("dsImages", images);

            var date = workdayClientList.First().Workday.Date.ToShortDateString();
            var additionalComments = string.Empty;
            foreach (Workday_Client item in workdayClientList)
            {
                if (!item.Present)
                {
                    if(additionalComments != string.Empty)
                        additionalComments = $"{additionalComments}\n{item.ClientName} - {item.CauseOfNotPresent}"; 
                    else
                        additionalComments = $"{item.ClientName} - {item.CauseOfNotPresent}";
                }
            }

            parameters.Add("date", date);
            parameters.Add("session", workdayClientList.First().Session);
            parameters.Add("AdditionalComments", additionalComments);

            var result = report.Execute(RenderType.Pdf, 1, parameters, mimetype);
            return File(result.MainStream, System.Net.Mime.MediaTypeNames.Application.Octet,
                        $"DailyAssistance_{workdayClientList.First().Workday.Date.ToShortDateString()}.pdf");
        }

        private IActionResult DailyAssistanceDavilaReport(List<Workday_Client> workdayClientList)
        {
            throw new NotImplementedException();
        }
    }
}