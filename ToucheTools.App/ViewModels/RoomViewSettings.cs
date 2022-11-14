using ToucheTools.App.ViewModels.Observables;
using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

public class RoomViewSettings
{
    private readonly DatabaseModel _model;
    private readonly ActiveRoom _room;
    private readonly ActiveProgram _program;

    public bool ShowRects { get; set; } = true;

    public RoomViewSettings(DatabaseModel model, ActiveRoom room, ActiveProgram program)
    {
        _model = model;
        _room = room;
        _program = program;
    }
}