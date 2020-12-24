using KyoS.Common.Enums;
using KyoS.Web.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
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
                                  new SelectListItem { Text = UserType.Operator.ToString(), Value = "2"},
                                  new SelectListItem { Text = UserType.Admin.ToString(), Value = "3"}};
            
            list.Insert(0, new SelectListItem
            {
                Text = "[Select rol...]",
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
            }).OrderBy(t => t.Text).ToList();

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
            }).OrderBy(f => f.Text).ToList();

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
            }).OrderBy(c => c.Text).ToList();

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
            }).OrderBy(a => a.Value).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select activity...]",
                Value = "0"
            });

            return list;
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
            }).OrderBy(a => a.Value).ToList();

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
            }).OrderBy(g => g.Text).ToList();

            list.Insert(0, new SelectListItem
            {
                Text = "[Select group...]",
                Value = "0"
            });

            return list;
        }
    }
}
