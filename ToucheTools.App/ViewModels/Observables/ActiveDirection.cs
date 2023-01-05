using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveDirection : ActiveObservable<int>
{
    private readonly DebuggingGame _game;
    private readonly ActiveSequence _sequence;
    private readonly ActiveCharacter _character;
    private readonly ActiveAnimation _animation;
    
    public ActiveDirection(ActiveSequence sequence, ActiveCharacter character, ActiveAnimation animation, DebuggingGame game)
    {
        _sequence = sequence;
        _character = character;
        _animation = animation;
        _sequence.ObserveActive(Update);
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
        SetElements(model
            .Sequences[_sequence.Active]
            .Characters[_character.Active]
            .Animations[_animation.Active]
            .Directions.Keys.ToList());
        if (!Elements.Contains(Active))
        {
            SetActive(Elements.First());
        }
    }
}