namespace ToucheTools.App.State;

public class MainWindowState
{
    public enum States
    {
        Idle = 0,
        ImageManagement = 1,
        AnimationManagement = 2,
        RoomManagement = 3,
        ProgramManagement = 4,
    }

    public States State { get; set; } = States.ProgramManagement; //TODO: better default
}