using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Constants;
using ToucheTools.Models;

namespace ToucheTools.Loaders;

public class ProgramDataLoader
{
    private readonly ILogger _logger = Logging.Factory.CreateLogger(typeof(ProgramDataLoader));
    private readonly Stream _stream;
    private readonly BinaryReader _reader;
    private readonly ResourceDataLoader _resourceDataLoader;
    
    public ProgramDataLoader(Stream stream, ResourceDataLoader resourceDataLoader)
    {
        _stream = stream;
        _reader = new BinaryReader(_stream, Encoding.ASCII, true);
        _resourceDataLoader = resourceDataLoader;
    }

    public void Read(int number, out ProgramDataModel program)
    {
        _resourceDataLoader.Read(Resource.Program, number, true, out var offset, out var size);
        if (size > Game.MaxProgramDataSize)
        {
            throw new Exception($"Program too large! Max: {Game.MaxProgramDataSize} Got: {size}");
        }

        _stream.Seek(offset, SeekOrigin.Begin);
        var bytes = _reader.ReadBytes(size);
        using var programStream = new MemoryStream(bytes);
        using var programReader = new BinaryReader(programStream);

        program = new ProgramDataModel();
        program.OriginalSize = size;
        uint programOffset = 0;
        
        //text
        if (programStream.Length > 4)
        {
            programStream.Seek(4, SeekOrigin.Begin);
            programOffset = programReader.ReadUInt32();
            var nextOffset = programReader.ReadUInt32();
            var textSize = (long)nextOffset - (long)programOffset;
            if (nextOffset <= programOffset)
            {
                throw new Exception("Unknown text size: " + textSize);
            }
            programStream.Seek(programOffset, SeekOrigin.Begin);
            var i = 1;
            
            while (i*4 < textSize)
            {
                programStream.Seek(programOffset + i * 4, SeekOrigin.Begin);
                var textOffset = programReader.ReadUInt32();
                if (textOffset == 0)
                {
                    break;
                }

                var nextTextOffset = programReader.ReadUInt32();
                var strSize = (long)nextTextOffset - (long)textOffset;
                if (nextTextOffset <= textOffset)
                {
                    break;
                }
                programStream.Seek(programOffset + textOffset, SeekOrigin.Begin);
                var s = "";
                while (s.Length < strSize)
                {
                    var chr = programReader.ReadByte();
                    
                    if (chr == 0)
                    {
                        break;
                    }

                    if (chr < 32 || chr >= 32 + Fonts.FontOffsets.Length)
                    {
                        throw new Exception("Invalid character: string '" + s + "' length " + s.Length + " char " + chr);
                    }

                    s += (char)chr;
                }

                if (!string.IsNullOrWhiteSpace(s) && s != "~")
                {
                    program.Strings[i] = s;
                }

                i++;
            }
        }
        
        //rects
        if (programStream.Length > 20)
        {
            programStream.Seek(20, SeekOrigin.Begin);
            programOffset = programReader.ReadUInt32();
            programStream.Seek(programOffset, SeekOrigin.Begin);
            while (true)
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
                _logger.Log(LogLevel.Debug, "Rect: {}x{} {}x{}", x, y, w, h);
            }

            _logger.Log(LogLevel.Debug, "Rects found: {}", program.Rects.Count);
        }

        //points
        if (programStream.Length > 24)
        {
            programStream.Seek(24, SeekOrigin.Begin);
            programOffset = programReader.ReadUInt32();
            programStream.Seek(programOffset, SeekOrigin.Begin);
            while (true)
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

            _logger.Log(LogLevel.Debug, "Points found: {}", program.Points.Count);
        }

