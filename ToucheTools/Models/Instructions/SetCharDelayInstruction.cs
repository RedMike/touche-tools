namespace ToucheTools.Models.Instructions;

public class SetCharDelayInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetCharDelay;
    public override int Width => 2;
    
    public ushort Delay { get; set; }

    public override void Load(BinaryReader reader)
    {
        Delay = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Delay);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Delay}";
    }
}