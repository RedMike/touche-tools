using ToucheTools.App.Services;
using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveSprite : ActiveObservable<int>
{
    private readonly DatabaseModel _databaseModel;
    private readonly ActivePalette _palette;
    private readonly SpriteSheetRenderer _renderer;
    
    public (string, int, int, int, int, byte[]) SpriteView { get; private set; }
    
    public ActiveSprite(DatabaseModel model, ActivePalette palette, SpriteSheetRenderer renderer)
    {
        _databaseModel = model;
        
        SetElements(model.Sprites.Keys.ToList());
        SetActive(Elements.First());
        ObserveActive(GenerateSpriteView);
        
        _palette = palette;
        _renderer = renderer;
        _palette.ObserveActive(GenerateSpriteView);
        
        GenerateSpriteView();
    }
    
    private void GenerateSpriteView()
    {
        var sprite = _databaseModel.Sprites[Active].Value;
        var palette = _databaseModel.Palettes[_palette.Active];

        var (viewId, bytes) = _renderer.RenderSpriteSheet(Active, sprite, _palette.Active, palette);

        SpriteView = (viewId, sprite.Width, sprite.Height, sprite.SpriteWidth, sprite.SpriteHeight, bytes);
    }
}