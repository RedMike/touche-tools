namespace ToucheTools.Models.Instructions;

public class SetupFollowingCharInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetupFollowingChar;
    public override int Width => 4;
    
    public ushort Val { get; set; }
    public ushort Character { get; set; }

    public override void Load(BinaryReader reader)
    {
        Val = reader.ReadUInt16();
        Character = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Val);
        writer.Write((ushort)Character);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Character} to {Val}";
    }
}