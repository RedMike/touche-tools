using ToucheTools.App.Services;
using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveRoom : ActiveObservable<int>
{
    private readonly DebuggingGame _game;
    private readonly ActivePalette _palette;
    private readonly RoomImageRenderer _renderer;
    
    public (string, int, int, byte[]) RoomView { get; private set; }
    
    public ActiveRoom(ActivePalette palette, RoomImageRenderer renderer, DebuggingGame game)
    {
        ObserveActive(GenerateRoomView);
        _palette = palette;
        _renderer = renderer;
        _palette.ObserveActive(GenerateRoomView);
        
        _game = game;
        game.Observe(Update);
        Update();

        GenerateRoomView();
    }

    private void Update()
    {
        if (!_game.IsLoaded())
        {
            return;
        }

        var model = _game.Model;
        
        SetElements(model.Rooms.Keys.ToList());
        SetActive(Elements.First());
    }
    
    private void GenerateRoomView()
    {
        if (!_game.IsLoaded())
        {
            return;
        }

        var model = _game.Model;
        
        var roomImageId = model.Rooms[Active].RoomImageNum;
        var roomImage = model.RoomImages[roomImageId].Value;
        var palette = model.Palettes[_palette.Active];

        var (viewId, bytes) = _renderer.RenderRoomImage(roomImageId, roomImage, palette, new List<(int, SpriteImageDataModel, int, int)>());

        RoomView = (viewId, roomImage.Width, roomImage.Height, bytes);
    }
}