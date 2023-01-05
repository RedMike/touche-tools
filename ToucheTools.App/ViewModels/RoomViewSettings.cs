namespace ToucheTools.App.ViewModels;

public class RoomViewSettings
{
    public bool ShowRects { get; set; } = true;
    public bool ShowBackgrounds { get; set; } = true;
    public bool ShowAreas { get; set; } = true;
    public bool ShowPoints { get; set; } = true;
    
    public int AreaOffsetX { get; set; }
    public int AreaOffsetY { get; set; }
}