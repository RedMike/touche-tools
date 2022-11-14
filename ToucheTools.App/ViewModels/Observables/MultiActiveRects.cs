using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class MultiActiveRects : MultiActiveObservable<int>
{
    private readonly DatabaseModel _model;
    private readonly ActiveProgram _program;
    
    public List<(int, int, int, int)> RectsView = null!;
    
    public MultiActiveRects(DatabaseModel model, ActiveProgram program)
    {
        _model = model;
        _program = program;
        _program.ObserveActive(UpdateProgram);
        UpdateProgram();
        ObserveChanged(Update);
        Update();
    }

    private void UpdateProgram()
    {
        var program = _model.Programs[_program.Active];
        SetElements(program.Rects.Select((_, idx) => idx).ToList(), false);
    }

    private void Update()
    {
        var program = _model.Programs[_program.Active];
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
        var program = _model.Programs[_program.Active];
        var rect = program.Rects[element];
        return $"{element} ({rect.X},{rect.Y} x {rect.W},{rect.H})";
    }
}