namespace ToucheTools.Models.Instructions;

public class SetupWaitingCharInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetupWaitingChar;
    
    public ushort Character { get; set; }
    public ushort Val1 { get; set; }
    public ushort Val2 { get; set; }
    
    public bool CurrentCharacter => Character == 256;

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadUInt16();
        Val1 = reader.ReadUInt16();
        Val2 = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Character);
        writer.Write((ushort)Val1);
        writer.Write((ushort)Val2);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(CurrentCharacter ? "current" : Character)} {Val1} {Val2}";
    }
}