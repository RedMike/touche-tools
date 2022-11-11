using System.Numerics;
using ImGuiNET;
using ToucheTools.App.ViewModels;

namespace ToucheTools.App.Windows;

public class SpriteViewSettingsWindow : IWindow
{
    private readonly WindowSettings _windowSettings;
    private readonly ActiveData _activeData;
    private readonly SpriteViewSettings _viewSettings;

    public SpriteViewSettingsWindow(WindowSettings windowSettings, ActiveData activeData, SpriteViewSettings viewSettings)
    {
        _windowSettings = windowSettings;
        _activeData = activeData;
        _viewSettings = viewSettings;
    }

    public void Render()
    {
        if (!_windowSettings.SpriteViewOpen)
        {
            return;
        }
        _viewSettings.Tick(); //TODO: move
    
        var origShowRoom = _viewSettings.ShowRoom;
        var showRoom = origShowRoom;

        var origAutoStep = _viewSettings.AutoStepFrame;
        var autoStep = origAutoStep;
        
        var originalSequenceId = _viewSettings.SequenceKeys.FindIndex(k => k == _viewSettings.ActiveSequence);
        var curSequenceId = originalSequenceId;
        var sequences = _viewSettings.SequenceKeys.ToArray();
        
        var originalCharacterId = _viewSettings.Characters.FindIndex(k => k == _viewSettings.ActiveCharacter);
        var curCharacterId = originalCharacterId;
        var characters = _viewSettings.Characters.ToArray();
        
        var originalAnimationId = _viewSettings.Animations.FindIndex(k => k == _viewSettings.ActiveAnimation);
        var curAnimationId = originalAnimationId;
        var animations = _viewSettings.Animations.ToArray();
        
        var originalDirectionId = _viewSettings.Directions.FindIndex(k => k == _viewSettings.ActiveDirection);
        var curDirectionId = originalDirectionId;
        var directions = _viewSettings.Directions.ToArray();
        
        var originalFrameId = _viewSettings.Frames.FindIndex(k => k == _viewSettings.ActiveFrame);
        var curFrameId = originalFrameId;
        var frames = _viewSettings.Frames.ToArray();
        
        ImGui.SetNextWindowPos(new Vector2(350.0f, 0.0f));
        ImGui.SetNextWindowSize(new Vector2(Constants.MainWindowWidth-500.0f, 200.0f));
        ImGui.Begin("View Settings", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);

        ImGui.Combo("Sequence", ref curSequenceId, sequences.Select(k => k.ToString()).ToArray(), sequences.Length);
        if (curSequenceId != originalSequenceId)
        {
            _viewSettings.SetActiveSequence(sequences[curSequenceId]);
        }
        
        ImGui.SetNextItemWidth((Constants.MainWindowWidth-500.0f)/4.0f);
        ImGui.Combo("Character", ref curCharacterId, characters.Select(k => k.ToString()).ToArray(), characters.Length);
        if (curCharacterId != originalCharacterId)
        {
            _viewSettings.SelectCharacter(characters[curCharacterId]);
        }

        ImGui.SetNextItemWidth((Constants.MainWindowWidth-500.0f)/4.0f);
        ImGui.SameLine();
        ImGui.Combo("Animation", ref curAnimationId, animations.Select(k => k.ToString()).ToArray(), animations.Length);
        if (curAnimationId != originalAnimationId)
        {
            _viewSettings.SelectAnimation(animations[curAnimationId]);
        }
        
        ImGui.SetNextItemWidth((Constants.MainWindowWidth-500.0f)/4.0f);
        ImGui.Combo("Direction", ref curDirectionId, directions.Select(k => k.ToString()).ToArray(), directions.Length);
        if (curDirectionId != originalDirectionId)
        {
            _viewSettings.SelectDirection(directions[curDirectionId]);
        }
        
        ImGui.SetNextItemWidth((Constants.MainWindowWidth-500.0f)/4.0f);
        ImGui.SameLine();
        ImGui.Combo("Frame", ref curFrameId, frames.Select(k => k.ToString()).ToArray(), frames.Length);
        if (curFrameId != originalFrameId)
        {
            _viewSettings.SelectFrame(frames[curFrameId]);
        }
        
        ImGui.Checkbox("Auto step", ref autoStep);
        if (autoStep != origAutoStep)
        {
            _viewSettings.AutoStepFrame = autoStep;
        }
        
        
        ImGui.Checkbox("Room background", ref showRoom);
        if (showRoom != origShowRoom)
        {
            _viewSettings.ShowRoom = showRoom;
        }

        if (showRoom)
        {
            var (_, roomW, roomH, _) = _activeData.RoomView;
            
            var origRoomX = _viewSettings.RoomOffsetX;
            if (origRoomX > roomW)
            {
                origRoomX = roomW;
            }
            var roomX = origRoomX;
            ImGui.SliderInt("X offset", ref roomX, 0, roomW);
            if (roomX != origRoomX)
            {
                _viewSettings.RoomOffsetX = roomX;
            }

            var origRoomY = _viewSettings.RoomOffsetY;
            if (origRoomY > roomH)
            {
                origRoomY = roomH;
            }
            var roomY = origRoomY;
            ImGui.SliderInt("Y offset", ref roomY, 0, roomH);
            if (roomY != origRoomY)
            {
                _viewSettings.RoomOffsetY = roomY;
            }
        }
        
        ImGui.End();
    }
}