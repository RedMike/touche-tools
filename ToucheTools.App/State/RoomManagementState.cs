﻿namespace ToucheTools.App.State;

public class RoomManagementState
{
    public string? SelectedRoom { get; set; } = null;
    public bool PreviewOpen { get; set; } = false;
    public bool EditorOpen { get; set; } = false;
    
    public int WalkableLinePoint1 { get; set; }
    public int WalkableLinePoint2 { get; set; }
}