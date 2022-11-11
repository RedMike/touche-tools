using ToucheTools.App.ViewModels.Observables;
using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

public class SpriteViewSettings
{
    private const long MinimumFrameStepInMillis = 100;
    private readonly DatabaseModel _databaseModel;
    private readonly ActiveSequence _sequence;
    private readonly ActiveCharacter _character;

    public bool ShowRoom { get; set; }
    public int RoomOffsetX { get; set; }
    public int RoomOffsetY { get; set; }
    
    public List<int> Animations { get; private set; } = null!;
    public List<int> Directions { get; private set; } = null!;
    public List<int> Frames { get; private set; } = null!;
    public int ActiveAnimation { get; private set; }
    public int ActiveDirection { get; private set; }
    public int ActiveFrame { get; private set; }
    
    public bool AutoStepFrame { get; set; }
    public DateTime LastStep { get; set; }
    
    public List<(int, int, int, bool, bool)> PartsView { get; private set; } = null!;

    public SpriteViewSettings(DatabaseModel model, ActiveSequence sequence, ActiveCharacter character)
    {
        _databaseModel = model;
        _sequence = sequence;
        _sequence.ObserveActive(GenerateSequenceView);
        _character = character;
        _character.ObserveActive(GenerateSequenceView);

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
            .Animations[ActiveAnimation]
            .Directions[ActiveDirection]
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

    public void SelectAnimation(int animation)
    {
        ActiveAnimation = animation;
        ActiveDirection = -1;
        ActiveFrame = -1;
        GenerateSequenceView();
    }

    public void SelectDirection(int direction)
    {
        ActiveDirection = direction;
        ActiveFrame = -1;
        GenerateSequenceView();
    }

    public void SelectFrame(int frame)
    {
        ActiveFrame = frame;
        GenerateSequenceView();
    }

    private void GenerateSequenceView()
    {
        var character = _databaseModel.Sequences[_sequence.Active].Characters[_character.Active];

        Animations = character.Animations.Keys.ToList();
        if (!Animations.Contains(ActiveAnimation))
        {
            ActiveAnimation = Animations.First();
        }
        var animation = character.Animations[ActiveAnimation];

        Directions = animation.Directions.Keys.ToList();
        if (!Directions.Contains(ActiveDirection))
        {
            ActiveDirection = Directions.First();
        }
        var direction = animation.Directions[ActiveDirection];

        Frames = direction.Frames.Select((_, idx) => idx).ToList();
        if (!Frames.Contains(ActiveFrame))
        {
            ActiveFrame = Frames.First();
        }
        
        var frame = _databaseModel.Sequences[_sequence.Active]
            .Characters[_character.Active]
            .Animations[ActiveAnimation]
            .Directions[ActiveDirection]
            .Frames[ActiveFrame];

        PartsView = frame.Parts.Select(p => ((int)p.FrameIndex, p.DestX, p.DestY, p.HFlipped, p.VFlipped))
            .ToList();
    }
}