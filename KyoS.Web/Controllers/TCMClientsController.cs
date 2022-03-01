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

namespace KyoS.Web.Controllers
{
    [Authorize(Roles = "Admin, Manager")]
    public class TCMClientsController : Controller
    {
        private readonly DataContext _context;
        private readonly IConverterHelper _converterHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IReportHelper _reportHelper;

        public TCMClientsController(DataContext context, ICombosHelper combosHelper, IConverterHelper converterHelper, IReportHelper reportHelper)
        {
            _context = context;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
            _reportHelper = reportHelper;
        }
        
        public async Task<IActionResult> Index()
        {
            UserEntity user_logged = _context.Users

                                             .Include(u => u.Clinic)
                                             .ThenInclude(c => c.Setting)

                                             .FirstOrDefault(u => u.UserName == User.Identity.Name);

            /*if (user_logged.Clinic == null || user_logged.Clinic.Setting == null || !user_logged.Clinic.Setting.MentalHealthClinic)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            ClinicEntity clinic = await _context.Clinics.FirstOrDefaultAsync(c => c.Id == user_logged.Clinic.Id);*/

            return View(await _context.TCMClient

                                      .Include(g => g.Casemanager)

                                      .Include(g => g.Clients)

                                      //.Where(g => (g.Casemanager.Clinic.Id == clinic.Id))
                                      .OrderBy(g => g.Casemanager.Name)
                                      .ToListAsync());
        }

        public async Task<IActionResult> Create(int id = 0, int error = 0, int idCaseMannager = 0, int idClient = 0)
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

            //the facilitator has not availability on that dates
            if (error == 1)
            {
                CaseMannagerEntity caseMannager = _context.CaseManagers
                                                        .FirstOrDefault(f => f.Id == idCaseMannager);
                ViewBag.Error = "0";
                ViewBag.errorText = $"Error. The CaseMannager {caseMannager.Name} has another therapy already at that time";
            }

            //One client has a created note from another service at that time.
            if (error == 2)
            {
                ClientEntity client = _context.Clients
                                              .FirstOrDefault(f => f.Id == idClient);
                ViewBag.Error = "1";
                ViewBag.errorText = $"Error. The client {client.Name} has a created note from another therapy at that time";
            }

            TCMClientViewModel model;
            MultiSelectList client_list;
            List<ClientEntity> clients = new List<ClientEntity>();

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    model = new TCMClientViewModel
                    {
                        CaseMannagers = _combosHelper.GetComboCasemannagersByClinic(user_logged.Clinic.Id)
                    };

                    clients = await _context.Clients

