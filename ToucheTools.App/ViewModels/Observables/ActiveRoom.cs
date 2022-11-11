using ToucheTools.Models;

namespace ToucheTools.App.ViewModels.Observables;

public class ActiveRoom : ActiveObservable<int>
{
    private readonly DatabaseModel _databaseModel;
    private readonly ActivePalette _palette;
    
    public (string, int, int, byte[]) RoomView { get; private set; }
    
    public ActiveRoom(DatabaseModel model, ActivePalette palette)
    {
        _databaseModel = model;
        SetElements(model.Rooms.Keys.ToList());
        SetActive(Elements.First());
        ObserveActive(GenerateRoomView);
        
        _palette = palette;
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
        
        var bytes = new byte[roomImage.Width * roomImage.Height * 4];
        for (var i = 0; i < roomImage.Width; i++)
        {
            for (var j = 0; j < roomImage.Height; j++)
            {
                var rawCol = roomImage.RawData[j, i];
                var col = palette.Colors[rawCol];
                bytes[(j * roomImage.Width + i) * 4 + 0] = col.R;
                bytes[(j * roomImage.Width + i) * 4 + 1] = col.G;
                bytes[(j * roomImage.Width + i) * 4 + 2] = col.B;
                bytes[(j * roomImage.Width + i) * 4 + 3] = 255;
            }
        }

        RoomView = (viewId, roomImage.Width, roomImage.Height, bytes);
    }
}