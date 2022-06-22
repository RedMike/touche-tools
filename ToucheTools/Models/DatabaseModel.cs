namespace ToucheTools.Models;

public class DatabaseModel
{
    public Dictionary<int, string> FailedPrograms { get; set; } = new Dictionary<int, string>();
    public Dictionary<int, ProgramDataModel> Programs { get; set; } = new Dictionary<int, ProgramDataModel>();

    public Dictionary<int, string> FailedSprites { get; set; } = new Dictionary<int, string>();
    public Dictionary<int, SpriteImageDataModel> Sprites { get; set; } = new Dictionary<int, SpriteImageDataModel>();
}