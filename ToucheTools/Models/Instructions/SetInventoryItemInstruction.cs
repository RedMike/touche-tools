namespace ToucheTools.Models.Instructions;

public class SetInventoryItemInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.SetInventoryItem;
    
    public ushort Character { get; set; }
    public ushort Item { get; set; }
    
    public bool CurrentCharacter => Character == 256;
    public bool MoneyItem => Item == 4;

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadUInt16();
        Item = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Character);
        writer.Write((ushort)Item);
    }

    public override string ToString()
    {
        return $"{Opcode:G} set {(CurrentCharacter ? "current" : Character)}'s {(MoneyItem ? "money" : Item)} to STK value";
    }
}