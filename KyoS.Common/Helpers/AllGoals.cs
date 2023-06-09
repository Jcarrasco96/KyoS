using KyoS.Common.Enums;
using System;
using System.Collections.Generic;

namespace KyoS.Common.Helpers
{
    public class AllGoals
    {
        public int NumberGoal { get; set; }
        public string Name { get; set; }
        public string AreaFocus { get; set; }
        public ServiceType Service { get; set; }
        public List<AllObjectives> AllObjectives { get; set; }
        public string Document { get; set; }
        public bool Compliment { get; set; }
        public int IdMtp { get; set; }
    }
}
