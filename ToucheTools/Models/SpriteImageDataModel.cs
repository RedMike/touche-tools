namespace ToucheTools.Models;

public class SpriteImageDataModel
{
    public int Width { get; set; }
    public int Height { get; set; }
    public byte[,] RawData { get; set; } = new byte[0, 0];
}