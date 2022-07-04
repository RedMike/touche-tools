using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using ToucheTools.Models;

namespace ToucheTools.Web.Services;

public interface IImageRenderingService
{
    byte[] RenderImage(SpriteImageDataModel sprite, List<PaletteDataModel.Rgb> palette, bool raw=false);
    byte[] RenderImage(IconImageDataModel icon, List<PaletteDataModel.Rgb> palette, bool raw=false);
    byte[] RenderImage(RoomImageDataModel room, List<PaletteDataModel.Rgb> palette, bool raw=false);
    byte[] RenderAnimationImage(int direction, SequenceDataModel.FrameInformation frame, SpriteImageDataModel sprite, List<PaletteDataModel.Rgb> palette);
    byte[] RenderAnimation(int directionId, List<SequenceDataModel.FrameInformation> dirFrames, SpriteImageDataModel spriteImage, List<PaletteDataModel.Rgb> paletteList);
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

        var image = RenderImageInternal(sprite.Width, sprite.Height, sprite.SpriteWidth, sprite.SpriteHeight, palette, raw, data);
        
        using var mem = new MemoryStream();
        image.SaveAsPng(mem);
        return mem.ToArray();
    }

    public byte[] RenderImage(IconImageDataModel icon, List<PaletteDataModel.Rgb> palette, bool raw = false)
    {
        var image = RenderImageInternal(icon.Width, icon.Height, icon.Width, icon.Height, palette, raw, icon.DecodedData);
        
        using var mem = new MemoryStream();
        image.SaveAsPng(mem);
        return mem.ToArray();
    }

    public byte[] RenderImage(RoomImageDataModel room, List<PaletteDataModel.Rgb> palette, bool raw = false)
    {
        var image = RenderImageInternal(room.Width, room.Height, room.Width, room.Height, palette, raw, room.RawData);
        
        using var mem = new MemoryStream();
        image.SaveAsPng(mem);
        return mem.ToArray();
    }

    private Image RenderImageInternal(int width, int height, int spriteWidth, int spriteHeight, List<PaletteDataModel.Rgb> palette, bool raw, byte[,] data,
        byte r = 255, byte g = 0, byte b = 255, byte a = 255)
    {
        var image = new Image<Rgba32>(width, height, new Rgba32(r, g, b, a));
        
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
                    var rgb = new Rgba32(p.R, p.G, p.B, 255);
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

                    if (col != 0)
                    {
                        row[j] = rgb;
                    }
                }
            }
        });

        var i = 0;
        _logger.LogInformation("Palette: {}", palette.Select(c => $"{i++} ({c.R}, {c.G}, {c.B}), "));
        _logger.LogInformation("Seen colors: {}", seenColors);
        _logger.LogInformation("Seen colors broken down: {}",
            seenColors.Select(c => $"({palette[c].R}, {palette[c].G}, {palette[c].B})"));
        return image;
    }

    public byte[] RenderAnimationImage(int direction, SequenceDataModel.FrameInformation frame, SpriteImageDataModel sprite, List<PaletteDataModel.Rgb> palette)
    {
        var w = 0;
        if (frame.Parts.Any(p => p.DestX < 0))
        {
            w += Math.Abs(frame.Parts.Select(p => p.DestX).Min());
        }
        if (frame.Parts.Any(p => p.DestX > 0))
        {
            w += Math.Abs(frame.Parts.Select(p => p.DestX).Max());
        }
        w += sprite.SpriteWidth;
        
        var h = 0;
        if (frame.Parts.Any(p => p.DestY < 0))
        {
            h += Math.Abs(frame.Parts.Select(p => p.DestY).Min());
        }
        if (frame.Parts.Any(p => p.DestY > 0))
        {
            h += Math.Abs(frame.Parts.Select(p => p.DestY).Max());
        }
        h += sprite.SpriteHeight;
        
        var ox = w/2;
        var oy = h/2+sprite.SpriteHeight;
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
                    var dy = srcY + y;
                    var dx = srcX + x;
                    if (part.HFlipped)
                    {
                        dx = srcX + (sprite.SpriteWidth - x);
                    }
                    if (part.VFlipped)
                    {
                        dy = srcY + (sprite.SpriteHeight - y);
                    }

                    if (dy >= sprite.Height)
                    {
                        dy = sprite.Height - 1;
                    }
                    if (dy < 0)
                    {
                        dy = 0;
                    }
                    
                    if (dx >= sprite.Width)
                    {
                        dx = sprite.Width - 1;
                    }
                    if (dx < 0)
                    {
                        dx = 0;
                    }
                    
                    var rawPixel = sprite.RawData[dy, dx];
                    var pixel = sprite.DecodedData[dy, dx];
                    if (rawPixel != 0 && rawPixel != 64 && rawPixel != 255)
                    {
                        try
                        {
                            var qy = oy + y + part.DestY;
                            var qx = ox + x + part.DestX;
                            if (direction == 3)
                            {
                                qx -= sprite.SpriteWidth;
                            }
                            rawData[qy, qx] = pixel;
                        }
                        catch (Exception)
                        {
                            //
                        }
                    }
                }
            }
        }

        var image = RenderImageInternal(w, h, w, h, palette, false, rawData);
        
        using var mem = new MemoryStream();
        image.SaveAsPng(mem);
        return mem.ToArray();
    }

    public byte[] RenderAnimation(int direction, List<SequenceDataModel.FrameInformation> frames,
        SpriteImageDataModel sprite, List<PaletteDataModel.Rgb> palette)
    {
        const int frameDelay = 100; //hundredths of a second
        //figure out image size
        var maxW = 0;
        var maxH = 0;
        foreach (var frame in frames)
        {
            var w = 0;
            var h = 0;
            if (frame.Parts.Any(p => p.DestX < 0))
            {
                w += Math.Abs(frame.Parts.Select(p => p.DestX).Min());
            }

            if (frame.Parts.Any(p => p.DestX > 0))
            {
                w += Math.Abs(frame.Parts.Select(p => p.DestX).Max());
            }

            w += sprite.SpriteWidth;

            if (frame.Parts.Any(p => p.DestY < 0))
            {
                h += Math.Abs(frame.Parts.Select(p => p.DestY).Min());
            }

            if (frame.Parts.Any(p => p.DestY > 0))
            {
                h += Math.Abs(frame.Parts.Select(p => p.DestY).Max());
            }

            h += sprite.SpriteHeight;

            if (maxW < w)
            {
                maxW = w;
            }

            if (maxH < h)
            {
                maxH = h;
            }
        }

        var fullImage = new Image<Rgba32>(maxW, maxH);
        var firstFrame = true;
        
        var ox = maxW/2;
        var oy = maxH/2+sprite.SpriteHeight;
        var framesPerLine = sprite.Width / sprite.SpriteWidth;

        foreach (var frame in frames)
        {
            var rawData = new byte[maxH, maxW];
            foreach (var part in frame.Parts)
            {
                var srcX = sprite.SpriteWidth * (part.FrameIndex % framesPerLine);
                var srcY = sprite.SpriteHeight * (part.FrameIndex / framesPerLine);
                for (var x = 0; x < sprite.SpriteWidth; x++)
                {
                    for (var y = 0; y < sprite.SpriteHeight; y++)
                    {
                        var dy = srcY + y;
                        var dx = srcX + x;
                        if (part.HFlipped)
                        {
                            dx = srcX + (sprite.SpriteWidth - x);
                        }

                        if (part.VFlipped)
                        {
                            dy = srcY + (sprite.SpriteHeight - y);
                        }

                        if (dy >= sprite.Height)
                        {
                            dy = sprite.Height - 1;
                        }

                        if (dy < 0)
                        {
                            dy = 0;
                        }

                        if (dx >= sprite.Width)
                        {
                            dx = sprite.Width - 1;
                        }

                        if (dx < 0)
                        {
                            dx = 0;
                        }

                        var rawPixel = sprite.RawData[dy, dx];
                        var pixel = sprite.DecodedData[dy, dx];
                        if (rawPixel != 0 && rawPixel != 64 && rawPixel != 255)
                        {
                            try
                            {
                                var qy = oy + y + part.DestY;
                                var qx = ox + x + part.DestX;
                                if (direction == 3)
                                {
                                    qx -= sprite.SpriteWidth;
                                }

                                rawData[qy, qx] = pixel;
                            }
                            catch (Exception)
                            {
                                //
                            }
                        }
                    }
                }
            }

            using var image = RenderImageInternal(maxW, maxH, maxW, maxH, palette, false, rawData, 0, 0, 0, 0);
            image.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = frame.Delay == 0 ? 10 : frame.Delay*10;
            image.Frames.RootFrame.Metadata.GetGifMetadata().DisposalMethod = GifDisposalMethod.RestoreToBackground;
            
            fullImage.Frames.AddFrame(image.Frames.RootFrame);
            if (firstFrame)
            {
                fullImage.Frames.RemoveFrame(0);
                firstFrame = false;
            }
        }

        fullImage.Metadata.GetGifMetadata().RepeatCount = 0;
        fullImage.Metadata.GetGifMetadata().ColorTableMode = GifColorTableMode.Local;
        using var mem = new MemoryStream();
        fullImage.SaveAsGif(mem);
        return mem.ToArray();
    }
}