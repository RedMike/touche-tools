using System.Numerics;
using ImGuiNET;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class ActiveObjectsWindow : IWindow
{
    private readonly WindowSettings _windowSettings;
    private readonly ActiveData _activeData;

    public ActiveObjectsWindow(WindowSettings windowSettings, ActiveData activeData)
    {
        _windowSettings = windowSettings;
        _activeData = activeData;
    }

    public void Render()
    {
        if (!_windowSettings.RoomViewOpen && !_windowSettings.SpriteViewOpen)
        {
            return;
        }
        var originalPaletteId = _activeData.PaletteKeys.FindIndex(k => k == _activeData.ActivePalette); 
        var curPaletteId = originalPaletteId;
        var palettes = _activeData.PaletteKeys.ToArray();

        var originalRoomId = _activeData.RoomKeys.FindIndex(k => k == _activeData.ActiveRoom);
        var curRoomId = originalRoomId;
        var rooms = _activeData.RoomKeys.ToArray();

        var originalSpriteId = _activeData.SpriteKeys.FindIndex(k => k == _activeData.ActiveSprite);
        var curSpriteId = originalSpriteId;
        var sprites = _activeData.SpriteKeys.ToArray();

        ImGui.SetNextWindowPos(new Vector2(150.0f, 0.0f));
        ImGui.SetNextWindowSize(new Vector2(200.0f, 200.0f));
        ImGui.Begin("Active", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
    
        ImGui.Combo("Palette", ref curPaletteId, palettes.Select(k => k.ToString()).ToArray(), palettes.Length);
        if (curPaletteId != originalPaletteId)
        {
            _activeData.SetActivePalette(palettes[curPaletteId]);
        }
    
        ImGui.Combo("Room", ref curRoomId, rooms.Select(k => k.ToString()).ToArray(), rooms.Length);
        if (curRoomId != originalRoomId)
        {
            _activeData.SetActiveRoom(rooms[curRoomId]);
        }
    
        ImGui.Combo("Sprite", ref curSpriteId, sprites.Select(k => k.ToString()).ToArray(), sprites.Length);
        if (curSpriteId != originalSpriteId)
        {
            _activeData.SetActiveSprite(sprites[curSpriteId]);
        }
    
        ImGui.End();
    }
}