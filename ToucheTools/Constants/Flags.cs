namespace ToucheTools.Constants;

public static class Flags
{
    public enum Known
    {
        StartedEpisodeFlag = 0,
        
        CurrentKeyChar = 104,
        KeepPaletteOnRoomLoad = 115,
        CurrentMoney = 118, 
        CurrentCursorObject = 119, 
        KeyCharMaxDirection = 176, 
        KeyCharDirectionOverride = 266,
        //NoDecodeImages = 267, //internal to loading logic??
        //NoDecodeOtherImages = 268, //internal to loading logic??
        DisableBackgroundAnim = 269,
        
        PlayRandomSound = 270, // 16 => play a sound from the range flags[273 + rnd(0, 16)]
        RndSndMinDelay = 271, //delay next change for at least X frames
        RndSndRandomDelay = 272, //delay next change for between X, and X+rnd(Y) frames
        RndSndPotential1 = 273, // potential sound 1
        RndSndPotential2 = 274, // potential sound 2
        //...
        
        ProcessRandomPalette = 290,
        RndPalMinColour = 291, //240 => the scale is at least 240
        RndPalRandomRange = 292, //16 => the scale is between 240 and 256
        RndPalMinDelay = 293, //delay next change for at least X frames
        RndPalRandomDelay = 294, //delay next change for between X, and X+rnd(Y) frames
        
        GameCycleCounter1 = 295,
        GameCycleCounter2 = 296,
        GameCycleCounter3 = 297, //cycles since the last speech
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
            return $"F{flag} ({(Known)flag:G})";
        }

        return $"F{flag}";
    }
}