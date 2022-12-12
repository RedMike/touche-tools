using ImGuiNET;
using ToucheTools.App.State;
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

        if (!_package.Value.Programs.ContainsKey(_programManagementState.SelectedProgram))
        {
            //error?
            return;
        }

        var programId = _package.GetIncludedPrograms().First(p => p.Key == _programManagementState.SelectedProgram).Value.Index;
        var program = _programs.GetProgram(programId);
        
        ImGui.Begin("Program Preview", ImGuiWindowFlags.NoCollapse);

        foreach (var (offset, instruction) in program)
        {
            ImGui.Button($"{offset:D5}");
            ImGui.SameLine();
            ImGui.Text(instruction.ToString());
        }
        
        if (ImGui.Button("Close preview"))
        {
            _programManagementState.PreviewOpen = false;
        }
        
        ImGui.End();
    }
}