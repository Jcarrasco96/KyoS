using KyoS.Api.Models.Records;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KyoS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillsController : Controller
{
    private readonly DataContext _context;

    public BillsController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("{idStatus}")]
    public async Task<IActionResult> NotBilled(int idStatus)    //  0 - Edition, 1 - Pending, 2 - Approved, 3 - all status, 4 - not started
    {
        List<Workday_Client> workday_Client = null!;
        if (idStatus == 0 || idStatus == 1 || idStatus == 2)
        {
            workday_Client = await _context.Workdays_Clients

                                            .Include(wc => wc.Facilitator)
                                            .Include(c => c.Client)
                                            .Include(w => w.Workday)

                                            .Include(wc => wc.Client)
                                            .ThenInclude(w => w.Clients_Diagnostics)
                                            .ThenInclude(w => w.Diagnostic)

                                            .Include(wc => wc.Client)
                                            .ThenInclude(w => w.Clients_HealthInsurances)
                                            .ThenInclude(w => w.HealthInsurance)

                                            .Include(wc => wc.Note)

                                            .Include(wc => wc.NoteP)
                                            .ThenInclude(w => w.NotesP_Activities)

                                            .Include(wc => wc.IndividualNote)

                                            .Include(wc => wc.GroupNote)
                                            .ThenInclude(w => w.GroupNotes_Activities)

                                            .Include(wc => wc.GroupNote2)
                                            .ThenInclude(w => w.GroupNotes2_Activities)

                                            .AsSplitQuery()

                                            .Where(wc => wc.Present == true
                                                        && wc.Client != null
                                                        && wc.BilledDate == null
                                                        && (((int)wc.Note.Status) == idStatus || (int)wc.NoteP.Status == idStatus
                                                                                    || (int)wc.IndividualNote.Status == idStatus
                                                                                    || (int)wc.GroupNote.Status == idStatus
                                                                                    || (int)wc.GroupNote2.Status == idStatus))
                                            .OrderBy(wc => wc.Client.Name)
                                            .ThenBy(wc => wc.Workday.Date)
                                            .ToListAsync();
        }
        else
        {
            if (idStatus == 3)
            {
                workday_Client = await _context.Workdays_Clients

                                            .Include(wc => wc.Facilitator)
                                            .Include(c => c.Client)
                                            .Include(w => w.Workday)

                                            .Include(wc => wc.Client)
                                            .ThenInclude(w => w.Clients_Diagnostics)
                                            .ThenInclude(w => w.Diagnostic)

                                            .Include(wc => wc.Client)
                                            .ThenInclude(w => w.Clients_HealthInsurances)
                                            .ThenInclude(w => w.HealthInsurance)

                                            .Include(wc => wc.Note)

                                            .Include(wc => wc.NoteP)
                                            .ThenInclude(w => w.NotesP_Activities)

                                            .Include(wc => wc.IndividualNote)

                                            .Include(wc => wc.GroupNote)
                                            .ThenInclude(w => w.GroupNotes_Activities)

                                            .Include(wc => wc.GroupNote2)
                                            .ThenInclude(w => w.GroupNotes2_Activities)

                                            .AsSplitQuery()

                                            .Where(wc => wc.Present == true
                                                        && wc.Client != null
                                                        && wc.BilledDate == null)
                                            .OrderBy(wc => wc.Client.Name)
                                            .ThenBy(wc => wc.Workday.Date)
                                            .ToListAsync();
            }
            else
            {
                if (idStatus == 4)
                {
                    workday_Client = await _context.Workdays_Clients

                                            .Include(wc => wc.Facilitator)
                                            .Include(c => c.Client)
                                            .Include(w => w.Workday)

                                            .Include(wc => wc.Client)
                                            .ThenInclude(w => w.Clients_Diagnostics)
                                            .ThenInclude(w => w.Diagnostic)

                                            .Include(wc => wc.Client)
                                            .ThenInclude(w => w.Clients_HealthInsurances)
                                            .ThenInclude(w => w.HealthInsurance)

                                            .Include(wc => wc.Note)

                                            .Include(wc => wc.NoteP)
                                            .ThenInclude(w => w.NotesP_Activities)

                                            .Include(wc => wc.IndividualNote)

                                            .Include(wc => wc.GroupNote)
                                            .ThenInclude(w => w.GroupNotes_Activities)

                                            .Include(wc => wc.GroupNote2)
                                            .ThenInclude(w => w.GroupNotes2_Activities)

                                            .AsSplitQuery()

                                            .Where(wc => wc.Present == true
                                                        && wc.Client != null
                                                        && wc.BilledDate == null
                                                        && (wc.Note == null && wc.NoteP == null && wc.IndividualNote == null
                                                                             && wc.GroupNote == null && wc.GroupNote2 == null))
                                            .OrderBy(wc => wc.Client.Name)
                                            .ThenBy(wc => wc.Workday.Date)
                                            .ToListAsync();
                }
            }
        }

        List<NotBilled> notBilleds = new List<NotBilled>();
        string insuranceMemberId = string.Empty;
        string service;
        string setting;
        int units;
        int amount;
        foreach (var item in workday_Client)
        {
            insuranceMemberId = "-";
            if (item.Client.Clients_HealthInsurances.Where(n => n.Active == true).Count() > 0)
            {
                insuranceMemberId = $"{item.Client.Clients_HealthInsurances.First(n => n.Active == true).HealthInsurance.Name} | {item.Client.Clients_HealthInsurances.First(n => n.Active == true).MemberId}";
            }

            if (item.Note != null)
            {
                service = "PSR";
                setting = item.Note.Setting;
                units = 16;
                amount = 16 * 9;
            }
            else
            {
                if (item.NoteP != null)
                {
                    service = "PSR";
                    setting = item.NoteP.Setting;
                    units = item.NoteP.RealUnits;
                    amount = item.NoteP.RealUnits * 9;
                }
                else
                {
                    if (item.IndividualNote != null)
                    {
                        service = "Ind.";
                        setting = "";
                        units = 4;
                        amount = 4 * 12;
                    }
                    else
                    {
                        if (item.GroupNote != null)
                        {
                            service = "Group";
                            setting = "";
                            units = 8;
                            amount = 8 * 7;
                        }
                        else
                        {
                            if (item.GroupNote2 != null)
                            {
                                service = "Group";
                                setting = "";
                                units = item.GroupNote2.GroupNotes2_Activities.Count();
                                amount = item.GroupNote2.GroupNotes2_Activities.Count() * 7;
                            }
                            else
                            {
                                service = "PSR";
                                setting = "53";
                                units = 16;
                                amount = 16 * 9;
                            }
                        }
                    }
                }

            }

            NotBilled value = new NotBilled(item.Client.Name, item.Client.Code, item.Client.DateOfBirth.ToShortDateString(), item.Client.MedicaidID,
                                            insuranceMemberId, (item.Client.Clients_Diagnostics.Count() > 0) ? item.Client.Clients_Diagnostics.ElementAt(0).Diagnostic.Code : "-",
                                            item.Workday.Date.ToShortDateString(), service, setting, units, amount, item.Facilitator.Name, "Not Billed");

            notBilleds.Add(value);
        }

        return Ok(notBilleds);
    }
}

