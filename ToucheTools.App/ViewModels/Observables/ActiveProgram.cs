using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveProgram : ActiveObservable<int>
{
    public ActiveProgram(DatabaseModel model)
    {
        SetElements(model.Programs.Keys.ToList());
        SetActive(Elements.Contains(90) ? 90 : Elements.First());
    }
}