using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class Clinic_Theme
    {
        public int Id { get; set; }
        public ClinicEntity Clinic { get; set; }
        public ThemeEntity Theme { get; set; }
        public DateTime Date { get; set; }
    }
}
