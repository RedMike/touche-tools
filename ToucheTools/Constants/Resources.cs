namespace ToucheTools.Constants;

public enum Resource
{
    Unknown = 0,
    
    RoomImage = 1,
    Sequence = 2,
    SpriteImage = 3,
    IconImage = 4,
    RoomInfo = 5,
    Program = 6,
    Music = 7,
    Sound = 8,
}

public class OffsetCount
{
    public int Offset { get; set; }
    public int Count { get; set; }
}

public static class Resources
{
    public static int BackdropWidth = 544;
    public static int BackdropHeight = 1996;
    
    public static Dictionary<Resource, OffsetCount> DataInfo = new Dictionary<Resource, OffsetCount>()
    {
        {
            Resource.RoomImage, new OffsetCount() { Offset = 0x048, Count = 100 }
        },
        {
            Resource.Sequence, new OffsetCount() { Offset = 0x228, Count = 30 }
        },
        {
            Resource.SpriteImage, new OffsetCount() { Offset = 0x2A0, Count = 50 }
        },
        {
            Resource.IconImage, new OffsetCount() { Offset = 0x390, Count = 100 }
        },
        {
            Resource.RoomInfo, new OffsetCount() { Offset = 0x6B0, Count = 80 }
        },
        {
            Resource.Program, new OffsetCount() { Offset = 0x908, Count = 150 }
        },
        {
            Resource.Music, new OffsetCount() { Offset = 0xB60, Count = 50 }
        },
        {
            Resource.Sound, new OffsetCount() { Offset = 0xC28, Count = 120 }
        },
    };

    public static int MenuKitSpriteImage = 18;
    public static int ConversionKitSpriteImage = 19;
}