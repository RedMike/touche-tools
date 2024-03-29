﻿namespace ToucheTools.App.Models;

public class RoomModel
{
    public int? RoomImageIndex { get; set; } = null;
    
    //does not include point 0 which is static and not walkable
    public Dictionary<int, (int, int, int)> WalkablePoints { get; set; } = new Dictionary<int, (int, int, int)>();
    public Dictionary<(int, int), (int, int, int)> WalkableLines { get; set; } = new Dictionary<(int, int), (int, int, int)>();
    
    public Dictionary<int, HitboxModel> Hitboxes { get; set; } = new Dictionary<int, HitboxModel>();

    public Dictionary<int, string> Texts { get; set; } = new Dictionary<int, string>();

    public Dictionary<int, BackgroundAreaModel> BackgroundAreas { get; set; } =
        new Dictionary<int, BackgroundAreaModel>();
}