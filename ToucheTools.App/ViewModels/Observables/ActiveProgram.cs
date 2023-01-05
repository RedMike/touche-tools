using ToucheTools.Constants;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveProgram : ActiveObservable<int>
{
    private readonly DebuggingGame _game;
    
    public ActiveProgram(DebuggingGame game)
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
        SetElements(model.Programs.Keys.ToList());
        SetActive(Elements.Contains(Game.StartupEpisode) ? Game.StartupEpisode : Elements.First());
    }
}