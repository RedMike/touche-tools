﻿using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Models;

namespace ToucheTools.Exporters;

public class ProgramDataExporter
{
    private readonly ILogger _logger = Logging.Factory.CreateLogger(typeof(ProgramDataExporter));
    private readonly Stream _stream;
    private readonly BinaryWriter _writer;
    
    public ProgramDataExporter(Stream stream)
    {
        _stream = stream;
        _writer = new BinaryWriter(_stream, Encoding.ASCII, true);
    }

    public void Export(ProgramDataModel program)
    {
        var startOfDataSegment = 48;
        var nextOffset = startOfDataSegment;
        var allocate = (int size) =>
        {
            var offset = nextOffset;
            nextOffset += size;
            return offset;
        };
        
        //text
        {
            var memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            
            var stringMax = 0;
            if (program.Strings.Count > 0)
            {
                stringMax = program.Strings.Keys.Max();
            }
            var programOffset = allocate((stringMax+3)*4);
            
            for (var i = 0; i < stringMax+2; i++) //1-indexed
            {
                var s = "";
                if (program.Strings.ContainsKey(i))
                {
                    s = program.Strings[i];
                }

                if (string.IsNullOrEmpty(s))
                {
                    s = "~";
                }

                var chars = s.ToCharArray();

                var textOffset = nextOffset;
                if (chars.Length != 0)
                {
                    textOffset = allocate(chars.Length + 1);
                    //NOTE: the data is being written to the original stream, not the mem stream!
                    _stream.Seek(textOffset, SeekOrigin.Begin);
                    for (var j = 0; j < chars.Length; j++)
                    {
                        _writer.Write((byte)chars[j]);
                    }

                    _writer.Write((byte)0);
                }
                
                //then write offset to the current stream
                memStream.Seek(i * 4, SeekOrigin.Begin);
                writer.Write((uint)(textOffset-programOffset));
            }
            
            var bytes = memStream.ToArray();
            _stream.Seek(programOffset, SeekOrigin.Begin);
            _writer.Write(bytes);

            //write the offset after writing the data
            _stream.Seek(4, SeekOrigin.Begin);
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
                writer.Write((ushort)area.InitialState);
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
            
            var bytes = memStream.ToArray();
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
            
            var bytes = memStream.ToArray();
            var size = bytes.Length;
            
            var programOffset = allocate(size);
            _stream.Seek(programOffset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //write the offset after writing the data
            _stream.Seek(12, SeekOrigin.Begin);
            _writer.Write((uint)programOffset);
        }
        
        //hitboxes
        {
            var memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            foreach (var hitbox in program.Hitboxes)
            {
                writer.Write((ushort)hitbox.Item);
                writer.Write((ushort)hitbox.Talk);
                writer.Write((ushort)hitbox.State);
                writer.Write((ushort)hitbox.String);
                writer.Write((ushort)hitbox.DefaultString);
                writer.Write((ushort)hitbox.Actions[0]);
                writer.Write((ushort)hitbox.Actions[1]);
                writer.Write((ushort)hitbox.Actions[2]);
                writer.Write((ushort)hitbox.Actions[3]);
                writer.Write((ushort)hitbox.Actions[4]);
                writer.Write((ushort)hitbox.Actions[5]);
                writer.Write((ushort)hitbox.Actions[6]);
                writer.Write((ushort)hitbox.Actions[7]);
                writer.Write((ushort)hitbox.Rect1.X);
                writer.Write((ushort)hitbox.Rect1.Y);
                writer.Write((ushort)hitbox.Rect1.W);
                writer.Write((ushort)hitbox.Rect1.H);
                writer.Write((ushort)hitbox.Rect2.X);
                writer.Write((ushort)hitbox.Rect2.Y);
                writer.Write((ushort)hitbox.Rect2.W);
                writer.Write((ushort)hitbox.Rect2.H);
                writer.Write((uint)0); //unused
                writer.Write((uint)0); //unused
            }

            writer.Write((ushort)0); //weirdly uses 0 instead of maxvalue
            
            var bytes = memStream.ToArray();
            var size = bytes.Length;
            
            var programOffset = allocate(size);
            _stream.Seek(programOffset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //write the offset after writing the data
            _stream.Seek(16, SeekOrigin.Begin);
            _writer.Write((uint)programOffset);
        }

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

            var bytes = memStream.ToArray();
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

            var bytes = memStream.ToArray();
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

            var bytes = memStream.ToArray();
            var size = bytes.Length;

            var programOffset = allocate(size);
            _stream.Seek(programOffset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //write the offset after writing the data
            _stream.Seek(28, SeekOrigin.Begin);
            _writer.Write((uint)programOffset);
        }
        
        //instructions
        {
            var memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            var trackedOffset = 0;
            foreach (var instruction in program.Instructions.OrderBy(p => p.Key))
            {
                memStream.Seek(trackedOffset, SeekOrigin.Begin);
                trackedOffset += instruction.Value.Export(writer);
            }
            memStream.Seek(trackedOffset, SeekOrigin.Begin);
            writer.Write(byte.MaxValue);

            var bytes = memStream.ToArray();
            var size = bytes.Length;

            var programOffset = allocate(size);
            _stream.Seek(programOffset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //write the offset after writing the data
            _stream.Seek(32, SeekOrigin.Begin);
            _writer.Write((uint)programOffset);
        }

        //action script offsets
        {
            var memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            foreach (var actionScriptOffset in program.ActionScriptOffsets)
            {
                writer.Write((ushort)actionScriptOffset.Object1);
                writer.Write((ushort)actionScriptOffset.Action);
                writer.Write((ushort)actionScriptOffset.Object2);
                writer.Write((ushort)actionScriptOffset.Offset);
            }

            writer.Write((ushort)0); //weirdly uses 0 instead of maxvalue

            var bytes = memStream.ToArray();
            var size = bytes.Length;

            var programOffset = allocate(size);
            _stream.Seek(programOffset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //write the offset after writing the data
            _stream.Seek(36, SeekOrigin.Begin);
            _writer.Write((uint)programOffset);
        }
        
        //conversations
        {
            var memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            foreach (var conversation in program.Conversations)
            {
                writer.Write((ushort)conversation.Num);
                writer.Write((ushort)conversation.Offset);
                writer.Write((ushort)conversation.Message);
            }

            var bytes = memStream.ToArray();
            var size = bytes.Length;

            var programOffset = allocate(size);
            _stream.Seek(programOffset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //write the offset after writing the data
            _stream.Seek(40, SeekOrigin.Begin);
            _writer.Write((uint)programOffset);
        }
        
        //char script offsets
        {
            var memStream = new MemoryStream();
            var writer = new BinaryWriter(memStream);
            foreach (var cso in program.CharScriptOffsets)
            {
                writer.Write((ushort)cso.Character);
                writer.Write((ushort)cso.Offs);
            }
            writer.Write((ushort)0); //weirdly uses 0 instead of maxvalue

            var bytes = memStream.ToArray();
            var size = bytes.Length;

            var programOffset = allocate(size);
            _stream.Seek(programOffset, SeekOrigin.Begin);
            _writer.Write(bytes);
            //write the offset after writing the data
            _stream.Seek(44, SeekOrigin.Begin);
            _writer.Write((uint)programOffset);
        }

        if (_stream.Length > Game.MaxProgramDataSize)
        {
            //program is too large, debug info
            var reader = new BinaryReader(_stream);
            var labelDict = new Dictionary<int, string>()
            {
                { 0, "None" },
                { 1, "Text" },
                { 2, "Areas" },
                { 3, "Backgrounds" },
                { 4, "Hitboxes" },
                { 5, "Rects" },
                { 6, "Points" },
                { 7, "Walks" },
                { 8, "Instructions" },
                { 9, "ASO" },
                { 10, "Conversations" },
                { 11, "CSO" }
            };
            var dict = new Dictionary<int, int>();
            for (var i = 0; i < 12; i++)
            {
                _stream.Seek(i * 4, SeekOrigin.Begin);
                var offset1 = reader.ReadUInt32();
                var offset2 = _stream.Length;
                if (i != 11)
                {
                    offset2 = reader.ReadUInt32();
                }

                dict[i] = (int)offset2 - (int)(offset1);
            }

            var total = dict.Values.Sum();
            throw new Exception($"Program too large! Max: {Game.MaxProgramDataSize} Made: {_stream.Length}. Sections: {string.Join(",", dict.Select(p => labelDict[p.Key] + "=" + p.Value))} Total data: {total}");
        }
    }
}