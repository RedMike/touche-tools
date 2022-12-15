namespace ToucheTools.Constants;

public static class Palettes
{
    public const int RoomColorCount = StartOfSpriteColors - 3; //not including 0, 64, 192
    public const int SpriteColorCount = 62; //the number of colours reserved for sprites after the start of it
    public const int StartOfSpriteColors = 193; //this colour and everything after is used for decoding sprites (1 becomes 193, ...)
    public const int TransparencyColor = 0;
    public const int TransparentSpriteMarkerColor = 64; //on x = 0 and y = 0, marks sprite tile size
    /// <summary>
    /// Also doubles as the UI text colour for hitboxes and hover colour for action menus
    /// </summary>
    public const int TransparentRoomMarkerColor = 255; //on y = 0, marks room width (only width)

    public const int InventoryBackgroundColor = 210;
    public const int ConversationTextColor = 214;
    public const int InventoryMoneyTextColor = 217;
    public const int ActionMenuBackgroundColor = 248;
    public const int ActionMenuTextColor = 249;

    public static readonly int[] SpecialColors =
    {
        TransparencyColor, TransparentSpriteMarkerColor, TransparentRoomMarkerColor,
        InventoryBackgroundColor, ConversationTextColor, InventoryMoneyTextColor,
        ActionMenuBackgroundColor, ActionMenuTextColor
    };
}