﻿namespace ToucheTools.Models.Instructions;

public class StartPaletteFadeInInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.StartPaletteFadeIn;
    
    public ushort Num { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadUInt16();
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Num}";
    }
}