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
    
    protected override string SerialiseInternal()
    {
        return $"{Num1},{Num2},{Val}";
    }

    public override void DeserialiseRemainder(string remainder)
    {
        var parts = remainder.Split(',');
        Num1 = short.Parse(parts[0]);
        Num2 = short.Parse(parts[1]);
        Val = short.Parse(parts[2]);
    }

    public override string ToString()
    {
        return $"{Opcode:G} walk from {Num1} to {Num2} drawn area now {Val}";
    }
}