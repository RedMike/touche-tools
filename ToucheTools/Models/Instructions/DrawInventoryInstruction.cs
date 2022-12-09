namespace ToucheTools.Models.Instructions;

public class DrawInventoryInstruction : BaseInstruction
{
    public override ProgramDataModel.Opcode Opcode => ProgramDataModel.Opcode.DrawInventory;
    public override int Width => 2;
    
    public short Num { get; set; }

    public override void Load(BinaryReader reader)
    {
        Num = reader.ReadInt16();
    }
    
    protected override void ExportInternal(BinaryWriter writer)
    {
        writer.Write((short)Num);
    }
    
    protected override string SerialiseInternal()
    {
        return $"{Num}";
    }

    public override void DeserialiseRemainder(string remainder)
    {
        Num = short.Parse(remainder);
    }

    public override string ToString()
    {
        return $"{Opcode:G} list {Num}";
    }
}