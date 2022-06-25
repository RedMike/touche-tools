using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Exceptions;

namespace ToucheTools.Exporters;

public class ResourceDataExporter
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(ResourceDataExporter));
    private readonly Stream _stream;
    private readonly BinaryWriter _writer;

    public ResourceDataExporter(Stream stream)
    {
        _stream = stream;
        _writer = new BinaryWriter(_stream, Encoding.ASCII, true);
    }

    public void Export(Resource resource, int number, int offset)
    {
        if (number < 0)
        {
            throw new UnknownResourceException();
        }
        if (Resources.DataInfo[resource].Count < number)
        {
            throw new UnknownResourceException();
        }

        _stream.Seek(Resources.DataInfo[resource].Offset + number * 4, SeekOrigin.Begin);
        _writer.Write((uint)offset);
    }
}