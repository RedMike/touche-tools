namespace ToucheTools.Constants;

public static class Flags
{
    public enum Known
    {
        StartedEpisode = 0,
        KeepPaletteOnRoomLoad = 115,
        CurrentMoney = 118, 
        CurrentCursorObject = 119, 
        KeyCharMaxDirection = 176, 
        KeyCharDirectionOverride = 266,
        //NoDecodeImages = 267, //internal to loading logic??
        //NoDecodeOtherImages = 268, //internal to loading logic??
        DisableBackgroundAnim = 269,
        PlayRandomSound = 270,
        ProcessRandomPalette = 290,
        
        GameCycleCounter1 = 295,
        GameCycleCounter2 = 296,
        GameCycleCounter3 = 297,
        GameCycleCounterDec1 = 298,
        GameCycleCounterDec2 = 299,
        
        LastAsciiKeyPress = 600,
        FadePaletteScaleIncrement = 603,
        FadePaletteScale = 605,
        DisableInventoryDraw = 606,
        FadePaletteFirstColour = 607,
        FadePaletteLastColour = 608,
        FadePaletteMaxScale = 609,
        FadePaletteMinScale = 610,
        
        QuitGame = 611,
        RandomNumberMod = 612,
        LastRandomNumber = 613,
        
        RoomScrollX = 614,
        RoomScrollY = 615,
        DisableRoomScroll = 616,
        
        CurrentSpeechFile = 617,
        HideCursor = 618,
        
        EnableFrench = 621, //?
        
        DebugDrawWalks = 902,
        LoadExternalScripts = 911, //not implemented in ScummVM
    }

    public static string GetFlagText(ushort flag)
    {
        if (Enum.IsDefined(typeof(Known), (int)flag))
        {
            return ((Known)flag).ToString("G");
        }

        return $"F{flag}";
    }
}