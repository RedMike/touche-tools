namespace ToucheTools.Models.Instructions;

public class StartSoundInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.StartSound;
    public override int Width => 4;
    
    public short Num { get; set; }
    public short Delay { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadInt16();
        Delay = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Num);
        writer.Write((short)Delay);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Num},{Delay}";
    }

    public override void DeserialiseRemainder(string remainder, Dictionary<string, uint> labels)
    {
        var parts = remainder.Split(',');
        Num = short.Parse(parts[0]);
        Delay = short.Parse(parts[1]);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Num} after {Delay}";
    }
}