using System.Numerics;

namespace ToucheTools.App.State;

public class GameViewState
{
    public Vector2 MousePos { get; set; } = Vector2.Zero;
    public int LeftClickCount { get; set; } = 0;
    public int RightClickCount { get; set; } = 0;
    public bool LeftClicked { get; set; } = false;
    public bool RightClicked { get; set; } = false;

    public Dictionary<int, (int, int, int, int)> KeyCharRenderedRects { get; set; } =
        new Dictionary<int, (int, int, int, int)>();
}