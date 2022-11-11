using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveRoom : ActiveObservable<int>
{
    public ActiveRoom(DatabaseModel model)
    {
        SetElements(model.Rooms.Keys.ToList());
        SetActive(Elements.First());
    }
}