using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Models;

namespace ToucheTools.Exporters;

public class RoomImageDataExporter
{
    private readonly ILogger _logger = Logging.Factory.CreateLogger(typeof(RoomImageDataExporter));
    private readonly Stream _stream;
    private readonly BinaryWriter _writer;

    public RoomImageDataExporter(Stream stream)
    {
        _stream = stream;
        _writer = new BinaryWriter(_stream, Encoding.ASCII, true);
    }

    public void Export(RoomImageDataModel room)
    {
        _logger.LogInformation("Saving room image of size {}x{}", room.Width, room.Height);
        
        _stream.Seek(0, SeekOrigin.Begin);
        _writer.Write((ushort)room.Width);
        _writer.Write((ushort)room.Height);
        //RLE compression
        for (var i = 0; i < room.Height; i++)
        {
            var compressing = false;
            byte compressingColor = 0x00;
            byte compressingLength = 0;
            for (var j = 0; j < room.Width; j++)
            {
                byte color = room.RawData[i, j];
                if (i == 0 && j == room.RoomWidth)
                {
                    color = 255;
                }
                if (compressing && 
                    (
                        color != compressingColor || //we're done compressing
                        compressingLength > 62 || //engine can only do RLE up to 63 length
                        j > (room.Width - 2) //we're near the edge
                    )
                ) 
                {
                    //RLE run over, save compressed run
                    _writer.Write((byte)(compressingLength | 0xC0));
                    _writer.Write(compressingColor);
                    compressing = false;
                }
                if (!compressing)
                {
                    if (
                        j >= (room.Width - 2) || //we're near the edge
                        room.RawData[i, j + 1] != color) //or there's a single colour so no point
                    {
                        if (color >= 0xC0)
                        {
                            //force compression at least of length 1
                            _writer.Write((byte)0xC1);
                        }
                        _writer.Write(color);
                    }
                    else
                    {
                        compressing = true;
                        compressingColor = color;
                        compressingLength = 1;
                    }
                }
                else
                {
                    if (color == compressingColor)
                    {
                        compressingLength++;
                    }
                }
            }
        }
    }
}