using KyoS.Web.Data.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class WeekEntity : AuditableEntity
    {
        public int Id { get; set; }
        [DataType(DataType.Date)]        
        public DateTime InitDate { get; set; }
        [DataType(DataType.Date)]        
        public DateTime FinalDate { get; set; }
        public IEnumerable<WorkdayEntity> Days { get; set; }
        public ClinicEntity Clinic { get; set; }
        public int WeekOfYear { get; set; }
        public string Alias
        {
            get
            {
                var month = (InitDate.Month < 10) ? $"0{InitDate.Month}" : InitDate.Month.ToString();
                var initday = (InitDate.Day < 10) ? $"0{InitDate.Day}" : InitDate.Day.ToString();
                var finalday = (FinalDate.Day < 10) ? $"0{FinalDate.Day}" : FinalDate.Day.ToString();

                return $"{month}_{initday}{finalday}_{InitDate.Year}";
            }
        }
        public int Units 
        {
            get
            {
                int units = 0;
                if (this.Days != null)
                {
                    foreach (var item in this.Days)
                    {
                        foreach (var workday_client in item.Workdays_Clients.Where(wc => wc.Present == true && wc.Hold == false && wc.Client != null))
                        {
                            if(workday_client.Note != null)
                            {
                                if ((workday_client.Note.Schema == Common.Enums.SchemaType.Schema1) || (workday_client.Note.Schema == Common.Enums.SchemaType.Schema2))
                                {
                                    units = units + 16;
                                }
                                if ((workday_client.Note.Schema == Common.Enums.SchemaType.Schema4))
                                {
                                    units = units + 12;
                                }
                            }
                            else
                            {
                                if (workday_client.NoteP != null)
                                {
                                    if ((workday_client.NoteP.Schema == Common.Enums.SchemaType.Schema3))
                                    {
                                        units = units + workday_client.NoteP.RealUnits;
                                    }
                                }
                                else
                                { 
                                    if ((workday_client.IndividualNote != null) || (workday_client.Workday.Service == Common.Enums.ServiceType.Individual))
                                    {
                                        units = units + 4;
                                    }
                                    else
                                    {
                                        if (workday_client.Workday.Service == Common.Enums.ServiceType.Group)
                                        {
                                            if (workday_client.GroupNote2 != null)
                                            {
                                                units = units + workday_client.GroupNote2.GroupNotes2_Activities.Count() * 4;
                                            }
                                            else
                                            {
                                                units = units + workday_client.Schedule.SubSchedules.Count() * 4;
                                            }
                                        }
                                        else
                                        { 
                                            if ((workday_client.Workday.Week.Clinic.Schema == Common.Enums.SchemaType.Schema1) || (workday_client.Workday.Week.Clinic.Schema == Common.Enums.SchemaType.Schema2) || (workday_client.Workday.Week.Clinic.Schema == Common.Enums.SchemaType.Schema3))
                                            {
                                                units = units + 16;
                                            }
                                            if ((workday_client.Workday.Week.Clinic.Schema == Common.Enums.SchemaType.Schema4))
                                            {
                                                units = units + 12;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        
                    }
                }
                return units;
            }
        }
        public int Units_Hold
        {
            get
            {
                int units = 0;
                if (this.Days != null)
                {
                    foreach (var item in this.Days)
                    {
                        foreach (var workday_client in item.Workdays_Clients.Where(wc => wc.Present == true && wc.Hold == true && wc.Client != null))
                        {
                            if (workday_client.Note != null)
                            {
                                if ((workday_client.Note.Schema == Common.Enums.SchemaType.Schema1) || (workday_client.Note.Schema == Common.Enums.SchemaType.Schema2))
                                {
                                    units = units + 16;
                                }
                                if ((workday_client.Note.Schema == Common.Enums.SchemaType.Schema4))
                                {
                                    units = units + 12;
                                }
                            }
                            else
                            {
                                if (workday_client.NoteP != null)
                                {
                                    if ((workday_client.NoteP.Schema == Common.Enums.SchemaType.Schema3))
                                    {
                                        units = units + workday_client.NoteP.RealUnits;
                                    }
                                }
                                else
                                {
                                    if ((workday_client.IndividualNote != null) || (workday_client.Workday.Service == Common.Enums.ServiceType.Individual))
                                    {
                                        units = units + 4;
                                    }
                                    else
                                    {
                                        if ((workday_client.GroupNote != null) || (workday_client.Workday.Service == Common.Enums.ServiceType.Group))
                                        {
                                            units = units + 8;
                                        }
                                        else
                                        {
                                            if ((workday_client.Workday.Week.Clinic.Schema == Common.Enums.SchemaType.Schema1) || (workday_client.Workday.Week.Clinic.Schema == Common.Enums.SchemaType.Schema2) || (workday_client.Workday.Week.Clinic.Schema == Common.Enums.SchemaType.Schema3))
                                            {
                                                units = units + 16;
                                            }
                                            if ((workday_client.Workday.Week.Clinic.Schema == Common.Enums.SchemaType.Schema4))
                                            {
                                                units = units + 12;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
                return units;
            }
        }
        public int Notes
        {
            get
            {
                int count = 0;
                if(this.Days != null)
                { 
                    foreach (var item in this.Days)
                    {
                        count += item.Workdays_Clients.Where(wc => wc.Present == true && wc.Hold == false && wc.Client != null).Count();
                    }
                }
                return count;
            }
        }
        public int Notes_Hold
        {
            get
            {
                int count = 0;
                if (this.Days != null)
                {
                    foreach (var item in this.Days)
                    {
                        count += item.Workdays_Clients.Where(wc => wc.Present == true && wc.Hold == true).Count();
                    }
                }
                return count;
            }
        }
        public string MonthYear
        {
            get
            {
                try
                {
                    var month = (InitDate.Month == 1) ? "January" : (InitDate.Month == 2) ? "February" : (InitDate.Month == 3) ? "March" :
                            (InitDate.Month == 4) ? "April" : (InitDate.Month == 5) ? "May" : (InitDate.Month == 6) ? "June" : (InitDate.Month == 7) ? "July" :
                            (InitDate.Month == 8) ? "August" : (InitDate.Month == 9) ? "September" : (InitDate.Month == 10) ? "October" :
                            (InitDate.Month == 11) ? "November" : (InitDate.Month == 12) ? "December" : string.Empty;
                    var year = InitDate.Year.ToString().Substring(2);

                    return $"{month} - {year}";
                }
                catch (Exception)
                {

                    return string.Empty;
                }
                
            }
        }
    }
}
