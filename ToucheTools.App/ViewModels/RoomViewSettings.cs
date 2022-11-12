using ToucheTools.App.ViewModels.Observables;
using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

public class RoomViewSettings
{
    private readonly DatabaseModel _model;
    private readonly ActiveRoom _room;
    private readonly ActiveProgram _program;

    public bool ShowRects { get; set; } = true;

    public List<(int, int, int, int)> RectsView = null!;

    public RoomViewSettings(DatabaseModel model, ActiveRoom room, ActiveProgram program)
    {
        _model = model;
        _room = room;
        _program = program;
        _room.ObserveActive(Update);
        _program.ObserveActive(Update);
        Update();
    }

    private void Update()
    {
        var program = _model.Programs[_program.Active];
        RectsView = program.Rects.Select(r => (r.X, r.Y, r.W, r.H)).ToList();
    }
}