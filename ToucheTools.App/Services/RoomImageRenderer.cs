using ToucheTools.Models;

namespace ToucheTools.App.Services;

public class RoomImageRenderer
{
    public byte[] RenderRoomImage(int width, int height, List<PaletteDataModel.Rgb> colours, byte[,] rawData)
    {
        var bytes = new byte[width * height * 4];
        for (var i = 0; i < width; i++)
        {
            for (var j = 0; j < height; j++)
            {
                var rawCol = rawData[j, i];
                var col = colours[rawCol];
                bytes[(j * width + i) * 4 + 0] = col.R;
                bytes[(j * width + i) * 4 + 1] = col.G;
                bytes[(j * width + i) * 4 + 2] = col.B;
                bytes[(j * width + i) * 4 + 3] = 255;
            }
        }

        return bytes;
    }
    
    public byte[] RenderPartialRoomImage(int imageWidth, int imageHeight, List<PaletteDataModel.Rgb> colours, 
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