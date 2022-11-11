using System.Numerics;
using ImGuiNET;
using ToucheTools.App.ViewModels;
using ToucheTools.App.ViewModels.Observables;

namespace ToucheTools.App.Windows;

public class SpriteViewSettingsWindow : BaseWindow
{
    private readonly WindowSettings _windowSettings;
    private readonly SpriteViewSettings _viewSettings;
    private readonly ActiveRoom _room;
    private readonly ActiveSequence _sequence;
    private readonly ActiveCharacter _character;

    public SpriteViewSettingsWindow(WindowSettings windowSettings, SpriteViewSettings viewSettings, ActiveRoom room, ActiveSequence sequence, ActiveCharacter character)
    {
        _windowSettings = windowSettings;
        _viewSettings = viewSettings;
        _room = room;
        _sequence = sequence;
        _character = character;
    }

    public override void Render()
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

        ObservableCombo("Sequence", _sequence);
        
        ImGui.SetNextItemWidth((Constants.MainWindowWidth-500.0f)/4.0f);
        ObservableCombo("Character", _character);

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
            var (_, roomW, roomH, _) = _room.RoomView;
            
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