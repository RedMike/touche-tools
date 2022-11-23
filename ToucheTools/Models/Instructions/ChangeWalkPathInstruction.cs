namespace ToucheTools.Models.Instructions;

public class ChangeWalkPathInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.ChangeWalkPath;
    public override int Width => 6;
    
    public short Num1 { get; set; }
    public short Num2 { get; set; }
    public short Val { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num1 = reader.ReadInt16();
        Num2 = reader.ReadInt16();
        Val = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Num1);
        writer.Write((short)Num2);
        writer.Write((short)Val);
    }

    public override string ToString()
    {
        return $"{Opcode:G} walk from {Num1} to {Num2} drawn area now {Val}";
    }
}