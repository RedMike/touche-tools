namespace ToucheTools.Models.Instructions;

public class LoadSequenceInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.LoadSequence;
    
    public ushort Index { get; set; }
    public ushort Num { get; set; }

    public override void Load(BinaryReader reader)
    {
        Index = reader.ReadUInt16();
        Num = reader.ReadUInt16();
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Index} {Num}";
    }
}