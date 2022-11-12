﻿using ToucheTools.App.State;
using ToucheTools.App.ViewModels.Observables;
using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

public class SpriteViewSettings
{
    private const long MinimumFrameStepInMillis = 100;
    private readonly DatabaseModel _databaseModel;
    private readonly ActiveSequence _sequence;
    private readonly ActiveCharacter _character;
    private readonly ActiveAnimation _animation;
    private readonly ActiveDirection _direction;
    private readonly ActiveFrame _frame;
    private readonly SpriteViewState _state;

    public bool ShowRoom { get; set; }
    public int RoomOffsetX { get; set; }
    public int RoomOffsetY { get; set; }
    public bool AutoStepFrame { get; set; }
    
    public SpriteViewSettings(DatabaseModel model, ActiveSequence sequence, ActiveCharacter character, ActiveAnimation animation, ActiveDirection direction, ActiveFrame frame, SpriteViewState state)
    {
        _databaseModel = model;
        _sequence = sequence;
        _character = character;
        _animation = animation;
        _direction = direction;
        _frame = frame;
        _state = state;
    }

    public void Tick()
    {
        if (!AutoStepFrame)
        {
            return;
        }

        var curTime = DateTime.UtcNow;
        var lastTime = _state.LastStep;
        var frames = _databaseModel.Sequences[_sequence.Active]
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