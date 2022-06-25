namespace ToucheTools.Models;

public class DatabaseModel
{
    public Dictionary<int, string> FailedPrograms { get; set; } = new Dictionary<int, string>();
    public Dictionary<int, ProgramDataModel> Programs { get; set; } = new Dictionary<int, ProgramDataModel>();

    public Dictionary<int, string> FailedSprites { get; set; } = new Dictionary<int, string>();
    public Dictionary<int, Lazy<SpriteImageDataModel>> Sprites { get; set; } = new Dictionary<int, Lazy<SpriteImageDataModel>>();
    
    public Dictionary<int, string> FailedIcons { get; set; } = new Dictionary<int, string>();
    public Dictionary<int, Lazy<IconImageDataModel>> Icons { get; set; } = new Dictionary<int, Lazy<IconImageDataModel>>();


    public Dictionary<int, Lazy<RoomImageDataModel>> RoomImages { get; set; } = new Dictionary<int, Lazy<RoomImageDataModel>>();
    public Dictionary<int, PaletteDataModel> Palettes { get; set; } = new Dictionary<int, PaletteDataModel>();

    public Dictionary<int, string> FailedRooms { get; set; } = new Dictionary<int, string>();

    public Dictionary<int, SequenceDataModel> Sequences { get; set; } = new Dictionary<int, SequenceDataModel>();
}