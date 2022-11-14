using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class MultiActiveBackgrounds : MultiActiveObservable<int>
{
    private readonly DatabaseModel _model;
    private readonly ActiveProgram _program;
    
    public List<((int, int, int, int), (int, int), int, int, int, int)> BackgroundsView = null!;
    
    public MultiActiveBackgrounds(DatabaseModel model, ActiveProgram program)
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
        SetElements(program.Backgrounds.Select((_, idx) => idx).ToList(), false);
    }

    private void Update()
    {
        var program = _model.Programs[_program.Active];
        var backgroundsView = new List<((int, int, int, int), (int, int), int, int, int, int)>();
        for (var idx = 0; idx < program.Backgrounds.Count; idx++)
        {
            if (program.Backgrounds[idx].IsDrawable)
            {
                if (Elements[idx])
                {
                    backgroundsView.Add(
                        (
                            (program.Backgrounds[idx].Rect.X, program.Backgrounds[idx].Rect.Y,
                                program.Backgrounds[idx].Rect.W, program.Backgrounds[idx].Rect.H),
                            (program.Backgrounds[idx].SrcX, program.Backgrounds[idx].SrcY),
                            program.Backgrounds[idx].Type, program.Backgrounds[idx].Offset,
                            program.Backgrounds[idx].ScaleMul, program.Backgrounds[idx].ScaleDiv
                        )
                    );
                }
            }
        }

        BackgroundsView = backgroundsView;
    }

    protected override string ConvertElementToString(int element)
    {
        var program = _model.Programs[_program.Active];
        var back = program.Backgrounds[element];
        if (!back.IsDrawable)
        {
            return $"{element} not drawable";
        }

        if (back.IsScaled)
        {
            return $"{element} scaled ({back.Rect.X},{back.Rect.Y} x {back.Rect.W},{back.Rect.H} src {back.SrcX}, {back.SrcY})";
        }
        return $"{element} type {back.Type} ({back.Rect.X},{back.Rect.Y} x {back.Rect.W},{back.Rect.H} src {back.SrcX}, {back.SrcY})";
    }
}