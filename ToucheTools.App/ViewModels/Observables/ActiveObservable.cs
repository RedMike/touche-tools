namespace ToucheTools.App.ViewModels.Observables;

public abstract class ActiveObservable<T>
{
    public List<T> Elements { get; private set; } = new List<T>();
    public T Active { get; private set; } = default!;
    
    #region Display methods
    public string[] ElementsAsArray { get; private set; } = Array.Empty<string>();
    public int ActiveElementAsIndex { get; private set; } = default!;
    #endregion

    protected void SetElements(List<T> elements)
    {
        Elements = elements;
        ElementsAsArray = elements.Select(ConvertElementToString).ToArray();
    }
    
    public void SetActive(T active)
    {
        if (!Elements.Contains(active))
        {
            throw new Exception($"Unknown {nameof(T)}: " + active);
        }

        Active = active;
        ActiveElementAsIndex = Elements.FindIndex(e => 
                EqualityComparer<T>.Default.Equals(e, Active)
            );
        _activeUpdated();
    }

    protected virtual string ConvertElementToString(T element)
    {
        return element?.ToString() ?? throw new InvalidOperationException();
    }
    
    #region Observable
    private Action _activeUpdated = () => { };
    public void ObserveActive(Action cb)
    {
        _activeUpdated += cb;
    }
    #endregion
}