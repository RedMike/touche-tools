namespace ToucheTools.Models.Instructions;

public class SetPaletteInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetPalette;
    public override int Width => 6;
    
    public short R { get; set; }
    public short G { get; set; }
    public short B { get; set; }

    public override void Load(BinaryReader reader)
    {
        R = reader.ReadInt16();
        G = reader.ReadInt16();
        B = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)R);
        writer.Write((short)G);
        writer.Write((short)B);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{R},{G},{B}";
    }

    public override void DeserialiseRemainder(string remainder, Dictionary<string, uint> labels)
    {
        var parts = remainder.Split(',');
        R = short.Parse(parts[0]);
        G = short.Parse(parts[1]);
        B = short.Parse(parts[2]);
    }

    public override string ToString()
    {
        return $"{Opcode:G} rScale {R} gScale {G} bScale {B}";
    }
}