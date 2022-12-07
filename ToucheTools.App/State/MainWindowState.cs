namespace ToucheTools.App.State;

public class MainWindowState
{
    public enum States
    {
        Idle = 0,
        ImageManagement = 1,
        AnimationManagement = 2,
        //rooms, programs
    }

    public States State { get; set; } = States.AnimationManagement; //TODO: better default
}