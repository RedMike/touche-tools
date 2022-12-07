namespace ToucheTools.Constants;

public static class Directions
{
    public const int Right = 0;
    public const int Down = 1;
    public const int Up = 2;
    public const int Left = 3;

    public static string DirectionName(int dir)
    {
        if (dir == Right)
        {
            return "Right";
        }

        if (dir == Down)
        {
            return "Down";
        }

        if (dir == Up)
        {
            return "Up";
        }

        if (dir == Left)
        {
            return "Left";
        }

        throw new Exception($"Unknown direction: {dir}");
    }
}