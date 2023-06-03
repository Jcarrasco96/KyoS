namespace KyoS.Common.Enums
{
    public enum SessionType
    {
        AM,
        PM
    }

    public class SessionUtils
    {
        public static SessionType GetSessionByIndex(int index)
        {
            return (index == 0) ? SessionType.AM :
                   (index == 1) ? SessionType.PM : SessionType.AM;
        }
    }

   
}
