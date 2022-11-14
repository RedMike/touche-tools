using System.Numerics;
using ImGuiNET;
using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;

namespace ToucheTools.App.Windows;

public class RoomViewSettingsWindow : BaseWindow
{
    private readonly WindowSettings _windowSettings;
    private readonly RoomViewSettings _viewSettings;
    private readonly MultiActiveRects _rects;


    public RoomViewSettingsWindow(WindowSettings windowSettings, RoomViewSettings viewSettings, MultiActiveRects rects)
    {
        _windowSettings = windowSettings;
        _viewSettings = viewSettings;
        _rects = rects;
    }

    public override void Render()
    {
        if (!_windowSettings.RoomViewOpen)
        {
            return;
        }
        
        ImGui.SetNextWindowPos(new Vector2(350.0f, 0.0f));
        ImGui.SetNextWindowSize(new Vector2(Constants.MainWindowWidth - 350.0f, 200.0f));
        ImGui.Begin("View Settings", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

        ObservableCheckboxList("Rects", _rects);
        
        ImGui.End();
    }
}