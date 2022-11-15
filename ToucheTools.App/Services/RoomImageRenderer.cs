using ToucheTools.Models;

namespace ToucheTools.App.Services;

public class RoomImageRenderer
{
    private readonly Dictionary<string, byte[]> _cache = new Dictionary<string, byte[]>();
    
    public (string, byte[]) RenderRoomImage(int roomImageId, RoomImageDataModel roomImage, int paletteId, PaletteDataModel palette, int offsetX = 0,
        int offsetY = 0, int? width = null, int? height = null)
    {
        width ??= roomImage.Width;
        height ??= roomImage.Height;
        var id = GetId(roomImageId, paletteId, offsetX, offsetY, width.Value, height.Value);
        if (_cache.ContainsKey(id))
        {
            return (id, _cache[id]);
        }

        var bytes = RenderPartialRoomImage(roomImage.Width, roomImage.Height, palette.Colors, roomImage.RawData,
            offsetX, offsetY, width.Value, height.Value);
        _cache[id] = bytes;
        return (id, bytes);
    }

    private static string GetId(int roomImageId, int paletteId, int offsetX, int offsetY, int width, int height)
    {
        return $"{roomImageId}_{paletteId}_{offsetX}_{offsetY}_{width}_{height}";
    }
    
    private byte[] RenderPartialRoomImage(int imageWidth, int imageHeight, List<PaletteDataModel.Rgb> colours, 
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
                var col = colours[rawCol];
                bytes[(j * width + i) * 4 + 0] = col.R;
                bytes[(j * width + i) * 4 + 1] = col.G;
                bytes[(j * width + i) * 4 + 2] = col.B;
                bytes[(j * width + i) * 4 + 3] = 255;
            }
        }

        return bytes;
    }
}