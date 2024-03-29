﻿namespace ToucheTools.Models.Instructions;

public class FadePaletteInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.FadePalette;
    public override int Width => 2;
    
    public short FadeOutRaw { get; set; }

    public bool FadeOut => FadeOutRaw > 0;

    public override void Load(BinaryReader reader)
    {
        FadeOutRaw = reader.ReadInt16(); 
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)FadeOutRaw);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{FadeOutRaw}";
    }

    public override void DeserialiseRemainder(string remainder, Dictionary<string, uint> labels)
    {
        FadeOutRaw = short.Parse(remainder);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(FadeOut ? "out" : "in")}";
    }
}