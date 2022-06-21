﻿using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Models;

namespace ToucheTools.Loaders;

public class ProgramDataLoader
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(ProgramDataLoader));
    private readonly Stream _stream;
    private readonly BinaryReader _reader;
    private readonly ResourceDataLoader _resourceDataLoader;
    
    public ProgramDataLoader(Stream stream, ResourceDataLoader resourceDataLoader)
    {
        _stream = stream;
        _reader = new BinaryReader(_stream, Encoding.ASCII, true);
        _resourceDataLoader = resourceDataLoader;
    }

    public void Read(int number)
    {
        _resourceDataLoader.Read(Resource.Program, number, true, out var offset, out var size);
        if (size > 61440)
        {
            throw new Exception("Program too large!");
        }

        _stream.Seek(offset, SeekOrigin.Begin);
        var bytes = _reader.ReadBytes(size);
        using var programStream = new MemoryStream(bytes);
        using var programReader = new BinaryReader(programStream);

        var program = new ProgramDataModel();
        uint programOffset = 0;
        //rects
        programStream.Seek(20, SeekOrigin.Begin);
        programOffset = programReader.ReadUInt32();
        programStream.Seek(programOffset, SeekOrigin.Begin);
        while(true)
        {
            var x = programReader.ReadUInt16();
            var y = programReader.ReadUInt16();
            var w = programReader.ReadUInt16();
            var h = programReader.ReadUInt16();
            if (x == ushort.MaxValue)
            {
                break;
            }
            program.Rects.Add(new ProgramDataModel.Rect()
            {
                X = x,
                Y = y,
                W = w,
                H = h
            });
            _logger.Log(LogLevel.Information, "Rect: {}x{} {}x{}", x, y, w, h);
        }
        _logger.Log(LogLevel.Debug, "Rects found: {}", program.Rects.Count);
        
        //points
        programStream.Seek(24, SeekOrigin.Begin);
        programOffset = programReader.ReadUInt32();
        programStream.Seek(programOffset, SeekOrigin.Begin);
        while(true)
        {
            var x = programReader.ReadUInt16();
            var y = programReader.ReadUInt16();
            var z = programReader.ReadUInt16();
            var order = programReader.ReadUInt16();
            if (x == ushort.MaxValue)
            {
                break;
            }
            program.Points.Add(new ProgramDataModel.Point()
            {
                X = x,
                Y = y,
                Z = z,
                Order = order
            });
            _logger.Log(LogLevel.Debug, "Point: {}x{}x{} {}", x, y, z, order);
        }
        _logger.Log(LogLevel.Information, "Points found: {}", program.Points.Count);
    }
}