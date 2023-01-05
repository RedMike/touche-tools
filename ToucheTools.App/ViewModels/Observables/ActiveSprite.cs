using ToucheTools.App.Services;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveSprite : ActiveObservable<int>
{
    private readonly DebuggingGame _game;
    private readonly ActivePalette _palette;
    private readonly SpriteSheetRenderer _renderer;
    
    public (string, int, int, int, int, byte[]) SpriteView { get; private set; }
    
    public ActiveSprite(ActivePalette palette, SpriteSheetRenderer renderer, DebuggingGame game)
    {
        ObserveActive(GenerateSpriteView);
        
        _palette = palette;
        _renderer = renderer;
        _palette.ObserveActive(GenerateSpriteView);
        
        _game = game;
        game.Observe(Update);
        Update();
        
        GenerateSpriteView();
    }
    
    private void Update()
    {
        if (!_game.IsLoaded())
        {
            return;
        }

        var model = _game.Model;
        SetElements(model.Sprites.Keys.ToList());
        SetActive(Elements.First());
    }
    
    private void GenerateSpriteView()
    {
        if (!_game.IsLoaded())
        {
            return;
        }

        var model = _game.Model;
        if (!model.Sprites.ContainsKey(Active))
        {
            return;
        }
        var sprite = model.Sprites[Active].Value;
        var palette = model.Palettes[_palette.Active];

        var (viewId, bytes) = _renderer.RenderSpriteSheet(Active, sprite, _palette.Active, palette);

        SpriteView = (viewId, sprite.Width, sprite.Height, sprite.SpriteWidth, sprite.SpriteHeight, bytes);
    }
}