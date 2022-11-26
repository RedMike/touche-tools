namespace ToucheTools.Constants;

public static class Game
{
    public const int RoomHeight = 352;
    public const int ScreenWidth = 640;
    public const int ScreenHeight = 400;
    public const int StartupEpisode = 90;
    public const int MaxProgramDataSize = 61440;
    public const int ZDepthEven = 160;
    public const int ZDepthMin = 1;
    public const int ZDepthMax = 500;
    public const int ZDepthSteps = 256; //for above even and under even separately

    public const int ZRatioMin = 4;
    public const int ZRatioMax = 2;
    
    public static float GetZFactor(int z)
    {
        if (z == ZDepthEven)
        {
            return 1.0f;
        }
        
        if (z < ZDepthMin)
        {
            z = ZDepthMin;
        }
        if (z > ZDepthMax)
        {
            z = ZDepthMax;
        }
        
        if (z < ZDepthEven)
        {
            var rz = (ZDepthEven - (float)z)/ZDepthEven;
            return 1.0f/(rz * (ZRatioMin - 1.0f) + 1.0f);
        }
        else
        {
            var rz = ((float)z - ZDepthEven) / (ZDepthMax - ZDepthEven);
            return rz * (ZRatioMax - 1.0f) + 1.0f;
        }
    }
}