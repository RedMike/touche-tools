using System.Text;
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

    public void Read(int number, out List<string> instructions)
    {
        _resourceDataLoader.Read(Resource.Program, number, true, out var offset, out var size);
        if (size > Game.MaxProgramDataSize)
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
        
        //walks
        programStream.Seek(28, SeekOrigin.Begin);
        programOffset = programReader.ReadUInt32();
        programStream.Seek(programOffset, SeekOrigin.Begin);
        while(true)
        {
            var point1 = programReader.ReadUInt16();
            if (point1 == ushort.MaxValue)
            {
                break;
            }

            if (point1 >= program.Points.Count)
            {
                throw new Exception("Unknown point referenced");
            }
            var point2 = programReader.ReadUInt16();
            if (point2 >= program.Points.Count)
            {
                throw new Exception("Unknown point referenced");
            }
            var clipRect = programReader.ReadUInt16();
            var area1 = programReader.ReadUInt16();
            var area2 = programReader.ReadUInt16();
            programStream.Seek(12, SeekOrigin.Current); //unused
            program.Walks.Add(new ProgramDataModel.Walk()
            {
                Point1 = point1,
                Point2 = point2,
                ClipRect = clipRect,
                Area1 = area1,
                Area2 = area2
            });
            _logger.Log(LogLevel.Debug, "Walk: {}->{} clip {} areas {} {}", point1, point2, clipRect, area1, area2);
        }
        _logger.Log(LogLevel.Information, "Walks found: {}", program.Walks.Count);
        
        //areas
        programStream.Seek(8, SeekOrigin.Begin);
        programOffset = programReader.ReadUInt32();
        programStream.Seek(programOffset, SeekOrigin.Begin);
        while(true)
        {
            var x = programReader.ReadUInt16();
            if (x == ushort.MaxValue)
            {
                break;
            }
            var y = programReader.ReadUInt16();
            var w = programReader.ReadUInt16();
            var h = programReader.ReadUInt16();
            var srcX = programReader.ReadUInt16();
            var srcY = programReader.ReadUInt16();
            var id = programReader.ReadUInt16();
            var state = programReader.ReadUInt16();
            var animCount = programReader.ReadUInt16();
            var animNext = programReader.ReadUInt16();
            
            var rect = new ProgramDataModel.Rect()
            {
                X = x,
                Y = y,
                W = w,
                H = h
            };
            var area = new ProgramDataModel.Area()
            {
                Rect = rect,
                SrcX = srcX,
                SrcY = srcY,
                Id = id,
                State = state,
                AnimationCount = animCount,
                AnimationNext = animNext
            };
            
            program.Areas.Add(area);
            _logger.Log(LogLevel.Debug, "Area: {}x{}x{}x{} {}x{} {} {} {} {}", x, y, w, h, srcX, srcY, id, state, animCount, animNext);
        }
        _logger.Log(LogLevel.Information, "Areas found: {}", program.Areas.Count);
        
        //backgrounds
        programStream.Seek(12, SeekOrigin.Begin);
        programOffset = programReader.ReadUInt32();
        programStream.Seek(programOffset, SeekOrigin.Begin);
        while(true)
        {
            var x = programReader.ReadUInt16();
            if (x == ushort.MaxValue)
            {
                break;
            }
            var y = programReader.ReadUInt16();
            var w = programReader.ReadUInt16();
            var h = programReader.ReadUInt16();
            var srcX = programReader.ReadUInt16();
            var srcY = programReader.ReadUInt16();
            var type = programReader.ReadUInt16();
            var bgOffset = programReader.ReadUInt16();
            var scaleMul = programReader.ReadUInt16();
            var scaleDiv = programReader.ReadUInt16();
            
            var rect = new ProgramDataModel.Rect()
            {
                X = x,
                Y = y,
                W = w,
                H = h
            };
            var bg = new ProgramDataModel.Background()
            {
                Rect = rect,
                SrcX = srcX,
                SrcY = srcY,
                Type = type,
                Offset = bgOffset,
                ScaleMul = scaleMul,
                ScaleDiv = scaleDiv
            };
            
            program.Backgrounds.Add(bg);
            _logger.Log(LogLevel.Debug, "Background: {}x{}x{}x{} {}x{} {} {} {} {}", x, y, w, h, srcX, srcY, type, bgOffset, scaleMul, scaleDiv);
        }
        _logger.Log(LogLevel.Information, "Backgrounds found: {}", program.Backgrounds.Count);
        
        //operations
        programStream.Seek(32, SeekOrigin.Begin);
        programOffset = programReader.ReadUInt32();
        var programInstructionLoader = new ProgramInstructionDataLoader(programStream);
        programInstructionLoader.Read((int)programOffset, out instructions);
    }
}