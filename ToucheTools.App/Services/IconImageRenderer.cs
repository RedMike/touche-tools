using ToucheTools.Models;

namespace ToucheTools.App.Services;

public class IconImageRenderer
{
    private readonly Dictionary<string, byte[]> _cache = new Dictionary<string, byte[]>();
    
    public (string, byte[]) RenderIconImage(int iconImageId, IconImageDataModel iconImage, PaletteDataModel palette, 
        int offsetX = 0, int offsetY = 0, int? width = null, int? height = null, bool transparency = true)
    {
        width ??= iconImage.Width;
        height ??= iconImage.Height;
        var id = GetId(iconImageId, palette, 
            offsetX, offsetY, width.Value, height.Value, transparency);
        if (_cache.ContainsKey(id))
        {
            return (id, _cache[id]);
        }
        
        var bytes = RenderPartialIconImage(width.Value, height.Value, palette.Colors, iconImage.DecodedData,
            offsetX, offsetY, width.Value, height.Value, transparency);
        _cache[id] = bytes;
        return (id, bytes);
    }

    private static string GetId(int iconImageId, PaletteDataModel palette, int offsetX, int offsetY, int width, int height, bool transparency)
    {
        var paletteId = string.Join("--", palette.Colors.Select(c => $"{c.R}-{c.G}-{c.B}"));
        return $"{iconImageId}_{paletteId}_{offsetX}_{offsetY}_{width}_{height}_{(transparency ? "t" : "n")}";
    }
    
    private byte[] RenderPartialIconImage(int imageWidth, int imageHeight, List<PaletteDataModel.Rgb> colours, 
        byte[,] rawData, int fromX, int fromY, int width, int height, bool transparency = true)
    {
        var bytes = new byte[width * height * 4];
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var realX = fromX + i;
                if (realX < 0)
                {
                    continue;
                }
                if (realX >= imageWidth)
                {
                    continue;
                }
                var realY = fromY + j;
                if (realY < 0)
                {
                    continue;
                }
                if (realY >= imageHeight)
                {
                    continue;
                }
                
                var rawCol = rawData[realY, realX];
                if (transparency && (rawCol == 0 || rawCol == 64))
                {
                    //transparent
                    bytes[(j * width + i) * 4 + 0] = 0;
                    bytes[(j * width + i) * 4 + 1] = 0;
                    bytes[(j * width + i) * 4 + 2] = 0;
                    bytes[(j * width + i) * 4 + 3] = 0;
                }
                else
                {
                    //actual colour
                    var col = colours[rawCol];
                    bytes[(j * width + i) * 4 + 0] = col.R;
                    bytes[(j * width + i) * 4 + 1] = col.G;
                    bytes[(j * width + i) * 4 + 2] = col.B;
                    bytes[(j * width + i) * 4 + 3] = 255;
                }
            }
        }

        return bytes;
    }
}