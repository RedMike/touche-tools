using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

class ActiveData
{
    private readonly DatabaseModel _databaseModel;
    
    public List<int> PaletteKeys { get; }
    public int ActivePalette { get; private set; }
    public List<int> RoomKeys { get; }
    public int ActiveRoom { get; private set; }
    public List<int> SpriteKeys { get; }
    public int ActiveSprite { get; private set; }
    
    public (string, int, int, byte[]) RoomView { get; private set; }
    public (string, int, int, int, int, byte[]) SpriteView { get; private set; }

    public ActiveData(DatabaseModel model)
    {
        _databaseModel = model;
        
        PaletteKeys = model.Palettes.Keys.ToList();
        RoomKeys = model.Rooms.Keys.ToList();
        SpriteKeys = model.Sprites.Keys.ToList();

        ActivePalette = PaletteKeys.First();
        ActiveRoom = RoomKeys.First();
        ActiveSprite = SpriteKeys.First();
        GenerateRoomView();
        GenerateSpriteView();
    }

    public void SetActivePalette(int palette)
    {
        if (!PaletteKeys.Contains(palette))
        {
            throw new Exception("Unknown palette: " + palette);
        }

        ActivePalette = palette;
        GenerateRoomView();
        GenerateSpriteView();
    }
    
    public void SetActiveRoom(int room)
    {
        if (!RoomKeys.Contains(room))
        {
            throw new Exception("Unknown room: " + room);
        }

        ActiveRoom = room;
        GenerateRoomView();
    }
    
    public void SetActiveSprite(int sprite)
    {
        if (!SpriteKeys.Contains(sprite))
        {
            throw new Exception("Unknown sprite: " + sprite);
        }

        ActiveSprite = sprite;
        GenerateSpriteView();
    }


    private void GenerateRoomView()
    {
        var roomImageId = _databaseModel.Rooms[ActiveRoom].RoomImageNum;
        var roomImage = _databaseModel.RoomImages[roomImageId].Value;
        var palette = _databaseModel.Palettes[ActivePalette];
        var viewId = $"{roomImageId}_{ActivePalette}";

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
    
    private void GenerateSpriteView()
    {
        var sprite = _databaseModel.Sprites[ActiveSprite].Value;
        var palette = _databaseModel.Palettes[ActivePalette];
        var viewId = $"{ActiveSprite}_{ActivePalette}";

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