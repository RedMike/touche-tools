using ImGuiNET;
using ToucheTools.App.Services;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class LoadFolderWindow : IWindow
{
    private readonly DebuggingGame _game;
    private readonly MainWindowState _state;
    private readonly DebugService _debugService;

    public LoadFolderWindow(DebuggingGame game, MainWindowState state, DebugService debugService)
    {
        _game = game;
        _state = state;
        _debugService = debugService;
    }

    public void Render()
    {
        if (_game.IsLoaded())
        {
            _state.FolderToLoad = null;
            _state.DatFileToLoad = null;
        }

        if (_state.FolderToLoad == null)
        {
            return;
        }

        ImGui.Begin("Load Folder");

        var origPath = _state.FolderToLoad ?? "";
        var path = origPath;
        ImGui.InputText("Path", ref path, 256);
        ImGui.Text("Example: C:\\Files\\MyPackage\\");
        if (path != origPath)
        {
            _state.FolderToLoad = path;
        }

        if (ImGui.Button("Cancel"))
        {
            _state.FolderToLoad = null;
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Load"))
        {
            _debugService.RunFolder(_state.FolderToLoad ?? "");
            _state.FolderToLoad = null;
        }
        
        ImGui.End();
    }
}