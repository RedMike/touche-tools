using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Exceptions;

namespace ToucheTools.Loaders;

public class ResourceDataLoader
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(ResourceDataLoader));
    private readonly Stream _stream;
    private readonly BinaryReader _reader;
    
    public ResourceDataLoader(Stream stream)
    {
        _stream = stream;
        _reader = new BinaryReader(_stream, Encoding.ASCII, true);
    }

    public void Read(Resource resource, int number, bool includeSize, out int offset, out int size)
    {
        if (Resources.DataInfo[resource].Count < number)
        {
            throw new UnknownResourceException();
        }
        _stream.Seek(Resources.DataInfo[resource].Offset + number * 4, SeekOrigin.Begin);
        uint rawOffset = _reader.ReadUInt32();
        if (rawOffset == 0)
        {
            throw new Exception("Null offset");
        }

        offset = (int)rawOffset;
        _logger.Log(LogLevel.Information, "Resource {} {}: offset {}", resource.ToString("G"), number, offset);
        
        if (!includeSize)
        {
            size = 0;
        }
        else
        {
            uint nextOffset = _reader.ReadUInt32();
            size = (int)(nextOffset - offset);
            if (size <= 0)
            {
                throw new UnknownResourceException();
            }
            _logger.Log(LogLevel.Information, "Resource {} {}: size {}", resource.ToString("G"), number, size);
        }
    }
}