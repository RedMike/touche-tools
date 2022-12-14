using System.Numerics;
using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class ProgramManagementWindow : BaseWindow
{
    private readonly OpenedPackage _package;
    private readonly MainWindowState _state;
    private readonly ProgramManagementState _programManagementState;

    public ProgramManagementWindow(OpenedPackage package, MainWindowState state, ProgramManagementState programManagementState)
    {
        _package = package;
        _state = state;
        _programManagementState = programManagementState;
    }

    public override void Render()
    {
        if (!_package.IsLoaded())
        {
            return;
        }
        if (_state.State != MainWindowState.States.ProgramManagement)
        {
            return;
        }
        var pos = Vector2.Zero + new Vector2(0.0f, ImGui.GetFrameHeight());
        ImGui.SetNextWindowPos(pos, ImGuiCond.Once);
        ImGui.Begin("Programs", ImGuiWindowFlags.NoCollapse);
        var game = _package.GetGame();
        var allPrograms = _package.GetAllPrograms().ToList();
        var includedPrograms = _package.GetIncludedPrograms();
        foreach (var path in allPrograms)
        {
            //included checkbox
            var origIsIncluded = includedPrograms.ContainsKey(path);
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

            //button to select for preview
            var isSelected = _programManagementState.SelectedProgram == path;
            if (isSelected)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.2f, 0.4f, 0.7f, 1.0f));
            }
            if (ImGui.Button(path))
            {
                _programManagementState.SelectedProgram = path;
                _programManagementState.PreviewOpen = true;
                _programManagementState.EditorOpen = true;
            }
            if (isSelected)
            {
                ImGui.PopStyleColor();
            }
            
            if (isIncluded)
            {
                ImGui.SameLine();
                //program type
                var program = includedPrograms[path];
                var types = OpenedPackage.ProgramTypeAsList();
                var origSelectedType = types.FindIndex(i => i == program.Type.ToString("G"));
                var selectedType = origSelectedType;
                ImGui.PushID($"{path}_type");
                ImGui.SetNextItemWidth(100.0f);
                ImGui.Combo("", ref selectedType, types.ToArray(), types.Count);
                ImGui.PopID();
                if (selectedType != origSelectedType)
                {
                    _package.Value.Programs[path].Type = Enum.Parse<OpenedPackage.ProgramType>(types[selectedType]);
                    _package.ForceUpdate();
                }
                ImGui.SameLine();
                
                //program index
                var indexes = Enumerable.Range(1, 200).ToList();
                var origIndex = program.Index - 1;
                var index = origIndex;
                ImGui.PushID($"{path}_index");
                ImGui.SetNextItemWidth(60.0f);
                ImGui.Combo("", ref index, indexes.Select(i => i.ToString()).ToArray(), indexes.Count);
                ImGui.PopID();
                if (index != origIndex)
                {
                    _package.Value.Programs[path].Index = index + 1;
                    _package.ForceUpdate();
                }
                
                //target
                if (program.Type == OpenedPackage.ProgramType.Action)
                {
                    var actions = game.ActionDefinitions.Select(a => (a.Key, a.Value)).ToList();
                    var actionList = actions.Select(a => a.Value).ToArray();
                    var origAction = -1;
                    if (program.Target != -1)
                    {
                        origAction = actions.FindIndex(a => a.Key == program.Target);
                    }
                    var action = origAction;
                    ImGui.SameLine();
                    ImGui.PushID($"{path}_action");
                    ImGui.SetNextItemWidth(60.0f);
                    ImGui.Combo("", ref action, actionList, actionList.Length);
                    ImGui.PopID();
                    if (action != origAction)
                    {
                        _package.Value.Programs[path].Target = actions[action].Key;
                        _package.ForceUpdate();
                    }

                    var origData = program.Data;
                    var data = origData;
                    
                    ImGui.SameLine();
                    //TODO: load list of targets from room/program
                    ImGui.PushID($"{path}_data");
                    ImGui.SetNextItemWidth(100.0f);
                    ImGui.InputInt("", ref data, 1);
                    ImGui.PopID();
                    if (data != origData)
                    {
                        _package.Value.Programs[path].Data = data;
                        _package.ForceUpdate();
                    }
                } else if (_package.Value.Programs[path].Type == OpenedPackage.ProgramType.KeyChar)
                {
                    ImGui.SameLine();
                    //TODO: load list of characters from room/program
                    var origTarget = program.Target;
                    var target = origTarget;
                    ImGui.PushID($"{path}_target");
                    ImGui.SetNextItemWidth(100.0f);
                    ImGui.InputInt("", ref target, 1);
                    ImGui.PopID();
                    if (target != origTarget)
                    {
                        _package.Value.Programs[path].Target = target;
                        _package.ForceUpdate();
                    }
                }
            }
        }

        ImGui.Separator();
        if (ImGui.Button("Refresh Programs"))
        {
            //TODO: add any new programs to list
        }
        ImGui.End();
    }
}