using KyoS.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace KyoS.Web.Data.Contracts
{
    public class Problem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Active { get; set; }
         
    }
}
