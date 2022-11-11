using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveSprite : ActiveObservable<int>
{
    private readonly DatabaseModel _databaseModel;
    private readonly ActivePalette _palette;
    
    public (string, int, int, int, int, byte[]) SpriteView { get; private set; }
    
    public ActiveSprite(DatabaseModel model, ActivePalette palette)
    {
        _databaseModel = model;
        
        SetElements(model.Sprites.Keys.ToList());
        SetActive(Elements.First());
        ObserveActive(GenerateSpriteView);
        
        _palette = palette;
        _palette.ObserveActive(GenerateSpriteView);
        
        GenerateSpriteView();
    }
    
    private void GenerateSpriteView()
    {
        var sprite = _databaseModel.Sprites[Active].Value;
        var palette = _databaseModel.Palettes[_palette.Active];
        var viewId = $"{Active}_{_palette.Active}";

        if (SpriteView.Item1 == viewId)
        {
            return;
        }
        
        var bytes = new byte[sprite.Width * sprite.Height * 4];
        for (var i = 0; i < sprite.Width; i++)
        {
            for (var j = 0; j < sprite.Height; j++)
            {
                var rawCol = sprite.DecodedData[j, i];
                if (rawCol == 0 || rawCol == 64)
                {
                    //transparent
                    bytes[(j * sprite.Width + i) * 4 + 0] = 0;
                    bytes[(j * sprite.Width + i) * 4 + 1] = 0;
                    bytes[(j * sprite.Width + i) * 4 + 2] = 0;
                    bytes[(j * sprite.Width + i) * 4 + 3] = 0;
                }
                else
                {
                    //actual colour
                    var col = palette.Colors[rawCol];
                    bytes[(j * sprite.Width + i) * 4 + 0] = col.R;
                    bytes[(j * sprite.Width + i) * 4 + 1] = col.G;
                    bytes[(j * sprite.Width + i) * 4 + 2] = col.B;
                    bytes[(j * sprite.Width + i) * 4 + 3] = 255;
                }
            }
        }

        SpriteView = (viewId, sprite.Width, sprite.Height, sprite.SpriteWidth, sprite.SpriteHeight, bytes);
    }
}