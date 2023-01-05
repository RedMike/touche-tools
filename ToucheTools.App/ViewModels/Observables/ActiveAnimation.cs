using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveAnimation : ActiveObservable<int>
{
    private readonly DebuggingGame _game;
    private readonly ActiveSequence _sequence;
    private readonly ActiveCharacter _character;
    
    public ActiveAnimation(ActiveSequence sequence, ActiveCharacter character, DebuggingGame game)
    {
        _character = character;
        _character.ObserveActive(Update);
        _sequence = sequence;
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
            .Animations.Keys.ToList());
        if (!Elements.Contains(Active))
        {
            SetActive(Elements.First());
        }
    }
}