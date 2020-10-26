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
    }
}
