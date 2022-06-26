namespace ToucheTools.Models.Instructions;

public class SetCharTextColorInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetCharTextColor;
    
    public ushort Character { get; set; }
    public ushort Color { get; set; }

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadUInt16();
        Color = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Character);
        writer.Write((ushort)Color);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Character} to {Color}";
    }
}