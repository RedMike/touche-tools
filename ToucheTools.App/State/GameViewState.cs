using System.Numerics;

namespace ToucheTools.App.State;

public class GameViewState
{
    public Vector2 ScreenMousePos { get; set; } = Vector2.Zero;
    public Vector2 MousePos { get; set; } = Vector2.Zero;
    public bool LeftClicked { get; set; } = false;
    public bool RightClicked { get; set; } = false;

    public Dictionary<int, (int, int, int, int)> KeyCharRenderedRects { get; set; } =
        new Dictionary<int, (int, int, int, int)>();
}