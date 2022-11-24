namespace ToucheTools.App.State;

public class SpriteViewState
{
    public DateTime LastStep { get; set; } = DateTime.UtcNow;
    public (int, int, int) PositionOffset { get; set; } = (0, 0, 0);
}