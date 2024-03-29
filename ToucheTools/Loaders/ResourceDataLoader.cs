﻿using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Exceptions;

namespace ToucheTools.Loaders;

public class ResourceDataLoader
{
    private readonly ILogger _logger = Logging.Factory.CreateLogger(typeof(ResourceDataLoader));
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
        _logger.Log(LogLevel.Debug, "Resource {} {}: offset {}", resource.ToString("G"), number, offset);
        
        uint nextOffset = _reader.ReadUInt32();
        size = 0;
        if (nextOffset != 0)
        {
            size = (int)(nextOffset - offset);
            if (size <= 0)
            {
                throw new UnknownResourceException();
            }
        }

        if (!includeSize)
        {
            size = 0;
        }
        else
        {
            _logger.Log(LogLevel.Debug, "Resource {} {}: size {}", resource.ToString("G"), number, size);
        }
    }
}