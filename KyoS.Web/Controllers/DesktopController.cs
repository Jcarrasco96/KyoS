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
                                                              && (wc.Note.Status == NoteStatus.Approved || wc.NoteP.Status == NoteStatus.Approved)
                                                              && wc.Workday.Service == ServiceType.PSR)).ToString();

                ViewBag.PendingNotes = _context.Workdays_Clients
                                                  .Include(wc => wc.Note)
                                                  .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                              && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
                                                              && wc.Workday.Service == ServiceType.PSR)).ToString();

                ViewBag.InProgressNotes = _context.Workdays_Clients
                                                  .Include(wc => wc.Note)
                                                  .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                              && (wc.Note.Status == NoteStatus.Edition || wc.NoteP.Status == NoteStatus.Edition)
                                                              && wc.Workday.Service == ServiceType.PSR)).ToString();
                                
                List <Workday_Client> not_started_list = await _context.Workdays_Clients
                                                          .Include(wc => wc.Note)
                                                          .Include(wc => wc.NoteP)
                                                          .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                      && wc.Present == true 
                                                                      && wc.Workday.Service == ServiceType.PSR)).ToListAsync();
                not_started_list = not_started_list.Where(wc => (wc.Note == null && wc.NoteP == null)).ToList();
                ViewBag.NotStartedNotes = not_started_list.Count.ToString();

                List<Workday_Client> notes_review_list = await _context.Workdays_Clients
                                                                 .Include(wc => wc.Messages)
                                                                 .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                            && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
                                                                            && wc.Workday.Service == ServiceType.PSR)).ToListAsync();
                notes_review_list = notes_review_list.Where(wc => wc.Messages.Count() > 0).ToList();
                ViewBag.NotesWithReview = notes_review_list.Count.ToString();

                ViewBag.NotPresentNotes = _context.Workdays_Clients
                                                  .Include(wc => wc.Note)
                                                  .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                              && wc.Present == false
                                                              && wc.Workday.Service == ServiceType.PSR)).ToString();
                
                //-----------------------------------------------------------------------------------------------------------------//
                
                ViewBag.ApprovedIndNotes = _context.Workdays_Clients
                                                   .Include(wc => wc.IndividualNote)
                                                   .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                              && wc.IndividualNote.Status == NoteStatus.Approved
                                                              && wc.Workday.Service == ServiceType.Individual)).ToString();

                ViewBag.PendingIndNotes = _context.Workdays_Clients
                                                  .Include(wc => wc.IndividualNote)
                                                  .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                              && wc.IndividualNote.Status == NoteStatus.Pending
                                                              && wc.Workday.Service == ServiceType.Individual)).ToString();

                ViewBag.InProgressIndNotes = _context.Workdays_Clients
                                                     .Include(wc => wc.IndividualNote)
                                                     .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                              && wc.IndividualNote.Status == NoteStatus.Edition
                                                              && wc.Workday.Service == ServiceType.Individual)).ToString();

                not_started_list = await _context.Workdays_Clients
                                                 .Include(wc => wc.IndividualNote)
                                                 .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                      && wc.Present == true
                                                                      && wc.Workday.Service == ServiceType.Individual)).ToListAsync();
                not_started_list = not_started_list.Where(wc => wc.IndividualNote == null).ToList();
                ViewBag.NotStartedIndNotes = not_started_list.Count.ToString();

                notes_review_list = await _context.Workdays_Clients
                                                  .Include(wc => wc.Messages)
                                                  .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                            && wc.IndividualNote.Status == NoteStatus.Pending
                                                                            && wc.Workday.Service == ServiceType.Individual)).ToListAsync();
                notes_review_list = notes_review_list.Where(wc => wc.Messages.Count() > 0).ToList();
                ViewBag.IndNotesWithReview = notes_review_list.Count.ToString();

                ViewBag.NotPresentIndNotes = _context.Workdays_Clients
                                                     .Include(wc => wc.IndividualNote)
                                                     .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                              && wc.Present == false
                                                              && wc.Workday.Service == ServiceType.Individual)).ToString();

                //-----------------------------------------------------------------------------------------------------------------//
                
                ViewBag.ApprovedGroupNotes = _context.Workdays_Clients                                                     
                                                     .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                && wc.GroupNote.Status == NoteStatus.Approved
                                                                && wc.Workday.Service == ServiceType.Group)).ToString();

                ViewBag.PendingGroupNotes = _context.Workdays_Clients                                                    
                                                    .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                               && wc.GroupNote.Status == NoteStatus.Pending
                                                               && wc.Workday.Service == ServiceType.Group)).ToString();

                ViewBag.InProgressGroupNotes = _context.Workdays_Clients                                                       
                                                       .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                  && wc.GroupNote.Status == NoteStatus.Edition
                                                                  && wc.Workday.Service == ServiceType.Group)).ToString();

                not_started_list = await _context.Workdays_Clients
                                                 .Include(wc => wc.GroupNote)
                                                 .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                            && wc.Present == true
                                                            && wc.Workday.Service == ServiceType.Group)).ToListAsync();
                not_started_list = not_started_list.Where(wc => wc.GroupNote == null).ToList();
                ViewBag.NotStartedGroupNotes = not_started_list.Count.ToString();

                notes_review_list = await _context.Workdays_Clients
                                                  .Include(wc => wc.Messages)
                                                  .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                             && wc.GroupNote.Status == NoteStatus.Pending
                                                             && wc.Workday.Service == ServiceType.Group)).ToListAsync();
                notes_review_list = notes_review_list.Where(wc => wc.Messages.Count() > 0).ToList();
                ViewBag.GroupNotesWithReview = notes_review_list.Count.ToString();

                ViewBag.NotPresentGroupNotes = _context.Workdays_Clients
                                                       .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                  && wc.Present == false
                                                                  && wc.Workday.Service == ServiceType.Group)).ToString();
                
                
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

                List<MTPEntity> mtps = await _context.MTPs
                                                     .Include(n => n.MtpReviewList)                                                       
                                                     .Where(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                              && m.Active == true && m.Client.Status == StatusType.Open
                                                              && m.Client.Group.Facilitator.Id == facilitator.Id)).ToListAsync();
                int count = 0;
                int month = 0;
                foreach (var item in mtps)
                {
                    foreach (var value in item.MtpReviewList)
                    {
                        month = month + value.MonthOfTreatment;
                    
                    }
                    if (item.NumberOfMonths != null)
                    {
                        if (DateTime.Now > item.MTPDevelopedDate.Date.AddMonths(Convert.ToInt32(item.NumberOfMonths+month)))
                        {
                            count++;
                        }
                    }
                    month = 0;
                }

                ViewBag.ExpiredMTPsFacilitator = count.ToString();

                List<ClientEntity> clientListPSR = await _context.Clients
                                                                  .Where(m => ((m.DischargeList.Count() == 0 || m.DischargeList.Where(n => n.TypeService == ServiceType.PSR).ToList().Count == 0) 
                                                                    && m.Clinic.Id == user_logged.Clinic.Id
                                                                    && m.Status == StatusType.Close && m.IdFacilitatorPSR == facilitator.Id)).ToListAsync();
                List<ClientEntity> clientListIND = await _context.Clients
                                                              .Include(m => m.DischargeList)

                                                              .Where(m => ((m.DischargeList.Count() == 0 || m.DischargeList.Where(n => n.TypeService == ServiceType.Individual).ToList().Count == 0)
                                                                    && m.Clinic.Id == user_logged.Clinic.Id
                                                                    && m.Status == StatusType.Close && m.IndividualTherapyFacilitator.Id == facilitator.Id)).ToListAsync();
                foreach (var item in clientListIND)
                {
                    clientListPSR.Add(item);
                }

                ViewBag.ClientDischarge = clientListPSR.Count().ToString();

                List<DischargeEntity> DischargeEdit = await _context.Discharge
                                                                    .Include(f => f.Client)
                                                                    .ThenInclude(n => n.Clinic)
                                                                    .Where(n => (n.Status == DischargeStatus.Edition
                                                                        && n.Client.Clinic.Id == user_logged.Clinic.Id 
                                                                        && n.CreatedBy == user_logged.UserName))
                                                                    .OrderBy(f => f.Client.Name)
                                                                    .ToListAsync();
                ViewBag.DischargeEdition = DischargeEdit.Count().ToString();

                List<DischargeEntity> DischargePending = await _context.Discharge
                                                                    .Include(f => f.Client)
                                                                    .ThenInclude(n => n.Clinic)
                                                                    .Where(n => (n.Status == DischargeStatus.Pending
                                                                        && n.Client.Clinic.Id == user_logged.Clinic.Id 
                                                                        && n.CreatedBy == user_logged.UserName))
                                                                    .OrderBy(f => f.Client.Name)
                                                                    .ToListAsync();
                ViewBag.DischargePending = DischargePending.Count().ToString();

                List<MTPReviewEntity> MTPReviewEdit = await _context.MTPReviews
                                                                .Include(n => n.Mtp)
                                                                .ThenInclude(n => n.Client)
                                                                .ThenInclude(n => n.Clinic)
                                                                .Where(m => (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                                   && m.Status == AdendumStatus.Edition
                                                                   && (m.Mtp.Client.IdFacilitatorPSR == facilitator.Id ||
                                                                       m.Mtp.Client.IndividualTherapyFacilitator.Id == facilitator.Id))).ToListAsync();
                ViewBag.MTPReviewEdition = MTPReviewEdit.Count().ToString();

                List<MTPReviewEntity> MTPReviewPending = await _context.MTPReviews
                                                                .Include(n => n.Mtp)
                                                                .ThenInclude(n => n.Client)
                                                                .ThenInclude(n => n.Clinic)
                                                                .Where(m => (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                                   && m.Status == AdendumStatus.Pending
                                                                   && m.Mtp.Client.Group.Facilitator.Id == facilitator.Id)).ToListAsync();
                ViewBag.MTPReviewPending = MTPReviewPending.Count().ToString();

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
                                                              && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
                                                              && wc.Workday.Service == ServiceType.PSR)).ToString();

                ViewBag.PendingIndNotes = _context.Workdays_Clients
                                                  .Count(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                             && wc.IndividualNote.Status == NoteStatus.Pending
                                                             && wc.Workday.Service == ServiceType.Individual)).ToString();

                ViewBag.PendingGroupNotes = _context.Workdays_Clients
                                                    .Count(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                               && wc.GroupNote.Status == NoteStatus.Pending
                                                               && wc.Workday.Service == ServiceType.Group)).ToString();

                ViewBag.PendingBIO = _context.Clients
                                                    .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                               && wc.Bio == null)).ToString();

                ViewBag.PendingInitialFars = _context.Clients
                                                    .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                               && wc.FarsFormList.Count == 0)).ToString();

                ViewBag.PendingFars = _context.FarsForm
                                                  .Count(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && m.Status == FarsStatus.Pending)).ToString();

                ViewBag.PendingAddendum = _context.Adendums
                                                  .Count(m => (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && m.Status == AdendumStatus.Pending)).ToString();

                ViewBag.PendingMTPReview = _context.MTPReviews
                                                  .Count(m => (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && m.Status == AdendumStatus.Pending)).ToString();

                ViewBag.PendingDischarge = _context.Discharge
                                                  .Count(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && m.Status == DischargeStatus.Pending)).ToString();

                List<ClientEntity> client = await _context.Clients
                                                          .Include(c => c.MTPs)
                                                          .Where(c => c.Clinic.Id == user_logged.Clinic.Id).ToListAsync();
                client = client.Where(wc => wc.MTPs.Count == 0).ToList();
                ViewBag.MTPMissing = client.Count.ToString();

                List<Workday_Client> notes_review_list = await _context.Workdays_Clients
                                                                       .Include(wc => wc.Messages)
                                                                       .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                               && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
                                                                               && wc.Workday.Service == ServiceType.PSR)).ToListAsync();
                notes_review_list = notes_review_list.Where(wc => wc.Messages.Count() > 0).ToList();
                ViewBag.NotesWithReview = notes_review_list.Count.ToString();

                notes_review_list = await _context.Workdays_Clients
                                                  .Include(wc => wc.Messages)
                                                  .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                             && wc.IndividualNote.Status == NoteStatus.Pending
                                                             && wc.Workday.Service == ServiceType.Individual)).ToListAsync();
                notes_review_list = notes_review_list.Where(wc => wc.Messages.Count() > 0).ToList();
                ViewBag.IndNotesWithReview = notes_review_list.Count.ToString();

                notes_review_list = await _context.Workdays_Clients
                                                  .Include(wc => wc.Messages)
                                                  .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                             && wc.GroupNote.Status == NoteStatus.Pending
                                                             && wc.Workday.Service == ServiceType.Group)).ToListAsync();
                notes_review_list = notes_review_list.Where(wc => wc.Messages.Count() > 0).ToList();
                ViewBag.GroupNotesWithReview = notes_review_list.Count.ToString();
            }
            if (User.IsInRole("Manager"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                ViewBag.PendingNotes = _context.Workdays_Clients
                                               .Count(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                         && (wc.Note.Status == NoteStatus.Pending || wc.IndividualNote.Status == NoteStatus.Pending 
                                                             || wc.GroupNote.Status == NoteStatus.Pending
                                                             || wc.NoteP.Status == NoteStatus.Pending))).ToString();

                ViewBag.InProgressNotes = _context.Workdays_Clients
                                                  .Count(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                            && (wc.Note.Status == NoteStatus.Edition || wc.IndividualNote.Status == NoteStatus.Edition
                                                                || wc.GroupNote.Status == NoteStatus.Edition || wc.NoteP.Status == NoteStatus.Edition))).ToString();

                List<Workday_Client> not_started_list = await _context.Workdays_Clients

                                                                      .Include(wc => wc.Note)
                                                                      .Include(wc => wc.IndividualNote)
                                                                      .Include(wc => wc.GroupNote)
                                                                      .Include(wc => wc.NoteP)

                                                                      .Where(wc => wc.Facilitator.Clinic.Id == user_logged.Clinic.Id)
                                                                      .ToListAsync();

                not_started_list = not_started_list.Where(wc => (wc.Note == null && wc.IndividualNote == null && 
                                                                 wc.GroupNote == null && wc.NoteP == null && wc.Present == true)).ToList();
                ViewBag.NotStartedNotes = not_started_list.Count.ToString();

                List<ClientEntity> client = await _context.Clients

                                                          .Include(c => c.MTPs)

                                                          .Where(c => c.Clinic.Id == user_logged.Clinic.Id)
                                                          .ToListAsync();

                client = client.Where(wc => wc.MTPs.Count == 0).ToList();
                ViewBag.MTPMissing = client.Count.ToString();

                List<Workday_Client> notes_review_list = await _context.Workdays_Clients
                                                                       .Include(wc => wc.Messages)
                                                                       .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id &&
                                                                                    (wc.Note.Status == NoteStatus.Pending || wc.IndividualNote.Status == NoteStatus.Pending ||
                                                                                     wc.GroupNote.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)))
                                                                       .ToListAsync();

                notes_review_list = notes_review_list.Where(wc => wc.Messages.Count() > 0).ToList();
                ViewBag.NotesWithReview = notes_review_list.Count.ToString();
                
                //-------------clients without documentation--------------//
                client = await _context.Clients
                                       .Where(c => c.Clinic.Id == user_logged.Clinic.Id).ToListAsync();
                int clients_without_doc = 0;                
                DocumentEntity doc_pe;
                DocumentEntity doc_intake;
                DocumentEntity doc_bio;
                DocumentEntity doc_mtp;
                DocumentEntity doc_fars;
                DocumentEntity doc_consent;
                foreach (var item in client)
                {
                    doc_pe = await _context.Documents
                                             .FirstOrDefaultAsync(d => (d.Description == DocumentDescription.Psychiatrist_evaluation
                                                                        && d.Client.Id == item.Id));
                    doc_intake = await _context.Documents
                                             .FirstOrDefaultAsync(d => (d.Description == DocumentDescription.Intake
                                                                        && d.Client.Id == item.Id));
                    doc_bio = await _context.Documents
                                             .FirstOrDefaultAsync(d => (d.Description == DocumentDescription.Bio
                                                                        && d.Client.Id == item.Id));
                    doc_mtp = await _context.Documents
                                             .FirstOrDefaultAsync(d => (d.Description == DocumentDescription.MTP
                                                                        && d.Client.Id == item.Id));
                    doc_fars = await _context.Documents
                                             .FirstOrDefaultAsync(d => (d.Description == DocumentDescription.Fars
                                                                        && d.Client.Id == item.Id));
                    doc_consent = await _context.Documents
                                             .FirstOrDefaultAsync(d => (d.Description == DocumentDescription.Consent
                                                                        && d.Client.Id == item.Id));

                    if (doc_pe == null || doc_intake == null || doc_bio == null || doc_mtp == null || doc_fars == null || doc_consent == null)
                    {
                        clients_without_doc++;
                    }
                }
                ViewBag.ClientsWithoutDoc = clients_without_doc.ToString();

                ViewBag.ApprovedNotes = _context.Workdays_Clients

                                                .Include(wc => wc.Note)

                                                .Count(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id &&
                                                             (wc.Note.Status == NoteStatus.Approved || wc.IndividualNote.Status == NoteStatus.Approved ||
                                                              wc.GroupNote.Status == NoteStatus.Approved || wc.NoteP.Status == NoteStatus.Approved))).ToString();

                ViewBag.NotPresentNotes = _context.Workdays_Clients
                                                  .Include(wc => wc.Note)
                                                  .Count(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                             && wc.Present == false)).ToString();

                List<MTPEntity> mtps = await _context.MTPs
                                                     .Include(n => n.MtpReviewList)
                                                     .Where(m => (m.Client.Clinic.Id == user_logged.Clinic.Id 
                                                               && m.Active == true && m.Client.Status == StatusType.Open)).ToListAsync();
                int count = 0;
                int month = 0;
                foreach (var item in mtps)
                {
                    foreach (var product in item.MtpReviewList)
                    {
                        month += product.MonthOfTreatment;
                    }
                    if (item.NumberOfMonths != null)
                    {
                        if (DateTime.Now > item.MTPDevelopedDate.Date.AddMonths(Convert.ToInt32(item.NumberOfMonths+month)))
                        {
                            count++;
                        }
                    }
                    month = 0;
                }

                ViewBag.ExpiredMTPs = count.ToString();
            }
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Index), "Incidents");
            }
            if (User.IsInRole("CaseManager"))
            {
                return RedirectToAction(nameof(Index), "Incidents");
            }
            return View();
        }
    }
}