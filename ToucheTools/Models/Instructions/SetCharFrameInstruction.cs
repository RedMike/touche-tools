namespace ToucheTools.Models.Instructions;

public class SetCharFrameInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetCharFrame;
    
    public ushort Character { get; set; }
    public ushort Val1 { get; set; }
    public ushort Val2 { get; set; }
    public ushort Val3 { get; set; }

    public bool CurrentCharacter => Character == 256;

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadUInt16();
        Val1 = reader.ReadUInt16();
        Val2 = reader.ReadUInt16();
        Val3 = reader.ReadUInt16();
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(CurrentCharacter ? "current" : Character)} {Val1} {Val2} {Val3}";
    }
}