                                            .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                && c.Status == Common.Enums.StatusType.Open
                                                && c.Service == Common.Enums.ServiceType.PSR))
                                            .OrderBy(c => c.Name).ToListAsync();

                   // clients = clients.Where(c => c.MTPs.Count > 0).ToList();
                    client_list = new MultiSelectList(clients, "Id", "Name");
                    ViewData["clients"] = client_list;
                    return View(model);
                }
            }

            model = new TCMClientViewModel
            {
                CaseMannagers = _combosHelper.GetComboCaseManager()
            };

            clients = await _context.Clients
                                            .Where(c => c.Service == Common.Enums.ServiceType.PSR)
                                            .OrderBy(c => c.Name).ToListAsync();

            client_list = new MultiSelectList(clients, "Id", "Name");
            ViewData["clients"] = client_list;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TCMClientViewModel model, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
              
                UserEntity user_logged = _context.Users
                                                 .Include(u => u.Clinic)
                                                 .FirstOrDefault(u => u.UserName == User.Identity.Name);
                
                model.Casemanager = _context.CaseManagers.FirstOrDefault(u => u.Id == model.IdCaseMannager);
                TCMClientEntity tcmClient = await _converterHelper.ToTCMClientEntity(model, true);
               
                if (!string.IsNullOrEmpty(form["clients"]))
                {
                    string[] clients = form["clients"].ToString().Split(',');
                    ClientEntity client;
                    for (int i = 0; i < clients.Length; i++)
                    {
                        client = await _context.Clients
                                             .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(clients[i]));
                        if (client != null)
                        {
                            tcmClient.Clients.Add(client);
                        }
                    }
                  
                }
                if (_context.TCMClient.FirstOrDefault(u => u.Casemanager.Id == model.IdCaseMannager) != null)
                {
                    return RedirectToAction("Create", new { id = 2 });
                }
                else 
                {
                    _context.Add(tcmClient);
                    await _context.SaveChangesAsync();
                }
                    
                return RedirectToAction("Create", new { id = 1 });
            }

            model.CaseMannagers = _combosHelper.GetComboCaseManager();

            MultiSelectList client_list = new MultiSelectList(await _context.Clients.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
            ViewData["clients"] = client_list;

            return View(model);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMServiceEntity tcmServiceEntity = await _context.TCMServices.FirstOrDefaultAsync(s => s.Id == id);
            if (tcmServiceEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            try
            {
                _context.TCMServices.Remove(tcmServiceEntity);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return RedirectToAction("Index", new { idError = 1 });
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int? id, int error = 0, int idFacilitator = 0, int idClient = 0)
        {
            if (id == null)
            {
                return RedirectToAction("Home/Error404");
            }

            TCMClientEntity tcmClientEntity = await _context.TCMClient.Include(g => g.Casemanager)
                                                           .Include(g => g.Clients).FirstOrDefaultAsync(g => g.Id == id);
            if (tcmClientEntity == null)
            {
                return RedirectToAction("Home/Error404");
            }

            MultiSelectList client_list;
            TCMClientViewModel tcmClientViewModel = _converterHelper.ToTCMClientViewModel(tcmClientEntity);
           
            List<ClientEntity> clients = new List<ClientEntity>();

            if (!User.IsInRole("Admin"))
            {
                UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                       .FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (user_logged.Clinic != null)
                {
                    tcmClientViewModel.CaseMannagers = _combosHelper.GetComboCasemannagersByClinic(user_logged.Clinic.Id);

                    clients = await _context.Clients

                                            .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                      && c.Status == Common.Enums.StatusType.Open))
                                            .OrderBy(c => c.Name)
                                            .ToListAsync();

                    client_list = new MultiSelectList(clients, "Id", "Name", tcmClientViewModel.Clients.Select(c => c.Id));
                    ViewData["clients"] = client_list;
                    return View(tcmClientViewModel);
                }
            }

            clients = await _context.Clients.ToListAsync();
            client_list = new MultiSelectList(clients, "Id", "Name", tcmClientViewModel.Clients.Select(c => c.Id));
            ViewData["clients"] = client_list;
            return View(tcmClientViewModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TCMClientViewModel model, IFormCollection form)
        {
            if (ModelState.IsValid)
            {
                model.Casemanager = _context.CaseManagers.FirstOrDefault(u => u.Id == model.IdCaseMannager);
              
                if (!string.IsNullOrEmpty(form["clients"]))
                {
                    string[] clients = form["clients"].ToString().Split(',');
                    ClientEntity client;
                    foreach (string value in clients)
                    {
                        client = await _context.Clients
                                               .FirstOrDefaultAsync(c => c.Id == Convert.ToInt32(value));
                        if (client != null)
                        {
                            _context.Update(client);
                            model.Clients.Add(client);
                        }                           
                    }
                }

                TCMClientEntity tcmClient = await _converterHelper.ToTCMClientEntity(model, false);
                _context.TCMClient.Update(tcmClient);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (System.Exception ex)
                {
                    if (ex.InnerException.Message.Contains("duplicate"))
                    {
                        ModelState.AddModelError(string.Empty, "Already exists the group");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, ex.InnerException.Message);
                    }
                }
            }

            TCMClientEntity tcmclientEntity = await _context.TCMClient.Include(g => g.Casemanager)
                                                           .Include(g => g.Clients).FirstOrDefaultAsync(g => g.Id == model.Id);
            TCMClientViewModel tcmclientViewModel = _converterHelper.ToTCMClientViewModel(tcmclientEntity);
           
            MultiSelectList client_list = new MultiSelectList(await _context.Clients.ToListAsync(), "Id", "Name", tcmclientViewModel.Clients.Select(c => c.Id));
            ViewData["clients"] = client_list;

            return View(tcmclientViewModel);
        }
    }
}
