namespace ToucheTools.Models.Instructions;

public class GetInventoryItemInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.GetInventoryItem;
    
    public ushort Character { get; set; }
    public ushort Item { get; set; }
    
    public bool CurrentCharacter => Character == 256;
    public bool MoneyItem => Item == 4;

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadUInt16();
        Item = reader.ReadUInt16();
    }

    public override string ToString()
    {
        return $"{Opcode:G} move STK position to {(CurrentCharacter ? "current" : Character)}'s {(MoneyItem ? "money" : Item)}";
    }
}