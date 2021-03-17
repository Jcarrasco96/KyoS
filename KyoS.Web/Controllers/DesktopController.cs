using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Web.Controllers
{
    [Authorize]
    public class DesktopController : Controller
    {
        private readonly DataContext _context;
        public DesktopController(DataContext context)
        {
            _context = context;           
        }
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Facilitator"))
            {
                ViewBag.ApprovedNotes = _context.Workdays_Clients
                                                   .Include(wc => wc.Note)
                                                   .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name 
                                                              && wc.Note.Status == NoteStatus.Approved)).ToString();

                ViewBag.PendingNotes = _context.Workdays_Clients
                                                  .Include(wc => wc.Note)
                                                  .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                              && wc.Note.Status == NoteStatus.Pending)).ToString();

                ViewBag.InProgressNotes = _context.Workdays_Clients
                                                  .Include(wc => wc.Note)
                                                  .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                              && wc.Note.Status == NoteStatus.Edition)).ToString();
                                
                List <Workday_Client> not_started_list = await _context.Workdays_Clients
                                                          .Include(wc => wc.Note)
                                                          .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                 && wc.Present == true)).ToListAsync();
                not_started_list = not_started_list.Where(wc => wc.Note == null).ToList();
                ViewBag.NotStartedNotes = not_started_list.Count.ToString();

                List<Workday_Client> notes_review_list = await _context.Workdays_Clients
                                                                 .Include(wc => wc.Messages)
                                                                 .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                            && wc.Note.Status == NoteStatus.Pending)).ToListAsync();
                notes_review_list = notes_review_list.Where(wc => wc.Messages.Count() > 0).ToList();
                ViewBag.NotesWithReview = notes_review_list.Count.ToString();

                ViewBag.NotPresentNotes = _context.Workdays_Clients
                                                  .Include(wc => wc.Note)
                                                  .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                              && wc.Present == false)).ToString();
            }
            if (User.IsInRole("Supervisor"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                ViewBag.PendingActivities = _context.Activities
                                                    .Count(a => (a.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                              && a.Status == ActivityStatus.Pending)).ToString();

                ViewBag.PendingNotes = _context.Workdays_Clients
                                               .Count(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                              && wc.Note.Status == NoteStatus.Pending)).ToString();

                List<ClientEntity> client = await _context.Clients
                                                          .Include(c => c.MTPs)
                                                          .Where(c => c.Clinic.Id == user_logged.Clinic.Id).ToListAsync();
                client = client.Where(wc => wc.MTPs.Count == 0).ToList();
                ViewBag.MTPMissing = client.Count.ToString();

                List<Workday_Client> notes_review_list = await _context.Workdays_Clients
                                                                       .Include(wc => wc.Messages)
                                                                       .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                               && wc.Note.Status == NoteStatus.Pending)).ToListAsync();
                notes_review_list = notes_review_list.Where(wc => wc.Messages.Count() > 0).ToList();
                ViewBag.NotesWithReview = notes_review_list.Count.ToString();
            }
            if (User.IsInRole("Mannager"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                ViewBag.PendingNotes = _context.Workdays_Clients
                                               .Count(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                              && wc.Note.Status == NoteStatus.Pending)).ToString();

                ViewBag.InProgressNotes = _context.Workdays_Clients
                                                  .Include(wc => wc.Note)
                                                  .Count(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                              && wc.Note.Status == NoteStatus.Edition)).ToString();

                List<Workday_Client> not_started_list = await _context.Workdays_Clients
                                                          .Include(wc => wc.Note)
                                                          .Where(wc => wc.Facilitator.Clinic.Id == user_logged.Clinic.Id).ToListAsync();
                not_started_list = not_started_list.Where(wc => wc.Note == null).ToList();
                ViewBag.NotStartedNotes = not_started_list.Count.ToString();

                List<ClientEntity> client = await _context.Clients
                                                          .Include(c => c.MTPs)
                                                          .Where(c => c.Clinic.Id == user_logged.Clinic.Id).ToListAsync();
                client = client.Where(wc => wc.MTPs.Count == 0).ToList();
                ViewBag.MTPMissing = client.Count.ToString();

                List<Workday_Client> notes_review_list = await _context.Workdays_Clients
                                                                      .Include(wc => wc.Messages)
                                                                      .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                              && wc.Note.Status == NoteStatus.Pending)).ToListAsync();
                notes_review_list = notes_review_list.Where(wc => wc.Messages.Count() > 0).ToList();
                ViewBag.NotesWithReview = notes_review_list.Count.ToString();
            }
            return View();
        }
    }
}