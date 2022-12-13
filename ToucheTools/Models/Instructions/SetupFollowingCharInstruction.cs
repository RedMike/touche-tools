namespace ToucheTools.Models.Instructions;

public class SetupFollowingCharInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetupFollowingChar;
    public override int Width => 4;
    
    public short Val { get; set; }
    public short Character { get; set; }

    public override void Load(BinaryReader reader)
    {
        Val = reader.ReadInt16();
        Character = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Val);
        writer.Write((short)Character);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Val},{Character}";
    }

    public override void DeserialiseRemainder(string remainder, Dictionary<string, uint> labels)
    {
        var parts = remainder.Split(',');
        Val = short.Parse(parts[0]);
        Character = short.Parse(parts[1]);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Character} to {Val}";
    }
}