        //walks
        if (programStream.Length > 28)
        {
            programStream.Seek(28, SeekOrigin.Begin);
            programOffset = programReader.ReadUInt32();
            programStream.Seek(programOffset, SeekOrigin.Begin);
            while (true)
            {
                var point1 = programReader.ReadUInt16();
                if (point1 == ushort.MaxValue)
                {
                    break;
                }

                if (point1 > program.Points.Count)
                {
                    throw new Exception($"Unknown point referenced: {point1} out of {program.Points.Count}");
                }

                var point2 = programReader.ReadUInt16();
                if (point2 > program.Points.Count)
                {
                    throw new Exception($"Unknown point referenced: {point2} out of {program.Points.Count}");
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

            _logger.Log(LogLevel.Debug, "Walks found: {}", program.Walks.Count);
        }

        //areas
        if (programStream.Length > 8)
        {
            programStream.Seek(8, SeekOrigin.Begin);
            programOffset = programReader.ReadUInt32();
            programStream.Seek(programOffset, SeekOrigin.Begin);
            while (true)
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
                    InitialState = state,
                    AnimationCount = animCount,
                    AnimationNext = animNext
                };

                program.Areas.Add(area);
                _logger.Log(LogLevel.Debug, "Area: {}x{}x{}x{} {}x{} {} {} {} {}", x, y, w, h, srcX, srcY, id, state,
                    animCount, animNext);
            }

            _logger.Log(LogLevel.Debug, "Areas found: {}", program.Areas.Count);
        }

        //backgrounds
        if (programStream.Length > 12)
        {
            programStream.Seek(12, SeekOrigin.Begin);
            programOffset = programReader.ReadUInt32();
            programStream.Seek(programOffset, SeekOrigin.Begin);
            while (true)
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
                _logger.Log(LogLevel.Debug, "Background: {}x{}x{}x{} {}x{} {} {} {} {}", x, y, w, h, srcX, srcY, type,
                    bgOffset, scaleMul, scaleDiv);
            }

