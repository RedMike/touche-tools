namespace ToucheTools.Models.Instructions;

public class LoadSpeechSegmentInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.LoadSpeechSegment;
    public override int Width => 2;
    
    public ushort Num { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Num);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Num}";
    }
}