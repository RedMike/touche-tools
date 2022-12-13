namespace ToucheTools.Models.Instructions;

public class UnlockWalkPathInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.UnlockWalkPath;
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
    
    protected override string SerialiseInternal()
    {
        return $"{Num1},{Num2}";
    }

    public override void DeserialiseRemainder(string remainder, Dictionary<string, uint> labels)
    {
        var parts = remainder.Split(',');
        Num1 = short.Parse(parts[0]);
        Num2 = short.Parse(parts[1]);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Num1} {Num2}";
    }
}