namespace ToucheTools.Models.Instructions;

public class GetInventoryItemInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.GetInventoryItem;
    public override int Width => 4;
    
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
        return $"{Opcode:G} load {(CurrentCharacter ? "current" : Character)}'s {(MoneyItem ? "money" : Item)} into STK value";
    }
}