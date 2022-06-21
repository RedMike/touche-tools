namespace ToucheTools.Models;

public class ProgramDataModel
{
    public class Rect
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
    }

    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int Order { get; set; }
    }

    public List<Rect> Rects { get; set; } = new List<Rect>();
    public List<Point> Points { get; set; } = new List<Point>();
}