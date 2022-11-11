using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Models;

namespace ToucheTools.Loaders;

public class RoomInfoDataLoader
{
    private readonly ILogger _logger = Logging.Factory.CreateLogger(typeof(RoomInfoDataLoader));
    private readonly Stream _stream;
    private readonly BinaryReader _reader;
    private readonly ResourceDataLoader _resourceDataLoader;
    
    public RoomInfoDataLoader(Stream stream, ResourceDataLoader resourceDataLoader)
    {
        _stream = stream;
        _reader = new BinaryReader(_stream, Encoding.ASCII, true);
        _resourceDataLoader = resourceDataLoader;
    }

    public void Read(int number, out PaletteDataModel palette, out RoomInfoDataModel roomInfo)
    {
        roomInfo = new RoomInfoDataModel();
        _resourceDataLoader.Read(Resource.RoomInfo, number, false, out var offset, out _);
        _stream.Seek(offset, SeekOrigin.Begin);
        
        _stream.Seek(2, SeekOrigin.Current);
        var roomImageNum = _reader.ReadUInt16();
        roomInfo.RoomImageNum = roomImageNum;
        _stream.Seek(2, SeekOrigin.Current);

        var paletteBytes = _reader.ReadBytes(3 * 256);
        palette = new PaletteDataModel();
        for (var i = 0; i < 256; i++)
        {
            palette.Colors.Add(new PaletteDataModel.Rgb()
            {
                R = paletteBytes[i * 3],
                G = paletteBytes[i * 3 + 1],
                B = paletteBytes[i * 3 + 2]
            });
        }
    }
}