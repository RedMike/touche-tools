namespace ToucheTools.Models;

public class IconImageDataModel
{
    public int Width { get; set; }
    public int Height { get; set; }
    public byte[,] RawData { get; set; } = new byte[0, 0];
    public byte[,] DecodedData { get; set; } = new byte[0, 0];
}