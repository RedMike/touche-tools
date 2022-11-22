using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Models;

namespace ToucheTools.Exporters;

public class SoundDataExporter
{
    private readonly ILogger _logger = Logging.Factory.CreateLogger(typeof(SoundDataExporter));
    private readonly Stream _stream;
    private readonly BinaryWriter _writer;
    
    public SoundDataExporter(Stream stream)
    {
        _stream = stream;
        _writer = new BinaryWriter(_stream, Encoding.ASCII, true);
    }

    public void Export(SoundDataModel sound)
    {
        _stream.Seek(0, SeekOrigin.Begin);
        _writer.Write(sound.RawData);
    }
}