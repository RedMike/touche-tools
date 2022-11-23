namespace ToucheTools.Models.Instructions;

public class LockWalkPathInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.LockWalkPath;
    public override int Width => 4;
    
    public short Num1 { get; set; }
    public short Num2 { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num1 = reader.ReadInt16();
        Num2 = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Num1);
        writer.Write((short)Num2);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Num1} {Num2}";
    }
}