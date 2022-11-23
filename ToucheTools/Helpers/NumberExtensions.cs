namespace ToucheTools.Helpers;

public static class NumberExtensions
{
    public static ushort AsUshort(this short a)
    {
        return BitConverter.ToUInt16(BitConverter.GetBytes(a), 0);
    }
    
    public static short AsShort(this ushort a)
    {
        return BitConverter.ToInt16(BitConverter.GetBytes(a), 0);
    }
}