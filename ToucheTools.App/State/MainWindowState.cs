namespace ToucheTools.App.State;

public class MainWindowState
{
    public enum States
    {
        Idle = 0,
        PackageManagement = 6,
        GameManagement = 1,
        ImageManagement = 2,
        AnimationManagement = 3,
        RoomManagement = 4,
        ProgramManagement = 5,
    }

    public States State { get; set; } = States.Idle;

    public bool ShowingEditorSettings { get; set; } = false;
}