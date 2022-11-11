using System.Numerics;
using ImGuiNET;
using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;

namespace ToucheTools.App.Windows;

public class ActiveObjectsWindow : BaseWindow
{
    private readonly WindowSettings _windowSettings;
    private readonly ActivePalette _palette;
    private readonly ActiveRoom _room;
    private readonly ActiveSprite _sprite;

    public ActiveObjectsWindow(WindowSettings windowSettings, ActivePalette palette, ActiveRoom room, ActiveSprite sprite)
    {
        _windowSettings = windowSettings;
        _palette = palette;
        _room = room;
        _sprite = sprite;
    }

    public override void Render()
    {
        if (!_windowSettings.RoomViewOpen && !_windowSettings.SpriteViewOpen)
        {
            return;
        }

        ImGui.SetNextWindowPos(new Vector2(150.0f, 0.0f));
        ImGui.SetNextWindowSize(new Vector2(200.0f, 200.0f));
        ImGui.Begin("Active", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

        ObservableCombo("Palette", _palette);
        ObservableCombo("Room", _room);
        ObservableCombo("Sprite", _sprite);
        
        ImGui.End();
    }
}