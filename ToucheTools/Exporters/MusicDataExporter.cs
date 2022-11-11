using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Models;

namespace ToucheTools.Exporters;

public class MusicDataExporter
{
    private readonly ILogger _logger = Logging.Factory.CreateLogger(typeof(MusicDataExporter));
    private readonly Stream _stream;
    private readonly BinaryWriter _writer;
    
    public MusicDataExporter(Stream stream)
    {
        _stream = stream;
        _writer = new BinaryWriter(_stream, Encoding.ASCII, true);
    }

    public void Export(MusicDataModel music)
    {
        _stream.Seek(0, SeekOrigin.Begin);
        _writer.Write(music.RawData);
    }
}