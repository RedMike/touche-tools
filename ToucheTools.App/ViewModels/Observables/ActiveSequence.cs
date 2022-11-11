using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveSequence : ActiveObservable<int>
{
    public ActiveSequence(DatabaseModel model)
    {
        SetElements(model.Sequences.Keys.ToList());
        SetActive(Elements.First());
    }
}