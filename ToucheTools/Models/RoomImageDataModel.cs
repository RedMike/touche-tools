namespace ToucheTools.Models;

public class RoomImageDataModel
{
    public int Width { get; set; }
    public int Height { get; set; }
    public byte[,] RawData { get; set; } = new byte[0, 0];
    public int RoomWidth { get; set; }
}