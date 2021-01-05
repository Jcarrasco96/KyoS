using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KyoS.Web.Data.Entities
{
    public class NotePrototype_Classification
    {
        public int Id { get; set; }

        public NotePrototypeEntity Note { get; set; }

        public ClassificationEntity Classification { get; set; }
    }
}
