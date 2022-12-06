namespace ToucheTools.App.State;

public class PreviewInfoState
{
    public string? SelectedRoomImage { get; set; } = null;
    public bool RoomImagePreviewOpen { get; set; } = false;
    
    public string? SelectedSpriteImage { get; set; } = null;
    public bool SpriteImagePreviewOpen { get; set; } = false;
}