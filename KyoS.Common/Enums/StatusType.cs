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
                   (index == 1) ? IncidentsStatus.Solved :
                   (index == 2) ? IncidentsStatus.NotValid :
                   (index == 3) ? IncidentsStatus.Reviewed : IncidentsStatus.NotValid;
        }
        public static SPRStatus GetSPRStatusByIndex(int index)
        {
            return (index == 0) ? SPRStatus.Open :
                   (index == 1) ? SPRStatus.Closed :
                   (index == 2) ? SPRStatus.Added : SPRStatus.Open;
        }
        public static TCMPayStubFiltro GetFiltroTCMPayStubByIndex(int index)
        {
            return (index == 0) ? TCMPayStubFiltro.Created :
                   (index == 1) ? TCMPayStubFiltro.Approved :
                   (index == 2) ? TCMPayStubFiltro.Billed :
                   (index == 3) ? TCMPayStubFiltro.Paid : TCMPayStubFiltro.Created;
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
        NotValid,
        Reviewed
    }
    public enum AdendumStatus
    {
        Edition,
        Pending,
        Approved
    }
    public enum FarsStatus
    {
        Edition,
        Pending,
        Approved
    }
    public enum DischargeStatus
    {
        Edition,
        Pending,
        Approved
    }
    public enum TCMDocumentStatus
    {
        Edition,
        Pending,
        Approved
    }
    public enum MTPStatus
    {
        Edition,
        Pending,
        Approved
    }
    public enum BioStatus
    {
        Edition,
        Pending,
        Approved
    }
    public enum SPRStatus
    {
        Open,
        Closed,
        Added
    }
    public enum SafetyPlanStatus
    {
        Edition,
        Pending,
        Approved
    }
    public enum TCMPayStubFiltro
    {
        Created,
        Approved,
        Billed,
        Paid
    }
}
