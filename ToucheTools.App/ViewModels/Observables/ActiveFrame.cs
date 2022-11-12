using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveFrame : ActiveObservable<int>
{
    private readonly DatabaseModel _model;
    private readonly ActiveSequence _sequence;
    private readonly ActiveCharacter _character;
    private readonly ActiveAnimation _animation;
    private readonly ActiveDirection _direction;

    public ActiveFrame(DatabaseModel model, ActiveSequence sequence, ActiveCharacter character, ActiveAnimation animation, ActiveDirection direction)
    {
        _model = model;
        _sequence = sequence;
        _character = character;
        _animation = animation;
        _direction = direction;
        _sequence.ObserveActive(Update);
        _character.ObserveActive(Update);
        _animation.ObserveActive(Update);
        _direction.ObserveActive(Update);
        Update();
    }

    private void Update()
    {
        var count = _model
            .Sequences[_sequence.Active]
            .Characters[_character.Active]
            .Animations[_animation.Active]
            .Directions[_direction.Active]
            .Frames.Count;
        SetElements(Enumerable.Range(0, count).ToList());
        if (!Elements.Contains(Active))
        {
            SetActive(Elements.First());
        }
    }
}