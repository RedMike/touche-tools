namespace ToucheTools.Models;

public class PaletteDataModel
{
    public class Rgb
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
    }

    public List<Rgb> Colors { get; set; } = new List<Rgb>();
}