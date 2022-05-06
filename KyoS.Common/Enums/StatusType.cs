namespace KyoS.Common.Enums
{
    public enum StatusType
    {
        Open,
        Close
    }
    public class StatusUtils
    {
        public static StatusType GetStatusByIndex(int index)
        {
            return (index == 1) ? StatusType.Open :
                   (index == 2) ? StatusType.Close: StatusType.Open;
        }
        public static IncidentsStatus GetIncidentStatusByIndex(int index)
        {
            return (index == 0) ? IncidentsStatus.Pending:
                   (index == 1) ? IncidentsStatus.Solved : IncidentsStatus.NotValid;
        }
    }
    public enum NoteStatus
    {
        Edition,
        Pending,
        Approved
    }
    public enum MessageStatus
    {
        Read,
        NotRead
    }
    public enum ActivityStatus
    {
        Pending,
        Approved
    }
    public enum IncidentsStatus
    {
        Pending,
        Solved,
        NotValid
    }
    public enum AdendumStatus
    {
        Edition,
        Pending,
        Approved
    }
}
