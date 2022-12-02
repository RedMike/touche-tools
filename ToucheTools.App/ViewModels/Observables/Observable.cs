namespace ToucheTools.App.ViewModels.Observables;

public abstract class Observable<T>
{
    public T Value { get; private set; } = default!;

    protected void SetValue(T val)
    {
        Value = val;
        _activeUpdated();
    }
    
    #region Observable
    private Action _activeUpdated = () => { };
    public void Observe(Action cb)
    {
        _activeUpdated += cb;
    }
    #endregion
}