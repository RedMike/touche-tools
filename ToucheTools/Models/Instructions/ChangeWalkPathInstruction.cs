namespace ToucheTools.Models.Instructions;

public class ChangeWalkPathInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.ChangeWalkPath;
    public override int Width => 6;
    
    public ushort Num1 { get; set; }
    public ushort Num2 { get; set; }
    public ushort Val { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num1 = reader.ReadUInt16();
        Num2 = reader.ReadUInt16();
        Val = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Num1);
        writer.Write((ushort)Num2);
        writer.Write((ushort)Val);
    }

    public override string ToString()
    {
        return $"{Opcode:G} walk between points {Num1} {Num2} set to {Val}";
    }
}