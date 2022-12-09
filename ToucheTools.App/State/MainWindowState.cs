namespace ToucheTools.App.State;

public class MainWindowState
{
    public enum States
    {
        Idle = 0,
        ImageManagement = 1,
        AnimationManagement = 2,
        RoomManagement = 3,
        //rooms, programs
    }

    public States State { get; set; } = States.RoomManagement; //TODO: better default
}