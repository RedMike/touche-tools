namespace ToucheTools.App.State;

public class MainWindowState
{
    public enum States
    {
        Idle = 0,
        ImageManagement = 1,
        //animations, rooms, programs
    }

    public States State { get; set; } = States.ImageManagement; //TODO: better default
}