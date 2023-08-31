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
                                { new SelectListItem { Text = UserType.Documents_Assistant.ToString(), Value = "1"},
                                  new SelectListItem { Text = UserType.Facilitator.ToString(), Value = "2"},
                                  new SelectListItem { Text = UserType.Supervisor.ToString(), Value = "3"},
                                  new SelectListItem { Text = UserType.CaseManager.ToString(), Value = "4"},
                                  new SelectListItem { Text = UserType.TCMSupervisor.ToString(), Value = "5"},
                                  new SelectListItem { Text = UserType.Manager.ToString(), Value = "6"},
                                  new SelectListItem { Text = UserType.Admin.ToString(), Value = "7"},
                                  new SelectListItem { Text = UserType.Frontdesk.ToString(), Value = "8"}
            };
            
            list.Insert(0, new SelectListItem
            {
                Text = "[Select rol...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboUserNames()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            list = _context.Users.OrderBy(u => u.UserName).Select (u => new SelectListItem
            {
                    Text = $"{u.UserName}",
                    Value = $"{u.Id}"
            }).ToList();            

            //list.Insert(0, new SelectListItem
            //{
            //    Text = "[Select user...]",
            //    Value = "0"
            //});

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

        public IEnumerable<SelectListItem> GetComboThemesByClinic(int idClinic, ThemeType service = ThemeType.PSR)
        {
            if (service == ThemeType.All)
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
            else
            {
                List<SelectListItem> list = _context.Themes.Where(t => t.Clinic.Id == idClinic && t.Service == service).Select(t => new SelectListItem
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
        }

        public IEnumerable<SelectListItem> GetComboThemesByClinic3(int idClinic, ThemeType service = ThemeType.PSR)
        {
            if (service == ThemeType.All)
            {
                List<SelectListItem> list = _context.Themes.Where(t => (t.Clinic.Id == idClinic && t.Day == null)).Select(t => new SelectListItem
                {
                    Text = $"{t.Name + " | " + t.Id}",
                    Value = $"{t.Id}"
                }).ToList();

                list.Insert(0, new SelectListItem
                {
                    Text = "[Select theme...]",
                    Value = "0"
                });

                return list;
            }
            else
            {
                List<SelectListItem> list = _context.Themes.Where(t => (t.Clinic.Id == idClinic && t.Day == null && t.Service == service)).Select(t => new SelectListItem
                {
                    Text = $"{t.Name + " | " + t.Id}",
                    Value = $"{t.Id}"
                }).ToList();

                list.Insert(0, new SelectListItem
                {
                    Text = "[Select theme...]",
                    Value = "0"
                });

                return list;
            }
            
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

        public IEnumerable<SelectListItem> GetComboFacilitatorsByClinic(int idClinic, bool blank)
        {
            List<SelectListItem> list = _context.Facilitators.Where(f => f.Clinic.Id == idClinic).OrderBy(f => f.Name).Select(f => new SelectListItem
            {
                Text = $"{f.Name}",
                Value = $"{f.Id}"
            }).ToList();

            if (!blank)
            {
                list.Insert(0, new SelectListItem
                {
                    Text = "[Select facilitator...]",
                    Value = "0"
                });
            }
            else
            {
                list.Insert(0, new SelectListItem
                {
                    Text = string.Empty,
                    Value = "0"
                });
            }
            
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

        public IEnumerable<SelectListItem> GetComboClientsByClinic(int idClinic, bool blank)
        {
            List<SelectListItem> list = _context.Clients.Where(c => (c.Clinic.Id == idClinic /* && c.MTPs.Count == 0*/))
                                                        .OrderBy(c => c.Name)
                                                        .Select(c => new SelectListItem
            {
                Text = $"{c.Name}",
                Value = $"{c.Id}"
            }).ToList();

            if (!blank)
            {
                list.Insert(0, new SelectListItem
                {
                    Text = "[Select client...]",
                    Value = "0"
                });
            }
            else
            {
                list.Insert(0, new SelectListItem
                {
                    Text = string.Empty,
                    Value = "0"
                });
            }

            return list;
        }

        public IEnumerable<SelectListItem> GetComboActiveClientsByClinic(int idClinic)
        {
            List<SelectListItem> list = _context.Clients
                                                .Where(c => (c.Clinic.Id == idClinic 
                                                          && c.MTPs.Where(m => m.Active == true).Count() > 0 && c.Status == StatusType.Open))
                                                .Select(c => new SelectListItem
                                                {
                                                    Text = $"{c.Name}",
                                                    Value = $"{c.Id}"
                                                })
                                                .ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select client...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboActiveClientsPSRByClinic(int idClinic)
        {
            List<SelectListItem> list = _context.Clients
                                                .Where(c => (c.Clinic.Id == idClinic
                                                          && c.MTPs.Where(m => m.Active == true).Count() > 0 && c.Status == StatusType.Open
                                                          && c.Service == ServiceType.PSR))
                                                .Select(c => new SelectListItem
                                                {
                                                    Text = $"{c.Name}",
                                                    Value = $"{c.Id}"
                                                })
                                                .ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select client...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboClientsForIndNotes(int idClinic, int idWeek, int idFacilitator, int idWorkday)
        {
            List<ClientEntity> clients = _context.Clients
                                                 .Include(n => n.DischargeList)
                                                 .Include(wc => wc.MTPs)
                                                 .ThenInclude(n => n.Goals)
                                                 .ThenInclude(n => n.Objetives)
                                                 .Where(c => (c.Clinic.Id == idClinic
                                                           && c.MTPs.Where(m => m.Active == true).Count() > 0 && c.Status == StatusType.Open
                                                           && c.IndividualTherapyFacilitator.Id == idFacilitator))
                                                 .ToList();

            List<Workday_Client> workdays_clients = _context.Workdays_Clients

                                                            .Include(wc => wc.Client)
                                                            .ThenInclude(n => n.DischargeList)
                                                            .Where(wc => (wc.Workday.Week.Id == idWeek && wc.Workday.Service == ServiceType.Individual))
                                                            .ToList();

            Workday_Client workday_client = _context.Workdays_Clients
                                                    .Include(n => n.Workday)
                                                    .Include(n => n.Schedule)
                                                    .ThenInclude(n => n.SubSchedules)
                                                    .FirstOrDefault(n => n.Id == idWorkday);

            foreach (var item in workdays_clients)
            {
                if (item.Client != null)
                {
                    if (clients.Exists(c => c.Id == item.Client.Id ))
                        clients.Remove(item.Client);

                }
            }

            clients = clients.Where(n => (n.DischargeList.Where(d => d.TypeService == ServiceType.Individual
                                                                 && d.DateDischarge > workday_client.Workday.Date).Count() > 0
                                      || n.DischargeList.Where(d => d.TypeService == ServiceType.Individual).Count() == 0)
                                      && n.AdmisionDate < workday_client.Workday.Date
                                      && n.MTPs.First(c => c.Active == true).Goals.Where(g => g.Service == ServiceType.Individual 
                                                                                           && g.Objetives.Where(o => o.DateResolved > workday_client.Workday.Date).Count() > 0 ).Count() > 0)
                             .OrderBy(n => n.Name)
                             .ToList();
            
            List<ClientEntity> salida = new List<ClientEntity>();
            List<Workday_Client> temp = new List<Workday_Client>();

            //-----obtengo el susSchedule-----------
            List<SubScheduleEntity> subSchedules = new List<SubScheduleEntity>();
            SubScheduleEntity subScheduleEntity = new SubScheduleEntity();

            if (workday_client.Schedule != null)
            {
                subSchedules =  _context.SubSchedule.Where(n => n.Schedule.Id == workday_client.Schedule.Id).OrderBy(n => n.InitialTime).ToList();

                string time = workday_client.Session.Substring(0, 7);
                foreach (var value in subSchedules)
                {
                    if (value.InitialTime.ToShortTimeString().ToString().Contains(time) == true)
                    {
                        subScheduleEntity = value;
                    }
                }

            }

            foreach (var item in clients)
            {
                temp = _context.Workdays_Clients.Where(n => n.Workday.Date == workday_client.Workday.Date
                                                               && ((n.Schedule.InitialTime.TimeOfDay >= subScheduleEntity.InitialTime.TimeOfDay
                                                               && n.Schedule.InitialTime.TimeOfDay <= subScheduleEntity.EndTime.TimeOfDay)
                                                                   || (n.Schedule.EndTime.TimeOfDay >= subScheduleEntity.InitialTime.TimeOfDay
                                                                      && n.Schedule.EndTime.TimeOfDay <= subScheduleEntity.EndTime.TimeOfDay)
                                                                   || (n.Schedule.InitialTime.TimeOfDay <= subScheduleEntity.InitialTime.TimeOfDay
                                                                      && n.Schedule.EndTime.TimeOfDay >= subScheduleEntity.InitialTime.TimeOfDay)
                                                                   || (n.Schedule.InitialTime.TimeOfDay <= subScheduleEntity.EndTime.TimeOfDay
                                                                      && n.Schedule.EndTime.TimeOfDay >= subScheduleEntity.EndTime.TimeOfDay))
                                                              && n.Id != workday_client.Id
                                                              && n.Client.Id == item.Id
                                                              && n.Present == true)
                                                           .ToList();
                if (temp.Count() == 0)
                {
                    salida.Add(item);
                    temp = new List<Workday_Client>();
                }
            }

            List<SelectListItem> list = salida.Select(c => new SelectListItem
                                                {
                                                    Text = $"{c.Name}",
                                                    Value = $"{c.Id}"
                                                })
                                                .ToList();

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
                waf = item.Workdays_Activities_Facilitators.Where(waf => waf.Facilitator.Id == idFacilitator).LastOrDefault();
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
            List<SelectListItem> list = _context.Goals.Where(g => (g.MTP.Id == idMTP)).Select(g => new SelectListItem
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

        public IEnumerable<SelectListItem> GetComboGoalsByService(int idMTP, ServiceType service)
        {            
            List<SelectListItem> list = _context.Goals.Where(g => (g.MTP.Id == idMTP && g.Service == service && g.Compliment == false)).Select(g => new SelectListItem
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
            List<SelectListItem> list = _context.Objetives.Where(o => o.Goal.Id == idGoal && o.Compliment == false).Select(o => new SelectListItem
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

        public IEnumerable<SelectListItem> GetComboRelationships()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = RelationshipType.Unknown.ToString(), Value = "0"},
                new SelectListItem { Text = RelationshipType.Brother.ToString(), Value = "1"},
                new SelectListItem { Text = RelationshipType.Child.ToString(), Value = "2"},
                new SelectListItem { Text = RelationshipType.Daugther.ToString(), Value = "3"},
                new SelectListItem { Text = RelationshipType.Father.ToString(), Value = "4"},
                new SelectListItem { Text = RelationshipType.Friend.ToString(), Value = "5"},
                new SelectListItem { Text = RelationshipType.Guardian.ToString(), Value = "6"},
                new SelectListItem { Text = RelationshipType.Mother.ToString(), Value = "7"},
                new SelectListItem { Text = RelationshipType.Psychiatrist.ToString(), Value = "8"},
                new SelectListItem { Text = RelationshipType.Self.ToString(), Value = "9"},
                new SelectListItem { Text = RelationshipType.Sibling.ToString(), Value = "10"},
                new SelectListItem { Text = RelationshipType.Sister.ToString(), Value = "11"},
                new SelectListItem { Text = RelationshipType.Spouse.ToString(), Value = "12"},
                new SelectListItem { Text = RelationshipType.Son.ToString(), Value = "13"},
                new SelectListItem { Text = RelationshipType.Other.ToString(), Value = "14"}
            };
            
            return list;
        }

        public IEnumerable<SelectListItem> GetComboRaces()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = RaceType.Unknown.ToString(), Value = "0"},
                new SelectListItem { Text = RaceType.White.ToString(), Value = "1"},
                new SelectListItem { Text = RaceType.Black.ToString(), Value = "2"},
                new SelectListItem { Text = RaceType.NativeAmerican.ToString(), Value = "3"},
                new SelectListItem { Text = RaceType.AfricanAmerican.ToString(), Value = "4"},
                new SelectListItem { Text = RaceType.Asian.ToString(), Value = "5"},
                new SelectListItem { Text = RaceType.Other.ToString(), Value = "6"}                
            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboMaritals()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = MaritalStatus.Unknown.ToString(), Value = "0"},
                new SelectListItem { Text = MaritalStatus.Single.ToString(), Value = "1"},
                new SelectListItem { Text = MaritalStatus.Married.ToString(), Value = "2"},
                new SelectListItem { Text = MaritalStatus.Cohabiting.ToString(), Value = "3"},
                new SelectListItem { Text = MaritalStatus.Divorced.ToString(), Value = "4"},
                new SelectListItem { Text = MaritalStatus.Separated.ToString(), Value = "5"},
                new SelectListItem { Text = MaritalStatus.Widowed.ToString(), Value = "6"}
            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboEthnicities()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = EthnicityType.Unknown.ToString(), Value = "0"},
                new SelectListItem { Text = EthnicityType.HispanicLatino.ToString(), Value = "1"},
                new SelectListItem { Text = EthnicityType.NonHispanicLatino.ToString(), Value = "2"}
                
            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboLanguages()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = PreferredLanguage.English.ToString(), Value = "0"},
                new SelectListItem { Text = PreferredLanguage.Spanish.ToString(), Value = "1"},
                new SelectListItem { Text = PreferredLanguage.French.ToString(), Value = "2"},
                new SelectListItem { Text = PreferredLanguage.Portuguese.ToString(), Value = "3"}                
            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboReferredsByClinic(string idUser)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.Id == idUser);

            ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);

            List<ReferredEntity> referreds = _context.Referreds.OrderBy(d => d.Name).ToList();
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

            List<SelectListItem> list = new List<SelectListItem>();
            list = referreds_by_clinic.Select(r => new SelectListItem
             {
                 Text = $"{r.Name}",
                 Value = $"{r.Id}"
             }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select referred...]",
                Value = "0"
            });

            return list.OrderBy(l => l.Text);
        }

        public IEnumerable<SelectListItem> GetComboEmergencyContactsByClinic(string idUser)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.Id == idUser);

            ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);

            List<EmergencyContactEntity> contacts = _context.EmergencyContacts.OrderBy(d => d.Name).ToList();
            List<EmergencyContactEntity> contacts_by_clinic = new List<EmergencyContactEntity>();
            UserEntity user;
            foreach (EmergencyContactEntity item in contacts)
            {
                user = _context.Users.FirstOrDefault(u => u.Id == item.CreatedBy);
                if (clinic.Users.Contains(user))
                {
                    contacts_by_clinic.Add(item);
                }
            }

            List<SelectListItem> list = new List<SelectListItem>();
            list = contacts_by_clinic.Select(ec => new SelectListItem
            {
                Text = $"{ec.Name}",
                Value = $"{ec.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = string.Empty,
                Value = "0"
            });

            return list.OrderBy(l => l.Text);
        }

        public IEnumerable<SelectListItem> GetComboDoctorsByClinic(string idUser)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.Id == idUser);

            ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);

            List<DoctorEntity> doctors = _context.Doctors.OrderBy(d => d.Name).ToList();
            List<DoctorEntity> doctors_by_clinic = new List<DoctorEntity>();
            UserEntity user;
            foreach (DoctorEntity item in doctors)
            {
                user = _context.Users.FirstOrDefault(u => u.Id == item.CreatedBy);
                if (clinic.Users.Contains(user))
                {
                    doctors_by_clinic.Add(item);
                }
            }

            List<SelectListItem> list = new List<SelectListItem>();
            list = doctors_by_clinic.Select(d => new SelectListItem
            {
                Text = $"{d.Name}",
                Value = $"{d.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = string.Empty,
                Value = "0"
            });

            return list.OrderBy(l => l.Text);
        }

        public IEnumerable<SelectListItem> GetComboPsychiatristsByClinic(string idUser)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.Id == idUser);

            ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);

            List<PsychiatristEntity> psychiatrists = _context.Psychiatrists.OrderBy(d => d.Name).ToList();
            List<PsychiatristEntity> psychiatrists_by_clinic = new List<PsychiatristEntity>();
            UserEntity user;
            foreach (PsychiatristEntity item in psychiatrists)
            {
                user = _context.Users.FirstOrDefault(u => u.Id == item.CreatedBy);
                if (clinic.Users.Contains(user))
                {
                    psychiatrists_by_clinic.Add(item);
                }
            }

            List<SelectListItem> list = new List<SelectListItem>();
            list = psychiatrists_by_clinic.Select(p => new SelectListItem
            {
                Text = $"{p.Name}",
                Value = $"{p.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = string.Empty,
                Value = "0"
            });

            return list.OrderBy(l => l.Text);
        }

        public IEnumerable<SelectListItem> GetComboLegalGuardiansByClinic(string idUser)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.Id == idUser);

            ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);

            List<LegalGuardianEntity> legalGuardians = _context.LegalGuardians.OrderBy(d => d.Name).ToList();
            List<LegalGuardianEntity> legalGuardians_by_clinic = new List<LegalGuardianEntity>();
            UserEntity user;
            foreach (LegalGuardianEntity item in legalGuardians)
            {
                user = _context.Users.FirstOrDefault(u => u.Id == item.CreatedBy);
                if (clinic.Users.Contains(user))
                {
                    legalGuardians_by_clinic.Add(item);
                }
            }

            List<SelectListItem> list = new List<SelectListItem>();
            list = legalGuardians_by_clinic.Select(lg => new SelectListItem
            {
                Text = $"{lg.Name}",
                Value = $"{lg.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = string.Empty,
                Value = "0"
            });

            return list.OrderBy(l => l.Text);
        }

        public IEnumerable<SelectListItem> GetComboDiagnosticsByClinic(string idUser)
        {
            UserEntity user_logged = _context.Users.Include(u => u.Clinic)
                                                   .FirstOrDefault(u => u.Id == idUser);

            ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == user_logged.Clinic.Id);

            List<DiagnosticEntity> diagnostics = _context.Diagnostics.OrderBy(d => d.Code).ToList();
            List<DiagnosticEntity> diagnostics_by_clinic = new List<DiagnosticEntity>();
            UserEntity user;
            foreach (DiagnosticEntity item in diagnostics)
            {
                user = _context.Users.FirstOrDefault(u => u.Id == item.CreatedBy);
                if (clinic.Users.Contains(user))
                {
                    diagnostics_by_clinic.Add(item);
                }
            }

            List<SelectListItem> list = new List<SelectListItem>();
            list = diagnostics_by_clinic.Select(d => new SelectListItem
            {
                Text = $"{d.Code} {d.Description}",
                Value = $"{d.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select diagnostic...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboDocumentDescriptions()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = DocumentDescription.Psychiatrist_evaluation.ToString(), Value = "0"},
                new SelectListItem { Text = DocumentDescription.Intake.ToString(), Value = "1"},
                new SelectListItem { Text = DocumentDescription.Bio.ToString(), Value = "2"},
                new SelectListItem { Text = DocumentDescription.Fars.ToString(), Value = "3"},
                new SelectListItem { Text = DocumentDescription.MTP.ToString(), Value = "4"},
                new SelectListItem { Text = DocumentDescription.Addendum.ToString(), Value = "5"},
                new SelectListItem { Text = DocumentDescription.MTP_review.ToString(), Value = "6"},
                new SelectListItem { Text = DocumentDescription.Consent.ToString(), Value = "7"},
                new SelectListItem { Text = DocumentDescription.Identification.ToString(), Value = "8"},
                new SelectListItem { Text = DocumentDescription.MedicaidCard.ToString(), Value = "9"},
                new SelectListItem { Text = DocumentDescription.MedicareCard.ToString(), Value = "10"},
                new SelectListItem { Text = DocumentDescription.Social.ToString(), Value = "11"},
                new SelectListItem { Text = DocumentDescription.Referrals.ToString(), Value = "12"},
                new SelectListItem { Text = DocumentDescription.MentalStateExamination.ToString(), Value = "13"},
                new SelectListItem { Text = DocumentDescription.AdicionalClinicDocumentation.ToString(), Value = "14"},
                new SelectListItem { Text = DocumentDescription.FollowUps.ToString(), Value = "15"},
                new SelectListItem { Text = DocumentDescription.YearlyPhysical.ToString(), Value = "16"},
                new SelectListItem { Text = DocumentDescription.MedicalReports.ToString(), Value = "17"},
                new SelectListItem { Text = DocumentDescription.Assessment.ToString(), Value = "18"},
                new SelectListItem { Text = DocumentDescription.ServicePlan.ToString(), Value = "19"},
                new SelectListItem { Text = DocumentDescription.ServicePlanReview.ToString(), Value = "20"},
                new SelectListItem { Text = DocumentDescription.AppendixJ.ToString(), Value = "21"},
                new SelectListItem { Text = DocumentDescription.Binder.ToString(), Value = "22"},
                new SelectListItem { Text = DocumentDescription.Others.ToString(), Value = "23"}
                
            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboIncidentsStatus()
        {
            List<SelectListItem> list = new List<SelectListItem>
                                { new SelectListItem { Text = IncidentsStatus.Pending.ToString(), Value = "0"},
                                  new SelectListItem { Text = IncidentsStatus.Solved.ToString(), Value = "1"},
                                  new SelectListItem { Text = IncidentsStatus.NotValid.ToString(), Value = "2"},
                                  new SelectListItem { Text = IncidentsStatus.Reviewed.ToString(), Value = "3"}};
            
            return list;
        }

        public IEnumerable<SelectListItem> GetComboActiveInsurancesByClinic(int idClinic)
        {
            List<SelectListItem> list = _context.HealthInsurances
                                                 .Where(hi => (hi.Clinic.Id == idClinic
                                                            && hi.Active == true))
                                                 .Select(hi => new SelectListItem
                                                 {
                                                     Text = $"{hi.Name}",
                                                     Value = $"{hi.Id}"
                                                 })
                                                 .ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select insurance...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboServices()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = ServiceType.PSR.ToString(), Value = "0"},
                new SelectListItem { Text = ServiceType.Individual.ToString(), Value = "1"},
                new SelectListItem { Text = ServiceType.Group.ToString(), Value = "2"}               
            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboCasemannagersByClinic(int idClinic)
        {
            List<SelectListItem> list = _context.CaseManagers
                                                .Where(f => f.Clinic.Id == idClinic)
                                                .OrderBy(f => f.Name)
                                                .Select(f => new SelectListItem
            {
                Text = $"{f.Name}",
                Value = $"{f.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select Case Manager...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboCaseMannagersByClinicFilter(int idClinic)
        {
            List<SelectListItem> list = _context.CaseManagers
                                                .Where(f => f.Clinic.Id == idClinic)
                                                .OrderBy(f => f.Name)
                                                .Select(f => new SelectListItem
                                                {
                                                    Text = $"{f.Name}",
                                                    Value = $"{f.Id}"
                                                }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[All Case Managers...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboCaseManager()
        {
            List<SelectListItem> list = _context.CaseManagers.Select(f => new SelectListItem
            {
                Text = $"{f.Name}",
                Value = $"{f.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select Casemanager...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboClientsForTCMCaseNotOpen(int idClinic)
        {
            List<ClientEntity> clients_Total = _context.Clients
                                                       .Where(c => (c.Clinic.Id == idClinic
                                                            && c.Status == StatusType.Open))
                                                       .OrderBy(c => c.Name)
                                                       .ToList();
            List<TCMClientEntity> clients_Open = _context.TCMClient
                                                         .Include(n => n.Client)
                                                         .Where(c => (c.Client.Clinic.Id == idClinic
                                                            && c.Status == StatusType.Open))
                                                         .ToList();
            
            foreach (var item in clients_Open)
            {
                if (item.Client != null)
                {
                    if (clients_Total.Exists(c => c.Id == item.Client.Id))
                        clients_Total.Remove(item.Client);
                }
            }

            List<SelectListItem> list = clients_Total.Select(c => new SelectListItem
            {
                Text = $"{c.Name}",
                Value = $"{c.Id}"
            })
                                                .ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select client...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboClientsForTCMCaseOpen(int idClinic)
        {
            
            List<TCMClientEntity> tcmClients_Open = _context.TCMClient
                                                            .Include(g => g.Client)
                                                            .Where(c => (c.Client.Clinic.Id == idClinic
                                                                      && c.Status == StatusType.Open))
                                                            .OrderBy(c => c.Client.Name)
                                                            .ToList();

            List<SelectListItem> list = tcmClients_Open.Select(c => new SelectListItem
            {
                Text = $"{c.Client.Name} | {c.CaseNumber}",
                Value = $"{c.Id}"
            })
                                                .ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select Client...]",
                Value = "0"
                
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboClientsForTCMCaseOpenFilter(int idClinic)
        {

            List<TCMClientEntity> tcmClients_Open = _context.TCMClient
                                                            .Include(g => g.Client)
                                                            .Where(c => (c.Client.Clinic.Id == idClinic))
                                                            .OrderBy(c => c.Client.Name)
                                                            .ToList();

            List<SelectListItem> list = tcmClients_Open.Select(c => new SelectListItem
            {
                Text = $"{c.Client.Name} | {c.CaseNumber}",
                Value = $"{c.Id}"
            })
                                                .ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[All Clients...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboServicesNotUsed(int idServicePlan)
        {
            List<TCMServiceEntity> Services_Total = _context.TCMServices
                                                                     .Include(n => n.Stages)
                                                                     .OrderBy(n => n.Code)
                                                                     .ToList();
            List<TCMDomainEntity> Services_Domain = _context.TCMDomains
                                                    .Include(d => d.TcmServicePlan)
                                                 .Where(d => d.TcmServicePlan.Id == idServicePlan)
                                                 .ToList();
            TCMServiceEntity Service = null;

            foreach (var item in Services_Domain)
            {
                if (item.TcmServicePlan != null)
                {
                    if (Services_Total.Exists(c => c.Code == item.Code))
                    {
                        Service = _context.TCMServices.FirstOrDefault(n => n.Code == item.Code);
                        Services_Total.Remove(Service);
                    } 
                }
            }

            List<SelectListItem> list = Services_Total.Select(c => new SelectListItem
            {
                Text = $"{c.Code + "-" + c.Name}",
                Value = $"{c.Id}"
            })
                                                .ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select service...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboStagesNotUsed(TCMDomainEntity Domain)
        {
            List<TCMStageEntity> Stages_Total = _context.TCMStages
                                                        .Where(f => f.tCMservice.Code == Domain.Code)
                                                        .OrderBy(n => n.Name)
                                                        .ToList();
            List<TCMDomainEntity> allDomain = _context.TCMDomains
                                                         .Where(g => g.TcmServicePlan.Id == Domain.TcmServicePlan.Id)
                                                         .OrderBy(g => g.Code)
                                                         .ToList();
            
           
            TCMStageEntity Stage = null;
            List<TCMObjetiveEntity> Stages_Objetive = null;
            foreach (var itemDomain in allDomain)
            {

                Stages_Objetive = _context.TCMObjetives
                                                   .Where(f => f.TcmDomain.Code == itemDomain.Code)
                                                       .OrderBy(n => n.Name)
                                                       .ToList();
                foreach (var item in Stages_Objetive)
                {
                    if (item.TcmDomain != null)
                    {
                        if (Stages_Total.Exists(c => c.Name == item.Name))
                        {
                            Stage = _context.TCMStages.FirstOrDefault(n => (n.Name == item.Name
                                                                        && n.tCMservice.Code == itemDomain.Code));
                            Stages_Total.Remove(Stage);
                        }
                    }
                }
            }
            

            List<SelectListItem> list = Stages_Total.Select(c => new SelectListItem
            {
                Text = $"{c.Name}",
                Value = $"{c.Id}"
            })
                .ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select stage...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboServicesPlan(int idClinic, int caseManagerId, string idClient = "")
        {
            List<TCMServicePlanEntity> tcmSerivicePlan = new List<TCMServicePlanEntity>();

            if (idClient == "")
            {
                tcmSerivicePlan  = _context.TCMServicePlans
                                           .Include(g => g.TcmClient)
                                           .ThenInclude(g => g.Client)
                                           .Where(c => (c.TcmClient.Client.Clinic.Id == idClinic
                                               && c.Status == StatusType.Open
                                                && c.Approved == 2 && c.TcmClient.Casemanager.Id == caseManagerId))
                                           .ToList();
            }
            else
            {
                tcmSerivicePlan = _context.TCMServicePlans
                                          .Include(g => g.TcmClient)
                                          .ThenInclude(g => g.Client)
                                          .Where(c => (c.TcmClient.Client.Clinic.Id == idClinic
                                               && c.Status == StatusType.Open
                                               && c.Approved == 2 && c.TcmClient.Casemanager.Id == caseManagerId
                                               && c.TcmClient.CaseNumber == idClient))
                                          .ToList();
            }

            

            List<SelectListItem> list = tcmSerivicePlan.Select(c => new SelectListItem
            {
                Text = $"{c.TcmClient.CaseNumber}",
                Value = $"{c.Id}"
            })
                                                .ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select Case Number...]",
                Value = "0"

            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboTCMServices()
        {

            List<TCMServiceEntity> tcmSerivices = _context.TCMServices
                                                  .OrderBy(g => g.Code)
                                                  .ToList();

            List<SelectListItem> list = tcmSerivices.Select(c => new SelectListItem
            {
                Text = $"{c.Code + "-" + c.Name}",
                Value = $"{c.Id}"
            })
                                                .ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select TCM Service...]",
                Value = "0"

            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboTCMStages()
        {

            List<TCMStageEntity> tcmStages = _context.TCMStages
                                                  .OrderBy(g => g.Name)
                                                  .ToList();

            List<SelectListItem> list = tcmStages.Select(c => new SelectListItem
            {
                Text = $"{c.Name}",
                Value = $"{c.Id}"
            })
                                                .ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select TCM Stage...]",
                Value = "0"

            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboObjetiveStatus()
        {
            List<SelectListItem> list = new List<SelectListItem>
                                { new SelectListItem { Text = StatusType.Open.ToString(), Value = "0"},
                                  new SelectListItem { Text = StatusType.Close.ToString(), Value = "1"}};

           
            return list;
        }

        public IEnumerable<SelectListItem> GetComboIntake_ClientIs()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = "[Select Client is...]", Value = "0"},
                new SelectListItem { Text = IntakeClientIsStatus.Clean.ToString(), Value = "1"},
                new SelectListItem { Text = "Poorly dressed", Value = "2"},
                new SelectListItem { Text = IntakeClientIsStatus.Flamboyant.ToString(), Value = "3"},
                new SelectListItem { Text = "Poor ADL's", Value = "4"},
                new SelectListItem { Text = IntakeClientIsStatus.Disheveled.ToString(), Value = "5"},
                new SelectListItem { Text = "Neatly dressed", Value = "6"}
            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboIntake_BehaviorIs()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = "[Select Behavior is...]", Value = "0"},
                new SelectListItem { Text = IntakeBehaviorIsStatus.Normal.ToString(), Value = "1"},
                new SelectListItem { Text = IntakeBehaviorIsStatus.Hyperactive.ToString(), Value = "2"},
                new SelectListItem { Text = IntakeBehaviorIsStatus.WithDrawn.ToString(), Value = "3"},
                new SelectListItem { Text = "Resistant or Aggressive", Value = "4"}
                
            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboIntake_SpeechIs()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = "[Select Speech is...]", Value = "0"},
                new SelectListItem { Text = IntakeSpeechIsStatus.Normal.ToString(), Value = "1"},
                new SelectListItem { Text = IntakeSpeechIsStatus.Rapid.ToString(), Value = "2"},
                new SelectListItem { Text = IntakeSpeechIsStatus.Slow.ToString(), Value = "3"},
                new SelectListItem { Text = "Slurred or Incoherent", Value = "4"}

            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboBio_Appetite()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = "[Select Appetite...]", Value = "0"},
                new SelectListItem { Text = Bio_Appetite.Diminished.ToString(), Value = "1"},
                new SelectListItem { Text = Bio_Appetite.Increased.ToString(), Value = "2"},
                new SelectListItem { Text = Bio_Appetite.WNL.ToString(), Value = "3"},
                new SelectListItem { Text = Bio_Appetite.Anorexia.ToString(), Value = "4"}

            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboBio_Hydration()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = "[Select Hydration...]", Value = "0"},
                new SelectListItem { Text = Bio_Hydration.Diminished.ToString(), Value = "1"},
                new SelectListItem { Text = Bio_Hydration.IncreaseFluids.ToString(), Value = "2"},
                new SelectListItem { Text = Bio_Hydration.RestrictFluids.ToString(), Value = "3"},
                new SelectListItem { Text = Bio_Hydration.WNL.ToString(), Value = "4"},
                new SelectListItem { Text = Bio_Hydration.Inadequate.ToString(), Value = "5"}

            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboBio_RecentWeight()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = "[Select Appetite...]", Value = "0"},
                new SelectListItem { Text = Bio_RecentWeightChange.Intended.ToString(), Value = "1"},
                new SelectListItem { Text = Bio_RecentWeightChange.Unintended.ToString(), Value = "2"},
                new SelectListItem { Text = Bio_RecentWeightChange.Gained.ToString(), Value = "3"},
                new SelectListItem { Text = Bio_RecentWeightChange.Lost.ToString(), Value = "4"},
                new SelectListItem { Text = Bio_RecentWeightChange.N_A.ToString(), Value = "5"}


            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboBio_IfSexuallyActive()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = "[Select...]", Value = "0"},
                new SelectListItem { Text = Bio_IfSexuallyActive.YES.ToString(), Value = "1"},
                new SelectListItem { Text = Bio_IfSexuallyActive.NO.ToString(), Value = "2"},
                new SelectListItem { Text = Bio_IfSexuallyActive.N_A.ToString(), Value = "3"}
            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboTCMNoteSetting()
        {
            List<SelectListItem> list = new List<SelectListItem>
                                { new SelectListItem { Text = "03 - School", Value = "1"},
                                  new SelectListItem { Text = "11 - Office", Value = "2"},
                                  new SelectListItem { Text = "12 - Home", Value = "3"},
                                  new SelectListItem { Text = "33 - ALF", Value = "4"},
                                  new SelectListItem { Text = "99 - Other", Value = "5"}};

            list.Insert(0, new SelectListItem
            {
                Text = "[Select Setting...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboServicesUsed(int idServicePlan)
        {
           
            List<TCMDomainEntity> Services_Domain = _context.TCMDomains
                                                            .Include(d => d.TcmServicePlan)
                                                            .Where(d => d.TcmServicePlan.Id == idServicePlan)
                                                            .ToList();
            
            List<SelectListItem> list = Services_Domain.Select(c => new SelectListItem
            {
                Text = $"{c.Code + " - " + c.Name}",
                Value = $"{c.Id}"
            })
                                                .ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select service...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboTCMNoteActivity(string codeDomain)
        {

            List<TCMServiceActivityEntity> activity = _context.TCMServiceActivity.Where(n => n.TcmService.Code == codeDomain).ToList();
            
            List<SelectListItem> list = activity.Select(c => new SelectListItem
            {
                Text = $"{c.Name}",
                Value = $"{c.Id}"
            })
                                                .ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select activity...]",
                Value = "0"
            });
            
            return list;
        }

        public IEnumerable<SelectListItem> GetComboTCMClientsByCaseManager(string user)
        {
            List<SelectListItem> list = _context.TCMServicePlans

                                                .Include(c => c.TcmClient)
                                                .ThenInclude(c => c.Client)

                                                .Where(c => (c.Approved == 2 && c.Status == 0
                                                          && c.TcmClient.Casemanager.LinkedUser == user))
                                                .OrderBy(c => c.TcmClient.Client.Name)

                                                .Select(c => new SelectListItem
                                                {
                                                    Text = $"{c.TcmClient.Client.Name} | {c.TcmClient.CaseNumber}",
                                                    Value = $"{c.TcmClient.Id}"
                                                }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select client...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboServiceAgency()
        {
            List<SelectListItem> list = new List<SelectListItem>
                                { new SelectListItem { Text = ServiceAgency.CMH.ToString(), Value = "0"},
                                  new SelectListItem { Text = ServiceAgency.TCM.ToString(), Value = "1"}};

            return list;
        }

        public IEnumerable<SelectListItem> GetComboResidential()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = ResidentialStatus.LivingAlone.ToString(), Value = "0"},
                new SelectListItem { Text = ResidentialStatus.livingWithRelatives.ToString(), Value = "1"},
                new SelectListItem { Text = ResidentialStatus.livingWithNoRelatives.ToString(), Value = "2"},
                new SelectListItem { Text = ResidentialStatus.AsistedLivingFacility.ToString(), Value = "3"},
                new SelectListItem { Text = ResidentialStatus.FosterCare_GroupHome.ToString(), Value = "4"},
                new SelectListItem { Text = ResidentialStatus.Hospital_NursingHome.ToString(), Value = "5"},
                new SelectListItem { Text = ResidentialStatus.ResidentialProgram.ToString(), Value = "6"}
            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboEmployed()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = EmploymentStatus.EmployetFT.ToString(), Value = "0"},
                new SelectListItem { Text = EmploymentStatus.EmployetPT.ToString(), Value = "1"},
                new SelectListItem { Text = EmploymentStatus.Retired.ToString(), Value = "2"},
                new SelectListItem { Text = EmploymentStatus.Disabled.ToString(), Value = "3"},
                new SelectListItem { Text = EmploymentStatus.Homemaker.ToString(), Value = "4"},
                new SelectListItem { Text = EmploymentStatus.Student.ToString(), Value = "5"},
                new SelectListItem { Text = EmploymentStatus.Unemployed.ToString(), Value = "6"}
            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboWeeksNameByClinic(int idClinic)
        {
            List<SelectListItem> list = _context.Weeks
                                                .Where(w => w.Clinic.Id == idClinic)
                                                .Select(c => new SelectListItem
                                            {
                                                Text = $"{c.InitDate.ToLongDateString()} - {c.FinalDate.ToLongDateString()}",
                                                Value = $"{c.Id}"
                                            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select date range...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboDiagnosticsByClient(int idClient)
        {
            ClientEntity client = _context.Clients.Include(n => n.Clinic).FirstOrDefault(c => c.Id == idClient);
            ClinicEntity clinic = _context.Clinics.FirstOrDefault(c => c.Id == client.Clinic.Id);

            List<DiagnosticEntity> diagnostics = _context.Diagnostics.OrderBy(d => d.Code).ToList();
            List<Client_Diagnostic> diagnosticsForClient = _context.Clients_Diagnostics.Where(n => n.Client.Id == idClient).ToList();

            foreach (var item in diagnosticsForClient)
            {
                if (diagnostics.Contains(item.Diagnostic))
                {
                    diagnostics.Remove(item.Diagnostic);               
                }
            }

            List<SelectListItem> list = new List<SelectListItem>();
            list = diagnostics.Select(d => new SelectListItem
            {
                Text = $"{d.Code} {d.Description}",
                Value = $"{d.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select diagnostic...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboFARSType()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = FARSType.Initial.ToString(), Value = "0"},
                new SelectListItem { Text = FARSType.MtpReview.ToString(), Value = "1"},
                new SelectListItem { Text = FARSType.Discharge_PSR.ToString(), Value = "2"},
                new SelectListItem { Text = FARSType.Discharge_Ind.ToString(), Value = "3"},
                new SelectListItem { Text = FARSType.Discharge_Group.ToString(), Value = "4"},
                new SelectListItem { Text = FARSType.Addendums.ToString(), Value = "5"}
            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboUserNamesByClinic(int idClinic)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            if (idClinic == 0)
            {
                list = _context.Users.Select(u => new SelectListItem
                {
                    Text = $"{u.FullName}",
                    Value = $"{u.Id}"
                }).ToList();
            }
            else
            {
                list = _context.Users.Where(u => u.Clinic.Id == idClinic).OrderBy(n => n.FirstName).Select(u => new SelectListItem
                {
                    Text = $"{u.FullName}",
                    Value = $"{u.Id}"
                }).ToList();
            }

            list.Insert(0, new SelectListItem
            {
                Text = "[Select a person to assign this incident...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboClientsAdmissionByClinic(int idClinic)
        {
            List<SelectListItem> list = _context.Clients.Where(c => c.Clinic.Id == idClinic).OrderBy(n => n.Name)
                                                        .Select(c => new SelectListItem
                                                        {
                                                            Text = $"{c.Name + " | " + c.AdmisionDate.ToShortDateString()}",
                                                            Value = $"{c.Id}"
                                                        }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[All clients...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboBio_Type()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = Bio_Type.BIO.ToString(), Value = "0"},
                new SelectListItem { Text = Bio_Type.BRIEF.ToString(), Value = "1"}
            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboSchedulesByClinic(int idClinic, ServiceType service)
        {
            List<SelectListItem> list = _context.Schedule.Include(m => m.SubSchedules).Where(n => n.Clinic.Id == idClinic && n.Service == service).OrderBy(f => f.InitialTime).Select(f => new SelectListItem
            {
                Text = $"{f.Service} {f.Session} {f.InitialTime.ToShortTimeString()} - {f.EndTime.ToShortTimeString()} ({f.SubSchedules.Count()})",
                Value = $"{f.Id}"
            }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select Schedule...]",
                Value = "0"
            });
           

            return list;
        }

        public IEnumerable<SelectListItem> GetComboSession()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = SessionType.AM.ToString(), Value = "0"},
                new SelectListItem { Text = SessionType.PM.ToString(), Value = "1"},
                
            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboSchedulesForFacilitatorForDay(int idFacilitator, int idWorkday, int idClient, int idWorkdayClient)
        {
            List<ScheduleEntity> listSchedules = _context.Workdays_Clients.Where(n => n.Facilitator.Id == idFacilitator && n.Workday.Id == idWorkday && n.Schedule != null).Select(m => m.Schedule).Distinct().ToList();
            WorkdayEntity worday = _context.Workdays.FirstOrDefault(n => n.Id == idWorkday);

            List<Workday_Client> allNotes = _context.Workdays_Clients
                                                    .Include(n => n.Schedule)
                                                    .ThenInclude(n => n.SubSchedules)
                                                    .Include(n => n.Workday)
                                                    .Include(n => n.IndividualNote)
                                                    .ThenInclude(n => n.SubSchedule)
                                                    .Where(n => n.Workday.Date == worday.Date
                                                        && n.Client.Id == idClient
                                                        && n.Id != idWorkdayClient)
                                                    .ToList();
           
            List<ScheduleEntity> listSchedulesSalida = new List<ScheduleEntity>();
            if (allNotes.Count() > 0)
            {
                foreach (var item in listSchedules)
                {
                    if (!allNotes.Exists(n => ((n.IndividualNote.SubSchedule.InitialTime.TimeOfDay < item.InitialTime.TimeOfDay && n.IndividualNote.SubSchedule.EndTime.TimeOfDay > item.InitialTime.TimeOfDay) 
                                            || (n.IndividualNote.SubSchedule.InitialTime.TimeOfDay < item.EndTime.TimeOfDay && n.IndividualNote.SubSchedule.EndTime.TimeOfDay > item.EndTime.TimeOfDay)))
                        && !allNotes.Exists(n => ((n.IndividualNote.SubSchedule.InitialTime.TimeOfDay <= item.InitialTime.TimeOfDay && n.IndividualNote.SubSchedule.InitialTime.TimeOfDay <= item.EndTime.TimeOfDay) 
                                               && (n.IndividualNote.SubSchedule.EndTime.TimeOfDay >= item.InitialTime.TimeOfDay && n.IndividualNote.SubSchedule.EndTime.TimeOfDay >= item.EndTime.TimeOfDay)))
                        && !allNotes.Exists(n => ((n.IndividualNote.SubSchedule.InitialTime.TimeOfDay > item.InitialTime.TimeOfDay && n.IndividualNote.SubSchedule.InitialTime.TimeOfDay < item.InitialTime.TimeOfDay)
                                            || (n.IndividualNote.SubSchedule.EndTime.TimeOfDay > item.InitialTime.TimeOfDay && n.IndividualNote.SubSchedule.EndTime.TimeOfDay < item.EndTime.TimeOfDay)))
                        && !allNotes.Exists(n => ((n.IndividualNote.SubSchedule.InitialTime.TimeOfDay >= item.InitialTime.TimeOfDay && n.IndividualNote.SubSchedule.InitialTime.TimeOfDay <= item.EndTime.TimeOfDay)
                                               && (n.IndividualNote.SubSchedule.EndTime.TimeOfDay >= item.InitialTime.TimeOfDay && n.IndividualNote.SubSchedule.EndTime.TimeOfDay <= item.EndTime.TimeOfDay))))
                    {
                        listSchedulesSalida.Add(item);
                    }
                }
            }
            else
            {
                listSchedulesSalida = listSchedules;
            }

            List<SelectListItem> list = new List<SelectListItem>();

            if (listSchedulesSalida.Count() > 0)
            {
                list = listSchedulesSalida.Select(f => new SelectListItem
                {
                    Text = $"{f.Service} {f.Session} {f.InitialTime.ToShortTimeString()} - {f.EndTime.ToShortTimeString()}",
                    Value = $"{f.Id}"
                }).ToList();

            }

            list.Insert(0, new SelectListItem
            {
                Text = "[Select Schedule...]",
                Value = "0"
            });


            return list;
        }

        public IEnumerable<SelectListItem> GetComboSubSchedulesForFacilitatorForDay(int idFacilitator, int idWorkday, int idSchedule, int idClient, int idWorkdayClient)
        {
            List<SubScheduleEntity> listSubSchedules = new List<SubScheduleEntity>();
            List<IndividualNoteEntity> listNotes = _context.IndividualNotes
                                                           .Include(n => n.SubSchedule)
                                                           .ThenInclude(n => n.Schedule)
                                                           .Where(n => n.Workday_Cient.Facilitator.Id == idFacilitator 
                                                                && n.Workday_Cient.Workday.Id == idWorkday)
                                                           .ToList();

            List<Workday_Client> workday_Clients = _context.Workdays_Clients
                                                           .Include(n => n.Schedule)
                                                           .ThenInclude(n => n.SubSchedules)
                                                           .Where(n => n.Facilitator.Id == idFacilitator 
                                                                    && n.Workday.Id == idWorkday
                                                                    && n.Client == null).ToList();

            foreach (var item in workday_Clients)
            {
                if (item.Schedule != null)
                {
                    string time = item.Session.Substring(0, 7);
                    foreach (var value in item.Schedule.SubSchedules)
                    {
                        if (value.InitialTime.ToShortTimeString().ToString().Contains(time) == true)
                        {
                            listSubSchedules.Add(value);
                            break;
                        }
                    }

                }
            }

            foreach (var item in listNotes)
            {
                listSubSchedules.Remove(item.SubSchedule);
            }

            WorkdayEntity worday = _context.Workdays.FirstOrDefault(n => n.Id == idWorkday);

            List<Workday_Client> allNotes = _context.Workdays_Clients
                                                    .Include(n => n.Schedule)
                                                    .Where(n => n.Workday.Date == worday.Date
                                                        && n.Client.Id == idClient
                                                        && n.Id != idWorkdayClient)
                                                    .ToList();

            List<SubScheduleEntity> listSubSchedulesSalida = new List<SubScheduleEntity>();
            if (allNotes.Count() > 0)
            {
                foreach (var item in listSubSchedules)
                {
                    if ((!allNotes.Exists(n => (n.Schedule.InitialTime.TimeOfDay < item.InitialTime.TimeOfDay && n.Schedule.EndTime.TimeOfDay > item.InitialTime.TimeOfDay || n.Schedule.InitialTime.TimeOfDay < item.EndTime.TimeOfDay && n.Schedule.EndTime.TimeOfDay > item.EndTime.TimeOfDay))
                           && !allNotes.Exists(n => (n.Schedule.InitialTime.TimeOfDay < item.InitialTime.TimeOfDay && n.Schedule.InitialTime.TimeOfDay < item.EndTime.TimeOfDay && n.Schedule.EndTime.TimeOfDay > item.InitialTime.TimeOfDay && n.Schedule.EndTime.TimeOfDay > item.EndTime.TimeOfDay)))
                        ||
                        (allNotes.Exists(n => (n.Schedule.InitialTime.TimeOfDay < item.InitialTime.TimeOfDay && n.Schedule.EndTime.TimeOfDay > item.InitialTime.TimeOfDay || n.Schedule.InitialTime.TimeOfDay < item.EndTime.TimeOfDay && n.Schedule.EndTime.TimeOfDay > item.EndTime.TimeOfDay) && n.Present == false)
                        || allNotes.Exists(n => (n.Schedule.InitialTime.TimeOfDay < item.InitialTime.TimeOfDay && n.Schedule.InitialTime.TimeOfDay < item.EndTime.TimeOfDay && n.Schedule.EndTime.TimeOfDay > item.InitialTime.TimeOfDay && n.Schedule.EndTime.TimeOfDay > item.EndTime.TimeOfDay && n.Present == false))))
                    {
                        listSubSchedulesSalida.Add(item);
                    }
                }
            }
            else
            {
                listSubSchedulesSalida = listSubSchedules;
            }
            
            List<SelectListItem> list = new List<SelectListItem>();

            if (listSubSchedulesSalida.Count() > 0)
            {
                list = listSubSchedulesSalida.Select(f => new SelectListItem
                {
                    Text = $"{f.InitialTime.ToShortTimeString()} - {f.EndTime.ToShortTimeString()}",
                    Value = $"{f.Id}"
                }).ToList();

            }

            list.Insert(0, new SelectListItem
            {
                Text = "[Select Schedule...]",
                Value = "0"
            });


            return list;
        }

        public IEnumerable<SelectListItem> GetComboThemeType()
        {
            List<SelectListItem> list = new List<SelectListItem>
            {
                new SelectListItem { Text = ServiceType.PSR.ToString(), Value = "0"},
                new SelectListItem { Text = ServiceType.Group.ToString(), Value = "1"}
            };

            return list;
        }

        public IEnumerable<SelectListItem> GetComboTypeReferred()
        {
            List<SelectListItem> list = new List<SelectListItem>
                                { new SelectListItem { Text = ReferredType.In.ToString(), Value = "0"},
                                  new SelectListItem { Text = ReferredType.Out.ToString(), Value = "1"}};

            return list;
        }

        public IEnumerable<SelectListItem> GetComboTCMSupervisorByClinic(int idClinic)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            if (idClinic == 0)
            {
                list = _context.TCMSupervisors.Select(u => new SelectListItem
                {
                    Text = $"{u.Name}",
                    Value = $"{u.Id}"
                }).ToList();
            }
            else
            {
                list = _context.TCMSupervisors.Where(u => (u.Clinic.Id == idClinic)).Select(u => new SelectListItem
                {
                    Text = $"{u.Name}",
                    Value = $"{u.Id}"
                }).ToList();
            }

            list.Insert(0, new SelectListItem
            {
                Text = "[Select TCM Supervisor...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboCaseManagersByTCMSupervisor(string user)
        {
            List<SelectListItem> list = _context.CaseManagers

                                                .Where(c => c.TCMSupervisor.LinkedUser == user)
                                                .Select(c => new SelectListItem
                                                {
                                                    Text = $"{c.Name} ",
                                                    Value = $"{c.Id}"
                                                }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[All TCM...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboRisk()
        {
            List<SelectListItem> list = new List<SelectListItem>
                                { new SelectListItem { Text = RiskType.Low.ToString(), Value = "1"},
                                  new SelectListItem { Text = RiskType.Moderate.ToString(), Value = "2"},
                                  new SelectListItem { Text = RiskType.High.ToString(), Value = "3"}};

            list.Insert(0, new SelectListItem
            {
                Text = "[Select risk...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboTCMClientsByCaseManagerByTCMSupervisor(string user)
        {
            List<SelectListItem> list = _context.TCMClient
                                                .Include(n => n.Client)
                                                .Where(c => (c.Casemanager.TCMSupervisor.LinkedUser == user))
                                                .Select(c => new SelectListItem
                                                {
                                                    Text = $"{c.Client.Name} ",
                                                    Value = $"{c.Id}"
                                                }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[All Clients...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboTCMClientsByClinic(int idClinic = 0)
        {
            List<SelectListItem> list = _context.TCMClient
                                                .Include(n => n.Client)
                                                .Where(c => c.Client.Clinic.Id == idClinic)
                                                .Select(c => new SelectListItem
                                                {
                                                    Text = $"{c.Client.Name} ",
                                                    Value = $"{c.Id}"
                                                }).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[All Clients...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboSiteStatus()
        {
            List<SelectListItem> list = new List<SelectListItem>
                                { new SelectListItem { Text = CiteStatus.S.ToString(), Value = "1"},
                                  new SelectListItem { Text = CiteStatus.C.ToString(), Value = "2"},
                                  new SelectListItem { Text = CiteStatus.R.ToString(), Value = "3"},
                                  new SelectListItem { Text = CiteStatus.NS.ToString(), Value = "4"},
                                  new SelectListItem { Text = CiteStatus.AR.ToString(), Value = "5"},
                                  new SelectListItem { Text = CiteStatus.CO.ToString(), Value = "6"},
                                  new SelectListItem { Text = CiteStatus.A.ToString(), Value = "7"},
                                  new SelectListItem { Text = CiteStatus.X.ToString(), Value = "8"}};

            list.Insert(0, new SelectListItem
            {
                Text = "[Select status...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboClientByIndfacilitator(int idFacilitator)
        {
            List<SelectListItem> list = _context.Clients.Where(u => u.IndividualTherapyFacilitator.Id == idFacilitator).Select(u => new SelectListItem
            {
                Text = $"{u.Name}",
                Value = $"{u.Id}"
            }).ToList();        
            
            list.Insert(0, new SelectListItem
            {
                Text = "[Select client...]",
                Value = "0"
            });

            return list;
        }

        public IEnumerable<SelectListItem> GetComboSchedulesByClinicForCites(int idClinic, ServiceType service, int idFacilitator, DateTime date)
        {
            FacilitatorEntity facilitator = _context.Facilitators
                                                    .Include(n => n.Groups)
                                                    .ThenInclude(n => n.Schedule)
                                                    .ThenInclude(n => n.SubSchedules)
                                                    .FirstOrDefault(n => n.Id == idFacilitator);
            List<SelectListItem> list = new List<SelectListItem>();
            if (facilitator != null && facilitator.Groups.Count() > 0)
            {
                List<SubScheduleEntity> aux = SubScheduleAvailable(idFacilitator, date);


                list = aux.OrderBy(n => n.InitialTime).Select(f => new SelectListItem
                                                    {
                                                        Text = $"{f.InitialTime.ToShortTimeString()} - {f.EndTime.ToShortTimeString()}",
                                                        Value = $"{f.Id}"
                                                    }).ToList();

            }

            list.Insert(0, new SelectListItem
            {
                Text = "[Select Schedule...]",
                Value = "0"
            });


            return list;
        }

        public List<SubScheduleEntity> SubScheduleAvailable(int idFacilitator, DateTime date)
        {
            FacilitatorEntity facilitator = _context.Facilitators
                                                    .Include(n => n.Groups)
                                                    .ThenInclude(n => n.Schedule)
                                                    .ThenInclude(n => n.SubSchedules)
                                                    .FirstOrDefault(n => n.Id == idFacilitator);

            List<SubScheduleEntity> subSchedule = facilitator.Groups
                                                             .FirstOrDefault(n => n.Service == ServiceType.Individual)
                                                             .Schedule
                                                             .SubSchedules
                                                             .ToList();

            List<CiteEntity> cites = _context.Cites
                                             .Include(n => n.SubSchedule)
                                             .Where(n => n.Facilitator.Id == idFacilitator 
                                                      && n.DateCite == date)
                                             .ToList();

            foreach (var item in cites)
            {
                subSchedule.Remove(item.SubSchedule);
            }

            return subSchedule;
        }

        public IEnumerable<SelectListItem> GetComboConsentType()
        {
            List<SelectListItem> list = new List<SelectListItem>
                                { new SelectListItem { Text = ConsentType.HURRICANE.ToString(), Value = "1"},
                                  new SelectListItem { Text = ConsentType.PCP.ToString(), Value = "2"},
                                  new SelectListItem { Text = ConsentType.PSYCHIATRIST.ToString(), Value = "3"},
                                  new SelectListItem { Text = ConsentType.EMERGENCY_CONTACT.ToString(), Value = "4"},
                                  new SelectListItem { Text = ConsentType.DCF.ToString(), Value = "5"},
                                  new SelectListItem { Text = ConsentType.SSA.ToString(), Value = "6"},
                                  new SelectListItem { Text = ConsentType.BANK.ToString(), Value = "7"},
                                  new SelectListItem { Text = ConsentType.HOUSING_OFFICES.ToString(), Value = "8"},
                                  new SelectListItem { Text = ConsentType.POLICE_STATION.ToString(), Value = "9"},
                                  new SelectListItem { Text = ConsentType.PHARMACY.ToString(), Value = "10"},
                                  new SelectListItem { Text = ConsentType.MEDICAL_INSURANCE.ToString(), Value = "11"},
                                  new SelectListItem { Text = ConsentType.CAC.ToString(), Value = "12"},
                                  new SelectListItem { Text = ConsentType.LIFELINESS_PROVIDERS.ToString(), Value = "13"},
                                  new SelectListItem { Text = ConsentType.TAG_AGENCY.ToString(), Value = "14"},
                                  new SelectListItem { Text = ConsentType.STS.ToString(), Value = "15"},
                                  new SelectListItem { Text = ConsentType.DONATION_CENTERS.ToString(), Value = "16"},
                                  new SelectListItem { Text = ConsentType.LTC.ToString(), Value = "17"},
                                  new SelectListItem { Text = ConsentType.INTERNET_SERVICES.ToString(), Value = "18"},
                                  new SelectListItem { Text = ConsentType.USCIS.ToString(), Value = "19"}
                                };

            list.Insert(0, new SelectListItem
            {
                Text = "[Select Classification...]",
                Value = "0"
            });

            return list;
        }

    }
}
