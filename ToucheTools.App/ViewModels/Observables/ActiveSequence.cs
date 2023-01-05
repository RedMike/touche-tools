namespace ToucheTools.App.ViewModels.Observables;

public class ActiveSequence : ActiveObservable<int>
{
    private readonly DebuggingGame _game;
    public ActiveSequence(DebuggingGame game)
    {
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
        SetElements(model.Sequences.Keys.ToList());
        SetActive(Elements.First());
    }
}