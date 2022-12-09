namespace ToucheTools.Models.Instructions;

public class SetCharBoxInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetCharBox;
    public override int Width => 4;
    
    public short Character { get; set; }
    public short Num { get; set; }
    
    public bool CurrentCharacter => Character == 256;

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadInt16();
        Num = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character);
        writer.Write((short)Num);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Character},{Num}";
    }

    public override void DeserialiseRemainder(string remainder)
    {
        var parts = remainder.Split(',');
        Character = short.Parse(parts[0]);
        Num = short.Parse(parts[1]);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(CurrentCharacter ? "current" : Character)} {Num}";
    }
}