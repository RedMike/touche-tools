namespace ToucheTools.Models.Instructions;

public class InitCharInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.InitChar;
    
    public ushort Character { get; set; }

    public bool CurrentCharacter => Character == 256;

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadUInt16();
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(CurrentCharacter ? "current" : Character)}";
    }
}