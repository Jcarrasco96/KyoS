using KyoS.Common.Enums;

namespace KyoS.Web.Data.Abstract
{
    public abstract class LegalContact : Contact
    {
        public string Country { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string TelephoneSecondary { get; set; }
        public string AdressLine2 { get; set; }
        public virtual RelationshipType RelationShip { get; set; }
    }
}
