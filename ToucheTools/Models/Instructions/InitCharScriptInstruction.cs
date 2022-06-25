namespace ToucheTools.Models.Instructions;

public class InitCharScriptInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.InitCharScript;
    
    public ushort Character { get; set; }
    public ushort Color { get; set; }
    public ushort F1 { get; set; }
    public ushort F2 { get; set; }
    public ushort F3 { get; set; }

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadUInt16();
        Color = reader.ReadUInt16();
        F1 = reader.ReadUInt16();
        F2 = reader.ReadUInt16();
        F3 = reader.ReadUInt16();
    }

    public override string ToString()
    {
        return $"{Opcode:G} {Character} {Color} {F1} {F2} {F3}";
    }
}