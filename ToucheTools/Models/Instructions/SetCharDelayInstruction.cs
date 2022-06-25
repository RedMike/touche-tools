namespace ToucheTools.Models.Instructions;

public class SetCharDelayInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetCharDelay;
    
    public ushort Delay { get; set; }

    public override void Load(BinaryReader reader)
    {
        Delay = reader.ReadUInt16();
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Delay}";
    }
}