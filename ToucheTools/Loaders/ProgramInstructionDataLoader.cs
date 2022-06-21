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
                    instruction += $" set STK value to {val}";
                }
                    break;
                case ProgramDataModel.Opcode.SetFlag:
                {
                    var flag = _reader.ReadUInt16();
                    //TODO: validate flag
                    instruction += $" {flag} to STK value";
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
                case ProgramDataModel.Opcode.SetCharBox:
                {
                    var ch = _reader.ReadUInt16();
                    var num = _reader.ReadUInt16();
                    var current = ch == 256;
                    instruction += $" {(current ? "current" : ch)} {num}";
                }
                    break;
                case ProgramDataModel.Opcode.GetInventoryItem:
                {
                    var ch = _reader.ReadUInt16();
                    var item = _reader.ReadUInt16();
                    var current = ch == 256;
                    var money = item == 4;
                    instruction += $" move STK to {(current ? "current" : ch)}'s {(money ? "money" : item)}";
                }
                    break;
                case ProgramDataModel.Opcode.Push:
                {
                    instruction += $" move STK back one and set to 0";
                }
                    break;
                case ProgramDataModel.Opcode.Add:
                {
                    instruction += $" take STK value, move STK forwards 1, then add to STK";
                }
                    break;
                case ProgramDataModel.Opcode.SetInventoryItem:
                {
                    var ch = _reader.ReadUInt16();
                    var item = _reader.ReadUInt16();
                    var current = ch == 256;
                    var money = item == 4;
                    instruction += $" set {(current ? "current" : ch)}'s {(money ? "money" : item)} to STK value";
                }
                    break;
                case ProgramDataModel.Opcode.AddItemToInventoryAndRedraw:
                {
                    var ch = _reader.ReadUInt16();
                    var current = ch == 256;
                    
                    instruction += $" add item of STK value to {(current ? "current" : ch)}";
                }
                    break;
                case ProgramDataModel.Opcode.LoadRoom:
                {
                    var num = _reader.ReadUInt16();
                    //TODO: mark room as needing load
                    
                    instruction += $" {num}";
                }
                    break;
                case ProgramDataModel.Opcode.SetCharDelay:
                {
                    var delay = _reader.ReadUInt16();
                    //TODO: identify which char
                    //TODO: quit flag 3?
                    instruction += $" {delay}";
                }
                    break;
                case ProgramDataModel.Opcode.StartMusic:
                {
                    var num = _reader.ReadUInt16();

                    instruction += $" {num}";
                }
                    break;
                case ProgramDataModel.Opcode.AddRoomArea:
                {
                    var num = _reader.ReadUInt16();
                    var flag = _reader.ReadUInt16();

                    instruction += $" room {num} position from flags {flag}, {flag + 1}";
                }
                    break;
                case ProgramDataModel.Opcode.MoveCharToPos:
                {
                    var ch = _reader.ReadUInt16();
                    var num = _reader.ReadUInt16();
                    var current = ch == 256;
                    var targetAnotherChar = num == ushort.MaxValue;
                    if (targetAnotherChar)
                    {
                        num = _reader.ReadUInt16();
                    }

                    //TODO: quit flag 3?
                    instruction +=
                        $" {(current ? " current" : ch)} {(targetAnotherChar ? "to char" : "to pos")} {num}";
                }
                    break;
                case ProgramDataModel.Opcode.SetupWaitingChar:
                {
                    var ch = _reader.ReadUInt16();
                    var val1 = _reader.ReadUInt16();
                    var val2 = _reader.ReadUInt16();
                    var current = ch == 256;
                    
                    //TODO: quit flag 3?
                    instruction += $" {(current ? "current" : ch)} {val1} {val2}";
                }
                    break;
                case ProgramDataModel.Opcode.InitChar:
                {
                    var ch = _reader.ReadUInt16();
                    var current = ch == 256;
                    
                    instruction += $" {(current ? "current" : ch)}";
                }
                    break;
                case ProgramDataModel.Opcode.StartEpisode:
                {
                    var num = _reader.ReadUInt16();
                    var flag = _reader.ReadUInt16();

                    instruction += $" {num} flag {flag}";
                }
                    break;
                case ProgramDataModel.Opcode.GetFlag:
                {
                    var flag = _reader.ReadUInt16();

                    instruction += $" set STK value to flag {flag}";
                }
                    break;
                case ProgramDataModel.Opcode.TestEquals:
                {
                    instruction +=
                        $" get STK value, move STK forwards one, check equals; set STK to -1 if yes, 0 if no";
                }
                    break;
                case ProgramDataModel.Opcode.Jz:
                {
                    var newOffset = _reader.ReadUInt16();
                    instruction +=
                        $" if STK value = 0, jump to new data offset {newOffset}";
                }
                    break;
                case ProgramDataModel.Opcode.Jnz:
                {
                    var newOffset = _reader.ReadUInt16();
                    instruction +=
                        $" if STK value != 0, jump to new data offset {newOffset}";
                }
                    break;
                case ProgramDataModel.Opcode.StartSound:
                {
                    var num = _reader.ReadUInt16();
                    var delay = _reader.ReadUInt16();
                    
                    instruction += $" {num} after {delay}";
                }
                    break;
                case ProgramDataModel.Opcode.StartTalk:
                {
                    var ch = _reader.ReadUInt16();
                    var num = _reader.ReadUInt16();
                    if (num == 750)
                    {
                        instruction += $" do nothing";
                    }
                    else
                    {
                        var current = ch == 256;

                        instruction += $" {(current ? "current to next" : (ch + " to " + num))}";
                    }
                }
                    break;
                case ProgramDataModel.Opcode.EnableInput: break;
                case ProgramDataModel.Opcode.StopScript: break;
                case ProgramDataModel.Opcode.StartPaletteFadeIn:
                    _reader.ReadUInt16(); break;
                case ProgramDataModel.Opcode.StartPaletteFadeOut: 
                    _reader.ReadUInt16(); break;
                default:
                    throw new Exception("Unhandled opcode: " + opcode.ToString("G"));
            }
            _logger.Log(LogLevel.Information, "Instruction: {}", instruction);
        }
    }
}