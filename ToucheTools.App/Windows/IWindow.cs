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

    protected void ObservableCheckboxList<T>(string label, MultiActiveObservable<T> observable)
    {
        CheckboxList(label, observable.ElementsAsDict, observable.ElementsAsArray, (name, state) =>
        {
            var key = observable.ElementMapping[name];
            observable.SetElement(key, state);
        });
    }

    protected void CheckboxList(string label, Dictionary<string, bool> activeElements, string[] displayElements, Action<string, bool> changedCb)
    {
        if (ImGui.TreeNodeEx(label, ImGuiTreeNodeFlags.DefaultOpen))
        {
            foreach (var (name, on) in activeElements)
            {
                var curValue = on;
                ImGui.Checkbox(name, ref curValue);
                //TODO: wrapping
                if (curValue != on)
                {
                    changedCb(name, curValue);
                }
            }
            ImGui.TreePop();
        }
    }
}