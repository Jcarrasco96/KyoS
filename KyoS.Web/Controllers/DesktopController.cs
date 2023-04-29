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
                List<Workday_Client> not_started_list;
                List<Workday_Client> notes_review_list;

                ViewBag.EnabledPSR = "1";
                if (_context.Workdays_Clients.Count(wc => (wc.Workday.Service == ServiceType.PSR && wc.Facilitator.LinkedUser == User.Identity.Name)) > 0)
                {
                    ViewBag.ApprovedNotes = _context.Workdays_Clients                                                   
                                                    .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                              && (wc.Note.Status == NoteStatus.Approved || wc.NoteP.Status == NoteStatus.Approved)
                                                              && wc.Workday.Service == ServiceType.PSR)).ToString();

                    ViewBag.PendingNotes = _context.Workdays_Clients                                                      
                                                   .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                             && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
                                                             && wc.Workday.Service == ServiceType.PSR)).ToString();

                    ViewBag.InProgressNotes = _context.Workdays_Clients                                                      
                                                      .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                  && (wc.Note.Status == NoteStatus.Edition || wc.NoteP.Status == NoteStatus.Edition)
                                                                  && wc.Workday.Service == ServiceType.PSR)).ToString();

                   not_started_list = await _context.Workdays_Clients
                                                    .Include(wc => wc.Note)
                                                    .Include(wc => wc.NoteP)
                                                    .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                               && wc.Present == true
                                                               && wc.Workday.Service == ServiceType.PSR)).ToListAsync();
                    not_started_list = not_started_list.Where(wc => (wc.Note == null && wc.NoteP == null)).ToList();
                    ViewBag.NotStartedNotes = not_started_list.Count.ToString();

                    notes_review_list = await _context.Workdays_Clients
                                                      .Include(wc => wc.Messages)
                                                      .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
                                                                && wc.Workday.Service == ServiceType.PSR)).ToListAsync();
                    notes_review_list = notes_review_list.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                    ViewBag.NotesWithReview = notes_review_list.Count.ToString();

                    ViewBag.NotPresentNotes = _context.Workdays_Clients                                                      
                                                      .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                  && wc.Present == false
                                                                  && wc.Workday.Service == ServiceType.PSR)).ToString();
                }
                else
                    ViewBag.EnabledPSR = "0";

                //-----------------------------------------------------------------------------------------------------------------//

                ViewBag.EnabledInd = "1";
                if (_context.Workdays_Clients.Count(wc => wc.Workday.Service == ServiceType.Individual && wc.Facilitator.LinkedUser == User.Identity.Name) > 0)
                {
                    ViewBag.ApprovedIndNotes = _context.Workdays_Clients                                                   
                                                       .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                              && wc.IndividualNote.Status == NoteStatus.Approved
                                                              && wc.Workday.Service == ServiceType.Individual)).ToString();

                    ViewBag.PendingIndNotes = _context.Workdays_Clients                                                      
                                                      .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                  && wc.IndividualNote.Status == NoteStatus.Pending
                                                                  && wc.Workday.Service == ServiceType.Individual)).ToString();

                    ViewBag.InProgressIndNotes = _context.Workdays_Clients                                                         
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
                    notes_review_list = notes_review_list.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                    ViewBag.IndNotesWithReview = notes_review_list.Count.ToString();

                    ViewBag.NotPresentIndNotes = _context.Workdays_Clients                                                         
                                                         .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                  && wc.Present == false
                                                                  && wc.Workday.Service == ServiceType.Individual)).ToString();

                }
                else
                    ViewBag.EnabledInd = "0";

                //-----------------------------------------------------------------------------------------------------------------//

                ViewBag.EnabledGroup = "1";
                if (_context.Workdays_Clients.Count(wc => wc.Workday.Service == ServiceType.Group && wc.Facilitator.LinkedUser == User.Identity.Name) > 0)
                {
                    ViewBag.ApprovedGroupNotes = _context.Workdays_Clients
                                                         .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                    && (wc.GroupNote.Status == NoteStatus.Approved
                                                                    || wc.GroupNote2.Status == NoteStatus.Approved)
                                                                    && wc.Workday.Service == ServiceType.Group)).ToString();

                    ViewBag.PendingGroupNotes = _context.Workdays_Clients
                                                        .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                   && (wc.GroupNote.Status == NoteStatus.Pending
                                                                    || wc.GroupNote2.Status == NoteStatus.Pending)
                                                                   && wc.Workday.Service == ServiceType.Group)).ToString();

                    ViewBag.InProgressGroupNotes = _context.Workdays_Clients
                                                           .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                      && (wc.GroupNote.Status == NoteStatus.Edition
                                                                    || wc.GroupNote2.Status == NoteStatus.Edition)
                                                                      && wc.Workday.Service == ServiceType.Group)).ToString();

                    not_started_list = await _context.Workdays_Clients
                                                     .Include(wc => wc.GroupNote)
                                                     .Include(wc => wc.GroupNote2)
                                                     .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                && wc.Present == true
                                                                && wc.Workday.Service == ServiceType.Group)).ToListAsync();
                    not_started_list = not_started_list.Where(wc => wc.GroupNote == null && wc.GroupNote2 == null).ToList();
                    ViewBag.NotStartedGroupNotes = not_started_list.Count.ToString();

                    notes_review_list = await _context.Workdays_Clients
                                                      .Include(wc => wc.Messages)
                                                      .Where(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                 && (wc.GroupNote.Status == NoteStatus.Pending
                                                                 || wc.GroupNote2.Status == NoteStatus.Pending)
                                                                 && wc.Workday.Service == ServiceType.Group)).ToListAsync();
                    notes_review_list = notes_review_list.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                    ViewBag.GroupNotesWithReview = notes_review_list.Count.ToString();

                    ViewBag.NotPresentGroupNotes = _context.Workdays_Clients
                                                           .Count(wc => (wc.Facilitator.LinkedUser == User.Identity.Name
                                                                      && wc.Present == false
                                                                      && wc.Workday.Service == ServiceType.Group)).ToString();
                }
                else
                    ViewBag.EnabledGroup = "0";

                //-----------------------------------------------------------------------------------------------------------------//

                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                FacilitatorEntity facilitator = _context.Facilitators.FirstOrDefault(n => n.LinkedUser == user_logged.UserName);

                List<MTPEntity> mtps = await _context.MTPs
                                                     .Include(n => n.MtpReviewList)                                                       
                                                     .Where(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                              && m.Active == true && m.Client.Status == StatusType.Open
                                                              && (m.Client.IdFacilitatorPSR == facilitator.Id || m.Client.IdFacilitatorGroup == facilitator.Id))).ToListAsync();
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
                                                                  .Where(m => (m.Clinic.Id == user_logged.Clinic.Id
                                                                            && m.Status == StatusType.Close
                                                                            && m.IdFacilitatorPSR == facilitator.Id
                                                                            && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.PSR).Count() > 0
                                                                            && m.DischargeList.Where(d => d.TypeService == ServiceType.PSR).Count() == 0))
                                                                 .ToListAsync();
                List<ClientEntity> clientListIND = await _context.Clients
                                                                 .Where(m => (m.Clinic.Id == user_logged.Clinic.Id
                                                                           && m.Status == StatusType.Close
                                                                           && m.IndividualTherapyFacilitator.Id == facilitator.Id
                                                                           && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.Individual).Count() > 0
                                                                           && m.DischargeList.Where(d => d.TypeService == ServiceType.Individual).Count() == 0))
                                                                 .ToListAsync();
                List<ClientEntity> clientListGroup = await _context.Clients
                                                                  .Where(m => (m.Clinic.Id == user_logged.Clinic.Id
                                                                             && m.Status == StatusType.Close
                                                                             && m.IdFacilitatorGroup == facilitator.Id
                                                                             && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.Group).Count() > 0
                                                                             && m.DischargeList.Where(d => d.TypeService == ServiceType.Group).Count() == 0))
                                                                   .ToListAsync();

                ViewBag.ClientDischarge = (clientListPSR.Count() + clientListIND.Count() + +clientListGroup.Count()).ToString();

                ViewBag.DischargeEdition = _context.Discharge
                                                   .Count(n => (n.Status == DischargeStatus.Edition
                                                             && n.Client.Clinic.Id == user_logged.Clinic.Id
                                                             && n.CreatedBy == user_logged.UserName)).ToString();
                
                ViewBag.DischargePending = _context.Discharge
                                                   .Count(n => (n.Status == DischargeStatus.Pending
                                                             && n.Client.Clinic.Id == user_logged.Clinic.Id
                                                             && n.CreatedBy == user_logged.UserName)).ToString();

                ViewBag.MTPReviewEdition =  _context.MTPReviews                                                               
                                                    .Count(m => (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                              && m.Status == AdendumStatus.Edition
                                                              && m.CreatedBy == user_logged.UserName)).ToString();                
                ViewBag.MTPReviewPending = _context.MTPReviews
                                                   .Count(m => (m.Mtp.Client.Clinic.Id == user_logged.Clinic.Id
                                                             && m.Status == AdendumStatus.Pending
                                                             && m.CreatedBy == user_logged.UserName)).ToString();

                ViewBag.ClientWithoutFARS = _context.Clients
                                                    
                                                    .Count(n => n.Clinic.Id == user_logged.Clinic.Id
                                                       && (((n.IdFacilitatorPSR == facilitator.Id || n.IdFacilitatorGroup == facilitator.Id) && n.MTPs.FirstOrDefault(m => m.Active == true).MtpReviewList.Where(r => r.CreatedBy == user_logged.UserName).Count() > 0 && n.FarsFormList.Where(f => f.Type == FARSType.MtpReview).Count() == 0)
                                                            || (n.DischargeList.Where(d => d.TypeService == ServiceType.PSR && d.CreatedBy == user_logged.UserName).Count() > 0 && n.FarsFormList.Where(f => f.Type == FARSType.Discharge_PSR).Count() == 0)
                                                            || (n.DischargeList.Where(d => d.TypeService == ServiceType.Group && d.CreatedBy == user_logged.UserName).Count() > 0 && n.FarsFormList.Where(f => f.Type == FARSType.Discharge_Group).Count() == 0)
                                                            || (n.DischargeList.Where(d => d.TypeService == ServiceType.Individual && d.CreatedBy == user_logged.UserName).Count() > 0 && n.FarsFormList.Where(f => f.Type == FARSType.Discharge_Ind).Count() == 0)
                                                            || (n.MTPs.Where(m => m.AdendumList.Where(a => a.CreatedBy == user_logged.UserName).Count() > n.FarsFormList.Where(f => f.Type == FARSType.Addendums && f.CreatedBy == user_logged.UserName).Count()).Count() > 0))
                                                       && n.OnlyTCM == false).ToString();

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
                                                               && (wc.GroupNote.Status == NoteStatus.Pending
                                                               || wc.GroupNote2.Status == NoteStatus.Pending)
                                                               && wc.Workday.Service == ServiceType.Group)).ToString();

                ViewBag.ClientWithoutBIO = _context.Clients
                                                    .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                               && (wc.Bio == null && wc.Brief == null)
                                                               && wc.OnlyTCM == false)).ToString();

                ViewBag.PendingInitialFars = _context.Clients
                                                     .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                               && wc.FarsFormList.Count == 0
                                                               && wc.OnlyTCM == false)).ToString();

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
                                                          .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                                    && c.OnlyTCM == false)).ToListAsync();
                client = client.Where(wc => wc.MTPs.Count == 0).ToList();
                ViewBag.MTPMissing = client.Count.ToString();

                List<Workday_Client> notes_review_list = await _context.Workdays_Clients
                                                                       .Include(wc => wc.Messages)
                                                                       .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                               && (wc.Note.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending)
                                                                               && wc.Workday.Service == ServiceType.PSR)).ToListAsync();
                notes_review_list = notes_review_list.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();

                ViewBag.NotesWithReview = notes_review_list.Count.ToString();

                notes_review_list = await _context.Workdays_Clients
                                                  .Include(wc => wc.Messages)
                                                  .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                             && wc.IndividualNote.Status == NoteStatus.Pending
                                                             && wc.Workday.Service == ServiceType.Individual)).ToListAsync();
                notes_review_list = notes_review_list.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                ViewBag.IndNotesWithReview = notes_review_list.Count.ToString();

                notes_review_list = await _context.Workdays_Clients
                                                  .Include(wc => wc.Messages)
                                                  .Where(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                             && (wc.GroupNote.Status == NoteStatus.Pending
                                                             || wc.GroupNote2.Status == NoteStatus.Pending)
                                                             && wc.Workday.Service == ServiceType.Group)).ToListAsync();
                notes_review_list = notes_review_list.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                ViewBag.GroupNotesWithReview = notes_review_list.Count.ToString();

                ViewBag.MedicalHistoryMissing = _context.Clients
                                                        .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                              && wc.IntakeMedicalHistory == null
                                                              && wc.OnlyTCM == false)).ToString();

                ViewBag.PendingMtp = _context.MTPs
                                             .Count(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                  && m.Status == MTPStatus.Pending)).ToString();

                ViewBag.PendingBio = _context.Bio
                                             .Count(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                  && m.Status == BioStatus.Pending)).ToString();

                ViewBag.PendingBrief = _context.Brief
                                               .Count(m => (m.Client.Clinic.Id == user_logged.Clinic.Id
                                                 && m.Status == BioStatus.Pending)).ToString();
            }
            if (User.IsInRole("Manager"))
            {
                UserEntity user_logged = await _context.Users

                                                       .Include(u => u.Clinic)
                                                       .ThenInclude(c => c.Setting)

                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                
                if (user_logged.Clinic.Setting.MentalHealthClinic)
                {
                    ViewBag.PendingNotes = await _context.Workdays_Clients
                                                     .CountAsync(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                    && (wc.Note.Status == NoteStatus.Pending || wc.IndividualNote.Status == NoteStatus.Pending
                                                                     || wc.GroupNote.Status == NoteStatus.Pending
                                                                     || wc.NoteP.Status == NoteStatus.Pending)));

                    ViewBag.InProgressNotes = await _context.Workdays_Clients
                                                            .CountAsync(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                           && (wc.Note.Status == NoteStatus.Edition || wc.IndividualNote.Status == NoteStatus.Edition
                                                                            || wc.GroupNote.Status == NoteStatus.Edition || wc.NoteP.Status == NoteStatus.Edition)));

                    ViewBag.NotStartedNotes = await _context.Workdays_Clients
                                                            .CountAsync(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id
                                                                           && wc.Note == null && wc.IndividualNote == null
                                                                           && wc.GroupNote == null && wc.NoteP == null && wc.Present == true));

                    ViewBag.MTPMissing = await _context.Clients
                                                       .CountAsync(c => (c.Clinic.Id == user_logged.Clinic.Id && c.OnlyTCM == false
                                                                      && c.MTPs.Count == 0));

                    ViewBag.NotesWithReview = await _context.Workdays_Clients
                                                            .CountAsync(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id &&
                                                                              (wc.Note.Status == NoteStatus.Pending || wc.IndividualNote.Status == NoteStatus.Pending ||
                                                                               wc.GroupNote.Status == NoteStatus.Pending || wc.NoteP.Status == NoteStatus.Pending) &&
                                                                               wc.Messages.Count() > 0));

                    ViewBag.AllDocumentsForClient = await this.AmountOfDocumentsCMH(user_logged.Clinic.Id);

                    ViewBag.ApprovedNotes = await _context.Workdays_Clients
                                                          .CountAsync(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id &&
                                                                            (wc.Note.Status == NoteStatus.Approved || wc.IndividualNote.Status == NoteStatus.Approved ||
                                                                             wc.GroupNote.Status == NoteStatus.Approved || wc.NoteP.Status == NoteStatus.Approved)));

                    ViewBag.NotPresentNotes = await _context.Workdays_Clients
                                                            .CountAsync(wc => (wc.Facilitator.Clinic.Id == user_logged.Clinic.Id &&
                                                                               wc.Present == false));

                    List<MTPEntity> mtps = await _context.MTPs
                                                         .Include(n => n.MtpReviewList)
                                                         .Where(m => (m.Client.Clinic.Id == user_logged.Clinic.Id &&
                                                                      m.Active == true &&
                                                                      m.Client.Status == StatusType.Open)).ToListAsync();
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
                            if (DateTime.Now > item.MTPDevelopedDate.Date.AddMonths(Convert.ToInt32(item.NumberOfMonths + month)))
                            {
                                count++;
                            }
                        }
                        month = 0;
                    }

                    ViewBag.ExpiredMTPs = count;

                    ViewBag.PendingBIO = _context.Clients
                                            .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                       && (wc.Bio == null && wc.Brief == null)
                                                       && wc.OnlyTCM == false)).ToString();

                    ViewBag.PendingInitialFars = _context.Clients
                                                         .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                                    && wc.FarsFormList.Count == 0
                                                                    && wc.OnlyTCM == false)).ToString();

                    ViewBag.MedicalHistoryMissing = _context.Clients
                                                            .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                                       && wc.IntakeMedicalHistory == null
                                                                       && wc.OnlyTCM == false)).ToString();

                    ViewBag.IntakeMissing = _context.Clients
                                                            .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                                       && wc.IntakeScreening == null
                                                                       && wc.IntakeConsentForTreatment == null 
                                                                       && wc.IntakeConsentForRelease == null
                                                                       && wc.IntakeConsumerRights == null 
                                                                       && wc.IntakeAcknowledgementHipa == null 
                                                                       && wc.IntakeAccessToServices == null
                                                                       && wc.IntakeOrientationChecklist == null
                                                                       && wc.IntakeTransportation == null
                                                                       && wc.IntakeConsentPhotograph == null 
                                                                       && wc.IntakeFeeAgreement == null 
                                                                       && wc.IntakeTuberculosis == null
                                                                       && wc.OnlyTCM == false)).ToString();

                    ViewBag.FarsMissing = _context.Clients
                                                  .Count(n => n.Clinic.Id == user_logged.Clinic.Id
                                                                 && ((n.MTPs.FirstOrDefault(m => m.Active == true).MtpReviewList.Count() > 0 && n.FarsFormList.Where(f => f.Type == FARSType.MtpReview).Count() == 0)
                                                                  || (n.DischargeList.Where(d => d.TypeService == ServiceType.PSR).Count() > 0 && n.FarsFormList.Where(f => f.Type == FARSType.Discharge_PSR).Count() == 0)
                                                                  || (n.DischargeList.Where(d => d.TypeService == ServiceType.Individual).Count() > 0 && n.FarsFormList.Where(f => f.Type == FARSType.Discharge_Ind).Count() == 0)
                                                                  || (n.DischargeList.Where(d => d.TypeService == ServiceType.Group).Count() > 0 && n.FarsFormList.Where(f => f.Type == FARSType.Discharge_Group).Count() == 0)
                                                                  || (n.MTPs.Where(m => m.AdendumList.Count() > 0).Count() > n.FarsFormList.Where(f => f.Type == FARSType.Addendums).Count()))
                                                                 && n.OnlyTCM == false)
                                                  .ToString();

                    List<ClientEntity> clientListPSR = await _context.Clients
                                                                 .Where(m => (m.Clinic.Id == user_logged.Clinic.Id
                                                                           && m.Status == StatusType.Close
                                                                           && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.PSR).Count() > 0
                                                                           && m.DischargeList.Where(d => d.TypeService == ServiceType.PSR).Count() == 0))
                                                                .ToListAsync();
                    List<ClientEntity> clientListIND = await _context.Clients
                                                                     .Where(m => (m.Clinic.Id == user_logged.Clinic.Id
                                                                               && m.Status == StatusType.Close
                                                                               && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.Individual).Count() > 0
                                                                               && m.DischargeList.Where(d => d.TypeService == ServiceType.Individual).Count() == 0))
                                                                     .ToListAsync();
                    List<ClientEntity> clientListGroup = await _context.Clients
                                                                      .Where(m => (m.Clinic.Id == user_logged.Clinic.Id
                                                                                 && m.Status == StatusType.Close
                                                                                 && m.Workdays_Clients.Where(w => w.Workday.Service == ServiceType.Group).Count() > 0
                                                                                 && m.DischargeList.Where(d => d.TypeService == ServiceType.Group).Count() == 0))
                                                                       .ToListAsync();

                    ViewBag.ClientDischarge = (clientListPSR.Count() + clientListIND.Count() + +clientListGroup.Count()).ToString();

                    ViewBag.ClientAuthorization = _context.Clients
                                                          .Where(n => n.Clients_HealthInsurances == null 
                                                                   || n.Clients_HealthInsurances.Where(m => m.Active == true
                                                                            && m.ApprovedDate.AddMonths(m.DurationTime) > DateTime.Today.AddDays(15)).Count() == 0)
                                                          .Count()
                                                          .ToString();
                }          

                //TCM Dashboard
                if (user_logged.Clinic.Setting.TCMClinic)
                {
                    ViewBag.NotStartedCases = await _context.TCMClient
                                                            .CountAsync(s => (s.Client.Clinic.Id == user_logged.Clinic.Id
                                                                           && s.Status == StatusType.Open
                                                                           && (s.TcmServicePlan == null
                                                                                || s.TcmServicePlan.Approved != 2
                                                                                || s.TCMAssessment == null
                                                                                || s.TCMAssessment.Approved != 2
                                                                                || s.TcmIntakeAppendixJ == null
                                                                                || s.TcmIntakeAcknowledgementHipa == null
                                                                                || s.TCMIntakeAdvancedDirective == null
                                                                                || s.TcmIntakeConsentForRelease == null
                                                                                || s.TcmIntakeConsentForTreatment == null
                                                                                || s.TcmIntakeConsumerRights == null
                                                                                || s.TCMIntakeCoordinationCare == null
                                                                                || s.TCMIntakeForeignLanguage == null
                                                                                || s.TCMIntakeForm == null
                                                                                || s.TCMIntakeOrientationChecklist == null
                                                                                || s.TCMIntakeWelcome == null)));
                    
                    ViewBag.OpenBinder = await _context.TCMClient
                                                       .CountAsync(g => (g.Status == StatusType.Open
                                                                      && g.Client.Clinic.Id == user_logged.Clinic.Id));

                    //ViewBag.CloseCases = await _context.TCMClient
                    //                                   .CountAsync(g => (g.Status == StatusType.Close
                    //                                                  && g.Client.Clinic.Id == user_logged.Clinic.Id));

                    //ViewBag.Billing = _context.TCMClient
                    //                          .Where(g => (g.Status == StatusType.Open
                    //                             && g.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                    //ViewBag.ServicePlanEdition = await _context.TCMClient
                    //                                           .CountAsync(g => (g.Status == StatusType.Open
                    //                                                          && g.Client.Clinic.Id == user_logged.Clinic.Id
                    //                                                          && g.TcmServicePlan.Approved == 0));

                    ViewBag.ServicePlanPending = await _context.TCMClient                                                         
                                                               .CountAsync(g => (g.Status == StatusType.Open
                                                                              && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                              && g.TcmServicePlan.Approved == 1));

                    //ViewBag.AdendumEdition = _context.TCMClient
                    //                                 .Include(n => n.TcmServicePlan)
                    //                                 .ThenInclude(n => n.TCMAdendum)
                    //                                 .Where(g => (g.Status == StatusType.Open
                    //                                    && g.Client.Clinic.Id == user_logged.Clinic.Id
                    //                                    && g.TcmServicePlan.TCMAdendum.Where(n => n.Approved == 0).Count() > 0)).Count().ToString();

                    ViewBag.AdendumPending = await _context.TCMClient                                                     
                                                           .CountAsync(g => (g.Status == StatusType.Open
                                                                          && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                          && g.TcmServicePlan.TCMAdendum.Where(n => n.Approved == 1).Count() > 0));

                    //ViewBag.ServicePlanReviewEdition = _context.TCMClient
                    //                                           .Include(n => n.TcmServicePlan)
                    //                                           .ThenInclude(n => n.TCMServicePlanReview)
                    //                                           .Where(g => (g.Status == StatusType.Open
                    //                                                && g.Client.Clinic.Id == user_logged.Clinic.Id
                    //                                                && g.TcmServicePlan.TCMServicePlanReview.Approved == 0)).Count().ToString();

                    ViewBag.ServicePlanReviewPending = await _context.TCMClient                                                               
                                                                     .CountAsync(g => (g.Status == StatusType.Open
                                                                                    && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                                    && g.TcmServicePlan.TCMServicePlanReview.Approved == 1));

                    //ViewBag.DischargeEdition = _context.TCMClient
                    //                                   .Include(n => n.TcmServicePlan)
                    //                                   .ThenInclude(n => n.TCMDischarge)
                    //                                   .Where(g => (g.Status == StatusType.Open
                    //                                       && g.Client.Clinic.Id == user_logged.Clinic.Id
                    //                                       && g.TcmServicePlan.TCMDischarge.Approved == 0)).Count().ToString();

                    ViewBag.DischargePending = await _context.TCMClient                                                       
                                                             .CountAsync(g => (g.Status == StatusType.Open
                                                                            && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                            && g.TcmServicePlan.TCMDischarge.Approved == 1));

                    ViewBag.TCMCaseManager = await _context.CaseManagers
                                                           .CountAsync(g => (g.Status == StatusType.Open
                                                                          && g.Clinic.Id == user_logged.Clinic.Id));

                    ViewBag.TCMFarsPending = await _context.TCMFarsForm
                                                           .CountAsync(g => (g.Status == FarsStatus.Pending
                                                                          && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id));

                    ViewBag.AssesmentPending = await _context.TCMAssessment
                                                       .CountAsync(g => (g.Approved == 1
                                                                      && g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id));

                    ViewBag.AllDocuments = await _context.TCMClient
                                                         .CountAsync(g => g.Client.Clinic.Id == user_logged.Clinic.Id);

                    ViewBag.TCMNotesEdition = await _context.TCMNote
                                                            .CountAsync(g => (g.Status == NoteStatus.Edition
                                                                           && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id));

                    ViewBag.TCMNotesPending = await _context.TCMNote
                                                            .CountAsync(g => (g.Status == NoteStatus.Pending
                                                                           && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id));

                    ViewBag.TCMNotesApproved = await _context.TCMNote
                                                             .CountAsync(g => (g.Status == NoteStatus.Approved
                                                                            && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id));

                    ViewBag.TCMNotesWithReview = await _context.TCMNote
                                                               .CountAsync(wc => (wc.TCMClient.Casemanager.Clinic.Id == user_logged.Clinic.Id &&
                                                                                  wc.Status == NoteStatus.Pending &&
                                                                                  wc.TCMMessages.Count() > 0));                                                                             
                }
            }
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Index), "Incidents");
            }
            if (User.IsInRole("CaseManager"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                CaseMannagerEntity caseManager = await _context.CaseManagers.FirstOrDefaultAsync(c => c.LinkedUser == user_logged.UserName);

                List<TCMClientEntity> tcmClient = await _context.TCMClient
                                                                .Include(g => g.Casemanager)
                                                                .Include(g => g.Client)
                                                                .Include(g => g.TcmServicePlan)
                                                                .Include(g => g.TcmIntakeAppendixJ)
                                                                .Include(g => g.TCMAssessment)
                                                                .Include(g => g.TcmIntakeAcknowledgementHipa)
                                                                .Include(g => g.TCMIntakeAdvancedDirective)
                                                                .Include(g => g.TcmIntakeConsentForRelease)
                                                                .Include(g => g.TcmIntakeConsentForTreatment)
                                                                .Include(g => g.TcmIntakeConsumerRights)
                                                                .Include(g => g.TCMIntakeForeignLanguage)
                                                                .Include(g => g.TCMIntakeForm)
                                                                .Include(g => g.TCMIntakeOrientationChecklist)
                                                                .Include(g => g.TCMIntakeWelcome)
                                                                .Where(g => (g.Casemanager.Id == caseManager.Id
                                                                        && g.Status == StatusType.Open
                                                                        && (g.TcmServicePlan == null
                                                                            || g.TcmServicePlan.Approved != 2
                                                                            || g.TCMAssessment == null
                                                                            || g.TCMAssessment.Approved != 2
                                                                            || g.TcmIntakeAppendixJ == null
                                                                            || g.TcmIntakeAcknowledgementHipa == null
                                                                            || g.TCMIntakeAdvancedDirective == null
                                                                            || g.TcmIntakeConsentForRelease == null
                                                                            || g.TcmIntakeConsentForTreatment == null
                                                                            || g.TcmIntakeConsumerRights == null
                                                                            || g.TCMIntakeCoordinationCare == null
                                                                            || g.TCMIntakeForeignLanguage == null
                                                                            || g.TCMIntakeForm == null
                                                                            || g.TCMIntakeOrientationChecklist == null
                                                                            || g.TCMIntakeWelcome == null)))
                                                               .ToListAsync();

                ViewBag.NotStartedCases = tcmClient.Count.ToString();

                ViewBag.OpenBinder = _context.TCMClient
                                             .Where(g => (g.Casemanager.Id == caseManager.Id
                                                && g.Status == StatusType.Open
                                                && g.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.CloseCases = _context.TCMClient
                                             .Where(g => (g.Casemanager.Id == caseManager.Id
                                                && g.Status == StatusType.Close
                                                && g.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.Billing = _context.TCMClient
                                          .Where(g => (g.Casemanager.Id == caseManager.Id
                                             && g.Status == StatusType.Open
                                             && g.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.ServicePlanEdition = _context.TCMClient
                                                     .Include(n => n.TcmServicePlan)
                                                     .Where(g => (g.Casemanager.Id == caseManager.Id
                                                        && g.Status == StatusType.Open
                                                        && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                        && g.TcmServicePlan.Approved == 0)).Count().ToString();

                ViewBag.ServicePlanPending = _context.TCMClient
                                                     .Include(n => n.TcmServicePlan)
                                                     .Where(g => (g.Casemanager.Id == caseManager.Id
                                                        && g.Status == StatusType.Open
                                                        && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                        && g.TcmServicePlan.Approved == 1)).Count().ToString();

                ViewBag.AdendumEdition = _context.TCMAdendums
                                                 .Include(n => n.TcmServicePlan)
                                                 .Where(g => (g.Approved == 0
                                                    && g.TcmServicePlan.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count()
                                                 .ToString();

                ViewBag.AdendumPending = _context.TCMClient
                                                 .Include(n => n.TcmServicePlan)
                                                 .ThenInclude(n => n.TCMAdendum)
                                                 .Where(g => (g.Casemanager.Id == caseManager.Id
                                                    && g.Status == StatusType.Open
                                                    && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                    && g.TcmServicePlan.TCMAdendum.Where(n => n.Approved == 1).Count() > 0)).Count().ToString();

                ViewBag.ServicePlanReviewEdition = _context.TCMClient
                                                           .Include(n => n.TcmServicePlan)
                                                           .ThenInclude(n => n.TCMServicePlanReview)
                                                           .Where(g => (g.Casemanager.Id == caseManager.Id
                                                                && g.Status == StatusType.Open
                                                                && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                && g.TcmServicePlan.TCMServicePlanReview.Approved == 0)).Count().ToString();

                ViewBag.ServicePlanReviewPending = _context.TCMClient
                                                           .Include(n => n.TcmServicePlan)
                                                           .ThenInclude(n => n.TCMServicePlanReview)
                                                           .Where(g => (g.Casemanager.Id == caseManager.Id
                                                                && g.Status == StatusType.Open
                                                                && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                && g.TcmServicePlan.TCMServicePlanReview.Approved == 1)).Count().ToString();

                ViewBag.DischargeEdition = _context.TCMDischarge
                                                   .Where(g => (g.Approved == 0
                                                     && g.TcmServicePlan.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                     && g.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id)).Count().ToString();

                ViewBag.DischargePending = _context.TCMDischarge
                                                   .Where(g => (g.Approved == 1
                                                     && g.TcmServicePlan.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                     && g.TcmServicePlan.TcmClient.Casemanager.Id == caseManager.Id)).Count().ToString();

                ViewBag.NotesEdition = _context.TCMNote
                                               .Where(g => (g.TCMClient.Casemanager.Id == caseManager.Id
                                                      && g.Status == NoteStatus.Edition
                                                      && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.NotesPending = _context.TCMNote
                                               .Where(g => (g.TCMClient.Casemanager.Id == caseManager.Id
                                                     && g.Status == NoteStatus.Pending
                                                     && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.NotesApproved = _context.TCMNote
                                                .Where(g => (g.TCMClient.Casemanager.Id == caseManager.Id
                                                    && g.Status == NoteStatus.Approved
                                                    && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.AppendiceJPending = _context.TCMIntakeAppendixJ
                                                    .Where(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                         && g.Approved == 1
                                                         && g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.FarsPending = _context.TCMFarsForm
                                              .Where(g => (g.TCMClient.Casemanager.Id == caseManager.Id
                                                         && g.Status == FarsStatus.Pending
                                                         && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.AssesmentPending = _context.TCMAssessment
                                              .Where(g => (g.TcmClient.Casemanager.Id == caseManager.Id
                                                         && g.Approved == 1
                                                         && g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.AllDocuments = _context.TCMClient
                                               .Where(g => (g.Casemanager.Id == caseManager.Id
                                                         && g.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.ActivityPending = _context.TCMServiceActivity
                                                  .Where(g => (g.Approved < 2
                                                               && g.CreatedBy == user_logged.UserName
                                                               && g.TcmService.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.TCMNoteReview = _context.TCMNote
                                                .Include(wc => wc.TCMClient)
                                                .ThenInclude(wc => wc.Client)
                                                .Include(wc => wc.TCMClient.Casemanager)
                                                .Include(wc => wc.TCMMessages.Where(m => m.Notification == false))
                                                .Where(wc => (wc.TCMClient.Casemanager.LinkedUser == user_logged.UserName
                                                       && wc.Status == NoteStatus.Pending
                                                       && wc.TCMMessages.Count() > 0)).Count()
                                                .ToString();
            }
            if (User.IsInRole("Documents_Assistant"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

                List<ClientEntity> client = await _context.Clients
                                                          .Include(c => c.MTPs)
                                                          .Where(c => (c.Clinic.Id == user_logged.Clinic.Id
                                                                    && c.OnlyTCM == false)).ToListAsync();
                client = client.Where(wc => wc.MTPs.Count == 0).ToList();
                ViewBag.MTPMissing = client.Count.ToString();

                ViewBag.PendingBIO = _context.Clients
                                             .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                        && (wc.Bio == null && wc.Brief == null)
                                                        && wc.OnlyTCM == false)).ToString();

                ViewBag.PendingInitialFars = _context.Clients
                                                     .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                                && wc.FarsFormList.Count == 0
                                                                && wc.OnlyTCM == false)).ToString();

                ViewBag.MedicalHistoryMissing = _context.Clients
                                                        .Count(wc => (wc.Clinic.Id == user_logged.Clinic.Id
                                                                   && wc.IntakeMedicalHistory == null
                                                                   && wc.OnlyTCM == false)).ToString();

                List<MTPEntity> MtpPending = await _context.MTPs
                                                           .Include(f => f.Client)
                                                           .ThenInclude(n => n.Clinic)
                                                           .Where(n => (n.Status == MTPStatus.Pending
                                                                     && n.Client.Clinic.Id == user_logged.Clinic.Id
                                                                     && n.CreatedBy == user_logged.UserName))
                                                           .OrderBy(f => f.Client.Name)
                                                           .ToListAsync();
                ViewBag.MtpPending = MtpPending.Count().ToString();

                List<BioEntity> BioPending = await _context.Bio
                                                           .Include(f => f.Client)
                                                           .ThenInclude(n => n.Clinic)
                                                           .Where(n => (n.Status == BioStatus.Pending
                                                                     && n.Client.Clinic.Id == user_logged.Clinic.Id
                                                                     && n.CreatedBy == user_logged.UserName))
                                                           .OrderBy(f => f.Client.Name)
                                                           .ToListAsync();
                ViewBag.BioPending = BioPending.Count().ToString();

                List<BriefEntity> BriefPending = await _context.Brief
                                                               .Include(f => f.Client)
                                                               .ThenInclude(n => n.Clinic)
                                                               .Where(n => (n.Status == BioStatus.Pending
                                                                    && n.Client.Clinic.Id == user_logged.Clinic.Id
                                                                    && n.CreatedBy == user_logged.UserName))
                                                               .OrderBy(f => f.Client.Name)
                                                               .ToListAsync();
                ViewBag.BriefPending = BriefPending.Count().ToString();

                List<MTPEntity> MtpWithReview = await _context.MTPs
                                                      .Include(wc => wc.Messages)
                                                      .Where(wc => (wc.DocumentAssistant.LinkedUser == User.Identity.Name
                                                                 && wc.Status == MTPStatus.Pending)).ToListAsync();
                MtpWithReview = MtpWithReview.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                ViewBag.MtpWithReview = MtpWithReview.Count.ToString();

                List<BioEntity> BioWithReview = await _context.Bio
                                                              .Include(wc => wc.Messages)
                                                              .Where(wc => (wc.DocumentsAssistant.LinkedUser == User.Identity.Name
                                                                     && wc.Status == BioStatus.Pending)).ToListAsync();
                BioWithReview = BioWithReview.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                ViewBag.BioWithReview = BioWithReview.Count.ToString();

                List<BriefEntity> BriefWithReview = await _context.Brief
                                                                  .Include(wc => wc.Messages)
                                                                  .Where(wc => (wc.DocumentsAssistant.LinkedUser == User.Identity.Name
                                                                     && wc.Status == BioStatus.Pending)).ToListAsync();
                BriefWithReview = BriefWithReview.Where(wc => wc.Messages.Where(m => m.Notification == false).Count() > 0).ToList();
                ViewBag.BriefWithReview = BriefWithReview.Count.ToString();
            }
            if (User.IsInRole("TCMSupervisor"))
            {
                UserEntity user_logged = await _context.Users
                                                       .Include(u => u.Clinic)
                                                       .FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

              
                List<TCMClientEntity> tcmClient = await _context.TCMClient
                                                                 .Include(g => g.Casemanager)
                                                                 .Include(g => g.Client)
                                                                 .Include(g => g.TcmServicePlan)
                                                                 .Include(g => g.TcmIntakeAppendixJ)
                                                                 .Include(g => g.TCMAssessment)
                                                                 .Include(g => g.TcmIntakeAcknowledgementHipa)
                                                                 .Include(g => g.TCMIntakeAdvancedDirective)
                                                                 .Include(g => g.TcmIntakeConsentForRelease)
                                                                 .Include(g => g.TcmIntakeConsentForTreatment)
                                                                 .Include(g => g.TcmIntakeConsumerRights)
                                                                 .Include(g => g.TCMIntakeForeignLanguage)
                                                                 .Include(g => g.TCMIntakeForm)
                                                                 .Include(g => g.TCMIntakeOrientationChecklist)
                                                                 .Include(g => g.TCMIntakeWelcome)
                                                                 .Where(s => (s.Client.Clinic.Id == user_logged.Clinic.Id
                                                                    && s.Status == StatusType.Open
                                                                    && (s.TcmServicePlan == null
                                                                        || s.TcmServicePlan.Approved != 2
                                                                        || s.TCMAssessment == null
                                                                        || s.TCMAssessment.Approved != 2
                                                                        || s.TcmIntakeAppendixJ == null
                                                                        || s.TcmIntakeAcknowledgementHipa == null
                                                                        || s.TCMIntakeAdvancedDirective == null
                                                                        || s.TcmIntakeConsentForRelease == null
                                                                        || s.TcmIntakeConsentForTreatment == null
                                                                        || s.TcmIntakeConsumerRights == null
                                                                        || s.TCMIntakeCoordinationCare == null
                                                                        || s.TCMIntakeForeignLanguage == null
                                                                        || s.TCMIntakeForm == null
                                                                        || s.TCMIntakeOrientationChecklist == null
                                                                        || s.TCMIntakeWelcome == null)))
                                                                .OrderBy(g => g.Casemanager.Name)
                                                                .ToListAsync();

                ViewBag.NotStartedCases = tcmClient.Count.ToString();

                ViewBag.OpenBinder = _context.TCMClient
                                             .Where(g => (g.Status == StatusType.Open
                                                && g.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.ServicePlanPending = _context.TCMClient
                                                     .Include(n => n.TcmServicePlan)
                                                     .Where(g => ( g.Status == StatusType.Open
                                                        && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                        && g.TcmServicePlan.Approved == 1)).Count().ToString();

                ViewBag.AdendumPending = _context.TCMAdendums
                                                 .Include(n => n.TcmServicePlan)
                                                 .Where(g => (g.Approved == 1
                                                    && g.TcmServicePlan.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count()
                                                 .ToString();

                ViewBag.ServicePlanReviewEdition = _context.TCMClient
                                                           .Include(n => n.TcmServicePlan)
                                                           .ThenInclude(n => n.TCMServicePlanReview)
                                                           .Where(g => (g.Status == StatusType.Open
                                                                && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                && g.TcmServicePlan.TCMServicePlanReview.Approved == 0)).Count().ToString();

                ViewBag.ServicePlanReviewPending = _context.TCMClient
                                                           .Include(n => n.TcmServicePlan)
                                                           .ThenInclude(n => n.TCMServicePlanReview)
                                                           .Where(g => (g.Status == StatusType.Open
                                                                && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                                && g.TcmServicePlan.TCMServicePlanReview.Approved == 1)).Count().ToString();

                ViewBag.DischargePending = _context.TCMClient
                                                   .Include(n => n.TcmServicePlan)
                                                   .ThenInclude(n => n.TCMDischarge)
                                                   .Where(g => (g.Status == StatusType.Open
                                                       && g.Client.Clinic.Id == user_logged.Clinic.Id
                                                       && g.TcmServicePlan.TCMDischarge.Approved == 1)).Count().ToString();

                ViewBag.TCMFarsPending = _context.TCMFarsForm
                                                 .Include(m =>m.TCMClient)
                                                 .ThenInclude(m => m.Client)
                                                 .ThenInclude(m => m.Clinic)
                                                 .Where(g => (g.Status == FarsStatus.Pending
                                                      && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.NotesPending = _context.TCMNote
                                               .Where(g => (g.Status == NoteStatus.Pending
                                                     && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.NotesApproved = _context.TCMNote
                                                .Where(g => (g.Status == NoteStatus.Approved
                                                    && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.ActivityPending = _context.TCMServiceActivity
                                                  .Where(g => (g.Approved < 2
                                                               && g.TcmService.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.AppendiceJPending = _context.TCMIntakeAppendixJ
                                                   .Where(g => (g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id
                                                        && g.Approved == 1
                                                        && g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.FarsPending = _context.TCMFarsForm
                                              .Where(g => (g.TCMClient.Casemanager.Id == user_logged.Clinic.Id
                                                         && g.Status == FarsStatus.Pending
                                                         && g.TCMClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.AssesmentPending = _context.TCMAssessment
                                              .Where(g => (g.TcmClient.Casemanager.Id == user_logged.Clinic.Id
                                                         && g.Approved == 1
                                                         && g.TcmClient.Client.Clinic.Id == user_logged.Clinic.Id)).Count().ToString();

                ViewBag.TCMNoteReview =  _context.TCMNote
                                                 .Include(wc => wc.TCMClient)
                                                 .ThenInclude(wc => wc.Client)
                                                 .Include(wc => wc.TCMClient.Casemanager)
                                                 .Include(wc => wc.TCMMessages.Where(m => m.Notification == false))
                                                 .Where(wc => (wc.TCMClient.Casemanager.Clinic.Id == user_logged.Clinic.Id
                                                        && wc.Status == NoteStatus.Pending
                                                        && wc.TCMMessages.Count() > 0)).Count()
                                                 .ToString();
            }
            return View();
        }

        private async Task<int> AmountOfDocumentsCMH(int idClinic)
        {
            int total = 0;
            total += await _context.IntakeAcknowledgement
                                   .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
            total += await _context.IntakeConsentForRelease
                                   .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
            total += await _context.IntakeConsentForTreatment
                                   .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
            total += await _context.IntakeConsumerRights
                                   .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
            total += await _context.IntakeOrientationCheckList
                                   .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));            
            total += await _context.IntakeAccessToServices
                                   .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
            total += await _context.IntakeConsentPhotograph
                                   .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
            total += await _context.IntakeFeeAgreement
                                   .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
            total += await _context.IntakeMedicalHistory
                                   .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
            total += await _context.IntakeScreenings
                                   .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
            total += await _context.IntakeTransportation
                                   .CountAsync(i => (i.Client.Clinic.Id == idClinic && i.Client.OnlyTCM == false));
            total += await _context.Bio
                                   .CountAsync(b => (b.Client.Clinic.Id == idClinic && b.Client.OnlyTCM == false));
            total += await _context.MTPs
                                   .CountAsync(m => (m.Client.Clinic.Id == idClinic && m.Client.OnlyTCM == false));
            total += await _context.MTPReviews
                                   .CountAsync(m => (m.Mtp.Client.Clinic.Id == idClinic && m.Mtp.Client.OnlyTCM == false));
            total += await _context.Adendums
                                   .CountAsync(a => (a.Mtp.Client.Clinic.Id == idClinic && a.Mtp.Client.OnlyTCM == false));
            total += await _context.Discharge
                                   .CountAsync(d => (d.Client.Clinic.Id == idClinic && d.Client.OnlyTCM == false));
            total += await _context.FarsForm
                                   .CountAsync(f => (f.Client.Clinic.Id == idClinic && f.Client.OnlyTCM == false));
            return total;
        }
    }
}