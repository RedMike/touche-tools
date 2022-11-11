﻿using System.Numerics;
using ImGuiNET;
using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;

namespace ToucheTools.App.Windows;

public class ActiveObjectsWindow : BaseWindow
{
    private readonly WindowSettings _windowSettings;
    private readonly ActiveData _activeData;
    private readonly ActivePalette _palette;

    public ActiveObjectsWindow(WindowSettings windowSettings, ActiveData activeData, ActivePalette palette)
    {
        _windowSettings = windowSettings;
        _activeData = activeData;
        _palette = palette;
    }

    public override void Render()
    {
        if (!_windowSettings.RoomViewOpen && !_windowSettings.SpriteViewOpen)
        {
            return;
        }

        var originalRoomId = _activeData.RoomKeys.FindIndex(k => k == _activeData.ActiveRoom);
        var curRoomId = originalRoomId;
        var rooms = _activeData.RoomKeys.ToArray();

        var originalSpriteId = _activeData.SpriteKeys.FindIndex(k => k == _activeData.ActiveSprite);
        var curSpriteId = originalSpriteId;
        var sprites = _activeData.SpriteKeys.ToArray();

        ImGui.SetNextWindowPos(new Vector2(150.0f, 0.0f));
        ImGui.SetNextWindowSize(new Vector2(200.0f, 200.0f));
        ImGui.Begin("Active", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

        ObservableCombo("Palette", _palette);
    
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