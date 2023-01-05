namespace ToucheTools.App.ViewModels.Observables;

public class ActivePalette : ActiveObservable<int>
{
    private readonly DebuggingGame _game;
    
    public ActivePalette(DebuggingGame game)
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
        SetElements(model.Palettes.Keys.ToList());
        SetActive(Elements.First());
    }
}