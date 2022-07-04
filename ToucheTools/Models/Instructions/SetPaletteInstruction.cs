namespace ToucheTools.Models.Instructions;

public class SetPaletteInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetPalette;
    public override int Width => 6;
    
    public ushort R { get; set; }
    public ushort G { get; set; }
    public ushort B { get; set; }

    public override void Load(BinaryReader reader)
    {
        R = reader.ReadUInt16();
        G = reader.ReadUInt16();
        B = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)R);
        writer.Write((ushort)G);
        writer.Write((ushort)B);
    }

    public override string ToString()
    {
        return $"{Opcode:G} rScale {R} gScale {G} bScale {B}";
    }
}