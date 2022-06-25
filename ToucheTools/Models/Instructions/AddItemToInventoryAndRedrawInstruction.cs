namespace ToucheTools.Models.Instructions;

public class AddItemToInventoryAndRedrawInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.AddItemToInventoryAndRedraw;
    
    public ushort Character { get; set; }
    
    public bool CurrentCharacter => Character == 256;

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadUInt16();
    }

    public override string ToString()
    {
        return $"{Opcode:G} add item of STK value to {(CurrentCharacter ? "current" : Character)}";
    }
}