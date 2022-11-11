using System.Numerics;
using ImGuiNET;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class RoomViewWindow : IWindow
{
    private readonly RenderWindow _render;
    private readonly WindowSettings _windowSettings;
    private readonly ActiveData _activeData;

    public RoomViewWindow(RenderWindow render, WindowSettings windowSettings, ActiveData activeData)
    {
        _render = render;
        _windowSettings = windowSettings;
        _activeData = activeData;
    }

    public void Render()
    {
        if (!_windowSettings.RoomViewOpen)
        {
            return;
        }
        
        ImGui.SetNextWindowPos(new Vector2(0.0f, 200.0f));
        ImGui.SetNextWindowSize(new Vector2(Constants.MainWindowWidth, 600.0f));
        ImGui.Begin("Room View", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);

        var (viewId, roomWidth, roomHeight, bytes) = _activeData.RoomView;
    
        var roomTexture = _render.RenderImage(RenderWindow.RenderType.Room, viewId, roomWidth, roomHeight, bytes);
        ImGui.Image(roomTexture, new Vector2(roomWidth, roomHeight));
    
        ImGui.End();
    }
}