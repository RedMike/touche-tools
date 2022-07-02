using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ToucheTools.Models;

namespace ToucheTools.Web.Services;

public interface IImageRenderingService
{
    byte[] RenderImage(int w, int h, int sw, int sh, byte[,] indexColorData, List<PaletteDataModel.Rgb> palette, bool raw=false);
}

public class MemoryImageRenderingService : IImageRenderingService
{
    private readonly ILogger _logger;

    public MemoryImageRenderingService(ILogger<MemoryImageRenderingService> logger)
    {
        _logger = logger;
    }

    public byte[] RenderImage(int w, int h, int sw, int sh, byte[,] indexColorData, List<PaletteDataModel.Rgb> palette, bool raw=false)
    {
        var image = new Image<Rgb24>(w, h);
        HashSet<byte> seenColors = new HashSet<byte>();
        var savedWidth = false;
        var savedHeight = false;
        image.ProcessPixelRows(pixelAccessor =>
        {
            for (var i = 0; i < h; i++)
            {
                var row = pixelAccessor.GetRowSpan(i);
                for (var j = 0; j < w; j++)
                {
                    var col = indexColorData[i, j];
                    if (!savedWidth && i == 0 && j < sw && col == 64)
                    {
                        throw new Exception("64 on top row before separator");
                    }
                    if (!savedWidth && i == 0 && j == sw)
                    {
                        col = 64;
                        savedWidth = true;
                    }
                    
                    if (!savedHeight && j == 0 && i < sh && col == 64)
                    {
                        throw new Exception("64 on top row before separator");
                    }
                    if (!savedHeight && j == 0 && i == sh)
                    {
                        col = 64;
                        savedHeight = true;
                    }
                    
                    seenColors.Add(col);
                    var p = palette[col];
                    var rgb = new Rgb24(p.R, p.G, p.B);
                    if (p.R == 0 && p.G == 0 && p.B == 0 && col != 64 && col != 255)
                    {
                        //it's a non-edge black, so just bump it up by a tiny bit to maintain the information
                        rgb.R += 1;
                    }

                    if (col == 64 || col == 255)
                    {
                        rgb.R = 0;
                        rgb.G = 0;
                        rgb.B = 0;
                    }

                    if (raw)
                    {
                        rgb.R = col;
                        rgb.G = col;
                        rgb.B = col;
                    }
                    row[j] = rgb;
                }
            }
        });

        var i = 0;
        _logger.LogInformation("Palette: {}", palette.Select(c => $"{i++} ({c.R}, {c.G}, {c.B}), "));
        _logger.LogInformation("Seen colors: {}", seenColors);
        _logger.LogInformation("Seen colors broken down: {}", seenColors.Select(c => $"({palette[c].R}, {palette[c].G}, {palette[c].B})"));
        
        using var mem = new MemoryStream();
        image.SaveAsPng(mem);
        return mem.ToArray();
    }
}