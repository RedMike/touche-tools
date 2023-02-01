using System.Numerics;
using ImGuiNET;
using ToucheTools.App.Services;
using ToucheTools.App.State;

namespace ToucheTools.App.Windows;

public class EditorSettingsWindow : BaseWindow
{
    private readonly MainWindowState _mainWindowState;
    private readonly ConfigService _configService;

    public EditorSettingsWindow(MainWindowState mainWindowState, ConfigService configService)
    {
        _mainWindowState = mainWindowState;
        _configService = configService;
    }

    public override void Render()
    {
        if (!_mainWindowState.ShowingEditorSettings)
        {
            return;
        }

        var config = _configService.LoadConfig();
        var changed = false;
        
        ImGui.Begin("Editor Settings", ImGuiWindowFlags.NoCollapse);

        var origExePath = config.ExecutablePath;
        var exePath = origExePath;
        ImGui.PushID("EditorSettingsExePath");
        ImGui.SetNextItemWidth((ImGui.GetWindowContentRegionMax()-ImGui.GetWindowContentRegionMin()).X);
        ImGui.InputText("", ref exePath, UInt32.MaxValue);
        ImGui.PopID();
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.6f, 0.8f, 1.0f));
        ImGui.Text("Path to executable to run when running the game (OS-dependent, absolute path).");
        ImGui.Text("Example for ScummVM (depends on where you installed it): C:\\Program Files\\ScummVM\\scummvm.exe");
        ImGui.PopStyleColor();
        if (origExePath != exePath)
        {
            config.ExecutablePath = exePath;
            changed = true;
        }
        ImGui.Text("\n");

        var origExeArgs = config.ExecutableArgumentFormatString;
        var exeArgs = origExeArgs;
        ImGui.PushID("EditorSettingsExeArguments");
        ImGui.SetNextItemWidth((ImGui.GetWindowContentRegionMax()-ImGui.GetWindowContentRegionMin()).X);
        ImGui.InputText("", ref exeArgs, UInt32.MaxValue);
        ImGui.PopID();
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.7f, 0.6f, 0.8f, 1.0f));
        ImGui.Text("Format string where {0} is replaced with the DAT file path in double quotes.");
        ImGui.Text("Example for ScummVM: -p {0} touche:touche");
        ImGui.PopStyleColor();
        if (origExeArgs != exeArgs)
        {
            config.ExecutableArgumentFormatString = exeArgs;
            changed = true;
        }
        ImGui.Text("\n");
        ImGui.Text("\n");
        
        ImGui.Separator();

        if (changed)
        {
            _configService.SaveConfig(config);
        }
        if (ImGui.Button("Close"))
        {
            _mainWindowState.ShowingEditorSettings = false;
        }
        
        ImGui.End();
    }
}