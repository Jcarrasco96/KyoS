﻿namespace KyoS.Common.Enums
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
}
