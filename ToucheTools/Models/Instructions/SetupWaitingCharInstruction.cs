namespace ToucheTools.Models.Instructions;

public class SetupWaitingCharInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetupWaitingChar;
    public override int Width => 6;
    
    public short Character { get; set; }
    public short Val1 { get; set; }
    public short Val2 { get; set; }
    
    public bool CurrentCharacter => Character == 256;

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadInt16();
        Val1 = reader.ReadInt16();
        Val2 = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character);
        writer.Write((short)Val1);
        writer.Write((short)Val2);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Character},{Val1},{Val2}";
    }

    public override void DeserialiseRemainder(string remainder, Dictionary<string, uint> labels)
    {
        var parts = remainder.Split(',');
        Character = short.Parse(parts[0]);
        Val1 = short.Parse(parts[1]);
        Val2 = short.Parse(parts[2]);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(CurrentCharacter ? "current" : Character)} {Val1} {Val2}";
    }
}