namespace ToucheTools.Models.Instructions;

public class UnsetCharFlagsInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.UnsetCharFlags;
    
    public ushort Character { get; set; }
    public ushort Flags { get; set; }

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadUInt16();
        Flags = reader.ReadUInt16();
        Flags &= 0xFF00;
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Character);
        writer.Write((ushort)Flags);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Character} from {Flags}";
    }
}