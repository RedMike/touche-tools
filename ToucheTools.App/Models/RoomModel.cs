namespace ToucheTools.App.Models;

public class RoomModel
{
    public int? RoomImageIndex { get; set; } = null;
    
    //does not include point 0 which is static and not walkable
    public Dictionary<int, (int, int, int)> WalkablePoints { get; set; } = new Dictionary<int, (int, int, int)>();
    public Dictionary<(int, int), (int, int, int)> WalkableLines { get; set; } = new Dictionary<(int, int), (int, int, int)>();
    
    //does not include inventory or keychar hitboxes
    public HashSet<HitboxModel> Hitboxes { get; set; } = new HashSet<HitboxModel>();
}