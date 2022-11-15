using ToucheTools.Models;

namespace ToucheTools.App.Services;

public class SpriteSheetRenderer
{
    private readonly Dictionary<string, byte[]> _cache = new Dictionary<string, byte[]>();

    public (string, byte[]) RenderSpriteSheet(int spriteId, SpriteImageDataModel spriteImage, int paletteId,
        PaletteDataModel palette, int offsetX = 0,
        int offsetY = 0, int? width = null, int? height = null)
    {
        width ??= spriteImage.Width;
        height ??= spriteImage.Height;
        var id = GetId(spriteId, paletteId, offsetX, offsetY, width.Value, height.Value);
        if (_cache.ContainsKey(id))
        {
            return (id, _cache[id]);
        }

        var bytes = RenderSpriteImage(spriteImage.Width, spriteImage.Height, palette.Colors, spriteImage.DecodedData,
            offsetX, offsetY, width.Value, height.Value);
        _cache[id] = bytes;
        return (id, bytes);
    }
    
    private static string GetId(int spriteId, int paletteId, int offsetX, int offsetY, int width, int height)
    {
        return $"{spriteId}_{paletteId}_{offsetX}_{offsetY}_{width}_{height}";
    }

    private byte[] RenderSpriteImage(int imageWidth, int imageHeight, List<PaletteDataModel.Rgb> colours, 
        byte[,] rawData, int fromX, int fromY, int width, int height)
    {
        var bytes = new byte[width * height * 4];
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var realX = fromX + i;
                if (realX >= imageWidth)
                {
                    continue;
                }
                var realY = fromY + j;
                if (realY >= imageHeight)
                {
                    continue;
                }
                
                var rawCol = rawData[realY, realX];
                if (rawCol == 0 || rawCol == 64)
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