namespace ToucheTools.Models.Instructions;

public class SetCharDelayInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetCharDelay;
    public override int Width => 2;
    
    public short Delay { get; set; }

    public override void Load(BinaryReader reader)
    {
        Delay = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Delay);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Delay}";
    }

    public override void DeserialiseRemainder(string remainder, Dictionary<string, uint> labels)
    {
        Delay = short.Parse(remainder);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Delay}";
    }
}