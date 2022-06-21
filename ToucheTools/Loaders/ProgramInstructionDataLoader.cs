using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Models;

namespace ToucheTools.Loaders;

public class ProgramInstructionDataLoader
{
    private readonly ILogger _logger = LoggerFactory.Create((builder) => builder.AddSimpleConsole()).CreateLogger(typeof(ProgramInstructionDataLoader));
    private readonly Stream _stream;
    private readonly BinaryReader _reader;
    
    public ProgramInstructionDataLoader(Stream stream)
    {
        _stream = stream;
        _reader = new BinaryReader(_stream, Encoding.ASCII, true);
    }

    public void Read(int offset)
    {
        _stream.Seek(offset, SeekOrigin.Begin);
        while (true)
        {
            var rawOpcode = _reader.ReadByte();
            
            if (rawOpcode == byte.MaxValue)
            {
                break;
            }
            if (!Enum.IsDefined(typeof(ProgramDataModel.Opcode), (int)rawOpcode))
            {
                throw new Exception("Unknown opcode: " + rawOpcode);
            }

            var opcode = (ProgramDataModel.Opcode)rawOpcode;
            if (opcode == ProgramDataModel.Opcode.Noop)
            {
                //not interesting
                _logger.Log(LogLevel.Debug, "Opcode: Noop");
                continue;
            }
            _logger.Log(LogLevel.Debug, "Opcode: {}", opcode);
            var instruction = opcode.ToString("G");
            switch (opcode)
            {
                case ProgramDataModel.Opcode.LoadSprite:
                {
                    var index = _reader.ReadUInt16();
                    var num = _reader.ReadUInt16();
                    instruction += $" {index} {num}";
                }
                    break;
                case ProgramDataModel.Opcode.LoadSequence:
                {
                    var index = _reader.ReadUInt16();
                    var num = _reader.ReadUInt16();
                    instruction += $" {index} {num}";
                }
                    break;
                case ProgramDataModel.Opcode.InitCharScript:
                {
                    var ch = _reader.ReadUInt16();
                    var color = _reader.ReadUInt16();
                    var f1 = _reader.ReadUInt16();
                    var f2 = _reader.ReadUInt16();
                    var f3 = _reader.ReadUInt16();
                    instruction += $" {ch} {color} {f1} {f2} {f3}";
                }
                    break;
                case ProgramDataModel.Opcode.SetCharFrame:
                {
                    var ch = _reader.ReadUInt16();
                    var val1 = _reader.ReadUInt16();
                    var val2 = _reader.ReadUInt16();
                    var val3 = _reader.ReadUInt16();
                    var current = ch == 256;
                    instruction += $" {(current ? "current" : ch)} {val1} {val2} {val3}";
                }
                    break;
                case ProgramDataModel.Opcode.FetchScriptWord:
                {
                    var val = _reader.ReadUInt16();
                    instruction += $" {val}";
                }
                    break;
                case ProgramDataModel.Opcode.SetFlag:
                {
                    var flag = _reader.ReadUInt16();
                    //TODO: validate flag
                    instruction += $" {flag} to STK";
                }
                    break;
                case ProgramDataModel.Opcode.SetCharFlags:
                {
                    var ch = _reader.ReadUInt16();
                    var flags = _reader.ReadUInt16();
                    flags &= 0xFF00;
                    instruction += $" {ch} to {flags}";
                }
                    break;
                case ProgramDataModel.Opcode.EnableInput: break;
                default:
                    throw new Exception("Unhandled opcode: " + opcode.ToString("G"));
            }
            _logger.Log(LogLevel.Information, "Instruction: {}", instruction);
        }
    }
}