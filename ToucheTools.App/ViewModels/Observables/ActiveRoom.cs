using ToucheTools.App.Services;
using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveRoom : ActiveObservable<int>
{
    private readonly DatabaseModel _databaseModel;
    private readonly ActivePalette _palette;
    private readonly RoomImageRenderer _renderer;
    
    public (string, int, int, byte[]) RoomView { get; private set; }
    
    public ActiveRoom(DatabaseModel model, ActivePalette palette, RoomImageRenderer renderer)
    {
        _databaseModel = model;
        SetElements(model.Rooms.Keys.ToList());
        SetActive(Elements.First());
        ObserveActive(GenerateRoomView);
        
        _palette = palette;
        _renderer = renderer;
        _palette.ObserveActive(GenerateRoomView);
        
        GenerateRoomView();
    }
    
    private void GenerateRoomView()
    {
        var roomImageId = _databaseModel.Rooms[Active].RoomImageNum;
        var roomImage = _databaseModel.RoomImages[roomImageId].Value;
        var palette = _databaseModel.Palettes[_palette.Active];
        var viewId = $"{roomImageId}_{_palette.Active}";

        if (RoomView.Item1 == viewId)
        {
            return;
        }

        var bytes = _renderer.RenderRoomImage(roomImageId, roomImage, _palette.Active, palette);

        RoomView = (viewId, roomImage.Width, roomImage.Height, bytes);
    }
}