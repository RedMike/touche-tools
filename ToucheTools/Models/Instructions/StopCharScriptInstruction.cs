﻿namespace ToucheTools.Models.Instructions;

public class StopCharScriptInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.StopCharScript;
    public override int Width => 2;
    
    public short Character { get; set; }
    public bool CurrentCharacter => Character == 256;

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Character}";
    }

    public override void DeserialiseRemainder(string remainder, Dictionary<string, uint> labels)
    {
        Character = short.Parse(remainder);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(CurrentCharacter ? "current" : Character)}";
    }
}