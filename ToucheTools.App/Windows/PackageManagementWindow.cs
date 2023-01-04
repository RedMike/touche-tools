using System.Numerics;
using ImGuiNET;
using ToucheTools.App.Models;
using ToucheTools.App.State;
using ToucheTools.App.Utils;
using ToucheTools.App.ViewModels;
using ToucheTools.Constants;

namespace ToucheTools.App.Windows;

public class PackageManagementWindow : BaseWindow
{
    private readonly MainWindowState _state;
    private readonly OpenedPackage _package;

    public PackageManagementWindow(MainWindowState state, OpenedPackage package)
    {
        _state = state;
        _package = package;
    }

    public override void Render()
    {
        if (_state.State != MainWindowState.States.PackageManagement)
        {
            return;
        }
        
        ImGui.Begin("Package Editor", ImGuiWindowFlags.NoCollapse);

        var origName = _package.LoadedPackage.Name;
        var name = origName;
        ImGui.InputText("Package Name", ref name, 128);
        if (name != origName)
        {
            _package.SetPackageName(name);
        }
        
        ImGui.Text("\n");
        ImGui.Text("\n");
        
        ImGui.Separator();

        if (ImGui.Button("Close"))
        {
            _state.State = MainWindowState.States.Idle;
        }
        ImGui.End();
    }
}