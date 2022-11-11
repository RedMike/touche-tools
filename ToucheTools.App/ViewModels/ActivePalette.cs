using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

public class ActivePalette : ActiveObservable<int>
{
    public ActivePalette(DatabaseModel model)
    {
        SetElements(model.Palettes.Keys.ToList());
        SetActive(Elements.First());
    }
}