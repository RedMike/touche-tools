namespace ToucheTools.Constants;

public static class Flags
{
    public static readonly Dictionary<int, string> KnownFlags = new Dictionary<int, string>()
    {
        {ScreenOffsetX, "ScreenOffsetX"},
        {ScreenOffsetY, "ScreenOffsetY"},
    };
    
    public const int ScreenOffsetX = 614;
    public const int ScreenOffsetY = 615;
}