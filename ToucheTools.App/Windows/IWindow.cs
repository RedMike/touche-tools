using ImGuiNET;
using ToucheTools.App.ViewModels.Observables;

namespace ToucheTools.App.Windows;

public interface IWindow
{
    void Render();
}

public abstract class BaseWindow : IWindow
{
    public abstract void Render();

    protected void ObservableCombo<T>(string label, ActiveObservable<T> observable)
    {
        Combo(label, observable.ActiveElementAsIndex, observable.ElementsAsArray, (newIndex) =>
        {
            observable.SetActive(observable.Elements[newIndex]);
        });
    }
    
    protected void Combo(string label, int originalActive, string[] displayElements, Action<int> changedCb)
    {
        var curActive = originalActive;
        ImGui.Combo(label, ref curActive, displayElements, displayElements.Length);
        if (curActive != originalActive)
        {
            changedCb(curActive);
        }
    }
}