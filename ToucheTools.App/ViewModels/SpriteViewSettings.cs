using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

public class SpriteViewSettings
{
    private const long MinimumFrameStepInMillis = 100;
    private readonly DatabaseModel _databaseModel;

    public bool ShowRoom { get; set; }
    public int RoomOffsetX { get; set; }
    public int RoomOffsetY { get; set; }
    
    public List<int> SequenceKeys { get; }
    public int ActiveSequence { get; private set; }
    
    public List<int> Characters { get; private set; } = null!;
    public List<int> Animations { get; private set; } = null!;
    public List<int> Directions { get; private set; } = null!;
    public List<int> Frames { get; private set; } = null!;
    public int ActiveCharacter { get; private set; }
    public int ActiveAnimation { get; private set; }
    public int ActiveDirection { get; private set; }
    public int ActiveFrame { get; private set; }
    
    public bool AutoStepFrame { get; set; }
    public DateTime LastStep { get; set; }
    
    public List<(int, int, int, bool, bool)> PartsView { get; private set; } = null!;

    public SpriteViewSettings(DatabaseModel model)
    {
        _databaseModel = model;
        
        SequenceKeys = model.Sequences.Keys.ToList();
        ActiveSequence = SequenceKeys.First();

        LastStep = DateTime.UtcNow;

        SetActiveSequence(ActiveSequence);
    }

    public void Tick()
    {
        if (!AutoStepFrame)
        {
            return;
        }

        var curTime = DateTime.UtcNow;
        var frames = _databaseModel.Sequences[SequenceKeys[ActiveSequence]]
            .Characters[Characters[ActiveCharacter]]
            .Animations[Animations[ActiveAnimation]]
            .Directions[Directions[ActiveDirection]]
            .Frames;
        var curFrame = frames[Frames[ActiveFrame]];
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

    public void SetActiveSequence(int sequence)
    {
        if (!SequenceKeys.Contains(sequence))
        {
            throw new Exception("Unknown sequence: " + sequence);
        }

        ActiveSequence = sequence;
        Characters = _databaseModel.Sequences[SequenceKeys[ActiveSequence]].Characters.Keys.ToList();
        if (!Characters.Contains(ActiveCharacter))
        {
            ActiveCharacter = Characters.First();
        }
        var character = _databaseModel.Sequences[SequenceKeys[ActiveSequence]].Characters[Characters[ActiveCharacter]];

        Animations = character.Animations.Keys.ToList();
        if (!Animations.Contains(ActiveAnimation))
        {
            ActiveAnimation = Animations.First();
        }
        var animation = character.Animations[Animations[ActiveAnimation]];

        Directions = animation.Directions.Keys.ToList();
        if (!Directions.Contains(ActiveDirection))
        {
            ActiveDirection = Directions.First();
        }
        var direction = animation.Directions[Directions[ActiveDirection]];

        Frames = direction.Frames.Select((_, idx) => idx).ToList();
        if (!Frames.Contains(ActiveFrame))
        {
            ActiveFrame = Frames.First();
        }
        
        GenerateSequenceView();
    }

    public void SelectCharacter(int character)
    {
        ActiveCharacter = character;
        ActiveAnimation = -1;
        ActiveDirection = -1;
        ActiveFrame = -1;
        SetActiveSequence(ActiveSequence);
    }

    public void SelectAnimation(int animation)
    {
        ActiveAnimation = animation;
        ActiveDirection = -1;
        ActiveFrame = -1;
        SetActiveSequence(ActiveSequence);
    }

    public void SelectDirection(int direction)
    {
        ActiveDirection = direction;
        ActiveFrame = -1;
        SetActiveSequence(ActiveSequence);
    }

    public void SelectFrame(int frame)
    {
        ActiveFrame = frame;
        SetActiveSequence(ActiveSequence);
    }

    private void GenerateSequenceView()
    {
        var frame = _databaseModel.Sequences[SequenceKeys[ActiveSequence]]
            .Characters[Characters[ActiveCharacter]]
            .Animations[Animations[ActiveAnimation]]
            .Directions[Directions[ActiveDirection]]
            .Frames[Frames[ActiveFrame]];

        PartsView = frame.Parts.Select(p => ((int)p.FrameIndex, p.DestX, p.DestY, p.HFlipped, p.VFlipped))
            .ToList();
    }
}