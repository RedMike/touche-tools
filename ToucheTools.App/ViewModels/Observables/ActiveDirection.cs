using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveDirection : ActiveObservable<int>
{
    private readonly DatabaseModel _model;
    private readonly ActiveSequence _sequence;
    private readonly ActiveCharacter _character;
    private readonly ActiveAnimation _animation;
    
    public ActiveDirection(DatabaseModel model, ActiveSequence sequence, ActiveCharacter character, ActiveAnimation animation)
    {
        _model = model;
        _sequence = sequence;
        _character = character;
        _animation = animation;
        _sequence.ObserveActive(Update);
        Update();
    }

    private void Update()
    {
        SetElements(_model
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