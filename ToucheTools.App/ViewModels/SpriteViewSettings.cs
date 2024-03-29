﻿using ToucheTools.App.State;
using ToucheTools.App.ViewModels.Observables;
using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

public class SpriteViewSettings
{
    private const long MinimumFrameStepInMillis = 100;
    private readonly ActiveSequence _sequence;
    private readonly ActiveCharacter _character;
    private readonly ActiveAnimation _animation;
    private readonly ActiveDirection _direction;
    private readonly ActiveFrame _frame;
    private readonly SpriteViewState _state;
    private readonly DebuggingGame _game;

    public bool ShowRoom { get; set; }
    public int RoomOffsetX { get; set; }
    public int RoomOffsetY { get; set; }
    public bool AutoStepFrame { get; set; }
    public bool ShowEntireSheet { get; set; }
    
    public SpriteViewSettings(ActiveSequence sequence, ActiveCharacter character, ActiveAnimation animation, ActiveDirection direction, ActiveFrame frame, SpriteViewState state, DebuggingGame game)
    {
        _sequence = sequence;
        _character = character;
        _animation = animation;
        _direction = direction;
        _frame = frame;
        _state = state;
        _game = game;
    }

    public void Tick()
    {
        if (!_game.IsLoaded())
        {
            return;
        }
        var model = _game.Model;
        
        if (!AutoStepFrame)
        {
            _state.PositionOffset = (0, 0, 0);
            return;
        }

        var curTime = DateTime.UtcNow;
        var lastTime = _state.LastStep;
        var frames = model.Sequences[_sequence.Active]
            .Characters[_character.Active]
            .Animations[_animation.Active]
            .Directions[_direction.Active]
            .Frames;
        var curFrame = frames[_frame.Active];
        var nextFrameId = _frame.Active + 1;
        if (!_frame.Elements.Contains(nextFrameId))
        {
            nextFrameId = 0;
        }

        _state.PositionOffset = (curFrame.WalkDx * nextFrameId * (_direction.Active == 3 ? -1 : 1), curFrame.WalkDy * nextFrameId,
            curFrame.WalkDz * nextFrameId);

        var delay = MinimumFrameStepInMillis;
        if (curFrame.Delay != 0)
        {
            delay = curFrame.Delay * 100;
        }

        if ((curTime - lastTime).TotalMilliseconds > delay)
        {
            _state.LastStep = curTime;
            _frame.SetActive(nextFrameId);
        }
    }
}