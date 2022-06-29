using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ToucheTools.Models;

namespace ToucheTools.Web.Services;

public interface IImageRenderingService
{
    byte[] RenderImage(int w, int h, byte[,] indexColorData, List<PaletteDataModel.Rgb> palette);
}

public class MemoryImageRenderingService : IImageRenderingService
{
    private readonly ILogger _logger;

    public MemoryImageRenderingService(ILogger<MemoryImageRenderingService> logger)
    {
        _logger = logger;
    }

    public byte[] RenderImage(int w, int h, byte[,] indexColorData, List<PaletteDataModel.Rgb> palette)
    {
        var image = new Image<Rgb24>(w, h);
        HashSet<byte> seenColors = new HashSet<byte>();
        image.ProcessPixelRows(pixelAccessor =>
        {
            for (var i = 0; i < h; i++)
            {
                var row = pixelAccessor.GetRowSpan(i);
                for (var j = 0; j < w; j++)
                {
                    var col = indexColorData[i, j];
                    seenColors.Add(col);
                    var rgb = new Rgb24(palette[col].R, palette[col].G, palette[col].B);
                    row[j] = rgb;
                }
            }
        });

        _logger.LogInformation("Palette: {}", palette.Select(c => $"({c.R}, {c.G}, {c.B})"));
        _logger.LogInformation("Seen colors: {}", seenColors);
        _logger.LogInformation("Seen colors broken down: {}", seenColors.Select(c => $"({palette[c].R}, {palette[c].G}, {palette[c].B})"));
        
        using var mem = new MemoryStream();
        image.SaveAsPng(mem);
        return mem.ToArray();
    }
}