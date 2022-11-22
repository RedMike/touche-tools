using ToucheTools.Models;

namespace ToucheTools.App.Services;

public class RoomImageRenderer
{
    private readonly Dictionary<string, byte[]> _cache = new Dictionary<string, byte[]>();
    
    public (string, byte[]) RenderRoomImage(int roomImageId, RoomImageDataModel roomImage, PaletteDataModel palette, 
        List<(int, SpriteImageDataModel, int, int)> roomSprites, int offsetX = 0, int offsetY = 0, 
        int? width = null, int? height = null, bool transparency = true)
    {
        width ??= roomImage.Width;
        height ??= roomImage.Height;
        var id = GetId(roomImageId, palette, roomSprites.Select(x => (x.Item1, x.Item3, x.Item4)).ToList(), 
            offsetX, offsetY, width.Value, height.Value, transparency);
        if (_cache.ContainsKey(id))
        {
            return (id, _cache[id]);
        }
        
        //active room sprites show up as the source of the background being changed entirely
        var processedBytes = new byte[roomImage.Height, roomImage.Width];
        for (var i = 0; i < roomImage.Height; i++)
        {
            for (var j = 0; j < roomImage.Width; j++)
            {
                processedBytes[i, j] = roomImage.RawData[i, j];
            }
        }
        var processedRoomImage = new RoomImageDataModel()
        {
            Width = roomImage.Width,
            Height = roomImage.Height,
            RawData = processedBytes,
            RoomWidth = roomImage.RoomWidth
        };
        foreach (var (_, sprite, bgDestX, bgDestY) in roomSprites)
        {
            for (var i = 0; i < sprite.Height; i++)
            {
                for (var j = 0; j < sprite.Width; j++)
                {
                    var destX = bgDestX + j;
                    var destY = bgDestY + i;
                    if (destX < roomImage.Width && destY < roomImage.Height)
                    {
                        processedBytes[destY, destX] = sprite.RawData[i, j];
                    }
                }
            }
        }

        var bytes = RenderPartialRoomImage(processedRoomImage.Width, processedRoomImage.Height, palette.Colors, processedRoomImage.RawData,
            offsetX, offsetY, width.Value, height.Value, transparency);
        _cache[id] = bytes;
        return (id, bytes);
    }

    private static string GetId(int roomImageId, PaletteDataModel palette, List<(int, int, int)> roomSprites, int offsetX, int offsetY, int width, int height, bool transparency)
    {
        var paletteId = string.Join("--", palette.Colors.Select(c => $"{c.R}-{c.G}-{c.B}"));
        var roomSpriteId = string.Join("--", roomSprites.Select(x => $"{x.Item1}-{x.Item2}-{x.Item3}"));
        return $"{roomImageId}_{paletteId}_{roomSpriteId}_{offsetX}_{offsetY}_{width}_{height}_{(transparency ? "t" : "n")}";
    }
    
    private byte[] RenderPartialRoomImage(int imageWidth, int imageHeight, List<PaletteDataModel.Rgb> colours, 
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