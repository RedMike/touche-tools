﻿namespace ToucheTools.Models.Instructions;

public class SetupFollowingCharInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetupFollowingChar;
    public override int Width => 4;
    
    public short Val { get; set; }
    public short Character { get; set; }

    public override void Load(BinaryReader reader)
    {
        Val = reader.ReadInt16();
        Character = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Val);
        writer.Write((short)Character);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Character} to {Val}";
    }
}