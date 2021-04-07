using KyoS.Common.Enums;
using KyoS.Web.Data;
using KyoS.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Helpers
{
    public class CombosHelper : ICombosHelper
    {
        private readonly DataContext _context;

        public CombosHelper(DataContext context)
        {
            _context = context;
        }

        public IEnumerable<SelectListItem> GetComboRoles()
        {
            List<SelectListItem> list = new List<SelectListItem>
                                { new SelectListItem { Text = UserType.Facilitator.ToString(), Value = "1"},
                                  new SelectListItem { Text = UserType.Supervisor.ToString(), Value = "2"},
                                  new SelectListItem { Text = UserType.Mannager.ToString(), Value = "3"},
                                  new SelectListItem { Text = UserType.Admin.ToString(), Value = "4"}
            };
            
            list.Insert(0, new SelectListItem
            {
                Text = "[Select rol...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboUserNamesByRolesClinic(UserType userType, int idClinic)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            if (idClinic == 0)
            {
                list = _context.Users.Where(u => u.UserType == userType).Select(u => new SelectListItem
                {
                    Text = $"{u.UserName}",
                    Value = $"{u.Id}"
                }).ToList();
            }
            else
            {
                list = _context.Users.Where(u => (u.UserType == userType && u.Clinic.Id == idClinic)).Select(u => new SelectListItem
                {
                    Text = $"{u.UserName}",
                    Value = $"{u.Id}"
                }).ToList();
            }

            list.Insert(0, new SelectListItem
            {
                Text = "[Select linked user...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboDays()
        {
            List<SelectListItem> list = new List<SelectListItem>
                                { new SelectListItem { Text = DayOfWeekType.Monday.ToString(), Value = "1"},
                                  new SelectListItem { Text = DayOfWeekType.Tuesday.ToString(), Value = "2"},
                                  new SelectListItem { Text = DayOfWeekType.Wednesday.ToString(), Value = "3"},
                                  new SelectListItem { Text = DayOfWeekType.Thursday.ToString(), Value = "4"},
                                  new SelectListItem { Text = DayOfWeekType.Friday.ToString(), Value = "5"}};

            list.Insert(0, new SelectListItem
            {
                Text = "[Select day...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboThemes()
        {
            List<SelectListItem> list = _context.Themes.Select(t => new SelectListItem
            {
                Text = $"{t.Day.ToString()} - {t.Name}",
                Value = $"{t.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select topic...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboThemesByClinic(int idClinic)
        {
            List<SelectListItem> list = _context.Themes.Where(t => t.Clinic.Id == idClinic).Select(t => new SelectListItem
            {
                Text = $"{t.Day.ToString()} - {t.Name}",
                Value = $"{t.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select topic...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboFacilitators()
        {
            List<SelectListItem> list = _context.Facilitators.Select(f => new SelectListItem
            {
                Text = $"{f.Name}",
                Value = $"{f.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select facilitator...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboFacilitatorsByClinic(int idClinic)
        {
            List<SelectListItem> list = _context.Facilitators.Where(f => f.Clinic.Id == idClinic).Select(f => new SelectListItem
            {
                Text = $"{f.Name}",
                Value = $"{f.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select facilitator...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboClients()
        {
            List<SelectListItem> list = _context.Clients.Select(c => new SelectListItem
            {
                Text = $"{c.Name}",
                Value = $"{c.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select client...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboClientsByClinic(int idClinic)
        {
            List<SelectListItem> list = _context.Clients.Where(c => (c.Clinic.Id == idClinic && c.MTPs.Count == 0))
                                                        .Select(c => new SelectListItem
            {
                Text = $"{c.Name}",
                Value = $"{c.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select client...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboActivities()
        {
            List<SelectListItem> list = _context.Activities.Select(a => new SelectListItem
            {
                Text = $"{a.Id.ToString()} - {a.Name}",
                Value = $"{a.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select activity...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboActivitiesByTheme(int idTheme, int idFacilitator, DateTime date)
        {
            List<ActivityEntity> activities = _context.Activities

                                                      .Include(a => a.Workdays_Activities_Facilitators)
                                                      .ThenInclude(waf => waf.Workday)

                                                      .Include(a => a.Workdays_Activities_Facilitators)
                                                      .ThenInclude(waf => waf.Facilitator)

                                                      .Where(a => (a.Theme.Id == idTheme && a.Status == ActivityStatus.Approved))                                                      

                                                      .ToList();

            List<SelectListItem> list = new List<SelectListItem>();            
            Workday_Activity_Facilitator waf;
            TimeSpan daysCount;
            int weeks;
            
            foreach (ActivityEntity item in activities)
            {
                waf = item.Workdays_Activities_Facilitators.Where(waf => waf.Facilitator.Id == idFacilitator).Max();
                if ((waf == null) || (date == waf.Workday.Date))    //Actividad no usada por el facilitator
                {
                    list.Insert(0, new SelectListItem
                    {
                        Text = $"{item.Id.ToString()} - NU - {item.Name}",
                        Value = $"{item.Id}"
                    });
                }
                else
                {
                    daysCount = date - waf.Workday.Date;                    
                    weeks = Convert.ToInt32(Math.Round(Convert.ToDouble(daysCount.Days) / 7, 0, MidpointRounding.AwayFromZero));
                    list.Insert(0, new SelectListItem
                    {
                        Text = $"{item.Id.ToString()} - {weeks}w - {item.Name}",
                        Value = $"{item.Id}"
                    });
                }                
            }

            list.Insert(0, new SelectListItem
            {
                Text = "[Select activity...]",
                Value = "0"
            });
            
            return list.OrderBy(l => Convert.ToInt32(l.Value));
        }

        public IEnumerable<SelectListItem> GetComboClassifications()
        {
            List<SelectListItem> list = new List<SelectListItem>
                                { new SelectListItem { Text = NoteClassification.Depressed.ToString(), Value = "1"},
                                  new SelectListItem { Text = NoteClassification.Negativistic.ToString(), Value = "2"},
                                  new SelectListItem { Text = NoteClassification.Sadness.ToString(), Value = "3"},
                                  new SelectListItem { Text = NoteClassification.Anxious.ToString(), Value = "4"},
                                  new SelectListItem { Text = NoteClassification.SleepProblems.ToString(), Value = "5"},
                                  new SelectListItem { Text = NoteClassification.Insomnia.ToString(), Value = "6" },
                                  new SelectListItem { Text = NoteClassification.Socialization.ToString(), Value = "7" },
                                  new SelectListItem { Text = NoteClassification.Isolation.ToString(), Value = "8" },
                                  new SelectListItem { Text = NoteClassification.Community.ToString(), Value = "9" },
                                  new SelectListItem { Text = NoteClassification.Motivation.ToString(), Value = "10" },
                                  new SelectListItem { Text = NoteClassification.Irritable.ToString(), Value = "11"},
                                  new SelectListItem { Text = NoteClassification.SelfEsteem.ToString(), Value = "12"},
                                  new SelectListItem { Text = NoteClassification.Concentration.ToString(), Value = "13"},
                                  new SelectListItem { Text = NoteClassification.Memory.ToString(), Value = "14"},
                                  new SelectListItem { Text = NoteClassification.Independent.ToString(), Value = "15"},
                                  new SelectListItem { Text = NoteClassification.MedicalManagenent.ToString(), Value = "16" },
                                  new SelectListItem { Text = NoteClassification.SelfCare.ToString(), Value = "17" },
                                  new SelectListItem { Text = NoteClassification.PositiveSelfTalk.ToString(), Value = "18" },
                                  new SelectListItem { Text = NoteClassification.NegativeSelfTalk.ToString(), Value = "19" }};


        list.Insert(0, new SelectListItem
            {
                Text = "[Select classification of note...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboClinics()
        {
            List<SelectListItem> list = _context.Clinics.Select(a => new SelectListItem
            {
                Text = $"{a.Name}",
                Value = $"{a.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select clinic...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboGender()
        {
            List<SelectListItem> list = new List<SelectListItem>
                                { new SelectListItem { Text = GenderType.Female.ToString(), Value = "1"},
                                  new SelectListItem { Text = GenderType.Male.ToString(), Value = "2"}};

            list.Insert(0, new SelectListItem
            {
                Text = "[Select gender...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboClientStatus()
        {
            List<SelectListItem> list = new List<SelectListItem>
                                { new SelectListItem { Text = StatusType.Open.ToString(), Value = "1"},
                                  new SelectListItem { Text = StatusType.Close.ToString(), Value = "2"}};

            list.Insert(0, new SelectListItem
            {
                Text = "[Select status...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboGroups()
        {
            List<SelectListItem> list = _context.Groups.Select(g => new SelectListItem
            {
                Text = $"{g.Facilitator.Name} - {g.Meridian}",
                Value = $"{g.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select group...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboGoals(int idMTP)
        {
            List<SelectListItem> list = _context.Goals.Where(g => g.MTP.Id == idMTP).Select(g => new SelectListItem
            {
                Text = $"{g.Number}",
                Value = $"{g.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select goal...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboObjetives(int idGoal)
        {
            List<SelectListItem> list = _context.Objetives.Where(o => o.Goal.Id == idGoal).Select(o => new SelectListItem
            {
                Text = $"{o.Objetive}",
                Value = $"{o.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[First select goal...]",
                Value = "0"
            });

            return list;
        }
    }
}
