namespace KyoS.Common.Enums
{
    public enum ReferredType
    {
        In,
        Out
    }

    public class ReferredUtils
    {
        public static ReferredType GetTypeReferredByIndex(int index)
        {
            return (index == 0) ? ReferredType.In :
                   (index == 1) ? ReferredType.Out : ReferredType.In;
        }
    }

   
}
