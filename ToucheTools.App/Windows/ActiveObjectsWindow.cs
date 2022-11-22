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
    private readonly ActiveProgram _program;

    public ActiveObjectsWindow(WindowSettings windowSettings, ActivePalette palette, ActiveRoom room, ActiveSprite sprite, ActiveProgram program)
    {
        _windowSettings = windowSettings;
        _palette = palette;
        _room = room;
        _sprite = sprite;
        _program = program;
    }

    public override void Render()
    {
        ImGui.SetNextWindowPos(new Vector2(150.0f, 0.0f));
        ImGui.SetNextWindowSize(new Vector2(200.0f, 200.0f));
        ImGui.Begin("Active", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

        ObservableCombo("Palette", _palette);
        ObservableCombo("Room", _room);
        ObservableCombo("Sprite", _sprite);
        ObservableCombo("Program", _program);
        
        ImGui.End();
    }
}