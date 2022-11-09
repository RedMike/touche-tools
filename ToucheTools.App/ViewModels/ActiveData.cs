using ToucheTools.Models;

namespace ToucheTools.App.ViewModels;

class ActiveData
{
    public List<int> PaletteKeys { get; }
    public int ActivePalette { get; private set; }
    public List<int> RoomKeys { get; }
    public int ActiveRoom { get; private set; }
    public List<int> RoomImageKeys { get; }

    public ActiveData(DatabaseModel model)
    {
        PaletteKeys = model.Palettes.Keys.ToList();
        RoomKeys = model.Rooms.Keys.ToList();
        RoomImageKeys = model.RoomImages.Keys.ToList();

        ActivePalette = PaletteKeys.First();
        ActiveRoom = RoomKeys.First();
    }

    public void SetActivePalette(int palette)
    {
        if (!PaletteKeys.Contains(palette))
        {
            throw new Exception("Unknown palette: " + palette);
        }

        ActivePalette = palette;
    }
    
    public void SetActiveRoom(int room)
    {
        if (!RoomKeys.Contains(room))
        {
            throw new Exception("Unknown room: " + room);
        }

        ActiveRoom = room;
    }
}