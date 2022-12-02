using ImGuiNET;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class MainMenuWindow : BaseWindow
{
    private readonly OpenedPackage _openedPackage;

    public MainMenuWindow(OpenedPackage openedPackage)
    {
        _openedPackage = openedPackage;
    }

    public override void Render()
    {
        ImGui.BeginMainMenuBar();
        
        if (ImGui.BeginMenu("File"))
        {
            if (!_openedPackage.IsLoaded())
            {
                ImGui.MenuItem("New"); //TODO: new package
                ImGui.MenuItem("Load"); //TODO: load menu
            }
            else
            {
                if (ImGui.MenuItem("Save"))
                {
                    //TODO: trigger all saving
                    _openedPackage.SaveManifest();
                }

                if (ImGui.MenuItem("Publish"))
                {
                    //TODO: trigger publish
                }
            }
            
            ImGui.EndMenu();
        }

        var text = $"Loaded: '{_openedPackage.Value}'";
        ImGui.SetCursorPos(ImGui.GetWindowContentRegionMax() - ImGui.CalcTextSize(text));
        if (ImGui.BeginMenu(text, false))
        {
            //TODO: info about loaded package and deltas
        }
        ImGui.EndMainMenuBar();
    }
}