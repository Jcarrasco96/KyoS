using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using KyoS.Web.Helpers;
using KyoS.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.IO;
using KyoS.Common.Enums;
using KyoS.Common.Helpers;
using AspNetCore.Reporting;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ReferredsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IExportExcellHelper _exportExcelHelper;

        public ReferredsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IExportExcellHelper exportExcelHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _exportExcelHelper = exportExcelHelper;
        }
        
        public async Task<IActionResult> Index(int idError = 0)
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            if (idError == 1) //Imposible to delete
            {
                ViewBag.Delete = "N";
            }            
            
            ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);

            if (clinic != null)
            {
                List<ReferredEntity> referreds = await _context.Referreds.OrderBy(d => d.Name).ToListAsync();
                List<ReferredEntity> referreds_by_clinic = new List<ReferredEntity>();
                UserEntity user;
                foreach (ReferredEntity item in referreds)
                {
                    user = _context.Users.FirstOrDefault(u => u.Id == item.CreatedBy);
                    if (clinic.Users.Contains(user))
                    {
                        referreds_by_clinic.Add(item);
                    }
                }
                return View(referreds_by_clinic);
            }
            else
            {
                return View(null);
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

            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            ReferredViewModel model = new ReferredViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReferredViewModel referredViewModel)
        {
            if (ModelState.IsValid)
            {
                ReferredEntity referred = await _context.Referreds.FirstOrDefaultAsync(c => c.Name == referredViewModel.Name);
                if (referred == null)
                {
                    UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                    ReferredEntity referredEntity = _converterHelper.ToReferredEntity(referredViewModel, true, user_logged.Id);

                    _context.Add(referredEntity);
                    try
                    {
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Create", new { id = 1 });
                    }
                    catch (System.Exception ex)
                    {
                        if (ex.InnerException.Message.Contains("duplicate"))
                        {
                            ModelState.AddModelError(string.Empty, $"Already exists the referred: {referredEntity.Name}");
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
            return View(referredViewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || (!user_logged.Clinic.Setting.MentalHealthClinic && !user_logged.Clinic.Setting.TCMClinic))
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            ReferredEntity referredEntity = await _context.Referreds.FirstOrDefaultAsync(c => c.Id == id);
            if (referredEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ReferredViewModel referredViewModel = _converterHelper.ToReferredViewModel(referredEntity);

            return View(referredViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReferredViewModel referredViewModel)
        {
            if (id != referredViewModel.Id)
            {
                return RedirectToAction("Home/Error404");
            }

            if (ModelState.IsValid)
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                           .FirstOrDefault(u => u.UserName == User.Identity.Name);

                ReferredEntity referredEntity = _converterHelper.ToReferredEntity(referredViewModel, false, user_logged.Id);
                _context.Update(referredEntity);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, $"Already exists the referred: {referredEntity.Name}");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }
            return View(referredViewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            ReferredEntity referredEntity = await _context.Referreds.FirstOrDefaultAsync(c => c.Id == id);
            if (referredEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.Referreds.Remove(referredEntity);
                await _context.SaveChangesAsync();
            }
            catch (System.Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }
            
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Manager")]
        public IActionResult EXCELallReferred()
        {
            UserEntity user_logged = _context.Users
                                             .Include(u => u.Clinic)
                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            List<Client_Referred> clientRefereeds = new List<Client_Referred>();
            string date = "Date Report: " + DateTime.Today.ToLongDateString();

            clientRefereeds = _context.Clients_Referreds
                              .Include(w => w.Client)
                              .Include(w => w.Referred)

                              .Where(n => n.Client.Clinic.Id == user_logged.Clinic.Id)
                              .OrderBy(n => n.Referred.Name)
                              .ToList();

            byte[] content = _exportExcelHelper.ExportAllReferreds(clientRefereeds);

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ALL_REFERREDS.xlsx");
        }
    }
}