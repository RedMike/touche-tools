using ImGuiNET;
using ToucheTools.App.Services;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class MainMenuWindow : BaseWindow
{
    private const string DatFilePath = "../../../../sample/DATABASE/TOUCHE.DAT";

    private static readonly List<(string, string)> SampleNamesAndPaths = new List<(string, string)>()
    {
        ("Basic Mechanics", "./samples/basic-mechanics/")
    };
    
    private readonly OpenedPackage _openedPackage;
    private readonly MainWindowState _state;
    private readonly PackagePublishService _publishService;
    private readonly RunService _runService;
    private readonly OpenedPath _openedPath;

    public MainMenuWindow(OpenedPackage openedPackage, MainWindowState state, PackagePublishService publishService, RunService runService, OpenedPath openedPath)
    {
        _openedPackage = openedPackage;
        _state = state;
        _publishService = publishService;
        _runService = runService;
        _openedPath = openedPath;
    }

    private void RenderFileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (!_openedPackage.IsLoaded())
            {
                ImGui.MenuItem("New"); //TODO: new package

                if (ImGui.BeginMenu("Load Sample"))
                {
                    foreach (var (name, path) in SampleNamesAndPaths)
                    {
                        if (ImGui.MenuItem(name))
                        {
                            _openedPath.LoadFolder(path);
                        }
                    }

                    ImGui.EndMenu();
                }
            }

            if (_openedPackage.IsLoaded())
            {
                if (ImGui.MenuItem("Close"))
                {
                    //TODO: check if saving is needed
                    _openedPath.Clear();
                }

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
                    _runService.Run(Path.GetDirectoryName(Path.GetFullPath(DatFilePath)) ??
                                    throw new InvalidOperationException());
                }
            }
            
            ImGui.Separator();

            if (ImGui.MenuItem("Editor Settings"))
            {
                _state.ShowingEditorSettings = true;
            }

            ImGui.EndMenu();
        }
    }

    private void RenderModeMenu()
    {
        if (!_openedPackage.IsLoaded())
        {
            return;
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
    }

    /// <summary>
    /// Renders to bottom-right corner
    /// </summary>
    private void RenderInfoMenuLast()
    {
        if (string.IsNullOrEmpty(_openedPath.LoadedPath))
        {
            return;
        }
        
        var text = $"Loaded: '{_openedPath.LoadedPath}'";
        var oldCursorPos = ImGui.GetCursorPos();
        try
        {
            ImGui.SetCursorPos(ImGui.GetWindowContentRegionMax() - ImGui.CalcTextSize(text));
            if (ImGui.BeginMenu(text, false))
            {
                //TODO: info about loaded package and deltas
                ImGui.EndMenu();
            }
        }
        finally
        {
            ImGui.SetCursorPos(oldCursorPos);
        }
    }
    
    public override void Render()
    {
        ImGui.BeginMainMenuBar();
        
        RenderFileMenu();
        RenderModeMenu();

        RenderInfoMenuLast();
        
        ImGui.EndMainMenuBar();
    }
}