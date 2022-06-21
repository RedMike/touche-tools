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

    public class Walk
    {
        public int Point1 { get; set; }
        public int Point2 { get; set; }
        public int ClipRect { get; set; }
        public int Area1 { get; set; }
        public int Area2 { get; set; }
    }

    public List<Rect> Rects { get; set; } = new List<Rect>();
    public List<Point> Points { get; set; } = new List<Point>();
    public List<Walk> Walks { get; set; } = new List<Walk>();
}