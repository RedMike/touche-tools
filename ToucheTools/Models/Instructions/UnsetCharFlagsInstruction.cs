using ToucheTools.Helpers;

namespace ToucheTools.Models.Instructions;

public class UnsetCharFlagsInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.UnsetCharFlags;
    public override int Width => 4;
    
    public short Character { get; set; }
    public ushort Flags { get; set; }

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadInt16();
        Flags = reader.ReadInt16().AsUshort();//game does it this way
        Flags &= 0xFF00;
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character);
        writer.Write(Flags.AsShort());
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Character},{Flags}";
    }

    public override void DeserialiseRemainder(string remainder)
    {
        var parts = remainder.Split(',');
        Character = short.Parse(parts[0]);
        Flags = ushort.Parse(parts[1]);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Character} from {Flags}";
    }
}