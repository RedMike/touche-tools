namespace ToucheTools.App.ViewModels.Observables;

public class ActiveCharacter : ActiveObservable<int>
{
    private readonly DebuggingGame _game;
    private readonly ActiveSequence _sequence;

    public ActiveCharacter(ActiveSequence sequence, DebuggingGame game)
    {
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
        SetElements(model.Sequences[_sequence.Active].Characters.Keys.ToList());
        if (!Elements.Contains(Active))
        {
            SetActive(Elements.First());
        }
    }
}