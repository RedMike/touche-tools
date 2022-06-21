using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using ToucheTools.Models;

namespace ToucheTools.Console;

[SuppressMessage("Interoperability", "CA1416:Validate platform compatibility")]
public static class DebugPaletteSaver
{
    private static readonly ILogger Logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(DebugPaletteSaver));
    
    public static void Save(string filename, PaletteDataModel palette)
    {
        var w = 1000;
        var h = 1000;
        var rows = 10;
        var numColors = palette.Colors.Count;

        if (numColors % 2 == 1)
        {
            numColors--; //TODO
        }

        Logger.Log(LogLevel.Information, "Palette num colors {}", numColors);
        var tilesH = w / (numColors / rows);
        var tilesV = h / rows;
        Logger.Log(LogLevel.Information, "Palette tiles {}x{}", tilesH, tilesV);
        
        
        var bitmap = new Bitmap(w, h, PixelFormat.Format24bppRgb);
        var bitmapData = bitmap.LockBits(Rectangle.FromLTRB(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
        IntPtr ptr = bitmapData.Scan0;
        byte[] rgbValues = new byte[Math.Abs(bitmapData.Stride) * bitmap.Height];

        var cols = palette.Colors;
        for (var i = 0; i < h; i++)
        {
            for (var j = 0; j < w; j++)
            {
                var colH = j / tilesH;
                var colV = i / tilesV;
                var colIdx = colV * (numColors / rows) + colH;
                var col = cols[colIdx];
                var idx = (i*w + j) * 3;
                rgbValues[idx] = col.R;
                rgbValues[idx + 1] = col.G;
                rgbValues[idx + 2] = col.B;
            }
        }
        Marshal.Copy(rgbValues, 0, ptr, Math.Abs(bitmapData.Stride) * bitmap.Height);
        bitmap.UnlockBits(bitmapData);

        bitmap.Save($"{filename}.png", ImageFormat.Png);
        Logger.LogInformation("Saved debug palette: {}.png", filename);
    }
}