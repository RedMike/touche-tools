namespace ToucheTools.App.ViewModels.Observables;

public class MultiActiveObservable<T> where T : notnull
{
    public Dictionary<T, bool> Elements { get; private set; } = new Dictionary<T, bool>();

    #region Display methods

    public Dictionary<string, T> ElementMapping { get; private set; } = default!;
    public Dictionary<string, bool> ElementsAsDict { get; private set; } = default!;
    
    #endregion
    
    protected void SetElements(List<T> elements, bool defaultState = false)
    {
        Elements = elements.ToDictionary(e => e, _ => defaultState);
        ElementsAsDict = Elements.ToDictionary(pair => ConvertElementToString(pair.Key), pair => pair.Value);
        ElementMapping = Elements.ToDictionary(pair => ConvertElementToString(pair.Key), pair => pair.Key);
        _changeUpdated();
    }

    public void SetElement(T element, bool state)
    {
        if (!Elements.ContainsKey(element))
        {
            throw new Exception($"Unknown {nameof(T)}: " + element);
        }

        if (Elements[element] == state)
        {
            return;
        }
        Elements[element] = state;
        ElementsAsDict[ConvertElementToString(element)] = state;
        _changeUpdated();
    }
    
    protected virtual string ConvertElementToString(T element)
    {
        return element?.ToString() ?? throw new InvalidOperationException();
    }
    
    #region Observable
    private Action _changeUpdated = () => { };
    public void ObserveChanged(Action cb)
    {
        _changeUpdated += cb;
    }
    #endregion
}