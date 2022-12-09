using ToucheTools.Helpers;

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
        Item = reader.ReadInt16().AsUshort(); //game does it this way
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Character);
        writer.Write(Item.AsShort());
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Character},{Item}";
    }

    public override void DeserialiseRemainder(string remainder)
    {
        var parts = remainder.Split(',');
        Character = short.Parse(parts[0]);
        Item = ushort.Parse(parts[1]);
    }

    public override string ToString()
    {
        return $"{Opcode:G} {(CurrentCharacter ? "current" : Character)}'s {(MoneyItem ? "money" : Item)} to STK value";
    }
}