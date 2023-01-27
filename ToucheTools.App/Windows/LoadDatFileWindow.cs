using ImGuiNET;
using ToucheTools.App.Services;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class LoadDatFileWindow : IWindow
{
    private readonly DebuggingGame _game;
    private readonly MainWindowState _state;
    private readonly DebugService _debugService;

    public LoadDatFileWindow(DebuggingGame game, MainWindowState state, DebugService debugService)
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

        if (_state.DatFileToLoad == null)
        {
            return;
        }

        ImGui.Begin("Load DAT File");

        var origPath = _state.DatFileToLoad ?? "";
        var path = origPath;
        ImGui.InputText("Path", ref path, 256);
        ImGui.Text("Example: C:\\Files\\MyPackage\\TOUCHE.DAT");
        if (path != origPath)
        {
            _state.DatFileToLoad = path;
        }

        if (ImGui.Button("Cancel"))
        {
            _state.DatFileToLoad = null;
        }
        
        ImGui.SameLine();
        if (ImGui.Button("Load"))
        {
            _debugService.Run(_state.DatFileToLoad ?? "");
            _state.DatFileToLoad = null;
        }
        
        ImGui.End();
    }
}