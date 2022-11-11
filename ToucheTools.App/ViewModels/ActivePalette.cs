using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

public class ActivePalette : ActiveObservable<int>
{
    public ActivePalette(DatabaseModel model)
    {
        Elements = model.Palettes.Keys.ToList();
        Active = Elements.First();
    }
}