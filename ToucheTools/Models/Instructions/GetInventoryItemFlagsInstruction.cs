namespace ToucheTools.Models.Instructions;

public class GetInventoryItemFlagsInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.GetInventoryItemFlags;
    public override int Width => 2;
    
    public short Item { get; set; }

    public override void Load(BinaryReader reader)
    {
        Item = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Item);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Item}";
    }

    public override void DeserialiseRemainder(string remainder, Dictionary<string, uint> labels)
    {
        Item = short.Parse(remainder);
    }

    public override string ToString()
    {
        return $"{Opcode:G} inventory of {Item} to STK val";
    }
}