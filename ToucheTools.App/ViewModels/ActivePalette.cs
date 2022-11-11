using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

public class ActivePalette
{
    public ActivePalette(DatabaseModel model)
    {
        PaletteKeys = model.Palettes.Keys.ToList();
        Active = PaletteKeys.First();
    }

    public List<int> PaletteKeys { get; }
    public int Active { get; private set; }
    private Action _activeUpdated = () => { };

    public void ObserveActive(Action cb)
    {
        _activeUpdated += cb;
    }
    
    public void SetActive(int palette)
    {
        if (!PaletteKeys.Contains(palette))
        {
            throw new Exception("Unknown palette: " + palette);
        }
        
        Active = palette;
        _activeUpdated();
    }
}