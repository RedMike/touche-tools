using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace ToucheTools.Console;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public static class DebugImageSaver
{
    private static readonly ILogger Logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(DebugImageSaver));
    
    public static void Save(int w, int h, byte[,] bytes, string filename)
    {
        var bitmap = new Bitmap(w, h, PixelFormat.Format24bppRgb);
        var bitmapData = bitmap.LockBits(Rectangle.FromLTRB(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
        IntPtr ptr = bitmapData.Scan0;
        byte[] rgbValues = new byte[Math.Abs(bitmapData.Stride) * bitmap.Height];

        for (var i = 0; i < h; i++)
        {
            for (var j = 0; j < w; j++)
            {
                var col = bytes[i, j];
                var idx = (i*w + j) * 3;
                rgbValues[idx] = col;
                rgbValues[idx + 1] = col;
                rgbValues[idx + 2] = col;
            }
        }
        Marshal.Copy(rgbValues, 0, ptr, Math.Abs(bitmapData.Stride) * bitmap.Height);
        bitmap.UnlockBits(bitmapData);

        bitmap.Save($"{filename}.png", ImageFormat.Png);
        Logger.LogInformation("Saved debug image: {}.png", filename);
    }
}