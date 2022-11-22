using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveCharacter : ActiveObservable<int>
{
    private readonly DatabaseModel _model;
    private readonly ActiveSequence _sequence;

    public ActiveCharacter(DatabaseModel model, ActiveSequence sequence)
    {
        _model = model;
        _sequence = sequence;
        _sequence.ObserveActive(Update);
        Update();
    }

    private void Update()
    {
        SetElements(_model.Sequences[_sequence.Active].Characters.Keys.ToList());
        if (!Elements.Contains(Active))
        {
            SetActive(Elements.First());
        }
    }
}