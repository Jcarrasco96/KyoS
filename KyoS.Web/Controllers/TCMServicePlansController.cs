using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace KyoS.Web.Controllers
{
    public class TCMServicePlansController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IRenderHelper _renderHelper;
        private readonly IReportHelper _reportHelper;
        public TCMServicePlansController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IRenderHelper renderHelper, IReportHelper reportHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _renderHelper = renderHelper;
            _reportHelper = reportHelper;
        }

        [Authorize(Roles = "Admin, Manager, TCMSupervisor, CaseManager")]
        public async Task<IActionResult> Index(int idError = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);
            if(user_logged.UserType.ToString() != "Admin")
            {
                /*if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    return RedirectToAction("NotAuthorized", "Account");
                }*/

                if (user_logged.UserType.ToString() == "CaseManager")
                {
                    ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);
                    CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);
                    List<TCMClientEntity> Client = await _context.TCMClient
                                                        .Include(g => g.Clients)
                                                        .Where(g => (g.Casemanager.Id == caseManager.Id))
                                                        .ToListAsync();
                   return Json(new { isValid = true, html = _renderHelper.RenderRazorViewToString(this, "_TCMServicePlan", Client) });
                }
                return View(null);

            }

            else
                return View(null);

        }

        [Authorize(Roles = "CaseManager")]
        public IActionResult Create(int id)
        {
            /* if (id == 1)
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
             }*/
            TCMServicePlanViewModel model;
            if (User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);

                ClientEntity client = _context.Clients.FirstOrDefault(c => c.Id == id);
                              
                if (user_logged.Clinic != null)
                {
                   
                    model = new TCMServicePlanViewModel
                     { 
                        Client = client,
                        Case_Number = 2,
                        Date_ServicePlan = DateTime.Today.Date,
                        Date_Intake = DateTime.Today.Date,
                        Date_Assessment = DateTime.Today.Date,
                        Date_Certification = DateTime.Today.Date,
                     };
                    return View(model);
                }
                return RedirectToAction("NotAuthorized", "Account");

            }
            return RedirectToAction("NotAuthorized", "Account");
        }

        public void UpdateTCMServicePlanToNonActive(ClientEntity client)
        {
            List<TCMServicePlanEntity> tcmServicePlan_list = _context.TCMServicePlans.Where(m => m.Client == client).ToList();
            if (tcmServicePlan_list.Count() > 0)
            {
                foreach (TCMServicePlanEntity item in tcmServicePlan_list)
                {
                    item.Active = false;
                    _context.Update(item);
                }
                _context.SaveChangesAsync();
            }
        }
    }
}
