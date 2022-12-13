using ToucheTools.Constants;

namespace ToucheTools.Models.Instructions;

public class StartEpisodeInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.StartEpisode;
    public override int Width => 4;
    
    public short Num { get; set; }
    public short Flag { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadInt16();
        Flag = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Num);
        writer.Write((short)Flag);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Num},{Flag}";
    }

    public override void DeserialiseRemainder(string remainder, Dictionary<string, uint> labels)
    {
        var parts = remainder.Split(',');
        Num = short.Parse(parts[0]);
        Flag = short.Parse(parts[1]);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Num} flag {Flag}";
    }
}