namespace ToucheTools.Models;

public class SpriteImageDataModel
{
    public short Width { get; set; }
    public short Height { get; set; }
    public short SpriteWidth { get; set; }
    public short SpriteHeight { get; set; }
    public byte[,] RawData { get; set; } = new byte[0, 0];
    public byte[,] DecodedData { get; set; } = new byte[0, 0];
}