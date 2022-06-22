using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using ToucheTools.Models;

namespace ToucheTools.Web.Services;

public interface IImageRenderingService
{
    byte[] RenderImage(int w, int h, byte[,] indexColorData, List<PaletteDataModel.Rgb> palette);
}

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public class MemoryImageRenderingService : IImageRenderingService
{
    private readonly ILogger _logger;

    public MemoryImageRenderingService(ILogger<MemoryImageRenderingService> logger)
    {
        _logger = logger;
    }

    public byte[] RenderImage(int w, int h, byte[,] indexColorData, List<PaletteDataModel.Rgb> palette)
    {
        var bitmap = new Bitmap(w, h, PixelFormat.Format24bppRgb);
        var bitmapData = bitmap.LockBits(Rectangle.FromLTRB(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
        IntPtr ptr = bitmapData.Scan0;
        byte[] rgbValues = new byte[Math.Abs(bitmapData.Stride) * bitmap.Height];

        HashSet<byte> seenColors = new HashSet<byte>();
        for (var i = 0; i < h; i++)
        {
            for (var j = 0; j < w; j++)
            {
                var col = indexColorData[i, j];
                seenColors.Add(col);
                var idx = (i*w + j) * 3;
                rgbValues[idx + 2] = palette[col].R;
                rgbValues[idx + 1] = palette[col].G;
                rgbValues[idx + 0] = palette[col].B;
            }
        }
        _logger.LogInformation("Seen colors: {}", seenColors);
        Marshal.Copy(rgbValues, 0, ptr, Math.Abs(bitmapData.Stride) * bitmap.Height);
        bitmap.UnlockBits(bitmapData);

        using var mem = new MemoryStream();
        bitmap.Save(mem, ImageFormat.Png);
        return mem.GetBuffer();
    }
}