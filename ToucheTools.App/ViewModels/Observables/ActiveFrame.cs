using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveFrame : ActiveObservable<int>
{
    private readonly DebuggingGame _game;
    private readonly ActiveSequence _sequence;
    private readonly ActiveCharacter _character;
    private readonly ActiveAnimation _animation;
    private readonly ActiveDirection _direction;

    public List<(int, int, int, bool, bool)> PartsView { get; private set; } = null!;
    public ((int, int, int), int)? FrameView { get; private set; } = null!;

    public ActiveFrame(ActiveSequence sequence, ActiveCharacter character, ActiveAnimation animation, ActiveDirection direction, DebuggingGame game)
    {
        _sequence = sequence;
        _character = character;
        _animation = animation;
        _direction = direction;
        _sequence.ObserveActive(Update);
        _character.ObserveActive(Update);
        _animation.ObserveActive(Update);
        _direction.ObserveActive(Update);
        ObserveActive(Update);
        _game = game;
        game.Observe(Update);
        Update();
    }

    private void Update()
    {
        if (!_game.IsLoaded())
        {
            return;
        }

        var model = _game.Model;
        
        var frames = model
            .Sequences[_sequence.Active]
            .Characters[_character.Active]
            .Animations[_animation.Active]
            .Directions[_direction.Active]
            .Frames;
        var count = frames.Count;
        SetElements(Enumerable.Range(0, count).ToList());
        if (!Elements.Contains(Active))
        {
            SetActive(Elements.First());
        }

        FrameView = ((frames[Active].WalkDx, frames[Active].WalkDy, frames[Active].WalkDz), frames[Active].Delay);
        PartsView = frames[Active].Parts.Select(p => 
                ((int)p.FrameIndex, p.DestX, p.DestY, p.HFlipped, p.VFlipped)
            )
            .ToList();
    }
}