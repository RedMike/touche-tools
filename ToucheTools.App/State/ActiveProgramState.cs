using ToucheTools.App.ViewModels;

namespace ToucheTools.App.State;

public class ActiveProgramState
{
    public ProgramViewSettings.ProgramState? CurrentState { get; set; } = null;
}