namespace ToucheTools.App.ViewModels;

public abstract class ActiveObservable<T>
{
    public List<T> Elements { get; protected set; } = new List<T>();
    public T Active { get; protected set; } = default!;
    private Action _activeUpdated = () => { };

    public void ObserveActive(Action cb)
    {
        _activeUpdated += cb;
    }

    public void SetActive(T active)
    {
        if (!Elements.Contains(active))
        {
            throw new Exception($"Unknown {nameof(T)}: " + active);
        }

        Active = active;
        _activeUpdated();
    }
}