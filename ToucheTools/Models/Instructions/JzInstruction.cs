﻿namespace ToucheTools.Models.Instructions;

public class JzInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.Jz;
    
    public ushort NewOffset { get; set; }

    public override void Load(BinaryReader reader)
    {
        NewOffset = reader.ReadUInt16();
    }

    public override string ToString()
    {
        return $"{Opcode:G} if STK value is 0, jump to {NewOffset}";
    }
}