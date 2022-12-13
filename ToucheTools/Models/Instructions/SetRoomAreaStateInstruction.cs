namespace ToucheTools.Models.Instructions;

public class SetRoomAreaStateInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetRoomAreaState;
    public override int Width => 4;
    
    public short Num { get; set; }
    public short Val { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadInt16();
        Val = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Num);
        writer.Write((short)Val);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Num},{Val}";
    }

    public override void DeserialiseRemainder(string remainder, Dictionary<string, uint> labels)
    {
        var parts = remainder.Split(',');
        Num = short.Parse(parts[0]);
        Val = short.Parse(parts[1]);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Num} {Val}";
    }
}