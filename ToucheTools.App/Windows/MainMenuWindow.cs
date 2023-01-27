using ImGuiNET;
using ToucheTools.App.Services;
using ToucheTools.App.State;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class MainMenuWindow : BaseWindow
{
    private static readonly List<(string, string)> SampleNamesAndPaths = new List<(string, string)>()
    {
        ("Basic Mechanics", "./samples/basic-mechanics/")
    };
    
    private readonly OpenedPackage _openedPackage;
    private readonly MainWindowState _state;
    private readonly PackagePublishService _publishService;
    private readonly RunService _runService;
    private readonly OpenedPath _openedPath;
    private readonly OpenedManifest _openedManifest;
    private readonly DebugService _debugService;
    private readonly DebuggingGame _game;

    public MainMenuWindow(OpenedPackage openedPackage, MainWindowState state, PackagePublishService publishService, RunService runService, OpenedPath openedPath, OpenedManifest openedManifest, DebugService debugService, DebuggingGame game)
    {
        _openedPackage = openedPackage;
        _state = state;
        _publishService = publishService;
        _runService = runService;
        _openedPath = openedPath;
        _openedManifest = openedManifest;
        _debugService = debugService;
        _game = game;
    }

    private void RenderFileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (!_openedPackage.IsLoaded())
            {
                //ImGui.MenuItem("New"); //TODO: new package

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

                if (ImGui.MenuItem("Load Published Folder"))
                {
                    _state.FolderToLoad = "";
                    _state.DatFileToLoad = null;
                }

                if (ImGui.MenuItem("Load Published DAT file"))
                {
                    _state.FolderToLoad = null;
                    _state.DatFileToLoad = "";
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
                    _openedManifest.Save();
                }

                var triggerPublish = false;
                var triggerRun = false;
                var triggerDebug = false;
                if (ImGui.MenuItem("Publish"))
                {
                    //TODO: check if saving is needed
                    //TODO: trigger saving
                    triggerPublish = true;
                }

                if (ImGui.MenuItem("Publish & Run"))
                {
                    //TODO: check if saving is needed
                    //TODO: trigger saving
                    //TODO: check if publish is needed
                    triggerPublish = true;
                    triggerRun = true;
                }
                
                if (ImGui.MenuItem("Publish & Debug"))
                {
                    //TODO: check if saving is needed
                    //TODO: trigger saving
                    //TODO: check if publish is needed
                    triggerPublish = true;
                    triggerDebug = true;
                }

                var publishPath = "";
                if (triggerPublish)
                {
                    publishPath = _publishService.Publish(Path.Combine(_openedPath.LoadedPath, "dist"));
                }

                if (triggerRun)
                {
                    if (string.IsNullOrEmpty(publishPath))
                    {
                        throw new Exception("Missing publish path");
                    }
                    _runService.Run(publishPath);
                }

                if (triggerDebug)
                {
                    if (string.IsNullOrEmpty(publishPath))
                    {
                        throw new Exception("Missing publish path");
                    }
                    _state.State = MainWindowState.States.Idle;
                    _debugService.RunFolder(publishPath);
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
        
        var text = $"Loaded: '{_openedPackage.GetPackageName()}' ({_openedPath.LoadedPath})";
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

        if (_game.IsLoaded())
        {
            //currently debugging
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("Stop Debugging"))
                {
                    _game.Clear();
                    _state.State = MainWindowState.States.Idle;
                }
                ImGui.EndMenu();
            }
        }
        else
        {
            RenderFileMenu();
            RenderModeMenu();

            RenderInfoMenuLast();
        }
        
        ImGui.EndMainMenuBar();
    }
}