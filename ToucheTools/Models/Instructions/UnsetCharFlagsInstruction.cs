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
        Flags = BitConverter.ToUInt16(BitConverter.GetBytes(reader.ReadInt16()), 0);//game does it this way
        Flags &= 0xFF00;
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character);
        writer.Write(BitConverter.ToUInt16(BitConverter.GetBytes(Flags), 0));//game does it this way
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Character} from {Flags}";
    }
}