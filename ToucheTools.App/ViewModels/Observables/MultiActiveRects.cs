namespace ToucheTools.App.ViewModels.Observables;

public class MultiActiveRects : MultiActiveObservable<int>
{
    private readonly DebuggingGame _game;
    private readonly ActiveProgram _program;
    
    public List<(int, int, int, int)> RectsView = null!;
    
    public MultiActiveRects(ActiveProgram program, DebuggingGame game)
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
        SetElements(program.Rects.Select((_, idx) => idx).ToList(), false);
    }

    private void Update()
    {
        if (!_game.IsLoaded())
        {
            return;
        }

        var model = _game.Model;
        var program = model.Programs[_program.Active];
        var rectsView = new List<(int, int, int, int)>();
        for (var idx = 0; idx < program.Rects.Count; idx++)
        {
            if (Elements[idx])
            {
                rectsView.Add((program.Rects[idx].X, program.Rects[idx].Y, program.Rects[idx].W, program.Rects[idx].H));
            }
        }

        RectsView = rectsView;
    }

    protected override string ConvertElementToString(int element)
    {
        if (!_game.IsLoaded())
        {
            return $"unknown {element}";
        }

        var model = _game.Model;
        var program = model.Programs[_program.Active];
        var rect = program.Rects[element];
        return $"{element} ({rect.X},{rect.Y} x {rect.W},{rect.H})";
    }
}