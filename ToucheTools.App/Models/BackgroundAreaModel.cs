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
}