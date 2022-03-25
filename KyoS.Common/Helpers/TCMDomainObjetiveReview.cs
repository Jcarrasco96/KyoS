using System;
using System.Collections.Generic;
using System.Text;

namespace KyoS.Common.Helpers
{
    class TCMDomainObjetiveReview
    {
        public int ID { get; set; }
        public int status { get; set; }
        public string texto { get; set; }
        public TCMObjetiveReview ObjectiveList { get; set; }
    }
}
