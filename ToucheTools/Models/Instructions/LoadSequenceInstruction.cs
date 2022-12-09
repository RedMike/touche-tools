namespace ToucheTools.Models.Instructions;

public class LoadSequenceInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.LoadSequence;
    public override int Width => 4;
    
    public short Index { get; set; }
    public short Num { get; set; }

    public override void Load(BinaryReader reader)
    {
        Index = reader.ReadInt16();
        Num = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Index);
        writer.Write((short)Num);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Index},{Num}";
    }

    public override void DeserialiseRemainder(string remainder)
    {
        var parts = remainder.Split(',');
        Index = short.Parse(parts[0]);
        Num = short.Parse(parts[1]);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Index} {Num}";
    }
}