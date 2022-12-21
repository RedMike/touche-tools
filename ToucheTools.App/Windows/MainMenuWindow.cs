using ImGuiNET;
using ToucheTools.App.Services;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class MainMenuWindow : BaseWindow
{
    private const string DatFilePath = "../../../../sample/DATABASE/TOUCHE.DAT";
    
    private readonly OpenedPackage _openedPackage;
    private readonly MainWindowState _state;
    private readonly PackagePublishService _publishService;
    private readonly RunService _runService;

    public MainMenuWindow(OpenedPackage openedPackage, MainWindowState state, PackagePublishService publishService, RunService runService)
    {
        _openedPackage = openedPackage;
        _state = state;
        _publishService = publishService;
        _runService = runService;
    }

    public override void Render()
    {
        ImGui.BeginMainMenuBar();
        if (!_openedPackage.IsLoaded())
        {
            if (ImGui.BeginMenu("File"))
            {
                ImGui.MenuItem("New"); //TODO: new package
                ImGui.MenuItem("Load"); //TODO: load menu
                
                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
            return;
        }
        
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem("Save"))
            {
                //TODO: trigger all saving
                _openedPackage.SaveManifest();
            }

            if (ImGui.MenuItem("Publish"))
            {
                _publishService.Publish(DatFilePath);
            }

            if (ImGui.MenuItem("Publish & Run"))
            {
                _publishService.Publish(DatFilePath);
                _runService.Run(Path.GetDirectoryName(Path.GetFullPath(DatFilePath)) ?? throw new InvalidOperationException());
            }
            
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Mode"))
        {
            foreach (var val in Enum.GetValues<MainWindowState.States>().Where(s => s != MainWindowState.States.Idle))
            {
                var selected = _state.State == val;
                if (ImGui.MenuItem($"{val:G}", "", selected, true))
                {
                    if (_state.State == val)
                    {
                        _state.State = MainWindowState.States.Idle;
                    }
                    else
                    {
                        _state.State = val;
                    }
                }
            }

            ImGui.EndMenu();
        }
        
        var text = $"Loaded: '{_openedPackage.GetLoadedPath()}'";
        ImGui.SetCursorPos(ImGui.GetWindowContentRegionMax() - ImGui.CalcTextSize(text));
        if (ImGui.BeginMenu(text, false))
        {
            //TODO: info about loaded package and deltas
            ImGui.EndMenu();
        }
        ImGui.EndMainMenuBar();
    }
}