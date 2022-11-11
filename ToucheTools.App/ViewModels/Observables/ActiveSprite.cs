using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveSprite : ActiveObservable<int>
{
    public ActiveSprite(DatabaseModel model)
    {
        SetElements(model.Sprites.Keys.ToList());
        SetActive(Elements.First());
    }
}