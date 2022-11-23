namespace ToucheTools.Models.Instructions;

public class GetInventoryItemInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.GetInventoryItem;
    public override int Width => 4;
    
    public short Character { get; set; }
    public ushort Item { get; set; }
    
    public bool CurrentCharacter => Character == 256;
    public bool MoneyItem => Item == 4;

    public override void Load(BinaryReader reader)
    {
        Character = reader.ReadInt16();
        Item = BitConverter.ToUInt16(BitConverter.GetBytes(reader.ReadInt16()), 0);//game does it this way
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character);
        writer.Write(BitConverter.ToInt16(BitConverter.GetBytes(Item), 0));//game does it this way
    }

    public override string ToString()
    {
        return $"{Opcode:G} load {(CurrentCharacter ? "current" : Character)}'s {(MoneyItem ? "money" : Item)} into STK value";
    }
}