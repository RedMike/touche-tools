namespace ToucheTools.Constants;

public static class Palettes
{
    public const int SpriteColorCount = 62; //the number of colours reserved for sprites after the start of it
    public const int StartOfSpriteColors = 193; //this colour and everything after is used for decoding sprites (1 becomes 193, ...)
    public const int TransparencyColor = 0;
    public const int TransparentSpriteMarkerColor = 64; //on x = 0 and y = 0, marks sprite tile size
    public const int TransparentRoomMarkerColor = 255; //on y = 0, marks room width (only width)
}