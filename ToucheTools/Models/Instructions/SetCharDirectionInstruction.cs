namespace ToucheTools.Models.Instructions;

public class SetCharDirectionInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetCharDirection;
    public override int Width => 4;
    
    public short Character { get; set; }
    public short Direction { get; set; }

    public bool CurrentCharacter => Character == 256;

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadInt16();
        Direction = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character);
        writer.Write((short)Direction);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Character},{Direction}";
    }

    public override void DeserialiseRemainder(string remainder, Dictionary<string, uint> labels)
    {
        var parts = remainder.Split(',');
        Character = short.Parse(parts[0]);
        Direction = short.Parse(parts[1]);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(CurrentCharacter ? "current" : Character)} to {Direction}";
    }
}