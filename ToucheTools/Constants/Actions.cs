namespace ToucheTools.Constants;

public static class Actions
{
    public const int DoNothing = -26;
    public const int LeftClick = -49;
    public const int LeftClickWithItem = -53;

    public static readonly int[] BuiltInActions = new [] {
        DoNothing, LeftClick, LeftClickWithItem
    };
}