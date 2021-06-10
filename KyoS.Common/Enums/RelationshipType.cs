namespace KyoS.Common.Enums
{
    public enum RelationshipType
    {
        Unknown,
        Brother,
        Child,
        Daugther,
        Father,
        Friend,
        Guardian,
        Mother,
        Psychiatrist,
        Self,
        Sibling,
        Sister,
        Spouse,
        Son,
        Other
    }

    public class RelationshipUtils
    {
        public static RelationshipType GetRelationshipByIndex(int index)
        {
            return (index == 0) ? RelationshipType.Unknown :
                   (index == 1) ? RelationshipType.Brother :
                   (index == 2) ? RelationshipType.Child :
                   (index == 3) ? RelationshipType.Daugther :
                   (index == 4) ? RelationshipType.Father :
                   (index == 5) ? RelationshipType.Friend :
                   (index == 6) ? RelationshipType.Guardian :
                   (index == 7) ? RelationshipType.Mother :
                   (index == 8) ? RelationshipType.Psychiatrist :
                   (index == 9) ? RelationshipType.Self :
                   (index == 10) ? RelationshipType.Sibling :
                   (index == 11) ? RelationshipType.Sister :
                   (index == 12) ? RelationshipType.Spouse :
                   (index == 13) ? RelationshipType.Son :
                   (index == 14) ? RelationshipType.Other : RelationshipType.Unknown;
        }
    }
}
