namespace ToucheTools.Models.Instructions;

public class GetInventoryItemFlagsInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.GetInventoryItemFlags;
    public override int Width => 2;
    
    public ushort Item { get; set; }

    public override void Load(BinaryReader reader)
    {
        Item = reader.ReadUInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((ushort)Item);
    }

    public override string ToString()
    {
        return $"{Opcode:G} load flags for item {Item} into STK value";
    }
}