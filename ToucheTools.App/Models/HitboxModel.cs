namespace ToucheTools.App.Models;

public class HitboxModel
{
    //TODO: redraw rect
    //TODO: fallback action?
    /// <summary>
    /// For inventory hitboxes, it's the item | 0x1000
    /// For characters it's the keychar | 0x4000
    /// For others it's an ID
    /// </summary>
    public int Item { get; set; }
    public bool Displayed { get; set; }
    public string Label { get; set; } = "~~~";
    public string SecondaryLabel { get; set; } = "~~~~";
    public int X { get; set; }
    public int Y { get; set; }
    public int W { get; set; }
    public int H { get; set; }
}