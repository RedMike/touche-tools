using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ToucheTools.Models;

namespace ToucheTools.Web.Services;

public interface IImageRenderingService
{
    byte[] RenderImage(SpriteImageDataModel sprite, List<PaletteDataModel.Rgb> palette, bool raw=false);
    byte[] RenderImage(IconImageDataModel icon, List<PaletteDataModel.Rgb> palette, bool raw=false);
    byte[] RenderImage(RoomImageDataModel room, List<PaletteDataModel.Rgb> palette, bool raw=false);
    byte[] RenderAnimationImage(SequenceDataModel.FrameInformation frame, SpriteImageDataModel sprite, List<PaletteDataModel.Rgb> palette);
}

public class MemoryImageRenderingService : IImageRenderingService
{
    private readonly ILogger _logger;

    public MemoryImageRenderingService(ILogger<MemoryImageRenderingService> logger)
    {
        _logger = logger;
    }

    public byte[] RenderImage(SpriteImageDataModel sprite, List<PaletteDataModel.Rgb> palette, bool raw=false)
    {
        var data = raw ? sprite.RawData : sprite.DecodedData;

        return RenderImageInternal(sprite.Width, sprite.Height, sprite.SpriteWidth, sprite.SpriteHeight, palette, raw, data);
    }

    public byte[] RenderImage(IconImageDataModel icon, List<PaletteDataModel.Rgb> palette, bool raw = false)
    {
        return RenderImageInternal(icon.Width, icon.Height, icon.Width, icon.Height, palette, raw, icon.DecodedData);
    }

    public byte[] RenderImage(RoomImageDataModel room, List<PaletteDataModel.Rgb> palette, bool raw = false)
    {
        return RenderImageInternal(room.Width, room.Height, room.Width, room.Height, palette, raw, room.RawData);
    }

    private byte[] RenderImageInternal(int width, int height, int spriteWidth, int spriteHeight, List<PaletteDataModel.Rgb> palette, bool raw, byte[,] data)
    {
        var image = new Image<Rgb24>(width, height);
        HashSet<byte> seenColors = new HashSet<byte>();
        var savedWidth = false;
        var savedHeight = false;
        image.ProcessPixelRows(pixelAccessor =>
        {
            for (var i = 0; i < height; i++)
            {
                var row = pixelAccessor.GetRowSpan(i);
                for (var j = 0; j < width; j++)
                {
                    var col = data[i, j];
                    if (!savedWidth && i == 0 && j < spriteWidth && col == 64)
                    {
                        throw new Exception("64 on top row before separator");
                    }

                    if (!savedWidth && i == 0 && j == spriteWidth)
                    {
                        col = 64;
                        savedWidth = true;
                    }

                    if (!savedHeight && j == 0 && i < spriteHeight && col == 64)
                    {
                        throw new Exception("64 on top row before separator");
                    }

                    if (!savedHeight && j == 0 && i == spriteHeight)
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
        _logger.LogInformation("Seen colors broken down: {}",
            seenColors.Select(c => $"({palette[c].R}, {palette[c].G}, {palette[c].B})"));

        using var mem = new MemoryStream();
        image.SaveAsPng(mem);
        return mem.ToArray();
    }

    public byte[] RenderAnimationImage(SequenceDataModel.FrameInformation frame, SpriteImageDataModel sprite, List<PaletteDataModel.Rgb> palette)
    {
        var w = sprite.SpriteWidth * 2;//TODO: calculate size correctly
        var h = sprite.SpriteHeight * 2;//TODO: calculate size correctly
        var rawData = new byte[h, w]; 
        var framesPerLine = sprite.Width / sprite.SpriteWidth;
        foreach (var part in frame.Parts)
        {
            var srcX = sprite.SpriteWidth * (part.FrameIndex % framesPerLine);
            var srcY = sprite.SpriteHeight * (part.FrameIndex / framesPerLine);
            for (var x = 0; x < sprite.SpriteWidth; x++)
            {
                for (var y = 0; y < sprite.SpriteHeight; y++)
                {
                    rawData[y, x] = sprite.DecodedData[srcY + y, srcX + x];
                }
            }
        }

        return RenderImageInternal(w, h, w, h, palette, false, rawData);
    }
}