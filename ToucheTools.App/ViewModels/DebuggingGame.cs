using ToucheTools.App.ViewModels.Observables;
using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

public class DebuggingGame : Observable<DatabaseModel>
{
    public DatabaseModel Model => Value;

    private bool _loaded = false;

    public bool IsLoaded()
    {
        return _loaded;
    }

    public void Load(DatabaseModel model)
    {
        _loaded = true;
        SetValue(model);
    }

    public void Clear()
    {
        _loaded = false;
        SetValue(new DatabaseModel());
    }
}