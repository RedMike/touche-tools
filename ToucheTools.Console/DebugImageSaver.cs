using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using ToucheTools.Models;

namespace ToucheTools.Console;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public static class DebugImageSaver
{
    private static readonly ILogger Logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(DebugImageSaver));
    
    public static void Save(int w, int h, byte[,] bytes, string filename, PaletteDataModel palette)
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
                var col = bytes[i, j];
                seenColors.Add(col);
                var idx = (i*w + j) * 3;
                rgbValues[idx] = palette.Colors[col].R;
                rgbValues[idx + 1] = palette.Colors[col].G;
                rgbValues[idx + 2] = palette.Colors[col].B;
            }
        }
        Logger.LogInformation("Seen colors: {}", seenColors);
        Marshal.Copy(rgbValues, 0, ptr, Math.Abs(bitmapData.Stride) * bitmap.Height);
        bitmap.UnlockBits(bitmapData);

        bitmap.Save($"{filename}.png", ImageFormat.Png);
        Logger.LogInformation("Saved debug image: {}.png", filename);
    }
}