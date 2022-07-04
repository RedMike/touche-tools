namespace ToucheTools.Models.Instructions;

public class UnlockWalkPathInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.UnlockWalkPath;
    public override int Width => 4;
    
    public ushort Num1 { get; set; }
    public ushort Num2 { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num1 = reader.ReadUInt16();
        Num2 = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Num1);
        writer.Write((ushort)Num2);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Num1} {Num2}";
    }
}