namespace KyoS.Common.Enums
{
    public enum ServiceTCMNoteType
    {
        School,
        Office,
        Home,
        ALF,
        Other
    }

    public class ServiceTCMNotesUtils
    {
        public static ServiceTCMNoteType GetServiceByIndex(int index)
        {
            return (index == 0) ? ServiceTCMNoteType.School :
                   (index == 1) ? ServiceTCMNoteType.Office :
                   (index == 2) ? ServiceTCMNoteType.Home :
                   (index == 3) ? ServiceTCMNoteType.ALF :
                   (index == 4) ? ServiceTCMNoteType.Other : ServiceTCMNoteType.Other;
        }

        public static string GetCodeByIndex(int index)
        {
            return (index == 1) ? "03" :
                   (index == 2) ? "11":
                   (index == 3) ? "12":
                   (index == 4) ? "33":
                   (index == 5) ? "99": "99";
        }

        public static int  GetIndexByCode(string code)
        {
            return (code == "03") ? 1 :
                   (code == "11") ? 2 :
                   (code == "12") ? 3 :
                   (code == "33") ? 4 :
                   (code == "99") ? 5 : 5;
        }
    }
}
