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

    public bool ShowRoom { get; set; }
    public int RoomOffsetX { get; set; }
    public int RoomOffsetY { get; set; }
    
    public List<int> Frames { get; private set; } = null!;
    public int ActiveFrame { get; private set; }
    
    public bool AutoStepFrame { get; set; }
    public DateTime LastStep { get; set; }
    
    public List<(int, int, int, bool, bool)> PartsView { get; private set; } = null!;

    public SpriteViewSettings(DatabaseModel model, ActiveSequence sequence, ActiveCharacter character, ActiveAnimation animation, ActiveDirection direction)
    {
        _databaseModel = model;
        _sequence = sequence;
        _sequence.ObserveActive(GenerateSequenceView);
        _character = character;
        _character.ObserveActive(GenerateSequenceView);
        _animation = animation;
        _animation.ObserveActive(GenerateSequenceView);
        _direction = direction;
        _direction.ObserveActive(GenerateSequenceView);

        LastStep = DateTime.UtcNow;

        GenerateSequenceView();
    }

    public void Tick()
    {
        if (!AutoStepFrame)
        {
            return;
        }

        var curTime = DateTime.UtcNow;
        var frames = _databaseModel.Sequences[_sequence.Active]
            .Characters[_character.Active]
            .Animations[_animation.Active]
            .Directions[_direction.Active]
            .Frames;
        var curFrame = frames[ActiveFrame];
        var nextFrameId = ActiveFrame + 1;
        if (!Frames.Contains(nextFrameId))
        {
            nextFrameId = 0;
        }

        var delay = MinimumFrameStepInMillis;
        if (curFrame.Delay != 0)
        {
            delay = curFrame.Delay * 100;
        }

        if ((curTime - LastStep).TotalMilliseconds > delay)
        {
            LastStep = curTime;
            SelectFrame(nextFrameId);
        }
    }

    public void SelectFrame(int frame)
    {
        ActiveFrame = frame;
        GenerateSequenceView();
    }

    private void GenerateSequenceView()
    {
        var direction = _databaseModel
            .Sequences[_sequence.Active]
            .Characters[_character.Active]
            .Animations[_animation.Active]
            .Directions[_direction.Active];

        Frames = direction.Frames.Select((_, idx) => idx).ToList();
        if (!Frames.Contains(ActiveFrame))
        {
            ActiveFrame = Frames.First();
        }
        
        var frame = _databaseModel.Sequences[_sequence.Active]
            .Characters[_character.Active]
            .Animations[_animation.Active]
            .Directions[_direction.Active]
            .Frames[ActiveFrame];

        PartsView = frame.Parts.Select(p => ((int)p.FrameIndex, p.DestX, p.DestY, p.HFlipped, p.VFlipped))
            .ToList();
    }
}