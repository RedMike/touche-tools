using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class MultiActivePoints : MultiActiveObservable<int>
{
    private readonly DebuggingGame _game;
    private readonly ActiveProgram _program;
    
    public List<(int, int, int, int)> PointsView = null!;
    
    public MultiActivePoints(ActiveProgram program, DebuggingGame game)
    {
        _program = program;
        _program.ObserveActive(UpdateProgram);
        _game = game;
        game.Observe(UpdateProgram);
        UpdateProgram();
        ObserveChanged(Update);
        Update();
    }

    private void UpdateProgram()
    {
        if (!_game.IsLoaded())
        {
            return;
        }

        var model = _game.Model;
        var program = model.Programs[_program.Active];
        SetElements(program.Points.Select((_, idx) => idx).ToList(), false);
    }

    private void Update()
    {
        if (!_game.IsLoaded())
        {
            return;
        }

        var model = _game.Model;
        var program = model.Programs[_program.Active];
        var pointsView = new List<(int, int, int, int)>();
        for (var idx = 0; idx < program.Points.Count; idx++)
        {
            if (Elements[idx])
            {
                pointsView.Add((program.Points[idx].X, program.Points[idx].Y, program.Points[idx].Z, program.Points[idx].Order));
            }
        }

        PointsView = pointsView;
    }

    protected override string ConvertElementToString(int element)
    {
        if (!_game.IsLoaded())
        {
            return $"unknown {element}";
        }

        var model = _game.Model;
        var program = model.Programs[_program.Active];
        var point = program.Points[element];
        return $"{element} ({point.X},{point.Y},{point.Z} order {point.Order})";
    }
}