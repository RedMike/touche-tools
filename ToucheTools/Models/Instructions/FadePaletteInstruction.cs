namespace ToucheTools.Models.Instructions;

public class FadePaletteInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.FadePalette;
    public override int Width => 2;
    
    public ushort FadeOutRaw { get; set; }

    public bool FadeOut => FadeOutRaw > 0;

    public override void Load(BinaryReader reader)
    {
        FadeOutRaw = reader.ReadUInt16(); 
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)FadeOutRaw);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(FadeOut ? "out" : "in")}";
    }
}