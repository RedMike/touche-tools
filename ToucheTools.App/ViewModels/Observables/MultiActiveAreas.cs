using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class MultiActiveAreas : MultiActiveObservable<int>
{
    private readonly DatabaseModel _model;
    private readonly ActiveProgram _program;
    
    public List<((int, int, int, int), (int, int), int, int, int, int)> AreaView = null!;
    
    public MultiActiveAreas(DatabaseModel model, ActiveProgram program)
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
        SetElements(program.Areas.Select((_, idx) => idx).ToList(), false);
    }

    private void Update()
    {
        var program = _model.Programs[_program.Active];
        var areaView = new List<((int, int, int, int), (int, int), int, int, int, int)>();
        for (var idx = 0; idx < program.Areas.Count; idx++)
        {
            if (Elements[idx])
            {
                areaView.Add(
                    (
                        (program.Areas[idx].Rect.X, program.Areas[idx].Rect.Y,
                            program.Areas[idx].Rect.W, program.Areas[idx].Rect.H),
                        (program.Areas[idx].SrcX, program.Areas[idx].SrcY),
                        program.Areas[idx].Id, (int)program.Areas[idx].InitialState,
                        program.Areas[idx].AnimationCount, program.Areas[idx].AnimationNext
                    )
                );
            }
        }

        AreaView = areaView;
    }

    protected override string ConvertElementToString(int element)
    {
        var program = _model.Programs[_program.Active];
        var area = program.Areas[element];

        return $"{element} id {area.Id} initial {area.InitialState:G} ({area.Rect.X},{area.Rect.Y} x {area.Rect.W},{area.Rect.H} src {area.SrcX}, {area.SrcY})";
    }
}