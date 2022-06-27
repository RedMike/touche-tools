namespace ToucheTools.Models;

public class TextDataModel
{
    public uint Offsets { get; set; }
    public uint Size { get; set; }
    
    public byte[] Data { get; set; }
    public Dictionary<int, string> Strings { get; set; } = new Dictionary<int, string>();
}