            _logger.Log(LogLevel.Debug, "Backgrounds found: {}", program.Backgrounds.Count);
        }
        
        //hitboxes
        if (programStream.Length > 16)
        {
            programStream.Seek(16, SeekOrigin.Begin);
            programOffset = programReader.ReadUInt32();
            programStream.Seek(programOffset, SeekOrigin.Begin);
            while (true)
            {
                var item = programReader.ReadUInt16();
                if (item == 0) //weirdly it uses 0 instead of maxvalue
                {
                    break;
                }

                var talk = programReader.ReadUInt16();
                var state = programReader.ReadUInt16();
                var str = programReader.ReadUInt16();
                var defaultStr = programReader.ReadUInt16();
                var action1 = programReader.ReadUInt16();
                var action2 = programReader.ReadUInt16();
                var action3 = programReader.ReadUInt16();
                var action4 = programReader.ReadUInt16();
                var action5 = programReader.ReadUInt16();
                var action6 = programReader.ReadUInt16();
                var action7 = programReader.ReadUInt16();
                var action8 = programReader.ReadUInt16();
                var x1 = programReader.ReadUInt16();
                var y1 = programReader.ReadUInt16();
                var w1 = programReader.ReadUInt16();
                var h1 = programReader.ReadUInt16();
                var x2 = programReader.ReadUInt16();
                var y2 = programReader.ReadUInt16();
                var w2 = programReader.ReadUInt16();
                var h2 = programReader.ReadUInt16();
                try
                {
                    programReader.ReadUInt32(); //unused
                    programReader.ReadUInt32(); //unused
                }
                catch (Exception)
                {
                    //ignore errors because these aren't used and sometimes not populated
                    //but the game doesn't actually check them
                }

                var rect1 = new ProgramDataModel.Rect()
                {
                    X = x1,
                    Y = y1,
                    W = w1,
                    H = h1
                };
                var rect2 = new ProgramDataModel.Rect()
                {
                    X = x2,
                    Y = y2,
                    W = w2,
                    H = h2
                };
                var hitbox = new ProgramDataModel.Hitbox()
                {
                    Item = item,
                    Talk = talk,
                    State = state,
                    String = str,
                    DefaultString = defaultStr,
                    Actions = new int[]
                    {
                        action1, action2, action3,
                        action4, action5, action6,
                        action7, action8
                    },
                    Rect1 = rect1,
                    Rect2 = rect2
                };

                program.Hitboxes.Add(hitbox);
                _logger.Log(LogLevel.Debug, "Hitbox: {} {}x{}x{}x{} {}x{}x{}x{}", item, x1, y1, w1, h1, x2, y2, w2, h2);
            }

            _logger.Log(LogLevel.Debug, "Hitboxes found: {}", program.Hitboxes.Count);
        }
        
        //action script offsets
        if (programStream.Length > 36)
        {
            programStream.Seek(36, SeekOrigin.Begin);
            programOffset = programReader.ReadUInt32();
            programStream.Seek(programOffset, SeekOrigin.Begin);
            while (true)
            {
                if (programStream.Position + 2 >= programStream.Length)
                {
                    break; //seems to be necessary because the game expects it to succeed even if EOS?
                }
                var object1 = programReader.ReadUInt16();

                if (object1 == 0) //weirdly it uses 0 instead of maxvalue
                {
                    break;
                }
                if (programStream.Position + 2 >= programStream.Length)
                {
                    break; //seems to be necessary because the game expects it to succeed even if EOS?
                }
                var action = programReader.ReadUInt16();
                if (programStream.Position + 2 >= programStream.Length)
                {
                    break; //seems to be necessary because the game expects it to succeed even if EOS?
                }
                var object2 = programReader.ReadUInt16();
                if (programStream.Position + 2 >= programStream.Length)
                {
                    break; //seems to be necessary because the game expects it to succeed even if EOS?
                }
                var offs = programReader.ReadUInt16();
                if (programStream.Position + 2 >= programStream.Length)
                {
                    break; //seems to be necessary because the game expects it to succeed even if EOS?
                }

                program.ActionScriptOffsets.Add(new ProgramDataModel.ActionScriptOffset()
                {
                    Object1 = object1,
                    Action = action,
                    Object2 = object2,
                    Offset = offs
                });
                _logger.Log(LogLevel.Debug, "Action script offsets: {} does {} to {} offset {}", object1, action, object2, offs);
            }

            _logger.Log(LogLevel.Debug, "Action script offsets found: {}", program.ActionScriptOffsets.Count);
        }
        
        //conversations
        if (programStream.Length > 44)
        {
            programStream.Seek(40, SeekOrigin.Begin);
            programOffset = programReader.ReadUInt32();
            programStream.Seek(44, SeekOrigin.Begin);
            var nextOffset = programReader.ReadUInt32();
            var count = (nextOffset - programOffset) / 6;
            
            programStream.Seek(programOffset, SeekOrigin.Begin);
            for (var i = 0; i < count; i++)
            {
                var num = programReader.ReadUInt16();
                var offs = programReader.ReadUInt16();
                var msg = programReader.ReadUInt16();
                
                program.Conversations.Add(new ProgramDataModel.Conversation()
                {
                    Num = num,
                    Offset = offs,
                    Message = msg
                });
                _logger.Log(LogLevel.Debug, "Conversations: {} {} {}", num, offs, msg);
            }

            _logger.Log(LogLevel.Debug, "Conversations found: {}", program.Conversations.Count);
        }
        
        //char script offset
        if (programStream.Length > 44)
        {
            programStream.Seek(44, SeekOrigin.Begin);
            programOffset = programReader.ReadUInt32();
            
            programStream.Seek(programOffset, SeekOrigin.Begin);
            while(true)
            {
                var ch = programReader.ReadUInt16();
                if (ch == 0)
                {
                    break;
                }
                var offs = programReader.ReadUInt16();
                
                program.CharScriptOffsets.Add(new ProgramDataModel.CharScriptOffset()
                {
                    Character = ch,
                    Offs = offs
                });
                _logger.Log(LogLevel.Debug, "Char script offsets: {} {}", ch, offs);
            }

            _logger.Log(LogLevel.Debug, "Char script offsets found: {}", program.CharScriptOffsets.Count);
        }

        //operations
        if (programStream.Length > 32)
        {
            programStream.Seek(32, SeekOrigin.Begin);
            programOffset = programReader.ReadUInt32();
            var programInstructionLoader = new ProgramInstructionDataLoader(programStream);
            programInstructionLoader.Read((int)programOffset, out var instructions);
            program.Instructions = instructions;
        }
    }
}