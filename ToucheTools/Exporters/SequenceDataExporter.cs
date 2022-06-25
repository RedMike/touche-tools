using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Models;

namespace ToucheTools.Exporters;

public class SequenceDataExporter
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(SequenceDataExporter));
    private readonly Stream _stream;
    private readonly BinaryWriter _writer;
    
    public SequenceDataExporter(Stream stream)
    {
        _stream = stream;
        _writer = new BinaryWriter(_stream, Encoding.ASCII, true);
    }

    public void Export(SequenceDataModel sequence)
    {
        _stream.Seek(0, SeekOrigin.Begin);
        _writer.Write(sequence.Bytes);
    }
}