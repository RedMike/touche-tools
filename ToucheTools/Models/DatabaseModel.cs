namespace ToucheTools.Models;

public class DatabaseModel
{
    public Dictionary<int, string> FailedPrograms { get; set; } = new Dictionary<int, string>();
    public Dictionary<int, ProgramDataModel> Programs { get; set; } = new Dictionary<int, ProgramDataModel>();

    public Dictionary<int, string> FailedSprites { get; set; } = new Dictionary<int, string>();
    public Dictionary<int, SpriteImageDataModel> Sprites { get; set; } = new Dictionary<int, SpriteImageDataModel>();

    public Dictionary<int, RoomImageDataModel> RoomImages { get; set; } = new Dictionary<int, RoomImageDataModel>();
    public Dictionary<int, PaletteDataModel> Palettes { get; set; } = new Dictionary<int, PaletteDataModel>();

    public Dictionary<int, string> FailedRooms { get; set; } = new Dictionary<int, string>();
}