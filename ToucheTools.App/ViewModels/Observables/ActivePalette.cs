using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActivePalette : ActiveObservable<int>
{
    public ActivePalette(DatabaseModel model)
    {
        SetElements(model.Palettes.Keys.ToList());
        SetActive(Elements.First());
    }
}