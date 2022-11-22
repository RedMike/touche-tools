using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Models;

namespace ToucheTools.Loaders;

public class RoomImageDataLoader
{
    private readonly ILogger _logger = Logging.Factory.CreateLogger(typeof(RoomImageDataLoader));
    private readonly Stream _stream;
    private readonly BinaryReader _reader;
    private readonly ResourceDataLoader _resourceDataLoader;
    
    public RoomImageDataLoader(Stream stream, ResourceDataLoader resourceDataLoader)
    {
        _stream = stream;
        _reader = new BinaryReader(_stream, Encoding.ASCII, true);
        _resourceDataLoader = resourceDataLoader;
    }

    public void Read(int number, out RoomImageDataModel roomImage)
    {
        _resourceDataLoader.Read(Resource.RoomImage, number, false, out var offset, out _);
        _stream.Seek(offset, SeekOrigin.Begin);
        ushort rawWidth = _reader.ReadUInt16();
        ushort rawHeight = _reader.ReadUInt16();
        var initialWidth = (int)rawWidth;
        var initialHeight = (int)rawHeight;
        _logger.Log(LogLevel.Debug, "Room image {}: initially {}x{}", number, initialWidth, initialHeight);

        var imageData = new byte[initialHeight, initialWidth];
        //RLE compression
        for (var i = 0; i < initialHeight; i++)
        {
            for (var j = 0; j < initialWidth; j++)
            {
                byte code = _reader.ReadByte();
                if ((code & 0xC0) == 0xC0)
                {
                    var len = code & 0x3F;
                    byte color = _reader.ReadByte();
                    
                    //it's RLE, the next {len} pixels are the same colour
                    if ((j + len) > initialWidth)
                    {
                        _logger.LogError("RLE encoding overflowing to next line: {}x{} RLE length {} width {} by {}", j, i, len, initialWidth, j + len - initialWidth);
                        throw new Exception("RLE encoding overflowed to next line");
                    }
                    
                    for (var q = 0; q < len; q++)
                    {
                        imageData[i, j+q] = color;
                    }
                    j = j + len - 1;
                }
                else
                {
                    imageData[i, j] = code;
                }
            }
        }
        
        //find actual room width
        var roomWidth = initialWidth;
        for (var i = 0; i < initialWidth; i++)
        {
            if (imageData[0, i] == 255)
            {
                roomWidth = i;
                imageData[0, i] = 0;
                break;
            }
        }

        if (roomWidth != initialWidth)
        {
            _logger.Log(LogLevel.Information, "Room image {}: room {}x{}", number, roomWidth, initialHeight);
        }

        roomImage = new RoomImageDataModel()
        {
            Width = initialWidth,
            Height = initialHeight,
            RawData = imageData,
            RoomWidth = roomWidth
        };
    }
}