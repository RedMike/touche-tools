﻿using System.Text;
using Microsoft.Extensions.Logging;
using ToucheTools.Helpers;
using ToucheTools.Models;
using ToucheTools.Models.Instructions;

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

    public void Read(int offset, out List<BaseInstruction> instructions)
    {
        instructions = new List<BaseInstruction>();
        
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
            _logger.Log(LogLevel.Debug, "Opcode: {}", opcode);
            var instruction = ProgramInstructionHelper.Get(opcode);
            instruction.Load(_reader);
            _logger.Log(LogLevel.Debug, "Instruction: {}", instruction.ToString());
            instructions.Add(instruction);
        }
    }
}