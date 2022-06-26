using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Exceptions;
using ToucheTools.Models;

namespace ToucheTools.Loaders;

public class MusicDataLoader
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(MusicDataLoader));
    private readonly Stream _stream;
    private readonly BinaryReader _reader;
    private readonly ResourceDataLoader _resourceDataLoader;
    
    public MusicDataLoader(Stream stream, ResourceDataLoader resourceDataLoader)
    {
        _stream = stream;
        _reader = new BinaryReader(_stream, Encoding.ASCII, true);
        _resourceDataLoader = resourceDataLoader;
    }

    public void Read(int number, out MusicDataModel music)
    {
        _resourceDataLoader.Read(Resource.Music, number, true, out var offset, out var size);
        _stream.Seek(offset, SeekOrigin.Begin);

        music = new MusicDataModel();

        if (size <= 0)
        {
            throw new UnknownResourceException();
        }
        var rawMusic = _reader.ReadBytes(size);
        music.RawData = rawMusic;
    }
}