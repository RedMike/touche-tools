namespace ToucheTools.App.Models;

public class HitboxModel
{
    public enum HitboxType
    {
        Unknown = 0,
        Normal = 1, //clickable area on the screen
        Inventory = 2, //represents an item in the inventory
        Disabled = 3, //does nothing?
        KeyChar = 4, //position is taken from a key character
    }
    
    //TODO: redraw rect
    //TODO: actions
    public int Item { get; set; }
    public HitboxType Type { get; set; } = HitboxType.Unknown;
    public string Label { get; set; } = "~~~";
    public string SecondaryLabel { get; set; } = "~~~~";
    public int X { get; set; }
    public int Y { get; set; }
    public int W { get; set; }
    public int H { get; set; }
}