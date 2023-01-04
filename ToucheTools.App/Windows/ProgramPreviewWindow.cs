using ImGuiNET;
using ToucheTools.App.State;
using ToucheTools.App.Utils;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class ProgramPreviewWindow : BaseWindow
{
    private readonly OpenedPackage _package;
    private readonly MainWindowState _state;
    private readonly ProgramManagementState _programManagementState;
    private readonly PackagePrograms _programs;

    public ProgramPreviewWindow(OpenedPackage package, MainWindowState state, ProgramManagementState programManagementState, PackagePrograms programs)
    {
        _package = package;
        _state = state;
        _programManagementState = programManagementState;
        _programs = programs;
    }

    public override void Render()
    {
        if (_state.State != MainWindowState.States.ProgramManagement)
        {
            return;
        }

        if (!_programManagementState.PreviewOpen)
        {
            return;
        }

        if (_programManagementState.SelectedProgram == null)
        {
            return;
        }

        if (!_package.LoadedManifest.Programs.ContainsKey(_programManagementState.SelectedProgram))
        {
            //error?
            return;
        }

        var programId = _package.GetIncludedPrograms().First(p => p.Key == _programManagementState.SelectedProgram).Value.Index;
        var program = _programs.GetProgram(programId);
        var actionOffsets = _programs.GetActionOffsetsForProgram(programId);
        var charOffsets = _programs.GetCharOffsetsForProgram(programId);
        var convoOffsets = _programs.GetConvoOffsetsForProgram(programId);
        var labels = _programs.GetLabelOffsetsForProgram(programId);
        
        ImGui.Begin("Program Preview", ImGuiWindowFlags.NoCollapse);

        foreach (var (offset, instruction) in program)
        {
            ImGui.Button($"{offset:D5}");
            ImGui.SameLine();
            ImGui.Text(instruction.ToString());
            if (labels.Any(p => p.Value == offset))
            {
                var label = labels.First(p => p.Value == offset).Key;
                ImGui.SameLine();
                ImGui.Text($" - Label \":{label}\"");
            }
            if (actionOffsets.Any(p => p.Value == offset))
            {
                var actionPath = actionOffsets.First(p => p.Value == offset).Key;
                ImGui.SameLine();
                ImGui.Text($" - Action {actionPath.ShortenPath()}");
            }
            if (charOffsets.Any(p => p.Value == offset))
            {
                var charPath = charOffsets.First(p => p.Value == offset).Key;
                ImGui.SameLine();
                ImGui.Text($" - KeyChar {charPath.ShortenPath()}");
            }
            if (convoOffsets.Any(p => p.Value == offset))
            {
                var convoPath = convoOffsets.First(p => p.Value == offset).Key;
                ImGui.SameLine();
                ImGui.Text($" - Conversation {convoPath.ShortenPath()}");
            }
        }
        
        if (ImGui.Button("Close preview"))
        {
            _programManagementState.PreviewOpen = false;
        }
        
        ImGui.End();
    }
}