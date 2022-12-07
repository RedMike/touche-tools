using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class AnimationManagementWindow : BaseWindow
{
    private readonly OpenedPackage _package;
    private readonly MainWindowState _state;

    public AnimationManagementWindow(OpenedPackage package, MainWindowState state)
    {
        _package = package;
        _state = state;
    }

    public override void Render()
    {
        if (!_package.IsLoaded())
        {
            return;
        }
        if (_state.State != MainWindowState.States.AnimationManagement)
        {
            return;
        }
        
        var pos = Vector2.Zero + new Vector2(0.0f, ImGui.GetFrameHeight());
        ImGui.SetNextWindowPos(pos, ImGuiCond.Once);
        ImGui.Begin("Animations", ImGuiWindowFlags.NoCollapse);

        var allAnimations = _package.GetAllAnimations();
        var includedAnimations = _package.GetIncludedAnimations();
        foreach (var path in allAnimations)
        {
            //included checkbox
            var origIsIncluded = includedAnimations.ContainsKey(path);
            var isIncluded = origIsIncluded;
            ImGui.PushID($"{path}_include");
            ImGui.Checkbox("", ref isIncluded);
            ImGui.PopID();
            if (isIncluded != origIsIncluded)
            {
                if (isIncluded)
                {
                    _package.IncludeFile(path);
                }
                else
                {
                    _package.ExcludeFile(path);
                }
            }
            ImGui.SameLine();

            if (ImGui.Button(path))
            {
                //TODO: open animation for editing                
            }
            
            if (isIncluded)
            {
                ImGui.SameLine();
                //image index
                var indexes = Enumerable.Range(1, 99).ToList();
                var origIndex = includedAnimations[path].Index - 1;
                var index = origIndex;
                ImGui.PushID($"{path}_index");
                ImGui.SetNextItemWidth(60.0f);
                ImGui.Combo("", ref index, indexes.Select(i => i.ToString()).ToArray(), indexes.Count);
                ImGui.PopID();
                if (index != origIndex)
                {
                    _package.Value.Animations[path].Index = index + 1;
                }
            }
        }

        if (ImGui.Button("Refresh Animations"))
        {
            //TODO: load from files
        }
        ImGui.End();
    }
}