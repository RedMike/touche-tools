namespace ToucheTools.App.Models;

public class BackgroundAreaModel
{
    public int? DestX { get; set; } = null;
    public int? DestY { get; set; } = null;
    public int Width { get; set; } = 100;
    public int Height { get; set; } = 100;
    public int SourceX { get; set; } = 400;
    public int SourceY { get; set; } = 100;
    public int? ScaledOffset { get; set; } = null; //if null, not scaled
    public int ScaleMul { get; set; } = 1;
    public int ScaleDiv { get; set; } = 1;

    /// <summary>
    /// Non-dynamic background areas are saved as Backgrounds, so are drawn on top of characters/background each frame.
    /// They can be enabled/disabled via script or be on by default.
    /// 
    /// Dynamic background areas are saved as Areas, so can be drawn before or after characters, each frame or once.
    /// They can be enabled/disabled via script but cannot be on by default.
    /// They can also be attached to walks to be on during walks.
    /// </summary>
    public bool Dynamic { get; set; } = false;
}