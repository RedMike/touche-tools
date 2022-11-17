namespace ToucheTools.App.ViewModels;

public class WindowSettings
{
    public bool RoomViewOpen { get; private set; }
    public bool SpriteViewOpen { get; private set; }
    public bool ProgramViewOpen { get; private set; } = true;

    public void CloseAllViews()
    {
        RoomViewOpen = false;
        SpriteViewOpen = false;
        ProgramViewOpen = false;
    }
    
    public void OpenRoomView()
    {
        CloseAllViews();
        RoomViewOpen = true;
    }
    
    public void OpenSpriteView()
    {
        CloseAllViews();
        SpriteViewOpen = true;
    }
    
    public void OpenProgramView()
    {
        CloseAllViews();
        ProgramViewOpen = true;
    }
}