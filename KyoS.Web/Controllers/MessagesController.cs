using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly DataContext _context;
        
        public MessagesController(DataContext context)
        {
            _context = context;
        }
        
        public async Task<IActionResult> Index()
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("Facilitator"))
            {
                ViewBag.CountsMessagesNotes = _context.Messages
                                                      .Count(m => (m.To == user_logged.UserName && m.Status == KyoS.Common.Enums.MessageStatus.NotRead
                                                                                                && m.Workday_Client != null && m.Notification == false))
                                                      .ToString();
                
                ViewBag.CountsMessagesFars = _context.Messages
                                                     .Count(m => (m.To == user_logged.UserName && m.Status == KyoS.Common.Enums.MessageStatus.NotRead
                                                                                               && m.FarsForm != null && m.Notification == false))
                                                     .ToString();

                ViewBag.CountsMessagesMTPReview = _context.Messages
                                                          .Count(m => (m.To == user_logged.UserName && m.Status == KyoS.Common.Enums.MessageStatus.NotRead
                                                                                                    && m.MTPReview != null && m.Notification == false))
                                                          .ToString();
                
                ViewBag.CountsMessagesAddendum = _context.Messages
                                                         .Count(m => (m.To == user_logged.UserName && m.Status == KyoS.Common.Enums.MessageStatus.NotRead
                                                                                                   && m.Addendum != null && m.Notification == false))
                                                         .ToString();

                ViewBag.CountsMessagesDischarge = _context.Messages
                                                          .Count(m => (m.To == user_logged.UserName && m.Status == KyoS.Common.Enums.MessageStatus.NotRead
                                                                                                    && m.Discharge != null && m.Notification == false))
                                                          .ToString();

                ViewBag.Total = Convert.ToInt32(ViewBag.CountsMessagesNotes) + Convert.ToInt32(ViewBag.CountsMessagesFars) + Convert.ToInt32(ViewBag.CountsMessagesMTPReview) + Convert.ToInt32(ViewBag.CountsMessagesAddendum) + Convert.ToInt32(ViewBag.CountsMessagesDischarge);
            }
            if (User.IsInRole("Documents_Assistant"))
            {
                ViewBag.CountsMessagesMtp = _context.Messages
                                                     .Count(m => (m.To == user_logged.UserName && m.Status == KyoS.Common.Enums.MessageStatus.NotRead
                                                                                               && m.Mtp != null && m.Notification == false))
                                                     .ToString();

                ViewBag.CountsMessagesBio = _context.Messages
                                                    .Count(m => (m.To == user_logged.UserName && m.Status == KyoS.Common.Enums.MessageStatus.NotRead
                                                                                              && m.Bio != null && m.Notification == false))
                                                    .ToString();

                ViewBag.Total = Convert.ToInt32(ViewBag.CountsMessagesMtp) + Convert.ToInt32(ViewBag.CountsMessagesBio);
            }
            return View();
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> MessagesOfNotes(int id = 0)
        {
            if (User.IsInRole("Facilitator"))
            {
                return View(await _context.Workdays_Clients

                                          .Include(wc => wc.Note)

                                          .Include(wc => wc.NoteP)

                                          .Include(wc => wc.IndividualNote)

                                          .Include(wc => wc.GroupNote)

                                          .Include(wc => wc.Facilitator)

                                          .Include(wc => wc.Client)

                                          .Include(wc => wc.Workday)
                                          .ThenInclude(w => w.Week)

                                          .Include(wc => wc.Messages.Where(m => m.Notification == false))

                                          .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name                                                                     
                                                     && wc.Messages.Count() > 0))
                                          .ToListAsync());
            }

            //if (User.IsInRole("Supervisor"))
            //{
            //    UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
            //                                                 .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            //    if (user_logged.Clinic != null)
            //    {
            //        return View(await _context.Workdays_Clients.Include(wc => wc.Note)

            //                                                   .Include(wc => wc.Facilitator)
            //                                                   .ThenInclude(f => f.Clinic)

            //                                                   .Include(wc => wc.Client)

            //                                                   .Include(wc => wc.Workday)
            //                                                   .ThenInclude(w => w.Week)

            //                                                   .Include(wc => wc.Messages)

            //                                                   .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
            //                                                       && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
            //                                                          && wc.Messages.Count() > 0
            //                                                          && (wc.Workday.Service == ServiceType.PSR || wc.Workday.Service == ServiceType.Individual)))
            //                                                   .ToListAsync());
            //    }
            //}

            return View();
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> MessagesOfFars(int id = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("Facilitator"))
            {
                return View(await _context.FarsForm

                                          .Include(f => f.Client)

                                          .Include(f => f.Messages.Where(m => m.Notification == false))

                                          .Where(f => (f.AdmissionedFor == user_logged.FullName
                                                    && f.Messages.Count() > 0))
                                          .ToListAsync());
            }

            //if (User.IsInRole("Supervisor"))
            //{
            //    UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
            //                                                 .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            //    if (user_logged.Clinic != null)
            //    {
            //        return View(await _context.Workdays_Clients.Include(wc => wc.Note)

            //                                                   .Include(wc => wc.Facilitator)
            //                                                   .ThenInclude(f => f.Clinic)

            //                                                   .Include(wc => wc.Client)

            //                                                   .Include(wc => wc.Workday)
            //                                                   .ThenInclude(w => w.Week)

            //                                                   .Include(wc => wc.Messages)

            //                                                   .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
            //                                                       && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
            //                                                          && wc.Messages.Count() > 0
            //                                                          && (wc.Workday.Service == ServiceType.PSR || wc.Workday.Service == ServiceType.Individual)))
            //                                                   .ToListAsync());
            //    }
            //}

            return View();
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> MessagesOfMTPReviews(int id = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("Facilitator"))
            {
                return View(await _context.MTPReviews

                                          .Include(m => m.Mtp)
                                          .ThenInclude(m => m.Client)

                                          .Include(m => m.Messages.Where(m => m.Notification == false))

                                          .Where(m => (m.CreatedBy == user_logged.UserName && m.Messages.Count() > 0))
                                          .ToListAsync());
            }

            //if (User.IsInRole("Supervisor"))
            //{
            //    UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
            //                                                 .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            //    if (user_logged.Clinic != null)
            //    {
            //        return View(await _context.Workdays_Clients.Include(wc => wc.Note)

            //                                                   .Include(wc => wc.Facilitator)
            //                                                   .ThenInclude(f => f.Clinic)

            //                                                   .Include(wc => wc.Client)

            //                                                   .Include(wc => wc.Workday)
            //                                                   .ThenInclude(w => w.Week)

            //                                                   .Include(wc => wc.Messages)

            //                                                   .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
            //                                                       && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
            //                                                          && wc.Messages.Count() > 0
            //                                                          && (wc.Workday.Service == ServiceType.PSR || wc.Workday.Service == ServiceType.Individual)))
            //                                                   .ToListAsync());
            //    }
            //}

            return View();
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> MessagesOfAddendums(int id = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("Facilitator"))
            {
                return View(await _context.Adendums

                                          .Include(a => a.Mtp)
                                          .ThenInclude(a => a.Client)

                                          .Include(a => a.Messages.Where(m => m.Notification == false))

                                          .Where(a => (a.CreatedBy == user_logged.UserName && a.Messages.Count() > 0))
                                          .ToListAsync());
            }

            //if (User.IsInRole("Supervisor"))
            //{
            //    UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
            //                                                 .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            //    if (user_logged.Clinic != null)
            //    {
            //        return View(await _context.Workdays_Clients.Include(wc => wc.Note)

            //                                                   .Include(wc => wc.Facilitator)
            //                                                   .ThenInclude(f => f.Clinic)

            //                                                   .Include(wc => wc.Client)

            //                                                   .Include(wc => wc.Workday)
            //                                                   .ThenInclude(w => w.Week)

            //                                                   .Include(wc => wc.Messages)

            //                                                   .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
            //                                                       && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
            //                                                          && wc.Messages.Count() > 0
            //                                                          && (wc.Workday.Service == ServiceType.PSR || wc.Workday.Service == ServiceType.Individual)))
            //                                                   .ToListAsync());
            //    }
            //}

            return View();
        }

        [Authorize(Roles = "Facilitator")]
        public async Task<IActionResult> MessagesOfDischarges(int id = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("Facilitator"))
            {
                return View(await _context.Discharge

                                          .Include(d => d.Client)

                                          .Include(d => d.Messages.Where(m => m.Notification == false))

                                          .Where(a => (a.CreatedBy == user_logged.UserName && a.Messages.Count() > 0))
                                          .ToListAsync());
            }

            //if (User.IsInRole("Supervisor"))
            //{
            //    UserEntity user_logged = await _context.Users.Include(u => u.Clinic)
            //                                                 .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            //    if (user_logged.Clinic != null)
            //    {
            //        return View(await _context.Workdays_Clients.Include(wc => wc.Note)

            //                                                   .Include(wc => wc.Facilitator)
            //                                                   .ThenInclude(f => f.Clinic)

            //                                                   .Include(wc => wc.Client)

            //                                                   .Include(wc => wc.Workday)
            //                                                   .ThenInclude(w => w.Week)

            //                                                   .Include(wc => wc.Messages)

            //                                                   .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
            //                                                       && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
            //                                                          && wc.Messages.Count() > 0
            //                                                          && (wc.Workday.Service == ServiceType.PSR || wc.Workday.Service == ServiceType.Individual)))
            //                                                   .ToListAsync());
            //    }
            //}

            return View();
        }

        [Authorize(Roles = "Facilitator, Supervisor, Manager, Documents_Assistant")]
        public async Task<IActionResult> Notifications(int id = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
            
            return View(await _context.Messages

                                      .Include(m => m.Workday_Client)
                                      .ThenInclude(wc => wc.Note)

                                      .Include(m => m.Workday_Client)
                                      .ThenInclude(wc => wc.NoteP)

                                      .Include(m => m.Workday_Client)
                                      .ThenInclude(wc => wc.IndividualNote)

                                      .Include(m => m.Workday_Client)
                                      .ThenInclude(wc => wc.GroupNote)

                                      .Include(m => m.FarsForm)

                                      .Include(m => m.MTPReview)

                                      .Include(m => m.Addendum)

                                      .Include(m => m.Discharge)

                                      .Include(m => m.Mtp)

                                      .Include(m => m.Bio)

                                      .Where(m => (m.To == user_logged.UserName && m.Notification == true))
                                      .ToListAsync());            
        }

        [Authorize(Roles = "Supervisor")]
        public async Task<IActionResult> Reviews(int id = 0)
        {
            MessageEntity notification = await _context.Messages

                                                       .Include(m => m.Workday_Client)
                                                       .Include(m => m.FarsForm)
                                                       .Include(m => m.MTPReview)
                                                       .Include(m => m.Addendum)
                                                       .Include(m => m.Discharge)
                                                       .Include(m => m.Mtp)
                                                       .Include(m => m.Bio)
                                                       .FirstOrDefaultAsync(m => m.Id == id);

            if (notification == null)
            {
                return null;
            }

            if (notification.Workday_Client != null)
            {
                return View(await _context.Messages
                                          .Where(m => (m.Workday_Client.Id == notification.Workday_Client.Id && m.Notification == false))
                                          .ToListAsync());
            }
            
            if (notification.FarsForm != null)
            {
                return View(await _context.Messages
                                          .Where(m => (m.FarsForm.Id == notification.FarsForm.Id && m.Notification == false))
                                          .ToListAsync());
            }

            if (notification.MTPReview != null)
            {
                return View(await _context.Messages

                                          .Where(m => (m.MTPReview.Id == notification.MTPReview.Id && m.Notification == false))
                                          .ToListAsync());
            }

            if (notification.Addendum != null)
            {
                return View(await _context.Messages

                                          .Where(m => (m.Addendum.Id == notification.Addendum.Id && m.Notification == false))
                                          .ToListAsync());
            }

            if (notification.Discharge != null)
            {
                return View(await _context.Messages

                                          .Where(m => (m.Discharge.Id == notification.Discharge.Id && m.Notification == false))
                                          .ToListAsync());
            }

            if (notification.Mtp != null)
            {
                return View(await _context.Messages

                                          .Where(m => (m.Mtp.Id == notification.Mtp.Id && m.Notification == false))
                                          .ToListAsync());
            }

            if (notification.Bio != null)
            {
                return View(await _context.Messages

                                          .Where(m => (m.Bio.Id == notification.Bio.Id && m.Notification == false))
                                          .ToListAsync());
            }

            return null;
        }

        [Authorize(Roles = "Documents_Assistant")]
        public async Task<IActionResult> MessagesOfMTP(int id = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("Documents_Assistant"))
            {
                return View(await _context.MTPs

                                          .Include(m => m.Client)

                                          .Include(m => m.Messages.Where(m => m.Notification == false))

                                          .Where(m => (m.CreatedBy == user_logged.UserName && m.Messages.Count() > 0))
                                          .ToListAsync());
            }

            return View();
        }

        [Authorize(Roles = "Documents_Assistant")]
        public async Task<IActionResult> MessagesOfBio(int id = 0)
        {
            UserEntity user_logged = await _context.Users
                                                   .Include(u => u.Clinic)
                                                   .ThenInclude(c => c.Setting)
                                                   .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            if (User.IsInRole("Documents_Assistant"))
            {
                return View(await _context.Bio

                                          .Include(m => m.Client)

                                          .Include(m => m.Messages.Where(m => m.Notification == false))

                                          .Where(m => (m.CreatedBy == user_logged.UserName && m.Messages.Count() > 0))
                                          .ToListAsync());
            }

            return View();
        }

    }
}