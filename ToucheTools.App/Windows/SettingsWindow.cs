using System.Numerics;
using ImGuiNET;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class SettingsWindow : IWindow
{
    private readonly DebuggingGame _game;
    private readonly WindowSettings _settings;

    public SettingsWindow(WindowSettings settings, DebuggingGame game)
    {
        _settings = settings;
        _game = game;
    }

    public void Render()
    {
        if (!_game.IsLoaded())
        {
            return;
        }
        
        bool origRoomViewShown = _settings.RoomViewOpen;
        bool roomViewShown = origRoomViewShown;

        bool origSpriteViewShown = _settings.SpriteViewOpen;
        bool spriteViewShown = origSpriteViewShown;

        bool origProgramViewShown = _settings.ProgramViewOpen;
        bool programViewShown = origProgramViewShown;
    
        ImGui.SetNextWindowPos(new Vector2(0.0f, 20.0f));
        ImGui.SetNextWindowSize(new Vector2(150.0f, 180.0f));
        ImGui.Begin("Windows", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
        ImGui.Checkbox("Room View", ref roomViewShown);
        if (roomViewShown != origRoomViewShown)
        {
            _settings.CloseAllViews();
            if (roomViewShown)
            {
                _settings.OpenRoomView();
            }
        }
        ImGui.Checkbox("Sprite View", ref spriteViewShown);
        if (spriteViewShown != origSpriteViewShown)
        {
            _settings.CloseAllViews();
            if (spriteViewShown)
            {
                _settings.OpenSpriteView();
            }
        }
        ImGui.Checkbox("Program View", ref programViewShown);
        if (programViewShown != origProgramViewShown)
        {
            _settings.CloseAllViews();
            if (programViewShown)
            {
                _settings.OpenProgramView();
            }
        }
        ImGui.End();
    }
}