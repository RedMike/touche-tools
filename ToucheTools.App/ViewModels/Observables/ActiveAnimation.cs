using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveAnimation : ActiveObservable<int>
{
    private readonly DatabaseModel _model;
    private readonly ActiveSequence _sequence;
    private readonly ActiveCharacter _character;
    
    public ActiveAnimation(DatabaseModel model, ActiveSequence sequence, ActiveCharacter character)
    {
        _model = model;
        _sequence = sequence;
        _sequence.ObserveActive(Update);
        _character = character;
        _character.ObserveActive(Update);
        Update();
    }

    private void Update()
    {
        SetElements(_model
            .Sequences[_sequence.Active]
            .Characters[_character.Active]
            .Animations.Keys.ToList());
        if (!Elements.Contains(Active))
        {
            SetActive(Elements.First());
        }
    }
}