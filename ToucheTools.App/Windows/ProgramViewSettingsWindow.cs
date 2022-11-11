using System.Numerics;
using ImGuiNET;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class ProgramViewSettingsWindow : IWindow
{
    private readonly WindowSettings _windowSettings;
    private readonly ActiveData _activeData;
    private readonly ProgramViewSettings _viewSettings;

    public ProgramViewSettingsWindow(WindowSettings windowSettings, ActiveData activeData, ProgramViewSettings viewSettings)
    {
        _windowSettings = windowSettings;
        _activeData = activeData;
        _viewSettings = viewSettings;
    }

    public void Render()
    {
        if (!_windowSettings.ProgramViewOpen)
        {
            return;
        }
        
        var originalProgramId = _viewSettings.Programs.FindIndex(k => k == _viewSettings.ActiveProgram);
        var curProgramId = originalProgramId;
        var programs = _viewSettings.Programs.ToArray();
    
        ImGui.SetNextWindowPos(new Vector2(150.0f, 0.0f));
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f));
        ImGui.Begin("View Settings", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

        ImGui.Combo("Program", ref curProgramId, programs.Select(k => k.ToString()).ToArray(), programs.Length);
        if (curProgramId != originalProgramId)
        {
            _viewSettings.SetActiveProgram(programs[curProgramId]);
        }
    
        ImGui.End();
    }
}