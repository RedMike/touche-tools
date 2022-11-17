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
    private readonly MultiActiveBackgrounds _backgrounds;
    private readonly ActiveRoom _room;
    private readonly MultiActiveAreas _areas;
    private readonly MultiActivePoints _points;


    public RoomViewSettingsWindow(WindowSettings windowSettings, RoomViewSettings viewSettings, MultiActiveRects rects, MultiActiveBackgrounds backgrounds, ActiveRoom room, MultiActiveAreas areas, MultiActivePoints points)
    {
        _windowSettings = windowSettings;
        _viewSettings = viewSettings;
        _rects = rects;
        _backgrounds = backgrounds;
        _room = room;
        _areas = areas;
        _points = points;
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

        var (_, roomW, roomH, _) = _room.RoomView;
        var origAreaOffsetX = _viewSettings.AreaOffsetX;
        if (origAreaOffsetX > roomW)
        {
            origAreaOffsetX = roomW;
        }
        var areaOffsetX = origAreaOffsetX;
        ImGui.SliderInt("Area X offset", ref areaOffsetX, -roomW, roomW);
        if (areaOffsetX != origAreaOffsetX)
        {
            _viewSettings.AreaOffsetX = areaOffsetX;
        }
        
        var origAreaOffsetY = _viewSettings.AreaOffsetY;
        if (origAreaOffsetY > roomH)
        {
            origAreaOffsetY = roomH;
        }
        var areaOffsetY = origAreaOffsetY;
        ImGui.SliderInt("Area Y offset", ref areaOffsetY, -roomH, roomH);
        if (areaOffsetY != origAreaOffsetY)
        {
            _viewSettings.AreaOffsetY = areaOffsetY;
        }
        
        ObservableCheckboxList("Rects", _rects);
        ObservableCheckboxList("Backgrounds", _backgrounds);
        ObservableCheckboxList("Areas", _areas);
        ObservableCheckboxList("Points", _points);
        
        ImGui.End();
    }
}