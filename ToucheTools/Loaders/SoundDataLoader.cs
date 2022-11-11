using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Models;

namespace ToucheTools.Loaders;

public class SoundDataLoader
{
    private readonly ILogger _logger = Logging.Factory.CreateLogger(typeof(SoundDataLoader));
    private readonly Stream _stream;
    private readonly BinaryReader _reader;
    private readonly ResourceDataLoader _resourceDataLoader;
    
    public SoundDataLoader(Stream stream, ResourceDataLoader resourceDataLoader)
    {
        _stream = stream;
        _reader = new BinaryReader(_stream, Encoding.ASCII, true);
        _resourceDataLoader = resourceDataLoader;
    }

    public void Read(int number, out SoundDataModel sound)
    {
        _resourceDataLoader.Read(Resource.Sound, number, true, out var offset, out var size);
        _stream.Seek(offset, SeekOrigin.Begin);

        sound = new SoundDataModel();
        
        var rawSound = _reader.ReadBytes(size);
        sound.RawData = rawSound;
    }
}