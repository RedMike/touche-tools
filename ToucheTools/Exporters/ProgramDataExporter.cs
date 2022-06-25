using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Models;

namespace ToucheTools.Exporters;

public class ProgramDataExporter
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(ProgramDataExporter));
    private readonly Stream _stream;
    private readonly BinaryWriter _writer;
    
    public ProgramDataExporter(Stream stream)
    {
        _stream = stream;
        _writer = new BinaryWriter(_stream, Encoding.ASCII, true);
    }

    public void Export(ProgramDataModel program)
    {
        var startOfDataSegment = 128;
        var gapBetweenBlocks = 0;
        var nextOffset = startOfDataSegment + gapBetweenBlocks;
        var allocate = (int size) =>
        {
            var offset = nextOffset;
            nextOffset += size + gapBetweenBlocks;
            return offset;
        };

        //rects
        {
            var memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            foreach (var rect in program.Rects)
            {
                writer.Write((ushort)rect.X);
                writer.Write((ushort)rect.Y);
                writer.Write((ushort)rect.W);
                writer.Write((ushort)rect.H);
            }

            writer.Write(ushort.MaxValue);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);

            var bytes = memStream.GetBuffer();
            var size = bytes.Length;

            var programOffset = allocate(size);
            _stream.Seek(programOffset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //write the offset after writing the data
            _stream.Seek(20, SeekOrigin.Begin);
            _writer.Write((uint)programOffset);
        }

        //points
        {
            var memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            foreach (var point in program.Points)
            {
                writer.Write((ushort)point.X);
                writer.Write((ushort)point.Y);
                writer.Write((ushort)point.Z);
                writer.Write((ushort)point.Order);
            }

            writer.Write(ushort.MaxValue);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);

            var bytes = memStream.GetBuffer();
            var size = bytes.Length;

            var programOffset = allocate(size);
            _stream.Seek(programOffset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //write the offset after writing the data
            _stream.Seek(24, SeekOrigin.Begin);
            _writer.Write((uint)programOffset);
        }
        
        //walks
        {
            var memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            foreach (var walk in program.Walks)
            {
                //TODO: validate points
                writer.Write((ushort)walk.Point1);
                writer.Write((ushort)walk.Point2);
                writer.Write((ushort)walk.ClipRect);
                writer.Write((ushort)walk.Area1);
                writer.Write((ushort)walk.Area2);
                writer.Write((uint)0); //unused
                writer.Write((uint)0); //unused
                writer.Write((uint)0); //unused
            }

            writer.Write(ushort.MaxValue);

            var bytes = memStream.GetBuffer();
            var size = bytes.Length;

            var programOffset = allocate(size);
            _stream.Seek(programOffset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //write the offset after writing the data
            _stream.Seek(28, SeekOrigin.Begin);
            _writer.Write((uint)programOffset);
        }
        
        //areas
        {
            var memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            foreach (var area in program.Areas)
            {
                //TODO: validate points
                writer.Write((ushort)area.Rect.X);
                writer.Write((ushort)area.Rect.Y);
                writer.Write((ushort)area.Rect.W);
                writer.Write((ushort)area.Rect.H);
                writer.Write((ushort)area.SrcX);
                writer.Write((ushort)area.SrcY);
                writer.Write((ushort)area.Id);
                writer.Write((ushort)area.State);
                writer.Write((ushort)area.AnimationCount);
                writer.Write((ushort)area.AnimationNext);
            }

            writer.Write(ushort.MaxValue);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            
            var bytes = memStream.GetBuffer();
            var size = bytes.Length;

            var programOffset = allocate(size);
            _stream.Seek(programOffset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //write the offset after writing the data
            _stream.Seek(8, SeekOrigin.Begin);
            _writer.Write((uint)programOffset);
        }

        //backgrounds
        {
            var memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            foreach (var bg in program.Backgrounds)
            {
                //TODO: validate points
                writer.Write((ushort)bg.Rect.X);
                writer.Write((ushort)bg.Rect.Y);
                writer.Write((ushort)bg.Rect.W);
                writer.Write((ushort)bg.Rect.H);
                writer.Write((ushort)bg.SrcX);
                writer.Write((ushort)bg.SrcY);
                writer.Write((ushort)bg.Type);
                writer.Write((ushort)bg.Offset);
                writer.Write((ushort)bg.ScaleMul);
                writer.Write((ushort)bg.ScaleDiv);
            }

            writer.Write(ushort.MaxValue);
            
            var bytes = memStream.GetBuffer();
            var size = bytes.Length;
            
            var programOffset = allocate(size);
            _stream.Seek(programOffset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //write the offset after writing the data
            _stream.Seek(12, SeekOrigin.Begin);
            _writer.Write((uint)programOffset);
        }

        //TODO: others
        
        //instructions
        {
            var memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            foreach (var instruction in program.Instructions)
            {
                instruction.Export(writer);
            }
            writer.Write(byte.MaxValue);

            var bytes = memStream.GetBuffer();
            var size = bytes.Length;

            var programOffset = allocate(size);
            _stream.Seek(programOffset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //write the offset after writing the data
            _stream.Seek(32, SeekOrigin.Begin);
            _writer.Write((uint)programOffset);
        }
    }
}